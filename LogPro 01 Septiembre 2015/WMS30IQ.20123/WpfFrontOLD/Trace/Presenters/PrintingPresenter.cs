using System;
using WpfFront.Models;
using WpfFront.Services;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace WpfFront.Presenters
{



    public interface IPrintingPresenter
    {
        IPrintingView View { get; set; }
        void LoadDocument(Document document, bool isReceived, string printSession);
        ToolWindow Window { get; set; }
    }


    public class PrintingPresenter : IPrintingPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public IPrintingView View { get; set; }
        private Printer SelectedPrinter { get; set; }
        private string TemplateData { get; set; }
        private int step = 0; //Indica en que paso estamos 0 - New, 1 - generated, next => Print.
        private string RB_MANUAL = "rbManual";
        private string RB_DOC = "rbDocument";
        public ToolWindow Window { get; set; }


        public PrintingPresenter(IUnityContainer container, IPrintingView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<PrintingModel>();

            //Event Delegate
            View.LoadPrintForm += new EventHandler<EventArgs>(this.OnLoadPrintForm);
            //View.LoadProducts += new EventHandler<DataEventArgs<string>>(this.OnLoadProducts);
            View.LoadUnits += new EventHandler<DataEventArgs<Product>>(this.OnLoadUnits);
            View.AddToPrint += new EventHandler<EventArgs>(this.OnAddingToPrint);
            View.PrintLabels += new EventHandler<EventArgs>(this.OnPrintLabels);
            View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
            View.LoadPrintLines += new EventHandler<DataEventArgs<Document>>(this.OnLoadPrintLines);
            View.SelectPack += new EventHandler<DataEventArgs<Unit>>(OnSelectPack);
            View.RemoveFromList += new EventHandler<EventArgs>(this.OnRemoveFromList);
            View.RefreshLabelList += new EventHandler<DataEventArgs<bool?>>(OnRefreshLabelList);
            View.PrintPreview += new EventHandler<EventArgs>(this.OnPrintPreview);
            View.GenerateLabels += new EventHandler<EventArgs>(OnGenerateLabels);
            View.ResetForm += new EventHandler<EventArgs>(OnResetForm);

            View.Model.PrinterList = App.printerList;

            //Label templates
            View.Model.TemplateList = service.GetLabelTemplate(new LabelTemplate
            {
                LabelType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Label } }
            });


            //Si  hay conexion a ERP se habilita el panel de posting
            //if (App.IsConnectedToErpReceving)
                View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Received }).First();
            ////else
            ////    View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Stored }).First();

        }

        public void LoadDocument(Document document, bool isReceived, string printSession )
        {
            View.PrintLot.Text = printSession;

            ClearPrintForms();
            View.StkPrintForm.Visibility = Visibility.Visible;
            View.Model.LabelType = service.GetLabelType();
            View.RbDocument.IsChecked = true;
            //View.Model.ShowOnlyPack = true; //por defecto muestra solo los pack unit

            View.StkPrintByDocument.Visibility = Visibility.Visible;
            View.ComboLabelType.SelectedItem = View.Model.LabelType.Where(f => f.DocTypeID == LabelType.ProductLabel).First();

            //Load Document List
            View.Model.DocumentList = new List<Document>(); //service.GetReceivingDocument(WmsSetupValues.NumRegs);
            View.Model.DocumentList.Add(document);
            View.DocumentList.SelectedIndex = 0;
            View.DocumentList.Items.Refresh();

            View.Model.Document = document;
            View.Model.LabelsToPrint = null;
            View.ToPrintLabels.Items.Refresh();

            if (isReceived)
            {
                step = 1;
                LoadReceivedLabels(document, true);
                View.StkLabel.Visibility = Visibility.Visible;
                View.StkLine.Visibility = Visibility.Collapsed;

                if (View.Model.LabelsToPrint != null && View.Model.LabelsToPrint.Count == 0)
                {
                    //View.Model.ShowOnlyPack = false;
                    LoadReceivedLabels(document, false);
                }
            }
            else
            {
                LoadPrintLines(document);
                View.StkLabel.Visibility = Visibility.Collapsed;
                View.StkLine.Visibility = Visibility.Visible;
            }



            EnablePrintModule();

        }


        private void OnLoadPrintForm(object sender, EventArgs e)
        {
            ResetForm();

            LoadPrintForm(((RadioButton)sender).Name);

        }

        private void LoadPrintForm(string rb)
        {
            View.Model.LabelType = service.GetLabelType();

            //Evaluate the option selected
            if (rb == RB_MANUAL)
            {

                View.StkPrintManually.Visibility = Visibility.Visible;
                View.ComboLabelType.SelectedItem = View.Model.LabelType.Where(f => f.DocTypeID == LabelType.ProductLabel).First();
                return;
            }


            //Evaluate the option selected
            if (rb == RB_DOC)
            {
                View.StkPrintByDocument.Visibility = Visibility.Visible;
                View.ComboLabelType.SelectedItem = View.Model.LabelType.Where(f => f.DocTypeID == LabelType.ProductLabel).First();

                //Load Document List
                DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Receiving } };
                View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany }, 0, 0); //service.GetReceivingDocument(WmsSetupValues.NumRegs);
                View.DocumentList.Items.Refresh();
                return;
            }
        }


        private void OnResetForm(object sender, EventArgs e)
        {
            ResetForm();

            if (View.RbDocument.IsChecked == true)
                LoadPrintForm(RB_DOC);
            else
                LoadPrintForm(RB_MANUAL);
        }


        public void ResetForm()
        {
            ClearPrintForms();

            View.StkPrintFinish.Visibility = Visibility.Collapsed;
            View.StkPrintForm.Visibility = Visibility.Visible;

            View.StkLabel.Visibility = Visibility.Collapsed;
            View.StkLine.Visibility = Visibility.Collapsed;
            //View.Model.ShowOnlyPack = false;
            View.Model.LinesToPrint = null;
            View.Model.LabelsToPrint = null;
            View.Model.Document = null;
            step = 0;
            View.BtnConfirm.IsEnabled = true;
            //View.StkPrintManually.Visibility = Visibility.Visible;
            //View.RbManual.IsChecked = true;
            View.PrinterList.SelectedItem = null;
            View.PrintTemplate.SelectedItem = null;

        }


        private void ClearPrintForms()
        {
            View.StkPrintManually.Visibility = Visibility.Collapsed;
            View.StkPrintByDocument.Visibility = Visibility.Collapsed;

        }


        //private void OnLoadProducts(object sender, DataEventArgs<string> e)
        //{

        //    View.Model.Products = service.SearchProduct(e.Value);

        //    if (View.Model.Products == null || View.Model.Products.Count == 0)
        //    {
        //        View.Model.ProductUnits = null;
        //        View.Model.PackingUnits = null;
        //    }

        //    //Si solo hay un resultado carga de una vez las unidades
        //    if (View.Model.Products != null && View.Model.Products.Count == 1)
        //    {
        //        View.ComboProduct.SelectedIndex = 0;
        //        LoadUnits(View.Model.Products[0]);
        //    }

        //}


        private void OnLoadUnits(object sender, DataEventArgs<Product> e)
        {
            if (e.Value == null)
                return;

            LoadUnits(e.Value);

        }


        private void LoadUnits(Product product)
        {
            View.Model.ProductUnits = service.GetProductUnit(product);

            //El pack puede ser cualquier unidad del producto
            View.Model.PackingUnits = product.ProductUnits.Select(f => f.Unit).ToList();

            //Si es una sola unit seleccionarla
            if (View.Model.ProductUnits != null && View.Model.ProductUnits.Count == 1)
            {
                View.ComboUnit.SelectedIndex = 0;
                //El pack puede ser cualquier unidad del producto
                View.Model.PackingUnits = product.ProductUnits.Select(f => f.Unit)
                    .Where(f => f.UnitID != View.Model.ProductUnits[0].UnitID).ToList();
            }
            

        }


        private void OnAddingToPrint(object sender, EventArgs e)
        {
            try
            {
                int qtyPerPack = 0;

                View.ProcessResult.Text = "";
                Unit baseUnit = null;

                //Validating Product
                if (View.TxtProduct.Product == null)
                { Util.ShowError("Product not selected."); return; }

                //Validating Unit
                if (View.ComboUnit.SelectedIndex == -1)
                { Util.ShowError("UoM not Selected."); return; }

                View.Model.CheckAllRules();
                if (!View.Model.IsValid())
                { Util.ShowError("Error validating data. Please check data and try again."); return; }

                if (string.IsNullOrEmpty(View.TxtPrintQty.Text) || !(int.Parse(View.TxtPrintQty.Text) > 0))
                { Util.ShowError("Number of packs not valid."); return; }

                if (string.IsNullOrEmpty(View.TxtQtyPerPack.Text) || !(int.Parse(View.TxtQtyPerPack.Text) > 0))
                { Util.ShowError("Units per pack not valid."); return; }


                //check if logistic unit can contain pack unit
                Unit logisticUnit = View.Model.PackUnit;

                if (logisticUnit != null)
                {
                    baseUnit = View.ComboUnit.SelectedItem as Unit;

                    if (logisticUnit.BaseAmount <= baseUnit.BaseAmount)
                    { 
                        Util.ShowError("Pack Unit can not contain the current UoM.");
                        View.LogisticUnit.SelectedIndex = -1;
                        View.Model.PackUnit = null;
                        return; 
                    }

                    if ((logisticUnit.BaseAmount % baseUnit.BaseAmount) != 0)
                    { 
                       Util.ShowError("UoM " + baseUnit.Name + " can not be contained in Pack Unit " + logisticUnit.Name + ".");
                       View.LogisticUnit.SelectedIndex = -1;
                       View.Model.PackUnit = null; 
                       return;
                    }

                }
                
                
                if (logisticUnit == null && View.TxtQtyPerPack.Text != "")
                {
                    try { logisticUnit = service.GetUnit(new Unit { Company = App.curCompany, Name = WmsSetupValues.CustomUnit }).First(); }
                    catch
                    {
                        Util.ShowError("Unit Custom not defined.");
                        return;
                    }
                    logisticUnit.BaseAmount = double.Parse(View.TxtQtyPerPack.Text);

                }



                 try  
                 {
                   if (!string.IsNullOrEmpty(View.TxtQtyPerPack.Text))
                       qtyPerPack = int.Parse(View.TxtQtyPerPack.Text);
                 }
                catch 
                 { Util.ShowError("Units per pack not valid."); return; }



                string notes = "";

                if (logisticUnit != null)
                    notes = "Pack:" + logisticUnit.UnitID.ToString() + ":" + logisticUnit.Name;


                //if (View.ChkOnlyLogistic.IsChecked == true)
                //    notes += ",ONLYPACK";

                View.StkLine.Visibility = Visibility.Visible;


                //Define Document, Product, Unit and Qty to send to receiving transaction
                DocumentBalance printLine = new DocumentBalance
                {
                    Product = View.TxtProduct.Product,
                    Unit = (Unit)View.ComboUnit.SelectedItem,
                    Quantity = int.Parse(View.TxtPrintQty.Text) * int.Parse(View.TxtQtyPerPack.Text),
                    //Cantidad a imprimir la division del Units sobre el Qty per pack
                    QtyPending = (qtyPerPack > 0) ? qtyPerPack : 1,         
                    QtyProcessed = int.Parse(View.TxtPrintQty.Text),
                    Notes = notes   //Indica que se deben imprimir solo logisticas, de lo contrario imprime todo
                    //Logistica y Basica
                };


                if (View.Model.LinesToPrint == null)
                    View.Model.LinesToPrint = new List<DocumentBalance>();

                View.Model.LinesToPrint.Add(printLine);

                //Update Process Result
                //View.ProcessResult.Text = "Line Added to Print List.";
                View.ToPrintLines.Items.Refresh();
                View.TxtPrintQty.Text = "0";

                //Acomoda el panel para nuevos datos limpia
                //View.Model.Products = null;
                View.Model.ProductUnits = null;
                View.Model.PackingUnits = null;
                View.Model.PackUnit = null;
                //View.ChkOnlyLogistic.IsChecked = false;
                //View.ChkOnlyLogistic.Visibility = Visibility.Collapsed;
                View.TxtProduct.Text = "";
                View.TxtProduct.Product = null;
                View.TxtProduct.ProductDesc = "";
                View.TxtQtyPerPack.Text = "";

                //Enable Print Module
                EnablePrintModule();

            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }


        private void EnablePrintModule()
        {
            View.StkPrintFinish.Visibility = Visibility.Visible;

            //Si hay lineas de producto a imprimir
            if (step == 0)
            {
                View.BrdPreview.Visibility = View.BrFinishPrint.Visibility =  Visibility.Collapsed;
                View.BrdGenerate.Visibility = Visibility.Visible;
                //View.BrdGenerate.IsEnabled = true;
                View.BtnGenerate.IsEnabled = true;

           }

            if (step == 1)
            {
                View.ToPrintLabels.Items.Refresh();
                View.ToPrintLines.Items.Refresh();

                View.StkLabel.Visibility = Visibility.Visible;
                View.StkLine.Visibility = Visibility.Collapsed;

                View.BrdPreview.Visibility = View.BrFinishPrint.Visibility = Visibility.Visible;
                //View.BrdGenerate.Visibility = Visibility.Collapsed;
                View.BtnGenerate.IsEnabled = false;

                //Definir Lote 
                if (string.IsNullOrEmpty(View.PrintLot.Text))
                    View.PrintLot.Text = "PR" + DateTime.Now.ToString("yyMMddHHmmss");

                if (View.Model.TemplateList != null && View.Model.TemplateList.Count == 1)
                    View.PrintTemplate.SelectedIndex = 0;

                View.Model.PrinterList = App.printerList;
            }

        }


        private void OnPrintLabels(object sender, EventArgs e)
        {
            SelectedPrinter = (Printer)View.PrinterList.SelectedItem;


            //if (View.Model.LabelsToPrint == null || View.Model.LabelsToPrint.Count == 0)
            if (View.ToPrintLabels.SelectedItems == null)
            {
                Util.ShowError("No labels selected to print.");
                return;
            }

            ProcessWindow pw = new ProcessWindow("Printing Labels ...");

            try
            {
                //Setea el template si lo escogio.
                LabelTemplate tplFile = View.PrintTemplate.SelectedItem != null ?
                    ((LabelTemplate)View.PrintTemplate.SelectedItem) : null;

                //Si el template es Null Trata de setear el Template De receiving
                if (!string.IsNullOrEmpty(Util.GetConfigOption("RECVTPL")) && tplFile == null)
                {
                    try { tplFile = service.GetLabelTemplate(
                        new LabelTemplate { RowID = int.Parse(Util.GetConfigOption("RECVTPL")) }).First(); }
                    catch { }
                }


                //Send Labels to Print.
                List<WpfFront.WMSBusinessService.Label> lblToPrint = new List<WpfFront.WMSBusinessService.Label>();

                foreach (object lbl in View.ToPrintLabels.SelectedItems)
                    lblToPrint.Add((WpfFront.WMSBusinessService.Label)lbl);


                if (tplFile.IsPL == true)
                    service.PrintLabelsFromDevice(WmsSetupValues.DEFAULT, tplFile.Header, lblToPrint); //View.Model.LabelsToPrint.ToList()
                else
                    ReportMngr.PrintLabelsInBatch(tplFile, SelectedPrinter, lblToPrint); //View.Model.LabelsToPrint


                ResetForm();
                pw.Close();
                Util.ShowMessage("Process Completed.");

                return;



            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error printing labels.\n" + ex.Message);
            }

        }





        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Receiving } };

                if (string.IsNullOrEmpty(e.Value))
                {
                    //Load Document List                   
                    View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany },0,0);
                    return;
                }


                View.Model.DocumentList = service.SearchDocument(e.Value, docType)
                    .Where(f=>f.DocType.DocClass.DocClassID == SDocClass.Receiving).ToList();

                if (View.DocumentList.SelectedIndex == -1)
                    View.Model.LinesToPrint = null;

            }
            catch { }
        }


        private void OnLoadPrintLines(object sender, DataEventArgs<Document> e)
        {
            if (e.Value != null)
                LoadPrintLines(e.Value);

        }


        private void LoadPrintLines(Document document)
        {

            View.StkLine.Visibility = Visibility.Visible;


            View.Model.Document = document;
            DocumentBalance docBalance = new DocumentBalance { Document = document, Node = View.Model.Node };
            View.Model.LinesToPrint = service.GetDocumentBalance(docBalance, false);

            //#########Receiving Balance
            if (document.DocType.DocTypeID == SDocType.ReceivingTask)
                View.Model.LinesToPrint = service.GetDocumentBalanceForEmpty(docBalance);
            else
                View.Model.LinesToPrint = service.GetDocumentBalance(docBalance, false);



            if (View.Model.LinesToPrint != null && View.Model.LinesToPrint.Count > 0)
                EnablePrintModule();
            else
                View.StkPrintFinish.Visibility = Visibility.Visible;
        }


        //Febrero 23 2009 - Select Pack
        private void OnSelectPack(object sender, DataEventArgs<Unit> e)
        {
            if (e.Value == null)
                return;

            View.Model.PackUnit = e.Value;

        }


        private void LoadReceivedLabels(Document document, bool usePrintLot)
        {

            //si usePrintLot viene en false muestra todo los labels organizados por lote

            if (document == null)
                return;

            //Carga en la lista de labels, los labels que pertenecen a este documento y que no estan printed
            NodeTrace patterTrace = new NodeTrace
            {
                Document = document,
                Node = View.Model.Node,
                Status = new Status { StatusID = EntityStatus.Active }
            };


            //PrintLot trae los ultimos labels procesados.
            patterTrace.Label = new WpfFront.WMSBusinessService.Label
            {                
                PrintingLot = usePrintLot ? View.PrintLot.Text : "",
                FatherLabel = new WpfFront.WMSBusinessService.Label { LabelID = -1 },
                ReceivingDocument = document
                //El -1 especifica que la consulta debe hacerse sobre labels con father label = null
            };
            

            try
            {
                View.Model.LabelsToPrint = null;
                View.Model.LabelsToPrint = service.GetNodeTrace(patterTrace).Select(f => f.Label)
                     .Where(f=>f.Product.PrintLabel == true)
                     .OrderByDescending(f => f.LabelID).ToList(); //Ordenadas por Lote
            }
            catch { }

            View.ToPrintLabels.Items.Refresh();

            if (View.Model.LabelsToPrint != null && View.Model.LabelsToPrint.Count > 0)
            {
                View.BrdPreview.Visibility = Visibility.Visible;
                View.BrdGenerate.Visibility = Visibility.Collapsed;
            }

        }


        private void OnRemoveFromList(object sender, EventArgs e)
        {
            // Removing a Product
            if (((Button)sender).Name == "btnRemLine")
            {
                foreach (Object obj in View.ToPrintLines.SelectedItems)
                    View.Model.LinesToPrint.Remove((DocumentBalance)obj);
 

                View.ToPrintLines.Items.Refresh();
            }

            //Removing a Label
            if (((Button)sender).Name == "btnRemLabel")
            {
                foreach (Object obj in View.ToPrintLabels.SelectedItems)

                    View.Model.LabelsToPrint.Remove((WpfFront.WMSBusinessService.Label)obj);

                View.ToPrintLabels.Items.Refresh();

            }

        }

        /// <summary>
        /// Refresh label list when going to print labels for document thas was already received
        /// </summary>
        private void OnRefreshLabelList(object sender, DataEventArgs<bool?> e)
        {
            //View.Model.ShowOnlyPack = e.Value;
            LoadReceivedLabels(View.Model.Document, false);
        }


        private void OnPrintPreview(object sender, EventArgs e)
        {
            if (View.PreviewTemplate.SelectedItem == null)
            {
                Util.ShowError("Please select a template to Preview.");
                return;
            }


            UtilWindow.ShowLabelsToPrint((LabelTemplate)View.PreviewTemplate.SelectedItem, true, View.Model.LabelsToPrint); //163 to Test

        }


        private void OnGenerateLabels(object sender, EventArgs e)
        {
 
            if (View.Model.LinesToPrint == null || View.Model.LinesToPrint.Count == 0)
            {
                Util.ShowError("No lines to generate.");
                return;
            }


            ProcessWindow pw = new ProcessWindow("Generating Labels ...");

            try
            {               
                View.Model.LabelsToPrint = service.GenerateLabelsToPrint(View.Model.LinesToPrint.ToList(),
                    View.PrintLot.Text, App.curRol).Where(f=>f.Product.PrintLabel == true).ToList();

                View.Model.LinesToPrint = null;

                step = 1;
                View.BtnConfirm.IsEnabled = false;
                EnablePrintModule();

                pw.Close();
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Problem generating Label.\n"+ex.Message);
                return;
            }

        }
    }
}
