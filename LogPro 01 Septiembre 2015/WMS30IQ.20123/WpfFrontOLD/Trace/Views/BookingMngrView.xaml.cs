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
using Xceed.Wpf.DataGrid.Settings;
using System.Collections;
using System.Data;
using System.Windows.Media;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class BookingMngrView : UserControlBase, IBookingMngrView
    {

        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;



        public BookingMngrView()
        {
            InitializeComponent();
            ucDocList.brDocList.Visibility = Visibility.Collapsed;
         }


        //View Events
        public event EventHandler<DataEventArgs<bool?>> LateDocuments;

        public event EventHandler<DataEventArgs<System.Data.DataRow>> LoadDetails;

        public event EventHandler<DataEventArgs<ShowData>> AccountSelected;

        public event EventHandler<EventArgs> CreateMergedDocument;

        public event EventHandler<EventArgs> ReloadLines;

        public event EventHandler<EventArgs> EnlistDetails;

        public event EventHandler<DataEventArgs<object[]>> CheckLineBalanceCancel;

        public event EventHandler<DataEventArgs<object[]>> CheckLineBalanceBO;

        public event EventHandler<DataEventArgs<long>> LineChecked;

        public event EventHandler<DataEventArgs<long>> LineUnChecked;

        public event EventHandler<DataEventArgs<int>> DocumentChecked;

        public event EventHandler<DataEventArgs<int>> DocumentUnChecked;

        public event EventHandler<DataEventArgs<ShowData>> AccountSelectedAllLines;

        public event EventHandler<DataEventArgs<DocumentLine>> LoadPopupLine;

        public event EventHandler<DataEventArgs<DocumentLine>> CancelLine;

        public event EventHandler<EventArgs> RefreshAddress;

        public event EventHandler<DataEventArgs<LabelTemplate>> ShowDocument;

        public event EventHandler<DataEventArgs<ShowData>> StartProcess;

        public event EventHandler<DataEventArgs<Document>> LoadDocument;
        
        public event EventHandler<EventArgs> UpdateAdditionalInfo; 


        public BookingMngrModel Model
        {
            get
            { return this.DataContext as BookingMngrModel; }
            set
            { this.DataContext = value; }

        }


        public TabControl TabStep
        {
            get
            { return this.tabStep; }
            set
            { this.tabStep = value; }
        }

        public DataGridControl DgDocument
        {
            get
            { return this.dgDocument; }
            set
            { this.dgDocument = value; }
        }

        public DataGridControl DgSelected
        {
            get
            { return this.dgSelected; }
            set
            { this.dgSelected = value; }
        }



        public ComboBox CboShipTo
        {
            get
            { return this.cboShipTo; }
            set
            { this.cboShipTo = value; }
        }

        public ComboBox CboToDo
        {
            get
            { return this.cboToDo; }
            set
            { this.cboToDo = value; }
        }



        public ListView DgDetails
        {
            get
            { return this.dgDetails; }
            set
            { this.dgDetails = value; }
        }


        public CheckBox ChkFilter
        {
            get
            { return this.filter; }
            set
            { this.filter = value; }
        }

        public DocumentList UCDocList
        {
            get { return this.ucDocList; }
            set { this.ucDocList = value; }
        }


        public ComboBox CboAccount
        {
            get { return this.cboAccount; }
            set { this.cboAccount = value; }
        }


        public ComboBox CboOpenDoc
        {
            get { return this.cboDoc; }
            set { this.cboDoc = value; }
        }


        public TextBox TxtComments
        {
            get { return this.txtComment; }
            set { this.txtComment = value; }
        }


        public StackPanel StkAddInfo
        {
            get { return this.stkAddInfo; }
            set { this.stkAddInfo = value; }
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

        public OdcExpander ExpAddinfo
        {
            get { return this.expBkInfo; }
            set { this.expBkInfo = value; }
            
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




        private void cboAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //If Account Selected 
            if (cboAccount.SelectedItem == null)
                return;

            //if (filter.IsChecked == true)
                //AccountSelected(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));
            //else
                
                StartProcess(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));
                filter.IsChecked = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateMergedDocument(sender, e);
        }


        private void dgDocument_GotFocus(object sender, RoutedEventArgs e)
        {
            if (dgDocument.SelectedItem == null)
                return;

            LoadDetails(this, new DataEventArgs<System.Data.DataRow>((System.Data.DataRow)dgDocument.SelectedItem));
        }



        private void dgDetails_Click(object sender, RoutedEventArgs e)
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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(dgDetails.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }



        private void tabStep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabStep.SelectedItem == null || tabStep.SelectedIndex != 1)
                return;

            if (tabStep.SelectedIndex == 1)
                EnlistDetails(sender, e);

            if (tabStep.SelectedIndex == 0 && cboAccount.SelectedItem != null)
            {
                if (filter.IsChecked == true)
                    AccountSelected(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));
                else
                    AccountSelectedAllLines(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));
            }

        }




        private void qtyBO_LostFocus(object sender, RoutedEventArgs e)
        {
            //Check Qty in Line.
            long line = long.Parse(((TextBox)(sender)).Tag.ToString());
            double qty = double.Parse( ((TextBox)sender).Text );
            CheckLineBalanceBO(this, new DataEventArgs<object[]>(new object[] {line, qty }));
        }


        private void qtyCan_LostFocus(object sender, RoutedEventArgs e)
        {
            long line = long.Parse(((TextBox)(sender)).Tag.ToString());
            double qty = double.Parse(((TextBox)sender).Text);
            CheckLineBalanceCancel(this, new DataEventArgs<object[]>(new object[] { line, qty }));
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Check the line for teh balance. Show message if problem.
            long line = long.Parse(((CheckBox)(sender)).CommandParameter.ToString());
            LineChecked(this, new DataEventArgs<long>(line));
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Adjust the availabilty balance.
            long line = long.Parse(((CheckBox)(sender)).CommandParameter.ToString());
            LineUnChecked(this, new DataEventArgs<long>(line));
        }


        private void chkAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Check All Checkbox for the cuerren document
            if (dgDocument.SelectedItem == null)
                return;

            int docID = int.Parse(((System.Data.DataRow)dgDocument.SelectedItem)["DocID"].ToString());

            DocumentChecked(this, new DataEventArgs<int>(docID));
        }


        private void uncheckAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (dgDocument.SelectedItem == null)
                return;

            //Uncheck for the current document.
            int docID = int.Parse(((System.Data.DataRow)dgDocument.SelectedItem)["DocID"].ToString());
            DocumentUnChecked(this, new DataEventArgs<int>(docID));
        }


        private void filter_Checked(object sender, RoutedEventArgs e)
        {
            dgDocument.Visibility = Visibility.Visible;
            gvLines.Columns[1].Width = 0; //Hide DocNumber
            docName.Visibility = Visibility.Visible;
            
            if (cboAccount.SelectedItem == null)
                return;

            dgDocument.SelectedIndex = 0;
            LoadDetails(this, new DataEventArgs<System.Data.DataRow>((System.Data.DataRow)dgDocument.SelectedItem));
        }

        private void filter_Unchecked(object sender, RoutedEventArgs e)
        {
            dgDocument.Visibility = Visibility.Collapsed;
            gvLines.Columns[1].Width = 100; //Show DocNumber
            docName.Visibility = Visibility.Collapsed;

            if (cboAccount.SelectedItem == null)
                return;

            this.Model.CurrentDetails = this.Model.OrdersDetail;

        }

        private void imgDocs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ucDocList.brDocList.Visibility = Visibility.Visible;
        }


        private void dgDetails_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDetails.SelectedItem == null)
                return;

            //Open the Line Popup to make line changes
            LoadPopupLine(sender, new DataEventArgs<DocumentLine>((DocumentLine)dgDetails.SelectedItem));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            LoadPopupLine(sender, new DataEventArgs<DocumentLine>(new DocumentLine()));
        }



        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //remover la linea de la Orden y de GP
            if (dgDetails.SelectedItem == null)
            {
                Util.ShowError("No line selected.");
                return;
            }

            if (UtilWindow.ConfirmOK("Are you sure about remove this line?") == true)
            {
                DocumentLine line = dgDetails.SelectedItem as DocumentLine;
                CancelLine(sender, new DataEventArgs<DocumentLine>(line));
            }

        }

        private void cboShipTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboShipTo.SelectedIndex == 0)
            {
                popAddr.IsOpen = true;
                ucAccuntAddr.LoadAddresses(this.Model.Customer);
            }
        }

        private void txtHide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popAddr.IsOpen = false;
            cboShipTo.SelectedIndex = -1;
            RefreshAddress(sender, e);
        }


        private void imgDocs_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (cboDocList.SelectedItem == null)
                return;

            ShowDocument(this, new DataEventArgs<LabelTemplate>((LabelTemplate)cboDocList.SelectedItem));
        }

        private void cboDoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDoc.SelectedItem == null)
                return;

            AccountSelectedAllLines(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));

            //Si es un existing manda a cargarlo.
            if (cboDoc.SelectedIndex > 0)
                LoadDocument(this, new DataEventArgs<Document>((Document)cboDoc.SelectedItem));
        }


        private void btnUpdInfo_Click(object sender, RoutedEventArgs e)
        {
            UpdateAdditionalInfo(sender, e);
        }



        private void imgRfr_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cboAccount.SelectedItem == null)
                return;

            AccountSelectedAllLines(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));

            filter.IsChecked = false;
        }


        private void cboDocList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDocList.SelectedItem == null)
                return;

            ShowDocument(this, new DataEventArgs<LabelTemplate>((LabelTemplate)cboDocList.SelectedItem));
        }

    }


    public interface IBookingMngrView
    {

        BookingMngrModel Model { get; set; }


        TabControl TabStep { get; set; }
        DataGridControl DgDocument { get; set; }
        DataGridControl DgSelected { get; set; }
        ComboBox CboToDo { get; set; }
        ListView DgDetails { get; set; }
        ComboBox CboShipTo { get; set; }
        CheckBox ChkFilter { get; set; }
        DocumentList UCDocList { get; set; }
        ComboBox CboAccount { get; set; }
        TextBox TxtComments { get; set; }
        AdminDocumentLine UcDocLine { get; set; }
        Popup Popup3 { get; set; }
        StackPanel StkUpdLines { get; set; }
        ComboBox CboOpenDoc { get; set; }
        StackPanel StkAddInfo { get; set; }
        OdcExpander ExpAddinfo { get; set; }



        //View Events
        event EventHandler<DataEventArgs<bool?>> LateDocuments;
        
        event EventHandler<DataEventArgs<System.Data.DataRow>> LoadDetails;               
        
        event EventHandler<DataEventArgs<ShowData>> AccountSelected;
        
        event EventHandler<EventArgs> CreateMergedDocument;
        
        event EventHandler<EventArgs> EnlistDetails;
        
        event EventHandler<DataEventArgs<object[]>> CheckLineBalanceBO;
        
        event EventHandler<DataEventArgs<object[]>> CheckLineBalanceCancel;
        
        event EventHandler<DataEventArgs<long>> LineChecked;
        
        event EventHandler<DataEventArgs<long>> LineUnChecked;

        event EventHandler<DataEventArgs<int>> DocumentChecked;
        
        event EventHandler<DataEventArgs<int>> DocumentUnChecked;
        
        event EventHandler<DataEventArgs<ShowData>> AccountSelectedAllLines;
        
        event EventHandler<DataEventArgs<DocumentLine>> LoadPopupLine;
        
        event EventHandler<DataEventArgs<DocumentLine>> CancelLine;

        event EventHandler<EventArgs> RefreshAddress;

        event EventHandler<DataEventArgs<LabelTemplate>> ShowDocument;

        event EventHandler<EventArgs> UpdateAdditionalInfo; 

        event EventHandler<DataEventArgs<ShowData>> StartProcess;

        event EventHandler<DataEventArgs<Document>> LoadDocument;

    }



}
