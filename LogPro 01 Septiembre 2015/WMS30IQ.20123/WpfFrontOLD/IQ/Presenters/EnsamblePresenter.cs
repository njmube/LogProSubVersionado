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

    public interface IEnsamblePresenter
    {
        IEnsambleView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EnsamblePresenter : IEnsamblePresenter
    {
        public IEnsambleView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public EnsamblePresenter(IUnityContainer container, IEnsambleView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EnsambleModel>();
           

            #region Metodos

            //View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);           

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListUbicacionesDestino = service.DirectSQLQuery("EXEC sp_GetProcesos 'UBICACIONESDESTINO', 'BODEGA', 'CLARO'", "", "dbo.Ubicaciones", Local);

            CargarDatosDetails();
            ListarDatos();
           

            #endregion
        }

       

        #region Metodos

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            
        }

        public void CargarDatosDetails()
        {
            //Inicializo el DataTable
            View.Model.ListRecords = new DataTable("ListadoIngresos");

            //Asigno las columnas
            View.Model.ListRecords.Columns.Add("ProductoID", typeof(String));
            View.Model.ListRecords.Columns.Add("Producto", typeof(String));
            View.Model.ListRecords.Columns.Add("Cantidad", typeof(String));
            View.Model.ListRecords.Columns.Add("Serial", typeof(String));
            View.Model.ListRecords.Columns.Add("Mac", typeof(String));
            View.Model.ListRecords.Columns.Add("Estado", typeof(String));

            //Cargo los datos de los combobox
            //View.Model.ListCampo1 = service.GetMMaster(new MMaster { MetaType = new MType { MetaTypeID = 161 } });
        }

        private void ListarDatos() 
        {

            DataSet ds = Util.GetListaDatosModulo("BODEGA", null);

            View.Model.ListRecords_1 = ds.Tables[0]; 
        
        }

        private void OnEvaluarTipoProducto(object sender, DataEventArgs<Product> e)
        {
            //if (View.GetProductLocation.Product != null)
            //{
            //    //Asigno el producto seleccionado a la variable del modelo
            //    View.Model.ProductoSerial = e.Value;
            //    //Inicio los campos para ingresar los seriales
            //    View.GetCantidadProducto.Text = "1";
            //    View.GetCantidadProducto.IsEnabled = false;
            //    View.GetSerial1.IsEnabled = View.GetSerial2.IsEnabled = View.GetUpLoadFile.IsEnabled = true;
            //    View.GetSerial1.Focus();
            //}
        }

      

        private void OnAddLine(object sender, EventArgs e)
        {

            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();


            //if (String.IsNullOrEmpty(View.GetProductLocation.Text.ToString()))
            //{
            //    Util.ShowError("Por favor ingrese un producto");
            //    return;
            //}
            //if (String.IsNullOrEmpty(View.GetCantidadProducto.Text.ToString()))
            //{
            //    Util.ShowError("Por favor ingrese la cantidad de producto");
            //    return;
            //}
            if (String.IsNullOrEmpty(View.GetSerial1.Text.ToString()))
            {
                Util.ShowError("Por favor ingrese el serial decodificador");
                return;
            }
            else if (String.IsNullOrEmpty(View.GetSerial2.Text.ToString()))
            {
                Util.ShowError("Por favor ingrese el serial MAC");
                return;
            }

            foreach (DataRow item in View.Model.ListRecords.Rows)
            {
                if (View.GetSerial1.Text == item.ItemArray[3].ToString())
                {
                    Util.ShowError("El serial " + View.GetSerial1.Text + " ya existe en el sistema");
                    return;
                }
            }

            //Asigno los campos
            //dr[0] = View.Model.ProductoSerial.ProductID;
            //dr[1] = View.Model.ProductoSerial.Name;
            //dr[2] = View.GetCantidadProducto.Text;
            dr[3] = View.GetSerial1.Text;
            dr[4] = View.GetSerial2.Text;

            //Agrego el registro al listado
            View.Model.ListRecords.Rows.Add(dr);

            //Limpio los seriales para digitar nuevos datos
            View.GetSerial1.Text = "";
            View.GetSerial2.Text = "";
            View.GetSerial1.Focus();
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
                dr1[0] = View.Model.ProductoSerial.ProductID;
                dr1[1] = View.Model.ProductoSerial.Name;
               // dr1[2] = View.GetCantidadProducto.Text;

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
                        ConsultaGuardar += " INSERT INTO dbo.EquiposCLARO(ProductoID,Producto,Cantidad,Serial,Mac,Campo1) VALUES(";

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
                    }
                    else
                    {
                        //Obtengo la cantidad de columnas del listado
                        ContadorCampos = View.Model.ListRecords.Columns.Count;

                        //Construyo la consulta para guardar los datos
                        ConsultaGuardar += " INSERT INTO dbo.EquiposCLARO(ProductoID,Producto,Cantidad,Serial,Mac,Campo1) VALUES(";

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
            //Inicializo el producto seleccionado
            //View.GetProductLocation.Product = null;
            //View.GetProductLocation.ProductDesc = "";
            //View.GetProductLocation.Text = "";
            //Limpio la cantidad de producto ingresado
            //View.GetCantidadProducto.Text = "";
            ////Inicializo la variable modelo del producto seleccionado
            //View.Model.ProductoSerial = null;
            ////Habilito los campos de seriales y cantidades para el producto
            //View.GetCantidadProducto.IsEnabled = View.GetSerial1.IsEnabled = View.GetSerial2.IsEnabled = View.GetUpLoadFile.IsEnabled = true;
        }



        #endregion

    }
}