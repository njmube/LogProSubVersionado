using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.Windows.Forms.Integration;
//using DataDynamics.Analysis.Windows.Forms;
//using DataDynamics.Analysis.Windows.Forms.DataSources;
using System.IO;
using System.Reflection;
using System.Xml;
using DataDynamics.Analysis;
using DataDynamics.Analysis.Layout;
using WpfFront.Common;
using WpfFront.Presenters;
using WpfFront.Common.UserControls;
using Odyssey.Controls;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.Windows.Controls;
using System.Windows.Input;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class InventoryCountView : UserControlBase, IInventoryCountView
    {


        public InventoryCountView()
        {
            InitializeComponent();
            expSetup.IsExpanded = false;
        }

        double curCellQty {get; set; }

        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;


        //Events
        public event EventHandler<DataEventArgs<String>> FilterByBin;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<EventArgs> AddToAssigned;
        public event EventHandler<EventArgs> CreateNewTask;
        public event EventHandler<DataEventArgs<Document>> LoadDetails;
        public event EventHandler<EventArgs> ShowTicket;
        public event EventHandler<EventArgs> ChangeStatus;
        //public event EventHandler<DataEventArgs<ProductStock>> BinTaskSelected;
        public event EventHandler<DataEventArgs<object[]>> ChangeCountedQty;
        public event EventHandler<EventArgs> ConfirmCountTask;
        public event EventHandler<EventArgs> CancelTask;
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<EventArgs> RefreshDocuments;
        public event EventHandler<EventArgs> ReloadDocument;
        public event EventHandler<DataEventArgs<Product>> FilterByProduct;
        public event EventHandler<DataEventArgs<int>> UpdateDocumentOption;
        public event EventHandler<EventArgs> ShowInitialTicket;
        public event EventHandler<EventArgs> LoadNoCountBalance;
        public event EventHandler<EventArgs> SendAdjustment;
        public event EventHandler<EventArgs> ChangeSendOption;
        public event EventHandler<EventArgs> SelectAll;
        public event EventHandler<EventArgs> UnSelectAll;

        public InventoryCountModel Model
        {
            get
            { return this.DataContext as InventoryCountModel; }
            set
            { this.DataContext = value; }

        }

        public Microsoft.Windows.Controls.DatePicker TxtSchDate
        {
            get { return this.txtSchDate; }
            set { this.txtSchDate = value; }
        }

        public TextBlock TxtTitle
        {
            get { return this.txtTitle; }
            set { this.txtTitle = value; }
        }

        public ListView LvAvailable
        {
            get { return this.lvAvailableBins; }
            set { this.lvAvailableBins = value; }

        }
        
        public ListView LvAssigned
        {
            get { return this.lvAsignedBins; }
            set { this.lvAsignedBins = value; }
        }

        public ListView LvAvailableProd
        {
            get { return this.lvAvailableProd; }
            set { this.lvAvailableProd = value; }

        }


        public DataGridControl DgDocument
        {
            get { return this.dgDocument; }
            set { this.dgDocument = value; }
        }

        public StackPanel StkTask
        {
            get { return this.stkTask; }
            set { this.stkTask = value; }
        }

        public OdcExpander ExpSetup
        {
            get { return this.expSetup; }
            set { this.expSetup = value; }
        }


        public OdcExpander ExpExe
        {
            get { return this.expExe; }
            set { this.expExe = value; }
        }


        //public ComboBox ComboStatus
        //{
        //    get { return this.cboStatus; }
        //    set { this.cboStatus = value; }
        //}

        public Button BtnUpdate
        {
            get { return this.btnChangeStatus; }
            set { this.btnChangeStatus = value; }
        }

        public StackPanel BtnRemove
        {
            get { return this.btnRem; }
            set { this.btnRem = value; }
        }

        //public StackPanel LvExeDetail
        //{
        //    get { return this.lvExeSumm; }
        //    set { this.lvExeSumm = value; }
        //}


        public ListView LvSumm
        {
            get { return this.lvSumm; }
            set { this.lvSumm = value; }
        }


        public Button BtnTicket
        {
            get { return this.btnTkt; }
            set { this.btnTkt = value; }
        }

        public Button BtnTicketList
        {
            get { return this.btnTktList; }
            set { this.btnTktList = value; }
        }




        public Button BtnConfirm
        {
            get { return this.btnStep2; }
            set { this.btnStep2 = value; }
        }


        public Button BtnCancel
        {
            get { return this.btnCancel; }
            set { this.btnCancel = value; }
        }


        public StackPanel StkConfirm
        {
            get { return this.stkConfirm; }
            set { this.stkConfirm = value; }
        }

        public ComboBox CboToDo
        {
            get { return this.cboToDo; }
            set { this.cboToDo = value; }
        }

        public StackPanel StkOptions
        {
            get { return this.stkOptions; }
            set { this.stkOptions = value; }
        }

        public StackPanel StkAdjustmentOptions
        {
            get { return this.stkAdjustmentOptions; }
            set { this.stkAdjustmentOptions = value; }
        }

        public Button BtnCfmNoCount
        {
            get { return this.btnStepN; }
            set { this.btnStepN = value; }
        }

        public BinLocation BinRestock
        {
            get { return this.binRestock; }
            set { this.binRestock = value; }
        }

        public ComboBox CboSendOptions
        {
            get { return this.cboSendOptions; }
            set { this.cboSendOptions = value; }
        }

        public StackPanel StkUcBin
        {
            get { return this.stkUcBin; }
            set { this.stkUcBin = value; }
        }


        public CheckBox ChkHideBin
        {
            get { return this.chkHideBin; }
            set { this.chkHideBin = value; }
        }

        public ListView LvSummNoCount
        {
            get { return this.lvSummNoCount; }
            set { this.lvSummNoCount = value; }
        }

        // CAA 
        /*
        public Button BtnFilter
        {
            get { return this.btnFilter; }
            set { this.btnFilter = value; }
        }
        */
        public QueryFilter BFilters
        {
            get { return this.bFilters; }
            set { this.bFilters = value; }
        }

        private void bRange_OnLoadRange(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.bRange.Text))
                return;

            FilterByBin(sender, new DataEventArgs<String>(this.bRange.Text));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddToAssigned(this, e);
        }

        private void btnRemDoc_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }



        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTask(sender, e);
            cboToDo.SelectedIndex = -1;
            stkOptions.Visibility = Visibility.Collapsed;
            StackPanel_f1.Visibility = Visibility.Collapsed;
        }

        private void dgDocument_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadDetails(this, new DataEventArgs<Document>((Document)dgDocument.SelectedItem));
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (cboToDo.SelectedIndex == 0)
            {
                this.lvAvailableBins.SelectAll();
                this.lvAvailableBins.Focus();
            }
            else
            {
                lvAvailableProd.SelectAll();
                lvAvailableProd.Focus();
            }


        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

            //if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            if (cboToDo.SelectedIndex == 0)
                this.lvAvailableBins.UnselectAll();
            else
                this.lvAvailableProd.UnselectAll();


        }


        private void expLabel_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, true);
        }

        private void expLabel_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, false);
        }

        private void CheckExpanders(OdcExpander sender, bool expand)
        {

            if (expSetup == null || expExe == null)
                return;

            if (sender.Name == "expSetup")
            {
                if (expand)
                {
                    //cboProduct.Focus();
                    expExe.IsExpanded = false;
                }
                else
                {
                    expExe.IsExpanded = true;

                }

                return;
            }

            if (sender.Name == "expExe")
            {
                if (expand)
                {
                    expSetup.IsExpanded = false;
                }
                else
                {
                    expSetup.IsExpanded = true;
                    //cboProduct.Focus();
                }

                return;
            }
        }

        private void btnTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowTicket(sender, e);
        }

        private void btnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus(sender, e);
        }


        private void btnStep2_Click(object sender, RoutedEventArgs e)
        {
            //Update Diferences
            ConfirmCountTask(sender, e);
        }

        //private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        BinTaskSelected(this, new DataEventArgs<ProductStock>((ProductStock)e.AddedItems[0]));
        //    }
        //}


        private void txtQty_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox t = ((TextBox)sender);

                if (curCellQty != double.Parse(t.Text))
                {
                    curCellQty = double.Parse(t.Text);
                    ChangeCountedQty(this, new DataEventArgs<object[]>(new object[] { t.Tag, curCellQty }));
                }
            }
            catch {}
        }

        private void txtQty_GotFocus(object sender, RoutedEventArgs e)
        {
            try { curCellQty = double.Parse(((TextBox)sender).Text); }
            catch { curCellQty = 0;  }
        }

        private void lvSumm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
                ListSortDirection direction;

                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                            direction = ListSortDirection.Ascending;
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                                direction = ListSortDirection.Descending;
                            else
                                direction = ListSortDirection.Ascending;
                        }

                        //string header = headerClicked.Column.HeaderStringFormat as string;
                        GridViewColumn colum = headerClicked.Column;
                        string header = ((Binding)(colum.DisplayMemberBinding)).Path.Path;
                        Sort(header, direction, lvSumm.ItemsSource);

                        if (direction == ListSortDirection.Ascending)
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        }
                        else
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        // Remove arrow from previously sorted header
                        if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                            _lastHeaderClicked.Column.HeaderTemplate = null;


                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            catch { }
        }


        private void Sort(string sortBy, ListSortDirection direction, object ItemSource)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ItemSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelTask(sender, e);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }


        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RefreshDocuments(sender, e);
        }

        private void Image_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ReloadDocument(sender, e);
        }


        private void cboToDo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboToDo.SelectedItem == null)
                return;

            if (stkByBin == null || stkByProduct == null)
                return;

            stkOptions.Visibility = Visibility.Visible;

            stkByBin.Visibility = Visibility.Collapsed;
            stkByProduct.Visibility = Visibility.Collapsed;
            StackPanel_f1.Visibility = Visibility.Collapsed;

            if (cboToDo.SelectedIndex == 0)
            {
                stkByBin.Visibility = Visibility.Visible;
                StackPanel_f1.Visibility = Visibility.Visible;
            }
            else if (cboToDo.SelectedIndex == 1)
                stkByProduct.Visibility = Visibility.Visible;


            UpdateDocumentOption(sender, new DataEventArgs<int>(cboToDo.SelectedIndex)); //0-BIN, 1-BIN-PRODUCT

        }

        private void imgLoad_Click(object sender, RoutedEventArgs e)
        {
            Product p = new Product { Name = txtProduct.Text };

            if (cboCategory.SelectedItem != null && cboCategory.SelectedIndex > 0)
                p.Category = cboCategory.SelectedItem as ProductCategory;

            FilterByProduct(sender, new DataEventArgs<Product>(p));
        }

        private void btnTktList_Click(object sender, RoutedEventArgs e)
        {
            ShowInitialTicket(sender, e);
        }


        private void lvSummNoCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
                ListSortDirection direction;

                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                            direction = ListSortDirection.Ascending;
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                                direction = ListSortDirection.Descending;
                            else
                                direction = ListSortDirection.Ascending;
                        }

                        //string header = headerClicked.Column.HeaderStringFormat as string;
                        GridViewColumn colum = headerClicked.Column;
                        string header = ((Binding)(colum.DisplayMemberBinding)).Path.Path;
                        Sort(header, direction, lvSummNoCount.ItemsSource);

                        if (direction == ListSortDirection.Ascending)
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        }
                        else
                        {
                            headerClicked.Column.HeaderTemplate =
                              Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        // Remove arrow from previously sorted header
                        if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                            _lastHeaderClicked.Column.HeaderTemplate = null;


                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            catch { }
        }

        private void btnStepN_Click(object sender, RoutedEventArgs e)
        {
            SendAdjustment(sender, e);
        }

        private void tabMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // CAA [2010/06/11]
            // validación especial para que solo entre cuando realmente cambie de Tab 

            if (Model.ActualTab == -1)  // no inicializado
                Model.ActualTab = tabMenu.SelectedIndex;

            //1. load no count balance
            if (tabMenu.SelectedIndex != Model.ActualTab)   // cambió el TAB
            {
                Model.ActualTab = tabMenu.SelectedIndex;
                if (tabMenu.SelectedIndex == 1)
                    LoadNoCountBalance(sender, e);
            }
        }



        private void chkAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectAll(sender, e);
            // lvSummNoCount.SelectAll();
        }


        private void uncheckAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UnSelectAll(sender, e);
            //lvSummNoCount.UnselectAll();
        }

        private void cboSendOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeSendOption(sender, e);
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.bFilters.txtFilter.Text) && string.IsNullOrEmpty(this.bFilters.txtFilter1.Text))
                return;

            FilterByBin(sender, new DataEventArgs<String>(this.bFilters.txtFilter.Text + ":" + this.bFilters.txtFilter1.Text));
        }

    }



    public interface IInventoryCountView
    {
        //Clase Modelo
        InventoryCountModel Model { get; set; }

        event EventHandler<DataEventArgs<String>> FilterByBin;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<EventArgs> AddToAssigned;
        event EventHandler<EventArgs> CreateNewTask;
        event EventHandler<EventArgs> ShowTicket;
        event EventHandler<DataEventArgs<Document>> LoadDetails;
        event EventHandler<EventArgs> ChangeStatus;
        //event EventHandler<DataEventArgs<ProductStock>> BinTaskSelected;
        event EventHandler<DataEventArgs<object[]>> ChangeCountedQty;
        event EventHandler<EventArgs> ConfirmCountTask;
        event EventHandler<EventArgs> CancelTask;
        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<EventArgs> RefreshDocuments;
        event EventHandler<EventArgs> ReloadDocument;
        event EventHandler<DataEventArgs<Product>> FilterByProduct;
        event EventHandler<DataEventArgs<int>> UpdateDocumentOption;
        event EventHandler<EventArgs> ShowInitialTicket;
        event EventHandler<EventArgs> LoadNoCountBalance;
        event EventHandler<EventArgs> SendAdjustment;
        event EventHandler<EventArgs> ChangeSendOption;
        event EventHandler<EventArgs> SelectAll;
        event EventHandler<EventArgs> UnSelectAll;

        Microsoft.Windows.Controls.DatePicker TxtSchDate { get; set; }
        TextBlock TxtTitle { get; set; }
        ListView LvAvailable { get; set; }
        ListView LvAssigned { get; set; }
        DataGridControl DgDocument { get; set; }
        StackPanel StkTask { get; set; }
        OdcExpander ExpExe { get; set; }
        OdcExpander ExpSetup { get; set; }
        //ComboBox ComboStatus { get; set; }
        Button BtnUpdate { get; set; }
        StackPanel BtnRemove { get; set; }
        //StackPanel LvExeDetail { get; set; }
        ListView LvSumm { get; set; }
        Button BtnTicket { get; set; }
        Button BtnCancel { get; set; }
        StackPanel StkConfirm { get; set; }
        Button BtnConfirm { get; set; }
        ListView LvAvailableProd { get; set; }
        ComboBox CboToDo { get; set; }
        StackPanel StkOptions { get; set; }
        Button BtnTicketList { get; set; }
        Button BtnCfmNoCount { get; set; }
        ComboBox CboSendOptions { get; set; }
        StackPanel StkUcBin { get; set; }
        BinLocation BinRestock { get; set; }
        StackPanel StkAdjustmentOptions { get; set; }
        CheckBox ChkHideBin { get; set; }
        ListView LvSummNoCount { get; set; }

        QueryFilter BFilters { get; set; }    
        //Button btnFilter { get; set; }    
    }
}