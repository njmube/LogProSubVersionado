using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;
using System.Data;
using System.Reflection;
using WpfFront.Common.WFUserControls;
using Microsoft.Windows.Controls;
using WpfFront.Common.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Data.OleDb;
using System.Diagnostics;

namespace WpfFront.Presenters
{

    public interface IBodegasPresenter
    {
        IBodegasView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class BodegasPresenter : IBodegasPresenter
    {
        public IBodegasView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        private int contFilas_byPallet = 0; // Almacena el conteo de los equipos que tiene un pallet seleccionado en la lista de busqueda
        private String codigoPallet = ""; // Guarda el codigo pallet cuando se selecciona una fila de la lista de pallets o cuando se genera un nuevo codigo de pallet
        private String ubicacionPallet = ""; // Guarda la ubicacion del pallet seleccionado de la lista de pallets
        private Boolean seleccionUbicacion = false; // Controla el dato de la ubicacion, si el usuario selecciona nueva pallet (False) se toma del combobox ubicacion, si selecciona una fila/pallet se captura de ahi (True)
        public int offset = 6; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        private DataTable SerialesIngresados = new DataTable();
        private DataRow NoLoad_Row = null;
        private Timer t;
        private Timer t1;
        private Timer tserial;
        private Timer treceiver;
        private Timer tsmart;
        private Timer talmacenado;
        private Thread updateLabelThread;
        private Boolean estado_cargue = false, busqueda_Repetidos = false, busqueda_serial = false, busqueda_receiver = false, busqueda_smart = false, busqueda_almacenado = false;
        private OleDbConnection oledbcon;
        private OleDbDataAdapter adaptador;
        private Thread hilo_repetidos;

        public BodegasPresenter(IUnityContainer container, IBodegasView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<BodegasModel>();

            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            //view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            //View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            //View.Imprimir_Hablador += new EventHandler<EventArgs>(this.OnImprimir_Hablador);
            View.EliminarEquipo_Fila += new EventHandler<EventArgs>(this.OnEliminarEquipo_Fila);
            View.GenerarPallet += new EventHandler<EventArgs>(this.OnGenerarPallet);
            View.SeleccionPallet_Consulta += new EventHandler<EventArgs>(this.OnSeleccionPalletConsulta);
            View.KeyConsultarPallet += new EventHandler<KeyEventArgs>(this.OnKeyConsultarPallet);
            View.EnterConsultarPallet += new EventHandler<KeyEventArgs>(this.OnEnterConsultarPallet);
            View.ImprimirHablador += new EventHandler<EventArgs>(this.OnImprimirHablador);

            //View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            //View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            //View.FilaSeleccionada += this.OnFilaSeleccionada;

            view.CargaMasiva += new EventHandler<EventArgs>(this.OnCargaMasiva);
            view.KillProcess += new EventHandler<EventArgs>(this.OnKillProcess);

            #endregion

            #region Datos

            //Cargo la variable de las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROPOSIC" } });
            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'RECIBOALMACEN', 'CLARO'", "", "dbo.Ubicaciones", Local);
            this.Actualizar_UbicacionDisponible();

            //Creo la consulta para buscar los ultimos 15 pallets registrados
            //String ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARPALLETALMACENAMIENTO','','" + this.user + "','',''";
            //View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            //Cargo los datos del listado
            CargarDatosDetails();
            ConsultarPallets();
            Calcular_TotalEquipos();

            #endregion
        }

        #region Metodos

        #region Metodos cargue masivo

        private void OnCargaMasiva(object sender, EventArgs e)
        {
            hilo_repetidos = new Thread(new ParameterizedThreadStart(SetRepeat));
            hilo_repetidos.SetApartmentState(ApartmentState.STA);
            hilo_repetidos.IsBackground = true;
            hilo_repetidos.Priority = ThreadPriority.Highest;

            String Cadena = View.GetUpLoadFile.FileName.ToString();
            String conexion = "Provider=Microsoft.Jet.OleDb.4.0; Extended Properties=\"Excel 8.0; HDR=yes\"; Data Source=" + Cadena;
            try
            {
                //creo la coneccion para leer  el .xls
                //OleDbConnection oledbcon = default(OleDbConnection);
                oledbcon = new OleDbConnection(conexion);

                //traigo los datos del .xls
                adaptador = new OleDbDataAdapter("select * from [Hoja1$]", oledbcon);

                //guardo la info en un datatable
                adaptador.Fill(SerialesIngresados);

                oledbcon.Close();
                oledbcon.Dispose();

                //valido que existan registros
                if (SerialesIngresados.Rows.Count == 0)
                {
                    Util.ShowMessage("No hay registros para procesar");
                }
                else
                {
                    View.Progress_Cargue.Value = 0;
                    View.GetEstado_Cargue.Text = "Iniciando operación";
                    hilo_repetidos.Start(SerialesIngresados);

                    StartTimer();//Inicia el timmer para mostrar visualmente el progreso del cargue masivo

                }
            }
            catch (Exception ex)
            {
                Util.ShowError("El archivo a cargar no cuenta con la estructura correcta.");
            }
        }

        int cont = 0;
        int cont_cargue = 0;
        int cont_repeat = 0;
        private void StartTimer()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            t = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont++;

                    if (cont == 1)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos duplicados";
                    }
                    else if (cont == 2)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos duplicados.";
                    }
                    else if (cont == 3)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos duplicados..";
                    }
                    else if (cont == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos duplicados...";
                        cont = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_Cargue.Value < 100D && busqueda_Repetidos == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        private void SetRepeat(object o)
        {
            busqueda_Repetidos = false;
            busqueda_serial = false;
            busqueda_receiver = false;
            busqueda_smart = false;
            busqueda_almacenado = false;
            estado_cargue = false;

            List<String> listNoCargue = new List<String>(); //Guarda los seriales que no cumplen con los requisitos del cargue

            for (int i = 0; i < SerialesIngresados.Rows.Count; i++)
            {
                for (int j = i + 1; j < SerialesIngresados.Rows.Count; j++)
                {
                    if (SerialesIngresados.Rows[i]["Serial"].ToString() == "" & SerialesIngresados.Rows[j]["Serial"].ToString() == "")
                    {
                        SerialesIngresados.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                        //j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        if (j > 0)
                            j = i - 1;
                    }
                    //if (SerialesIngresados.Rows[i]["SERIAL"].ToString() != "" && SerialesIngresados.Rows[j]["SERIAL"].ToString() != "")
                    else
                    {
                        if (SerialesIngresados.Rows[i]["SERIAL"].ToString() == SerialesIngresados.Rows[j]["SERIAL"].ToString())
                        {
                            listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[j]["SERIAL"].ToString(), 
                                                                                       "Serial duplicado dentro del archivo de cargue"});

                            SerialesIngresados.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                            j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        }
                    }
                }
                cont_repeat++;
            }

            busqueda_Repetidos = true;
            Thread.Sleep(1000);
            //t.Dispose();

            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);

            StartTimerSerial();

            //validamos que los seriales ya se encuentren liberados
            String ConsultaBuscarSerial = "";
            int temp2 = 0;
            while (temp2 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarSerial = "SELECT serial from dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp2]["SERIAL"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSerial, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count == 0)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp2]["SERIAL"].ToString(), 
                                                                                        "Serial NO existente en el sistema."});

                    SerialesIngresados.Rows.RemoveAt(temp2);
                }
                else
                {
                    temp2++;
                }
                cont_repeat2++;
            }

            busqueda_serial = true;
            Thread.Sleep(1000);
            tserial.Dispose();

            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);

            StartTimerAlmacenado();

            //validamos que los seriales ya se encuentren liberados
            String ConsultaBuscarAlmacenado = "";
            int temp3 = 0;
            while (temp3 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarAlmacenado = "SELECT serial from dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp3]["SERIAL"].ToString() + "') and Estado = 'ALMACENAMIENTO'";
                DataTable ResultadoAlmacenado = service.DirectSQLQuery(ConsultaBuscarAlmacenado, "", "dbo.EquiposCLARO", Local);

                if (ResultadoAlmacenado.Rows.Count > 0)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp3]["SERIAL"].ToString(), 
                                                                                        "Serial ya se encuentra almacenado."});

                    SerialesIngresados.Rows.RemoveAt(temp3);
                }
                else
                {
                    temp3++;
                }
                cont_repeat3++;
            }

            busqueda_almacenado = true;
            Thread.Sleep(1000);
            talmacenado.Dispose();

            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);


            this.MostrarErrores_Cargue(listNoCargue); // Agrega a un segundo listview los equipos que no fueron cargados

            StartTimer2();

            cont = 0;
            cont2 = 0;
            cont3 = 0;

            CargarListDetails();
            estado_cargue = true;
        }

        private void MostrarErrores_Cargue(List<String> listNoCargue)
        {
            int columna = 0;
            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                NoLoad_Row = View.Model.List_Nocargue.NewRow();
            }), null);

            foreach (var dr in listNoCargue)
            {
                View.Dispatcher_Cargue.Invoke(new System.Action(() =>
                {
                    if (columna != View.Model.List_Nocargue.Columns.Count - 1)
                    {
                        NoLoad_Row[columna] = dr;
                        columna++;
                    }
                    else
                    {
                        NoLoad_Row[columna] = dr;
                        columna = 0;
                        View.Model.List_Nocargue.Rows.Add(NoLoad_Row);
                        NoLoad_Row = View.Model.List_Nocargue.NewRow();
                    }
                }), null);
            }
        }

        private void StartTimer2()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            t1 = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_cargue++;

                    if (cont_cargue == 1)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo";
                    }
                    else if (cont_cargue == 2)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo.";
                    }
                    else if (cont_cargue == 3)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo..";
                    }
                    else if (cont_cargue == 4 || cont_cargue > 4)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo...";
                        cont_cargue = 0;
                    }
                    // Implementación del método anónimo  && SerialesIngresados.Rows.Count > 0
                    if (estado_cargue == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = ((View.Model.ListRecords.Rows.Count * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t1.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Carga de archivo terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        int cont2 = 0;
        int cont_repeat2 = 0;
        private void StartTimerSerial()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tserial = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont2++;

                    if (cont2 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados";
                    }
                    else if (cont2 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados.";
                    }
                    else if (cont2 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados..";
                    }
                    else if (cont2 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados...";
                        cont2 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_serial == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat2 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tserial.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de seriales terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        int cont3 = 0;
        int cont_repeat3 = 0;
        private void StartTimerAlmacenado()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            talmacenado = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont3++;

                    if (cont3 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados";
                    }
                    else if (cont3 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados.";
                    }
                    else if (cont3 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados..";
                    }
                    else if (cont3 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales previamente ingresados...";
                        cont3 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_almacenado == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat3 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        talmacenado.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de seriales terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        #endregion

        public void CargarListDetails()
        {
            String ConsultaBuscar = "";
            DataTable RegistroValidado;
            foreach (DataRow dr in SerialesIngresados.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_Cargue.Invoke(new System.Action(() =>
                {
                    ConsultaBuscar = "EXEC sp_GetProcesos 'BUSCAREQUIPOSALMACENAR', '" + dr["SERIAL"].ToString() +"', NULL, NULL";
                    RegistroValidado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    DataRow RegistroGuardar = View.Model.ListRecords.NewRow();

                    //Asigno los campos
                    RegistroGuardar["Serial"] = dr[0].ToString();
                    RegistroGuardar["Mac"] = dr[1].ToString();
                    RegistroGuardar["Sap"] = dr[2].ToString();
                    RegistroGuardar["ProductoID"] = RegistroValidado.Rows[0]["ProductoID"].ToString();
                    RegistroGuardar["Fecha_RR"] = RegistroValidado.Rows[0]["Fecha_RR"].ToString();
                    RegistroGuardar["Fecha_SAP"] = RegistroValidado.Rows[0]["Fecha_SAP"].ToString();
                    RegistroGuardar["TLiberado"] = RegistroValidado.Rows[0]["TLiberado"].ToString();

                    View.Model.ListRecords.Rows.Add(RegistroGuardar);

                }), null);
            }

            adaptador.Dispose();
        }

        /*Actualiza la informacion de los combobox cambio de ubicacion, permite que una posicion que esta ocupada no aparecezca en el comobobox*/
        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                //List<String> validValues = new List<String>() { "A1A1", "A1A2" };
                DataTable dt_auxiliar = service.DirectSQLQuery("select posicion from dbo.EquiposCLARO where posicion is not null AND (estado LIKE 'ALMACENAMIENTO' OR estado LIKE 'DESPACHO')  group by posicion ", "", "dbo.EquiposCLARO", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("posicion"))
                           .ToList();

                var query = from item in View.Model.ListadoPosiciones
                            where !list.Contains(item.Name)
                            select item;

                View.Model.ListadoPosiciones = query.ToList();
                View.GetUbicacionPallet.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.ToString());
            }
        }

        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            foreach (DataRow item in View.Model.ListRecords.Rows)
            {
                if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
                {
                    Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }

            try
            {
                //Validacion existe o no el equipo en DB  select form table where para= ?
                //ConsultaBuscar = "SELECT * FROM dbo.EquiposCLARO WHERE upper(Serial) = upper('" + View.GetSerial1.Text.ToString() + "') AND (Estado IS NULL OR Estado ='CUARENTENA')";
                //DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);
                //Evaluo si el serial existe
                ConsultaBuscar = "EXEC sp_GetProcesos 'BUSCAREQUIPOSALMACENAR', '" + View.GetSerial1.Text + "', NULL, NULL";
                RegistroValidado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                //RegistroValidado = service.DirectSQLQuery("select RowID,serial,MAC,CODIGO_SAP,productoid,fecha_doc,fecha_doc_sap,estado_RR,ubicacion,estado from dbo.EquiposCLARO where serial = '" + View.GetSerial1.Text + "' and estado not like 'DESPACHO'", "", "dbo.EquiposCLARO", Local);

                //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                //RegistroValidado = service.DirectSQLQuery("EXEC sp_GetEstadoLiberacion 'BUSCAREQUIPO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposCLARO", Local);

                if (RegistroValidado.Rows.Count > 0)
                {
                    String estado = RegistroValidado.Rows[0]["estado"].ToString();

                    if (estado == "LIBERADO")
                    {
                        string doc_rr = RegistroValidado.Rows[0]["Fecha_RR"].ToString();
                        string doc_sap = RegistroValidado.Rows[0]["Fecha_SAP"].ToString();
                        string estado_rr = RegistroValidado.Rows[0]["estado_RR"].ToString();
                        string ubicacion = RegistroValidado.Rows[0]["Ubicacion"].ToString();
                        string tliberado = RegistroValidado.Rows[0]["TLiberado"].ToString();
                        if (doc_rr == null || doc_rr == "")
                        {
                            if (doc_sap == null || doc_sap == "")
                            {
                                Util.ShowMessage("El equipo se encuentra en cuarentena, no se a encontrado documento SAP ni RR.");
                                View.GetSerial1.Text = "";
                                View.GetSerial2.Text = "";
                                View.GetSerial1.Focus();
                            }
                            else
                            {
                                Util.ShowMessage("El equipo cuenta con documento de habilitación SAP (Fecha habilitación: " + doc_sap + "), pero todavia no tiene un documento RR.");
                                View.GetSerial1.Text = "";
                                View.GetSerial2.Text = "";
                                View.GetSerial1.Focus();
                            }
                        }
                        else
                        {
                            if (doc_sap == null || doc_sap == "")
                            {
                                Util.ShowMessage("El equipo cuenta con documento de habilitación RR (Fecha habilitación: " + doc_rr + ", Estado: " + estado_rr + "), pero todavia no tiene un documento SAP.");
                                View.GetSerial1.Text = "";
                                View.GetSerial2.Text = "";
                                View.GetSerial1.Focus();
                            }
                            else
                            {
                                if (estado_rr.Contains("IQ"))
                                {
                                    //Asigno los campos
                                    dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                                    dr["Serial"] = RegistroValidado.Rows[0]["serial"].ToString().ToUpper();
                                    dr["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString().ToUpper();
                                    dr["Sap"] = RegistroValidado.Rows[0]["CODIGO_SAP"].ToString().ToUpper();
                                    dr["ProductoID"] = RegistroValidado.Rows[0]["productoid"].ToString().ToUpper();
                                    dr["Fecha_RR"] = RegistroValidado.Rows[0]["Fecha_RR"].ToString().ToUpper();
                                    dr["Fecha_SAP"] = RegistroValidado.Rows[0]["Fecha_SAP"].ToString().ToUpper();
                                    dr["TLiberado"] = RegistroValidado.Rows[0]["TLiberado"].ToString().ToUpper();
                                    //Agrego el registro al listado
                                    View.Model.ListRecords.Rows.Add(dr);

                                    var border = (Border)VisualTreeHelper.GetChild(View.ListadoEquiposAlmacenamiento, 0);
                                    var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                                    scrollViewer.ScrollToBottom();
                                    //this.contFilas_byPallet++;

                                    if ((View.Model.ListRecords.Rows.Count % 50) == 0)
                                    {
                                        OnSaveDetails(null, e);
                                    }

                                    //Limpio los seriales para digitar nuevos datos
                                    View.GetSerial1.Text = "";
                                    View.GetSerial2.Text = "";
                                    View.GetSerial1.Focus();
                                }
                                else
                                {
                                    Util.ShowMessage("*El equipo tiene documento de habilitación SAP (Fecha habilitación: " + doc_sap + ")\n*Estado de habilitación RR (Fecha habilitación: " + doc_rr + ", Estado: " + estado_rr + "  -  Estado no valido para liberación).");
                                    View.GetSerial1.Text = "";
                                    View.GetSerial2.Text = "";
                                    View.GetSerial1.Focus();
                                }
                            }
                        }
                    }
                    else
                    {
                        Util.ShowMessage("El equipo no se encuentra LIBERADO, estado actual: (" + estado + ")");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                }
                else
                {
                    Util.ShowError("El equipo no se encuentra registrado en el sistema");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Se presento un error al momento de validar el serial. Error: " + Ex.Message);
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
                return;
            }
        }

        //private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        //{
        //    //Busco los registros
        //    BuscarRegistrosRecibo();
        //}

        /** Se ejecuta cuando se clickea en una fila del listado de pallets registrados **/
        private void OnSeleccionPalletConsulta(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
                return;

            View.GetCodPallet.Text = "";
            String aux_idpallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();

            //consulto los seriales contenidos en esa estiba
            String consultaSeleccion = "EXEC sp_GetProcesos 'BUSCAREQUIPOSSELECCIONALMACEN', '" + aux_idpallet + "', NULL, NULL";
            View.Model.ListRecords = service.DirectSQLQuery(consultaSeleccion, "", "dbo.EquiposCLARO", Local);

            DataRowView drv = (DataRowView)View.ListadoPalletsBusqueda.SelectedItem;
            this.codigoPallet = drv[0].ToString();

            this.ubicacionPallet = drv[1].ToString();

            this.seleccionUbicacion = true;

            this.contFilas_byPallet = View.Model.ListRecords.Rows.Count; // Guarda el numero de equipos almacenados en ese pallet
        }

        /** Se ejecuta cuando se levanta una tecla en el cuadro de texto de busqueda de pallets **/
        private void OnKeyConsultarPallet(object sender, KeyEventArgs e)
        {
            String tecla_idpallet = View.GetCodPalletBusqueda.Text.ToString();
            String ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARPALLETALMACENAMIENTO','" + tecla_idpallet + "','" + this.user + "','',''";

            View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
            this.Calcular_TotalEquipos();
        }

        /** Se ejecuta cuando se pulsa la tecla enter en el cuadro de texto de busqueda de pallets **/
        private void OnEnterConsultarPallet(object sender, KeyEventArgs e)
        {
            ConsultarPallets();
        }

        private void ConsultarPallets()
        {
            try
            {
                String ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARPALLETALMACENAMIENTO','','" + this.user + "','',''";
                Console.WriteLine(ConsultaSQL);
                View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
                this.Calcular_TotalEquipos();
            }
            catch(Exception ex){
                Util.ShowError(ex.Message);
            }
        }

        private void OnGenerarPallet(object sender, EventArgs e)
        {
            //DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            try
            {
                //Si el contador de equipos por pallet es igual a la lista de almacenamiento, quiere decir que no se agregaron nuevos equipos y se puede crear el nuevo pallet sin problema
                if (View.Model.ListRecords.Rows.Count == this.contFilas_byPallet)
                {

                    View.Model.ListRecords.Clear();

                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

                    if (Resultado.Rows.Count > 0)
                    {
                        //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                        RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposClaro WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposCLARO", Local);

                        //Evaluo si el serial existe
                        if (RegistroValidado.Rows.Count > 0)
                        {
                            Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
                        }
                        else
                        {
                            //Asigno los campos
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
                            ////Limpio los seriales para digitar nuevos datos
                            //View.GetSerial1.Text = "";
                            //View.GetSerial2.Text = "";
                            //View.GetSerial1.Focus();
                        }
                    }
                    else
                    {
                        Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
                    }
                }
                else if (this.codigoPallet == "")
                {
                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

                    if (Resultado.Rows.Count > 0)
                    {
                        //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                        RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposClaro WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposCLARO", Local);

                        //Evaluo si el serial existe
                        if (RegistroValidado.Rows.Count > 0)
                        {
                            Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
                            return;
                        }
                        else
                        {
                            //Asigno los campos
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
                            ////Limpio los seriales para digitar nuevos datos
                            //View.GetSerial1.Text = "";
                            //View.GetSerial2.Text = "";
                            //View.GetSerial1.Focus();
                        }
                    }
                    else
                    {
                        Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
                    }
                }
                else
                {
                    // Si no son iguales quiere decir que se agregaron nuevos equipos al pallet
                    ////Util.ShowMessage("Esta seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan");
                    //Console.WriteLine("Se borraran los seriales no guardados");

                    if (!UtilWindow.ConfirmOK("¿Está seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan?") == true)
                        return;
                    View.Model.ListRecords.Clear();

                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

                    if (Resultado.Rows.Count > 0)
                    {
                        //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                        RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposClaro WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposCLARO", Local);

                        //Evaluo si el serial existe
                        if (RegistroValidado.Rows.Count > 0)
                        {
                            Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
                        }
                        else
                        {
                            //Asigno los campos
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
                            ////Limpio los seriales para digitar nuevos datos
                            //View.GetSerial1.Text = "";
                            //View.GetSerial2.Text = "";
                            //View.GetSerial1.Focus();
                        }
                    }
                    else
                    {
                        Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
                    }
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de generar el pallet. Error: " + Ex.Message);
                return;
            }
        }

        //public void BuscarRegistrosRecibo()
        //{
        //    //Variables Auxiliares
        //    String ConsultaSQL;

        //    //Creo la consulta para buscar los registros
        //    ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIA_RECIBEALMANCEN', 'PARA ALMACENAMIENTO' ";

        //    //Valido si fue digitado una estiba para buscar
        //    if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
        //        ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
        //    else
        //        ConsultaSQL += ",NULL";

        //    //Ejecuto la consulta
        //    View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        //}

        //private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        //{
        //    //Variables Auxiliares
        //    String ConsultaSQL;

        //    //Limpio los campos de la busqueda
        //    View.BuscarEstibaRecibo.Text = "";
        //    //View.BuscarPosicionRecibo.SelectedIndex = -1;

        //    //Creo la consulta para buscar los registros
        //    ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIA_RECIBEALMANCEN', 'PARA ALMACENAMIENTO', NULL, NULL";

        //    //Ejecuto la consulta
        //    View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        //}

        public void BuscarRegistrosEntrega()
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ALMACENAMIENTO'";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        //public void OnConfirmarMovimiento(object sender, EventArgs e)
        //{
        //    //Variables Auxiliares
        //    String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, ConsultaTrack = "";

        //    //Evaluo que haya sido seleccionado un registro
        //    if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
        //        return;

        //    //Coloco la ubicacion
        //    NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

        //    //Obtengo el valor de origen de la primera fila
        //    //((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Origen"].ToString();

        //    int cont_noValidos = 0, cont_validos = 0; // Se cuentan los registros no validos para enviar a DESPACHO, aquellos que provengan de PRODUCCION(Reparacion, Diagnostico)

        //    //Valido la ubicacion para colocar el estado
        //    if (NuevaUbicacion == "DESPACHO")
        //    {
        //        NuevoEstado = "DESPACHO";

        //        foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
        //        {
        //            if (Registros.Row["Origen"].ToString() == "EMPAQUE")
        //            {
        //                //Creo la consulta para cambiar la ubicacion de la estiba
        //                ConsultaSQL = "EXEC sp_GetProcesos 'UPDATE_ALMACENAMIENTO','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "'";
        //                ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "' ";

        //                ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'PALLET RECIBIDO PARA DESPACHO','RECEPCIÓN ALMACENAMIENTO','Z DESPACHO','" + Registros.Row["Pallet"].ToString()
        //                            + "','','ALMACENAMIENTO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";

        //                //Ejecuto la consulta
        //                service.DirectSQLNonQuery(ConsultaSQL, Local);
        //                service.DirectSQLNonQuery(ConsultaTrack, Local);
        //                cont_validos++;
        //            }
        //            else
        //            {
        //                cont_noValidos++;
        //            }
        //        }
        //    }
        //    else if (NuevaUbicacion == "ALMACENAMIENTO")
        //    {
        //        NuevoEstado = "ALMACENAMIENTO";
        //        String Estado = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Estado"].ToString();

        //        ConsultaTrack = "declare @fechaActual datetime = getdate();";

        //        //Recorro el listado de registros seleccionados para confirmar el recibo
        //        foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
        //        {
        //            //Creo la consulta para cambiar la ubicacion de la estiba
        //            ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEALMACENAMIENTO_FROMDIAGNOSTICO','" + NuevoEstado + "', '" + ((Estado == "PARA SCRAP") ? "SCRAP" : NuevaUbicacion) + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "';";
        //            ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET FECHA_ING_ALMACEN = @fechaActual, ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "';";

        //            ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'PALLET RECIBIDO PARA ALMACENAMIENTO','RECEPCIÓN ALMACENAMIENTO','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + " ALMACEN','" + Registros.Row["Pallet"].ToString()
        //                + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "';";

        //            //Ejecuto la consulta
        //            service.DirectSQLNonQuery(ConsultaSQL, Local);
        //            service.DirectSQLNonQuery(ConsultaTrack, Local);
        //        }
        //    }

        //    ////Elimino las filas seleccionadas de la vista, sin necesidad de volver a consultar la BD VA DENTRO DEL FOR
        //    //while (View.ListadoBusquedaRecibo.SelectedItems.Count >= fila)
        //    //{
        //    //  if (View.Model.ListadoRecibo.Rows[fila]["Origen"].ToString() == "EMPAQUE")
        //    //  {
        //    //      View.Model.ListadoRecibo.Rows.RemoveAt(View.ListadoBusquedaRecibo.Items.IndexOf(View.ListadoBusquedaRecibo.SelectedItem));
        //    //  }
        //    // fila++;
        //    //}

        //    //Muestro el mensaje de confirmacion
        //    if (cont_noValidos > 0)
        //    {
        //        Util.ShowMessage(cont_validos + " pallet recibidos exitosamente, " + cont_noValidos + " pallets no se pueden enviar a DESPACHO,\n los pallets provenientes de PRODUCCIÓN no se pueden enviar a DESPACHO.");
        //    }
        //    else
        //    {
        //        Util.ShowMessage("Recibo de pallet realizado satisfactoriamente.");
        //    }

        //    View.Ubicacion.SelectedIndex = -1;

        //    View.UbicacionDesp.SelectedIndex = -1;

        //    //Busco los registros para actualizar el listado
        //    BuscarRegistrosRecibo();

        //    //Actualiza el combobox de posiciones
        //    this.Actualizar_UbicacionDisponible();
        //}

        //private void OnImprimir_Hablador(object sender, EventArgs e)
        //{
        //    //Variables Auxiliares
        //    String ConsultaSQL = "";
        //    DataTable SerialesImprimir;
        //    String destino = "";

        //    //Evaluo que haya sido seleccionado un registro
        //    if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
        //    {
        //        Util.ShowMessage("Debe seleccionar al menos un registro");
        //        return;
        //    }

        //    String NroSeriales = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Cantidad"].ToString();
        //    String fechaIngreso = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Fecha"].ToString();
        //    String pallet = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Pallet"].ToString();
        //    String modelo = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Modelo"].ToString();

        //    String origen = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Origen"].ToString();
        //    String estado = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Estado"].ToString();

        //    //Evaluo que haya seleccionado laexport plain text  nueva clasificacion
        //    if (View.Ubicacion.SelectedIndex == -1)
        //    {
        //        Util.ShowError("Por favor seleccionar la nueva clasificacion.");
        //        return;
        //    }
        //    else
        //    {
        //        destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

        //        destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
        //    }

        //    if (destino == "ALMACENAMIENTO")
        //    {

        //        if (origen == "PRODUCCION")
        //        {
        //            //Creo la base de la consulta para traer los seriales respectivos
        //            ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from dbo.EquiposClaro where codigoEmpaque LIKE '" + pallet + "'";

        //            //Ejecuto la consulta
        //            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

        //            //Imprimo los registros
        //            PrinterControl.PrintMovimientosBodega(SerialesImprimir, "PALLET", pallet, estado, "CLARO", "POSICIONAR EN ALMACEN", "", "aux");
        //        }
        //        else
        //        {
        //            Util.ShowMessage("    No es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de EMPAQUE deben enviarse a DESPACHO.");
        //        }
        //    }
        //    else if (destino == "DESPACHO")
        //    {

        //        if (origen == "EMPAQUE")
        //        {
        //            //Creo la base de la consulta para traer los seriales respectivos
        //            ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from Pallets_EmpaqueCLARO pallet join EquiposCLARO eqc on pallet.id_pallet = eqc.pila where pallet.codigo_pallet = '" + pallet + "'";

        //            //Ejecuto la consulta
        //            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

        //            //Imprimo los registros
        //            PrinterControl.PrintMovimientosBodega(SerialesImprimir, "PALLET", pallet, destino, "CLARO", "ALMACENAMIENTO - " + destino, "", "");
        //        }
        //        else
        //        {
        //            Util.ShowMessage("\tNo es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de PRODUCCIÓN no se pueden enviar a DESPACHO.");
        //        }
        //    }
        //}

        public void OnEliminarEquipo_Fila(object sender, EventArgs e)
        {
            //Confirmo si desea confirmar eliminar el/los equipos de la estiba en almacenamiento
            if (UtilWindow.ConfirmOK("¿Esta seguro de borrar los equipos del pallet " + codigoPallet + "?") == true)
            {
                if (View.ListadoEquiposAlmacenamiento.SelectedItems.Count == 0)
                {
                    Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");
                }
                else
                {
                    String ConsultaActualizar = "";
                    try
                    {
                        //Borra de la lista los elementos seleccionados
                        while (View.ListadoEquiposAlmacenamiento.SelectedItems.Count > 0)
                        {
                            //Construyo la consulta para guardar los datos
                            ConsultaActualizar += " UPDATE dbo.EquiposCLARO SET Ubicacion = NULL, Estado = 'LIBERADO', idPallet=NULL, posicion=NULL";
                            ConsultaActualizar += " WHERE RowID = '" + View.Model.ListRecords.Rows[View.ListadoEquiposAlmacenamiento.SelectedIndex][0].ToString() + "';";

                            ConsultaActualizar += " UPDATE dbo.TrackEquiposCLARO SET ESTIBA_ENTRADA = NULL, ESTADO_ALMACEN1 = NULL, UBICACION_ENTRADA=NULL, FECHA_ING_ALMACEN = NULL";
                            ConsultaActualizar += " WHERE Serial = '" + View.Model.ListRecords.Rows[View.ListadoEquiposAlmacenamiento.SelectedIndex][1].ToString() + "';";

                            View.Model.ListRecords.Rows.RemoveAt(View.ListadoEquiposAlmacenamiento.Items.IndexOf(View.ListadoEquiposAlmacenamiento.SelectedItem));
                        }

                        //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                        if (!String.IsNullOrEmpty(ConsultaActualizar))
                        {
                            //Ejecuto la consulta
                            service.DirectSQLNonQuery(ConsultaActualizar, Local);
                        }
                        Actualizar_UbicacionDisponible();
                        ConsultarPallets();
                    }
                    catch (Exception ex)
                    {
                        Util.ShowMessage("Error en eliminación de equipo - almacenamiento, " + ex.Message);
                    }
                }
            }
        }

        //public void CargarDatosDetails()
        //{
        //    //Variables Auxiliares
        //    GridViewColumn Columna;
        //    FrameworkElementFactory Txt;
        //    Assembly assembly;
        //    string TipoDato;

        //    //Inicializo el DataTable
        //    View.Model.ListRecords = new DataTable("ListadoRegistros");

        //    //Asigno las columnas
        //    View.Model.ListRecords.Columns.Add("RowID", typeof(String));
        //    View.Model.ListRecords.Columns.Add("Serial", typeof(String));
        //    View.Model.ListRecords.Columns.Add("Mac", typeof(String));
        //    View.Model.ListRecords.Columns.Add("Sap", typeof(String));
        //    View.Model.ListRecords.Columns.Add("ProductoID", typeof(String));
        //    View.Model.ListRecords.Columns.Add("Fecha_RR", typeof(String));
        //    View.Model.ListRecords.Columns.Add("Fecha_SAP", typeof(String));
        //    View.Model.ListRecords.Columns.Add("TLiberado", typeof(String));

        //    //Inicializo el DataTable
        //    View.Model.ListPallets_Almacenamiento = new DataTable("ListadoPallets");

        //    //Asigno las columnas
        //    View.Model.ListPallets_Almacenamiento.Columns.Add("Pallet", typeof(String));
        //    View.Model.ListPallets_Almacenamiento.Columns.Add("Posicion", typeof(String));
        //    View.Model.ListPallets_Almacenamiento.Columns.Add("Cantidad", typeof(String));
        //    View.Model.ListPallets_Almacenamiento.Columns.Add("FechaIngreso", typeof(String));
        //    View.Model.ListPallets_Almacenamiento.Columns.Add("Usuario", typeof(String));

        //    //View.Model.ListRecords.Columns.Add("Estado", typeof(String));
        //    //View.Model.ListRecords.Columns.Add("IdPallet", typeof(String));           

        //    //  Columna = new GridViewColumn();
        //    //  assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
        //    //  TipoDato = "System.Windows.Controls.ComboBox";
        //    //  Columna.Header = "Ubicaciones";
        //    //  Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
        //    //  Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoPosiciones);
        //    //  Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
        //    //  //Txt.SetValue(ComboBox.SelectedValuePathProperty, "MetaMasterID");
        //    //  Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
        //    //// Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Posicion"));
        //    //  Txt.SetValue(ComboBox.WidthProperty, (double)110);

        //    //// add textbox template
        //    //Columna.CellTemplate = new DataTemplate();
        //    //Columna.CellTemplate.VisualTree = Txt;
        //    //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
        //    //View.Model.ListRecords.Columns.Add("Posicion", typeof(String)); //Creacion de la columna en el DataTable

        //    //COLUMNA ESTADO
        //    IList<MMaster> ListadoEstados = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOS-AL" } });
        //    Columna = new GridViewColumn();
        //    assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
        //    TipoDato = "System.Windows.Controls.ComboBox";
        //    Columna.Header = "Estado";
        //    Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
        //    Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoEstados);
        //    Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
        //    Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
        //    Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_SAP"));
        //    Txt.SetValue(ComboBox.WidthProperty, (double)110);


        //    // add textbox template
        //    Columna.CellTemplate = new DataTemplate();
        //    Columna.CellTemplate.VisualTree = Txt;
        //    View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
        //    View.Model.ListRecords.Columns.Add("ESTADO_SAP", typeof(String)); //Creacion de la columna en el DataTable
        //}

        public void CargarDatosDetails()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Sap", typeof(String));
            View.Model.ListRecords.Columns.Add("ProductoID", typeof(String));
            View.Model.ListRecords.Columns.Add("Fecha_RR", typeof(String));
            View.Model.ListRecords.Columns.Add("Fecha_SAP", typeof(String));
            View.Model.ListRecords.Columns.Add("TLiberado", typeof(String));

            // Datatable lista de seriales no cargados
            View.Model.List_Nocargue = new DataTable("ListadoNoCargue");
            View.Model.List_Nocargue.Columns.Add("Serial", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue

            #region Columna Estado Material.

            IList<MMaster> ListadoSerializados = service.GetMMaster(new MMaster { MetaType = new MType { Code = "SI _NO" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ensamblado";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoSerializados);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ENSAMBLADO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ENSAMBLADO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        {
            //Variables Auxiliares
            DataRow RegistroGuardar;
            DataTable RegistroValidado;
            DataTable SerialesIngresados;
            Boolean Existe;
            Boolean aux;
            List<String> listRepetidos = new List<string>(); //Guarda los seriales que estan repetidos en el carge masivo

            //Valido la existencia de los equipos ingresados
            SerialesIngresados = ValidarSerialesIngresados(e.Value);

            foreach (DataRow dr in e.Value.Rows)
            {
                //Iniciamos la variable para validar existencia
                Existe = false;

                //Validamos si el serial existe o no en el sistema
                foreach (DataRow dr1 in SerialesIngresados.Rows)
                {
                    if (dr1["Serial"].ToString().ToUpper() == dr[0].ToString().ToUpper())
                    {
                        Existe = true;
                        break;
                    }
                }

                //Valido el serial si existe
                if (Existe)
                    continue;

                aux = true;

                //Recorro el listado de equipos ingresados al archivo plano para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {

                    if (dr[0].ToString().ToUpper() == item["Serial"].ToString().ToUpper())
                    {
                        //Util.ShowError("El serial " + dr[0].ToString() + " y Mac " + dr[1].ToString() + " esta repetido, por favor verificar.");
                        aux = false;
                        listRepetidos.Add(dr[0].ToString());
                    }
                }

                //Inicializo el registro para guardar el dato
                RegistroGuardar = View.Model.ListRecords.NewRow();

                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (dr[0].ToString().ToUpper() == item["Serial"].ToString().ToUpper())
                        continue;
                }

                try
                {
                    if (aux)
                    {
                        //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                        RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPO_ALM','" + dr[0].ToString() + "'", "", "dbo.EquiposCLARO", Local);

                        //Evaluo si el serial existe
                        if (RegistroValidado.Rows.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            //Asigno los campos
                            RegistroGuardar["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                            RegistroGuardar["ProductoID"] = RegistroValidado.Rows[0]["Producto"].ToString();
                            RegistroGuardar["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                            RegistroGuardar["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString();
                            RegistroGuardar["IdPallet"] = dr[2].ToString();
                            RegistroGuardar["Posicion"] = dr[3].ToString();
                            RegistroGuardar["ESTADO_SAP"] = dr[4].ToString();

                            //Agrego el registro al listado
                            View.Model.ListRecords.Rows.Add(RegistroGuardar);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Util.ShowError(Ex.ToString());
                }
            }

            //Si existen seriales repetidos muestra en un dialog los seriales
            if (listRepetidos.Count > 0)
            {
                String cadena = "";

                if (listRepetidos.Count > 0)
                {
                    cadena = cadena + "[FILAS REPETIDAS, SERIALES TRUNCADOS]: \n\n";
                    //Impresion de seriales no aceptados

                    foreach (var item in listRepetidos)
                    {
                        if (!cadena.Contains(item))
                        {
                            cadena = cadena + " " + item + "\n";
                        }
                    }
                }
                Util.ShowError(cadena);
            }
        }

        private void OnReplicateDetails(object sender, EventArgs e)
        {
            //Obtenemos el indice del encabezado
            if (View.ListadoEquiposAlmacenamiento.SelectedIndex != -1)
            {
                if (View.ListadoEquiposAlmacenamiento.SelectedItems.Count > 1)// Se selecciona mas de una fila
                {
                    int indice_fila1 = View.ListadoEquiposAlmacenamiento.SelectedIndex;

                    //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                    foreach (DataRowView dr in View.ListadoEquiposAlmacenamiento.SelectedItems)
                    {
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            dr.Row[z] = View.Model.ListRecords.Rows[indice_fila1][z];
                    }
                }
                else
                {
                    int SComp;
                    SComp = View.ListadoEquiposAlmacenamiento.SelectedIndex;
                    //recorre la lista desde la fila seleccionada y setea las filas que esten vacias
                    for (int i = SComp; i < View.Model.ListRecords.Rows.Count; i++)
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            if (View.Model.ListRecords.Rows[i][z].ToString().Equals(""))
                                View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[SComp][z];
                }
            }
            else
            {
                Util.ShowMessage("Debe seleccionar una fila para replicar la informacion");
            }
        }

        private void OnSaveDetails(object sender, EventArgs e)
        {
            

            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "";
            String ConsultaGuardarTrack = "";
            Int32 ContadorFilas = 0;
            DataTable SerialesIngresados;
            Boolean Existe;
            String ubicacion = "";

            if (View.ListadoPalletsBusqueda.SelectedIndex != -1 && View.GetCodPallet.Text.ToString() == "")
            {
                this.codigoPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
                ubicacion = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Posicion"].ToString();
            }
            else
            {
                this.codigoPallet = View.GetCodPallet.Text.ToString();
                ubicacion = (this.seleccionUbicacion) ? this.ubicacionPallet : View.GetUbicacionPallet.SelectedValue.ToString();
            }
            //Valido la existencia de los equipos ingresados
            //SerialesIngresados = ValidarSerialesIngresados(View.Model.ListRecords);   // NO ES NECESARIO VALIDAR LA EXISTENCIA CUANDO NO ESTA HABILITADO EL CARGUE MASIVO
            try
            {
                for (int i = this.contFilas_byPallet; i < View.Model.ListRecords.Rows.Count; i++)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;

                    //String ubicacion = (this.seleccionUbicacion) ? this.ubicacionPallet : View.GetUbicacionPallet.SelectedValue.ToString();


                    if (ubicacion != "" && ubicacion != null)
                    {
                        if (this.codigoPallet != "")
                        {
                            //Valido que la ubicacion no este ocupada por otro pallet 
                            String consulta = "select top 1 idpallet from dbo.EquiposClaro where Posicion LIKE '" + ubicacion + "' AND idpallet NOT LIKE '" + this.codigoPallet + "';";
                            DataTable aux = service.DirectSQLQuery(consulta, "", "dbo.EquiposClaro", Local);

                            if (aux.Rows.Count > 0)
                            {
                                Util.ShowMessage("La posición seleccionada ya se encuentra ocupada por el pallet " + aux.Rows[0]["idpallet"].ToString());
                                Actualizar_UbicacionDisponible();
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Posicion = '" + ubicacion + "', IdPallet = '" + this.codigoPallet + "', Ubicacion = 'ALMACENAMIENTO', Estado= 'ALMACENAMIENTO'  WHERE Serial = '" + View.Model.ListRecords.Rows[i]["Serial"].ToString() + "';";
                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTIBA_ENTRADA = '" + this.codigoPallet + "', ESTADO_ALMACEN1 = 'ALMACENAMIENTO', UBICACION_ENTRADA = 'ALMACENAMIENTO', FECHA_ING_ALMACEN = GETDATE() WHERE Serial = '" + View.Model.ListRecords.Rows[i]["Serial"].ToString() + "'";
                                ConsultaGuardar += "EXEC sp_InsertarNuevo_Movimiento 'ALMACENAMIENTO DE EQUIPOS EN BODEGA','ESPERANDO POR ENVIO A PRODUCCION','" + ubicacion  + "','" + View.GetCodPallet.Text + "','','ALMACENAMIENTO','ALMACENARENBODEGA','" + this.user + "','" + View.Model.ListRecords.Rows[i]["Serial"].ToString() + "';";
                
                            }
                        }
                        else
                        {
                            Util.ShowMessage("Debe generar un código de pallet");
                            return;
                        }
                    }
                    else
                    {
                        Util.ShowMessage("Se debe seleccionar una ubicación para el pallet");
                        return;
                    }
                }


                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {

                    Console.WriteLine(ConsultaGuardar);
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);
                    service.DirectSQLNonQuery(ConsultaGuardarTrack, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                    ConsultaGuardarTrack = "";

                    //Muestro el mensaje de confirmacion
                    Util.ShowMessage("Registros guardados satisfactoriamente.");
                    //Reinicio los campos
                    LimpiarDatosIngresoSeriales();
                    View.GetCodPallet.Text = "";
                    View.GetUpLoadFile.IsEnabled = true;
                    Actualizar_UbicacionDisponible();
                    this.ubicacionPallet = "";
                    this.contFilas_byPallet = 0;
                    this.codigoPallet = "";
                    ConsultarPallets();

                }
            }
            catch (Exception Ex) { Util.ShowError("Se presento un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        /**
         * Metodo utilizado para obtener referencia al datatable de estibas cuando se pulsa sobre una fila
         */
        //private void OnFilaSeleccionada(object sender, SelectionChangedEventArgs e)
        //{
        //    int total = 0;
        //    int num_estibas = View.ListadoBusquedaRecibo.SelectedItems.Count;

        //    foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
        //    {
        //        total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());
        //    }

        //    View.TotalSeriales.Text = total.ToString();
        //    View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        //}

        public DataTable ValidarSerialesIngresados(DataTable ListadoSeriales)
        {
            //Variables Auxiliares
            DataTable SerialesIngresados;
            String CadenaSQL = "";
            Int32 CantidadRegistros;

            //Creo la cadena de consulta
            CadenaSQL += "SELECT * FROM dbo.EquiposClaro WHERE UPPER(Serial) IN (";

            //Obtengo la cantidad de registros
            CantidadRegistros = ListadoSeriales.Rows.Count;

            //Recorremos el listado
            foreach (DataRow dr in ListadoSeriales.Rows)
            {
                //Evaluo si es el ultimo registro para crear la cadena sin la coma
                if (CantidadRegistros == 1)
                {
                    //Adiciono los valores a la cade de cosulta
                    CadenaSQL += "'" + dr[0].ToString().ToUpper() + "') AND Estado != 'DESPACHADO'";
                }
                else
                {
                    //Adiciono los valores a la cade de cosulta
                    CadenaSQL += "'" + dr[0].ToString().ToUpper() + "',";
                }

                //Disminuyo el contador
                CantidadRegistros--;
            }

            //Ejecutamos la consulta
            Console.WriteLine(CadenaSQL);

            SerialesIngresados = service.DirectSQLQuery(CadenaSQL, "", "dbo.EquiposClaro", Local);

            //Retornamos el resultado
            return SerialesIngresados;
        }

        /*
         * Calcula el total de los equipos ubicados en los pallets
         */
        public void Calcular_TotalEquipos()
        {
            int acum = 0;

            if (View.Model.ListPallets_Almacenamiento.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow Registros in View.Model.ListPallets_Almacenamiento.Rows)
            {
                acum += Int32.Parse(Registros["Cantidad"].ToString());
            }

            View.GetTotalEquipos.Text = acum.ToString();

            
        }

        public void OnImprimirHablador(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String destino = "ALMACENAMIENTO";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

            String NroSeriales = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Cantidad"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();

            //Creo la base de la consulta para traer los seriales respectivos
            ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from Pallets_EmpaqueCLARO pallet join EquiposCLARO eqc on pallet.id_pallet = eqc.pila where pallet.codigo_pallet = '" + pallet + "'";

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            if (SerialesImprimir.Rows.Count == 0)
            {
                ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from EquiposCLARO where idpallet LIKE '" + pallet + "' OR CodigoEmpaque2 LIKE '" + pallet + "' AND Estado LIKE 'DESPACHO'";

                //Ejecuto la consulta
                SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
            }

            //Imprimo los registros
            PrinterControl.PrintMovimientosBodega(this.userName, SerialesImprimir, "PALLET", pallet, destino, "CLARO", "ALMACENAMIENTO", "", "CLARO");
        }

        public void OnKillProcess(object sender, EventArgs e)
        {
            LimpiarList();
            //detengo procesos activos de excel para el cargue masivo
            Process[] proceso = Process.GetProcessesByName("EXCEL");

            if (proceso.Length > 0)
                proceso[0].Kill();
        }

        public void LimpiarList()
        {
            View.Model.ListRecords.Rows.Clear();
            View.Model.List_Nocargue.Rows.Clear();
        }

        #endregion
    }
}