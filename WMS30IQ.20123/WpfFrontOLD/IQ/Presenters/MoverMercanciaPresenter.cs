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

namespace WpfFront.Presenters
{

    public interface IMoverMercanciaPresenter
    {
        IMoverMercanciaView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class MoverMercanciaPresenter : IMoverMercanciaPresenter
    {
        public IMoverMercanciaView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        //Variables Auxiliares 
        public Connection Local;

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public MoverMercanciaPresenter(IUnityContainer container, IMoverMercanciaView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MoverMercanciaModel>();

            #region Metodos

            #region Cambio Ubicaciones

            View.BuscarRegistrosCambioUbicaciones += this.OnBuscarRegistrosCambioUbicaciones;
            View.ActualizarRegistrosCambioUbicaciones += this.OnActualizarRegistrosCambioUbicaciones;
            View.HabilitarCambioUbicacion += this.OnHabilitarCambioUbicacion;
            View.GuardarNuevaUbicacion += this.OnGuardarNuevaUbicacion;
            View.FilaSeleccionada += this.OnFilaSeleccionada;

            #endregion

            #region Cambio Clasificacion

            View.BuscarRegistrosCambioClasificacion += this.OnBuscarRegistrosCambioClasificacion;
            View.ActualizarRegistrosCambioClasificacion += this.OnActualizarRegistrosCambioClasificacion;
            View.HabilitarCambioClasificacion += this.OnHabilitarCambioClasificacion;
            View.GuardarNuevaClasificacion += this.OnGuardarNuevaClasificacion;
            View.ImprimirRegistros += this.OnImprimirRegistros;
            View.ExportPalletSeleccion += this.OnExportPalletSeleccion;
            View.ExportSerialesSeleccion += this.OnExportSerialesSeleccion;
            #endregion

            #region Recibo

            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.Imprimir_Hablador += new EventHandler<EventArgs>(this.OnImprimir_Hablador);
            View.FilaSeleccionadaRecibo += this.OnFilaSeleccionadaRecibo;

            //Cargo las ubicaciones
            //View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            //View.Model.ListadoPosicionesRecibo = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            //this.Actualizar_UbicacionDisponible();

            #endregion

            #region Combinar Estibas

            View.CombinarEstibas += new EventHandler<EventArgs>(this.OnCombinarEstibas);
            View.GenerarEstiba += new EventHandler<EventArgs>(this.OnGenerarPallet);

            #endregion

            #region Adicion de estibas 1 a 1

            View.AddLine += new EventHandler<KeyEventArgs>(this.OnAddLine);
            View.AnadirSeriales += new EventHandler<EventArgs>(this.OnAnadirSeriales);
            View.RemoveItemsSelected += new EventHandler<EventArgs>(this.OnRemoveItemSelected);

            #endregion

            #endregion

            #region Datos

            //Cargo la conexion local
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListadoPosicionesUnionEstibas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROPOSIC" } });

            this.Actualizar_UbicacionDisponible();
            this.UbicacionDisponibleUnionEstiba();

            View.Model.ListadoProductos = service.GetProduct(new Product { Reference = "1" });

            //Cargo el nombre de los productos en almacenamiento a los comobobox de filtrado 
            View.Model.ListadoProductosActivos = service.DirectSQLQuery("select productoid from dbo.EquiposCLARO where estado = 'ALMACENAMIENTO' group by productoid ", "", "dbo.EquiposCLARO", Local);

            //Cargo los datos del listado de ubicaciones destino
            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'ALMACENAMIENTO', 'CLARO'", "", "dbo.Ubicaciones", Local);

            View.Model.ListUbicacionesDestino_Recibo = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'RECIBOALMACEN', 'CLARO'", "", "dbo.Ubicaciones", Local);

            createTable();
            #endregion
        }

        /*Actualiza la informacion de los combobox cambio de ubicacion, permite que una posicion que esta ocupada no aparecezca en el comboBox*/
        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                //Cargo todas las ubicaciones
                View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROPOSIC" } });

                DataTable dt_auxiliar = service.DirectSQLQuery("SELECT posicion FROM dbo.EquiposCLARO WHERE posicion IS NOT NULL AND (estado LIKE 'ALMACENAMIENTO' OR Estado LIKE 'DESPACHO') GROUP BY posicion ", "", "dbo.EquiposCLARO", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("posicion"))
                           .ToList();

                var query = from item in View.Model.ListadoPosiciones
                            where !list.Contains(item.Name)
                            select item;

                var query1 = from item in View.Model.ListadoPosiciones
                             where list.Contains(item.Name)
                             select item;

                View.Model.ListadoPosicionesCambioUbicacion = query.ToList();
                View.Model.ListadoPosiciones = query1.ToList();
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.ToString());
            }
        }

        #region Cambio Ubicaciones

        private void OnBuscarRegistrosCambioUbicaciones(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosCambioUbicaciones();
        }

        public void BuscarRegistrosCambioUbicaciones()
        {
            //Variables Auxiliares
            String ConsultaSQL;
            String num_pallet, Posicion, ProductoID, FechaIngreso;

            num_pallet = View.BuscarEstibaCambioUbicacion.Text.ToString().Replace("'", "-");
            Posicion = View.BuscarPosicionCambioUbicacion.Text.ToString();
            ProductoID = View.BuscarModeloCambioUbicacion.Text.ToString();
            FechaIngreso = View.GetFechaIngresoCambioUbicacion.Text.ToString();

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAUBICACION', '" + num_pallet + "','" + Posicion + "','" + ProductoID + "','" + FechaIngreso + "'";

            //Ejecuto la consulta
            View.Model.ListadoCambioUbicacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosCambioUbicaciones(object sender, EventArgs e)
        {
            //Limpio los campos de la busqueda
            View.BuscarEstibaCambioUbicacion.Text = "";
            View.BuscarPosicionCambioUbicacion.SelectedIndex = -1;
            View.BuscarModeloCambioUbicacion.SelectedIndex = -1;
            View.GetFechaIngresoCambioUbicacion.Text = "";
        }

        private void OnHabilitarCambioUbicacion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioUbicacion.SelectedIndex == -1)
                return;

            //Coloco visible el panel para seleccionar el cambio de ubicacion
            View.StackCambioUbicacion.Visibility = Visibility.Visible;

            //Coloco la ubicacion del registro seleccionado en el campo de ubicacion actual
            View.TextoUbicacionActual.Text = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Posicion"].ToString();
        }

        private void OnGuardarNuevaUbicacion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioUbicacion.SelectedIndex == -1)
                return;

            //Evaluo que haya seleccionado la nueva ubicacion
            if (View.NuevaUbicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva ubicacion.");
                return;
            }

            String pallet = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["idpallet"].ToString();
            String posicion_actual = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Posicion"].ToString();
            String posicion_seleccionada = ((MMaster)View.NuevaUbicacion.SelectedItem).Code.ToString();

            ConsultaSQL = "select TOP 1 Posicion,idpallet from dbo.EquiposCLARO where estado = 'ALMACENAMIENTO' AND POSICION = '" + posicion_seleccionada + "'";

            //Ejecuto la consulta
            DataTable aux = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO ", Local);

            if (aux.Rows.Count == 0)
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEPOSICION1','" + posicion_seleccionada + "','" + ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["IdPallet"].ToString() + "';";

                //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                ConsultaSQL += "EXEC sp_InsertarNuevo_Movimiento 'CAMBIO DE UBICACIÓN ALMACENAMIENTO','" + posicion_actual + "','" + posicion_seleccionada + "','" + pallet + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";
                Console.WriteLine("###### " + ConsultaSQL);

                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

                //Oculto el bloque de actualizacion
                View.StackCambioUbicacion.Visibility = Visibility.Collapsed;

                //Quito la selecion de la nueva ubicacion
                View.NuevaUbicacion.SelectedIndex = -1;

                //Quito la seleccion del listado
                View.ListadoBusquedaCambioUbicacion.SelectedIndex = -1;

                //Hago la busqueda de registros para actualizar el listado
                BuscarRegistrosCambioUbicaciones();

                //Realiza la actualizacion de los combobox de ubicacion
                Actualizar_UbicacionDisponible();
            }
            else
            {
                Util.ShowMessage("La posición en bodega " + posicion_seleccionada + " se encuentra ocupada actualmente por el pallet " + aux.Rows[0]["idpallet"].ToString());
            }
        }

        #endregion

        #region Cambio Clasificación

        private void OnBuscarRegistrosCambioClasificacion(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosCambioClasificacion();
        }

        public void BuscarRegistrosCambioClasificacion()
        {
            //Variables Auxiliares
            String ConsultaSQL;
            String num_pallet, Posicion, ProductoID, FechaIngreso;

            num_pallet = View.BuscarEstibaCambioClasificacion.Text.ToString().Replace("'", "-"); ;
            Posicion = View.BuscarPosicionCambioClasificacion.Text.ToString();
            ProductoID = View.BuscarModeloCambioClasificacion.Text.ToString();
            FechaIngreso = View.GetFechaIngresoCambioClasificacion.Text.ToString();

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAUBICACIONENVIO', '" + num_pallet + "','" + Posicion + "','" + ProductoID + "','" + FechaIngreso + "'";

            //Ejecuto la consulta
            View.Model.ListadoCambioClasificacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosCambioClasificacion(object sender, EventArgs e)
        {
            //Limpio los campos de la busqueda
            View.BuscarEstibaCambioClasificacion.Text = "";
            View.BuscarPosicionCambioClasificacion.SelectedIndex = -1;
            View.BuscarModeloCambioClasificacion.SelectedIndex = -1;
            View.GetFechaIngresoCambioClasificacion.Text = "";
        }

        private void OnHabilitarCambioClasificacion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
                return;

            String aux_idpallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["idpallet"].ToString();

            //consulto los seriales contenidos en esa estiba
            View.Model.Listado_PalletSerial = service.DirectSQLQuery("SELECT IdPallet as PPallet,serial as PSerial,codigo_SAP as SAP,mac as PMac,ProductoID as Modelo,TIPO_ORIGEN as PTRecibo,Origen as PTOrigen,consecutivo as Remision,convert(VARCHAR,FECHA_INGRESO,120) as PFRegistro," +
            "DATEDIFF(day, FECHA_INGRESO,GETDATE()) as NumeroDias,dbo.TIMELAPSELEO(FECHA_INGRESO) as horas	from dbo.EquiposCLARO WHERE ((IdPallet IS NOT NULL) AND (Posicion IS NOT NULL) AND (ESTADO = 'ALMACENAMIENTO'))" +
            "AND idpallet = '" + aux_idpallet + "' ", "", "dbo.EquiposCLARO", Local);

            //Coloco visible el panel para seleccionar el cambio de clasificacion
            View.StackCambioClasificacion.Visibility = Visibility.Visible;

            //Coloco la clasificacion del registro seleccionado en el campo de clasificacion actual
            View.TextoClasificacionActual.Text = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Ubicacion"].ToString();

            int total = 0;
            int num_estibas = View.ListadoBusquedaCambioClasificacion.SelectedItems.Count;

            foreach (DataRowView Registros in View.ListadoBusquedaCambioClasificacion.SelectedItems)
            {
                total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());
            }

            View.TotalSeriales.Text = total.ToString();
            View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        }

        private void OnGuardarNuevaClasificacion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL, NuevaUbicacion, NuevoEstado, ConsultaTrack, ConsultaMovSQL;

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedItems.Count == 0)
                return;

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.NuevaClasificacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }

            if (!UtilWindow.ConfirmOK("¿Esta seguro que quiere cambiar la ubicación de los pallets seleccionados?") == true)
                return;

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.NuevaClasificacion.SelectedItem).Row["UbicacionDestino"].ToString();

            foreach (DataRowView fila in View.ListadoBusquedaCambioClasificacion.SelectedItems)
            {
                if (NuevaUbicacion == "TRANSITO A REP.")
                {
                    NuevoEstado = "PARA REPARACION";

                    //Creo la consulta la cambiar la clasificacion de la estiba
                    ConsultaSQL = "UPDATE dbo.equiposCLARO SET Posicion=NULL,Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "', CodigoEmpaque = '" + fila.Row["idPallet"].ToString() + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "' AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";

                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaMovSQL = "EXEC sp_InsertarNuevo_Movimiento 'ENVIO A PRODUCCIÓN " + NuevoEstado + "','ALMACENAMIENTO','REPARACIÓN','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";

                    ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "';";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
                else if (NuevaUbicacion == "TRANSITO A DIAG.")
                {
                    NuevoEstado = "PARA DIAGNOSTICO";

                    //Creo la consulta la cambiar la clasificacion de la estiba
                    ConsultaSQL = "UPDATE dbo.equiposCLARO SET Posicion=NULL,Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "'  AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";

                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaMovSQL = "EXEC sp_InsertarNuevo_Movimiento 'ENVIO A PRODUCCIÓN " + NuevoEstado + "','ALMACENAMIENTO','DIAGNOSTICO','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";

                    ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "';";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
                else
                {
                    NuevoEstado = "DESPACHO";

                    //Creo la consulta la cambiar la clasificacion de la estiba
                    ConsultaSQL = "UPDATE dbo.equiposCLARO SET CodigoEmpaque2 = IdPallet, Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "'  AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";

                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaMovSQL = "EXEC sp_InsertarNuevo_Movimiento 'ENVIO A " + NuevoEstado + "','ALMACENAMIENTO','DESPACHO','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";

                    ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "'";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
            }

            View.TotalSeriales.Text = "0";
            View.Estibas_Seleccionadas.Text = "0";

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de clasificacion realizado satisfactoriamente.");

            //Hago la busqueda de registros para actualizar el listado
            BuscarRegistrosCambioClasificacion();

            //Oculto el bloque de actualizacion
            View.StackCambioClasificacion.Visibility = Visibility.Collapsed;

            //Quito la selecion de la nueva ubicacion
            View.NuevaClasificacion.SelectedIndex = -1;

            View.Model.Listado_PalletSerial.Rows.Clear();

            //Quito la seleccion del listado
            View.ListadoBusquedaCambioClasificacion.SelectedIndex = -1;
        }

        private void OnImprimirRegistros(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            String NuevaUbicacion = "", NuevoEstado;

            NuevaUbicacion = ((DataRowView)View.NuevaClasificacion.SelectedItem).Row["UbicacionDestino"].ToString();

            if (NuevaUbicacion == "TRANSITO A REP.")
            {
                NuevoEstado = "ALMACENAMIENTO - REPARACIÓN";
            }
            else if (NuevaUbicacion == "TRANSITO A DIAG.")
            {
                NuevoEstado = "ALMACENAMIENTO - DIAGNOSTICO";
            }
            else
            {
                NuevoEstado = "ALMACENAMIENTO - DESPACHO";
            }

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar uno o más pallets");
                return;
            }

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.NuevaClasificacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }

            //ListView PalletImprimir = View.ListadoBusquedaCambioClasificacion;
            ListView SerialesImprimir = View.ListadoSerialesCambioClasificacion;

            String transito = NuevoEstado;
            String ubicacion = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Ubicacion"].ToString();
            //String posicion = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Posicion"].ToString();
            String cantidad = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Cantidad"].ToString();
            String fechaIngreso = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["FechaIngreso"].ToString();
            String idpallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["IdPallet"].ToString();
            String dias = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["NumeroDias"].ToString();
            String tiempoTranscurrido = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Horas"].ToString();

            //Imprimo los registros
            PrinterControl.PrintMovimientosMercancia(this.userName, transito, ubicacion, cantidad, fechaIngreso, idpallet, dias, tiempoTranscurrido, SerialesImprimir);
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
                int filas_seleccion = View.ListadoBusquedaCambioClasificacion.SelectedItems.Count;

                if (filas_seleccion > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaClasificacion.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaClasificacion.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Orange);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoBusquedaCambioClasificacion.SelectedItems)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaClasificacion.Columns.Count].Value =
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
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }

        private void OnExportSerialesSeleccion(object sender, EventArgs e)
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

                for (int Idx = 0; Idx < View.GridViewListaSerialesClasificacion.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaSerialesClasificacion.Columns[Idx].Header.ToString();
                    ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Orange);
                }


                int cont = 0;
                foreach (DataRowView Registros in View.ListadoSerialesCambioClasificacion.Items)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                    ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaSerialesClasificacion.Columns.Count].Value =
                            Registros.Row.ItemArray;
                    cont++;
                }


                rng = ws.get_Range("A1", "H" + cont + 1);
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

        #region Recibo

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            View.BuscarEstibaRecibo.Text = "";
            //View.BuscarPosicionRecibo.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIA_RECIBEALMANCEN', 'PARA ALMACENAMIENTO', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, ConsultaTrack = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
                return;

            //Coloco la ubicacion
            //NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
            NuevaUbicacion = View.Ubicacion.Text.ToString();

            //Obtengo el valor de origen de la primera fila
            //((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Origen"].ToString();

            int cont_noValidos = 0, cont_validos = 0; // Se cuentan los registros no validos para enviar a DESPACHO, aquellos que provengan de PRODUCCION(Reparacion, Diagnostico)
            String Estado = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Estado"].ToString();

            //Valido la ubicacion para colocar el estado
            if (NuevaUbicacion == "DESPACHO")
            {
                NuevoEstado = "DESPACHO";

                if (Estado == "PARA SCRAP")
                {
                    Util.ShowMessage("Los pallets provenientes de produccion en estado SCRAP no pueden ser despachados.");
                    return;
                }
                else
                {

                    foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
                    {
                        if (Registros.Row["Origen"].ToString() == "EMPAQUE")
                        {
                            //Creo la consulta para cambiar la ubicacion de la estiba
                            ConsultaSQL = "EXEC sp_GetProcesos 'UPDATE_ALMACENAMIENTO','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "'";
                            ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "' ";

                            ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'PALLET RECIBIDO PARA DESPACHO','RECEPCIÓN ALMACENAMIENTO','Z DESPACHO','" + Registros.Row["Pallet"].ToString()
                                        + "','','ALMACENAMIENTO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";

                            //Ejecuto la consulta
                            service.DirectSQLNonQuery(ConsultaSQL, Local);
                            service.DirectSQLNonQuery(ConsultaTrack, Local);
                            cont_validos++;
                        }

                        else if (Registros.Row["Estado"].ToString() == "PARA SCRAP")
                        {
                            //Creo la consulta para cambiar la ubicacion de la estiba
                            ConsultaSQL = "EXEC sp_GetProcesos 'UPDATE_ALMACENAMIENTOSCRAP','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "'";
                            ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "' ";

                            ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'PALLET RECIBIDO PARA DESPACHO','RECEPCIÓN ALMACENAMIENTO','Z DESPACHO','" + Registros.Row["Pallet"].ToString()
                                        + "','','ALMACENAMIENTO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";

                            //Ejecuto la consulta
                            service.DirectSQLNonQuery(ConsultaSQL, Local);
                            service.DirectSQLNonQuery(ConsultaTrack, Local);
                            cont_validos++;
                        }
                        else
                        {
                            cont_noValidos++;
                        }
                    }
                }
            }
            else if (NuevaUbicacion == "ALMACENAMIENTO")
            {
                NuevoEstado = "ALMACENAMIENTO";
                ConsultaTrack = "declare @fechaActual datetime = getdate();";

                String ubicacion = ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString();
                String consulta = "select top 1 idpallet from dbo.EquiposClaro where Posicion LIKE '" + ubicacion + "';";

                DataTable aux = service.DirectSQLQuery(consulta, "", "dbo.EquiposClaro", Local);

                if (aux.Rows.Count > 0)
                {
                    Util.ShowMessage("La posición seleccionada ya se encuentra ocupada por el pallet " + aux.Rows[0]["idpallet"].ToString());
                    Actualizar_UbicacionDisponible();
                    return;
                }
                else
                {
                    String origen = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Origen"].ToString();
                    String pallet = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Pallet"].ToString();

                    if (origen.Contains("PRODUCCION"))
                    {
                        //Recorro el listado de registros seleccionados para confirmar el recibo
                        foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
                        {
                            //Creo la consulta para cambiar la ubicacion de la estiba
                            //ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEALMACENAMIENTO_FROMDIAGNOSTICO','" + NuevoEstado + "', '" + ((Estado == "PARA SCRAP") ? "SCRAP" : NuevaUbicacion) + "','" + ubicacion + "','" + Registros.Row["Pallet"].ToString() + "';";

                            ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEALMACENAMIENTO_FROMDIAGNOSTICO','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ubicacion + "','" + Registros.Row["Pallet"].ToString() + "';";
                            ConsultaTrack = "UPDATE dbo.TrackEquiposCLARO SET FECHA_ING_ALMACEN = @fechaActual, ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "';";

                            ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'PALLET RECIBIDO PARA ALMACENAMIENTO','RECEPCIÓN ALMACENAMIENTO','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + " ALMACEN','" + Registros.Row["Pallet"].ToString()
                            + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "';";

                            //Ejecuto la consulta
                            service.DirectSQLNonQuery(ConsultaSQL, Local);
                            service.DirectSQLNonQuery(ConsultaTrack, Local);
                        }
                    }
                    else
                    {
                        Util.ShowMessage("    No es posible realizar el movimiento para el pallet " + pallet + ",\n los pallets provenientes de EMPAQUE deben enviarse a DESPACHO.");
                        return;
                    }
                }
            }

            ////Elimino las filas seleccionadas de la vista, sin necesidad de volver a consultar la BD VA DENTRO DEL FOR
            //while (View.ListadoBusquedaRecibo.SelectedItems.Count >= fila)
            //{
            //  if (View.Model.ListadoRecibo.Rows[fila]["Origen"].ToString() == "EMPAQUE")
            //  {
            //      View.Model.ListadoRecibo.Rows.RemoveAt(View.ListadoBusquedaRecibo.Items.IndexOf(View.ListadoBusquedaRecibo.SelectedItem));
            //  }
            // fila++;
            //}

            //Muestro el mensaje de confirmacion
            if (cont_noValidos > 0)
            {
                Util.ShowMessage(cont_validos + " pallet recibidos exitosamente, " + cont_noValidos + " pallets no se pueden enviar a DESPACHO,\n los pallets provenientes de PRODUCCIÓN no se pueden enviar a DESPACHO.");
            }
            else
            {
                Util.ShowMessage("Recibo de pallet realizado satisfactoriamente.");
            }

            View.Ubicacion.SelectedIndex = -1;

            View.UbicacionDesp.SelectedIndex = -1;

            //Busco los registros para actualizar el listado
            BuscarRegistrosRecibo();

            //Actualiza el combobox de posiciones
            View.Model.ListadoPosicionesRecibo = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            //this.Actualizar_UbicacionDisponible();
        }

        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosRecibo();
        }

        public void BuscarRegistrosRecibo()
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIA_RECIBEALMANCEN', 'PARA ALMACENAMIENTO' ";

            //Valido si fue digitado una estiba para buscar
            if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
                ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            else
                ConsultaSQL += ",''";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnImprimir_Hablador(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String destino = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

            String NroSeriales = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Cantidad"].ToString();
            String fechaIngreso = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Fecha"].ToString();
            String pallet = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Pallet"].ToString();
            String modelo = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Modelo"].ToString();

            String origen = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Origen"].ToString();
            String estado = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Estado"].ToString();

            //Evaluo que haya seleccionado laexport plain text  nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }
            else
            {
                //destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
                //destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
                destino = View.Ubicacion.Text.ToString();
            }

            if (destino == "ALMACENAMIENTO")
            {
                if (origen.Contains("PRODUCCION"))
                {
                    //Creo la base de la consulta para traer los seriales respectivos
                    ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from dbo.EquiposClaro where codigoEmpaque LIKE '" + pallet + "'";

                    //Ejecuto la consulta
                    SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

                    //Imprimo los registros
                    PrinterControl.PrintMovimientosBodega(this.userName, SerialesImprimir, "PALLET", pallet, estado, "CLARO", "POSICIONAR EN ALMACEN", "", "aux");
                }
                else
                {
                    Util.ShowMessage("    No es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de EMPAQUE deben enviarse a DESPACHO.");
                }
            }
            else if (destino == "DESPACHO")
            {

                if (origen == "EMPAQUE")
                {
                    //Creo la base de la consulta para traer los seriales respectivos
                    ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from Pallets_EmpaqueCLARO pallet join EquiposCLARO eqc on pallet.id_pallet = eqc.pila where pallet.codigo_pallet = '" + pallet + "'";

                    //Ejecuto la consulta
                    SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

                    //Imprimo los registros
                    PrinterControl.PrintMovimientosBodega (this.userName, SerialesImprimir, "PALLET", pallet, destino, "CLARO", "ALMACENAMIENTO - " + destino, "", "");
                }

                else if (estado == "PARA SCRAP")
                {
                    Util.ShowMessage("Los pallets provenientes de produccion en estado SCRAP no pueden ser despachados.");
                    return;

                    ////Creo la base de la consulta para traer los seriales respectivos
                    //ConsultaSQL = "select serial,ProductoID,CODIGO_SAP,Fecha_Ingreso from EquiposCLARO where CodigoEmpaque = '" + pallet + "'";

                    ////Ejecuto la consulta
                    //SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

                    ////Imprimo los registros
                    //PrinterControl.PrintMovimientosBodega(SerialesImprimir, "PALLET", pallet, destino, "CLARO", "ALMACENAMIENTO - " + destino, "", "");
                }

                else
                {
                    Util.ShowMessage("\tNo es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de PRODUCCIÓN no se pueden enviar a DESPACHO.");
                }
            }
        }

        /**
         * Metodo utilizado para obtener referencia al datatable de estibas cuando se pulsa sobre una fila
         */
        private void OnFilaSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            int total = 0;
            int num_estibas = View.ListadoBusquedaRecibo.SelectedItems.Count;

            foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
            {
                total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());
            }

            View.TotalSeriales.Text = total.ToString();
            View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        }

        private void OnFilaSeleccionadaRecibo(object sender, SelectionChangedEventArgs e)
        {
            int total = 0;
            int num_estibas = View.ListadoBusquedaRecibo.SelectedItems.Count;

            foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
            {
                total = total + Int32.Parse(Registros.Row["Cantidad"].ToString());
            }

            View.TotalSeriales.Text = total.ToString();
            View.Estibas_Seleccionadas.Text = num_estibas.ToString();
        }

        #endregion

        #region Union de estibas

        private void UbicacionDisponibleUnionEstiba()
        {
            try
            {
                DataTable dt_auxiliar = service.DirectSQLQuery("select posicion from dbo.EquiposCLARO where posicion is not null AND (estado LIKE 'ALMACENAMIENTO' OR estado LIKE 'DESPACHO')  group by posicion ", "", "dbo.EquiposCLARO", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("posicion"))
                           .ToList();

                var query = from item in View.Model.ListadoPosicionesUnionEstibas
                            where !list.Contains(item.Name)
                            select item;

                View.Model.ListadoPosicionesUnionEstibas = query.ToList();
                View.CBO_UbicacionUnionEstibas.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.Message.ToString());
            }
        }

        private void OnGenerarPallet(object sender, EventArgs e)
        {
            if (View.CBO_UbicacionUnionEstibas.SelectedIndex == -1)
            {
                View.TXT_seleccionarUbicacion.Visibility = Visibility.Visible;
                return;
            }
            //Variables Auxiliares
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            //Creo el número de pallet aleatorio 
            ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

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
                    View.TXT_palletGeneratedUnionEstibas.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                    if (View.CBO_UbicacionUnionEstibas.SelectedIndex > 0)
                    {
                        View.TXT_seleccionarUbicacion.Visibility = Visibility.Hidden;
                    }
                    View.BTN_CombinarEstibas.IsEnabled = true;
                    View.BTN_CombinarEstibas.FontWeight = FontWeights.Light;
                }
            }
            else
            {
                Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            }
        }

        private void OnCombinarEstibas(object sender, EventArgs e)
        {
            if (View.ListadoBusquedaCambioClasificacion.SelectedItems.Count <= 1)
            {
                Util.ShowMessage("Por favor seleccione dos o mas estibas de la lista");
                return;
            }
            string nuevaEstiba = View.TXT_palletGeneratedUnionEstibas.Text.ToString();
            string nuevaUbicacion = View.CBO_UbicacionUnionEstibas.Text.ToString();
            string updateQuery = "UPDATE dbo.EquiposClaro SET idPallet = '" + nuevaEstiba + "', Posicion = '" + nuevaUbicacion + "' WHERE idPallet IN (";
            string ConsultaSQL = "";
            string idPallets = "''";
            try
            {
                foreach (DataRowView item in View.ListadoBusquedaCambioClasificacion.SelectedItems)
                {
                    ConsultaSQL = "EXEC sp_InsertarNuevo_Movimiento 'UNIÓN DE ESTIBAS MOV. MERCANCIA', '" + nuevaEstiba + "', '" + item[0].ToString()  + "', '" + nuevaUbicacion + "', '', 'MOV. MERCANCIA','UNIONESTIBAS','" + this.user + "','';";
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    idPallets += "," + "'" + item[0].ToString() + "'";
                }
                idPallets += ")";
                updateQuery += idPallets;
                service.DirectSQLNonQuery(updateQuery, Local);
               
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Hubo un error al realizar la combinación de estibas " + ex.Message.ToString());
            }

            this.UbicacionDisponibleUnionEstiba();
            this.BuscarRegistrosCambioClasificacion();
            View.TXT_palletGeneratedUnionEstibas.Text = "";

        }

        #endregion

        #region Adición de seriales 1 a 1 
        
        private void OnAddLine(object sender, KeyEventArgs e)
        {
            string serial;
            string consultaBuscar;
            DataRow dr = View.Model.ListSerialsOneByOne.NewRow();
            DataTable RegistroValidado = null;

            serial = View.TXT_serialAdicionSeriales.Text.ToString();

            foreach (DataRow item in View.Model.ListSerialsOneByOne.Rows)
            {
                if (serial.ToUpper() == item["Serial"].ToString().ToUpper())
                {
                    Util.ShowError("El serial " + serial.ToUpper() + " ya esta en el listado.");
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                    return;
                }
            }
            consultaBuscar = "EXEC sp_GetProcesos 'BUSCAREQUIPOSALMACENAR', '" + serial  + "', NULL, NULL";
            try
            {
                RegistroValidado = service.DirectSQLQuery(consultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (RegistroValidado.Rows.Count > 0)
                {
                    dr["Serial"] = RegistroValidado.Rows[0]["serial"].ToString().ToUpper();
                    dr["Producto"] = RegistroValidado.Rows[0]["ProductoID"].ToString();
                    dr["COD_SAP"] = RegistroValidado.Rows[0]["CODIGO_SAP"].ToString();
                    View.Model.ListSerialsOneByOne.Rows.Add(dr);
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                }
                else
                {
                    Util.ShowMessage("¡El equipo no se encuentra registrado en el sistema!");
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Ha ocurrido un error " + ex.Message);
            }
        }

        private void createTable()
        {
            // Creo la tabla para añadir las nuevas filas.
            // Inicializo el DataTable.
            View.Model.ListSerialsOneByOne = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListSerialsOneByOne.Columns.Add("Serial", typeof(String));
            View.Model.ListSerialsOneByOne.Columns.Add("Producto", typeof(String));
            View.Model.ListSerialsOneByOne.Columns.Add("COD_SAP", typeof(String));
        }

        private void OnAnadirSeriales(object sender, EventArgs e)
        {
            string idPalletSelected = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row[0].ToString();
            string UbicacionSelected = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row[1].ToString();
            string Ubicacion;
            string Estado = Ubicacion = "ALMACENAMIENTO";

            string ConsultaSQL = "";
            string UpdateQuery = "UPDATE dbo.EquiposClaro SET idPallet = '" + idPalletSelected + "', Posicion = '" + UbicacionSelected + "', Ubicacion = '" + Ubicacion + "', Estado = '" + Estado + "' WHERE Serial IN ('' ";
            try
            {
                foreach (DataRowView item in View.LV_serialesOneByOne.Items)
                {
                    ConsultaSQL = "EXEC dbo.sp_InsertarNuevo_Movimiento 'ADICION SERIAL A ESTIBA', 'SERIAL ADICIONADO', '" + item[0].ToString() + "', '" + idPalletSelected + "', '', 'MOV. MERCANCIA','ADICIONSERIALES','" + this.user + "','';";
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    UpdateQuery += ", '" + item[0].ToString() +"'";
                }
                UpdateQuery += ")";
           
                service.DirectSQLNonQuery(UpdateQuery, Local);
                this.UbicacionDisponibleUnionEstiba();
                this.BuscarRegistrosCambioClasificacion();
                View.TXT_serialAdicionSeriales.Text = "";
                View.TXT_serialAdicionSeriales.Focus();
                View.Model.ListSerialsOneByOne.Clear();
            }
            catch (Exception ex)
            {
                Util.ShowError("Se produjo un error al actualizar el listado de seriales: " + ex.Message);
            }
        }

        private void OnRemoveItemSelected(object sender, EventArgs e)
        {
            if (View.LV_serialesOneByOne.SelectedItems.Count == -1)
            {
                Util.ShowMessage("Por favor seleccione uno o mas seriales para remover de la lista");
                return;
            }

            for (int i = 0; i < View.LV_serialesOneByOne.SelectedItems.Count; i++ )
            {
                while (View.LV_serialesOneByOne.SelectedItems.Count > 0)
                {
                    View.Model.ListSerialsOneByOne.Rows.RemoveAt(View.LV_serialesOneByOne.Items.IndexOf(View.LV_serialesOneByOne.SelectedItem));
                }
            }
        }

        #endregion
    }
}