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
using System.Windows.Shapes;
using WpfFront.Services;
using WpfFront.WMSBusinessService;

namespace WpfFront.Common
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private WMSServiceClient service = new WMSServiceClient();
        string menuOption;

        public AuthorizationWindow(string MenuOption)
        {
            InitializeComponent();
            menuOption = MenuOption;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ValidateUser();
        }

        private void ValidateUser()
        {
            try
            {
                // valida user
                SysUser newUser = service.UserAuthentication(
                     new SysUser
                     {
                         UserName = txtUsername.Text,
                         Password = txtPassword.Password,
                         Domain = App.curUser.Domain
                     });

                // valida si tiene permisos para la opción enviada 
                if (Util.AllowOption(menuOption, newUser.UserRols[0].Rol))
                {
                    App.curAuthUser = newUser.UserName;
                    DialogResult = true;                    
                    Close();
                }
                else
                {
                    Util.ShowError("Unauthorized User.");
                    DialogResult = false;
                }
            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Util.AllowOption(menuOption))
            {
                DialogResult = true;
                Close();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ValidateUser();
        }
    }
}
