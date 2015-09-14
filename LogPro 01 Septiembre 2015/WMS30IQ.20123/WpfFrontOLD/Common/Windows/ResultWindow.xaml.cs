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
	public partial class ResultWindow : Window
	{

        public bool showNextTime;

        public ResultWindow()
		{
			InitializeComponent();
        }


        private void btn1_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
	}
}