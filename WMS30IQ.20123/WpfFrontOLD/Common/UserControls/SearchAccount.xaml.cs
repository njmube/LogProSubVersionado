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

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class SearchAccount : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnSelected;

        public SearchAccount()
        {
            InitializeComponent();
            DataContext = this;
        }

        //Image envent    
        protected void imgLoad_FocusHandler(object sender, EventArgs e)
        {
            EventHandler temp = OnSelected;
            if (temp != null)
                temp(sender, e);
        }


        private void txtData_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtData.Text))
                return;


            this.Account = null;

            LoadDataList();

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;

            if (DataList.Count == 1)
            {
                this.cboData.Visibility = Visibility.Collapsed;
                txtData.Text = DataList[0].FullDesc;
                this.Account = DataList[0];
                imgLoad.Focus();
                //imgLoad.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, imgLoad));
            }
        }



        private void LoadDataList()
        {
            if (this.DefaultList != null && this.DefaultList.Count > 0)
            {
                DataList = DefaultList;
                if (DefaultList.Where(f => f.Name.ToUpper().Contains(txtData.Text.ToUpper())).Count() == 1)
                    DataList = DefaultList.Where(f => f.Name.ToUpper().Contains(txtData.Text.ToUpper())).ToList();

                return;
            }
            

            //Search for a Records
            if (this.AccType == AccntType.Vendor)
                DataList = (new WMSServiceClient()).SearchVendor(txtData.Text);
            else
                DataList = (new WMSServiceClient()).SearchCustomer(txtData.Text);

            if (DataList == null || DataList.Count == 0)
                return;
        }


        private void cboData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account data = ((ComboBox)sender).SelectedItem as Account;

            if (data == null)
                return;

            txtData.Text = data.FullDesc;
            this.Account = data;
            cboData.Visibility = Visibility.Collapsed;
            imgLoad.Focus();
        }



        //Text Property
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), 
            typeof(SearchAccount));

        public String Text
        {
            get { return this.txtData.Text; } //(String)GetValue(TextProperty);
            set { this.txtData.Text = value;}
        }

        private IList<Account> _DataList;
        public IList<Account> DataList
        {
            get { return _DataList; }
            set
            {
                _DataList = value;
                OnPropertyChanged("DataList");
            }
        }


        private IList<Account> _DefList;
        public IList<Account> DefaultList
        {
            get { return _DefList; }
            set { _DefList = value; }
        }



        public static DependencyProperty AccountProperty = DependencyProperty.Register("Account", typeof(Account), typeof(SearchAccount));

        public Account Account
        {
            get { return (Account)GetValue(AccountProperty); }
            set
            {
                SetValue(AccountProperty, value);
            }
        }


        public static DependencyProperty AccTypeProperty = DependencyProperty.Register("AccType", typeof(Int16), typeof(SearchAccount));

        public Int16 AccType
        {
            get { return (Int16)GetValue(AccTypeProperty); }
            set
            {
                SetValue(AccTypeProperty, value);
            }
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

        private void imgLook_Click(object sender, RoutedEventArgs e)
        {
            //Search for a Records
            LoadDataList();

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;

            if (DataList.Count == 1)
            {
                this.cboData.Visibility = Visibility.Collapsed;
                txtData.Text = DataList[0].FullDesc;
                this.Account = DataList[0];
                imgLoad.Focus();
                //imgLoad.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, imgLoad));
            }
        }
              

    }
}
