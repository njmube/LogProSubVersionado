using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using Odyssey.Controls;
using Microsoft.Windows.Controls;
using System.Windows;
using WpfFront.Common;
using System.Windows.Input;
using System.Linq;
using WpfFront.Common.UserControls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Data;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for DocumentManagerView.xaml
    /// </summary>
    public partial class DocumentManagerView : UserControlBase, IDocumentManagerView
    {
        public DocumentManagerView()
        {
            InitializeComponent();
            expHeader.IsExpanded = true;
            expDetail.IsExpanded = false;
        }

        public event EventHandler<EventArgs> SaveHeader;
        public event EventHandler<DataEventArgs<DocumentType>> ChangeVendorCustomer;
        public event EventHandler<EventArgs> DeleteDocumentLines;
        public event EventHandler<DataEventArgs<int>> SearchAddress;

        #region Properties

        public DocumentManagerModel Model
        {
            get
            { return this.DataContext as DocumentManagerModel; }
            set
            { this.DataContext = value; }

        }

        //HEADER

        //Datos Documento
        public ComboBox DocType
        { get { return this.Lst_DocType; } }

        /*public ComboBox DocConcept
        { get { return this.Lst_DocConcept; } }

        public ComboBox DocStatus
        { get { return this.Lst_DocStatus; } }*/

        public ComboBox Location
        { get { return this.Lst_Location; } }

        //Direcciones
        /*********/
        public ComboBox ShippingAddress
        {
            get { return this.Lst_ShipAddress; }
            set { this.Lst_ShipAddress = value; }
        }
        public StackPanel MostrarOcultarShippingAddress
        {
            get { return this.stkShipAddress; }
            set { this.stkShipAddress = value; }
        }
        public TextBlock TextoShippingAddress
        {
            get { return this.txtb_ShipAddress; }
            set { this.txtb_ShipAddress = value; }
        }
        /*********/
        /*********/
        public ComboBox BildAddress
        {
            get { return this.Lst_BildAddress; }
            set { this.Lst_BildAddress = value; }
        }
        public StackPanel MostrarOcultarBildAddress
        {
            get { return this.stkBildAddress; }
            set { this.stkBildAddress = value; }
        }
        public TextBlock TextoBildAddress
        {
            get { return this.txtb_BildAddress; }
            set { this.txtb_BildAddress = value; }
        }
        /*********/

        //Fechas
        /*********/
        public Microsoft.Windows.Controls.DatePicker DocumentDate
        { 
            get { return this.Txt_Date; }
            set { this.Txt_Date = value; }
        }
        /*********/
        /*********/
        public Microsoft.Windows.Controls.DatePicker Date2
        { 
            get { return this.Txt_Date2; }
            set { this.Txt_Date2 = value; }
        }
        public StackPanel MostrarOcultarDate2
        {
            get { return this.stkDate2; }
            set { this.stkDate2 = value; }
        }
        public TextBlock TextoDate2
        {
            get { return this.txtb_Date2; }
            set { this.txtb_Date2 = value; }
        }
        /*********/
        /*********/
        public Microsoft.Windows.Controls.DatePicker Date3
        { 
            get { return this.Txt_Date3; }
            set { this.Txt_Date3 = value; }
        }
        public StackPanel MostrarOcultarDate3
        {
            get { return this.stkDate3; }
            set { this.stkDate3 = value; }
        }
        public TextBlock TextoDate3
        {
            get { return this.txtb_Date3; }
            set { this.txtb_Date3 = value; }
        }
        /*********/
        /*********/
        public Microsoft.Windows.Controls.DatePicker Date4
        { 
            get { return this.Txt_Date4; }
            set { this.Txt_Date4 = value; }
        }
        public StackPanel MostrarOcultarDate4
        {
            get { return this.stkDate4; }
            set { this.stkDate4 = value; }
        }
        public TextBlock TextoDate4
        {
            get { return this.txtb_Date4; }
            set { this.txtb_Date4 = value; }
        }
        /*********/
        /*********/
        public Microsoft.Windows.Controls.DatePicker Date5
        { 
            get { return this.Txt_Date5; }
            set { this.Txt_Date5 = value; }
        }
        public StackPanel MostrarOcultarDate5
        {
            get { return this.stkDate5; }
            set { this.stkDate5 = value; }
        }
        public TextBlock TextoDate5
        {
            get { return this.txtb_Date5; }
            set { this.txtb_Date5 = value; }
        }
        /*********/

        //Datos Adicionales
        /*********/
        public TextBox CustPONumber
        {
            get { return this.txt_CustPONumber; }
            set { this.txt_CustPONumber = value; }
        }
        public StackPanel MostrarOcultarCustPONumber
        {
            get { return this.stkCustPONumber; }
            set { this.stkCustPONumber = value; }
        }
        public TextBlock TextoCustPONumber
        {
            get { return this.txtb_CustPONumber; }
            set { this.txtb_CustPONumber = value; }
        }
        /*********/
        /*********/
        public SearchAccount VendorID
        {
            get { return this.txt_VendorID; }
            set { this.txt_VendorID = value; }
        }
        public StackPanel MostrarOcultarVendorID
        {
            get { return this.stkVendor; }
            set { this.stkVendor = value; }
        }
        public TextBlock TextoVendorID
        {
            get { return this.txtb_VendorID; }
            set { this.txtb_VendorID = value; }
        }
        /*********/
        /*********/
        public SearchAccount CustomerID
        {
            get { return this.txt_CustomerID; }
            set { this.txt_CustomerID = value; }
        }
        public StackPanel MostrarOcultarCustomerID
        {
            get { return this.stkCustomer; }
            set { this.stkCustomer = value; }
        }
        public TextBlock TextoCustomerID
        {
            get { return this.txtb_CustomerID; }
            set { this.txtb_CustomerID = value; }
        }
        /*********/
        /*********/
        public TextBox SalesPersonName
        {
            get { return this.txt_SalesPersonName; }
            set { this.txt_SalesPersonName = value; }
        }
        public StackPanel MostrarOcultarSalesPersonName
        {
            get { return this.stkSalesPersonName; }
            set { this.stkSalesPersonName = value; }
        }
        public TextBlock TextoSalesPersonName
        {
            get { return this.txtb_SalesPersonName; }
            set { this.txtb_SalesPersonName = value; }
        }
        /*********/
        /*********/
        public TextBox QuoteNumber
        {
            get { return this.txt_QuoteNumber; }
            set { this.txt_QuoteNumber = value; }
        }
        public StackPanel MostrarOcultarQuoteNumber
        {
            get { return this.stkQuoteNumber; }
            set { this.stkQuoteNumber = value; }
        }
        public TextBlock TextoQuoteNumber
        {
            get { return this.txtb_QuoteNumber; }
            set { this.txtb_QuoteNumber = value; }
        }
        /*********/
        /*********/
        public ComboBox ShipMethodID
        { 
            get { return this.Lst_ShipMethodID; }
            set { this.Lst_ShipMethodID = value; }
        }
        public StackPanel MostrarOcultarShipMethodID
        {
            get { return this.stkShipMethodID; }
            set { this.stkShipMethodID = value; }
        }
        public TextBlock TextoShipMethodID
        {
            get { return this.txtb_ShipMethodID; }
            set { this.txtb_ShipMethodID = value; }
        }
        /*********/
        /*********/
        public ComboBox PickMethodID
        { 
            get { return this.Lst_PickMethodID; }
            set { this.Lst_PickMethodID = value; }
        }
        public StackPanel MostrarOcultarPickMethodID
        {
            get { return this.stkPickMethodID; }
            set { this.stkPickMethodID = value; }
        }
        public TextBlock TextoPickMethodID
        {
            get { return this.txtb_PickMethodID; }
            set { this.txtb_PickMethodID = value; }
        }
        /*********/

        public OdcExpander expDetails
        {
            get { return this.expDetail; }
            set { this.expDetail = value; } 
        }


        //DETAILS
        public AdminDocumentLineV2 AdminDocumentLine
        {
            get { return this.txtAdminDocumentLine; }
            set { this.txtAdminDocumentLine = value; }
        }
        
        public ListView ListViewDocumentLines
        {
            get { return this.lvDocumentLines; }
            set { this.lvDocumentLines = value; }
        }

        #endregion

        #region Events

        private void expLabel_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, true);
        }

        private void expLabel_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, false);
        }

        private void CheckExpanders(OdcExpander sender, bool expand)
        {

            if (expHeader == null || expDetail == null)
                return;

            if (sender.Name == "expHeader")
            {
                if (expand)
                {
                    expDetail.IsExpanded = false;
                }
                else
                {
                    expDetail.IsExpanded = true;

                }

                return;
            }

            if (sender.Name == "expDetail")
            {
                if (expand)
                {
                    expHeader.IsExpanded = false;
                }
                else
                {
                    expHeader.IsExpanded = true;
                }

                return;
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            //DATOS DOCUMENTO
            if (Lst_DocType.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar el tipo de documento");
                return;
            }
            if (Txt_DocNumber.Text == "")
            {
                Util.ShowError("Por favor digitar el numero de documento");
                return;
            }
            if (Lst_Location.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar la ubicacion del documento");
                return;
            }
            /*if (DocConcept.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar el concepto del documento");
                return;
            }
            if (DocStatus.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar el estado del documento");
                return;
            }*/
            //FECHAS
            if (Txt_Date.Text == "")
            {
                Util.ShowError("Por favor seleccionar la fecha de creacion del documento");
                return;
            }
            //DATOS ADICIONALES
            if (Txt_Reference.Text == "")
            {
                Util.ShowError("Por favor digitar el codigo de referencia");
                return;
            }

            SaveHeader(sender, e);
            expDetail.IsEnabled = true;
        }

        private void Lst_DocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeVendorCustomer(sender, new DataEventArgs<DocumentType>((DocumentType)DocType.SelectedItem));
        }

        private void btnDeleteDocumentLines_Click(object sender, RoutedEventArgs e)
        {
            DeleteDocumentLines(sender, e);
        }

        private void txt_CustomerID_OnSelected(object sender, EventArgs e)
        {
            SearchAddress(sender, new DataEventArgs<int>(CustomerID.Account.AccountID));
        }

        #endregion

    }

    public interface IDocumentManagerView
    {
        DocumentManagerModel Model { get; set; }

        //HEADER

        //Datos Documento
        ComboBox DocType { get; }
        //ComboBox DocConcept { get; }
        //ComboBox DocStatus { get; }
        ComboBox Location { get; }

        //Direcciones
        /*********/
        ComboBox ShippingAddress { get; set; }
        StackPanel MostrarOcultarShippingAddress { get; set; }
        TextBlock TextoShippingAddress { get; set; }
        /*********/
        /*********/
        ComboBox BildAddress { get; set; }
        StackPanel MostrarOcultarBildAddress { get; set; }
        TextBlock TextoBildAddress { get; set; }
        /*********/

        //Fechas
        /*********/
        Microsoft.Windows.Controls.DatePicker DocumentDate { get; set; }
        /*********/
        /*********/
        Microsoft.Windows.Controls.DatePicker Date2 { get; set; }
        StackPanel MostrarOcultarDate2 { get; set; } //Para cambiar a visibility
        TextBlock TextoDate2 { get; set; } //Para colocar el texto
        /*********/
        /*********/
        Microsoft.Windows.Controls.DatePicker Date3 { get; set; }
        StackPanel MostrarOcultarDate3 { get; set; } //Para cambiar a visibility
        TextBlock TextoDate3 { get; set; } //Para colocar el texto
        /*********/
        /*********/
        Microsoft.Windows.Controls.DatePicker Date4 { get; set; }
        StackPanel MostrarOcultarDate4 { get; set; } //Para cambiar a visibility
        TextBlock TextoDate4 { get; set; } //Para colocar el texto
        /*********/
        /*********/
        Microsoft.Windows.Controls.DatePicker Date5 { get; set; }
        StackPanel MostrarOcultarDate5 { get; set; } //Para cambiar a visibility
        TextBlock TextoDate5 { get; set; } //Para colocar el texto
        /*********/

        //Datos Adicionales
        /*********/
        TextBox CustPONumber { get; set; }
        StackPanel MostrarOcultarCustPONumber { get; set; } //Para cambiar a visibility
        TextBlock TextoCustPONumber { get; set; } //Para colocar el texto
        /*********/
        /*********/
        SearchAccount VendorID { get; set; }
        StackPanel MostrarOcultarVendorID { get; set; } //Para cambiar a visibility
        TextBlock TextoVendorID { get; set; } //Para colocar el texto
        /*********/
        /*********/
        SearchAccount CustomerID { get; set; }
        StackPanel MostrarOcultarCustomerID { get; set; } //Para cambiar a visibility
        TextBlock TextoCustomerID { get; set; } //Para colocar el texto
        /*********/
        /*********/
        TextBox SalesPersonName { get; set; }
        StackPanel MostrarOcultarSalesPersonName { get; set; } //Para cambiar a visibility
        TextBlock TextoSalesPersonName { get; set; } //Para colocar el texto
        /*********/
        /*********/
        TextBox QuoteNumber { get; set; }
        StackPanel MostrarOcultarQuoteNumber { get; set; } //Para cambiar a visibility
        TextBlock TextoQuoteNumber { get; set; } //Para colocar el texto
        /*********/
        /*********/
        ComboBox ShipMethodID { get; set; }
        StackPanel MostrarOcultarShipMethodID { get; set; } //Para cambiar a visibility
        TextBlock TextoShipMethodID { get; set; } //Para colocar el texto
        /*********/
        /*********/
        ComboBox PickMethodID { get; set; }
        StackPanel MostrarOcultarPickMethodID { get; set; } //Para cambiar a visibility
        TextBlock TextoPickMethodID { get; set; } //Para colocar el texto
        /*********/
        OdcExpander expDetails { get; set; }
        

        //DETAILS
        AdminDocumentLineV2 AdminDocumentLine { get; set; }
        ListView ListViewDocumentLines { get; set; }

        //Events
        event EventHandler<EventArgs> SaveHeader;
        event EventHandler<DataEventArgs<DocumentType>> ChangeVendorCustomer;
        event EventHandler<EventArgs> DeleteDocumentLines;
        event EventHandler<DataEventArgs<int>> SearchAddress;
    }
}
