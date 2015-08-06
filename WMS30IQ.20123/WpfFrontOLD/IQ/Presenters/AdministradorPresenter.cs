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
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace WpfFront.Presenters
{

    public interface IAdministradorPresenter
    {
        IAdministradorView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class AdministradorPresenter : IAdministradorPresenter
    {
        public IAdministradorView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;
        private int cont = 0;

        public AdministradorPresenter(IUnityContainer container, IAdministradorView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AdministradorModel>();


            #region Metodos

            //View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.CargarDatosAdministrador += new EventHandler<EventArgs>(this.CargarDatosAdministrador);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            View.ReiniciarCapacitacion += new EventHandler<EventArgs>(this.OnReiniciarCapacitacion);
            View.ConsultarMovimientos += new EventHandler<EventArgs>(this.OnConsultarMovimientos);
            View.BuscarEquipoTracking += new EventHandler<EventArgs>(this.OnBuscarEquipoTracking);
            View.exportarTracking += new EventHandler<EventArgs>(this.OnexportarTracking);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.BuscarNombreMaterial += this.OnBuscarNombreMaterial;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

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
            View.Model.ListadoProductos = service.GetProduct(new Product());

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
            //Ejecuto la consulta
            //try
            //{
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

                //View.chart1.Series["S"].Points.AddXY("DIAGNOSTICO", 30); 
                //View.chart1.Series["S"].Points.AddXY("REPARACION", 60);
                //View.chart1.Series["S"].Points.AddXY("ETIQUETADO", 40);
                //View.chart1.Series["S"].Points.AddXY("VERIFICACION", 10);
                //View.chart1.Series["S"].Points.AddXY("EMPAQUE", 10);
                //View.chart1.Series["S"].Points.AddXY("DESPACHO", 1720);

                //View.chart1.Series["S"].Points[0].Color = System.Drawing.Color.Green; #21E10D
                //View.chart1.Series["S"].Points[1].Color = System.Drawing.Color.Red; #E60E0E
                //View.chart1.Series["S"].Points[2].Color = System.Drawing.Color.Orange; #FA9D0A
                //View.chart1.Series["S"].Points[3].Color = System.Drawing.Color.Purple; #7B5FBB
                //View.chart1.Series["S"].Points[4].Color = System.Drawing.Color.Indigo; #4CD8D8
                //View.chart1.Series["S"].Points[5].Color = System.Drawing.Color.Azure; #0099FF

                //View.chart1.Series.Add("Serie1");
                //View.chart1.ChartAreas.Add("A");

                //View.chart1.Series.Add("Serie2");
                //View.chart1.ChartAreas.Add("B");

                //View.chart1.Series["Serie1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;

                //View.chart1.DataSource = service.DirectSQLQuery("select 'REPA' AS detalle, 45 as valor union all select 'DIAG' AS detalle, 85 as valor", "", "dbo.movimientoclaro", Local);
                //View.chart1.Series["Serie1"].LegendText = "Valor de compras";
                //View.chart1.Series["Serie1"].XValueMember = "detalle";
                //View.chart1.Series["Serie1"].YValueMembers = "valor";
            }
            //}
            //catch (Exception ex)
            //{
            //  Util.ShowMessage("Error al momento de reiniciar escenario de pruebas " + ex.Message);
            //}
        }

        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            View.Ciudad.Text = View.Model.ListadoOrigen.Where(f => f.Code == ((MMaster)View.Origen.SelectedItem).Code.ToString()).First().Code2.ToString();
            View.Centro.Text = View.Model.ListadoOrigen.Where(f => f.Code == ((MMaster)View.Origen.SelectedItem).Code.ToString()).First().Description.ToString();
        }

        private void OnBuscarNombreMaterial(object sender, EventArgs e)
        {
            View.CodigoSAP.Text = View.Model.ListadoProductos.Where(f => f.ProductCode == ((Product)View.NombreMaterial.SelectedItem).ProductCode.ToString()).First().Brand.ToString();
            View.Familia.Text = View.Model.ListadoProductos.Where(f => f.ProductCode == ((Product)View.NombreMaterial.SelectedItem).ProductCode.ToString()).First().Manufacturer.ToString();
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

        private void CargarDatosAdministrador(object sender, EventArgs e)
        {

            DataTable Consulta;
            Consulta = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARDATOSADMIN','" + View.GetSerial1.Text + "'", "", "dbo.EquiposClaro", Local);

            if (Consulta.Rows.Count == 0)
            {
                Consulta = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARDATOSADMINDESPACHO','" + View.GetSerial1.Text + "'", "", "dbo.Despacho_EquiposClaro", Local);
                if (Consulta.Rows.Count == 0)
                {
                    Util.ShowError("El serial no existe dentro del sistema.");
                    return;
                }
                else
                {
                    //View.EstadoSerial.Text = Consulta.Rows[0]["Estado"].ToString();

                    View.Mac.Text = Consulta.Rows[0]["Mac"].ToString();
                    View.NombreMaterial.Text = Consulta.Rows[0]["NombreMaterial"].ToString();
                    View.TipoOrigen.Text = Consulta.Rows[0]["TipoOrigen"].ToString();
                    View.Ciudad.Text = Consulta.Rows[0]["Ciudad"].ToString();
                    View.Origen.Text = Consulta.Rows[0]["Origen"].ToString();
                    View.CodigoSAP.Text = Consulta.Rows[0]["CodigoSAP"].ToString();
                    View.EstadoRR.Text = Consulta.Rows[0]["EstadoRR"].ToString();
                    View.TipoRecibo.Text = Consulta.Rows[0]["TipoRecibo"].ToString();
                    View.FechaIngreso.Text = Consulta.Rows[0]["FechaIng"].ToString();
                    View.DocIngreso.Text = Consulta.Rows[0]["DocIng"].ToString();
                    View.FechaDoc.Text = Consulta.Rows[0]["FechaDoc"].ToString();
                    View.Centro.Text = Consulta.Rows[0]["Centro"].ToString();
                    View.Familia.Text = Consulta.Rows[0]["Familia"].ToString();
                    View.Consecutivo.Text = Consulta.Rows[0]["Consecutivo"].ToString();
                    View.EstadoSerial.Text = Consulta.Rows[0]["EstadoSerial"].ToString();
                    View.IdPallet.Text = Consulta.Rows[0]["IdPallet"].ToString();
                    View.Ubicacion.Text = Consulta.Rows[0]["Ubicacion"].ToString();
                    View.CodigoEmpaque.Text = Consulta.Rows[0]["CodEmpaque"].ToString();
                    View.CodigoEmpaque2.Text = Consulta.Rows[0]["CodEmpaque2"].ToString();
                    View.Posicion.Text = Consulta.Rows[0]["Posicion"].ToString();
                    View.FallaDiagnostico.Text = Consulta.Rows[0]["FallaDiag"].ToString();
                    View.TecnicoAsigDiag.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                    View.EstatusDiagnostico.Text = Consulta.Rows[0]["EstatusDiag"].ToString();
                    View.TecnicoAsignadoRep.Text = Consulta.Rows[0]["TecnicoRep"].ToString();
                    View.EstadoReparacion.Text = Consulta.Rows[0]["EstatusRep"].ToString();
                    View.FallaReparacion.Text = Consulta.Rows[0]["FallaRep"].ToString();
                    View.FallaReparacion1.Text = Consulta.Rows[0]["FallaRep1"].ToString();
                    View.FallaReparacion2.Text = Consulta.Rows[0]["FallaRep2"].ToString();
                    View.FallaReparacion3.Text = Consulta.Rows[0]["FallaRep3"].ToString();
                    View.FallaReparacion4.Text = Consulta.Rows[0]["FallaRep4"].ToString();
                    View.MotivoScrap.Text = Consulta.Rows[0]["MotScrap"].ToString();
                    View.FallaVerificacion.Text = Consulta.Rows[0]["FallaVerif"].ToString();
                    View.StatusVerificacion.Text = Consulta.Rows[0]["EstatusVerif"].ToString();
                    View.FechaDespacho.Text = Consulta.Rows[0]["FechaDespacho"].ToString();

                    View.BotonGuardar.IsEnabled = false;
                }
            }
            else
            {
                //View.EstadoSerial.Text = Consulta.Rows[0]["Estado"].ToString();

                View.Mac.Text = Consulta.Rows[0]["Mac"].ToString();
                View.NombreMaterial.Text = Consulta.Rows[0]["NombreMaterial"].ToString();
                View.TipoOrigen.Text = Consulta.Rows[0]["TipoOrigen"].ToString();
                View.Ciudad.Text = Consulta.Rows[0]["Ciudad"].ToString();
                View.Origen.Text = Consulta.Rows[0]["Origen"].ToString();
                View.CodigoSAP.Text = Consulta.Rows[0]["CodigoSAP"].ToString();
                View.EstadoRR.Text = Consulta.Rows[0]["EstadoRR"].ToString();
                View.TipoRecibo.Text = Consulta.Rows[0]["TipoRecibo"].ToString();
                View.FechaIngreso.Text = Consulta.Rows[0]["FechaIng"].ToString();
                View.DocIngreso.Text = Consulta.Rows[0]["DocIng"].ToString();
                View.FechaDoc.Text = Consulta.Rows[0]["FechaDoc"].ToString();
                View.Centro.Text = Consulta.Rows[0]["Centro"].ToString();
                View.Familia.Text = Consulta.Rows[0]["Familia"].ToString();
                View.Consecutivo.Text = Consulta.Rows[0]["Consecutivo"].ToString();
                View.EstadoSerial.Text = Consulta.Rows[0]["EstadoSerial"].ToString();
                View.IdPallet.Text = Consulta.Rows[0]["IdPallet"].ToString();
                View.Ubicacion.Text = Consulta.Rows[0]["Ubicacion"].ToString();
                View.CodigoEmpaque.Text = Consulta.Rows[0]["CodEmpaque"].ToString();
                View.CodigoEmpaque2.Text = Consulta.Rows[0]["CodEmpaque2"].ToString();
                View.Posicion.Text = Consulta.Rows[0]["Posicion"].ToString();
                View.FallaDiagnostico.Text = Consulta.Rows[0]["FallaDiag"].ToString();
                View.TecnicoAsigDiag.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                View.EstatusDiagnostico.Text = Consulta.Rows[0]["EstatusDiag"].ToString();
                View.TecnicoAsignadoRep.Text = Consulta.Rows[0]["TecnicoRep"].ToString();
                View.EstadoReparacion.Text = Consulta.Rows[0]["EstatusRep"].ToString();
                View.FallaReparacion.Text = Consulta.Rows[0]["FallaRep"].ToString();
                View.FallaReparacion1.Text = Consulta.Rows[0]["FallaRep1"].ToString();
                View.FallaReparacion2.Text = Consulta.Rows[0]["FallaRep2"].ToString();
                View.FallaReparacion3.Text = Consulta.Rows[0]["FallaRep3"].ToString();
                View.FallaReparacion4.Text = Consulta.Rows[0]["FallaRep4"].ToString();
                View.MotivoScrap.Text = Consulta.Rows[0]["MotScrap"].ToString();
                View.FallaVerificacion.Text = Consulta.Rows[0]["FallaVerif"].ToString();
                View.StatusVerificacion.Text = Consulta.Rows[0]["EstatusVerif"].ToString();
                View.FechaDespacho.Text = Consulta.Rows[0]["FechaDespacho"].ToString();

                View.BotonGuardar.IsEnabled = true;
            }

        }

        private void OnConfirmarRecibo(object sender, EventArgs e)
        {
            View.Ubicacion.Text = View.Model.ListadoEstatusLogPro.Where(f => f.Code == ((MMaster)View.EstadoSerial.SelectedItem).Code.ToString()).First().Description.ToString();
        }

        private void OnAddLine(object sender, EventArgs e)
        {
            if (((MMaster)View.FallaVerificacion.SelectedItem).Code.ToString() == "SIN FALLA")
                View.StatusVerificacion.Text = "VERIFICADO";
            else
                View.StatusVerificacion.Text = "DAÑADO";
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            if (((MMaster)View.FallaDiagnostico.SelectedItem).Code.ToString() == "SIN FALLA")
                View.EstatusDiagnostico.Text = "BUEN ESTADO";
            else
                View.EstatusDiagnostico.Text = "MAL ESTADO";
        }

        public void OnReiniciarCapacitacion(object sender, EventArgs e)
        {
            //Ejecuto la consulta
            try
            {
                String ConsultaSQL = "exec sp_trainning_return;";
                service.DirectSQLNonQuery(ConsultaSQL, Local);

                Util.ShowMessage("Escenario de pruebas reiniciado");
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Error al momento de reiniciar escenario de pruebas " + ex.Message);
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
            //Recorre la primera linea y con esa setea el valor de las demas lineas.
            for (int i = 1; i < View.Model.ListRecords.Rows.Count; i++)
                for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                    View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[0][z];
        }

        private void OnSaveDetails(object sender, EventArgs e)
        {

            //Variables Auxiliares
            String ConsultaGuardar = "";

            try
            {
                String EstadoActual = View.EstadoSerial.Text.ToString();

                string FechaIngreso = "";
                DateTime aux_fechaIngreso;
                string dateFormat = "yyyy-MM-dd";
                FechaIngreso = View.FechaIngreso.Text;
                aux_fechaIngreso = DateTime.Parse(View.FechaIngreso.Text);
                FechaIngreso = aux_fechaIngreso.ToString(dateFormat);

                string FechaDoc = "";
                DateTime aux_fechaDoc;
                string dateFormatDoc = "yyyy-MM-dd";
                aux_fechaDoc = DateTime.Parse(View.FechaDoc.Text);
                FechaDoc = aux_fechaDoc.ToString(dateFormatDoc);

                ConsultaGuardar += "";

                //Construyo la consulta para guardar los datos
                ConsultaGuardar += " UPDATE dbo.EquiposClaro SET Mac = '" + View.Mac.Text.ToString() + "', ProductoID = '" + View.NombreMaterial.Text.ToString() + "'";

                ConsultaGuardar += ", TIPO_ORIGEN = '" + View.TipoOrigen.Text.ToString() + "', CIUDAD = '" + View.Ciudad.Text.ToString() + "'";

                ConsultaGuardar += ", ORIGEN = '" + View.Origen.Text.ToString() + "', CODIGO_SAP = '" + View.CodigoSAP.Text.ToString() + "'";

                ConsultaGuardar += ", ESTADO_RR = '" + View.EstadoRR.Text.ToString() + "', TIPO_REC = '" + View.TipoRecibo.Text.ToString() + "'";

                ConsultaGuardar += ", FECHA_INGRESO = '" + FechaIngreso + "', DOC_INGRESO = '" + View.DocIngreso.Text.ToString() + "'";

                ConsultaGuardar += ", FECHA_DOC = '" + FechaDoc + "', CENTRO = '" + View.Centro.Text.ToString() + "'";

                ConsultaGuardar += ", FAMILIA = '" + View.Familia.Text.ToString() + "', CONSECUTIVO = '" + View.Consecutivo.Text.ToString() + "'";

                ConsultaGuardar += ", Estado = '" + View.EstadoSerial.Text.ToString() + "', IdPallet = '" + View.IdPallet.Text.ToString() + "'";

                ConsultaGuardar += ", Ubicacion = '" + View.Ubicacion.Text.ToString() + "'";

                if (View.CodigoEmpaque.Text.ToString() != "" && View.CodigoEmpaque.Text.ToString() != null)
                    ConsultaGuardar += ", CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() + "'";

                if (View.CodigoEmpaque2.Text.ToString() != "" && View.CodigoEmpaque2.Text.ToString() != null)
                    ConsultaGuardar += ", CodigoEmpaque2 = '" + View.CodigoEmpaque2.Text.ToString() + "'";

                if (View.Posicion.Text.ToString() != "" && View.Posicion.Text.ToString() != null)
                    ConsultaGuardar += ", Posicion = '" + View.Posicion.Text.ToString() + "'";

                if (View.TecnicoAsigDiag.Text.ToString() != "" && View.TecnicoAsigDiag.Text.ToString() != null)
                    ConsultaGuardar += ", TECNICO_ASIGNADO_DIAG = '" + View.TecnicoAsigDiag.Text.ToString() + "'";

                if (View.FallaDiagnostico.Text.ToString() != "" && View.FallaDiagnostico.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_DIAGNOSTICO = '" + View.FallaDiagnostico.Text.ToString() + "'";

                if (View.TecnicoAsignadoRep.Text.ToString() != "" && View.TecnicoAsignadoRep.Text.ToString() != null)
                    ConsultaGuardar += ", Tecnico_Reparacion = '" + View.TecnicoAsignadoRep.Text.ToString() + "'";

                if (View.EstatusDiagnostico.Text.ToString() != "" && View.EstatusDiagnostico.Text.ToString() != null)
                    ConsultaGuardar += ", ESTATUS_DIAGNOSTICO = '" + View.EstatusDiagnostico.Text.ToString() + "'";

                if (View.FallaReparacion.Text.ToString() != "" && View.FallaReparacion.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_REP = '" + View.FallaReparacion.Text.ToString() + "'";

                if (View.EstadoReparacion.Text.ToString() != "" && View.EstadoReparacion.Text.ToString() != null)
                    ConsultaGuardar += ", ESTATUS_REPARACION = '" + View.EstadoReparacion.Text.ToString() + "'";

                if (View.FallaReparacion2.Text.ToString() != "" && View.FallaReparacion2.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_REP2 = '" + View.FallaReparacion2.Text.ToString() + "'";

                if (View.FallaReparacion1.Text.ToString() != "" && View.FallaReparacion1.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_REP1 = '" + View.FallaReparacion1.Text.ToString() + "'";

                if (View.FallaReparacion4.Text.ToString() != "" && View.FallaReparacion4.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_REP4 = '" + View.FallaReparacion4.Text.ToString() + "'";

                if (View.FallaReparacion3.Text.ToString() != "" && View.FallaReparacion3.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_REP3 = '" + View.FallaReparacion3.Text.ToString() + "'";

                if (View.FallaVerificacion.Text.ToString() != "" && View.FallaVerificacion.Text.ToString() != null)
                    ConsultaGuardar += ", FALLA_VERIFICACION = '" + View.FallaVerificacion.Text.ToString() + "'";

                if (View.MotivoScrap.Text.ToString() != "" && View.MotivoScrap.Text.ToString() != null)
                    ConsultaGuardar += ", MOTIVO_SCRAP = '" + View.MotivoScrap.Text.ToString() + "'";

                if (View.StatusVerificacion.Text.ToString() != "" && View.StatusVerificacion.Text.ToString() != null)
                    ConsultaGuardar += ", ESTATUS_VERIFICACION = '" + View.StatusVerificacion.Text.ToString() + "'";

                if (View.FechaDespacho.Text.ToString() != "" && View.FechaDespacho.Text.ToString() != null)
                    ConsultaGuardar += ", FECHA_DESPACHO = '" + View.FechaDespacho.Text.ToString() + "'";


                ConsultaGuardar += " WHERE Serial = '" + View.GetSerial1.Text.ToString() + "';";

                String ConsultaBuscar = "SELECT * FROM dbo.EquiposCLARO WHERE UPPER(Serial) = UPPER('" + View.GetSerial1.Text.ToString() + "')";
                DataTable Resultado = service.DirectSQLQuery(ConsultaBuscar, "", "dbo.EquiposCLARO", Local);

                //insertar movimiento
                ConsultaGuardar += "EXEC sp_InsertarNuevo_Movimiento 'MODIFICACION DEL EQUIPO EN ADMIN. SERIALES','" + Resultado.Rows[0]["Estado"].ToString() + "','" + View.EstadoSerial.Text.ToString() + "',''," + Resultado.Rows[0]["RowId"] + ",'" + View.EstadoSerial.Text.ToString() + "','CAMBIOADMINSERIALES','" + this.user + "','';";
                
                // Declaramos variables Globales
                string nombreMovimiento = "MODIFICACION DEL EQUIPO EN ADMIN. SERIALES PRODUCCION";
                string origen = View.Origen.Text.ToString();
                string destino = View.EstadoSerial.Text.ToString();
                string movPallet = View.IdPallet.Text.ToString();
                string idSerial = View.GetSerial1.Text.ToString();

                #region Insertar Movimiento Reparacion 
                // Declaramos las variables para realizar la insersión de REPARACION
                string repaTecnico = View.TecnicoAsignadoRep.Text.ToString();
                string fallaPrincipal = View.FallaReparacion.Text.ToString();
                string fallaRep1 = View.FallaReparacion1.Text.ToString();
                string fallaRep2 = View.FallaReparacion2.Text.ToString();
                string fallaRep3 = View.FallaReparacion3.Text.ToString();
                string fallaRep4 = View.FallaReparacion4.Text.ToString();
                string estadoFinalReparacion = View.EstadoReparacion.Text.ToString();
                string repaScrap = View.MotivoScrap.Text.ToString();
                string usuario = this.user.ToString();
                // Insertar movimiento en la tabla de reparación
                ConsultaGuardar += " EXEC [dbo].[sp_InsertarNuevo_MovimientoReparacion] @nombre_movimiento = '" + nombreMovimiento + "', " +
                "@origen = '" + origen + "', " +
                "@destino = '" + destino + "', " +
                "@mov_pallet = '" + movPallet + "', " +
                "@id_serial = '" + idSerial + "', " +
                "@repa_tecnico = '" + repaTecnico + "', " +
                "@repa_fallaPrincipal = '" + fallaPrincipal + "', " +
                "@repa_falla1 = '" + fallaRep1 + "', " +
                "@repa_falla2 = '" + fallaRep2 + "', " +
                "@repa_falla3 = '" + fallaRep3 + "', " +
                "@repa_falla4 = '" + fallaRep4 + "', " +
                "@repa_estadoFinal = '" + estadoFinalReparacion + "', " +
                "@repa_scrap = '" + repaScrap + "', " +
                "@usuario = '" + usuario + "'; ";
                #endregion

                #region Insertar Movimiento Diagnostico

                string diagFalla = View.FallaDiagnostico.Text.ToString();
                string diagStatus = View.EstatusDiagnostico.Text.ToString();
                string diagDiagnosticador = View.TecnicoAsigDiag.Text.ToString();
                string diagUsuario = this.user.ToString();

                ConsultaGuardar += " EXEC [dbo].[sp_InsertarNuevo_MovimientoDiagnostico] @nombre_movimiento = '" + nombreMovimiento + "', " +
                "@origen = '" + origen + "', " +
                "@destino = '" + destino + "', " +
                "@mov_pallet = '" + movPallet + "', " +
                "@id_serial = '" + idSerial + "', " +
                "@diag_falla = '" + diagFalla + "', " +
                "@diag_status = '" + diagStatus + "', " +
                "@diag_diagnosticador = '" + diagDiagnosticador + "', " +
                "@usuario = '" + diagUsuario + "';";
                #endregion

                #region Insertar Movimiento Verificacion

                string verif_falla = View.FallaVerificacion.Text.ToString();
                string verif_status = View.StatusVerificacion.Text.ToString();
                string verif_diagnosticador = "N/A";
                string verif_usuario = this.user.ToString();

                ConsultaGuardar += " EXEC [dbo].[sp_InsertarNuevo_MovimientoVerificacion] @nombre_movimiento = '" + nombreMovimiento + "', " +
                "@origen = '" + origen + "', " +
                "@destino = '" + destino + "', " +
                "@mov_pallet = '" + movPallet + "', " +
                "@id_serial = '" + idSerial + "', " +
                "@verif_falla = '" + verif_falla + "', " +
                "@verif_status = '" + verif_status + "', " +
                "@verif_diagnosticador = '" + verif_diagnosticador + "', " +
                "@usuario = '" + diagUsuario + "';";

                #endregion

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

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            //View.Model.ListRecords.Rows.Clear();
        }

    }
}