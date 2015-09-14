using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows;
using WpfFront.Services;
using System.Linq;


namespace WpfFront.Presenters
{

    public interface IUC_IV_Presenter
    {
        IUC_IV_Replanish_PackingView ViewRepPack { get; set; }
        void SetRepPacking(IUC_IV_Replanish_PackingView view);

        IUC_IV_ComparerView ViewComp { get; set; }
        void SetComparer(IUC_IV_ComparerView view);


        ToolWindow Window { get; set; }
    }


    public class UC_IV_Presenter : IUC_IV_Presenter
    {
        public IUC_IV_Replanish_PackingView ViewRepPack { get; set; }
        public IUC_IV_ComparerView ViewComp { get; set; }


        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private ShowData ReplenishSelector;
        public ToolWindow Window { get; set; }
        public bool fullLoaded = false;


        //Constructor
        public UC_IV_Presenter(IUnityContainer container)
        {
            this.container = container;
            this.service = new WMSServiceClient();

        }


        #region Replenishment


        public void SetRepPacking(IUC_IV_Replanish_PackingView view)
        {

            this.ViewRepPack = view;

            ViewRepPack.Model = this.container.Resolve<UC_IV_Model>();

            //DocType
            ViewRepPack.Model.DocType = service.GetDocumentType(new DocumentType { DocTypeID = SDocType.ReplenishPackTask }).FirstOrDefault();

            //Events
            ViewRepPack.ProcessReplenish += new EventHandler<EventArgs>(this.OnProcessReplenish);
            ViewRepPack.LoadReplenishment += new EventHandler<EventArgs>(this.OnLoadReplenish);
            ViewRepPack.SelectAll += new EventHandler<EventArgs>(ViewRepPack_SelectAll);
            ViewRepPack.UnSelectAll += new EventHandler<EventArgs>(ViewRepPack_UnSelectAll);

            
            ViewRepPack.FilterByBin += new EventHandler<DataEventArgs<String>>(ViewRepPack_FilterByBin);
            ViewRepPack.FilterByProduct += new EventHandler<DataEventArgs<String>>(ViewRepPack_FilterByProduct);

            ViewRepPack.ChangeSelector += new EventHandler<DataEventArgs<ShowData>>(ViewRepPack_ChangeSelector);
            ViewRepPack.ClearRecords += new EventHandler<DataEventArgs<bool>>(ViewRepPack_ClearRecords);

            ViewRepPack.Model.ShowProcess = false;

            ViewRepPack.Model.CurLocation = App.curLocation;

            //Setting Selector
            try {
                if (Util.GetConfigOption("STOCKSEL").Equals("1"))
                {
                    ReplenishSelector = ViewRepPack.Model.SelectorList[0];
                    ViewRepPack.CboSelector.SelectedIndex = 0;
                }
                else
                {
                    ReplenishSelector = ViewRepPack.Model.SelectorList[1];
                    ViewRepPack.CboSelector.SelectedIndex = 1;
                }
            }
            catch { 
                ReplenishSelector = ViewRepPack.Model.SelectorList[0];
                ViewRepPack.CboSelector.SelectedIndex = 0;
            }


            //loading Last Document List
            ViewRepPack.UCDocList.CurDocumentType = ViewRepPack.Model.DocType;
            ViewRepPack.UCDocList.LoadDocuments("");

            //LoadRepPack(App.curLocation);
            //FilterData();
        }




        void ViewRepPack_ClearRecords(object sender, DataEventArgs<bool> e)
        {
            if (!fullLoaded && ViewRepPack.ShowEmpty.IsChecked == true)
                LoadRepPack(App.curLocation);

            if (ViewRepPack.ShowEmpty.IsChecked == false)
                ViewRepPack.Model.RepPackList = ViewRepPack.Model.OriRepPackList.Where(f => f.PackStock > 0).ToList();
            else
                ViewRepPack.Model.RepPackList = ViewRepPack.Model.OriRepPackList;

            //FilterData();
        }

        

        
        private void FilterData()
        {
            LoadRepPack(App.curLocation);

            /*
            if (ViewRepPack.Model.OriRepPackList == null)
                LoadRepPack(App.curLocation);

            if (ViewRepPack.Model.OriRepPackList.Count == 0)
                return;


            //Filter by by and product Always.
            IList<ProductStock> repList = ViewRepPack.Model.OriRepPackList;


            //Product 
            string search = ViewRepPack.CboProduct.Text.ToUpper();
            if (!string.IsNullOrEmpty(search))
            {
                repList = ViewRepPack.Model.OriRepPackList.
                        Where(f => f.Product.ProductCode.ToUpper().StartsWith(search) || (search.Length > 2 && f.Product.Name.ToUpper().Contains(search))).ToList();
            }


            //Binrange
            String[] binRange = ViewRepPack.BinRange.Text.Split(':');
            try
            {
                if (!string.IsNullOrEmpty(binRange[0]))
                    repList = repList.Where(f => f.Bin.BinCode.ToUpper().CompareTo(binRange[0].ToUpper()) >= 0 || f.Bin.BinCode.Equals(binRange[0].ToUpper())).ToList();

                if (!string.IsNullOrEmpty(binRange[1]))
                    repList = repList.Where(f => f.Bin.BinCode.ToUpper().CompareTo(binRange[1].ToUpper()) <= 0 || f.Bin.BinCode.Equals(binRange[1].ToUpper())).ToList();
            }
            catch { }


            ViewRepPack.Model.RepPackList = repList;

            //Clear Empty
            if (ViewRepPack.ShowEmpty.IsChecked == false)
                ViewRepPack.Model.RepPackList = ViewRepPack.Model.RepPackList.Where(f => f.PackStock > 0).ToList();

             * */
        }




        void ViewRepPack_ChangeSelector(object sender, DataEventArgs<ShowData> e)
        {
            if (e.Value == null)
                return;

            ReplenishSelector = e.Value;
            LoadRepPack(ViewRepPack.Model.CurLocation);
        }


        void ViewRepPack_FilterByProduct(object sender, DataEventArgs<String> e)
        {
            ProcessWindow pw = new ProcessWindow("Loading Records ... ");

            FilterData();

            pw.Close();
        }



        void ViewRepPack_FilterByBin(object sender, DataEventArgs<String> e)
        {
            ProcessWindow pw = new ProcessWindow("Loading Records ... ");

            FilterData();

            pw.Close();
        }


        void ViewRepPack_UnSelectAll(object sender, EventArgs e)
        {
            for(int i = 0; i < ViewRepPack.Model.RepPackList.Count; i++)
                ViewRepPack.Model.RepPackList[i].Mark = false;

            ViewRepPack.DgRepList.Items.Refresh();

        }

        void ViewRepPack_SelectAll(object sender, EventArgs e)
        {
            for (int i = 0; i < ViewRepPack.Model.RepPackList.Count; i++)
                ViewRepPack.Model.RepPackList[i].Mark = true;

            ViewRepPack.DgRepList.Items.Refresh();
        }



        private void LoadRepPack(Location location)
        {

            //Load the RepPack
            ProcessWindow pw = new ProcessWindow("Loading Replenishment Records ... ");

            ProductStock productStock = new ProductStock { BinType = BinType.Out_Only };

            if (!string.IsNullOrEmpty(ViewRepPack.CboProduct.Text.ToUpper())) {
                productStock.Product = new Product  { ProductCode = ViewRepPack.CboProduct.Text.ToUpper() };
            }

            //Binrange
            String[] binRange = ViewRepPack.BinRange.Text.Split(':');
            string bin1 = "", bin2 = "";
            try
            {
                if (!string.IsNullOrEmpty(binRange[0]))
                    bin1 = binRange[0].ToUpper();

                if (!string.IsNullOrEmpty(binRange[1]))
                    bin2 = binRange[1].ToUpper();
            }
            catch { }


            IList<ProductStock> repList = service.GetReplanishmentList(productStock, location,
                short.Parse(ReplenishSelector.DataKey), (bool)ViewRepPack.ShowEmpty.IsChecked, bin1, bin2);

            if (repList == null || repList.Count == 0)
            {
                Util.ShowError("No replenishment records.");
                ViewRepPack.Model.RepPackList = null;
                ViewRepPack.Model.BinList = null;
                ViewRepPack.Model.ProductList = null;
                pw.Close();
                return;
            }

            //Le avisa al view     que     fue cargado el replenish.
            ViewRepPack.WasLoaded = true;

            if (ViewRepPack.ShowEmpty.IsChecked == true)
                fullLoaded = true;
            else
                fullLoaded = false;


            //Load the Replanish Value.
            for (int i = 0; i < repList.Count; i++)
            {
                if (repList[i].Stock < repList[i].MinStock)
                {
                    repList[i].PackStock = (repList[i].MaxStock - repList[i].Stock) > 0 ? repList[i].MaxStock - repList[i].Stock : 0;

                    if (repList[i].PackStock > repList[i].AuxQty1)
                        repList[i].PackStock = repList[i].AuxQty1;

                    //Anterior solo marca los que completan el minimo
                    //if (repList[i].MinStock > 0 && (repList[i].Stock + repList[i].PackStock) >= repList[i].MinStock)
                        //repList[i].Mark = true;

                    //nuevo, marca todo lo que sea mayor a cero.
                    if (repList[i].MinStock > 0 && (repList[i].Stock + repList[i].PackStock) > 0 )
                        repList[i].Mark = true;

                }


            }

            repList = repList.OrderBy(f => f.Product.ProductCode).ToList();

            ViewRepPack.Model.OriRepPackList = repList; //OrderByDescending(f => f.PackStock).ToList(); //.Where(f => f.PackStock > 0).ToList();
            ViewRepPack.Model.RepPackList = repList; //.Where(f => f.PackStock > 0).ToList();
            ViewRepPack.Model.ShowProcess = true;


            //Load Filters Bin, Product
            //ViewRepPack.Model.BinList = ViewRepPack.Model.RepPackList.Select(f => f.Bin.BinCode).Distinct().ToList();
            //ViewRepPack.Model.ProductList = ViewRepPack.Model.RepPackList.Select(f => f.Product.ProductCode).Distinct().ToList();
            //ViewRepPack.Model.BinList.Add("Show All");
            //ViewRepPack.Model.ProductList.Add("Show All");

            pw.Close();         


        }


        private void OnProcessReplenish(object sender, EventArgs e)
        {
            //ViewRepPack.DgRepList.Items.Refresh();
            IList<ProductStock> list = ViewRepPack.Model.RepPackList
                                        .Where(f => f.PackStock > 0 && f.Mark == true)
                                        .ToList();

            ////foreach (Object obj in ViewRepPack.DgRepList.Items)
            //    if (((ProductStock)obj).Mark && ((ProductStock)obj).PackStock > 0)
            //        list.Add((ProductStock)obj);


            if (list.Count() == 0)
            {
                Util.ShowError("No record selected.");
                return;
            }

            ProcessWindow pw = new ProcessWindow("Creating Replenishment Order ... ");
            //1. Create a Replenishment order (new)
            Document repOrder = service.CreateReplenishOrder(list.ToList(), App.curUser.UserName, App.curLocation);

            pw.Close();

            //Refresh Document List.
            ViewRepPack.UCDocList.LoadDocuments("");

            Util.ShowMessage("Replenishment Order ["+repOrder.DocNumber+"] Created.");

            //Document to print
            UtilWindow.ShowDocument(repOrder.DocType.Template, repOrder.DocID, "", false); 


        }


        private void OnLoadReplenish(object sender, EventArgs e)
        {
            LoadRepPack(App.curLocation); //ViewRepPack.Model.CurLocation
        }


       #endregion



        #region Inventory Conciliation


        public void SetComparer(IUC_IV_ComparerView view)
        {

            this.ViewComp = view;
            ViewComp.Model = this.container.Resolve<UC_IV_Model>();

            ProductStock pstock = new ProductStock{ Bin = new Bin { Location = App.curLocation }};
            ViewComp.Model.RepPackList = service.GetStockComparation(pstock, false, App.curCompany);

        }


        #endregion

    }
}