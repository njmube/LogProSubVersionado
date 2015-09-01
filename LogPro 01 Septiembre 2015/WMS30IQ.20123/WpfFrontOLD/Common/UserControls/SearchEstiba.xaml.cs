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
    /// Interaction logic for PopUpDocumentDetail.xaml
    /// </summary>
    public partial class SearchEstiba : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadRecord;

        public SearchEstiba()
        {
            InitializeComponent();
            DataContext = this;
        }

        //Image envent    
        protected void imgLoad_FocusHandler(object sender, EventArgs e)
        {
            EventHandler temp = OnLoadRecord;
            if (temp != null)
                temp(sender, e);
        }


        private void txtData_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtData.Text))
                return;

            this.Product = null;
            this.ProductDesc = null;
            //Search for a Records

            if (this.DefaultList != null)
            {
                DataList = DefaultList;
                if (DefaultList.Where(f => f.ProductCode.ToUpper().StartsWith(txtData.Text.ToUpper())).Count() == 1)
                    DataList = DefaultList.Where(f => f.ProductCode.ToUpper().StartsWith(txtData.Text.ToUpper())).ToList();
            }
            else
                DataList = (new WMSServiceClient()).SearchProduct(txtData.Text, App.curLocation.LocationID.ToString());

            if (DataList == null || DataList.Count == 0)
                return;

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;

            if (DataList.Count == 1)
                FireEvent(sender, e);
        }


        private void FireEvent(object sender, EventArgs e)
        {
                this.cboData.Visibility = Visibility.Collapsed;
                //txtData.Text = DataList[0].ProductCode;
                this.Product = DataList[0];
                this.ProductDesc = Product.FullDesc;


                EventHandler temp = OnLoadRecord;
                if (temp != null)
                    temp(sender, e);

        }


        private void cboData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Product data = ((ComboBox)sender).SelectedItem as Product;

            if (data == null)
                return;

            txtData.Text = data.ProductCode;
            this.Product = data;
            this.ProductDesc = Product.FullDesc;
            cboData.Visibility = Visibility.Collapsed;

            //imgLoad.Focus();

            EventHandler temp = OnLoadRecord;
            if (temp != null)
                temp(sender, e);

        }


        //Text Property
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(SearchEstiba));

        public String Text
        {
            get { return this.txtData.Text; } //(String)GetValue(TextProperty);
            set
            {
                this.txtData.Text = value;
                //SetValue(TextProperty, value);
            }
        }

        private IList<Product> _DataList;
        public IList<Product> DataList
        {
            get { return _DataList; }
            set
            {
                _DataList = value;
                OnPropertyChanged("DataList");
            }
        }


        public static DependencyProperty ProductProperty =
    DependencyProperty.Register("Product", typeof(Product), typeof(SearchEstiba));

        public Product Product
        {
            get { return (Product)GetValue(ProductProperty); }
            set
            {
                SetValue(ProductProperty, value);
                OnPropertyChanged("Product");
            }
        }


        public static DependencyProperty ProductDescProperty = DependencyProperty.Register("ProductDesc", typeof(String), typeof(SearchEstiba));

        public String ProductDesc
        {
            get { return (String)GetValue(ProductDescProperty); }
            set
            {
                this.txtProductDesc.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
                SetValue(ProductDescProperty, value);
            }
        }


        //Product List - Predefinend cuando solo pude cargar los productos de esta lista.
        public static DependencyProperty DefaultListProperty = DependencyProperty.Register("DefaultList", typeof(IList<Product>), typeof(SearchEstiba));

        public IList<Product> DefaultList
        {
            get { return (IList<Product>)GetValue(DefaultListProperty); }
            set
            {
                SetValue(DefaultListProperty, value);
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
            if (this.DefaultList != null)
                DataList = DefaultList;
            else
                DataList = (new WMSServiceClient()).GetProductApp(new Product { Reference = App.curLocation.LocationID.ToString() }, WmsSetupValues.NumRegs);

            if (DataList == null || DataList.Count == 0)
            {
                this.ProductDesc = "No products to show.";
                return;
            }

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;
            
            if (DataList.Count == 1)
                FireEvent(sender, e);

        }

        private void SearchEstibaName_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DefaultList != null && this.DefaultList.Count == 1)
            {
                DataList = DefaultList;
                FireEvent(sender, e);
            }
        }






    }




}
