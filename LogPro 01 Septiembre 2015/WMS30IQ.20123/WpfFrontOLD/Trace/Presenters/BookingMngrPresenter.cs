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
using System.Windows.Data;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;




namespace WpfFront.Presenters

{


    public interface IBookingMngrPresenter
    {
        IBookingMngrView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class BookingMngrPresenter : IBookingMngrPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }

        public IBookingMngrView View { get; set; }
        
        ProductInventory piInUse = null;
        IList<ProductStock> productInUseList = null;
        IList<ProductStock> documentStock = null;
        bool showMessage = true;
        ProcessWindow pw = null;
        double availabilityMark = 0;
        bool firstTime = false;
        double TotWeight = 0;
        double TotVolume = 0;
        bool isNewDoc = true;

        public BookingMngrPresenter(IUnityContainer container, IBookingMngrView view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.region = region;
                this.service = new WMSServiceClient();
                View.Model = this.container.Resolve<BookingMngrModel>();


                //Delegates 
                View.AccountSelected += new EventHandler<DataEventArgs<ShowData>>(View_AccountSelected);

                View.LoadDetails += new EventHandler<DataEventArgs<System.Data.DataRow>>(View_LoadDetails);

                View.CreateMergedDocument += new EventHandler<EventArgs>(View_CreateMergedDocument);

                View.EnlistDetails += new EventHandler<EventArgs>(View_EnlistDetails);

                View.CheckLineBalanceBO += new EventHandler<DataEventArgs<object[]>>(View_CheckLineBalanceBO);

                View.CheckLineBalanceCancel += new EventHandler<DataEventArgs<object[]>>(View_CheckLineBalanceCancel);

                View.LineChecked += new EventHandler<DataEventArgs<long>>(View_LineChecked);
                
                View.LineUnChecked += new EventHandler<DataEventArgs<long>>(View_LineUnChecked);

                View.DocumentChecked += new EventHandler<DataEventArgs<int>>(View_DocumentChecked);
                
                View.DocumentUnChecked += new EventHandler<DataEventArgs<int>>(View_DocumentUnChecked);

                View.AccountSelectedAllLines += new EventHandler<DataEventArgs<ShowData>>(View_AccountSelectedAllLines);

                View.LoadPopupLine += new EventHandler<DataEventArgs<DocumentLine>>(View_LoadPopupLine);

                View.CancelLine += new EventHandler<DataEventArgs<DocumentLine>>(View_CancelLine);

                View.RefreshAddress += new EventHandler<EventArgs>(View_RefreshAddress);

                View.ShowDocument += new EventHandler<DataEventArgs<LabelTemplate>>(View_ShowDocument);

                View.StartProcess += new EventHandler<DataEventArgs<ShowData>>(View_StartProcess);

                View.LoadDocument += new EventHandler<DataEventArgs<Document>>(View_LoadDocument);

                View.UpdateAdditionalInfo += new EventHandler<EventArgs>(View_UpdateAdditionalInfo);



                View.DgDocument.MinHeight = SystemParameters.FullPrimaryScreenHeight - 290;
                View.DgDocument.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 290;
                View.DgDetails.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 310;
                View.DgDetails.MinHeight = SystemParameters.FullPrimaryScreenHeight - 310;

                try {  availabilityMark = double.Parse(Util.GetConfigOption("AVAILMARK")); }
                catch { availabilityMark = 1; }


                //Add Remove Lines

                try
                {
                    if (Util.AllowOption("REMSOLINE"))
                        View.StkUpdLines.Visibility = Visibility.Visible;
                }
                catch { }
                
                LoadAccounts();
                
                LoadLastestDocuments();

                LoadTemplateList();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error loading view.\n" + ex.Message);
            }
        }



        void View_ShowDocument(object sender, DataEventArgs<LabelTemplate> e)
        {
            if (e.Value == null)
                return;

            if (View.Model.CurOpenDoc == null || View.Model.CurOpenDoc.DocID == 0)
            {
                Util.ShowError("No document to open. Please create the merged document first.");
                return;
            }

            try
            {
                pw = new ProcessWindow("Opening Document Form ");

                UtilWindow.ShowDocument(e.Value, View.Model.CurOpenDoc.DocID, "", true); //"PDF995"

                pw.Close();
            }
            catch { }
            finally { pw.Close(); }
        }






        void View_LoadDocument(object sender, DataEventArgs<Document> e)
        {
           //Carga las lineas del documento, y las marca como ya cargadas. porque tienen ID            
            View.Model.CurOpenDoc = View.CboOpenDoc.SelectedItem as Document;
            View.DgSelected.Items.Refresh();
        }



        void View_StartProcess(object sender, DataEventArgs<ShowData> e)
        {

            if (e.Value == null)
                return;

            View.TabStep.Visibility = Visibility.Hidden;
            View.CboOpenDoc.IsEnabled = false;
            View.CboOpenDoc.Background = Brushes.Transparent;

            View.Model.OpenList = service.GetPendingDocument(
                new Document
                {
                    Customer = new Account { AccountID = int.Parse(e.Value.DataKey) },
                    //DocStatus = new Status { StatusID = DocStatus.PENDING },
                    DocType = new DocumentType { DocTypeID = SDocType.MergedSalesOrder },
                    Location = App.curLocation
                },0,0);

            View.Model.OpenList.Insert(0, new Document { DocNumber = "NEW DOCUMENT" });


            firstTime = true;

            //UN Nuevo Booking
            if (View.Model.OpenList == null || View.Model.OpenList.Count == 1)
            {                
                //LoadAccountDocuments(int.Parse(e.Value.DataKey), false, true);
                View.CboOpenDoc.SelectedIndex = 0;
                View.CboOpenDoc.IsEnabled = false;
                isNewDoc = true;
                View.Model.CurOpenDoc = View.CboOpenDoc.SelectedItem as Document;
            }
            else
            {
                View.CboOpenDoc.IsEnabled = true;
                View.CboOpenDoc.Items.Refresh();
                View.CboOpenDoc.Background = Brushes.Yellow;
                isNewDoc = false;
            }


        }




        void View_RefreshAddress(object sender, EventArgs e)
        {
            View.Model.CustomerAddress = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = View.Model.Customer.AccountID } });
            View.Model.CustomerAddress.Insert(0, new AccountAddress { ErpCode = "NEW", FullDesc = "Add New Address ... " });
            View.CboShipTo.Items.Refresh();
        }
        


        void View_CancelLine(object sender, DataEventArgs<DocumentLine> e)
        {
            if (e.Value == null)
                return;

            if (e.Value.LineStatus.StatusID != DocStatus.New)
            {
                Util.ShowError("Only lines in status [New] can be removed.");
                return;
            }

            try
            {
                //Manejo Especial para las Merged Order
                if (e.Value.Document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                {
                    service.CancelMergerOrder(null, e.Value);
                    e.Value.LineStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.Cancelled).First();
                }
                else
                    service.SaveUpdateDocumentLine(e.Value, true);


                try { View.Model.OrdersDetail.Remove(e.Value); }
                catch { }

                try { View.Model.CurrentDetails.Remove(e.Value); }
                catch { }

                View.DgDetails.Items.Refresh();

                Util.ShowMessage("Line was Removed.");


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

            if (View.ChkFilter.IsChecked != true)
            {
                Util.ShowError("Please select filtered by order to update lines.");
                return;
            }


            if (e.Value != null && e.Value.LineID > 0 && e.Value.LineStatus.StatusID != DocStatus.New)
            {
                Util.ShowError("Only lines in status [New] can be updated.");
                return;
            }
            



            View.UcDocLine.scProduct.IsEnabled = true;
            View.UcDocLine.cboUnit.IsEnabled = true;
            View.UcDocLine.btnProcess.Content = "Add Line";

            //Adicionar la Linea a la Orden
            View.Popup3.IsOpen = true;
            
            //Document
            View.UcDocLine.CurDocument = service.GetDocument(new Document { DocID = View.Model.DocID }).First();

            View.UcDocLine.CurDocLine = e.Value;
            View.UcDocLine.PresenterParent = this;


            if (e.Value != null && e.Value.LineID > 0)
            {

                View.UcDocLine.scProduct.Product = e.Value.Product;
                View.UcDocLine.scProduct.ProductDesc = e.Value.Product.Description;
                View.UcDocLine.scProduct.txtProductDesc.Text = e.Value.Product.Description;

                View.UcDocLine.ProductUnits = e.Value.Product.ProductUnits.Select(f => f.Unit).ToList();
                View.UcDocLine.cboUnit.SelectedItem = e.Value.Unit;


                View.UcDocLine.scProduct.IsEnabled = false;
                View.UcDocLine.cboUnit.IsEnabled = false;

                View.UcDocLine.btnProcess.Content = "Update Line";
            }
        }



        private void LoadLastestDocuments()
        {
            //loading Last Document List
            View.UCDocList.CurDocumentType = service.GetDocumentType(new DocumentType { DocTypeID = SDocType.MergedSalesOrder }).First();
            View.UCDocList.LoadDocuments("");
        }



        #region TAB1 - Selecting Lines

        void View_DocumentChecked(object sender, DataEventArgs<int> e)
        {
            pw = new ProcessWindow("Checking lines ...");

            showMessage = false;
            IList<DocumentLine> docLines;

            //Chequea todas las lineas del documento que no esten checked
            if (View.ChkFilter.IsChecked == true)
                docLines = View.Model.OrdersDetail.Where(f => f.Document.DocID == e.Value).ToList();
            else
                docLines = View.Model.OrdersDetail;


            foreach (DocumentLine dl in docLines)
            {
                if (dl.IsDebit == true) //  || dl.QtyShipped <= 0
                    continue;

                dl.IsDebit = true;
            }

            showMessage = true;

            pw.Close();
        }



        void View_DocumentUnChecked(object sender, DataEventArgs<int> e)
        {
            pw = new ProcessWindow("Unchecking lines ...");
            IList<DocumentLine> docLines;

            //Chequea todas las lineas del documento que no esten checked
            if (View.ChkFilter.IsChecked == true)
                docLines = View.Model.OrdersDetail.Where(f => f.Document.DocID == e.Value).ToList();
            else
                docLines = View.Model.OrdersDetail;

            foreach (DocumentLine dl in docLines)
            {
                if (dl.IsDebit == false)
                    continue;

                dl.IsDebit = false;
            }

            pw.Close();
        }



        void View_LineChecked(object sender, DataEventArgs<long> e)
        {
            LineChecked(e.Value);
        }



        private void LineChecked(long lineID)
        {

            try
            {
                //Check the line for teh balance. Show message if problem.
                DocumentLine curLine;

                try { curLine = View.Model.OrdersDetail.Where(f => f.LineID == lineID).First(); }
                catch { return; }


                double qtyAllocated = 0, qtyAvailable = 0;

                if (curLine.QtyAllocated == 0)
                {

                    //if (documentStock == null)
                    //    qtyAvailable = GetQtyAvailable(curLine.Product);
                    //else
                    //{

                    /*
                        try { qtyAvailable = View.Model.DocumentProductStock
                            .Where(f => f.Product.ProductID == curLine.Product.ProductID).First()
                            .PackStock; }
                        catch { qtyAvailable = 0; }
                    //}

                    //Quitando la cantidad en Uso.
                    curLine.QtyShipped = qtyAvailable; //On Hand
                    */

                    qtyAvailable = curLine.QtyOnHand; //curLine.QtyShipped; //On Hand

                    //curLine.QtyInvoiced = GetProductInUse(curLine.Product);
                    
                    qtyAvailable -= curLine.QtyInvoiced;

                    if (qtyAvailable > 0)
                    {
                        //Se hacen los calculos del Allocation.
                        qtyAllocated = curLine.Quantity - curLine.QtyCancel - curLine.QtyBackOrder;

                        if (qtyAllocated > (qtyAvailable/curLine.Unit.BaseAmount))
                            qtyAllocated = (qtyAvailable / curLine.Unit.BaseAmount);


                        //Guardar en Base de Datos.
                        //piInUse.QtyInUse += qtyAllocated;
                        //PersistProductInUse(new ProductInventory
                        //{
                        //    QtyInUse = qtyAllocated * curLine.Unit.BaseAmount,
                        //    Product = curLine.Product,
                        //    Document = curLine.Document
                        //});

                        curLine.QtyAllocated = qtyAllocated;
                        qtyAvailable -= qtyAllocated * curLine.Unit.BaseAmount;

                    }
                    else
                    {
                        curLine.QtyAllocated = 0;
                        curLine.QtyAvailable = 0;

                        //if (showMessage)
                            //Util.ShowError("No quantity available to allocate this line.");
                    }


                    //Redistribucion del Cancell y Allocated segun valores.
                    if (View.CboToDo.SelectedIndex == 0)
                        curLine.QtyBackOrder = curLine.Quantity - curLine.QtyCancel - qtyAllocated;
                    else
                        curLine.QtyCancel = curLine.Quantity - curLine.QtyBackOrder - qtyAllocated;


                    //Cambiando el Qty Pending donde se use el mismo producto.
                    for (int i = 0; i < View.Model.OrdersDetail.Count; i++)
                    {
                        if (View.Model.OrdersDetail[i].Product.ProductID == curLine.Product.ProductID)
                        {
                            View.Model.OrdersDetail[i].QtyAvailable = (qtyAvailable > 0 ? qtyAvailable : 0) / View.Model.OrdersDetail[i].Unit.BaseAmount;
                            View.Model.OrdersDetail[i].QtyInvoiced += qtyAllocated;
                        }
                    }



                }

            }
            catch { }

        }



        void View_LineUnChecked(object sender, DataEventArgs<long> e)
        {
            LineUnChecked(e.Value);            
        }


        private void LineUnChecked(long lineID)
        {
            //Adjust the availabilty (In Use) balance of the quantity.
            DocumentLine curLine;

            //ProductInventory piInUse;
            try { curLine = View.Model.OrdersDetail.Where(f => f.LineID == lineID).First(); }
            catch { return;  }

            if (curLine.QtyAllocated <= 0)
                return;

            try
            {
                //PersistProductInUse(new ProductInventory
                //{
                //    QtyInUse = -1 * curLine.QtyAllocated * curLine.Unit.BaseAmount,
                //    Product = curLine.Product,
                //    Document = curLine.Document
                //});

                //Cambiando el Qty Pending donde se use el mismo producto.
                for (int i = 0; i < View.Model.OrdersDetail.Count; i++)
                {
                    if (View.Model.OrdersDetail[i].Product.ProductID == curLine.Product.ProductID)
                    {
                        View.Model.OrdersDetail[i].QtyAvailable += curLine.QtyAllocated;
                        View.Model.OrdersDetail[i].QtyInvoiced -= curLine.QtyAllocated;

                        if (View.Model.OrdersDetail[i].QtyInvoiced < 0)
                            View.Model.OrdersDetail[i].QtyInvoiced = 0;
                    }
                }

                curLine.QtyAllocated = 0;

            }
            catch { }

            //piInUse = null;
        }


        private Double GetProductInUse(Product product)
        {
            //Cantidad en Uso - PROGRAMA. Allocated y Reservada
            /*
            try
            {
                return service.GetProductInventoryByProduct(
                    new ProductInventory(), new List<int> { product.ProductID })
                   .Sum(f=>f.QtyAllocated + f.QtyInUse);
            }
            catch { return 0; }
             */

            double inUse = 0, curAllocated = 0;

            //Trae la cantidad en uso de las lineas allocated.
            //De las MO, - Las lineas shipped.            
            try
            {
                /*
                inUse = service.GetProductInUseForMerged(
                    new List<int> { product.ProductID }).First().Stock;
                 */

                inUse = productInUseList
                   .Where(f => f.Product.ProductID == product.ProductID)
                   .Sum(f => f.Stock);

            }
            catch { inUse = 0; }

            //los que estan en uso currently que salen de los order documents pero solo de Allocation
            try
            {
                curAllocated = View.Model.OrdersDetail.Where(f => f.Product.ProductID == product.ProductID)
                    .Sum(f => f.QtyAllocated * f.Unit.BaseAmount);
            }
            catch { curAllocated = 0; }

            return inUse + curAllocated;

        }

        /*
        private double GetQtyAvailable(Product product)
        {
            //Calculing Qty Available.
            try
            {
                return (double)((int)(service.GetDocumentProductStock(new Document
                {
                    Customer = new Account { AccountID = View.Model.Customer.AccountID },
                    Location = App.curLocation,
                    DocType = new DocumentType { DocTypeID = SDocType.SalesOrder },
                    DocID = View.Model.DocID 
                },
                    product).First().FullStock * availabilityMark));

            }
            catch { return 0; }
        }

        
        private void PersistProductInUse(ProductInventory piInUse)
        {
            bool save = true;
            while (save)
            {
                try
                {
                    piInUse.CreatedBy = App.curUser.UserName;
                    piInUse.Location = App.curLocation;
                    service.PersistProductInUse(piInUse); save = false;
                }
                catch { }
            }
        }
        */

        void View_CheckLineBalanceCancel(object sender, DataEventArgs<object[]> e)
        {
            DocumentLine curLine;
            Double qtyInUse = 0;

            curLine = View.Model.CurrentDetails.Where(f => f.LineID == long.Parse(e.Value[0].ToString())).First();

            double qtyCancel = double.Parse(e.Value[1].ToString());
            double qtyInitialAllocated = curLine.QtyAllocated;

            //Obteniendo los valores de available.
            double qtyAvailable = curLine.QtyOnHand; //curLine.QtyShipped; // GetQtyAvailable(curLine.Product);
            //curLine.QtyShipped = qtyAvailable;

            //Quitando la cantidad en Uso.    
            //curLine.QtyInvoiced = GetProductInUse(curLine.Product);
            qtyAvailable -= curLine.QtyInvoiced;

            curLine.QtyAvailable = (qtyAvailable > 0 ? qtyAvailable : 0)/curLine.Unit.BaseAmount;


            //Obteniendo el Allocated
            double qtyAllocated = curLine.Quantity - curLine.QtyBackOrder - qtyCancel;    

            if (qtyAllocated > curLine.QtyAvailable) qtyAllocated = curLine.QtyAvailable;
            if (qtyAllocated < 0) qtyAllocated = 0;

            curLine.Sequence = (int)(curLine.Quantity - curLine.QtyBackOrder - qtyCancel - qtyAllocated);

            //Ajustando el allocated.            
            //piInUse.QtyInUse += (qtyAllocated - curLine.QtyAllocated);
            //PersistProductInUse(new ProductInventory
            //{
            //    QtyInUse = (qtyAllocated - curLine.QtyAllocated) * curLine.Unit.BaseAmount,
            //    Product = curLine.Product,
            //    Document = curLine.Document
            //});

            curLine.QtyAllocated = qtyAllocated;

            qtyInitialAllocated -= qtyAllocated;

            if (qtyInitialAllocated != 0)
            {
                for (int i = 0; i < View.Model.OrdersDetail.Count; i++)
                {
                    if (View.Model.OrdersDetail[i].Product.ProductID == curLine.Product.ProductID)
                    {
                        View.Model.OrdersDetail[i].QtyAvailable += qtyInitialAllocated;
                        View.Model.OrdersDetail[i].QtyInvoiced -= qtyInitialAllocated;
                        if (View.Model.OrdersDetail[i].QtyInvoiced < 0)
                            View.Model.OrdersDetail[i].QtyInvoiced = 0;
                    }
                }
            }


            //ListViewItem lastRowChanged = View.DgDetails.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
            //lastRowChanged.Background = Brushes.LightGreen;

            //((ListViewItem)View.DgDetails.Items[0]).Background = Brushes.Yellow;

            View.DgDetails.Items.Refresh();


        }



        void View_CheckLineBalanceBO(object sender, DataEventArgs<object[]> e)
        {
            DocumentLine curLine;
            Double qtyInUse = 0;
            curLine = View.Model.CurrentDetails.Where(f => f.LineID == long.Parse(e.Value[0].ToString())).First();

            double qtyBO = double.Parse(e.Value[1].ToString());
            double qtyInitialAllocated = curLine.QtyAllocated;

            //Obteniendo los valores de available.
            double qtyAvailable = curLine.QtyOnHand;  //curLine.QtyShipped; // GetQtyAvailable(curLine.Product);
            //curLine.QtyShipped = qtyAvailable;

            //Quitando la cantidad en Uso.
            //curLine.QtyInvoiced = GetProductInUse(curLine.Product);
            qtyAvailable -= curLine.QtyInvoiced;

            curLine.QtyAvailable = (qtyAvailable  > 0 ? qtyAvailable: 0) / curLine.Unit.BaseAmount;


            //Obteniendo el Allocated
            double qtyAllocated = curLine.Quantity - curLine.QtyCancel - qtyBO;

            if (qtyAllocated > curLine.QtyAvailable) qtyAllocated = curLine.QtyAvailable;
            if (qtyAllocated < 0) qtyAllocated = 0;

            curLine.Sequence = (int)(curLine.Quantity - curLine.QtyCancel - qtyBO - qtyAllocated);

            //Ajustando el allocated.
            //piInUse.QtyInUse += (qtyAllocated - curLine.QtyAllocated);
            //PersistProductInUse(new ProductInventory
            //{
            //    QtyInUse = (qtyAllocated - curLine.QtyAllocated) * curLine.Unit.BaseAmount,
            //    Product = curLine.Product,
            //    Document = curLine.Document
            //});
            curLine.QtyAllocated = qtyAllocated;
            //View.Model.OrdersDetail[i].QtyInvoiced += qtyAllocated;

            qtyInitialAllocated -= qtyAllocated;

            if (qtyInitialAllocated != 0)
            {
                for (int i = 0; i < View.Model.OrdersDetail.Count; i++)
                {
                    if (View.Model.OrdersDetail[i].Product.ProductID == curLine.Product.ProductID)
                    {
                        View.Model.OrdersDetail[i].QtyAvailable += qtyInitialAllocated; //Available
                        View.Model.OrdersDetail[i].QtyInvoiced -= qtyInitialAllocated; //Allocated
                        if (View.Model.OrdersDetail[i].QtyInvoiced < 0)
                            View.Model.OrdersDetail[i].QtyInvoiced = 0;
                    }
                }
            }


            View.DgDetails.Items.Refresh();
       
        }




        void View_LoadDetails(object sender, DataEventArgs<System.Data.DataRow> e)
        {
            if (e.Value == null)
                return;

            if (e.Value["DocNumber"].ToString() == View.Model.DocNumber)
                return;

            LoadDetails(e.Value);    
        }



        private void LoadDetails(System.Data.DataRow dr)
        {
            if (dr == null)
                return;

            TotVolume = TotWeight = 0;

            View.Model.DocNumber = dr["DocNumber"].ToString();
            View.Model.DocID = int.Parse(dr["DocID"].ToString());

            pw = new ProcessWindow("Loading lines for document " + View.Model.DocNumber + " ...");

            View.Model.CurrentDetails = View.Model.OrdersDetail.Where(f => f.Document.DocID == View.Model.DocID).ToList();

            //Ejecuta la disponibilidad para los current details       

            //if (View.Model.CurrentDetails.Any(f => f.IsDebit != true))
            //{

                #region INVENTORY LINE BY LINE

                //1. Obtener el Stock de los productos de los documentos de ese cliente.
                View.Model.DocumentProductStock = service.GetDocumentProductStock(new Document
                {
                    Customer = new Account { AccountID = View.Model.Customer.AccountID },
                    Location = App.curLocation,
                    DocType = new DocumentType { DocTypeID = SDocType.SalesOrder } //,
                    //DocID = View.Model.DocID 
                }, null);

                //Allocated, Available de los productos del documento.
                //Obtiene la lista de productos para obtener su stock en uso (Product Inventory).
                List<int> productList = new List<int>();
                foreach (Int32 p in View.Model.CurrentDetails.Select(f => f.Product.ProductID).Distinct())
                    productList.Add(p);

                productInUseList = service.GetProductInUseForMerged(productList, App.curLocation);

                double qtyAvailable, qtyInUse;

                ProductStock piInUse;

                for (int i = 0; i < View.Model.CurrentDetails.Count; i++)
                {

                    qtyInUse = 0; qtyAvailable = 0;

                    if (View.Model.CurrentDetails[i].IsDebit == true)
                        continue;

                    if (View.Model.CurrentDetails[i].IsDebit == null)
                        View.Model.CurrentDetails[i].IsDebit = false;

                    View.Model.CurrentDetails[i].Note = View.Model.CurrentDetails[i].Document.DocNumber;

                    //QTY AVAILABLE DE INVENTARIO - Debe Restarsele lo allocated y lo que este en uso
                    //que sale de la clase product Inventory.
                    try
                    {
                        qtyAvailable = View.Model.DocumentProductStock
                           .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                           .First().FullStock;
                    }
                    catch { qtyAvailable = 0; }

                    //Cantidad en Uso - PROGRAMA. Allocated y Reservada
                    qtyAvailable = (double)((int)(qtyAvailable * availabilityMark));
                    View.Model.CurrentDetails[i].QtyOnHand = qtyAvailable; //ON HAND

                    try
                    {
                     
                        piInUse = productInUseList
                           .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                           .First();

                        qtyInUse = piInUse.Stock;
                    }
                    catch{}


                    //try {

                    //    //Adicionar En uso del order detail de otras lineas y el mismo producto
                    //    qtyInUse += View.Model.OrdersDetail
                    //       .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                    //        //&& f.LineID != View.Model.CurrentDetails[i].LineID)
                    //       .Sum(f => f.QtyInvoiced);  

                    //}
                    //catch { }

                    qtyAvailable -= qtyInUse;

                    //Cantidad final disponible
                    //View.Model.CurrentDetails[i].QtyInvoiced = qtyInUse; //Allocated

                    //View.Model.CurrentDetails[i].QtyAvailable = ((qtyAvailable > 0) ? qtyAvailable : 0) / View.Model.CurrentDetails[i].Unit.BaseAmount;
                    
                    View.Model.CurrentDetails[i].QtyAllocated = 0; //qtyAllocated;
                    
                    View.Model.CurrentDetails[i].Sequence = 0; //BALANCE


                    View.Model.CurrentDetails[i].LineWeight = View.Model.CurrentDetails[i].Quantity * View.Model.CurrentDetails[i].Product.Weight * View.Model.CurrentDetails[i].Unit.BaseAmount;

                    View.Model.CurrentDetails[i].LineVolume = View.Model.CurrentDetails[i].Quantity * View.Model.CurrentDetails[i].Product.Volume * View.Model.CurrentDetails[i].Unit.BaseAmount;


                    //TOTALES
                    TotWeight += View.Model.CurrentDetails[i].LineWeight;
                    TotVolume += View.Model.CurrentDetails[i].LineVolume;

                }

                #endregion

                View.Model.Totals = new List<DocumentLine>() { new DocumentLine { LineWeight = TotWeight, 
                LineVolume = TotVolume, QtyInvoiced = 75.03 } };

            pw.Close();

            View.DgDetails.Items.Refresh();
            View.TabStep.Visibility = Visibility.Visible;
        }




        //Propiedades adicionales
        IList<DocumentLine> docLines;



        void View_AccountSelected(object sender, DataEventArgs<ShowData> e)
        {

            if (e.Value == null)
                return;

            View.TabStep.Visibility = Visibility.Hidden;

            if (e.Value == null)
                return;

            if (View.CboOpenDoc.SelectedIndex == 0)
                isNewDoc = true;
            else
                isNewDoc = false;

            LoadAccountDocuments(int.Parse(e.Value.DataKey), true, true);
        }



        void View_AccountSelectedAllLines(object sender, DataEventArgs<ShowData> e)
        {
            if (e.Value == null)
                return;

            if (View.CboOpenDoc.SelectedIndex == 0)
                isNewDoc = true;
            else
                isNewDoc = false;

            LoadAccountDocuments(int.Parse(e.Value.DataKey), true, true);

        }





        private void LoadAccountDocuments(int accountID, bool showAll, bool showError)
        {

            pw = new ProcessWindow("Loading Documents ...");


            //Reset Qty In Use for this user.
            //service.ResetQtyInUse(new ProductInventory { Location = App.curLocation, CreatedBy = App.curUser.UserName });


            View.Model.CurrentDetails = null;
            View.Model.OrdersData = null;

            docLines = service.GetDocumentLine(
                 new DocumentLine
                 {
                     Document = new Document
                     {
                         Customer = new Account { AccountID = accountID },
                         DocStatus = new Status { StatusID = DocStatus.PENDING },
                         DocType = new DocumentType { DocTypeID = SDocType.SalesOrder },
                         Location = App.curLocation
                     },
                     LineStatus = new Status { StatusID = DocStatus.PENDING }
                 }
             ).Where(f => f.LineStatus.StatusID == DocStatus.New) // || f.QtyBackOrder > 0 || f.QtyCancel > 0 
             .Where(f=>f.Product.Status.StatusID == EntityStatus.Active)
             //.Where(f => f.QtyPendingShip > 0)
             .ToList();


            if ((docLines == null || docLines.Count() == 0) && isNewDoc)
            {
                if (showError)
                {
                    Util.ShowError("No pending documents for the Customer selected.");
                    View.TabStep.Visibility = Visibility.Hidden;
                }


                pw.Close();
                return;
            }

            //Account & Address
            View.Model.Customer = docLines.Select(f => f.Document.Customer).First();
            View.Model.CustomerAddress = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = accountID} });
            View.Model.CustomerAddress.Insert(0,new AccountAddress { ErpCode = "NEW", FullDesc = "Add New Address ... " });
            View.CboShipTo.Items.Refresh();

            //Seleccionando Documentos.
            if (showAll)
                View.Model.OrdersDetail = docLines.OrderBy(f => f.Document.DocNumber).ToList();
            else
                View.Model.OrdersDetail = docLines;

            IEnumerable<Document> docList = docLines.Select(f => f.Document).Distinct();
            View.Model.OrdersData = GenerateMasterData(docList);


            pw.Close();



            if (showAll)
                LoadAllDetails();
            else
            {
                //select first
                View.DgDocument.Items.Refresh();
                View.DgDocument.SelectedIndex = 0;
                LoadDetails(View.Model.OrdersData.Rows[0]);
            }

            View.TabStep.SelectedIndex = 0;
        }


        private void LoadAllDetails()
        {
            TotVolume = TotWeight = 0;
            pw = new ProcessWindow("Loading lines for all documents ... ");

            View.Model.CurrentDetails = View.Model.OrdersDetail;


            //Ejecuta la disponibilidad para los current details       
            //if (View.Model.CurrentDetails.Any(f => f.IsDebit != true))
            //{


            #region INVENTORY LINE BY LINE

            //1. Obtener el Stock de los productos de los documentos de se cliente.
            View.Model.DocumentProductStock = service.GetDocumentProductStock(new Document
            {
                Customer = new Account { AccountID = View.Model.Customer.AccountID },
                Location = App.curLocation,
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder }//,
                //DocStatus = new Status { StatusID = DocStatus.PENDING }
            }, null);

            //Allocated, Available de los productos del documento.
            //Obtiene la lista de productos para obtener su stock en uso (Product Inventory).
            //Allocated in this moment
            List<int> productList = new List<int>();
            foreach (Int32 p in View.Model.CurrentDetails.Select(f => f.Product.ProductID).Distinct())
                productList.Add(p);

            //Inventario en Use de esos productos
            productInUseList = service.GetProductInUseForMerged(productList, App.curLocation);

            double qtyAvailable, qtyInUse;
            //ProductStock piInUse;

            for (int i = 0; i < View.Model.CurrentDetails.Count; i++)
            {
                qtyInUse = 0;
                qtyAvailable = 0;

                if (View.Model.CurrentDetails[i].IsDebit == true)
                    continue;

                if (View.Model.CurrentDetails[i].IsDebit == null)
                    View.Model.CurrentDetails[i].IsDebit = false;

                View.Model.CurrentDetails[i].Note = View.Model.CurrentDetails[i].Document.DocNumber;

                //QTY AVAILABLE DE INVENTARIO - Debe Restarsele lo allocated y lo que este en uso
                //que sale de la clase product Inventory.
                try
                {
                    qtyAvailable = View.Model.DocumentProductStock
                       .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                       .First().FullStock;
                }
                catch { qtyAvailable = 0; }

                qtyAvailable = (double)((int)(qtyAvailable * availabilityMark));
                View.Model.CurrentDetails[i].QtyOnHand = qtyAvailable; //ON HAND

                //Cantidad en Uso - PROGRAMA. Allocated y Reservada
                try
                {
                    //Otros documentos
                    qtyInUse = productInUseList
                       .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                       .Sum(f => f.Stock);
                }
                catch { }


                qtyAvailable -= qtyInUse;  //Finalmente la cantidad que se puede usar.

                //Cantidad final disponible
                if (firstTime)
                {
                    View.Model.CurrentDetails[i].QtyInvoiced = qtyInUse; //Allocated

                    View.Model.CurrentDetails[i].QtyAvailable = ((qtyAvailable > 0) ? qtyAvailable : 0) / View.Model.CurrentDetails[i].Unit.BaseAmount;
                }


                View.Model.CurrentDetails[i].QtyAllocated = 0; //qtyToAllocated;

                View.Model.CurrentDetails[i].Sequence = 0; //BALANCE

                View.Model.CurrentDetails[i].LineWeight = View.Model.CurrentDetails[i].Quantity * View.Model.CurrentDetails[i].Product.Weight * View.Model.CurrentDetails[i].Unit.BaseAmount;

                View.Model.CurrentDetails[i].LineVolume = View.Model.CurrentDetails[i].Quantity * View.Model.CurrentDetails[i].Product.Volume * View.Model.CurrentDetails[i].Unit.BaseAmount;


                //TOTALES
                TotWeight += View.Model.CurrentDetails[i].LineWeight;
                TotVolume += View.Model.CurrentDetails[i].LineVolume;
            }

            #endregion


            View.Model.Totals = new List<DocumentLine>() { new DocumentLine { LineWeight = TotWeight, 
                LineVolume = TotVolume, QtyInvoiced = 75.03 } };

            pw.Close();

            firstTime = false;
            View.DgDetails.Items.Refresh();
            View.TabStep.Visibility = Visibility.Visible;
        }


        private DataColumn GetDoubleColumn(string columnName)
        {
            return new DataColumn { ColumnName = columnName, ReadOnly = false, DataType = typeof(double)};
        }



        public DataTable GenerateMasterData(IEnumerable<Document> docList)
        {

            DataTable master = new DataTable("Orders");

            DataColumn col = new DataColumn { ColumnName = "Selected", ReadOnly = false, DataType = typeof(bool), DefaultValue = false };
            master.Columns.Add(col);
            master.Columns.Add("DocID");
            master.Columns.Add("Date1");
            master.Columns.Add("Status");
            master.Columns.Add("Reference");
            master.Columns.Add("CustPONumber");
            master.Columns.Add("ShipMethod");
            master.Columns.Add("Date2");
            master.Columns.Add("Date3");
            master.Columns.Add("DocType");
            master.Columns.Add("DocNumber");
            master.Columns.Add("Comment");
            master.Columns.Add("Hazmat");
            master.Columns.Add("LinkDocNumber");
            master.Columns.Add("Notes");
            master.Columns.Add("AssignedUsers");

            foreach (Document document in docList.Distinct())           
                master.Rows.Add(GetRowDocument(document, master.NewRow()));
            

            return master;
        }




        private System.Data.DataRow GetRowDocument(Document document, System.Data.DataRow curRow)
        {

            curRow["DocID"] = document.DocID;
            curRow["Date1"] = document.Date1;
            
            try { curRow["Status"] = document.DocStatus.Name; }
            catch { }

            curRow["Reference"] = document.Reference;
            curRow["CustPONumber"] = document.CustPONumber;

            try { curRow["ShipMethod"] = document.ShippingMethod.Name; }
            catch { }

            curRow["Date2"] = document.Date2;
            curRow["Date3"] = document.Date3;
            curRow["DocType"] = document.DocType.Name;
            curRow["Comment"] = document.Comment;
            curRow["DocNumber"] = document.DocNumber;
            //Valores de las columnas adicionales;
            curRow["Hazmat"] = docLines.Where(f => f.Document.DocID == document.DocID && !string.IsNullOrEmpty(f.Product.Quality)).Count() > 0 ? "HAZMAT" : "";
            curRow["LinkDocNumber"] = docLines.Where(f => f.Document.DocID == document.DocID &&  !string.IsNullOrEmpty(f.LinkDocNumber)).Count() > 0 ? "YES" : "";
            curRow["Notes"] = document.Notes;
            curRow["AssignedUsers"] = document.AssignedUsers;

            return curRow;
        }

  

        private void LoadAccounts()
        {
            //1. Load Customers
            IList<ShowData> list = service.GetDocumentAccount(new Document
            {
                //Customer = new Account { AccountID = e.Value.AccountID },
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder },
                DocStatus = new Status { StatusID = DocStatus.New } //,
                //Location = App.curLocation
            }, AccntType.Customer, true
                );

            if (list == null || list.Count == 0)
            {
                Util.ShowError("No Customers found.");
                View.TabStep.Visibility = Visibility.Hidden;
                return;
            }

            View.Model.CustomerList = list;
        }

        #endregion



        #region TAB 2 - Creating Merged Document



        void View_EnlistDetails(object sender, EventArgs e)
        {
            try
            {
                View.ExpAddinfo.IsEnabled = false;

                if (View.Model.OrdersDetail == null && isNewDoc)
                {
                    View.TabStep.SelectedIndex = 0;
                    return;
                }

                //if (View.Model.OrdersDetail.Where(f => f.IsDebit == true && f.QtyAllocated > 0 && f.Sequence == 0).Count() == 0)
                if (View.Model.OrdersDetail.Where(f => f.IsDebit == true && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) == 0).Count() == 0)
                {                   
                    if (isNewDoc)
                    {
                        Util.ShowError("No lines selected to merge. Or lines contain problems in quantities.");
                        View.TabStep.SelectedIndex = 0;
                        return;
                    }
                }


                if (isNewDoc && View.Model.OrdersDetail.Where(f => f.IsDebit == true && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) != 0).Count() > 0)
                {
                    Util.ShowError("Some lines contain problems in quantities and will not added to the list. Please check.");
                    //return;
                }

                if (isNewDoc)
                    View.Model.SelectedLines = new List<DocumentLine>();
                else
                {
                    //Actualizando campos del documento existente como lineas, comentarios y direccion
                    View.Model.SelectedLines = service.GetDocumentLine(new DocumentLine { Document = new Document { DocID = View.Model.CurOpenDoc.DocID } });                    
                    View.TxtComments.Text = View.Model.CurOpenDoc.Comment;
                    try
                    {
                        View.CboShipTo.SelectedItem = View.Model.CustomerAddress.Where(f => f.AddressID == int.Parse(View.Model.CurOpenDoc.UserDef1)).First();
                    }
                    catch { }
                    View.ExpAddinfo.IsEnabled = true;

                    //Additional Info
                    LoadAdditionalInfo();

                }


                //Adiciona las lineas
                foreach (DocumentLine line in View.Model.OrdersDetail.Where(f => f.IsDebit == true
                    && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) == 0).OrderBy(f => f.Document.DocID))
                {
                    View.Model.SelectedLines.Add(line);
                }
                View.DgSelected.Items.Refresh();




            }
            catch
            {

                Util.ShowError("No lines selected to merge.");

                if (isNewDoc)
                {
                    View.TabStep.SelectedIndex = 0;
                    return;
                }
            }

        }



        private void LoadAdditionalInfo()
        {
            try
            {
                EntityExtraData exData = service.GetEntityExtraData(new EntityExtraData
                    {
                        Entity = new ClassEntity { ClassEntityID = EntityID.Document },
                        EntityRowID = View.Model.CurOpenDoc.DocID
                    }).First();

                //Obtiene el XML en un DataTable
                System.Data.DataRow dr = Util.GetDataSet(exData.XmlData).Tables[0].Rows[0];
                string curName;
                string curVal;

                foreach (UIElement uiE in View.StkAddInfo.Children)
                {
                    try
                    {
                        if (((StackPanel)uiE).Children.Count > 0)


                            foreach (UIElement uiE2 in ((StackPanel)uiE).Children)

                                try
                                {
                                    if (((StackPanel)uiE2).Children.Count > 0)

                                        foreach (UIElement uiE3 in ((StackPanel)uiE2).Children)
                                        {
                                            try
                                            {
                                                curName = uiE3.GetType().GetProperty("Uid").GetValue(uiE3, null).ToString();
                                                curVal = dr[curName].ToString();

                                                if (curName.StartsWith("_"))
                                                    uiE3.GetType().GetProperty("Text").SetValue(uiE3, curVal, null);
                                            }
                                            catch { }
                                        }
                                }
                                catch { }

                    }
                    catch { }

                }


            }
            catch { }



        }



        void View_CreateMergedDocument(object sender, EventArgs e)
        {

            if (View.CboShipTo.SelectedItem == null)
            {
                Util.ShowError("Ship to Address is required.");
                return;
            }


            AccountAddress curAddr = View.CboShipTo.SelectedItem as AccountAddress;
            Document document;

            if (View.Model.CurOpenDoc == null || View.Model.CurOpenDoc.DocID == 0)
            {
                document = new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.MergedSalesOrder },
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Location = App.curLocation,
                    Company = App.curCompany,
                    IsFromErp = true,
                    CrossDocking = false,
                    Date1 = DateTime.Now,
                    UseAllocation = true,
                    //Guarda el ShrtAge - BACKORDER OR CANCEL
                    CustPONumber = "", //View.CboToDo.SelectedIndex == 0 ? ShortAge.BackOrder : ShortAge.Cancel,
                    Customer = View.Model.Customer,
                    Comment = View.TxtComments.Text,
                    UserDef1 = curAddr.AddressID.ToString()
                };
            }
            else
            {
                document = View.Model.CurOpenDoc;
                document.Comment = View.TxtComments.Text;
                document.UserDef1 = curAddr.AddressID.ToString();
            }


            //Address.
            DocumentAddress selectedAddress = new DocumentAddress
            {
                AddressLine1 = curAddr.AddressLine1,
                AddressLine2 = curAddr.AddressLine2,
                AddressLine3 = curAddr.AddressLine3,
                AddressType = AddressType.Shipping,
                City = curAddr.City,
                ContactPerson = curAddr.ContactPerson,
                Country = curAddr.Country,
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                Name = curAddr.Name,
                Phone1 = curAddr.Phone1,
                Phone2 = curAddr.Phone2,
                State = curAddr.State,
                ZipCode = curAddr.ZipCode
            };


            try
            {
                document = service.CreateMergedDocumentV2(document, View.Model.SelectedLines.ToList(), null, //View.Model.Pickers
                    new List<DocumentAddress> { selectedAddress });

                View.Model.CurOpenDoc = document;
                View.Model.SelectedLines = service.GetDocumentLine(new DocumentLine { Document = new Document { DocID = View.Model.CurOpenDoc.DocID } });
                View.DgSelected.Items.Refresh();
                View.ExpAddinfo.IsEnabled = true;

                ResetTab();

                Util.ShowMessage("Merged Document #" + document.DocNumber + " was saved.\nTo process document please go to Shipping Process.");
                

            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be saved.\n" + ex.Message);
            }

        }


        private void ResetTab()
        {
            //Bloquear Boton.
            //Llimpiar selected
            //View.Model.SelectedLines = null;
            View.Model.CurrentDetails = null;
            View.Model.OrdersDetail = null;
            View.Model.OrdersData = null;
            firstTime = true;

            //Clear texbox
            //View.DtmDOB.Text = "";
            //View.DtmIPS.Text = "";
            //View.UcPort.txtData.Text = "";
            //View.TxtComments.Text = "";


            //View.CboAccount.SelectedIndex = -1;
            //View.TabStep.Visibility = Visibility.Hidden;
            //View.TabStep.SelectedIndex = 0;
        }


        void View_UpdateAdditionalInfo(object sender, EventArgs e)
        {
            try
            {

                string xmlMsg = "";
                string curName = "";
                //Envia el XM con la informacion guardada adicional
                foreach (UIElement uiE in View.StkAddInfo.Children)
                {
                    try
                    {
                        if (((StackPanel)uiE).Children.Count > 0)


                            foreach (UIElement uiE2 in ((StackPanel)uiE).Children)

                                try
                                {
                                    if (((StackPanel)uiE2).Children.Count > 0)

                                        foreach (UIElement uiE3 in ((StackPanel)uiE2).Children)
                                        {
                                            try
                                            {
                                                curName = uiE3.GetType().GetProperty("Uid").GetValue(uiE3, null).ToString();

                                                if (curName.StartsWith("_"))
                                                {
                                                    //xmlMsg += curName;
                                                    //xmlMsg += uiE3.GetType().GetProperty("Text").GetValue(uiE3, null).ToString();
                                                    xmlMsg += "\t<" + curName + ">" + uiE3.GetType().GetProperty("Text").GetValue(uiE3, null).ToString() + "</" + curName + ">\n";
                                                }
                                            }
                                            catch { }
                                        }
                                }
                                catch { }

                    }
                    catch { }

                }

                xmlMsg = "<Data>\n" + xmlMsg + "</Data>\n";

                EntityExtraData exData = new EntityExtraData
                {
                    Entity = new ClassEntity { ClassEntityID = EntityID.Document },
                    EntityRowID = View.Model.CurOpenDoc.DocID
                };



                try { exData = service.GetEntityExtraData(exData).First(); }
                catch { }

                exData.XmlData = xmlMsg;


                //Guardar la data adicional.
                if (exData.RowID == 0)
                {
                    exData.CreatedBy = App.curUser.UserName;
                    exData.CreationDate = DateTime.Now;
                    service.SaveEntityExtraData(exData);
                }
                else
                {
                    exData.ModifiedBy = App.curUser.UserName;
                    exData.ModDate = DateTime.Now;
                    service.UpdateEntityExtraData(exData);
                }

                Util.ShowMessage("Additional Information Updated.");

            }
            catch
            {
                Util.ShowError("Additional Information could not be updated.");
            }

        }



        private void LoadTemplateList()
        {
            View.Model.TemplateList = service.GetProcessEntityResource(new ProcessEntityResource
            {
                Entity = new ClassEntity { ClassEntityID = EntityID.DocumentType },
                EntityRowID = SDocType.MergedSalesOrder
            })
            .Select(f => f.Template).ToList();
        }




        #endregion


        internal void UpdateLines(DocumentLine docLine)
        {
            if (docLine.Note == "NEW")
            {

                //Pner en la linea la info del stock de producto.
                #region INVENTORY LINE BY LINE

                //1. Obtener el Stock de los productos de los documentos de se cliente.
                IList<ProductStock> curStock = service.GetDocumentProductStock(new Document
                {
                    Customer = new Account { AccountID = View.Model.Customer.AccountID },
                    Location = App.curLocation,
                    DocType = new DocumentType { DocTypeID = SDocType.SalesOrder }//,
                    //DocStatus = new Status { StatusID = DocStatus.PENDING }
                }, docLine.Product);

                //Allocated, Available de los productos del documento.
                //Obtiene la lista de productos para obtener su stock en uso (Product Inventory).
                //Allocated in this moment
                List<int> productList = new List<int> { docLine.Product.ProductID };

                //Inventario en Use de esos productos
                productInUseList = service.GetProductInUseForMerged(productList, App.curLocation);

                double qtyAvailable, qtyInUse;
                //ProductStock piInUse;

                    if (docLine.IsDebit == null)
                        docLine.IsDebit = false;

                    docLine.Note = docLine.Document.DocNumber;

                    //QTY AVAILABLE DE INVENTARIO - Debe Restarsele lo allocated y lo que este en uso
                    //que sale de la clase product Inventory.
                    try
                    {
                        qtyAvailable = curStock
                           .Where(f => f.Product.ProductID == docLine.Product.ProductID)
                           .First().FullStock;
                    }
                    catch { qtyAvailable = 0; }

                    qtyAvailable = (double)((int)(qtyAvailable * availabilityMark));
                    docLine.QtyOnHand = qtyAvailable; //ON HAND

                    //Cantidad en Uso - PROGRAMA. Allocated y Reservada
                    try
                    {
                        //Otros documentos
                        qtyInUse = productInUseList
                           .Where(f => f.Product.ProductID == docLine.Product.ProductID)
                           .Sum(f => f.Stock);

                        //qtyInUse = piInUse.QtyAllocated + piInUse.QtyInUse;
                    }
                    catch { qtyInUse = 0; }

                    qtyAvailable -= qtyInUse;  //Finalmente la cantidad que se puede usar.

                    //Cantidad final disponible
                    docLine.QtyInvoiced = qtyInUse; //Allocated
                    docLine.QtyAvailable = ((qtyAvailable > 0) ? qtyAvailable : 0) / docLine.Unit.BaseAmount;
                    docLine.QtyAllocated = 0; //qtyToAllocated;
                    docLine.Sequence = 0; //BALANCE


                #endregion


                //Adiciona las lineas
                View.Model.OrdersDetail.Add(docLine);

                if(View.ChkFilter.IsChecked == true)
                    View.Model.CurrentDetails.Add(docLine);

            }
            else
            {
                try
                {
                    View.Model.OrdersDetail.Where(f => f.LineID == docLine.LineID).First().Quantity = docLine.Quantity;
                    View.Model.OrdersDetail.Where(f => f.LineID == docLine.LineID).First().QtyBackOrder = docLine.QtyBackOrder;
                    View.Model.OrdersDetail.Where(f => f.LineID == docLine.LineID).First().QtyCancel = docLine.QtyCancel;
                    View.Model.OrdersDetail.Where(f => f.LineID == docLine.LineID).First().LineDescription = docLine.LineDescription;
                }
                catch { }

                try
                {
                    View.Model.CurrentDetails.Where(f => f.LineID == docLine.LineID).First().Quantity = docLine.Quantity;
                    View.Model.CurrentDetails.Where(f => f.LineID == docLine.LineID).First().QtyBackOrder = docLine.QtyBackOrder;
                    View.Model.CurrentDetails.Where(f => f.LineID == docLine.LineID).First().QtyCancel = docLine.QtyCancel;
                    View.Model.CurrentDetails.Where(f => f.LineID == docLine.LineID).First().LineDescription = docLine.LineDescription;
                }
                catch { }

                /*
                try
                {
                        DocumentLine oldLine = View.Model.OrdersDetail.Where(f => f.LineID == docLine.LineID).First();
                        View.Model.OrdersDetail.Remove(oldLine);
                        View.Model.OrdersDetail.Add(docLine);

                    if (View.ChkFilter.IsChecked == true) {
                        DocumentLine oldCurLine = View.Model.CurrentDetails.Where(f => f.LineID == docLine.LineID).First();
                        View.Model.CurrentDetails.Remove(oldCurLine);
                        View.Model.CurrentDetails.Add(docLine);
                    }
      
                }
                catch { }
                */
            }

            View.DgDetails.Items.Refresh();
        }
    }
}
