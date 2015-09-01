using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;

namespace WpfFront.Presenters
{

    public interface IDocumentManagerPresenter
    {
        IDocumentManagerView View { get; set; }
        ToolWindow Window { get; set; }
    }

    public class DocumentManagerPresenter: IDocumentManagerPresenter
    {
        public IDocumentManagerView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables para el manejo de los campos a mostrar y guardar en el header
        string[] CadenaCampos = null;
        char[] SeparadorCampos = { ';' };
        char[] SeparadorDatos = { ':' };
        bool NeedAddress = false;


        public DocumentManagerPresenter(IUnityContainer container, IDocumentManagerView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DocumentManagerModel>();

            //Event Delegate
            View.SaveHeader += new EventHandler<EventArgs>(this.OnSaveHeader);
            View.ChangeVendorCustomer += new EventHandler<DataEventArgs<DocumentType>>(this.OnChangeVendorCustomer);
            View.DeleteDocumentLines += new EventHandler<EventArgs>(this.OnDeleteDocumentLines);
            View.SearchAddress += new EventHandler<DataEventArgs<int>>(OnSearchAddress);

            //ProcessWindow pw = new ProcessWindow("Loading ...");
            //Inicializo las variables del sistema y los listados a mostrar
            View.Model.Record = new Document();
            View.Model.RecordShippingAddress = new DocumentAddress();
            View.Model.RecordBildAddress = new DocumentAddress();
            View.Model.DocumentLine = new DocumentLine();
            View.Model.DocumentLineList = new List<DocumentLine>();
            View.Model.DocTypeList = service.GetDocumentType(new DocumentType()).Where(f => f.DocClass.HasAdmin == true).ToList();
            /*View.Model.DocConceptList = service.GetDocumentConcept(new DocumentConcept());
            View.Model.DocStatusList = service.GetStatus(new Status());*/
            View.Model.LocationList = service.GetLocation(new Location());
            View.Model.ShippingMethodList = service.GetShippingMethod(new ShippingMethod());
            View.Model.PickingMethodList = service.GetPickMethod(new PickMethod());
            View.DocumentDate.Text = DateTime.Now.ToString();
            View.AdminDocumentLine.PresenterParent = this;

            //Asigno los campos obligatorios para el documento de cabecera
            View.Model.Record.Company = new Company { CompanyID = App.curCompany.CompanyID };
            View.Model.Record.IsFromErp = false;
            View.Model.Record.Priority = 0;
            View.Model.Record.CrossDocking = false;
            View.Model.Record.CreatedBy = App.curUser.UserName;
            View.Model.Record.CreationDate = DateTime.Now;

            //Asigno los campos obligatorios para los registros de direcciones de envio y facturacion
            View.Model.RecordShippingAddress.CreatedBy = App.curUser.UserName;
            view.Model.RecordShippingAddress.CreationDate = DateTime.Now;
            View.Model.RecordBildAddress.CreatedBy = App.curUser.UserName;
            view.Model.RecordBildAddress.CreationDate = DateTime.Now;

            //pw.Close();
        }

        #region metodos

        private void OnSaveHeader(object sender, EventArgs e)
        {
            try
            {
                //Obtengo los datos seleccionados en los combobox para asignarlos a la variable y poder guardarla o actualizarla
                /*Evaluo si fue realizado una compra o una venta para asignar los datos
                 * Purchase Order: Vendor ---> AccountID, Customer ---> 3717(Default)
                 * Sales Order: Customer ---> AccountID, Vendor ---> 3717(Default)
                 */
                View.Model.Record.DocType = new DocumentType { DocTypeID = (short)View.DocType.SelectedValue };
                //Valores por defecto del concepto y el estado del documento, cuando se activen los campos
                //se debe quitar estos valores
                /*View.Model.Record.DocConcept = new DocumentConcept { DocConceptID = (short)View.DocConcept.SelectedValue };
                View.Model.Record.DocStatus = new Status { StatusID = (int)View.DocStatus.SelectedValue };*/
                View.Model.Record.DocConcept = new DocumentConcept { DocConceptID = 101 };
                View.Model.Record.DocStatus = new Status { StatusID = 101 };
                View.Model.Record.Location = new Location { LocationID = (int)View.Location.SelectedValue };
                View.Model.Record.Date1 = DateTime.Parse(View.DocumentDate.Text);
                if (CadenaCampos != null)
                {
                    foreach (string Campos in CadenaCampos)
                    {
                        string[] Datos;
                        Datos = Campos.Split(SeparadorDatos);
                        switch (Datos[0])
                        {
                            case "CustPONumber":
                                if (View.CustPONumber.Text == "")
                                {
                                    Util.ShowError("Por favor digitar el numero PO cliente");
                                    return;
                                }
                                break;
                            case "VendorID":
                                if (View.VendorID.Account == null)
                                {
                                    Util.ShowError("Por favor seleccione un Vendedor");
                                    return;
                                }
                                View.Model.Record.Vendor = new Account { AccountID = View.VendorID.Account.AccountID };
                                View.Model.Record.Customer = new Account { AccountID = 3717 };
                                NeedAddress = false; //Si es vendedor no es necesario las direcciones
                                break;
                            case "CustomerID":
                                if (View.CustomerID.Account == null)
                                {
                                    Util.ShowError("Por favor seleccione un Cliente");
                                    return;
                                }
                                View.Model.Record.Customer = new Account { AccountID = View.CustomerID.Account.AccountID };
                                View.Model.Record.Vendor = new Account { AccountID = 3717 };
                                NeedAddress = true; //Si es cliente es necesario las direcciones
                                break;
                            case "Date2":
                                if(View.Date2.Text == "")
                                {
                                    Util.ShowError("Por favor seleccionar la fecha del campo " + Datos[1]);
                                    return;
                                }
                                View.Model.Record.Date2 = DateTime.Parse(View.Date2.Text);
                                break;
                            case "Date3":
                                if (View.Date3.Text == "")
                                {
                                    Util.ShowError("Por favor seleccionar la fecha del campo " + Datos[1]);
                                    return;
                                }
                                View.Model.Record.Date3 = DateTime.Parse(View.Date3.Text);
                                break;
                            case "Date4":
                                if (View.Date4.Text == "")
                                {
                                    Util.ShowError("Por favor seleccionar la fecha del campo " + Datos[1]);
                                    return;
                                }
                                View.Model.Record.Date4 = DateTime.Parse(View.Date4.Text);
                                break;
                            case "Date5":
                                if (View.Date5.Text == "")
                                {
                                    Util.ShowError("Por favor seleccionar la fecha del campo " + Datos[1]);
                                    return;
                                }
                                View.Model.Record.Date5 = DateTime.Parse(View.Date5.Text);
                                break;
                            case "SalesPersonName":
                                if (View.SalesPersonName.Text == "")
                                {
                                    Util.ShowError("Por favor digitar el nombre del vendedor");
                                    return;
                                }
                                break;
                            case "QuoteNumber":
                                if (View.QuoteNumber.Text == "")
                                {
                                    Util.ShowError("Por favor digitar el numero de la cotizacion");
                                    return;
                                }
                                break;
                            case "ShipMethodID":
                                if (View.ShipMethodID.SelectedIndex == -1)
                                {
                                    Util.ShowError("Por favor seleccionar el metodo de envio");
                                    return;
                                }
                                View.Model.Record.ShippingMethod = new ShippingMethod { ShpMethodID = (short)View.ShipMethodID.SelectedValue };
                                break;
                            case "PickMethodID":
                                if (View.PickMethodID.SelectedIndex == -1)
                                {
                                    Util.ShowError("Por favor seleccionar el metodo de seleeccion");
                                    return;
                                }
                                View.Model.Record.PickMethod = new PickMethod { MethodID = (short)View.PickMethodID.SelectedValue };
                                break;
                            case "ShippingAddress":
                                if (View.ShippingAddress.SelectedIndex == -1)
                                {
                                    Util.ShowError("Por favor seleccionar direccion de envio");
                                    return;
                                }
                                //Datos variables seleccionados
                                View.Model.RecordShippingAddress.AddressLine1 = ((AccountAddress)View.ShippingAddress.SelectedItem).AddressLine1;
                                View.Model.RecordShippingAddress.AddressType = (short)AddressType.Shipping;
                                View.Model.RecordShippingAddress.ShpMethod = new ShippingMethod { ShpMethodID = (short)View.ShipMethodID.SelectedValue };
                                //Datos estaticos traidos directamente del registro en la DB
                                View.Model.RecordShippingAddress.Name = View.CustomerID.Account.Name;
                                View.Model.RecordShippingAddress.AddressLine2 = ((AccountAddress)View.ShippingAddress.SelectedItem).AddressLine2;
                                View.Model.RecordShippingAddress.AddressLine3 = ((AccountAddress)View.ShippingAddress.SelectedItem).AddressLine3;
                                View.Model.RecordShippingAddress.City = ((AccountAddress)View.ShippingAddress.SelectedItem).City;
                                View.Model.RecordShippingAddress.State = ((AccountAddress)View.ShippingAddress.SelectedItem).State;
                                View.Model.RecordShippingAddress.ZipCode = ((AccountAddress)View.ShippingAddress.SelectedItem).ZipCode;
                                View.Model.RecordShippingAddress.Country = ((AccountAddress)View.ShippingAddress.SelectedItem).Country;
                                View.Model.RecordShippingAddress.ContactPerson = ((AccountAddress)View.ShippingAddress.SelectedItem).ContactPerson;
                                View.Model.RecordShippingAddress.Phone1 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone1;
                                View.Model.RecordShippingAddress.Phone2 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone2;
                                View.Model.RecordShippingAddress.Phone3 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone3;
                                break;
                            case "BildAddress":
                                if (View.BildAddress.SelectedIndex == -1)
                                {
                                    Util.ShowError("Por favor seleccionar direccion de facturacion");
                                    return;
                                }
                                View.Model.RecordBildAddress.AddressLine1 = ((AccountAddress)View.BildAddress.SelectedItem).AddressLine1;
                                View.Model.RecordBildAddress.AddressType = AddressType.Billing;
                                //Datos estaticos traidos directamente del registro en la DB
                                View.Model.RecordBildAddress.Name = "PRIMARY";
                                View.Model.RecordBildAddress.AddressLine2 = ((AccountAddress)View.ShippingAddress.SelectedItem).AddressLine2;
                                View.Model.RecordBildAddress.AddressLine3 = ((AccountAddress)View.ShippingAddress.SelectedItem).AddressLine3;
                                View.Model.RecordBildAddress.City = ((AccountAddress)View.ShippingAddress.SelectedItem).City;
                                View.Model.RecordBildAddress.State = ((AccountAddress)View.ShippingAddress.SelectedItem).State;
                                View.Model.RecordBildAddress.ZipCode = ((AccountAddress)View.ShippingAddress.SelectedItem).ZipCode;
                                View.Model.RecordBildAddress.Country = ((AccountAddress)View.ShippingAddress.SelectedItem).Country;
                                View.Model.RecordBildAddress.ContactPerson = ((AccountAddress)View.ShippingAddress.SelectedItem).ContactPerson;
                                View.Model.RecordBildAddress.Phone1 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone1;
                                View.Model.RecordBildAddress.Phone2 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone2;
                                View.Model.RecordBildAddress.Phone3 = ((AccountAddress)View.ShippingAddress.SelectedItem).Phone3;
                                break;
                        }
                    }
                }

                //Evaluo si el document no existe para crearlo o si existe realizarle una actualizacion
                if (View.Model.Record.DocID == 0)
                    View.Model.Record = service.SaveDocument(View.Model.Record);
                else
                    service.UpdateDocument(View.Model.Record);
                View.AdminDocumentLine.CurDocument = View.Model.Record;

                //Evaluo si las direcciones de envio y facturacion ya fueron guardadas para actualizarlas o crearlas
                //Se realiza luego de crear el documento para asignarle las direcciones
                if (NeedAddress)
                {
                    if (View.Model.RecordShippingAddress.RowID == 0)
                    {
                        View.Model.RecordShippingAddress.Document = new Document { DocID = View.Model.Record.DocID };
                        View.Model.RecordBildAddress.Document = new Document { DocID = View.Model.Record.DocID };
                        View.Model.RecordShippingAddress = service.SaveDocumentAddress(View.Model.RecordShippingAddress);
                        View.Model.RecordBildAddress = service.SaveDocumentAddress(View.Model.RecordBildAddress);
                    }
                    else
                    {
                        service.UpdateDocumentAddress(View.Model.RecordShippingAddress);
                        service.UpdateDocumentAddress(View.Model.RecordBildAddress);
                    }
                }

                
                Util.ShowMessage("Documento Guardado exitosamente");
            }
            catch { Util.ShowError("Se encontro un problema al momento de realizar la grabacion del Documento, por favor intentelo nuevamente"); }
        }

        public void ReloadDocumentLines()
        {
            try
            {
                View.Model.DocumentLineList = service.GetDocumentLine(new DocumentLine { Document = View.Model.Record }).ToList();
            }
            catch { }
        }

        private void OnDeleteDocumentLines(object sender, EventArgs e)
        {
            if (View.ListViewDocumentLines.Items.Count == 0)
                return;
            try
            {
                foreach (DocumentLine dl in View.ListViewDocumentLines.SelectedItems)
                {
                    service.DeleteDocumentLine(dl);
                }
                ReloadDocumentLines();
            }
            catch { }
        }

        private void OnSearchAddress(object sender, DataEventArgs<int> e)
        {
            View.Model.ShippingAddressList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = e.Value } });
            View.Model.BildAddressList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = e.Value } });
        }

        private void OnChangeVendorCustomer(object sender, DataEventArgs<DocumentType> e)
        {
            //Oculto todos los campos para volver a evaluar cuales son los nuevos a mostrar
            View.MostrarOcultarCustPONumber.Visibility = Visibility.Collapsed;
            View.MostrarOcultarVendorID.Visibility = Visibility.Collapsed;
            View.MostrarOcultarCustomerID.Visibility = Visibility.Collapsed;
            View.MostrarOcultarDate2.Visibility = Visibility.Collapsed;
            View.MostrarOcultarDate3.Visibility = Visibility.Collapsed;
            View.MostrarOcultarDate4.Visibility = Visibility.Collapsed;
            View.MostrarOcultarDate5.Visibility = Visibility.Collapsed;
            View.MostrarOcultarSalesPersonName.Visibility = Visibility.Collapsed;
            View.MostrarOcultarQuoteNumber.Visibility = Visibility.Collapsed;
            View.MostrarOcultarShipMethodID.Visibility = Visibility.Collapsed;
            View.MostrarOcultarPickMethodID.Visibility = Visibility.Collapsed;
            View.MostrarOcultarShippingAddress.Visibility = Visibility.Collapsed;
            View.MostrarOcultarBildAddress.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(e.Value.DocClass.Fields))
            {
                CadenaCampos = e.Value.DocClass.Fields.Split(SeparadorCampos);

                //Evaluacion de los campos para mostrar los actuales
                foreach (string Campos in CadenaCampos)
                {
                    string[] Datos;
                    Datos = Campos.Split(SeparadorDatos);
                    switch (Datos[0])
                    {
                        case "CustPONumber":
                            View.MostrarOcultarCustPONumber.Visibility = Visibility.Visible;
                            View.TextoCustPONumber.Text = Datos[1];
                            break;
                        case "VendorID":
                            View.MostrarOcultarVendorID.Visibility = Visibility.Visible;
                            View.TextoVendorID.Text = Datos[1];
                            break;
                        case "CustomerID":
                            View.MostrarOcultarCustomerID.Visibility = Visibility.Visible;
                            View.TextoCustomerID.Text = Datos[1];
                            break;
                        case "Date2":
                            View.MostrarOcultarDate2.Visibility = Visibility.Visible;
                            View.TextoDate2.Text = Datos[1];
                            break;
                        case "Date3":
                            View.MostrarOcultarDate3.Visibility = Visibility.Visible;
                            View.TextoDate3.Text = Datos[1];
                            break;
                        case "Date4":
                            View.MostrarOcultarDate4.Visibility = Visibility.Visible;
                            View.TextoDate4.Text = Datos[1];
                            break;
                        case "Date5":
                            View.MostrarOcultarDate5.Visibility = Visibility.Visible;
                            View.TextoDate5.Text = Datos[1];
                            break;
                        case "SalesPersonName":
                            View.MostrarOcultarSalesPersonName.Visibility = Visibility.Visible;
                            View.TextoSalesPersonName.Text = Datos[1];
                            break;
                        case "QuoteNumber":
                            View.MostrarOcultarQuoteNumber.Visibility = Visibility.Visible;
                            View.TextoQuoteNumber.Text = Datos[1];
                            break;
                        case "ShipMethodID":
                            View.MostrarOcultarShipMethodID.Visibility = Visibility.Visible;
                            View.TextoShipMethodID.Text = Datos[1];
                            break;
                        case "PickMethodID":
                            View.MostrarOcultarPickMethodID.Visibility = Visibility.Visible;
                            View.TextoPickMethodID.Text = Datos[1];
                            break;
                        case "ShippingAddress":
                            View.MostrarOcultarShippingAddress.Visibility = Visibility.Visible;
                            View.TextoShippingAddress.Text = Datos[1];
                            break;
                        case "BildAddress":
                            View.MostrarOcultarBildAddress.Visibility = Visibility.Visible;
                            View.TextoBildAddress.Text = Datos[1];
                            break;
                    }
                }
            }
            else
            {
                CadenaCampos = null;
            }
        }

        #endregion

    }
}
