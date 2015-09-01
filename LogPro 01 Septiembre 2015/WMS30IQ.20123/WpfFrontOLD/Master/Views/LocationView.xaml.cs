using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for LocationView.xaml
    /// </summary>
    public partial class LocationView : UserControlBase, ILocationView
    {
        public LocationView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Location>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

         public LocationModel Model
        {
            get
            { return this.DataContext as LocationModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

         public DataGridControl ListRecords 
            { get { return this.dgList; } }

        public StackPanel StkEdit 
            { get { return this.stkEdit; } }

        public Button BtnDelete
            { get { return this.btnDelete; } }

        public ComboBox ListStatus
        {
            get { return this.ls_Status; }
            set { this.ls_Status = value; }
        }

        #endregion




        #region ViewEvents

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSearch(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
        }


        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<Location>((Location)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ListStatus.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar un estado para la bodega");
                return;
            }
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        #endregion





    }



    public interface ILocationView
    {
        //Clase Modelo
        LocationModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        ComboBox ListStatus { get; set; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Location>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
    }
}