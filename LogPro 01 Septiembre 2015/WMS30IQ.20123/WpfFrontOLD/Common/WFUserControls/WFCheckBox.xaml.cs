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
    public partial class WFCheckBox : UserControl, INotifyPropertyChanged
    {
        public WFCheckBox()
        {
            InitializeComponent();
            DataContext = this;
        }


        public static DependencyProperty UcLabelProperty = DependencyProperty.Register("UcLabel", typeof(String), typeof(WFCheckBox));

        public String UcLabel
        {
            get { return (String)GetValue(UcLabelProperty); }
            set
            {
                SetValue(UcLabelProperty, value);
                OnPropertyChanged("UcLabel");
            }
        }


        public static DependencyProperty UcValueProperty = DependencyProperty.Register("UcValue", typeof(Boolean), typeof(WFCheckBox));

        public Boolean UcValue
        {
            get { return (Boolean)GetValue(UcValueProperty); }
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
