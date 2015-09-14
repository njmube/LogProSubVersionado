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
using WpfFront.Presenters;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for PopUpDocumentDetail.xaml
    /// </summary>
    public partial class AdminDocumentLine : UserControl, INotifyPropertyChanged
    {


        public AdminDocumentLine()
        {
            InitializeComponent();
            DataContext = this;



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


        private IList<Unit> _ProductUnits;
        public IList<Unit> ProductUnits
        {
            get
            {
                return _ProductUnits;
            }
            set
            {
                _ProductUnits = value;
                OnPropertyChanged("ProductUnits");
            }
        }



        public Document CurDocument { get; set; }
        public Object PresenterParent { get; set; }


        private DocumentLine _CurDocLine;
        public DocumentLine CurDocLine
        {
            get { return _CurDocLine; }
            set
            {
                _CurDocLine = value;
                OnPropertyChanged("CurDocLine");
            }
        }



        private void SearchProduct_OnLoadRecord(object sender, EventArgs e)
        {
            if (scProduct.Product == null)
                return;
            //Carga las unidades disponibles
            ProductUnits = service.GetProductUnit(scProduct.Product);
            cboUnit.Items.Refresh();

            //if (ProductUnits.Count == 1)
                cboUnit.SelectedIndex = 0;

            txtNotes.Text = scProduct.Product.Description;

        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (scProduct.Product == null)
            {
                Util.ShowError("Please select a Product.");
                return;
            }

            if (cboUnit.SelectedItem == null)
            {
                Util.ShowError("Please select a Unit.");
                return;
            }

            if (string.IsNullOrEmpty(txtQty.Text))
            {
                
                return;
            }

            double qty, qtycancel, qtybo;
            if (!Double.TryParse(txtQty.Text, out qty))
            {
                Util.ShowError("Please enter a valid Quantity.");
                return;
            }

            if (!Double.TryParse(txtCancel.Text, out qtycancel))
            {
                Util.ShowError("Please enter a valid Quantity to Cancel.");
                return;
            }

            if (!Double.TryParse(txtBO.Text, out qtybo))
            {
                Util.ShowError("Please enter a valid Quantity to Backorder.");
                return;
            }


            if (qty <= 0 || qtycancel < 0 || qtybo < 0 )
            {
                Util.ShowError("Please enter a valid quantities.");
                return;
            }

            if (qty - qtycancel - qtybo < 0)
            {
                Util.ShowError("Wrong Quantity balance. Quantity - QtyCancel - QtyBackOrder must be greather than zero.");
                return;
            }

            string resultMsg = "";
            //Add Update the Document Line
            if (CurDocLine == null)
                CurDocLine = new DocumentLine { };

            try
            {
                CurDocLine.Unit = cboUnit.SelectedItem as Unit;
                CurDocLine.Product = scProduct.Product;
                CurDocLine.Quantity = qty;
                CurDocLine.QtyCancel = double.Parse(txtCancel.Text);
                CurDocLine.QtyBackOrder = double.Parse(txtBO.Text);
                CurDocLine.LineDescription = txtNotes.Text;
                CurDocLine.Location = App.curLocation;

                CurDocLine.Document = CurDocument;
                CurDocLine.ModifiedBy = App.curUser.UserName;

                //Si es un Merged Order y es una linea nueva debe adicionarle el link Document
                if (CurDocLine.Document.DocType.DocTypeID == SDocType.MergedSalesOrder && CurDocLine.LineID == 0) {

                    string oriDoc = service.GetDocumentLine(new DocumentLine
                                {
                                    Document = new Document { DocID = CurDocLine.Document.DocID },
                                    LineStatus = new Status { StatusID = DocStatus.New }
                                }).First().LinkDocNumber;
                    
                    CurDocLine.LinkDocNumber = oriDoc;
                    resultMsg = "\nLine added also in document " + oriDoc;
                }


                CurDocLine = service.SaveUpdateDocumentLine(CurDocLine, false);


                //Refrescar el Padre.
                if (PresenterParent.GetType().Name.Contains("ShippingPresenter"))
                    ((ShippingPresenter)PresenterParent).ReloadDocumentLines();
                else if (PresenterParent.GetType().Name.Contains("ShipperMngrPresenter"))                
                    ((ShipperMngrPresenter)PresenterParent).UpdateLines(CurDocLine);
                


                ResetPanel();
                ((Popup)this.Parent).IsOpen = false;

                Util.ShowMessage("Process Completed." + resultMsg);


            }
            catch (Exception ex) {
                Util.ShowError("Error creating Line.\n" + ex.Message);
            }


        }

        private void ResetPanel()
        {
            scProduct.Product = null;
            scProduct.txtProductDesc.Text = "";
            scProduct.txtData.Text = "";

            cboUnit.SelectedItem = null;
            txtBO.Text = txtCancel.Text = txtQty.Text = "0";
            txtNotes.Text = "";

        }



        public static DependencyProperty LineProperty = DependencyProperty.Register("Line", typeof(DocumentLine), typeof(AdminDocumentLine));




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


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ResetPanel();
        }

        private void txtHide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Popup)this.Parent).IsOpen = false;
        }

 



    }
}