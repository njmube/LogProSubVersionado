using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for CustomProcessView.xaml
    /// </summary>
    public partial class CustomProcessView : UserControlBase, ICustomProcessView
    {
        public CustomProcessView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<CustomProcess>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

         public CustomProcessModel Model
        {
            get
            { return this.DataContext as CustomProcessModel; }
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
            LoadData(sender, new DataEventArgs<CustomProcess>((CustomProcess)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        #endregion





    }



    public interface ICustomProcessView
    {
        //Clase Modelo
        CustomProcessModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<CustomProcess>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
    }
}