using System;
using System.Text;
using System.Diagnostics;
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
using System.Windows.Media;

namespace WpfFront.Presenters
{

    public interface IEtiquetadosPresenter
    {
        IEtiquetadosView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EtiquetadosPresenter : IEtiquetadosPresenter
    {
        public IEtiquetadosView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private string Formato_fecha = "dd/MM/yyyy";
        private String ipservidor;
        private String rutaEtiqueta;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;


        public EtiquetadosPresenter(IUnityContainer container, IEtiquetadosView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EtiquetadosModel>();


            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.ReplicateDetailsBy_Column += new EventHandler<RoutedEventArgs>(this.OnReplicateDetailsBy_Column);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.DeleteDetails += new EventHandler<EventArgs>(this.OnDeleteDetails);
            View.ImprimirEtiqueta_Individual += new EventHandler<EventArgs>(this.OnImprimirEtiqueta_Individual);
            View.GetNumeroCodigos += new EventHandler<SelectionChangedEventArgs>(this.OnGetNumeroCodigos);


            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'ETIQUETADO', 'CLARO'", "", "dbo.Ubicaciones", Local);
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            CargarDatosDetails();
            ListarDatos();


            #endregion
        }

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            ListarDatos();
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("RowID", typeof(String));
            View.Model.ListRecords.Columns.Add("ProductoID", typeof(String));
            View.Model.ListRecords.Columns.Add("Direccionable", typeof(String));
            View.Model.ListRecords.Columns.Add("SAP", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("Familia", typeof(String));

            View.Model.ListadoEtiquetas = service.DirectSQLQuery("EXEC sp_GetProcesos 'ETIQUETASDIS', '', ''", "", "General.Sticker", Local);

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
        }

        private void ListarDatos()
        {
            DataSet ds = Util.GetListaDatosModulo("ETIQUETADO", null);

            View.Model.ListRecords_1 = ds.Tables[0];
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Reuso

            IList<MMaster> ListadoStatusDiagnostico = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REACON" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Reuso";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoStatusDiagnostico);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Reuso"));
            Txt.SetValue(ComboBox.WidthProperty, (double)90);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Reuso", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Observaciones Etiquetado

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Observaciones Etiquetado";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)130);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Observaciones_Etiquetado"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Observaciones_Etiquetado", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        private void OnGetNumeroCodigos(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                String Stickid = ((DataRowView)(sender as ComboBox).SelectedItem).Row["stickCode"].ToString(); //Obtengo el rowid del diseño de etiqueta seleccionado
                DataTable aux = service.DirectSQLQuery("EXEC sp_GetProcesos 'ETIQUETASDIS_BYID', '" + Stickid + "', ''", "", "General.Sticker", Local); //Realizo una consulta que traiga la cantidad de codigos, ruta y ip servidor
                
                Int32 cantCod_adicionales = Int32.Parse(aux.Rows[0]["Cantidad"].ToString()); // Cantidad de codigos adicionales 
                this.rutaEtiqueta = aux.Rows[0]["Ruta"].ToString(); // Ruta del fichero en el servidor
                this.ipservidor = aux.Rows[0]["Servidor"].ToString(); // Dirección ip del servidor 

                if (aux.Rows.Count > 0) //si la consulta devuelve un valor realiza el proceso
                {
                  if (cantCod_adicionales == 0) //Si el diseño unicamente incluye el Serial y Mac no se crean componentes adicionales
                  {
                      View.Get_StackCodigosAdicionales.Visibility = Visibility.Collapsed; // El stackpanel de codigos adicionales se oculta
                      View.Get_StackCodigosAdicionales.Children.Clear(); // Se eliminan todos los controles graficos del stackpanel
                  }
                  else 
                  {
                    /*Si por el contrario el diseño necesita codigos adicionales a los de serial y mac, se crean textblock y textbox 
                      dependiendo de la cantidad de codigos que necesite ese diseño (este valor se indica en la BD, tabla General.Stickets, Columna=stick_cantidadCodes)*/

                    View.Get_StackCodigosAdicionales.Children.Clear(); // Elimina todos los controles graficos del stackpanel
                    View.Get_StackCodigosAdicionales.Visibility = Visibility.Visible; //Stackpanel visible al usuario
                      
                    for(int fila=1;fila<=cantCod_adicionales;fila++){ // Se creean los componenetes de codigos adicionales 

                        //Se obtiene acceso al stackpanel y se crea un nuevo hijo a este. Estara compuesto por Textblock's y Textbox's
                        View.Get_StackCodigosAdicionales.Children.Add(new TextBlock { Text = "Identificador adicional "+fila, Name = "txt_a"+fila, Width = 130, Margin = new Thickness(-70,5,0,0), TextAlignment=TextAlignment.Left });
                        View.Get_StackCodigosAdicionales.Children.Add(new TextBox { Name = "tb_serialaux" + fila, Width = 182, Height = 20, Margin = new Thickness(0, 2, 20, 0) }); 
                    }
                  }
                }
                else
                {
                    Util.ShowError("Error al buscar diseño de etiqueta en el servidor");
                }
            }
            catch(Exception ex){
                Util.ShowError("Error al buscar diseño de etiqueta en el servidor "+ex.Message);
            }
        }

        private void KeyUp_CodigoAdicional(object sender, KeyEventArgs e)
        {
          //Retira el borde rojo del textbox de codigo adicional donde se lleno el campo
          (sender as TextBox).BorderBrush = Brushes.AliceBlue; 
            //Util.ShowMessage("You Entered: " + e.Key);
        }

        private void OnImprimirEtiqueta_Individual(object sender, EventArgs e)
        {
          Boolean validacion = false;
          List<String> codAuxiliares = new List<String>();
          String serial=View.GetMacImpre_Individual.Text;
          String mac = View.GetSerialImpre_Individual.Text;
          Int32 cantidadImpresiones=1;

          if(serial!="" || mac!=""){
         
            if (View.Get_StackCodigosAdicionales.Children.Count > 0) { //Si el stackpanel tiene controles adicionales para codigos aparte de serial y mac

              // Se recorre todo el stackpanel en busca de Textbox, especificamente del valor que tengan estos
              foreach (var child in View.Get_StackCodigosAdicionales.Children)
              {
                 var textBox = child as TextBox;
                 if (textBox == null) continue;

                 if (textBox.Text == "")
                 {
                   textBox.BorderBrush = Brushes.Red;
                   textBox.KeyUp += new KeyEventHandler(KeyUp_CodigoAdicional);
                   validacion = true;
                 }
                 else {
                    textBox.BorderBrush = Brushes.AliceBlue;
                    codAuxiliares.Add(textBox.Text);
                 }
              }
            }

            if (validacion)
            {
                Util.ShowError("Todos los campo deben estar completos.");
                return;
            }
            else { 
               System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
               pd.PrinterSettings = new PrinterSettings();
               Int32.TryParse(View.GetCantidad_Impresiones.Text, out cantidadImpresiones);

               //try {
                   if (System.Windows.Forms.DialogResult.OK == pd.ShowDialog())
                   {
                       PrinterControl.EtiquetadoEquipoIndividual(serial, mac, this.rutaEtiqueta, this.ipservidor, codAuxiliares, cantidadImpresiones, pd.PrinterSettings.PrinterName);
                   }
               //}
               // catch(Exception ex){
               //     Util.ShowError("Error en la impresión, por favor verifique la conexión a la impresora Zebra "+ex.Message);
               // }
             }
         }
         else{
           Util.ShowError("Todos los campo deben estar completos.");
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
                ConsultaBuscar = "SELECT * FROM dbo.EquiposCLARO WHERE Serial = '" + View.GetSerial1.Text.ToString() + "'";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (Resultado.Rows.Count > 0)
                {
                    //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                    RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPOP_ETIQUETADO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposCLARO", Local);

                    if (RegistroValidado.Rows[0]["Estado"].ToString() != "P-ETIQUETADO")
                    {
                        Util.ShowError("El serial ingresado no esta en Etiquetado, El serial se encuentra en estado " + RegistroValidado.Rows[0]["Estado"].ToString() + "");
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        //Asigno los campos
                        dr["RowID"] = RegistroValidado.Rows[0]["RowID"].ToString();
                        dr["ProductoID"] = RegistroValidado.Rows[0]["Producto"].ToString();
                        dr["Direccionable"] = RegistroValidado.Rows[0]["Direccionable"].ToString();
                        dr["SAP"] = RegistroValidado.Rows[0]["CODIGO_SAP"].ToString();
                        dr["Serial"] = RegistroValidado.Rows[0]["Serial"].ToString();
                        dr["Mac"] = RegistroValidado.Rows[0]["Mac"].ToString();
                        dr["Estado"] = RegistroValidado.Rows[0]["Estado"].ToString();
                        dr["Familia"] = RegistroValidado.Rows[0]["Familia"].ToString();

                        //Agrego el registro al listado
                        View.Model.ListRecords.Rows.Add(dr);

                        var border = (Border)VisualTreeHelper.GetChild(View.ListadoItems, 0);
                        var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                        scrollViewer.ScrollToBottom();

                        //Limpio los seriales para digitar nuevos datos
                        View.GetSerial1.Text = "";
                        View.GetSerial2.Text = "";
                        View.GetSerial1.Focus();
                    }
                }
                else {
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
            //Obtenemos el indice del encabezado
            if (View.ListadoItems.SelectedIndex != -1)
            {
                if (View.ListadoItems.SelectedItems.Count > 1)// Se selecciona mas de una fila
                {
                    int indice_fila1 = View.ListadoItems.SelectedIndex;

                    //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                    foreach (DataRowView dr in View.ListadoItems.SelectedItems)
                    {
                        for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                            dr.Row[z] = View.Model.ListRecords.Rows[indice_fila1][z];
                    }
                }
                else
                {
                    int SComp;
                    SComp = View.ListadoItems.SelectedIndex;
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

                if (View.ListadoItems.SelectedIndex != -1)
                {

                    if (View.ListadoItems.SelectedItems.Count > 1)// Se selecciona mas de una fila
                    {
                        DataRowView drv = (DataRowView)View.ListadoItems.SelectedItem;
                        String valueOfItem = drv[index].ToString();

                        if (index >= offset)
                        {
                            //Replica el valor de la primera fila seleccionada en las demas filas seleccionadas
                            foreach (DataRowView dr in View.ListadoItems.SelectedItems)
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
                            for (int i = View.ListadoItems.SelectedIndex; i < View.Model.ListRecords.Rows.Count; i++)
                                View.Model.ListRecords.Rows[i][index] = View.Model.ListRecords.Rows[View.ListadoItems.SelectedIndex][index];
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
            Int32 ContadorEquiposOK=0,ContadorEquiposNOK = 0;

            try
            {
                if (!UtilWindow.ConfirmOK("¿Desea imprimir las etiquetas de los equipos ahora?") == false) {
                    // Se solicita al usuario seleccionar una impresora
                    System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
                    pd.PrinterSettings = new PrinterSettings();

                    if (System.Windows.Forms.DialogResult.OK == pd.ShowDialog())
                    {
                        ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");

                        foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                        {
                            //Aumento el contador de filas
                            ContadorFilas++;

                            //Construyo la consulta para guardar los datos
                            ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'VERIFICACION', Estado = 'P-VERIFICACION'";
                            ConsultaGuardar += ", REUSO = '" + DataRow["Reuso"].ToString() + "', OBSERVACIONES_ETIQUETADO = '" + DataRow["Observaciones_Etiquetado"].ToString() + "'";
                            ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                            ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_ETIQUETADO = 'VERIFICACION', FECHA_ETIQUETADO = GETDATE() WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "';";

                            //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                            ConsultaGuardar += "EXEC sp_InsertarNuevo_Movimiento 'ETIQUETADO DE EQUIPO','ETIQUETADO','VERIFICACION','Sin pallet','" + DataRow["RowID"].ToString() + "','ETIQUETADO','UBICACIONPRODUCCION','" + this.user + "','';";
               
                            String archivo = DataRow["ProductoID"].ToString();
                            archivo = archivo.Replace("/", "");

                            try
                            {
                                PrinterControl.EtiquetadoEquipo(DataRow["Serial"].ToString(), DataRow["Mac"].ToString(), archivo, pd.PrinterSettings.PrinterName);
                                ContadorEquiposOK++;
                            }
                            catch (Exception ex)
                            {
                                ContadorEquiposNOK++;
                            }
                        }
                        pw.Visibility = Visibility.Collapsed;
                        pw.Close();
                    }
                }
                else
                {
                    foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                    {
                        //Aumento el contador de filas
                        ContadorFilas++;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Ubicacion = 'VERIFICACION', Estado = 'P-VERIFICACION'";
                        ConsultaGuardar += ", REUSO = '" + DataRow["Reuso"].ToString() + "', OBSERVACIONES_ETIQUETADO = '" + DataRow["Observaciones_Etiquetado"].ToString() + "'";
                        ConsultaGuardar += " WHERE RowID = '" + DataRow["RowID"].ToString() + "';";

                        //Guardo en la tabla de movimientos el cambio de ubicacion del equipo
                        ConsultaGuardar += "EXEC sp_InsertarNuevo_Movimiento 'ETIQUETADO DE EQUIPO','ETIQUETADO','VERIFICACION','Sin pallet','" + DataRow["RowID"].ToString() + "','ETIQUETADO','UBICACIONPRODUCCION','" + this.user + "','';";
                        Console.WriteLine("###### " + ConsultaGuardar);

                        ConsultaGuardarTrack += "UPDATE dbo.TrackEquiposCLARO SET ESTADO_ETIQUETADO = 'ETIQUETADO', FECHA_ETIQUETADO = GETDATE() WHERE ID_SERIAL = '" + DataRow["RowID"].ToString() + "'";
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

                if (ContadorEquiposOK > 0)
                {
                    if (ContadorEquiposNOK > 0){
                        Util.ShowMessage("\t\t  Se encontrarón etiquetas de " + ContadorEquiposOK + " equipos.\n Número de etiquetas no encontradas " + ContadorEquiposNOK + ", Registros guardados satisfactoriamente");
                    }
                    else
                    {
                        Util.ShowMessage("   Número de etiquetas impresas " + ContadorEquiposOK + ",\nRegistros guardados satisfactoriamente");
                    }
                }
                else if (ContadorEquiposNOK > 0) {
                    Util.ShowMessage("Etiquetas no encontradas, número de etiquetas no encontradas " + ContadorEquiposNOK + ",\n\t  Registros guardados satisfactoriamente");         
                }
                else
                {
                    //Muestro el mensaje de confirmacion
                    Util.ShowMessage("Registros guardados satisfactoriamente.");
                }
                
                //Reinicio los campos
                LimpiarDatosIngresoSeriales();
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            //String ConsultaSQL = "", NuevaUbicacion, NuevoEstado;

            ////Evaluo que haya sido seleccionado un registro
            //if (View.ListadoItems.SelectedItems.Count == 0)
            // return;

            ////Evaluo que haya seleccionado la nueva clasificacion
            //if (View.Ubicacion.SelectedIndex == -1)
            //{
            // Util.ShowError("Por favor seleccionar la nueva clasificacion.");
            // return;
            //}

            ////Coloco la ubicacion
            //NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            ////Valido la ubicacion para colocar el estado
            //if (NuevaUbicacion != "VERIFICACION")
            // NuevoEstado = "VERIFICACION";
            //else
            // NuevoEstado = "VERIFICACION";

            //foreach (DataRowView item in View.ListadoItems.SelectedItems)
            //{
            // //Creo la consulta para cambiar la ubicacion de la estiba
            // ConsultaSQL += " UPDATE dbo.EquiposCLARO SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "' WHERE RowID = '" + item.Row["RowID"] + "'";

            // //Ejecuto la consulta
            // service.DirectSQLNonQuery(ConsultaSQL, Local);
            //}

            ////Muestro el mensaje de confirmacion
            //Util.ShowMessage("Cambio de ubicacion realizado satisfactoriamente.");

            //ListarDatos();

            ////Quito la selecion de la nueva ubicacion
            //View.Ubicacion.SelectedIndex = -1;

            ////Quito la seleccion del listado
            //View.ListadoItems.SelectedIndex = -1;

        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        private void OnDeleteDetails(object sender, EventArgs e)
        {
            if (View.ListadoItems.SelectedItems.Count == 0)
                Util.ShowMessage("Debe seleccionar al menos una fila para eliminar");

            //borra de la lista los elementos seleccionados
            while (View.ListadoItems.SelectedItems.Count > 0)
                View.Model.ListRecords.Rows.RemoveAt(View.ListadoItems.Items.IndexOf(View.ListadoItems.SelectedItem));
        }

        #endregion

    }
}