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
using WpfFront.Common.UserControls;
using System.Windows.Controls.Primitives;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for KitAssemblyV2View.xaml
    /// </summary>
    public partial class KitAssemblyV2View : UserControlBase, IKitAssemblyV2View
    {
        public KitAssemblyV2View()
        {
            InitializeComponent();
            expManual.IsExpanded = true;
            expLabel.IsExpanded = false;
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<DataEventArgs<Document>> LoadDetails;
        public event EventHandler<DataEventArgs<Product>> LoadUnits;
        public event EventHandler<EventArgs> PickComponent;
        public event EventHandler<DataEventArgs<string>> ReceiveLabel;
        public event EventHandler<EventArgs> ReceiveLabelList;
        public event EventHandler<EventArgs> PickAtOnce;
        //public event EventHandler<DataEventArgs<string>> LoadProducts;
        public event EventHandler<EventArgs> ChangeStatus;
        public event EventHandler<EventArgs> ReceiveLabelTrackOption;
        public event EventHandler<EventArgs> LoadProductManualTrackOption;
        public event EventHandler<EventArgs> AddManualTrackToList;
        public event EventHandler<EventArgs> ReceiveManualTrack;
        public event EventHandler<EventArgs> SelectedUnit;
        public event EventHandler<EventArgs> RemoveManualTrack;
        public event EventHandler<EventArgs> ShowReceivingTicket;
        public event EventHandler<EventArgs> PrintLabels;
        public event EventHandler<DataEventArgs<bool?>> LateDocuments;
        public event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;
        //public event EventHandler<DataEventArgs<string>> LoadBins;
        public event EventHandler<EventArgs> ConfirmOrder;
        public event EventHandler<EventArgs> RefreshBin;
        public event EventHandler<EventArgs> NewDocument;

        
        private bool onlyPrint = false;

        //private bool showList = false;


        public KitAssemblyV2Model Model
        {
            get
            { return this.DataContext as KitAssemblyV2Model; }
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



        public DataGridControl DgDocumentBalance
        {
            get { return this.dgDocumentBalance; }
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


        public TextBox BinLocation
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

        public Button BtnReceiveAtOnce
        {
            get { return this.btnReceiptAtOnce; }
            set { this.btnReceiptAtOnce = value; }
        }


        public Button BtnConfirmOrder
        {
            get { return this.btnConfirmAsemblyOrder; }
            set { this.btnConfirmAsemblyOrder = value; }
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


        public TextBox TxtQtyTrack
        {
            get { return this.txtQtyTrack; }
            set { this.txtQtyTrack = value; }
        }


        public ComboBox ComboStatus
        {
            get { return this.cboStatus; }
            set { this.cboStatus = value; }
        }

        /*public ComboBox CboPickMethod
        {
            get { return this.cboPickM; }
            set { this.cboPickM = value; }
        }*/

        //public Border PanelPosting
        //{
        //    get { return this.pnPosting; }
        //    set { this.pnPosting = value; }
        //}


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


        public ListView ManualTrackList
        {
            get { return this.lvManualTrackList; }
            set { this.lvManualTrackList = value; }
        }


        public ListView LvTrackProduct
        {
            get { return this.lvTrackProduct; }
            set { this.lvTrackProduct = value; }
        }


        public Button BtnAddTrack
        {
            get { return this.btnAddTrackOpt; }
            set { this.btnAddTrackOpt = value; }
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



        //public ComboBox CboBinList
        //{
        //    get { return this.cboBin; }
        //    set { this.cboBin = value; }
        //}


        public StackPanel LvStock
        {
            get { return this.lvBinStock; }
            set { this.lvBinStock = value; }
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

        public ComboBox PrinterList
        {
            get { return this.cboPrinter; }
        }

        //public DirectPrint DirectPrint
        //{
        //    get { return this.ucDirectPrint; }
        //    set { thibtnReceiveLabel_Clicks.ucDirectPrint = value; }
        //}

        public SearchProduct KitAssemblyProduct
        {
            get { return this.txtFatherProduct; }
            set { this.txtFatherProduct = value; }
        }

        public TextBox KitAssemblyQuantity
        {
            get { return this.Txt_Cantidad; }
            set { this.Txt_Cantidad = value; }
        }

        public StackPanel DatosDocumento
        {
            get { return this.Stack_DatosDocumento; }
            set { this.Stack_DatosDocumento = value; }
        }

        public TabItem TabSerialesUtilizados
        {
            get { return this.tbItemSerialCreated; }
            set { this.tbItemSerialCreated = value; }
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


        private void btnReceive_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickComponent(sender, e);
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


        private void btnReceiptAtOnce_Click(object sender, RoutedEventArgs e)
        {
            PickAtOnce(sender, e);
        }


        //private void txtProduct_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadProducts(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}



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

        private void Add_TrackOpt_Click(object sender, RoutedEventArgs e)
        {
            AddManualTrackToList(sender, e);
        }

        private void btnTrackReceive_Click(object sender, RoutedEventArgs e)
        {
            ReceiveManualTrack(sender, e);
        }

        private void cboUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedUnit(sender, e);
        }

        private void btnTrackRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveManualTrack(sender, e);
        }


        #endregion


        private void btnRecTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowReceivingTicket(sender, e);
        }

        private void btnPrintOpc_Click(object sender, RoutedEventArgs e)
        {
            //Muestra el popup y setea la printer si desea imprimir
            //popup2.IsOpen = true;
            //popup2.StaysOpen = true;
            //this.Model.PrinterList = App.printerList;
            //onlyPrint = true;
            //txtConfirm.Visibility = Visibility.Collapsed;

            PrintLabels(sender, e);

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

        //private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (showList)
        //    {
        //        txtTitle.Visibility = Visibility.Visible;
        //        stpDocLis.Visibility = Visibility.Visible;
        //        showList = !showList;
        //        txtHide.Text = "Hide List";
        //    }
        //    else
        //    {
        //        txtTitle.Visibility = Visibility.Collapsed;
        //        stpDocLis.Visibility = Visibility.Collapsed;
        //        showList = !showList;
        //        txtHide.Text = "Show List";
        //    }
        //}



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


        private void btnConfirmAsemblyOrder_Click(object sender, RoutedEventArgs e)
        {
            //Muestra el popup y setea la printer si desea imprimir
           /* 
            popup2.IsOpen = true;
            popup2.StaysOpen = true;
            this.Model.PrinterList = App.printerList;
            onlyPrint = false;
            txtConfirm.Visibility = Visibility.Visible;
            */

            ConfirmOrder(sender, e);
        }


        private void lsStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductStock ps = lsStock.SelectedItem as ProductStock;
            if (ps == null)
                return;

            txtBinLabel.Text = ps.Bin.BinCode;
        }


        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            popup2.IsOpen = false;
        }


        private void btnCofirmContinue_Click(object sender, RoutedEventArgs e)
        {
            //Close the popup, set the printer.
            popup2.IsOpen = false;

            if (onlyPrint)
                PrintLabels(sender, e);
            else
                ConfirmOrder(sender, e);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(""));
        }

        private void txtBinLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshBin(sender, e);     
        }

        private void txtProduct_OnLoadRecord(object sender, EventArgs e)
        {
            LoadUnits(this, new DataEventArgs<Product>((Product)txtProduct.Product));
        }

        private void xRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Document curDoc = Model.Document;
            Model.Document = null;
            LoadDetails(sender, new DataEventArgs<Document>(curDoc));
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            NewDocument(sender, e);
        }

     }


    public interface IKitAssemblyV2View
    {
        //Clase Modelo
        KitAssemblyV2Model Model { get; set; }


        //ListView ComboProduct { get; }
        ListView ComboUnit { get; }
        TextBox TxtRcvQty { get; }
        DataGridControl DgDocumentBalance { get; }
        DataGridControl DgDocument { get; }
        DataGridControl DgDocumentLine { get; }
        TextBlock ProcessResult { get; set; }
        TextBox BinLocation { get; }
        ListView LabelListAvailable { get; }
        TabControl TabDocDetails { get; set; }
        Button BtnReceiveLabel { get; set; }
        //OdcExpander ExpPendingDocs { get; set; }
        Button BtnReceiveAtOnce { get; set; }
        Button BtnConfirmOrder { get; set; }
        TabItem TabItemReceive { get; set; }
        OdcExpander ExpDocLines { get; set; }
        TextBox TxtScanLabel { get; set; }
        SearchProduct TxtProduct { get; set; }
        ComboBox ComboStatus { get; set; }
        //Border PanelPosting { get; set; }
        OdcExpander ExpManual { get; set; }
        StackPanel StkLabelTrack { get; set; }
        TextBlock TxtProductTrackMsg { get; set; }
        Button BtnTrack { get; set; }
        ListView TrackLabelList { get; }
        TabItem TabItemTrackOption { get; set; }
        ListView ManualTrackList { get; set; }
        TextBox TxtQtyTrack { get; set; }
        Button BtnAddTrack { get; set; }
        Button BtnReceive { get; set; }
        Button BtnTrackReceive { get; set; }
        Button BtnRecTkt { get; set; }
        ListView LvTrackProduct { get; set; }
        //ComboBox CboBinList { get; set; }
        StackPanel LvStock { get; set; }
        Popup PopupReceived { get; set; }
        PopUpShell UcReceivedBal { get; set; }
        ComboBox PrinterList { get;  }
        //ComboBox CboPickMethod { get; set; }
        //DirectPrint DirectPrint { get; set; }
        SearchProduct KitAssemblyProduct { get; set; }
        TextBox KitAssemblyQuantity { get; set; }
        StackPanel DatosDocumento { get; set; }
        TabItem TabSerialesUtilizados { get; set; }


        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<DataEventArgs<Document>> LoadDetails;
        event EventHandler<DataEventArgs<Product>> LoadUnits;
        event EventHandler<EventArgs> PickComponent;
        event EventHandler<DataEventArgs<string>> ReceiveLabel;
        event EventHandler<EventArgs> ReceiveLabelList;
        event EventHandler<EventArgs> PickAtOnce;
        //event EventHandler<DataEventArgs<string>> LoadProducts;
        event EventHandler<EventArgs> ChangeStatus;
        event EventHandler<EventArgs> ReceiveLabelTrackOption;
        event EventHandler<EventArgs> LoadProductManualTrackOption;
        event EventHandler<EventArgs> AddManualTrackToList;
        event EventHandler<EventArgs> ReceiveManualTrack;
        event EventHandler<EventArgs> SelectedUnit;
        event EventHandler<EventArgs> RemoveManualTrack;
        event EventHandler<EventArgs> ShowReceivingTicket;
        event EventHandler<EventArgs> PrintLabels;
        //event EventHandler<DataEventArgs<string>> LoadBins;
        event EventHandler<DataEventArgs<DocumentBalance>> RemoveFromNode;
        event EventHandler<EventArgs> ConfirmOrder;
        event EventHandler<EventArgs> RefreshBin;
        event EventHandler<DataEventArgs<bool?>> LateDocuments;
        event EventHandler<EventArgs> NewDocument;

    }
}