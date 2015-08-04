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
using System.Text.RegularExpressions;
using System.Collections;
using WpfFront.IQ.Views;
using System.Data.OleDb;
using System.Threading;
using System.Windows.Media;
using System.Diagnostics;
using WpfFront.IQ.Models;

namespace WpfFront.Presenters
{

    public interface IEntradaAlmacenV2Presenter
    {
        IEntradaAlmacenV2View View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EntradaAlmacenV2Presenter : IEntradaAlmacenV2Presenter
    {
        public IEntradaAlmacenV2View View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 2; //# columnas que no se debe replicar porque son fijas.
        private DataRow RegistroGuardar = null;
        private DataRow NoLoad_Row = null, NoLoadPrealert_Row = null;
        private DataTable SerialesIngresados = new DataTable();
        private DataTable SerialesIngresados_alerta = new DataTable();
        private Int32 ContadorSave;
        private Timer t;
        private Timer t1;
        private Timer talerSAP;
        private Timer t9;
        private Timer t3;
        private Timer trbd;
        private Timer trsap;
        private Timer trOnSave;
        private Timer trOnSaveAlert;
        private Thread updateLabelThread;
        private Thread hilo_seriales;
        private Thread hilo_saveSerial;
        private DataTable RegistrosSave_Aux;
        private Boolean estado_cargue = false, busqueda_Repetidos = false, busqueda_RepetidosSerial = false, busqueda_SAPAlert = false;
        private Boolean estado_cargueSeriales = false, busqueda_RBDSerial = false, busqueda_SAPSerial = false, estado_almacenamiento = false, estado_almacenamientoAlert = false;
        private DataTable serialesSave_Aux;
        private Thread hilo_savePreAlert;
        private int cont_registros = 0, ContadorSave2 = 0;
        private int cont_seriales = 0;
        private int cont_serialesRep = 0;
        private int cont_serialesRepBD = 0;
        private int cont_serialesSAP = 0;
        private int cont_RepBDseriales = 0;
        private int cont_SAPseriales = 0;
        private int cont_SAPAlert = 0;
        private int cont = 0;
        private int cont_repeat = 0;
        private int cont_repeatSerial = 0;

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public EntradaAlmacenV2Presenter(IUnityContainer container, IEntradaAlmacenV2View view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EntradaAlmacenV2Model>();

            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<EventArgs>(this.OnCargaMasiva);
            view.CargaMasiva_Alerta += new EventHandler<EventArgs>(this.OnCargaMasiva_Alerta);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.SaveDetails_Prealert += new EventHandler<EventArgs>(this.OnSaveDetails_Prealert);
            View.SavePrealertAll += new EventHandler<EventArgs>(this.OnSavePrealert);
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            View.ExportCargue += new EventHandler<EventArgs>(this.OnExportCargue);
            View.ExportCargueAlerta += new EventHandler<EventArgs>(this.OnExportCargueAlerta);
            view.KillProcess += new EventHandler<EventArgs>(this.OnKillProcess);

            #endregion

            #region Datos

            //Cargo la variable para las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo los productos en el tableview y se ordenan
            //View.Model.ListadoProductos = service.GetProduct(new Product { Reference = "425" });
            IList<Product> list = service.GetProduct(new Product { Reference = "425" });
            IEnumerable<Product> sortedEnum = list.OrderBy(f => f.Brand);
            IList<Product> sortedList = sortedEnum.ToList();

            View.Model.ListadoProductos = sortedList;

            //Cargo los listados de los detalles
            View.Model.ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            //View.Model.ListadoCiudades = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD" } });
            View.Model.ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            //View.Model.ListadoCodigoSAP = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TELMEXCOD" } });
            View.Model.ListadoEstadoRR = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADO RR" } });
            View.Model.ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            //View.Model.ListadoCentros = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CENTRO" } });
            //View.Model.ListadoFamilias = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FAMILIA" } });

            View.Model.ListadoPreaTipoRecoleccion = service.GetMMaster(new MMaster { MetaType = new MType { Code = "PREATREC" } });
            View.Model.ListadoPreaTipoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "PREATORIGE" } });

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

            //Evaluo que haya sido digitado el serial para buscar-
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            if (View.GetSerial1.Text.ToString().Contains(" ") || View.GetSerial2.Text.ToString().Contains(" "))
            {
                Util.ShowError("Los campos no pueden contener espacios. Verifique el serial.");
                return;
            }

            if (String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
            {
                Util.ShowError("El campo Mac no puede ser vacio.");
                return;
            }

            //Validacion existe o no el equipo en DB
            ConsultaBuscar = "SELECT Serial, Mac FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + View.GetSerial1.Text.ToString() + "')";
            
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

            if (Resultado.Rows.Count > 0)
            {
                Util.ShowError("El serial " + View.GetSerial1.Text.ToUpper() + " ya esta registrado en el sistema.");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
                return;
            }
            else
            {
                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
                    {
                        Util.ShowError("El serial " + View.GetSerial1.Text.ToUpper() + " ya esta en el listado.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }

                    if (View.GetSerial2.Text.ToUpper() == item["Mac"].ToString().ToUpper())
                    {
                        Util.ShowError("El serial con mac " + View.GetSerial2.Text.ToUpper() + " ya esta en el listado.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                }

                //Asigno los campos
                //dr["Producto"] = RegistroValidado.Rows[0]["Producto"].ToString();
                dr["Serial"] = View.GetSerial1.Text.ToString().ToUpper();
                dr["Mac"] = View.GetSerial2.Text.ToString().ToUpper();

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr);

                var border = (Border)VisualTreeHelper.GetChild(View.ListadoEquiposAProcesar, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();

                //Limpio los seriales para digitar nuevos datos
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
            }
            
        }

        private void OnCargaMasiva(object sender, EventArgs e)
        {
            hilo_seriales = new Thread(new ParameterizedThreadStart(LoadSerials));
            hilo_seriales.SetApartmentState(ApartmentState.STA);
            hilo_seriales.IsBackground = true;
            hilo_seriales.Priority = ThreadPriority.Highest;

            String Cadena = View.GetUpLoadFile.FileName.ToString(); //Ruta del archivo seleccionado
            String conexion = "Provider=Microsoft.Jet.OleDb.4.0; Extended Properties=\"Excel 8.0; HDR=yes\"; Data Source=" + Cadena;

            try
            {
                //creo la coneccion para leer  el .xls
                OleDbConnection oledbcon = default(OleDbConnection);
                oledbcon = new OleDbConnection(conexion);

                //Datatable con todos los campos registros
                DataSet ds = new DataSet();

                //Selecciona todas las columnas del documento .xls en la Hoja1
                OleDbDataAdapter adaptador = new OleDbDataAdapter("select SERIAL,MAC,TIPO_ORIGEN,ORIGEN,CODIGO_SAP,TIPO_RECIBO,CONSECUTIVO,CAJA from [Hoja1$]", oledbcon);

                //Guardo la info en un datatable
                adaptador.Fill(SerialesIngresados);

                oledbcon.Close();

                if (SerialesIngresados.Rows.Count == 0)
                {
                    Util.ShowMessage("No hay registros para procesar");
                }
                else
                {
                    this.serialesSave_Aux = View.Model.ListRecords;
                    View.Progress_CargueAlertasSer.Value = 0;
                    View.Model.List_Nocargue.Rows.Clear();
                    View.GetEstado_CargueSer.Text = "Iniciando operación";
                    hilo_seriales.Start(SerialesIngresados);

                    StartTimer_RSerial();//Inicia el timmer para mostrar visualmente el progreso del cargue masivo
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("El archivo a cargar no cuenta con la estructura correcta.");
            }
        }

        private void OnCargaMasiva_Alerta(object sender, EventArgs e)
        {
            Thread hilo_repetidos = new Thread(new ParameterizedThreadStart(SetRepeat));
            hilo_repetidos.SetApartmentState(ApartmentState.STA);
            hilo_repetidos.IsBackground = true;
            hilo_repetidos.Priority = ThreadPriority.Highest;

            //List<String> listRepetidos = new List<String>(); //Guarda los seriales que estan repetidos en el carge masivo

            String Cadena = View.GetUpLoadFile_Prealerta.FileName.ToString(); //Ruta del archivo seleccionado
            String conexion = "Provider=Microsoft.Jet.OleDb.4.0; Extended Properties=\"Excel 8.0; HDR=yes\"; Data Source=" + Cadena;

            string[] split = Cadena.Split(new Char[] { '\\' });
            Cadena = split.Last();

            String ConsultaBuscar = "SELECT prea_id FROM dbo.preAlertaCLARO where prea_archivo = '" + Cadena.ToUpper() + "'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.preAlertaCLARO", Local);

            if (Resultado.Rows.Count > 0)
            {
                Util.ShowMessage("La prealerta ya fue cargada");
                return;
            }

            //creo la coneccion para leer  el .xls
            OleDbConnection oledbcon = default(OleDbConnection);
            oledbcon = new OleDbConnection(conexion);

            //Datatable con todos los campos registros
            DataSet ds = new DataSet();

            //Selecciona todas las columnas del documento .xls en la Hoja1
            OleDbDataAdapter adaptador = new OleDbDataAdapter("select SAP,DESCRIPCION,CANTIDAD,SERIAL,OBSERVACIONES from [Hoja1$]", oledbcon);

            //Guardo la info en un datatable
            adaptador.Fill(SerialesIngresados_alerta);

            oledbcon.Close();  //Se cierra la conexión al archivo de Excel

            if (SerialesIngresados_alerta.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para procesar");
            }
            else
            {
                View.Progress_CargueAlertas.Value = 0;
                View.GetEstado_Cargue.Text = "Iniciando operación";
                hilo_repetidos.Start(SerialesIngresados_alerta);

                //updateLabelThread.Start(SerialesIngresados); //Se inicia el proceso background utilizando el Hilo
                StartTimer();//Inicia el timmer para mostrar visualmente el progreso del cargue masivo
            }
        }

        /** Recibo de seriales - Se ejecuta cuando se invoca el hilo en el proceso de cargue masivo - recibo seriales **/
        private void LoadSerials(object o)
        {
            busqueda_RepetidosSerial = false;
            busqueda_RBDSerial = false;
            busqueda_SAPSerial = false;
            estado_cargueSeriales = false;

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
                    //if (SerialesIngresados.Rows[i]["Serial"].ToString() != "" && SerialesIngresados.Rows[j]["Serial"].ToString() != "")
                    else
                    {
                        if (SerialesIngresados.Rows[i]["Serial"].ToString() == SerialesIngresados.Rows[j]["Serial"].ToString())
                        {
                            listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[j]["Serial"].ToString(), SerialesIngresados.Rows[j]["Mac"].ToString(), SerialesIngresados.Rows[j]["Codigo_SAP"].ToString(),
                                SerialesIngresados.Rows[j]["TIPO_RECIBO"].ToString(), SerialesIngresados.Rows[j]["TIPO_ORIGEN"].ToString(), SerialesIngresados.Rows[j]["CONSECUTIVO"].ToString(),SerialesIngresados.Rows[j]["CAJA"].ToString(), "Serial duplicado dentro del archivo de cargue"});

                            SerialesIngresados.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                            j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        }
                    }
                }
                cont_repeatSerial++;
            }

            //listNoCargue = listNoCargue.Distinct().ToList(); //Realiza un distinct de la lista en caso que tenga filas repetidas

            Thread.Sleep(1000);
            busqueda_RepetidosSerial = true;
            t3.Dispose();
            cont_repeatSerial = 0;

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                View.Progress_CargueAlertasSer.Value = 0;
            }), null);

            StartTimer_RBDSerial();

            //Validamos que los seriales existan en la base de datos
            String ConsultaBuscar = "";
            int temp2 = 0;
            while (temp2 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscar = "SELECT TOP 1 Serial FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp2]["SERIAL"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count >= 1)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp2]["Serial"].ToString(), SerialesIngresados.Rows[temp2]["Mac"].ToString(), SerialesIngresados.Rows[temp2]["Codigo_SAP"].ToString(),
                                SerialesIngresados.Rows[temp2]["TIPO_RECIBO"].ToString(), SerialesIngresados.Rows[temp2]["TIPO_ORIGEN"].ToString(), SerialesIngresados.Rows[temp2]["CONSECUTIVO"].ToString(),SerialesIngresados.Rows[temp2]["CAJA"].ToString(), "El equipo ya esta insertado en el sistema"});

                    SerialesIngresados.Rows.RemoveAt(temp2);
                }
                else
                {
                    temp2++;
                }
                cont_RepBDseriales++;
            }

            Thread.Sleep(1000);
            busqueda_RBDSerial = true;
            trbd.Dispose();
            cont_RepBDseriales = 0;

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                View.Progress_CargueAlertasSer.Value = 0;
            }), null);

            StartTimer_SAPSerial();

            //validamos que los SAP existan en la base de datos
            int temp = 0;

            while (temp < SerialesIngresados.Rows.Count)
            {
                Boolean existSAP = BuscarCodigo_SAP(SerialesIngresados.Rows[temp]["CODIGO_SAP"].ToString());

                if (existSAP == false)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp]["Serial"].ToString(), SerialesIngresados.Rows[temp]["Mac"].ToString(), SerialesIngresados.Rows[temp]["Codigo_SAP"].ToString(),
                                SerialesIngresados.Rows[temp]["TIPO_RECIBO"].ToString(), SerialesIngresados.Rows[temp]["TIPO_ORIGEN"].ToString(), SerialesIngresados.Rows[temp]["CONSECUTIVO"].ToString(),SerialesIngresados.Rows[temp]["CAJA"].ToString(), "El equipo no tiene un código SAP coincidente"});

                    SerialesIngresados.Rows.RemoveAt(temp);
                }
                else
                {
                    temp++;
                }
                cont_SAPseriales++;
            }

            Thread.Sleep(1000);
            busqueda_SAPSerial = true;
            trsap.Dispose();
            cont_SAPseriales = 0;

            this.MostrarErrores_Cargue(listNoCargue); // Agrega a un segundo listview los equipos que no fueron cargados

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                View.Progress_CargueAlertasSer.Value = 0;
            }), null);

            StartTimer_Seriales();

            foreach (DataRow dr in SerialesIngresados.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    RegistroGuardar = this.serialesSave_Aux.NewRow();

                    RegistroGuardar["Serial"] = dr[0].ToString();
                    RegistroGuardar["Mac"] = dr[1].ToString();
                    RegistroGuardar["TIPO_ORIGEN"] = dr[2].ToString();
                    RegistroGuardar["ORIGEN"] = dr[3].ToString();
                    RegistroGuardar["ProductoID"] = dr[4].ToString();
                    RegistroGuardar["Tipo_Rec"] = dr[5].ToString();
                    //RegistroGuardar["Fecha_Ingreso"] = dr[5].ToString();
                    RegistroGuardar["CONSECUTIVO"] = dr[6].ToString();
                    RegistroGuardar["CAJA"] = dr[7].ToString();

                    View.Model.ListRecords.Rows.Add(RegistroGuardar);
                }), null);
            }

            estado_cargueSeriales = true; //Finalizo el cargue de seriales
        }

        /*
         * @param List<String> listNoCargue : recibe una lista con los equipos que no cumplieron los requisitos de guardado y se cargan en una segunda lista
         */
        private void MostrarErrores_Cargue(List<String> listNoCargue)
        {
            int columna = 0;
            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                NoLoad_Row = View.Model.List_Nocargue.NewRow();
            }), null);

            foreach (var dr in listNoCargue)
            {
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
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

        #region Timmers Recibo Seriales

        private void StartTimer_RSerial()
        {
            // Creamos diferentes hilos a través de un temporizador
            t3 = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_serialesRep++;

                    if (cont_serialesRep == 1)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos duplicados";
                    }
                    else if (cont_serialesRep == 2)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos duplicados.";
                    }
                    else if (cont_serialesRep == 3)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos duplicados..";
                    }
                    else if (cont_serialesRep == 4 || cont_serialesRep > 4)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos duplicados...";
                        cont_serialesRep = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CargueAlertasSer.Value < 100D && busqueda_RepetidosSerial == false)
                    {
                        int num_seriales = SerialesIngresados.Rows.Count;
                        if (num_seriales <= 0) { num_seriales = 1; }
                        View.Progress_CargueAlertasSer.Value = cont_repeatSerial * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t3.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CargueSer.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_CargueAlertasSer.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertasSer, 0, 1000);
        }

        private void StartTimer_RBDSerial()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            trbd = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_serialesRepBD++;

                    if (cont_serialesRepBD == 1)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos previamente registrados";
                    }
                    else if (cont_serialesRepBD == 2)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos previamente registrados.";
                    }
                    else if (cont_serialesRepBD == 3)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos previamente registrados..";
                    }
                    else if (cont_serialesRepBD == 4 || cont_serialesRepBD > 4)
                    {
                        View.GetEstado_CargueSer.Text = "Buscando equipos previamente registrados...";
                        cont_serialesRepBD = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CargueAlertasSer.Value < 100D && busqueda_RBDSerial == false)
                    {
                        if (num_seriales <= 0) { num_seriales = 1; }
                        View.Progress_CargueAlertasSer.Value = cont_RepBDseriales * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trbd.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CargueSer.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_CargueAlertasSer.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertasSer, 0, 300);
        }

        private void StartTimer_SAPSerial()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            trsap = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_serialesSAP++;

                    if (cont_serialesSAP == 1)
                    {
                        View.GetEstado_CargueSer.Text = "Validando código de producto SAP";
                    }
                    else if (cont_serialesSAP == 2)
                    {
                        View.GetEstado_CargueSer.Text = "Validando código de producto SAP.";
                    }
                    else if (cont_serialesSAP == 3)
                    {
                        View.GetEstado_CargueSer.Text = "Validando código de producto SAP..";
                    }
                    else if (cont_serialesSAP == 4 || cont_serialesSAP > 4)
                    {
                        View.GetEstado_CargueSer.Text = "Validando código de producto SAP...";
                        cont_serialesSAP = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CargueAlertasSer.Value < 100D && busqueda_SAPSerial == false)
                    {
                        if (num_seriales <= 0) { num_seriales = 1; }
                        View.Progress_CargueAlertasSer.Value = cont_SAPseriales * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trsap.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CargueSer.Text = "Verificación código de producto SAP terminada.";
                            View.Progress_CargueAlertasSer.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertasSer, 0, 1000);
        }

        private void StartTimer_Seriales()
        {
            // Creamos diferentes hilos a través de un temporizador
            t9 = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {

                    cont_seriales++;

                    if (cont_seriales == 1)
                    {
                        View.GetEstado_CargueSer.Text = "Cargando archivo";
                    }
                    else if (cont_seriales == 2)
                    {
                        View.GetEstado_CargueSer.Text = "Cargando archivo.";
                    }
                    else if (cont_seriales == 3)
                    {
                        View.GetEstado_CargueSer.Text = "Cargando archivo..";
                    }
                    else if (cont_seriales == 4 || cont_seriales > 4)
                    {
                        View.GetEstado_CargueSer.Text = "Cargando archivo...";
                        cont_seriales = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_cargueSeriales == false)
                    {
                        int num_seriales = SerialesIngresados.Rows.Count;
                        if (num_seriales <= 0) { num_seriales = 1; }
                        View.Progress_CargueAlertasSer.Value = ((View.Model.ListRecords.Rows.Count * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t9.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CargueSer.Text = "Carga de archivo terminada.";
                            View.Progress_CargueAlertasSer.Value = 100D;
                            // t9.Dispose();
                        }));
                    }
                }));
            }), View.Progress_CargueAlertasSer, 0, 100);
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
                        View.GetEstado_CargueSer.Text = "Guardando equipos";
                    }
                    else if (cont_seriales == 2)
                    {
                        View.GetEstado_CargueSer.Text = "Guardando equipos.";
                    }
                    else if (cont_seriales == 3)
                    {
                        View.GetEstado_CargueSer.Text = "Guardando equipos..";
                    }
                    else if (cont_seriales == 4 || cont_seriales > 4)
                    {
                        View.GetEstado_CargueSer.Text = "Guardando equipos...";
                        cont_seriales = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_almacenamiento == false)
                    {
                        View.Progress_CargueAlertasSer.Value = ((ContadorSave * 100D) / View.Model.ListRecords.Rows.Count);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trOnSave.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_CargueSer.Text = "Proceso de guardado terminado.";
                            View.Progress_CargueAlertasSer.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertasSer, 0, 300);
        }

        #endregion Timmers Recibo Seriales

        #region Timmers Recibo Pre-alertas

        private void StartTimer()
        {
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
                    if (View.Progress_CargueAlertas.Value < 100D && busqueda_Repetidos == false)
                    {
                        int num_seriales = SerialesIngresados_alerta.Rows.Count;
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueAlertas.Value = cont_repeat * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_CargueAlertas.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertas, 0, 1000);
        }

        private void StartTimer_SAPAlert()
        {
            int num_seriales = SerialesIngresados_alerta.Rows.Count;

            if (num_seriales == 0) { num_seriales = 1; }
            // Creamos diferentes hilos a través de un temporizador
            talerSAP = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont++;

                    if (cont == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando código SAP";
                    }
                    else if (cont == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando código SAP.";
                    }
                    else if (cont == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando código SAP..";
                    }
                    else if (cont == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando código SAP...";
                        cont = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CargueAlertas.Value < 100D && busqueda_SAPAlert == false)
                    {
                        View.Progress_CargueAlertas.Value = ((cont_SAPAlert * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        talerSAP.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación SAP terminada.";
                            View.Progress_CargueAlertas.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertas, 0, 1000);
        }

        private void StartTimer2()
        {
            // Creamos diferentes hilos a través de un temporizador
            t1 = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont++;

                    if (cont == 1)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo";
                    }
                    else if (cont == 2)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo.";
                    }
                    else if (cont == 3)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo..";
                    }
                    else if (cont == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Cargando archivo...";
                        cont = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_cargue == false)
                    {
                        int num_seriales = SerialesIngresados_alerta.Rows.Count;
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueAlertas.Value = ((View.Model.ListRecords_Alertas.Rows.Count * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t1.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Carga de archivo terminada.";
                            View.Progress_CargueAlertas.Value = 100D;
                            t1.Dispose();
                        }));
                    }
                }));
            }), View.Progress_CargueAlertas, 0, 1000);
        }

        #endregion Timmers Recibo Pre-alertas

        private void SetDataRowsProcess(object o)
        {
            estado_cargue = false; //Como no se puede enviar el datatable por parametro se deja la variable global

            foreach (DataRow dr in SerialesIngresados.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    RegistroGuardar = View.Model.ListRecords_Alertas.NewRow();

                    RegistroGuardar["Aler_SAP"] = dr[0].ToString();
                    RegistroGuardar["Aler_Description"] = dr[1].ToString();
                    RegistroGuardar["Aler_Cantidad"] = dr[2].ToString();
                    RegistroGuardar["Aler_Serial"] = dr[3].ToString();
                    RegistroGuardar["Aler_Observacion"] = dr[4].ToString();

                    View.Model.ListRecords_Alertas.Rows.Add(RegistroGuardar);

                }), null);
            }
            estado_cargue = true;
        }

        private void SetRepeat(object o)
        {
            busqueda_Repetidos = false;
            busqueda_SAPAlert = false;

            List<String> listNoCargue = new List<String>(); //Guarda los seriales que no cumplen con los requisitos del cargue en Prealerta

            for (int i = 0; i < SerialesIngresados_alerta.Rows.Count; i++)
            {
                for (int j = i + 1; j < SerialesIngresados_alerta.Rows.Count; j++)
                {
                    if (SerialesIngresados_alerta.Rows[i]["Serial"].ToString() == "" & SerialesIngresados_alerta.Rows[j]["Serial"].ToString() == "")
                    {
                        SerialesIngresados_alerta.Rows.RemoveAt(i); //Elimino la fila vacia
                        //j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial anterior al borrado
                        if (j > 0)
                            j = i - 1;
                    }
                    //if (SerialesIngresados_alerta.Rows[i]["Serial"].ToString() != "" && SerialesIngresados_alerta.Rows[j]["Serial"].ToString() != "")
                    else
                    {
                        if (SerialesIngresados_alerta.Rows[i]["Serial"].ToString() == SerialesIngresados_alerta.Rows[j]["Serial"].ToString())
                        {
                            listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados_alerta.Rows[j]["Serial"].ToString(),SerialesIngresados_alerta.Rows[j]["Sap"].ToString(), SerialesIngresados_alerta.Rows[j]["Descripcion"].ToString(),
                                SerialesIngresados_alerta.Rows[j]["Cantidad"].ToString(), "Serial duplicado dentro del archivo de cargue"});

                            SerialesIngresados_alerta.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                            j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        }
                    }
                }
                cont_repeat++;
            }

            listNoCargue = listNoCargue.Distinct().ToList();
            busqueda_Repetidos = true;
            estado_cargue = false; //Como no se puede enviar el datatable por parametro se deja la variable global

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                View.Progress_CargueAlertas.Value = 0;
            }), null);

            Thread.Sleep(1000);
            cont = 0;
            t.Dispose();

            StartTimer_SAPAlert();

            //validamos que los SAP existan en la base de datos
            int temp = 0;

            while (temp < SerialesIngresados_alerta.Rows.Count)
            {
                Boolean existSAP = BuscarCodigo_SAP(SerialesIngresados_alerta.Rows[temp]["SAP"].ToString());

                if (existSAP == false)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados_alerta.Rows[temp]["Serial"].ToString(),SerialesIngresados_alerta.Rows[temp]["Sap"].ToString(), SerialesIngresados_alerta.Rows[temp]["Descripcion"].ToString(),
                                SerialesIngresados_alerta.Rows[temp]["Cantidad"].ToString(),"El equipo no tiene un código SAP coincidente"});

                    SerialesIngresados_alerta.Rows.RemoveAt(temp);
                }
                else
                {
                    temp++;
                }
                cont_SAPAlert++;
            }

            Thread.Sleep(1000);
            cont = 0;
            busqueda_SAPAlert = true;
            talerSAP.Dispose();
            cont_SAPAlert = 0;

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                NoLoadPrealert_Row = View.Model.List_NocargueAlert.NewRow();
            }), null);

            //// Se cargan la lista de los seriales que no cumplieron los requisitos de la carga masiva a un listview
            int columna = 0;

            foreach (var dr in listNoCargue)
            {
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {

                    if (columna != View.Model.List_NocargueAlert.Columns.Count - 1)
                    {
                        NoLoadPrealert_Row[columna] = dr;
                        columna++;
                    }
                    else
                    {
                        NoLoadPrealert_Row[columna] = dr;
                        columna = 0;
                        View.Model.List_NocargueAlert.Rows.Add(NoLoadPrealert_Row);
                        NoLoadPrealert_Row = View.Model.List_NocargueAlert.NewRow();
                    }
                }), null);
            }

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
            {
                View.Progress_CargueAlertas.Value = 0;
            }), null);

            StartTimer2();

            foreach (DataRow dr in SerialesIngresados_alerta.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    RegistroGuardar = View.Model.ListRecords_Alertas.NewRow();

                    RegistroGuardar["Aler_SAP"] = dr[0].ToString();
                    RegistroGuardar["Aler_Description"] = dr[1].ToString();
                    RegistroGuardar["Aler_Cantidad"] = dr[2].ToString();
                    RegistroGuardar["Aler_Serial"] = dr[3].ToString();
                    RegistroGuardar["Aler_Observacion"] = dr[4].ToString();

                    View.Model.ListRecords_Alertas.Rows.Add(RegistroGuardar);

                }), null);
            }

            estado_cargue = true;
        }

        /**
         * @param String codigoSAP: recibe el codigo SAP del producto a registrar en el carge masivo
         * @return Boolean existencia: Devuelve True si el codigo SAP esta registrado, de lo contrario retorna FALSE
        **/
        private Boolean BuscarCodigo_SAP(string codigoSAP)
        {
            string ConsultaBuscar = "SELECT TOP 1 Brand FROM Master.Product WHERE Brand= '" + codigoSAP + "'";
            DataTable Result = service.DirectSQLQuery(ConsultaBuscar, "", "Master.Product", Local);
            Boolean existencia = false;

            if (Result.Rows.Count > 0)
            {
                existencia = true;
            }
            return existencia;
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");
            View.Model.ListRecords_Alertas = new DataTable("ListadoRegistros");
            View.Model.List_Nocargue = new DataTable("ListadoRegistros");
            View.Model.List_NocargueAlert = new DataTable("ListadoRegistros");

            //Asigno las columnas restantes
            View.Model.ListRecords_Alertas.Columns.Add("Aler_SAP", typeof(String));
            View.Model.ListRecords_Alertas.Columns.Add("Aler_Serial", typeof(String));
            View.Model.ListRecords_Alertas.Columns.Add("Aler_Description", typeof(String));
            View.Model.ListRecords_Alertas.Columns.Add("Aler_Cantidad", typeof(String));
            View.Model.ListRecords_Alertas.Columns.Add("Aler_Observacion", typeof(String));

            // Datatable lista de seriales no cargados - Recibo/escanear seriales
            View.Model.List_Nocargue.Columns.Add("Serial1", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Mac1", typeof(String));
            View.Model.List_Nocargue.Columns.Add("CodSAP", typeof(String));
            View.Model.List_Nocargue.Columns.Add("TipoRecibo", typeof(String));
            View.Model.List_Nocargue.Columns.Add("TipoOrigen", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Remision", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Caja", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue

            // Datatable lista de seriales no cargados - Recibo Prealertas
            View.Model.List_NocargueAlert.Columns.Add("Serial", typeof(String));
            View.Model.List_NocargueAlert.Columns.Add("CodSAP", typeof(String));
            View.Model.List_NocargueAlert.Columns.Add("Modelo", typeof(String));
            View.Model.List_NocargueAlert.Columns.Add("Cantidad", typeof(String));
            View.Model.List_NocargueAlert.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue, seriales repetidos o sap no existe

            //Asigno las columnas restantes
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void OnDeleteDetails(object sender, EventArgs e)
        {
            if (View.ListadoEquiposAProcesar.SelectedItems.Count == 0)
                Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");

            //borra de la lista los elementos seleccionados
            while (View.ListadoEquiposAProcesar.SelectedItems.Count > 0)
                View.Model.ListRecords.Rows.RemoveAt(View.ListadoEquiposAProcesar.Items.IndexOf(View.ListadoEquiposAProcesar.SelectedItem));
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

                        if (index >= offset && index != 4 && index != 5 && index != 9)
                        {
                            //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                            foreach (DataRowView dr in View.ListadoEquiposAProcesar.SelectedItems)
                            {
                                dr[index] = valueOfItem;
                            }
                        }
                    }
                    else
                    {
                        //Filtramos las columnas descartando las que no son para replicar
                        if (index >= offset && index != 4 && index != 5 && index != 9)
                        {
                            //Util.ShowMessage(index.ToString());
                            //var valor_real = View.ListadoEquiposAProcesar.Items.GetItemAt(View.ListadoEquiposAProcesar.SelectedIndex);
                            //Util.ShowMessage(valor_real.ToString());
                            for (int i = View.ListadoEquiposAProcesar.SelectedIndex; i < View.Model.ListRecords.Rows.Count; i++)
                                View.Model.ListRecords.Rows[i][index] = View.Model.ListRecords.Rows[View.ListadoEquiposAProcesar.SelectedIndex][index];
                        }
                    }
                }
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

            View.Progress_CargueAlertasSer.Value = 0;
            estado_almacenamiento = false;
            View.GetEstado_CargueSer.Text = "Iniciando guardado";
            hilo_saveSerial.Start(View.Model.ListRecords); //Se inicia el proceso background utilizando el Hilo

            StartTimer_SerialesOnSaveBd(); //Inicia el timmer para mostrar visualmente el progreso del guardado en la BD
        }

        private void SaveSerial(Object e)
        {
            //Variables Auxiliares
            String ConsultaGuardar = "Declare @RowId int ";
            String ConsultaGuardarMovimiento = "";
            try
            {
                foreach (DataRow DataRow in this.serialesSave_Aux.Rows)
                {
                    //Obtengo la cantidad de columnas del listado
                    int ContadorCampos = this.serialesSave_Aux.Columns.Count;

                    //Construyo la consulta para guardar los datos
                    ConsultaGuardar += " INSERT INTO dbo.EquiposCLARO(Serial,Mac,Tipo_REC,Codigo_SAP,ProductoID,FAMILIA,TIPO_ORIGEN,ORIGEN,Ciudad,CENTRO,CONSECUTIVO,CONTROL,ESTADO) VALUES(";

                    //Obtengo los datos de cada campo con su nombre
                    foreach (DataColumn c in this.serialesSave_Aux.Columns)
                    {
                        if (String.IsNullOrEmpty(DataRow["ProductoID"].ToString()))
                        {
                            Util.ShowError("El campo Nombre Material no puede ser vacio.");
                            return;
                        }

                        //Adiciono cada dato a la consulta
                        ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                        //Evaluo el contador de columnas para saber si adiciono la coma
                        ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                        //Disminuyo el contador
                        ContadorCampos--;


                    }

                    ConsultaGuardar += ", 'CUARENTENA') SET @RowId = SCOPE_IDENTITY();";

                    ConsultaGuardar += "INSERT INTO dbo.TrackEquiposClaro(ID_SERIAL,Serial,Mac,FECHA_INGRESO,ESTADO_RECIBO) VALUES (@RowId, '" + DataRow[0].ToString() + "', '" + DataRow[1].ToString() + "', getdate(), 'RECIBO'); ";

                    ConsultaGuardar += "exec sp_InsertarNuevo_Movimiento 'EQUIPO RECIBIDO ENTRADA ALMACEN','RECIBIDO','ESPERANDO POR SER LIBERADO',''"
                                    + ",@RowId,'RECIBO','UBICACIONENTRADAALMACEN','" + this.user + "','';";

                    ContadorSave++;
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {
                    Console.WriteLine(ConsultaGuardar);
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                }
                estado_almacenamiento = true;
                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");
                //Reinicio los campos
                //LimpiarDatosIngresoSeriales();
                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    ClearForm();
                }), null);
                trOnSave.Dispose();
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        private void OnSaveDetails_Prealert(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords_Alertas.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "";
            Int32 ContadorCampos, ContadorFilas = 0;

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords_Alertas.Rows)
                {
                    //Aumento el contador de filas
                    //ContadorFilas++;

                    //if (ContadorFilas % 50 != 0)
                    //{
                        //Obtengo la cantidad de columnas del listado
                        ContadorCampos = View.Model.ListRecords_Alertas.Columns.Count;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " INSERT INTO DBO.preAlerta_EQUIPOSCLARO(aleEquip_codSap,aleEquip_serial,aleEquip_codDescripcion,aleEquip_cantidad,aleEquip_observaciones) VALUES(";

                        //Obtengo los datos de cada campo con su nombre
                        foreach (DataColumn c in View.Model.ListRecords_Alertas.Columns)
                        {
                            //Adiciono cada dato a la consulta
                            ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                            //Evaluo el contador de columnas para saber si adiciono la coma
                            ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                            //Disminuyo el contador
                            ContadorCampos--;
                        }
                        ConsultaGuardar += ") ";

                        //ConsultaGuardar += "INSERT INTO dbo.TrackEquiposClaro(ID_SERIAL,Serial,Mac,FECHA_INGRESO,ESTADO_RECIBO) VALUES (@@IDENTITY, '" + DataRow[0].ToString() + "', '" + DataRow[1].ToString() + "', getdate(), 'RECIBO') ";
                    //}
                    //else
                    //{
                    //    //Obtengo la cantidad de columnas del listado
                    //    ContadorCampos = View.Model.ListRecords_Alertas.Columns.Count;

                    //    //Construyo la consulta para guardar los datos
                    //    ConsultaGuardar += ConsultaGuardar += " INSERT INTO DBO.preAlerta_EQUIPOSCLARO(aleEquip_codSap,aleEquip_serial,aleEquip_codDescripcion,aleEquip_cantidad,aleEquip_observaciones) VALUES(";

                    //    //Obtengo los datos de cada campo con su nombre
                    //    foreach (DataColumn c in View.Model.ListRecords_Alertas.Columns)
                    //    {
                    //        //Adiciono cada dato a la consulta
                    //        ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                    //        //Evaluo el contador de columnas para saber si adiciono la coma
                    //        ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                    //        //Disminuyo el contador
                    //        ContadorCampos--;
                    //    }

                    //    ConsultaGuardar += ")";

                    //    //ConsultaGuardar += "INSERT INTO dbo.TrackEquiposClaro(ID_SERIAL,SERIAL,MAC,FECHA_INGRESO,ESTADO_RECIBO) VALUES (@@IDENTITY, '" + DataRow[0].ToString() + "', '" + DataRow[1].ToString() + "', getdate(), 'RECIBO') ";

                    //    //Ejecuto la consulta
                    //    //Console.WriteLine(ConsultaGuardar);
                    //    service.DirectSQLNonQuery(ConsultaGuardar, Local);

                    //    //Limpio la consulta para volver a generar la nueva
                    //    ConsultaGuardar = "";
                    //}
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                }

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                View.Model.ListRecords_Alertas.Rows.Clear();
                View.Model.ListadoPreaTipoRecoleccion = service.GetMMaster(new MMaster { MetaType = new MType { Code = "PREATREC" } });
                View.Model.ListadoPreaTipoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "PREATORIGE" } });
                return;
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message);
            }
        }

        private void OnSavePrealert(object sender, EventArgs e)
        {
            if (View.Model.ListRecords_Alertas.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para guardar");
            }
            else
            {
                hilo_savePreAlert = new Thread(new ParameterizedThreadStart(SavePreAlert));
                hilo_savePreAlert.SetApartmentState(ApartmentState.STA);
                hilo_savePreAlert.IsBackground = true;
                hilo_savePreAlert.Priority = ThreadPriority.Highest;

                cont_registros = 0;
                this.ContadorSave2 = 0;

                this.RegistrosSave_Aux = View.Model.ListRecords_Alertas;

                View.Progress_CargueAlertas.Value = 0;
                this.estado_almacenamientoAlert = false;
                View.GetEstado_Cargue.Text = "Iniciando guardado";
                hilo_savePreAlert.Start(View.Model.ListRecords); //Se inicia el proceso background utilizando el Hilo

                int a = View.Model.ListRecords.Rows.Count;
                StartTimer_RegistrosAlertOnSaveBd(); //Inicia el timmer para mostrar visualmente el progreso del guardado en la BD
            }
        }

        private void StartTimer_RegistrosAlertOnSaveBd()
        {
            // Creamos diferentes hilos a través de un temporizador
            trOnSaveAlert = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont_registros++;

                    if (cont_registros == 1)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos";
                    }
                    else if (cont_registros == 2)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos.";
                    }
                    else if (cont_registros == 3)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos..";
                    }
                    else if (cont_registros == 4 || cont_registros > 4)
                    {
                        View.GetEstado_Cargue.Text = "Guardando equipos...";
                        cont_registros = 0;
                    }

                    // Implementación del método anónimo
                    if (estado_almacenamientoAlert == false)
                    {
                        View.Progress_CargueAlertas.Value = ((ContadorSave2 * 100D) / View.Model.ListRecords_Alertas.Rows.Count);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        trOnSaveAlert.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Proceso de guardado terminado.";
                            View.Progress_CargueAlertas.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueAlertas, 0, 300);
        }

        private void SavePreAlert(Object e)
        {
            String FechaEmitido ="";
            DateTime aux_fechaEmitido;
            string dateFormat = "MM/dd/yyyy";

            View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                FechaEmitido = View.GetPreFecha_Emision.Text;
                

                if (!String.IsNullOrEmpty(FechaEmitido))
                {
                        aux_fechaEmitido = DateTime.Parse(View.GetPreFecha_Emision.Text);
                        FechaEmitido = aux_fechaEmitido.ToString(dateFormat);
                }
                }), null);


            try
            {
                String Cadena = "";

                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    //guarda unicamente el nombre del archivo sin toda la ruta
                    Cadena = View.GetUpLoadFile_Prealerta.FileName.ToString();
                }), null);

                //String Cadena = "C:\\Users\\Desarrollo\\Desktop\\cargue_PREALERTA_claro.xls";
                string[] split = Cadena.Split(new Char[] { '\\' });
                Cadena = split.Last();

                

                String ConsultaGuardar = "Declare @prea_id int, @aleEquip_id int; ";

                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.preAlertaCLARO(prea_archivo,prea_consecutivo,prea_origen,prea_nombreOrigen,prea_direccion,prea_Contacto,prea_contactMovil,prea_nroPedido,prea_tipoRecoleccion,prea_fechaEmitido)" +
                    "VALUES('" + Cadena.ToUpper() + "','" + View.GetPreaConsecutivo.Text + "','" + View.GetPreaTipo_Origen.Text + "','" + View.GetPreaOrigen.Text + "','" + View.GetPreaDireccion.Text + "','" + View.GetPreaNombre_contacto.Text +
                    "','" + View.GetPreaCelular_contacto.Text + "','" + View.GetPreaNro_Pedido.Text + "','" + View.GetPreaTipo_Recoleccion.Text + "','" + FechaEmitido + "'); ";
                    ConsultaGuardar = ConsultaGuardar + "SET @prea_id = SCOPE_IDENTITY();";
                }), null);

                int contcoma = 1;
                foreach (DataRow DataRow in this.RegistrosSave_Aux.Rows)
                {
                    ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.preAlerta_EQUIPOSCLARO(aleEquip_codSap,aleEquip_serial,aleEquip_codDescripcion,aleEquip_cantidad,aleEquip_observacion) VALUES(";
                    foreach (DataColumn c in this.RegistrosSave_Aux.Columns)
                    {
                        if (contcoma == this.RegistrosSave_Aux.Columns.Count)
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
                    ConsultaGuardar = ConsultaGuardar + "INSERT INTO dbo.PreAlertaCLARO_EquiposCLARO(prea_id,aleEquip_id) VALUES(@prea_id, @aleEquip_id);";
                    contcoma = 1;
                    ContadorSave2++;
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {
                    Console.WriteLine(ConsultaGuardar);
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);
                    ConsultaGuardar = "";
                }

                this.estado_almacenamientoAlert = true;
                Util.ShowMessage("Registros guardados satisfactoriamente.");
                trOnSaveAlert.Dispose();

                View.Dispatcher_PreAlertas.Invoke(new System.Action(() =>
                {
                    ClearForm();
                }), null);
            }
            catch (Exception ex)
            {
                Util.ShowError("Se presentó un error al momento de guardar los registros. Error: " + ex.Message);
            }
        }

        private void OnExportCargue(object sender, EventArgs e)
        {
            OnExportCargue_Excel(View.Model.List_Nocargue);
        }

        private void OnExportCargueAlerta(object sender, EventArgs e)
        {
            OnExportCargue_Excel(View.Model.List_NocargueAlert);
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

        public void ClearForm()
        {
            View.GetPreaNro_Pedido.Text = "";
            View.GetPreaConsecutivo.Text = "";
            View.GetPreaTipo_Recoleccion.Text = "";
            View.GetPreaTipo_Origen.Text = "";
            View.GetPreaOrigen.Text = "";
            View.GetPreFecha_Emision.Text = "";
            View.GetPreaDireccion.Text = "";
            View.GetPreaNombre_contacto.Text = "";
            View.GetPreaCelular_contacto.Text = "";

            SerialesIngresados.Rows.Clear();
            SerialesIngresados_alerta.Rows.Clear();
            View.Model.ListRecords.Clear();
            View.Model.ListRecords_Alertas.Rows.Clear();
            View.Model.List_NocargueAlert.Rows.Clear();
            View.Model.List_Nocargue.Rows.Clear();
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Tipo Recibo

            //IList<MMaster> ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Tipo Recibo";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoTipoREC);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Tipo_Rec"));
            Txt.SetValue(ComboBox.WidthProperty, (double)80);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Tipo_Rec", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna SAP- Combobox

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Nombre Material";
            Columna.Header = "Cod. Product SAP";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));

            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoProductos);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Brand"); // Brand es una columna de la Base de datos -  Este valor es el que muestra en la view
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Brand"); // Este valor es el que almacena el control al momento de leerlo
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ProductoID"));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarNombreMaterial)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ProductoID", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Nombre Material

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Producto";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.WidthProperty, (double)250);
            Txt.SetValue(ComboBox.HeightProperty, (double)22);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Codigo_SAP"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Codigo_SAP", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Familia

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Familia";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("FAMILIA"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("FAMILIA", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Tipo Origen

            //IList<MMaster> ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Tipo Origen";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoOrigen);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("TIPO_ORIGEN"));
            Txt.SetValue(ComboBox.WidthProperty, (double)80);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidar_TipoOrigen)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("TIPO_ORIGEN", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Origen

            //IList<MMaster> ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Origen";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));


            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoAliado);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ORIGEN"));
            Txt.SetValue(ComboBox.WidthProperty, (double)130);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarOrigen)); //NUEVO EVENTO
            Txt.AddHandler(System.Windows.Controls.ComboBox.MouseEnterEvent, new MouseEventHandler(OnValidarOrigen_MouseHover)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ORIGEN", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Ciudad

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Ciudad";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)80);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Ciudad"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Ciudad", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Centro

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Centro";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)55);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("CENTRO"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("CENTRO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Consecutivo

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Remision";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)65);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("CONSECUTIVO"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);
            Txt.SetValue(TextBox.MarginProperty, new Thickness(0, 0, 0, 0));

            // add textbox template
            //Columna.Width = (double)80;
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("CONSECUTIVO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Caja

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "N. Caja";
            Txt = new FrameworkElementFactory(typeof(TextBox));
            Txt.SetValue(TextBox.MinWidthProperty, (double)45);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("CAJA"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);
            Txt.SetValue(TextBox.MarginProperty, new Thickness(0, 0, 0, 0));
            // Txt.SetValue(TextBox.ToolTipProperty, "Número de caja");

            // add textbox template
            // Columna.Width = (double)80;
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;

            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("CAJA", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnValidarOrigen(object sender, SelectionChangedEventArgs e)
        {
            //ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            //Util.ShowMessage(lbi.Content.ToString());

            // Util.ShowMessage(((ComboBox)sender).Tag.ToString());
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    dr[8] = View.Model.ListadoAliado.Where(f => f.Code == dr[7].ToString()).First().Code2.ToString();
                    dr[9] = View.Model.ListadoAliado.Where(f => f.Code == dr[7].ToString()).First().Description.ToString();
                    break;
                }
            }
            return;
        }

        /** Se activa cuando paso el mouse por encima a cualquiera de los combobox del listview **/
        private void OnValidarOrigen_MouseHover(object sender, MouseEventArgs evt)
        {
            String filtro = "";
            ComboBox comboBox = sender as ComboBox;
            Console.WriteLine("NOMBRE: " + comboBox.Name + " code: ");
            try
            {
                foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
                {
                    if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
                    {
                        filtro = dr[6].ToString();
                        break;
                    }
                }

                if (filtro != "")
                {
                    comboBox.ItemsSource = View.Model.ListadoAliado.Where(f => f.Name.StartsWith(filtro));
                }
                else
                {
                    comboBox.ItemsSource = null;
                }
            }
            catch (Exception e)
            {

            }
            return;
        }

        private void OnValidar_TipoOrigen(object sender, SelectionChangedEventArgs e)
        {
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            IList<MMaster> ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Origen";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));

            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoAliado.Where(f => f.Name.StartsWith(((ComboBox)sender).SelectedValue.ToString())));
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ORIGEN"));//cambia la columna serial
            Txt.SetValue(ComboBox.WidthProperty, (double)130);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));//obtiene el dato de origen
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarOrigen)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            // View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView

            int cont = 0;

            //var yourListViewItem = (ListViewItem)View.ListadoEquiposAProcesar.ItemContainerGenerator.ContainerFromItem(View.ListadoEquiposAProcesar.SelectedItem);
            //ComboBox cb = FindByName("", yourListViewItem) as ComboBox;
            // Util.ShowMessage(cb.Content + " IsChecked :" + cb.IsChecked);

            foreach (GridViewColumn rere in View.ListadoEquipos.Columns)
            {
                if (cont == 7)
                {
                    //Util.ShowMessage(i.GetType().ToString());
                    //i.CellTemplate = new DataTemplate();
                    //i.CellTemplate.VisualTree = Txt;
                }
                cont++;
            }

            //DataRowView dataRow = (DataRowView)View.ListadoEquiposAProcesar.SelectedItem;            

            //Object[] cellValue = dataRow.Row.ItemArray;

            //int cont = 0;
            //foreach (GridViewColumn i in View.ListadoEquipos.Columns)
            //{   
            //    Console.WriteLine("------------- "+i.Header.ToString()+" "+cont);
            //    cont++;
            //}



            //foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            //{
            //    if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
            //    {
            //        dr[7] = 
            //    }
            //}

            //ComboBox.ItemsSourceProperty, ComboBox.ItemsSourceProperty, View.Model.ListadoAliado.Where(f => f.Name.StartsWith(filtro)));

            //GridViewColumn Columna;
            //FrameworkElementFactory Txt;
            //Assembly assembly;
            //string TipoDato;

            //IList<MMaster> ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Origen";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));

            //String filtro = "";

            //foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            //{
            //    if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
            //    {
            //        filtro = dr[6].ToString();

            //        Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoAliado.Where(f => f.Name.StartsWith(filtro)));
            //        Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //        Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //        Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ORIGEN"));//cambia la columna serial
            //        Txt.SetValue(ComboBox.WidthProperty, (double)130);
            //        Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));//obtiene el dato de origen
            //        Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarOrigen)); //NUEVO EVENTO

            //        // add textbox template
            //        Columna.CellTemplate = new DataTemplate();
            //        Columna.CellTemplate.VisualTree = Txt;
            //        // View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView

            //        i.CellTemplate = new DataTemplate();
            //        i.CellTemplate.VisualTree = Txt;

            //        //int cont = 0;
            //        //foreach (GridViewColumn i in View.ListadoEquipos.Columns)
            //        //{
            //        //    Console.Write(cont);
            //        //    if (cont == 7)
            //        //    {
            //        //        i.CellTemplate = new DataTemplate();
            //        //        i.CellTemplate.VisualTree = Txt;
            //        //    }
            //        //    cont++;
            //        //}
            //        break;
            //    }  
            //    //Util.ShowMessage(dr[6].ToString());
            //}
            //return;
        }

        private FrameworkElement FindByName(string name, FrameworkElement root)
        {
            Stack<FrameworkElement> tree = new Stack<FrameworkElement>();
            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (current.Name == name)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }

            return null;
        }

        /**
         * Se activa cuando el usuario selecciona un item del combobox del modulo recibo ubicado en el tableview
         **/
        private void OnValidarNombreMaterial(object sender, SelectionChangedEventArgs e)
        {
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                if (dr[0].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    dr[4] = View.Model.ListadoProductos.Where(f => f.Brand == dr[3].ToString()).First().ProductCode.ToString();
                    dr[5] = View.Model.ListadoProductos.Where(f => f.Brand == dr[3].ToString()).First().Manufacturer.ToString();
                    break;
                }
            }

            return;
        }

        public void OnKillProcess(object sender, EventArgs e)
        {
            Process[] proceso = Process.GetProcessesByName("EXCEL");

            if (proceso.Length > 0)
                proceso[0].Kill();
        }

        #endregion
    }
}
