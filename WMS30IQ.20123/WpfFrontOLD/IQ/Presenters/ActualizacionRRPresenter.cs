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
using System.Diagnostics;
using System.Text;

namespace WpfFront.Presenters
{

    public interface IActualizacionRRPresenter
    {
        IActualizacionRRView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ActualizacionRRPresenter : IActualizacionRRPresenter
    {
        public IActualizacionRRView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        private DataTable SerialesIngresados = new DataTable();
        private DataRow NoLoad_Row = null;
        private Timer t;
        private Timer t1;
        private Timer tsap;
        private Timer tsap_serial;
        private Timer tdb;
        private Timer tlib;
        private Timer trOnSave;
        private Thread updateLabelThread;
        private Thread hilo_saveLiberacion;
        private int cont_registros = 0, ContadorSave = 0;
        private DataTable RegistrosSave_Aux;
        private Boolean estado_cargue = false, busqueda_Repetidos = false, busqueda_SAP = false, busqueda_Repetidosdb = false, busqueda_SAP_Serial = false, busqueda_liberado = false, estado_almacenamiento = false;
        private OleDbConnection oledbcon;
        private OleDbDataAdapter adaptador;

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public ActualizacionRRPresenter(IUnityContainer container, IActualizacionRRView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ActualizacionRRModel>();

            #region Metodos

            //view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ClearDetails += new EventHandler<EventArgs>(this.OnClearDetails);
            view.CargaMasiva_RR += new EventHandler<EventArgs>(this.OnCargaMasiva_RR);
            view.KillProcess += new EventHandler<EventArgs>(this.OnKillProcess);
            view.ExportCuarentena += new EventHandler<EventArgs>(this.OnExportCuarentena);
            view.ExportNoLiberados += new EventHandler<EventArgs>(this.OnExportNoLiberados);
            #endregion

            #region Datos

            //Cargo la variable para las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo los productos en el tableview
            //View.Model.ListadoProductos = service.GetProduct(new Product { Reference = "425" });

            //Cargo los listados de los detalles
            //View.Model.ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            ////View.Model.ListadoCiudades = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD" } });
            //View.Model.ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            ////View.Model.ListadoCodigoSAP = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TELMEXCOD" } });
            //View.Model.ListadoEstadoRR = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADO RR" } });
            //View.Model.ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            //View.Model.ListadoCentros = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CENTRO" } });
            //View.Model.ListadoFamilias = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FAMILIA" } });

            //Cargo los datos del listado
            CargarDatosDetails();
            CargarEquiposCuarentena();

            #endregion
        }

        #region Metodos

        private void OnCargaMasiva_RR(object sender, EventArgs e)
        {
            Thread hilo_repetidos = new Thread(new ParameterizedThreadStart(SetRepeat));
            hilo_repetidos.SetApartmentState(ApartmentState.STA);
            hilo_repetidos.IsBackground = true;
            hilo_repetidos.Priority = ThreadPriority.Highest;

            //creo la cadena de conexion
            String Cadena = View.GetUpLoadFile.FileName.ToString();
            String conexion = "Provider=Microsoft.Jet.OleDb.4.0; Extended Properties=\"Excel 8.0; HDR=yes\"; Data Source=" + Cadena;
            try
            {
                //creo la coneccion para leer  el .xls
                //OleDbConnection oledbcon = default(OleDbConnection);
                oledbcon = new OleDbConnection(conexion);

                //traigo los datos del .xls
                adaptador = new OleDbDataAdapter("select SERIAL, DIRECCIONABLE, ESTADO_RR, DOCUMENTO_SAP, CODIGO_SAP, DESCRIPCION, LOTE from [Hoja1$]", oledbcon);

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
                    View.Progress_CargueRR.Value = 0;
                    View.GetEstado_Cargue.Text = "Iniciando operación";
                    hilo_repetidos.Start(SerialesIngresados);

                    StartTimer();//Inicia el timmer para mostrar visualmente el progreso del cargue masivo

                }
            }
            catch (Exception ex)
            {
                Util.ShowError("El archivo a cargar no cuenta con la estructura correcta.");
                View.GetUpLoadFile.IsEnabled = true;
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
                    if (View.Progress_CargueRR.Value < 100D && busqueda_Repetidos == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueRR.Value = cont_repeat * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de equipos duplicados terminada.";
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
        }

        int cont2 = 0;
        int cont_repeat2 = 0;
        private void StartTimerSAP()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tsap = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont2++;

                    if (cont2 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Codigos SAP";
                    }
                    else if (cont2 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Codigos SAP.";
                    }
                    else if (cont2 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Codigos SAP..";
                    }
                    else if (cont2 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Codigos SAP...";
                        cont2 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_SAP == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueRR.Value = cont_repeat2 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tsap.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de codigos SAP terminada.";
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
        }

        int cont3 = 0;
        int cont_repeat3 = 0;
        private void StartTimerdb()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tdb = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont3++;

                    if (cont3 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales en Base de Datos";
                    }
                    else if (cont3 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales en Base de Datos.";
                    }
                    else if (cont3 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales en Base de Datos..";
                    }
                    else if (cont3 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Seriales en Base de Datos...";
                        cont3 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_Repetidosdb == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueRR.Value = cont_repeat3 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tdb.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Validacion de seriales terminada.";
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
        }

        int cont4 = 0;
        int cont_repeat4 = 0;
        private void StartTimerSAP_Serial()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tsap_serial = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont4++;

                    if (cont4 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Coincidencia de SAP con Serial";
                    }
                    else if (cont4 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Coincidencia de SAP con Serial.";
                    }
                    else if (cont4 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Coincidencia de SAP con Serial..";
                    }
                    else if (cont4 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Coincidencia de SAP con Serial...";
                        cont4 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_SAP_Serial == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueRR.Value = cont_repeat4 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tsap_serial.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        //Util.ShowMessage("Cargue masivo finalizado");
                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de coincidencia terminada.";
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
        }

        int cont5 = 0;
        int cont_repeat5 = 0;
        private void StartTimerLiberado()
        {
            int num_seriales = SerialesIngresados.Rows.Count;
            // Creamos diferentes hilos a través de un temporizador
            tlib = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont5++;

                    if (cont5 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos liberados";
                    }
                    else if (cont5 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos liberados.";
                    }
                    else if (cont5 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos liberados..";
                    }
                    else if (cont5 == 4 || cont5 > 4)
                    {
                        View.GetEstado_Cargue.Text = "Buscando equipos liberados...";
                        cont5 = 0;
                    }

                    // Implementación del método anónimo
                    if (View.Progress_CargueRR.Value < 100D && busqueda_liberado == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_CargueRR.Value = cont_repeat5 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tlib.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Verificación de equipos liberados terminada.";
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
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
                        View.Progress_CargueRR.Value = ((View.Model.ListRecords.Rows.Count * 100D) / num_seriales);
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        t1.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        // Invocamos de nuevo al mismo delegado para modificar la apariencia visual de la barra de progreso
                        (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                        {
                            View.GetEstado_Cargue.Text = "Carga de archivo terminada.";
                            View.Progress_CargueRR.Value = 100D;
                            //t1.Dispose();
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 1000);
        }

        private void StartTimer_RegistrosOnSaveBd()
        {
            // Creamos diferentes hilos a través de un temporizador
            trOnSave = new Timer(new TimerCallback((o) =>
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
                    if (estado_almacenamiento == false)
                    {
                        View.Progress_CargueRR.Value = ((ContadorSave * 100D) / View.Model.ListRecords.Rows.Count);
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
                            View.Progress_CargueRR.Value = 100D;
                        }));
                    }
                }));
            }), View.Progress_CargueRR, 0, 300);
        }

        private void SetRepeat(object o)
        {
            busqueda_Repetidos = false;
            busqueda_SAP = false;
            busqueda_Repetidosdb = false;
            busqueda_SAP_Serial = false;
            estado_cargue = false;
            List<String> listRepetidos = new List<String>(); //Guarda los seriales que estan repetidos en el carge masivo
            List<String> listCodigoSAP = new List<String>(); // Guarda los seriales cuyo codigo SAP no es valido
            List<String> listRepetidosBD = new List<String>(); // Guarda los seriales que ya estan en la base de datos
            List<String> listCoincidente_SAP_Serial = new List<String>(); // Guarda los seriales cuyo SAP no coincide con el serial
            List<String> listLiberado = new List<String>(); // Guarda los seriales que ya se encuentran liberados

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
                            listRepetidos.Add(SerialesIngresados.Rows[j]["SERIAL"].ToString());//agrego el serial a la lista de repetidos

                            listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[j]["SERIAL"].ToString(), 
                                SerialesIngresados.Rows[j]["CODIGO_SAP"].ToString(), SerialesIngresados.Rows[j]["DESCRIPCION"].ToString(),
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

            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                View.Progress_CargueRR.Value = 0;
            }), null);

            StartTimerLiberado();

            //validamos que los seriales ya se encuentren liberados
            String ConsultaBuscarliberado = "";
            int temp5 = 0;
            while (temp5 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarliberado = "SELECT RowID,serial,MAC,CODIGO_SAP,productoid,fecha_doc,fecha_doc_sap,ESTADO_RR from dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp5]["SERIAL"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarliberado, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count > 0)
                {
                    String doc_rr = Resultado.Rows[0]["fecha_doc"].ToString();
                    String doc_sap = Resultado.Rows[0]["fecha_doc_sap"].ToString();
                    String estado_rr = Resultado.Rows[0]["ESTADO_RR"].ToString();

                    if (estado_rr == "" || estado_rr == null || doc_rr == "" || doc_rr == null || doc_sap == "" || doc_sap == null)
                    {
                        temp5++;
                    }
                    else
                    {
                        listLiberado.Add(SerialesIngresados.Rows[temp5]["SERIAL"].ToString().ToUpper());

                        listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp5]["SERIAL"].ToString(), 
                                SerialesIngresados.Rows[temp5]["CODIGO_SAP"].ToString(), SerialesIngresados.Rows[temp5]["DESCRIPCION"].ToString(),
                                "Serial ya liberado"});

                        SerialesIngresados.Rows.RemoveAt(temp5);
                    }

                }
                else
                {
                    temp5++;
                }
                cont_repeat5++;
            }

            busqueda_liberado = true;
            Thread.Sleep(1000);
            tlib.Dispose();

            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                View.Progress_CargueRR.Value = 0;
            }), null);

            StartTimerSAP();

            //validamos que los SAP existan en la base de datos
            String ConsultaBuscarSAP = "";
            int temp = 0;
            while (temp < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarSAP = "SELECT TOP 1 Brand FROM Master.Product WHERE Brand= '" + SerialesIngresados.Rows[temp]["CODIGO_SAP"].ToString() + "'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSAP, "", "Master.Product", Local);
                if (Resultado.Rows.Count == 0)
                {
                    listCodigoSAP.Add(SerialesIngresados.Rows[temp]["SERIAL"].ToString().ToUpper());

                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp]["SERIAL"].ToString(), 
                                SerialesIngresados.Rows[temp]["CODIGO_SAP"].ToString(), SerialesIngresados.Rows[temp]["DESCRIPCION"].ToString(),
                                "Codigo SAP no existe en la Base de Datos"});

                    SerialesIngresados.Rows.RemoveAt(temp);
                }
                else
                {
                    temp++;
                }
                cont_repeat2++;
            }

            busqueda_SAP = true;
            Thread.Sleep(1000);
            tsap.Dispose();

            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                View.Progress_CargueRR.Value = 0;
            }), null);

            StartTimerdb();

            //validamos que los seriales existan en la base de datos
            String ConsultaBuscar = "";
            int temp2 = 0;
            while (temp2 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscar = "SELECT TOP 1 Serial FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp2]["SERIAL"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count == 0)
                {
                    listRepetidosBD.Add(SerialesIngresados.Rows[temp2]["SERIAL"].ToString().ToUpper());

                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp2]["SERIAL"].ToString(), 
                                SerialesIngresados.Rows[temp2]["CODIGO_SAP"].ToString(), SerialesIngresados.Rows[temp2]["DESCRIPCION"].ToString(),
                                "Serial no existe en la Base de Datos"});

                    SerialesIngresados.Rows.RemoveAt(temp2);
                }
                else
                {
                    temp2++;
                }
                cont_repeat3++;
            }

            busqueda_Repetidosdb = true;
            Thread.Sleep(1000);
            tdb.Dispose();

            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                View.Progress_CargueRR.Value = 0;
            }), null);

            StartTimerSAP_Serial();

            //validamos que los SAP Coincida con el serial ingresado
            String ConsultaBuscarSAP_Serial = "";
            int temp3 = 0;
            while (temp3 < SerialesIngresados.Rows.Count)
            {
                ConsultaBuscarSAP_Serial = "SELECT TOP 1 Serial FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + SerialesIngresados.Rows[temp3]["SERIAL"].ToString() + "') AND UPPER(CODIGO_SAP) = UPPER('" + SerialesIngresados.Rows[temp3]["CODIGO_SAP"].ToString() + "')";

                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSAP_Serial, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count == 0)
                {
                    listCoincidente_SAP_Serial.Add(SerialesIngresados.Rows[temp3]["SERIAL"].ToString().ToUpper());

                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp3]["SERIAL"].ToString(), 
                                SerialesIngresados.Rows[temp3]["CODIGO_SAP"].ToString(), SerialesIngresados.Rows[temp3]["DESCRIPCION"].ToString(),
                                "Serial y Codigo SAP no coinciden"});

                    SerialesIngresados.Rows.RemoveAt(temp3);
                }
                else
                {
                    temp3++;
                }
                cont_repeat4++;
            }

            busqueda_SAP_Serial = true;
            Thread.Sleep(1000);
            tsap_serial.Dispose();

            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                View.Progress_CargueRR.Value = 0;
            }), null);

            this.MostrarErrores_Cargue(listNoCargue); // Agrega a un segundo listview los equipos que no fueron cargados

            StartTimer2();

            listRepetidos = listRepetidos.Distinct().ToList();
            listCodigoSAP = listCodigoSAP.Distinct().ToList();
            listRepetidosBD = listRepetidosBD.Distinct().ToList();
            listCoincidente_SAP_Serial = listCoincidente_SAP_Serial.Distinct().ToList();

            cont = 0;
            cont2 = 0;
            cont3 = 0;
            cont4 = 0;

            CargarListDetails(listRepetidos, listCodigoSAP, listRepetidosBD, listCoincidente_SAP_Serial);

            estado_cargue = true;
        }

        private void MostrarErrores_Cargue(List<String> listNoCargue)
        {
            int columna = 0;
            View.Dispatcher_RR.Invoke(new System.Action(() =>
            {
                NoLoad_Row = View.Model.List_Nocargue.NewRow();
            }), null);

            foreach (var dr in listNoCargue)
            {
                View.Dispatcher_RR.Invoke(new System.Action(() =>
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

        public void CargarListDetails(List<String> listRepetidos, List<String> listCodigoSAP, List<String> listRepetidosBD, List<String> listCoincidente_SAP_Serial)
        {
            foreach (DataRow dr in SerialesIngresados.Rows)
            {
                // Modificamos un UIElement que no corresponde con este hilo, para ello usamos su dispatcher
                View.Dispatcher_RR.Invoke(new System.Action(() =>
                {
                    DataRow RegistroGuardar = View.Model.ListRecords.NewRow();

                    RegistroGuardar["SERIAL"] = dr[0].ToString();
                    RegistroGuardar["SAP"] = dr[4].ToString();
                    RegistroGuardar["PRODUCTO"] = dr[5].ToString();
                    RegistroGuardar["DIRECCIONABLE"] = dr[1].ToString();
                    RegistroGuardar["ESTADORR"] = dr[2].ToString();
                    RegistroGuardar["DOCSAP"] = dr[3].ToString();
                    RegistroGuardar["LOTE"] = dr[6].ToString();

                    View.Model.ListRecords.Rows.Add(RegistroGuardar);

                }), null);
            }

            adaptador.Dispose();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");
            View.Model.ListRecords.Columns.Add("SERIAL", typeof(String));
            View.Model.ListRecords.Columns.Add("SAP", typeof(String));
            View.Model.ListRecords.Columns.Add("PRODUCTO", typeof(String));
            View.Model.ListRecords.Columns.Add("DIRECCIONABLE", typeof(String));
            View.Model.ListRecords.Columns.Add("ESTADORR", typeof(String));
            View.Model.ListRecords.Columns.Add("DOCSAP", typeof(String));
            View.Model.ListRecords.Columns.Add("LOTE", typeof(String));

            // Datatable lista de seriales liberados
            View.Model.List_Nocargue = new DataTable("ListadoNoCargue");
            View.Model.List_Nocargue.Columns.Add("Serial", typeof(String));
            View.Model.List_Nocargue.Columns.Add("CodSAP", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Modelo", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue

            // Datatable lista de seriales en Cuarentena
            View.Model.List_Cuarentena = new DataTable("ListadoCuarentena");
            View.Model.List_Cuarentena.Columns.Add("Serial", typeof(String));
            View.Model.List_Cuarentena.Columns.Add("Estado", typeof(String));
            View.Model.List_Cuarentena.Columns.Add("CodSAP", typeof(String));
            View.Model.List_Cuarentena.Columns.Add("Movimiento", typeof(String));
            View.Model.List_Cuarentena.Columns.Add("Usuario", typeof(String));
            View.Model.List_Cuarentena.Columns.Add("FIngreso", typeof(String));

            //Inicializo el datatable para registros repetidos en archivo
            View.Model.ListRecordsRep = new DataTable("ListadoRegistrosRep");
            View.Model.ListRecordsRep.Columns.Add("SERIAL", typeof(String));

            //Inicializo el datatable para registros que no existen en BD
            View.Model.ListRecordsRepDB = new DataTable("ListadoRegistrosRepDB");
            View.Model.ListRecordsRepDB.Columns.Add("SERIAL", typeof(String));

            //Inicializo el datatable para registros cuyo codigo SAP no existe
            View.Model.ListRecordsSAP = new DataTable("ListadoRegistrosSAP");
            View.Model.ListRecordsSAP.Columns.Add("SERIAL", typeof(String));

            //Inicializo el datatable para registros cuyo codigo SAP no existe
            View.Model.ListRecordsSAP_Serial = new DataTable("ListadoRegistrosSAP_Serial");
            View.Model.ListRecordsSAP_Serial.Columns.Add("SERIAL", typeof(String));
        }

        private void OnSaveDetails(object sender, EventArgs e)
        {
            if (View.Model.ListRecords.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para actualizar");
                return;
            }

            hilo_saveLiberacion = new Thread(new ParameterizedThreadStart(SaveLiberacion));
            hilo_saveLiberacion.SetApartmentState(ApartmentState.STA);
            hilo_saveLiberacion.IsBackground = true;
            hilo_saveLiberacion.Priority = ThreadPriority.Highest;

            cont_registros = 0;
            this.ContadorSave = 0;

            this.RegistrosSave_Aux = View.Model.ListRecords;

            View.Progress_CargueRR.Value = 0;
            this.estado_almacenamiento = false;
            View.GetEstado_Cargue.Text = "Iniciando guardado";
            hilo_saveLiberacion.Start(View.Model.ListRecords); //Se inicia el proceso background utilizando el Hilo

            int a = View.Model.ListRecords.Rows.Count;
            StartTimer_RegistrosOnSaveBd(); //Inicia el timmer para mostrar visualmente el progreso del guardado en la BD
        }

        private void SaveLiberacion(Object e)
        {
            try
            {
                System.Text.StringBuilder ConsultaActualizar = new StringBuilder();
                System.Text.StringBuilder ConsultaMovimiento = new StringBuilder();
                string ESTADORR = "";
                string DOCSAP = "";
                string DIRECCIONABLE = "";
                string LOTE = "";
                string SERIAL = "";
                //guarda el nuevo estado RR si no esta vacio en el archivo
                foreach (DataRow dr1 in RegistrosSave_Aux.Rows)
                {
                    // Almaceno las variables auxiliares.
                    ESTADORR = dr1["ESTADORR"].ToString().ToUpper();
                    DOCSAP = dr1["DOCSAP"].ToString().ToUpper();
                    DIRECCIONABLE = dr1["DIRECCIONABLE"].ToString().ToUpper();
                    LOTE = dr1["LOTE"].ToString().ToUpper();
                    SERIAL = dr1["SERIAL"].ToString().ToUpper();

                    ConsultaActualizar.AppendLine(" UPDATE dbo.EquiposClaro SET ESTADO_RR = IIF('" + ESTADORR + "' LIKE '' OR '" + ESTADORR + "' IS NULL,ESTADO_RR,'" + ESTADORR + "')," +
                                         " FECHA_DOC = IIF('" + ESTADORR + "' LIKE '' OR '" + ESTADORR + "' IS NULL,FECHA_DOC,GETDATE())," +
                                         " DOC_INGRESO = IIF('" + DOCSAP + "' LIKE '' OR '" + DOCSAP + "' IS NULL, DOC_INGRESO,'" + DOCSAP + "')," +
                                         " FECHA_DOC_SAP = IIF('" + DOCSAP + "' LIKE '' OR '" + DOCSAP + "' IS NULL, FECHA_DOC_SAP, GETDATE())," +
                                         " DIRECCIONABLE = '" + DIRECCIONABLE + "',  TIPO = '" + LOTE + "' WHERE Serial LIKE '" + SERIAL + "';");

                    //INSERTO EL MOVIMIENTO EN LA TABLA DE MOVIMIENTOS
                    if (ESTADORR != "" && ESTADORR != null)
                    {
                        ConsultaActualizar.AppendLine("EXEC dbo.sp_InsertarNuevo_Movimiento 'EQUIPO LIBERADO ESTADO RR','LIBERADO','ESPERANDO POR SER ALMACENADO',''"
                                        + ",'','LIBERACION','UBICACIONENTRADALIBERACION','" + this.user + "','" + SERIAL + "';");
                    }
                    if (DOCSAP != "" && DOCSAP != null)
                    {
                        ConsultaMovimiento.AppendLine("EXEC dbo.sp_InsertarNuevo_Movimiento 'EQUIPO LIBERADO ESTADO SAP','LIBERADO','ESPERANDO POR SER ALMACENADO',''"
                                        + ",'','LIBERACION','UBICACIONENTRADALIBERACION','" + this.user + "','" + SERIAL + "';");
                    }

                    if ((ESTADORR != "" && ESTADORR != null) && (DOCSAP != "" && DOCSAP != null))
                    {
                        ConsultaActualizar.AppendLine("UPDATE dbo.EquiposCLARO SET Estado = 'LIBERADO' WHERE Serial like '" + SERIAL + "';");
                    }
                    ContadorSave++;
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaActualizar.ToString()))
                {
                    service.DirectSQLNonQuery(ConsultaActualizar.ToString(), Local);
                    service.DirectSQLNonQuery(ConsultaMovimiento.ToString(), Local);
                    ConsultaActualizar = new StringBuilder();
                }

                this.estado_almacenamiento = true;
                //Muestro el mensaje de confirmación
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                ValidarLiberado();

                View.Dispatcher_Liberacion.Invoke(new System.Action(() =>
                {
                    View.Model.ListRecords.Rows.Clear();
                }), null);

                //CargarEquiposCuarentena();
                trOnSave.Dispose();
            }
            catch (Exception Ex)
            {
                Util.ShowError("Se presento un error al momento de guardar los registros. Error: " + Ex.Message);
            }
        }

        public void ValidarLiberado()
        {
            DataTable Resultado = null;
            string ConsultaActualizar = "";
            string ConsultaBuscarliberado = "";
            string serial = "";
            string doc_rr = "";
            string doc_sap = "";
            string estado_rr = "";

            ConsultaActualizar += " UPDATE dbo.EquiposClaro SET Estado = 'LIBERADO', ESTADO_LIBERACION = 'LIBERADO', FECHA_LIBERADO = GETDATE() WHERE UPPER(SERIAL) IN (";
            foreach (DataRow dr1 in RegistrosSave_Aux.Rows)
            {
                serial = dr1["SERIAL"].ToString().ToUpper() ;
                //ConsultaBuscarliberado = "SELECT RowID,serial,MAC,CODIGO_SAP,productoid,fecha_doc,fecha_doc_sap,ESTADO_RR FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + serial + "');";
                ConsultaBuscarliberado = "SELECT RowID, serial, fecha_doc, fecha_doc_sap, ESTADO_RR FROM dbo.EquiposCLARO WHERE UPPER(Serial) = '" + serial + "';";
                Resultado = service.DirectSQLQuery(ConsultaBuscarliberado, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count > 0)
                {
                    doc_rr = Resultado.Rows[0]["fecha_doc"].ToString();
                    doc_sap = Resultado.Rows[0]["fecha_doc_sap"].ToString();
                    estado_rr = Resultado.Rows[0]["ESTADO_RR"].ToString();

                    if (estado_rr != "" && estado_rr != null && doc_rr != "" && doc_rr != null && doc_sap != "" && doc_sap != null)
                    {
                        ConsultaActualizar= "'" + serial + "',";
                    }
                }
            }
            ConsultaActualizar += ");";
            ConsultaActualizar = ConsultaActualizar.Replace(",);", ");");
            service.DirectSQLNonQuery(ConsultaActualizar, Local);
            ConsultaBuscarliberado = "";
            ConsultaActualizar = "";
        }

        public void OnClearDetails(object sender, EventArgs e)
        {
            //Limpio los registros de la lista
            View.Model.ListRecordsRep.Clear();
            View.Model.ListRecordsRepDB.Clear();
            View.Model.ListRecordsSAP.Clear();
            SerialesIngresados.Rows.Clear();
            View.Model.List_Nocargue.Clear();
            View.Model.List_Cuarentena.Clear();
            //oculto los list de las validaciones
            //View.StackInFile.Visibility = Visibility.Collapsed;
            //View.StackInDB.Visibility = Visibility.Collapsed;
            //View.StackSAP.Visibility = Visibility.Collapsed;
        }

        public void OnKillProcess(object sender, EventArgs e)
        {

            //detengo procesos activos de excel para el cargue masivo
            Process[] proceso = Process.GetProcessesByName("EXCEL");

            if (proceso.Length > 0)
                proceso[0].Kill();
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Util.ShowMessage("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        public void CargarEquiposCuarentena()
        {
            try
            {
                if (userName == "cavendano" || userName == "admin")
                {
                    View.ExpCuarentena.Visibility = Visibility.Visible;
                    DataTable Resultado = service.DirectSQLQuery("EXEC sp_GetProcesos 'TOTALEQUIPOSCUARENTENA', '', ''", "", "dbo.EquiposCLARO", Local);
                    View.Model.List_Cuarentena = service.DirectSQLQuery("EXEC sp_GetProcesos 'EQUIPOSCUARENTENA', '', ''", "", "dbo.EquiposCLARO", Local);

                    View.TotalCuarentena.Text = Resultado.Rows[0]["Total"].ToString();

                }
            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }

        private void OnExportCuarentena(object sender, EventArgs e)
        {
            OnExportCargue_Excel(View.Model.List_Cuarentena);
        }

        private void OnExportCargue_Excel(DataTable List_Cuarentena)
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

                for (int Idx = 0; Idx < List_Cuarentena.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = List_Cuarentena.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < List_Cuarentena.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, List_Cuarentena.Columns.Count].Value =
                        List_Cuarentena.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + List_Cuarentena.Rows.Count + 1);
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

        private void OnExportNoLiberados(object sender, EventArgs e)
        {
            OnExportNoLiberados_Excel(View.Model.List_Nocargue);
        }

        private void OnExportNoLiberados_Excel(DataTable List_Nocargue)
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

                for (int Idx = 0; Idx < List_Nocargue.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = List_Nocargue.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < List_Nocargue.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, List_Nocargue.Columns.Count].Value =
                        List_Nocargue.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + List_Nocargue.Rows.Count + 1);
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