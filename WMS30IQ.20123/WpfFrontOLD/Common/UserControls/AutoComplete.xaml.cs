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
    public partial class AutoComplete : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadRecord;

        public AutoComplete()
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

            if (this.DefaultList != null)
            {
                DataList = DefaultList;

                if (DefaultList.Where(f => f.DataKey.ToUpper().StartsWith(txtData.Text.ToUpper())).Count() > 0)
                    DataList = DefaultList.Where(f => f.DataKey.ToUpper().StartsWith(txtData.Text.ToUpper())).ToList();

            }


            if (DataList == null || DataList.Count == 0)
                return;

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            //this.cboData.IsDropDownOpen = true;

            if (DataList.Count == 1)
                FireEvent(sender, e);
        }


        private void FireEvent(object sender, EventArgs e)
        {
            this.cboData.Visibility = Visibility.Collapsed;

            EventHandler temp = OnLoadRecord;
            if (temp != null)
                temp(sender, e);

        }


        private void cboData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowData data = ((ComboBox)sender).SelectedItem as ShowData;

            if (data == null)
                return;

            txtData.Text = data.DataKey;
            cboData.Visibility = Visibility.Collapsed;

            //imgLoad.Focus();

            EventHandler temp = OnLoadRecord;
            if (temp != null)
                temp(sender, e);

        }


        //Text Property
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(AutoComplete));

        public String Text
        {
            get { return this.txtData.Text; } 
            set
            {
                this.txtData.Text = value;
            }
        }

        private IList<ShowData> _DataList;
        public IList<ShowData> DataList
        {
            get { return _DataList; }
            set
            {
                _DataList = value;
                OnPropertyChanged("DataList");
            }
        }


        //Product List - Predefinend cuando solo pude cargar los productos de esta lista.
        public static DependencyProperty DefaultListProperty = DependencyProperty.Register("DefaultList", typeof(IList<ShowData>), typeof(AutoComplete));

        public IList<ShowData> DefaultList
        {
            get { return (IList<ShowData>)GetValue(DefaultListProperty); }
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
            DataList = DefaultList;

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;
            
            if (DataList.Count == 1)
                FireEvent(sender, e);

        }



    }




}
