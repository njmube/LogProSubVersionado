using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assergs.Windows;
using Microsoft.Practices.Unity;
using WpfFront.Services;
using WpfFront.WMSBusinessService;
using WpfFront.IQ.Views;
using WpfFront.Models;
using System.Windows.Input;
using System.Data;
using WpfFront.Common;
using System.Windows;

namespace WpfFront.Presenters
{
    public interface IAdminEstibasPresenter
    {
        IAdminEstibasView View { get; set; }
        ToolWindow Window { get; set; }
    }
    public class AdminEstibasPresenter : IAdminEstibasPresenter
    {
        public IAdminEstibasView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        public Connection Local;
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;

        public AdminEstibasPresenter(IUnityContainer container, IAdminEstibasView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AdminEstibasModel>();

            #region Combinar Estibas

            View.CombinarEstibas += new EventHandler<EventArgs>(this.OnCombinarEstibas);
            View.GenerarEstiba += new EventHandler<EventArgs>(this.OnGenerarPallet);

            #endregion

            #region Adicion de estibas 1 a 1

            View.AddLine += new EventHandler<KeyEventArgs>(this.OnAddLine);
            View.AnadirSeriales += new EventHandler<EventArgs>(this.OnAnadirSeriales);
            View.RemoveItemsSelected += new EventHandler<EventArgs>(this.OnRemoveItemSelected);

            #endregion
            //Cargo la conexion local
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListadoPosicionesUnionEstibas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CLAROPOSIC" } });
          
            this.UbicacionDisponibleUnionEstiba();

            createTable();
        }
        #region Union de estibas

        private void UbicacionDisponibleUnionEstiba()
        {
            try
            {
                DataTable dt_auxiliar = service.DirectSQLQuery("select posicion from dbo.EquiposCLARO where posicion is not null AND (estado LIKE 'ALMACENAMIENTO' OR estado LIKE 'DESPACHO')  group by posicion ", "", "dbo.EquiposCLARO", Local);

                List<String> list = dt_auxiliar.AsEnumerable()
                           .Select(r => r.Field<String>("posicion"))
                           .ToList();

                var query = from item in View.Model.ListadoPosicionesUnionEstibas
                            where !list.Contains(item.Name)
                            select item;

                View.Model.ListadoPosicionesUnionEstibas = query.ToList();
                View.CBO_UbicacionUnionEstibas.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Util.ShowError("No es posible actualizar el listado de ubicaciones disponibles " + ex.Message.ToString());
            }
        }

        private void OnGenerarPallet(object sender, EventArgs e)
        {
            if (View.CBO_UbicacionUnionEstibas.SelectedIndex == -1)
            {
                View.TXT_seleccionarUbicacion.Visibility = Visibility.Visible;
                return;
            }
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
                    View.TXT_palletGeneratedUnionEstibas.Text = "RES-A" + Resultado.Rows[0]["idpallet"].ToString();
                    if (View.CBO_UbicacionUnionEstibas.SelectedIndex > 0)
                    {
                        View.TXT_seleccionarUbicacion.Visibility = Visibility.Hidden;
                    }
                    View.BTN_CombinarEstibas.IsEnabled = true;
                    View.BTN_CombinarEstibas.FontWeight = FontWeights.Light;
                }
            }
            else
            {
                Util.ShowError("No es posible generar el número de pallet, intente en unos momentos.");
            }
        }

        private void OnCombinarEstibas(object sender, EventArgs e)
        {
            if (View.ListViewListadoPallets.SelectedItems.Count <= 1)
            {
                Util.ShowMessage("Por favor seleccione dos o mas estibas de la lista");
                return;
            }
            string nuevaEstiba = View.TXT_palletGeneratedUnionEstibas.Text.ToString();
            string nuevaUbicacion = View.CBO_UbicacionUnionEstibas.Text.ToString();
            string updateQuery = "UPDATE dbo.EquiposClaro SET idPallet = '" + nuevaEstiba + "', Posicion = '" + nuevaUbicacion + "' WHERE idPallet IN (";
            string ConsultaSQL = "";
            string idPallets = "''";
            try
            {
                foreach (DataRowView item in View.ListViewListadoPallets.SelectedItems)
                {
                    ConsultaSQL = "EXEC sp_InsertarNuevo_Movimiento 'UNIÓN DE ESTIBAS MOV. MERCANCIA', '" + nuevaEstiba + "', '" + item[0].ToString() + "', '" + nuevaUbicacion + "', '', 'MOV. MERCANCIA','UNIONESTIBAS','" + this.user + "','';";
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    idPallets += "," + "'" + item[0].ToString() + "'";
                }
                idPallets += ")";
                updateQuery += idPallets;
                service.DirectSQLNonQuery(updateQuery, Local);

            }
            catch (Exception ex)
            {
                Util.ShowMessage("Hubo un error al realizar la combinación de estibas " + ex.Message.ToString());
            }

            this.UbicacionDisponibleUnionEstiba();
            //this.BuscarRegistrosCambioClasificacion();  llamar al metodo de buscar sin parametros para que busque todos
            View.TXT_palletGeneratedUnionEstibas.Text = "";

        }

        #endregion

        #region Adición de seriales 1 a 1

        private void OnAddLine(object sender, KeyEventArgs e)
        {
            string serial;
            string consultaBuscar;
            DataRow dr = View.Model.ListSerialsOneByOne.NewRow();
            DataTable RegistroValidado = null;

            serial = View.TXT_serialAdicionSeriales.Text.ToString();

            foreach (DataRow item in View.Model.ListSerialsOneByOne.Rows)
            {
                if (serial.ToUpper() == item["Serial"].ToString().ToUpper())
                {
                    Util.ShowError("El serial " + serial.ToUpper() + " ya esta en el listado.");
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                    return;
                }
            }
            consultaBuscar = "EXEC sp_GetProcesos 'BUSCAREQUIPOSALMACENAR', '" + serial + "', NULL, NULL";
            try
            {
                RegistroValidado = service.DirectSQLQuery(consultaBuscar, "", "dbo.EquiposCLARO", Local);

                if (RegistroValidado.Rows.Count > 0)
                {
                    dr["Serial"] = RegistroValidado.Rows[0]["serial"].ToString().ToUpper();
                    dr["Producto"] = RegistroValidado.Rows[0]["ProductoID"].ToString();
                    dr["COD_SAP"] = RegistroValidado.Rows[0]["CODIGO_SAP"].ToString();
                    View.Model.ListSerialsOneByOne.Rows.Add(dr);
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                }
                else
                {
                    Util.ShowMessage("¡El equipo no se encuentra registrado en el sistema!");
                    View.TXT_serialAdicionSeriales.Text = "";
                    View.TXT_serialAdicionSeriales.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Ha ocurrido un error " + ex.Message);
            }
        }

        private void createTable()
        {
            // Creo la tabla para añadir las nuevas filas.
            // Inicializo el DataTable.
            View.Model.ListSerialsOneByOne = new DataTable("ListadoRegistros");

            //Asigno las columnas
            View.Model.ListSerialsOneByOne.Columns.Add("Serial", typeof(String));
            View.Model.ListSerialsOneByOne.Columns.Add("Producto", typeof(String));
            View.Model.ListSerialsOneByOne.Columns.Add("COD_SAP", typeof(String));
        }

        private void OnAnadirSeriales(object sender, EventArgs e)
        {
            string idPalletSelected = ((DataRowView)View.ListViewListadoPallets.SelectedItem).Row[0].ToString();
            string UbicacionSelected = ((DataRowView)View.ListViewListadoPallets.SelectedItem).Row[1].ToString();
            string Ubicacion;
            string Estado = Ubicacion = "ALMACENAMIENTO";

            string ConsultaSQL = "";
            string UpdateQuery = "UPDATE dbo.EquiposClaro SET idPallet = '" + idPalletSelected + "', Posicion = '" + UbicacionSelected + "', Ubicacion = '" + Ubicacion + "', Estado = '" + Estado + "' WHERE Serial IN ('' ";
            try
            {
                foreach (DataRowView item in View.LV_serialesOneByOne.Items)
                {
                    ConsultaSQL = "EXEC dbo.sp_InsertarNuevo_Movimiento 'ADICION SERIAL A ESTIBA', 'SERIAL ADICIONADO', '" + item[0].ToString() + "', '" + idPalletSelected + "', '', 'MOV. MERCANCIA','ADICIONSERIALES','" + this.user + "','';";
                    service.DirectSQLNonQuery(ConsultaSQL, Local);
                    UpdateQuery += ", '" + item[0].ToString() + "'";
                }
                UpdateQuery += ")";

                service.DirectSQLNonQuery(UpdateQuery, Local);
                this.UbicacionDisponibleUnionEstiba();
                //this.BuscarRegistrosCambioClasificacion();  llamar al metodo de buscar sin parametros para que busque todos
                View.TXT_serialAdicionSeriales.Text = "";
                View.TXT_serialAdicionSeriales.Focus();
                View.Model.ListSerialsOneByOne.Clear();
            }
            catch (Exception ex)
            {
                Util.ShowError("Se produjo un error al actualizar el listado de seriales: " + ex.Message);
            }
        }

        private void OnRemoveItemSelected(object sender, EventArgs e)
        {
            if (View.LV_serialesOneByOne.SelectedItems.Count == -1)
            {
                Util.ShowMessage("Por favor seleccione uno o mas seriales para remover de la lista");
                return;
            }

            for (int i = 0; i < View.LV_serialesOneByOne.SelectedItems.Count; i++)
            {
                while (View.LV_serialesOneByOne.SelectedItems.Count > 0)
                {
                    View.Model.ListSerialsOneByOne.Rows.RemoveAt(View.LV_serialesOneByOne.Items.IndexOf(View.LV_serialesOneByOne.SelectedItem));
                }
            }
        }

        #endregion
    }
}
