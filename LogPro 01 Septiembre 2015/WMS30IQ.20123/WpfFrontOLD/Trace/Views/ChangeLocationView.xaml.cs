using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common.UserControls;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class ChangeLocationView : UserControlBase, IChangeLocationView
    {
        public ChangeLocationView()
        {
            InitializeComponent();
        }



        //View Events
        public event EventHandler<EventArgs> LoadBins;
        public event EventHandler<EventArgs> LoadBinsD;
        //public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<EventArgs> MoveSelected;
        public event EventHandler<EventArgs> MoveRetail;
        public event EventHandler<EventArgs> MoveAll;


        public ChangeLocationModel Model
        {
            get
            { return this.DataContext as ChangeLocationModel; }
            set
            { this.DataContext = value; }

        }


        public BinLocation BinLocation
        {
            get { return this.ucBinLocation; }
        }

        public TextBox TxtQty
        {
            get { return this.txtQty; }
            set { this.txtQty = value; }
        }

        public BinLocation BinLocationD
        {
            get { return this.ucBinLocationD; }
        }


        public StackPanel StkRetail
        {
            get { return this.stkRetail; }
        }

        public StackPanel StkLabel
        {
            get { return this.stkLabel; }
        }


        public Border BrMove
        {
            get { return this.brMove; }
        }

        public Border BrDestination
        {
            get { return this.brDest; }
        }

        public StackPanel StkMovedData
        {
            get { return this.stkMovedData; }
        }


        public ListView LvLabelsToMove {
            get { return this.lvLabelsToProcess; }
            set { this.lvLabelsToProcess = value; }
        }

        public ListView LvProductToMove { 
            get { return this.lvListToProcess; }
            set { this.lvListToProcess = value; }
        }

        public ListView LvLabelsMoved
        {
            get { return this.lvLabelsMoved; }
            set { this.lvLabelsMoved = value; }
        }

        public ListView LvProductMoved
        {
            get { return this.lvListMoved; }
            set { this.lvListMoved = value; }
        }


        public Button BtnMoveAll
        {
            get { return this.btnMoveAll; }
            set { this.btnMoveAll = value; }
        }

        public Button BtnMoveLabel
        {
            get { return this.btnMove; }
            set { this.btnMove = value; }
        }


        public StackPanel StkQtyRetail
        {
            get { return this.stkQtyRetail; }
            set { this.stkQtyRetail = value; }
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            {
                this.lvListToProcess.SelectAll();
                this.lvListToProcess.Focus();
            }
            else if (((CheckBox)sender).Name == chkSelectAllLabel.Name)
            {
                this.lvLabelsToProcess.SelectAll();
                this.lvLabelsToProcess.Focus();
            }

        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
 
           if (((CheckBox)sender).Name == chkSelectAllLines.Name)
                this.lvListToProcess.UnselectAll();

            else if (((CheckBox)sender).Name == chkSelectAllLabel.Name)
                this.lvLabelsToProcess.UnselectAll();

        }


        //private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        //}

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            //btnMoveSelected
            MoveSelected(sender, e);
        }

        private void btnMoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (!UtilWindow.ConfirmOK("Really wish to consolidate Bins?") == true)
                return;

            MoveAll(sender, e);
        }


        private void ucBinLocation_OnLoadLocation(object sender, EventArgs e)
        {
            LoadBins(sender, e);
        }

        private void ucBinLocationD_OnLoadLocation(object sender, EventArgs e)
        {
            LoadBinsD(sender, e);
        }

        private void btnMoveRetail_Click(object sender, RoutedEventArgs e)
        {
            MoveRetail(sender, e);
        }

        private void lvListToProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvListToProcess.SelectedItem == null)
                return;

            ProductStock line = lvListToProcess.SelectedItem  as ProductStock;

            this.txtQty.Text = line.Stock.ToString();
        }





    }



    public interface IChangeLocationView
    {

        BinLocation BinLocation { get; }
        StackPanel StkRetail { get; }
        StackPanel StkLabel { get; }
        BinLocation BinLocationD { get; }


        Border BrMove { get;  }
        Border BrDestination { get; }
        StackPanel StkMovedData { get; }


        ListView LvLabelsToMove { get; set; }
        ListView LvProductToMove { get; set; }
        ListView LvLabelsMoved { get; set; }
        ListView LvProductMoved { get; set; }

        TextBox TxtQty { get; }
        Button BtnMoveAll { get; set; }
        Button BtnMoveLabel { get; set; }
        StackPanel StkQtyRetail { get; set; }


        ChangeLocationModel Model { get; set; }


        event EventHandler<EventArgs> LoadBins;
        event EventHandler<EventArgs> LoadBinsD;
        //event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<EventArgs> MoveSelected;
        event EventHandler<EventArgs> MoveRetail;
        event EventHandler<EventArgs> MoveAll;
    }
}