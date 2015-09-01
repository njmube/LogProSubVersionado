using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Reflection;
using System.Data;
using WpfFront.WMSBusinessService;
using System.Xml.Serialization;
using WpfFront.Services;
using System.Threading;



namespace WpfFront.Common
{
    class ReportMngr
    {

        //ReportDocument report;
        private static Printer usePrinter;
        //private static int m_currentPageIndex;
        //private static IList<Stream> m_streams;
        private static Dictionary<LabelTemplate, IList<Stream>> m_streams;
        private static Dictionary<LabelTemplate, int> m_currentPageIndex;
        private static LabelTemplate curTemplate;
        private static LocalReport localReport;
        private static IList<Label> listLabels;



        public static void PrintLabelsInBatch(LabelTemplate defTemplate, Printer printer, IList<Label> listOfLabels) 
        {
            //if (string.IsNullOrEmpty(printer))
            //{
            //    Util.ShowError("Printer not found");
            //    return;
            //}

            Thread tl;

            if (printer != null && printer.PrinterName != WmsSetupValues.DEFAULT)
                usePrinter = printer;


            //printerName = printer;
            //m_streams = new List<Stream>();
            m_streams = new Dictionary<LabelTemplate, IList<Stream>>();
            m_currentPageIndex = new Dictionary<LabelTemplate, int>();
            

            //1. Si viene un template imprime los labes con ese template
            if (defTemplate != null)
            {
                try
                {
                    PrintLabelByTemplate(defTemplate, listOfLabels.Where(f => f.Product.PrintLabel != false).ToList());

                    //tl = new Thread(new ThreadStart(UpdateLabelPrintStatus));
                    //tl.Start();
                    UpdateLabelPrintStatus();

                }
                catch (Exception ex) { Util.ShowError(ex.Message); }

                return;
            }


            //2. Agrupa a los diferentes tipos de label y a los null y los manda por aparte.
            //Filtra los que no imprime label (double check)
            IList<LabelTemplate> templateList = new List<LabelTemplate>();
            //Si el lable es de producto busca las templates del producto.
            if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel) 
                templateList = listOfLabels.Where(f => f.Product.DefaultTemplate != null)
                    .Select(f => f.Product.DefaultTemplate).Distinct().ToList();

            string error = "";

            //Configurando el template por defecto para impresion
            LabelTemplate defLabelTemplate = App.defTemplate;


            //Para cada template mandando la impresion
            foreach (LabelTemplate template in templateList)
            {
                try
                {
                    PrintLabelByTemplate(template, listOfLabels.Where(f => f.Product.DefaultTemplate == template && f.Product.PrintLabel != false)
                        .ToList());

                    tl = new Thread(new ThreadStart(UpdateLabelPrintStatus));
                    tl.Start();

                }
                catch (Exception ex) {
                    error += ex.Message;
                }
            }

            //Mandando las labels con template ne Null
            try
            {
                List<Label> labelsWoTemplate = null;
                if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel)
                    labelsWoTemplate = listOfLabels.Where(f => f.Product.DefaultTemplate == null && f.Product.PrintLabel != false).ToList();
                else
                    labelsWoTemplate = listOfLabels.ToList();

                PrintLabelByTemplate(defLabelTemplate, labelsWoTemplate);

                tl = new Thread(new ThreadStart(UpdateLabelPrintStatus));
                tl.Start();

            }
            catch (Exception ex) { error += ex.Message; }


            //Final Error
            if (!string.IsNullOrEmpty(error))
                Util.ShowError(error);

                      
        }




        private static void PrintLabelByTemplate(LabelTemplate template, List<Label> listOfLabels)
        {

            if (listOfLabels == null || listOfLabels.Count == 0)
                return;

            listLabels = listOfLabels;

            try
            {
                //Si viene una impresora definida utiliza esa, si no utiliza la del template
                if (template != null)
                    curTemplate = template;
                else
                    curTemplate = (new WMSProcessClient()).GetLabelTemplate(new LabelTemplate { Header = WmsSetupValues.ProductLabelTemplate }).First();


                usePrinter = usePrinter == null
                    ? new Printer { PrinterName = curTemplate.DefPrinter.Name, PrinterPath = curTemplate.DefPrinter.CnnString }
                    : usePrinter;
            }
            catch { throw new Exception("Printer not defined for template " + curTemplate.Name); }

            if (usePrinter == null)
                throw new Exception("Printer not defined for template " + curTemplate.Name);

            //Revisa si el label imprime por comandos y lo manda a esa ruta.
            if (template.IsPL == true)
            {
                PrintLabelByPL(template, listOfLabels, usePrinter);
                return;
            }


            try
            {
                string labelPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                    WmsSetupValues.RdlTemplateDir + "\\" + template.Header);

                if (!File.Exists(labelPath))
                    throw new Exception("Label template " + template.Header + " does not exists.\n");
                

                //Definicion de Reporte
                localReport = new LocalReport();
                localReport.EnableExternalImages = true;
                localReport.ExecuteReportInCurrentAppDomain(System.Reflection.Assembly.GetExecutingAssembly().Evidence);
                localReport.AddTrustedCodeModuleInCurrentAppDomain("Barcode, Version=1.0.5.40001, Culture=neutral, PublicKeyToken=6dc438ab78a525b3");
                localReport.AddTrustedCodeModuleInCurrentAppDomain("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                localReport.EnableExternalImages = true;


                localReport.ReportPath = labelPath;

                DataSet ds = ProcessLabels(listOfLabels);
                localReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["Details"]));


                //Proceso de Creacion de archivos 
                curTemplate = template;
                m_streams.Add(curTemplate, new List<Stream>());
                m_currentPageIndex.Add(curTemplate, 0);

                Export(localReport, curTemplate, "IMAGE");  //1 - Document, 2 -  Label


                m_currentPageIndex[curTemplate] = 0;
                Thread th = new Thread(new ParameterizedThreadStart(Print));
                th.Start(usePrinter.PrinterName);
                //Print(usePrinter.PrinterName);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        //Imprime en impresora mandando la lineas de comando PCL
        private static void PrintLabelByPL(LabelTemplate template, IList<Label> listLabels, Printer printer)
        {
            string templateReplaced = (new WMSServiceClient())
                .GetReplacedTemplateForLabels(listLabels, template, "");

            if (string.IsNullOrEmpty(templateReplaced))
            {
                Util.ShowError("Error creating Printer Template.");
                return;
            }

            PrinterControl.PrintLabels(templateReplaced, printer.PrinterPath);
        }



        private static void UpdateLabelPrintStatus()
        {
            try
            {
                WMSServiceClient service = new WMSServiceClient();

                foreach (Label label in listLabels)
                {
                    if (label.Printed == true)
                        continue;

                    label.Printed = true;
                    service.UpdateLabel(label);
                }
            }
            catch { }
        }



        //Process Labels
        public static DataSet ProcessLabels(IList<Label> labelList)
        {
            if (labelList == null || labelList.Count == 0)
                throw new Exception("No labels found.");

            //Pricessing Labels
            return ReportMngr.GetReportDataset(labelList, 10);
        }


        //Process Document
        public static DataSet ProcessDocument(int documentID, WMSServiceClient service, string template)
        {
            //Llama al Dao de reportes, y segun el Tipo Obtiene un DataSet Con Los datos Requeridos
            ReportHeaderFormat rptHdr = service.GetReportInformation(new Document { DocID = documentID, Company = App.curCompany, Location = App.curLocation }, 
                template);
            return ReportMngr.GetReportDataset(rptHdr);
        }


        //Entrega el dataset de un ReportHeader
        public static DataSet GetReportDataset(ReportHeaderFormat header)
        {
            if (header == null)
                return null;

            //Add Header to DataSet
            DataSet dh = new DataSet("Header");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ReportHeaderFormat));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, header);
            StringReader reader = new StringReader(writer.ToString());
            dh.ReadXml(reader);
            //dh.Tables[0].TableName = "Header";


            ////Add Detail List to DataSet
            //DataSet dd = new DataSet("Details");
            //xmlSerializer = new XmlSerializer(header.ReportDetails.ToArray().GetType());  //  detailList.GetType()
            //writer = new StringWriter();
            //xmlSerializer.Serialize(writer, header.ReportDetails.ToArray());
            //reader = new StringReader(writer.ToString());
            //dd.ReadXml(reader);
            //dd.Tables[0].TableName = "Details";

            //dd.Tables.Add(dh.Tables[0].Copy()); //Adicionamos el header a los details
            return dh;
        }


        //Obtiene el detalle de label y entrega el dataset
        public static DataSet GetReportDataset(IList<Label> labelList, int quantity)
        {
            try
            {

                ReportDetailFormat detail;
                IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();
                //string barcodeURL = App.curCompany.WebURL + "/"+ WmsSetupValues.BarcodeDir + "/";
                //int i = 1;
                string notes = "";

                int seq = 1;
                foreach (Label label in labelList)
                {
                    detail = new ReportDetailFormat();

                    if (label.Product != null)
                    {
                        detail.ProductCode = label.Product.ProductCode;
                        detail.ProductDescription = label.Product.Name;
                        detail.Weight = label.Product.Weight * label.StartQty;
                    }

                    notes = (label.Notes != null) ? label.Notes : "";

                    if (label.Printed == true)
                    {
                        try { detail.Unit = notes.Split(',')[0].Trim(); }
                        catch { }
                        detail.QtyOrder = label.StartQty; //Aqui guardan las logisitcas las hijas que contienen unas vez se imprimen
                    }
                    else
                    {
                        detail.Unit = label.Unit.Name;
                        detail.QtyOrder = label.CurrQty;
                    }


                    //detail.BarcodeProduct = label.Product.ProductCode;
                    detail.BarcodeLabel = label.Barcode;
                    detail.Lote = (label.LotCode != null) ? label.LotCode : "";
                    detail.Date1 = (label.ExpirationDate != null) ? label.ExpirationDate.Value.ToShortDateString() : "";
                    detail.Printed = DateTime.Today.ToString("MM/dd/yyyy hh:mm:ss");
                    detail.Serial = (label.SerialNumber != null) ? label.SerialNumber : "";
                    detail.Notes = notes;
                    detail.PrintLot = (label.PrintingLot != null) ? label.PrintingLot : "";
                    detail.UserName = label.CreatedBy;
                    detail.LogisticNote = ""; // (label.IsLogistic == true) ? "** LOGISTIC **" : "";



                    //Si el Label es de tipo Custom Label, Ejecuta un proceso adicional.
                    //JM 10 de Junio de 2009
                    if (label.LabelType.DocTypeID == LabelType.CustomerLabel)
                    {
                        //For Packages
                        detail.DocNumber = (label.ShippingDocument != null) ? label.ShippingDocument.DocNumber : "";

                        //Sequence
                        try { detail.PrintLot = seq.ToString() + " / " + labelList.Count(); }
                        catch { }

                        //Weight
                        try { detail.Weight = label.DocumentPackages[0].Weight; }
                        catch { }

                        //Dimemsion
                        try { detail.Dimension = label.DocumentPackages[0].Dimension; }
                        catch { }


                        //Pieces
                        try { detail.Pieces = label.DocumentPackages[0].Pieces; }
                        catch { }


                        //Shipping Address
                        try
                        {
                            DocumentAddress shipAddr = (new WMSServiceClient()).GetDocumentAddress(
                                new DocumentAddress { Document = label.ShippingDocument, AddressType = AddressType.Shipping })
                            .Where(f => f.DocumentLine == null).First();

                            detail.ContactPerson = shipAddr.Name;  //shipAddr.ContactPerson;

                            //detail.ShipAddress = shipAddr.Name + "\n";
                            detail.ShipAddress = shipAddr.AddressLine1 + " " + shipAddr.AddressLine2 + "\n";
                            detail.ShipAddress += shipAddr.City + ", " + shipAddr.State + " " + shipAddr.ZipCode + ", ";
                            detail.ShipAddress += shipAddr.Country + "\n";
                            detail.ShipAddress += shipAddr.ContactPerson;

                        }
                        catch { }
                    }



                    detailList.Add(detail);
                    seq++;

                    //if (i++ >= quantity)
                    //    break;
                }

                //Add Header to DataSet
                XmlSerializer xmlSerializer;
                StringWriter writer = new StringWriter();

                DataSet dd = new DataSet("Details");
                xmlSerializer = new XmlSerializer(detailList.ToArray().GetType());  //  detailList.GetType()
                xmlSerializer.Serialize(writer, detailList.ToArray());
                StringReader reader = new StringReader(writer.ToString());
                dd.ReadXml(reader);
                dd.Tables[0].TableName = "Details";
                return dd;

            }
            catch { return null; }

        }




        public static void PrintDocument(Document document, Printer printer)
        {

            if (document.DocType.Template == null)
                return;

            try
            {
                //Inicializando variables usadas en la impresion
                //if (m_streams == null)
                    m_streams = new Dictionary<LabelTemplate, IList<Stream>>();

                //if (m_currentPageIndex == null)
                    m_currentPageIndex = new Dictionary<LabelTemplate, int>();

                curTemplate = document.DocType.Template;
                usePrinter = printer;


                //Ejecutar la impresion global en un Hilo            
                Thread th = new Thread(new ParameterizedThreadStart(PrintDocumentThread));
                th.Start(document);


            }
            catch (Exception ex)
            {
                Util.ShowError("Report could not shown: " + ex.Message);
            }
        }


        private static void PrintDocumentThread(Object document)
        {

            //Report File exists
            string reportPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WmsSetupValues.RdlTemplateDir + "\\" + curTemplate.Header);

            if (!File.Exists(reportPath))
                return;

            //Rendering Report
            localReport = new LocalReport();
            localReport.EnableExternalImages = true;
            localReport.ExecuteReportInCurrentAppDomain(System.Reflection.Assembly.GetExecutingAssembly().Evidence);
            localReport.AddTrustedCodeModuleInCurrentAppDomain("Barcode, Version=1.0.5.40001, Culture=neutral, PublicKeyToken=6dc438ab78a525b3");
            localReport.AddTrustedCodeModuleInCurrentAppDomain("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            localReport.EnableExternalImages = true;
            localReport.ReportPath = reportPath;


            DataSet ds;
            //Document

            ds = ReportMngr.ProcessDocument(((Document)document).DocID, new WMSServiceClient(), curTemplate.Header);
            if (ds == null)
                return;

            localReport.DataSources.Add(new ReportDataSource("Header", ds.Tables["ReportHeaderFormat"]));
            localReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["ReportDetailFormat"]));



            //Print Report
            //Proceso de Creacion de archivos 
            m_streams.Add(curTemplate, new List<Stream>());

            m_currentPageIndex.Add(curTemplate, 0);

            Export(localReport, curTemplate, "IMAGE");  //1 - Document, 2 -  Label


            m_currentPageIndex[curTemplate] = 0;

            //Ejecutar la impresion global en un Hilo            
            //Thread th = new Thread(new ParameterizedThreadStart(Print));
            //th.Start(printer.PrinterName);

            Print(usePrinter.PrinterName);

        }



        #region Print Batch Methods

        private static Stream CreateStream(string name, string fileNameExtension, Encoding encoding, 
            string mimeType, bool willSeek)
        {   
            // @"c:\My Reports\" +            
            //Stream stream = new FileStream(App.curCompany.ResourcesDiskPath + WmsSetupValues.pdfDir + "\\" + Guid.NewGuid() + name + "." + fileNameExtension, FileMode.Create);

            Stream stream = new FileStream(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    //Environment.CurrentDirectory,
                    WmsSetupValues.PrintReportDir + "\\" + Guid.NewGuid() + name + "." + fileNameExtension), 
                FileMode.Create);

            m_streams[curTemplate].Add(stream); 
            return stream;
        }


        private static void Export(LocalReport report, LabelTemplate template, string renderFormat)
        {
            //docType => 1 - Document, 2 - labels
            string deviceInfo = "";

            deviceInfo = template.Body;

            //if (docType == 1)
            //{
            //    deviceInfo = "<DeviceInfo>" +
            //      "  <OutputFormat>EMF</OutputFormat>" +
            //      "  <PageWidth>8.5in</PageWidth>" +
            //      "  <PageHeight>11in</PageHeight>" +
            //      "  <MarginTop>0.25in</MarginTop>" +
            //      "  <MarginLeft>0.25in</MarginLeft>" +
            //      "  <MarginRight>0.25in</MarginRight>" +
            //      "  <MarginBottom>0.25in</MarginBottom>" +
            //      "</DeviceInfo>";
            //}
            //else if (docType == 2)
            //{
            //      deviceInfo = "<DeviceInfo>" +
            //      "  <OutputFormat>EMF</OutputFormat>" +
            //      "  <PageWidth>4.1in</PageWidth>" +
            //      "  <PageHeight>6.2in</PageHeight>" +
            //      "  <MarginTop>0in</MarginTop>" +
            //      "  <MarginLeft>0in</MarginLeft>" +
            //      "  <MarginRight>0in</MarginRight>" +
            //      "  <MarginBottom>0in</MarginBottom>" +
            //      "</DeviceInfo>";
                
            //}

            Warning[] warnings;
            //m_streams = new List<Stream>();
            report.Render(renderFormat, deviceInfo, CreateStream, out warnings);

            foreach (Stream stream in m_streams[curTemplate])
                stream.Position = 0;
        }


        private static void PrintPage(object sender, PrintPageEventArgs ev)
        {
            try
            {
                Metafile pageImage = new Metafile(m_streams[curTemplate][m_currentPageIndex[curTemplate]]);
                ev.Graphics.DrawImage(pageImage, ev.PageBounds);

                m_currentPageIndex[curTemplate]++;
                ev.HasMorePages = (m_currentPageIndex[curTemplate] < m_streams[curTemplate].Count);
            }
            catch { }

        }



        private static void Print(object printerName)
        {
            if (m_streams == null || m_streams.Count == 0)
                return;


            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = printerName.ToString();
                if (!printDoc.PrinterSettings.IsValid)
                {
                    //string msg = String.Format("Can't find printer \"{0}\".", printerName);
                    Util.ShowError("Can't find printer " + printerName.ToString());
                    return;
                }
                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                printDoc.Print();

            }
            catch { }

            //  Borrar archivos temporales de impresion ?
            //Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                WmsSetupValues.PrintReportDir));

            foreach (FileInfo file in dir.GetFiles().Where(f => f.Name.Contains("emf") || f.Name.Contains("prn") ))
            {
                try { file.Delete(); }
                catch { }
            }
        }


        #endregion
    }
}
