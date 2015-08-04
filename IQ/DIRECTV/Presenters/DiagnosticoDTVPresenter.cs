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

    public interface IDiagnosticoDTVPresenter
    {
        IDiagnosticoDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class DiagnosticoDTVPresenter : IDiagnosticoDTVPresenter
    {
        public IDiagnosticoDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 3; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public DiagnosticoDTVPresenter(IUnityContainer container, IDiagnosticoDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DiagnosticoDTVModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);
            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;
            View.FilaSeleccionada += this.OnFilaSeleccionada;
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.ListarEquiposSeleccion += new EventHandler<EventArgs>(this.OnListarEquiposSeleccion);
            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'UBICACIONESDESTINO', 'DIAGNOSTICO', 'CLARO'", "", "dbo.Ubicaciones", Local);

            CargarDatosDetails();
            ListarDatos();

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //foreach (DataRowView item in View.ListadoItems.SelectedItems)
            //{
            //    if (item.Row["Estado"] == "MAL ESTADO")
            //    {
            //        View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV 'UBICACIONESDiagnosticoDTV', 'DiagnosticoDTV', 'CLARO'", "REPARACION", "dbo.Ubicaciones", Local);
            //    }
            //    else
            //    {
            //        View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV 'UBICACIONESDiagnosticoDTV', 'DiagnosticoDTV', 'CLARO'", "VERIFICACION", "dbo.Ubicaciones", Local);
            //    }
            //}

            //ListarDatos();
            FiltrarDatosEntrega();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModuloDIRECTVC("DIAGNOSTICO", null);

            View.Model.ListRecords_1 = ds.Tables[0];

        }

        private void FiltrarDatosEntrega()
        {

            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAFILTRADA', '" + ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() + "', 'DIAGNOSTICO'";

            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            //Valido el estado por el cual se filtro para filtrar el destino al que pueden ir los items
            if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() == "MAL ESTADO")
            {
                View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'UBICACIONFILTRADA', 'DIAGNOSTICO', 'CLARO', 'REPARACION'", "", "dbo.Ubicaciones", Local);
            }

        }


        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
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
                    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "PARA PROCESO DIAGNOSTICO")
                    {
                        Util.ShowError("El serial ingresado no se encuentra para Diagnosticar, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
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
            //Obtenemos el indice del encabezado
            if (View.ListadoEquiposAProcesar.SelectedIndex != -1)
            {
                if (View.ListadoEquiposAProcesar.SelectedItems.Count > 1)// Se selecciona mas de una fila
                {
                    int indice_fila1 = View.ListadoEquiposAProcesar.SelectedIndex;

                    //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                    foreach (DataRowView dr in View.ListadoEquiposAProcesar.SelectedItems)
                    {
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            dr.Row[z] = View.Model.ListRecords.Rows[indice_fila1][z];
                    }
                }
                else
                {
                    int SComp;
                    SComp = View.ListadoEquiposAProcesar.SelectedIndex;
                    //recorre la lista desde la fila seleccionada y setea las filas que esten vacias
                    for (int i = SComp; i < View.Model.ListRecords.Rows.Count; i++)
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            if (View.Model.ListRecords.Rows[i][z].ToString().Equals(""))
                                View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[SComp][z];
                }
            }
            else
            {
                Util.ShowMessage("Debe seleccionar una fila para replicar la informacion");
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
            Int32 ContadorFilas = 0;


            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;


                    

                        if (DataRow["ESTATUS_DIAGNOSTICO"].ToString() == "BUEN ESTADO")
                        {
                            if (String.IsNullOrEmpty(DataRow["ESTATUS_DIAGNOSTICO"].ToString()))
                            {
                                Util.ShowError("Por favor ingrese estatus de diagnostico");
                                return;
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'ETIQUETADO', Estado = 'P-ETIQUETADO'";
                                ConsultaGuardar += ", TIPO_DIAGNOSTICO = '" + DataRow["TIPO_DIAGNOSTICO"].ToString() + "', FALLA_DIAGNOSTICO = '" + DataRow["FALLA_DIAGNOSTICO"].ToString() + "', TECNICO_DIAG = '" + App.curUser.UserName.ToString() + "', TECNICO_ASIGNADO_DIAG = '" + DataRow["TECNICO_DIAG"].ToString() + "', ESTATUS_DIAGNOSTICO = '" + DataRow["ESTATUS_DIAGNOSTICO"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoDiagnosticoDIRECTV 'DIAGNOSTICO TERMINADO, BUEN ESTADO','DIAGNOSTICO','ETIQUETADO','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Diagnostico"].ToString() + "','" +
                                DataRow["Estatus_Diagnostico"].ToString() + "','" + DataRow["TECNICO_DIAG"].ToString() + "','','" + this.user + "';";

                                Console.WriteLine("###### " + ConsultaGuardar);

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_DIAGNOSTICO = '" + DataRow["ESTATUS_DIAGNOSTICO"].ToString() + "', FECHA_DIAGNOSTICADO = '" + DateTime.Now.ToString("dd/MM/yyyy") + "' WHERE ID_SERIAL='" + DataRow["RowID"].ToString() + "';";

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
                                ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'DIAGNOSTICO', Estado = 'DIAGNOSTICO'";
                                ConsultaGuardar += ", TIPO_DIAGNOSTICO = '" + DataRow["TIPO_DIAGNOSTICO"].ToString() + "', FALLA_DIAGNOSTICO = '" + DataRow["FALLA_DIAGNOSTICO"].ToString() + "', TECNICO_DIAG = '" + App.curUser.UserName.ToString() + "', TECNICO_ASIGNADO_DIAG = '" + DataRow["TECNICO_DIAG"].ToString() + "', ESTATUS_DIAGNOSTICO = '" + DataRow["ESTATUS_DIAGNOSTICO"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoDiagnosticoDIRECTV 'DIAGNOSTICO TERMINADO, MAL ESTADO','DIAGNOSTICO','DIAGNOSTICO EMPAQUE','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Diagnostico"].ToString() + "','" +
                                DataRow["Estatus_Diagnostico"].ToString() + "','" + DataRow["TECNICO_DIAG"].ToString() + "','','" + this.user + "';";

                                Console.WriteLine("###### " + ConsultaGuardar);

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_DIAGNOSTICO = '" + DataRow["ESTATUS_DIAGNOSTICO"].ToString() + "', FECHA_DIAGNOSTICADO = '" + DateTime.Now.ToString("dd/MM/yyyy") + "' WHERE ID_SERIAL='" + DataRow["RowID"].ToString() + "';";

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
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado;

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
                NuevoEstado = "ALMACENAMIENTO";
            }

            if (NuevoEstado == "PARA ALMACENAMIENTO")
            {
                foreach (DataRowView item in View.ListadoItems.SelectedItems)
                {
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" 
                        + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString() + "', CodigoEmpaque = '" 
                        + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" + item.Row["RowID"] + "';";

                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE DIAGNOSTICO','DIAGNOSTICO','ALMACENAMIENTO','" 
                        + View.CodigoEmpaque.Text.ToString() + "','" + item.Row["RowID"] + "','DIAGNOSTICO','UBICACIONPRODUCCION','" + this.user + "','';";
                    Console.WriteLine("###### " + ConsultaSQL);

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    ConsultaSQL = "";
                }
            }
            else
            {
                foreach (DataRowView item in View.ListadoItems.SelectedItems)
                {
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "',Estado = '" + NuevoEstado + "', UA = '" 
                        + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString() + "', CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() +
                        "', idpallet = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" + item.Row["RowID"] + "';";

                    //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                    ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'EMPAQUE DIAGNOSTICO','DIAGNOSTICO','" + NuevoEstado + "','" 
                        + View.CodigoEmpaque.Text.ToString() + "','" + item.Row["RowID"] + "','DIAGNOSTICO','UBICACIONPRODUCCION','" + this.user + "','';";
                    Console.WriteLine("###### " + ConsultaSQL);

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    ConsultaSQL = "";
                }
            }
            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            ListarDatos();

            View.GetListaEstado.SelectedItem = "...";

            //Quito la selecion de la nueva ubicacion
            View.Ubicacion.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.ListadoItems.SelectedIndex = -1;

            //Quito la seleccion del listado
            View.UnidadAlmacenamiento.SelectedIndex = -1;

            View.CodigoEmpaque.Text = "";

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
            //DataTable RegistroValidado;
            //String ConsultaBuscar = "";

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
            //        View.CodigoEmpaque.Text = "RES-D" + Resultado.Rows[0]["idpallet"].ToString();

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
            ConsultaSQL = "SELECT idPallet,Posicion,serial,Receiver,SMART_CARD_ENTRADA,MODELO,Estado,Fecha_Ingreso FROM dbo.EquiposDIRECTVC WHERE serial IN (''";

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

            try
            {
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

                //Imprimo los registros
                PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, unidad_almacenamiento, codigoEmp, "REPARACIÓN", "DIRECTV", "DIAGNÓSTICO - " + destino, "", "");
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Se presento un error en el momento de generar el documento, " + ex.Message);
            }
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

            #region Columna Estatus Diagnostico

            //IList<MMaster> ListadoStatusDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTATUSD" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Status Diagnostico";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoStatusDiagnostico);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Estatus_Diagnostico"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);


            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("Estatus_Diagnostico", typeof(String)); //Creacion de la columna en el DataTable


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

            #region Columna Tipo Diagnostico

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Tipo Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("TIPO_DIAGNOSTICO"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("TIPO_DIAGNOSTICO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Falla Equipo

            IList<MMaster> ListadoFallaEquipoDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLADIR" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Falla Equipo";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFallaEquipoDiagnostico);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("FALLA_DIAGNOSTICO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDiagnostico)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("FALLA_DIAGNOSTICO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Tecnico Diagnosticador

            //IList<Rol> ObtRol1 = service.GetRol(new Rol { RolCode = "DTVDIAG" });
            //String ObtRol = service.GetUserByRol(new UserByRol { Rol = (Rol)ObtRol1 }).Select(f => f.Rol).ToString();
            //IList<SysUser> ListadoTecnicoDiagnosticadorCalidad = service.GetSysUser(new SysUser { UserRols = (List<UserByRol>)ObtRol });

            IList<SysUser> ListadoTecnicoDiagnosticadorCalidad = service.GetSysUser(new SysUser());
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Tecnico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoTecnicoDiagnosticadorCalidad);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "FullDesc");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "UserName");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("TECNICO_DIAG"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("TECNICO_DIAG", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'ACTUALIZARMERCANCIADIAGNOS', 'PARA DIAGNOSTICO' ";

            //Valido si fue digitado una estiba para buscar
            if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
                ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            else
                ConsultaSQL += ",NULL";


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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIADIAGNOSTICO', 'PARA DIAGNOSTICO', NULL, NULL";

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
                ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATEPOSICIONDIAGNOSTICO','" + Registros.Row["Posicion"] + "','PARA PROCESO DIAGNOSTICO', 'DIAGNOSTICO','" + Registros.Row["Estiba"].ToString() + "';";
                ConsultaSQL += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'RECIBO DE MERCANCIA DIAGNOSTICO','','DIAGNOSTICO','" + Registros.Row["Estiba"].ToString() + "','','DIAGNOSTICO','UBICACIONALMACEN','" + this.user + "','';";
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

        private void OnValidarDiagnostico(object sender, SelectionChangedEventArgs e)
        {
            //Recorro el listado re registros para buscar el ComboBox que disparo el evento
            foreach (DataRowView dr in View.ListadoEquiposAProcesar.Items)
            {
                //Valido si el Tag es el mismo Serial
                if (dr[2].ToString() == ((ComboBox)sender).Tag.ToString())
                {
                    if (((MMaster)((ComboBox)sender).SelectedItem).Code.ToString() == "SIN FALLA")
                        dr[5] = "BUEN ESTADO";
                    else
                        dr[5] = "MAL ESTADO";
                    break;
                }
            }
            return;
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

        private void OnListarEquiposSeleccion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
                return;

            string aux_idPallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Estiba"].ToString();

            String Consulta = "SELECT IdPallet as PPallet, "
            + "serial as PSerial, "
            + "RECEIVER as Receiver, "
            + "SMART_CARD_ENTRADA as PMac,  "
            + "MODELO as Modelo,  "
            + "TIPO_ORIGEN as PTRecibo, "
            + "convert(VARCHAR,FECHA_INGRESO,120) as PFRegistro,  "
            + "DATEDIFF(day, FECHA_INGRESO,GETDATE()) as NumeroDias,dbo.TIMELAPSELEO(FECHA_INGRESO) as horas "
            + "from dbo.EquiposDIRECTVC WHERE ((IdPallet IS NOT NULL) AND (Posicion IS NOT NULL) AND (ESTADO = 'PARA DIAGNOSTICO')) "
            + " AND IdPallet = '" + aux_idPallet + "'";

            Console.WriteLine(Consulta);

            View.Model.Listado_PalletSerial =
            service.DirectSQLQuery(Consulta, "", "dbo.EquiposDIRECTVC", Local);

        }

        #endregion
    }
}