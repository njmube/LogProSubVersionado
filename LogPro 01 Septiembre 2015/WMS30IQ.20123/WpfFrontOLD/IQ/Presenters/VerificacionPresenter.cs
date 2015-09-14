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

    public interface IVerificacionPresenter
    {
        IVerificacionView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class VerificacionPresenter : IVerificacionPresenter
    {
        public IVerificacionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 3; //# columnas que no se debe replicar porque son fijas.

        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public VerificacionPresenter(IUnityContainer container, IVerificacionView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<VerificacionModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += this.OnConfirmarMovimiento;
            View.ConfirmarImpresion += new EventHandler<EventArgs>(this.OnConfirmarImpresion);
            View.Imprimir += this.OnImprimir;
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

            #endregion

            #region Datos

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'VERIFICACION', 'CLARO'", "", "dbo.Ubicaciones", Local);
            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            CargarDatosDetails();
            ListarDatos();


            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            FiltrarDatosEntrega();
            ListarDatos();
        }

        private void FiltrarDatosEntrega()
        {

            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIA_VERIFICACION', '" + ((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() + "', 'VERIFICACION'";

            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            //Valido el estado por el cual se filtro para filtrar el destino al que pueden ir los items
            if (((ComboBoxItem)View.GetListaEstado.SelectedItem).Content.ToString() == "DAÑADO")
            {
                View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONFILTRADA', 'DIAGNOSTICO', 'CLARO', 'ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
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
            ConsultaSQL = "SELECT idPallet,Posicion,serial,Mac,Codigo_SAP,ProductoID,fecha_ingreso FROM dbo.EquiposCLARO WHERE serial IN (''";

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
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);

            //foreach (var item in SerialesImprimir.Rows)
            //{
            //    Util.ShowMessage(item.ToString());
            //}

            //foreach (DataColumn item in SerialesImprimir.Columns)
            //{
            //    Util.ShowMessage(item.ColumnName);
            //}

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
            PrinterControl.PrintMovimientosBodega( this.userName, SerialesImprimir, unidad_almacenamiento, codigoEmp, destino, "CLARO", "VERIFICACIÓN - ALMACENAMIENTO", "DAÑADOS", "");
        }

        private void OnDeleteDetails(object sender, EventArgs e)
        {
            if (View.ListadoEquiposAProcesar.SelectedItems.Count == 0)
                Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");

            //borra de la lista los elementos seleccionados
            while (View.ListadoEquiposAProcesar.SelectedItems.Count > 0)
                View.Model.ListRecords.Rows.RemoveAt(View.ListadoEquiposAProcesar.Items.IndexOf(View.ListadoEquiposAProcesar.SelectedItem));
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

            String ConsultaValidar = "SELECT * FROM dbo.EquiposCLARO WHERE Serial = '" + View.GetSerial1.Text.ToString() + "'";
            DataTable ValidarExistencia = service.DirectSQLQuery(ConsultaValidar, "", "dbo.EquiposCLARO", Local);

            if (ValidarExistencia.Rows.Count == 0)
            {
                Util.ShowError("El serial no se encuentra registrado en el sistema.");
                View.GetSerial1.Text = "";
                View.GetSerial2.Text = "";
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
                    View.GetSerial1.Focus();
                    return;
                }
            }

            try
            {
                //Validacion existe o no el equipo en DB
                ConsultaBuscar = "SELECT * FROM dbo.EquiposCLARO WHERE Serial = '" + View.GetSerial1.Text.ToString() + "' AND Estado != 'DESPACHADO'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count > 0)
                {

                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOGENERAL','" + View.GetSerial1.Text + "'", "", "dbo.EquiposCLARO", Local);

                    //Evaluo si el serial existe
                    if (RegistroValidado.Rows.Count == 0)
                    {
                        Util.ShowError("El serial no existe o no esta en la ubicacion requerida.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else if (RegistroValidado.Rows[0]["Estado"].ToString() != "P-VERIFICACION")
                    {
                        Util.ShowError("El serial ingresado no esta en Verificacion, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Asigno los campos
                        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                        dr["Producto"] = RegistroValidado.Rows[0]["Producto"].ToString();
                        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                        dr["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString();
                        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();

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
            DataSet ds = Util.GetListaDatosModulo("VERIFICACION", null);
            View.Model.ListRecords2 = ds.Tables[0];
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
                    View.CodigoEmpaque.Text = "RES-V" + Resultado.Rows[0]["idpallet"].ToString();

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

                        if (index >= offset)
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
                        if (index >= offset)
                        {
                            for (int i = View.ListadoEquiposAProcesar.SelectedIndex; i < View.Model.ListRecords.Rows.Count; i++)
                                View.Model.ListRecords.Rows[i][index] = View.Model.ListRecords.Rows[View.ListadoEquiposAProcesar.SelectedIndex][index];
                        }
                    }
                }
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
                    //ContadorFilas++;

                    //if (ContadorFilas % 50 != 0)
                    //{

                        if (DataRow["ESTATUS_VERIFICACION"].ToString() == "VERIFICADO")
                        {
                            if (String.IsNullOrEmpty(DataRow["ESTATUS_VERIFICACION"].ToString()))
                            {
                                Util.ShowError("Por favor ingrese estatus de diagnostico");
                                return;
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'EMPAQUE', Estado = 'P-EMPAQUE'";
                                ConsultaGuardar += ", ESTATUS_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', TECNICO_DIAGNOSTICADOR_VERIF = '" + App.curUser.UserName.ToString() + "', FALLA_VERIFICACION = '" + DataRow["Falla_Verificacion"].ToString() + "', OBSERVACIONES_VERIFICACION = '" + DataRow["Observaciones_Verificacion"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', FECHA_VERIFICADO =  CONVERT(VARCHAR(10),GETDATE(), 103) WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";

                                String destino = "";
                                if (DataRow["Estatus_Verificacion"].ToString() == "DAÑADO") { destino = "VERIFICACIÓN EMPAQUE"; } else { destino = "EMPAQUE"; }

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoVerificacion 'VERIFICACIÓN TERMINADA, " + DataRow["Estatus_Verificacion"].ToString() + "','VERIFICACION','" + destino + "','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Verificacion"].ToString() + "','" +
                                DataRow["Estatus_Verificacion"].ToString() + "','" + App.curUser.UserName.ToString() + "','" + DataRow["Observaciones_Verificacion"].ToString() + "','" + this.user + "';";

                                Console.WriteLine("###### " + ConsultaGuardar);
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(DataRow["ESTATUS_VERIFICACION"].ToString()))
                            {
                                Util.ShowError("Por favor ingrese estatus de diagnostico");
                                return;
                            }
                            else
                            {
                                //Construyo la consulta para guardar los datos
                                ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'VERIFICACION', Estado = 'VERIFICACION'";
                                ConsultaGuardar += ", ESTATUS_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', TECNICO_DIAGNOSTICADOR_VERIF = '" + App.curUser.UserName.ToString() + "', FALLA_VERIFICACION = '" + DataRow["Falla_Verificacion"].ToString() + "', OBSERVACIONES_VERIFICACION = '" + DataRow["Observaciones_Verificacion"].ToString() + "'";
                                ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

                                ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', FECHA_VERIFICADO = CONVERT(VARCHAR(10),GETDATE(), 103)  WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";

                                String destino = "";
                                if (DataRow["Estatus_Verificacion"].ToString() == "DAÑADO") { destino = "VERIFICACIÓN EMPAQUE"; } else { destino = "EMPAQUE"; }

                                ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoVerificacion 'VERIFICACIÓN TERMINADA, " + DataRow["Estatus_Verificacion"].ToString() + "','VERIFICACION','" + destino + "','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Verificacion"].ToString() + "','" +
                                DataRow["Estatus_Verificacion"].ToString() + "','" + App.curUser.UserName.ToString() + "','" + DataRow["Observaciones_Verificacion"].ToString() + "','" + this.user + "';";

                                Console.WriteLine("###### " + ConsultaGuardar);
                            }
                        }
                    //}
                    //else
                    //{
                    //    //Construyo la consulta para guardar los datos
                    //    ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'VERIFICACION', Estado = 'VERIFICACION'";
                    //    ConsultaGuardar += ", ESTATUS_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', TECNICO_DIAGNOSTICADOR_VERIF = '" + App.curUser.UserName.ToString() + "', FALLA_VERIFICACION = '" + DataRow["Falla_Verificacion"].ToString() + "', OBSERVACIONES_VERIFICACION = '" + DataRow["Observaciones_Verificacion"].ToString() + "'";
                    //    ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

                    //    ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_VERIFICACION = '" + DataRow["Estatus_Verificacion"].ToString() + "', FECHA_VERIFICADO = CONVERT(VARCHAR(10),GETDATE(), 103)  WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";

                    //    String destino = "";
                    //    if (DataRow["Estatus_Verificacion"].ToString() == "DAÑADO") { destino = "VERIFICACIÓN EMPAQUE"; } else { destino = "EMPAQUE"; }

                    //    ConsultaGuardar += "exec sp_InsertarNuevo_MovimientoVerificacion 'VERIFICACIÓN TERMINADA, " + DataRow["Estatus_Verificacion"].ToString() + "','VERIFICACION','" + destino + "','Sin pallet','" + DataRow["RowID"].ToString() + "','" + DataRow["Falla_Verificacion"].ToString() + "','" +
                    //    DataRow["Estatus_Verificacion"].ToString() + "','" + App.curUser.UserName.ToString() + "','" + DataRow["Observaciones_Verificacion"].ToString() + "','" + this.user + "';";

                    //    Console.WriteLine("###### " + ConsultaGuardar);

                    //    //Ejecuto la consulta
                    //    service.DirectSQLNonQuery(ConsultaGuardar, Local);
                    //    service.DirectSQLNonQuery(ConsultaGuardarTrack, Local);

                    //    //Limpio la consulta para volver a generar la nueva
                    //    ConsultaGuardar = "";
                    //    ConsultaGuardarTrack = "";
                    //}
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

                //Consulto los equipos que estan procesados en el modulo
                ListarDatos();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        private void OnConfirmarMovimiento(object sender, EventArgs e)
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

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();


            if (NuevaUbicacion == "ALMACENAMIENTO")
            {
                NuevoEstado = "PARA ALMACENAMIENTO";
            }
            else
            {
                NuevoEstado = "PARA ALMACENAMIENTO";
            }

            //if (NuevaUbicacion == "REPARACION")
            //{
            //    NuevoEstado = "PARA REPARACION";
            //}
            //else
            //{
            //    NuevoEstado = "PARA REPARACION";
            //}


            foreach (DataRowView item in View.ListadoItems.SelectedItems)
            {
                //Creo la consulta para cambiar la ubicacion de la estiba
                ConsultaSQL += " UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString() + "', CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" + item.Row["RowID"] + "'";
                ConsultaTrack += "UPDATE dbo.TrackEquiposCLARO SET CODEMPAQUE_VERIF = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE ID_SERIAL = '" + item.Row["RowID"] + "'";

                ConsultaSQL += "exec sp_InsertarNuevo_Movimiento 'EMPAQUE VERIFICACIÓN','VERIFICACIÓN','ALMACENAMIENTO','" + View.CodigoEmpaque.Text.ToString() + "','" + item.Row["RowID"] + "','VERIFICACION','UBICACIONPRODUCCION','" + this.user + "','';";

                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                service.DirectSQLNonQuery(ConsultaTrack, Local);

                ConsultaSQL = "";
                ConsultaTrack = "";
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

        private void OnImprimir(object sender, EventArgs e)
        {
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
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA VERIFICACION'";

            //Valido si fue digitado una estiba para buscar
            //if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
            //    ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            //Valido si fue seleccionado ubicaciones para filtrar
            //if (View.BuscarPosicionRecibo.SelectedIndex != -1)
            //    ConsultaSQL += ",'" + ((MMaster)View.BuscarPosicionRecibo.SelectedItem).Code.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Limpio los campos de la busqueda
            //View.BuscarEstibaRecibo.Text = "";
            //View.BuscarPosicionRecibo.SelectedIndex = -1;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIADIAGNOSTICO', 'PARA VERIFICACION', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnConfirmarRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Evaluo que haya sido seleccionado un registro
            //if (View.ListadoBusquedaRecibo.SelectedIndex == -1)
            //    return;

            ////Recorro el listado de registros seleccionados para confirmar el recibo
            //foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
            //{
            //    //Creo la consulta para confirmar el cambio de ubicacion de la estiba
            //    ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEPOSICION','" + Registros.Row["Posicion"] + "','VERIFICACION','VERIFICACION','" + Registros.Row["UA"].ToString() + "'";

            //    //Ejecuto la consulta
            //    service.DirectSQLNonQuery(ConsultaSQL, Local);
            //}

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //Busco los registros para actualizar el listado
            BuscarRegistrosRecibo();
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Falla Verificacion

            IList<MMaster> ListadoFallaDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLADIA" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Falla Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFallaDiagnostico);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("FALLA_VERIFICACION"));
            Txt.SetValue(ComboBox.WidthProperty, (double)150);
            Txt.SetBinding(ComboBox.TagProperty, new System.Windows.Data.Binding("Serial"));
            Txt.AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnValidarDiagnostico)); //NUEVO EVENTO

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("FALLA_VERIFICACION", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Estatus Verificacion

            //IList<MMaster> ListadoStatusDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTATUSVE" } });
            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            //TipoDato = "System.Windows.Controls.ComboBox";
            //Columna.Header = "Status Verificacion";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoStatusDiagnostico);
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Estatus_Verificacion"));
            //Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("Estatus_Verificacion", typeof(String)); //Creacion de la columna en el DataTable

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Status Verificacion";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ESTATUS_VERIFICACION"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTATUS_VERIFICACION", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Diagnosticador

            //IList<MMaster> ListadoDiagnosticador = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DIAGNOSTI" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBlock, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBlock";
            Columna.Header = "Diagnosticador";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBlock.TextProperty, App.curUser.UserName.ToString());
            //Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            //Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Diagnosticador"));
            Txt.SetValue(TextBlock.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Diagnosticador", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Observaciones Verificacion

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Observaciones Verificacion";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)200);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Observaciones_Verificacion"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Observaciones_Verificacion", typeof(String)); //Creacion de la columna en el DataTable

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
                        dr[6] = "VERIFICADO";
                    else
                        dr[6] = "DAÑADO";
                    break;
                }
            }
            return;
        }
        #endregion

    }
}