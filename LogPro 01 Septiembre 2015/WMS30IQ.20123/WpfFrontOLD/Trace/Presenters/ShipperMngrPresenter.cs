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


    public interface IShipperMngrPresenter
    {
        IShipperMngrView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class ShipperMngrPresenter : IShipperMngrPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }

        public IShipperMngrView View { get; set; }
        
        ProductInventory piInUse = null;
        IList<ProductStock> productInUseList = null;
        IList<ProductStock> documentStock = null;
        bool showMessage = true;
        ProcessWindow pw = null;
        double availabilityMark = 0;
        bool firstTime = false;


        public ShipperMngrPresenter(IUnityContainer container, IShipperMngrView view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.region = region;
                this.service = new WMSServiceClient();
                View.Model = this.container.Resolve<ShipperMngrModel>();


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

                View.DateDOBChanged += new EventHandler<DataEventArgs<DateTime?>>(View_DateDOBChanged);
                View.LoadPopupLine += new EventHandler<DataEventArgs<DocumentLine>>(View_LoadPopupLine);

                View.CancelLine += new EventHandler<DataEventArgs<DocumentLine>>(View_CancelLine);
                View.RefreshAddress += new EventHandler<EventArgs>(View_RefreshAddress);





                View.DgDocument.MinHeight = SystemParameters.FullPrimaryScreenHeight - 290;
                View.DgDocument.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 290;
                View.DgDetails.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 300;
                View.DgDetails.MinHeight = SystemParameters.FullPrimaryScreenHeight - 300;

                try {  availabilityMark = double.Parse(Util.GetConfigOption("AVAILMARK")); }
                catch { availabilityMark = 1; }


                //Add Remove Lines
                
                if (Util.AllowOption("REMSOLINE"))
                    View.StkUpdLines.Visibility = Visibility.Visible;
                


                LoadAccounts();
                
                //Cargando los UcPorts
                LoadPorts();

                LoadLastestDocuments();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error loading view.\n" + ex.Message);
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




        void View_DateDOBChanged(object sender, DataEventArgs<DateTime?> e)
        {
            //Desarrollo especifico para image service.
            //Debe poblar los combos necesarios del sistema segun el DOB mirando la vista de IMAGEService
            string strWhere = " WHERE 'Z'+ ShipCode = '" + View.Model.Customer.AccountCode.Trim() + "' AND DOB <= '" + ((DateTime)e.Value).ToString("yyyy-MM-dd") + "' AND DOB > GetDate() ORDER BY DOB";                       
            
            //View.Model.Exit_IPS = service.GetCustomList("vwExternalLoadSchedules", "CONVERT(varchar(10),ExitIPS,101)", strWhere);
            View.Model.Exit_IPS = service.GetCustomList("vwExternalLoadSchedules", "ExitIPS, CONVERT(varchar(10),ExitIPS,101) + ' >> ' + ISNULL(Port,'') + ' >> '+ ISNULL(Voyage,'') ", strWhere);
            //View.UcPort.DefaultList = service.GetCustomList("vwExternalLoadSchedules", "Port", strWhere);
            //View.Model.Voyages = service.GetCustomList("vwExternalLoadSchedules", "Voyage", strWhere);

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
                    //Jun 08 2010
                    //View.Model.CurrentDetails[i].Quantity = View.Model.CurrentDetails[i].Quantity; // - View.Model.CurrentDetails[i].QtyShipped);


                    //Limpiar Qty in use
                    //View.Model.StockInUse = null;
                }

                #endregion

                //productInUseList = null;
                //View.Model.DocumentProductStock = null;

            //}

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

            firstTime = true;
            LoadAccountDocuments(int.Parse(e.Value.DataKey), false, true);

        }



        void View_AccountSelectedAllLines(object sender, DataEventArgs<ShowData> e)
        {
            if (e.Value == null)
                return;

            firstTime = true;
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


            if (docLines == null || docLines.Count() == 0)
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

            pw = new ProcessWindow("Loading lines for all documents ");

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
                           .Sum(f=>f.Stock);                       
                    }
                    catch{}

                        //qtyInUse = piInUse.QtyAllocated + piInUse.QtyInUse;

                    //try
                    //{   
                    //    //Adicionar En uso del order detail de otras lineas y el mismo producto

                    //    qtyInUse += View.Model.OrdersDetail
                    //       .Where(f => f.Product.ProductID == View.Model.CurrentDetails[i].Product.ProductID)
                    //        //&& f.LineID != View.Model.CurrentDetails[i].LineID)
                    //       .Sum(f => f.QtyInvoiced);

                    //}
                    //catch { }

                    qtyAvailable -= qtyInUse;  //Finalmente la cantidad que se puede usar.

                    //Cantidad final disponible
                    if (firstTime)
                    {
                        View.Model.CurrentDetails[i].QtyInvoiced = qtyInUse; //Allocated

                        View.Model.CurrentDetails[i].QtyAvailable = ((qtyAvailable > 0) ? qtyAvailable : 0) / View.Model.CurrentDetails[i].Unit.BaseAmount;
                    }


                    View.Model.CurrentDetails[i].QtyAllocated = 0; //qtyToAllocated;
                    
                    View.Model.CurrentDetails[i].Sequence = 0; //BALANCE

                    //Jun 08 2010
                    //View.Model.CurrentDetails[i].Quantity = View.Model.CurrentDetails[i].Quantity; // - View.Model.CurrentDetails[i].QtyShipped);


                }

                #endregion

                //productInUseList = null;
                //View.Model.DocumentProductStock = null;

            //}

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
                if (View.Model.OrdersDetail == null)
                {
                    View.TabStep.SelectedIndex = 0;
                    return;
                }

                //if (View.Model.OrdersDetail.Where(f => f.IsDebit == true && f.QtyAllocated > 0 && f.Sequence == 0).Count() == 0)
                if (View.Model.OrdersDetail.Where(f => f.IsDebit == true && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) == 0).Count() == 0)
                {
                    Util.ShowError("No lines selected to merge. Or lines contain problems in quantities.");
                    View.TabStep.SelectedIndex = 0;
                    return;
                }


                if (View.Model.OrdersDetail.Where(f => f.IsDebit == true && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) != 0).Count() > 0)
                {
                    Util.ShowError("Some lines contain problems in quantities and will not added to the list. Please check.");
                    //return;
                }


                View.Model.SelectedLines = View.Model.OrdersDetail.Where(f => f.IsDebit == true
                    && (f.Quantity - f.QtyAllocated - f.QtyCancel - f.QtyBackOrder) == 0).OrderBy(f => f.Document.DocID).ToList(); //Marked

                View.DgSelected.Items.Refresh();

            }
            catch
            {

                Util.ShowError("No lines selected to merge.");
                View.TabStep.SelectedIndex = 0;
                return;
            }

        }



        void View_CreateMergedDocument(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(View.DtmDOB.Text))
            {
                Util.ShowError("Date of Board DOB is required.");
                return;
            }

            if (string.IsNullOrEmpty(View.DtmIPS.Text))
            {
                Util.ShowError("ExitIPS Date is required.");
                return;
            }

            //if (string.IsNullOrEmpty(View.UcPort.Text))
            //{
            //    Util.ShowError("Delivery Port is required.");
            //    return;
            //}

            if (View.CboShipTo.SelectedItem == null)
            {
                Util.ShowError("Ship to Address is required.");
                return;
            }


            AccountAddress curAddr = View.CboShipTo.SelectedItem as AccountAddress;

            /*
            string port="",voyage="";

            if (View.CboExitIps.SelectedItem != null)
            {
                ShowData portAndVoyage = ((ShowData)View.CboExitIps.SelectedItem);
                string[] data = portAndVoyage.DataValue.Split(">>".ToCharArray());

                try
                {
                    if (portAndVoyage != null)
                    {
                        port = data[2].Trim();
                        voyage = data[4].Trim();
                    }
                }
                catch { }
            }
            */


            Document document = new Document
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

                Date3 = DateTime.Parse(View.DtmDOB.Text), //DOB
                Date4 = DateTime.Parse(View.DtmIPS.Text), //EXIT IPS       
                Notes =  View.RPort.Text, //port, //View.UcPort.Text,
                Reference = View.RVoyage.Text, //View.CboVoyage.SelectedItem != null ? ((ShowData)View.CboVoyage.SelectedItem).DataValue : ""
                Comment = View.TxtComments.Text
            };

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
                document = service.CreateMergedDocument(document, View.Model.SelectedLines.ToList(), null, //View.Model.Pickers
                    new List<DocumentAddress> { selectedAddress });

                //Refresh document and line available.
                //LoadAccountDocuments(View.Model.Customer.AccountID, (View.ChkFilter.IsChecked == true ? true : false), false);

                ResetTab();

                Util.ShowMessage("Merged Document #" + document.DocNumber + " was created.\nTo process document please go to Shipping Process.");
                

            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be created.\n" + ex.Message);
            }


            try
            {
                //Open PickTicket for Merged Order. - Document to print
                UtilWindow.ShowDocument(document.DocType.Template, document.DocID, "", false);
            }
            catch (Exception ex)
            {
                Util.ShowError("Pick Ticket could not be displayed.\n" + ex.Message);
            }


        }


        private void ResetTab()
        {
            //Bloquear Boton.
            //Llimpiar selected
            View.Model.SelectedLines = null;
            View.Model.CurrentDetails = null;
            View.Model.OrdersDetail = null;
            View.Model.OrdersData = null;

            //Clear texbox
            View.DtmDOB.Text = "";
            View.DtmIPS.Text = "";
            View.UcPort.txtData.Text = "";
            View.TxtComments.Text = "";


            View.CboAccount.SelectedIndex = -1;
            View.TabStep.Visibility = Visibility.Hidden;
            View.TabStep.SelectedIndex = 0;


        }



        private void LoadPorts()
        {
            //IEnumerable<String> portList = service.GetDocument(
            //    new Document { DocType = new DocumentType { DocTypeID = SDocType.MergedSalesOrder } })
            //    .Select(f => f.Notes).Distinct();

            //if (portList == null || portList.Count() == 0)
            //    return;

            //View.UcPort.DefaultList = new List<ShowData>();

            //foreach (String s in portList)
            //{
            //    if (string.IsNullOrEmpty(s))
            //        continue;

            //    View.UcPort.DefaultList.Add(new ShowData { DataKey = s, DataValue = s });
            //}
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
