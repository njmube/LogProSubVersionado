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
using System.Collections.Specialized;
using System.Reflection;

namespace WpfFront.Presenters
{


    public interface IInventoryAdjustmentPresenter
    {
        IInventoryAdjustmentView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class InventoryAdjustmentPresenter : IInventoryAdjustmentPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public InventoryAdjustmentPresenter(IUnityContainer container, IInventoryAdjustmentView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<InventoryAdjustmentModel>();

            //Event Delegate
            View.LoadForm += new EventHandler<EventArgs>(this.OnLoadForm);
            //View.LoadProducts += new EventHandler<DataEventArgs<string>>(this.OnLoadProducts);
            View.LoadUnits += new EventHandler<DataEventArgs<Product>>(this.OnLoadUnits);
            View.AddToConfirm += new EventHandler<EventArgs>(this.OnAddingToConfirm);
            View.ExeInventoryAdjustment += new EventHandler<DataEventArgs<DocumentConcept>>(this.OnExecute);
            View.LoadSourceLocation += new EventHandler<DataEventArgs<string>>(this.OnLoadSource);
            View.RemoveFromList += new EventHandler<EventArgs>(this.OnRemoveFromList);                        
            View.Model.LinesToProcess = new List<DocumentLine>();
            View.ResetForm += new EventHandler<EventArgs>(this.OnResetForm);
            view.LoadAdjustment += new EventHandler<DataEventArgs<Document>>(this.OnLoadAdjustment);
            view.ReverseAdjustment += new EventHandler<EventArgs>(this.OnReverseAdjustment);
            View.AddSerial += new EventHandler<DataEventArgs<string>>(this.OnAddSerial);
            view.ReverseAdjustment += new EventHandler<EventArgs>(this.OnReverseAdjustment);
            View.ResendToERP += new EventHandler<EventArgs>(View_ResendToERP);
            View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);

            LoadHistAdjustments();
        }

        void View_ResendToERP(object sender, EventArgs e)
        {
            if (View.ListAdj.SelectedItem == null)
                return;

            try
            {
                Document document = View.ListAdj.SelectedItem as Document;
                document.ModifiedBy = App.curUser.UserName;
                document.ModDate = DateTime.Now;

                service.ReSendInventoryAdjustmentToERP(document);

                Util.ShowMessage("Adjustment Resent to ERP");
                View.BtnReSend.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex) {
                Util.ShowError("Sending Problems.\n" + ex.Message);
            }

        }

        public IInventoryAdjustmentView View { get; set; }
        ProcessWindow pw = null;

        // histórico de adjustments
        private void LoadHistAdjustments()
        {
            View.Model.Adjustments = service.GetDocument(
                new Document { DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment } } //, DocStatus = new Status { StatusID = DocStatus.Completed }
              );
        }

        //Carga el Stpe Dos de listado a Mover (product o label)
        private void OnLoadForm(object sender, EventArgs e)
        {
            ClearForms();
            View.StkForm.Visibility = Visibility.Visible;
           
        }


        private void ClearForms()
        {
            //View.StkManually.Visibility = Visibility.Collapsed;
            View.StkFinish.Visibility = Visibility.Collapsed;
            View.BrCart.Visibility = Visibility.Collapsed;

            //View.TxtProduct.IsEnabled = true;
            View.ComboUnit.IsEnabled = View.ComboProduct.IsEnabled = true;
            View.BtnExecute.IsEnabled = false;
            //View.TxtComment.Text = "";
            View.Model.Document = null;
            View.CboConcept.SelectedIndex = -1;

            View.Model.Products = null;
            View.Model.SourceLocation = null;
            View.TxtSource.Text = "";


            //View.Model.SourceData = null;
            View.TxtSource.IsEnabled = true;

            View.Model.LinesToProcess.Clear();
            ClearPanel();
            View.ComboProduct.Focus();

        }


        //private void OnLoadProducts(object sender, DataEventArgs<string> e)
        //{

        //    View.Model.Products = service.SearchProduct(e.Value);
        //    //Si solo hay un resultado carga de una vez las unidades
        //    if (View.Model.Products != null && View.Model.Products.Count == 1)
        //    {
        //        View.ComboProduct.SelectedIndex = 0;
        //        LoadUnits(View.Model.Products[0]);
        //    }

        //}


        private void OnLoadUnits(object sender, DataEventArgs<Product> e)
        {
            LoadUnits(e.Value);
        }


        private void LoadUnits(Product e)
        {
            try
            {
                if (e != null)
                {
                    View.Model.ProductUnits = service.GetProductUnit(e);
                    if (View.Model.ProductUnits.Count == 1)
                        View.ComboUnit.SelectedIndex = 0;

                }
                else
                    View.Model.ProductUnits = null;
            }
            catch { }
        }


        private void OnAddingToConfirm(object sender, EventArgs e)
        {
            try
            {
                //View.ProcessResult.Text = "";

                //Validating Product
                if (View.ComboProduct.Product == null)
                { Util.ShowError("Product not selected."); return;  }

                //Validating Unit
                if (View.ComboUnit.SelectedIndex == -1)
                { Util.ShowError("Unit not Selected."); return;  }

                View.Model.CheckAllRules();
                if (!View.Model.IsValid())
                { Util.ShowError("Error validating data. Please check data and try again."); return; }

                int qty;
                if (!int.TryParse(View.TxtQty.Text, out qty))
                    { Util.ShowError("Quantity is not valid."); return; }

                if (qty <= 0)
                { Util.ShowError("Quantity is not valid."); return; }

                //Validating Product
                if (View.AdjType.SelectedIndex == -1)
                { Util.ShowError("Adjustment type not selected."); return; }

                if (View.Model.SourceLocation == null)
                { Util.ShowError("Please select a valid Bin."); return; }

                // CAA [2010/05/26]
                // Si el producto requiere seriales se guarda el detallado del ajuste 1 x 1
                int cont=0, qtyLine=0, qtySerials=0;
                string serial="";
                if (((Product)View.ComboProduct.Product).IsUniqueTrack)
                // if (((Product)View.ComboProduct.Product).ProductCode.Equals("C3802010"))
                {
                    cont = int.Parse(View.TxtQty.Text);
                    qtyLine = 1;

                    // Ajuste solo para bines  (si es con serial)
                    if (View.Model.SourceLocation.LabelType.DocTypeID != LabelType.BinLocation)
                    {
                        Util.ShowError("Ubication not valid.\nPlease enter a bin not a label barcode."); 
                        return; 
                    }
                }
                else
                {
                    cont = 1;
                    qtyLine = int.Parse(View.TxtQty.Text);
                    qtySerials = 1;
                    serial = "-";
                }

                View.Model.QtySerials = cont;
                View.Model.QtySerialsRead = qtySerials;

                for (int x = 0; x < cont; x++)
                {
                    //Define Document, Product, Unit and Qty to send to receiving transaction
                    DocumentLine processLine = new DocumentLine
                    {
                        Product = (Product)View.ComboProduct.Product,
                        Unit = (Unit)View.ComboUnit.SelectedItem,
                        Quantity = qtyLine,
                        CreatedBy = App.curUser.UserName,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        IsDebit = (View.AdjType.Text == "Debit (-)") ? true : false,
                        UnitBaseFactor = ((Unit)View.ComboUnit.SelectedItem).BaseAmount,
                        LinkDocNumber = View.Model.SourceLocation.LabelID.ToString(),
                        Note = View.Model.SourceLocation.LabelCode,
                        BinAffected = View.Model.SourceLocation.LabelCode,
                        AccountItem = serial
                        //Guarda el label ID de donde se origina la transaccion
                    };
                    
                    service.CheckAdjustmentLine(processLine, View.Model.SourceLocation);
                    View.Model.LinesToProcess.Add(processLine);
                }
                
                //Update Process Result
                //View.ProcessResult.Text = "Line Added to Process List.";
                View.ToProcessLines.Items.Refresh();
                View.TxtQty.Text = "0";


                //Enable Execute Module
                EnableExecuteModule();

                ClearPanel();
                //View.TxtSource.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Util.ShowError("Adjustment detail could not be added.\n" + ex.Message);
            }
        }


        public void ClearPanel()
        {
            View.TxtQty.Text = "";
            View.Model.Products = null;
            View.Model.ProductUnits = null;
            View.ComboProduct.Text = "";
            View.AdjType.SelectedItem = null;

            //if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel)
            //{
                View.TxtSource.Text = "";
                View.Model.SourceLocation = null;
            //}

            // CAA [2010/05/26]
            // si no se han leido todos los seriales debe bloquear la entrada de datos
            if( View.Model.QtySerials==View.Model.QtySerialsRead)
                View.BtnConfirm.IsEnabled = true;
            else
                View.BtnConfirm.IsEnabled = false;
        }


        private void OnRemoveFromList(object sender, EventArgs e)
        {
            // Removing a Product
            if (((Button)sender).Name == "btnRemProduct")
            {
                foreach (DocumentLine obj in View.ToProcessLines.SelectedItems)
                {
                    View.Model.LinesToProcess.Remove(obj);
                    // CAA [2010/05/27]
                    // Si tiene serial asignado se disminuye el contador
                    View.Model.QtySerials--;
                    if (!obj.AccountItem.Equals(""))
                        View.Model.QtySerialsRead--;     // los leidos
                }

                View.ToProcessLines.Items.Refresh();
            }
                

            //Enable Print Module
            EnableExecuteModule();
        }



        //Habilita el step 2 despues de que se ha ingresados un Source Location 
        //Valido
        private void OnLoadSource(object sender, DataEventArgs<string> e)
        {
            //View.Model.SourceData = null;
            //ClearForms();

                if (string.IsNullOrEmpty(e.Value))
                return;

            //Get Location Data
            WpfFront.WMSBusinessService.Label sourceLabel = service.GetLocationData(e.Value, false);
            
            //Show Location Data in the List View
            View.Model.SourceLocation = sourceLabel;

            //2. Logistic Label
            if (sourceLabel.LabelType.DocTypeID == LabelType.ProductLabel)
            {
                    View.ComboProduct.Text = sourceLabel.Product.FullDesc;
                    View.ComboProduct.Product = sourceLabel.Product;

                    LoadUnits(sourceLabel.Product);
            }

            EnableExecuteModule();
        }


        private void EnableExecuteModule() {

            // CAA [2010/05/26]
            // si no se han leido todos los seriales debe bloquear el proceso
            if (View.Model.LinesToProcess.Count > 0 && View.Model.QtySerials==View.Model.QtySerialsRead )
            {
                View.StkFinish.Visibility = Visibility.Visible;
                View.BrCart.Visibility = Visibility.Visible;
                View.BtnExecute.IsEnabled = true;
                View.BrSerials.Visibility = Visibility.Collapsed;
                View.BtnConfirm.IsEnabled = true;
            }
            else
            {
                View.StkFinish.Visibility = Visibility.Collapsed;
                if (View.Model.LinesToProcess.Count == 0)
                {
                    View.BrCart.Visibility = Visibility.Collapsed;
                    View.BrSerials.Visibility = Visibility.Collapsed;
                }
                else
                    if (View.Model.QtySerials != View.Model.QtySerialsRead) // forma q pide los seriales
                    {
                        View.BrCart.Visibility = Visibility.Visible;
                        View.BrSerials.Visibility = Visibility.Visible;

                    }
            }
            
            
        }


        private void OnExecute(object sender, DataEventArgs<DocumentConcept> e)
        {
            //if (string.IsNullOrEmpty(View.TxtComment.Text.Trim()))
            //{
            //    Util.ShowError("Please enter the adjustment cause or observation.");
            //    return;
            //}

            if (e.Value == null)
            {
                Util.ShowError("Please select the adjustment cause.");
                return;
            }



            int count = 1;
            int step = 0;

            Document curDocument = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                DocConcept = e.Value,
                CreatedBy = App.curUser.UserName,
                Location = App.curLocation,
                Company = App.curCompany,
                IsFromErp = false,
                CrossDocking = false,         
                Comment = View.TxtComment.Text.Trim(),
                Date1 = DateTime.Now
            };

            if (Util.GetConfigOption("REASON2ERP").Equals("T"))
                curDocument.Notes = curDocument.DocConcept.Name.ToUpper();


            ProcessWindow pw = new ProcessWindow("Processing the Adjustment ... ");

            try
            {


                //Header del Documento de Ajuste
                curDocument = service.CreateNewDocument(curDocument, true);
                step = 1; //Creo el header del documento

                DocumentLine curLine;
                foreach (DocumentLine docLine in View.Model.LinesToProcess)
                { 
                    docLine.Document = curDocument;
                    docLine.Location = App.curLocation;
                    docLine.LineNumber = count++;

                    //LAbel Origen de la transaccion, guardada en lao notes de la linea
                    WpfFront.WMSBusinessService.Label source = service.GetLabel(new WpfFront.WMSBusinessService.Label { LabelID = long.Parse(docLine.LinkDocNumber) }).First();

                    curLine = service.SaveAdjustmentTransaction(docLine, source);
                    if (curLine.Note != "Adjust OK.")
                        throw new Exception(curLine.Note);
                }

                step = 2; //Creo las lineas

                service.CreateInventoryAdjustment(curDocument);


                View.BtnExecute.IsEnabled = false;
                ClearForms();
                LoadHistAdjustments();

                pw.Close();
                Util.ShowMessage("Adjustment document " + curDocument.DocNumber + " was created.");


            }
            catch (Exception ex)
            {

                pw.Close();

                if (step > 0)
                {
                    curDocument.DocStatus = new Status { StatusID = DocStatus.Cancelled };
                    curDocument.Comment = "Cancelled: " + ex.Message;
                    service.UpdateDocument(curDocument);
                }

                Util.ShowError("Adjustment document could not be created.\n" + ex.Message);
            }


        }


        // CAA [2010/05/27]
        // Agrega serial a cada detalle pendiente por setear
        private void OnAddSerial(object sender, DataEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(e.Value))
                return;

            // detalles pendientes por asignarle serial
            List<DocumentLine> docLines = View.Model.LinesToProcess.Where(f => f.AccountItem.Equals("")).ToList();
            if (docLines == null || docLines.Count == 0)
                return;
            
            // validar serial
            // a.  Q no haya sido asignado ya 
            // b.  si es credito, q no exista.... o debito que si exista...
            if (View.Model.LinesToProcess.Where(f => f.AccountItem.Equals(e.Value)).Count() > 0)
            {
                Util.ShowError("Serial# ["+e.Value+"] was already added !");
                return;
            }

            IList<WpfFront.WMSBusinessService.Label> labels = service.GetLabel(new WpfFront.WMSBusinessService.Label
            {
                LabelCode = e.Value,
                Product = View.ComboProduct.Product,
                Node = new Node { NodeID = NodeType.Stored },
                Status = new Status { StatusID = EntityStatus.Active }
            });

            if (labels.Count() > 0 && !docLines[0].IsDebit.Value)  // credit
            {
                Util.ShowError("Serial# [" + e.Value + "] already exists !");
                return;
            }

            if (labels.Count() == 0 && docLines[0].IsDebit.Value)  // Debit
            {
                Util.ShowError("Serial# [" + e.Value + "] doesn't exist !");
                return;
            }

            // actualizamos serial al 1ero con Serial vacío q encuentre
            docLines[0].AccountItem = e.Value;
            View.Model.QtySerialsRead++;

            EnableExecuteModule();
        }

        private void OnResetForm(object sender, EventArgs e)
        {
            ClearForms();
            LoadHistAdjustments();
        }

        //Carga información del Adjustment seleccionado
        private void OnLoadAdjustment(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            LoadAdjustment(e.Value);

        }


        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                ProcessWindow("Searching ...", false);

                if (string.IsNullOrEmpty(e.Value))
                {
                    pw.Close();
                    LoadHistAdjustments();
                    return;
                }

                View.Model.Adjustments = service.SearchDocument(e.Value, new DocumentType { DocTypeID = SDocType.InventoryAdjustment   });

                //si encuentra un resultado lo carga
                if (View.Model.Adjustments != null && View.Model.Adjustments.Count == 1)
                {
                    View.ListAdj.SelectedIndex = 0;
                    LoadAdjustment(View.Model.Adjustments[0]);
                }


                pw.Close();
            }
            catch { pw.Close(); }
        }


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore)
        {
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }


        private void LoadAdjustment(Document document)
        {
            try
            {
                View.Model.Adjustment= document;
                // ponemos visible la info
                View.StkAdjustData.Visibility = Visibility.Visible;
                View.BtnReverse.Visibility = Visibility.Collapsed;
                View.BtnReSend.Visibility = Visibility.Collapsed;


                if (document.DocStatus.StatusID == DocStatus.New || document.DocStatus.StatusID == DocStatus.Completed)
                    View.BtnReverse.Visibility = Visibility.Visible;

                if ((document.DocStatus.StatusID == DocStatus.New || document.DocStatus.StatusID == DocStatus.Cancelled || document.DocStatus.StatusID == DocStatus.NotCompleted) && Util.GetConfigOption("WITHERPIN").Equals("T"))
                    View.BtnReSend.Visibility = Visibility.Visible;


                // basic data
                View.Model.AdjustmentData = Util.ToShowData(document);

                //Actualizando datos del Documento requeridos para posibles transacciones
                document.ModifiedBy = App.curUser.UserName;

                //detalle del adjustment
                View.Model.AdjustmentLines = service.GetDocumentLine(new DocumentLine { Document = document });

            }
            catch (Exception ex)
            {
                Util.ShowError("Adjustment could not be loaded.\n" + ex.Message);
            }
        }

        private void OnReverseAdjustment(object sender, EventArgs e)
        {
            try
            {
                // CAA [2010/05/28]
                // NO se pueden reversar "AJUSTES" que incluyan seriales
                if (View.Model.AdjustmentLines.Where(f => !f.AccountItem.Equals("") && !f.AccountItem.Equals("-")).Count() > 0)
                {
                    Util.ShowError("Adjustment for serial #'s can't be reversed. ");
                    return;
                }

                //View.Model.Adjustment.Comment = e.Value;
                View.Model.Adjustment.ModDate = DateTime.Now;
                View.Model.Adjustment.ModifiedBy = App.curUser.UserName;
                service.ReverseInventoryAdjustment(View.Model.Adjustment);

                Util.ShowMessage("Adjustment " + View.Model.Adjustment.DocNumber + " was cancelled.");
                View.StkAdjustData.Visibility = Visibility.Collapsed;
                LoadHistAdjustments();


            }
            catch (Exception ex)
            {
                Util.ShowError("Adjustment could not be cancelled.\n" + ex.Message);
            }
        }
    }
}
 