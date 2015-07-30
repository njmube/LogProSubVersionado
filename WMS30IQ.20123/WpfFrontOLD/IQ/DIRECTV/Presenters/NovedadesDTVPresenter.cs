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

    public interface INovedadesDTVPresenter
    {
        INovedadesDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class NovedadesDTVPresenter : INovedadesDTVPresenter
    {
        public INovedadesDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public NovedadesDTVPresenter(IUnityContainer container, INovedadesDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<NovedadesDTVModel>();


            #region Metodos

            View.ExportNovedades += new EventHandler<EventArgs>(this.OnExportNovedades);

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            CargarListadoNovedades();
            
            #endregion
        }

        #region Metodos

        public void CargarListadoNovedades()
        {
            String ConsultaSQL = "EXEC sp_GetProcesosDIRECTVC 'BUSCARLISTNOVEDADES','','','',''";
            View.Model.ListadoNovedades = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposDIRECTVC", Local);
        }

        private void OnExportNovedades(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;

            try
            {

                if (View.ListadoBusquedaNovedades.Items.Count > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();

                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int Idx = 0; Idx < View.GridViewListaNovedades.Columns.Count; Idx++)
                    {
                        ws.Range["A1"].Offset[0, Idx].Value = View.GridViewListaNovedades.Columns[Idx].Header.ToString();
                        ws.Range["A1"].Offset[0, Idx].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DimGray);
                    }
                    int cont = 0;

                    foreach (DataRowView Registros in View.ListadoBusquedaNovedades.Items)
                    {
                        ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";

                        ws.Range["A2"].Offset[cont].Resize[1, View.GridViewListaNovedades.Columns.Count].Value =
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

        #endregion

    }
}