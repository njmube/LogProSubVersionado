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
    public partial class BinLocation : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadLocation;

        public BinLocation()
        {
            InitializeComponent();
            DataContext = this;
        }

        //Image envent    
        protected void imgLoad_FocusHandler(object sender, EventArgs e)
        {
            EventHandler temp = OnLoadLocation;
            if (temp != null)
                temp(sender, e);
        }



        private void cboBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bin bin = ((ComboBox)sender).SelectedItem as Bin;

            if (bin == null)
                return;

            txtBin.Text = bin.BinCode;
            this.Bin = bin;
            cboBin.Visibility = Visibility.Collapsed;
            imgLoad.Focus();

        }


        private void txtBinLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBin.Text))
            {
                this.cboBin.IsDropDownOpen = false;
                return;
            }

            //Search for a Bin            
            BinList = (new WMSServiceClient()).SearchBin(txtBin.Text);

            if (BinList == null || BinList.Count == 0)
                return;

            //Cargar la lista de Bins
            this.cboBin.Visibility = Visibility.Visible;
            this.cboBin.IsDropDownOpen = true;

            if (BinList.Count == 1)
            {
                this.cboBin.Visibility = Visibility.Collapsed;
                txtBin.Text = BinList[0].BinCode;
                this.Bin = BinList[0];
                imgLoad.Focus();
                //imgLoad.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, imgLoad));
            }
        }



        //Text Property
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(BinLocation));

        public String Text
        {
            get { return this.txtBin.Text; } //(String)GetValue(TextProperty);
            set
            {
                this.txtBin.Text = value;
                //SetValue(TextProperty, value);
            }
        }

        private IList<Bin> _BinList;
        public IList<Bin> BinList {
            get { return _BinList; }
            set
            {
                _BinList = value;
                OnPropertyChanged("BinList");
            }
        }




        public static DependencyProperty BinProperty =
     DependencyProperty.Register("Bin", typeof(Bin), typeof(BinLocation));

        public Bin Bin
        {
            get { return (Bin)GetValue(BinProperty); }
            set
            {
                SetValue(BinProperty, value);
            }
        }


        //Product
        public static DependencyProperty ProductProperty =
     DependencyProperty.Register("Product", typeof(Product), typeof(BinLocation));

        public Product Product
        {
            get { return (Product)GetValue(ProductProperty); }
            set
            {
                SetValue(ProductProperty, value);
            }
        }




        private void imgLook_Click(object sender, RoutedEventArgs e)
        {
            //Search for a Bin
            if (this.Product != null)
            {
                ProductStock ps = new ProductStock { Product = this.Product, Bin = new Bin { Location = App.curLocation } };
                try { BinList = (new WMSServiceClient()).GetBinStock(ps).Select(f=>f.Bin).ToList(); }
                catch { }
            }
            else
                BinList = (new WMSServiceClient()).GetBin(new Bin { Location = App.curLocation });


            if (BinList == null || BinList.Count == 0)
            {
                Util.ShowError("No Bins defined for location: " + App.curLocation.Name);
                return;
            }

            //Cargar la lista de Bins
            this.cboBin.Visibility = Visibility.Visible;
            this.cboBin.IsDropDownOpen = true;

            if (BinList.Count == 1)
            {
                this.cboBin.Visibility = Visibility.Collapsed;
                txtBin.Text = BinList[0].BinCode;
                this.Bin = BinList[0];
                imgLoad.Focus();
                //imgLoad.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, imgLoad));
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





    }




}
