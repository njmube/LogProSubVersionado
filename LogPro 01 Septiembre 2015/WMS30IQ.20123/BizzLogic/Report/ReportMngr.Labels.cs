using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Report;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Entities.Trace;
using Integrator;
using Entities;
using Entities.Master;
using System.Reflection;
using Entities.General;
using Microsoft.Reporting.WinForms;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using Entities.Profile;
using System.Diagnostics;

namespace BizzLogic.Logic
{
    /// <summary>
    /// Maneja el modulo de despliegue de reportes documentos - Tickets y otros
    /// </summary>
    public partial class ReportMngr : BasicMngr
    {
        //***************************************************
        //Importante, Estas variable sson usadas en (Labels y Documents)
        //***************************************************

        private static Printer usePrinter = null;
        //private static int m_currentPageIndex;
        private static Dictionary<LabelTemplate, IList<Stream>> m_streams = null;
        private static Dictionary<LabelTemplate, int> m_currentPageIndex = null;
        private static LabelTemplate curTemplate = null;
        private static LocalReport localReport = null;
        private static string AppPath = null;
        private static IList<Label> sListLabels = null;
        private static string batFile = null;



        public ReportMngr()
        { Factory = new DaoFactory(); }


        public static void PrintLabelsFromFacade(Printer printer, LabelTemplate defTemplate, IList<Label> listOfLabels, string appPath)
        {

            usePrinter = null;

            if (listOfLabels == null || listOfLabels.Count == 0)
                return;

            //if (string.IsNullOrEmpty(printer))
            //throw new Exception("Printer not found");

            if (printer != null && printer.PrinterName != WmsSetupValues.DEFAULT)
                usePrinter = printer;


            AppPath = appPath;
            m_streams = new Dictionary<LabelTemplate, IList<Stream>>();
            m_currentPageIndex = new Dictionary<LabelTemplate, int>();

            //1. Si viene un template imprime los labes con ese template
            if (defTemplate != null)
            {
                try
                {
                    if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel)
                        PrintLabelByTemplate(defTemplate, listOfLabels.Where(f => f.Product.PrintLabel != false).ToList());
                    else
                        PrintLabelByTemplate(defTemplate, listOfLabels);
                    
                    UpdateLabelPrintStatus();
                    return;
                }
                catch { throw; }                
            }

            //Agrupa a los diferentes tipos de label y a los null y los manda por aparte.
            //Filtra los que no imprime label (double check)
            IList<LabelTemplate> templateList = new List<LabelTemplate>();

            //Si el lable es de producto busca las templates del producto.
            if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel) 
                templateList = listOfLabels.Where(f => f.Product.DefaultTemplate != null)
                    .Select(f => f.Product.DefaultTemplate).Distinct().ToList();

            string error = "";


            //Para cada template mandando la impresion
            foreach (LabelTemplate template in templateList)
            {
                try
                {
                    if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel)
                        PrintLabelByTemplate(template, listOfLabels.Where(f => f.Product.DefaultTemplate.RowID == template.RowID && f.Product.PrintLabel != false)
                        .ToList());
                    else
                        PrintLabelByTemplate(template, listOfLabels);



                    UpdateLabelPrintStatus();
                    //tl = new Thread(new ThreadStart(UpdateLabelPrintStatus));
                    //tl.Start();

                }
                catch (Exception ex)
                {
                    error += ex.Message;
                }
            }

            //Mandando las labels con template en Null
            try
            {

                defTemplate = (new DaoFactory()).DaoLabelTemplate().Select(new LabelTemplate { 
                    Header = WmsSetupValues.DefaultLabelTemplate}).First();
                
                List<Label> labelsWoTemplate = null;
                if (listOfLabels[0].LabelType.DocTypeID == LabelType.ProductLabel)
                    labelsWoTemplate = listOfLabels.Where(f => f.Product.DefaultTemplate == null && f.Product.PrintLabel != false).ToList();
                else
                    labelsWoTemplate = listOfLabels.ToList();
                
                PrintLabelByTemplate(defTemplate, labelsWoTemplate);

                //tl = new Thread(new ThreadStart(UpdateLabelPrintStatus));
                //tl.Start();
                UpdateLabelPrintStatus();

            }
            catch (Exception ex) { error += ex.Message; }



            //Final Error
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);


        }



        private static void PrintLabelByTemplate(LabelTemplate rdlTemplateName, IList<Label> listOfLabels)
        {
            try
            {
                if (listOfLabels == null || listOfLabels.Count == 0)
                    return;

                sListLabels = listOfLabels;


                try
                {
                    //Si viene una impresora definida utiliza esa, si no utiliza la del template
                    if (rdlTemplateName != null)
                        curTemplate = rdlTemplateName;
                    else
                        curTemplate = (new DaoFactory()).DaoLabelTemplate().Select(new LabelTemplate { Header = WmsSetupValues.ProductLabelTemplate }).First();


                    usePrinter = (usePrinter == null)
                        ? new Printer { PrinterName = curTemplate.DefPrinter.Name, PrinterPath = curTemplate.DefPrinter.CnnString }
                        : usePrinter;
                }
                catch { throw new Exception("Printer not defined for template " + curTemplate.Name); }

                if (usePrinter == null)
                    throw new Exception("Printer not defined for template " + curTemplate.Name);


                //Revisa si el label imprime por comandos y lo manda a esa ruta.
                if (rdlTemplateName.IsPL == true)
                {
                    PrintLabelByPL(rdlTemplateName, listOfLabels, usePrinter);
                    return;
                }


                string labelPath = Path.Combine(AppPath, WmsSetupValues.RdlTemplateDir + "\\" + rdlTemplateName.Header);

                if (!File.Exists(labelPath))
                    throw new Exception("Label template " + labelPath + " does not exists.\n");


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
                m_streams.Add(curTemplate, new List<Stream>());

                m_currentPageIndex.Add(curTemplate, 0);

                Export(localReport, curTemplate, "IMAGE");  //1 - Document, 2 -  Label


                m_currentPageIndex[curTemplate] = 0;

                //Ejecutar la impresion global en un Hilo            
                Thread th = new Thread(new ParameterizedThreadStart(Print));
                th.Start(curTemplate.DefPrinter.CnnString);
                //Print(curTemplate.DefPrinter.CnnString);


            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("PrintLabelByTemplate:" + rdlTemplateName.Name, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(ex.Message);
            }
        }



        public int PrintPackageLabels(Document shipment, string printer, string appPath, string labelcode)
        {


            IList<Label> labelsToPrint = null;
            LabelTemplate defTemplate = null;

            if (shipment != null && shipment.DocID != 0)
            {
                labelsToPrint = Factory.DaoDocumentPackage().Select(
                         new DocumentPackage { PostingDocument = shipment }
                         ).Select(f => f.PackLabel).ToList();

                try
                {
                    defTemplate = Factory.DaoLabelTemplate().Select(
                        new LabelTemplate { Header = WmsSetupValues.DefaultPackLabelTemplate }).First();
                }
                catch { }


            }
            else if (!string.IsNullOrEmpty(labelcode))
            {
                labelsToPrint = Factory.DaoDocumentPackage().Select(
                     new DocumentPackage { PackLabel = new Label { LabelCode = labelcode } }
                     ).Select(f => f.PackLabel).ToList();


                try
                {
                    if (labelsToPrint[0].Package.PackageType == "P" || labelsToPrint[0].Package.Level == 0) // || labelsToPrint[0].Package.ParentPackage == null
                        defTemplate = Factory.DaoLabelTemplate().Select(
                            new LabelTemplate { Header = WmsSetupValues.DefaultPalletTemplate }).First();

                    else if (labelsToPrint[0].Package.PackageType == "B")
                        defTemplate = Factory.DaoLabelTemplate().Select(
                            new LabelTemplate { Header = WmsSetupValues.DefaultPackLabelTemplate }).First();

                }
                catch { }

            }

            if (labelsToPrint == null || labelsToPrint.Count == 0)
                return 0;


            Printer objPrinter = null;

            //Printer - Default for the Template
            objPrinter = new Printer { PrinterName = WmsSetupValues.DEFAULT };


            ReportMngr.PrintLabelsFromFacade(objPrinter, defTemplate, labelsToPrint, appPath);
            return labelsToPrint.Count;

        }



        private static void UpdateLabelPrintStatus()
        {
            DaoFactory f = new DaoFactory();

            if (sListLabels == null || sListLabels.Count == 0)
                return;


            foreach (Label label in sListLabels)
            {
                if (label.Printed == true || label.LabelID == 0)
                    continue;

                label.Printed = true;
                f.DaoLabel().Update(label);
            }
        }


        //Process Labels
        public static DataSet ProcessLabels(IList<Label> labelList)
        {
            if (labelList == null || labelList.Count == 0)
                throw new Exception("No labels found.");

            //Pricessing Labels
            return ReportMngr.GetReportDataset(labelList, 10);
        }


        //Obtiene el detalle de label y entrega el dataset
        public static DataSet GetReportDataset(IList<Label> labelList, int quantity)
        {
            try
            {

                ReportDetailFormat detail;
                IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();

                string notes = "";

                int seq = 1;
                foreach (Label label in labelList)
                {
                    detail = new ReportDetailFormat();

                    if (label.Product != null)
                    {
                        detail.ProductCode = label.Product.ProductCode;
                        detail.ProductDescription = label.Product.Name;
                    }

                    notes = (label.Notes != null) ? label.Notes : "";

                    if (label.IsLogistic == true)
                    {
                        try { detail.Unit = notes.Split(',')[0].Trim().Replace("Default", ""); }
                        catch { }
                        detail.QtyOrder = label.StartQty; //Aqui guardan las logisitcas las hijas que contienen unas vez se imprimen
                    }
                    else
                    {
                        detail.Unit = label.Unit != null ? label.Unit.Name : "Default";
                        detail.QtyOrder = label.CurrQty;
                    }

                    //detail.BarcodeProduct = label.Product.ProductCode;
                    detail.BarcodeLabel = (label.Barcode != null) ? label.Barcode : "";
                    detail.Lote = (label.LotCode != null) ? label.LotCode : "";
                    detail.Date1 = (label.ExpirationDate != null) ? label.ExpirationDate.Value.ToShortDateString() : "";
                    detail.Printed = DateTime.Today.ToString("MM/dd/yyyy hh:mm:ss");
                    detail.Serial = (label.SerialNumber != null) ? label.SerialNumber : "";
                    detail.Notes = notes;
                    detail.PrintLot = (label.PrintingLot != null) ? label.PrintingLot : "";
                    detail.UserName = (label.CreatedBy != null) ? label.Barcode : "";
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

                        try { detail.Pieces = label.DocumentPackages[0].Pieces; }
                        catch { }



                        //Shipping Address
                        try
                        {
                            DocumentAddress shipAddr = (new DaoFactory()).DaoDocumentAddress().Select(
                                new DocumentAddress { Document = label.ShippingDocument, AddressType = AddressType.Shipping })
                            .Where(f => f.DocumentLine == null).First();

                            detail.ContactPerson = shipAddr.Name; //shipAddr.ContactPerson;

                            //detail.ShipAddress = shipAddr.Name + "\n";
                            detail.ShipAddress = shipAddr.AddressLine1 + " " + shipAddr.AddressLine2 + "\n";
                            detail.ShipAddress += shipAddr.City + ", " + shipAddr.State + " " + shipAddr.ZipCode + ", ";
                            detail.ShipAddress += shipAddr.Country + "\n";
                            detail.ShipAddress += shipAddr.ContactPerson;

                        }
                        catch { }
                    }

                    seq++;
                    detailList.Add(detail);

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



        #region Print Batch Methods

        private static Stream CreateStream(string name, string fileNameExtension, Encoding encoding,
            string mimeType, bool willSeek)
        {
            try
            {
                Stream stream = new FileStream(
                    Path.Combine(
                        AppPath,
                        WmsSetupValues.PrintReportDir + "\\" + Guid.NewGuid() + name + "." + fileNameExtension),
                    FileMode.Create);

                m_streams[curTemplate].Add(stream);
                return stream;
            }
            catch { return null;  }
        }


        private static void Export(LocalReport report, LabelTemplate template, string renderFormat)
        {
            //renderFormat = "IMAGE", "PDF", 

            string deviceInfo = template.Body;

            Warning[] warnings;
            //m_streams = new List<Stream>();


            try
            {
                report.Render(renderFormat, deviceInfo, CreateStream, out warnings);

                foreach (Stream stream in m_streams[curTemplate])
                    stream.Position = 0;

            }
            catch (Exception ex)
            {
                string exMessage = WriteLog.GetTechMessage(ex);
                //throw new Exception(exMessage);
            }


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
            try
            {
                if (m_streams == null || m_streams.Count == 0)
                    return;

                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = printerName.ToString();

                if (!printDoc.PrinterSettings.IsValid)
                {
                    //throw new Exception("Can't found printer " + printerName.ToString());
                    ExceptionMngr.WriteEvent("Print: " + "Can't found printer " + printerName.ToString(), ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Printer);
                    return;
                }


                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                printDoc.Print();


                //  Borrar archivos temporales de impresion ?
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppPath, WmsSetupValues.PrintReportDir));

                foreach (FileInfo file in dir.GetFiles().Where(f => f.Name.Contains("emf") || f.Name.Contains("prn") || f.Name.Contains("tif")))
                {
                    if (file.CreationTime.AddDays(+8) < DateTime.Now)
                    {
                        try { file.Delete(); }
                        catch { }
                    }
                }

            }
            catch { }
        }


        #endregion



        #region CPL Print Methods


        //Imprime en impresora mandando la lineas de comando PCL
        private static void PrintLabelByPL(LabelTemplate template, IList<Label> listLabels, Printer printer )
        {
            LabelMngr lblMngr = new LabelMngr();
            string templateReplaced = lblMngr.GetReplacedTemplateForLabels(listLabels, template, "");

            if (string.IsNullOrEmpty(templateReplaced))
            {
                //throw new Exception("Error creating Printer Template.");
                ExceptionMngr.WriteEvent("Error creating Printer Template. " + template.Header, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Printer);

            }

            PrintLabels(templateReplaced, printer.PrinterPath);
        }


        //Envia un archivo de templates a imprimir
        public static bool PrintLabels(string labelData, string printerPort)
        {
            //TODO: Módulo de impresión
            bool result = false;
            string printPort = printerPort;
            string filePath = CreatePrintTemporaryFile("", labelData);

            batFile =  AppPath + "\\PRINT.BAT"; 
            batFile = batFile.Replace("\\\\", "\\");

            if (!File.Exists(batFile))
            {
                //throw new Exception("Setup file " + batFile + " does not exists.\n");
                ExceptionMngr.WriteEvent("Setup file " + batFile + " does not exists.\n", ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Printer);
            }

            batFile = "\"" + batFile + "\""; //Comillas para que el DOS lo reconozca

            try
            {
                if (File.Exists(filePath) == false)
                {
                    //throw new Exception("Please setup the temporary printing file.");
                    ExceptionMngr.WriteEvent("Please setup the temporary printing file.", ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Printer);

                }


                //TEST FILE TO SEE THE COMMAND
                //CreatePrintTemporaryFile("Command" + Guid.NewGuid().ToString(), batFile + " \"" + filePath + "\" " + printPort);
                
                string printCommand = "\"" + filePath + "\" \"" + printPort + "\"";

                //////Asynchronously start the Thread to process the Execute command request.
                //Thread objThread = new Thread(new ParameterizedThreadStart(SendPrintProcess));
                //////Make the thread as background thread.
                //objThread.IsBackground = true;
                //////Set the Priority of the thread.
                //objThread.Priority = ThreadPriority.AboveNormal;
                //////Start the thread.
                //objThread.Start(printCommand);


                SendPrintProcess(printCommand);

            }
            catch (Exception ex)
            {
                result = true;
                //throw ex;
            }

            return result;
        }


        public static string CreatePrintTemporaryFile(string printLot, string labelData)
        {
            if (string.IsNullOrEmpty(printLot))
                printLot = Guid.NewGuid().ToString();

            string tmpPrintFile = AppPath + "\\" + WmsSetupValues.PrintReportDir
                   + "\\" + printLot + ".prn";

            tmpPrintFile = tmpPrintFile.Replace("\\\\", "\\");

            StreamWriter writer = new StreamWriter(tmpPrintFile);

            writer.WriteLine(labelData);
            writer.Flush();
            writer.Close();

            return tmpPrintFile;

        }


        private static void SendPrintProcess(object printCommand)
        {
            //Declare and instantiate a new process component.
            Process cmdProcess = new Process();

             try
            {

                //ExceptionMngr.WriteEventLog(batFile + " "+ printCommand);

                //Do not receive an event when the process exits.
                cmdProcess.EnableRaisingEvents = false;
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.FileName = batFile;
                cmdProcess.StartInfo.Arguments = printCommand.ToString();
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.Start();
                cmdProcess.WaitForExit();
                cmdProcess.Close();
            }
            catch (Exception ex) {
                //ExceptionMngr.WriteEvent("SendPrintProcess: " + printCommand, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                ExceptionMngr.WriteEventLog(WriteLog.GetTechMessage(ex));
            }

        }

        #endregion

    }
}
