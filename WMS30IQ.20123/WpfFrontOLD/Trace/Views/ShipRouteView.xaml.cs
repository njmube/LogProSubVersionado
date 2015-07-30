using System;
using System.Collections.Generic;
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
    /// Interaction logic for ShipRouteView.xaml
    /// </summary>
    public partial class ShipRouteView : UserControlBase, IShipRouteView
    {

        //Listview Sort
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        
        public ShipRouteView()
        {
            InitializeComponent();
        }


        public event EventHandler<EventArgs> LoadDocuments;
        public event EventHandler<EventArgs> ProcessLines;
        public event EventHandler<DataEventArgs<Document>> LoadOpenProcess;
        public event EventHandler<DataEventArgs<Document>> ShowTicket;
        public event EventHandler<DataEventArgs<String>> UpdateDriver;
        public event EventHandler<EventArgs> CreateShipment;
        public event EventHandler<EventArgs>  ShowShipTkt;



        #region Properties


        public ComboBox CboLocation
        {
            get
            { return cboLocation; }
            set
            { cboLocation = value; }
        }



        public ShipRouteModel Model
        {
            get
            { return this.DataContext as ShipRouteModel; }
            set
            { this.DataContext = value; }
        }



        public ListView ListViewDocuments
        {
            get { return this.lvDocuments; }
            set { this.lvDocuments = value; }
        }


        public TextBox DocNumberSearch
        {
            get { return this.txtSearchDoc; }
            set { this.txtSearchDoc = value; }
        }

        public ComboBox CboDate
        {
            get { return this.cboDate; }
            set { this.cboDate = value; }
        }

        public ComboBox CboRoute
        {
            get { return this.cboRoute; }
            set { this.cboRoute = value; }
        }


        public StackPanel StkMain
        {
            get { return this.stkMain; }
            set { this.stkMain = value; }
        }

        public AutoComplete UcDriver
        {
            get { return this.ucDriver; }
            set { this.ucDriver = value; }
        }

        public Button BtnCreateShipment
        {
            get { return this.btnShipment; }
            set { this.btnShipment = value; }
        }

        public ComboBox CboProcessRoute
        {
            get { return this.cboProcessRoute; }
            set { this.cboProcessRoute = value; }
        }


        public Border BrdDocuments
        {
            get { return this.brdDocs; }
            set { this.brdDocs = value; }
        }


        public StackPanel StkCreateP
        {
            get { return this.stkCreate; }
            set { this.stkCreate = value; }
        }


        public Button BtnRemoveL
        {
            get { return this.btnRemove; }
            set { this.btnRemove = value; }
        }


        #endregion


        #region Events



        private void cboDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Ejecutar Busqueda
            cboCurOpen.SelectedIndex = -1;
            LoadDocuments(sender, e);
        }

        #endregion



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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvDocuments.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void lvDocuments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            popup1.IsOpen = true;
            popup1.StaysOpen = true;
        }



        private void btn_Process_Click(object sender, RoutedEventArgs e)
        {
            if (cboProcessRoute.SelectedItem == null)
            {
                Util.ShowError("Por favor seleccione una ruta para el documento.");
                return;
            }

            //Procesa la lineas seleccionadas
            ProcessLines(sender, e);

        }


        private void cboCurOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCurOpen.SelectedItem == null)
                return;

            //Selecionar una planilla Disponible
            LoadOpenProcess(sender, new DataEventArgs<Document>((Document)cboCurOpen.SelectedItem));
        }

        private void cboDriver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnPktTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowTicket(sender, new DataEventArgs<Document>((Document)this.Model.CurDoc));
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Actualizar Driver
            UpdateDriver(sender, new DataEventArgs<String>((String)ucDriver.txtData.Text));
        }

        private void btnShipment_Click(object sender, RoutedEventArgs e)
        {
            CreateShipment(sender, e);
        }

        private void btnTktShip_Click(object sender, RoutedEventArgs e)
        {

            ShowShipTkt(sender, e);
        }

        private void cboLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



    }

    public interface IShipRouteView
    {
        //Variable Modelo
        ShipRouteModel Model { get; set; }

        //Variables
        ListView ListViewDocuments { get; set; }
        TextBox DocNumberSearch { get; set; }
        ComboBox CboRoute { get; set; }
        ComboBox CboDate { get; set; }
        StackPanel StkMain { get; set; }
        AutoComplete UcDriver { get; set; }
        Button BtnCreateShipment { get; set; }
        ComboBox CboProcessRoute { get; set; }
         Border BrdDocuments{ get; set; }
         StackPanel StkCreateP{ get; set; }
         Button BtnRemoveL { get; set; }
         ComboBox CboLocation { get; set; }


        //Events
        event EventHandler<EventArgs> LoadDocuments;
        event EventHandler<EventArgs> ProcessLines;
        event EventHandler<DataEventArgs<Document>> LoadOpenProcess;
        event EventHandler<DataEventArgs<Document>> ShowTicket;
        event EventHandler<DataEventArgs<String>> UpdateDriver;
        event EventHandler<EventArgs> CreateShipment;
        event EventHandler<EventArgs> ShowShipTkt;


    }
}
