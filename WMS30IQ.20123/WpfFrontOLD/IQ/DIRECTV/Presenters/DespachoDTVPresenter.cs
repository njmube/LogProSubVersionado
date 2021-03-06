﻿using System;
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

namespace WpfFront.Presenters
{

    public interface IDespachoDTVPresenter
    {
        IDespachoDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class DespachoDTVPresenter : IDespachoDTVPresenter
    {
        public IDespachoDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        private string dateFormat = "dd/MM/yyyy";

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public DespachoDTVPresenter(IUnityContainer container, IDespachoDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DespachoDTVModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ImprimirHablador += new EventHandler<EventArgs>(this.OnImprimirHablador);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

            View.BuscarRegistrosDespachos += new EventHandler<EventArgs>(this.OnBuscarRegistroDespachos);
            View.MostrarEquiposDespacho += new EventHandler<EventArgs>(this.OnMostrarEquiposDespacho);
            View.FilaSeleccionada += this.OnFilaSeleccionada;
            View.ExportPalletSeleccion += this.OnExportPalletSeleccion;
            View.ListarEquiposSeleccion += new EventHandler<EventArgs>(this.OnListarEquiposSeleccion);

            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'DESPACHO', 'CLARO'", "", "dbo.Ubicaciones", Local);
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVPOSIC" } });

            View.Model.ListadoProductosDespacho = service.DirectSQLQuery("SELECT MODELO FROM dbo.Despacho_EquiposDIRECTVC WHERE ESTADO = 'DESPACHADO' GROUP BY MODELO", "", "dbo.Despacho_EquiposDIRECTVC", Local);

            CargarDatosDetails();
            //ListarDatos();


            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            string ConsultaSQL;
            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARUNIDAD_DESPACHO', 'DESPACHO', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            //ListarDatos();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("PROVEEDOR", typeof(String));
            View.Model.ListRecords.Columns.Add("SMART_CARD_ASIGNADA", typeof(String));

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

            #region Columna Fecha Despacho

            /*Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Fecha Despacho";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("FECHA_DESPACHO"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("FECHA_DESPACHO", typeof(String)); //Creacion de la columna en el DataTable
            */
            #endregion

            #region Columna Dealers

            IList<MMaster> ListadoDealersDespacho = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DEALER" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Dealer";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoDealersDespacho);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("DEALER"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("DEALER", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Ciudad

            IList<MMaster> ListadoCiudadDespacho = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD_DTV" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ciudad Despacho";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoCiudadDespacho);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("CIUDAD_DESPACHO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("CIUDAD_DESPACHO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Codigo Dealer

            IList<MMaster> ListadoCodigoDealersDespacho = service.GetMMaster(new MMaster { MetaType = new MType { Code = "COD_DEALER" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Codigo Dealer";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoCodigoDealersDespacho);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("CODIGO_DEALER_DESPACHO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("CODIGO_DEALER_DESPACHO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Empresa Transportadora

            IList<MMaster> ListadoEmpresaTransportadoraDespacho = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TRANSP" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Empresa Transportadora";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoEmpresaTransportadoraDespacho);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("EMPRESA_TRANSPORTADORA_DESPACHO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("EMPRESA_TRANSPORTADORA_DESPACHO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Estado

            IList<MMaster> ListadoEstadosDespacho = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADESPA" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Estado Despacho";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoEstadosDespacho);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_DESPACHO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTADO_DESPACHO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnBuscarRegistroDespachos(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosDespachos();
        }

        private void BuscarRegistrosDespachos()
        {
            //Variables Auxiliares
            string ConsultaSQL;
            string ProductoID, FechaDespacho;
            DateTime aux_fechaDespacho;
            string dateFormat = "dd/MM/yyyy";

            ProductoID = View.BuscarModeloDespacho.Text.ToString();
            FechaDespacho = View.GetFechaDespacho.ToString();

            if (!String.IsNullOrEmpty(FechaDespacho))
            {
                aux_fechaDespacho = DateTime.Parse(View.GetFechaDespacho.Text);
                FechaDespacho = aux_fechaDespacho.ToString(dateFormat); 
            }

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADESPACHADA','" + ProductoID + "','" + FechaDespacho + "'";

            //Ejecuto la consulta
            View.Model.ListadoDespachos = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Despacho_EquiposDIRECTVC", Local);

            LimpiarCamposDepacho();
        }

        protected void LimpiarCamposDepacho()
        {
            // Limpio los filtros
            View.GetFechaDespacho.Text = "";
            View.BuscarModeloDespacho.SelectedIndex = -1;
        }

        private void OnMostrarEquiposDespacho(object sender, EventArgs e)
        {
            // Evaluo que haya sido seleccionado un registro.
            if (View.ListadoBusquedaDespachos.SelectedIndex == -1)
                return;

            string aux_idpallet = ((DataRowView)View.ListadoBusquedaDespachos.SelectedItem).Row["Pallet"].ToString();
            
            string ConsultaSQL = 
              " SELECT CodigoEmpaque2 as Pallet, "
            + " Serial AS PSerial, "
            + " '' as PMac, "
            + " '' as SAP,  "
            + " MODELO as Modelo," 
            + " TIPO_ORIGEN as PTRecibo," 
            + " '' as PTOrigen,"
            + " '' as Remision,"
            + " CONVERT(VARCHAR,FECHA_DESPACHO,120) as PFDespacho"
            + " FROM dbo.Despacho_EquiposDIRECTVC "
            + " WHERE ((CodigoEmpaque2 IS NOT NULL) AND (ESTADO = 'DESPACHADO'))" +
              " AND CodigoEmpaque2 = '" + aux_idpallet + "'";

            // Consulto los seriales contenidos en esa estiba.
            View.Model.Listado_PalletSerial = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Despacho_EquiposDIRECTVC", Local);

            int total = 0;
            int num_estibas = View.ListadoBusquedaDespachos.SelectedItems.Count;
            foreach (DataRowView Registros in View.ListadoBusquedaDespachos.SelectedItems)
            {
                total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());
            }
            
            View.TotalSeriales.Text = total.ToString();
            View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        }

        private void OnFilaSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            int total = 0;
            int num_estibas = View.ListadoBusquedaDespachos.SelectedItems.Count;
            foreach (DataRowView Registros in View.ListadoBusquedaDespachos.SelectedItems)
            {
                total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());                                      
            }

            View.TotalSeriales.Text = total.ToString();
            View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        }

        private void OnExportPalletSeleccion(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {
                int filas_seleccion = View.ListadoBusquedaDespachos.SelectedItems.Count;

                if (filas_seleccion > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaEquiposDespacho.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaEquiposDespacho.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Orange);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoPalletSeriales.Items)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaEquiposDespacho.Columns.Count].Value =
                                Registros.Row.ItemArray;
                        cont++;
                    }

                    rng = ws.get_Range("A1", "H" + cont + 1);
                    rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    rng.Columns.AutoFit();

                    excel.Visible = true;
                    wb.Activate();
                }
                else
                {
                    Util.ShowMessage("Debe seleccionar uno o varios pallets para la generación del archivo");
                }
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Problema para crear el archivo Excel: " + ex.ToString());
            }
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

            //Variables Auxiliares
            //DataRow dr = View.Model.ListRecords.NewRow();
            //DataTable RegistroValidado;

            //Evaluo que haya sido digitado el serial para buscar
            //if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            //{
            //    Util.ShowError("El campo serial no puede ser vacio.");
            //    return;
            //}

            ////Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            //foreach (DataRow item in View.Model.ListRecords.Rows)
            //{
            //    if (View.GetSerial1.Text == item["Serial"].ToString())
            //    {
            //        Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
            //        return;
            //    }
            //}

            //try
            //{
            //    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
            //    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCAREQUIPO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

            //    //Evaluo si el serial existe
            //    if (RegistroValidado.Rows.Count == 0)
            //    {
            //        Util.ShowError("El serial no existe o no esta en la ubicacion requerida.");
            //        return;
            //    }
            //    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "DESPACHO")
            //    {
            //        Util.ShowError("El serial ingresado no esta en Etiquetado, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
            //        return;
            //    }
            //    else
            //    {
            //        //Asigno los campos
            //        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
            //        dr["Producto"] = RegistroValidado.Rows[0]["Modelo"].ToString();
            //        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
            //        dr["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString();
            //        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();
            //        dr["PROVEEDOR"] = RegistroValidado.Rows[0]["PROVEEDOR"].ToString();
            //        dr["SMART_CARD_ASIGNADA"] = RegistroValidado.Rows[0]["SMART_CARD_ASIGNADA"].ToString();

            //        //Agrego el registro al listado
            //        View.Model.ListRecords.Rows.Add(dr);

            //        //Limpio los seriales para digitar nuevos datos
            //        View.GetSerial1.Text = "";
            //        View.GetSerial2.Text = "";
            //        View.GetSerial1.Focus();
            //    }
            //}
            //catch (Exception Ex)
            //{
            //    Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
            //    return;
            //}
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
            Int32 ContadorFilas = 0;

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;

                    if (ContadorFilas % 50 != 0)
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTV SET Ubicacion = 'DESPACHO', Estado = 'DESPACHO'";
                        ConsultaGuardar += ", DEALER = '" + DataRow["DEALER"].ToString() + "', CIUDAD_DESPACHO = '" + DataRow["CIUDAD_DESPACHO"].ToString() + "', CODIGO_DEALER_DESPACHO = '" + DataRow["CODIGO_DEALER_DESPACHO"].ToString() + "', EMPRESA_TRANSPORTADORA_DESPACHO = '" + DataRow["EMPRESA_TRANSPORTADORA_DESPACHO"].ToString() + "', ESTADO_DESPACHO = '" + DataRow["ESTADO_DESPACHO"].ToString() + "'";
                        ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";
                    }
                    else
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTV SET Ubicacion = 'DESPACHO', Estado = 'DESPACHO'";
                        ConsultaGuardar += ", DEALER = '" + DataRow["DEALER"].ToString() + "', CIUDAD_DESPACHO = '" + DataRow["CIUDAD_DESPACHO"].ToString() + "', CODIGO_DEALER_DESPACHO = '" + DataRow["CODIGO_DEALER_DESPACHO"].ToString() + "', EMPRESA_TRANSPORTADORA_DESPACHO = '" + DataRow["EMPRESA_TRANSPORTADORA_DESPACHO"].ToString() + "', ESTADO_DESPACHO = '" + DataRow["ESTADO_DESPACHO"].ToString() + "'";
                        ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

                        //Ejecuto la consulta
                        service.DirectSQLNonQuery(ConsultaGuardar, Local);

                        //Limpio la consulta para volver a generar la nueva
                        ConsultaGuardar = "";
                    }
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
                LimpiarDatosIngresoSeriales();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, consultaDespacho = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedItems.Count == 0)
                return;

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            if (NuevaUbicacion == "DESPACHO")
                NuevoEstado = "DESPACHADO";
            else
                NuevoEstado = "DESPACHADO";

            foreach (DataRowView item in View.ListadoItems.SelectedItems)
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', FECHA_DESPACHO = '" + DateTime.Now.ToString("dd/MM/yyyy") + "' WHERE CodigoEmpaque2 = '" + item.Row["Estiba"] + "';";
                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'PALLET DESPACHADO','DESPACHADO','DESPACHADO','" + item.Row["Estiba"] + "','','DESPACHO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";
                
                consultaDespacho = "EXEC sp_SetDespachoEquiposDIRECTVC 'UPDATE_TABLEDESPACHO', 'DESPACHADO', '" + item.Row["Estiba"] + "';";

                //Ejecuto las consultas
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                service.DirectSQLNonQuery(consultaDespacho, Local);
            }
            Console.WriteLine(ConsultaSQL);
            Console.WriteLine(consultaDespacho);
            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            OnConfirmBasicData(sender, e);

            //Quito la selecion de la nueva ubicacion
            View.Ubicacion.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.ListadoItems.SelectedIndex = -1;

            View.Model.Listado_PalletSerialDespacho.Rows.Clear();
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        public void OnImprimirHablador(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String destino = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

            String NroSeriales = ((DataRowView)View.ListadoItems.SelectedItem).Row["Cantidad"].ToString();
            String pallet = ((DataRowView)View.ListadoItems.SelectedItem).Row["Estiba"].ToString();
            String modelo = ((DataRowView)View.ListadoItems.SelectedItem).Row["Modelo"].ToString();

            //Evaluo que haya seleccionado laexport plain text  nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }
            else
            {
                destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
            }

            //Creo la base de la consulta para traer los seriales respectivos
            ConsultaSQL = "select serial,MODELO,SMART_CARD_ENTRADA,Fecha_Ingreso from Pallets_EmpaqueDIRECTV pallet join EquiposDIRECTVC eqc on pallet.id_pallet = eqc.pila where pallet.codigo_pallet = '" + pallet + "'";

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            if (SerialesImprimir.Rows.Count == 0)
            {
                ConsultaSQL = "select serial,MODELO,SMART_CARD_ENTRADA,Fecha_Ingreso from EquiposDIRECTVC where idpallet LIKE '" + pallet + "' OR CodigoEmpaque2 LIKE '" + pallet + "' AND Estado LIKE 'DESPACHO'";

                //Ejecuto la consulta
                SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
            }

            //Imprimo los registros
            PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, "PALLET", pallet, destino, "DIRECTV", "ALMACENAMIENTO - DESPACHO", "", "DIRECTV");
        }

        private void OnListarEquiposSeleccion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedIndex == -1)
                return;

            string aux_idPallet = ((DataRowView)View.ListadoItems.SelectedItem).Row["Estiba"].ToString();

            String Consulta = "SELECT IdPallet as PPallet, "
                            + "serial as PSerial, "
                            + "RECEIVER as Receiver, "
                            + "SMART_CARD_ASIGNADA as SmartCardAsignada,  "
                            + "MODELO as Modelo "
                            + "from dbo.EquiposDIRECTVC WHERE ((IdPallet IS NOT NULL) AND (ESTADO = 'DESPACHO')) "
                            + " AND IdPallet = '" + aux_idPallet + "';";

            Console.WriteLine(Consulta);

            View.Model.Listado_PalletSerialDespacho =
            service.DirectSQLQuery(Consulta, "", "dbo.EquiposDIRECTVC", Local);

        }

        #endregion

    }
}