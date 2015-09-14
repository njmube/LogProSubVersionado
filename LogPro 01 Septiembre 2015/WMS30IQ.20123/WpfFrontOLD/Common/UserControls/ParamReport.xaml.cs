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
    public partial class ParamReport : UserControl, INotifyPropertyChanged
    {

        //public event EventHandler OnLoadLocation;

        public ParamReport()
        {
            InitializeComponent();
            DataContext = this;
        }



        private void cboParam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowData param = ((ComboBox)sender).SelectedItem as ShowData;

            if (param == null)
                return;

            txtParam.Text = param.DataValue;
            cboParam.Visibility = Visibility.Collapsed;
        }


        private void txtParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParamList == null || ParamList.Count == 0)
                return;

            //Cargar la lista de Bins
            this.cboParam.Visibility = Visibility.Visible;
            this.cboParam.IsDropDownOpen = true;

            if (ParamList.Count == 1)
            {
                this.cboParam.Visibility = Visibility.Collapsed;
                txtParam.Text = ParamList[0].DataValue;
            }
        }


        ////Text Property
        //public static DependencyProperty TextProperty =
        //    DependencyProperty.Register("Text", typeof(String), typeof(BinLocation));

        //public String Text
        //{
        //    get { return this.txtParam.Text; } //(String)GetValue(TextProperty);
        //    set
        //    {
        //        this.txtParam.Text = value;
        //        //SetValue(TextProperty, value);
        //    }
        //}

        private IList<ShowData> _ParamList;
        public IList<ShowData> ParamList
        {
            get { return _ParamList; }
            set
            {
                _ParamList = value;
                OnPropertyChanged("ParamList");
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
