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

    public interface IReparacionesDTVPresenter
    {
        IReparacionesDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ReparacionesDTVPresenter : IReparacionesDTVPresenter
    {
        public IReparacionesDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        private String SerialReparacionH;
        private String MacReparacionH;

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public ReparacionesDTVPresenter(IUnityContainer container, IReparacionesDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ReparacionesDTVModel>();

            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.FiltrarDatosEntrega += new EventHandler<EventArgs>(this.OnFiltrarDatosEntrega);
            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);
            View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);

            //OnFiltrarDatosEntrega
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.CargarDatosReparacion += new EventHandler<EventArgs>(this.CargarDatosReparacion);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ListarEquiposSeleccion += new EventHandler<EventArgs>(this.OnListarEquiposSeleccion);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

            //Asignacion
            View.BuscarRegistrosAsignacion += this.OnBuscarRegistrosAsignacion;
            View.ActualizarRegistrosAsignacion += this.OnActualizarRegistrosAsignacion;
            View.ListarEquiposEstibas += this.OnListarEquiposEstibas;
            View.MostrarTecnicosEstibas += this.OnMostrarTecnicosEstibas;
            View.ConfirmarTecnicoEquipo += this.OnConfirmarTecnicoEquipo;
            View.FilaSeleccionada += this.OnFilaSeleccionada;
            //Proceso
            View.CargarHistorico += new EventHandler<EventArgs>(this.CargarHistorico);
            

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
            //ListarDatos();

            //Cargo los tecnicos
            View.Model.ListadoTecnicos = service.GetSysUser(new SysUser());

            view.StackProcesoReparacion.IsEnabled = false;

            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIA_REPARACION', 'REPARACION'";

            ListarDatos();
        }

        private void OnFiltrarDatosEntrega(object sender, EventArgs e)
        {
            FiltrarDatosEntrega();
        }

        private void FiltrarDatosEntrega()
        {

            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAFILTRADA_REP', '" + ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() + "', 'REPARACION'";

            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            ////Valido el estado por el cual se filtro para filtrar el destino al que pueden ir los items
            //if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() != "REPARADO")
            //{
            //    View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'UBICACIONFILTRADA', 'REPARACION', 'CLARO', 'ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
            //}
            //else
            //{
            //    View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'UBICACIONFILTRADA', 'REPARACION', 'CLARO', 'DIAGNOSTICO', 'ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
            //}

        }



        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");
            
            View.Model.ListadoFallaDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOMAT" } });
            View.Model.ListadoMotivos = service.GetMMaster(new MMaster { MetaType = new MType { Code = "MOTSCRAP" } });
            View.Model.ListadoSubFalla = service.GetMMaster(new MMaster { MetaType = new MType { Code = "SUBFALLA" } });

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("IdPallet", typeof(String));
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
                ConsultaBuscar = "SELECT SERIAL,RECEIVER,SMART_CARD_ENTRADA,repa_tecnico as 'Tecnico',repa_fallaPrincipal AS 'FallaP',repa_falla1 as 'Falla1',repa_falla2 as 'Falla2',repa_falla3 as 'Falla3',repa_falla4 as 'Falla4',repa_estadoFinal as 'EstadoF',repa_observaciones as 'OBV',repa_scrap as 'Scrap',ProcesoReparacionDIRECTV.mov_id as 'Id_movimiento', mov_fechaMovimiento as 'Fecha_Reparacion' from EquiposDIRECTVC inner join MovimientoDIRECTV on (EquiposDIRECTVC.RowID = MovimientoDIRECTV.rowid) inner join ProcesoReparacionDIRECTV on (MovimientoDIRECTV.mov_id = ProcesoReparacionDIRECTV.mov_id) WHERE SERIAL='" + SerialReparacionH + "'";
                View.Model.ListReparaciones = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);
                if (View.Model.ListReparaciones.Rows.Count <= 0)
                {
                    //Util.ShowError("El equipo no tiene historial");
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
        private void ListarDatos()
        {
            DataSet ds = Util.GetListaDatosModuloDIRECTVC("REPARACION", null);

            View.Model.ListRecords_1 = ds.Tables[0];
        }

        private void CargarDatosReparacion(object sender, EventArgs e)
        {

            DataTable Consulta;


            Consulta = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCARPRODUCTO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposDIRECTVC", Local);


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
                String TecnicoAsignado = Consulta.Rows[0]["Tecnico"].ToString().ToUpper();
                String NombreTecnicoSesion = (this.userName + ", " + this.user).ToUpper();
                String UserTecnicoSesion = this.userName.ToUpper();

                if ((TecnicoAsignado == NombreTecnicoSesion) || (TecnicoAsignado == UserTecnicoSesion))
                {
                    View.StackProcesoReparacion.IsEnabled = true;
                    View.ProductoProcesamiento.Text = Consulta.Rows[0]["Producto"].ToString();
                    View.FallaProcesamiento.Text = Consulta.Rows[0]["Falla"].ToString();
                    View.TecnicoAsignado.Text = Consulta.Rows[0]["Tecnico"].ToString();
                    View.TecnicoDiagnosticador.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                }
                else
                {
                    Util.ShowError("No tiene permisos para realizar esta accion, tecnico asignado a este serial: " + Consulta.Rows[0]["Tecnico"].ToString() + "");
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
                    Util.ShowError("El serial " + View.GetSerialAsignacion.Text + " ya esta en el listado.");
                    View.GetSerialAsignacion.Text = "";
                    View.GetMacAsignacion.Text = "";
                    View.GetSerialAsignacion.Focus();
                    return;
                }
            }

            try
            {

                //Validacion existe o no el equipo en DB
                ConsultaBuscar = "SELECT * FROM dbo.EquiposDIRECTVC WHERE Serial = '" + View.GetSerialAsignacion.Text.ToString() + "' AND Estado != 'DESPACHADO'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

                if (Resultado.Rows.Count > 0)
                {

                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCAREQUIPOGENERAL','" + View.GetSerialAsignacion.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

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
                        //Asigno los campos
                        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                        dr["Producto"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                        dr["Receiver"] = RegistroValidado.Rows[0]["Receiver"].ToString();
                        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();

                        //Agrego el registro al listado
                        View.Model.ListRecords.Rows.Add(dr);

                        //Limpio los seriales para digitar nuevos datos
                        View.GetSerialAsignacion.Text = "";
                        View.GetMacAsignacion.Text = "";
                        View.GetSerialAsignacion.Focus();
                    }

                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
                return;
            }
        }

        private void OnGenerarCodigo(object sender, EventArgs e)
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
                        View.CodigoEmpaque.Text = Resultado.Rows[0]["idpallet"].ToString();
                    }

                    ConsultaBuscar = "";
                    ConsultaValidar = "";
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Se presento un error generando el pallet: " + ex.Message);
            }
            ////Variables Auxiliares
            //String ConsultaBuscar;
            //DataTable RegistroValidado;

            ////Creo el número de pallet aleatorio 
            //ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            //DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            //if (Resultado.Rows.Count > 0)
            //{
            //    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
            //    RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposDIRECTVC WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposDIRECTVC", Local);

            //    //Evaluo si el serial existe
            //    if (RegistroValidado.Rows.Count > 0)
            //    {
            //        Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
            //    }
            //    else
            //    {
            //        //Asigno los campos
            //        View.CodigoEmpaque.Text = "RES-R" + Resultado.Rows[0]["idpallet"].ToString();

            //        ////Limpio los seriales para digitar nuevos datos
            //        //View.GetSerial1.Text = "";
            //        //View.GetSerial2.Text = "";
            //        //View.GetSerial1.Focus();
            //    }
            //}
            //else
            //{
            //    Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            //}
        }

        private void OnConfirmarImpresion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String unidad_almacenamiento = "";
            String codigoEmp = View.CodigoEmpaque.Text.ToString();
            String destino = "";
            String tipo_listado = "";
            String estado = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

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

            if (String.Compare("", codigoEmp) == 0)
            {
                Util.ShowError("Por favor generar un código de empaque");
                return;
            }

            //Creo la base de la consulta para traer los seriales respectivos
            ConsultaSQL = "SELECT idPallet,Posicion,serial,Receiver,SMART_CARD_ENTRADA,Fecha_ingreso,MODELO FROM dbo.EquiposDIRECTVC WHERE serial IN (''";

            //Recorro el listado de registros seleccionados para obtener los seriales e imprimirlos
            foreach (DataRowView Registros in View.ListadoItems.SelectedItems)
            {
                //Util.ShowMessage(Registros.Row["Serial"].ToString());
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += ",'" + Registros.Row["Serial"] + "'";
            }

            //Completo la consulta
            ConsultaSQL += ")";

            //Elimino la basura en la cadena
            ConsultaSQL = ConsultaSQL.Replace("'',", "");

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            if (View.UnidadAlmacenamiento.SelectedIndex != -1)
            {
                unidad_almacenamiento = ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString();
            }
            else
            {
                Util.ShowError("Selecciona una unidad de empaque");
                return;
            }

            if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() != "REPARADO")
            {
                tipo_listado = "SCRAP";
                estado = "PARA SCRAP";
            }
            else
            {
                tipo_listado = "REPARADOS";
                estado = "REPARADOS";
            }

            //Imprimo los registros
            PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, unidad_almacenamiento, codigoEmp, estado, "DIRECTV", "REPARACIÓN - " + destino, tipo_listado, "AUX");
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
            String ConsultaGuardar = "";
            String ConsultaGuardarTrack = "";
            string FallaRep = "";
            string subfalla = "";
            string Falla1 = "";
            string Falla2 = "";
            string Falla3 = "";
            string Falla4 = "";
            string EstatusRep = "";
            string PartesCambiadas = "";
            string MotivoScrap = "";

            try
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
                if (((ComboBoxItem)View.FallaReparacionAdic5.SelectedItem == null || (View.FallaReparacionAdic5.Text.ToString() == "")))
                {
                    Util.ShowError("Por favor ingresar el estado de reparacion");
                    return;
                }
                else
                { EstatusRep = ((ComboBoxItem)View.FallaReparacionAdic5.SelectedItem).Content.ToString(); }

                //----------------------------------
                if (View.PartesCambiadas.Text == "")
                {
                    Util.ShowError("Por favor ingresar la descripcion de las partes cambiadas");
                    return;
                }
                else
                { PartesCambiadas = View.PartesCambiadas.Text.ToString(); }

                if ((View.FallaReparacionAdic5.Text.ToString() == "SCRAP"))
                {
                    //----------------------------------
                    if (View.MotivoSCRAP.Text == "" || View.MotivoSCRAP.Text == null)
                    {
                        Util.ShowError("Por favor ingresar el motivo de SCRAP");
                        return;
                    }
                    else
                    { MotivoScrap = View.MotivoSCRAP.Text.ToString(); }
                }

                if (((MMaster)View.SubFallas.SelectedItem == null))
                {
                    Util.ShowError("Por favor ingresar la sub-falla de reparacion");
                    return;
                }
                else
                { subfalla = ((MMaster)View.SubFallas.SelectedItem).Code.ToString(); }

                //Construyo la consulta para guardar los datos
                ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'REPARACION', Estado = 'REPARACION',";
                ConsultaGuardar += " FALLA_REP = '" + FallaRep + "', FALLA_REP1 = '" + Falla1 + "', FALLA_REP2 = '" + Falla2 + "'";
                ConsultaGuardar += ", FALLA_REP3 = '" + Falla3 + "', FALLA_REP4 = '" + Falla4 + "', MOTIVO_SCRAP= '" + MotivoScrap + "'";
                ConsultaGuardar += ", ESTATUS_REPARACION = '" + EstatusRep + "', PARTES_CAMBIADAS = '" + PartesCambiadas 
                            + "', SUBFALLA_REPARACION = '" + subfalla + "'" 
                            + " WHERE Serial = '" + View.GetSerial1.Text.ToString() + "' AND Estado != 'DESPACHADO';";

                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoReparacionDIRECTV 'REPARACIÓN TERMINADA','REPARACIÓN','REPARACIÓN','Sin pallet','" + View.GetSerial1.Text.ToString() +
                    "','" + View.TecnicoAsignado.Text + "','" + FallaRep + "','" + Falla1 + "','" + Falla2 + "','" + Falla3 + "','" + Falla4 + "','" + EstatusRep +
                    "','" + PartesCambiadas + "','" + ((MotivoScrap != "NULL") ? MotivoScrap : "") + "','" + this.user + "';";

                Console.WriteLine("###### " + ConsultaGuardar);

                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_REPARACION = '" + EstatusRep + "', FECHA_REPARADO = '" + DateTime.Now.ToString("dd/MM/yyyy") + "' WHERE SERIAL = '" + View.GetSerial1.Text.ToString() + "';";


                service.DirectSQLNonQuery(ConsultaGuardar, Local);
                service.DirectSQLNonQuery(ConsultaGuardarTrack, Local);

                //Limpio la consulta para volver a generar la nueva
                ConsultaGuardar = "";
                ConsultaGuardarTrack = "";


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

                View.MotivoSCRAP.SelectedIndex = -1;

                //Reinicio los campos
                LimpiarDatosIngresoSeriales();

                return;
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message);
                return;
            }
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, ConsultaTrack = "";


            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoItems.SelectedItems.Count == 0)
                return;

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }

            if (String.IsNullOrEmpty(View.CodigoEmpaque.Text))
            {
                Util.ShowError("Por favor ingrese un codigo de unidad de almacenamiento");
                return;
            }

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            if (NuevaUbicacion == "DIAGNOSTICO")
            {
                NuevoEstado = "PARA DIAGNOSTICO";

                foreach (DataRowView item in View.ListadoItems.SelectedItems)
                {
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" 
                                + NuevoEstado + "', UA = '" + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString()
                                + "', IdPallet = '" + View.CodigoEmpaque.Text.ToString() + "', CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() + "', CodigoEmpaque2 = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" 
                                + item.Row["RowID"] + "';";

                    ConsultaTrack += "UPDATE dbo.TrackEquiposDIRECTV SET CODEMPAQUE_REP = '" 
                                + View.CodigoEmpaque.Text.ToString() + "' WHERE ID_SERIAL = '" + item.Row["RowID"] + "';";

                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE REPARACIÓN','REPARACIÓN','DIAGNOSTICO','" 
                                + View.CodigoEmpaque.Text.ToString() + "','" + item.Row["RowID"] + "','REPARACION','UBICACIONPRODUCCION','" + this.user + "','';";
                    
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
            }
            else
            {
                NuevoEstado = "PARA ALMACENAMIENTO";

                foreach (DataRowView item in View.ListadoItems.SelectedItems)
                {
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" 
                                + NuevoEstado + "', UA = '" + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString()
                                + "', CodigoEmpaque2 = '" + View.CodigoEmpaque.Text.ToString() + "', CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() + "', IdPallet = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" 
                                + item.Row["RowID"] + "';";

                    ConsultaTrack += "UPDATE dbo.TrackEquiposDIRECTV SET CODEMPAQUE_REP = '" 
                                + View.CodigoEmpaque.Text.ToString() + "' WHERE ID_SERIAL = '" + item.Row["RowID"] + "';";

                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE REPARACIÓN','REPARACIÓN','ALMACENAMIENTO','" 
                                + View.CodigoEmpaque.Text.ToString() + "','" + item.Row["RowID"] + "','REPARACION','UBICACIONPRODUCCION','" + this.user + "','';";
                    
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
            }


            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            ListarDatos();

            //Quito la selecion de la nueva ubicacion
            View.Ubicacion.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.ListadoItems.SelectedIndex = -1;

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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADIAGNOSTICO', 'PARA REPARACION' ";

            //Valido si fue digitado una estiba para buscar
            if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
                ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            else
                ConsultaSQL += ",NULL";

            //Valido si fue seleccionado ubicaciones para filtrar
            //if (View.BuscarPosicionRecibo.SelectedIndex != -1)
            //    ConsultaSQL += ",'" + ((MMaster)View.BuscarPosicionRecibo.SelectedItem).Code.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            View.BuscarEstibaRecibo.Text = "";
            //View.BuscarPosicionRecibo.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARRECIBOREPARACION', 'PARA REPARACION', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
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
                //Creo la consulta para confirmar el cambio de ubicacion de la estiba
                ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEPOSICIONREP','PARA ASIGNACION','REPARACION','" + Registros.Row["UA"].ToString() + "';";

                //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'RECIBO DE MERCANCIA REPARACIÓN','','REPARACIÓN','" + Registros.Row["UA"].ToString() + "','','REPARACION','UBICACIONALMACEN','" + this.user + "','';";

                Console.WriteLine("###### " + ConsultaSQL);
                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //Busco los registros para actualizar el listado
            BuscarRegistrosRecibo();
            View.Model.Listado_PalletSerial.Rows.Clear();
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
            View.FallaReparacion.SelectedIndex = -1;
            View.ObservacionesRep.Text = "";
            View.FallaReparacionAdic.SelectedIndex = -1;
            View.FallaReparacionAdic2.SelectedIndex = -1;
            View.FallaReparacionAdic3.SelectedIndex = -1;
            View.FallaReparacionAdic4.SelectedIndex = -1;
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

        private void OnListarEquiposSeleccion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
                return;

            string aux_idPallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["UA"].ToString();

            String Consulta = "SELECT IdPallet as PPallet, "
            + "serial as PSerial, "
            + "RECEIVER as Receiver, "
            + "SMART_CARD_ENTRADA as PMac,  "
            + "MODELO as Modelo,  "
            + "TIPO_ORIGEN as PTRecibo, "
            + "convert(VARCHAR,FECHA_INGRESO,120) as PFRegistro,  "
            + "DATEDIFF(day, FECHA_INGRESO,GETDATE()) as NumeroDias,dbo.TIMELAPSELEO(FECHA_INGRESO) as horas "
            + "from dbo.EquiposDIRECTVC WHERE ((IdPallet IS NOT NULL) AND (ESTADO = 'PARA REPARACION')) "
            + " AND IdPallet = '" + aux_idPallet + "'";

            Console.WriteLine(Consulta);

            View.Model.Listado_PalletSerial =
            service.DirectSQLQuery(Consulta, "", "dbo.EquiposDIRECTVC", Local);
            
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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAREPARACION', 'PARA ASIGNACION'";

            //Ejecuto la consulta
            View.Model.ListRecords = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnActualizarRegistrosAsignacion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            //View.BuscarEstibaAsignacion.Text = "";
            //View.BuscarPosicionAsignacion.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADIAGNOSTICO', 'REPARACION', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoAsignacion = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
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
            String ConsultaSQL = "";
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
                String TecnicoAsignacion = View.TecnicosAsignar.Text.ToString();

                foreach (DataRowView item in View.ListadoItemsAsignacion.SelectedItems)
                {
                    //Creo la consulta para asignar el tecnico
                    ConsultaSQL += "UPDATE dbo.EquiposDIRECTVC SET Tecnico_Reparacion = '" + TecnicoAsignacion + "', Estado = 'PARA PROCESO' WHERE RowID = '" + item.Row["RowID"].ToString() + "';";

                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'TECNICO ASIGNADO','REPARACIÓN','REPARACIÓN','Sin pallet','" + item.Row["RowID"].ToString() + "','REPARACION','UBICACIONPRODUCCION','" + this.user + " - " + TecnicoAsignacion + "','';";
                    Console.WriteLine("###### " + ConsultaSQL);

                    //Ejecuto la query
                    //service.DirectSQLNonQuery(ConsultaSQL, Local);
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaSQL))
                {
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaSQL = "";

                    //Muestro el mensaje de confirmacion
                    Util.ShowMessage("Tecnico asignado correctamente.");
                }

                //BuscarRegistrosAsignacion();

                //Lispio los datos del combo de tecnicos
                //View.TecnicoReparador.Text = "";
                //View.FallaReparacion.Text = "";
                //View.FechaReparacion.Text = "";
                //Lispio los datos del combo de tecnicos
                View.TecnicosAsignar.SelectedIndex = -1;
                View.ListadoItemsAsignacion.SelectedIndex = -1;
                View.Model.ListRecords.Clear();

            }
            catch (Exception Ex)
            {
                //Muestro el mensaje de error
                Util.ShowError("Hubo un error al intentar asignar el tecnico al equipo. Error: " + Ex.Message);
                return;
            }
        }

        #endregion

        #endregion

    }
}