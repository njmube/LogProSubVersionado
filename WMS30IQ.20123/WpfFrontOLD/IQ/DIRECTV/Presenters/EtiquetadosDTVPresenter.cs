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
using System.Drawing.Printing;

namespace WpfFront.Presenters
{

    public interface IEtiquetadosDTVPresenter
    {
        IEtiquetadosDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EtiquetadosDTVPresenter : IEtiquetadosDTVPresenter
    {
        public IEtiquetadosDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 4; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;
        private String ipservidor;
        private String rutaEtiqueta;

        public EtiquetadosDTVPresenter(IUnityContainer container, IEtiquetadosDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EtiquetadosDTVModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);


            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            View.ImprimirEtiqueta_Individual += new EventHandler<EventArgs>(this.OnImprimirEtiqueta_Individual);
            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV 'UBICACIONESDESTINO', 'ETIQUETADO', 'CLARO'", "", "dbo.Ubicaciones", Local);
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            CargarDatosDetails();
            ListarDatos();
            View.Model.ListadoEtiquetas = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'ETIQUETASDIS', '', ''", "", "General.StickerDIRECTV", Local);

            #endregion
        }



        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            ListarDatos();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            //View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("SMART_CARD_ASIGNADA", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModuloDIRECTVC("ETIQUETADO", null);

            View.Model.ListRecords_1 = ds.Tables[0];

        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Falla Equipo

            //IList<MMaster> ListadoFallaEquipoEtiquetado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVCFALLAR" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Falla Equipo";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFallaEquipoEtiquetado);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("FALLA_EQUIPO_VERIF"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("FALLA_EQUIPO_VERIF", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Tecnico Diagnosticador

            //IList<MMaster> ListadonNiveles = service.GetMMaster(new MMaster { MetaType = new MType { Code = "NIVELDTVET" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Nivel";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, ListadonNiveles);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("NIVEL"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("NIVEL", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna SmartCard Asignada

            ////IList<MMaster> ListadoCodigoSAP = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TELMEXCOD" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.TextBox";
            //Columna.Header = "SmartCard Asignada";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(TextBox.MinWidthProperty, (double)100);
            //Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("SMART_CARD_ASIGNADA"));
            //Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            //Txt.SetValue(TextBox.IsTabStopProperty, true);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("SMART_CARD_ASIGNADA", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Nivel.

            IList<MMaster> ListadoNivel = service.GetMMaster(new MMaster { MetaType = new MType { Code = "NIVELMAT" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Nivel";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoNivel);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("NIVEL_MATERIAL"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("NIVEL_MATERIAL", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
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
            //ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ETIQUETADO'";

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
            //View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            ////Variables Auxiliares
            //String ConsultaSQL;

            ////Limpio los campos de la busqueda
            //View.BuscarEstibaRecibo.Text = "";
            ////View.BuscarPosicionRecibo.SelectedIndex = -1;

            ////Creo la consulta para buscar los registros
            //ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ETIQUETADO', NULL, NULL";

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
            //    ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEPOSICION','" + Registros.Row["Posicion"] + "','ETIQUETADO','ETIQUETADO','" + Registros.Row["UA"].ToString() + "'";

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

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado, SmartValidado, SmartValidadoAsig;
            String ConsultaBuscar = "", ConsultaValidarAsig ="";

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            String ConsultaValidar = "select SMART_SERIAL, SMART_ESTADOASIG, SMART_ESTADO from dbo.SmartCardEquiposDIRECTV where SMART_SERIAL = '" + View.GetSmartCard.Text + "'";
            SmartValidado = service.DirectSQLQuery(ConsultaValidar, "", "dbo.SmartCardEquiposDIRECTV", Local);

            if (SmartValidado.Rows.Count == 0)
            {
                Util.ShowError("La Smart Card no esta registrada.");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSmartCard.Text = "";
                View.GetSerial1.Focus();
                return;
            }

            if (SmartValidado.Rows[0]["SMART_ESTADO"].ToString() != "BUEN ESTADO")
            {
                Util.ShowError("La Smart Card " + View.GetSmartCard.Text + " no puede ser asignada. Esta en estado " + SmartValidado.Rows[0]["SMART_ESTADO"].ToString());
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSmartCard.Text = "";
                View.GetSerial1.Focus();
                return;
            }

            if (SmartValidado.Rows[0]["SMART_ESTADOASIG"].ToString() == "ASIGNADA")
            {
                Util.ShowError("La Smart Card " + View.GetSmartCard.Text + " ya se encuentra asignada.");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSmartCard.Text = "";
                View.GetSerial1.Focus();
                return;
            }

            //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            foreach (DataRow item in View.Model.ListRecords.Rows)
            {
                if (View.GetSerial1.Text == item["Serial"].ToString())
                {
                    Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSmartCard.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }

            

            foreach (DataRow item in View.Model.ListRecords.Rows)
            {
                ConsultaValidarAsig = "select SMART_SERIAL,SMART_ESTADOASIG from dbo.SmartCardEquiposDIRECTV where SMART_SERIAL = '" + item["SMART_CARD_ASIGNADA"].ToString() + "'";
                SmartValidadoAsig = service.DirectSQLQuery(ConsultaValidarAsig, "", "dbo.SmartCardEquiposDIRECTV", Local);

                if (View.GetSmartCard.Text == item["SMART_CARD_ASIGNADA"].ToString())
                {
                    Util.ShowError("La Smart Card " + View.GetSmartCard.Text + " ya se encuentra asignada.");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSmartCard.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }

            try
            {

                //Validacion existe o no el equipo en DB
                ConsultaBuscar = "SELECT * FROM dbo.EquiposDIRECTVC WHERE Serial = '" + View.GetSerial1.Text.ToString() + "' AND Estado != 'DESPACHADO'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

                if (Resultado.Rows.Count > 0)
                {
                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCAREQUIPOGENERAL','" + View.GetSerial1.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

                    //Evaluo si el serial existe
                    if (RegistroValidado.Rows.Count == 0)
                    {
                        Util.ShowError("El serial no existe o no esta en la ubicacion requerida.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSmartCard.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "P-ETIQUETADO")
                    {
                        Util.ShowError("El serial ingresado no esta en Etiquetado, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSmartCard.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Llamamos al metodo para imprimir la etiqueta
                        //PrinterControl.ImprimirEtiquetasDTV(RegistroValidado.Rows[0]["Modelo"].ToString(), RegistroValidado.Rows[0]["Mac"].ToString(), RegistroValidado.Rows[0]["Serial"].ToString());

                        //Asigno los campos
                        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                        //dr["Producto"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                        dr["Receiver"] = RegistroValidado.Rows[0]["Receiver"].ToString();
                        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();
                        dr["SMART_CARD_ASIGNADA"] = View.GetSmartCard.Text.ToUpper();
                        dr["NIVEL_MATERIAL"] = RegistroValidado.Rows[0]["NIVEL_MATERIAL"].ToString();

                        //Agrego el registro al listado
                        View.Model.ListRecords.Rows.Add(dr);

                        //Limpio los seriales para digitar nuevos datos
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSmartCard.Text = "";
                        View.GetSerial1.Focus();
                    }
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
                return;
            }
        }


        private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        {
            //Variables Auxiliares
            DataRow dr1;
            int NumeroSerial;

            foreach (DataRow dr in e.Value.Rows)
            {
                dr1 = View.Model.ListRecords.NewRow();

                //Asigno los campos
                /*dr1[0] = View.Model.ProductoSerial.ProductID;
                dr1[1] = View.Model.ProductoSerial.Name;*/

                NumeroSerial = 1;
                foreach (DataColumn dc in e.Value.Columns)
                {
                    switch (NumeroSerial.ToString())
                    {
                        case "1":
                            dr1[3] = dr[dc.ColumnName].ToString();
                            break;
                        case "2":
                            dr1[4] = dr[dc.ColumnName].ToString();
                            break;
                        case "3":
                            dr1[5] = dr[dc.ColumnName].ToString();
                            break;
                    }
                    NumeroSerial++;
                }

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr1);
            }
        }


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
            String ConsultaGuardar = "";
            String ConsultaGuardarTrack = "";
            Int32 ContadorFilas = 0;
            Int32 Cont1 = 0;
            List<DataRow> filasEliminar = new List<DataRow>();
              
            try
            {
                string aux = "";
                string aux1 = "";

                //Recorro el listado de equipos ingresados al listado para saber la smartCard no este ya ingresada
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    Cont1++;

                    if (Cont1 % 2 == 0)
                    {
                      aux = item["SMART_CARD_ASIGNADA"].ToString(); 
                    }
                    else{
                      aux1 = item["SMART_CARD_ASIGNADA"].ToString(); 
                    }

                    if(aux.Equals(aux1, StringComparison.Ordinal)){
                        Util.ShowError("No se permite asignar una smart card a dos registros");
                        return;
                    }                    
                }

                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;

                    if (String.IsNullOrEmpty(DataRow["SMART_CARD_ASIGNADA"].ToString()))
                    {
                        Util.ShowError("Por favor ingrese la SmartCard asignada");
                        return;
                    }
                    else
                    {
                        //Verificacamos que exista una smartCard
                        DataTable Resultado11 = service.DirectSQLQuery("declare @salida int execute PA_SMART_EXIST '" + DataRow["SMART_CARD_ASIGNADA"] + "',@salida output select @salida as columna", "", "dbo.SMART_EXIST", Local);

                        string auxiliar=Resultado11.Rows[0]["columna"].ToString();
                        
                        //Si devuelve 3 significa que la smartCard cumple con todos las caracteristicas para asignarse a un dispositivo
                        
                        bool areEqual = String.Equals(auxiliar, "3", StringComparison.Ordinal);

                        if (!areEqual)
                        {
                            Util.ShowError(this.getMensajeSmartCard(auxiliar, DataRow["SMART_CARD_ASIGNADA"].ToString()));
                        }
                        else
                        {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'VERIFICACION', Estado = 'P-VERIFICACION'";
                                ConsultaGuardar += ",NIVEL_FINAL = '" + DataRow["NIVEL_MATERIAL"] + "', SMART_CARD_ASIGNADA = '" + DataRow["SMART_CARD_ASIGNADA"] + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

                                //VALIDAR QUE SMART CARD NO ESTE DAÑADA NI ASIGNADADataRow["SMART_CARD_ASIGNADA"]
                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_ETIQUETADO = 'ETIQUETADO', FECHA_ETIQUETADO = '" + DateTime.Now.ToString("dd/MM/yyyy") + "' WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";

                                //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                                ConsultaGuardar += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'ETIQUETADO DE EQUIPO','ETIQUETADO','VERIFICACION','Sin pallet','" + DataRow["RowID"].ToString() + "','ETIQUETADO','UBICACIONPRODUCCION','" + this.user + "','';";
                                Console.WriteLine("###### " + ConsultaGuardar);

                                //Ejecuto la consulta
                                service.DirectSQLNonQuery(ConsultaGuardar, Local);
                                service.DirectSQLNonQuery(ConsultaGuardarTrack, Local);

                                //Limpio la consulta para volver a generar la nueva
                                ConsultaGuardar = "";
                                ConsultaGuardarTrack = "";
                            
                            filasEliminar.Add(DataRow);
                        } 
                    }
                }

                foreach(DataRow item in filasEliminar){
                    //Util.ShowMessage("Filas que se pueden eliminar "+item["RowID"].ToString());
                    LimpiarDatosIngresoSeriales(item);
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {
                    //Ejecuto la consulta
                    
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);
                    service.DirectSQLNonQuery(ConsultaGuardarTrack, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                    ConsultaGuardarTrack = "";
                }

                //LimpiarDatosIngresoSeriales(DataRow);

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            //String ConsultaSQL = "", NuevaUbicacion, NuevoEstado;

            ////Evaluo que haya sido seleccionado un registro
            //if (View.ListadoItems.SelectedItems.Count == 0)
            //    return;

            ////Evaluo que haya seleccionado la nueva clasificacion
            //if (View.Ubicacion.SelectedIndex == -1)
            //{
            //    Util.ShowError("Por favor seleccionar la nueva clasificacion.");
            //    return;
            //}

            ////Coloco la ubicacion
            //NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            ////Valido la ubicacion para colocar el estado
            //if (NuevaUbicacion != "VERIFICACION")
            //    NuevoEstado = "VERIFICACION";
            //else
            //    NuevoEstado = "VERIFICACION";

            //foreach (DataRowView item in View.ListadoItems.SelectedItems)
            //{
            //    //Creo la consulta para cambiar la ubicacion de la estiba
            //    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "' WHERE RowID = '" + item.Row["RowID"] + "'";

            //    //Ejecuto la consulta
            //    service.DirectSQLNonQuery(ConsultaSQL, Local);
            //}

            ////Muestro el mensaje de confirmacion
            //Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            //ListarDatos();

            ////Quito la selecion de la nueva ubicacion
            //View.Ubicacion.SelectedIndex = -1;

            ////Quito la seleccion del listado
            //View.ListadoItems.SelectedIndex = -1;

        }

        public void LimpiarDatosIngresoSeriales(DataRow item)
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Remove(item);
        }


        /*
         @param string valor - Recibe el valor obtenido del procedimiento almacenado [SMART_EXIST]
         * 0 SmartCard no registrada en el sistema
         * 1 SmardCard registrada pero asignada a otro item
         * 2 SmardCard dañada
         * 3 SmardCard asignada exitosamente
         */
        public String getMensajeSmartCard(string valor,string smartcard)
        {
            string auxiliar;
            switch(valor){
                case "1":
                   auxiliar = "La smart card "+smartcard+" esta registrada en el sistema, pero no esta disponible";
                break;
                case "2":
                auxiliar = "Smart card " + smartcard + " averiada";
                break;
                case "3":
                   auxiliar = "Smart card asignada exitosamente";
                break;
                default:
                   auxiliar = "La Smart card "+smartcard+" no se esta registrada en el sistema";
                break;
            }
            return auxiliar;
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

        private void KeyUp_CodigoAdicional(object sender, KeyEventArgs e)
        {
            //Retira el borde rojo del textbox de codigo adicional donde se lleno el campo
            (sender as TextBox).BorderBrush = Brushes.AliceBlue;
            //Util.ShowMessage("You Entered: " + e.Key);
        }

        private void OnImprimirEtiqueta_Individual(object sender, EventArgs e)
        {
            Boolean validacion = false;
            List<String> codAuxiliares = new List<String>();
            String serial = View.GetMacImpre_Individual.Text;
            String mac = View.GetSerialImpre_Individual.Text;
            Int32 cantidadImpresiones = 1;

            if (serial != "" || mac != "")
            {

                if (View.Get_StackCodigosAdicionales.Children.Count > 0)
                { //Si el stackpanel tiene controles adicionales para codigos aparte de serial y mac

                    // Se recorre todo el stackpanel en busca de Textbox, especificamente del valor que tengan estos
                    foreach (var child in View.Get_StackCodigosAdicionales.Children)
                    {
                        var textBox = child as TextBox;
                        if (textBox == null) continue;

                        if (textBox.Text == "")
                        {
                            textBox.BorderBrush = Brushes.Red;
                            textBox.KeyUp += new KeyEventHandler(KeyUp_CodigoAdicional);
                            validacion = true;
                        }
                        else
                        {
                            textBox.BorderBrush = Brushes.AliceBlue;
                            codAuxiliares.Add(textBox.Text);
                        }
                    }
                }

                if (validacion)
                {
                    Util.ShowError("Todos los campo deben estar completos.");
                    return;
                }
                else
                {
                    System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
                    pd.PrinterSettings = new PrinterSettings();
                    Int32.TryParse(View.GetCantidad_Impresiones.Text, out cantidadImpresiones);

                    //try {
                    if (System.Windows.Forms.DialogResult.OK == pd.ShowDialog())
                    {
                        PrinterControl.EtiquetadoEquipoIndividual(serial, mac, this.rutaEtiqueta, this.ipservidor, codAuxiliares, cantidadImpresiones, pd.PrinterSettings.PrinterName);
                    }
                    //}
                    // catch(Exception ex){
                    //     Util.ShowError("Error en la impresión, por favor verifique la conexión a la impresora Zebra "+ex.Message);
                    // }
                }
            }
            else
            {
                Util.ShowError("Todos los campo deben estar completos.");
            }
        }

        private void OnGetNumeroCodigos(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                String Stickid = ((DataRowView)(sender as ComboBox).SelectedItem).Row["stickCode"].ToString(); //Obtengo el rowid del diseño de etiqueta seleccionado
                DataTable aux = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'ETIQUETASDIS_BYID', '" + Stickid + "', ''", "", "General.StickerDIRECTVx|", Local); //Realizo una consulta que traiga la cantidad de codigos, ruta y ip servidor

                Int32 cantCod_adicionales = Int32.Parse(aux.Rows[0]["Cantidad"].ToString()); // Cantidad de codigos adicionales 
                this.rutaEtiqueta = aux.Rows[0]["Ruta"].ToString(); // Ruta del fichero en el servidor
                this.ipservidor = aux.Rows[0]["Servidor"].ToString(); // Dirección ip del servidor 

                if (aux.Rows.Count > 0) //si la consulta devuelve un valor realiza el proceso
                {
                    if (cantCod_adicionales == 0) //Si el diseño unicamente incluye el Serial y Mac no se crean componentes adicionales
                    {
                        View.Get_StackCodigosAdicionales.Visibility = Visibility.Collapsed; // El stackpanel de codigos adicionales se oculta
                        View.Get_StackCodigosAdicionales.Children.Clear(); // Se eliminan todos los controles graficos del stackpanel
                    }
                    else
                    {
                        /*Si por el contrario el diseño necesita codigos adicionales a los de serial y mac, se crean textblock y textbox 
                          dependiendo de la cantidad de codigos que necesite ese diseño (este valor se indica en la BD, tabla General.Stickets, Columna=stick_cantidadCodes)*/

                        View.Get_StackCodigosAdicionales.Children.Clear(); // Elimina todos los controles graficos del stackpanel
                        View.Get_StackCodigosAdicionales.Visibility = Visibility.Visible; //Stackpanel visible al usuario

                        for (int fila = 1; fila <= cantCod_adicionales; fila++)
                        { // Se creean los componenetes de codigos adicionales 

                            //Se obtiene acceso al stackpanel y se crea un nuevo hijo a este. Estara compuesto por Textblock's y Textbox's
                            View.Get_StackCodigosAdicionales.Children.Add(new TextBlock { Text = "Identificador adicional " + fila, Name = "txt_a" + fila, Width = 130, Margin = new Thickness(-70, 5, 0, 0), TextAlignment = TextAlignment.Left });
                            View.Get_StackCodigosAdicionales.Children.Add(new TextBox { Name = "tb_serialaux" + fila, Width = 182, Height = 20, Margin = new Thickness(0, 2, 20, 0) });
                        }
                    }
                }
                else
                {
                    Util.ShowError("Error al buscar diseño de etiqueta en el servidor");
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Error al buscar diseño de etiqueta en el servidor " + ex.Message);
            }
        }

        #endregion

    }
}