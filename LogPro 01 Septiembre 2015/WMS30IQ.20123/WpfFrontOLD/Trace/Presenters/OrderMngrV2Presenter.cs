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


    public interface IOrderMngrV2Presenter
    {
        IOrderMngrV2View View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class OrderMngrV2Presenter : IOrderMngrV2Presenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }

        public IOrderMngrV2View View { get; set; }
        
        ProductInventory piInUse = null;
        IList<ProductStock> productInUseList = null;
        IList<ProductStock> documentStock = null;
        bool showMessage = true;
        ProcessWindow pw = null;
        double availabilityMark = 0;
        bool firstTime = false;


        public OrderMngrV2Presenter(IUnityContainer container, IOrderMngrV2View view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.region = region;
                this.service = new WMSServiceClient();
                View.Model = this.container.Resolve<OrderMngrV2Model>();


                //Delegates 

                View.LoadDetails += new EventHandler<EventArgs>(View_LoadDetails);

                View.LineChecked += new EventHandler<DataEventArgs<long>>(View_LineChecked);

                View.LineUnChecked += new EventHandler<DataEventArgs<long>>(View_LineUnChecked);

                View.CreateMergedDocument += new EventHandler<EventArgs>(View_CreateMergedDocument);

                View.EnlistDetails += new EventHandler<EventArgs>(View_EnlistDetails);

                View.RefineSearch += new EventHandler<EventArgs>(View_RefineSearch);

                View.BOLineSelected += new EventHandler<DataEventArgs<Product>>(View_BOLineSelected);



                View.DgDetails.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 300;
                View.DgDetails.MinHeight = SystemParameters.FullPrimaryScreenHeight - 300;

                availabilityMark = 1;

                LoadDocuments(true);

                LoadLastestDocuments();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error loading view.\n" + ex.Message);
            }
        }



        void View_BOLineSelected(object sender, DataEventArgs<Product> e)
        {
            Connection local = service.GetConnection(new Connection {Name = "LOCAL"}).First();

            View.Model.VendorDetails = service.DirectSQLQuery("SELECT * FROM vwBackOrderData WHERE ITEMNMBR= '" + e.Value.ProductCode +"'", "", "ITEMVEND", local);
            //View.Model.VendorDetails = service.DirectSQLQuery("SELECT top 10 * FROM vwBackOrderData ", "", "ITEMVEND", local);
        }



        void View_RefineSearch(object sender, EventArgs e)
        {
            LoadDocuments(false);
        }


        private void LoadLastestDocuments()
        {
            //loading Last Document List
            View.UCDocList.CurDocumentType = service.GetDocumentType(new DocumentType { DocTypeID = SDocType.SalesOrder }).First();
            View.UCDocList.LoadDocuments("FROMBO");
        }



        #region TAB1 - Selecting Lines


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

                try { curLine = View.Model.CurrentDetails.Where(f => f.LineID == lineID).First(); }
                catch { return; }


                double qtyAllocated = 0, qtyAvailable = 0;

                if (curLine.QtyAllocated == 0)
                {

                    qtyAvailable = curLine.QtyOnHand; //curLine.QtyShipped; //On Hand

                    //curLine.QtyInvoiced = GetProductInUse(curLine.Product);
                    
                    qtyAvailable -= curLine.QtyInvoiced;

                    if (qtyAvailable > 0)
                    {
                        //Se hacen los calculos del Allocation.
                        qtyAllocated = curLine.Quantity - curLine.QtyCancel - curLine.QtyBackOrder;

                        if (qtyAllocated > (qtyAvailable/curLine.Unit.BaseAmount))
                            qtyAllocated = (qtyAvailable / curLine.Unit.BaseAmount);


                        curLine.QtyAllocated = qtyAllocated;
                        qtyAvailable -= qtyAllocated * curLine.Unit.BaseAmount;

                    }
                    else
                    {
                        curLine.QtyAllocated = 0;
                        curLine.QtyAvailable = 0;
                    }



                    //Cambiando el Qty Pending donde se use el mismo producto.
                    for (int i = 0; i < View.Model.CurrentDetails.Count; i++)
                    {
                        if (View.Model.CurrentDetails[i].Product.ProductID == curLine.Product.ProductID)
                        {
                            View.Model.CurrentDetails[i].QtyAvailable = (qtyAvailable > 0 ? qtyAvailable : 0) / View.Model.CurrentDetails[i].Unit.BaseAmount;
                            View.Model.CurrentDetails[i].QtyInvoiced += qtyAllocated;
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
            try { curLine = View.Model.CurrentDetails.Where(f => f.LineID == lineID).First(); }
            catch { return;  }

            if (curLine.QtyAllocated <= 0)
                return;

            try
            {

                //Cambiando el Qty Pending donde se use el mismo producto.
                for (int i = 0; i < View.Model.CurrentDetails.Count; i++)
                {
                    if (View.Model.CurrentDetails[i].Product.ProductID == curLine.Product.ProductID)
                    {
                        View.Model.CurrentDetails[i].QtyAvailable += curLine.QtyAllocated;
                        View.Model.CurrentDetails[i].QtyInvoiced -= curLine.QtyAllocated;

                        if (View.Model.CurrentDetails[i].QtyInvoiced < 0)
                            View.Model.CurrentDetails[i].QtyInvoiced = 0;
                    }
                }

                curLine.QtyAllocated = 0;

            }
            catch { }

            //piInUse = null;
        }


        void View_LoadDetails(object sender, EventArgs e)
        {
            LoadDocuments(false); 
        }



        //Propiedades adicionales
        IList<DocumentLine> docLines;




        private void LoadDocuments(bool firstTime)
        {

            pw = new ProcessWindow("Loading Document Lines ...");


            //Carga las lineas segun los criterios de Cliente, Orden, Item

            View.Model.CurrentDetails = null;

            //FILTERS

            Product product = (View.CboItem.SelectedItem == null || ((Product)View.CboItem.SelectedItem).ProductID == 0 ) ? 
                new Product() : (Product)View.CboItem.SelectedItem;

            Account account = (View.CboAccount.SelectedItem == null || ((Account)View.CboAccount.SelectedItem).AccountID == 0) ? 
                new Account() : (Account)View.CboAccount.SelectedItem;

            Document document = (View.CboOrder.SelectedItem == null || ((Document)View.CboOrder.SelectedItem).DocID == 0) ? 
                new Document() : (Document)View.CboOrder.SelectedItem;


            docLines = service.GetDocumentLine(
                 new DocumentLine
                 {
                     Document = new Document
                     {
                         DocID = document.DocID,
                         Customer = new Account { AccountID = account.AccountID },
                         DocStatus = new Status { StatusID = DocStatus.PENDING },
                         DocType = new DocumentType { DocTypeID = SDocType.BackOrder },
                         Location = App.curLocation
                     },
                     LineStatus = new Status { StatusID = DocStatus.PENDING },
                     Product = new Product { ProductID = product.ProductID }
                 }
             ).Where(f => f.LineStatus.StatusID == DocStatus.New)
             .Where(f => f.Product.Status.StatusID == EntityStatus.Active)
             .ToList();


            if (docLines == null || docLines.Count() == 0)
            {

                Util.ShowError("No found records for the filters selected.");
                View.TabStep.Visibility = Visibility.Hidden;

                pw.Close();
                return;
            }

            if (firstTime)
            {
                View.Model.CustomerList = docLines.Select(f => f.Document.Customer).Distinct().OrderBy(f=>f.AccountCode).ToList();
                View.Model.CustomerList.Insert(0, new Account { FullDesc = "All Customers" });

                View.Model.ItemList = docLines.Select(f => f.Product).Distinct().OrderBy(f=>f.ProductCode).ToList();
                View.Model.ItemList.Insert(0, new Product { FullDesc = "All Items" });

                View.Model.DocAdminList = docLines.Select(f => f.Document).Distinct().OrderBy(f=>f.DocNumber).ToList();
                View.Model.DocAdminList.Insert(0, new Document { DocNumber = "All Documents" });
            }
            


            View.Model.CurrentDetails = docLines.OrderBy(f => f.Document.DocNumber).ToList();


            pw.Close();
            LoadAllDetails();

            View.TabStep.SelectedIndex = 0;
        }
       


        private void LoadAllDetails()
        {

            pw = new ProcessWindow("Loading lines for all documents ");

            #region INVENTORY LINE BY LINE

            //1. Obtener el Stock de los productos de los documentos de se cliente.
            View.Model.DocumentProductStock = service.GetDocumentProductStock(new Document
            {
                Location = App.curLocation,
                DocType = new DocumentType { DocTypeID = SDocType.BackOrder }                
            }, null);


            double qtyAvailable;

            for (int i = 0; i < View.Model.CurrentDetails.Count; i++)
            {
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


                if (qtyAvailable >= View.Model.CurrentDetails[i].Quantity)
                    View.Model.CurrentDetails[i].QtyBackOrder = View.Model.CurrentDetails[i].Quantity;
                else
                    View.Model.CurrentDetails[i].QtyBackOrder = qtyAvailable;


                //Cantidad final disponible
                //if (firstTime)
                    View.Model.CurrentDetails[i].QtyAvailable = ((qtyAvailable > 0) ? qtyAvailable : 0) / View.Model.CurrentDetails[i].Unit.BaseAmount;
 

            }

            #endregion


            pw.Close();

            firstTime = false;
            View.DgDetails.Items.Refresh();
            View.TabStep.Visibility = Visibility.Visible;
        }



        #endregion



        #region TAB 2 - Creating Merged Document



        void View_EnlistDetails(object sender, EventArgs e)
        {
            try
            {
                if (View.Model.CurrentDetails == null)
                {
                    View.TabStep.SelectedIndex = 0;
                    return;
                }

                //if (View.Model.CurrentDetails.Where(f => f.IsDebit == true && f.QtyAllocated > 0 && f.Sequence == 0).Count() == 0)
                if (View.Model.CurrentDetails.Where(f => f.IsDebit == true && f.Quantity > 0).Count() == 0)
                {
                    Util.ShowError("No lines selected to merge. Or lines contain problems in quantities.");
                    View.TabStep.SelectedIndex = 0;
                    return;
                }


                if (View.Model.CurrentDetails.Where(f => f.IsDebit == true && f.Quantity  <= 0).Count() > 0)
                {
                    Util.ShowError("Some lines contain problems in quantities and will not added to the list. Please check.");
                    //return;
                }


                View.Model.SelectedLines = View.Model.CurrentDetails.Where(f => f.IsDebit == true
                    && f.Quantity > 0).OrderBy(f => f.Document.DocID).ToList(); //Marked

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

            Document document = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder },
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                Location = App.curLocation,
                Company = App.curCompany,
                IsFromErp = true,
                CrossDocking = false,
                Date1 = DateTime.Now,
                UseAllocation = true,
                CustPONumber = "",
                Comment = View.TxtComments.Text,
                UserDef1 = "FROMBO"
            };


            try
            {
                string result = service.CreateMergedDocumentForBackOrder(document,
                    View.Model.SelectedLines.ToList(), null, null, View.CboProcess.SelectedIndex);
                //Process 0 = BY DOCUMENT: Create One new Sales Order by Document
                //1 = BY CUSTOMER: Create One new Sales Order by Customer

                ResetTab();

                Util.ShowMessage(result + "\nTo pick/ship documents please go to Shipping Process.");
                

            }
            catch (Exception ex)
            {
                Util.ShowError("Documents could not be created.\n" + ex.Message);
            }


            /*
            try
            {
                //Open PickTicket for Merged Order. - Document to print
                UtilWindow.ShowDocument(document.DocType.Template, document.DocID, "", false);
            }
            catch (Exception ex)
            {
                Util.ShowError("Pick Ticket could not be displayed.\n" + ex.Message);
            }
            */

        }


        private void ResetTab()
        {
            //Bloquear Boton.
            //Llimpiar selected
            View.Model.SelectedLines = null;
            View.Model.CurrentDetails = null;

            //Clear texbox
            View.TxtComments.Text = "";


            View.CboAccount.SelectedIndex = -1;
            View.TabStep.Visibility = Visibility.Hidden;
            View.TabStep.SelectedIndex = 0;


        }



        #endregion


    }
}
