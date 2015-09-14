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
    public partial class PopUpDocumentDetail : UserControl, INotifyPropertyChanged
    {

        public PopUpDocumentDetail()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += new RoutedEventHandler(PopUpDocumentDetail_Loaded); 
        }


        private WMSServiceClient _service;
        public WMSServiceClient service
        {
            get
            {

                if (_service == null)
                    return new WMSServiceClient();
                else
                    return _service;
            }
        }

        void PopUpDocumentDetail_Loaded(object sender, RoutedEventArgs e)    {        
           
            //Cierra el popup si el documento es nulo
            if (this.CurDocument == null)
            {
                ((Popup)this.Parent).IsOpen = false;
                return;
            }

            //Si obtiene los document lines.
            DocumentLines = service.GetDocumentLine(new DocumentLine {Document = this.CurDocument});

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
        public static DependencyProperty CurDocumentProperty =
            DependencyProperty.Register("CurDocument", typeof(Document), typeof(PopUpDocumentDetail));

        public Document CurDocument {
            get { return (Document)GetValue(CurDocumentProperty); }
            set { 
                SetValue(CurDocumentProperty, value);
            } 
        }


        private IList<DocumentLine> _DocumentLines;
        public IList<DocumentLine> DocumentLines
        {
            get
            {
                return _DocumentLines;
            }
            set
            {
                _DocumentLines = value;
                OnPropertyChanged("DocumentLines");
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Popup)this.Parent).IsOpen = false;
        }




    }




}
