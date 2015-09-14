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
using WpfFront.Common.UserControls;
using System.ComponentModel;
using System.Windows.Data;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class UC_IV_Replanish_PackingView : UserControlBase, IUC_IV_Replanish_PackingView
    {

        public UC_IV_Replanish_PackingView()
        {
            InitializeComponent();
        }

        //Events
        public event EventHandler<EventArgs> ProcessReplenish;
        public event EventHandler<EventArgs> LoadReplenishment;
        public event EventHandler<EventArgs> SelectAll;
        public event EventHandler<EventArgs> UnSelectAll;
        public event EventHandler<DataEventArgs<String>> FilterByBin;
        public event EventHandler<DataEventArgs<String>> FilterByProduct;
        public event EventHandler<DataEventArgs<ShowData>> ChangeSelector;
        public event EventHandler<DataEventArgs<bool>> ClearRecords;


        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        public bool loaded = false;


        public UC_IV_Model Model
        {
            get
            { return this.DataContext as UC_IV_Model; }
            set
            { this.DataContext = value; }

        }


        public DocumentList UCDocList
        {
            get { return this.ucDocList; }
            set { this.ucDocList = value; }
        }



        public TextBox CboProduct
        {
            get { return this.cboProduct; }
            set { this.cboProduct = value; }
        }

        public ListView DgRepList
        {
            get { return this.dgRepPack; }
            set { this.dgRepPack = value; }
        }


        public BinRange BinRange
        {
            get { return this.bRange; }
            set { this.bRange = value; }
        }

        public ComboBox CboSelector
        {
            get { return this.cboSelector; }
            set { this.cboSelector = value; }
        }

        public CheckBox ShowEmpty
        {
            get { return this.shEmpty; }
            set { this.shEmpty = value; }
        }


        public Boolean WasLoaded
        {
            get { return this.loaded; }
            set { this.loaded = value; }
        }


        private void btnProces_Click(object sender, RoutedEventArgs e)
        {
            btnProces.Focus();
            //Recorre el data grid y gener ana orden de empaque para los marcados.
            //Que deben proceder de los Bins con Stock.
            ProcessReplenish(sender, e);
        }

        private void cboLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReplenishment(sender, e);
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectAll(sender, e);

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UnSelectAll(sender, e);
        }


        //private void cboProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (this.cboProduct.SelectedItem == null)
        //        return;

        //    FilterByProduct(sender, new DataEventArgs<String>((String)cboProduct.SelectedItem));
        //}

        //private void imgRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    LoadReplenishment(sender, e);
        //}

        private void cboSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSelector.SelectedItem == null || !this.loaded)
                return;

            ChangeSelector(sender, new DataEventArgs<ShowData>((ShowData)cboSelector.SelectedItem));
        }

        private void bRange_OnLoadRange(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.bRange.Text))
                return;

            FilterByBin(sender, new DataEventArgs<String>(this.bRange.Text));
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            ClearRecords(sender, new DataEventArgs<bool>(false));
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            ClearRecords(sender, new DataEventArgs<bool>(true));
        }



        private void dgRepPack_Click(object sender, RoutedEventArgs e)
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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(dgRepPack.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        private void cboProduct_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboProduct.Text))
                return;

            FilterByProduct(sender, new DataEventArgs<String>((String)cboProduct.Text));
        }

        private void imgRep_Click(object sender, RoutedEventArgs e)
        {
            LoadReplenishment(sender, e);
        }

    }




    public interface IUC_IV_Replanish_PackingView
    {
        //Clase Modelo
        UC_IV_Model Model { get; set; }

        //Properties
        DocumentList UCDocList { get; set; }
        ListView DgRepList { get; set; }
        TextBox CboProduct { get; set; }
        ComboBox CboSelector { get; set; }
        BinRange BinRange { get; set; }
        CheckBox ShowEmpty { get; set; }
        Boolean WasLoaded { get; set; }

        //Events
        event EventHandler<EventArgs> ProcessReplenish;
        event EventHandler<EventArgs> LoadReplenishment;
        event EventHandler<EventArgs> SelectAll;
        event EventHandler<EventArgs> UnSelectAll;
        event EventHandler<DataEventArgs<String>> FilterByBin;
        event EventHandler<DataEventArgs<String>> FilterByProduct;
        event EventHandler<DataEventArgs<ShowData>> ChangeSelector;
        event EventHandler<DataEventArgs<bool>> ClearRecords;

    }
}