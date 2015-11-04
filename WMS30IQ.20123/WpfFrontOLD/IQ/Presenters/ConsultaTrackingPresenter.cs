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
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using WpfFront.Views;
using Assergs.Windows;
using System;
using WpfFront.IQ.Models;


namespace WpfFront.Presenters
{
    public interface IConsultaTrackingPresenter
    {
        IConsultaTrackingView View { get; set; }
        ToolWindow Window { get; set; }
    }
    public class ConsultaTrackingPresenter :  IConsultaTrackingPresenter
    {
        public IConsultaTrackingView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private string userName = App.curUser.UserName;
        private string user = App.curUser.FirstName + " " + App.curUser.LastName;
        private int cont = 0;

        public ConsultaTrackingPresenter(IUnityContainer container, IConsultaTrackingView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ConsultaTrackingModel>();
            #region Metodos

            View.ConsultarMovimientos += new EventHandler<EventArgs>(this.OnConsultarMovimientos);
            View.BuscarEquipoTracking += new EventHandler<EventArgs>(this.OnBuscarEquipoTracking);
            View.exportarTracking += new EventHandler<EventArgs>(this.OnexportarTracking);
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;

            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListadoEstatusLogPro = service.GetMMaster(new MMaster { MetaType = new MType { Code = "LOGPROSTAT" } });
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            //View.Model.ListadoModelos = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DRIRECMODE" } });
            View.Model.ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            View.Model.ListadoTipoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            View.Model.ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            View.Model.ListadoFallas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLADIA" } });
            View.Model.ListadoMotScrap = service.GetMMaster(new MMaster { MetaType = new MType { Code = "MOTSCRAP" } });
            View.Model.ListadoEstadoRR = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADO RR" } });

            //View.Model.ListadoProductos = service.GetProduct(new Product());

            //Utilizar el service.GetProduct genera un retardo muy grande en la carga de la vista asi que se utiliza de la siguiente manera para evitar retardos

            DataTable aux = service.DirectSQLQuery("select brand,ProductID,ProductCode,Manufacturer from Master.Product", "", "Master.Product", Local);

            List<WpfFront.WMSBusinessService.Product> list = aux.AsEnumerable()
               .Select(row => new WpfFront.WMSBusinessService.Product
               {
                   ProductID = Convert.ToInt32(row["ProductID"]),
                   Brand = row["brand"].ToString(),
                   ProductCode = row["ProductCode"].ToString(),
                   Manufacturer = row["Manufacturer"].ToString()
               }).ToList<WpfFront.WMSBusinessService.Product>();

            View.Model.ListadoProductos = list;
            /////////////////////////////////////////////////////////////////////////////////////

            View.Model.ListadoEquipos = service.DirectSQLQuery("exec sp_GetProcesos2 'HISTORICOSERIAL'", "", "dbo.movimientoclaro", Local);

            #endregion
        }
        public void OnexportarTracking(object sender, EventArgs e)
        {
            if (View.ListadoEquipos_Track.SelectedIndex == -1)
            {
                Util.ShowMessage("Por favor seleccione un equipo para poder exportar todos sus movimientos");
                return;
            }

            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;
            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                wb = excel.Workbooks.Add();
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                for (int i = 0; i < View.Model.ListMovimientos.Columns.Count; i++)
                {
                    ws.Range["A1"].Offset[0, i].Value = View.Model.ListMovimientos.Columns[i].ColumnName.ToString().ToUpper(); ;
                    ws.Range["A1"].Offset[0, i].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                }

                for (int i = 0; i < View.Model.ListMovimientos.Rows.Count; i++)
                {
                    ws.get_Range("A1", "H" + cont + 1).EntireColumn.NumberFormat = "@";
                    ws.Range["A2"].Offset[i].Resize[1, View.Model.ListMovimientos.Columns.Count].Value = View.Model.ListMovimientos.Rows[i].ItemArray;
                }

                rng = ws.get_Range("A1", "H" + View.Model.ListMovimientos.Rows.Count + 1);
                rng.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                rng.Columns.AutoFit();

                ws.Name = "Movimiento del equipo ";
                excel.Visible = true;
                wb.Activate();
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error al crear el archivo excel de exportación " + ex.Message);
            }
        }
        public void OnBuscarEquipoTracking(object sender, EventArgs e)
        {

            String serial_track = View.GetSerialTrack.Text;
            String mac_track = View.GetMac_Track.Text;

            try
            {
                View.Model.ListadoEquipos = service.DirectSQLQuery("exec sp_GetProcesos2 'HISTORIAL_BYSERIAL','" + serial_track + "','" + mac_track + "'", "", "dbo.movimientoclaro", Local);
                View.Model.ListMovimientos = service.DirectSQLQuery("exec sp_GetProcesos2 'TRACK_BYSERIAL','" + serial_track + "','" + mac_track + "'", "", "dbo.movimientoclaro", Local);

                View.ListadoEquipos_Track.SelectedIndex = 0;
            }
            catch (Exception ex)
            {

            }
        }
        public void OnConsultarMovimientos(object sender, EventArgs e)
        {
            String serial = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["Serial"].ToString();
            String mac = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["mac"].ToString();
            String estado_actual = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["estado_actual"].ToString();

            if (serial != null && mac != null)
            {
                DataTable datos_grafica;

                if (estado_actual == "ACTIVO")
                {
                    datos_grafica = service.DirectSQLQuery("EXEC sp_GetProcesos2 'AGING_BYAREA','" + serial + "','" + mac + "'", "", "dbo.movimientoclaro", Local);
                }
                else
                {
                    datos_grafica = service.DirectSQLQuery("EXEC sp_GetProcesos2 'AGING_BYAREA_DESPACHO','" + serial + "','" + mac + "'", "", "dbo.movimientoclaro", Local);
                }

                View.Model.ListMovimientos = service.DirectSQLQuery("EXEC sp_GetProcesos2 'TRACK_BYSERIAL','" + serial + "','" + mac + "'", "", "dbo.movimientoclaro", Local);
                List<String> colores = new List<String>(new String[] { "#21E10D", "#E60E0E", "#FA9D0A", "#7B5FBB", "#4CD8D8", "#0099FF", "#41F717" });

                View.chart1.Series.Clear();
                View.chart1.ChartAreas.Clear();

                View.chart1.Series.Add("S");
                View.chart1.ChartAreas.Add("A");

                View.chart1.Series["S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;

                int cont = 0;
                foreach (DataRow fila in datos_grafica.Rows)
                {
                    View.chart1.Series["S"].Points.AddXY(fila["mov_codigoUbicacion"].ToString() + " " + fila["tiempo"].ToString(), fila["segundos"]);
                    View.chart1.Series["S"].Points[cont].Color = System.Drawing.ColorTranslator.FromHtml(colores[cont]);
                    cont++;
                }
            }
            
        }
        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAAdministrador', 'PARA Administrador', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }
   
    }
}