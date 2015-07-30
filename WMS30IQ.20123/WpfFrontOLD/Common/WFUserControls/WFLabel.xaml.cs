using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;


namespace WpfFront.Common.WFUserControls
{
    /// <summary>
    /// Interaction logic for StringText.xaml
    /// </summary>
    public partial class WFLabel : UserControl, INotifyPropertyChanged
    {
        public WFLabel()
        {
            InitializeComponent();
            DataContext = this;
        }


        public static DependencyProperty UcLabelProperty = DependencyProperty.Register("UcLabel", typeof(String), typeof(WFLabel));

        public String UcLabel
        {
            get { return (String)GetValue(UcLabelProperty); }
            set
            {
                SetValue(UcLabelProperty, value);
                OnPropertyChanged("UcLabel");
            }
        }


        public static DependencyProperty UcValueProperty = DependencyProperty.Register("UcValue", typeof(String), typeof(WFLabel));

        public String UcValue
        {
            get { return (String)GetValue(UcValueProperty); }
            set
            {
                SetValue(UcValueProperty, value);
                OnPropertyChanged("UcValue");
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
