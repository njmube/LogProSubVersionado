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
using System.Drawing.Printing;

namespace WpfFront.Presenters
{

    public interface IEmpaqueDTVPresenter
    {
        IEmpaqueDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EmpaqueDTVPresenter : IEmpaqueDTVPresenter
    {
        public IEmpaqueDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private int contFilas_byPallet = 0; // Almacena el conteo de los equipos que tiene un pallet seleccionado en la lista de busqueda
        private String codigoPallet = ""; // Guarda el codigo pallet cuando se selecciona una fila de la lista de pallets o cuando se genera un nuevo codigo de pallet
        private String ubicacionPallet = ""; // Guarda la ubicacion del pallet seleccionado de la lista de pallets
        private Boolean seleccionUbicacion = false; // Controla el dato de la ubicacion, si el usuario selecciona nueva pallet (False) se toma del combobox ubicacion, si selecciona una fila/pallet se captura de ahi (True)
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public EmpaqueDTVPresenter(IUnityContainer container, IEmpaqueDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EmpaqueDTVModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ConfirmarMovimientoProcesamiento += new EventHandler<EventArgs>(this.OnConfirmarMovimientoProcesamiento);

            //View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);
            View.ConfirmarImpresionHablador += new EventHandler<EventArgs>(this.OnConfirmarImpresionHablador);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;

            View.GenerarPallet += new EventHandler<EventArgs>(this.OnGenerarPallet);
            View.CrearNuevoPallet += new EventHandler<EventArgs>(this.OnCrearNuevoPallet);
            View.EnterConsultarPallet += new EventHandler<KeyEventArgs>(this.OnEnterConsultarPallet);
            View.KeyConsultarPallet += new EventHandler<KeyEventArgs>(this.OnKeyConsultarPallet);
            View.SeleccionPallet_Consulta += new EventHandler<EventArgs>(this.OnSeleccionPallet_Consulta);
            View.CerrarPallet += new EventHandler<EventArgs>(this.OnCerrarPallet);
            View.AbrirPallet += new EventHandler<EventArgs>(this.OnAbrirPallet);
            View.EliminarPallet += new EventHandler<EventArgs>(this.OnEliminarPallet);
            View.CrearNuevaCaja += new EventHandler<EventArgs>(this.OnCrearNuevaCaja);
            View.SeleccionCaja_Consulta += new EventHandler<EventArgs>(this.OnSeleccionCaja_Consulta);
            View.CerrarCaja += new EventHandler<EventArgs>(this.OnCerrarCaja);
            View.EliminarCaja += new EventHandler<EventArgs>(this.OnEliminarCaja);
            View.AbrirCaja += new EventHandler<EventArgs>(this.OnAbrirCaja);
            View.DesempacarEquipos += new EventHandler<EventArgs>(this.OnDesempacarEquipos);
            View.ImprimirEtiqueta += new EventHandler<EventArgs>(this.OnImprimirEtiqueta);

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'EMPAQUE', 'CLARO'", "", "dbo.Ubicaciones", Local);

            ConsultarPallets();

            CargarDatosDetails();
            // ListarDatos();

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARUNIDADALMACENAMIENTO', 'EMPAQUE', '" + this.user + "', NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
            ConsultarPallets();
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


            //View.Model.ListSeriales_Empaque = new DataTable("ListadoSerialesEmpaque");
            ////Asigno las columnas
            //View.Model.ListSeriales_Empaque.Columns.Add("Serial", typeof(String));
            //View.Model.ListSeriales_Empaque.Columns.Add("SMART_CARD_ASIGNADA", typeof(String));
            //View.Model.ListSeriales_Empaque.Columns.Add("Serial", typeof(String));
            //View.Model.ListSeriales_Empaque.Columns.Add("Estado", typeof(String));
            //View.Model.ListSeriales_Empaque.Columns.Add("RowID", typeof(String));
            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModuloDIRECTVC("EMPAQUE", null);

            View.Model.ListRecords = ds.Tables[0];

        }

        private void RefrescarDatos()
        {

            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAEMPAQUE', 'P-EMPAQUE'";

            //Ejecuto la consulta
            View.Model.ListRecords = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        


        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListSeriales_Empaque.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            foreach (DataRow item in View.Model.ListSeriales_Empaque.Rows)
            {
                if (View.GetSerial1.Text == item["Serial"].ToString())
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
                        View.GetSerial1.Focus();
                        return;
                    }
                    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "P-EMPAQUE")
                    {
                        Util.ShowError("El serial ingresado no esta en EMPAQUE, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Asigno los campos           
                        
                        //dr["Producto"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                        dr["SMART_CARD_ASIGNADA"] = RegistroValidado.Rows[0]["SMART_CARD_ASIGNADA"].ToString();
                        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();
                        dr["Modelo"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();

                        //Agrego el registro al listado
                        View.Model.ListSeriales_Empaque.Rows.Add(dr);

                        //Limpio los seriales para digitar nuevos datos
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
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
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTV SET Ubicacion = 'EMPAQUE', Estado = 'EMPAQUE'";
                        ConsultaGuardar += ", OBSERVACIONES_EmpaqueDTV = '" + DataRow["Observaciones_EmpaqueDTV"].ToString() + "'";
                        ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";
                    }
                    else
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTV SET Ubicacion = 'EMPAQUE', Estado = 'EMPAQUE'";
                        ConsultaGuardar += ", OBSERVACIONES_EmpaqueDTV = '" + DataRow["Observaciones_EmpaqueDTV"].ToString() + "'";
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

            //Variables Auxiliares
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, ConsultaTrack = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedIndex == -1)
                return;

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            //Valido la ubicacion para colocar el estado
            if (NuevaUbicacion != "ALMACENAMIENTO")
                NuevoEstado = "PARA DESPACHO";
            else
                NuevoEstado = "PARA ALMACENAMIENTO";

            //Recorro el listado de registros seleccionados para confirmar el recibo
            foreach (DataRowView Registros in View.ListadoItems.SelectedItems)
            {
                String aux_idPallet = Registros["idPallet"].ToString();

                //Creo la consulta para confirmar el cambio de ubicacion de la estiba
                ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEEMPAQUE','" + Registros["Pallet"].ToString() + "','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + aux_idPallet + "';";
                //ConsultaTrack += "UPDATE dbo.TrackEquiposCLARO SET CODEMPAQUE_EMPAQ_ENTR = '" + Registros["Pallet"].ToString() + "' WHERE CODEMPAQUE_EMPAQ_PROC = '" + aux_idPallet + "';";

                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE ENTREGA','EMPAQUE','ALMACENAMIENTO','" + Registros["Pallet"].ToString() + "','" + aux_idPallet + "','EMPAQUE','UBICACIONPRODUCCION_EMPAQUE','" + this.user + "','';";

                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                service.DirectSQLNonQuery(ConsultaTrack, Local);

                ConsultaSQL = "";
                ConsultaTrack = "";
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //Busco los registros para actualizar el listado
            //BuscarRegistrosEntrega();
            OnConfirmBasicData(sender, e);
            OnSeleccionPallet_Consulta(sender, e);
            this.Actualizar_UbicacionDisponible();

            //View.UnidadAlmacenamiento.SelectedIndex = -1;
            // View.CodigoEmpaque.Text = "";
            View.Ubicacion.SelectedIndex = -1;

        }

        public void OnConfirmarMovimientoProcesamiento(object sender, EventArgs e)
        {
            String ConsultaSQL = "";
            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            String aux_idCaja = ((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["idCaja"].ToString();

            if (View.ListadoSerialesBusqueda.Items.Count == 0)
            {
                Util.ShowMessage("No se encontraron equipos para empacar");
                return;
            }

            foreach (DataRowView item in View.ListadoSerialesBusqueda.Items)
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Estado = 'PARA ENTREGA_EMPAQUE',CodigoEmpaque = '" + aux_idPallet + "', PILA = '" + aux_idPallet + "', NROCAJA = '" + aux_idCaja + "' WHERE Rowid = '" + item.Row["RowID"].ToString() + "'; ";
                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE REALIZADO','EMPAQUE','EMPAQUE - ESPERANDO CIERRE DE CAJA','" + pallet + "','" + item.Row["RowID"].ToString() + "','EMPAQUE','UBICACIONPRODUCCION','" + this.user + "','';";
            }

            try
            {
                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //service.DirectSQLNonQuery(ConsultaTrack, Local);
                Util.ShowMessage("Los equipos se guardaron correctamente.");
            }
            catch (Exception ex)
            {
                Util.ShowError("Error guardando los equipos.");
            }

            OnSeleccionPallet_Consulta(sender, e);
            this.Actualizar_UbicacionDisponible();
            View.Model.ListSeriales_Empaque.Rows.Clear();
            View.StackSeriales.IsEnabled = false;

        }

        private void OnDesempacarEquipos(object sender, EventArgs e)
        {
            String ConsultaSQL = "";
            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            String aux_idCaja = ((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["idCaja"].ToString();

            if (View.ListadoSerialesBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar los equipos a desempacar");
                return;
            }

            foreach (DataRowView item in View.ListadoSerialesBusqueda.SelectedItems)
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Estado = 'P-EMPAQUE',CodigoEmpaque = '" + aux_idPallet + "', PILA = '" + aux_idPallet + "', NROCAJA = NULL WHERE Rowid = '" + item.Row["RowID"].ToString() + "'; ";
                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EQUIPO DESEMPACADO','EMPAQUE','EMPAQUE - ESPERANDO POR SER EMPACADO','" + pallet + "','" + item.Row["RowID"].ToString() + "','EMPAQUE','UBICACIONPRODUCCION','" + this.user + "','';";
                //ConsultaTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_EMPAQUE = 'EMPACADO', FECHA_EMPACADO = getdate(), CODEMPAQUE_EMPAQ_PROC = '" + View.CodigoEmpaqueProcesamiento.Text.ToString() + "' WHERE ID_SERIAL = '" + item.Row["RowID"].ToString() + "'";
            }

            try
            {
                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //service.DirectSQLNonQuery(ConsultaTrack, Local);
                Util.ShowMessage("Los equipos se desempacaron correctamente.");
            }
            catch (Exception ex)
            {
                Util.ShowError("Error desempacando los equipos.");
            }

            OnSeleccionPallet_Consulta(sender, e);
            this.Actualizar_UbicacionDisponible();
            View.Model.ListSeriales_Empaque.Rows.Clear();
            View.StackSeriales.IsEnabled = false;
        }

        //private void OnConfirmarImpresion(object sender, EventArgs e)
        //{
        //    Util.ShowMessage("Imprimiendo...");
        //    //Variables Auxiliares
        //    String ConsultaSQL1 = "";
        //    DataTable SerialesImprimir;

        //    //Evaluo que haya sido seleccionado un registro
        //    if (View.ListadoSeriales.SelectedIndex == -1)
        //    {
        //        Util.ShowMessage("Por favor seleccione un item de la lista");
        //        return;
        //    }
        //    //Creo la base de la consulta para traer los seriales respectivos
        //    ConsultaSQL1 = "SELECT idPallet,Posicion,modelo,receiver,serial,Estado,Fecha_Ingreso FROM dbo.EquiposDIRECTVC WHERE serial IN (''";

        //    //Recorro el listado de registros seleccionados para obtener los seriales e imprimirlos
        //    foreach (DataRowView Registros in View.ListadoSeriales.SelectedItems)
        //    {
        //        //Creo la consulta para cambiar la ubicacion de la estiba
        //        ConsultaSQL1 += ",'" + Registros.Row["Serial"] + "'";
        //    }

        //    //Completo la consulta
        //    ConsultaSQL1 += ")";

        //    //Elimino la basura en la cadena
        //    ConsultaSQL1 = ConsultaSQL1.Replace("'',", "");

        //    //Ejecuto la consulta
        //    SerialesImprimir = service.DirectSQLQuery(ConsultaSQL1, "", "dbo.EquiposDIRECTVC", Local);

        //    String unidad_almacenamiento = ((ComboBoxItem)View.UnidadAlmacenamientoProcesamiento.SelectedItem).Content.ToString();
        //    String codigoEmp = View.CodigoEmpaqueDTVProcesamiento.Text.ToString();

        //    //Imprimo los registros
            
        //    PrinterControl.PrintMovimientosBodega(SerialesImprimir, unidad_almacenamiento, codigoEmp, "EMPAQUE", "DTV", "EMPAQUE PROCESAMIENTO", "", "");
        //}

        private void OnConfirmarImpresionHablador(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String unidad_almacenamiento = "";
            String destino = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

            String transito = "EMPAQUE - ALMACENAMIENTO";
            String estado = ((DataRowView)View.ListadoItems.SelectedItem).Row["Estado"].ToString();
            String NroCajas = ((DataRowView)View.ListadoItems.SelectedItem).Row["Nro_Cajas"].ToString();
            String NroSeriales = ((DataRowView)View.ListadoItems.SelectedItem).Row["Nro_Seriales"].ToString();
            String fechaIngreso = ((DataRowView)View.ListadoItems.SelectedItem).Row["FechaIngreso"].ToString();
            String pallet = ((DataRowView)View.ListadoItems.SelectedItem).Row["Pallet"].ToString();
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
            ConsultaSQL = "select serial,MODELO,SMART_CARD_ENTRADA from Pallets_EmpaqueDIRECTV pallet join EquiposDIRECTVC eqc on pallet.id_pallet = eqc.Pila where pallet.codigo_pallet = '" + pallet + "'";

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            //Imprimo los registros
            PrinterControl.PrintMovimientosEmpaqueDIRECTV(SerialesImprimir, transito, estado, NroCajas, NroSeriales, fechaIngreso, pallet, modelo);
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

            #region Columna Observaciones EmpaqueDTV

            /*Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Observaciones EmpaqueDTV";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Observaciones_EmpaqueDTV"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Observaciones_EmpaqueDTV", typeof(String));*/ //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosEntrega();
        }

        public void BuscarRegistrosEntrega()
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTV 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ENTREGA_EMPAQUE'";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnGenerarPallet(object sender, EventArgs e)
        {
            String ConsultaBuscar = "";
            String ConsultaValidar = "";
            try
            {

                ConsultaBuscar = "select concat('DTV-',CONVERT(NVARCHAR, getdate(), 12),cast(NEXT VALUE FOR dbo.PalletSecuence as varchar)) as idpallet";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.PalletSecuence", Local);

                //ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                //DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count > 0)
                {
                    ConsultaValidar = "select RowId from dbo.EquiposDIRECTVC where IDPALLET = '" + Resultado.Rows[0]["idpallet"].ToString()
                                                                             + "' or CodigoEmpaque = '" + Resultado.Rows[0]["idpallet"].ToString()
                                                                             + "' or CodigoEmpaque2 = '" + Resultado.Rows[0]["idpallet"].ToString() + "';";
                    Console.WriteLine(ConsultaValidar);
                    DataTable RegistroValidado = service.DirectSQLQuery(ConsultaValidar, "", "dbo.EquiposDIRECTVC", Local);

                    if (RegistroValidado.Rows.Count > 0)
                    {
                        Util.ShowError("El codigo de pallet ya se encuentra registrado. Por favor genere uno nuevo!");
                    }
                    else
                    {
                        View.GetCodPallet.Text = Resultado.Rows[0]["idpallet"].ToString();
                    }

                    ConsultaBuscar = "";
                    ConsultaValidar = "";
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Se presento un error generando el pallet: " + ex.Message);
            }
            ////DataRow dr = View.Model.ListRecords.NewRow();
            //DataTable RegistroValidado;
            //String ConsultaBuscar = "";

            //try
            //{
            //    //Si el contador de equipos por pallet es igual a la lista de almacenamiento, quiere decir que no se agregaron nuevos equipos y se puede crear el nuevo pallet sin problema
            //    if (View.Model.ListPallets_Empaque.Rows.Count == this.contFilas_byPallet)
            //    {

            //        View.Model.ListPallets_Empaque.Clear();

            //        //Creo el número de pallet aleatorio 
            //        ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            //        DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            //        this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

            //        if (Resultado.Rows.Count > 0)
            //        {
            //            //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
            //            RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposDIRECTVC WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposDIRECTVC", Local);

            //            //Evaluo si el serial existe
            //            if (RegistroValidado.Rows.Count > 0)
            //            {
            //                Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
            //            }
            //            else
            //            {
            //                //Asigno los campos
            //                View.GetCodPallet.Text = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();
            //                this.codigoPallet = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();

            //                this.seleccionUbicacion = false;
            //                ////Limpio los seriales para digitar nuevos datos
            //                //View.GetSerial1.Text = "";
            //                //View.GetSerial2.Text = "";
            //                //View.GetSerial1.Focus();
            //            }
            //        }
            //        else
            //        {
            //            Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            //        }
            //    }
            //    else if (this.codigoPallet == "")
            //    {
            //        //Creo el número de pallet aleatorio 
            //        ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            //        DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            //        this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

            //        if (Resultado.Rows.Count > 0)
            //        {
            //            //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
            //            RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposDIRECTVC WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposDIRECTVC", Local);

            //            //Evaluo si el serial existe
            //            if (RegistroValidado.Rows.Count > 0)
            //            {
            //                Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
            //                return;
            //            }
            //            else
            //            {
            //                //Asigno los campos
            //                View.GetCodPallet.Text = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();
            //                this.codigoPallet = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();

            //                this.seleccionUbicacion = false;
            //                ////Limpio los seriales para digitar nuevos datos
            //                //View.GetSerial1.Text = "";
            //                //View.GetSerial2.Text = "";
            //                //View.GetSerial1.Focus();
            //            }
            //        }
            //        else
            //        {
            //            Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            //        }
            //    }
            //    else
            //    {
            //        // Si no son iguales quiere decir que se agregaron nuevos equipos al pallet
            //        ////Util.ShowMessage("Esta seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan");
            //        //Console.WriteLine("Se borraran los seriales no guardados");

            //        if (!UtilWindow.ConfirmOK("Esta seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan?") == true)
            //            return;
            //        View.Model.ListRecords.Clear();

            //        //Creo el número de pallet aleatorio 
            //        ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            //        DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            //        this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

            //        if (Resultado.Rows.Count > 0)
            //        {
            //            //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
            //            RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposDIRECTVC WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposDIRECTVC", Local);

            //            //Evaluo si el serial existe
            //            if (RegistroValidado.Rows.Count > 0)
            //            {
            //                Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
            //            }
            //            else
            //            {
            //                //Asigno los campos
            //                View.GetCodPallet.Text = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();
            //                this.codigoPallet = "RES-E" + Resultado.Rows[0]["idpallet"].ToString();

            //                this.seleccionUbicacion = false;
            //                ////Limpio los seriales para digitar nuevos datos
            //                //View.GetSerial1.Text = "";
            //                //View.GetSerial2.Text = "";
            //                //View.GetSerial1.Focus();
            //            }
            //        }
            //        else
            //        {
            //            Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            //        }

            //    }
            //}
            //catch (Exception Ex)
            //{
            //    Util.ShowError("Hubo un error al momento de generar el pallet. Error: " + Ex.Message);
            //    return;
            //}

        }

        private void OnCrearNuevoPallet(object sender, EventArgs e)
        {
            if (View.GetCodPallet.Text == null || View.GetCodPallet.Text == "")
            {
                Util.ShowMessage("Debe generar el pallet");
                return;
            }

            //valido que el pallet no exista
            DataTable resultado = service.DirectSQLQuery("select id_pallet from dbo.Pallets_EmpaqueDIRECTV where codigo_pallet = '" + View.GetCodPallet.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

            if (resultado.Rows.Count > 0)
            {
                Util.ShowError("Ya existe el pallet. Por favor genere uno nuevo");
                return;
            }
            else
            {
                //construyo la consulta para guardar pallet
                String ConsultaGuardar = "INSERT INTO dbo.Pallets_EmpaqueDIRECTV(codigo_pallet,usuario_creacionPallet) values('" + View.GetCodPallet.Text + "','" + this.user + "');";


                try
                {
                    //guardo el pallet en la bd
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);

                    //Util.ShowMessage("El pallet fue creado");
                    ConsultarPallets();
                    this.Actualizar_UbicacionDisponible();
                }
                catch (Exception ex)
                {
                    Util.ShowError("Error al guardar pallet");
                }
            }
        }

        private void ConsultarPallets()
        {
            //buscar pallets
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARPALLETEMPAQUE','','" + this.user + "','',''";

            View.Model.ListPallets_Empaque = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Pallets_EmpaqueDIRECTV", Local);
        }

        //Actualiza la informacion de los combobox cambio de ubicacion, permite que una posicion que esta ocupada no aparecezca en el comobobox
        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                //List<String> validValues = new List<String>() { "A1A1", "A1A2" };
                DataTable dt_auxiliar = service.DirectSQLQuery("select posicion_pallet from dbo.Pallets_EmpaqueDIRECTV where posicion_pallet is not null ", "", "dbo.Pallets_EmpaqueDIRECTV", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("posicion_pallet"))
                           .ToList();

                var query = from item in View.Model.ListadoPosiciones
                            where !list.Contains(item.Name)
                            select item;

                View.Model.ListadoPosiciones = query.ToList();
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.ToString());
            }
        }

        /** Se ejecuta cuando se pulsa la tecla enter en el cuadro de texto de busqueda de pallets **/
        private void OnEnterConsultarPallet(object sender, KeyEventArgs e)
        {
            ConsultarPallets();
        }

        /** Se ejecuta cuando se levanta una tecla en el cuadro de texto de busqueda de pallets **/
        private void OnKeyConsultarPallet(object sender, KeyEventArgs e)
        {
            String tecla_idpallet = View.GetCodPalletBusqueda.Text.ToString();
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARPALLETEMPAQUE','" + tecla_idpallet + "','" + this.user + "','',''";

            View.Model.ListPallets_Empaque = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Pallets_EmpaqueDIRECTV", Local);
        }

        private void OnSeleccionPallet_Consulta(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
                return;

            //evaluo si hay seriales en la lista y los limpio para la nueva consulta
            if (View.ListadoSerialesBusqueda.Items.Count > 0)
                View.Model.ListSeriales_Empaque.Rows.Clear();

            //evaluo el estado del pallet para habilitar opciones
            if (((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Estado"].ToString().Equals("COMPLETADO"))
            {
                //Util.ShowMessage("El pallet seleccionado esta completo.");
                View.btnNuevaCaja.IsEnabled = false;
                View.btnCerrarCaja.IsEnabled = false;
                View.btnEmpacar.IsEnabled = false;
                View.btnAbrirCaja.IsEnabled = false;
                View.btnEliminarCaja.IsEnabled = false;
                View.btnCerrarPallet.IsEnabled = false;
                View.btnEliminarPallet.IsEnabled = false;
                View.btnAbrirPallet.IsEnabled = true;
            }
            else
            {
                View.StackCajas.IsEnabled = true;
                View.btnNuevaCaja.IsEnabled = true;
                View.btnCerrarCaja.IsEnabled = true;
                View.btnEmpacar.IsEnabled = true;
                View.btnAbrirCaja.IsEnabled = true;
                View.btnEliminarCaja.IsEnabled = true;
                View.btnCerrarPallet.IsEnabled = true;
                View.btnEliminarPallet.IsEnabled = true;
                View.btnAbrirPallet.IsEnabled = false;
            }

            //obtengo el id del pallet
            String aux_idpallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();

            //contruyo la consulta
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARCAJAEMPAQUE','" + aux_idpallet + "','','',''";

            Console.WriteLine("consulta pallet: " + ConsultaSQL);
            //actualizo el listview de cajas
            View.Model.ListCajas_Empaque = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Pallets_EmpaqueDIRECTV", Local);

        }

        private void OnCerrarPallet(object sender, EventArgs e)
        {
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar el pallet a cerrar");
                return;
            }

            String ConsultaSQL = "";

            foreach (DataRowView item in View.ListadoPalletsBusqueda.SelectedItems)
            {
                String aux_idPallet = item["idPallet"].ToString();

                String Pallet = item["Pallet"].ToString();

                //valido que el pallet no tenga cajas en estado ABIERTO
                String ConsultaCajas = "select estado_caja from dbo.Cajas_EmpaqueDIRECTV where estado_caja = 'ABIERTA' and id_pallet='" + aux_idPallet + "'";
                DataTable ResultadoCaja = service.DirectSQLQuery(ConsultaCajas, "", "dbo.Pallets_EmpaqueDIRECTV", Local);
                if (ResultadoCaja.Rows.Count > 0)
                {
                    Util.ShowError("El pallet aun contiene cajas en estado 'ABIERTA'. Debe cerrar todas las cajas para continuar");
                    return;
                }
                else
                {
                    ConsultaSQL += " UPDATE dbo.Pallets_EmpaqueDIRECTV SET estado_pallet = 'COMPLETADO' WHERE id_pallet = '" + aux_idPallet + "';";
                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'CIERRE DE PALLET','EMPAQUE','EMPAQUE - PARA ENTREGA','" + Pallet + "','" + aux_idPallet + "','EMPAQUE','UBICACIONPRODUCCION_EMPAQUE','" + this.user + "','';";

                    Console.WriteLine("#### " + ConsultaSQL);
                }
            }

            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");
                //actualizo la lista y las posiciones

                View.Model.ListCajas_Empaque.Clear();
                View.Model.ListSeriales_Empaque.Clear();
                View.StackCajas.IsEnabled = false;
                View.StackSeriales.IsEnabled = false;
                ConsultarPallets();
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }
        }

        private void OnEliminarPallet(object sender, EventArgs e)
        {
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar el pallet a eliminar");
                return;
            }

            String ConsultaSQL = "";

            foreach (DataRowView item in View.ListadoPalletsBusqueda.SelectedItems)
            {
                String aux_idPallet = item["idPallet"].ToString();
                int cajasAux = Convert.ToInt32(item["Nro_Cajas"]);

                if (cajasAux > 0)
                {
                    Util.ShowError("El pallet contiene cajas guardadas y no puede ser eliminado");
                    return;
                }
                else
                {
                    ConsultaSQL += " delete from dbo.Pallets_EmpaqueDIRECTV WHERE id_pallet = '" + aux_idPallet + "'";
                }
            }

            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");
                //actualizo la lista y las posiciones

                View.Model.ListCajas_Empaque.Clear();
                ConsultarPallets();
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }
        }

        private void OnAbrirPallet(object sender, EventArgs e)
        {
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar el pallet a editar");
                return;
            }


            String ConsultaSQL = "";

            foreach (DataRowView item in View.ListadoPalletsBusqueda.SelectedItems)
            {
                String aux_idPallet = item["idPallet"].ToString();
                String Pallet = item["Pallet"].ToString();

                ConsultaSQL += " UPDATE dbo.Pallets_EmpaqueDIRECTV SET estado_pallet = 'ABIERTO' WHERE id_pallet = '" + aux_idPallet + "';";

                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'APERTURA DE PALLET','EMPAQUE','EMPAQUE - PARA ENTREGA','" + Pallet + "','" + aux_idPallet + "','EMPAQUE','UBICACIONPRODUCCION_EMPAQUE','" + this.user + "','';";
                Console.WriteLine("#### " + ConsultaSQL);
            }

            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");
                //actualizo la lista y las posiciones

                View.Model.ListCajas_Empaque.Clear();
                ConsultarPallets();
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }

        }

        private void OnCrearNuevaCaja(object sender, EventArgs e)
        {
            DataTable Resultado;

            //evaluo si se selecciono un pallet
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar el pallet donde desea crear la nueva caja");
            }
            else
            {
                //limpio el list de seriales
                if (View.ListadoSerialesBusqueda.Items.Count > 0)
                    View.Model.ListSeriales_Empaque.Rows.Clear();

                //obtengo el id del pallet
                String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();
                Console.WriteLine("Registros: " + View.Model.ListCajas_Empaque.Rows.Count);
                //evaluo si existen cajas en el pallet, si no creo la caja 1
                if (View.Model.ListCajas_Empaque.Rows.Count == 0)
                {
                    View.NuevaCaja.Text = "1";

                    String ConsultaGuardar = "INSERT INTO dbo.Cajas_EmpaqueDIRECTV(nro_caja,id_pallet) values(" + View.NuevaCaja.Text + ",'" + aux_idPallet + "');";
                    try
                    {
                        //guardo la caja en la bd
                        service.DirectSQLNonQuery(ConsultaGuardar, Local);

                        //Util.ShowMessage("La caja fue creada");
                        OnSeleccionPallet_Consulta(sender, e);
                        //ConsultarPallets();
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError("Error al guardar caja en la Base de datos");
                    }
                }
                else//si existen cajas, tomo la caja de mayor valor y le sumo 1 para seguir con el consecutivo
                {
                    //busco el numero de caja mayor y le sumo 1 para agregar la caja
                    String ConsultaBuscar = "SELECT max(nro_caja) AS NroCaja FROM dbo.Cajas_EmpaqueDIRECTV where id_pallet ='" + aux_idPallet + "'";
                    Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.Cajas_EmpaqueDIRECTV", Local);
                    Console.WriteLine("Numero mayor de caja registrado: " + Resultado.Rows[0][0].ToString());
                    int nuevacaja = Convert.ToInt32(Resultado.Rows[0][0].ToString()) + 1;
                    Console.WriteLine("Numeroconvertido + 1: " + (Convert.ToInt32(Resultado.Rows[0][0].ToString()) + 1));
                    View.NuevaCaja.Text = nuevacaja.ToString();

                    //guardo el nuevo numero de caja
                    String ConsultaGuardar = "INSERT INTO dbo.Cajas_EmpaqueDIRECTV(nro_caja,id_pallet) values(" + View.NuevaCaja.Text + ",'" + aux_idPallet + "');";
                    try
                    {
                        //guardo la caja en la bd
                        service.DirectSQLNonQuery(ConsultaGuardar, Local);

                        //Util.ShowMessage("La caja fue creada");
                        OnSeleccionPallet_Consulta(sender, e);
                        //ConsultarPallets();
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError("Error al crear la caja");
                    }
                }
            }
        }

        private void OnSeleccionCaja_Consulta(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoCajasBusqueda.SelectedIndex == -1)
                return;

            //evaluo el estado de la caja y habilito opciones
            if (((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["Estado"].ToString().Equals("COMPLETADA"))
            {
                //Util.ShowMessage("La caja seleccionada esta completa.");
                View.btnEmpacar.IsEnabled = false;
                View.btnDesempacar.IsEnabled = false;
                View.btnCerrarCaja.IsEnabled = false;
                View.btnEliminarCaja.IsEnabled = false;
            }
            else
            {
                View.StackSeriales.IsEnabled = true;
                View.btnEmpacar.IsEnabled = true;
                View.btnDesempacar.IsEnabled = true;
                View.btnCerrarCaja.IsEnabled = true;
                View.btnEliminarCaja.IsEnabled = true;
                View.btnAbrirCaja.IsEnabled = false; ;
            }

            //obtengo el id del pallet
            String aux_idpallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();

            //obtengo el numero de caja
            String aux_nrocaja = ((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["idCaja"].ToString();

            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARSERIALEMPAQUE','" + aux_idpallet + "','" + aux_nrocaja + "','',''";

            Console.WriteLine("consulta caja: " + ConsultaSQL);

            View.Model.ListSeriales_Empaque = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnCerrarCaja(object sender, EventArgs e)
        {
            if (View.ListadoCajasBusqueda.SelectedIndex == -1)
            {
                Util.ShowError("Debe seleccionar la caja a cerrar");
                return;
            }

            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            String ConsultaSQL = "";

            foreach (DataRowView item in View.ListadoCajasBusqueda.SelectedItems)
            {
                String caja = item["NroCaja"].ToString();

                ConsultaSQL += " UPDATE dbo.Cajas_EmpaqueDIRECTV SET estado_caja = 'COMPLETADA' WHERE id_pallet = '" + aux_idPallet + "' AND nro_caja = '" + caja + "';";

                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'CIERRE DE CAJA NÚMERO " + caja + "','EMPAQUE - CAJA CERRADA','EMPAQUE - ESPERANDO CIERRE DE PALLET','" + pallet + "','" + aux_idPallet + "','EMPAQUE','UBICACIONPRODUCCION_EMPAQUE','" + this.user + "','';";

            }

            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");

                View.Model.ListCajas_Empaque.Rows.Clear();
                View.Model.ListSeriales_Empaque.Rows.Clear();
                View.StackSeriales.IsEnabled = false;
                OnSeleccionPallet_Consulta(sender, e);
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }
        }

        private void OnEliminarCaja(object sender, EventArgs e)
        {
            if (View.ListadoCajasBusqueda.SelectedIndex == -1)
            {
                Util.ShowError("Debe seleccionar la caja a eliminar");
                return;
            }
            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();

            String ConsultaSQL = "";
            int equiposAux;

            foreach (DataRowView item in View.ListadoCajasBusqueda.SelectedItems)
            {
                equiposAux = Convert.ToInt32(item["Equipos"]);
                if (equiposAux > 0)
                {
                    Util.ShowError("La caja tiene equipos y no puede ser eliminada");
                    return;
                }
                else
                {
                    String caja = item["NroCaja"].ToString();
                    ConsultaSQL += " delete from dbo.Cajas_EmpaqueDIRECTV WHERE id_pallet = '" + aux_idPallet + "' AND nro_caja = '" + caja + "'";
                }
            }

            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");

                View.Model.ListCajas_Empaque.Rows.Clear();
                //OnSeleccionPallet_Consulta(sender, e);
                this.ConsultarPallets();
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }

        }

        private void OnAbrirCaja(object sender, EventArgs e)
        {
            if (View.ListadoCajasBusqueda.SelectedIndex == -1)
            {
                Util.ShowError("Debe seleccionar la caja a editar");
                return;
            }


            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["idPallet"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            String ConsultaSQL = "";

            foreach (DataRowView item in View.ListadoCajasBusqueda.SelectedItems)
            {
                String caja = item["NroCaja"].ToString();

                ConsultaSQL += " UPDATE dbo.Cajas_EmpaqueDIRECTV SET estado_caja = 'ABIERTA' WHERE id_pallet = '" + aux_idPallet + "' AND nro_caja = '" + caja + "';";
                ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'APERTURA DE CAJA NÚMERO " + caja + "','EMPAQUE - CAJA ABIERTA','EMPAQUE - ESPERANDO CIERRE DE CAJA','" + pallet + "','" + aux_idPallet + "','EMPAQUE','UBICACIONPRODUCCION_EMPAQUE','" + this.user + "','';";
            }
            try
            {
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                //Util.ShowMessage("Operacion realizada con exito");

                View.Model.ListCajas_Empaque.Rows.Clear();
                OnSeleccionPallet_Consulta(sender, e);
            }
            catch
            {
                Util.ShowError("Se presento un error al realizar la operacion");
            }

        }

        private void OnImprimirEtiqueta(object sender, EventArgs e)
        {
            if (View.ListadoCajasBusqueda.SelectedIndex == -1)
            {
                Util.ShowError("Debe seleccionar una caja para realizar la impresión de la etiqueta.");
                return;
            }

            Int32 cantidad = Int32.Parse(((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["Equipos"].ToString());
            String archivo_impresion = "";

            if (cantidad < 5)
            {
                if (cantidad == 0)
                {
                    Util.ShowMessage("Para realizar una impresión la caja debe contener equipos.");
                }
                else
                {
                    Util.ShowMessage("La cantidad minima para realizar una impresión es de 5 unidades.");
                }

                return;
            }
            else if (cantidad == 10)
            {
                archivo_impresion = "etiquetapor10.prn";
            }
            else if (cantidad == 5)
            {
                archivo_impresion = "etiquetapor5.prn";
            }
            else
            {
                Util.ShowMessage("No es posible imprimir una etiqueta para cajas con más de 10 equipos");
                return;
            }

            System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
            pd.PrinterSettings = new PrinterSettings();

            String aux_idPallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            String nroCaja = ((DataRowView)View.ListadoCajasBusqueda.SelectedItem).Row["NroCaja"].ToString();
            String familia = View.Model.ListSeriales_Empaque.Rows[0]["FAMILIA"].ToString();
            String modelo = View.Model.ListSeriales_Empaque.Rows[0]["PRODUCTO"].ToString();
            String Codsap = "";
            DataTable lista_equipos = View.Model.ListSeriales_Empaque;

            try
            {
                if (System.Windows.Forms.DialogResult.OK == pd.ShowDialog())
                {
                    PrinterControl.EtiquetaEmpaque(archivo_impresion, lista_equipos, aux_idPallet, nroCaja, familia, modelo, Codsap, pd.PrinterSettings.PrinterName);
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Error en la impresión, por favor verifique la conexión a la impresora Zebra " + ex.Message);
            }
        }

        #endregion

    }
}