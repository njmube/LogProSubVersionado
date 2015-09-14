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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using WpfFront.WMSBusinessService;
using WpfFront.Services;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for DirectPrint.xaml
    /// </summary>
    public partial class DirectPrint : UserControl, INotifyPropertyChanged
    {

        public DirectPrint()
        {
            InitializeComponent();
            DataContext = this;
        }


        public static DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(Document), typeof(DirectPrint));

        public Document Document
        {
            get { return (Document)GetValue(DocumentProperty); }
            set
            {
                SetValue(DocumentProperty, value);
            }
        }


        public static DependencyProperty NewStatusProperty = DependencyProperty.Register("NewStatus", typeof(Status), typeof(DirectPrint));

        public Status NewStatus
        {
            get { return (Status)GetValue(NewStatusProperty); }
            set
            {
                SetValue(NewStatusProperty, value);
            }
        }


        public IList<Printer> PrinterList
        {
            get
            {
                try { return App.printerList; } //.Where(f => f.FromServer == true).ToList(); }
                catch { return null; }
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

        private void imgPrint_Click(object sender, RoutedEventArgs e)
        {
            if (this.Document == null)
                return;

            if (this.cboPrinter.SelectedItem == null)
            {
                Util.ShowError("Please select a Printer.");
                return;
            }

            ProcessWindow pw = new ProcessWindow("Sending to Print ...");

            try
            {
                PrinterControl.PrintDocumentsInBatch(new List<Document> { this.Document },
                    (Printer)this.cboPrinter.SelectedItem);

                //(new WMSServiceClient()).PrintDocumentsInBatch(new List<Document> { this.Document },
                //    ((Printer)this.cboPrinter.SelectedItem).PrinterPath, null);


                if (NewStatus != null)
                {
                    this.Document.DocStatus = NewStatus;
                    (new WMSServiceClient()).UpdateDocument(this.Document);
                }
            }

            catch { }
            finally { pw.Close(); }



        }

        private void DirectPrintInstance_Loaded(object sender, RoutedEventArgs e)
        {
            try { cboPrinter.SelectedItem = PrinterList.Where(f => f.IsDefault == true).FirstOrDefault(); }
            catch { }
        }


    }
}
