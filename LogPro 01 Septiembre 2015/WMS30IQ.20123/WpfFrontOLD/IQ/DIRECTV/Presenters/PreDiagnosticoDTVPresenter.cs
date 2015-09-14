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

    public interface IPreDiagnosticoDTVPresenter
    {
        IPreDiagnosticoDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class PreDiagnosticoDTVPresenter : IPreDiagnosticoDTVPresenter
    {
        public IPreDiagnosticoDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 3; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public PreDiagnosticoDTVPresenter(IUnityContainer container, IPreDiagnosticoDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<PreDiagnosticoDTVModel>();


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

            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
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
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            //ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAPREDIAG', '', ''";
            //service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV2 'BUSCARMERCANCIAPREDIAG', '', '',''", "", "dbo.EquiposDIRECTVC", Local);

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'UBICACIONFILTRADA', 'DIAGNOSTICO', 'CLARO', 'REPARACION', 'ALMACENAMIENTO'", "", "dbo.Ubicaciones", Local);
        }

        public void CargarDatosDetails()
        {
            View.Model.ListRecords = new DataTable("ListRecords");
            //Asigno las columnas restantes    
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("SmartCard", typeof(String));
            View.Model.ListRecords.Columns.Add("Modelo", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("Nivel", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModuloDIRECTVC("DIAGNOSTICO", null);

            View.Model.ListRecords_1 = ds.Tables[0];

        }

        


        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            String ConsultaBuscar = "";
            String ConsultaValidar = "";


            //Evaluo que haya sido digitado el serial para buscar
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("El campo serial no puede ser vacio.");
                return;
            }

            ConsultaBuscar = "SELECT SERIAL FROM dbo.EquiposDIRECTVC WHERE upper(SERIAL) = upper('" + View.GetSerial1.Text.ToString() + "')";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            if (Resultado.Rows.Count > 0)
            {
                ConsultaValidar = "SELECT RowId, SERIAL, RECEIVER, SMART_CARD_ENTRADA, MODELO, ESTADO_MATERIAL, NIVEL_CANDIDATO, ESTADO  FROM"
                                  + " dbo.EquiposDIRECTVC WHERE upper(SERIAL) = upper('" + View.GetSerial1.Text.ToString()
                                  + "') AND (ESTADO = 'PARA PROCESO' OR ESTADO = 'CUARENTENA') AND NIVEL_CANDIDATO = 'NIVEL 1'";
                DataTable ResultadoValidado = service.DirectSQLQuery(ConsultaValidar, "", "dbo.EquiposDIRECTVC", Local);

                if (ResultadoValidado.Rows.Count > 0)
                {
                    foreach (DataRow item in View.Model.ListRecords.Rows)
                    {
                        if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
                        {
                            Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
                            return;
                        }
                    }

                    //dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                    //dr["Producto"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                    //dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                    //dr["Receiver"] = RegistroValidado.Rows[0]["Receiver"].ToString();
                    //dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();
                    //dr["NIVEL_CANDIDATO"] = RegistroValidado.Rows[0]["NIVEL_MATERIAL"].ToString();

                    dr["RowId"] = ResultadoValidado.Rows[0]["RowId"].ToString();
                    dr["Serial"] = ResultadoValidado.Rows[0]["SERIAL"].ToString();
                    dr["Receiver"] = ResultadoValidado.Rows[0]["RECEIVER"].ToString();
                    dr["SmartCard"] = ResultadoValidado.Rows[0]["SMART_CARD_ENTRADA"].ToString();
                    dr["Modelo"] = ResultadoValidado.Rows[0]["MODELO"].ToString();
                    dr["Estado"] = ResultadoValidado.Rows[0]["ESTADO_MATERIAL"].ToString();
                    dr["Nivel"] = ResultadoValidado.Rows[0]["NIVEL_CANDIDATO"].ToString();

                    View.Model.ListRecords.Rows.Add(dr);

                    View.GetSerial1.Text = "";

                }
                else
                {
                    Util.ShowError("El serial no se encuentra PARA PROCESO o no es Candidato Nivel 1");
                    return;
                }

            }
            else
            {
                Util.ShowError("El serial no se encuentra registrado.");
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

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    if (String.IsNullOrEmpty(DataRow["ESTADO_PREDIAG"].ToString()))
                        {
                            Util.ShowError("Por favor ingrese el pre-diagnostico");
                            return;
                        }

                    if (DataRow["ESTADO_PREDIAG"].ToString() == "BUEN ESTADO")
                    {
                            //Construyo la consulta para guardar los datos
                            ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'ETIQUETADO', Estado = 'P-ETIQUETADO', ESTATUS_PREDIAG = '"
                                            + DataRow["ESTADO_PREDIAG"].ToString() + "'";
                            ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                            ConsultaGuardar += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'PRE-DIAGNOSTICO EQUIPO, BUEN ESTADO','PRE-DIAGNOSTICO','ETIQUETADO','Sin Pallet','" 
                                            + DataRow["RowID"].ToString() + "','ETIQUETADO','UBICACIONPRODUCCION','" + this.user + "','';";

                            Console.WriteLine("###### " + ConsultaGuardar);
                    }
                    else
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = 'ALMACENAMIENTO', Estado = 'P-ENTREGA-PREDIAG', ESTATUS_PREDIAG = '"
                                        + DataRow["ESTADO_PREDIAG"].ToString() + "'";
                        ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                        ConsultaGuardar += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'PRE-DIAGNOSTICO EQUIPO, MAL ESTADO','PRE-DIAGNOSTICO','ALMACEN','Sin Pallet','"
                                        + DataRow["RowID"].ToString() + "','ALMACEN','UBICACIONLOGISTICA','" + this.user + "','';";

                        Console.WriteLine("###### " + ConsultaGuardar);
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

            #region Columna Estado PreDiag.

            IList<MMaster> ListadoEstado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOS" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Pre-Diagnostico";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoEstado);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_PREDIAG"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTADO_PREDIAG", typeof(String)); //Creacion de la columna en el DataTable

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
                    if (((MMaster)((ComboBox)sender).SelectedItem).Code.ToString() == "SIN FALLA")
                        dr[5] = "BUEN ESTADO";
                    else
                        dr[5] = "MAL ESTADO";
                    break;
                }
            }
            return;
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

        #endregion
    }
}