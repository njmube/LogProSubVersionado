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
    public partial class SearchC_CasNumber : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadRecord;

        public SearchC_CasNumber()
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

            this.C_CasNumber = null;
            this.C_CasNumberDesc = null;
            //Search for a Records

            if (this.DefaultList != null)
            {
                DataList = DefaultList;
                if (DefaultList.Where(f => f.Code.ToUpper().StartsWith(txtData.Text.ToUpper())).Count() == 1)
                    DataList = DefaultList.Where(f => f.Code.ToUpper().StartsWith(txtData.Text.ToUpper())).ToList();
            }
            else
                DataList = (new WMSServiceClient()).GetC_CasNumber(new C_CasNumber { Name = txtData.Text });

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
                //txtData.Text = DataList[0].C_CasNumberCode;
                this.C_CasNumber = DataList[0];
                this.C_CasNumberDesc = C_CasNumber.Name;


                EventHandler temp = OnLoadRecord;
                if (temp != null)
                    temp(sender, e);

        }


        private void cboData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C_CasNumber data = ((ComboBox)sender).SelectedItem as C_CasNumber;

            if (data == null)
                return;

            txtData.Text = data.Code;
            this.C_CasNumber = data;
            this.C_CasNumberDesc = C_CasNumber.Name;
            cboData.Visibility = Visibility.Collapsed;

            //imgLoad.Focus();

            EventHandler temp = OnLoadRecord;
            if (temp != null)
                temp(sender, e);

        }


        //Text Property
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(SearchC_CasNumber));

        public String Text
        {
            get { return this.txtData.Text; } //(String)GetValue(TextProperty);
            set
            {
                this.txtData.Text = value;
                //SetValue(TextProperty, value);
            }
        }

        private IList<C_CasNumber> _DataList;
        public IList<C_CasNumber> DataList
        {
            get { return _DataList; }
            set
            {
                _DataList = value;
                OnPropertyChanged("DataList");
            }
        }


        public static DependencyProperty C_CasNumberProperty =
    DependencyProperty.Register("C_CasNumber", typeof(C_CasNumber), typeof(SearchC_CasNumber));

        public C_CasNumber C_CasNumber
        {
            get { return (C_CasNumber)GetValue(C_CasNumberProperty); }
            set
            {
                SetValue(C_CasNumberProperty, value);
                OnPropertyChanged("C_CasNumber");
            }
        }


        public static DependencyProperty C_CasNumberDescProperty = DependencyProperty.Register("C_CasNumberDesc", typeof(String), typeof(SearchC_CasNumber));

        public String C_CasNumberDesc
        {
            get { return (String)GetValue(C_CasNumberDescProperty); }
            set
            {
                this.txtC_CasNumberDesc.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
                SetValue(C_CasNumberDescProperty, value);
            }
        }


        //C_CasNumber List - Predefinend cuando solo pude cargar los C_CasNumberos de esta lista.
        public static DependencyProperty DefaultListProperty = DependencyProperty.Register("DefaultList", typeof(IList<C_CasNumber>), typeof(SearchC_CasNumber));

        public IList<C_CasNumber> DefaultList
        {
            get { return (IList<C_CasNumber>)GetValue(DefaultListProperty); }
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
                DataList = (new WMSServiceClient()).GetC_CasNumber(new C_CasNumber());

            if (DataList == null || DataList.Count == 0)
            {
                this.C_CasNumberDesc = "No C_CasNumbers to show.";
                return;
            }

            //Cargar la lista de Records
            this.cboData.Visibility = Visibility.Visible;
            this.cboData.IsDropDownOpen = true;
            
            if (DataList.Count == 1)
                FireEvent(sender, e);

        }

        private void SearchC_CasNumberName_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DefaultList != null && this.DefaultList.Count == 1)
            {
                DataList = DefaultList;
                FireEvent(sender, e);
            }
        }






    }




}
