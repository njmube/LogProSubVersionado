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

    public interface IDiagnosticoPresenter
    {
        IDiagnosticoView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class DiagnosticoPresenter : IDiagnosticoPresenter
    {
        public IDiagnosticoView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;

        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public static int offset = 4; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public DiagnosticoPresenter(IUnityContainer container, IDiagnosticoView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DiagnosticoModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);
            View.HabilitarUbicacion += new EventHandler<SelectionChangedEventArgs>(this.OnHabilitarUbicacion);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;
            View.FilaSeleccionada += this.OnFilaSeleccionada;
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            View.BuscarEquipos += new EventHandler<EventArgs>(this.OnBuscarEquipos);
            View.AddToList += new EventHandler<EventArgs>(this.OnAddToList);
            View.RemoveSelection += new EventHandler<EventArgs>(this.OnRemoveSelection);
            View.ExportPalletSeleccion += new EventHandler<EventArgs>(this.OnExportPalletSeleccion);
            View.ExportSerialesSeleccion += new EventHandler<EventArgs>(this.OnExportSerialesSeleccion);
            View.ConsultarTecnicos += new EventHandler<EventArgs>(this.OnGetListTecnicos);
            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }
            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'DIAGNOSTICO', 'CLARO'", "", "dbo.Ubicaciones", Local);

            CargarDatosDetails();
            OcultarPestanas();

            View.Model.ListRecordsAddToPallet = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARMERCANCIAENTREGADIAG', '', '',''", "", "dbo.EquiposClaro", Local);

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            this.Actualizar_UbicacionDisponible();
            #endregion
        }

       
        #region Metodos

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
                Console.WriteLine(ex.Message);
            }
        }

        private void OnConfirmarImpresion(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String unidad_almacenamiento = "";
            String codigoEmp = View.CodigoEmpaque.Text.ToString();
            String destino = "";

            //Evaluo que haya sido seleccionado un registro
            //if (View.ListadoItems.SelectedIndex == -1)
            //{
            //    Util.ShowMessage("Debe seleccionar al menos un registro");
            //    return;
            //}

            if (View.Model.ListRecordsAddToPallet.Rows.Count == 0 )
            {
                Util.ShowMessage("No hay registros para empacar.");
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
            ConsultaSQL = "SELECT idPallet,Posicion,serial,Mac,Codigo_SAP,ProductoID,Estado,Fecha_Ingreso FROM dbo.EquiposCLARO WHERE serial IN (''";

            //Recorro el listado de registros seleccionados para obtener los seriales e imprimirlos
            foreach (DataRowView Registros in View.ListadoItemsAgregados.Items)
            {
                //Util.ShowMessage(Registros.Row["Serial"].ToString());
                //Creo la consulta para cambiar la ubicacion de la estiba

                ConsultaSQL += ",'" + Registros.Row["Serial"] + "'";
            }

            //Completo la consulta
            ConsultaSQL += ")";

            //Elimino la basura en la cadena
            ConsultaSQL = ConsultaSQL.Replace("'',", "");

            try
            {
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

                //Imprimo los registros
                PrinterControl.PrintMovimientosBodega(this.userName,SerialesImprimir, unidad_almacenamiento, codigoEmp, "REPARACIÓN", "CLARO", "DIAGNÓSTICO - " + destino, "", "");
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Se presento un error en el momento de generar el documento, " + ex.Message);
            }
        }

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            OnBuscarEquipos(sender, e);
        }

        private void OnGenerarCodigo(object sender, EventArgs e)
        {
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
                    View.CodigoEmpaque.Text = "RES-D" + Resultado.Rows[0]["idpallet"].ToString();

                    ////Limpio los seriales para digitar nuevos datos
                    //View.GetSerial1.Text = "";
                    //View.GetSerial2.Text = "";
                    //View.GetSerial1.Focus();
                }
            }
            else
            {
                Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            }
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

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModulo("DIAGNOSTICO", null);

            View.Model.ListRecords_1 = ds.Tables[0];

        }


        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            //DataTable RegistroValidado = null;
            String ConsultaBuscar = "";

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()) && String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
            {
                Util.ShowError("El campo serial y mac no pueden ser vacios.");
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
                    View.GetSerial1.Focus();
                    return;
                }

                if (View.GetSerial2.Text == item["Mac"].ToString())
                {
                    Util.ShowError("La dirección MAC " + View.GetSerial2.Text + " ya esta en el listado.");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }

            try
            {
                //Validacion existe o no el equipo en DB
                ConsultaBuscar = "SELECT * FROM dbo.EquiposCLARO WHERE Serial = '" + View.GetSerial1.Text.ToString() + "'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);
                Console.WriteLine(ConsultaBuscar);
                if (Resultado.Rows.Count > 0)
                {
                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    //RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERAL','" + ((String.IsNullOrEmpty(View.GetSerial1.Text.ToString())) ? "'"+View.Ge//tSerial2.Text+"'" : "'"+View.GetSerial1.Text+"'") + "'", "", "dbo.EquiposCLARO", Local);

                    if (Resultado.Rows[0]["Estado"].ToString() != "PARA PROCESO DIAGNOSTICO")
                    {
                        Util.ShowError("El serial ingresado no esta Diagnosticado, El serial se encuentra en estado " + Resultado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Asigno los campos           
                        dr["RowID"] = Resultado.Rows[0]["RowID"].ToString();
                        dr["Producto"] = Resultado.Rows[0]["Producto"].ToString();
                        dr["Serial"] = Resultado.Rows[0]["Serial"].ToString();
                        dr["Mac"] = Resultado.Rows[0]["Mac"].ToString();
                        dr["Estado"] = Resultado.Rows[0]["Estado"].ToString();

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
                else
                {
                    Util.ShowError("El serial no se encuentra registrado en el sistema.");
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                    return;
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
                View.GetSerial1.Focus();
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

        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "";
            String ConsultaGuardarTrack = "";

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    //ContadorFilas++;

                    //if (ContadorFilas % 50 != 0)
                    //{
                        if (DataRow["Estatus_Diagnostico"].ToString() == "BUEN ESTADO")
                        {

                            if (String.IsNullOrEmpty(DataRow["ESTATUS_DIAGNOSTICO"].ToString()))
                            {
                                Util.ShowError("Por favor ingrese estatus de diagnostico");
                                return;
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'ETIQUETADO', Estado = 'P-ETIQUETADO'";
                                ConsultaGuardar += ", ESTATUS_DIAGNOSTICO = '" + DataRow["Estatus_Diagnostico"].ToString() + "', FALLA_DIAGNOSTICO = '" + DataRow["Falla_Diagnostico"].ToString() + "', DIAGNOSTICADOR = '" + App.curUser.UserName.ToString() + "', TECNICO_ASIGNADO_DIAG = '" + DataRow["Diagnosticador"].ToString() + "', OBSERVACIONES_DIAGNOSTICADOR = '" + DataRow["Observaciones_Diagnosticador"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoDiagnostico 'DIAGNOSTICO TERMINADO, BUEN ESTADO','DIAGNOSTICO','ETIQUETADO','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Diagnostico"].ToString() + "','" +
                                DataRow["Estatus_Diagnostico"].ToString() + "','" + DataRow["Diagnosticador"].ToString() + "','" + DataRow["Observaciones_Diagnosticador"].ToString() + "','" + this.user + "';";

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_DIAGNOSTICO = '" + DataRow["Estatus_Diagnostico"].ToString() + "', FECHA_DIAGNOSTICADO = CONVERT(VARCHAR(10),GETDATE(), 103)  WHERE ID_SERIAL='" + DataRow["RowID"].ToString() + "';";
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(DataRow["ESTATUS_DIAGNOSTICO"].ToString()))
                            {
                                Util.ShowError("Por favor ingrese estatus de diagnostico");
                                return;
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'DIAGNOSTICO', Estado = 'DIAGNOSTICO'";
                                ConsultaGuardar += ", ESTATUS_DIAGNOSTICO = '" + DataRow["Estatus_Diagnostico"].ToString() + "', FALLA_DIAGNOSTICO = '" + DataRow["Falla_Diagnostico"].ToString() + "', DIAGNOSTICADOR = '" + App.curUser.UserName.ToString() + "', TECNICO_ASIGNADO_DIAG = '" + DataRow["Diagnosticador"].ToString() + "', OBSERVACIONES_DIAGNOSTICADOR = '" + DataRow["Observaciones_Diagnosticador"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoDiagnostico 'DIAGNOSTICO TERMINADO, MAL ESTADO','DIAGNOSTICO','DIAGNOSTICO EMPAQUE','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Diagnostico"].ToString() + "','" +
                                DataRow["Estatus_Diagnostico"].ToString() + "','" + DataRow["Diagnosticador"].ToString() + "','" + DataRow["Observaciones_Diagnosticador"].ToString() + "','" + this.user + "';";

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_DIAGNOSTICO = '" + DataRow["Estatus_Diagnostico"].ToString() + "', FECHA_DIAGNOSTICADO = CONVERT(VARCHAR(10),GETDATE(), 103) WHERE ID_SERIAL='" + DataRow["RowID"].ToString() + "';";
                            }
                        }
                   
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
            string ConsultaSQL = "", ConsultaMov = "", NuevaUbicacion, NuevoEstado;

            //Evaluo que haya seleccionado la nueva clasificacion
            if (View.Ubicacion.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la nueva clasificacion.");
                return;
            }

            if (View.Model.ListRecordsAddToPallet.Rows.Count == 0)
            {
                Util.ShowMessage("No hay registros para empacar.");
                return;
            }
            if (String.IsNullOrEmpty(View.CodigoEmpaque.Text.ToString()))
            {
                Util.ShowMessage("Por favor genere un codigo un pallet");
                return;
            }

            //Asigno las variables concurrentes para cualquier opción que el usuario elija.
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();
            string UnidadAlmacenamiento = ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString();
            string idPallet = View.CodigoEmpaque.Text.ToString();
            string RowID = "";

            
            if (NuevaUbicacion == "REPARACION")
            {
                NuevoEstado = "PARA REPARACION";
            }
            else if (NuevaUbicacion == "ETIQUETADO")
            {
                NuevoEstado = "PARA ETIQUETADO";
            }
            else
            {
                NuevoEstado = "PARA ALMACENAMIENTO";
            }

            if (NuevoEstado == "PARA ALMACENAMIENTO")
            {
                ConsultaSQL += " UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Posicion = '' , Estado = '" + NuevoEstado + "', UA = '" + UnidadAlmacenamiento + "', CodigoEmpaque = '" + idPallet + "', idpallet = '" + idPallet + "' WHERE RowID IN (";

                foreach (DataRowView item in View.ListadoItemsAgregados.Items)
                {
                    RowID = item.Row["RowID"].ToString();
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += "'" + RowID + "',";

                    //Guardo en la tabla de movimientos el cambio de ubicación del equipo
                    ConsultaMov += "EXEC dbo.sp_InsertarNuevo_Movimiento 'EMPAQUE DIAGNOSTICO','DIAGNOSTICO','ALMACENAMIENTO','" + idPallet + "','" + RowID + "', 'DIAGNOSTICO', 'UBICACIONPRODUCCION','" + this.user + "','';";
                }
                ConsultaSQL += ");";
                ConsultaSQL = ConsultaSQL.Replace(",);", ");");
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                service.DirectSQLNonQuery(ConsultaMov, Local);

                ConsultaSQL = "";
                ConsultaMov = "";
            }
            else
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += " UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" + UnidadAlmacenamiento + "', CodigoEmpaque = '" + idPallet + "', idPallet = '" + idPallet + "' WHERE RowID IN (";
                foreach (DataRowView item in View.ListadoItemsAgregados.Items)
                {
                    RowID = item.Row["RowID"].ToString();
                    ConsultaSQL += "'" + RowID + "',";
                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaMov += "EXEC sp_InsertarNuevo_Movimiento 'EMPAQUE DIAGNOSTICO','DIAGNOSTICO','" + NuevoEstado + "','" + idPallet + "','" + RowID + "','DIAGNOSTICO','UBICACIONPRODUCCION','" + this.user + "','';";
                }
                ConsultaSQL += ");";
                ConsultaSQL = ConsultaSQL.Replace(",);", ");");
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                service.DirectSQLNonQuery(ConsultaMov, Local);

                ConsultaSQL = "";
                ConsultaMov = "";
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicación realizado satisfactoriamente.");

            View.GetListaEstado.SelectedItem = "...";

            //Quito la selecion de la nueva ubicacion
            View.Ubicacion.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.ListadoItems.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.UnidadAlmacenamiento.SelectedIndex = -1;
            View.Model.ListRecordsAddToPallet.Rows.Clear();
            View.CodigoEmpaque.Text = "";
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

            #region Columna Falla Diagnostico

            IList<MMaster> ListadoFallaDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLADIA" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Falla Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFallaDiagnostico);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Falla_Diagnostico"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDiagnostico)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Falla_Diagnostico", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Estatus Diagnostico

            /*IList<MMaster> ListadoStatusDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTATUSD" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Status Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoStatusDiagnostico);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Estatus_Diagnostico"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDiagnostico)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Estatus_Diagnostico", typeof(String)); //Creacion de la columna en el DataTable*/

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Status Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Estatus_Diagnostico"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Estatus_Diagnostico", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Diagnosticador

            IList<SysUser> ListadoTecnicoDiagnosticadorCalidad = service.GetSysUser(new SysUser());
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Diagnosticador";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoTecnicoDiagnosticadorCalidad);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "FullDesc");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "UserName");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Diagnosticador"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Diagnosticador", typeof(String)); //Creacion de la columna en el DataTable

            ////IList<MMaster> ListadoDiagnosticador = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DIAGNOSTI" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.TextBlock";
            //Columna.Header = "Diagnosticador";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(TextBlock.TextProperty, App.curUser.UserName.ToString());
            ////Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            ////Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            ////Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Diagnosticador"));
            //Txt.SetValue(TextBlock.WidthProperty, (double)110);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("Diagnosticador", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Observaciones Diagnosticador

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Observaciones Diagnosticador";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)120);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Observaciones_Diagnosticador"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Observaciones_Diagnosticador", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnValidarDiagnostico(object sender, SelectionChangedEventArgs e)
        {
            //Recorro el listado re registros para buscar el ComboBox que disparo el evento
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                //Valido si el Tag es el mismo Serial
                if (dr[2].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    //Valido el diagnostico seleccionado para asignar el estado del diagnostico
                    if (((MMaster)((ComboBox)sender).SelectedItem).Code.ToString() == "SF")
                        dr[6] = "BUEN ESTADO";
                    else
                        dr[6] = "MAL ESTADO";
                    break;
                }
            }
            return;
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
            ConsultaSQL = "EXEC sp_GetProcesos 'ACTUALIZARMERCANCIADIAGNOS', 'PARA DIAGNOSTICO' ";

            //Valido si fue digitado una estiba para buscar
            if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
                ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString().Replace("'", "-") + "'";
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
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA DIAGNOSTICO', NULL, NULL";

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
                //Creo la consulta para confirmar el cambio de ubicacion de la estiba  " + Registros.Row["Posicion"] + "
                ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEPOSICIONDIAGNOSTICO','','PARA PROCESO DIAGNOSTICO', 'DIAGNOSTICO','" + Registros.Row["Estiba"].ToString() + "';";

                //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                ConsultaSQL += "EXEC sp_InsertarNuevo_Movimiento 'RECIBO DE MERCANCIA DIAGNOSTICO','','DIAGNOSTICO','" + Registros.Row["Estiba"].ToString() + "','','DIAGNOSTICO','UBICACIONALMACEN','" + this.user + "','';";

                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                ConsultaSQL = "";
            }

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //Busco los registros para actualizar el listado
            BuscarRegistrosRecibo();

            View.Model.ListadoRecibo.Clear();
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

            string consultaSQL = "";
            string pallet = ((DataRowView)View.ListadoBusquedaRecibo.SelectedItem).Row["Estiba"].ToString();

            consultaSQL = "EXEC sp_GetProcesos 'BUSCARSERIALESESTIBARECIBODIAG', '" + pallet + "'";
            View.Model.Listado_PalletSerial = service.DirectSQLQuery(consultaSQL, "", "dbo.EquiposClaro", Local);
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
                Console.WriteLine("INDICE " + index);
                if (View.ListadoEquiposAProcesar.SelectedIndex != -1)
                {

                    if (View.ListadoEquiposAProcesar.SelectedItems.Count > 1)// Se selecciona mas de una fila
                    {
                        DataRowView drv = (DataRowView)View.ListadoEquiposAProcesar.SelectedItem;
                        String valueOfItem = drv[index].ToString();

                        if (index >= offset && index != 6)
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
                        if (index >= offset && index != 6)
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

        public void OcultarPestanas()
        {
            if (App.curUser.UserRols.Where(f => f.Rol.RolCode == "ADMIN" || f.Rol.RolCode == "CLARODIAG").Count() == 0)
            {
                View.EntregaDiagnostico.IsEnabled = false;
                //View.ReciboDiagnostico.IsEnabled = false;
                //Util.ShowError("No puede realizar esta accion");
                //return;
            }
        }

        private void OnAddToList(object sender, EventArgs e)
        {
            DataRow dr;
            int aux = 0;
            string ConsultaAgregar = "";
            string RowID = "";
            ConsultaAgregar += "UPDATE dbo.EquiposCLARO set Estado = 'DIAGNOSTICO_ENTREGA' WHERE RowID IN (";
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

                    dr["Producto"] = item.Row["Producto"].ToString();
                    dr["Serial"] = item.Row["Serial"].ToString();
                    dr["Mac"] = item.Row["Mac"].ToString();
                    dr["Estado"] = item.Row["Estado"].ToString();
                    dr["Tecnico"] = item.Row["Tecnico"].ToString();
                    dr["RowID"] = item.Row["RowID"].ToString();

                    View.Model.ListRecordsAddToPallet.Rows.Add(dr);

                    //cambio el estado para no mostrar mas en el listado general
                     ConsultaAgregar += "'" + RowID + "',";
                }
            }
            ConsultaAgregar += ");";
            ConsultaAgregar = ConsultaAgregar.Replace(",);", ");");
            service.DirectSQLNonQuery(ConsultaAgregar, Local);
            ConsultaAgregar = "";

            while (View.ListadoItems.SelectedItems.Count > 0)
            {
                View.Model.ListRecords_1.Rows.RemoveAt(View.ListadoItems.Items.IndexOf(View.ListadoItems.SelectedItem));
            }
            OnBuscarEquipos(sender, e);
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
                ConsultaAgregar = "UPDATE dbo.EquiposCLARO SET Estado = 'DIAGNOSTICO' where RowID = " + itemAdd.Row["RowID"];
                service.DirectSQLNonQuery(ConsultaAgregar, Local);
                ConsultaAgregar = "";
            }
            // Elimino el registro de la lista
            while (View.ListadoItemsAgregados.SelectedItems.Count > 0)
            {
                View.Model.ListRecordsAddToPallet.Rows.RemoveAt(View.ListadoItemsAgregados.Items.IndexOf(View.ListadoItemsAgregados.SelectedItem));
            }
            OnBuscarEquipos(sender, e);
        }

        private void OnGetListTecnicos(object sender, EventArgs e)
        {
            GetListTecnicos();
        }

        private void GetListTecnicos()
        {
            string consultaSQL = "";
            DataTable listTecnicos = null;

            consultaSQL = "SELECT TECNICO_ASIGNADO_DIAG AS Tecnico FROM dbo.EquiposCLARO WHERE Estado = 'DIAGNOSTICO' GROUP BY TECNICO_ASIGNADO_DIAG";
            listTecnicos = service.DirectSQLQuery(consultaSQL, "", "dbo.EquiposCLARO", Local);

            View.Model.ListadoTecnicoReparacion = new DataTable("ListadoTecnicoReparacion");
            View.Model.ListadoTecnicoReparacion.Columns.Add("Tecnico", typeof(string));

            try
            {
                foreach (DataRow item in listTecnicos.Rows)
                {
                    string cadena = item["Tecnico"].ToString();

                    if (cadena.Contains(","))
                    {
                        string[] split = cadena.Split(new Char[] { ',' });
                        cadena = split.First();
                        View.Model.ListadoTecnicoReparacion.Rows.Add(cadena);
                    }
                    else
                    {
                        View.Model.ListadoTecnicoReparacion.Rows.Add(cadena);
                    }
                }
                View.Model.ListadoTecnicoReparacion = View.Model.ListadoTecnicoReparacion.DefaultView.ToTable(true);
            }
            catch (Exception ex)
            {
                Util.ShowError("Error: " + ex.Message);
            }
        }

        public void OnBuscarEquipos(object sender, EventArgs e)
        {
            string filtroTecnico = ((DataRowView)View.cbo_FilterByWorker.SelectedItem).Row["Tecnico"].ToString();
            string selectedItem = ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString();
            string consultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAFILTRADADIAG', '" + selectedItem + "', '" + filtroTecnico + "';";

            try
            {
                // Ejecuto la consulta 
                View.Model.ListRecords_1 = service.DirectSQLQuery(consultaSQL, "", "dbo.EquiposCLARO", Local);
            }
            catch (Exception ex)
            {
                Util.ShowError("Ocurrio un error, ERROR: " + ex.Message);
            }
            //Valido el estado por el cual se filtro para filtrar el destino al que pueden ir los items
            if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() == "MAL ESTADO")
            {
                View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONFILTRADA', 'DIAGNOSTICO', 'CLARO', 'REPARACION','ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
            }

            int equiposCargados = View.Model.ListRecords_1.Rows.Count;
            if (equiposCargados < 1)
            {
                View.TXT_filterResults.Text = "No se encontraron equipos reparados por el usuario " + filtroTecnico.ToUpper();
            }
            else if (equiposCargados == 1)
            {
                View.TXT_filterResults.Text = equiposCargados.ToString() + " equipo fue reparado por el usuario " + filtroTecnico.ToUpper();
            }
            else
            {
                View.TXT_filterResults.Text = equiposCargados + " equipos reparados por el usuario " + filtroTecnico.ToUpper();
            }

            View.TXT_filterResults.Visibility = Visibility.Visible;
            View.ListadoItems.Visibility = Visibility.Visible;
            View.StackListaEquiposEntrega.Visibility = Visibility.Visible;
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
                int filas_seleccion = View.ListadoBusquedaRecibo.SelectedItems.Count;

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

                    foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
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
    }
}
