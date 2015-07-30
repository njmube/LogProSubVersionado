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
using WpfFront.WMSBusinessService;
using Core.BusinessEntity;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using WpfFront.Services;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for PopUpDocumentDetail.xaml
    /// </summary>
    public partial class DocumentList : UserControl, INotifyPropertyChanged
    {

        public DocumentList()
        {
            InitializeComponent();
            DataContext = this;
            //this.Loaded += new RoutedEventHandler(DocumentList_Loaded); 
        }


        void DocumentList_Loaded(object sender, RoutedEventArgs e)
        {
            if (AllowHide)
                this.txtHide.Visibility = Visibility.Visible;
        }


        public void LoadDocuments(string UserDef)
        {
            //ProcessWindow pw = new ProcessWindow("Loading ... ");

            //Si obtiene los document lines.
            OrderList = (new WMSServiceClient())
                .GetPendingDocument(new Document { DocType = this.CurDocumentType, UserDef1 = UserDef }, 15, WmsSetupValues.NumRegs)
                .Where(f=>f.DocStatus.StatusID != DocStatus.Completed).OrderByDescending(f => f.DocID).ToList();

            this.imgTicket.Visibility = (this.CurDocumentType.Template != null) ? Visibility.Visible : Visibility.Collapsed;


            //si lo muestra o no.
            this.Visibility = (OrderList != null && OrderList.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

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


        //Document Property
        public static DependencyProperty CurDocumentTypeProperty =
            DependencyProperty.Register("CurDocumentType", typeof(DocumentType), typeof(DocumentList));

        public DocumentType CurDocumentType {
            get { return (DocumentType)GetValue(CurDocumentTypeProperty); }
            set {
                SetValue(CurDocumentTypeProperty, value);
            } 
        }


        private IList<Document> _List;
        public IList<Document> OrderList
        {
            get
            {
                return _List;
            }
            set
            {
                _List = value;
                OnPropertyChanged("OrderList");
            }
        }


        private Boolean _AllowHide;
        public Boolean AllowHide
        {
            get
            {  return _AllowHide; }
            set
            { _AllowHide = value; }
        }



        private void imgTicket_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lvOrders.SelectedItem == null)
            {
                Util.ShowError("Please select a record.");
                return;
            }

             ProcessWindow pw = new ProcessWindow("Generating Document ... ");

            //Open the Document Ticket
            try
            {               
                UtilWindow.ShowDocument(CurDocumentType.Template , ((Document)lvOrders.SelectedItem).DocID, "", false); 
                pw.Close();
            }
            catch { pw.Close(); }
        }


        private void imgRefresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadDocuments("");
        }

        private void imgDetails_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popup1.IsOpen = true;
            popup1.StaysOpen = true;
        }

        private void lvOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            popup1.IsOpen = true;
            popup1.StaysOpen = true;
        }

        private void txtHide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.brDocList.Visibility = Visibility.Collapsed;
        }



    }




}
