using System;
using Assergs.Windows;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace WpfFront.Common
{
	public partial class ProcessWindow : Window
	{
        public ProcessWindow(string message)
		{
			InitializeComponent();            
            this.txtMesage.Text = string.IsNullOrEmpty(message) ? "Processing ..." : message;
            this.Show();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

	}
}