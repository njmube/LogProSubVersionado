using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Core.WPF;
using WMComposite.Events;
using WpfFront.WMSBusinessService;
using WpfFront.Models;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ConnectionView.xaml
    /// </summary>
    public partial class ConnectionView : UserControlBase, IConnectionView
    {
        public ConnectionView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<ConnectionType>> LoadChilds;
        public event EventHandler<DataEventArgs<Connection>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> TestConnection;

        public ConnectionModel Model
        {
            get
            { return this.DataContext as ConnectionModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

        public ListView ListTypes
        { get { return this.dgList; } }

        public ListView ListDataSource
        { get { return this.dgListDataSource; } }

        public StackPanel StkEdit
        { get { return this.stkEdit; } }

        public Button BtnDelete
        { get { return this.btnDelete; } }

        public DockPanel DpChilds
        {
            get { return this.dpChilds; }
            set { this.dpChilds = value; }
        }


        #endregion




        #region ViewEvents


        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
            dsName.Focus();
        }


        private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadChilds(sender, new DataEventArgs<ConnectionType>((ConnectionType)dgList.SelectedItem));
        }


        private void dgListDataSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<Connection>((Connection)dgListDataSource.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        private void btnTestCnn_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(sender, e);
        }


        #endregion






    }



    public interface IConnectionView
    {
        //Clase Modelo
        ConnectionModel Model { get; set; }

        ListView ListTypes { get; }
        ListView ListDataSource { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        DockPanel DpChilds { get; set; }


        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<ConnectionType>> LoadChilds;
        event EventHandler<DataEventArgs<Connection>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> TestConnection;
    }
}
