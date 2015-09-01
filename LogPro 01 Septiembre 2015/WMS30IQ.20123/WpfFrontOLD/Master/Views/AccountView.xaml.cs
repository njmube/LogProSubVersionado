using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WMComposite.Events;
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for AccountView.xaml
    /// </summary>
    public partial class AccountView : UserControlBase, IAccountView
    {
        public AccountView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Account>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

        //Admin Account Address
        //public event EventHandler<EventArgs> SaveAccountAddress;
        //public event EventHandler<EventArgs> DeleteAccountAddress;
        //public event EventHandler<DataEventArgs<AccountAddress>> LoadDataAccountAddress;
        //public event EventHandler<EventArgs> NewAccountAddress;

         public AccountModel Model
        {
            get
            { return this.DataContext as AccountModel; }
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


        /*
        public ListView ListViewAccountAddress
        {
            get { return this.lvAccountAddress; }
            set { this.lvAccountAddress = value; }
        }

        public TextBox Nombre
        {
            get { return this.txt_nombre; }
            set { this.txt_nombre = value; }
        }

        public TextBox Direccion
        {
            get { return this.txt_direccion; }
            set { this.txt_direccion = value; }
        }

        public TextBox Direccion2
        {
            get { return this.txt_direccion2; }
            set { this.txt_direccion2 = value; }
        }

        public TextBox Direccion3
        {
            get { return this.txt_direccion3; }
            set { this.txt_direccion3 = value; }
        }

        public TextBox Ciudad
        {
            get { return this.txt_ciudad; }
            set { this.txt_ciudad = value; }
        }

        public TextBox Estado
        {
            get { return this.txt_estado; }
            set { this.txt_estado = value; }
        }

        public TextBox CodigoPostal
        {
            get { return this.txt_codigopostal; }
            set { this.txt_codigopostal = value; }
        }

        public TextBox Pais
        {
            get { return this.txt_pais; }
            set { this.txt_pais = value; }
        }

        public TextBox PersonaContacto
        {
            get { return this.txt_personacontacto; }
            set { this.txt_personacontacto = value; }
        }

        public TextBox Telefono
        {
            get { return this.txt_telefono; }
            set { this.txt_telefono = value; }
        }

        public TextBox Telefono2
        {
            get { return this.txt_telefono2; }
            set { this.txt_telefono2 = value; }
        }

        public TextBox Telefono3
        {
            get { return this.txt_telefono3; }
            set { this.txt_telefono3 = value; }
        }
        */


        public UcAccountAddress UCAddress
        {
            get { return this.ucAddress; }
            set { this.ucAddress = value; }
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
            LoadData(sender, new DataEventArgs<Account>((Account)dgList.SelectedItem));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        //private void btnSaveAddress_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveAccountAddress(sender, e);
        //}

        //private void btnDeleteAccountAddress_Click(object sender, RoutedEventArgs e)
        //{
        //    DeleteAccountAddress(sender, e);
        //}

        //private void lvAccountAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LoadDataAccountAddress(sender, new DataEventArgs<AccountAddress>((AccountAddress)lvAccountAddress.SelectedItem));
        //}

        //private void btnNewAddress_Click(object sender, RoutedEventArgs e)
        //{
        //    NewAccountAddress(sender, e);
        //}

        #endregion
    }



    public interface IAccountView
    {
        //Clase Modelo
        AccountModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        UcAccountAddress UCAddress { get; set; }

        //TextBox Nombre { get; set; }
        //TextBox Direccion { get; set; }
        //TextBox Direccion2 { get; set; }
        //TextBox Direccion3 { get; set; }
        //TextBox Ciudad { get; set; }
        //TextBox Estado { get; set; }
        //TextBox CodigoPostal { get; set; }
        //TextBox Pais { get; set; }
        //TextBox PersonaContacto { get; set; }
        //TextBox Telefono { get; set; }
        //TextBox Telefono2 { get; set; }
        //TextBox Telefono3 { get; set; }

        //ListView ListViewAccountAddress { get; set; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Account>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        //event EventHandler<EventArgs> SaveAccountAddress;
        //event EventHandler<EventArgs> DeleteAccountAddress;
        //event EventHandler<DataEventArgs<AccountAddress>> LoadDataAccountAddress;
        //event EventHandler<EventArgs> NewAccountAddress;
    }
}