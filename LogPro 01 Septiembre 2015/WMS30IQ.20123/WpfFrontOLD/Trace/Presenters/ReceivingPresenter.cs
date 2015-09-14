using System;
//using WpfFront.BusinessObject;
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
using System.Windows;

using Xceed.Wpf.DataGrid;
using System.Collections;
using WMComposite.Regions;
using Core.WPF;



namespace WpfFront.Presenters

{


    public interface IReceivingPresenter
    {
        IReceivingView View { get; set; }
        void SetDocument(Document document);
        ToolWindow Window { get; set; }
    }



    public class ReceivingPresenter : IReceivingPresenter
    {
        private readonly IUnityContainer container;
        private WMSServiceClient service;
        private readonly IShellPresenter region;
        private bool showNextTime = true;
        ProcessWindow pw = null;
        private bool AllPosted = true;
        private bool AllReceived = true;
        private string PrintSessionLot = "";
        private ITrackOptionPresenter trackControl;
        private bool IsDirectReceipt;
        public ToolWindow Window { get; set; }
        Bin damageBin;


        public ReceivingPresenter(IUnityContainer container, IReceivingView view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.service = new WMSServiceClient();
                this.region = region;
                View.Model = this.container.Resolve<ReceivingModel>();

                //Event Delegate
                View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
                View.LoadDetails += new EventHandler<DataEventArgs<Document>>(this.OnLoadDetails);
                View.LoadUnits += new EventHandler<DataEventArgs<Product>>(this.OnLoadUnits);
                View.ReceiveProduct += new EventHandler<EventArgs>(this.OnReceivingProduct);
                View.ReceiveLabel += new EventHandler<DataEventArgs<string>>(this.OnReceivingLabel);
                View.ReceiveLabelList += new EventHandler<EventArgs>(this.OnReceivingLabelList);
                View.PostReceipt += new EventHandler<EventArgs>(this.OnPostReceipt);
                View.ReceiptAtOnce += new EventHandler<EventArgs>(this.OnReceiptAtOnce);
                View.CreateEmptyReceipt += new EventHandler<EventArgs>(this.OnCreateEmptyReceipt);
                //View.LoadProducts += new EventHandler<DataEventArgs<string>>(this.OnLoadProducts);
                //View.LoadVendors += new EventHandler<DataEventArgs<string>>(this.OnLoadVendors);
                View.ChangeStatus += new EventHandler<EventArgs>(this.OnChangeStatus);

                View.ReceiveLabelTrackOption += new EventHandler<EventArgs>(this.OnReceivingLabelTrackOption);

                //View.LoadProductManualTrackOption += new EventHandler<EventArgs>(this.OnLoadProductManualTrackOption);
                //View.ReceiveManualTrack += new EventHandler<EventArgs>(this.OnReceiveManualTrack);

                //View.AddManualTrackToList += new EventHandler<EventArgs>(this.OnAddManualTrackToList);
                //View.RemoveManualTrack += new EventHandler<EventArgs>(this.OnRemoveManualTrack);

                View.SelectedUnit += new EventHandler<EventArgs>(this.OnSelectUnit);
                View.LoadPostedReceipt += new EventHandler<DataEventArgs<Document>>(this.OnLoadPostedReceipt);
                View.ReversePosted += new EventHandler<DataEventArgs<string>>(this.OnReversePosted);
                View.ShowReceivingTicket += new EventHandler<EventArgs>(this.OnShowReceivingTicket);
                View.GoToPrintLabels += new EventHandler<EventArgs>(this.OnGoToPrintLabels);
                View.LateDocuments += new EventHandler<DataEventArgs<bool?>>(this.OnLateDocuments);
                View.RemoveFromNode += new EventHandler<DataEventArgs<DocumentBalance>>(this.OnRemoveFromNode);
                View.AssignBinToProduct += new EventHandler<DataEventArgs<string>>(OnAssignBinToProduct);
                View.SelectPack += new EventHandler<DataEventArgs<Unit>>(OnSelectPack);
                //View.LoadBins += new EventHandler<DataEventArgs<string>>(OnLoadBins);
                View.GoToCrossDock += new EventHandler<EventArgs>(OnGoToCrossDock);
                View.ReceiveReturn += new EventHandler<DataEventArgs<double>>(View_ReceiveReturn);


                //Version Adicionada para IMAGE Services
                View.ReceiptAcknowledge += new EventHandler<DataEventArgs<double>>(View_ReceiptAcknowledge);
                // CAA [2010/05/03]
                View.ShowPurchaseReceive += new EventHandler<EventArgs>(this.OnShowPurchaseReceive);

                //Set Node to Receiving Node. Cuando se crea el PR se pasa el producto a Stored
                View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Received }).First();

                //Mostrar la columna de Arrived                
                if (Util.GetConfigOption("RECACKN").Equals("T"))
                {
                    View.DgDocument.Columns["Date5"].Visible = true; //Arrived
                    View.GBArrive.Visibility = Visibility.Visible;
                }
                

                //Cargue de Documentos
                LoadDocuments();
            }
            catch (Exception ex)
            {
                Util.ShowError("Error loading view.\n" + ex.Message);
            }

        }





        public IReceivingView View { get; set; }
        private DocumentType docType;
        private DateTime refDate;



        private void LoadDocuments()
        {
            try
            {

               View.DgDocument.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 330;

               ProcessWindow(Util.GetResourceLanguage("LOADING_DOCUMENTS"), false);
            
                docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Receiving } };
                refDate = DateTime.Now;

                //Load the Base Documents (Pendig pero sin fecha de referencia)
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation }, 0, 0) // 
                    .OrderByDescending(f => f.Date1).ToList(); //;

                //LoadStatus 
                View.Model.DocStatus = App.DocStatusList; 

                View.ProcessResult.Text = "";
                View.TxtPostResult.Text = "";

                pw.Close();

            }
            catch (Exception ex) {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }


        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                ProcessWindow(Util.GetResourceLanguage("SEARCHING"), false);

                if (string.IsNullOrEmpty(e.Value))
                {
                    pw.Close();
                    LoadDocuments();
                    return;
                }

                View.Model.DocumentList = service.SearchDocument(e.Value, docType);

                //si encuentra un resultado lo carga
                if (View.Model.DocumentList != null && View.Model.DocumentList.Count == 1)
                {
                    View.DgDocument.SelectedIndex = 0;
                    LoadDetails(View.Model.DocumentList[0]);
                }
                else
                    ResetDetails();

                pw.Close();
            }
            catch { pw.Close();  }
        }


        private void OnLateDocuments(object sender, DataEventArgs<bool?> e)
        {
            ProcessWindow(Util.GetResourceLanguage("LOADING_LATE_DOCUMENTS"), false);

            if (e.Value == true)
                //Pending with reference Date = NOW
                View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany },0,0)
                    .Where(f => f.Date1 <= refDate).OrderByDescending(f=>f.Date1).ToList(); //
            else
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation }, 0, 0)
                    .OrderByDescending(f => f.Date1).ToList(); //

            View.DgDocument.Items.Refresh();

            pw.Close();
        }


        private void ResetDetails()
        {
            View.Model.DocumentData = null;
            View.Model.DocumentLines = null;
            View.Model.DocProducts = null;
            View.Model.VendorItem = "";
            View.Model.LabelsAvailable = null;
            View.Model.DocumentBalance = null;
            View.TabDocDetails.Visibility = Visibility.Hidden;
        }


        private void OnLoadDetails(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;


            if (View.Model.Document != null && View.Model.Document.DocID == e.Value.DocID)
                return;

            try
            {
                ProcessWindow(Util.GetResourceLanguage("LOADING_DOCUMENT") + e.Value.DocNumber + " ...", false);
                LoadDetails(e.Value);
                pw.Close();


                //Mesaje de que todo fue recibido
                //if (AllReceived == true && View.Model.Document.IsFromErp == true)
                    //Util.ShowMessage("This document does not have quantities pending to receive.");

            }
            catch (Exception ex) {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_LOADED")+ "\n" + ex.Message);
            }

        }


        private void LoadDetails(Document document)
        {
            
            View.BinLocation.Text = "";

            //Reseting UControl of Product
            View.TxtProduct.Text = "";
            View.TxtProduct.Product = null;
            View.TxtProduct.DefaultList = null;
            View.TxtProduct.DataList = null;
            View.TxtProduct.ProductDesc = "";


            //if (!View.Model.PutAwayDirect)
                //View.ChkPutAway.IsChecked = false;
            View.Model.PutAwayDirect = false;
            if (Util.GetConfigOption("PUTAWAYDIRECT").Equals("T"))
                View.Model.PutAwayDirect = true;


            View.BtnCrossDock.Visibility = Visibility.Visible;
            //View.BtnCreateReceipt.Content = "Create Purchase Receipt";
            View.BtnCreateReceipt.Content = Util.GetResourceLanguage("CREATE_RECEIPT");

            if (document.DocType.DocTypeID != SDocType.PurchaseOrder)
            {
                View.BtnCreateReceipt.Content = Util.GetResourceLanguage("CONFIRM_DOCUMENT");
                View.BtnCrossDock.Visibility = Visibility.Collapsed;
            }

            //Comportamiento Especial de Return- 26 Mayo 2009
            View.BinLocation.IsEnabled = true;
            View.BinLocation.Text = "";
            
            View.ChkPutAway.IsEnabled = true;
            View.ChkPutAway.IsChecked = View.Model.PutAwayDirect;


            View.GridManual.Visibility = Visibility.Visible;
            View.GridReturn.Visibility = Visibility.Collapsed;


            if (document.DocType.DocTypeID == SDocType.Return)
            {
                View.BinLocation.IsEnabled = false;
                View.BinLocation.Text = DefaultBin.RETURN;
                View.ChkPutAway.IsChecked = false;
                View.ChkPutAway.IsEnabled = false;

                //Habilitando el Expander de Return
                View.GridManual.Visibility = Visibility.Collapsed;
                View.GridReturn.Visibility = Visibility.Visible;

                try
                {
                    damageBin = service.GetBin(new Bin { BinCode = DefaultBin.DAMAGE, Location = App.curLocation }).First();
                }
                catch { }
            }

            //SI EL DOCUMENT ES RECEIVING TASK -- MIRA si tiene configurado un BIN Especifico para las TASKS
            //Si no USA MAIN
            if (document.DocType.DocTypeID == SDocType.ReceivingTask && !string.IsNullOrEmpty(Util.GetConfigOption("RCTASKBIN")))
            {
                View.BinLocation.IsEnabled = false;
                View.BinLocation.Text = Util.GetConfigOption("RCTASKBIN"); //Sale del Config
                View.ChkPutAway.IsChecked = false;
                View.ChkPutAway.IsEnabled = false;
            }



            //Actualizando datos del Documento requeridos para posibles transacciones
            document.ModifiedBy = View.Model.User.UserName;
            //document.Location = App.curLocation;
            
            View.Model.ProductUnits = null;            
            View.Model.PackingUnits = null;
            View.Model.PackUnit = null;
            View.Model.CurQtyPending = 0;


            //No product Selected
            View.Model.Product = null;
            View.Model.VendorItem = "";
            View.TabProductInfo.Visibility = Visibility.Collapsed;

            View.Model.Document = document;

            View.ProcessResult.Text = "";

            View.TxtPostResult.Text = "";

            View.TabDocDetails.Visibility = Visibility.Visible;

            View.Model.DocumentData = Util.ToShowData(document);


            //DIRECT PRINT 30oct2009
            View.DirectPrint.Document = View.Model.Document;
            if (View.Model.Document.DocStatus.StatusID == DocStatus.New)
                View.DirectPrint.NewStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.InProcess).First();


            //Check If CrossDock 7 Marzo 09
            CheckIfCrossDock();

            View.Model.DocumentLines = service.GetDocumentLine(new DocumentLine { Document = document });

            View.ExpDocLines.IsExpanded = true;
            View.BtnRecTkt.Visibility = Visibility.Visible;

            IsDirectReceipt = false;
            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
            {
                IsDirectReceipt = true;
                View.ExpDocLines.IsExpanded = false;
                //Receiving Ticket
                View.BtnRecTkt.Visibility = Visibility.Collapsed;
            }
            
            //Select Status
            //View.ComboStatus.SelectedValue = View.Model.Document.DocStatus.StatusID;


            RefreshBalance(document);            


            //Cargue del tab de recibo, solo si el documento es new or in process   
            View.TabItemReceive.Visibility = Visibility.Collapsed;

            //Si no esta en receiving vuelve al tab de detalles
            View.TabDocDetails.SelectedIndex = View.TabDocDetails.SelectedIndex > 1 ? 0 : View.TabDocDetails.SelectedIndex;  

            if (document.DocStatus.StatusID == DocStatus.InProcess || document.DocStatus.StatusID == DocStatus.New
                || document.DocStatus.StatusID == DocStatus.Completed)
            {
                View.TabItemReceive.Visibility = Visibility.Visible;
                
                View.Model.VendorItem = "";

                //View.Model.DocProducts = service.GetDocumentProductFromLines(View.Model.DocumentLines);
                View.Model.DocProducts = View.Model.DocumentBalance.Where(f => f.QtyPending > 0)
                    .Select(f=>f.Product).ToList();

                //Setea para que el Control de producto solo muestre el balance.
                if (View.Model.Document.IsFromErp == true)
                    View.TxtProduct.DefaultList = View.Model.DocProducts;

                //si encuentra un resultado lo carga
                //SelectOneProduct();

                //Solo muestra los labels si hay saldos en el documento
                if (AllReceived == false)
                    ShowLabelsAvailable(document);

                View.BtnReceiveLabel.Visibility = Visibility.Collapsed;

                if (View.Model.LabelsAvailable != null && View.Model.LabelsAvailable.Count > 0)
                    View.BtnReceiveLabel.Visibility = Visibility.Visible;

                //Reset the manual receiving
                View.Model.ReceivingQuantity = 0;
            }

          

            //Reset Posted Receipts
            View.DgReceiptLines.Visibility = View.BtnReversePosted.Visibility = Visibility.Collapsed;
            View.StkReceiptData.Visibility = Visibility.Collapsed;


            if (View.TabItemReceive.Visibility != Visibility.Visible && View.TabDocDetails.SelectedIndex > 0 )
                View.TabDocDetails.SelectedIndex = 0;          

          

            //Receiving Expande
            View.ExpManual.IsEnabled = true;            
            if (document.DocType.DocTypeID == SDocType.WarehouseTransferReceipt)
            {
                View.ExpManual.IsEnabled = false;
                View.ExpManual.IsExpanded = false;
            }

        }
        

        //private void SelectOneProduct()
        //{
        //    if (View.Model.DocProducts != null && View.Model.DocProducts.Count == 1)
        //    {
        //        View.ComboProduct.SelectedIndex = 0;
        //        LoadUnits((Product)View.ComboProduct.SelectedItem);
        //        View.ComboUnit.Focus();
        //    }
        //}


        private void ShowLabelsAvailable(Document document) {

            Label searchLabel;
            //Si es un documento de Transfer debe sacar las Lables de las Zona de Release donde lo Dejo el Shipment
            searchLabel = new Label { Node = new Node { NodeID = NodeType.PreLabeled }, Status = new Status { StatusID = EntityStatus.Locked } }; 

            // Mustra posibles labels si es fromERP, si no muestra todo lo disponible en node PreLabeled
            if (document.IsFromErp == true)
                View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(document, searchLabel);
            else
                View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(new Document { Location = App.curLocation } , searchLabel);
        }


        private void OnLoadUnits(object sender, DataEventArgs<Product> e)
        {
            try
            {
                if (e.Value != null)
                    LoadUnits(e.Value);
                else
                    View.Model.ProductUnits = null;

            }
            catch { }
        }


        private void LoadUnits(Product product)
        {
            View.ComboUnit.Focus();
            View.Model.Product = product;

            //Item Number
            IList<ProductAccountRelation> vendItem = product.ProductAccounts.Where(f => f.Account.AccountID == View.Model.Document.Vendor.AccountID).ToList();
            if (vendItem != null && vendItem.Count() > 0)
                View.Model.VendorItem = "Vendor Item Number: " + vendItem[0].ItemNumber;

            View.BtnReceive.Visibility = Visibility.Visible;
            View.TabProductInfo.Visibility = Visibility.Visible;

            if (View.Model.Document.IsFromErp == true)
            {
                //Debe traer solo la unidad requerida por el documento para ese producto
                View.Model.ProductUnits = View.Model.DocumentLines.Where(f => f.Product.ProductID == product.ProductID)
                        .Select(f => f.Unit).Distinct().ToList();

                //El pack puede ser cualquier unidad del producto
                View.Model.PackingUnits = product.ProductUnits.Select(f => f.Unit).ToList();

            }
            else
                View.Model.PackingUnits = View.Model.ProductUnits = product.ProductUnits.Select(f => f.Unit).ToList();


            //Adicionando el paquete en Blanco
            try { View.Model.PackingUnits.Add(new Unit { }); }
            catch { }


            if (View.Model.ProductUnits != null && View.Model.ProductUnits.Count == 1)
            {
                View.ComboUnit.SelectedIndex = 0;
                View.TxtRcvQty.Focus();
                SelectUnit();
            }


            //Asignacion al producto del modelo
            //View.Model.Product = product;

            //Si Tiene trackOption se muestran
            //View.TxtProductTrackMsg.Visibility = Visibility.Collapsed;

            //COMENTARIADO PARA MANEJAR EL trackControl GLOBAL DEL RECIBO.
            //if (product.ProductTrack != null && product.ProductTrack.Where(f => f.IsRequired == true).Count() > 0)
            //{
            //    //View.TxtProductTrackMsg.Visibility = Visibility.Visible;
            //    if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
            //        View.BtnReceive.Visibility = Visibility.Collapsed;
            //}


            ShowProductStock();

        }


        private void ShowProductStock()
        {
            //Show Product Stock
            View.LvStock.Visibility = Visibility.Collapsed;
            View.Model.ProductBinStock = service.GetProductStock(
                new ProductStock { Product = View.Model.Product, Bin = new Bin { Location = App.curLocation } }, null);

            if (View.Model.ProductBinStock != null && View.Model.ProductBinStock.Count > 0)
                View.LvStock.Visibility = Visibility.Visible;
        }


        private void OnSelectUnit(object sender, EventArgs e)
        {
            SelectUnit();
        }


        private void SelectUnit()
        {
            if (View.ComboUnit.SelectedItem == null) 
                return;

            //Remover la Unidad que no pueda contener a la otra del Packing List
            IList<Unit> curPack = View.Model.Product.ProductUnits.Select(f => f.Unit).ToList();
            View.Model.PackingUnits = curPack;
            Unit[] unitList = new Unit[curPack.Count];
            curPack.CopyTo(unitList, 0);

            foreach (Unit unit in unitList)
                if (((Unit)View.ComboUnit.SelectedItem).BaseAmount >= unit.BaseAmount)
                    View.Model.PackingUnits.Remove(unit);         
            
            View.LogisticUnit.Items.Refresh();

            //Poner el Qty Pending para esa unidad
            try
            {
                View.Model.CurQtyPending = int.Parse(View.Model.DocumentBalance
                    .Where(f => f.Product.ProductID == View.Model.Product.ProductID && f.Unit.UnitID == ((Unit)View.ComboUnit.SelectedItem).UnitID)
                    .First().QtyPending.ToString());
            }
            catch { View.Model.CurQtyPending = 0;  }

            View.Model.AllowReceive = true;
            if (View.Model.Document.IsFromErp == true)
                View.Model.AllowReceive = (View.Model.CurQtyPending > 0) ? true : false;

        }


        private void OnReceivingProduct(object sender, EventArgs e )
        {
            try
            {
                View.ProcessResult.Text = "";
                View.TxtPostResult.Text = "";

                Bin destLocation = service.GetBinLocation(View.BinLocation.Text, View.ChkPutAway.IsChecked);

                if (destLocation == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("BIN_DESTINATION") + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                    return;
                }

                //Validating Product
                if (View.TxtProduct.Product == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("PRODUCT_NOT_SELECTED"));
                    return;
                }

                //Validating Unit
                if (View.ComboUnit.SelectedIndex == -1)
                {
                    Util.ShowError(Util.GetResourceLanguage("UNIT_NOT_SELECTED"));
                    return;
                }

                View.Model.CheckAllRules();
                if (!View.Model.IsValid())
                {
                    Util.ShowError(Util.GetResourceLanguage("ERR_VAL_DATA_PLE_CHE_DAT_AND_TRY_AGA"));
                    return;
                }

                if (View.TxtRcvQty.Text == "" || !(int.Parse(View.TxtRcvQty.Text) > 0))
                {
                    Util.ShowError(Util.GetResourceLanguage("NUMBER_OF_PACKAGES_NOT_VALID"));
                    return;
                }

                if (View.TxtQtyPerPack.Text == "" || !(int.Parse(View.TxtQtyPerPack.Text) > 0))
                {
                    Util.ShowError(Util.GetResourceLanguage("UNITS_PER_PACKAGE_NOT_VALID"));
                    return;
                }


                Unit logistic = null;
                Unit baseUnit = null;

                //check if logistic unit can contain pack unit
                if (View.LogisticUnit.SelectedItem != null)
                {
                    logistic = View.LogisticUnit.SelectedItem as Unit;
                    baseUnit = View.ComboUnit.SelectedItem as Unit;

                    if (logistic.BaseAmount <= baseUnit.BaseAmount)
                    { Util.ShowError(Util.GetResourceLanguage("LOGISTIC_UNIT_CAN_NOT_CONTAIN_PACK_UNIT")); return; }

                    if ((logistic.BaseAmount % baseUnit.BaseAmount) != 0)
                    { Util.ShowError(Util.GetResourceLanguage("LOGISTIC_UNIT") + logistic.Name + Util.GetResourceLanguage("CAN_NOT_CONTAINED_IN_PACK_UNIT") + baseUnit.Name + "."); return; }
                }


                ProcessWindow(Util.GetResourceLanguage("RECEIVING_PRODUCT"), false);
                ProcessReceivingProduct(destLocation, int.Parse(View.TxtRcvQty.Text));
                pw.Close();

            }
            catch (Exception ex) {
                Util.ShowError(ex.Message);
                pw.Close();
            }
        }


        private void ProcessReceivingProduct(Bin destLocation, int numberOfPackages)
        {
            //Lote de Session Actual - Manejado para imprimir los ultimos labels
            if (string.IsNullOrEmpty(PrintSessionLot))
                PrintSessionLot = "PR" + DateTime.Now.ToString("yyMMddHHmmss");

            //Define Document, Product, Unit and Qty to send to receiving transaction
            DocumentLine receivingLine = new DocumentLine
            {
                Document = View.Model.Document,
                Product = (Product)View.TxtProduct.Product, //View.ComboProduct.SelectedItem,
                Unit = (Unit)View.ComboUnit.SelectedItem,
                Quantity = numberOfPackages * double.Parse(View.TxtQtyPerPack.Text),
                QtyPending = (double.Parse(View.TxtQtyPerPack.Text) > 0) ? double.Parse(View.TxtQtyPerPack.Text) : 1,
                QtyAllocated = numberOfPackages,
                CreatedBy = App.curUser.UserName,
                Note = PrintSessionLot
            };


            Unit logisticUnit = View.Model.PackUnit;
            if (logisticUnit == null && int.Parse(View.TxtQtyPerPack.Text) > 1)
            {
                try { logisticUnit = service.GetUnit(new Unit { Company = App.curCompany, Name = WmsSetupValues.CustomUnit }).First(); }
                catch
                {
                    Util.ShowError(Util.GetResourceLanguage("UNIT_CUSTOM_NOT_DEFINED"));
                    return;
                }
                logisticUnit.BaseAmount = double.Parse(View.TxtQtyPerPack.Text);
            }
            

            service.ReceiveProduct(receivingLine, logisticUnit, destLocation, View.Model.Node);


            //If Update product info at receiving is enabled - Update Product
            if (Util.GetConfigOption("ALLUPPRO").Equals("T"))
            {
                View.Model.Product.ModifiedBy = App.curUser.UserName;
                View.Model.Product.ModDate = DateTime.Now;
                service.UpdateProduct(View.Model.Product);
            }


            //Refresh el Balance
            RefreshBalance(receivingLine.Document);

            //After Process
            AfterReceiving();


            View.ProcessResult.Text = Util.GetResourceLanguage("PRODUCT_RECEIVED");
            //Util.ShowMessage(View.ProcessResult.Text);
            if (showNextTime)
            {
                pw.Close();
                showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
            }

        }


        private void OnReceivingLabel(object sender, DataEventArgs<string> e)
        {
            View.ProcessResult.Text = "";
            View.TxtPostResult.Text = "";

            //Ocultar el Tracking Options
            View.StkLabelTrack.Visibility = Visibility.Collapsed;
            View.Model.CurScanedLabel = null;


            if (string.IsNullOrEmpty(e.Value.Trim()))
                return;


                Label label = new Label { LabelCode = e.Value };

                //Trae el label
                try { 

                    IList<Label> labelList = service.GetLabel(label);

                    //Revisa que solo un label sea entregado.
                    if (labelList != null && labelList.Count > 1)
                    {
                        Util.ShowError(Util.GetResourceLanguage("LABEL")+ " " + label.LabelCode + Util.GetResourceLanguage("EXISTS_MORE_THAN_ONCE_PLEASE_CHECK_IT"));
                        return;
                    }

                    label = labelList.First();
                    View.Model.CurScanedLabel = label;

                }
                catch
                {
                    Util.ShowError(Util.GetResourceLanguage("LABEL") + e.Value + Util.GetResourceLanguage("DOES_NOT_EXISTS"));
                    return;
                }



                //El label debe esta en la lista available cuando es un transfer.
                if (View.Model.Document.DocType.DocTypeID == SDocType.WarehouseTransferReceipt && View.Model.Document.IsFromErp != true)
                {
                    if (!View.Model.LabelsAvailable.Any(f => f.Barcode.Equals(e.Value) || f.LabelCode.Equals(e.Value)))
                    {
                        Util.ShowError(Util.GetResourceLanguage("LABEL") + " [" + e.Value + "] " + Util.GetResourceLanguage("IS_NOT_IN_THE_LIS_OF_LAB_TO_BE_RECE"));
                        return;
                    }

                }


                //Revisar las track Options
                if (label.Product.ProductTrack != null && label.Product.ProductTrack.Count > 0)
                {
                    View.StkLabelTrack.Visibility = Visibility.Visible;

                    //A temp value le asigna el valor "" para que se deje editar
                    for (int i = 0; i < label.Product.ProductTrack.Count; i++)
                        label.Product.ProductTrack[i].TempValue = "";

                    //Lenar el Grid que contiene el Track
                    View.Model.LabelTrackData = label.Product.ProductTrack;

                    return;
                }
             

                ProcessReceiveLabel(label);


        }


        //Recibe Varios lables de los disponibles
        private void OnReceivingLabelList(object sender, EventArgs e)
        {
            if (!(View.LabelListAvailable.SelectedItems != null
                && View.LabelListAvailable.SelectedItems.Count > 0))
            {
                Util.ShowError(Util.GetResourceLanguage("NO_LABEL_SELECTED"));
                return;
            }

            try
            {

                //Gettting Bin Location
                Bin destLocation = service.GetBinLocation(View.BinLocation.Text, View.ChkPutAway.IsChecked);

                if (destLocation == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("BIN_DESTINATION") + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                    return;
                }


                View.ProcessResult.Text = "";
                View.TxtPostResult.Text = "";

                ProcessWindow(Util.GetResourceLanguage("RECEIVING_LABELS"), false);

                // CAA [2010/04/08]
                // El bin destino puede ser el default o el seleccionado
                Boolean defaultBin=false;
                if (Util.GetConfigOption("USEBINDEFAULT").Equals("T"))
                    defaultBin=true;

                // procedimiento que recibe toda la lista masivamente
                service.ReceiveLabels(View.Model.Document, View.LabelListAvailable.SelectedItems.Cast<Label>().ToList(), destLocation, View.Model.Node, defaultBin, App.curLocation);

                //recorre la lista 
                foreach (object label in View.LabelListAvailable.SelectedItems)
                {
                    //service.ReceiveLabel(View.Model.Document, (Label)label, destLocation, View.Model.Node);
                    View.Model.LabelsAvailable.Remove((Label)label);
                }

                //Update Process Result
                View.LabelListAvailable.Items.Refresh();
                ShowLabelsAvailable(View.Model.Document);

                //Refresh el Balancee
                RefreshBalance(View.Model.Document);
                View.ProcessResult.Text = Util.GetResourceLanguage("LABEL_RECEIVED");


                pw.Close();
                Util.ShowMessage(View.ProcessResult.Text);



            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }



        public void RefreshBalance(Document document)
        {
            AllReceived = true;

            DocumentBalance docBalance = new DocumentBalance
            {
                Document = document,
                Node = View.Model.Node,
                Location = App.curLocation
            };

            //#########Receiving Balance

            if (View.Model.DocumentLines == null || IsDirectReceipt)
                View.Model.DocumentBalance = service.GetDocumentBalanceForEmpty(docBalance);
            else
                View.Model.DocumentBalance = service.GetDocumentBalance(docBalance, false);

            View.DgDocumentBalance.Items.Refresh();

            //El boton de Recibir todo se muestra si hay balance
            View.BtnReceiveAtOnce.IsEnabled = false;
            if (View.Model.DocumentBalance.Any(f => f.QtyPending > 0))
            {
                View.BtnReceiveAtOnce.IsEnabled = true;
                AllReceived = false;
            }

            //Si algun producto recibido para habilitar el boton de impresion.
            View.Model.AnyReceived = View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0);


            //##########Posting Balance

            View.Model.PendingToPostList = service.GetDocumentPostingBalance(docBalance);
            View.DgPostingBalance.Items.Refresh();


            //El boton de Posting solo se muestra si hay balance
            View.BtnCreateReceipt.IsEnabled = false;
            View.StkPosting.Visibility = Visibility.Collapsed;
            if (View.Model.PendingToPostList.Any(f => f.QtyPending > 0))
            {
                View.BtnCreateReceipt.IsEnabled = true;
                View.StkPosting.Visibility = Visibility.Visible;
                AllPosted = false;
            } 

            //Refresh Qty Pending If product is selected
            //Update Pending quantity 21 marzo 09
            try
            {
                if (View.Model.Product != null)
                {
                    View.Model.CurQtyPending = int.Parse(View.Model.DocumentBalance
                    .Where(f => f.Product.ProductID == View.Model.Product.ProductID && f.Unit.UnitID == ((Unit)View.ComboUnit.SelectedItem).UnitID)
                    .First().QtyPending.ToString());


                    View.Model.AllowReceive = true;
                    if (View.Model.Document.IsFromErp == true)
                        View.Model.AllowReceive = (View.Model.CurQtyPending > 0) ? true : false;

                }
            }
            catch { }


            //Marzo 21, si todo esta recibido y es un ERP documet cambia el estatus a Completed.
            //Si tiene Any Received cambia el status a In Process

            //Completed
            if (AllReceived && View.Model.Document.IsFromErp == true 
                && View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0)  //garantiza que al menos uno este procesado
                && View.Model.Document.DocStatus.StatusID != DocStatus.Completed)
            {
                View.Model.Document.DocStatus = new Status { StatusID = DocStatus.Completed };
                View.Model.Document.ModDate = DateTime.Now;
                View.Model.Document.ModifiedBy = App.curUser.UserName;
                service.UpdateDocument(View.Model.Document);
            }

            //In Process
            if (!AllReceived && View.Model.AnyReceived && View.Model.Document.DocStatus.StatusID != DocStatus.InProcess)
            {
                View.Model.Document.DocStatus = new Status { StatusID = DocStatus.InProcess };
                View.Model.Document.ModDate = DateTime.Now;
                View.Model.Document.ModifiedBy = App.curUser.UserName;
                service.UpdateDocument(View.Model.Document);
            }


            //Muestra el posting panel solo si permite parcial o si ha recibido completo.
            View.PanelPosting.Visibility = Visibility.Visible;
            if (!AllReceived && Util.GetConfigOption("PARTIALREC").Equals("F") && View.Model.Document.AllowPartial != true)
                View.PanelPosting.Visibility = Visibility.Collapsed;



            //Hidden Tracking y Others
            View.TabItemTrackOption.Visibility = Visibility.Collapsed;
            //Evaluar si la orden tiene producto para hacerle Tracking

            if(View.Model.DocumentLines.Select(f=>f.Product).Any(
                f=> f.ProductTrack != null && f.ProductTrack.Count > 0 &&
                    f.ProductTrack.Where(z => z.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).Count() > 0 ))
            {
                View.TabItemTrackOption.Visibility = Visibility.Visible;
                LoadTrackOptionsControl();
            }


            /*
            foreach (DocumentBalance db in View.Model.DocumentBalance)
            {
                //Se adiciono que el track option no sea de calidad de producto
                if (db.Product.ProductTrack != null && db.Product.ProductTrack.Count > 0 &&
                    db.Product.ProductTrack.Where(f => f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).Count() > 0) //&& db.QtyProcessed > 0
                {
                    View.TabItemTrackOption.Visibility = Visibility.Visible;
                    LoadTrackOptionsControl();
                    break;
                }
            }
             * */


            RefreshReceipts();

        }


        //Used for external methods
        public void RefreshProductList()
        {
            //Update The Default List
            View.TxtProduct.DefaultList = null;
            if (View.Model.Document.IsFromErp == true)
                View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0)
                    .Select(f => f.Product).ToList();
        }


        public void RefreshReceipts()
        {
            View.GrpReceipts.Visibility = Visibility.Collapsed;
            View.Model.Receipts = service.GetDocument(
                new Document { CustPONumber = View.Model.Document.DocNumber, 
                    DocType = new DocumentType { DocClass = new DocumentClass { 
                        DocClassID = SDocClass.Posting}  } }
              );

            if (View.Model.Receipts != null && View.Model.Receipts.Count > 0)
                View.GrpReceipts.Visibility = Visibility.Visible;

        }


        private void OnPostReceipt(object sender, EventArgs e)
        {
            try
            {

                ProcessWindow(Util.GetResourceLanguage("CREATING_PURCHASE_RECEIPT"), false);

                Document receiptToPost = View.Model.Document;

                //Datos de Nro de Documento del vendor y Fecha de Documento
                if (!string.IsNullOrEmpty(View.TxtDocDate.Text))
                    receiptToPost.Date5 = DateTime.Parse(View.TxtDocDate.Text);

                receiptToPost.Reference = View.TxtVendorDoc.Text;
                receiptToPost.CreatedBy = App.curUser.UserName;
                receiptToPost.Location = App.curLocation;
                receiptToPost.Comment = View.TxtRctComment.Text;

                //Llamdo asincrono al proceso de creacion

                //View.TxtPostResult.Text = "Wait Receipt is being created in ERP ...";
                View.BtnCreateReceipt.IsEnabled = false;

                Document pReceipt = service.CreatePurchaseReceipt(receiptToPost);


                View.TxtPostResult.Text = Util.GetResourceLanguage("PURCHASE_RECEIPT") + " [" + pReceipt.DocNumber + "]" + Util.GetResourceLanguage("CREATED");


                //Post - Process
                View.TxtVendor.Text = "";
                View.TxtVendorDoc.Text = "";

                RefreshReceipts();

                pw.Close();
                Util.ShowMessage(View.TxtPostResult.Text);

                //Mostrar el recibo y Mandarlo a IMprmir automaticamente
                if (Util.GetConfigOption("RECTKT").Equals("T"))                
                    UtilWindow.ShowDocument(pReceipt.DocType.Template, pReceipt.DocID, "", false);
                

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("RECEIPT_COULD_NOT_BE_CREATED") + "\n" + ex.Message);
                View.BtnCreateReceipt.IsEnabled = true;
            }
        }


        private void OnReceiptAtOnce(object sender, EventArgs e)
        {
            try
            {
                //Gettting Bin Location
                Bin destLocation = service.GetBinLocation(View.BinLocation.Text, View.ChkPutAway.IsChecked);

                if (destLocation == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("BIN_DESTINATION") + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                    return;
                }

                ProcessWindow(Util.GetResourceLanguage("RECEIVING_PRODUCT"), false);


                View.ProcessResult.Text = "";

                //Definiendo lotes de impresion
                if (string.IsNullOrEmpty(PrintSessionLot))
                    PrintSessionLot = "PR" + DateTime.Now.ToString("yyMMddHHmmss");

                View.Model.Document.Notes = PrintSessionLot;

                service.ReceiptAtOnce(View.Model.Document, destLocation, View.Model.Node);
                RefreshBalance(View.Model.Document);

                View.ProcessResult.Text = Util.GetResourceLanguage("DOCUMENT") + " " + View.Model.Document.DocNumber + Util.GetResourceLanguage("RECEIVED_AT_ONCE");

                pw.Close();
                Util.ShowMessage(View.ProcessResult.Text);

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("PRODUCT_COULD_NOT_BE_RECEIVED") + "\n" + ex.Message);
            }
        }


        //Crea un documento en Blanco para recibir sin tener en cuenta las document lines,
        //recibe cualquier producto
        private void OnCreateEmptyReceipt(object sender, EventArgs e)
        {
            if (View.TxtVendor.Account == null)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_VENDOR"));
                return;
            }

            try
            {
                ProcessWindow(Util.GetResourceLanguage("CREATING_RECEIPT_WITHOUT_PO"), false);

                DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Receiving } };
                docType.DocTypeID = SDocType.ReceivingTask;

                Document document = new Document
                {
                    DocType = docType,
                    CrossDocking = false,
                    IsFromErp = false,
                    Location = App.curLocation,
                    Company = App.curCompany,
                    Vendor = View.TxtVendor.Account,
                    Date1 = DateTime.Today,
                    CreatedBy = App.curUser.UserName
                };

                document = service.CreateNewDocument(document, true);
                LoadDetails(document);
                View.Model.DocumentList.Add(document);
                View.DgDocument.Items.Refresh();

                //Post Result
                View.TxtVendor.Text = "";

                pw.Close();
                Util.ShowMessage(Util.GetResourceLanguage("EMPTY_RECEIPT") + document.DocNumber + " " + Util.GetResourceLanguage("CREATED"));

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_CREATED") + "\n" + ex.Message);
            }
        }


        //private void OnLoadProducts(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.VendorItem = "";
        //    View.Model.DocProducts = service.SearchProduct(e.Value, View.Model.Document.Vendor);
        //    SelectOneProduct();
        //}


        private void OnChangeStatus(object sender, EventArgs e)
        {
            try
            {
                // CAA [2010/06/09]
                // no puede cancelar la PO si tiene items recibidos. (incluye PR)
                if (((Status)View.ComboStatus.SelectedItem).StatusID == DocStatus.Cancelled)
                {
                    if (View.Model.Receipts.Any(f => f.DocStatus.StatusID == DocStatus.Completed ||
                                                     f.DocStatus.StatusID == DocStatus.Posted))
                    {
                        Util.ShowError(Util.GetResourceLanguage("DOC_CON_COM_POS_REC_CAN_NOT_BE_CAN"));
                        return;
                    }
                    else
                        if (View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0))
                        {
                            Util.ShowError(Util.GetResourceLanguage("DOC_HAS_ITE_REC_CAN_NOT_BE_CAN"));
                            return;
                        }
                }

                View.Model.Document.DocStatus = (Status)View.ComboStatus.SelectedItem;
                View.Model.Document.ModifiedBy = View.Model.User.UserName;
                View.Model.Document.ModDate = DateTime.Now;
                service.UpdateDocument(View.Model.Document);

                Util.ShowMessage(Util.GetResourceLanguage("DOCUMENT_UPDATED"));
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_UPDATED") + "\n" + ex.Message);
            }
        }


        private void OnReceivingLabelTrackOption(object sender, EventArgs e)
        {
            View.ProcessResult.Text = "";
            View.TxtPostResult.Text = "";

            try
            {

                //Asigna los valores ingresados de las TrackOption al correspodiente campo del label
                foreach (ProductTrackRelation trackRel in View.Model.LabelTrackData)
                {
                    View.Model.CurScanedLabel.GetType().GetProperty(trackRel.TrackOption.Name)
                        .SetValue(View.Model.CurScanedLabel, trackRel.TempValue, null);
                }

                if (ProcessReceiveLabel(View.Model.CurScanedLabel))
                    View.StkLabelTrack.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }

        }


        private Boolean ProcessReceiveLabel(Label label)
        {
            //Gettting Bin Location
            Bin destLocation = service.GetBinLocation(View.BinLocation.Text, View.ChkPutAway.IsChecked);

            if (destLocation == null)
            {
                Util.ShowError(Util.GetResourceLanguage("BIN_DESTINATION") + " " + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                return false;
            }

            try
            {
                ProcessWindow(Util.GetResourceLanguage("RECEIVING_LABEL"), false);

                service.ReceiveLabel(View.Model.Document, label, destLocation, View.Model.Node);

                //Label borra si esta enum label lista de desplegadas
                if (View.Model.LabelsAvailable != null)
                {
                    IList<Label> labelToRemove = View.Model.LabelsAvailable.Where(f => f.LabelID == label.LabelID).ToList();

                    if (labelToRemove.Count > 0)
                    {
                        View.Model.LabelsAvailable.Remove(labelToRemove.First());
                        View.LabelListAvailable.Items.Refresh();
                    }
                }

                //Post - Process
                //Refresh el Balance
                RefreshBalance(View.Model.Document);
                View.TxtRecLabel.Text = "";
                View.ProcessResult.Text = Util.GetResourceLanguage("LABEL_RECEIVED");

                pw.Close();
                if (showNextTime)               
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);


                //Util.ShowMessage(View.ProcessResult.Text);

                return true;
            }

            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
                return false;
            }
        }

        /*
        private void OnLoadProductManualTrackOption(object sender, EventArgs e)
        {

            int qtyPendingtoTrack = 0; 
            int qtyperPack = 0;  //Unidades por Caja
            int qtyToTrack = 0;  //Numero de paquetes

             //Si no digita cantidad se va a administrar muestra los Tracks pendientes o ya existentes.
             //Pasa derecho:
                //Si No hay pendientes y el documento no es un directreceipt.
                //Si no hay cantidades a recibir


            if (View.Model.CurQtyPending > 0)
            {

                if (View.TxtRcvQty.Text == "" || !(int.Parse(View.TxtRcvQty.Text) > 0))
                {
                    Util.ShowError("Number of packages not valid.");
                    return;
                }

                if (View.TxtQtyPerPack.Text == "" || !(int.Parse(View.TxtQtyPerPack.Text) > 0))
                {
                    Util.ShowError("Units per Package not valid.");
                    return;
                }

            }
            
            
            if ((View.Model.CurQtyPending == 0 && !IsDirectReceipt) || View.TxtRcvQty.Text == "" || !int.TryParse(View.TxtRcvQty.Text, out qtyToTrack) || int.Parse(View.TxtRcvQty.Text) <= 0)
            {
                LoadTrackOptionsControl(qtyPendingtoTrack, qtyperPack, qtyToTrack);
                View.BtnTrackReceive.IsEnabled = false;
                return;
            }



            if (View.TxtQtyPerPack.Text == "" || !int.TryParse(View.TxtQtyPerPack.Text, out qtyperPack) || int.Parse(View.TxtQtyPerPack.Text) <= 0)
            {
                Util.ShowError("Units per package not valid.");
                return;
            }

            //Gettting Bin Location
            View.Model.Bin = service.GetBinLocation(View.BinLocation.Text, View.ChkPutAway.IsChecked);

            if (View.Model.Bin == null)
            {
                Util.ShowError("Bin destination " + View.BinLocation.Text + " is not valid.");
                return;
            }


            if (View.ComboUnit.SelectedIndex == -1)
            {
                Util.ShowError("Plase select a unit.");
                return;
            }



            if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
                View.BtnTrackReceive.IsEnabled = false;

            View.BtnTrackReceive.Visibility = Visibility.Visible;


            //Load User Contron el el ItemControl
            qtyPendingtoTrack = Int32.Parse(View.TxtQtyPerPack.Text) * Int32.Parse(View.TxtRcvQty.Text);
            LoadTrackOptionsControl(qtyPendingtoTrack, qtyperPack, qtyToTrack);


        }
        */

        private void LoadTrackOptionsControl()
        //private void LoadTrackOptionsControl(int qtyPendingtoTrack, int qtyperPack, int qtyToTrack)
        {
            //Making Tab Visible
            //View.TabItemTrackOption.Visibility = Visibility.Visible;
            //View.TabItemTrackOption.Focus();

            trackControl = container.Resolve<TrackOptionPresenter>();
            View.TrackOpts.Items.Clear();
            View.TrackOpts.Items.Add(trackControl.View);

            //Seting Values
            //trackControl.View.Model.Product = View.Model.Product;
            trackControl.View.Model.Document = View.Model.Document;
            trackControl.View.Model.Node = View.Model.Node;
            //trackControl.View.Model.CurUnit = View.ComboUnit.SelectedItem as Unit;
            //trackControl.View.Model.CurQtyPending = IsDirectReceipt ? qtyPendingtoTrack : View.Model.CurQtyPending;
            trackControl.View.Model.Bin = View.Model.Bin;
            //trackControl.View.Model.QtyPerPack = qtyperPack; //Int32.Parse(View.TxtQtyPerPack.Text);
            //trackControl.View.Model.QtyToTrack = qtyToTrack; //Int32.Parse(View.TxtRcvQty.Text);
            trackControl.View.Model.TrackType = 0; //RECEIVING

            //Lista de Productos Disponibles
            try
            {
                /* IList<Product> productList = View.Model.DocumentBalance.Select(f => f.Product) //Where(f => f.QtyProcessed > 0)
                    .Where(f => f.ProductTrack != null && f.ProductTrack.Count > 0).ToList(); */

                IList<Product> productList = View.Model.DocumentLines.Select(f => f.Product) //Where(f => f.QtyProcessed > 0)
                    .Distinct().Where(f => f.ProductTrack != null && f.ProductTrack.Count > 0).ToList();


                //Se setea la lista de productos a los que se le hace tracking
                trackControl.View.UcProduct.DefaultList = new List<Product>();
                foreach (Product p in productList)
                    if (p.ProductTrack.Any(f => f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality))
                        trackControl.View.UcProduct.DefaultList.Add(p);

            }
            catch { }

            



            //Inicializa el componente de tracking.
            //trackControl.SetupManualTrackOption();


            //Si el ERP requiere de manera obligatoria los tracks habilita el boton
            //if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
                //View.BtnTrackReceive.IsEnabled = true;
        }

        /*
        //Envia los Labels (por receiveLabel) y el producto pendiente (recevieProduct) si remainin es > 0
        private void OnReceiveManualTrack(object sender, EventArgs e)
        {

            //variable que viene del control de Track Options
            IList<Label> trackLabelList = trackControl.View.Model.ManualTrackList; //Viene del control de track Options
            Int32 remainQty = trackControl.View.Model.RemainingQty;
            bool received = false;

           try
            {
                ProcessWindow("Receiving Product ...", false);

                View.BtnTrackReceive.IsEnabled = false;


               //Recibe los labels si los hay.
                if (trackLabelList != null && trackLabelList.Count > 0)
                {
                    Label curLabel;
                    //Funcion para obtener siguiente Label
                    DocumentTypeSequence initSequence = service.GetNextDocSequence(App.curCompany, trackLabelList[0].LabelType);

                    //Lo que sea Label lo manda a Crear y luego manda el Label for Receiving Label
                    foreach (Label label in trackLabelList)
                    {
                        label.LabelCode = (initSequence.NumSequence++).ToString();
                        label.ReceivingDate = DateTime.Now;
                        curLabel = service.SaveLabel(label); //Save label
                        service.ReceiveLabel(View.Model.Document, curLabel, View.Model.Bin, View.Model.Node); //Receive Label
                    }

                    initSequence.NumSequence++;
                    service.UpdateDocumentTypeSequence(initSequence);
                    received = true;
                }


                //Lo remaining lo manda Normal
                if (remainQty > 0)
                {
                    ProcessReceivingProduct(View.Model.Bin, remainQty);
                    received = true;
                    //pw.Close();
                }


                RefreshBalance(View.Model.Document);

                //After Process
                AfterReceiving();

                pw.Close();

                if (received)
                {
                    View.ProcessResult.Text = "Product Received.";

                    if (showNextTime)
                        showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);

                    //Util.ShowMessage(View.ProcessResult.Text);
                }

                //Reset the Track
                trackControl.View.Model.ManualTrackList = null;
                trackControl.View.Model.TrackData = null;
                trackControl.View.ManualTrackList.Items.Refresh();

                //Go to Pedir Producto.
                View.TabItemReceive.IsSelected = true;
                //After Process
                AfterReceiving();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error receiving. " + ex.Message);
                return;
            }

        }
        */

        private void AfterReceiving()
        {
            if (View.Model.Document.IsFromErp != true)
            {
                //View.ComboProduct.SelectedIndex = -1;
                //else
                //{
                View.Model.DocProducts = null;
                View.Model.VendorItem = "";
                //View.ComboProduct.Items.Refresh();
            }

            View.Model.ProductUnits = null;
            View.Model.PackingUnits = null;
            View.ComboUnit.Items.Refresh();
            View.LogisticUnit.Items.Refresh();
            View.TxtRcvQty.Text = View.TxtProduct.Text = View.TxtQtyPerPack.Text = "";
            View.Model.PackUnit = null;
            View.Model.Product = null;

            View.TxtProduct.ProductDesc = "";
            View.TxtProduct.Product = null;
            View.TxtProduct.DataList = null;

            View.Model.VendorItem = "";
            View.TabProductInfo.Visibility = Visibility.Collapsed;

            //Update The Default List
            View.TxtProduct.DefaultList = null;
            if (View.Model.Document.IsFromErp == true)
                View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0)
                    .Select(f => f.Product).ToList();


            //Return to Zero
            View.Model.RetDamage = 0;
            View.Model.RetOnHnd = 0;
            View.Model.RetTotal = 0;

        }


        //Carga las lineas de los recibos posteados en el ERP
        private void OnLoadPostedReceipt(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            LoadPostedReceipt(e.Value);

        }


        private void LoadPostedReceipt(Document document)
        {
            try
            {
                View.Model.PostedReceipt = document;
                View.StkReceiptData.Visibility = Visibility.Visible;

                View.Model.ReceiptData = Util.ToShowData(document);

                //Actualizando datos del Documento requeridos para posibles transacciones
                document.ModifiedBy = View.Model.User.UserName;

                //Calling Service
                View.Model.ReceiptLines = service.GetDocumentLine(new DocumentLine { Document = document });

                View.DgReceiptLines.Visibility = View.BtnReversePosted.Visibility = Visibility.Visible;

                if (View.Model.ReceiptLines == null || View.Model.ReceiptLines.Count == 0)
                {
                    View.DgReceiptLines.Visibility = View.BtnReversePosted.Visibility = Visibility.Collapsed;
                }

                //si el documento esta cancelado no muetsra el boton de reverse
                if (document.DocStatus.StatusID == DocStatus.Cancelled || document.DocStatus.StatusID == DocStatus.Posted || document.DocType.DocTypeID != SDocType.PurchaseReceipt)
                    View.BtnReversePosted.Visibility = Visibility.Collapsed;

                // CAA  [2010/05/03]
                if (document.DocType.DocTypeID == SDocType.PurchaseReceipt)
                    View.BtnViewPR.Visibility = Visibility.Visible ;
                else
                    View.BtnViewPR.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("RECEIPT_COULD_NOT_BE_LOADED") + "\n" + ex.Message);

            }
        }


        private void OnReversePosted(object sender, DataEventArgs<string> e)
        {
            try
            {
                //Check for Cross Dock.
                if (CheckReceiptCrossDock(View.Model.PostedReceipt))
                {
                    if (!UtilWindow.ConfirmOK(WmsSetupValues.Confirm_ReverseReceipt_CrossDock) == true)
                        return;
                }


                ProcessWindow(Util.GetResourceLanguage("REVERSING_RECEIPT"), false);

                View.Model.PostedReceipt.Comment += "\n"+e.Value;
                View.Model.PostedReceipt.ModDate = DateTime.Now;
                View.Model.PostedReceipt.ModifiedBy = App.curUser.UserName;
                service.ReversePurchaseReceipt(View.Model.PostedReceipt);                

                View.BtnReversePosted.Visibility = Visibility.Collapsed;

                pw.Close();
                RefreshReceipts();
                Util.ShowMessage(Util.GetResourceLanguage("RECEIPT") + " " + View.Model.PostedReceipt.DocNumber + Util.GetResourceLanguage("WAS_CANCELLED"));
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("RECEIPT_COULD_NOT_BE_CANCELLED") + "\n" + ex.Message);
            }
        }


        private bool CheckReceiptCrossDock(Document document)
        {
            TaskDocumentRelation taskRel = new TaskDocumentRelation
            {
                IncludedDoc = document,
                TaskDoc = new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } }
            };

            try
            {
                IList<TaskDocumentRelation> listTask = service.GetTaskDocumentRelation(taskRel)
                    .Where(f => f.TaskDoc.DocStatus.StatusID != DocStatus.Cancelled).ToList();
                if (listTask != null && listTask.Count > 0)
                    return true;
            }
            catch { }

            return false;

        }


        private void OnShowReceivingTicket(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"), false);
                UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.Document.DocID, "", false); //"PDF995"
                pw.Close();
            }
            catch { pw.Close(); }
        }

        // CAA [2010/05/03]
        private void OnShowPurchaseReceive(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"), false);
                UtilWindow.ShowDocument(View.Model.PostedReceipt.DocType.Template, View.Model.PostedReceipt.DocID, "", false); //"PDF995"
                pw.Close();
            }
            catch { pw.Close(); }
        }

        private void OnGoToPrintLabels(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow(Util.GetResourceLanguage("LOADING_PRINTING_OPTION"), false);

                IPrintingPresenter presenter = container.Resolve<PrintingPresenter>();
                presenter.LoadDocument(View.Model.Document, true, PrintSessionLot);

                InternalWindow window = Util.GetInternalWindow(this.Window.Parent, "Print Labels");
                presenter.Window = window;
                window.GridContent.Children.Add((PrintingView)presenter.View);
                window.Show();

                PrintSessionLot = "";
                pw.Close();
            }
            catch { pw.Close(); }
        }


        private void OnRemoveFromNode(object sender, DataEventArgs<DocumentBalance> e)
        {

            if (e.Value == null)
                return;


            DocumentBalance balanceLine = e.Value;
            bool refreshBalance = ((DataGridControl)sender).Name == "dgDocumentBalance" ? true : false;

            if (((DataGridControl)sender).Name == "dgPostingBalance" && balanceLine.QtyPending.Equals(0))
            {
                Util.ShowMessage(Util.GetResourceLanguage("NONE_LABELS_AVAILABLE"));
                return;
            }


            if (((DataGridControl)sender).Name == "dgDocumentBalance" && balanceLine.QtyProcessed.Equals(0))
            {
                Util.ShowMessage(Util.GetResourceLanguage("NONE_LABELS_AVAILABLE"));
                return;
            }
          

            try
            {
                IRemoveNodePresenter presenter = container.Resolve<IRemoveNodePresenter>();
                presenter.ParamRecord(balanceLine, this, refreshBalance); //refresBalance=true hace que traiga de nuevo el balance


                View.UcReceivedBal.ShowViewInShell(presenter.View);
                View.PopupReceived.IsOpen = true;
                View.PopupReceived.StaysOpen = true;
            }
            catch (Exception ex)
            {
                View.PopupReceived.IsOpen = false;
                View.PopupReceived.StaysOpen = false;
                Util.ShowError(ex.Message);
            }

        }


        private void OnAssignBinToProduct(object sender, DataEventArgs<string> e)
        {

            Bin assignLocation;

            try
            { assignLocation = service.GetBin(new Bin { BinCode = e.Value, Location = App.curLocation }).First(); }
            catch { assignLocation = null; }

            if (assignLocation == null)
            {
                Util.ShowError(Util.GetResourceLanguage("BIN") + " " + e.Value + Util.GetResourceLanguage("IS_NOT_VALID"));
                return;
            }

            //Assign Bin to Product
            try {
                assignLocation.ModifiedBy = App.curUser.UserName;
                int binDirection = View.CboBinDirection.SelectedItem == null ? 0 : (int)View.CboBinDirection.SelectedValue; //int.Parse(((DictionaryEntry)View.CboBinDirection.SelectedItem).Key.ToString()); //


                ZoneBinRelation zonBin = new ZoneBinRelation
                {
                    Bin = assignLocation,
                    BinType = (short)binDirection,
                    CreatedBy = App.curUser.UserName,
                };

                //try { zonBin.MinUnitCapacity = Double.Parse(View.MinStock.Text); }
                //catch { }
                //try { zonBin.UnitCapacity = Double.Parse(View.MinStock.Text); }
                //catch { }

                service.AssignBinToProduct(View.Model.Product, zonBin);
                ShowProductStock();
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("BIN_COULD_NOT_BE_ASSIGNED") + "\n" + ex.Message);
            }
        }


        //Febrero 23 2009 - Select Pack - JM
        private void OnSelectPack(object sender, DataEventArgs<Unit> e)
        {
            if (e.Value == null)
                return;

            View.Model.PackUnit = e.Value;

        }

        ////Febrero 23 2009 - Seleccion de Bins - JM
        //private void OnLoadBins(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.BinList = service.SearchBin(e.Value);
            
        //    if (View.Model.BinList == null || View.Model.BinList.Count == 0)
        //        return;

        //    //Cargar la lista de Bins
        //    View.CboBinList.Visibility = Visibility.Visible;
        //    View.CboBinList.IsDropDownOpen = true;

        //    if (View.Model.BinList.Count == 1)
        //    {
        //        View.CboBinList.Visibility = Visibility.Collapsed;
        //        View.BinLocation.Text = View.Model.BinList[0].BinCode;
        //        View.BinLocation.Focus();
        //    }
        //}


        private void OnGoToCrossDock(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow(Util.GetResourceLanguage("LOADING_CROSS_DOCK_OPTION"), false);

                //Lama al view que maneja el Cross Dock
                ICrossDockPresenter presenter = container.Resolve<CrossDockPresenter>();
                presenter.SetShowProcess(true);
                presenter.LoadDocument(View.Model.Document, View.Model.CrossDock);

                InternalWindow window = Util.GetInternalWindow(this.Window.Parent, "Cross Dock");
                presenter.Window = window;
                window.GridContent.Children.Add((CrossDockView)presenter.View);
                window.Show();

                pw.Close();

            }
            catch { pw.Close(); }
        }


        public void SetDocument(Document document)
        {
            //Load Document Calling Document come from other panel.
            LoadDocuments();
            LoadDetails(document);
        }


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore){
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }


        //07 - Marzo 2009 Check if was involved i a cross dock process
        private void CheckIfCrossDock()
        {
            View.StkCross.Visibility = Visibility.Collapsed;
            View.Model.CrossDock = null;

            //Revisar si ya se realizo una tarea de crossdock sobre ses documento
            TaskDocumentRelation taskRel = new TaskDocumentRelation
            {
                IncludedDoc = View.Model.Document,
                TaskDoc = new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } }
            };

            IList<TaskDocumentRelation> listTask = service.GetTaskDocumentRelation(taskRel)
                .Where(f => f.TaskDoc.DocStatus.StatusID != DocStatus.Cancelled).ToList();
            if (listTask != null && listTask.Count > 0)
            {
                View.StkCross.Visibility = Visibility.Visible;
                View.Model.CrossDock = listTask[0].TaskDoc;
            }
        }

        //10 Octubre 2009
        //Procesa el Return de Mercancia.
        void View_ReceiveReturn(object sender, DataEventArgs<double> e)
        {
            if (View.Model.Product == null)
            {
                Util.ShowError(Util.GetResourceLanguage("NO_PRODUCT_SELECTED"));
                return;
            }

            if (e.Value <= 0)
            {
                Util.ShowError(Util.GetResourceLanguage("QUANTITY_NOT_IS_INVALID"));
                return;
            }


            ProcessWindow(Util.GetResourceLanguage("RECEIVING_PRODUCT"), false);

            try
            {
                IList<ProductStock> retProduct = new List<ProductStock>();

                //On Hand
                retProduct.Add(new ProductStock
                {
                    Bin = View.BinLocation.Bin,
                    Product = View.Model.Product,
                    Unit = (Unit)View.ComboUnit.SelectedItem,
                    Stock = View.Model.RetOnHnd
                });

                //Damage
                retProduct.Add(new ProductStock
                {
                    Bin = damageBin,
                    Product = View.Model.Product,
                    Unit = (Unit)View.ComboUnit.SelectedItem,
                    Stock = View.Model.RetDamage
                });



                //Recibo el Producto por Completo
                //Luego lo Muevo a los Bines especificos
                service.ReceiveReturn(View.Model.Document, retProduct, App.curUser, e.Value, View.Model.Node);


                //Refresh el Balance
                RefreshBalance(View.Model.Document);

                //After Process
                AfterReceiving();


                View.ProcessResult.Text = Util.GetResourceLanguage("PRODUCT_RECEIVED");
                //Util.ShowMessage(View.ProcessResult.Text);
                if (showNextTime)
                {
                    pw.Close();
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
                }

            
            }

            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
            finally {
                pw.Close();
            }

        }


        //Llama al Acknologement que envia un mail e imprime X labels de Aknologement Labels.
        void View_ReceiptAcknowledge(object sender, DataEventArgs<double> e)
        {
            ProcessWindow(Util.GetResourceLanguage("PROCESSING"), false);

            try
            {

                service.ReceiptAcknowledge(View.Model.Document, e.Value, App.curUser);
                Util.ShowMessage(Util.GetResourceLanguage("NOTIFICATION_FOR_PO") + View.Model.Document.DocNumber + Util.GetResourceLanguage("WAS_SENT"));

            }
            catch (Exception ex) {
                Util.ShowError(ex.Message);
            }
            finally
            {
                pw.Close();
            }
        }

    }
}
