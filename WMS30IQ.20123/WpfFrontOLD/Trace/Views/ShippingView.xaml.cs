using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using Odyssey.Controls;
using Microsoft.Windows.Controls;
using System.Windows;
using WpfFront.Common;
using System.Windows.Input;
using System.Linq;
using WpfFront.Common.UserControls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Data;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class ShippingView : UserControlBase, IShippingView
    {
        public ShippingView()
        {
            InitializeComponent();
            expManual.IsExpanded = true;
            expLabel.IsExpanded = false;
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<DataEventArgs<Document>> LoadDetails;
        public event EventHandler<DataEventArgs<Product>> LoadUnits;
        public event EventHandler<EventArgs> PickProduct;
        public event EventHandler<DataEventArgs<string>> PickLabel;
        public event EventHandler<EventArgs> PickLabelList;
        public event EventHandler<EventArgs> PostShipment;
        public event EventHandler<EventArgs> ShipmentAtOnce;
        public event EventHandler<EventArgs> CreateEmptyPickTicket;
        //public event EventHandler<DataEventArgs<string>> LoadProducts;
        //public event EventHandler<DataEventArgs<string>> LoadCustomers;
        public event EventHandler<EventArgs> ChangeStatus;
        //public event EventHandler<EventArgs> PickLabelTrackOption;
        public event EventHandler<EventArgs> LoadProductManualTrackOption;
        public event EventHandler<EventArgs> AddManualTrackToList;
        public event EventHandler<EventArgs> PickManualTrack;
        public event EventHandler<EventArgs> SelectedUnit;
        public event EventHandler<EventArgs> RefreshBin;
        public event EventHandler<DataEventArgs<Document>> LoadShipment;
        public event EventHandler<DataEventArgs<string>> ReversePosted;
        public event EventHandler<DataEventArgs<bool?>> LateDocuments;
        public event EventHandler<EventArgs> ShowPickingTicket;
        public event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;
        public event EventHandler<EventArgs> RefreshShipments;
        public event EventHandler<DataEventArgs<Document>> ShowPackingList;
        public event EventHandler<EventArgs> UpdatePackages;
        public event EventHandler<DataEventArgs<Document>> PreviewPackLabels;
        public event EventHandler<DataEventArgs<Document>> PrintPackLabels;
        public event EventHandler<DataEventArgs<Document>> ShowPackageAdmin;
        public event EventHandler<EventArgs> RefreshPacks;
        public event EventHandler<EventArgs> PrintTicketInBatch;
        public event EventHandler<EventArgs> CheckBalance;

        public event EventHandler<EventArgs> UnShowOnlyMerged;
        public event EventHandler<EventArgs> ShowOnlyMerged;
        public event EventHandler<DataEventArgs<DocumentLine>> CancelLine;

        public event EventHandler<DataEventArgs<DocumentLine>> RemoveFromShipmentLine;
        public event EventHandler<EventArgs> ReFullFilOrder;
        public event EventHandler<DataEventArgs<DocumentLine>> LoadPopupLine;
        public event EventHandler<EventArgs> ConfirmPicking;


        private bool showList = false;


        //List Order
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;


        public ShippingModel Model
        {
            get
            { return this.DataContext as ShippingModel; }
            set
            { this.DataContext = value; }

        }



        #region Properties


        public TextBlock ProcessResult
        {
            get { return this.txtProcessResult; }
            set { this.txtProcessResult = value; }
        }


        //public ListView ComboProduct
        //{
        //    get { return this.cboProduct; }
        //}

        public ListView ComboUnit
        {
            get { return this.cboUnit; }
        }

        public ListView LabelListAvailable
        {
            get { return this.lvLabelsAvailable; }
        }

        public TextBox TxtRcvQty
        {
            get { return this.txtQuantity; }
        }

        //public ComboBox LogisticUnit
        //{
        //    get { return this.cboLogiUnit; }
        //}


        public DataGridControl DgDocumentBalance
        {
            get { return this.dgDocumentBalance; }
        }

        public DataGridControl DgPostingBalance
        {
            get { return this.dgPostingBalance; }
        }


        public DataGridControl DgDocument
        {
            get { return this.dgDocument; }
        }


        public DataGridControl DgDocumentLine
        {
            get { return this.DgDocumentLine; }
        }



        public TextBox BinLocation
        {
            get { return this.txtBinLabel; }
        }

        public TabControl TabShipping
        {
            get { return this.tabMenu; }
            set { this.tabMenu = value; }
        }

        public Button BtnShipLabel
        {
            get { return this.btnShipLabel; }
            set { this.btnShipLabel = value; }
        }

        public Button BtnShipAtOnce
        {
            get { return this.btnShipmentAtOnce; }
            set { this.btnShipmentAtOnce = value; }
        }


        public Button BtnCreateShipment
        {
            get { return this.btnCreateShipment; }
            set { this.btnCreateShipment = value; }
        }

        //public OdcExpander ExpPendingDocs
        //{
        //    get { return this.exPendingDocs; }
        //    set { this.exPendingDocs = value; }
        //}

        public OdcExpander ExpDocLines
        {
            get { return this.expDocLines; }
            set { this.expDocLines = value; }
        }


        public OdcExpander ExpManual
        {
            get { return this.expManual; }
            set { this.expManual = value; }
        }

        public StackPanel StkPosting
        {
            get { return this.stkPosting; }
            set { this.stkPosting = value; }
        }




        public TabItem TabItemShip
        {
            get { return this.tbItemShip; }
            set { this.tbItemShip = value; }
        }


        //public ListView LvCustomer
        //{
        //    get { return this.lvCustomer; }
        //}


        public SearchAccount TxtCustomer
        {
            get { return this.txtCustomer; }
            set { this.txtCustomer = value; }
        }


        public TextBox TxtScanLabel
        {
            get { return this.txtScanLabel; }
            set { this.txtScanLabel = value; }
        }


        public SearchProduct TxtProduct
        {
            get { return this.txtProduct; }
            set { this.txtProduct = value; }
        }


        public Popup PopupReceived
        {
            get { return this.popup1; }
            set { this.popup1 = value; }
        }


        //public TextBox TxtQtyTrack
        //{
        //    get { return this.txtQtyTrack; }
        //    set { this.txtQtyTrack = value; }
        //}


        public ComboBox ComboStatus
        {
            get { return this.cboStatus; }
            set { this.cboStatus = value; }
        }


        public Border PanelPosting
        {
            get { return this.pnPosting; }
            set { this.pnPosting = value; }
        }



        public TextBlock TxtProductTrackMsg
        {
            get { return this.txtProductTrackMsg; }
            set { this.txtProductTrackMsg = value; }
        }


        public Button BtnTrack
        {
            get { return this.btnTrack; }
            set { this.btnTrack = value; }
        }


        //public Button BtnTrackPick
        //{
        //    get { return this.btnTrackShip; }
        //    set { this.btnTrackShip = value; }
        //}

        public TabItem TabItemTrackOption
        {
            get { return this.tbItemTracking; }
            set { this.tbItemTracking = value; }
        }


        //public ListView ManualTrackList
        //{
        //    get { return this.lvManualTrackList; }
        //    set { this.lvManualTrackList = value; }
        //}


        //public Button BtnAddTrack
        //{
        //    get { return this.btnAddTrackOpt; }
        //    set { this.btnAddTrackOpt = value; }
        //}

        public Button BtnPick
        {
            get { return this.btnPick; }
            set { this.btnPick = value; }
        }

        public StackPanel LvStock
        {
            get { return this.lvBinStock; }
            set { this.lvBinStock = value; }
        }

        //public ListView LvTrackProduct
        //{
        //    get { return this.lvTrackProduct; }
        //    set { this.lvTrackProduct = value; }
        //}

        //public TabControl TabDocDetails
        //{
        //    get { return this.tabMenu; }
        //    set { this.tabMenu = value; }
        //}


        public TabItem GrpShipments
        {
            get { return this.tbPosting; }
            set { this.tbPosting = value; }
        }

        public Border StkShipmentData
        {
            get { return this.stkShipmentData; }
            set { this.stkShipmentData = value; }
        }

        public TextBlock TxtPostResult
        {
            get { return this.txtPostResult; }
            set { this.txtPostResult = value; }
        }

        public StackPanel DgShipmentLines
        {
            get { return this.dgShipmentLines; }
            set { dgShipmentLines = value; }
        }


        public StackPanel BtnReversePosted
        {
            get { return this.btnReversePosted; }
            set { this.btnReversePosted = value; }
        }

        public ItemsControl TrackOpts
        {
            get { return this.itmTrackOpts; }
            set { this.itmTrackOpts = value; }
        }


        public PopUpShell UcReceivedBal
        {
            get { return this.ucRecBalance; }
            set { this.ucRecBalance = value; }
        }



        public BinLocation BinRestock
        {
            get { return this.binRestock; }
        }

        public ComboBox CboPack
        {
            get { return this.cboPackage; }
            set { this.cboPackage = value; }
        }

        public ComboBox CboPickMethod
        {
            get { return this.cboPickM; }
            set { this.cboPickM = value; }
        }


        public ListView LvShipment
        {
            get { return this.lvInvoice; }
            set { this.lvInvoice = value; }
        }


        public ListView LvPacks
        {
            get { return this.lvPacks; }
            set { this.lvPacks = value; }
        }


        public DirectPrint DirectPrint
        {
            get { return this.ucDirectPrint; }
            set { this.ucDirectPrint = value; }
        }

        public DirectPrint DirectPrintShip
        {
            get { return this.ucDirectShip; }
            set { this.ucDirectShip = value; }
        }


        public Popup PopAdmPackV2
        {
            get { return this.popup2; }
            set { this.popup2 = value; }
        }

        public AdminPackagesV2 UcAdmPAckV2
        {
            get { return this.ucAdminPkg; }
            set { this.ucAdminPkg = value; }
        }

        public CheckBox ChkUseMerged
        {
            get { return this.chkMerged; }
            set { this.chkMerged = value; }
        }


        public Button BtnRefullfil
        {
            get { return this.btnFullfil; }
            set { this.btnFullfil = value; }
        }

        public Button BtnConfirmPicking
        {
            get { return this.btnConfirmPicking; }
            set { this.btnConfirmPicking = value; }
        }

        public Popup Popup3
        {
            get { return this.popup3; }
            set { this.popup3 = value; }
        }

        public AdminDocumentLine UcDocLine
        {
            get { return this.ucDocLine; }
            set { this.ucDocLine = value; }
        }

        public StackPanel StkUpdLines
        {
            get { return this.stkUpdLines; }
            set { this.stkUpdLines = value; }
        }

        public TextBlock TxtWarning
        {
            get { return this.txtWarning; }
            set { this.txtWarning = value; }
        }


        public Button BtnAdmPk
        {
            get { return this.btnAdmPacks; }
            set { this.btnAdmPacks = value; }
        }


        #endregion



        #region ViewEvents


        private void dgDocument_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadDetails(this, new DataEventArgs<Document>((Document)dgDocument.SelectedItem));
        }

        //private void dgPendingDocument_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    LoadDetails(this, new DataEventArgs<Document>((Document)dgPendingDocument.SelectedItem));
        //}


        //private void cboProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LoadUnits(this, new DataEventArgs<Product>((Product)cboProduct.SelectedItem));
        //}


        private void btnShip_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickProduct(sender, e);
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }


        private void txtScanLabel_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            PickLabel(this, new DataEventArgs<string>(txtScanLabel.Text));
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

            if (expLabel == null || expManual == null)
                return;

            if (sender.Name == "expManual")
            {
                if (expand)
                {
                    //cboProduct.Focus();
                    expLabel.IsExpanded = false;
                }
                else
                {
                    txtScanLabel.Focus();
                    expLabel.IsExpanded = true;

                }

                return;
            }

            if (sender.Name == "expLabel")
            {
                if (expand)
                {
                    expManual.IsExpanded = false;
                    txtScanLabel.Focus();
                }
                else
                {
                    expManual.IsExpanded = true;
                    //cboProduct.Focus();
                }

                return;
            }
        }


        private void btnShipLabel_Click(object sender, RoutedEventArgs e)
        {
            PickLabelList(sender, e);
        }

        private void btnCreateShipment_Click(object sender, RoutedEventArgs e)
        {
            PostShipment(sender, e);
        }


        private void btnShipmentAtOnce_Click(object sender, RoutedEventArgs e)
        {
            ShipmentAtOnce(sender, e);
        }

        private void btnEmptyTask_Click(object sender, RoutedEventArgs e)
        {
            CreateEmptyPickTicket(sender, e);
        }


        //private void txtProduct_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadProducts(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}


        //private void txtCustomer_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadCustomers(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}


        private void txtQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            //cboLogiUnit.Focus();
        }

        private void btnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus(sender, e);
        }

        private void btnTrack_Click(object sender, RoutedEventArgs e)
        {
            //Habilitar El TrackOption - Para el producto Actual
            LoadProductManualTrackOption(sender, e);
        }


        private void Add_TrackOpt_Click(object sender, RoutedEventArgs e)
        {
            AddManualTrackToList(sender, e);
        }

        private void btnTrackShip_Click(object sender, RoutedEventArgs e)
        {
            PickManualTrack(sender, e);
        }

        private void cboUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedUnit(sender, e);
        }


        private void btnPktTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowPickingTicket(sender, e);
        }

        private void txtBinLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshBin(sender, e);
        }


        private void lvInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvInvoice.SelectedItem == null)
                return;

            LoadShipment(this, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));

            ucShowFiles.ClassEntity = EntityID.Document;
            ucShowFiles.RowID = (lvInvoice.SelectedItem as Document).DocID;
            ucShowFiles.LoaFiles();
        }


        #endregion

        private void btnReversePosted_Click(object sender, RoutedEventArgs e)
        {
            string reason = UtilWindow.ConfirmWindow("Please enter the reason to Cancel the Shipment.");

            if (string.IsNullOrEmpty(reason))
                return;

            ReversePosted(sender, new DataEventArgs<string>(reason));

        }

        private void chkLate_Checked(object sender, RoutedEventArgs e)
        {
            LateDocuments(sender, new DataEventArgs<bool?>(((CheckBox)sender).IsChecked));
        }

        private void lsStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductStock ps = lsStock.SelectedItem as ProductStock;
            if (ps == null)
                return;

            txtBinLabel.Text = ps.Bin.BinCode;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(""));
        }

        private void txtHide_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (showList)
            {
                txtTitle.Visibility = Visibility.Visible;
                stpDocLis.Visibility = Visibility.Visible;
                showList = !showList;
                txtHide.Text = "Hide List";
            }
            else
            {
                txtTitle.Visibility = Visibility.Collapsed;
                stpDocLis.Visibility = Visibility.Collapsed;
                showList = !showList;
                txtHide.Text = "Show List";
            }
        }


        void ShowColumn(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Header.ToString().Contains("Hide"))
                this.dgDocument.Columns[item.Name.Replace("_", ".")].Visible = false;
            else
                this.dgDocument.Columns[item.Name.Replace("_", ".")].Visible = true;
        }


        void ShowAllColumn(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.dgDocument.Columns.Count; i++)
                this.dgDocument.Columns[i].Visible = true;
        }

        void ResetColumn(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.dgDocument.Columns.Count; i++)
                if (i < 5)
                    this.dgDocument.Columns[i].Visible = true;
                else
                    this.dgDocument.Columns[i].Visible = false;

        }


        private void MenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            //TO DO: create context menu to show columns
            MenuItem item = sender as MenuItem;
            ContextMenu cm = item.Parent as ContextMenu;
            item.Items.Clear();

            foreach (Column myColumns in this.dgDocument.Columns.OrderBy(f => f.Visible))
            {
                if (myColumns.Visible == false)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = "Show " + myColumns.Title;
                    mi.Name = myColumns.FieldName.Replace(".", "_");
                    mi.Click += new RoutedEventHandler(ShowColumn);
                    item.Items.Add(mi);
                    //showColumns.Items.Add(myColumns.FieldName);
                }
                else
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = "Hide " + myColumns.Title;
                    mi.Name = myColumns.FieldName.Replace(".", "_");
                    mi.Click += new RoutedEventHandler(ShowColumn);
                    item.Items.Add(mi);
                }
            }

            MenuItem mix = new MenuItem();
            mix.Header = ".................";
            item.Items.Add(mix);

            mix = new MenuItem();
            mix.Header = "Show All Columns";
            mix.Click += new RoutedEventHandler(ShowAllColumn);
            item.Items.Add(mix);

            mix = new MenuItem();
            mix.Header = "Show Default Columns";
            mix.Click += new RoutedEventHandler(ResetColumn);
            item.Items.Add(mix);



            cm.IsOpen = true;
            cm.PlacementTarget = this.dgDocument;
            cm.StaysOpen = true;
        }



        private void dgPostingBalance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DocumentBalance balance = ((DataGridControl)sender).SelectedItem as DocumentBalance;

            RemoveFromNode(sender, new DataEventArgs<DocumentBalance>(balance));
        }


        private void imgTicket_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowPackingList(sender, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));
        }


        private void imgRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefreshShipments(sender, e);
        }

        private void txtProduct_OnLoadRecord(object sender, EventArgs e)
        {
            LoadUnits(this, new DataEventArgs<Product>(this.txtProduct.Product));
        }

        private void updPack_Click(object sender, RoutedEventArgs e)
        {
            //Update Info de Packages.
            UpdatePackages(sender, e);
        }

        private void prPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintPackLabels(sender, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));
        }

        private void btnAdmPacks_Click(object sender, RoutedEventArgs e)
        {
            if (lvInvoice.SelectedItem == null)
                return;

            //Open Pakage Administrator
            ShowPackageAdmin(this, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));
        }


        private void btnPS_Click(object sender, RoutedEventArgs e)
        {
            ShowPackingList(sender, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));
        }

        private void imgPack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviewPackLabels(sender, new DataEventArgs<Document>((Document)lvInvoice.SelectedItem));
        }

        private void imgPackL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefreshPacks(sender, e);
        }

        private void imgPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintTicketInBatch(sender, e);
        }


        private void lvLabelsAvailable_Click(object sender, RoutedEventArgs e)
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
                        Sort(header, direction);

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


        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvLabelsAvailable.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void ucDirectShip_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabPack.SelectedItem == null)
                return;

            if (tabPack.SelectedIndex == 1)
            {
                ucShowFiles.ClassEntity = EntityID.Document;
                ucShowFiles.RowID = (lvInvoice.SelectedItem as Document).DocID;
                ucShowFiles.LoaFiles();
            }
        }

        private void tabMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMenu.SelectedItem == null)
                return;

            if (tabMenu.SelectedIndex < 2)
                CheckBalance(sender, e);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.popup2.IsOpen = false;
            this.popup2.StaysOpen = false;
        }

        private void chkMerged_Checked(object sender, RoutedEventArgs e)
        {
            try { ShowOnlyMerged(sender, e); }
            catch { }
        }

        private void chkMerged_Unchecked(object sender, RoutedEventArgs e)
        {
            UnShowOnlyMerged(sender, e);
        }


        private void xRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Document curDoc = Model.Document;
            Model.Document = null;
            LoadDetails(sender, new DataEventArgs<Document>(curDoc));
        }

        /*
        private void MenuItem_MouseEnter_1(object sender, MouseEventArgs e)
        {
           //TO DO: create context menu to show columns
            MenuItem item = sender as MenuItem;
            ContextMenu cm = item.Parent as ContextMenu;
            item.Items.Clear();

            MenuItem mi = new MenuItem();
            mi.Header = "Cancel Selected Line";
            mi.Name = "canLine";
            mi.Click += new RoutedEventHandler(CancelSelectedLine);
            item.Items.Add(mi);

        }


        void CancelSelectedLine(object sender, RoutedEventArgs e)
        {
            //Document Lines Cancelation
            if (dgLines.SelectedItem == null)
                return;

            DocumentLine line = dgLines.SelectedItem as DocumentLine;
            CancelLine(sender, new DataEventArgs<DocumentLine>(line));
        }
        */


        private void DataGridControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGridControl)sender).SelectedItem == null)
                return;

            DocumentLine shipmentLine = ((DataGridControl)sender).SelectedItem as DocumentLine;
            RemoveFromShipmentLine(sender, new DataEventArgs<DocumentLine>(shipmentLine));
        }



        private void btnFullfil_Click(object sender, RoutedEventArgs e)
        {
            ReFullFilOrder(sender, e);
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            LoadPopupLine(sender, new DataEventArgs<DocumentLine>(new DocumentLine()));
        }



        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //remover la linea de la Orden y de GP
            if (dgLines.SelectedItem == null)
            {
                Util.ShowError("No line selected.");
                return;
            }

            if (UtilWindow.ConfirmOK("Are you sure about remove this line?") == true)
            {
                DocumentLine line = dgLines.SelectedItem as DocumentLine;
                CancelLine(sender, new DataEventArgs<DocumentLine>(line));
            }

        }

        private void expDocLines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgLines.SelectedItem == null)
                return;

            //Open the Line Popup to make line changes
            LoadPopupLine(sender, new DataEventArgs<DocumentLine>((DocumentLine)dgLines.SelectedItem));
        }



        private void btnConfirmPicking_Click(object sender, RoutedEventArgs e)
        {
            ConfirmPicking(sender, e);
        }



    }



    public interface IShippingView
    {

        //ListView ComboProduct { get; }
        ListView ComboUnit { get; }
        TextBox TxtRcvQty { get; }
        DataGridControl DgDocumentBalance { get; }
        DataGridControl DgDocument { get; }
        DataGridControl DgDocumentLine { get; }
        //ComboBox LogisticUnit { get; }
        TextBlock ProcessResult { get; set; }
        TextBox BinLocation { get; }
        ListView LabelListAvailable { get; }
        TabControl TabShipping { get; set; }
        Button BtnShipLabel { get; set; }
        //OdcExpander ExpPendingDocs { get; set; }
        Button BtnShipAtOnce { get; set; }
        Button BtnCreateShipment { get; set; }
        DataGridControl DgPostingBalance { get; }
        StackPanel StkPosting { get; set; }
        TabItem TabItemShip { get; set; }
        OdcExpander ExpDocLines { get; set; }
        //ListView LvCustomer { get; }
        SearchAccount TxtCustomer { get; set; }
        TextBox TxtScanLabel { get; set; }
        SearchProduct TxtProduct { get; set; }
        ComboBox ComboStatus { get; set; }
        Border PanelPosting { get; set; }
        //GroupBox GrpShipments { get; set; }
        OdcExpander ExpManual { get; set; }

        TextBlock TxtProductTrackMsg { get; set; }
        Button BtnTrack { get; set; }
        TabItem TabItemTrackOption { get; set; }
        //ListView ManualTrackList { get; set; }
        //TextBox TxtQtyTrack { get; set; }
        //Button BtnAddTrack { get; set; }
        StackPanel LvStock { get; set; }
        Button BtnPick { get; set; }
        //Button BtnTrackPick { get; set; }
        //ListView LvTrackProduct { get; set; }
        //TabControl TabDocDetails { get; set; }
        TabItem GrpShipments { get; set; }
        Border StkShipmentData { get; set; }
        TextBlock TxtPostResult { get; set; }
        StackPanel DgShipmentLines { get; set; }
        StackPanel BtnReversePosted { get; set; }
        ItemsControl TrackOpts { get; set; }
        Popup PopupReceived { get; set; }
        PopUpShell UcReceivedBal { get; set; }
        BinLocation BinRestock { get; }
        ComboBox CboPickMethod { get; set; }
        ListView LvShipment { get; set; }
        ListView LvPacks { get; set; }
        DirectPrint DirectPrint { get; set; }
        DirectPrint DirectPrintShip { get; set; }

        //Packs
        ComboBox CboPack { get; set; }

        ShippingModel Model { get; set; }
        Popup PopAdmPackV2 { get; set; }
        AdminPackagesV2 UcAdmPAckV2 { get; set; }
        CheckBox ChkUseMerged { get; set; }
        Button BtnRefullfil { get; set; }

        Button BtnConfirmPicking { get; set; }
        AdminDocumentLine UcDocLine { get; set; }
        Popup Popup3 { get; set; }
        StackPanel StkUpdLines { get; set; }
        TextBlock TxtWarning { get; set; }
        Button BtnAdmPk { get; set; }


        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<DataEventArgs<Document>> LoadDetails;
        event EventHandler<DataEventArgs<Product>> LoadUnits;
        event EventHandler<EventArgs> PickProduct;
        event EventHandler<DataEventArgs<string>> PickLabel;
        event EventHandler<EventArgs> PickLabelList;
        event EventHandler<EventArgs> PostShipment;
        event EventHandler<EventArgs> ShipmentAtOnce;
        event EventHandler<EventArgs> CreateEmptyPickTicket;
        //event EventHandler<DataEventArgs<string>> LoadProducts;
        //event EventHandler<DataEventArgs<string>> LoadCustomers;
        event EventHandler<EventArgs> ChangeStatus;
        event EventHandler<EventArgs> LoadProductManualTrackOption;
        event EventHandler<EventArgs> AddManualTrackToList;
        event EventHandler<EventArgs> PickManualTrack;
        event EventHandler<EventArgs> SelectedUnit;
        event EventHandler<EventArgs> RefreshBin;
        event EventHandler<DataEventArgs<Document>> LoadShipment;
        event EventHandler<DataEventArgs<string>> ReversePosted;
        event EventHandler<DataEventArgs<bool?>> LateDocuments;
        event EventHandler<EventArgs> ShowPickingTicket;
        event EventHandler<EventArgs> PrintTicketInBatch;

        event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;


        event EventHandler<EventArgs> RefreshShipments;
        event EventHandler<DataEventArgs<Document>> ShowPackingList;
        event EventHandler<EventArgs> UpdatePackages;
        event EventHandler<DataEventArgs<Document>> PreviewPackLabels;
        event EventHandler<DataEventArgs<Document>> PrintPackLabels;
        event EventHandler<DataEventArgs<Document>> ShowPackageAdmin;
        event EventHandler<EventArgs> RefreshPacks;
        event EventHandler<EventArgs> CheckBalance;

        event EventHandler<EventArgs> UnShowOnlyMerged;
        event EventHandler<EventArgs> ShowOnlyMerged;

        event EventHandler<DataEventArgs<DocumentLine>> CancelLine;


        //Adiciones para mejorar la parte de los shipments
        event EventHandler<DataEventArgs<DocumentLine>> RemoveFromShipmentLine;
        event EventHandler<EventArgs> ReFullFilOrder;
        event EventHandler<DataEventArgs<DocumentLine>> LoadPopupLine;
        event EventHandler<EventArgs> ConfirmPicking;
    }

}
