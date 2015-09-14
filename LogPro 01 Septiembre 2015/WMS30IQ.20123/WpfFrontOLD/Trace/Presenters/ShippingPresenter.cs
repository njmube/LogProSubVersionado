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
using WMComposite.Regions;




namespace WpfFront.Presenters

{


    public interface IShippingPresenter
    {
        IShippingView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class ShippingPresenter : IShippingPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }
        //private IList<ProductStock> curBinStock { get; set; }


        private ITrackOptionShipPresenter trackControl;
        private bool IsDirectReceipt;
        //private bool AllPosted = true;
        private bool AllReceived = true;
        ProcessWindow pw = null;
        private bool showNextTime = true;
        private bool multipack;

        public IShippingView View { get; set; }
        DateTime refDate;
        DocumentType docType;
        public bool refreshBalance = false;
        public bool addRemoveLines = false;



        public ShippingPresenter(IUnityContainer container, IShippingView view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.region = region;
                this.service = new WMSServiceClient();
                View.Model = this.container.Resolve<ShippingModel>();

                //Event Delegate
                View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
                View.LoadDetails += new EventHandler<DataEventArgs<Document>>(this.OnLoadDetails);
                View.LoadUnits += new EventHandler<DataEventArgs<Product>>(this.OnLoadUnits);
                View.PickProduct += new EventHandler<EventArgs>(this.OnPickProduct);
                View.PickLabel += new EventHandler<DataEventArgs<string>>(this.OnPickingLabel);
                View.PickLabelList += new EventHandler<EventArgs>(this.OnShippingLabelList);
                View.PostShipment += new EventHandler<EventArgs>(this.OnFullFillOrder);
                View.ShipmentAtOnce += new EventHandler<EventArgs>(this.OnShipmentAtOnce);
                View.CreateEmptyPickTicket += new EventHandler<EventArgs>(this.OnCreateEmptyShipment);
                //View.LoadProducts += new EventHandler<DataEventArgs<string>>(this.OnLoadProducts);
                //View.LoadCustomers += new EventHandler<DataEventArgs<string>>(this.OnLoadCustomers);
                View.ChangeStatus += new EventHandler<EventArgs>(this.OnChangeStatus);
                View.LoadProductManualTrackOption += new EventHandler<EventArgs>(this.OnLoadProductManualTrackOption);
                //View.AddManualTrackToList += new EventHandler<EventArgs>(this.OnAddManualTrackToList);
                View.PickManualTrack += new EventHandler<EventArgs>(this.OnPickManualTrack);
                View.SelectedUnit += new EventHandler<EventArgs>(this.OnSelectUnit);
                View.RefreshBin += new EventHandler<EventArgs>(this.OnRefreshBin);
                View.LoadShipment += new EventHandler<DataEventArgs<Document>>(this.OnLoadShipment);
                View.ReversePosted += new EventHandler<DataEventArgs<string>>(this.OnReversePosted);
                View.LateDocuments += new EventHandler<DataEventArgs<bool?>>(this.OnLateDocuments);
                View.ShowPickingTicket+= new EventHandler<EventArgs>(this.OnShowPickingTicket);

                View.RemoveFromNode += new EventHandler<DataEventArgs<DocumentBalance>>(this.OnRemoveFromNode);
                View.RefreshShipments += new EventHandler<EventArgs>(View_RefreshShipments);
                View.ShowPackingList += new EventHandler<DataEventArgs<Document>>(View_ShowPackingList);
                View.UpdatePackages += new EventHandler<EventArgs>(View_UpdatePackages);
                View.PreviewPackLabels += new EventHandler<DataEventArgs<Document>>(View_PreviewPackLabels);
                View.PrintPackLabels += new EventHandler<DataEventArgs<Document>>(View_PrintPackLabels);
                View.ShowPackageAdmin += new EventHandler<DataEventArgs<Document>>(View_ShowPackageAdmin);
                View.RefreshPacks += new EventHandler<EventArgs>(View_RefreshPacks);
                View.PrintTicketInBatch += new EventHandler<EventArgs>(View_PrintTicketInBatch);
                View.CheckBalance += new EventHandler<EventArgs>(View_CheckBalance);

                View.ShowOnlyMerged += new EventHandler<EventArgs>(View_ShowOnlyMerged);
                View.UnShowOnlyMerged += new EventHandler<EventArgs>(View_UnShowOnlyMerged);
                View.CancelLine += new EventHandler<DataEventArgs<DocumentLine>>(View_CancelLine);

                View.RemoveFromShipmentLine += new EventHandler<DataEventArgs<DocumentLine>>(View_RemoveFromShipmentLine);

                View.ReFullFilOrder += new EventHandler<EventArgs>(View_ReFullFilOrder);
                View.LoadPopupLine += new EventHandler<DataEventArgs<DocumentLine>>(View_LoadPopupLine);
                View.ConfirmPicking += new EventHandler<EventArgs>(View_ConfirmPicking);


                LoadEnvironment();

                LoadDocuments();                



            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_LOADING_VIEW") + "\n" + ex.Message);
            }
        }



        void View_ConfirmPicking(object sender, EventArgs e)
        {
            try
            {

                bool? reason = UtilWindow.ConfirmOK(Util.GetResourceLanguage("PLE_REV_THE_PICK_BAL_BEF_OF_THE_PICK_CONF_WIS_YOU_CONT"));

                if (reason == false)
                    return;


                //Ejecuta el fullfil del Shipment.
                ProcessWindow(Util.GetResourceLanguage("PROCESSING"), false);
                service.ConfirmPicking(View.Model.Document, App.curUser.UserName);
                pw.Close();

                View.BtnConfirmPicking.IsEnabled = false;
                Util.ShowMessage(Util.GetResourceLanguage("PICKING_FOR") + View.Model.Document.DocNumber + Util.GetResourceLanguage("CONFIRMED"));

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_PROCESSED") + "\n" + ex.Message);
            }
        }


        void View_ReFullFilOrder(object sender, EventArgs e)
        {
            try
            {
                //Ejecuta el fullfil del Shipment.
                ProcessWindow(Util.GetResourceLanguage("PROCESSING"), false);
                service.FullfilSalesOrder(View.Model.PostedShipment);
                pw.Close();

                Util.ShowMessage(Util.GetResourceLanguage("DOCUMENT") + " " + View.Model.Document.DocNumber + Util.GetResourceLanguage("WAS_FULLFILLED"));

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_PROCESSED") + "\n" + ex.Message);
            }
        }



        void View_RemoveFromShipmentLine(object sender, DataEventArgs<DocumentLine> e)
        {
            //Debe abrir el Remove From Node Igual que para remover de node picking
            if (e.Value.Document.DocStatus.StatusID != DocStatus.Completed)
                return;

            try
            {
                DocumentBalance shipmentLine = new DocumentBalance
                {
                    Document = e.Value.Document,
                    Product = e.Value.Product,
                    Location = e.Value.Document.Location,
                    Node = new Node { NodeID = NodeType.Picked }, //Producto Pickeado y Posteado en un Shipment
                    DocumentLine = new DocumentLine { LineNumber = e.Value.LineNumber } //Indica que fue posteado
                };

                IRemoveNodePresenter presenter = container.Resolve<RemoveNodePresenter>();
                presenter.ParamRecord(shipmentLine, this, false); //refresBalance=true hace que traiga de nuevo el balance


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



        private void LoadEnvironment()
        {
            //Si  hay conexion a ERP se habilita el panel de posting
            //if (App.IsConnectedToErpShipping)
            View.PanelPosting.Visibility = Visibility.Visible;
            View.BtnCreateShipment.IsEnabled = true;


            //If use Merged
            docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };
            if (Util.GetConfigOption("USEMERGED").Equals("T"))
            {
                View.ChkUseMerged.Visibility = Visibility.Visible;
                docType = new DocumentType
                {
                    DocClass = new DocumentClass { DocClassID = SDocClass.Shipping },
                    DocTypeID = SDocType.MergedSalesOrder
                };
            }

            //MultiPacking
            if (Util.GetConfigOption("MULTIPACK").Equals("T"))
            {
                View.Model.ShowMultiPack = Visibility.Visible;
                View.Model.CurPackage = new Label();
                multipack = true;
            }
            else
            {
                View.Model.ShowMultiPack = Visibility.Collapsed;
                View.Model.CurPackage = new Label { LabelID = -1 };
                multipack = false;
            }


            //IF use packing version 1 or 2
            if (Util.GetConfigOption("PACKMANGR").Equals("2"))
                View.BtnCreateShipment.Content = Util.GetResourceLanguage("PACKING_AND_DOCUMENTS");


            //Add Remove Lines
            if (Util.AllowOption("REMSOLINE"))
                addRemoveLines = true;

        }




        void View_UnShowOnlyMerged(object sender, EventArgs e)
        {
            docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };
            LoadDocuments();
        }


        void View_ShowOnlyMerged(object sender, EventArgs e)
        {
            docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping },
            DocTypeID = SDocType.MergedSalesOrder };
            LoadDocuments();
        }



        void View_CheckBalance(object sender, EventArgs e)
        {
            if (refreshBalance)
            {
                refreshBalance = false;

                ProcessWindow(Util.GetResourceLanguage("REFRESHING_DOCUMENT"), false);

                RefreshBalance(View.Model.Document);
                AfterReceiving();
                

                pw.Close();
            }

        }





        void View_RefreshPacks(object sender, EventArgs e)
        {

            if (Util.GetConfigOption("PACKMANGR").Equals("2"))
            {

                View.Model.DocPackages = service.GetDocumentPackage(new DocumentPackage { PostingDocument = View.Model.PostedShipment })
                    .Where(f => f.Sequence > 1 || f.ChildPackages.Count > 0).ToList();

            }
            else
                View.Model.DocPackages = service.GetDocumentPackage(new DocumentPackage { PostingDocument = View.Model.PostedShipment });


            View.LvPacks.Items.Refresh();
        }




        void View_ShowPackageAdmin(object sender, DataEventArgs<Document> e)
        {
            OpenPackingControl(View.Model.Document, e.Value);
        }



        void View_PreviewPackLabels(object sender, DataEventArgs<Document> e)
        
        {
            if (e.Value == null)
                return;

            //Open View Packages
            LabelTemplate template = null;
            try { template = service.GetLabelTemplate(new LabelTemplate { 
                Header = WmsSetupValues.DefaultPackLabelTemplate }).First(); }
            catch {
                Util.ShowError(Util.GetResourceLanguage("NO_PACKING_LABEL_DEFINED"));
                return;
            }


            ProcessWindow(Util.GetResourceLanguage("LOADING_PRINTING_OPTION"), false);

            try
            {
                IList<Label> listOfPacks = View.Model.DocPackages.Select(f => f.PackLabel).ToList();

                UtilWindow.ShowLabelsToPrint(template, true, listOfPacks); //163 to Test
            }
            catch { }

            pw.Close();

            

        }


        void View_PrintPackLabels(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            Util.PrintShipmentPackLabels(e.Value);


        }



        void View_UpdatePackages(object sender, EventArgs e)
        {
            try
            {
                //Actualiza el Valor de los Packages, Weight and Dimension
                foreach (DocumentPackage pk in View.Model.DocPackages)
                {
                    pk.ModDate = DateTime.Now;
                    pk.ModifiedBy = App.curUser.UserName;
                    service.UpdateDocumentPackage(pk);
                    
                }

                Util.ShowMessage(Util.GetResourceLanguage("PACKAGES_UPDATED"));
            }
            catch (Exception ex) {
                Util.ShowError(Util.GetResourceLanguage("PACKAGES_COULD_NOT_BE_UPDATED") + "\n" + ex.Message);  
            }
        }






        private void LoadDocuments()
        {
            try
            {

                ProcessWindow(Util.GetResourceLanguage("LOADING_DOCUMENTS"), false);


                View.DgDocument.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 330;                
                
                //Load the Base Documents (Pendig pero sin fecha de referencia)
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation  }, 0, 0)
                    //.OrderBy(f=>f.Priority)
                    .ToList();

                //Loading Pending Docs
                //View.ExpPendingDocs.Visibility = Visibility.Collapsed;

                refDate = DateTime.Now;
                //Pending with reference Date = NOW
                //View.Model.PendingDocumentList = service.GetPendingDocument(new Document { DocType = docType, Date1 = refDate });
                //View.Model.PendingDocumentList = View.Model.DocumentList.Where(f=>f.Date2 <= refDate).ToList();
                
                //if (View.Model.PendingDocumentList != null && View.Model.PendingDocumentList.Count > 0)
                //    View.ExpPendingDocs.Visibility = Visibility.Visible;

                //Set Node to Shipping Node.
                View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Picked }).First();


                //LoadStatus 
                View.Model.DocStatus = service.GetStatus(new Status { StatusType = new StatusType { StatusTypeID = SStatusType.Document } });

                View.ProcessResult.Text = "";
                //View.ExpManual.IsExpanded = false;

                pw.Close();
            }
            catch (Exception ex) {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }


        private void OnLateDocuments(object sender, DataEventArgs<bool?> e)
        {
            ProcessWindow(Util.GetResourceLanguage("LOADING_LATE_DOCUMENTS"), false);


            if (e.Value == true)
                //Pending with reference Date = NOW
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation  },0,0).Where(f => f.Date2 <= refDate)
                    //.OrderBy(f=>f.Priority)
                    .ToList();

            else
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany },0,0)
                    //.OrderBy(f => f.Priority)
                    .ToList();

            View.DgDocument.Items.Refresh();

            pw.Close();
        }



        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {


                if (string.IsNullOrEmpty(e.Value))
                {
                    LoadDocuments();
                    return;
                }

                ProcessWindow(Util.GetResourceLanguage("SEARCHING"), false);

                View.Model.DocumentList = service.SearchDocument(e.Value, docType)
                    //.OrderBy(f => f.Priority)
                    .ToList();

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
            catch { pw.Close(); }
        }


        private void ResetDetails()
        {
            View.Model.DocumentData = null;
            View.Model.DocumentLines = null;
            View.Model.DocProducts = null;
            View.Model.LabelsAvailable = null;
            View.Model.DocumentBalance = null;
            View.TabShipping.Visibility = Visibility.Collapsed;
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

                //Mesaje de que todo fue piqueado
                //if (AllReceived == true && View.Model.Document.IsFromErp == true)
                //    Util.ShowMessage("This document does not have quantities pending to pick.");

            }
            catch (Exception ex) {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_LOADED") + "\n" + ex.Message);
            }

        }


        private void LoadDetails(Document document)
        {

            //Actualizando datos del Documento requeridos para posibles transacciones
            document.ModifiedBy = View.Model.User.UserName;
            document.Location = App.curLocation;

            View.BtnConfirmPicking.IsEnabled = true;

            //Reseting Form Variable for the new Document
            View.Model.CurQtyPending = 0;
            View.BinLocation.Text = "";
            View.TxtProduct.Text = "";
            View.TxtProduct.Product = null;
            View.TxtProduct.ProductDesc = "";
            View.TxtProduct.DataList = null;
            View.TxtProduct.DefaultList = null;
            View.TabShipping.Visibility = Visibility.Visible;
            View.ProcessResult.Text = "";

            //Adden On ENE 13 /2010
            View.Model.ProductUnits = null; 
            View.ComboUnit.Items.Refresh();
            View.TxtRcvQty.Text = "0";
            View.TxtProduct.Text = "";
            View.Model.Product = null;

            //Hidden Tracking y Others
            View.BtnPick.Visibility = Visibility.Visible;
            View.TxtProductTrackMsg.Visibility = View.BtnTrack.Visibility = Visibility.Collapsed;
            View.TabItemTrackOption.Visibility = Visibility.Collapsed;

            //OCultar Add Line
            


            View.Model.Document = document;
            View.Model.DocumentData = Util.ToShowData(document);

            //DIRECT PRINT 30oct2009
            View.DirectPrint.Document = View.Model.Document;
            if (View.Model.Document.DocStatus.StatusID == DocStatus.New)
                View.DirectPrint.NewStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.InProcess).First();
            
            //Document Lines
            View.Model.DocumentLines = service.GetDocumentLine(new DocumentLine { Document = document });

            View.ExpDocLines.IsExpanded = true;
            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
                View.ExpDocLines.IsExpanded = false;

            //Select Status
            View.ComboStatus.SelectedValue = View.Model.Document.DocStatus.StatusID;


            RefreshBalance(document);            


            //Cargue del tab de recibo, solo si el documento es new or in process   

            View.TabItemShip.Visibility = Visibility.Collapsed;


            //Si no esta en receiving vuelve al tab de detalles
            View.TabShipping.SelectedIndex = View.TabShipping.SelectedIndex > 1 ? 0 : View.TabShipping.SelectedIndex;  


            if (document.DocStatus.StatusID == DocStatus.InProcess || document.DocStatus.StatusID == DocStatus.New || document.DocStatus.StatusID == DocStatus.Completed)
            {
                View.TabItemShip.Visibility = Visibility.Visible;

                //Lista de Productos
                View.Model.DocProducts = View.Model.DocumentBalance.Where(f => f.QtyPending > 0)
                    .Select(f => f.Product).ToList();

                foreach (DocumentLine dline in View.Model.DocumentLines.Where(f => f.Note == "1"))
                    View.Model.DocProducts.Remove(dline.Product);

                //Setea para que el Control de producto solo muestre el balance.
                if (View.Model.Document.IsFromErp == true)
                    View.TxtProduct.DefaultList = View.Model.DocProducts;

                if (AllReceived == false)
                    ShowLabelsAvailable(document);

                View.BtnShipLabel.Visibility = Visibility.Collapsed;

                if (View.Model.LabelsAvailable != null && View.Model.LabelsAvailable.Count > 0)
                    View.BtnShipLabel.Visibility = Visibility.Visible;

                //lista de packages si permite multipack
                LoadPackages();

            }

            //Getting the product bin Stock (For all product in the document)
            //curBinStock = service.GetDocumentStock(View.Model.Document, false);



            if (View.TabItemShip.Visibility != Visibility.Visible && View.TabShipping.SelectedIndex > 0)
                View.TabShipping.SelectedIndex = 0;


            //Reset Invoices


            // CAA [2010/05/04]
            // Si tiene prods kits de Caterpillar, se activa botón para actualizar info en ERP
            if (View.Model.DocumentLines.Where(f => f.Product.Category.ExplodeKit == ExplodeKit.Caterpillar
                || f.Product.Category.ExplodeKit == ExplodeKit.CaterpillarKit).Count() > 0)
                View.BtnConfirmPicking.Visibility = Visibility.Visible;
            else
                View.BtnConfirmPicking.Visibility = Visibility.Collapsed;

            //Botones de Actualizacion de Lineas.
            View.StkUpdLines.Visibility = Visibility.Collapsed;
            if (addRemoveLines 
                && (document.DocStatus.StatusID == DocStatus.InProcess || document.DocStatus.StatusID == DocStatus.New)
                && (document.DocType.DocTypeID == SDocType.MergedSalesOrder || document.DocType.DocTypeID == SDocType.SalesOrder))
                View.StkUpdLines.Visibility = Visibility.Visible;


                        //Si es un MSO y tienen lineas canceladas por haber sido deleted in GP
            View.TxtWarning.Visibility = Visibility.Collapsed;
            if (document.DocType.DocTypeID == SDocType.MergedSalesOrder)
            {
                if (View.Model.DocumentLines.Any(f => f.LineStatus.StatusID == DocStatus.Cancelled &&
                    f.BinAffected == "DEL"))
                {
                    //Util.ShowError("Some order(s) used to create this merged order were deleted in the ERP.\nPlease review the order quantities and the picked quantities.");
                    View.TxtWarning.Text = Util.GetResourceLanguage("SOME_ORD_USE_CRE_THIS_MER_ORD_WERE_DEL_IN_ERP");
                    View.TxtWarning.Visibility = Visibility.Visible;
                }
            }

        }




        private void LoadPackages()
        {
            View.Model.OpenPackages = null;

            if (multipack)
            {
                View.Model.OpenPackages = service.GetDocumentPackage(
                    new DocumentPackage { 
                        Document = new Document { DocID = View.Model.Document.DocID },
                        PostingDocument = new Document { DocID = -1 }})
                    .Where(f=>f.PackLabel.Status.StatusID == EntityStatus.Active).ToList();
                //Obtiene solo los packages de labels activos.

                View.Model.OpenPackages.Add(new DocumentPackage { PackDesc = "New Package ...", PackLabel = new Label () });
                View.CboPack.Items.Refresh();
            }
        }



        private void ShowLabelsAvailable(Document document) {

            /*
            //Gettting Bin Location
            if (!string.IsNullOrEmpty(View.BinLocation.Text))
                View.Model.BinLabel = CheckDestination(View.BinLocation.Text);

            Label searchLabel = new Label { Node = new Node { NodeID = NodeType.Stored }, Status = new Status {  StatusID = EntityStatus.Active} };

            if (View.Model.BinLabel != null && View.Model.BinLabel.Bin != null)
                searchLabel.Bin = View.Model.BinLabel.Bin;
            else
                searchLabel.Bin = new Bin { Location = App.curLocation, BinCode = DefaultBin.MAIN };


            // Mustra posibles labels si es fromERP, si no muestra todo lo disponible en node PreLabeled
            if (document.IsFromErp == true)
                View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(document, searchLabel);
            else
                View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(new Document { Location = App.curLocation }, searchLabel);
            */
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

            View.BtnPick.Visibility = Visibility.Visible;

            //if (View.Model.Document.IsFromErp == true)
            //{
                //Debe traer solo la unidad requerida por el documento para ese producto
                View.Model.ProductUnits = View.Model.DocumentLines.Where(f => f.Product.ProductID == product.ProductID)
                        .Select(f => f.Unit).Distinct().ToList();
            //}
            //else
                //View.Model.ProductUnits = product.ProductUnits.Select(f => f.Unit).OrderBy(f=>f.BaseAmount).Take(1).ToList();


            if (View.Model.ProductUnits != null && View.Model.ProductUnits.Count == 1)
            {
                View.ComboUnit.SelectedIndex = 0;                
                View.TxtRcvQty.Focus();
                SelectUnit();
            }


            //Asignacion al producto del modelo
            View.Model.Product = product;


            //Si Tiene trackOption se muestran
            View.TxtProductTrackMsg.Visibility = View.BtnTrack.Visibility = Visibility.Collapsed;
            if (product.ProductTrack != null && product.ProductTrack.Count > 0 &&
                    product.ProductTrack.Where(f => f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).Count() > 0)
            {
                View.TxtProductTrackMsg.Visibility = View.BtnTrack.Visibility = Visibility.Visible;
                if (Util.GetConfigOption("SHERPTRACK").Equals("T"))
                    View.BtnPick.Visibility = Visibility.Collapsed;

                //Load the Shipping TAB
                //AfterReceiving();
                //LoadTrackControl(); //Cargar el control de tracking
                //View.TabShipping.SelectedIndex = 2;

            }


            //Show Product Stock
            View.LvStock.Visibility = Visibility.Collapsed;

            //Obteniendo el Stock del Producto
            View.Model.ProductBinStock = service.GetProductStock(
                new ProductStock { Product = View.Model.Product, 
                    Bin = new Bin { Location = App.curLocation } }, 
              View.Model.Document.PickMethod);

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


            try
            {
                //Poner el Qty Pending para esa unidad
                View.Model.CurQtyPending = int.Parse(View.Model.DocumentBalance
                    .Where(f => f.Product.ProductID == View.Model.Product.ProductID && f.Unit.UnitID == ((Unit)View.ComboUnit.SelectedItem).UnitID)
                    .First().QtyPending.ToString());
            }
            catch { View.Model.CurQtyPending = 0; }

            View.TxtRcvQty.Text = View.Model.CurQtyPending.ToString();


            View.Model.AllowReceive = true;
            if (View.Model.Document.IsFromErp == true)
                View.Model.AllowReceive = (View.Model.CurQtyPending > 0) ? true : false;

        }

        private void OnPickProduct(object sender, EventArgs e)
        {
            try
            {
                View.ProcessResult.Text = "";

                Label sourceLocation = CheckDestination(View.BinLocation.Text);

                if (sourceLocation == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("BIN_SOURCE") + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
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

                if (!(int.Parse(View.TxtRcvQty.Text) > 0))
                {
                    Util.ShowError(Util.GetResourceLanguage("QUANTITY_IS_NOT_VALID"));
                    return;
                }

                if (multipack && View.CboPack.SelectedIndex == -1)
                {
                    Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_PACKAGE"));
                    return;
                }

                ProcessWindow(Util.GetResourceLanguage("PICKING_PRODUCT"), false);
                ProcessPickingProduct(sourceLocation, int.Parse(View.TxtRcvQty.Text));
                pw.Close();


            }
            catch (Exception ex) {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }


        private void ProcessPickingProduct(Label sourceLocation, int quantiy)
        {
            //Define Document, Product, Unit and Qty to send to receiving transaction
            DocumentLine line = new DocumentLine
            {
                Document = View.Model.Document,
                Product = View.TxtProduct.Product,
                Unit = (Unit)View.ComboUnit.SelectedItem,
                Quantity = quantiy,
                CreatedBy = App.curUser.UserName         
            };


            if (multipack)
                View.Model.CurPackage = ((DocumentPackage)View.CboPack.SelectedItem).PackLabel;

            //Lamando al servicio
            service.PickProduct(line, sourceLocation, View.Model.Node, View.Model.CurPackage);

            //Refresh el Balance
            RefreshBalance(line.Document);

            //After Process
            AfterReceiving();

            pw.Close();
            View.ProcessResult.Text = Util.GetResourceLanguage("PRODUCT_PICKED");
            //Util.ShowMessage(View.ProcessResult.Text);
            if (showNextTime)            
                showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
            
        }


        private void OnPickingLabel(object sender, DataEventArgs<string> e)
        {
            View.ProcessResult.Text = "";

            //Ocultar el Tracking Options
            View.Model.CurScanedLabel = null;


            if (string.IsNullOrEmpty(e.Value.Trim()))
                return;


            Label label = new Label { LabelCode = e.Value };

            ProcessWindow(Util.GetResourceLanguage("PICKING_LABEL"), false);


            //Trae el label
            try
            {
                label = service.GetLabel(label).First();
                View.Model.CurScanedLabel = label;
            }
            catch
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("LABEL") + " " + e.Value + Util.GetResourceLanguage("DOES_NOT_EXISTS"));
                return;
            }


            try
            {
                service.PickLabel(View.Model.Document, label, View.Model.Node, View.Model.CurPackage);

                //Label borra si esta enum label lista de desplegadas
                  if (View.Model.LabelsAvailable != null 
                    && View.Model.LabelsAvailable.Where(f => f.LabelID == label.LabelID).Count() > 0)
                {
                    View.Model.LabelsAvailable.Remove(View.Model.LabelsAvailable.Where(f => f.LabelID == label.LabelID).First());
                    View.LabelListAvailable.Items.Refresh();
                }
                
                //Post - Process
                //Refresh el Balance
                RefreshBalance(View.Model.Document);
                View.TxtScanLabel.Text = "";
                View.ProcessResult.Text = Util.GetResourceLanguage("LABEL_PICKED");
                
                
                //Util.ShowMessage(View.ProcessResult.Text);
                pw.Close();
                if (showNextTime)
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);

                return;
            }

            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
                return;
            }

        }


        //Recibe Varios lables de los disponibles
        private void OnShippingLabelList(object sender, EventArgs e)
        {
            if (!(View.LabelListAvailable.SelectedItems != null
                && View.LabelListAvailable.SelectedItems.Count > 0))
            {
                Util.ShowError(Util.GetResourceLanguage("NO_LABEL_SELECTED"));
                return;
            }

            try
            {


                View.ProcessResult.Text = "";

                ProcessWindow(Util.GetResourceLanguage("PICKING_LABELS"), false);

                //recorre la lista para enviarla 
                foreach (object label in View.LabelListAvailable.SelectedItems)
                {
                    service.PickLabel(View.Model.Document, (Label)label, View.Model.Node, View.Model.CurPackage);
                    View.Model.LabelsAvailable.Remove((Label)label);
                }

                //Update Process Result
                View.LabelListAvailable.Items.Refresh();
                ShowLabelsAvailable(View.Model.Document);

                //Refresh el Balancee
                RefreshBalance(View.Model.Document);
                View.ProcessResult.Text = Util.GetResourceLanguage("LABELS_PICKED");

                pw.Close();
                Util.ShowMessage(View.ProcessResult.Text);

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }


        private Label CheckDestination(string binLocation) {

            View.BinLocation.Text = View.BinLocation.Text.ToUpper();

            //Checking if valid source location
            if (string.IsNullOrEmpty(View.BinLocation.Text.Trim()))            
                return null;
            

            try
            { return service.GetLocationData(View.BinLocation.Text, false); }
            catch
            { return null; }
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

            //#########Shipping Balance
            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
                View.Model.DocumentBalance = service.GetDocumentBalanceForEmpty(docBalance);
            else 
                View.Model.DocumentBalance = service.GetDocumentBalance(docBalance, 
                    View.Model.Document.CrossDocking == true ? true : false);

            View.DgDocumentBalance.Items.Refresh();

            //El boton de Recibir todo se muestra si hay balance
            View.BtnShipAtOnce.IsEnabled = false;
            if (View.Model.DocumentBalance.Any(f => f.QtyPending > 0))
            {
                View.BtnShipAtOnce.IsEnabled = true;
                AllReceived = false;
            }


            //##########Posting Balance
            View.Model.PendingToPostList = service.GetDocumentPostingBalance(docBalance);
            View.DgPostingBalance.Items.Refresh();


            //El boton de Posting solo se muestra si hay balance
            //View.BtnCreateShipment.IsEnabled = false;
            View.StkPosting.Visibility = Visibility.Collapsed;
            View.PanelPosting.Visibility = Visibility.Collapsed;

            //if (View.Model.PendingToPostList.Any(f => f.QtyPending > 0))
            if (View.Model.PendingToPostList != null && View.Model.PendingToPostList.Count > 0)
            {
                View.BtnCreateShipment.IsEnabled = true;
                View.StkPosting.Visibility = Visibility.Visible;
                View.PanelPosting.Visibility = Visibility.Visible;
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


            //Marzo 21, si todo esta despachado y es un ERP documet cambia el estatus a Completed.
            //Si tiene Any Shipped cambia el status a In Process

            //Completed
            if (AllReceived && View.Model.Document.IsFromErp == true
                && View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0)  //garantiza que al menos uno este procesado
                && View.Model.Document.DocStatus.StatusID != DocStatus.Completed 
                //INgresada para evitar que ponga en completed lso que tienen BO
                && !View.Model.DocumentLines.Any(f=>f.QtyBackOrder > 0 && f.LinkDocLineNumber != -1)
                )
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
            if (View.Model.PendingToPostList != null && View.Model.PendingToPostList.Count > 0)
            {
                View.PanelPosting.Visibility = Visibility.Visible;
                View.BtnCreateShipment.IsEnabled = true;

                if (!AllReceived && Util.GetConfigOption("PARTIALSHI").Equals("F")
                    && View.Model.Document.AllowPartial != true)
                    View.PanelPosting.Visibility = Visibility.Collapsed;


                //PackManager
                if (Util.GetConfigOption("PACKMANGR").Equals("2"))
                    View.PanelPosting.Visibility = Visibility.Visible;
            }


            //Hidden Tracking y Others
            View.TabItemTrackOption.Visibility = Visibility.Collapsed;
            //Evaluar si la orden tiene producto para hacerle Tracking
            foreach (DocumentBalance db in View.Model.DocumentBalance)
            {
                if (db.Product.ProductTrack != null && db.Product.ProductTrack.Count > 0 && 
                    db.Product.ProductTrack.Where(f=>f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).Count() > 0 )
                //&& db.QtyProcessed > 0
                {
                    View.TabItemTrackOption.Visibility = Visibility.Visible;
                    LoadTrackOptionsControl();
                    break;
                }
            }


            //Ene 04 2010 - Revision use allcoation to change label
            View.DgDocumentBalance.Columns[3].Title = Util.GetResourceLanguage("ORDERED");
            if (View.Model.Document.UseAllocation == true)
                View.DgDocumentBalance.Columns[3].Title = Util.GetResourceLanguage("ALLOCATED");

            RefreshShipments();

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



        private void LoadTrackOptionsControl()
        //private void LoadTrackOptionsControl(int qtyPendingtoTrack, int qtyperPack, int qtyToTrack)
        {
         
            trackControl = container.Resolve<TrackOptionShipPresenter>();
            View.TrackOpts.Items.Clear();
            View.TrackOpts.Items.Add(trackControl.View);

            //Seting Values
            trackControl.View.Model.Document = View.Model.Document;
            trackControl.View.Model.Node = View.Model.Node;
            //trackControl.View.Model.Bin = View.Model.Bin;
            trackControl.View.Model.TrackType = 1; //PICKING
            trackControl.View.Model.FatherPresenter = this;

            //Lista de Productos Disponibles
            try
            {
                trackControl.View.UcProduct.Visibility = Visibility.Visible;
                trackControl.View.StkPrdDesc.Visibility = Visibility.Collapsed;

                trackControl.View.UcProduct.DefaultList = View.Model.DocumentBalance.Select(f => f.Product) //Where(f => f.QtyProcessed > 0)
                    .Where(f => f.ProductTrack != null && f.ProductTrack.Count > 0).ToList();
            }
            catch { }
           

        }



        public void RefreshShipments()
        {
            View.GrpShipments.Visibility = Visibility.Collapsed;
            View.Model.Shipments = service.GetDocument(
                new Document { CustPONumber = View.Model.Document.DocNumber,
                               DocType = new DocumentType
                               {
                                   DocClass = new DocumentClass
                                   {
                                       DocClassID = SDocClass.Posting
                                   }
                               }
                }
              );

            if (View.Model.Shipments != null && View.Model.Shipments.Count > 0)
            {
                View.GrpShipments.Visibility = Visibility.Visible;
                //LoadShipment(View.Model.Shipments[0]);
                View.LvShipment.SelectedIndex = 0;
            }
        }



        private void OnFullFillOrder(object sender, EventArgs e)
        {
            bool requestAuth = false;
            App.curAuthUser = "";

            // CAA [2010/05/25]
            // Valida si tiene el perfil para continuar; si no, pide User/password autorizado
            if (Util.GetConfigOption("REQAUTH").Equals("T"))
                //Adicion Image 
                //Revision del Random
                requestAuth = Util.ReviewOrderPerRandom(service) ||
                    Util.ReviewOrderPerOverAmount(service, View.Model.DocumentBalance
                    .Sum(f => f.BaseQtyProcessed * f.Product.ProductCost));


            if (requestAuth && !UtilWindow.AuthorizateWindow("CREASHIP"))
                return; //Se sale si requiere autorizacion y no se valida la ventana

            //Adignando el Checked by
            //View.Model.Document.UserDef3 = App.curAuthUser;

            if (View.Model.PendingToPostList != null && View.Model.PendingToPostList.Count > 0)
            {
                if (Util.GetConfigOption("PACKMANGR").Equals("2"))
                    OpenPackingControl(View.Model.Document, null);

                else
                    //Si hay algo que postear en un shipment
                    CreateShipment();
            }
            else
            {
                Util.ShowError(Util.GetResourceLanguage("NO_QUANTITIES_PENDING_TO_CREATE_A_SHIPMENT"));
                return;
            }

        }



        private void OpenPackingControl(Document document, Document shipment)
        {
            //PRIMERA Version del REORDER

            //Show Package Admin
            if (document == null && shipment == null)
                return;


            if (Util.GetConfigOption("PACKMANGR").Equals("2"))
            {
                try
                {
                    View.UcAdmPAckV2.LoadPackages(document, shipment);
                    View.UcAdmPAckV2.parentPresenter = this;
                    View.PopAdmPackV2.IsOpen = true;
                    View.PopAdmPackV2.StaysOpen = true;
                }
                catch (Exception ex)
                {
                    View.PopAdmPackV2.IsOpen = false;
                    View.PopAdmPackV2.StaysOpen = false;
                    Util.ShowError(ex.Message);
                }

                return;
            }


            try
            {
                IPackageAdminPresenter presenter = container.Resolve<PackageAdminPresenter>();
                presenter.View.Model.Document = shipment;
                presenter.LoadPackages();

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



        public void CreateShipment()
        {

            ProcessWindow(Util.GetResourceLanguage("CREATING_SALES_SHIPMENT"), false);


            Document salesDocToFill = View.Model.Document;
            salesDocToFill.Arrived = AllReceived;
            Document shipment = null;

            //Llamdo proceso de creacion del Shipment
            try
            {
                shipment = service.CreateShipmentDocument(salesDocToFill);
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("SHIPMENT_COULD_NOT_BE_CREATED") + "\n" + ex.Message);
                return;
            }


            try
            {
                ProcessWindow(Util.GetResourceLanguage("PRINTING_LEGAL_DOCUMENTS_PACKAGE_LABELS"), true);


                //Print Package Labels       
                if (Util.GetConfigOption("PRPACKLBL").Equals("T"))
                    service.PrintPackageLabels(shipment);


                //Print Shipping Documents (Packing Slip, BOL etc)
                if (Util.GetConfigOption("PRSHPDOCS").Equals("T"))
                {
                    CustomProcess process;
                    try { process = service.GetCustomProcess(new CustomProcess { Name = BasicProcess.Shipping }).First(); }
                    catch { process = null; }

                    service.PrintDocumentsInBatch(new List<Document> { shipment }, null, process);
                }


                RefreshBalance(View.Model.Document);

                //Post - Process
                View.BtnCreateShipment.IsEnabled = false;
                Util.ShowMessage(Util.GetResourceLanguage("SHIPMENT_DOCUMENT") + " [#" + shipment.DocNumber + "] " + Util.GetResourceLanguage("FOR_ORDER") + View.Model.Document.DocNumber + Util.GetResourceLanguage("WAS_CREATED"));

                //View.TabShipping.SelectedIndex = 3; //Select Shipment
                View.GrpShipments.IsSelected = true;

                pw.Close();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("PRINT_DOCUMENTS_AND_LABELS_FAIL") + "\n" + ex.Message);
            }
        }


        private void OnShipmentAtOnce(object sender, EventArgs e)
        {
            try
            {
                //Gettting Bin Location
                Label sourceLocation = CheckDestination(View.BinLocation.Text);

                if (sourceLocation == null)
                {
                    Util.ShowError(Util.GetResourceLanguage("BIN_SOURCE") + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                    return;
                }


                ProcessWindow(Util.GetResourceLanguage("PICKING_PRODUCT"), false);


                View.ProcessResult.Text = "";
                service.PickAtOnce(View.Model.Document, sourceLocation, View.Model.Node);
                RefreshBalance(View.Model.Document);

                View.ProcessResult.Text = Util.GetResourceLanguage("DOCUMENT") + " " + View.Model.Document.DocNumber + Util.GetResourceLanguage("PICK_AT_ONCE_PLE_REV_THE_BAL_FOR_PEN_QUAN");

                pw.Close();

                Util.ShowMessage(View.ProcessResult.Text);



            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_PICKED_AT_ONCE") + "\n" + ex.Message);
            }
        }


        //Crea un documento en Blanco para recibir sin tener en cuenta las document lines,
        //recibe cualquier producto
        private void OnCreateEmptyShipment(object sender, EventArgs e)
        {
            if (View.TxtCustomer.Account == null)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_CUSTOMER"));
                return;
            }

            try
            {

                ProcessWindow(Util.GetResourceLanguage("CREATING_PICKTICKET_WITHOUT_SO"), false);

                DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };
                docType.DocTypeID = SDocType.PickTicket;

                Document document = new Document
                {
                    DocType = docType,
                    CrossDocking = false,
                    IsFromErp = false,
                    Location = App.curLocation,
                    Customer = View.TxtCustomer.Account,
                    Company = App.curCompany,
                    Date1 = DateTime.Today,
                    CreatedBy = App.curUser.UserName

                };


                document = service.CreateNewDocument(document, true);
                LoadDetails(document);
                View.Model.DocumentList.Add(document);
                View.DgDocument.Items.Refresh();


                //Post Result
                View.TxtCustomer.Text = "";

                pw.Close();

                Util.ShowMessage(Util.GetResourceLanguage("EMPTY_PICKTICKET") + document.DocNumber + Util.GetResourceLanguage("CREATED"));

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_CREATED") + "\n" + ex.Message);
            }
        }


        //private void OnLoadProducts(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.DocProducts = service.SearchProduct(e.Value);
        //    SelectOneProduct();
        //}


        //private void OnLoadCustomers(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.CustomerList = service.SearchCustomer(e.Value);

        //    //si encuentra un resultado lo carga
        //    if (View.Model.CustomerList != null && View.Model.CustomerList.Count == 1)
        //        View.LvCustomer.SelectedIndex = 0;
            
        //}


        private void OnChangeStatus(object sender, EventArgs e)
        {

            try
            {
                View.Model.Document.DocStatus = (Status)View.ComboStatus.SelectedItem;

                /*
                if (View.Model.Document.DocStatus.StatusID == DocStatus.Cancelled && View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0))
                {
                    Util.ShowError("Document could not be cancelled because was completed or is already in process.");
                    return;
                }
                */


                View.Model.Document.PickMethod = (PickMethod)View.CboPickMethod.SelectedItem;
                View.Model.Document.ModifiedBy = View.Model.User.UserName;
                View.Model.Document.ModDate = DateTime.Now;


                //Si es un Merged Order y Fue Cancelada mostrar mensaje.
                if (View.Model.Document.DocStatus.StatusID == DocStatus.Cancelled)
                {
                    // CAA [2010/06/03]
                    // si tiene Ship Docs completados o posteados no puede cancelarse
                    if (View.Model.Shipments.Any(f => f.DocStatus.StatusID == DocStatus.Completed || f.DocStatus.StatusID == DocStatus.Posted ))
                    {
                        Util.ShowError(Util.GetResourceLanguage("DOC_CON_COM_POS_SHIP_CAN_NOT_BE_CAN"));
                        return;
                    }

                    bool? reason = UtilWindow.ConfirmOK(Util.GetResourceLanguage("ARE_YOU_SURE_ABOUT_TO_CANCEL_THIS_DOCUMENT") + View.Model.Document.DocNumber + "? ");

                    if (reason == false)
                        return;

                    //Modificacion este metodo aplica tanto para merged (206) como para sencillos (201)
                    service.CancelMergerOrder(View.Model.Document, null);
                    Util.ShowMessage(Util.GetResourceLanguage("PROCESS_DONE"));
                }
                else
                {
                    service.UpdateDocument(View.Model.Document);
                    Util.ShowMessage(Util.GetResourceLanguage("DOCUMENT_UPDATED"));

                }

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("DOCUMENT_COULD_NOT_BE_UPDATED") + "\n" + ex.Message);
            }
        }



        private void OnLoadProductManualTrackOption(object sender, EventArgs e)
        {

            LoadTrackControl();

        }

        private void LoadTrackControl()
        {

            int qtyPendingtoTrack = 0;
            int qtyToTrack = 0;


            //Si no digita cantidad se va a administrar muestra los Tracks pendientes o ya existentes.
            //Pasa derecho:
            //Si No hay pendientes y el documento no es un directreceipt.
            //Si no hay cantidades a recibir
            if ((View.Model.CurQtyPending == 0 && !IsDirectReceipt) || View.TxtRcvQty.Text == "" || !int.TryParse(View.TxtRcvQty.Text, out qtyToTrack) || int.Parse(View.TxtRcvQty.Text) <= 0)
            {
                LoadTrackOptionsControl(qtyPendingtoTrack, qtyToTrack);
                //View.BtnTrackPick.IsEnabled = false;
                return;
            }

            if (View.ComboUnit.SelectedIndex == -1)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_UNIT"));
                return;
            }

            /*
                //Gettting Bin Location -- REMOVIDO porque el track option va a pedir el BIN
            
                View.Model.BinLabel = CheckDestination(View.BinLocation.Text);

                if (View.Model.BinLabel == null)
                {
                    Util.ShowError("Bin/Label source " + View.BinLocation.Text + " is not valid.");
                    return;
                }
            */

            //if (Util.GetConfigOption("SHERPTRACK").Equals("T"))
            //View.BtnTrackPick.IsEnabled = false;

            //View.BtnTrackPick.Visibility = Visibility.Visible;


            //Load User Contron el el ItemControl
            qtyPendingtoTrack = Int32.Parse(View.TxtRcvQty.Text);
            LoadTrackOptionsControl(qtyPendingtoTrack, qtyToTrack);
        }


        private void LoadTrackOptionsControl(int qtyPendingtoTrack, int qtyToTrack)
        {
            //Making Tab Visible
            View.TabItemTrackOption.Visibility = Visibility.Visible;
            View.TabItemTrackOption.Focus();

            trackControl = container.Resolve<TrackOptionShipPresenter>();
            View.TrackOpts.Items.Clear();
            View.TrackOpts.Items.Add(trackControl.View);

            //Seting Values
            trackControl.View.Model.Product = View.Model.Product;
            trackControl.View.Model.Document = View.Model.Document;
            trackControl.View.Model.Node = View.Model.Node;
            trackControl.View.Model.CurUnit = View.ComboUnit.SelectedItem as Unit;
            trackControl.View.Model.CurQtyPending = IsDirectReceipt ? qtyPendingtoTrack : View.Model.CurQtyPending;
            trackControl.View.Model.QtyToTrack = qtyToTrack;
            trackControl.View.Model.TrackType = 1; //PICKING
            trackControl.View.Model.FatherPresenter = this;

            //Inicializa el componente de tracking.
            trackControl.SetupManualTrackOption();

            trackControl.View.UcProduct.Visibility = Visibility.Collapsed;
        }


        //Envia los Labels (por pickLabel) y el producto pendiente (pickProduct) si remainin es > 0
        private void OnPickManualTrack(object sender, EventArgs e)
        {
            //variable que viene del control de Track Options
            IList<Label> trackLabelList = trackControl.View.Model.ManualTrackList; //Viene del control de track Options
            Int32 remainQty = trackControl.View.Model.RemainingQty;


            if (trackLabelList == null || trackLabelList.Count == 0)
            {
                Util.ShowError(Util.GetResourceLanguage("NO_PRODUCT_TO_PICK"));
                return;
            }


            try
            {
                ProcessWindow(Util.GetResourceLanguage("PICKING_PRODUCT"), false);

                //View.BtnTrackPick.IsEnabled = false;

                //Lo que sea Label lo manda a piquea por lable lo otro lo piquea normal
                foreach (Label label in trackLabelList)
                    service.PickLabel(View.Model.Document, label, View.Model.Node, View.Model.CurPackage); //Pick Label

                //Lo remaining lo manda Normal
                //if (View.Model.RemainingQty > 0)
                //    ProcessPickingProduct(View.Model.BinLabel, View.Model.RemainingQty);

                View.ProcessResult.Text = Util.GetResourceLanguage("PRODUCT_PICKED");

                RefreshBalance(View.Model.Document);

                //After Process
                AfterReceiving();


                pw.Close();

                if (showNextTime)
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
                //Util.ShowMessage(View.ProcessResult.Text);

                //Reset the Track
                trackControl.View.Model.ManualTrackList = null;
                trackControl.View.Model.TrackData = null;
                trackControl.View.ManualTrackList.Items.Refresh();


            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("ERROR_PICKING_PRODUCT") + "\n" + ex.Message);
                return;
            }

        }


        public void AfterReceiving()
        {
            View.TxtProduct.Product = null;
            View.TxtProduct.Text = "";
            View.TxtProduct.ProductDesc = "";
            View.TxtProduct.DataList = null;
            View.Model.ProductBinStock = null;
            View.BinLocation.Text = "";           

            View.Model.ProductUnits = null; //View.Model.PackingUnits =
            View.ComboUnit.Items.Refresh();
            View.TxtRcvQty.Text = "0";
            View.TxtProduct.Text = "";


            //Update The Default List
            View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0).Select(f => f.Product).ToList();

            //recargar los packages disponibles
            LoadPackages();
        }



        private void OnRefreshBin(object sender, EventArgs e)
        {
            ShowLabelsAvailable(View.Model.Document);

        }


        //Carga las lineas de los recibos posteados en el ERP
        private void OnLoadShipment(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            LoadShipment(e.Value);

        }


        private void LoadShipment(Document document)
        {
            try
            {
                View.Model.PostedShipment = document;
                View.StkShipmentData.Visibility = Visibility.Visible;

                View.Model.ShipmentData = Util.ToShowData(document);

                //Actualizando datos del Documento requeridos para posibles transacciones
                document.ModifiedBy = View.Model.User.UserName;


                //Print Shipment In Batch
                View.DirectPrintShip.Document = View.Model.PostedShipment;


                //Calling Service
                View.Model.ShipmentLines = service.GetDocumentLine(new DocumentLine { Document = document })
                    .OrderBy(x=>x.LineNumber).ToList();

               View.DgShipmentLines.Visibility = View.BtnReversePosted.Visibility = Visibility.Visible;

               if (View.Model.ShipmentLines == null || View.Model.ShipmentLines.Count == 0)
                   View.DgShipmentLines.Visibility = View.BtnReversePosted.Visibility = Visibility.Collapsed;
               

                //Calling Shipment Packages
               if (Util.GetConfigOption("PACKMANGR").Equals("2"))
               {
                   View.Model.DocPackages = service.GetDocumentPackage(new DocumentPackage { PostingDocument = document })
                       .Where(f => f.PackageType != "R" || f.ChildPackages.Count > 0).ToList();
               }
               else
                   View.Model.DocPackages = service.GetDocumentPackage(new DocumentPackage { PostingDocument = document });


               //si el documento esta cancelado o posted no muetsra el boton de reverse
               if (document.DocStatus.StatusID == DocStatus.Cancelled || document.DocStatus.StatusID == DocStatus.Posted)
                   View.BtnReversePosted.Visibility = Visibility.Collapsed;


                //Ocultar el Repore Packaged
               //View.BtnAdmPk.Visibility = Visibility.Visible;
               //if (Util.GetConfigOption("ALWREORDERPK").Equals("F"))
                   //View.BtnAdmPk.Visibility = Visibility.Collapsed;



                //OCultar el Boton de ReFulFill
               View.BtnRefullfil.Visibility = Visibility.Collapsed;
               if (View.Model.Document.DocType.DocTypeID == SDocType.SalesOrder && document.DocStatus.StatusID == DocStatus.Completed && Util.GetConfigOption("WITHERPSH").Equals("T")
                   && Util.GetConfigOption("CREERPSH").Equals("T") && Util.GetConfigOption("ONESHIPMENT").Equals("T"))
                    View.BtnRefullfil.Visibility = Visibility.Visible;


            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("SHIPMENT_COULD_NOT_BE_LOADED") + "\n" + ex.Message);

            }
        }


        //REveerse the shipment sent to GP
        private void OnReversePosted(object sender, DataEventArgs<string> e)
        {
            try
            {

                //Evaluar si viene Bin, de restock, si no usa Main.
                Bin restoreBin = View.BinRestock.Bin; 

                try
                {
                    //Si no viene el Bin usa el Bin por defecto.
                    if (restoreBin == null)
                        restoreBin = service.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = View.Model.Document.Location })
                            .First();

                }
                catch
                {
                    Util.ShowError(Util.GetResourceLanguage("DEFAULT_BIN_DOES_NOT_EXIST"));
                    return;
                }

                string restoreBinStr = restoreBin.BinCode;

                View.Model.PostedShipment.Comment = e.Value + Util.GetResourceLanguage("STOCK_RESTORED_TO") + " [" + restoreBinStr + "]";
                View.Model.PostedShipment.ModDate = DateTime.Now;
                View.Model.PostedShipment.ModifiedBy = App.curUser.UserName;
                service.ReverseShipmentDocument(View.Model.PostedShipment, restoreBin);

                Util.ShowMessage(Util.GetResourceLanguage("SHIPMENT") + " " + View.Model.PostedShipment.DocNumber
                    + Util.GetResourceLanguage("WAS_CANCELLED_AND_STOCK_RESTORED_TO") + restoreBinStr);
                View.BtnReversePosted.Visibility = Visibility.Collapsed;

                View.BinRestock.Text = "";
                RefreshBalance(View.Model.Document);
                RefreshShipments();

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("SHIPMENT_COULD_NOT_BE_CANCELED") + ex.Message);
            }
        }



        private void OnShowPickingTicket(object sender, EventArgs e)
        {
            ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"), false);
            UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.Document.DocID,"",false);

            
            //Update document to InProcess
            if (View.Model.Document.DocStatus.StatusID == DocStatus.New)
            {
                View.Model.Document.DocStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.InProcess).First();
                service.UpdateDocument(View.Model.Document);
            }

            // CAA [2010/05/06]
            // Se imprimen los labels de los kits, para las SO
            if (View.Model.Document.DocType.DocTypeID == SDocType.SalesOrder && Util.GetConfigOption("PRTKTLBL").Equals("T"))
            {
                try
                {
                    service.PrintKitAssemblyLabels(View.Model.Document, 1);
                }
                catch { }
            }


            pw.Close();
        }


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore)
        {
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }



        private void OnRemoveFromNode(object sender, DataEventArgs<DocumentBalance> e)
        {

            if (e.Value == null)
                return;


            DocumentBalance balanceLine = e.Value;
            bool refreshBalance = ((DataGridControl)sender).Name == "dgDocumentBalance" ? true : false;

            if (((DataGridControl)sender).Name == "dgPostingBalance" && balanceLine.QtyPending.Equals(0))
            {
                Util.ShowMessage(Util.GetResourceLanguage("NO_QUANTITY_AVAILABLE"));
                return;
            }


            if (((DataGridControl)sender).Name == "dgDocumentBalance" && balanceLine.QtyProcessed.Equals(0))
            {
                Util.ShowMessage(Util.GetResourceLanguage("NO_QUANTITY_AVAILABLE"));
                return;
            }


            try
            {
                IRemoveNodePresenter presenter = container.Resolve<RemoveNodePresenter>();
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


        //Recarga los shipments en tab de shipments
        void View_RefreshShipments(object sender, EventArgs e)
        {
            RefreshShipments();
        }


        //Muestra el packing list
        void View_ShowPackingList(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_RECORD"));
                return;
            }

            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"));

            //Open the Document Ticket
            try
            {
                UtilWindow.ShowDocument(e.Value.DocType.Template, e.Value.DocID, "", false);
                pw.Close();
            }
            catch { pw.Close(); }
        }


        //29 Oct 2009
        void View_PrintTicketInBatch(object sender, EventArgs e)
        {
            try
            {
                service.PrintDocumentsInBatch(new List<Document> { View.Model.Document }, null, null);
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("PROBLEM_PRINTING_THE_DOCUMENT") + "\n" + ex.Message);
            }
        }



        void View_CancelLine(object sender, DataEventArgs<DocumentLine> e)
        {
            if (e.Value == null)
                return;

            if (e.Value.LineStatus.StatusID != DocStatus.New)
            {
                Util.ShowError(Util.GetResourceLanguage("ONLY_LINES_IN_STATUS_CAN_BE_REMOVED"));
                return;
            }

            try
            {
                //Manejo Especial para las Merged Order
                if (View.Model.Document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                {
                    service.CancelMergerOrder(null, e.Value);
                    e.Value.LineStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.Cancelled).First();
                }
                else                
                    service.SaveUpdateDocumentLine(e.Value, true);
                

                //View.Model.DocumentLines = service.GetDocumentLine(new DocumentLine { Document = View.Model.Document });                

                ReloadDocumentLines();
                Util.ShowMessage(Util.GetResourceLanguage("LINE_WAS_REMOVED"));


            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }



        void View_LoadPopupLine(object sender, DataEventArgs<DocumentLine> e)
        {
            if (View.StkUpdLines.Visibility == Visibility.Collapsed)
                return;


            if (e.Value != null && e.Value.LineID > 0 && e.Value.LineStatus.StatusID != DocStatus.New)
            {
                Util.ShowError(Util.GetResourceLanguage("ONLY_LINES_IN_STATUS_CAN_BE_UPDATED"));
                return;
            }



            View.UcDocLine.scProduct.IsEnabled = true;
            View.UcDocLine.cboUnit.IsEnabled = true;
            View.UcDocLine.btnProcess.Content = Util.GetResourceLanguage("ADD_LINE");

            //Adicionar la Linea a la Orden
            View.Popup3.IsOpen = true;
            View.UcDocLine.CurDocument = View.Model.Document;
            View.UcDocLine.CurDocLine = e.Value;
            View.UcDocLine.PresenterParent = this;


            if (e.Value != null && e.Value.LineID > 0)
            {

                View.UcDocLine.scProduct.Product = e.Value.Product;
                View.UcDocLine.scProduct.ProductDesc = e.Value.Product.Description;
                View.UcDocLine.scProduct.txtProductDesc.Text = e.Value.Product.Description;

                View.UcDocLine.ProductUnits = e.Value.Product.ProductUnits.Select(f=>f.Unit).ToList();
                View.UcDocLine.cboUnit.SelectedItem = e.Value.Unit;
                

                View.UcDocLine.scProduct.IsEnabled = false;
                View.UcDocLine.cboUnit.IsEnabled = false;

                View.UcDocLine.btnProcess.Content = Util.GetResourceLanguage("UPDATE_LINE");
            }

        }




        public void ReloadDocumentLines()
        {
            View.Model.DocumentLines = null;
            try
            {
                View.Model.DocumentLines =
                    service.GetDocumentLine(new DocumentLine { Document = View.Model.Document })
                    .OrderByDescending(f => f.LineNumber).ToList();
            }
            catch { }
        }

    }
}
