using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common.UserControls;
using System.Windows.Input;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class InventoryAdjustmentView : UserControlBase, IInventoryAdjustmentView
    {
        public InventoryAdjustmentView()
        {
            InitializeComponent();
        }



        //View Events
        public event EventHandler<EventArgs> LoadForm;
        public event EventHandler<DataEventArgs<Product>> LoadUnits;
        public event EventHandler<EventArgs> AddToConfirm;
        //public event EventHandler<EventArgs> AddChildToConfirm;
        public event EventHandler<DataEventArgs<DocumentConcept>> ExeInventoryAdjustment;
        //public event EventHandler<DataEventArgs<string>> SearchDocument;
        //public event EventHandler<DataEventArgs<Document>> LoadProcessLines;
        public event EventHandler<DataEventArgs<string>> LoadSourceLocation;
        //public event EventHandler<DataEventArgs<string>> LoadDestLocation;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<EventArgs> ResetForm;
        public event EventHandler<DataEventArgs<Document>> LoadAdjustment;
        public event EventHandler<EventArgs> ReverseAdjustment;
        public event EventHandler<DataEventArgs<string>> AddSerial;
        public event EventHandler<EventArgs> ResendToERP;
        public event EventHandler<DataEventArgs<string>> SearchDocument;


        public InventoryAdjustmentModel Model
        {
            get
            { return this.DataContext as InventoryAdjustmentModel; }
            set
            { this.DataContext = value; }

        }



        #region Properties



        //public TextBlock ProcessResult
        //{
        //    get { return this.txtProcessResult; }
        //    set { this.txtProcessResult = value; }
        //}


        public ListView ComboUnit
        {
            get { return this.cboUnit; }
        }


        public SearchProduct ComboProduct
        {
            get { return this.cboProduct; }
        }


        public ListView ToProcessLines
        {
            get { return this.lvListToProcess; }
        }



        public ComboBox AdjType
        {
            get { return this.txtAdjType; }
            set { this.AdjType = value; }
        }



        public TextBox TxtQty
        {
            get { return this.txtQuantity; }
        }

        public StackPanel StkForm
        {
            get { return this.stkForm; }
        }


        public StackPanel StkManually
        {
            get { return this.stkManually; }
        }




        public StackPanel StkFinish
        {
            get { return this.stkFinish; }
        }


        public Button BtnExecute
        {
            get { return this.btnMove; }
            set { this.btnMove = value; }
        }

        public Button BtnConfirm
        {
            get { return this.btnConfirm; }
            set { this.btnConfirm = value; }
        }

        public TextBox TxtComment
        {
            get { return this.txtComment; }
            set { this.txtComment = value; }
        }

        //public ListView ListSource
        //{
        //    get { return this.lvSourceData; }
        //}


        //public TextBox TxtProduct
        //{
        //    get { return this.txtProduct; }
        //    set { this.txtProduct = value; }
        //}

        public BinLocation TxtSource
        {
            get { return this.txtSourceLocation; }
            set { this.txtSourceLocation = value; }
        }

        public Border BrCart
        {
            get { return this.brCart; }
        }

        public StackPanel StkAdjustData
        {
            get { return this.stkAdjustData; }
            set { this.stkAdjustData = value; }
        }

        public Button BtnReverse
        {
            get { return this.btnReversePosted; }
            set { this.btnReversePosted = value; }
        }

        public ComboBox CboConcept
        {
            get { return this.cboConcepts; }
            set { this.cboConcepts = value; }
        }

        public Border BrSerials
        {
            get { return this.brSerials; }
            set { this.brSerials = value; }
        }

        public Button BtnReSend
        {
            get { return this.btnSendErp; }
            set { this.btnSendErp = value; }
        }


        public ListView ListAdj
        {
            get { return this.lvAdjustments; }
            set { this.lvAdjustments = value; }
        }


        #endregion



        #region ViewEvents



        private void cboProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProduct.Product == null)
                return;

            LoadUnits(this, new DataEventArgs<Product>(cboProduct.Product));
            this.TxtQty.Text = "";
        }


        private void RadioButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadForm(sender, e);
        }




        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //Add The Document Line to The List
            AddToConfirm(sender, e);
        }



        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            ExeInventoryAdjustment(sender, 
                new DataEventArgs<DocumentConcept>((DocumentConcept)cboConcepts.SelectedItem));
        }


        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            LoadSourceLocation(this, new DataEventArgs<string>(txtSourceLocation.Text));
        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            {
                this.lvListToProcess.SelectAll();
                this.lvListToProcess.Focus();
            }



        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
                this.lvListToProcess.UnselectAll();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetForm(sender, e);

        }

        private void lvAdjustments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAdjustment(sender, new DataEventArgs<Document>((Document)lvAdjustments.SelectedItem));
        }

        private void btnReversePosted_Click(object sender, RoutedEventArgs e)
        {
            ReverseAdjustment(sender, e);
        }

        private void txtSourceLocation_OnLoadLocation(object sender, EventArgs e)
        {
            LoadSourceLocation(this, new DataEventArgs<string>(txtSourceLocation.Text));
        }


        private void cboProduct_OnLoadRecord(object sender, EventArgs e)
        {
            if (this.cboProduct.Product == null)
                return;

            LoadUnits(sender, new DataEventArgs<Product>(this.cboProduct.Product));

            txtSourceLocation.Product = this.cboProduct.Product;

        }

        private void btnAddSerial_Click(object sender, RoutedEventArgs e)
        {
            AddSerial(sender, new DataEventArgs<string>(txtSerial.Text));
        }

        #endregion

        private void btnSendErp_Click(object sender, RoutedEventArgs e)
        {
            ResendToERP(sender, e);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(""));
        }

    }



    public interface IInventoryAdjustmentView
    {

        StackPanel StkForm { get; }

        //Manually Print
        //TextBox TxtProduct { get; set; }
        //TextBlock ProcessResult { get; set; }
        TextBox TxtQty { get; }
        ComboBox AdjType { get; set;  }
        SearchProduct ComboProduct { get; }
        ListView ComboUnit { get; }
        StackPanel StkManually { get; }
        BinLocation TxtSource { get; set; }
        Border BrCart { get; }

        //ListView ListSource { get; }
        ListView ToProcessLines { get; }

        //Moving Options
        StackPanel StkFinish { get; }
        Button BtnExecute { get; set; }
        TextBox TxtComment { get; }

        StackPanel StkAdjustData { get; set; }
        Button BtnReverse { get; set; }
        ComboBox CboConcept { get; set; }
        Button BtnConfirm { get; set; }
        Border BrSerials { get; set; }
        Button BtnReSend { get; set; }
        ListView ListAdj { get; set; }

        InventoryAdjustmentModel Model { get; set; }


        event EventHandler<EventArgs> LoadForm;
        event EventHandler<DataEventArgs<Product>> LoadUnits;
        event EventHandler<EventArgs> AddToConfirm;
        event EventHandler<DataEventArgs<DocumentConcept>> ExeInventoryAdjustment;
        event EventHandler<DataEventArgs<string>> LoadSourceLocation;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<EventArgs> ResetForm;
        event EventHandler<DataEventArgs<Document>> LoadAdjustment;
        event EventHandler<EventArgs> ReverseAdjustment;

        event EventHandler<DataEventArgs<string>> AddSerial;
        event EventHandler<EventArgs> ResendToERP;
        event EventHandler<DataEventArgs<string>> SearchDocument;
    }
}