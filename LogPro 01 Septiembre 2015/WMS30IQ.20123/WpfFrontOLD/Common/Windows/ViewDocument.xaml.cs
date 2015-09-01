using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using Microsoft.Reporting.WinForms;
using WpfFront.WMSBusinessService;
using WpfFront.Services;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace WpfFront.Common
{
    /// <summary>
    /// Interaction logic for FormatsView.xaml
    /// </summary>
    public partial class ViewDocument : Window
    {

        private readonly ReportViewer pivotView = new ReportViewer();
        private WMSServiceClient service = new WMSServiceClient();
        //private LabelTemplate report;        
        private string printerName;
        public IList<WpfFront.WMSBusinessService.Label> labelList;
        private int documentID;


        public ViewDocument(LabelTemplate report, int docID, string printer, bool showBtnPrint, IList<WpfFront.WMSBusinessService.Label> list)
        {
            if (report == null)
            {
                Util.ShowError("Report could not be found.");
                return;
            }

            InitializeComponent();
            printerName = printer;
            documentID = docID;
            labelList = list;



            #region Windows Form Host

            //if (showBtnPrint)
            //    btnPrintBatch.Visibility = Visibility.Visible;
            //else
            //    btnPrintBatch.Visibility = Visibility.Collapsed;

            //Create a Windows Forms Host to host a form
            WindowsFormsHost host = new WindowsFormsHost();

            //Report ddimensions
            host.HorizontalAlignment = HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;

            //pivotView.Width = 900;
            //pivotView.Height = 700;

            pivotView.Margin = new System.Windows.Forms.Padding { All = 5 };

            //Add the component to the host
            host.Child = pivotView;
            gridP.Children.Add(host);

            #endregion





            try
            {
                //Report File exists
                string reportPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    WmsSetupValues.RdlTemplateDir + "\\" + report.Header);

                if (!File.Exists(reportPath))
                {
                    Util.ShowError("Report file does not exists.");
                    return;
                }

                //Rendering Report
                this.pivotView.ProcessingMode = ProcessingMode.Local;
                this.pivotView.LocalReport.ReportPath = reportPath;
                this.pivotView.LocalReport.EnableExternalImages = true;
                this.pivotView.LocalReport.ExecuteReportInCurrentAppDomain(System.Reflection.Assembly.GetExecutingAssembly().Evidence);
                this.pivotView.LocalReport.AddTrustedCodeModuleInCurrentAppDomain("Barcode, Version=1.0.5.40001, Culture=neutral, PublicKeyToken=6dc438ab78a525b3");
                this.pivotView.LocalReport.AddTrustedCodeModuleInCurrentAppDomain("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                DataSet ds;
                //Document
                if (documentID > 0)
                {
                    ds = ReportMngr.ProcessDocument(documentID, service, report.Header);
                    if (ds == null)
                        return;

                    pivotView.LocalReport.DataSources.Add(new ReportDataSource("Header", ds.Tables["ReportHeaderFormat"]));
                    pivotView.LocalReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["ReportDetailFormat"]));
                }

                //Labels
                else if (report.LabelType.DocClass.DocClassID == SDocClass.Label)
                {
                    ds = ReportMngr.ProcessLabels(labelList);
                    pivotView.LocalReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["Details"]));
                }

                /*
                // CAA [2010/06/22]
                // Nueva opción para enviar parámetros al reporte
                if (parameters != null)
                {
                    ReportParameter rp;
                    ReportParameter[] rpList= new ReportParameter[parameters.Count()];
                    int cont = 1;
                    foreach (string parameter in parameters)
                    {
                        rp = new ReportParameter("p"+cont.ToString(), parameter);
                        rpList[cont-1] = rp;
                        cont++;
                    }
                    pivotView.LocalReport.SetParameters(rpList);
                }
                 */

                //Showing
                pivotView.Show();
                pivotView.LocalReport.Refresh();
                pivotView.RefreshReport();

            }
            catch (Exception ex)
            {
                Util.ShowError("Report could not shown: " + ex.Message);
            }


        }

        public ViewDocument(DataSet ds, String ReportName)
        {
            string flag = "";

            if (ReportName == null)
            {
                Util.ShowError("Report could not be found.");
                return;
            }

            if (ds == null)
                return;

            flag = "Initialize";

            InitializeComponent();
            printerName = "";



            #region Windows Form Host

            //if (showBtnPrint)
            //    btnPrintBatch.Visibility = Visibility.Visible;
            //else
            //    btnPrintBatch.Visibility = Visibility.Collapsed;

            //Create a Windows Forms Host to host a form
            WindowsFormsHost host = new WindowsFormsHost();

            //Report ddimensions
            host.HorizontalAlignment = HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;

            //pivotView.Width = 900;
            //pivotView.Height = 700;

            pivotView.Margin = new System.Windows.Forms.Padding { All = 5 };

            //Add the component to the host
            host.Child = pivotView;
            gridP.Children.Add(host);

            #endregion





            try
            {
                //Report File exists
                string reportPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    WmsSetupValues.RdlTemplateDir + "\\" + ReportName);

                if (!File.Exists(reportPath))
                {
                    Util.ShowError("Archivo del reporte no existe.");
                    return;
                }

                //Rendering Report
                flag = "Rendering";
                this.pivotView.ProcessingMode = ProcessingMode.Local;
                this.pivotView.LocalReport.ReportPath = reportPath;

                try { this.pivotView.LocalReport.SetBasePermissionsForSandboxAppDomain(new PermissionSet(PermissionState.Unrestricted)); }
                catch { }

                this.pivotView.LocalReport.EnableExternalImages = true;
                try { this.pivotView.LocalReport.ExecuteReportInCurrentAppDomain(System.Reflection.Assembly.GetExecutingAssembly().Evidence); }
                catch { }
                try { this.pivotView.LocalReport.AddTrustedCodeModuleInCurrentAppDomain("Barcode, Version=1.0.5.40001, Culture=neutral, PublicKeyToken=6dc438ab78a525b3"); }
                catch { }
                try { this.pivotView.LocalReport.AddTrustedCodeModuleInCurrentAppDomain("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"); }
                catch { }

                flag = "Process";     
                pivotView.LocalReport.DataSources.Add(new ReportDataSource("Header", ds.Tables[0]));
                pivotView.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables[1]));
                //try { pivotView.LocalReport.DataSources.Add(new ReportDataSource("Details2", ds.Tables[2])); }
                //catch { }

                //Showing
                flag = "Showing";
                pivotView.Show();
                pivotView.LocalReport.Refresh();
                pivotView.RefreshReport();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error mostrando el reporte: " + flag + ", " + ex);
            }
        }


        ////Imprime el reporte en batch
        //private void btnPrintBatch_Click(object sender, RoutedEventArgs e)
        //{
        //    //Llama al Dao de reportes, y segun el Tipo Obtiene un DataSet Con Los datos Requeridos
        //    ReportHeaderFormat rptHdr = service.SerClient.GetReportInformation(new Document { DocID = documentID, Company = App.curCompany, Location = App.curLocation });
        //    DataSet ds = ReportMngr.GetReportDataset(rptHdr);
        //    ReportMngr.PrintReportInBatch(report, ds, printerName);
        //}


    }
}

