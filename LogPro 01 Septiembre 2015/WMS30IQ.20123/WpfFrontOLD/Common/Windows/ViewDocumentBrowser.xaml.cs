using System;
using System.Windows.Forms;
using System.Windows;

namespace WpfFront.Common
{
    /// <summary>
    /// Interaction logic for winPdf.xaml
    /// </summary>
    public partial class ViewDocumentBrowser : Window
    {
        private System.Windows.Forms.WebBrowser webbrowserOne;

        public ViewDocumentBrowser(string urlDocument)
        {
            InitializeComponent();
            webbrowserOne = new WebBrowser();
            windowsFormsHost1.Child = webbrowserOne;
            webbrowserOne.Url = new Uri(urlDocument); 

        }
    }
}
