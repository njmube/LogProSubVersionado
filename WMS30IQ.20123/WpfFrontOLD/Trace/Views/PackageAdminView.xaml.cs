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
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for PackageAdminView.xaml
    /// </summary>
    public partial class PackageAdminView : UserControlBase, IPackageAdminView
    {

        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;


        public PackageAdminView()
        {
            InitializeComponent();
        }


        public PackageAdminModel Model
        {
            get
            { return this.DataContext as PackageAdminModel; }
            set
            { this.DataContext = value; }

        }


        public ComboBox CboPack2
        {
            get { return this.cboPack2; }
            set { this.cboPack2 = value; }
        }

        public ComboBox CboPack1
        {
            get { return this.cboPack1; }
            set { this.cboPack1 = value; }
        }


        public ListView LvDetails1
        {
            get { return this.pkgDetails1; }
            set { this.pkgDetails1 = value; }
        }

        public TextBox TxtQty
        {
            get { return this.txtQty; }
            set { this.txtQty = value; }
        }


        public Button BtnUnpick
        {
            get { return this.btnUnPick; }
            set { this.btnUnPick = value; }
        }



        //Events
        public event EventHandler<DataEventArgs<DocumentPackage>> LoadPackDetails;
        public event EventHandler<DataEventArgs<DocumentPackage>> LoadPackDetails2;
        public event EventHandler<EventArgs> MoveSelected;
        public event EventHandler<EventArgs> MoveRetail;
        public event EventHandler<EventArgs> UnPickSelected;



        private void btnMoveRetail_Click(object sender, RoutedEventArgs e)
        {
            MoveRetail(sender, e);
        }


        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            MoveSelected(sender, e);
        }


        private void cboPack1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Pack Origen Seleccionado.
            stkDest2.IsEnabled = false;

            if (cboPack1.SelectedItem == null)
                return;

            stkDest2.IsEnabled = true;

            LoadPackDetails(this, new DataEventArgs<DocumentPackage>((DocumentPackage)cboPack1.SelectedItem));
            
        }


        private void pkgDetails1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            brMove.IsEnabled = false;

            if (pkgDetails1.SelectedItem == null)
                return;

            if (cboPack2.SelectedItem != null)
                brMove.IsEnabled = true;


            //Load Qty Product in textBox
            ProductStock line = pkgDetails1.SelectedItem as ProductStock;
            this.txtQty.Text = line.Stock.ToString();

        }


        private void cboPack2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            brMove.IsEnabled = false;

            if (cboPack2.SelectedItem == null)
                return;

            if (pkgDetails1.SelectedItem != null)
                brMove.IsEnabled = true;

            LoadPackDetails2(this, new DataEventArgs<DocumentPackage>((DocumentPackage)cboPack2.SelectedItem));
        }


        private void pkgDetails1_Click(object sender, RoutedEventArgs e)
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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(pkgDetails1.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        private void btnUnPick_Click(object sender, RoutedEventArgs e)
        {
            UnPickSelected(sender, e);
        }

    }


    public interface IPackageAdminView
    {
        PackageAdminModel Model { get; set; }

        ComboBox CboPack2 { get; set; }
        ComboBox CboPack1 { get; set; }
        ListView LvDetails1{ get; set; }
        TextBox TxtQty { get; set; }
        Button BtnUnpick { get; set; }

        event EventHandler<DataEventArgs<DocumentPackage>> LoadPackDetails;
        event EventHandler<DataEventArgs<DocumentPackage>> LoadPackDetails2;
        event EventHandler<EventArgs> MoveSelected;
        event EventHandler<EventArgs> MoveRetail;
        event EventHandler<EventArgs> UnPickSelected;
    }
}
