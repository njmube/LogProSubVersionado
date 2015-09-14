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
    public partial class BinRange : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadRange;

        public BinRange()
        {
            InitializeComponent();
            DataContext = this;
        }

        //Image envent    
        protected void imgLoad_FocusHandler(object sender, EventArgs e)
        {
            EventHandler temp = OnLoadRange;
            if (temp != null)
                temp(sender, e);           
        }


        //Text Property
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(BinRange));

        public String Text
        {
            get {
                if (string.IsNullOrEmpty(txtBin1.Text.Trim()) && string.IsNullOrEmpty(txtBin2.Text.Trim()))
                    return "";
                else
                    return this.txtBin1.Text.Trim() + ":" + this.txtBin2.Text.Trim(); 
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
