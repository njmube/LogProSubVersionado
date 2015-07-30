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
using System.Threading;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WpfFront.Presenters
{

    public interface IReciboPresenter
    {
        IReciboView View { get; set; }
        ToolWindow Window { get; set; }
    }

    public class ReciboPresenter : IReciboPresenter
    {
        public IReciboView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 3; //# columnas que no se debe replicar porque son fijas.

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        private DataTable SerialesIngresados = new DataTable();
        private DataRow NoLoad_Row = null;
        private Timer t;
        private Timer t1;
        private Timer tserial;
        private Timer treceiver;
        private Timer tsmart;
        private Thread updateLabelThread;
        private Boolean estado_cargue = false, busqueda_Repetidos = false, busqueda_serial = false, busqueda_receiver = false, busqueda_smart = false;
        private OleDbConnection oledbcon;
        private OleDbDataAdapter adaptador;
        private Thread hilo_repetidos;

        private Thread hilo_saveSerial;
        private DataTable serialesSave_Aux;
        private Timer trOnSave;
        int cont_seriales = 0;
        private Int32 ContadorSave;
        private Boolean estado_almacenamiento = false;

        //PREALERTA
        private DataTable SerialesIngresadosPrea = new DataTable();
        private DataRow NoLoad_RowPrea = null;
        private Thread hilo_prealerta;
        private Boolean estado_cargueprea = false, busqueda_Repetidosprea = false;
        private Timer tprea;
        private Timer tcargueprea;

        private Thread hilo_savePrealerta;
        private DataTable serialesSave_AuxPrea;
        private Timer trOnSavePrea;
        int cont_serialesPrea = 0;
        private Int32 ContadorSavePrea;
        private Boolean estado_almacenamientoPrea = false;

        public ReciboPresenter(IUnityContainer container, IReciboView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ReciboModel>();

            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<EventArgs>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            view.KillProcess += new EventHandler<EventArgs>(this.OnKillProcess);
            View.ExportCargue += new EventHandler<EventArgs>(this.OnExportCargue);

            //PREALERTA
            view.CargaPrealerta += new EventHandler<EventArgs>(this.OnCargaPrealerta);
            View.SaveDetailsPrealerta += new EventHandler<EventArgs>(this.OnSaveDetailsPrealerta);
            View.ExportCarguePrea += new EventHandler<EventArgs>(this.OnExportCarguePrea);

            #endregion

            #region Datos

            //Cargo la variable para las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo los productos
            View.Model.ListadoProductos = service.GetProduct(new Product { Reference = "1" });

            //Cargo los lsitados de los combobx
            View.Model.ListadoModelosDescripcion = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DRIRECMODE" } });

            //Cargo los datos del listado
            CargarDatosDetails();

            #endregion
        }

        #region Metodos

        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            String ConsultaBuscar = "";
            String ConsultaBuscar1 = "";
            String consultaCruzar = "";
            //System.Windows.MessageBox.Show(View.GetSerial1.Text.ToString());

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            // Andres Leonardo Arevalo - 06 feb 2015
            if (String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
            {
                Util.ShowError("El campo Receiver no puede ser vacio.");
                return;
            }
            // Andres Leonardo Arevalo - 06 feb 2015

            String serial = View.GetSerial1.Text.ToString();
            String validar = serial.Substring(serial.Length - 9);

            //cruzar los ultimos 9 digitos
            consultaCruzar = "SELECT TOP 1 * FROM dbo.preAlerta_EQUIPOSDIRECTV WHERE upper(aleEquip_serial) = upper('" + validar + "')";
            DataTable ResultadoCruce = service.DirectSQLQuery(consultaCruzar, "", "dbo.preAlerta_EQUIPOSDIRECTV", Local);

            if (ResultadoCruce.Rows.Count == 0)
            {
                Util.ShowError("NOVEDAD: El equipo no ingreso en ninguna prealerta!");
            }
            
            //Validacion existe o no el equipo en DB
            ConsultaBuscar = "SELECT TOP 1 * FROM dbo.EquiposDIRECTVC WHERE upper(SERIAL) = upper('" + View.GetSerial1.Text.ToString() + "') AND ESTADO = 'DESPACHADO'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            if (Resultado.Rows.Count > 0)
            {
                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
                    {
                        Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
                        return;
                    }
                }

                //Asigno los campos
                //dr["Producto"] = RegistroValidado.Rows[0]["Producto"].ToString();
                dr["Serial"] = View.GetSerial1.Text.ToString().ToUpper();
                dr["Receiver"] = View.GetSerial2.Text.ToString().ToUpper();
                dr["SmartCard"] = View.GetSerial3.Text.ToString().ToUpper();

                if (ResultadoCruce.Rows.Count == 0)
                    dr["Novedad"] = "SI";
                else
                    dr["Novedad"] = "NO";

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr);

                //Limpio los seriales para digitar nuevos datos
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial3.Text = "";
                View.GetSerial1.Focus();
            }
            else
            {
                //System.Windows.MessageBox.Show("No encontro el dispositivo en la BD, DISPOSITIVO NUEVO" + Resultado.Rows.Count + " - " + View.GetSerial3.Text.ToString());
                ConsultaBuscar = "SELECT TOP 1 * FROM dbo.EquiposDIRECTVC WHERE upper(Serial) = upper('" + View.GetSerial1.Text.ToString() + "')";
                ConsultaBuscar1 = "SELECT TOP 1 * FROM dbo.EquiposDIRECTVC WHERE upper(SMART_CARD_ENTRADA) = upper('" + View.GetSerial3.Text.ToString() + "')";
                
                DataTable Result = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);
                DataTable Result1 = service.DirectSQLQuery(ConsultaBuscar1, "", "dbo.EquiposDIRECTVC", Local);

                if (Result.Rows.Count > 0)
                {
                    Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el sistema. Por favor verificar");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial3.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }

                /*** Andres Leonardo Arevalo - 06 feb 2015 ***/
                if (View.GetSerial3.Text != "" && Result1.Rows.Count > 0)
                {
                    Util.ShowError("La smart card " + View.GetSerial3.Text + " ya esta en el sistema. Por favor verificar");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial3.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
                /*** Andres Leonardo Arevalo - 06 feb 2015 ***/

                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
                    {
                        Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial3.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    } /*** Andres Leonardo Arevalo - 09 feb 2015 ***/
                    else if ((View.GetSerial3.Text.ToUpper() != "") && (item["SmartCard"].ToString().ToUpper() != "") && (View.GetSerial3.Text.ToUpper() == item["SmartCard"].ToString().ToUpper()))
                    {
                        Util.ShowError("La smart card " + View.GetSerial3.Text + " ya esta en el listado.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial3.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    /*** Andres Leonardo Arevalo - 09 feb 2015 ***/
                }

                //Asigno los campos
                //dr["Producto"] = RegistroValidado.Rows[0]["Producto"].ToString();
                dr["Serial"] = View.GetSerial1.Text.ToString().ToUpper();
                dr["Receiver"] = View.GetSerial2.Text.ToString().ToUpper();
                dr["SmartCard"] = View.GetSerial3.Text.ToString().ToUpper();

                if (ResultadoCruce.Rows.Count == 0)
                    dr["Novedad"] = "SI";
                else
                    dr["Novedad"] = "NO";

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr);

                //Limpio los seriales para digitar nuevos datos
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial3.Text = "";
                View.GetSerial1.Focus();
            }
        }


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
        private void StartTimerReceiver()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            treceiver = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont3++;

                    if (cont3 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando ID Receiver";
                    }
                    else if (cont3 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando ID Receiver.";
                    }
                    else if (cont3 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando ID Receiver..";
                    }
                    else if (cont3 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando ID Receiver...";
                        cont3 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_receiver == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat3 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        treceiver.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Validacion de ID Receiver terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        int cont4 = 0;
        int cont_repeat4 = 0;
        private void StartTimerSmart()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tsmart = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont4++;

                    if (cont4 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart Card";
                    }
                    else if (cont4 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart Card.";
                    }
                    else if (cont4 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart Card..";
                    }
                    else if (cont4 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart Card...";
                        cont4 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_smart == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat4 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tsmart.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de Smart Card terminada.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
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
                            t1.Dispose();
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
                                                                                        SerialesIngresados.Rows[j]["ID_RECEIVER"].ToString(), 
                                                                                        SerialesIngresados.Rows[j]["SMART_CARD"].ToString(),
                                                                                        SerialesIngresados.Rows[j]["MODELO"].ToString(),
                                                                                        SerialesIngresados.Rows[j]["ORIGEN"].ToString(),
                                                                                        SerialesIngresados.Rows[j]["DOA"].ToString(),
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
            t.Dispose();

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
                ConsultaBuscarSerial = "SELECT serial from dbo.EquiposDIRECTVC WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp2]["SERIAL"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSerial, "", "dbo.EquiposDIRECTVC", Local);

                

                if (Resultado.Rows.Count > 0)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp2]["SERIAL"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp2]["ID_RECEIVER"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp2]["SMART_CARD"].ToString(),
                                                                                        SerialesIngresados.Rows[temp2]["MODELO"].ToString(),
                                                                                        SerialesIngresados.Rows[temp2]["ORIGEN"].ToString(),
                                                                                        SerialesIngresados.Rows[temp2]["DOA"].ToString(),
                                                                                        "Serial existente en el sistema."});

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

            StartTimerReceiver();

            //validamos que los SAP existan en la base de datos
            String ConsultaBuscarReceiver = "";
            int temp3 = 0;
            while (temp3 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarReceiver = "SELECT RECEIVER FROM dbo.EquiposDIRECTVC WHERE upper(RECEIVER)= UPPER('" + SerialesIngresados.Rows[temp3]["ID_RECEIVER"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarReceiver, "", "dbo.EquiposDIRECTVC", Local);

                

                if (Resultado.Rows.Count > 0)
                {

                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp3]["SERIAL"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp3]["ID_RECEIVER"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp3]["SMART_CARD"].ToString(),
                                                                                        SerialesIngresados.Rows[temp3]["MODELO"].ToString(),
                                                                                        SerialesIngresados.Rows[temp3]["ORIGEN"].ToString(),
                                                                                        SerialesIngresados.Rows[temp3]["DOA"].ToString(),
                                                                                        "ID Receiver registrado en el sistema."});

                    SerialesIngresados.Rows.RemoveAt(temp3);
                }
                else
                {
                    temp3++;
                }
                cont_repeat3++;
            }

            busqueda_receiver = true;
            Thread.Sleep(1000);
            treceiver.Dispose();

            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);

            StartTimerSmart();

            //validamos que los seriales existan en la base de datos
            String ConsultaBuscarSmart = "";
            int temp4 = 0;
            while (temp4 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarSmart = "SELECT SMART_CARD_ENTRADA FROM dbo.EquiposDIRECTVC WHERE UPPER(SMART_CARD_ENTRADA) = UPPER('" + SerialesIngresados.Rows[temp4]["SMART_CARD"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSmart, "", "dbo.EquiposDIRECTVC", Local);

                if (Resultado.Rows.Count > 0)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp4]["SERIAL"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp4]["ID_RECEIVER"].ToString(), 
                                                                                        SerialesIngresados.Rows[temp4]["SMART_CARD"].ToString(),
                                                                                        SerialesIngresados.Rows[temp4]["MODELO"].ToString(),
                                                                                        SerialesIngresados.Rows[temp4]["ORIGEN"].ToString(),
                                                                                        SerialesIngresados.Rows[temp4]["DOA"].ToString(),
                                                                                        "Smart Card registrado en el sistema."});

                    SerialesIngresados.Rows.RemoveAt(temp4);
                }
                else
                {
                    temp4++;
                }
                cont_repeat4++;
            }

            busqueda_smart = true;
            Thread.Sleep(1000);
            tsmart.Dispose();

            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);


            this.MostrarErrores_Cargue(listNoCargue); // Agrega a un segundo listview los equipos que no fueron cargados

            StartTimer2();

            cont = 0;
            cont2 = 0;
            cont3 = 0;
            cont4 = 0;

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

        public void CargarListDetails()
        {
            foreach (DataRow dr in SerialesIngresados.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_Cargue.Invoke(new System.Action(() =>
                {
                    DataRow RegistroGuardar = View.Model.ListRecords.NewRow();

                    //Asigno los campos
                    RegistroGuardar["Serial"] = dr[0].ToString();
                    RegistroGuardar["Receiver"] = dr[1].ToString();
                    RegistroGuardar["SmartCard"] = dr[2].ToString();
                    RegistroGuardar["MODELO"] = dr[3].ToString();
                    RegistroGuardar["DESCRIPCION"] = dr[4].ToString();
                    RegistroGuardar["TIPO_ORIGEN"] = dr[5].ToString();
                    //RegistroGuardar["FECHA_INGRESO"] = dr[5].ToString();
                    RegistroGuardar["TIPOS_DEVOLUCIONES"] = dr[6].ToString();
                    RegistroGuardar["DOA"] = dr[7].ToString();
                    //RegistroGuardar["FECHA_DOC"] = dr[7].ToString();
                    RegistroGuardar["Ciudad"] = dr[8].ToString();
                    //RegistroGuardar["DOC_INGRESO"] = dr[10].ToString();
                    RegistroGuardar["ESTADO_MATERIAL"] = dr[9].ToString();
                    //Agrego el registro al listado
                    View.Model.ListRecords.Rows.Add(RegistroGuardar);

                }), null);
            }

            adaptador.Dispose();
        }

        private void OnReplicateDetailsBy_Column(object sender, RoutedEventArgs e)
        {
            //Obtiene una referencia del encabezado de la lista
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            //Cuando se selecciona uno de los select dentro de la lista se ejecuta este metodo por eso se valida que sea el encabezado
            if (headerClicked != null)
            {
                //Obtenemos el indice del encabezado
                var index = View.ListadoEquipos.Columns.IndexOf(headerClicked.Column);

                if (View.ListadoEquiposAProcesar.SelectedIndex != -1)
                {

                    if (View.ListadoEquiposAProcesar.SelectedItems.Count > 1)// Se selecciona mas de una fila
                    {
                        DataRowView drv = (DataRowView)View.ListadoEquiposAProcesar.SelectedItem;
                        String valueOfItem = drv[index].ToString();

                        //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                        foreach (DataRowView dr in View.ListadoEquiposAProcesar.SelectedItems)
                        {
                            dr[index] = valueOfItem;
                        }
                    }
                    else
                    {
                        //Filtramos las columnas descartando las que no son para replicar
                        if (index >= offset)
                        {
                            for (int i = View.ListadoEquiposAProcesar.SelectedIndex; i < View.Model.ListRecords.Rows.Count; i++)
                                View.Model.ListRecords.Rows[i][index] = View.Model.ListRecords.Rows[View.ListadoEquiposAProcesar.SelectedIndex][index];
                        }
                    }
                }
            }
        }


        private void OnDeleteDetails(object sender, EventArgs e)
        {
            if (View.ListadoEquiposAProcesar.SelectedItems.Count == 0)
                Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");

            //borra de la lista los elementos seleccionados
            while (View.ListadoEquiposAProcesar.SelectedItems.Count > 0)
                View.Model.ListRecords.Rows.RemoveAt(View.ListadoEquiposAProcesar.Items.IndexOf(View.ListadoEquiposAProcesar.SelectedItem));
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas restantes            
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("SmartCard", typeof(String));
            View.Model.ListRecords.Columns.Add("Novedad", typeof(String));

            // Datatable lista de seriales no cargados
            View.Model.List_Nocargue = new DataTable("ListadoNoCargue");
            View.Model.List_Nocargue.Columns.Add("Serial", typeof(String));
            View.Model.List_Nocargue.Columns.Add("IDReceiver", typeof(String));
            View.Model.List_Nocargue.Columns.Add("SmartCard", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Modelo", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Origen", typeof(String));
            View.Model.List_Nocargue.Columns.Add("DOA", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue

            //Datatable lista de prealerta
            View.Model.ListPrealerta = new DataTable("ListadoPrealerta");
            View.Model.ListPrealerta.Columns.Add("Codigo", typeof(String));
            View.Model.ListPrealerta.Columns.Add("Tipo", typeof(String));
            View.Model.ListPrealerta.Columns.Add("Serial", typeof(String));
            View.Model.ListPrealerta.Columns.Add("RID", typeof(String));
            View.Model.ListPrealerta.Columns.Add("SerialVinculado", typeof(String));
            View.Model.ListPrealerta.Columns.Add("Estado", typeof(String));

            //Datatable lista de seriales no cargados en prealerta
            View.Model.List_NocarguePrea = new DataTable("ListadoNoCarguePrealerta");
            View.Model.List_NocarguePrea.Columns.Add("Codigo", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("Tipo", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("Serial", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("RID", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("SerialVinculado", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("Estado", typeof(String));
            View.Model.List_NocarguePrea.Columns.Add("Motivo", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void OnReplicateDetails(object sender, EventArgs e)
        {
            //Obtenemos el indice del encabezado
            if (View.ListadoEquiposAProcesar.SelectedIndex != -1)
            {
                if (View.ListadoEquiposAProcesar.SelectedItems.Count > 1)// Se selecciona mas de una fila
                {
                    int indice_fila1 = View.ListadoEquiposAProcesar.SelectedIndex;

                    //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                    foreach (DataRowView dr in View.ListadoEquiposAProcesar.SelectedItems)
                    {
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            dr.Row[z] = View.Model.ListRecords.Rows[indice_fila1][z];
                    }
                }
                else
                {
                    int SComp;
                    SComp = View.ListadoEquiposAProcesar.SelectedIndex;
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

            hilo_saveSerial = new Thread(new ParameterizedThreadStart(SaveSerial));
            hilo_saveSerial.SetApartmentState(ApartmentState.STA);
            hilo_saveSerial.IsBackground = true;
            hilo_saveSerial.Priority = ThreadPriority.Highest;
            cont_seriales = 0;
            this.ContadorSave = 0;

            this.serialesSave_Aux = View.Model.ListRecords;

            View.Progress_Cargue.Value = 0;
            estado_almacenamiento = false;
            View.GetEstado_Cargue.Text = "Iniciando guardado";
            hilo_saveSerial.Start(View.Model.ListRecords); //Se inicia el proceso background utilizando el Hilo

            StartTimer_SerialesOnSaveBd(); //Inicia el timmer para mostrar visualmente el progreso del guardado en la BD
 
        }

        private void StartTimer_SerialesOnSaveBd()
        {
            // Creamos diferentes hilos a través de un temporizador
            trOnSave = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_seriales++;

                    if (cont_seriales == 1)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos";
                    }
                    else if (cont_seriales == 2)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos.";
                    }
                    else if (cont_seriales == 3)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos..";
                    }
                    else if (cont_seriales == 4 || cont_seriales > 4)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos...";
                        cont_seriales = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_almacenamiento == false)
                    {
                        View.Progress_Cargue.Value = ((ContadorSave * 100D) / View.Model.ListRecords.Rows.Count);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trOnSave.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Proceso de guardado terminado.";
                            View.Progress_Cargue.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 300);
        }

        private void SaveSerial(Object e)
        {
            //Variables Auxiliares
            String ConsultaGuardar = "";
            String ConsultaGuardar1 = "";
            String ConsultaGuardarMovimiento = "";
            Int32 ContadorCampos, ContadorFilas = 0;
            String Estado = "";
            
            try
            {

                String ConsultaSemana = "EXEC sp_GetProcesosDIRECTVC 'OBTENERSEMANA'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaSemana, "", "dbo.EquiposDIRECTVC", Local);

                

                ConsultaGuardar = "Declare @RowId int ";
                foreach (DataRow DataRow in this.serialesSave_Aux.Rows)
                {
                    if (Resultado.Rows[0]["Semana"].ToString() == "5")
                    {
                        if (DataRow["Novedad"].ToString() == "SI")
                            Estado = "NOVEDAD";
                        else
                            Estado = "CUARENTENA";
                    }
                    else
                    {
                        if (DataRow["Novedad"].ToString() == "SI")
                            Estado = "NOVEDAD";
                        else
                            Estado = "PARA PROCESO";
                    }


                    //Aumento el contador de filas
                    ContadorFilas++;

                    if (DataRow["SmartCard"].ToString() != "")
                    { 
                        /*** Andres Leonardo Arevalo - 09 feb 2015 ***/
                        ConsultaGuardar1 += "INSERT INTO dbo.SmartCardEquiposDIRECTV(SMART_SERIAL,SMART_FECHAASIG) VALUES(";
                        ConsultaGuardar1 = ConsultaGuardar1 + "'" + DataRow["SmartCard"].ToString() + "', CONVERT(nvarchar(100), GETDATE(), 120))";
                        /*** Andres Leonardo Arevalo - 09 feb 2015 ***/
                    }
                    
                        //System.Windows.MessageBox.Show("Número de filas: " + ContadorFilas + " Modulo: " + ContadorFilas % 50);
                        //Obtengo la cantidad de columnas del listado
                        ContadorCampos = this.serialesSave_Aux.Columns.Count;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " INSERT INTO dbo.EquiposDIRECTVC(Serial,Receiver,SMART_CARD_ENTRADA,GABETAS_DIAG,MODELO,DESCRIPCION,TIPO_ORIGEN,TIPOS_DEVOLUCIONES,DOA,Ciudad,ESTADO_MATERIAL,FECHA_INGRESO, ESTADO) VALUES(";
                        
                        //Obtengo los datos de cada campo con su nombre
                        foreach (DataColumn c in this.serialesSave_Aux.Columns)
                        {
                            //Adiciono cada dato a la consulta
                            ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                            //Evaluo el contador de columnas para saber si adiciono la coma
                            ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                            //Disminuyo el contador
                            ContadorCampos--;
                        }
                        //Termino la consulta
                        ConsultaGuardar += ", GETDATE(), '" + Estado + "');";
                        //ConsultaGuardar1 += ") ";
                        ConsultaGuardar = ConsultaGuardar + "SET @RowId = SCOPE_IDENTITY();";

                        ConsultaGuardar += "INSERT INTO dbo.TrackEquiposDIRECTV(ID_SERIAL,SERIAL,ID_RECEIVER,SMARTCARD_ENTRADA,FECHA_INGRESO,ESTADO_RECIBO) VALUES (@RowId, '" + DataRow[0].ToString() + "', '" + DataRow[1].ToString() + "', '" + DataRow[2].ToString() + "', GETDATE(), 'RECIBO'); ";

                        ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EQUIPO RECIBIDO ENTRADA ALMACEN','RECIBIDO','ESPERANDO POR SER ALMACENADO',''"
                                    + ",@RowId,'RECIBO','UBICACIONENTRADAALMACEN','" + this.user + "','';";
                    
                    ContadorSave++;
                        
                   
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar) && !String.IsNullOrEmpty(ConsultaGuardar1))
                {
                    
                    //Ejecuto la consulta
                    Console.WriteLine(ConsultaGuardar);
                    Console.WriteLine(ConsultaGuardar1);
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);
                    service.DirectSQLNonQuery(ConsultaGuardar1, Local);
                    
                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                    ConsultaGuardar1 = "";
                }
                estado_almacenamiento = true;
                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                View.Dispatcher_Cargue.Invoke(new System.Action(() =>
                {
                    LimpiarDatosIngresoSeriales();
                }), null);
                
                trOnSave.Dispose();
                
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }    

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
            View.Model.List_Nocargue.Rows.Clear();
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Modelo

            IList<MMaster> ListadoModelos = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DRIRECMODE" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Modelo";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoModelos);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("MODELO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)150);
            Txt.SetValue(ComboBox.HeightProperty, (double)22);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDescripcion)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("MODELO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Descripcion


            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Descripcion";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("DESCRIPCION"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("DESCRIPCION", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Origen

            IList<MMaster> ListadoTipoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TIPORIGEND" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Origen";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoTipoOrigen);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Name");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("TIPO_ORIGEN"));
            Txt.SetValue(ComboBox.WidthProperty, (double)150);
            Txt.SetValue(ComboBox.HeightProperty, (double)22);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("TIPO_ORIGEN", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Fecha Ingreso

            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("Microsoft.Windows.Controls.DatePicker, WPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=c93dde70475aea7e"));
            //TipoDato = "Microsoft.Windows.Controls.DatePicker";
            //Columna.Header = "Fecha Ingreso";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)130);
            //Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding("FECHA_INGRESO"));

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("FECHA_INGRESO", typeof(String)); //Creacion de la columna en el DataTable

            ////Columna = new GridViewColumn();
            ////assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            ////TipoDato = "System.Windows.Controls.TextBlock";
            ////Columna.Header = "Fecha Ingreso";
            ////Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            ////Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(DateTime.Now.ToString()));


            ////// add textbox template
            ////Columna.CellTemplate = new DataTemplate();
            ////Columna.CellTemplate.VisualTree = Txt;
            ////View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            ////View.Model.ListRecords.Columns.Add("FECHA_INGRESO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Tipo Devolucion

            IList<MMaster> ListadoTipoDev = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TIPODEVDTV" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Tipo Devolucion";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoTipoDev);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("TIPOS_DEVOLUCIONES"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDOA)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("TIPOS_DEVOLUCIONES", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region DOA

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "DOA";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("DOA"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("DOA", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Ciudad

            IList<MMaster> ListadoCiudades = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ciudad";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoCiudades);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Ciudad"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Ciudad", typeof(String)); //Creacion de la columna en el DataTable
            #endregion              

            #region Columna Estado Material.

            IList<MMaster> ListadoFalla = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOMAT" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Estado Material";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFalla);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_MATERIAL"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTADO_MATERIAL", typeof(String)); //Creacion de la columna en el DataTable

            #endregion


        }

        public DataTable ValidarSerialesIngresados(DataTable ListadoSeriales)
        {
            //Variables Auxiliares
            DataTable SerialesIngresados;
            String CadenaSQL = "";
            Int32 CantidadRegistros;

            //Creo la cadena de consulta
            CadenaSQL += "SELECT * FROM dbo.EquiposDIRECTVC WHERE upper(SERIAL) IN (";

            //Obtengo la cantidad de registros
            CantidadRegistros = ListadoSeriales.Rows.Count;

            //Recorremos el listado
            foreach (DataRow dr in ListadoSeriales.Rows)
            {
                //Evaluo si es el ultimo registro para crear la cadena sin la coma
                if (CantidadRegistros == 1)
                {
                    //Adiciono los valores a la cade de cosulta
                    CadenaSQL += "'" + dr[0].ToString().ToUpper() + "') AND ESTADO != 'DESPACHADO'";
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
            SerialesIngresados = service.DirectSQLQuery(CadenaSQL, "", "dbo.EquiposDIRECTVC", Local);

            //Retornamos el resultado
            return SerialesIngresados;
        }

        private void OnValidarDOA(object sender, SelectionChangedEventArgs e)
        {
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    if (dr[7].ToString() == "MANTENIMIENTO")
                    {
                        dr[8] = "SI";
                        break;
                    }
                    else
                    {
                        dr[8] = "NO";
                        break;
                    }
                }
            }
            return;
        }

        private void OnValidarDescripcion(object sender, SelectionChangedEventArgs e)
        {
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    dr[5] = View.Model.ListadoModelosDescripcion.Where(f => f.Code == dr[4].ToString()).First().Description.ToString();
                    break;
                }
            }
            return;
        }

        public void OnKillProcess(object sender, EventArgs e)
        {

            //detengo procesos activos de excel para el cargue masivo
            Process[] proceso = Process.GetProcessesByName("EXCEL");

            if (proceso.Length > 0)
                proceso[0].Kill();
        }

        private void OnExportCargue(object sender, EventArgs e)
        {
            OnExportCargue_Excel(View.Model.List_Nocargue);
        }

        private void OnExportCargue_Excel(DataTable List_NocargueAlert)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                wb = excel.Workbooks.Add();
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                for (int Idx = 0; Idx < List_NocargueAlert.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = List_NocargueAlert.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < List_NocargueAlert.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, List_NocargueAlert.Columns.Count].Value =
                        List_NocargueAlert.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + List_NocargueAlert.Rows.Count + 1);
                rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                rng.Columns.AutoFit();

                excel.Visible = true;
                wb.Activate();
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }

        ////////////////////////////////////////////////////////PREALERTA/////////////////////////////////////////

        private void OnCargaPrealerta(object sender, EventArgs e)
        {
            //guarda unicamente el nombre del archivo sin toda la ruta
            String CadenaValidar = View.GetUpLoadFilePrea.FileName.ToString();
            string[] split = CadenaValidar.Split(new Char[] { '\\' });
            CadenaValidar = split.Last();

            String ConsultaBuscar = "SELECT prea_id FROM dbo.preAlertaDIRECTV where prea_archivo = '" + CadenaValidar.ToUpper() + "'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.preAlertaDIRECTV", Local);

            if (Resultado.Rows.Count > 0)
            {
                Util.ShowMessage("La prealerta ya fue cargada");
                return;
            }


            hilo_prealerta = new Thread(new ParameterizedThreadStart(SetPrealert));
            hilo_prealerta.SetApartmentState(ApartmentState.STA);
            hilo_prealerta.IsBackground = true;
            hilo_prealerta.Priority = ThreadPriority.Highest;


            String Cadena = View.GetUpLoadFilePrea.FileName.ToString();
            String conexion = "Provider=Microsoft.Jet.OleDb.4.0; Extended Properties=\"Excel 8.0; HDR=yes\"; Data Source=" + Cadena;
            try
            {
                //creo la coneccion para leer  el .xls
                oledbcon = new OleDbConnection(conexion);

                //traigo los datos del .xls
                adaptador = new OleDbDataAdapter("select * from [Hoja1$]", oledbcon);

                //guardo la info en un datatable
                adaptador.Fill(SerialesIngresadosPrea);

                oledbcon.Close();
                oledbcon.Dispose();

                //valido que existan registros
                if (SerialesIngresadosPrea.Rows.Count == 0)
                {
                    Util.ShowMessage("No hay registros para procesar");
                }
                else
                {
                    this.serialesSave_AuxPrea = View.Model.ListPrealerta;
                    View.Progress_CarguePrealerta.Value = 0;
                    View.GetEstado_CarguePrealerta.Text = "Iniciando operación";
                    hilo_prealerta.Start(SerialesIngresadosPrea);

                    StartTimerPrea();//Inicia el timmer para mostrar visualmente el progreso del cargue masivo

                }
            }
            catch (Exception ex)
            {
                Util.ShowError("El archivo a cargar no cuenta con la estructura correcta.");
            }
        }

        int contprea = 0;
        int cont_cargueprea = 0;
        int cont_repeatprea = 0;
        private void StartTimerPrea()
        {
            int num_seriales = SerialesIngresadosPrea.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tprea = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    contprea++;

                    if (cont == 1)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Buscando equipos duplicados";
                    }
                    else if (contprea == 2)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Buscando equipos duplicados.";
                    }
                    else if (contprea == 3)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Buscando equipos duplicados..";
                    }
                    else if (contprea == 4 || contprea > 4)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Buscando equipos duplicados...";
                        contprea = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CarguePrealerta.Value < 100D && busqueda_Repetidosprea == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CarguePrealerta.Value = cont_repeatprea * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tprea.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CarguePrealerta.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_CarguePrealerta.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CarguePrealerta, 0, 1000);
        }

        private void StartTimerCargaPrea()
        {
            int num_seriales = SerialesIngresadosPrea.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tcargueprea = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_cargueprea++;

                    if (cont_cargueprea == 1)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Cargando archivo";
                    }
                    else if (cont_cargueprea == 2)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Cargando archivo.";
                    }
                    else if (cont_cargueprea == 3)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Cargando archivo..";
                    }
                    else if (cont_cargueprea == 4 || cont_cargueprea > 4)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Cargando archivo...";
                        cont_cargueprea = 0;
                    }
                    // Implementación del método anónimo  && SerialesIngresados.Rows.Count > 0
                    if (estado_cargueprea == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CarguePrealerta.Value = ((View.Model.ListPrealerta.Rows.Count * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tcargueprea.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CarguePrealerta.Text = "Carga de archivo terminada.";
                            View.Progress_CarguePrealerta.Value = 100D;
                            tcargueprea.Dispose();
                        }));
                    }
                }));
            }), View.Progress_CarguePrealerta, 0, 1000);
        }

        private void SetPrealert(object o)
        {
            busqueda_Repetidosprea = false;

            List<String> listNoCarguePrea = new List<String>(); //Guarda los seriales que no cumplen con los requisitos del cargue en Prealerta

            for (int i = 0; i < SerialesIngresadosPrea.Rows.Count; i++)
            {
                for (int j = i + 1; j < SerialesIngresadosPrea.Rows.Count; j++)
                {
                    if (SerialesIngresadosPrea.Rows[i]["SERIAL"].ToString() == "" & SerialesIngresadosPrea.Rows[j]["SERIAL"].ToString() == "")
                    {
                        SerialesIngresadosPrea.Rows.RemoveAt(i); //Elimino la fila vacia
                        if (j > 0)
                            j = i - 1;
                    }
                    else
                    {
                        if (SerialesIngresadosPrea.Rows[i]["SERIAL"].ToString() == SerialesIngresadosPrea.Rows[j]["SERIAL"].ToString())
                        {
                            listNoCarguePrea.InsertRange(listNoCarguePrea.Count, new string[] {
                                SerialesIngresadosPrea.Rows[j]["CODIGO_TIPO_ELEMENTO"].ToString(),
                                SerialesIngresadosPrea.Rows[j]["TIPO_ELEMENTO"].ToString(), 
                                SerialesIngresadosPrea.Rows[j]["SERIAL"].ToString(),
                                SerialesIngresadosPrea.Rows[j]["RID"].ToString(), 
                                SerialesIngresadosPrea.Rows[j]["SERIAL_VINCULADO"].ToString(), 
                                SerialesIngresadosPrea.Rows[j]["ESTADO"].ToString(), 
                                "Serial duplicado dentro del archivo de cargue"});

                            SerialesIngresadosPrea.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                            j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        }
                    }

                }
                cont_repeatprea++;
            }


            busqueda_Repetidosprea = true;
            Thread.Sleep(1000);
            tprea.Dispose();

            View.Dispatcher_CarguePrealerta.Invoke(new System.Action(() =>
            {
                View.Progress_Cargue.Value = 0;
            }), null);

            listNoCarguePrea = listNoCarguePrea.Distinct().ToList();
            this.MostrarErrores_CarguePrea(listNoCarguePrea); // Agrega a un segundo listview los equipos que no fueron cargados

            StartTimerCargaPrea();

            contprea = 0;

            CargarListDetailsPrea();

            estado_cargueprea = true;
        }

        public void CargarListDetailsPrea()
        {
            foreach (DataRow dr in SerialesIngresadosPrea.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_CarguePrealerta.Invoke(new System.Action(() =>
                {
                    DataRow RegistroGuardar = View.Model.ListPrealerta.NewRow();

                    RegistroGuardar["Codigo"] = dr[0].ToString();
                    RegistroGuardar["Tipo"] = dr[1].ToString();
                    RegistroGuardar["Serial"] = dr[2].ToString();
                    RegistroGuardar["RID"] = dr[3].ToString();
                    RegistroGuardar["SerialVinculado"] = dr[4].ToString();
                    RegistroGuardar["Estado"] = dr[5].ToString();

                    View.Model.ListPrealerta.Rows.Add(RegistroGuardar);

                }), null);
            }
        }

        private void MostrarErrores_CarguePrea(List<String> listNoCarguePrea)
        {
            int columna = 0;
            View.Dispatcher_CarguePrealerta.Invoke(new System.Action(() =>
            {
                NoLoad_RowPrea = View.Model.List_NocarguePrea.NewRow();
            }), null);

            foreach (var dr in listNoCarguePrea)
            {
                View.Dispatcher_CarguePrealerta.Invoke(new System.Action(() =>
                {
                    if (columna != View.Model.List_NocarguePrea.Columns.Count - 1)
                    {
                        NoLoad_RowPrea[columna] = dr;
                        columna++;
                    }
                    else
                    {
                        NoLoad_RowPrea[columna] = dr;
                        columna = 0;
                        View.Model.List_NocarguePrea.Rows.Add(NoLoad_RowPrea);
                        NoLoad_RowPrea = View.Model.List_NocarguePrea.NewRow();
                    }
                }), null);
            }
        }

        private void OnSaveDetailsPrealerta(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListPrealerta.Rows.Count == 0)
                return;

            hilo_savePrealerta = new Thread(new ParameterizedThreadStart(SavePrealerta));
            hilo_savePrealerta.SetApartmentState(ApartmentState.STA);
            hilo_savePrealerta.IsBackground = true;
            hilo_savePrealerta.Priority = ThreadPriority.Highest;
            cont_serialesPrea = 0;
            this.ContadorSavePrea = 0;

            this.serialesSave_AuxPrea = View.Model.ListPrealerta;

            View.Progress_CarguePrealerta.Value = 0;
            estado_almacenamientoPrea = false;
            View.GetEstado_CarguePrealerta.Text = "Iniciando guardado";
            hilo_savePrealerta.Start(View.Model.ListPrealerta); //Se inicia el proceso background utilizando el Hilo

            StartTimer_PrealertaOnSaveBd(); //Inicia el timmer para mostrar visualmente el progreso del guardado en la BD


        }

        private void StartTimer_PrealertaOnSaveBd()
        {
            // Creamos diferentes hilos a través de un temporizador
            trOnSavePrea = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_serialesPrea++;

                    if (cont_serialesPrea == 1)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Guardando equipos";
                    }
                    else if (cont_serialesPrea == 2)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Guardando equipos.";
                    }
                    else if (cont_serialesPrea == 3)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Guardando equipos..";
                    }
                    else if (cont_serialesPrea == 4 || cont_serialesPrea > 4)
                    {
                        View.GetEstado_CarguePrealerta.Text = "Guardando equipos...";
                        cont_serialesPrea = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_almacenamientoPrea == false)
                    {
                        View.Progress_CarguePrealerta.Value = ((ContadorSavePrea * 100D) / this.serialesSave_AuxPrea.Rows.Count);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trOnSavePrea.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CarguePrealerta.Text = "Proceso de guardado terminado.";
                            View.Progress_CarguePrealerta.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CarguePrealerta, 0, 300);
        }

        private void SavePrealerta(Object e)
        {
            if (this.serialesSave_AuxPrea.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para guardar");
            }
            else
            {
                    //guarda unicamente el nombre del archivo sin toda la ruta
                    String Cadena = View.GetUpLoadFilePrea.FileName.ToString();
                    string[] split = Cadena.Split(new Char[] { '\\' });
                    Cadena = split.Last();
                    
                    String ConsultaGuardar = "Declare @prea_id int, @aleEquip_id int; ";

                    ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.preAlertaDIRECTV(prea_archivo)" +
                    "VALUES('" + Cadena.ToUpper() + "'); ";
                    ConsultaGuardar = ConsultaGuardar + "SET @prea_id = SCOPE_IDENTITY();";

                    int contcoma = 1;
                    foreach (DataRow DataRow in this.serialesSave_AuxPrea.Rows)
                    {
                        int ContadorCampos = this.serialesSave_AuxPrea.Columns.Count;

                        ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.preAlerta_EQUIPOSDIRECTV(aleEquip_codElemento, aleEquip_tipoElemento, aleEquip_serial, aleEquip_RID, aleEquip_serialVinculado, aleEquip_Estado) VALUES(";
                        foreach (DataColumn c in this.serialesSave_AuxPrea.Columns)
                        {
                            if (contcoma == this.serialesSave_AuxPrea.Columns.Count)
                            {
                                //Adiciono cada dato a la consulta
                                ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";
                            }
                            else
                            {
                                ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "',";
                            }
                            contcoma++;
                        }
                        ConsultaGuardar = ConsultaGuardar + ");";
                        ConsultaGuardar = ConsultaGuardar + "SET @aleEquip_id = SCOPE_IDENTITY();";
                        ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.PreAlertaDIRECTV_EquiposDIRECTV(prea_id,aleEquip_id) VALUES(@prea_id, @aleEquip_id);";
                        ContadorSavePrea++;
                        contcoma = 1;
                    }

                    //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                    if (!String.IsNullOrEmpty(ConsultaGuardar))
                    {
                        
                        service.DirectSQLNonQuery(ConsultaGuardar, Local);
                        ConsultaGuardar = "";
                    }
                    estado_almacenamiento = true;

                    Util.ShowMessage("Registros guardados satisfactoriamente.");

                    trOnSavePrea.Dispose();

                View.Dispatcher_CarguePrealerta.Invoke(new System.Action(() =>
                {
                    View.Model.ListPrealerta.Clear();
                    View.Model.List_NocarguePrea.Clear();
                }), null);
                
            }
        }

        private void OnExportCarguePrea(object sender, EventArgs e)
        {
            OnExportCarguePrea_Excel(View.Model.List_NocarguePrea);
        }

        private void OnExportCarguePrea_Excel(DataTable List_NocarguePrea)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                wb = excel.Workbooks.Add();
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                for (int Idx = 0; Idx < List_NocarguePrea.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = List_NocarguePrea.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < List_NocarguePrea.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, List_NocarguePrea.Columns.Count].Value =
                        List_NocarguePrea.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + List_NocarguePrea.Rows.Count + 1);
                rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                rng.Columns.AutoFit();

                excel.Visible = true;
                wb.Activate();
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }

        #endregion
    }
}