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
using System.Windows.Data;
using System.Diagnostics;
using System.Threading;
using System.Data.OleDb;

namespace WpfFront.Presenters
{

    public interface ISmartCardDTVPresenterP
    {
        ISmartCardDTVViewP View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class SmartCardDTVPresenterP : ISmartCardDTVPresenterP
    {
        public ISmartCardDTVViewP View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 1; //# columnas que no se debe replicar porque son fijas.

        private DataTable SerialesIngresados = new DataTable();
        private DataRow NoLoad_Row = null;
        private Timer t;
        private Timer t1;
        private Timer tsmart;
        private Thread updateLabelThread;
        private Boolean estado_cargue = false, busqueda_Repetidos = false, busqueda_smart = false;
        private OleDbConnection oledbcon;
        private OleDbDataAdapter adaptador;
        private Thread hilo_repetidos;

        public SmartCardDTVPresenterP(IUnityContainer container, ISmartCardDTVViewP view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<SmartCardDTVModelP>();

            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            //View.AddLineReciclaje += new EventHandler<EventArgs>(this.OnAddLineReciclaje);
            //view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.SaveDetailsReciclaje += new EventHandler<EventArgs>(this.OnSaveDetailsReciclaje);
            //View.ActualizarSmart += new EventHandler<EventArgs>(this.OnActualizarSmart);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;
            //View.FilaSeleccionada += this.OnFilaSeleccionada;
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            //View.ReplicateDetailsBy_ColumnRec += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_ColumnRec);
            View.ExportCargue += new EventHandler<EventArgs>(this.OnExportCargue);
            View.ExportCargueAsig += new EventHandler<EventArgs>(this.OnExportCargueAsig);
            View.LoadSmartAsig += new EventHandler<EventArgs>(this.OnLoadSmartAsig);

            view.CargaMasiva += new EventHandler<EventArgs>(this.OnCargaMasiva);
            view.KillProcess += new EventHandler<EventArgs>(this.OnKillProcess);
            View.AddLineAsignacion += new EventHandler<EventArgs>(this.OnAddLineAsignacion);

            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            DataTable table = new DataTable();
            table.Columns.Add("SmartEstadoAsig", typeof(string));
            table.Rows.Add("BUEN ESTADO");
            table.Rows.Add("AVERIADO");
            //View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'DESPACHO', 'CLARO'", "", "dbo.Ubicaciones", Local);
            //View.Model.ListUbicacionesDestino = service.DirectSQLQuery("select distinct smart_estado as SmartEstadoAsig from SmartCardEquiposDIRECTV", "", 
            //"dbo.SmartCardEquiposDIRECTV", Local);
            View.Model.ListUbicacionesDestino = table;
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            /*Andres Leonardo Arevalo - 10 de febrero 2015 - Inicializando combobox de la vista SmartCard*/
             //view.Model.ListUbicacionesDestino = new List<string>() { "Buen estado", "BR", "CT", "CM", "AD" };
            /*Andres Leonardo Arevalo - 10 de febrero 2015 - Inicializando combobox de la vista SmartCard*/

            View.Model.ListEstados = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOSMAR" } });

            CargarDatosDetails();
            //ListarDatos();

            #endregion
        }
        #region Metodos

        /*Carga los smartCard disponibles en la tabla principal*/
        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            if (View.GetFiltroEstado.Text == "" || View.GetFiltroEstado.Text == null)
            {
                ConsultaSQL = "select smart_serial,smart_estado,smart_fecha,smart_estadoasig,smart_fechaasig from SmartCardEquiposDIRECTV where smart_estadoasig = 'SIN ASIGNAR'";
                View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
                View.GetFiltroEstado.Text = "";
            }
            else
            {
                if (View.GetFiltroEstado.Text == "STOCK BULCK S02")
                {
                    ConsultaSQL = "select smart_serial,smart_estado,smart_fecha,smart_estadoasig,smart_fechaasig from SmartCardEquiposDIRECTV where smart_estadoasig = 'SIN ASIGNAR' and smart_estado = 'BUEN ESTADO'";
                    View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
                    View.GetFiltroEstado.Text = "";
                }
                else
                {
                    ConsultaSQL = "select smart_serial,smart_estado,smart_fecha,smart_estadoasig,smart_fechaasig from SmartCardEquiposDIRECTV where smart_estadoasig = 'SIN ASIGNAR' and smart_estado = '" + View.GetFiltroEstado.Text + "'";
                    View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
                    View.GetFiltroEstado.Text = "";
                }
            }
            

        }

        private void OnLoadSmartAsig(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;
            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARSMARTASIGNADA', '', '', ''";
            //ConsultaSQL = "select TOP 1000 smart_serial,smart_estado,smart_fecha,smart_estadoasig,smart_fechaasig from SmartCardEquiposDIRECTV where smart_estadoasig = 'ASIGNADA' ORDER BY smart_fechaasig";

            //Ejecuto la consulta
            View.Model.ListRecords_Asignada = service.DirectSQLQuery(ConsultaSQL, "", "dbo.SmartCardEquiposDIRECTV", Local);
            //View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL,"","dbo.SmartCardEquiposDIRECTV",Local);
            //ListarDatos();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListRecords");
            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("SmartCard", typeof(String));

            //Inicializo el DataTable
            View.Model.ListRecordsReciclaje = new DataTable("ListRecordsReciclaje");
            //Asigno las columnas
            View.Model.ListRecordsReciclaje.Columns.Add("SmartCard", typeof(String));
            View.Model.ListRecordsReciclaje.Columns.Add("Modelo", typeof(String));

            // Datatable lista de seriales no cargados
            View.Model.List_Nocargue = new DataTable("ListadoNoCargue");
            View.Model.List_Nocargue.Columns.Add("SmartCard", typeof(String));
            View.Model.List_Nocargue.Columns.Add("Motivo", typeof(String)); // Motivo del no cargue

            //Inicializo el DataTable
            View.Model.ListRecords_1 = new DataTable("ListRecords_1");
            //Asigno las columnas
            View.Model.ListRecords_1.Columns.Add("smart_serial", typeof(String));
            View.Model.ListRecords_1.Columns.Add("smart_estado", typeof(String));
            View.Model.ListRecords_1.Columns.Add("smart_fecha", typeof(String));
            View.Model.ListRecords_1.Columns.Add("smart_estadoasig", typeof(String));
            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {
            DataSet ds = Util.GetListaDatosModuloDIRECTVC("DESPACHO", null);

            View.Model.ListRecords_1 = ds.Tables[0];
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Estado Material.

            IList<MMaster> ListadoFalla = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOSMAR" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";

            Columna.Header = "Estado";

            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFalla);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_MATERIAL"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

             //add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;

            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTADO_MATERIAL", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Modelo

            IList<MMaster> ListadoModelo = service.GetMMaster(new MMaster { MetaType = new MType { Code = "SMARTMODEL" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";

            Columna.Header = "Modelo";

            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoModelo);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Modelo"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;

            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Modelo", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Origon Material.

            IList<MMaster> ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ORIGENSMAR" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";

            Columna.Header = "Origen";

            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoOrigen);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ORIGEN_SMARTCARD"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;

            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ORIGEN_SMARTCARD", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            //#region Columna Fecha Ingreso

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
            //#endregion

            //#region Columna Estado Material Reciclaje.

            //IList<MMaster> ListadoFallaReciclaje = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOSMAR" } });

            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";

            //Columna.Header = "Estado";

            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFallaReciclaje);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_MATERIAL"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);

            ////add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;

            //View.ListadoEquiposReciclaje.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecordsReciclaje.Columns.Add("ESTADO_MATERIAL", typeof(String)); //Creacion de la columna en el DataTable

            //#endregion
        }

        //Recibo
        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosRecibo();
        }

        public void BuscarRegistrosRecibo()
        {
            ////Variables Auxiliares
            //String ConsultaSQL;

            ////Creo la consulta para buscar los registros
            //ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADESPACHO', 'PARA DESPACHO'";

            ////Valido si fue digitado una estiba para buscar
            //if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
            //    ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            ////Valido si fue seleccionado ubicaciones para filtrar
            ////if (View.BuscarPosicionRecibo.SelectedIndex != -1)
            ////    ConsultaSQL += ",'" + ((MMaster)View.BuscarPosicionRecibo.SelectedItem).Code.ToString() + "'";
            ////else
            ////    ConsultaSQL += ",NULL";

            ////Ejecuto la consulta
            //View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTV", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            ////Variables Auxiliares
            //String ConsultaSQL;

            ////Limpio los campos de la busqueda
            //View.BuscarEstibaRecibo.Text = "";
            ////View.BuscarPosicionRecibo.SelectedIndex = -1;

            ////Creo la consulta para buscar los registros
            //ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADESPACHO', 'PARA DESPACHO', NULL, NULL";

            ////Ejecuto la consulta
            //View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnConfirmarRecibo(object sender, EventArgs e)
        {
            ////Variables Auxiliares
            //String ConsultaSQL;

            ////Evaluo que haya sido seleccionado un registro
            //if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
            //    return;

            ////Recorro el listado de registros seleccionados para confirmar el recibo
            //foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
            //{
            //    //Creo la consulta para confirmar el cambio de ubicacion de la estiba
            //    ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEPOSICIONDESPACHO','" + Registros.Row["Posicion"] + "','DESPACHO','DESPACHO','" + Registros.Row["UA"].ToString() + "'";

            //    //Ejecuto la consulta
            //    service.DirectSQLNonQuery(ConsultaSQL, Local);
            //}

            ////Muestro el mensaje de confirmacion
            //Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            ////Busco los registros para actualizar el listado
            //BuscarRegistrosRecibo();
        }


        private void OnAddLine(object sender, EventArgs e)
        {
            // Andres Leonardo Arevalo - 09 feb 2015
          
            //Variables Auxiliares

            if (View.GetSmartCard1.Text.Length < 12 || View.GetSmartCard1.Text.Length > 12)
            {
                Util.ShowError("La Smart Card debe contener 12 digitos");
                View.GetSmartCard1.Text = "";
                return;
            }

            DataRow dr = View.Model.ListRecords.NewRow();
            String ConsultaBuscar = "";
            String ConsultaBuscar1 = "";

            if (String.IsNullOrEmpty(View.GetSmartCard1.Text.ToString()))
            {
                Util.ShowError("El campo smart card no puede ser vacio.");
                return;
            }

            //Validacion existe o no el equipo en DB
            ConsultaBuscar = "SELECT SMART_CARD_ENTRADA FROM dbo.EquiposDIRECTVC WHERE SMART_CARD_ENTRADA = UPPER('" + View.GetSmartCard1.Text.ToString() + "')";
            ConsultaBuscar1 = "SELECT SMART_SERIAL FROM dbo.SmartCardEquiposDIRECTV WHERE SMART_SERIAL = UPPER('" + View.GetSmartCard1.Text.ToString() + "')";
            
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);
            DataTable Resultado1 = service.DirectSQLQuery(ConsultaBuscar1, "", "dbo.SmartCardEquiposDIRECTV", Local);

            if ((Resultado.Rows.Count == 0) && (Resultado1.Rows.Count == 0))
            {
                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (View.GetSmartCard1.Text == item["SmartCard"].ToString())
                    {
                        Util.ShowError("El serial " + View.GetSmartCard1.Text + " ya esta en el listado.");
                        View.GetSmartCard1.Text = "";
                        return;
                    }
                }

                //Asigno los campos
                dr["SmartCard"] = View.GetSmartCard1.Text.ToString();

                if (View.GetSmartCard1.Text.ToString().StartsWith("00000"))
                    dr["Modelo"] = "P1";

                if (View.GetSmartCard1.Text.ToString().StartsWith("00006") || View.GetSmartCard1.Text.ToString().StartsWith("00007") || View.GetSmartCard1.Text.ToString().StartsWith("00008") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00009") || View.GetSmartCard1.Text.ToString().StartsWith("00010") || View.GetSmartCard1.Text.ToString().StartsWith("00011")
                    )
                    dr["Modelo"] = "P2";

                if (View.GetSmartCard1.Text.ToString().StartsWith("00012") || View.GetSmartCard1.Text.ToString().StartsWith("00013") || View.GetSmartCard1.Text.ToString().StartsWith("00014")
                    )
                    dr["Modelo"] = "P3";

                if (View.GetSmartCard1.Text.ToString().StartsWith("00015") || View.GetSmartCard1.Text.ToString().StartsWith("00016") || View.GetSmartCard1.Text.ToString().StartsWith("00017") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00018") || View.GetSmartCard1.Text.ToString().StartsWith("00019") || View.GetSmartCard1.Text.ToString().StartsWith("00020") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00021") || View.GetSmartCard1.Text.ToString().StartsWith("00022") || View.GetSmartCard1.Text.ToString().StartsWith("00023") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00024") || View.GetSmartCard1.Text.ToString().StartsWith("00025") || View.GetSmartCard1.Text.ToString().StartsWith("00026") 
                    )
                    dr["Modelo"] = "P5";

                if (View.GetSmartCard1.Text.ToString().StartsWith("00065") || View.GetSmartCard1.Text.ToString().StartsWith("00066") || View.GetSmartCard1.Text.ToString().StartsWith("00067") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00068") || View.GetSmartCard1.Text.ToString().StartsWith("00069") || View.GetSmartCard1.Text.ToString().StartsWith("00070") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00071") || View.GetSmartCard1.Text.ToString().StartsWith("00072") || View.GetSmartCard1.Text.ToString().StartsWith("00073") ||
                    View.GetSmartCard1.Text.ToString().StartsWith("00074") || View.GetSmartCard1.Text.ToString().StartsWith("00075") || View.GetSmartCard1.Text.ToString().StartsWith("00076")
                    )
                    dr["Modelo"] = "P6";

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr);

                //Limpio los seriales para digitar nuevos datos
                View.GetSmartCard1.Text = "";
                View.GetSmartCard1.Focus();
            }
            else
            {
                Util.ShowError("La smart card " + View.GetSmartCard1.Text.ToString() + " ya se encuentra registrada.");
                View.GetSmartCard1.Text = "";
                return;
            }
        }

        //private void OnFilaSeleccionada(object sender, SelectionChangedEventArgs e)
        //{
        //    string auxiliar = "Buen estado";

        //    foreach(DataRowView rowView in e.AddedItems)
        //    {
        //        auxiliar = rowView.Row[1].ToString();
        //    }

        //    View.BuscarPosicionRecibo.Text = auxiliar;
        //}


        //private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        //{
        //    //Variables Auxiliares
        //    DataRow dr1;
        //    int NumeroSerial;

        //    foreach (DataRow dr in e.Value.Rows)
        //    {
        //        dr1 = View.Model.ListRecords.NewRow();

        //        //Asigno los campos
        //        /*dr1[0] = View.Model.ProductoSerial.ProductID;
        //        dr1[1] = View.Model.ProductoSerial.Name;*/

        //        NumeroSerial = 1;
        //        foreach (DataColumn dc in e.Value.Columns)
        //        {
        //            switch (NumeroSerial.ToString())
        //            {
        //                case "1":
        //                    dr1[3] = dr[dc.ColumnName].ToString();
        //                    break;
        //                case "2":
        //                    dr1[4] = dr[dc.ColumnName].ToString();
        //                    break;
        //                case "3":
        //                    dr1[5] = dr[dc.ColumnName].ToString();
        //                    break;

        //            }
        //            NumeroSerial++;
        //        }

        //        //Agrego el registro al listado
        //        View.Model.ListRecords.Rows.Add(dr1);
        //    }
        //}

        private void OnReplicateDetails(object sender, EventArgs e)
        {
            //Recorre la primera linea y con esa setea el valor de las demas lineas.
            for (int i = 1; i < View.Model.ListRecords.Rows.Count; i++)
                for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                    View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[0][z];
        }

        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "", Estado = "";
            Int32 ContadorFilas = 0;

            

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    if (DataRow["ESTADO_MATERIAL"].ToString() == "STOCK BULCK S02")
                    {
                        ConsultaGuardar += "INSERT INTO dbo.SmartCardEquiposDIRECTV(SMART_SERIAL,SMART_ESTADO,SMART_FECHA, ORIGEN_SMARTCARD, SMART_MODELO) VALUES(";
                        ConsultaGuardar = ConsultaGuardar + "'" + DataRow["SmartCard"].ToString() + "','BUEN ESTADO',CONVERT(nvarchar(100), GETDATE(), 120),'" + DataRow["ORIGEN_SMARTCARD"].ToString() + "', '" + DataRow["Modelo"].ToString() + "');";
                    }
                    else
                    {
                        ConsultaGuardar += "INSERT INTO dbo.SmartCardEquiposDIRECTV(SMART_SERIAL,SMART_ESTADO,SMART_FECHA, ORIGEN_SMARTCARD, SMART_MODELO) VALUES(";
                        ConsultaGuardar = ConsultaGuardar + "'" + DataRow["SmartCard"].ToString() + "','" + DataRow["ESTADO_MATERIAL"].ToString() + "',CONVERT(nvarchar(100), GETDATE(), 120),'" + DataRow["ORIGEN_SMARTCARD"].ToString() + "','" + DataRow["Modelo"].ToString() + "');";
                    }
                    
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

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                
                LimpiarDatosIngresoSeriales();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        /*Boton Actualizar*/
        //public void OnActualizarSmart(object sender, EventArgs e)
        //{
        //    String ConsultaSQL = "", NuevaUbicacion, NuevoEstado;

        //    //Evaluo que haya sido seleccionado un registro
        //    if (View.ListadoItems.SelectedItems.Count == 0)
        //        return;
          
        //    //Evaluo que haya seleccionado la nueva clasificacion
        //    if (View.SmartCardEstado.SelectedIndex == -1)
        //    {
        //        Util.ShowError("Por favor seleccionar la nueva clasificacion.");
        //        return;
        //    }

        //    //Coloco la ubicacion
        //    NuevoEstado = ((DataRowView)View.SmartCardEstado.SelectedItem).Row["SmartEstadoAsig"].ToString();
            
        //    foreach (DataRowView item in View.ListadoItems.SelectedItems)
        //    {
        //        //Creo la consulta para cambiar la ubicacion de la estiba
        //        ConsultaSQL += " UPDATE dbo.SmartCardEquiposDIRECTV SET SMART_ESTADO = '" + NuevoEstado + "', SMART_FECHA = '" + DateTime.Now.ToString() + "' WHERE SMART_SERIAL  = '" + item.Row["smart_serial"] + "'";
        //        //Ejecuto la consulta
        //        service.DirectSQLNonQuery(ConsultaSQL, Local);
        //    }

        //    //Muestro el mensaje de confirmacion
        //    Util.ShowMessage("Actualización satisfactoria.");

        //    OnConfirmBasicData(sender, e);

        //    //Quito la selecion del combobox actualizada
        //    View.SmartCardEstado.SelectedIndex = -1;

        //    //Quito la seleccion de la fila actualizada
        //    View.ListadoItems.SelectedIndex = -1;

        //    //Quito la seleccion del listado
        //    //View.UnidadAlmacenamiento.SelectedIndex = -1;
        //    //View.CodigoEmpaque.Text = "";
        //}

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
            View.Model.ListRecordsReciclaje.Rows.Clear();
        }

        //private void OnAddLineReciclaje(object sender, EventArgs e)
        //{
        //    DataRow dr = View.Model.ListRecordsReciclaje.NewRow();
        //    String ConsultaBuscar = "";
        //    String ConsultaBuscar1 = "";

        //    if (String.IsNullOrEmpty(View.GetSmartCardReciclaje.Text.ToString()))
        //    {
        //        Util.ShowError("El campo smart card no puede ser vacio.");
        //        return;
        //    }

        //    //Validacion existe o no el equipo en DB
        //    ConsultaBuscar = "SELECT TOP 1 SMART_CARD_ENTRADA as SmartCard FROM dbo.EquiposDIRECTVC WHERE SMART_CARD_ENTRADA = '" + View.GetSmartCardReciclaje.Text.ToString() + "'";
        //    ConsultaBuscar1 = "SELECT TOP 1 SMART_SERIAL,SMART_MODELO as Modelo FROM dbo.SmartCardEquiposDIRECTV WHERE SMART_SERIAL = '" + View.GetSmartCardReciclaje.Text.ToString() + "'";

        //    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);
        //    DataTable Resultado1 = service.DirectSQLQuery(ConsultaBuscar1, "", "dbo.SmartCardEquiposDIRECTV", Local);

        //    if ((Resultado1.Rows.Count > 0) || (Resultado.Rows.Count > 0))
        //    {
        //        //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
        //        foreach (DataRow item in View.Model.ListRecordsReciclaje.Rows)
        //        {
        //            if (View.GetSmartCardReciclaje.Text == item["SmartCard"].ToString())
        //            {
        //                Util.ShowError("El serial " + View.GetSmartCard1.Text + " ya esta en el listado.");
        //                return;
        //            }
        //        }

        //        //Asigno los campos
        //        dr["SmartCard"] = View.GetSmartCardReciclaje.Text.ToString();
        //        dr["Modelo"] = Resultado1.Rows[0]["Modelo"].ToString();

        //        //Agrego el registro al listado
        //        View.Model.ListRecordsReciclaje.Rows.Add(dr);

        //        //Limpio los seriales para digitar nuevos datos
        //        View.GetSmartCardReciclaje.Text = "";
        //        View.GetSmartCardReciclaje.Focus();
        //    }
        //    else
        //    {
        //        Util.ShowError("La smart card " + View.GetSmartCardReciclaje.Text.ToString() + " no se encuentra registrada.");
        //    }
            
        //}

        private void OnSaveDetailsReciclaje(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecordsReciclaje.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "";

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecordsReciclaje.Rows)
                {
                    if (DataRow["ESTADO_MATERIAL"].ToString() == "STOCK BULCK S02")
                    {
                        ConsultaGuardar += "update dbo.SmartCardEquiposDIRECTV set SMART_ESTADO = 'BUEN ESTADO', SMART_FECHA_RECICLAJE = GETDATE()"
                                        + " where SMART_SERIAL = '" + DataRow["SmartCard"].ToString() + "';";
                    }
                    else
                    {
                        ConsultaGuardar += "update dbo.SmartCardEquiposDIRECTV set SMART_ESTADO = '" + DataRow["ESTADO_MATERIAL"].ToString() 
                                    + "', SMART_FECHA_RECICLAJE = GETDATE()"
                                    + " where SMART_SERIAL = '" + DataRow["SmartCard"].ToString() + "';";
                    }
                    
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

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos

                LimpiarDatosIngresoSeriales();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
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

        private void OnExportCargue(object sender, EventArgs e)
        {
            OnExportCargue_Excel(View.Model.ListRecords_1);
        }

        private void OnExportCargue_Excel(DataTable ListRecords_1)
        {
            int cont = 0;
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

                for (int Idx = 0; Idx < ListRecords_1.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = ListRecords_1.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < ListRecords_1.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, ListRecords_1.Columns.Count].Value =
                        ListRecords_1.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + ListRecords_1.Rows.Count + 1);
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

        private void OnExportCargueAsig(object sender, EventArgs e)
        {
            OnExportCargue_ExcelAsig(View.Model.ListRecords_Asignada);
        }

        private void OnExportCargue_ExcelAsig(DataTable ListRecords_Asignada)
        {
            int cont = 0;
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

                for (int Idx = 0; Idx < ListRecords_Asignada.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = ListRecords_Asignada.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
                }

                for (int Idx = 0; Idx < ListRecords_Asignada.Rows.Count; Idx++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[Idx].Resize[1, ListRecords_Asignada.Columns.Count].Value =
                        ListRecords_Asignada.Rows[Idx].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + ListRecords_Asignada.Rows.Count + 1);
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

        //private void OnReplicateDetailsBy_ColumnRec(object sender, RoutedEventArgs e)
        //{
        //    //Obtiene una referencia del encabezado de la lista
        //    GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

        //    //Cuando se selecciona uno de los select dentro de la lista se ejecuta este metodo por eso se valida que sea el encabezado
        //    if (headerClicked != null)
        //    {
        //        //Obtenemos el indice del encabezado
        //        var index = View.ListadoEquiposReciclaje.Columns.IndexOf(headerClicked.Column);

        //        if (View.ListEquiposReciclaje.SelectedIndex != -1)
        //        {

        //            if (View.ListEquiposReciclaje.SelectedItems.Count > 1)// Se selecciona mas de una fila
        //            {
        //                DataRowView drv = (DataRowView)View.ListEquiposReciclaje.SelectedItem;
        //                String valueOfItem = drv[index].ToString();

        //                //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
        //                foreach (DataRowView dr in View.ListEquiposReciclaje.SelectedItems)
        //                {
        //                    dr[index] = valueOfItem;
        //                }
        //            }
        //            else
        //            {
        //                //Filtramos las columnas descartando las que no son para replicar
        //                if (index >= offset)
        //                {
        //                    for (int i = View.ListEquiposReciclaje.SelectedIndex; i < View.Model.ListRecordsReciclaje.Rows.Count; i++)
        //                        View.Model.ListRecordsReciclaje.Rows[i][index] = View.Model.ListRecordsReciclaje.Rows[View.ListEquiposReciclaje.SelectedIndex][index];
        //                }
        //            }
        //        }
        //    }
        //}

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
            View.Dispatcher_Cargue.Invoke(new System.Action(() =>
            {
                View.Model.ListRecords.Rows.Clear();
                View.Model.List_Nocargue.Rows.Clear();
            }), null);
            
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
                adaptador = new OleDbDataAdapter("select SMART_CARD, ESTADO, MODELO, ORIGEN from [Hoja1$]", oledbcon);

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
            tsmart = new Timer(new TimerCallback((o) =>
            {
                // Invocamos un método anónimo que cumpla con un delegado genérico
                (o as System.Windows.Controls.ProgressBar).Dispatcher.Invoke(new System.Action(() =>
                {
                    cont2++;

                    if (cont2 == 1)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart previamente ingresadas";
                    }
                    else if (cont2 == 2)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart previamente ingresadas.";
                    }
                    else if (cont2 == 3)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart previamente ingresadas..";
                    }
                    else if (cont2 == 4 || cont > 4)
                    {
                        View.GetEstado_Cargue.Text = "Validando Smart previamente ingresadas...";
                        cont2 = 0;
                    }

                    // Implementación del método anónimo
                    if (busqueda_smart == false)
                    {
                        if (num_seriales == 0) { num_seriales = 1; }
                        View.Progress_Cargue.Value = cont_repeat2 * 100D / num_seriales;
                    }
                    else
                    {
                        // Anulamos el ciclo del temporizador ya que el proceso de la barra de progreso ha acabado
                        tsmart.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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
                            //t1.Dispose();
                        }));
                    }
                }));
            }), View.Progress_Cargue, 0, 1000);
        }

        private void SetRepeat(object o)
        {
            busqueda_Repetidos = false;
            busqueda_smart = false;
            estado_cargue = false;

            List<String> listNoCargue = new List<String>(); //Guarda los seriales que no cumplen con los requisitos del cargue

            for (int i = 0; i < SerialesIngresados.Rows.Count; i++)
            {
                for (int j = i + 1; j < SerialesIngresados.Rows.Count; j++)
                {
                    if (SerialesIngresados.Rows[i]["SMART_CARD"].ToString() == "" & SerialesIngresados.Rows[j]["SMART_CARD"].ToString() == "")
                    {
                        SerialesIngresados.Rows.RemoveAt(i); //Elimino el primer serial y dejo el repetido o posterior.
                        //j = i + 1; //Como borre el elemento de la posicion i provoco que j continue un serial posterior al borrado
                        if (j > 0)
                            j = i - 1;
                    }
                    //if (SerialesIngresados.Rows[i]["SERIAL"].ToString() != "" && SerialesIngresados.Rows[j]["SERIAL"].ToString() != "")
                    else
                    {
                        if (SerialesIngresados.Rows[i]["SMART_CARD"].ToString() == SerialesIngresados.Rows[j]["SMART_CARD"].ToString())
                        {
                            listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[j]["SMART_CARD"].ToString(), 
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
                ConsultaBuscarSerial = "SELECT SMART_SERIAL from dbo.SmartCardEquiposDIRECTV WHERE UPPER(SMART_SERIAL) = UPPER('" + SerialesIngresados.Rows[temp2]["SMART_CARD"].ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscarSerial, "", "dbo.SmartCardEquiposDIRECTV", Local);

                if (Resultado.Rows.Count > 0)
                {
                    listNoCargue.InsertRange(listNoCargue.Count, new string[] {SerialesIngresados.Rows[temp2]["SMART_CARD"].ToString(), 
                                                                                        "Smart Card existente en el sistema."});

                    SerialesIngresados.Rows.RemoveAt(temp2);
                }
                else
                {
                    temp2++;
                }
                cont_repeat2++;
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
                    RegistroGuardar["SmartCard"] = dr[0].ToString();
                    RegistroGuardar["ESTADO_MATERIAL"] = dr[1].ToString();
                    RegistroGuardar["Modelo"] = dr[2].ToString();
                    RegistroGuardar["ORIGEN_SMARTCARD"] = dr[3].ToString();

                    View.Model.ListRecords.Rows.Add(RegistroGuardar);

                }), null);
            }

            adaptador.Dispose();
        }

        private void OnAddLineAsignacion(object sender, EventArgs e)
        {
            DataRow dr = View.Model.ListRecords_1.NewRow();
            String ConsultaBuscar = "";
            String ConsultaBuscar1 = "";

            if (String.IsNullOrEmpty(View.GetSmartCardAsignacion.Text.ToString()))
            {
                Util.ShowError("El campo smart card no puede ser vacio.");
                return;
            }

            //Validacion existe o no el equipo en DB
            ConsultaBuscar = "SELECT TOP 1 SMART_CARD_ENTRADA as SmartCard FROM dbo.EquiposDIRECTVC WHERE SMART_CARD_ENTRADA = '" + View.GetSmartCardAsignacion.Text.ToString() + "'";
            ConsultaBuscar1 = "SELECT TOP 1 SMART_SERIAL,SMART_MODELO, SMART_ESTADO, SMART_FECHA, SMART_ESTADOASIG FROM dbo.SmartCardEquiposDIRECTV WHERE SMART_SERIAL = '" + View.GetSmartCardAsignacion.Text.ToString() + "'";

            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);
            DataTable Resultado1 = service.DirectSQLQuery(ConsultaBuscar1, "", "dbo.SmartCardEquiposDIRECTV", Local);

            if ((Resultado1.Rows.Count > 0) || (Resultado.Rows.Count > 0))
            {
                View.Model.ListRecords_1.Rows.Clear();

                //Asigno los campos
                dr["smart_serial"] = Resultado1.Rows[0]["SMART_SERIAL"].ToString();
                dr["smart_estado"] = Resultado1.Rows[0]["SMART_ESTADO"].ToString();
                dr["smart_fecha"] = Resultado1.Rows[0]["SMART_FECHA"].ToString();
                dr["smart_estadoasig"] = Resultado1.Rows[0]["SMART_ESTADOASIG"].ToString();

                //Agrego el registro al listado
                View.Model.ListRecords_1.Rows.Add(dr);

                //Limpio los seriales para digitar nuevos datos
                View.GetSmartCardAsignacion.Text = "";
                View.GetSmartCardAsignacion.Focus();
            }
            else
            {
                Util.ShowError("La smart card " + View.GetSmartCardAsignacion.Text.ToString() + " no se encuentra registrada.");
                //Limpio los seriales para digitar nuevos datos
                View.GetSmartCardAsignacion.Text = "";
                View.GetSmartCardAsignacion.Focus();
            }

        }

        #endregion
    }
}