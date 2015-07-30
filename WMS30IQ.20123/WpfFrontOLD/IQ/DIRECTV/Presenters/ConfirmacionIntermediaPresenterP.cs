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
using System.Windows.Data;

namespace WpfFront.Presenters
{

    public interface IConfirmacionIntermediaPresenterP
    {
        IConfirmacionIntermediaViewP View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ConfirmacionIntermediaPresenterP : IConfirmacionIntermediaPresenterP
    {
        public IConfirmacionIntermediaViewP View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public ConfirmacionIntermediaPresenterP(IUnityContainer container, IConfirmacionIntermediaViewP view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ConfirmacionIntermediaModelP>();

            #region Metodos

            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);

            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.EmpacarConfirmacion += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.FilaSeleccionada += this.OnFilaSeleccionada;
            View.ImprimirHablador += new EventHandler<EventArgs>(this.OnImprimirHablador);
            View.GenerarCodigo += new EventHandler<EventArgs>(this.OnGenerarCodigo);

            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            DataTable table = new DataTable();
            table.Columns.Add("UbicacionDestino", typeof(string));
            table.Rows.Add("DIAGNOSTICO");
            
            View.Model.ListUbicacionesDestino = table;
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            CargarDatosDetails();
            //ListarDatos();

            #endregion
        }
        #region Metodos

        /*Carga los smartCard disponibles en la tabla principal*/
        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;
            //Creo la consulta para buscar los registros

            ConsultaSQL = "select rowid,modelo as prod_nombre,Serial AS prod_serial,receiver as prod_receiver,ESTATUS_DIAGNOSTICO AS prod_status,FALLA_DIAGNOSTICO AS prod_falla from dbo.EquiposDIRECTVC WHERE estado ='P-ETIQUETADO'";

            //Ejecuto la consulta
            View.Model.ListRecords_1 = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListRecords");

            View.Model.ListRecords.Columns.Add("SmartCard", typeof(String));
        }

        private void ListarDatos()
        {
            DataSet ds = Util.GetListaDatosModuloDIRECTVC("DESPACHO", null);

            View.Model.ListRecords_1 = ds.Tables[0];
        }

        public void GenerarColumnasDinamicas()
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;

            #region Columna Estado Material.

            IList<MMaster> ListadoFalla = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TIPODEVDTV" } });

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";

            Columna.Header = "Estado";

            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoFalla);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ESTADO_MATERIAL"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            //add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;

            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ESTADO_MATERIAL", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Fecha Ingreso

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("Microsoft.Windows.Controls.DatePicker, WPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=c93dde70475aea7e"));
            TipoDato = "Microsoft.Windows.Controls.DatePicker";
            Columna.Header = "Fecha Ingreso";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)130);
            Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding("FECHA_INGRESO"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("FECHA_INGRESO", typeof(String)); //Creacion de la columna en el DataTable
            #endregion
        }

        //Recibo
        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            //Busco los registros
            //BuscarRegistrosRecibo();
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

            //Coloco la ubicacion
            NuevaUbicacion = ((DataRowView)View.Ubicacion.SelectedItem).Row["UbicacionDestino"].ToString();

            if (NuevaUbicacion == "DIAGNOSTICO")
            {
                NuevoEstado = "PARA DIAGNOSTICO";

                foreach (DataRowView item in View.ListadoItems.SelectedItems)
                {
                    //Creo la consulta para cambiar la ubicacion de la estiba
                    ConsultaSQL += " UPDATE dbo.EquiposDIRECTVC SET Ubicacion = '" + NuevaUbicacion + "', Estado = '" + NuevoEstado + "', UA = '" + ((ComboBoxItem)View.UnidadAlmacenamiento.SelectedItem).Content.ToString() + "', IdPallet = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE RowID = '" + item.Row["RowID"] + "'";
                    ConsultaTrack += "UPDATE dbo.TrackEquiposDIRECTV SET CODEMPAQUE_REP = '" + View.CodigoEmpaque.Text.ToString() + "' WHERE ID_SERIAL = '" + item.Row["RowID"] + "'";

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

        private void OnFilaSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            string auxiliar = "Buen estado";

            foreach (DataRowView rowView in e.AddedItems)
            {
                auxiliar = rowView.Row[1].ToString();
            }

            //View.BuscarPosicionRecibo.Text = auxiliar;
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

        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            String ConsultaGuardar = "";
            Int32 ContadorFilas = 0;

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    ConsultaGuardar += "INSERT INTO dbo.SmartCardEquiposDIRECTV(SMART_SERIAL,SMART_ESTADO,SMART_FECHA) VALUES(";
                    ConsultaGuardar = ConsultaGuardar + "lower('" + DataRow["SmartCard"].ToString() + "'),'" + DataRow["ESTADO_MATERIAL"].ToString() + "','" + DataRow["FECHA_INGRESO"].ToString() + "');";
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

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
        }

        private void OnImprimirHablador(object sender, EventArgs e)
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

                ConsultaSQL += ",'" + Registros.Row["prod_serial"] + "'";
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
                PrinterControl.PrintMovimientosBodegaDIRECTV(SerialesImprimir, unidad_almacenamiento, codigoEmp, "REPARACIÓN", "DIRECTV", "DAÑO FISICO - " + destino, "", "");
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Se presento un error en el momento de generar el documento, " + ex.Message);
            }
        }

        private void OnGenerarCodigo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataTable RegistroValidado;
            String ConsultaBuscar = "";

            //Creo el número de pallet aleatorio 
            ConsultaBuscar = "SELECT concat(right('0000'+cast(NEXT VALUE FOR dbo.PalletSecuence as varchar),5),right('0'+cast(ABS(CAST(NEWID() as binary(5)) % 1000) as varchar),3)) as idpallet";
            DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposDIRECTVC", Local);

            if (Resultado.Rows.Count > 0)
            {
                //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                RegistroValidado = service.DirectSQLQuery("SELECT TOP 1 Serial FROM dbo.EquiposDIRECTVC WHERE idpallet ='" + Resultado.Rows[0]["idpallet"].ToString() + "' AND idpallet is not null", "", "dbo.EquiposDIRECTVC", Local);

                //Evaluo si el serial existe
                if (RegistroValidado.Rows.Count > 0)
                {
                    Util.ShowError("El número de pallet ya existe, intente generarlo nuevamente.");
                }
                else
                {
                    //Asigno los campos
                    View.CodigoEmpaque.Text = "RES-C" + Resultado.Rows[0]["idpallet"].ToString();

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

        #endregion
    }
}