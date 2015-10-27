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

namespace WpfFront.Presenters
{

    public interface IReparacionesPresenter
    {
        IReparacionesView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ReparacionesPresenter : IReparacionesPresenter
    {
        public IReparacionesView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        private string Formato_fecha = "dd/MM/yyyy";
        private String SerialReparacionH;
        private String MacReparacionH;

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String SerialAsignacion;

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public ReparacionesPresenter(IUnityContainer container, IReparacionesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ReparacionesModel>();

            #region Metodos

            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.CargarDatosReparacion += new EventHandler<EventArgs>(this.CargarDatosReparacion);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);
            View.HabilitarUbicacion += new EventHandler<SelectionChangedEventArgs>(this.OnHabilitarUbicacion);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;
            View.FilaSeleccionada += this.OnFilaSeleccionada;

            //Asignacion
            View.BuscarRegistrosAsignacion += this.OnBuscarRegistrosAsignacion;
            View.ActualizarRegistrosAsignacion += this.OnActualizarRegistrosAsignacion;
            View.ListarEquiposEstibas += this.OnListarEquiposEstibas;
            View.MostrarTecnicosEstibas += this.OnMostrarTecnicosEstibas;
            View.ConfirmarTecnicoEquipo += this.OnConfirmarTecnicoEquipo;
            View.ConsultaReparacionAnterior += new EventHandler<EventArgs>(this.OnConsultaReparacionAnterior);
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            View.AddToList += new EventHandler<EventArgs>(this.OnAddToList);
            View.RemoveSelection += new EventHandler<EventArgs>(this.OnRemoveSelection);
            View.HabilitarMotivo += new EventHandler<EventArgs>(this.OnHabilitarMotivo);
            View.CargarHistorico += new EventHandler<EventArgs>(this.CargarHistorico);
            View.BuscarEquiposPorTecnico += new EventHandler<MouseButtonEventArgs>(this.OnBuscarEquiposPorTecnico);
            View.BuscarEquiposPorTecnicoEntrega += new EventHandler<EventArgs>(this.OnBuscarEquiposPorTecnicoEntrega);
            View.ConsultarTecnicos += new EventHandler<EventArgs>(this.OnGetListTecnicos);
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'REPARACION', 'CLARO'", "", "dbo.Ubicaciones", Local);
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            CargarDatosDetails();
            View.Model.ListRecordsAddToPallet = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARMERCANCIAENTREGAREP', '', '',''", "", "dbo.EquiposClaro", Local);

            //Cargo los tecnicos
            View.Model.ListadoTecnicos = service.GetSysUser (new SysUser());
            View.Model.ListadoTecnicos = View.Model.ListadoTecnicos.OrderBy(x => x.UserName).ToList();

            this.Actualizar_UbicacionDisponible();

            if (App.curUser.UserRols.Where(f => f.Rol.RolCode == "ADMIN" || f.Rol.RolCode == "CLARODIAG").Count() == 0)
            {
                View.GetTabEntrega.IsEnabled = false;
                if (this.userName == "GLUGO" || this.userName == "JHERNANDEZ")
                {
                    View.GetTabEntrega.IsEnabled = true;
                }
            }

            view.StackProcesoReparacion.IsEnabled = false;

            #endregion
        }

   
        #region Metodos

        private void OnGetListTecnicos(object sender, EventArgs e)
        {
            CargarTecnicosReparacion();
        }

        private void OnBuscarEquiposPorTecnicoEntrega(object sender, EventArgs e)
        {
            BuscarEquiposPorTecnicoEntrega();
        }

        private void BuscarEquiposPorTecnicoEntrega()
        {
            //Variables Auxiliares
            String ConsultaSQL;
            string tipoEstado = ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString();
            string tecnico = ((DataRowView)View.TecnicosReparacion.SelectedItem).Row[0].ToString();

            if (tipoEstado.Equals("REPARADO"))
            {
                tipoEstado = "REP";
            }

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAFILTRADA_REP', '" + tipoEstado + "','" + tecnico + "';";

            try
            {
                //Ejecuto la consulta
                View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposClaro", Local);
            }
            catch (Exception ex)
            {
                Util.ShowError("Error desconocido: " + ex.Message.ToString());
            }

            if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString().Contains("REP"))
            {
                View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONFILTRADA', 'REPARACION', 'CLARO', 'ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
            }
            else
            {
                View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONFILTRADA', 'REPARACION', 'CLARO', 'DIAGNOSTICO', 'ALMACENAMIENTO' ", "", "dbo.Ubicaciones", Local);
                View.StackUbicacion.Visibility = Visibility.Collapsed;
            }
            int equiposCargados = View.Model.ListRecords_1.Rows.Count;
            if (equiposCargados < 1)
            {
                View.TXT_filterResults.Text = "No se encontraron equipos reparados por el usuario " + tecnico.ToUpper();
            }
            else if (equiposCargados == 1)
            {
                View.TXT_filterResults.Text = equiposCargados.ToString() + " equipo fue reparado por el usuario " + tecnico.ToUpper();
            }
            else
            {
                View.TXT_filterResults.Text = equiposCargados + " equipos reparados por el usuario " + tecnico.ToUpper();
            }

            View.TXT_filterResults.Visibility = Visibility.Visible;
            View.ListadoItems.Visibility = Visibility.Visible;
            View.StackListaEquiposEntrega.Visibility = Visibility.Visible;
        }

        private void OnBuscarEquiposPorTecnico(object sender, MouseButtonEventArgs e)
        {
            string currentUser = this.user.ToUpper().ToString();
            View.txt_User.Text = "Equipos reparados por el usuario: " + currentUser;
            string ConsultaSQL = "EXEC dbo.sp_GetProcesos 'BUSCAREQUIPOSPORTECNICO', '" + currentUser + "'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposClaro", Local);
            View.Model.ListEquiposReparadosByUser = Resultado;
        }

        private void OnConfirmarImpresion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String unidad_almacenamiento = "";
            String codigoEmp = View.CodigoEmpaque.Text;
            String destino = "";
            String tipo_listado = "";
            String estado = "";


            if (View.Model.ListRecordsAddToPallet.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para empacar");
                return;
            }
            if (View.GetListaEstado.SelectedIndex == -1)
            {
                Util.ShowMessage("Por favor seleccionar un estado para los equipos que desea empacar o imprimir, los estados son SCRAP, REPARADO, ETIQUETA ERRONEA.");
                return;
            }

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }
            else
            {
                destino = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
            }

            if (String.Compare("", codigoEmp) == 0)
            {
                Util.ShowError("Por favor generar un código de empaque");
                return;
            }
           

            //Creo la base de la consulta para traer los seriales respectivos
            ConsultaSQL = "SELECT idPallet,Posicion,serial,Mac,Codigo_SAP,Fecha_ingreso,ProductoID FROM dbo.EquiposCLARO WHERE serial IN (''";

            //Recorro el listado de registros seleccionados para obtener los seriales e imprimirlos
            //foreach (DataRowView Registros in View.ListadoItems.SelectedItems)
            foreach (DataRowView Registros in View.ListadoItemsAgregados.Items)
            {
                //Util.ShowMessage(Registros.Row["Serial"].ToString());
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += ",'" + Registros["Serial"] + "'";
            }

            //Completo la consulta
            ConsultaSQL += ")";

            //Elimino la basura en la cadena
            ConsultaSQL = ConsultaSQL.Replace("'',", "");

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            if (View.UnidadAlmacenamiento.SelectedIndex != -1)
            {
                unidad_almacenamiento = ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString();
            }
            else
            {
                Util.ShowError("Selecciona una unidad de empaque");
                return;
            }

            string tipoEstado = ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString();
            if (tipoEstado.Contains("REP") == false)
            {
                tipo_listado = "SCRAP";
                estado = "PARA SCRAP";
            }
            else
            {
                tipo_listado = "REPARADOS";
                estado = "REPARADOS";
            }
            if (tipoEstado.Contains("ERRÓNEA"))
            {
                tipo_listado = "ETIQUETA ERRONEA";
                estado = "ETIQUETA ERRONEA";
            }
            

            //Imprimo los registros
            PrinterControl.PrintMovimientosBodega(this.userName, SerialesImprimir, unidad_almacenamiento, codigoEmp, estado, "CLARO", "REPARACIÓN - " + destino, tipo_listado, "AUX");
        }

        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                DataTable dt_auxiliar = service.DirectSQLQuery("select posicion from dbo.EquiposCLARO where posicion is not null AND estado='ALMACENAMIENTO' group by posicion ", "", "dbo.EquiposCLARO", Local);

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
                View.NuevaUbicacion.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.ToString());
            }
        }

        public void CargarDatosDetails()
        {
            //Variables Auxiliares
            //GridViewColumn Columna;
            //FrameworkElementFactory Txt;
            //Assembly assembly;
            //string TipoDato;

            //Inicializo el DataTable

            View.Model.ListadoFallaDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLATEC" } });
            View.Model.ListadoMotivos = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROSCRAP" } });
            View.Model.ListadoEstadoFinal = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROESTAD" } });


            View.Model.ListRecords = new DataTable("ListadoRegistros");
            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("IdPallet", typeof(String));
            View.Model.ListRecords.Columns.Add("Tecnico", typeof(String));

            View.Model.ListRecords_1 = new DataTable("ListadoEquiposEntrega");
            //Asigno las columnas
            
            View.Model.ListRecords_1.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords_1.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords_1.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords_1.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords_1.Columns.Add("FECHA", typeof(String));
            View.Model.ListRecords_1.Columns.Add("TIEMPOREP", typeof(String));
            View.Model.ListRecords_1.Columns.Add("Tecnico", typeof(String));
            View.Model.ListRecords_1.Columns.Add("CAJA", typeof(String));
            View.Model.ListRecords_1.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords_1.Columns.Add("Usuario", typeof(String));


            View.Model.ListRecordsAddToPallet = new DataTable("ListadoAddToPallet");
            //Asigno las columnas
            View.Model.ListRecordsAddToPallet.Columns.Add("Producto", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("Serial", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("Mac", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("Estado", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("FECHA", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("TIEMPOREP", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("Tecnico", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("CAJA", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("RowID", typeof(String));
            View.Model.ListRecordsAddToPallet.Columns.Add("Usuario", typeof(String));

            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Ubicaciones";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoPosiciones);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            ////Txt.SetValue(ComboBox.SelectedValuePathProperty, "MetaMasterID");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Posicion"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("Posicion", typeof(String)); //Creacion de la columna en el DataTable
        }

        private void OnHabilitarUbicacion(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string NuevaUbicacion = ((DataRowView)(sender as ComboBox).SelectedItem).Row["UbicacionDestino"].ToString();

                if (NuevaUbicacion == "ALMACENAMIENTO")
                {
                    this.Actualizar_UbicacionDisponible();
                    //View.StackUbicacion.Visibility = Visibility.Visible;
                }
                else
                {
                    View.StackUbicacion.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
        }

        private void CargarDatosReparacion(object sender, EventArgs e)
        {
            //Validacion existe o no el equipo en DB
            String ConsultaBuscar = "SELECT Serial FROM dbo.EquiposCLARO WHERE Serial = '" + View.GetSerial1.Text.ToString() + "'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

            if (Resultado.Rows.Count == 0)
            {
                Util.ShowError("El serial no se encuentra registrado en el sistema.");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
                return;
            }

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                if (String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
                {
                    Util.ShowError("Los campos no pueden estar vacios.");
                    return;
                }
            }

            DataTable Consulta;
            Consulta = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARPRODUCTO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposClaro", Local);

            if (Consulta.Rows[0]["Estado"].ToString() != "PARA PROCESO")
            {
                Util.ShowError("El serial ingresado no esta en PROCESAMIENTO DE REPARACION, El serial se encuentra en estado " + Consulta.Rows[0]["Estado"].ToString() + "");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
                return;
            }
            else
            {
               
                string TecnicoAsignado = Consulta.Rows[0]["Tecnico"].ToString().ToUpper();
                if (TecnicoAsignado.Contains(","))
                {
                    string[] split = TecnicoAsignado.Split(new Char[] { ',' });
                    TecnicoAsignado = split.First();
                }
                String UserTecnicoSesion = this.userName.ToUpper();

                if ((UserTecnicoSesion == TecnicoAsignado))
                {
                    View.StackProcesoReparacion.IsEnabled = true;
                    View.ProductoProcesamiento.Text = Consulta.Rows[0]["Producto"].ToString();
                    View.FallaProcesamiento.Text = Consulta.Rows[0]["Falla"].ToString();
                    View.TecnicoAsignado.Text = Consulta.Rows[0]["Tecnico"].ToString();
                    View.TecnicoDiagnosticador.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                }
                else
                {
                    Util.ShowError("No tiene permisos para realizar esta accion, tecnico asignado a este serial: " + TecnicoAsignado + "");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }
        }

        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";
            SerialAsignacion = View.GetSerialAsignacion.Text.ToString();
            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerialAsignacion.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            foreach (DataRow item in View.Model.ListRecords.Rows)
            {
                if (View.GetSerialAsignacion.Text == item["Serial"].ToString())
                {
                    Util.ShowError("El serial " + SerialAsignacion + " ya esta en el listado.");
                    View.GetSerialAsignacion.Text = "";
                    View.GetMacAsignacion.Text = "";
                    View.GetSerialAsignacion.Focus();
                    return;
                }
            }

            try
            {
                //Validacion existe o no el equipo en DB
                ConsultaBuscar = "SELECT Serial FROM dbo.EquiposCLARO WHERE Serial = '" + SerialAsignacion + "'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);


                if (Resultado.Rows.Count > 0)
                {
                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERALCARGAR','" + SerialAsignacion + "'", "", "dbo.EquiposCLARO", Local);

                    //Evaluo si el serial existe

                    if (RegistroValidado.Rows.Count == 0)
                    {
                        Util.ShowError("El serial no existe o no esta en la ubicacion requerida.");
                        View.GetSerialAsignacion.Text = "";
                        View.GetMacAsignacion.Text = "";
                        View.GetSerialAsignacion.Focus();
                        return;
                    }
                    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "PARA ASIGNACION")
                    {
                        Util.ShowError("El serial ingresado no esta en Reparacion, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerialAsignacion.Text = "";
                        View.GetMacAsignacion.Text = "";
                        View.GetSerialAsignacion.Focus();
                        return;
                    }
                    else
                    {
                        OnConsultaReparacionAnterior(sender, e);
                        String validar = "EXEC sp_GetProcesos 'BUSCARUSUARIORECIBOREP', '" + SerialAsignacion + "', '" + this.user + "', NULL";
                        DataTable Garantia = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERALREPARACION','" + SerialAsignacion + "'", "", "dbo.EquiposCLARO", Local);

                        DataTable ValidarUsuario = service.DirectSQLQuery(validar, "", "dbo.EquiposCLARO", Local);

                        if (ValidarUsuario.Rows.Count > 0)
                        {
                            if (Garantia.Rows.Count > 0)
                            {
                                String tecnico = Garantia.Rows[0]["TecnicoRep"].ToString();
                                String falla = Garantia.Rows[0]["FallaDiag"].ToString();
                                String fecha = Garantia.Rows[0]["FechaRep"].ToString();
                                //int TiempoGarantia = Convert.ToInt32(Garantia.Rows[0]["NumeroDias"].ToString());
                                //String EstadoReparacion = Garantia.Rows[0]["EstadoReparacion"].ToString();

                                //if (TiempoGarantia < 180 && EstadoReparacion.Equals("REPARADO"))
                                //{
                                //    Util.ShowMessage("REING. REPARACION");
                                    
                                //}
                                //else if (TiempoGarantia < 180 && EstadoReparacion.Equals("SCRAP"))
                                //{
                                //    Util.ShowMessage("REING. SCRAP");
                                //}

                                if (UtilWindow.ConfirmOK("El equipo entra como garantia. \nTecnico anterior "
                                                        + tecnico + " \nFalla: "
                                                        + falla + "\nFecha ultima reparación: "
                                                        + fecha + /*TiempoGarantia +EstadoReparacion +*/  " \n\n ¿Desea asignar el equipo ahora?") == false)
                                {
                                    View.TecnicoReparador.Text = "";
                                    View.FallaAnteriorDiag.Text = "";
                                    View.FechaReparacion.Text = "";
                                    return;
                                }
                            }
                            //Asigno los campos
                            dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                            dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                            dr["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString();
                            dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();

                            //Agrego el registro al listado
                            View.Model.ListRecords.Rows.Add(dr);

                            var border = (Border)VisualTreeHelper.GetChild(View.ListadoItemsAsignacion, 0);
                            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                            scrollViewer.ScrollToBottom();

                            //Limpio los seriales para digitar nuevos datos
                            View.GetSerialAsignacion.Text = "";
                            View.GetMacAsignacion.Text = "";
                            View.GetSerialAsignacion.Focus();
                        }
                        else
                        {
                            Util.ShowMessage("No puede asignar este equipo por que fue recibido por otro usuario");
                            View.GetSerialAsignacion.Text = "";
                            View.GetMacAsignacion.Text = "";
                            View.GetSerialAsignacion.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    Util.ShowError("El serial no ha sido registrado en el sistema.");
                    View.GetSerialAsignacion.Text = "";
                    View.GetMacAsignacion.Text = "";
                    View.GetSerialAsignacion.Focus();
                    return;
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
                View.GetSerialAsignacion.Text = "";
                View.GetMacAsignacion.Text = "";
                View.GetSerialAsignacion.Focus();
                return;
            }
        }

        private void OnGenerarCodigo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaBuscar;
            DataTable RegistroValidado;

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
                    View.CodigoEmpaque.Text = "RES-R" + Resultado.Rows[0]["idpallet"].ToString();
                }
            }
            else
            {
                Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
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
            //Variables Auxiliares
            string ConsultaGuardar = "";
            string ConsultaGuardarTrack = "";
            string FallaRep = "";
            string Falla1 = "";
            string Falla2 = "";
            string Falla3 = "";
            string Falla4 = "";
            string EstatusRep = "";
            string PartesCambiadas = "";
            string MotivoScrap = "";

            try
            {
                //----------------------------------
                if ((View.FallaReparacionAdic5.Text.ToString() == null) || (View.FallaReparacionAdic5.Text.ToString() == ""))
                {
                    Util.ShowError("Por favor ingresar el estado de reparación");
                    return;
                }
                else
                {
                    EstatusRep = View.FallaReparacionAdic5.Text.ToString();
                }

                if (!((MMaster)View.FallaReparacionAdic5.SelectedItem).Name.ToString().Contains("SCRAP"))
                {
                    if (((MMaster)View.FallaReparacion.SelectedItem == null))
                    {
                        Util.ShowError("Por favor ingresar la falla de reparacion");
                        return;
                    }
                    else
                    { FallaRep = ((MMaster)View.FallaReparacion.SelectedItem).Code.ToString(); }

                    //----------------------------------
                    if (((MMaster)View.FallaReparacionAdic.SelectedItem == null))
                    { Falla1 = "NULL"; }
                    else
                    { Falla1 = ((MMaster)View.FallaReparacionAdic.SelectedItem).Code.ToString(); }

                    //----------------------------------
                    if (((MMaster)View.FallaReparacionAdic2.SelectedItem == null))
                    { Falla2 = "NULL"; }
                    else
                    { Falla2 = ((MMaster)View.FallaReparacionAdic2.SelectedItem).Code.ToString(); }

                    //----------------------------------
                    if (((MMaster)View.FallaReparacionAdic3.SelectedItem == null))
                    { Falla3 = "NULL"; }
                    else
                    { Falla3 = ((MMaster)View.FallaReparacionAdic3.SelectedItem).Code.ToString(); }

                    //----------------------------------
                    if (((MMaster)View.FallaReparacionAdic4.SelectedItem == null))
                    { Falla4 = "NULL"; }
                    else
                    { Falla4 = ((MMaster)View.FallaReparacionAdic4.SelectedItem).Code.ToString(); }

                    //----------------------------------
                    if ((View.FallaReparacionAdic5.Text.ToString() == null) || (View.FallaReparacionAdic5.Text.ToString() == ""))
                    {
                        Util.ShowError("Por favor ingresar el estado de reparacion");
                        return;
                    }
                    else
                    {
                        EstatusRep = View.FallaReparacionAdic5.Text.ToString();
                    }

                    //----------------------------------
                    if (View.PartesCambiadas.Text == "")
                    {
                        Util.ShowError("Por favor ingresar la descripcion de las partes cambiadas");
                        return;
                    }
                    else
                    { PartesCambiadas = View.PartesCambiadas.Text.ToString(); }

                    if (View.GetNroCaja.Text == "")
                    {
                        Util.ShowError("Por favor ingresar un número de caja");
                        return;
                    }
                    //----------------------------------
                }
                else
                {
                    if (View.MotivoSCRAP.Text == "" || View.MotivoSCRAP.Text == null)
                    {
                        Util.ShowError("Por favor ingresar el motivo de SCRAP");
                        return;
                    }
                    else
                    {
                        if (View.GetNroCaja.Text == "")
                        {
                            Util.ShowError("Por favor ingresar un número de caja");
                            return;
                        }
                        else
                        {
                            if (View.PartesCambiadas.Text == "")
                            {
                                Util.ShowError("Por favor ingresar la descripcion de las partes cambiadas");
                                return;
                            }
                            else
                            {
                                MotivoScrap = View.MotivoSCRAP.Text.ToString();
                            }
                        }
                    }
                }

                //Construyo la consulta para guardar los datos
                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'REPARACION', Estado = 'REPARACION',";
                ConsultaGuardar += " FALLA_REP = '" + FallaRep + "', FALLA_REP1 = '" + Falla1 + "', FALLA_REP2 = '" + Falla2 + "'";
                ConsultaGuardar += ", FALLA_REP3 = '" + Falla3 + "', FALLA_REP4 = '" + Falla4 + "', MOTIVO_SCRAP= '" + MotivoScrap + "'";
                ConsultaGuardar += ", ESTATUS_REPARACION = '" + EstatusRep + "', PARTES_CAMBIADAS = '" + PartesCambiadas + "'  WHERE Serial = '" + View.GetSerial1.Text.ToString() + "' AND Estado != 'DESPACHADO';";

                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_REPARACION = '" + EstatusRep + "', FECHA_REPARADO = getdate() WHERE SERIAL = '" + View.GetSerial1.Text.ToString() + "';";

                ConsultaGuardar += "EXEC dbo.sp_InsertarNuevo_MovimientoReparacion 'REPARACIÓN TERMINADA','REPARACIÓN','REPARACIÓN','Sin pallet','" + View.GetSerial1.Text.ToString() +
                    "','" + View.TecnicoAsignado.Text + "','" + FallaRep + "','" + Falla1 + "','" + Falla2 + "','" + Falla3 + "','" + Falla4 + "','" + EstatusRep +
                    "','" + PartesCambiadas + "','" + ((MotivoScrap != "NULL") ? MotivoScrap : "") + "','" + this.user + "','" + View.GetNroCaja.Text + "';";


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

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                LimpiarDatosIngresoSeriales();

                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";

                View.MotivoSCRAP.Text = "";
                View.FallaReparacionAdic5.Text = "";

                View.Border_ListaHP.Visibility = Visibility.Collapsed;

                View.GetSerial1.Focus();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            string ConsultaSQL = "", ConsultaTrack = "", ConsultaMov = "", NuevaUbicacion, NuevoEstado;
            string UnidadAlmacenamiento = "";
            string idPallet = "";
            string RowID = "";

            if (String.IsNullOrEmpty(View.CodigoEmpaque.Text.ToString()))
            {
                Util.ShowMessage("Por favor generar un código de estiba.");
                return;
            }

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowMessage("Por favor seleccionar la nueva clasificacion.");
                return;
            }
            // Evaluo si hay registros para empacar.
            if (View.Model.ListRecordsAddToPallet.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para empacar.");
                return;
            }

            //Coloco la ubicación
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
            UnidadAlmacenamiento = ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString();
            idPallet = View.CodigoEmpaque.Text.ToString();
            try
            {
                if (NuevaUbicacion == "DIAGNOSTICO")
                {
                    NuevoEstado = "PARA DIAGNOSTICO";
                    ConsultaSQL = "UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" + UnidadAlmacenamiento + "', IdPallet = '" + idPallet + "' WHERE RowID IN (";
                    ConsultaTrack = "UPDATE dbo.TrackEquiosCLARO SET CODEMPAQUE_REP = '" + idPallet + "' WHERE ID_SERIAL IN (";
                    
                    foreach (DataRowView item in View.ListadoItemsAgregados.Items)
                    {
                        // Almaceno el rowID de cada serial en la base de datos
                        RowID = item.Row["RowID"].ToString();

                        //Creo la consulta para cambiar la ubicacion de la estiba
                        ConsultaSQL += "'" + RowID + "',";
                        ConsultaTrack += "'" + RowID + "',";
                        ConsultaMov += "EXEC dbo.sp_InsertarNuevo_Movimiento 'EMPAQUE REPARACIÓN','REPARACIÓN','DIAGNOSTICO','" + idPallet + "','" + RowID + "','REPARACION','UBICACIONPRODUCCION','" + this.user + "','';";
                    }

                    ConsultaSQL += ");";
                    ConsultaTrack += ");";
                    ConsultaSQL = ConsultaSQL.Replace(",);", ");");
                    ConsultaTrack = ConsultaTrack.Replace(",);", ");");

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                    service.DirectSQLNonQuery(ConsultaMov, Local);

                    ConsultaSQL = "";
                    ConsultaTrack = "";
                    ConsultaMov = "";
                }
                else // Sino es DIAGNOSTICO Es ALMACENAMIENTO Y EL EQUIPO PUEDE SER SCRAP O REPARADO
                {
                    NuevoEstado = "PARA ALMACENAMIENTO";
                    ConsultaSQL += " UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" + UnidadAlmacenamiento + "', CodigoEmpaque = '" + idPallet + "' WHERE RowID IN (";
                    ConsultaTrack += "UPDATE dbo.TrackEquiposCLARO SET CODEMPAQUE_REP = '" + idPallet + "' WHERE ID_SERIAL IN (";
                    
                    foreach (DataRowView item in View.ListadoItemsAgregados.Items)
                    {
                        // Almaceno el rowID de cada serial en la base de datos
                        RowID = item.Row["RowID"].ToString();

                        //Creo la consulta para cambiar la ubicacion de la estiba
                        ConsultaSQL += "'" + RowID + "',";
                        ConsultaTrack += "'" + RowID + "',";
                        ConsultaMov += "EXEC dbo.sp_InsertarNuevo_Movimiento 'EMPAQUE REPARACIÓN','REPARACIÓN','ALMACENAMIENTO','" + idPallet + "','" + RowID + "','REPARACION','UBICACIONPRODUCCION','" + this.user + "','';";
                    }
                    ConsultaSQL += ");";
                    ConsultaTrack += ");";
                    ConsultaSQL = ConsultaSQL.Replace(",);", ");");
                    ConsultaTrack = ConsultaTrack.Replace(",);", ");");

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                    service.DirectSQLNonQuery(ConsultaMov, Local);

                    ConsultaSQL = "";
                    ConsultaTrack = "";
                    ConsultaMov = "";
                }
            }// EndTry
            catch (Exception ex)
            {
                string Ubicacion = "REPARACION - ENTREGA";
                string Usuario = this.userName;
                string codigoError = ex.HResult.ToString();
                string errorMessage = ex.Message.ToString() ;
                string errorTrackMessage = ex.StackTrace.ToString() ;
                service.DirectSQLNonQuery("INSERT INTO dbo.LogProError VALUES ('"+ Ubicacion +"', '"+ Usuario +"', '" + codigoError + "', '"+ errorTrackMessage +"', GETDATE())", Local);
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicación realizado satisfactoriamente.");

            //Quito la selecion de la nueva ubicacion
            View.Ubicacion.SelectedIndex = -1;
            View.Model.ListRecordsAddToPallet.Rows.Clear();

            //Quito la seleccion del listado
            View.UnidadAlmacenamiento.SelectedIndex = -1;
            View.CodigoEmpaque.Text = "";
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
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA REPARACION' ";

            //Valido si fue digitado una estiba para buscar
            if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
                ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            else
                ConsultaSQL += ",NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            View.BuscarEstibaRecibo.Text = "";
            //View.BuscarPosicionRecibo.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA REPARACION', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnConfirmarRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
                return;

            //Recorro el listado de registros seleccionados para confirmar el recibo
            foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
            {
                //Creo la consulta para confirmar el cambio de ubicacion de la estiba " + Registros.Row["Posicion"] + "
                ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEPOSICION','','PARA ASIGNACION','REPARACION','" + Registros.Row["UA"].ToString() + "'";

                //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                ConsultaSQL += "EXEC sp_InsertarNuevo_Movimiento 'RECIBO DE MERCANCIA REPARACIÓN','','REPARACIÓN','" + Registros.Row["UA"].ToString() + "','','REPARACION','UBICACIONALMACEN','" + this.user + "','';";

                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //Busco los registros para actualizar el listado
            BuscarRegistrosRecibo();

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

        public void LimpiarDatosIngresoSeriales()
        {
            try
            {
                View.StackProcesoReparacion.IsEnabled = false;
                View.TecnicoDiagnosticador.Text = "";
                View.TecnicoAsignado.Text = "";
                View.ProductoProcesamiento.Text = "";
                View.FallaReparacion.SelectedIndex = -1;
                View.ObservacionesRep.Text = "";
                View.FallaReparacionAdic.SelectedIndex = -1;
                View.FallaReparacionAdic2.SelectedIndex = -1;
                View.FallaReparacionAdic3.SelectedIndex = -1;
                View.FallaReparacionAdic4.SelectedIndex = -1;
                View.MotivoSCRAP.SelectedIndex = -1;
                View.ObservacionesAdic.Text = "";
                View.ObservacionesAdic2.Text = "";
                View.ObservacionesAdic3.Text = "";
                View.ObservacionesAdic4.Text = "";
                //ObservacionesAdic5.Text = "";
                View.ProductoProcesamiento.Text = "";
                View.FallaProcesamiento.Text = "";
                View.PartesCambiadas.Text = "";
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.MotivoSCRAP.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.ShowError("Error: " + ex.Message);
            }
        }

        #region Asignacion

        //Asignacion
        private void OnBuscarRegistrosAsignacion(object sender, EventArgs e)
        {
            //Busco los registros
            BuscarRegistrosAsignacion();
        }

        public void BuscarRegistrosAsignacion()
        {

            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAREPARACION', 'PARA ASIGNACION'";

            //Ejecuto la consulta
            View.Model.ListRecords = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            ////Variables Auxiliares
            //String ConsultaSQL;

            ////Creo la consulta para buscar los registros
            //ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ASIGNACION'";

            ////Ejecuto la consulta
            //View.Model.ListRecords = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosAsignacion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            //View.BuscarEstibaAsignacion.Text = "";
            //View.BuscarPosicionAsignacion.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'REPARACION', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoAsignacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnListarEquiposEstibas(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Valido que haya sido seleccionado una estiba
            //if (View.ListadoBusquedaAsignacion.SelectedItems.Count == -1)
            //    return;

            //Creo la consulta
            //ConsultaSQL = "SELECT * FROM dbo.EquiposCLARO WHERE IdPallet = '" + ((DataRowView)View.ListadoBusquedaAsignacion.SelectedItem).Row["Estiba"].ToString() + "'";

            //Ejecuto la consulta
            //View.Model.ListadoEquiposEstiba = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            //Habilito el listado de equipos asignados a la estiba
            //View.StackListadoEquiposEstiba.Visibility = Visibility.Visible;
        }

        private void OnMostrarTecnicosEstibas(object sender, EventArgs e)
        {
            //Valido que haya sido seleccionado un equipo
            //if (View.ListadoEquiposEstiba.SelectedItems.Count == -1)
            //    return;

            ////Evaluo el tecnico para mostrar el usuario o un mensaje de no asignacion
            //if (String.IsNullOrEmpty(((DataRowView)View.ListadoEquiposEstiba.SelectedItem).Row["Tecnico_Reparacion"].ToString()))
            //    View.TecnicoAsignado.Text = "Sin Asignar";
            //else
            //    View.TecnicoAsignado.Text = ((DataRowView)View.ListadoEquiposEstiba.SelectedItem).Row["Tecnico_Reparacion"].ToString();

            ////Habilito el bloque para asignar el tecnico al equipo
            //View.StackAsignacionTecnico.Visibility = Visibility.Visible;
        }

        private void OnConfirmarTecnicoEquipo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            string ConsultaSQL = "", ConsultaMov = "",  TecnicoAsignacion = "", RowID = "";
            //DataTable RegistroAuxiliar;

            //Valido que haya sido seleccionado un equipo
            if (View.ListadoItemsAsignacion.SelectedItems.Count == 0)
                return;

            //Evaluo que haya sido seleccionado un tecnico
            if (View.TecnicosAsignar.SelectedIndex == -1)
            {
                //Muestro el mensaje de error
                Util.ShowError("Por favor seleccionar el tecnico para este equipo.");
                return;
            }
            try
            {
                TecnicoAsignacion = View.TecnicosAsignar.Text.ToString();
                //Creo la consulta para asignar el tecnico
                ConsultaSQL += "UPDATE dbo.EquiposCLARO SET Tecnico_Reparacion = '" + TecnicoAsignacion + "', Estado = 'PARA PROCESO' WHERE RowID IN (";
                foreach (DataRowView item in View.ListadoItemsAsignacion.SelectedItems)
                {
                    RowID = item.Row["RowID"].ToString();
                    ConsultaSQL += "'" + RowID + "',";
                    ConsultaMov += "EXEC dbo.sp_InsertarNuevo_Movimiento 'TECNICO ASIGNADO','REPARACIÓN','REPARACIÓN','Sin pallet','" + item.Row["RowID"].ToString() + "','REPARACION','UBICACIONPRODUCCION','" + this.user + " - " + TecnicoAsignacion + "','';";
                }

                //borra de la lista los elementos seleccionados
                while (View.ListadoItemsAsignacion.SelectedItems.Count > 0)
                {
                    View.Model.ListRecords.Rows.RemoveAt(View.ListadoItemsAsignacion.Items.IndexOf(View.ListadoItemsAsignacion.SelectedItem));
                }
                
                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaSQL))
                {
                    //Ejecuto la consulta
                    ConsultaSQL += ");";
                    ConsultaSQL = ConsultaSQL.Replace(",);", ");");
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaMov, Local);
                    //Limpio la consulta para volver a generar la nueva
                    ConsultaSQL = "";
                    ConsultaMov = "";

                    //Muestro el mensaje de confirmacion
                    Util.ShowMessage("Técnico asignado correctamente.");
                }

                //BuscarRegistrosAsignacion();
                View.TecnicoReparador.Text = "";
                View.FallaReparacion.Text = "";
                View.FechaReparacion.Text = "";

                //Limspio los datos del combo de tecnicos
                View.TecnicosAsignar.SelectedIndex = -1;
                View.ListadoItemsAsignacion.SelectedIndex = -1;
                return;
            }
            catch (Exception Ex)
            {
                //Muestro el mensaje de error
                Util.ShowError("Hubo un error al intentar asignar el tecnico al equipo. Error: " + Ex.Message);
                return;
            }
        }

        private void OnConsultaReparacionAnterior(object sender, EventArgs e)
        {
            //Validacion existe o no el equipo en DB
            String ConsultaBuscar = "SELECT Serial FROM dbo.EquiposCLARO WHERE Serial = '" + SerialAsignacion + "'";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

            //Validacion existe o no el equipo en DB Despacho
            String ConsultaBuscarDespacho = "SELECT Serial FROM dbo.Despacho_EquiposCLARO WHERE Serial = '" + SerialAsignacion + "'";
            DataTable ResultadoDespacho = service.DirectSQLQuery(ConsultaBuscarDespacho, "", "dbo.Despacho_EquiposCLARO", Local);

            if (Resultado.Rows.Count > 0 && ResultadoDespacho.Rows.Count == 0)
            {
                //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                DataTable RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERALREPARACION','" + SerialAsignacion + "'", "", "dbo.EquiposCLARO", Local);

                if (RegistroValidado.Rows.Count == 0)
                {
                    View.TecnicoReparador.Text = "Primera asignación del equipo";
                    View.FallaAnteriorDiag.Text = "";
                    View.FechaReparacion.Text = "";
                }
                else // Entra como garantia
                {
                    String tecnico = RegistroValidado.Rows[0]["TecnicoRep"].ToString();
                    String falla = RegistroValidado.Rows[0]["FallaDiag"].ToString();
                    String fecha = RegistroValidado.Rows[0]["FechaRep"].ToString();

                    if (UtilWindow.ConfirmOK("El equipo entra como garantia. \nTecnico anterior " + tecnico + " \nFalla: " + falla + "\nFecha ultima reparación: " + fecha + " \n\n ¿Desea asignar el equipo ahora?") == false)
                    {
                        View.TecnicoReparador.Text = "";
                        View.FallaAnteriorDiag.Text = "";
                        View.FechaReparacion.Text = "";
                        return;
                    }

                    View.TecnicoReparador.Text = tecnico;
                    View.FallaAnteriorDiag.Text = falla;
                    View.FechaReparacion.Text = fecha;
                }
            }
            else if (Resultado.Rows.Count > 0 && ResultadoDespacho.Rows.Count > 0)
            {
                DataTable RegistroValidadoDespacho = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERALDESPACHO','" + SerialAsignacion + "'", "", "dbo.Despacho_EquiposCLARO", Local);

                String tecnico = RegistroValidadoDespacho.Rows[0]["TecnicoRep"].ToString();
                String falla = RegistroValidadoDespacho.Rows[0]["FallaDiag"].ToString();
                String fecha = RegistroValidadoDespacho.Rows[0]["FechaRep"].ToString();

                if (UtilWindow.ConfirmOK("El equipo entra como garantia. \nTecnico anterior " + tecnico + " \nFalla: " + falla + "\nFecha ultima reparación: " + fecha + " \n\n ¿Desea asignar el equipo ahora?") == false)
                {
                    View.TecnicoReparador.Text = "";
                    View.FallaAnteriorDiag.Text = "";
                    View.FechaReparacion.Text = "";
                    return;
                }

                View.TecnicoReparador.Text = RegistroValidadoDespacho.Rows[0]["TecnicoRep"].ToString();
                View.FallaAnteriorDiag.Text = RegistroValidadoDespacho.Rows[0]["FallaDiag"].ToString();
                View.FechaReparacion.Text = RegistroValidadoDespacho.Rows[0]["FechaRep"].ToString();
            }
        }

        private void OnDeleteDetails(object sender, EventArgs e)
        {
            if (View.ListadoItemsAsignacion.SelectedItems.Count == 0)
                Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");

            //borra de la lista los elementos seleccionados
            while (View.ListadoItemsAsignacion.SelectedItems.Count > 0)
            {
                View.Model.ListRecords.Rows.RemoveAt(View.ListadoItemsAsignacion.Items.IndexOf(View.ListadoItemsAsignacion.SelectedItem));
            }

            View.TecnicoReparador.Text = "";
            View.FallaAnteriorDiag.Text = "";
            View.FechaReparacion.Text = "";
        }

        #endregion

        public void CargarTecnicosReparacion()
        {
            String ConsultaSQL = "SELECT TECNICO_REPARACION as Tecnico from dbo.EquiposCLARO where Estado = 'REPARACION' group by TECNICO_REPARACION";
            DataTable Resultado = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            View.Model.ListadoTecnicosReparacion = new DataTable("ListadoTecnicosReparacion");
            View.Model.ListadoTecnicosReparacion.Columns.Add("Tecnico", typeof(string));

            try
            {
                foreach (DataRow item in Resultado.Rows)
                {
                    String Cadena = item["Tecnico"].ToString();

                    if (Cadena.Contains(","))
                    {
                        string[] split = Cadena.Split(new Char[] { ',' });
                        Cadena = split.First();
                        
                        View.Model.ListadoTecnicosReparacion.Rows.Add(Cadena);
                    }
                    else
                    {
                        View.Model.ListadoTecnicosReparacion.Rows.Add(Cadena);
                    }
                }
                View.Model.ListadoTecnicosReparacion = View.Model.ListadoTecnicosReparacion.DefaultView.ToTable( /*distinct*/ true);
            }
            catch (Exception ex)
            {
                Util.ShowError("Error: " + ex.Message);
            }
        }

        private void OnAddToList(object sender, EventArgs e)
        {
            DataRow dr;
            int aux = 0;
            String ConsultaAgregar = "";
            ConsultaAgregar = "UPDATE dbo.EquiposCLARO set Estado = 'REPARACION_ENTREGA' WHERE RowID IN (";
            foreach (DataRowView item in View.ListadoItems.SelectedItems)
            {
                foreach (DataRowView itemAdd in View.ListadoItemsAgregados.Items)
                {
                    if (item.Row["RowID"].ToString() == itemAdd.Row["RowID"].ToString())
                    {
                        aux++;
                    }
                }

                if (aux > 0)
                {
                    Util.ShowError("El serial " + item.Row["Serial"].ToString() + " ya se encuentra en el listado");
                    aux = 0;
                }
                else
                {
                    //agrego los equipos al listado de agregados
                    dr = View.Model.ListRecordsAddToPallet.NewRow();

                    dr["RowID"] = item.Row["RowID"].ToString();
                    dr["Serial"] = item.Row["Serial"].ToString();
                    dr["Mac"] = item.Row["Mac"].ToString();
                    dr["Tecnico"] = item.Row["Tecnico"].ToString();
                    dr["Producto"] = item.Row["Producto"].ToString();
                    dr["Estado"] = item.Row["Estado"].ToString();
                    dr["FECHA"] = item.Row["FECHA"].ToString();
                    dr["TIEMPOREP"] = item.Row["TIEMPOREP"].ToString();
                    dr["CAJA"] = item.Row["CAJA"].ToString();

                    View.Model.ListRecordsAddToPallet.Rows.Add(dr);

                    //cambio el estado para no mostrar mas en el listado general
                    ConsultaAgregar += "'" + item.Row["RowID"] + "',";
                }
            }
            ConsultaAgregar += ");";
            ConsultaAgregar = ConsultaAgregar.Replace(",);", ");");
            service.DirectSQLNonQuery(ConsultaAgregar, Local);
            while (View.ListadoItems.SelectedItems.Count > 0)
            {
                View.Model.ListRecords_1.Rows.RemoveAt(View.ListadoItems.Items.IndexOf(View.ListadoItems.SelectedItem));
            }
            BuscarEquiposPorTecnicoEntrega();
        }

        private void OnRemoveSelection(object sender, EventArgs e)
        {
            String ConsultaAgregar = "";

            if (View.ListadoItemsAgregados.SelectedIndex == -1)
            {
                Util.ShowError("Seleccione los registros que desea remover");
                return;
            }

            foreach (DataRowView itemAdd in View.ListadoItemsAgregados.SelectedItems)
            {
                //cambio el estado para no mostrar mas en el listado general
                ConsultaAgregar = "update dbo.EquiposCLARO set Estado = 'REPARACION' where RowID = " + itemAdd.Row["RowID"];
                Console.WriteLine(ConsultaAgregar);
                service.DirectSQLNonQuery(ConsultaAgregar, Local);
                ConsultaAgregar = "";
            }

            while (View.ListadoItemsAgregados.SelectedItems.Count > 0)
            {
                View.Model.ListRecordsAddToPallet.Rows.RemoveAt(View.ListadoItemsAgregados.Items.IndexOf(View.ListadoItemsAgregados.SelectedItem));
            }
            BuscarEquiposPorTecnicoEntrega();
        }

        private void OnHabilitarMotivo(object sender, EventArgs e)
        {
            try
            {
                //if (((ComboBoxItem)FallaReparacionAdic5.SelectedItem).Content.ToString() != "SCRAP")
                if (!((MMaster)View.FallaReparacionAdic5.SelectedItem).Name.Contains("SCRAP"))
                //if (View.FallaReparacionAdic5.Text.Contains("SCRAP"))
                {
                    //Util.ShowMessage(((MMaster)View.FallaReparacionAdic5.SelectedItem).Name);
                    View.MotivoSCRAP.SelectedValue = "";
                    View.MotivoSCRAP.IsEnabled = false;
                    View.FallaReparacion.IsEnabled = true;
                    View.FallaReparacionAdic.IsEnabled = true;
                    View.FallaReparacionAdic2.IsEnabled = true;
                    View.FallaReparacionAdic3.IsEnabled = true;
                    View.FallaReparacionAdic4.IsEnabled = true;
                }
                else
                {
                    View.FallaReparacion.SelectedIndex = -1;
                    View.FallaReparacionAdic.SelectedIndex = -1;
                    View.FallaReparacionAdic2.SelectedIndex = -1;
                    View.FallaReparacionAdic3.SelectedIndex = -1;
                    View.FallaReparacionAdic4.SelectedIndex = -1;
                    View.FallaReparacion.IsEnabled = false;
                    View.FallaReparacionAdic.IsEnabled = false;
                    View.FallaReparacionAdic2.IsEnabled = false;
                    View.FallaReparacionAdic3.IsEnabled = false;
                    View.FallaReparacionAdic4.IsEnabled = false;
                    //cb_FallaRep.SelectedValue = "";
                    //cb_FallaAdic.SelectedValue = "";
                    //cb_FallaAdic2.SelectedValue = "";
                    //cb_FallaAdic3.SelectedValue = "";
                    //cb_FallaAdic4.SelectedValue = "";
                    View.MotivoSCRAP.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void CargarHistorico(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaBuscar = "";
            SerialReparacionH = View.GetSerial1.Text.ToString();
            MacReparacionH = View.GetSerial2.Text.ToString();
            try
            {
                if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
                {
                    if (String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
                    {
                        Util.ShowError("Los campos no pueden estar vacios.");
                        return;
                    }
                }
                ConsultaBuscar = "select repa_id,Tecnico_Reparacion as repa_tecnico,FALLA_REP as  repa_fallaPrincipal,FALLA_REP1 as repa_falla1,FALLA_REP2 as repa_falla2,FALLA_REP3 as repa_falla3,FALLA_REP4 as repa_falla4,ESTATUS_REPARACION AS repa_estadoFinal,OBSERVACIONES repa_observaciones,MOTIVO_SCRAP AS  repa_scrap,ProcesoReparacionClaro.mov_id as 'Id_movimiento',repa_caja from Despacho_EquiposCLARO LEFT join MovimientoClaro on (Despacho_EquiposCLARO.RowID = MovimientoClaro.rowid) LEFT join ProcesoReparacionClaro on (MovimientoClaro.mov_id = ProcesoReparacionClaro.mov_id)  where Serial=" + "'" + SerialReparacionH + "'";
                View.Model.ListReparaciones = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);
                if (View.Model.ListReparaciones.Rows.Count <= 0)
                {

                }
                else
                {
                    View.Border_ListaHP.Visibility = Visibility.Visible;
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Error: " + Ex.Message);
                return;
            }
        }
        #endregion

    }
}
