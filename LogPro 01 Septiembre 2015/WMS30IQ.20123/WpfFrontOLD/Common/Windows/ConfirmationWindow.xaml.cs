

using System;
using Assergs.Windows;
using System.Windows;
using System.Windows.Controls; 



namespace WpfFront.Common
{
    public partial class ConfirmationWindow : Window
    {

        public ConfirmationWindow()
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
            MessageBoxText.Text = "";
            DialogResult = false;
            this.Close();
        }

        private void MessageBoxText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MessageBoxText.Text.Length >= 10)
                btnConfirm.IsEnabled = true;

        }
    }
}