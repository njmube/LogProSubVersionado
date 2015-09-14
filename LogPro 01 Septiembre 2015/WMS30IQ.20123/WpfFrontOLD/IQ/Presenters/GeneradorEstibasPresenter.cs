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

    public interface IGeneradorEstibasPresenter
    {
        IGeneradorEstibasView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class GeneradorEstibasPresenter : IGeneradorEstibasPresenter
    {
        public IGeneradorEstibasView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 2; //# columnas que no se debe replicar porque son fijas.

        public GeneradorEstibasPresenter(IUnityContainer container, IGeneradorEstibasView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<GeneradorEstibasModel>();

            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);

            #endregion

            #region Datos

            //Cargo la variable para las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo los productos
            View.Model.ListadoProductos = service.GetProduct(new Product { Reference = "1" });

            //Cargo los datos del listado
            CargarDatosDetails();

            #endregion
        }

        #region Metodos

        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();

            //Evaluo que haya sido digitado el serial para buscar
            //if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            //{
            //    Util.ShowError("El campo serial no puede ser vacio.");
            //    return;
            //}           

            //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
            //foreach (DataRow item in View.Model.ListRecords.Rows)
            //{
            //    if (View.GetSerial1.Text == item["Serial"].ToString())
            //    {
            //        Util.ShowError("El serial " + View.GetSerial1.Text + " ya esta en el listado.");
            //        return;
            //    }
            //}

            //Asigno los campos
            //dr["Producto"] = RegistroValidado.Rows[0]["Producto"].ToString();
            //dr["Serial"] = View.GetSerial1.Text.ToString();
            ////dr["Mac"] = View.GetSerial2.Text.ToString();

            ////Agrego el registro al listado
            //View.Model.ListRecords.Rows.Add(dr);

            ////Limpio los seriales para digitar nuevos datos
            //View.GetSerial1.Text = "";
            ////View.GetSerial2.Text = "";
            //View.GetSerial1.Focus();
        }

        private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        {
            //Variables Auxiliares
            DataRow RegistroGuardar;

            foreach (DataRow dr in e.Value.Rows)
            {
                //Inicializo el registro para guardar el dato
                RegistroGuardar = View.Model.ListRecords.NewRow();

                //Recorro el listado de equipos ingresados al listado para saber que el serial no este ya ingresado
                foreach (DataRow item in View.Model.ListRecords.Rows)
                {
                    if (dr[0].ToString() == item["Serial"].ToString())
                        continue;
                }

                try
                {
                    //Asigno los campos
                    RegistroGuardar["Serial"] = dr[0].ToString();
                    RegistroGuardar["Mac"] = dr[1].ToString();
                    RegistroGuardar["ProductoID"] = dr[2].ToString();
                    RegistroGuardar["Origen"] = dr[3].ToString();
                    RegistroGuardar["Ciudad"] = dr[4].ToString();
                    RegistroGuardar["Aliado"] = dr[5].ToString();
                    RegistroGuardar["Codigo_SAP"] = dr[6].ToString();
                    RegistroGuardar["Estado_RR"] = dr[7].ToString();
                    RegistroGuardar["Observaciones"] = dr[8].ToString();
                    RegistroGuardar["Tipo_Rec"] = dr[9].ToString();
                    RegistroGuardar["Fecha_Ingreso"] = dr[10].ToString();

                    //Agrego el registro al listado
                    View.Model.ListRecords.Rows.Add(RegistroGuardar);
                }
                catch (Exception Ex)
                {
                    continue;
                }
            }
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoRegistros");

            //Asigno las columnas restantes
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));

            //Genero las columnas dinamicas
            GenerarColumnasDinamicas();
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
            Int32 ContadorCampos, ContadorFilas = 0;

            try
            {
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Aumento el contador de filas
                    ContadorFilas++;

                    if (ContadorFilas % 50 != 0)
                    {
                        //Obtengo la cantidad de columnas del listado
                        ContadorCampos = View.Model.ListRecords.Columns.Count;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " INSERT INTO dbo.EquiposCLARO(Serial,Mac,ProductoID,Origen,Ciudad,Aliado,Codigo_SAP,Estado_RR,Observaciones,Tipo_REC,Fecha_Ingreso) VALUES(";

                        //Obtengo los datos de cada campo con su nombre
                        foreach (DataColumn c in View.Model.ListRecords.Columns)
                        {
                            if (String.IsNullOrEmpty(DataRow["ProductoID"].ToString()))
                            {
                                Util.ShowError("El campo Producto no puede ser vacio.");
                                return;
                            }

                            //Adiciono cada dato a la consulta
                            ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                            //Evaluo el contador de columnas para saber si adiciono la coma
                            ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                            //Disminuyo el contador
                            ContadorCampos--;
                        }

                        //Termino la consulta
                        ConsultaGuardar += ")";
                    }
                    else
                    {
                        //Obtengo la cantidad de columnas del listado
                        ContadorCampos = View.Model.ListRecords.Columns.Count;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " INSERT INTO dbo.EquiposCLARO(Serial,Mac,ProductoID,Origen,Ciudad,Aliado,Codigo_SAP,Estado_RR,Observaciones,Tipo_REC,Fecha_Ingreso) VALUES(";

                        //Obtengo los datos de cada campo con su nombre
                        foreach (DataColumn c in View.Model.ListRecords.Columns)
                        {
                            //Adiciono cada dato a la consulta
                            ConsultaGuardar = ConsultaGuardar + "'" + DataRow[c.ColumnName].ToString() + "'";

                            //Evaluo el contador de columnas para saber si adiciono la coma
                            ConsultaGuardar += (ContadorCampos != 1) ? "," : "";

                            //Disminuyo el contador
                            ContadorCampos--;
                        }

                        //Termino la consulta
                        ConsultaGuardar += ")";


                        //Ejecuto la consulta
                        service.DirectSQLNonQuery(ConsultaGuardar, Local);

                        //Limpio la consulta para volver a generar la nueva
                        ConsultaGuardar = "";
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

            #region Columna Producto

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Producto";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoProductos);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "ProductCode");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("ProductoID"));
            Txt.SetValue(ComboBox.WidthProperty, (double)150);
            Txt.SetValue(ComboBox.HeightProperty, (double)22);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("ProductoID", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Origen

            IList<MMaster> ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Origen";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoOrigen);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Origen"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Origen", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Ciudad

            IList<MMaster> ListadoCiudades = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ciudad";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoCiudades);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Ciudad"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Ciudad", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Aliado

            IList<MMaster> ListadoAliado = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Aliado";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoAliado);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Aliado"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Aliado", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Codigo SAP

            IList<MMaster> ListadoCodigoSAP = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TELMEXCOD" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Codigo_SAP";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoCodigoSAP);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Codigo_SAP"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
           // View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Codigo_SAP", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Estado RR

            IList<MMaster> ListadoEstadoRR = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADO RR" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Estado_RR";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoEstadoRR);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Estado_RR"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Estado_RR", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Observaciones

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.TextBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.TextBox";
            Columna.Header = "Observaciones";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(TextBox.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Observaciones"));
            Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
            Txt.SetValue(TextBox.IsTabStopProperty, true);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Observaciones", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Tipo Rec

            IList<MMaster> ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Tipo_Rec";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, ListadoTipoREC);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Tipo_Rec"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Tipo_Rec", typeof(String)); //Creacion de la columna en el DataTable

            #endregion

            #region Columna Fecha Ingreso

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("Microsoft.Windows.Controls.DatePicker, WPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=c93dde70475aea7e"));
            TipoDato = "Microsoft.Windows.Controls.DatePicker";
            Columna.Header = "Fecha_Ingreso";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)130);
            Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding("Fecha_Ingreso"));

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            //View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Fecha_Ingreso", typeof(String)); //Creacion de la columna en el DataTable

            #endregion
        }

        #endregion

    }
}