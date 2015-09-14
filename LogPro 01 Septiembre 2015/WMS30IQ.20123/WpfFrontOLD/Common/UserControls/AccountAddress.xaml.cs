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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfFront.WMSBusinessService;
using Core.BusinessEntity;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using WpfFront.Services;

/*
 * Creado por: Jorge Armando Ortega
 * Fecha: Octubre 21 / 2010
 */

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class UcAccountAddress : UserControl, INotifyPropertyChanged
    {

        public UcAccountAddress()
        {
            InitializeComponent();
            DataContext = this;

        }


        #region Model

        private WMSServiceClient _service;
        public WMSServiceClient service
        {
            get
            {

                if (_service == null)
                    return new WMSServiceClient();
                else
                    return _service;
            }
        }

        private AccountAddress _Record;
        public AccountAddress Record
        {
            get { return _Record; }
            set
            {
                _Record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<AccountAddress> _RecordList;
        public IList<AccountAddress> RecordList
        {
            get { return _RecordList; }
            set
            {
                _RecordList = value;
                OnPropertyChanged("RecordList");
            }
        }


        public static DependencyProperty AccountProperty = DependencyProperty.Register("Account", typeof(Account), typeof(UcAccountAddress));

        public Account Account
        {
            get { return (Account)GetValue(AccountProperty); }
            set
            {
                SetValue(AccountProperty, value);
            }
        }



        #endregion



        internal void LoadAddresses(Account account)
        {
            Account = account;
            Record = new AccountAddress();
            RecordList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = account.AccountID } });
        }


        private void btnSaveAddress_Click(object sender, RoutedEventArgs e)
        {
            //Validacion para los campos obligatorios del formulario
            if (string.IsNullOrEmpty(Record.Name))
            {
                Util.ShowError("Please enter an Address Name");
                return;
            }

            if (string.IsNullOrEmpty(Record.AddressLine1))
            {
                Util.ShowError("Please enter an Address Line1");
                return;
            }

            if (string.IsNullOrEmpty(Record.State))
            {
                Util.ShowError("Please enter a State");
                return;
            }

            if (string.IsNullOrEmpty(Record.City))
            {
                Util.ShowError("Please enter a City");
                return;
            }

            if (string.IsNullOrEmpty(Record.Country))
            {
                Util.ShowError("Please enter a Country");
                return;
            }

            Record.Account = Account;
            Record.ErpCode = Record.Name;
            Record.IsMain = false;
            Record.IsFromErp = false;
            Record.Status = new Status { StatusID = EntityStatus.Active };
            Record.CreatedBy = App.curUser.UserName;
            Record.CreationDate = DateTime.Now;

            try
            {
                if (Record.AddressID == 0)
                    Record = service.SaveAccountAddress(Record);
                else
                    service.UpdateAccountAddress(Record);

                RecordList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = Account.AccountID } });
                LimpiarCampos();
                Util.ShowMessage("Address Updated.");

            }
            catch (Exception ex) {
                Util.ShowError("Error: " + ex.Message);
            }

        }

        private void btnDeleteAccountAddress_Click(object sender, RoutedEventArgs e)
        {
            if (RecordList.Count <= 0)
                return;

            foreach (AccountAddress aa in lvAccountAddress.SelectedItems)
            {
                service.DeleteAccountAddress(aa);
            }

            RecordList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = Account.AccountID } });

            //Limpio los datos
            LimpiarCampos();
        }

        private void lvAccountAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            Record = e.AddedItems[0] as AccountAddress;
        }

        private void btnNewAddress_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            Record = new AccountAddress();
            //View.Nombre.Text = View.Direccion.Text = View.Direccion2.Text = View.Direccion3.Text = "";
            //View.Ciudad.Text = View.Estado.Text = View.CodigoPostal.Text = View.Pais.Text = "";
            //View.PersonaContacto.Text = View.Telefono.Text = View.Telefono2.Text = View.Telefono3.Text = "";
        }








        #region INotifyPropertyChanged Members

        private event PropertyChangedEventHandler propertyChangedEvent;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChangedEvent += value; }
            remove { propertyChangedEvent -= value; }
        }

        protected void OnPropertyChanged(string prop)
        {
            if (propertyChangedEvent != null)
                propertyChangedEvent(this, new PropertyChangedEventArgs(prop));
        }

        #endregion



    }
}
