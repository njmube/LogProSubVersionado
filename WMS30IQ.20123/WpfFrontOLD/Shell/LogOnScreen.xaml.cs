/*
 * LogOnScreen.cs    6/13/2008 6:06:24 PM
 *
 * Copyright 2008  All rights reserved.
 * Use is subject to license terms
 *
 * Author: bryan
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.DirectoryServices;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using WpfFront.Services;


namespace WpfFront
{

    /// <summary>
    /// Interaction logic for LogOnScreen.xaml
    /// </summary>
    public partial class LogOnScreen : Window, INotifyPropertyChanged
    {

        private Visibility hintVisibility;
        WMSServiceClient service;

        /// <summary>
        /// Creates a new instance of <c>LogOnScreen</c>.
        /// </summary>
        public LogOnScreen()
        {
            InitializeComponent();
            DataContext = this;
            HintVisibility = Visibility.Hidden;

            try
            {
                service = new WMSServiceClient();

                /*xDomain.ItemsSource = service.GetDomainList();*/
                xDomain.ItemsSource = service.GetCustomersList();
                xDomain.SelectedIndex = 0;

                xUsername.Focus();
            }
            catch
            {
                throw; //new Exception( + ex.Message);
            }

        }

        /// <summary>
        /// Returns the username entered within the UI.
        /// </summary>
        private void DoLogonClick(object sender, RoutedEventArgs e)
        {

            try
            {

                App.curUser = service.UserAuthentication(
                    new SysUser
                    {
                        UserName = xUsername.Text,
                        Password = xPassword.Password,
                        Domain = ((ShowData)xDomain.SelectedItem).DataValue
                    }
                    );

                //Aqui debe Esocogerse la Compania   
                App.curRol = App.curUser.UserRols[0]; //Aqui se toma el de la compania que se seleccione
                App.curLocation = App.curRol.Location;
                App.curCompany = App.curLocation.Company;
                App.currentLocation = ((ShowData)xDomain.SelectedItem).DataValue;

                DialogResult = true;
                Close();

            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }

        }


        public bool HintVisible
        {
            get { return HintVisibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    HintVisibility = Visibility.Visible;
                }
                else
                {
                    HintVisibility = Visibility.Hidden;
                }
            }
        }

        public Visibility HintVisibility
        {
            get { return hintVisibility; }
            set
            {
                if (value != hintVisibility)
                {
                    this.hintVisibility = value;
                    OnPropertyChanged("HintVisibility");
                }
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


        private void DoCredentialsFocussed(object sender, RoutedEventArgs e)
        {
            TextBoxBase tb = sender as TextBoxBase;
            if (tb == null)
            {
                PasswordBox pwb = sender as PasswordBox;
                pwb.SelectAll();
            }
            else
            {
                tb.SelectAll();
            }
        }
    }

}
