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

    public interface IAlmacenamientoPresenter
    {
        IAlmacenamientoView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class AlmacenamientoPresenter : IAlmacenamientoPresenter
    {
        public IAlmacenamientoView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 4; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        private int contFilas_byPallet = 0; // Almacena el conteo de los equipos que tiene un pallet seleccionado en la lista de busqueda
        private String codigoPallet = ""; // Guarda el codigo pallet cuando se selecciona una fila de la lista de pallets o cuando se genera un nuevo codigo de pallet
        private String ubicacionPallet = ""; // Guarda la ubicacion del pallet seleccionado de la lista de pallets
        private Boolean seleccionUbicacion = false; // Controla el dato de la ubicacion, si el usuario selecciona nueva pallet (False) se toma del combobox ubicacion, si selecciona una fila/pallet se captura de ahi (True)
        

        public AlmacenamientoPresenter(IUnityContainer container, IAlmacenamientoView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AlmacenamientoModel>();


            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);

            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;

            View.GenerarPallet += new EventHandler<EventArgs>(this.OnGenerarPallet);
            View.KeyConsultarPallet += new EventHandler<KeyEventArgs>(this.OnKeyConsultarPallet);
            View.EnterConsultarPallet += new EventHandler<KeyEventArgs>(this.OnEnterConsultarPallet);
            View.SeleccionPallet_Consulta += new EventHandler<EventArgs>(this.OnSeleccionPalletConsulta);
            View.Imprimir_Hablador += new EventHandler<EventArgs>(this.OnImprimir_Hablador);
            View.ListarEquiposSeleccion += new EventHandler<EventArgs>(this.OnListarEquiposSeleccion);
            View.ImprimirHabladorAlmacen += new EventHandler<EventArgs>(this.OnImprimirHabladorAlmacen);
            View.EliminarEquipo_Fila += new EventHandler<EventArgs>(this.OnEliminarEquipo_Fila);
            View.GenerarNumero += new EventHandler<EventArgs>(this.OnGenerarNumero);

            #endregion

            #region Datos

            //Cargo la variable de las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVPOSIC" } });
            View.Model.ListadoEstadosPallet = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVESTREC" } });
            this.Actualizar_UbicacionDisponible();

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'RECIBOALMACEN', 'CLARO'", "", "dbo.Ubicaciones", Local);

            

            CargarListPallets();
            //Cargo los datos del listado
            CargarDatosDetails();

            #endregion

        }

        #region Metodos

        //Actualiza la informacion de los combobox cambio de ubicacion, permite que una posicion que esta ocupada no aparecezca en el comobobox
        private void Actualizar_UbicacionDisponible()
        {
            try
            {
                //List<String> validValues = new List<String>() { "A1A1", "A1A2" };
                DataTable dt_auxiliar = service.DirectSQLQuery("select POSICION from dbo.EquiposDIRECTVC where posicion is not null ", "", "dbo.EquiposDIRECTVC", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("POSICION"))
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
                if (View.GetSerial1.Text.ToUpper() == item["Serial"].ToString().ToUpper())
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
                RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCAREQUIPO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

                    //Evaluo si el serial existe
                    if (RegistroValidado.Rows.Count == 0)
                    {
                        Util.ShowError("El serial no existe en el sistema.");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Validacion existe o no el equipo en DB
                        ConsultaBuscar = "SELECT * FROM dbo.EquiposDIRECTVC WHERE upper(Serial) = upper('" + View.GetSerial1.Text.ToString() + "') AND (Estado = 'CUARENTENA' OR Estado = 'PARA PROCESO')";
                        DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

                        if (Resultado.Rows.Count > 0)
                        { 
                            //Asigno los campos
                            dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                            dr["Modelo"] = RegistroValidado.Rows[0]["Modelo"].ToString().ToUpper();
                            dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString().ToUpper();
                            dr["Receiver"] = RegistroValidado.Rows[0]["Receiver"].ToString().ToUpper();
                            dr["SmartCard"] = RegistroValidado.Rows[0]["SmartCard"].ToString().ToUpper();
                            

                            //Agrego el registro al listado
                            View.Model.ListRecords.Rows.Add(dr);

                            //Limpio los seriales para digitar nuevos datos
                            View.GetSerial1.Text = "";
                            View.GetSerial2.Text = "";
                            View.GetSerial1.Focus();
                        }
                        else
                        {
                            Util.ShowError("El serial no se encuentra en la ubicacion requerida. Esta en estado " + RegistroValidado.Rows[0]["Estado"].ToString());
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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAALMACENA', 'PARA ALMACENAMIENTO' ";

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
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARMERCANCIAALMACENA', 'PARA ALMACENAMIENTO', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        public void BuscarRegistrosEntrega()
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesosDIRECTV 'BUSCARMERCANCIADIAGNOSTICO', 'PARA ALMACENAMIENTO'";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {

            //Variables Auxiliares
            String ConsultaSQL = "", NuevaUbicacion, NuevoEstado, ConsultaTrack = "";

            //Evaluo que haya registros en el listado
            if (View.ListadoBusquedaRecibo.Items.Count == 0)
                return;

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            //Valido la ubicacion para colocar el estado
            if (NuevaUbicacion != "DESPACHO")
            {
                NuevoEstado = "ALMACENAMIENTO";
                //Recorro el listado de registros para confirmar
                foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
                {
                    //Creo la consulta para confirmar el cambio de ubicacion de la estiba
                    ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATE_ALMACENAMIENTO','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "';";
                    ConsultaTrack = "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "';";

                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'PALLET RECIBIDO PARA DESPACHO','RECEPCIÓN ALMACENAMIENTO','Z DESPACHO','" + Registros.Row["Pallet"].ToString()
                                            + "','','ALMACENAMIENTO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
            }

            else
            { 
                NuevoEstado = "DESPACHO";
                //Recorro el listado de registros para confirmar
                foreach (DataRowView Registros in View.ListadoBusquedaRecibo.SelectedItems)
                {
                    //Creo la consulta para confirmar el cambio de ubicacion de la estiba
                    ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'UPDATE_ALMACENAMIENTO','" + NuevoEstado + "', '" + NuevaUbicacion + "','" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "','" + Registros.Row["Pallet"].ToString() + "';";
                    ConsultaTrack = "UPDATE dbo.TrackEquiposDIRECTV SET ESTADO_ALMACEN2 = 'DESDE PRODUCCION', UBICACION_ALMACEN2 = '" + ((MMaster)View.UbicacionDesp.SelectedItem).Code.ToString() + "' WHERE CODEMPAQUE_EMPAQ_ENTR = '" + Registros.Row["Pallet"].ToString() + "';";

                    ConsultaSQL += "exec sp_InsertarNuevo_MovimientoDIRECTV 'PALLET POSIIONADO EN ALMACEN','RECEPCIÓN ALMACENAMIENTO','ALMACENAMIENTO','" + Registros.Row["Pallet"].ToString()
                                            + "','','ALMACENAMIENTO','UBICACIONALMACEN_SALIDAS','" + this.user + "','';";

                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    service.DirectSQLNonQuery(ConsultaTrack, Local);
                }
            }
                

            

            //Muestro el mensaje de confirmacion
            Util.ShowMessage("Recibo de estibas realizado satisfactoriamente.");

            //View.UnidadAlmacenamiento.SelectedIndex = -1;
            //View.CodigoEmpaque.Text = "";
            View.Ubicacion.SelectedIndex = -1;

            View.UbicacionDesp.SelectedIndex = -1;

            //Busco los registros para actualizar el listado
            BuscarRegistrosEntrega();

        }

        public void CargarDatosDetails()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("Modelo", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Receiver", typeof(String));
            View.Model.ListRecords.Columns.Add("SmartCard", typeof(String));
            //View.Model.ListRecords.Columns.Add("IdPallet", typeof(String));

            //#region Columna Fecha Cambio a bodega

            //Columna = new GridViewColumn();
            //assembly = Assembly.GetAssembly(Type.GetType("Microsoft.Windows.Controls.DatePicker, WPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=c93dde70475aea7e"));
            //TipoDato = "Microsoft.Windows.Controls.DatePicker";
            //Columna.Header = "Fecha Cambio Bod.";
            //Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            //Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)130);
            //Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding("FECHA_CAMBIO_BODEGA"));

            //// add textbox template
            //Columna.CellTemplate = new DataTemplate();
            //Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            //View.Model.ListRecords.Columns.Add("FECHA_CAMBIO_BODEGA", typeof(String)); //Creacion de la columna en el DataTable

            //#endregion

            #region Columna Estado Material.

            IList<MMaster> ListadoSerializados = service.GetMMaster(new MMaster { MetaType = new MType { Code = "SI _NO" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ensamblado";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoSerializados);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ENSAMBLADO"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ENSAMBLADO", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            //#region Posicion

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

            //#endregion 
        }

        private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        {
            //Variables Auxiliares
            DataRow RegistroGuardar;
            DataTable RegistroValidado;
            DataTable SerialesIngresados;
            List<String> listRepetidos = new List<string>(); //Guarda los seriales que estan repetidos en el carge masivo
            List<String> listReceiver = new List<string>(); // Guarda los receiver repetidos en el carge masivo
            List<String> listSmartCard = new List<string>(); // Guarda los SmartCard repetidos en el carge masivo
            Boolean Existe;
            Boolean aux;

            //Valido la existencia de los equipos ingresados
            SerialesIngresados = ValidarSerialesIngresados(e.Value);

            foreach (DataRow dr in e.Value.Rows)
            {

                //Iniciamos la variable para validar existencia
                Existe = false;

                //Validamos si el serial existe o no en el sistema
                foreach (DataRow dr1 in SerialesIngresados.Rows)
                {
                    if (dr1["SERIAL"].ToString().ToUpper() == dr[0].ToString().ToUpper())
                    {
                        Existe = true;
                        break;
                    }
                }

                //Valido el serial si existe
                if (Existe)
                    continue;

                //Inicializo el registro para guardar el dato
                RegistroGuardar = View.Model.ListRecords.NewRow();

                try
                {
                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCAREQUIPO','" + dr[0].ToString() + "'", "", "dbo.EquiposDIRECTVC", Local);
                    aux = true;

                    //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                    foreach (DataRow item in View.Model.ListRecords.Rows)
                    {
                        if (dr[0].ToString().ToUpper() == item["Serial"].ToString().ToUpper())
                        {
                            //Util.ShowError("El serial " + dr[0].ToString() + " y Mac " + dr[1].ToString() + " esta repetido, por favor verificar.");
                            aux = false;
                            listRepetidos.Add(dr[0].ToString());
                        }
                    }

                    //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                    foreach (DataRow item in View.Model.ListRecords.Rows)
                    {
                        if (dr[1].ToString().ToUpper() == item["Mac"].ToString().ToUpper())
                        {
                            aux = false;
                            listReceiver.Add(dr[1].ToString());
                        }
                    }

                    //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                    foreach (DataRow item in View.Model.ListRecords.Rows)
                    {
                        if (dr[2].ToString().ToUpper() == item["SmartCard"].ToString().ToUpper())
                        {
                            aux = false;
                            listSmartCard.Add(dr[2].ToString());
                        }
                    }

                    if (aux)
                    {
                        //Asigno los campos
                        RegistroGuardar["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                        RegistroGuardar["ProductoID"] = RegistroValidado.Rows[0]["Modelo"].ToString();
                        RegistroGuardar["Serial"] = dr[0].ToString();
                        RegistroGuardar["Mac"] = dr[1].ToString();
                        RegistroGuardar["SmartCard"] = dr[2].ToString();
                        RegistroGuardar["IdPallet"] = dr[3].ToString();
                        RegistroGuardar["FECHA_CAMBIO_BODEGA"] = dr[4].ToString();
                        RegistroGuardar["ENSAMBLADO"] = dr[5].ToString();
                        RegistroGuardar["Posicion"] = dr[6].ToString();

                        //Agrego el registro al listado
                        View.Model.ListRecords.Rows.Add(RegistroGuardar);
                    }
                }
                catch (Exception Ex)
                {
                    //Util.ShowMessage(Ex.ToString());
                    continue;
                }
            }

            //Si existen seriales repetidos muestra en un dialog los seriales
            if (listRepetidos.Count > 0 || listReceiver.Count > 0 || listSmartCard.Count > 0)
            {
                String cadena = "";

                if (listRepetidos.Count > 0)
                {
                    cadena = cadena + "[FILAS REPETIDAS, SERIALES TRUNCADOS]: \n";
                    //Impresion de seriales no aceptados

                    foreach (var item in listRepetidos)
                    {
                        if (!cadena.Contains(item))
                        {
                            cadena = cadena + " " + item + "\n";
                        }
                    }
                }

                if (listReceiver.Count > 0)
                {
                    cadena = cadena + "[FILAS REPETIDAS, RECEIVER TRUNCADOS]: \n";
                    //Impresion de seriales no aceptados

                    foreach (var item in listReceiver)
                    {
                        if (!cadena.Contains(item))
                        {
                            cadena = cadena + " " + item + "\n";
                        }
                    }
                }

                if (listSmartCard.Count > 0)
                {
                    cadena = cadena + "[FILAS REPETIDAS, SMARTCARD TRUNCADOS]: \n";
                    //Impresion de seriales no aceptados

                    foreach (var item in listSmartCard)
                    {
                        if (!cadena.Contains(item))
                        {
                            cadena = cadena + " " + item + "\n";
                        }
                    }
                }

                Util.ShowError(cadena);
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

            if (View.GetCodPallet.Text.ToString() == "" && View.ListadoPalletsBusqueda.SelectedIndex == -1 )
            {
                Util.ShowMessage("Debe generar el pallet o selecionar uno existente para almacenar los equipos.");
                return;
            }

            String aux_idpallet = "";
            if (View.GetCodPallet.Text.ToString() == "" && View.ListadoPalletsBusqueda.SelectedIndex > -1)
                aux_idpallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            else if (View.GetCodPallet.Text.ToString() != "" && View.ListadoPalletsBusqueda.SelectedIndex == -1)
                aux_idpallet = View.GetCodPallet.Text.ToString();
            
            //Variables Auxiliares
            String ConsultaGuardar = "";
            String ConsultaGuardarTrack = "";
            Int32 ContadorFilas = 0;
            DataTable SerialesIngresados;
            Boolean Existe;

            //Valido la existencia de los equipos ingresados
            SerialesIngresados = ValidarSerialesIngresados(View.Model.ListRecords);

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;

                    //Iniciamos la variable para validar existencia
                    Existe = false;

                    //Validamos si el serial existe o no en el sistema
                    foreach (DataRow dr1 in SerialesIngresados.Rows)
                    {
                        if (dr1["SERIAL"].ToString() == DataRow[0].ToString())
                        {
                            Existe = true;
                            break;
                        }
                    }

                    //Valido el serial si existe
                    if (Existe)
                    {
                        continue;
                    }

                    
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET Posicion = '" + View.GetUbicacionPallet.Text.ToString() + "', IdPallet = '" + aux_idpallet + "', Ubicacion = 'ALMACENAMIENTO', Estado= 'ALMACENAMIENTO', ENSAMBLADO='" + DataRow["ENSAMBLADO"] + "' WHERE Serial = '" + DataRow["SERIAL"].ToString() + "' AND (Estado = 'CUARENTENA' OR Estado = 'PARA PROCESO') AND ESTADO IS NOT NULL;";
                        ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposDIRECTV SET ESTIBA_ENTRADA = '" + aux_idpallet + "', ESTADO_ALMACEN1 = 'ALMACENAMIENTO', UBICACION_ENTRADA = 'ALMACENAMIENTO', FECHA_ING_ALMACEN = GETDATE() WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";
                        ConsultaGuardar += "EXEC sp_InsertarNuevo_MovimientoDIRECTV 'ALMACENAMIENTO DE EQUIPOS EN BODEGA','ESPERANDO POR ENVIO A PRODUCCION','" + View.GetUbicacionPallet.Text.ToString() + "','" + View.GetCodPallet.Text + "','','ALMACENAMIENTO','ALMACENARENBODEGA','" + this.user + "','" + DataRow["Serial"].ToString() + "';";
                }

                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {
                    Console.WriteLine(ConsultaGuardar);
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
                //actualizo la lista de pallets
                CargarListPallets();
                //actualizo el listado de posiciones
                this.Actualizar_UbicacionDisponible();
                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
            
            View.GetCodPallet.Text = "";
        }

        public DataTable ValidarSerialesIngresados(DataTable ListadoSeriales)
        {
            //Variables Auxiliares
            DataTable SerialesIngresados;
            String CadenaSQL = "";
            Int32 CantidadRegistros;

            //Creo la cadena de consulta
            CadenaSQL += "SELECT * FROM dbo.EquiposDIRECTVC WHERE UPPER(SERIAL) IN (";

            //Obtengo la cantidad de registros
            CantidadRegistros = ListadoSeriales.Rows.Count;

            //Recorremos el listado
            foreach (DataRow dr in ListadoSeriales.Rows)
            {
                //Evaluo si es el ultimo registro para crear la cadena sin la coma
                if (CantidadRegistros == 1)
                {
                    //Adiciono los valores a la cade de cosulta
                    CadenaSQL += "'" + dr[0].ToString().ToUpper() + "') AND ESTADO != 'DESPACHADO'";
                }
                else
                {
                    //Adiciono los valores a la cade de cosulta
                    CadenaSQL += "'" + dr[0].ToString() + "',";
                }

                //Disminuyo el contador
                CantidadRegistros--;
            }

            //Ejecutamos la consulta
            SerialesIngresados = service.DirectSQLQuery(CadenaSQL, "", "dbo.EquiposDIRECTVC", Local);

            //Retornamos el resultado
            return SerialesIngresados;
        }

        public void CargarListPallets()
        {
            //Creo la consulta para buscar los ultimos 15 pallets registrados
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARPALLETALMACENAMIENTO','','','',''";
            View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnGenerarPallet(object sender, EventArgs e)
        {
            CargarListPallets();
            LimpiarDatosIngresoSeriales();

            //DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            try
            {
                //Si el contador de equipos por pallet es igual a la lista de almacenamiento, quiere decir que no se agregaron nuevos equipos y se puede crear el nuevo pallet sin problema
                if (View.Model.ListRecords.Rows.Count == this.contFilas_byPallet)
                {

                    View.Model.ListRecords.Clear();

                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

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
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
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
                else if (this.codigoPallet == "")
                {
                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

                    if (Resultado.Rows.Count > 0)
                    {
                        //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                        RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposClaro WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposCLARO", Local);

                        //Evaluo si el serial existe
                        if (RegistroValidado.Rows.Count > 0)
                        {
                            Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
                            return;
                        }
                        else
                        {
                            //Asigno los campos
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
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
                else
                {
                    // Si no son iguales quiere decir que se agregaron nuevos equipos al pallet
                    ////Util.ShowMessage("Esta seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan");
                    //Console.WriteLine("Se borraran los seriales no guardados");

                    if (!UtilWindow.ConfirmOK("Esta seguro de crear un nuevo pallet, si realiza esto los cambios realizados se perderan?") == true)
                        return;
                    View.Model.ListRecords.Clear();

                    //Creo el número de pallet aleatorio 
                    ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
                    DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                    this.contFilas_byPallet = 0; // Se crea un nueva pallet por lo tanto se vacia el contador de equipos por pallet

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
                            View.GetCodPallet.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                            this.codigoPallet = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();

                            this.seleccionUbicacion = false;
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
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de generar el pallet. Error: " + Ex.Message);
                return;
            }

        }

        /** Se ejecuta cuando se levanta una tecla en el cuadro de texto de busqueda de pallets **/
        private void OnKeyConsultarPallet(object sender, KeyEventArgs e)
        {
            String tecla_idpallet = View.GetCodPalletBusqueda.Text.ToString();
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARPALLETALMACENAMIENTO','" + tecla_idpallet + "','','',''";

            View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        /** Se ejecuta cuando se pulsa la tecla enter en el cuadro de texto de busqueda de pallets **/
        private void OnEnterConsultarPallet(object sender, KeyEventArgs e)
        {
            ConsultarPallets();
        }

        private void ConsultarPallets()
        {
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARPALLETALMACENAMIENTO','','','',''";

            View.Model.ListPallets_Almacenamiento = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        /** Se ejecuta cuando se clickea en una fila del listado de pallets registrados **/
        private void OnSeleccionPalletConsulta(object sender, EventArgs e)
        {
            View.GetCodPallet.Text = "";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
                return;

            String aux_idpallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();
            
            //consulto los seriales contenidos en esa estiba
            
            DataTable resultado = service.DirectSQLQuery("SELECT RowID, Modelo, Serial, Receiver, SMART_CARD_ENTRADA as SmartCard, ENSAMBLADO FROM dbo.EquiposDIRECTVC WHERE IdPallet like '" + aux_idpallet + "'" +
                "AND ESTADO = 'ALMACENAMIENTO'", "", "dbo.EquiposDIRECTVC", Local);
           
            View.Model.ListRecords = resultado;


            //DataRowView drv = (DataRowView)View.ListadoPalletsBusqueda.SelectedItem;
            //this.codigoPallet = drv[0].ToString();

            //this.ubicacionPallet = drv[1].ToString();

            //this.seleccionUbicacion = true;

            //this.contFilas_byPallet = View.Model.ListRecords.Rows.Count; // Guarda el numero de equipos almacenados en ese pallet
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

            String origen = "";
            String estado = "";

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
                //if (origen.Contains("PRODUCCION"))
                //{
                    //Creo la base de la consulta para traer los seriales respectivos
                ConsultaSQL = "select serial,MODELO,Receiver, Estado,SMART_CARD_ENTRADA,Fecha_Ingreso from EquiposDIRECTVC where CodigoEmpaque = '" + pallet + "'";

                //Ejecuto la consulta
                SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

                    //Imprimo los registros
                    PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, "PALLET", pallet, destino, "DIRECTV", "POSICIONAR EN ALMACEN", "", "");
                //}
                //else
                //{
                //    Util.ShowMessage("    No es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de EMPAQUE deben enviarse a DESPACHO.");
                //}
            }
            else if (destino == "DESPACHO")
            {

                //if (origen == "EMPAQUE")
                //{
                    //Creo la base de la consulta para traer los seriales respectivos
                    ConsultaSQL = "select serial,MODELO,Receiver, Estado,SMART_CARD_ENTRADA,Fecha_Ingreso from EquiposDIRECTVC where CodigoEmpaque = '" + pallet + "'";

                    //Ejecuto la consulta
                    SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

                    //Imprimo los registros
                    PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, "PALLET", pallet, destino, "DIRECTV", "ALMACENAMIENTO - " + destino, "", "");
                //}

                //else if (estado == "PARA SCRAP")
                //{
                //    Util.ShowMessage("Los pallets provenientes de produccion en estado SCRAP no pueden ser despachados.");
                //    return;
                //}

                //else
                //{
                //    Util.ShowMessage("\tNo es posible generar Hablador para el pallet " + pallet + ",\n los pallets provenientes de PRODUCCIÓN no se pueden enviar a DESPACHO.");
                //}
            }
        }

        private void OnListarEquiposSeleccion(object sender, EventArgs e)
        {
            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoBusquedaCambioClasificacion.SelectedIndex == -1)
                return;

            string aux_idPallet = ((DataRowView)View.ListadoBusquedaCambioClasificacion.SelectedItem).Row["Pallet"].ToString();

            String Consulta = "SELECT IdPallet as PPallet, "
            + "serial as PSerial, "
            + "RECEIVER as Receiver, "
            + "SMART_CARD_ENTRADA as PMac,  "
            + "MODELO as Modelo,  "
            + "TIPO_ORIGEN as PTRecibo, "
            + "convert(VARCHAR,FECHA_INGRESO,120) as PFRegistro,  "
            + "DATEDIFF(day, FECHA_INGRESO,GETDATE()) as NumeroDias,dbo.TIMELAPSELEO(FECHA_INGRESO) as horas "
            + "from dbo.EquiposDIRECTVC WHERE ((IdPallet IS NOT NULL) AND (ESTADO = 'PARA ALMACENAMIENTO')) "
            + " AND CodigoEmpaque = '" + aux_idPallet + "'";

            View.Model.Listado_PalletSerial = service.DirectSQLQuery(Consulta, "", "dbo.EquiposDIRECTVC", Local);
            Console.WriteLine(Consulta);
        }

        public void OnImprimirHabladorAlmacen(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL = "";
            DataTable SerialesImprimir;
            String destino = "ALMACENAMIENTO";

            //Evaluo que haya sido seleccionado un registro
            if (View.ListadoPalletsBusqueda.SelectedIndex == -1)
            {
                Util.ShowMessage("Debe seleccionar al menos un registro");
                return;
            }

            String NroSeriales = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Cantidad"].ToString();
            String pallet = ((DataRowView)View.ListadoPalletsBusqueda.SelectedItem).Row["Pallet"].ToString();

            //Creo la base de la consulta para traer los seriales respectivos
            ConsultaSQL = "select serial,MODELO,Receiver,Fecha_Ingreso from Pallets_EmpaqueDIRECTV pallet join EquiposDIRECTVC eqc on pallet.id_pallet = eqc.pila where pallet.codigo_pallet = '" + pallet + "'";

            //Ejecuto la consulta
            SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);

            if (SerialesImprimir.Rows.Count == 0)
            {
                ConsultaSQL = "select serial,MODELO,Receiver,Fecha_Ingreso from EquiposDIRECTVC where idpallet LIKE '" + pallet + "' OR CodigoEmpaque2 LIKE '" + pallet + "' AND Estado LIKE 'DESPACHO'";

                //Ejecuto la consulta
                SerialesImprimir = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
            }

            //Imprimo los registros
            PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, "PALLET", pallet, destino, "DIRECTV", "ALMACENAMIENTO", "", "DIRECTV");
        }

        public void OnEliminarEquipo_Fila(object sender, EventArgs e)
        {
            //Confirmo si desea confirmar eliminar el/los equipos de la estiba en almacenamiento
            if (UtilWindow.ConfirmOK("¿Esta seguro de borrar los equipos del pallet " + codigoPallet + "?") == true)
            {
                if (View.ListadoEquiposAProcesar.SelectedItems.Count == 0)
                {
                    Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");
                }
                else
                {
                    String ConsultaActualizar = "";
                    try
                    {
                        //Borra de la lista los elementos seleccionados
                        while (View.ListadoEquiposAProcesar.SelectedItems.Count > 0)
                        {
                            //Construyo la consulta para guardar los datos
                            ConsultaActualizar += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = NULL, Estado = 'PARA PROCESO', idPallet=NULL, posicion=NULL";
                            ConsultaActualizar += " WHERE RowID = '" + View.Model.ListRecords.Rows[View.ListadoEquiposAProcesar.SelectedIndex][0].ToString() + "';";

                            ConsultaActualizar += " UPDATE dbo.TrackEquiposDIRECTV SET ESTIBA_ENTRADA = NULL, ESTADO_ALMACEN1 = NULL, UBICACION_ENTRADA=NULL, FECHA_ING_ALMACEN = NULL";
                            ConsultaActualizar += " WHERE Serial = '" + View.Model.ListRecords.Rows[View.ListadoEquiposAProcesar.SelectedIndex][1].ToString() + "';";

                            View.Model.ListRecords.Rows.RemoveAt(View.ListadoEquiposAProcesar.Items.IndexOf(View.ListadoEquiposAProcesar.SelectedItem));
                        }

                        //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                        if (!String.IsNullOrEmpty(ConsultaActualizar))
                        {
                            //Ejecuto la consulta
                            service.DirectSQLNonQuery(ConsultaActualizar, Local);
                        }
                        Actualizar_UbicacionDisponible();
                        ConsultarPallets();
                    }
                    catch (Exception ex)
                    {
                        Util.ShowMessage("Error en eliminación de equipo - almacenamiento, " + ex.Message);
                    }
                }
            }
        }


        private void OnGenerarNumero(object sender, EventArgs e)
        {
            String ConsultaBuscar = "";
            String ConsultaValidar = "";
            try{

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

                    if(RegistroValidado.Rows.Count > 0)            
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
            }catch(Exception ex){
                Util.ShowError("Se presento un error generando el pallet: " + ex.Message);
            }
        }

        #endregion

    }
}