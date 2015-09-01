using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.IO;
using Microsoft.Win32;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for CompanyView.xaml
    /// </summary>
    public partial class CompanyView : UserControlBase, ICompanyView
    {
        public CompanyView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Company>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<DataEventArgs<Stream>> SetLogo;
        public event EventHandler<EventArgs> ViewConnections;

         public CompanyModel Model
        {
            get
            { return this.DataContext as CompanyModel; }
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

        public ComboBox CboConnection
        { get { return this.cboConnection; }
            set { this.cboConnection = value; }
        }

        public ComboBox CboStatus
        { get { return this.cboStatus; } }

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
            LoadData(sender, new DataEventArgs<Company>((Company)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }



        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog open = new OpenFileDialog();
        //    open.Multiselect = false;
        //    open.Filter = "AllFiles|*.*";

        //    if ((bool)open.ShowDialog())
        //    {
        //        SetLogo(sender, new DataEventArgs<Stream>(open.OpenFile()));
        //        txtLogo.Text = open.FileName;

        //    }
        //}


        #endregion

        private void btnCnn_Click(object sender, RoutedEventArgs e)
        {
            ViewConnections(sender, e);
        }


        private void fUpload_OnFileUpload(object sender, EventArgs e)
        {

            if (fUpload.StreamFile != null)
                SetLogo(sender, new DataEventArgs<Stream>(fUpload.StreamFile));
                            
        }

    }



    public interface ICompanyView
    {
        //Clase Modelo
        CompanyModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        ComboBox CboConnection { get; set; }
        ComboBox CboStatus { get; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Company>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> ViewConnections;
        event EventHandler<DataEventArgs<Stream>> SetLogo;
    }
}