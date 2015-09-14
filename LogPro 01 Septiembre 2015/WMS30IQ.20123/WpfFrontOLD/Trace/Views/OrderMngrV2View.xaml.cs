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
    public partial class OrderMngrV2View : UserControlBase, IOrderMngrV2View
    {

        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;



        public OrderMngrV2View()
        {
            InitializeComponent();
            ucDocList.brDocList.Visibility = Visibility.Collapsed;
         }


        //View Events
        public event EventHandler<DataEventArgs<bool?>> LateDocuments;

        public event EventHandler<EventArgs> LoadDetails;

        public event EventHandler<EventArgs> CreateMergedDocument;

        public event EventHandler<EventArgs> ReloadLines;

        public event EventHandler<EventArgs> EnlistDetails;

        public event EventHandler<DataEventArgs<long>> LineChecked;

        public event EventHandler<DataEventArgs<long>> LineUnChecked;

        public event EventHandler<EventArgs> ResetPanel;

        public event EventHandler<EventArgs> RefineSearch;

        public event EventHandler<DataEventArgs<Product>> BOLineSelected;


        public OrderMngrV2Model Model
        {
            get
            { return this.DataContext as OrderMngrV2Model; }
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


        public DataGridControl DgSelected
        {
            get
            { return this.dgSelected; }
            set
            { this.dgSelected = value; }
        }



        public ListView DgDetails
        {
            get
            { return this.dgDetails; }
            set
            { this.dgDetails = value; }
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


        public ComboBox CboOrder
        {
            get { return this.cboDocs; }
            set { this.cboDocs = value; }
        }

        public ComboBox CboItem
        {
            get { return this.cboItem; }
            set { this.cboItem = value; }
        }

        public ComboBox CboProcess
        {
            get { return this.cboProcess; }
            set { this.cboProcess = value; }
        }

        public TextBox TxtComments
        {
            get { return this.txtComment; }
            set { this.txtComment = value; }
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




        private void cboAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboAccount.SelectedItem == null || cboAccount.SelectedIndex == 0)
                return;

            cboDocs.SelectedIndex = 0;
            cboItem.SelectedIndex = 0;
            RefineSearch(sender, e);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateMergedDocument(sender, e);
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

            //if (tabStep.SelectedIndex == 0 && cboAccount.SelectedItem != null)
            //{
            //    AccountSelectedAllLines(this, new DataEventArgs<ShowData>((ShowData)cboAccount.SelectedItem));
            //}

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


        private void imgDocs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ucDocList.brDocList.Visibility = Visibility.Visible;
        }


        private void chkAll_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void uncheckAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
        
        }

        private void cboDocs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDocs.SelectedItem == null || cboDocs.SelectedIndex == 0)
                return;

            cboAccount.SelectedIndex = 0;
            cboItem.SelectedIndex = 0;
            RefineSearch(sender, e);
        }

        private void cboItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboItem.SelectedItem == null || cboItem.SelectedIndex == 0)
                return;

            cboDocs.SelectedIndex = 0;
            cboAccount.SelectedIndex = 0;
            RefineSearch(sender, e);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefineSearch(sender, e);
        }

        private void bntReset_Click(object sender, RoutedEventArgs e)
        {
            //Resetea todo el modelo y arranca de cero.
            cboAccount.SelectedIndex = 0;
            cboItem.SelectedIndex = 0;
            cboDocs.SelectedIndex = 0;

            RefineSearch(sender, e);
        }

        private void dgDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgVendorDetails.Visibility = Visibility.Hidden;

            if (dgDetails.SelectedItem == null)
                return;

            DocumentLine l = dgDetails.SelectedItem as DocumentLine;

            BOLineSelected(this, new DataEventArgs<Product>(l.Product));

            dgVendorDetails.Visibility = Visibility.Visible;
        }


    }


    public interface IOrderMngrV2View
    {

        OrderMngrV2Model Model { get; set; }


        TabControl TabStep { get; set; }
        DataGridControl DgSelected { get; set; }
        ListView DgDetails { get; set; }
        DocumentList UCDocList { get; set; }
        ComboBox CboAccount { get; set; }
        TextBox TxtComments { get; set; }
        AdminDocumentLine UcDocLine { get; set; }
        Popup Popup3 { get; set; }
        ComboBox CboOrder { get; set; }
        ComboBox CboItem { get; set; }
        ComboBox CboProcess { get; set; }
        




        //View Events
        event EventHandler<DataEventArgs<bool?>> LateDocuments;
        event EventHandler<EventArgs> LoadDetails;
        event EventHandler<EventArgs> CreateMergedDocument;
        event EventHandler<EventArgs> EnlistDetails;
        event EventHandler<DataEventArgs<long>> LineChecked;
        event EventHandler<DataEventArgs<long>> LineUnChecked;
        event EventHandler<EventArgs> ResetPanel;
        event EventHandler<EventArgs> RefineSearch;
        event EventHandler<DataEventArgs<Product>> BOLineSelected;

    }



}
