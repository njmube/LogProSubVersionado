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

    public interface INovedadesPresenter
    {
        INovedadesView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class NovedadesPresenter : INovedadesPresenter
    {
        public INovedadesView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public NovedadesPresenter(IUnityContainer container, INovedadesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<NovedadesModel>();


            #region Metodos

            View.VerEquiposNovedades += new EventHandler<EventArgs>(this.OnVerEquiposNovedades);
            View.ExportPrealertas += new EventHandler<EventArgs>(this.OnExportPrealertas);
            View.ExportNovedadTipoA += new EventHandler<EventArgs>(this.OnExportNovedadTipoA);
            View.ExportNovedadTipoB += new EventHandler<EventArgs>(this.OnExportNovedadTipoB);
            View.BuscarPrealertas += new EventHandler<EventArgs>(this.OnBuscarPrealertas);
            //View.BuscarNoveTipoB += new EventHandler<EventArgs>(this.OnBuscarNoveTipoB);

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }


            CargarDatosDetails();
            ActualizarListPrealertas();
            ActualizarListNoveTipoB();
            ActualizarComboArchivos();

            #endregion
        }

        #region Metodos


        public void CargarDatosDetails()
        {
            //Inicializo el DataTable de prealertas
            View.Model.ListadoPrealertas = new DataTable("ListadoPrealertas");
            //View.Model.ListadoPrealertas.Columns.Add("prea_id", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_archivo", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_consecutivo", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_origen", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_nombreOrigen", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_direccion", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_Contacto", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_contactMovil", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_nroPedido", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_tipoRecoleccion", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_fechaEmitido", typeof(String));
            View.Model.ListadoPrealertas.Columns.Add("prea_fechaRegistro", typeof(String));


            //inicializo el datatable de lista de Novedades de la prealerta seleccionada - TIPO A
            View.Model.ListadoNovedades = new DataTable("ListadoNovedadesTipoA");
            //View.Model.ListadoNovedades.Columns.Add("prea_id", typeof(String));
            //View.Model.ListadoNovedades.Columns.Add("nove_id", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("Serial", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("SAP", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("Cantidad", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("Descripcion", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("nove_fechaRegistro", typeof(String));

            //inicializo el datatable de lista de Novedades del TIPO B
            View.Model.ListadoNovedades = new DataTable("ListadoNovedadesTipoB");
            //View.Model.ListadoNovedades.Columns.Add("nove_id", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("Serial", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("SAP", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("Modelo", typeof(String));
            View.Model.ListadoNovedades.Columns.Add("nove_fechaRegistro", typeof(String));
        }

        private void ActualizarListPrealertas()
        {
            //Creo la consulta para buscar los ultimos 15 prealertas registradas
            String ConsultaSQL = "EXEC sp_GetProcesos 'LISTADOPREALERTAS','','','',''";
            View.Model.ListadoPrealertas = service.DirectSQLQuery(ConsultaSQL, "", "dbo.preAlertaCLARO", Local);
        }

        private void ActualizarListNoveTipoB()
        {
            //Creo la consulta para buscar los ultimos 15 prealertas registradas
            String ConsultaSQL = "EXEC sp_GetProcesos 'LISTADONOVEDADESTIPOB','','','',''";
            View.Model.ListadoNovedadesTipoB = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Novedad_PREAEQUIPOCLARO", Local);
        }

        private void OnVerEquiposNovedades(object sender, EventArgs e)
        {
            //Creo la consulta para cargar las noveades de la prealerta seleccionada
            String ConsultaSQL = "EXEC sp_GetProcesos 'LISTADONOVEDADESTIPOA','','','',''";
            View.Model.ListadoNovedadesTipoA = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Novedad_PREAEQUIPOCLARO", Local);
        }

        private void OnBuscarPrealertas(object sender, EventArgs e)
        {

            String aux_archivo = View.ComboArchivos.Text.ToString();

            String aux_fechaEmitido = View.GetFechaEmitido.Text.ToString();
            String aux_fechaRegistro = View.GetFechaRegistro.Text.ToString();
            Console.WriteLine(aux_fechaRegistro);
            //Creo la consulta para cargar las noveades de la prealerta seleccionada
            String ConsultaSQL = "EXEC sp_GetProcesos 'LISTADOPREALERTAS','" + aux_archivo + "','" + aux_fechaEmitido + "','" + aux_fechaRegistro + "',''";
            View.Model.ListadoPrealertas = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Novedad_PREAEQUIPOCLARO", Local);

            ActualizarComboArchivos();
            LimpiarFiltros();
        }

        private void ActualizarComboArchivos()
        {
            //Creo la consulta para buscar los ultimos 15 prealertas registradas
            String ConsultaSQL = "EXEC sp_GetProcesos 'LISTADOARCHIVOS','','','',''";
            View.Model.ListadoArchivos = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Novedad_PREAEQUIPOCLARO", Local);
        }

        private void LimpiarFiltros()
        {
            View.ComboArchivos.Text = "";
            View.GetFechaEmitido.Text = "";
            View.GetFechaRegistro.Text = "";
        }

        private void OnExportPrealertas(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {

                if (View.ListadoBusquedaPrealertas.Items.Count > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaPrealertas.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaPrealertas.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DimGray);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoBusquedaPrealertas.Items)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaPrealertas.Columns.Count].Value =
                                Registros.Row.ItemArray;
                        cont++;
                    }

                    rng = ws.get_Range("A1", "H" + cont + 1);
                    rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    rng.Columns.AutoFit();

                    excel.Visible = true;
                    wb.Activate();
                }
                else
                {
                    Util.ShowMessage("La tabla no tiene registros para exportar.");
                }
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }

        private void OnExportNovedadTipoA(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {
                if (View.ListadoBusquedaNovedadesTipoA.Items.Count > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaNovedadA.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaNovedadA.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DimGray);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoBusquedaNovedadesTipoA.Items)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaNovedadA.Columns.Count].Value =
                                Registros.Row.ItemArray;
                        cont++;
                    }

                    rng = ws.get_Range("A1", "H" + cont + 1);
                    rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    rng.Columns.AutoFit();

                    excel.Visible = true;
                    wb.Activate();
                }
                else
                {
                    Util.ShowMessage("La tabla no tiene registros para exportar.");
                }
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }


        private void OnExportNovedadTipoB(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {
                if (View.ListadoBusquedaNovedadesTipoB.Items.Count > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaNovedadB.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaNovedadB.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DimGray);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoBusquedaNovedadesTipoB.Items)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaNovedadB.Columns.Count].Value =
                                Registros.Row.ItemArray;
                        cont++;
                    }

                    rng = ws.get_Range("A1", "H" + cont + 1);
                    rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    rng.Columns.AutoFit();

                    excel.Visible = true;
                    wb.Activate();
                }
                else
                {
                    Util.ShowMessage("La tabla no tiene registros para exportar.");
                }
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error creando el archivo Excel: " + ex.ToString());
            }
        }

        //private void OnBuscarNoveTipoB(object sender, EventArgs e)
        //{
        //    String aux_fechaRegistroNoveTipoB = View.GetFechaRegistroNoveTipoB.Text.ToString();

        //    //Creo la consulta para cargar las noveades de la prealerta seleccionada
        //    String ConsultaSQL = "EXEC sp_GetProcesos 'FILTROFECHA_NOVETIPOB','" + aux_fechaRegistroNoveTipoB + "','','',''";
        //    View.Model.ListadoNovedadesTipoB = service.DirectSQLQuery(ConsultaSQL, "", "dbo.Novedad_PREAEQUIPOCLARO", Local);
        //    Console.WriteLine(aux_fechaRegistroNoveTipoB);
        //}

        #endregion

    }
}