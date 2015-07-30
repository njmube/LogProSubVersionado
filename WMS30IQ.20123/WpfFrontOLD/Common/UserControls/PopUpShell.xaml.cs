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
using System.Windows.Controls.Primitives;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for PopUpShell.xaml
    /// </summary>
    public partial class PopUpShell : UserControl
    {
        public PopUpShell()
        {
            InitializeComponent();
        }


        public void ShowViewInShell(object view)
        {
                this.MainInformation.Items.Clear();
                this.MainInformation.Items.Add(view);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Popup)this.Parent).IsOpen = false;
        }

    }
}
