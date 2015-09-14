using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class PrintingView : UserControlBase, IPrintingView
    {
        public PrintingView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<EventArgs> LoadPrintForm;
        //public event EventHandler<DataEventArgs<string>> LoadProducts;
        public event EventHandler<DataEventArgs<Product>> LoadUnits;
        public event EventHandler<EventArgs> AddToPrint;
        public event EventHandler<EventArgs> PrintLabels;
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<DataEventArgs<Document>> LoadPrintLines;
        public event EventHandler<DataEventArgs<Unit>> SelectPack;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<DataEventArgs<bool?>> RefreshLabelList;
        public event EventHandler<EventArgs> PrintPreview;
        public event EventHandler<EventArgs> GenerateLabels;
        public event EventHandler<EventArgs> ResetForm;

        public PrintingModel Model
        {
            get
            { return this.DataContext as PrintingModel; }
            set
            { this.DataContext = value; }

        }



        #region Properties



        public TextBlock ProcessResult
        {
            get { return this.txtProcessResult; }
            set { this.txtProcessResult = value; }
        }


        public ListView ComboUnit
        {
            get { return this.cboUnit; }
        }


        //public ListView ComboProduct
        //{
        //    get { return this.cboProduct; }
        //}


        public ListView ToPrintLines
        {
            get { return this.lvListToPrint; }
        }

        public ListView ToPrintLabels
        {
            get { return this.lvLabelsToProcess; }
        }


        public ComboBox ComboLabelType
        {
            get { return this.cboLabelType; }
            set { this.cboLabelType = value; }
        }


        public TextBox TxtPrintQty
        {
            get { return this.txtQuantity; }
        }

        public StackPanel StkPrintForm
        {
            get { return this.stkPrintForm; }
        }


        public StackPanel StkPrintManually
        {
            get { return this.stkPrintManually; }
        }

        public StackPanel StkPrintByDocument
        {
            get { return this.stkPrintByDocument; }
        }

        public StackPanel StkPrintFinish
        {
            get { return this.stkPrintFinish; }
        }


        public ComboBox PreviewTemplate
        {
            get { return this.cboTemplates; }
        }

        public ComboBox PrintTemplate
        {
            get { return this.cboPrintTemplate; }
        }


        public TextBlock PrintLot
        {
            get { return this.tbkPrintLot; }
            set { this.tbkPrintLot = value; }
        }


        public ComboBox PrinterList
        {
            get { return this.cboPrinter; }
        }


        public ListView DocumentList
        {
            get { return this.lvDocuments; }
        }


        public ComboBox LogisticUnit
        {
            get { return this.cboLogiUnit; }
        }

        //public CheckBox ChkOnlyLogistic
        //{
        //    get { return this.chkOnlyLogistic; }
        //}

        public SearchProduct TxtProduct
        {
            get { return this.txtProduct; }
            set { this.txtProduct = value; }
        }

        public TextBox TxtQtyPerPack
        {
            get { return this.txtQtyPerPack; }
            set { this.txtQtyPerPack = value; }
        }

        public StackPanel StkLine
        {
            get { return this.stkLine; }
            set { this.stkLine = value; }
        }

        public StackPanel StkLabel
        {
            get { return this.stkLabel; }
            set { this.stkLabel = value; }
        }

        public RadioButton RbDocument 
        {
            get { return this.rbDocument; }
            set { this.rbDocument = value; }
        }


        public Border BrdPreview
        {
            get { return this.brdPreview; }
            set { this.brdPreview = value; }
        }


        public Border BrdGenerate
        {
            get { return this.brdGenerate; }
            set { this.brdGenerate = value; }
        }

        public Button BtnGenerate
        {
            get { return this.btnGenerate; }
            set { this.btnGenerate = value; }
        }


        public Border BrFinishPrint 
        {
            get { return this.brFinishPrint; }
            set { this.brFinishPrint = value; }
        }

        public Button BtnPrint
        {
            get { return this.btnPrint; }
            set { this.btnPrint = value; }
        }

        public RadioButton RbManual
        {
            get { return this.rbManual; }
            set { this.rbManual = value; }
        }

        public Button BtnConfirm 
        {
            get { return this.btnConfirm; }
            set { this.btnConfirm = value; }
        }

        #endregion



        #region ViewEvents



        //private void cboProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LoadUnits(this, new DataEventArgs<Product>((Product)cboProduct.SelectedItem));
        //    this.TxtPrintQty.Text = "0";
        //}


        private void RadioButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadPrintForm(sender, e);
        }


        //private void txtProduct_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    LoadProducts(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //Add The Document Line to The PrintList
            AddToPrint(sender, e);
        }


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //Sent to Print the List 
            PrintLabels(sender, e);
        }


        private void lvDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Carga las lineas del documento a imprimir
            LoadPrintLines(sender, new DataEventArgs<Document>((Document)lvDocuments.SelectedItem));
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }



        private void cboLogiUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (((ComboBox)sender).SelectedIndex != -1)
            //    chkOnlyLogistic.Visibility = Visibility.Visible;

            Unit unit = ((ComboBox)sender).SelectedItem as Unit;
            SelectPack(sender, new DataEventArgs<Unit>(unit));

        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAllLabel.Name)
            {
                this.lvLabelsToProcess.SelectAll();
                this.lvLabelsToProcess.Focus();
            }
            else if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            {
                this.lvListToPrint.SelectAll();
                this.lvListToPrint.Focus();
            }

        }


        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAllLabel.Name)
                this.lvLabelsToProcess.UnselectAll();

            else if (((CheckBox)sender).Name == chkSelectAllLines.Name)
                this.lvListToPrint.UnselectAll();

        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }

        private void chkOnlyPack_Checked(object sender, RoutedEventArgs e)
        {
            //Show OnlyPack Unit
            bool? onlypack = ((CheckBox)sender).IsChecked;
            RefreshLabelList(this, new DataEventArgs<bool?>(onlypack));

        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            PrintPreview(sender, e);
        }

        #endregion

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateLabels(sender, e);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetForm(sender, e);
        }

        private void txtProduct_OnLoadRecord(object sender, EventArgs e)
        {
            LoadUnits(this, new DataEventArgs<Product>((Product)this.txtProduct.Product));
            this.TxtPrintQty.Text = "0";
        }

        //private void txtQtyPerPack_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(this.txtQtyPerPack.Text))
        //        chkOnlyLogistic.Visibility = Visibility.Visible;
        //}


 

    }



    public interface IPrintingView
    {

        StackPanel StkPrintForm { get; }


        //Manually Print
        TextBlock ProcessResult { get; set; }
        TextBox TxtPrintQty { get; }
        ComboBox ComboLabelType { get; }
        //ListView ComboProduct { get; }
        ListView ComboUnit { get; }
        StackPanel StkPrintManually { get; }

        //By Document
        StackPanel StkPrintByDocument { get; }
        ListView DocumentList { get; }


        ListView ToPrintLines { get; }


        //Print Options
        TextBlock PrintLot { get; set; }

        ComboBox PrintTemplate { get; }
        ComboBox PreviewTemplate { get; }

        ComboBox PrinterList { get; }
        StackPanel StkPrintFinish { get; }

        ComboBox LogisticUnit { get; }
        //CheckBox ChkOnlyLogistic { get; }
        SearchProduct TxtProduct { get; set; }
        TextBox TxtQtyPerPack { get; set; }
        StackPanel StkLine { get; set; }
        StackPanel StkLabel { get; set; }
        ListView ToPrintLabels { get; }
        RadioButton RbDocument { get; set; }
        Border BrdPreview { get; set; }
        Border BrdGenerate { get; set; }
        Border BrFinishPrint { get; set; }
        Button BtnPrint { get; set; }
        RadioButton RbManual { get; set; }
        Button BtnGenerate { get; set; }
        Button BtnConfirm { get; set; }

        PrintingModel Model { get; set; }

        event EventHandler<EventArgs> LoadPrintForm;
        //event EventHandler<DataEventArgs<string>> LoadProducts;
        event EventHandler<DataEventArgs<Product>> LoadUnits;
        event EventHandler<EventArgs> AddToPrint;
        event EventHandler<EventArgs> PrintLabels;
        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<DataEventArgs<Document>> LoadPrintLines;
        event EventHandler<DataEventArgs<Unit>> SelectPack;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<DataEventArgs<bool?>> RefreshLabelList;
        event EventHandler<EventArgs> PrintPreview;
        event EventHandler<EventArgs> GenerateLabels;
        event EventHandler<EventArgs> ResetForm;
    }
}