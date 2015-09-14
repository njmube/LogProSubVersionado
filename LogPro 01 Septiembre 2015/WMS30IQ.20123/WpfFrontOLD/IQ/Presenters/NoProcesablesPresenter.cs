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

    public interface INoProcesablesPresenter
    {
        INoProcesablesView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class NoProcesablesPresenter : INoProcesablesPresenter
    {
        public INoProcesablesView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public NoProcesablesPresenter(IUnityContainer container, INoProcesablesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<NoProcesablesModel>();


            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += this.OnConfirmarMovimiento;

            #endregion

            #region Datos

            //Cargo la variable de las consultas directas
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            //Cargo los datos del listado de ubicaciones destino
            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'NOPROCESABLES', 'CLARO'", "", "dbo.Ubicaciones", Local);

            //Cargo las ubicaciones
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });

            //Cargo los datos del listado
            CargarDatosDetails();

            //Carlo el listado de estibas en el modulo
            ListarDatos();
           
            #endregion
        }

        #region Metodos

        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            DataTable RegistroValidado;

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
                    return;
                }
            }

            try
            {
                //Busco el registro en la DB para validar que exista y que este en la ubicacion valida
                RegistroValidado = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCAREQUIPO','" + View.GetSerial1.Text + "'", "", "dbo.EquiposCLARO", Local);

                //Evaluo si el serial existe
                if (RegistroValidado.Rows.Count == 0)
                {
                    Util.ShowError("El serial no existe o no esta en la ubicacion requerida.");
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
                    //dr["IdPallet"] = RegistroValidado.Rows[0]["IdPallet"].ToString();

                    //Agrego el registro al listado
                    View.Model.ListRecords.Rows.Add(dr);

                    //Limpio los seriales para digitar nuevos datos
                    View.GetSerial1.Text = "";
                    View.GetSerial2.Text = "";
                    View.GetSerial1.Focus();
                }
            }
            catch (Exception Ex)
            {
                Util.ShowError("Hubo un error al momento de validar el serial. Error: " + Ex.Message);
                return;
            }
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
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));
            View.Model.ListRecords.Columns.Add("IdPallet", typeof(String));

            Columna = new GridViewColumn();
            assembly = Assembly.GetAssembly(Type.GetType("System.Windows.Controls.ComboBox, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            TipoDato = "System.Windows.Controls.ComboBox";
            Columna.Header = "Ubicaciones";
            Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));
            Txt.SetValue(ComboBox.ItemsSourceProperty, View.Model.ListadoPosiciones);
            Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
            //Txt.SetValue(ComboBox.SelectedValuePathProperty, "MetaMasterID");
            Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
            Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding("Posicion"));
            Txt.SetValue(ComboBox.WidthProperty, (double)110);

            // add textbox template
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.ListadoEquipos.Columns.Add(Columna); //Creacion de la columna en el GridView
            View.Model.ListRecords.Columns.Add("Posicion", typeof(String)); //Creacion de la columna en el DataTable
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
                    //Aumento el contador de filas
                    ContadorFilas++;

                    if (ContadorFilas % 50 != 0)
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Posicion = '" + DataRow["Posicion"].ToString() + "', IdPallet = '" + DataRow["IdPallet"].ToString() + "', Ubicacion = 'NOPROCESABLES' WHERE RowID = '" + DataRow["RowID"].ToString() + "'";
                    }
                    else
                    {
                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " UPDATE dbo.EquiposCLARO SET Posicion = '" + DataRow["Posicion"].ToString() + "', IdPallet = '" + DataRow["IdPallet"].ToString() + "', Ubicacion = 'NOPROCESABLES' WHERE RowID = '" + DataRow["RowID"].ToString() + "'";

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

                //Cargo el listado de estibas en el modulo
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

        private void ListarDatos()
        {

            DataSet ds = Util.GetListaDatosModulo("NOPROCESABLES", null);

            View.Model.ListRecords2 = ds.Tables[0];

        }

        private void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Evaluo que haya sido seleccionado un destino
            if (View.UbicacionDestino.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar una ubicacion destino.");
                return;
            }

            //Evaluo que haya sido seleccionado alguna estiba para mover
            if (View.ListadoEstibas.SelectedItems.Count == 0)
            {
                Util.ShowError("Por favor seleccionar la(s) estiba(s) que se desea mover.");
                return;
            }

            //Recorro cada estiba seleccionada para actualizar la nueva posicion de cada serial asignado a esta
            foreach (DataRowView Registro in View.ListadoEstibas.SelectedItems)
            {
                //Creo la query
                ConsultaSQL = "EXEC sp_GetProcesos 'UPDATEUBICACIONES', '" + ((DataRowView)View.UbicacionDestino.SelectedValue).Row["UbicacionDestino"] + "', '" + Registro["Estiba"].ToString() + "'";

                //Ejecuto la query
                service.DirectSQLNonQuery(ConsultaSQL, Local);
            }

            //Recargo el listado de estibas luego del movimiento
            ListarDatos();

            //Quito la seleccion de los listados
            View.UbicacionDestino.SelectedIndex = -1;

            //Confirmo el proceso
            Util.ShowMessage("Moviemiento de estibas realizado correctamente.");
        }

        #endregion

    }
}