

using System;
using Assergs.Windows;
using System.Windows;
using System.Windows.Controls;



namespace WpfFront.Common
{
    public partial class ConfirmationOK : Window
    {

        public ConfirmationOK()
        {
            InitializeComponent();
        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}