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
using Microsoft.Office.Interop.Excel;
using WpfFront.Common.Windows;
using System.Windows.Input;

namespace WpfFront.Presenters
{

    public interface IMoverMercanciaDTVPresenter
    {
        IMoverMercanciaDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class MoverMercanciaDTVPresenter : IMoverMercanciaDTVPresenter
    {
        public IMoverMercanciaDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName; 

        public MoverMercanciaDTVPresenter(IUnityContainer container, IMoverMercanciaDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MoverMercanciaDTVModel>();

            #region Metodos

            #region Cambio Ubicaciones

            View.BuscarRegistrosCambioUbicaciones += this.OnBuscarRegistrosCambioUbicaciones;
            View.ActualizarRegistrosCambioUbicaciones += this.OnActualizarRegistrosCambioUbicaciones;
            View.HabilitarCambioUbicacion += this.OnHabilitarCambioUbicacion;
            View.GuardarNuevaUbicacion += this.OnGuardarNuevaUbicacion;
            View.GuardarNuevoEstado += this.OnGuardarNuevoEstado;
            View.FilaSeleccionada += this.OnFilaSeleccionadas;

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

            #endregion

            #region Datos

            //Cargo la conexion local
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVPOSIC" } });
            this.Actualizar_UbicacionDisponible();

            // Cargo el nombre de los productos en almacenamiento a los combobox de filtrado
            View.Model.ListadoProductosActivos = service.DirectSQLQuery("SELECT MODELO FROM dbo.EquiposDIRECTVC WHERE ESTADO = 'ALMACENAMIENTO' GROUP BY MODELO", "", "dbo.EquiposDIRECTVC", Local);
            View.Model.ListadoEstadosPallet = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVESTREC" } });
            //Cargo los datos del listado de ubicaciones destino
            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'ALMACENAMIENTO', 'CLARO'", "", "dbo.Ubicaciones", Local);

            #endregion
        }

        #region Cambio Ubicaciones

        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVPOSIC" } });
                //List<String> validValues = new List<String>() { "A1A1", "A1A2" };
                System.Data.DataTable dt_auxiliar = service.DirectSQLQuery("select POSICION from dbo.EquiposDIRECTVC where posicion is not null ", "", "dbo.EquiposDIRECTVC", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("POSICION"))
                           .ToList();

                var query = from item in View.Model.ListadoPosiciones
                            where !list.Contains(item.Name)
                            select item;

                View.Model.ListadoPosiciones = query.ToList();
                View.Model.ListadoPosicionesOcupadas = service.DirectSQLQuery("SELECT POSICION FROM dbo.EquiposDIRECTVC WHERE ESTADO = 'ALMACENAMIENTO' and POSICION is not null GROUP BY POSICION", "", "dbo.EquiposDIRECTVC", Local);
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.ToString());
            }
        }

        private void OnBuscarRegistrosCambioUbicaciones(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosCambioUbicaciones();
        }

        public void BuscarRegistrosCambioUbicaciones()
        {
            // Variables Auxiliares.
            string ConsultaSQL;
            string num_pallet, Modelo, Posicion, aux_FechaIngreso;
            DateTime fechaIngreso;

            // Asignacion de variables.
            aux_FechaIngreso = View.GetFechaIngresoCambioUbicacion.Text;
            if (aux_FechaIngreso != "")
            {
                // Si se filtro por la fecha le daremos un formato para la busqueda en la base de datos. 
                fechaIngreso = DateTime.Parse(aux_FechaIngreso);
                String DateFormat = "dd/MM/yyyy";
                aux_FechaIngreso = fechaIngreso.ToString(DateFormat);
            }

            num_pallet = View.BuscarEstibaCambioUbicacion.Text;
            Posicion = View.BuscarPosicionCambioUbicacion.Text;
            Modelo = View.BuscarModeloCambioUbicacion.Text;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAUBICACION', '" + num_pallet + "', '" + Posicion + "', '" + Modelo + "', '" + aux_FechaIngreso + "'";
            //Ejecuto la consulta
            View.Model.ListadoCambioUbicacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            limpiarFormularioCambioUbicacion();
        }

        protected void limpiarFormularioCambioUbicacion() 
        {
            //Limpio los campos de la busqueda
            View.BuscarEstibaCambioUbicacion.Text = "";
            View.BuscarPosicionCambioUbicacion.SelectedIndex = -1;
            View.BuscarModeloCambioUbicacion.SelectedIndex = -1;
            View.GetFechaIngresoCambioUbicacion.Text = "";
        }

        private void OnActualizarRegistrosCambioUbicaciones(object sender, EventArgs e) { }

        private void OnHabilitarCambioUbicacion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioUbicacion.SelectedIndex == -1)
                return;

            //Coloco visible el panel para seleccionar el cambio de ubicacion
            View.StackCambioUbicacion.Visibility = Visibility.Visible;

            //Coloco la ubicacion del registro seleccionado en el campo de ubicacion actual
            View.TextoUbicacionActual.Text = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Posicion"].ToString();
            View.TextoEstadoActual.Text = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Estado"].ToString();
        }

        private void OnGuardarNuevaUbicacion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;
            String pallet = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["IdPallet"].ToString();
            String posicion_seleccionada = ((MMaster)View.NuevaUbicacion.SelectedItem).Code.ToString();
            String posicion_actual = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Posicion"].ToString();

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioUbicacion.SelectedIndex == -1)
                return;

            //Evaluo que haya seleccionado la nueva ubicacion
            if (View.NuevaUbicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva ubicacion.");
                return;
            }

            //Creo la consulta para cambiar la ubicacion de la estiba
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEPOSICION1','" + posicion_seleccionada + "','" + pallet + "';";

            ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'CAMBIO DE UBICACIÓN ALMACENAMIENTO','" + posicion_actual + "','" + posicion_seleccionada + "','" + pallet + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";
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
            BuscarRegistrosCambioClasificacion();
            Actualizar_UbicacionDisponible();
        }

        private void OnGuardarNuevoEstado(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;
            String pallet = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["IdPallet"].ToString();
            String estado_seleccionado = ((MMaster)View.NuevoEstado.SelectedItem).Name.ToString();
            String estado_actual = ((DataRowView)View.ListadoBusquedaCambioUbicacion.SelectedItem).Row["Estado"].ToString();

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioUbicacion.SelectedIndex == -1)
                return;

            //Evaluo que haya seleccionado la nueva ubicacion
            if (View.NuevoEstado.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar el nuevo estado.");
                return;
            }

            //Creo la consulta para cambiar la ubicacion de la estiba
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEESTADORECIBO','" + estado_seleccionado + "','" + pallet + "';";

            ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'CAMBIO DE ESTADO ALMACENAMIENTO','" + estado_actual + "','" + estado_seleccionado + "','" + pallet + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";
            Console.WriteLine("###### " + ConsultaSQL);

            //Ejecuto la consulta
            service.DirectSQLNonQuery(ConsultaSQL, Local);

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            //Oculto el bloque de actualizacion
            //View.StackCambioUbicacion.Visibility = Visibility.Collapsed;

            //Quito la selecion de la nueva ubicacion
            View.NuevoEstado.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.ListadoBusquedaCambioUbicacion.SelectedIndex = -1;

            //Hago la busqueda de registros para actualizar el listado
            BuscarRegistrosCambioUbicaciones();
            BuscarRegistrosCambioClasificacion();
            //Actualizar_UbicacionDisponible();
        }

        #endregion

        #region Cambio Clasificacion

        private void OnBuscarRegistrosCambioClasificacion(object sender, EventArgs e)
        {
            //Busco los registros.
            BuscarRegistrosCambioClasificacion();
        }

        public void BuscarRegistrosCambioClasificacion()
        {
            // Variables Auxiliares.
            string ConsultaSQL;
            string num_pallet, Modelo, Posicion, aux_FechaIngreso;
            DateTime fechaIngreso;
            
            // Asignacion de variables.
            aux_FechaIngreso = View.GetFechaIngresoCambioClasificacion.Text;
            if (aux_FechaIngreso != "")
            {
                // Si se filtro por la fecha le daremos un formato para la busqueda en la base de datos. 
                fechaIngreso = DateTime.Parse(aux_FechaIngreso);
                String DateFormat = "dd/MM/yyyy";
                aux_FechaIngreso = fechaIngreso.ToString(DateFormat);
            }

            num_pallet = View.BuscarEstibaCambioClasificacion.Text;
            Posicion = View.BuscarPosicionCambioClasificacion.Text;
            Modelo = View.BuscarModeloCambioClasificacion.Text;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAUBICACION', '" + num_pallet + "', '" + Posicion + "', '" + Modelo + "', '" + aux_FechaIngreso + "'";
            //Ejecuto la consulta
            View.Model.ListadoCambioClasificacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
            // Limpiamos el formulario para un nuevo filtro. 
            limpiarFormularioCambioClasificacion();
        }

        protected void limpiarFormularioCambioClasificacion()
        {
            // Limpio los campos de la busqueda.
            View.BuscarEstibaCambioClasificacion.Text = "";
            View.BuscarPosicionCambioClasificacion.SelectedIndex = -1;
            View.BuscarModeloCambioClasificacion.SelectedIndex = -1;
            View.GetFechaIngresoCambioClasificacion.Text = "";
            View.TextoClasificacionActual.Text = "";
        }

        private void OnActualizarRegistrosCambioClasificacion(object sender, EventArgs e) { }

        private void OnHabilitarCambioClasificacion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
                return;

            string aux_idPallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["idpallet"].ToString();

            View.Model.Listado_PalletSerial = 
            service.DirectSQLQuery("SELECT IdPallet as PPallet, "
            + "serial as PSerial, "
            + "RECEIVER as Receiver, "
            + "SMART_CARD_ENTRADA as PMac,  "
            + "MODELO as Modelo,  "
            + "TIPO_ORIGEN as PTRecibo, "
            + "convert(VARCHAR,FECHA_INGRESO,120) as PFRegistro,  "
            + "DATEDIFF(day, FECHA_INGRESO,GETDATE()) as NumeroDias,dbo.TIMELAPSELEO(FECHA_INGRESO) as horas "
            + "from dbo.EquiposDIRECTVC WHERE ((IdPallet IS NOT NULL) AND (Posicion IS NOT NULL) AND (ESTADO = 'ALMACENAMIENTO')) "
            + " AND IdPallet = '" + aux_idPallet +"'", "", "dbo.EquiposDIRECTVC", Local);

            //Coloco visible el panel para seleccionar el cambio de clasificacion
            View.StackCambioClasificacion.Visibility = Visibility.Visible;

            //Coloco la clasificacion del registro seleccionado en el campo de clasificacion actual
            View.TextoClasificacionActual.Text = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Ubicacion"].ToString();


            int total = 0;
            int num_estibas = View.ListadoBusquedaCambioClasificacion.SelectedItems.Count;

            foreach (DataRowView registros in View.ListadoBusquedaCambioClasificacion.SelectedItems)
            {
                total = total + Int32.Parse(registros.Row["Cantidad"].ToString());
            }

            View.TotalSeriales.Text = total.ToString();
            View.estibasSeleccionadas.Text = num_estibas.ToString();
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
                    if (fila.Row["Estado"].ToString() == "CUARENTENA")
                    {
                        Util.ShowError("No es posible enviar pallet en CUARENTENA para REPARACION");
                        return;
                    }
                    else
                    {
                        //Creo la consulta la cambiar la clasificacion de la estiba
                        ConsultaSQL = "UPDATE dbo.EquiposDIRECTVC SET Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "', Posicion = NULL, CodigoEmpaque = '" + fila.Row["idPallet"].ToString() + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "' AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";
                        ConsultaTrack = "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "';";
                        //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                        ConsultaMovSQL = "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'ENVIO A PRODUCCIÓN " + NuevoEstado + "','ALMACENAMIENTO','REPARACIÓN','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";

                        //Ejecuto la consulta
                        service.DirectSQLNonQuery(ConsultaSQL, Local);
                        service.DirectSQLNonQuery(ConsultaTrack, Local);
                        service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    }
                }
                else if (NuevaUbicacion == "TRANSITO A DIAG.")
                {
                    NuevoEstado = "PARA DIAGNOSTICO";
                    if (fila.Row["Estado"].ToString() == "CUARENTENA")
                    {
                        Util.ShowError("No es posible enviar pallet en CUARENTENA para DIAGNOSTICO");
                        return;
                    }
                    else
                    {
                        //Creo la consulta la cambiar la clasificacion de la estiba
                        ConsultaSQL = "UPDATE dbo.EquiposDIRECTVC SET Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "' AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";
                        ConsultaTrack = "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "';";
                        ConsultaMovSQL = "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'ENVIO A PRODUCCIÓN " + NuevoEstado + "','ALMACENAMIENTO','DIAGNOSTICO','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";
                        //Ejecuto la consulta
                        service.DirectSQLNonQuery(ConsultaSQL, Local);
                        service.DirectSQLNonQuery(ConsultaTrack, Local);
                        service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    }
                }
                else
                {
                    NuevoEstado = "DESPACHO";
                    if (fila.Row["Estado"].ToString() == "CUARENTENA")
                    {
                        Util.ShowError("No es posible enviar pallet en CUARENTENA para DESPACHO");
                        return;
                    }
                    else { 
                    //Creo la consulta la cambiar la clasificacion de la estiba
                    ConsultaSQL = "UPDATE dbo.equiposDIRECTVC SET CodigoEmpaque2 = IdPallet, Estado = '" + NuevoEstado + "', Ubicacion = '" + NuevaUbicacion + "' WHERE IdPallet = '" + fila.Row["idPallet"].ToString() + "'  AND ESTADO = 'ALMACENAMIENTO' AND ESTADO != 'DESPACHADO';";
                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaMovSQL = "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'ENVIO A " + NuevoEstado + "','ALMACENAMIENTO','DESPACHO','" + fila.Row["idPallet"].ToString() + "','','ALMACENAMIENTO','UBICACIONALMACEN','" + this.user + "','';";
                    ConsultaTrack = "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_MOVMERCANCIA = '" + NuevoEstado + "' WHERE ESTIBA_ENTRADA = '" + fila.Row["idPallet"].ToString() + "';";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaMovSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                    }
                }
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de clasificacion realizado satisfactoriamente.");

            ////Ejecuto la consulta
            //service.DirectSQLNonQuery(ConsultaSQL, Local);

            ////Muestro el mensaje de confirmacion
            //Util.ShowMessage("Cambio de clasificacion realizado satisfactoriamente.");

            //Hago la busqueda de registros para actualizar el listado
            BuscarRegistrosCambioClasificacion();

            //Oculto el bloque de actualizacion
            View.StackCambioClasificacion.Visibility = Visibility.Collapsed;

            //Quito la selecion de la nueva ubicacion
            View.NuevaClasificacion.SelectedIndex = -1;

            Actualizar_UbicacionDisponible();

            //Quito la seleccion del listado
            View.ListadoBusquedaCambioClasificacion.SelectedIndex = -1;
            View.Model.Listado_PalletSerial.Rows.Clear();
            View.TotalSeriales.Text = "0";
            View.estibasSeleccionadas.Text = "0";
        }

        private void OnImprimirRegistros(object sender, EventArgs e) 
        {
            // Variables auxiliares.
            string consultaSQL = "";
            string nuevaUbicacion = "", nuevoEstado;

            nuevaUbicacion = ((DataRowView)View.NuevaClasificacion.SelectedItem).Row["UbicacionDestino"].ToString();

            if (nuevaUbicacion == "TRANSITO A REP.")
            {
                nuevoEstado = "ALMACENAMIENTO - REPARACIÓN";
            }
            else if (nuevaUbicacion == "TRANSITO A DIAG.")
            {
                nuevoEstado = "ALMACENAMIENTO - DIAGNOSTICO";
            }
            else
            {
                nuevoEstado = "ALMACENAMIENTO - DESPACHO";
            }

            // Evaluo que haya sido seleccionado un registro.

            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar uno o más pallets");
                return;
            }

            // Evaluo que haya seleccionado la nueva clasificación.
            if (View.NuevaClasificacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificación.");
                return;
            }

            ListView serialesImprimir = View.ListadoSerialesCambioClasificacion;

            string transito = nuevoEstado;
            string ubicacion = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Ubicacion"].ToString();
            string cantidad = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Cantidad"].ToString();
            string fechaIngreso = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["FechaIngreso"].ToString();
            string idpallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["IdPallet"].ToString();
            string dias = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["NumeroDias"].ToString();
            string tiempoTranscurrido = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Horas"].ToString();

            // Imprimo los registros
            PrinterControl.PrintMovimientosMercanciaDIRECTV(transito, ubicacion, cantidad, fechaIngreso, idpallet, dias, tiempoTranscurrido, serialesImprimir);
        }

        private void OnExportPalletSeleccion(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Workbook wb = null;

            object missing = Type.Missing;
            Worksheet ws = null;
            Range rng = null;

            try
            {
                int filas_seleccion = View.ListadoBusquedaCambioClasificacion.SelectedItems.Count;

                if (filas_seleccion > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int idx = 0; idx < View.GridViewListaClasificacion.Columns.Count; idx++)
                    {
                        ws.Range["A1"].Offset[0, idx].Value = View.GridViewListaClasificacion.Columns[idx].Header.ToString();
                        ws.Range["A1"].Offset[0, idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Orange);
                    }
                    int count = 0;

                    foreach (DataRowView registros in View.ListadoBusquedaCambioClasificacion.SelectedItems)
                    {
                        ws.get_Range("A1", "H" + count + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[count].Resize[1, View.GridViewListaClasificacion.Columns.Count].Value =
                            registros.Row.ItemArray;
                        count++;
                    }
                    rng = ws.get_Range("A1", "H" + count + 1);
                    rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    rng.Columns.AutoFit();

                    excel.Visible = true;
                    wb.Activate();
                }
                else
                {
                    Util.ShowMessage("Debe seleccionar uno o varios Pallets para la generación del archivo.");
                }
            }
            catch (Exception ex )
            {
                Util.ShowMessage("Error al crear el archivo en Excel: " + ex.ToString());
            }
            
        }

        private void OnExportSerialesSeleccion(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Workbook wb = null;

            object missing = Type.Missing;
            Worksheet ws = null;
            Range rng = null;

            try
            {
                int filas_Seleccion = View.ListadoBusquedaCambioClasificacion.SelectedItems.Count;

                if (filas_Seleccion > 0 )
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
                else 
                {
                    Util.ShowMessage("Debe seleccionar uno o varios seriales para generar el archivo");
                }

            }
            catch (Exception ex)
            {
                Util.ShowMessage("Problema al exportar la información a Excel: " + ex.ToString());
            }
        }

        private void OnFilaSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            int total = 0;
            int num_estibas = View.ListadoBusquedaCambioUbicacion.SelectedItems.Count;

            foreach (DataRowView registros in View.ListadoBusquedaCambioUbicacion.SelectedItems)
            {
                total = total + Int32.Parse(registros.Row["cantidad"].ToString());
            }

            View.TotalSeriales.Text = total.ToString();
            View.estibasSeleccionadas.Text = num_estibas.ToString();
        }
        #endregion
    }
}