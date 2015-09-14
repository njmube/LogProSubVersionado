﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using WpfFront.WMSBusinessService;


namespace WpfFront.Common.WFUserControls
{
    /// <summary>
    /// Interaction logic for StringText.xaml
    /// </summary>
    public partial class WFComboBox : UserControl, INotifyPropertyChanged
    {
        public WFComboBox()
        {
            InitializeComponent();
            DataContext = this;
        }


        public static DependencyProperty UcLabelProperty = DependencyProperty.Register("UcLabel", typeof(String), typeof(WFComboBox));

        public String UcLabel
        {
            get { return (String)GetValue(UcLabelProperty); }
            set
            {
                SetValue(UcLabelProperty, value);
                OnPropertyChanged("UcLabel");
            }
        }


        public static DependencyProperty UcValueProperty = DependencyProperty.Register("UcValue", typeof(String), typeof(WFComboBox));

        public String UcValue
        {
            get { return (String)GetValue(UcValueProperty); }
            set
            {
                SetValue(UcValueProperty, value);
                OnPropertyChanged("UcValue");
            }
        }


        private IList<MMaster> _UcList;
        public IList<MMaster> UcList
        {
            get { return _UcList; }
            set
            {
                _UcList = value;
                OnPropertyChanged("UcList");
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
