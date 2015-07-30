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
using System.Windows.Media;
using Odyssey.Controls.Classes;
using System.Linq;
using System.Windows.Controls.Primitives;
using WpfFront.Common.UserControls;
using System.ComponentModel;
using System.Windows.Data;



namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class ReceivingView : UserControlBase, IReceivingView
    {
        public ReceivingView()
        {
            InitializeComponent();
            expManual.IsExpanded = true;
            expLabel.IsExpanded = false;
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<DataEventArgs<Document>> LoadDetails;
        public event EventHandler<DataEventArgs<Product>> LoadUnits;
        public event EventHandler<EventArgs> ReceiveProduct;
        public event EventHandler<DataEventArgs<string>> ReceiveLabel;
        public event EventHandler<EventArgs> ReceiveLabelList;
        public event EventHandler<EventArgs> PostReceipt;
        public event EventHandler<EventArgs> ReceiptAtOnce;
        public event EventHandler<EventArgs> CreateEmptyReceipt;
        //public event EventHandler<DataEventArgs<string>> LoadProducts;
        //public event EventHandler<DataEventArgs<string>> LoadVendors;
        public event EventHandler<EventArgs> ChangeStatus;


        public event EventHandler<EventArgs> LoadProductManualTrackOption;

        public event EventHandler<EventArgs> ReceiveLabelTrackOption;
        public event EventHandler<EventArgs> ReceiveManualTrack;

        ////public event EventHandler<EventArgs> AddManualTrackToList;
        ////public event EventHandler<EventArgs> RemoveManualTrack;

        public event EventHandler<EventArgs> SelectedUnit;
        public event EventHandler<DataEventArgs<Document>> LoadPostedReceipt;
        public event EventHandler<DataEventArgs<string>> ReversePosted;
        public event EventHandler<EventArgs> ShowReceivingTicket;
        public event EventHandler<EventArgs> GoToPrintLabels;
        public event EventHandler<DataEventArgs<bool?>> LateDocuments;
        public event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;
        public event EventHandler<DataEventArgs<string>> AssignBinToProduct;
        public event EventHandler<DataEventArgs<Unit>> SelectPack;
        //public event EventHandler<DataEventArgs<string>> LoadBins;
        public event EventHandler<EventArgs> GoToCrossDock;

        public event EventHandler<DataEventArgs<double>> ReceiveReturn;
        public event EventHandler<DataEventArgs<double>> ReceiptAcknowledge;

        public event EventHandler<EventArgs> ShowPurchaseReceive;


        private bool showList = false;

        //List Order
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;


        public ReceivingModel Model
        {
            get
            { return this.DataContext as ReceivingModel; }
            set
            { this.DataContext = value; }

        }



        #region Properties


        public TextBlock ProcessResult
        {
            get { return this.txtProcessResult; }
            set { this.txtProcessResult = value; }
        }

        public TextBox TxtVendorDoc
        {
            get { return this.txtVendDoc; }
        }


        public Microsoft.Windows.Controls.DatePicker TxtDocDate
        {
            get { return this.txtDocDate; }
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

        public ComboBox LogisticUnit
        {
            get { return this.cboLogiUnit; }
        }


        public DataGridControl DgDocumentBalance
        {
            get { return this.dgDocumentBalance; }
        }

        public StackPanel DgReceiptLines
        {
            get { return this.dgReceiptLines; }
            set { dgReceiptLines = value; }
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

        public ListView TrackLabelList
        {
            get { return this.lvTrackLabel; }
        }


        public BinLocation BinLocation
        {
            get { return this.txtBinLabel; }
        }

        public TabControl TabDocDetails
        {
            get { return this.tabMenu; }
            set { this.tabMenu = value; }
        }

        public Button BtnReceiveLabel
        {
            get { return this.btnReceiveLabel; }
            set { this.btnReceiveLabel = value; }
        }


        public Button BtnReceive
        {
            get { return this.btnReceive; }
            set { this.btnReceive = value; }
        }

        public Button BtnReversePosted
        {
            get { return this.btnReversePosted; }
            set { this.btnReversePosted = value; }
        }


        public Button BtnReceiveAtOnce
        {
            get { return this.btnReceiptAtOnce; }
            set { this.btnReceiptAtOnce = value; }
        }


        public Button BtnCreateReceipt
        {
            get { return this.btnCreateReceipt; }
            set { this.btnCreateReceipt = value; }
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

        public StackPanel StkLabelTrack
        {
            get { return this.stkLabelTrack; }
            set { this.stkLabelTrack = value; }
        }


        public TabItem TabItemReceive
        {
            get { return this.tbItemReceive; }
            set { this.tbItemReceive = value; }
        }


        //public ListView LvVendor
        //{
        //    get { return this.lvVendor; }
        //}


        public SearchAccount TxtVendor
        {
            get { return this.txtVendor; }
            set { this.txtVendor = value; }
        }


        public TextBox TxtRecLabel
        {
            get { return this.txtScanLabel; }
            set { this.txtScanLabel = value; }
        }


        public SearchProduct TxtProduct
        {
            get { return this.txtProduct; }
            set { this.txtProduct = value; }
        }



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

        public TabItem GrpReceipts
        {
            get { return this.tbPosting; }
            set { this.tbPosting = value; }
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

        public TabItem TabItemTrackOption
        {
            get { return this.tbItemTracking; }
            set { this.tbItemTracking = value; }
        }


  

        public Border StkReceiptData
        {
            get { return this.stkReceiptData; }
            set { this.stkReceiptData = value; }
        }


        public Button BtnTrackReceive
        {
            get { return this.btnTrackReceive; }
            set { this.btnTrackReceive = value; }
        }





        public Button BtnRecTkt
        {
            get { return this.btnRecTkt; }
            set { this.btnRecTkt = value; }
        }


        public TextBlock TxtPostResult
        {
            get { return this.txtPostResult; }
            set { this.txtPostResult = value; }
        }

        public ListView LvStock
        {
            get { return this.lvBinStock; }
            set { this.lvBinStock = value; }
        }

        public CheckBox ChkPutAway
        {
            get { return this.chkPutAway; }
            set { this.chkPutAway = value; }
        }

        public TabControl TabProductInfo
        {
            get { return this.tbProductInfo; }
            set { this.tbProductInfo = value; }
        }

        public TextBox TxtQtyPerPack
        {
            get { return this.txtQtyPerPack; }
            set { this.txtQtyPerPack = value; }
        }


        //public ComboBox CboBinList
        //{
        //    get { return this.cboBin; }
        //    set { this.cboBin = value; }
        //}

        public StackPanel StkCross
        {
            get { return this.stkCross; }
            set { this.stkCross = value; }

        }

        public ListView LvReceipts
        {
            get { return this.lvReceipts; }
            set { this.lvReceipts = value; }

        }

        public Popup PopupReceived
        {
            get { return this.popup1; }
            set { this.popup1 = value; }
        }


        public PopUpShell UcReceivedBal
        {
            get { return this.ucRecBalance; }
            set { this.ucRecBalance = value; }
        }

        public ComboBox CboBinDirection
        {
            get { return this.cboBinDirection; }
            set { this.cboBinDirection = value; }
        }


        public ItemsControl TrackOpts
        {
            get { return this.itmTrackOpts; }
            set { this.itmTrackOpts = value; }
        }


        public Button BtnCrossDock
        {
            get { return this.btnCrossDock; }
            set { this.btnCrossDock = value; }
        }

        public TextBox TxtRctComment
        {
            get { return this.txtRctComment; }
            set { this.txtRctComment = value; }
        }

        public Grid GridReturn
        {
            get { return this.dgReturn; }
            set { this.dgReturn = value; }
        }

        public Grid GridManual
        {
            get { return this.grdRecManual; }
            set { this.grdRecManual = value; }
        }

        public DirectPrint DirectPrint
        {
            get { return this.ucDirectPrint; }
            set { this.ucDirectPrint = value; }
        }

        public GroupBox GBArrive
        {
            get { return this.gAbArrive; }
            set { this.gAbArrive = value; }
        }

        public Button BtnViewPR
        {
            get { return this.btnViewPR; }
            set { this.btnViewPR = value; }
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
        //    LoadUnits(this, new DataEventArgs<Product>((Product)txtProduct.Product));
        //}


        private void btnReceive_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReceiveProduct(sender, e);
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }


        private void txtScanLabel_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ReceiveLabel(this, new DataEventArgs<string>(txtScanLabel.Text));
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


        private void btnReceiveLabel_Click(object sender, RoutedEventArgs e)
        {
            ReceiveLabelList(sender, e);
        }

        private void btnCreteReceipt_Click(object sender, RoutedEventArgs e)
        {
            PostReceipt(sender, e);
        }


        private void btnReceiptAtOnce_Click(object sender, RoutedEventArgs e)
        {
            ReceiptAtOnce(sender, e);
        }

        private void btnEmptyTask_Click(object sender, RoutedEventArgs e)
        {
            CreateEmptyReceipt(sender, e);
        }


        //private void txtProduct_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadProducts(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}


        //private void txtVendor_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadVendors(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}


        private void txtQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            try {

                if (((Unit)cboUnit.SelectedItem).BaseAmount == 1)
                    txtQtyPerPack.Text = (int.Parse(txtCurQty.Text) / int.Parse(txtQuantity.Text)).ToString();
                else
                    txtQtyPerPack.Text = "1";
            }
            catch { }

            cboLogiUnit.Focus();
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

        private void btnLabelTrakOpt_Click(object sender, RoutedEventArgs e)
        {
            //Recibe el Label with the tracking option
            ReceiveLabelTrackOption(this, new DataEventArgs<string>(txtScanLabel.Text));
        }

        private void btnTrackReceive_Click(object sender, RoutedEventArgs e)
        {
            ReceiveManualTrack(sender, e);
        }

        //private void Add_TrackOpt_Click(object sender, RoutedEventArgs e)
        //{
        //    AddManualTrackToList(sender, e);
        //}

        //private void btnTrackRemove_Click(object sender, RoutedEventArgs e)
        //{
        //    RemoveManualTrack(sender, e);
        //}

        private void cboUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedUnit(sender, e);
        }

        private void lvReceipts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPostedReceipt(this, new DataEventArgs<Document>((Document)lvReceipts.SelectedItem));
        }



        private void btnReversePosted_Click(object sender, RoutedEventArgs e)
        {
            string reason = UtilWindow.ConfirmWindow("Please enter the reason to Cancel the Receipt. Remember you manually have to delete the receipt in the ERP.");
            ReversePosted(sender, new DataEventArgs<string>(reason));
        }

        #endregion

        private void btnRecTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowReceivingTicket(sender, e);
        }

        private void btnPrintOpc_Click(object sender, RoutedEventArgs e)
        {
            GoToPrintLabels(sender, e);
        }

        private void chkLate_Checked(object sender, RoutedEventArgs e)
        {
            LateDocuments(sender, new DataEventArgs<bool?>(((CheckBox)sender).IsChecked));
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }

        private void dgPostingBalance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DocumentBalance balance = ((DataGridControl)sender).SelectedItem as DocumentBalance;

            RemoveFromNode(sender, new DataEventArgs<DocumentBalance>(balance));
        }


        private void lsStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductStock ps = lvBinStock.SelectedItem as ProductStock;
            if (ps == null)
                return;

            txtBinLabel.Text = ps.Bin.BinCode;

        }

        private void btnAssignBin_Click(object sender, RoutedEventArgs e)
        {
            AssignBinToProduct(sender, new DataEventArgs<string>(txtAssigBin.Text));
            txtAssigBin.Text = "";
            cboBinDirection.SelectedIndex = -1;
        }

        private void cboLogiUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unit unit = ((ComboBox)sender).SelectedItem as Unit;
            SelectPack(sender, new DataEventArgs<Unit>(unit));
        }


        private void ComboBoxItem_PreviewMouseLeftButtonDown(Object sender, MouseEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            if (item != null)
            {
                String content = item.Content.ToString();
                Rect bound = VisualTreeHelper.GetDescendantBounds(item);
            }
        }

        //private void txtBinLabel_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //Search for a Bin
        //    LoadBins(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}

        //private void cboBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Bin bin = ((ComboBox)sender).SelectedItem as Bin;

        //    if (bin == null)
        //        return;

        //    txtBinLabel.Text = bin.BinCode;
        //    cboBin.Visibility = Visibility.Collapsed;
        //    txtBinLabel.Focus();

        //}

        private void btnCrossDock_Click(object sender, RoutedEventArgs e)
        {
            GoToCrossDock(sender, e);
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            GoToCrossDock(sender, e);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
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
                txtHide.Text = Util.GetResourceLanguage("SHOWLIST");
            }
        }



        void ShowColumn(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Header.ToString().Contains("Hide"))
                this.dgDocument.Columns[item.Name.Replace("_",".")].Visible = false;
            else
                this.dgDocument.Columns[item.Name.Replace("_",".")].Visible = true;
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

            foreach (Column myColumns in this.dgDocument.Columns.OrderBy(f=>f.Visible))
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
                    mi.Name = myColumns.FieldName.Replace(".","_");
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




        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(""));
        }

        private void txtProduct_OnLoadRecord(object sender, EventArgs e)
        {
            LoadUnits(this, new DataEventArgs<Product>((Product)txtProduct.Product));
        }


        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            this.lvLabelsAvailable.SelectAll();
            this.lvLabelsAvailable.Focus();
        }


        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lvLabelsAvailable.UnselectAll();

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

        private void txtQtyHnd_LostFocus(object sender, RoutedEventArgs e)
        {
            try { this.txtRetTotal.Text = (double.Parse(txtQtyHnd.Text) + double.Parse(txtDamage.Text)).ToString(); }
                catch{}
        }

        private void txDamage_LostFocus(object sender, RoutedEventArgs e)
        {
            try { this.txtRetTotal.Text = (double.Parse(txtQtyHnd.Text) + double.Parse(txtDamage.Text)).ToString(); }
            catch { }

        }

        private void btnReceiveReturn_Click(object sender, RoutedEventArgs e)
        {
            Double qtyRet = 0;
            try
            {
                qtyRet = double.Parse(txtRetTotal.Text);
            }
            catch {
                Util.ShowError("Please check the quantities entered.");
            }

            ReceiveReturn(this, new DataEventArgs<double>(qtyRet));
        }



        private void btnArrived_Click(object sender, RoutedEventArgs e)
        {
            
            Double qtyArrived = 0;
            try
            {
                qtyArrived = double.Parse(txtQtyArrived.Text);
            }
            catch {
                Util.ShowError("Please check the quantities entered.");
            }


            ReceiptAcknowledge(sender, new DataEventArgs<double>(qtyArrived));
            txtQtyArrived.Text = "";
        }

        private void xRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Document curDoc = Model.Document;
            Model.Document = null;
            LoadDetails(sender, new DataEventArgs<Document>(curDoc));
        }

        private void btnViewPR_Click(object sender, RoutedEventArgs e)
        {
            ShowPurchaseReceive(sender, e);
        }

    }


    public interface IReceivingView
    {

        //ListView ComboProduct { get; }
        ListView ComboUnit { get; }
        TextBox TxtRcvQty { get; }
        DataGridControl DgDocumentBalance { get; }
        DataGridControl DgDocument { get; }
        DataGridControl DgDocumentLine { get; }
        ComboBox LogisticUnit { get; }
        TextBlock ProcessResult { get; set; }
        
        BinLocation BinLocation { get; }

        ListView LabelListAvailable { get; }
        TabControl TabDocDetails { get; set; }
        Button BtnReceiveLabel { get; set; }
        //OdcExpander ExpPendingDocs { get; set; }
        Button BtnReceiveAtOnce { get; set; }
        Button BtnCreateReceipt { get; set; }
        DataGridControl DgPostingBalance { get; }
        StackPanel StkPosting { get; set; }
        TabItem TabItemReceive { get; set; }
        OdcExpander ExpDocLines { get; set; }
        TextBox TxtVendorDoc { get; }
        Microsoft.Windows.Controls.DatePicker TxtDocDate { get; }
        //ListView LvVendor { get; }
        SearchAccount TxtVendor { get; set; }
        TextBox TxtRecLabel { get; set; }
        SearchProduct TxtProduct { get; set; }
        ComboBox ComboStatus { get; set; }
        Border PanelPosting { get; set; }
        TabItem GrpReceipts { get; set; }
        OdcExpander ExpManual { get; set; }
        StackPanel StkLabelTrack { get; set; }
        TextBlock TxtProductTrackMsg { get; set; }
        Button BtnTrack { get; set; }

        TabItem TabItemTrackOption { get; set; }
        ListView TrackLabelList { get; }
        ItemsControl TrackOpts { get; set; }

        //ListView ManualTrackList { get; set; }
        //TextBox TxtQtyTrack { get; set; }
        //Button BtnAddTrack { get; set; }
        //ListView LvTrackProduct { get; set; }
        //Button BtnTrackRemove { get; set; }

        StackPanel DgReceiptLines { get; set; }
        Button BtnReversePosted { get; set; }
        Border StkReceiptData { get; set; }
        Button BtnReceive { get; set; }
        Button BtnTrackReceive { get; set; }
        Button BtnRecTkt { get; set; }

        TextBlock TxtPostResult { get; set; }
        ListView LvStock { get; set; }
        CheckBox ChkPutAway { get; set; }
        TabControl TabProductInfo { get; set; }
        TextBox TxtQtyPerPack { get; set; }
        //ComboBox CboBinList { get; set; }
        StackPanel StkCross { get; set; }
        ListView LvReceipts { get; set; }
        Popup PopupReceived { get; set; }
        PopUpShell UcReceivedBal { get; set; }

        ComboBox CboBinDirection { get; set; }
        Button BtnCrossDock { get; set; }
        TextBox TxtRctComment { get; set; }

        Grid GridReturn { get; set; }
        Grid GridManual { get; set; }
        DirectPrint DirectPrint { get; set; }
        GroupBox GBArrive { get; set; }

        Button BtnViewPR { get; set; }


        ReceivingModel Model { get; set; }



        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<DataEventArgs<Document>> LoadDetails;
        event EventHandler<DataEventArgs<Product>> LoadUnits;
        event EventHandler<EventArgs> ReceiveProduct;
        event EventHandler<DataEventArgs<string>> ReceiveLabel;
        event EventHandler<EventArgs> ReceiveLabelList;
        event EventHandler<EventArgs> PostReceipt;
        event EventHandler<EventArgs> ReceiptAtOnce;
        event EventHandler<EventArgs> CreateEmptyReceipt;
        //event EventHandler<DataEventArgs<string>> LoadProducts;
        //event EventHandler<DataEventArgs<string>> LoadVendors;
        event EventHandler<EventArgs> ChangeStatus;
        event EventHandler<EventArgs> ReceiveLabelTrackOption;
        event EventHandler<EventArgs> LoadProductManualTrackOption;

        event EventHandler<EventArgs> ReceiveManualTrack;
        event EventHandler<EventArgs> SelectedUnit;

        //event EventHandler<EventArgs> AddManualTrackToList;
        //event EventHandler<EventArgs> RemoveManualTrack;

        event EventHandler<DataEventArgs<Document>> LoadPostedReceipt;
        event EventHandler<DataEventArgs<string>> ReversePosted;
        event EventHandler<EventArgs> ShowReceivingTicket;
        event EventHandler<EventArgs> GoToPrintLabels;
        event EventHandler<DataEventArgs<bool?>> LateDocuments;
        event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;
        event EventHandler<DataEventArgs<string>> AssignBinToProduct;
        event EventHandler<DataEventArgs<Unit>> SelectPack;
        //event EventHandler<DataEventArgs<string>> LoadBins;
        event EventHandler<EventArgs> GoToCrossDock;
        event EventHandler<DataEventArgs<double>> ReceiveReturn;
        event EventHandler<DataEventArgs<double>> ReceiptAcknowledge;
        event EventHandler<EventArgs> ShowPurchaseReceive;

    }

}
