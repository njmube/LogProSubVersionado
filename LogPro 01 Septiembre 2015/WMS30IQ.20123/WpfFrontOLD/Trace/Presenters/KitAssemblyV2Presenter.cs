using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows;
using WpfFront.Services;

using System.Linq;
using Xceed.Wpf.DataGrid;
using WMComposite.Regions;

namespace WpfFront.Presenters
{


    public interface IKitAssemblyV2Presenter
    {
        IKitAssemblyV2View View { get; set; }
        void SetDocument(Document document);
        ToolWindow Window { get; set; }
    }



    public class KitAssemblyV2Presenter : IKitAssemblyV2Presenter
    {
        private readonly IUnityContainer container;
        private WMSServiceClient service;
        private readonly IShellPresenter region;
        private bool showNextTime = true;
        ProcessWindow pw = null;
        public ToolWindow Window { get; set; }
        //private IList<ProductStock> curBinStock { get; set; }



        public KitAssemblyV2Presenter(IUnityContainer container, IKitAssemblyV2View view, IShellPresenter region)
        {
            try
            {
                View = view;
                this.container = container;
                this.service = new WMSServiceClient();
                this.region = region;
                View.Model = this.container.Resolve<KitAssemblyV2Model>();

                //Event Delegate
                View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
                View.LoadDetails += new EventHandler<DataEventArgs<Document>>(this.OnLoadDetails);
                View.LoadUnits += new EventHandler<DataEventArgs<Product>>(this.OnLoadUnits);
                View.PickComponent += new EventHandler<EventArgs>(this.OnPickComponent);
                View.ReceiveLabel += new EventHandler<DataEventArgs<string>>(this.OnReceivingLabel);
                View.ReceiveLabelList += new EventHandler<EventArgs>(this.OnReceivingLabelList);
                View.PickAtOnce += new EventHandler<EventArgs>(this.OnPickAtOnce);
                //View.LoadProducts += new EventHandler<DataEventArgs<string>>(this.OnLoadProducts);
                View.ChangeStatus += new EventHandler<EventArgs>(this.OnChangeStatus);
                //View.ReceiveLabelTrackOption += new EventHandler<EventArgs>(this.OnReceivingLabelTrackOption);
                //View.LoadProductManualTrackOption += new EventHandler<EventArgs>(this.OnLoadProductManualTrackOption);
                //View.AddManualTrackToList += new EventHandler<EventArgs>(this.OnAddManualTrackToList);
                //View.ReceiveManualTrack += new EventHandler<EventArgs>(this.OnReceiveManualTrack);
                View.SelectedUnit += new EventHandler<EventArgs>(this.OnSelectUnit);
                //View.RemoveManualTrack += new EventHandler<EventArgs>(this.OnRemoveManualTrack);
                View.ShowReceivingTicket += new EventHandler<EventArgs>(this.OnShowReceivingTicket);
                View.PrintLabels += new EventHandler<EventArgs>(this.OnPrintLabels);
                //View.LoadBins += new EventHandler<DataEventArgs<string>>(OnLoadBins);
                View.RemoveFromNode += new EventHandler<DataEventArgs<DocumentBalance>>(OnRemoveFromNode);
                View.RefreshBin +=new EventHandler<EventArgs>(OnRefreshBin);
                View.LateDocuments += new EventHandler<DataEventArgs<bool?>>(this.OnLateDocuments);
                View.NewDocument += new EventHandler<EventArgs>(this.OnNewDocument);

                View.ConfirmOrder += new EventHandler<EventArgs>(this.OnConfirmOrder);

                View.Model.Document = new Document();
                View.Model.AllPicked = true;
                View.Model.PrinterList = App.printerList;
                View.Model.PrinterList.Add(new Printer { PrinterName = WmsSetupValues.DEFAULT });

                //Set Node to Stored Node. (el producto pasa directo por no estar conectado a un ERP)
                View.Model.Node = service.GetNode(new Node { NodeID = NodeType.Process }).First();

                //Cargue de Documentos
                LoadDocuments();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error cargando vista.\n" + ex.Message);
            }

        }

        private void OnNewDocument(object sender, EventArgs e)
        {
            ResetDetails();
            View.Model.Document = new Document();
            View.ComboStatus.SelectedIndex = 0;
            View.TabDocDetails.Visibility = Visibility.Visible;
        }


        public IKitAssemblyV2View View { get; set; }
        private DocumentType docType;
        private DateTime refDate;



        private void LoadDocuments()
        {
            try
            {
                ProcessWindow("Cargando Documentos ...", false);

                
                View.DgDocument.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 330;


                docType = new DocumentType { DocTypeID = SDocType.KitAssemblyTask };
                refDate = DateTime.Now;

                //Load the Base Documents (Pendig pero sin fecha de referencia)
                View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, 
                    Company = App.curCompany, Location = App.curLocation  }, 0, 0)
                    .OrderByDescending(f => f.Date1).ToList();

                //LoadStatus 
                View.Model.DocStatus = App.DocStatusList; //service.GetStatus(new Status { StatusType = new StatusType { StatusTypeID = SStatusType.Document } });

                View.ProcessResult.Text = "";
                //View.ExpManual.IsExpanded = false;

                pw.Close();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }
        }


        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                ProcessWindow("Buscando ...", false);

                if (string.IsNullOrEmpty(e.Value))
                {
                    pw.Close();
                    LoadDocuments();
                    return;
                }

                View.Model.DocumentList = service.SearchDocument(e.Value, new DocumentType { DocTypeID = SDocType.KitAssemblyTask });

                //si encuentra un resultado lo carga
                if (View.Model.DocumentList != null && View.Model.DocumentList.Count == 1)
                {
                    View.DgDocument.SelectedIndex = 0;
                    LoadDetails(View.Model.DocumentList[0]);
                }
                else
                    ResetDetails();

                pw.Close();
            }
            catch { pw.Close(); }
        }



        private void OnLateDocuments(object sender, DataEventArgs<bool?> e)
        {
            ProcessWindow("Cargando Documentos ...", false);

            if (e.Value == true)
                //Pending with reference Date = NOW
                View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany }, 0, 0)
                    .Where(f => f.Date1 <= refDate).OrderByDescending(f => f.Date1).ToList();
            else
                View.Model.DocumentList = service.GetPendingDocument(
                    new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation }, 0, 0)
                    .OrderByDescending(f => f.Date1).ToList();

            View.DgDocument.Items.Refresh();

            pw.Close();
        }


        private void ResetDetails()
        {
            View.Model.DocumentData = null;
            View.Model.DocumentLines = null;
            View.Model.DocProducts = null;
            View.Model.VendorItem = "";
            View.Model.LabelsAvailable = null;
            View.Model.DocumentBalance = null;
            View.TabDocDetails.Visibility = Visibility.Hidden;
            View.DatosDocumento.IsEnabled = true;
            View.KitAssemblyProduct.Product = null;
            View.KitAssemblyProduct.Text = "";
            View.KitAssemblyQuantity.Text = "0";
            View.TabItemReceive.IsEnabled = false;
            View.TabSerialesUtilizados.IsEnabled = false;
        }


        private void OnLoadDetails(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            if (View.Model.Document != null && View.Model.Document.DocID == e.Value.DocID)
                return;

            PreLoadDocument(e.Value);
        }

        private void PreLoadDocument(Document e)
        {
            if (e == null)
                return;

            try
            {
                ProcessWindow("Cargando Documento " + e.DocNumber + " ...", false);
                LoadDetails(e);
                pw.Close();

                //Mesaje de que todo fue piqueado
                //if (View.Model.AllPicked == true && View.Model.Document.IsFromErp == true)
                //Util.ShowMessage("This document does not have quantities pending to pick.");

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Documento no pudo ser cargado.\n" + ex.Message);
            }
        }


        private void LoadDetails(Document document)
        {

            View.BinLocation.Text = "";
            //View.CboBinList.Visibility = Visibility.Collapsed;
            View.Model.BinList = null;

            View.TxtProduct.Text = "";
            View.TxtProduct.Product = null;
            View.TxtProduct.DataList = null;
            View.TxtProduct.DefaultList = null;
            View.TxtProduct.ProductDesc = "";

            //Actualizando datos del Documento requeridos para posibles transacciones
            document.ModifiedBy = View.Model.User.UserName;

            View.Model.PackingUnits = null;
            View.Model.PackUnit = null;
            View.Model.CurQtyPending = 0;


            //No product Selected
            View.Model.Product = null;
            View.Model.VendorItem = "";

            View.Model.Document = document;

            View.ProcessResult.Text = "";

            View.TabDocDetails.Visibility = Visibility.Visible;

            View.Model.DocumentData = Util.ToShowData(document);


            //DIRECT PRINT 30oct2009
            //View.DirectPrint.Document = View.Model.Document;
            //if (View.Model.Document.DocStatus.StatusID == DocStatus.New)
                //View.DirectPrint.NewStatus = App.DocStatusList.Where(f => f.StatusID == DocStatus.InProcess).First();



            IList<DocumentLine> docLines = service.GetDocumentLine(new DocumentLine { Document = document });


            View.Model.DocumentLines = docLines.Where(f => f.LineNumber > 0).ToList();
            View.Model.KitProduct = docLines.Where(f => f.LineNumber == 0).Select(f => f.Product).First();


            View.ExpDocLines.IsExpanded = true;
            View.BtnRecTkt.Visibility = Visibility.Visible;

            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
            {
                View.ExpDocLines.IsExpanded = false;
                //Receiving Ticket
                View.BtnRecTkt.Visibility = Visibility.Collapsed;
            }

            //Select Status
            //View.ComboStatus.SelectedValue = View.Model.Document.DocStatus.StatusID;

            RefreshBalance(document);


            //Cargue del tab de recibo, solo si el documento es new or in process   
            View.TabItemReceive.Visibility = Visibility.Collapsed;



            if (document.DocStatus.StatusID == DocStatus.InProcess || document.DocStatus.StatusID == DocStatus.New || document.DocStatus.StatusID == DocStatus.Completed)
            {
                View.TabItemReceive.Visibility = Visibility.Visible;

                View.Model.VendorItem = "";
                View.Model.DocProducts = View.Model.DocumentLines
                          .Where(f => f.LineStatus.StatusID != DocStatus.Cancelled && f.LineNumber > 0 && f.Quantity > 0)
                          .Select(f => f.Product).Distinct().Where(f => f.Status.StatusID == EntityStatus.Active)
                          .ToList();


                //View.TxtProduct.DefaultList = View.Model.DocProducts;
                //Update The Default List
                View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0).Select(f => f.Product).ToList();


                //si encuentra un resultado lo carga
                //SelectOneProduct();

                //Solo muestra los labels si hay saldos en el documento
                if (View.Model.AllPicked == false)
                    ShowLabelsAvailable(document);

                View.BtnReceiveLabel.Visibility = Visibility.Collapsed;

                if (View.Model.LabelsAvailable != null && View.Model.LabelsAvailable.Count > 0)
                    View.BtnReceiveLabel.Visibility = Visibility.Visible;

                //Reset the manual receiving
                View.Model.ReceivingQuantity = 0;
            }


            //Getting the product bin Stock (For all product in the document)
            //curBinStock = service.GetDocumentStock(View.Model.Document, false);

            //Hidden Tracking y Others
            View.TabItemTrackOption.Visibility = Visibility.Collapsed;



            if (View.TabItemReceive.Visibility != Visibility.Visible && View.TabDocDetails.SelectedIndex > 0)
                View.TabDocDetails.SelectedIndex = 0;


            View.BtnConfirmOrder.Visibility = Visibility.Visible;
            if (View.Model.Document.DocStatus.StatusID == DocStatus.Completed || View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
                View.BtnConfirmOrder.Visibility = Visibility.Collapsed;

            //Los campos de producto y cantidad son bloqueados para no dejar cambiar
            View.DatosDocumento.IsEnabled = false;
            //Habilito el tab de kitting y tab de seriales utilzados
            View.TabItemReceive.IsEnabled = true;
            View.TabSerialesUtilizados.IsEnabled = true;
            //Asigno los datos de producto y cantidad
            View.KitAssemblyProduct.Product = View.Model.KitProduct;
            View.KitAssemblyProduct.Text = View.Model.KitProduct.Name;
            View.KitAssemblyQuantity.Text = docLines.Where(f => f.LineNumber == 0).First().Quantity.ToString();
            //Cargo los seriales utilizados en el documento
            View.Model.SerialesUtilizados = service.GetLabel(new Label { ReceivingDocument = View.Model.Document });

        }


        private void SelectOneProduct()
        {
            if (View.Model.DocProducts != null && View.Model.DocProducts.Count == 1)
            {
                //View.ComboProduct.SelectedIndex = 0;
                LoadUnits(View.TxtProduct.Product);
                View.ComboUnit.Focus();
            }
        }


        private void ShowLabelsAvailable(Document document)
        {

            Label searchLabel = new Label { Printed = true,  Node = new Node { NodeID = NodeType.Stored } };

            // Mustra posibles labels si es fromERP, si no muestra todo lo disponible en node PreLabeled
            //if (document.IsFromErp == true)
                View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(document, searchLabel);
            //else
                //View.Model.LabelsAvailable = service.GetDocumentLabelAvailable(new Document { Location = App.curLocation }, searchLabel);
        }


        private void OnLoadUnits(object sender, DataEventArgs<Product> e)
        {
            try
            {
                if (e.Value != null)
                    LoadUnits(e.Value);
                else
                    View.Model.ProductUnits = null;

            }
            catch { }
        }


        private void LoadUnits(Product product)
        {
            View.ComboUnit.Focus();
            View.Model.Product = product;

            //Item Number
            IList<ProductAccountRelation> vendItem = product.ProductAccounts.Where(f => f.Account.AccountID == View.Model.Document.Vendor.AccountID).ToList();
            if (vendItem != null && vendItem.Count() > 0)
                View.Model.VendorItem = "Vendor Item Number: " + vendItem[0].ItemNumber;

            View.BtnReceive.Visibility = Visibility.Visible;

            if (View.Model.Document.IsFromErp == true)
            {
                //Debe traer solo la unidad requerida por el documento para ese producto
                View.Model.ProductUnits = View.Model.DocumentLines.Where(f => f.Product.ProductID == product.ProductID)
                        .Select(f => f.Unit).Distinct().ToList();

                //El pack puede ser cualquier unidad del producto
                View.Model.PackingUnits = product.ProductUnits.Select(f => f.Unit).ToList();

            }
            else
                View.Model.PackingUnits = View.Model.ProductUnits = product.ProductUnits.Select(f => f.Unit).ToList();




            if (View.Model.ProductUnits != null && View.Model.ProductUnits.Count == 1)
            {
                View.ComboUnit.SelectedIndex = 0;
                View.TxtRcvQty.Focus();
                SelectUnit();
            }


            //Show Product Stock
            View.LvStock.Visibility = Visibility.Collapsed;


            //Obteniendo el Stock del Producto
            View.Model.ProductBinStock = service.GetProductStock(
                new ProductStock
                {
                    Product = View.Model.Product,
                    Bin = new Bin { Location = App.curLocation }
                },
              View.Model.Document.PickMethod);

            if (View.Model.ProductBinStock != null && View.Model.ProductBinStock.Count > 0)
                View.LvStock.Visibility = Visibility.Visible;



            //Asignacion al producto del modelo
            //View.Model.Product = product;

            //Si Tiene trackOption se muestran 
            /*
            View.TxtProductTrackMsg.Visibility = View.BtnTrack.Visibility = Visibility.Collapsed;

            if (product.ProductTrack != null && product.ProductTrack.Count > 0)
            {
                View.TxtProductTrackMsg.Visibility = View.BtnTrack.Visibility = Visibility.Visible;

                if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
                    View.BtnReceive.Visibility = Visibility.Collapsed;
            }
            */

        }


        private void OnSelectUnit(object sender, EventArgs e)
        {
            SelectUnit();
        }


        private void SelectUnit()
        {

            if (View.ComboUnit.SelectedItem == null)
                return;

            //Remover la Unidad que no pueda contener a la otra del Packing List
            IList<Unit> curPack = View.Model.Product.ProductUnits.Select(f => f.Unit).ToList();
            View.Model.PackingUnits = curPack;
            Unit[] unitList = new Unit[curPack.Count];
            curPack.CopyTo(unitList, 0);

            foreach (Unit unit in unitList)
                if (((Unit)View.ComboUnit.SelectedItem).BaseAmount >= unit.BaseAmount)
                    View.Model.PackingUnits.Remove(unit);



            //Poner el Qty Pending para esa unidad
            try
            {
                View.Model.CurQtyPending = int.Parse(View.Model.DocumentBalance
                    .Where(f => f.Product.ProductID == View.Model.Product.ProductID && f.Unit.UnitID == ((Unit)View.ComboUnit.SelectedItem).UnitID)
                    .First().QtyPending.ToString());
            }
            catch { View.Model.CurQtyPending = 0; }

            View.Model.AllowReceive = true;
            if (View.Model.Document.IsFromErp == true)
                View.Model.AllowReceive = (View.Model.CurQtyPending > 0) ? true : false;


        }


        private void OnPickComponent(object sender, EventArgs e)
        {
            try
            {

                View.ProcessResult.Text = "";

                Label sourceLocation = CheckDestination(View.BinLocation.Text);

                if (sourceLocation == null)
                {
                    Util.ShowError("Bin source " + View.BinLocation.Text + " is not valid.");
                    return;
                }

                //Validating Product
                if (View.TxtProduct.Product == null)
                {
                    Util.ShowError("Producto no seleccionado.");
                    return;
                }

                //Validating Unit
                if (View.ComboUnit.SelectedIndex == -1)
                {
                    Util.ShowError("Unidad no seleccionada.");
                    return;
                }

                View.Model.CheckAllRules();
                if (!View.Model.IsValid())
                {
                    Util.ShowError("Error validando datos. por favor revise y trate nuevamente.");
                    return;
                }

                if (!(int.Parse(View.TxtRcvQty.Text) > 0))
                {
                    Util.ShowError("Cantidad no valida.");
                    return;
                }

                ProcessWindow("Alistando Componente ...", false);
                ProcessPickingComponent(sourceLocation, int.Parse(View.TxtRcvQty.Text));
                pw.Close();

            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
                pw.Close();
            }
        }


        private void ProcessPickingComponent(Label sourceLocation, int quantiy)
        {
            //Define Document, Product, Unit and Qty to send to receiving transaction
            DocumentLine line = new DocumentLine
            {
                Document = View.Model.Document,
                Product = View.TxtProduct.Product,
                Unit = (Unit)View.ComboUnit.SelectedItem,
                Quantity = quantiy,
                CreatedBy = App.curUser.UserName
            };

            //Lamado al Servicio
            service.PickProduct(line, sourceLocation, View.Model.Node, null);

            //Refresh el Balance
            RefreshBalance(line.Document);

            //After Process
            //if (View.Model.Document.IsFromErp == true)
            //    View.ComboProduct.SelectedIndex = -1;
            //else
            //{
            //    View.Model.DocProducts = null;
            //    View.ComboProduct.Items.Refresh();
            //}

            View.TxtProduct.Text = "";
            View.TxtProduct.Product = null;
            View.TxtProduct.ProductDesc = "";

            View.Model.ProductUnits = null; //View.Model.PackingUnits =
            View.ComboUnit.Items.Refresh();
            View.TxtRcvQty.Text = View.TxtProduct.Text = "";            

            //Update The Default List
            View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0).Select(f => f.Product).ToList();

            View.ProcessResult.Text = "Component Alistado.";

            //Util.ShowMessage(View.ProcessResult.Text);
            if (showNextTime)
            {
                pw.Close();
                showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
            }
        }




        private Label CheckDestination(string binLocation)
        {

            View.BinLocation.Text = View.BinLocation.Text.ToUpper();

            //Checking if valid source location
            if (string.IsNullOrEmpty(View.BinLocation.Text.Trim()))
                return null;


            try
            { return service.GetLocationData(View.BinLocation.Text, false); }
            catch
            { return null; }
        }



        private void OnReceivingLabel(object sender, DataEventArgs<string> e)
        {


            View.ProcessResult.Text = "";

            //Ocultar el Tracking Options
            View.Model.CurScanedLabel = null;


            if (string.IsNullOrEmpty(e.Value.Trim()))
                return;


            Label label = new Label { LabelCode = e.Value };

            ProcessWindow("Alistando Etiqueta ...", false);


            //Trae el label
            try
            {
                label = service.GetLabel(label).First();
                View.Model.CurScanedLabel = label;
            }
            catch
            {
                pw.Close();
                Util.ShowError("Etiqueta " + label.LabelCode + " no existe.");
                return;
            }


            try
            {
                service.PickLabel(View.Model.Document, label, View.Model.Node, null);

                //Label borra si esta enum label lista de desplegadas
                try
                {
                    IList<Label> labelToRemove = View.Model.LabelsAvailable.Where(f => f.LabelID == label.LabelID).ToList();
                    if (labelToRemove.Count > 0)
                    {
                        View.Model.LabelsAvailable.Remove(labelToRemove.First());
                        View.LabelListAvailable.Items.Refresh();
                    }
                }
                catch { }

                //Post - Process
                //Refresh el Balance
                RefreshBalance(View.Model.Document);
                View.TxtScanLabel.Text = "";
                View.ProcessResult.Text = "Etiqueta Alistada.";


                //Util.ShowMessage(View.ProcessResult.Text);
                pw.Close();
                if (showNextTime)
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);

                return;
            }

            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
                return;
            }


            /*
            View.ProcessResult.Text = "";

            //Ocultar el Tracking Options
            View.StkLabelTrack.Visibility = Visibility.Collapsed;
            View.Model.CurScanedLabel = null;


            if (string.IsNullOrEmpty(e.Value.Trim()))
                return;


            Label label = new Label { LabelCode = e.Value };

            //Trae el label
            try
            {

                IList<Label> labelList = service.GetLabel(label);

                //Revisa que solo un label sea entregado.
                if (labelList != null && labelList.Count > 1)
                {
                    Util.ShowError("Label " + label.LabelCode + " exists more than once.\nPlease check it.");
                    return;
                }

                label = labelList.First();
                View.Model.CurScanedLabel = label;

            }
            catch
            {
                Util.ShowError("Label " + e.Value + " does not exists.");
                return;
            }


            //Revisar las track Options
            if (label.Product.ProductTrack != null && label.Product.ProductTrack.Count > 0)
            {
                View.StkLabelTrack.Visibility = Visibility.Visible;

                //A temp value le asigna el valor "" para que se deje editar
                for (int i = 0; i < label.Product.ProductTrack.Count; i++)
                    label.Product.ProductTrack[i].TempValue = "";

                //Lenar el Grid que contiene el Track
                View.Model.TrackData = label.Product.ProductTrack;

                return;
            }


            ProcessReceiveLabel(label);
             * */


        }


        //Recibe Varios lables de los disponibles
        private void OnReceivingLabelList(object sender, EventArgs e)
        {

            if (!(View.LabelListAvailable.SelectedItems != null && View.LabelListAvailable.SelectedItems.Count > 0))
            {
                Util.ShowError("No hay etiquetas seleccionadas.");
                return;
            }

            try
            {


                View.ProcessResult.Text = "";

                ProcessWindow("Alistando  Etiquetas ...", false);

                //recorre la lista para enviarla 
                foreach (object label in View.LabelListAvailable.SelectedItems)
                {
                    service.PickLabel(View.Model.Document, (Label)label, View.Model.Node, null);
                    View.Model.LabelsAvailable.Remove((Label)label);
                }

                //Update Process Result
                View.LabelListAvailable.Items.Refresh();
                ShowLabelsAvailable(View.Model.Document);

                //Refresh el Balancee
                RefreshBalance(View.Model.Document);
                View.ProcessResult.Text = "Etiquetas(s) Alistadas.";

                pw.Close();
                Util.ShowMessage(View.ProcessResult.Text);

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }

            /*
            if (!(View.LabelListAvailable.SelectedItems != null
                && View.LabelListAvailable.SelectedItems.Count > 0))
            {
                Util.ShowError("No label selected.");
                return;
            }

            try
            {

                //Gettting Bin Location
                Bin destLocation = service.GetBinLocation(View.BinLocation.Text, false);

                if (destLocation == null)
                {
                    Util.ShowError("Bin destination " + View.BinLocation.Text + " is not valid.");
                    return;
                }


                View.ProcessResult.Text = "";

                ProcessWindow("Receiving Labels ...", false);

                //recorre la lista para enviarla 
                foreach (object label in View.LabelListAvailable.SelectedItems)
                {
                    service.ReceiveLabel(View.Model.Document, (Label)label, destLocation, View.Model.Node);
                    View.Model.LabelsAvailable.Remove((Label)label);
                }

                //Update Process Result
                View.LabelListAvailable.Items.Refresh();
                ShowLabelsAvailable(View.Model.Document);

                //Refresh el Balancee
                RefreshBalance(View.Model.Document);
                View.ProcessResult.Text = "Label(s) Received.";


                pw.Close();
                Util.ShowMessage(View.ProcessResult.Text);



            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
            }
             * */
        }



        public void RefreshBalance(Document document)
        {
            DocumentBalance docBalance = new DocumentBalance
            {
                Document = document,
                Node = View.Model.Node,
                Location = App.curLocation
            };

            //#########Receiving Balance

            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
            {
                View.Model.DocumentBalance = service.GetDocumentBalanceForEmpty(docBalance);
            }
            else
            {
                View.Model.DocumentBalance = service.GetDocumentBalance(docBalance, false);

                View.Model.DocumentBalance = (from balance in View.Model.DocumentBalance
                                              join lines in View.Model.DocumentLines.Where(f => f.Note == "1" || (f.Quantity > 0 && f.LineNumber != 0))
                                              on balance.Product.ProductID equals lines.Product.ProductID
                                              select balance).ToList();
            }

            View.DgDocumentBalance.Items.Refresh();

            //El boton de Recibir todo se muestra si hay balance
            View.BtnReceiveAtOnce.IsEnabled = false;
            //View.Model.AllPicked = true;
            View.Model.AllPicked = false;
            /*if (View.Model.DocumentBalance.Any(f => f.QtyPending > 0))
            {
                View.BtnReceiveAtOnce.IsEnabled = true;
                View.Model.AllPicked = false;
            }*/
            if (View.Model.DocumentBalance.Any(f => f.Product.ErpTrackOpt == 1 && f.QtyPending == 0))
            {
                View.BtnReceiveAtOnce.IsEnabled = true;
                View.Model.AllPicked = true;
            }


            //##########Posting Balance

            View.Model.PendingToPostList = service.GetDocumentPostingBalance(docBalance);


            //El boton de Posting todo se muestra si hay balance
            //View.BtnConfirmOrder.IsEnabled = false;
            //if (View.Model.AllPicked)
            //{
            //    View.BtnConfirmOrder.IsEnabled = true;
            //}




        }



        //Used for external methods
        public void RefreshProductList()
        {
            //Update The Default List
            View.TxtProduct.DefaultList = null;
            if (View.Model.Document.IsFromErp == true)
                View.TxtProduct.DefaultList = View.Model.DocumentBalance.Where(f => f.QtyPending > 0)
                    .Select(f => f.Product).ToList();
        }



        private void OnPickAtOnce(object sender, EventArgs e)
        {

            try
            {
                //Gettting Bin Location
                Label sourceLocation = CheckDestination(View.BinLocation.Text);

                if (sourceLocation == null)
                {
                    Util.ShowError("Bin origen " + View.BinLocation.Text + " no es valido.");
                    return;
                }


                ProcessWindow("Alistando Componentes ...", false);

                View.ProcessResult.Text = "";
                service.PickAtOnce(View.Model.Document, sourceLocation, View.Model.Node);
                RefreshBalance(View.Model.Document);

                View.ProcessResult.Text = "Documento " + View.Model.Document.DocNumber + " Alistado completamante. Popr favor revise el balance.";
                Util.ShowMessage(View.ProcessResult.Text);

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Documento puede ser alistado completamente. \n" + ex.Message);
            }
        }


        //private void OnLoadProducts(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.VendorItem = "";
        //    View.Model.DocProducts = service.SearchProduct(e.Value, View.Model.Document.Vendor);
        //    SelectOneProduct();
        //}



        private void OnChangeStatus(object sender, EventArgs e)
        {
            try
            {
                bool IsNew = true;

                if (View.Model.Document.DocID != 0)
                    IsNew = false;

                //Evaluo si fue seleccionado un producto para kit y si se escribio una cantidad
                if (View.KitAssemblyProduct.Product == null)
                {
                    Util.ShowError("Por favor seleccionar un kit");
                    return;
                }
                if (View.KitAssemblyQuantity.Text == "")
                {
                    Util.ShowError("Por favor indicar la cantidad de kits a crear");
                    return;
                }

                //Evaluo si el producto seleccionado es un kit
                if (service.GetKitAssembly(new KitAssembly { Product = View.KitAssemblyProduct.Product }, 1).Count() == 0)
                {
                    Util.ShowError("El producto elejido no es un kit.");
                    return;
                }

                if (IsNew)
                {
                    View.Model.Document.Location = App.curLocation;
                    View.Model.Document.DocType = new DocumentType { DocTypeID = SDocType.KitAssemblyTask };
                    View.Model.Document.IsFromErp = true;
                    View.Model.Document.CrossDocking = false;
                    View.Model.Document.Vendor = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                    View.Model.Document.Customer = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                    View.Model.Document.Date1 = DateTime.Now;
                    View.Model.Document.Company = new Company { CompanyID = App.curCompany.CompanyID };
                    View.Model.Document.CreatedBy = App.curUser.UserName;
                    View.Model.Document.CreationDate = DateTime.Now;
                    View.Model.Document = service.CreateNewDocument(View.Model.Document, true);
                    View.Model.DocumentLines = service.CreateAssemblyOrderLines(View.Model.Document, View.KitAssemblyProduct.Product, Double.Parse(View.KitAssemblyQuantity.Text));
                    
                    LoadDocuments();

                    //LoadDetails(View.Model.Document);

                    PreLoadDocument(View.Model.Document);

                    Util.ShowMessage("Documento Guardado");

                    //OnLoadDetails(sender,new DataEventArgs<Document>(View.Model.Document));
                    //View.TabDocDetails.Visibility = Visibility.Collapsed;
                   // View.DgDocument.SelectedIndex = -1;
                }
                else
                {
                    View.Model.Document.ModifiedBy = App.curUser.UserName;
                    View.Model.Document.ModDate = DateTime.Now;
                    service.UpdateDocument(View.Model.Document);
                    Util.ShowMessage("Documento Actualizado");
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Hubo un error durante el proceso. \n" + ex.Message);
            }
        }


        //private void OnReceivingLabelTrackOption(object sender, EventArgs e)
        //{
        //    View.ProcessResult.Text = "";

        //    try
        //    {

        //        //Asigna los valores ingresados de las TrackOption al correspodiente campo del label
        //        foreach (ProductTrackRelation trackRel in View.Model.TrackData)
        //        {
        //            View.Model.CurScanedLabel.GetType().GetProperty(trackRel.TrackOption.Name)
        //                .SetValue(View.Model.CurScanedLabel, trackRel.TempValue, null);
        //        }

        //        if (ProcessReceiveLabel(View.Model.CurScanedLabel))
        //            View.StkLabelTrack.Visibility = Visibility.Collapsed;

        //    }
        //    catch (Exception ex)
        //    {
        //        pw.Close();
        //        Util.ShowError(ex.Message);
        //    }

        //}

        /*
        private void ProcessReceiveLabel(Label label)
        {




            //Gettting Bin Location
            Bin destLocation = service.GetBinLocation(View.BinLocation.Text, false);

            if (destLocation == null)
            {
                Util.ShowError("Bin destination " + View.BinLocation.Text + " is not valid.");
                return false;
            }

            try
            {
                ProcessWindow("Picking Label ...", false);

                service.ReceiveLabel(View.Model.Document, label, destLocation, View.Model.Node);

                //Label borra si esta enum label lista de desplegadas
                IList<Label> labelToRemove = View.Model.LabelsAvailable.Where(f => f.LabelID == label.LabelID).ToList();
                if (labelToRemove.Count > 0)
                {
                    View.Model.LabelsAvailable.Remove(labelToRemove.First());
                    View.LabelListAvailable.Items.Refresh();
                }

                //Post - Process
                //Refresh el Balance
                RefreshBalance(View.Model.Document);
                View.TxtRecLabel.Text = "";
                View.ProcessResult.Text = "Label Picked.";

                if (showNextTime)
                    showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);

                //Util.ShowMessage(View.ProcessResult.Text);
                pw.Close();
                return true;
            }

            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(ex.Message);
                return false;
            }
             
        }
         */
/*

        private void OnLoadProductManualTrackOption(object sender, EventArgs e)
        {

            if (View.ComboUnit.SelectedIndex == -1)
            {
                Util.ShowError("Plase select a unit.");
                return;
            }

            int qty;
            if (!int.TryParse(View.TxtRcvQty.Text, out qty) || int.Parse(View.TxtRcvQty.Text) <= 0)
            {
                Util.ShowError("Plase enter a valid quantity.");
                return;
            }

            //Gettting Bin Location
            View.Model.BinLabel = CheckDestination(View.BinLocation.Text);

            if (View.Model.BinLabel == null)
            {
                Util.ShowError("Bin/Label destination " + View.BinLocation.Text + " is not valid.");
                return;
            }

            //loading track Options
            View.Model.TrackData = View.Model.Product.ProductTrack;
            View.TabItemTrackOption.Visibility = Visibility.Visible;
            View.TabItemTrackOption.Focus();


            if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
                View.BtnTrackReceive.IsEnabled = false;


            //Load Label List Columns
            IList<ProductTrackRelation> listTrack;



            System.Windows.Controls.GridViewColumn[] grid =
                new System.Windows.Controls.GridViewColumn[((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Count];
            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.CopyTo(grid, 0);

            //Arma le grid que pedira los datos de Tracking
            foreach (System.Windows.Controls.GridViewColumn col in grid)
            {
                listTrack = View.Model.TrackData.Where(f => f.TrackOption.Name == col.Header.ToString()).ToList();

                //si es una opcion de tracking y no tiene elementos la elimina del grid
                if (col.Header.ToString().Contains("Track") && !(listTrack.Count > 0))
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Remove(col);

                //Si tiene elementos le pone el nombre en el header
                else if (col.Header.ToString().Contains("Track"))
                {  //Cambia el Header  y el with
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Where(f => f.Header == col.Header)
                        .First().Header = listTrack.First().TrackOption.DisplayName;

                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Where(f => f.Header == col.Header)
                        .First().Width = 130;
                }
            }


            //Detentacto si hay alguna track unique que restrinja el maximo qty a entrar en 1 each.
            for (int i = 0; i < View.Model.TrackData.Count; i++)
                if (View.Model.TrackData[i].IsUnique == true)
                {
                    View.Model.MaxTrackQty = 1;
                    break;
                }


            Unit selectedUnit = (Unit)View.ComboUnit.SelectedItem;

            //Cantidad pendiente por poner el tracking
            if (View.Model.MaxTrackQty == 1)
            { //Si es unique debe pedir 1 a 1 hasta todas las unidades basicas
                View.Model.RemainingQty = Int32.Parse(View.TxtRcvQty.Text) * (Int32)selectedUnit.BaseAmount;
                View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
                View.Model.TrackUnit = View.Model.Product.BaseUnit;
                View.TxtQtyTrack.IsEnabled = false;
            }
            else
            {
                View.Model.RemainingQty = Int32.Parse(View.TxtRcvQty.Text);
                View.TxtQtyTrack.Text = View.TxtRcvQty.Text;
                View.Model.TrackUnit = selectedUnit;
                View.TxtQtyTrack.IsEnabled = true;
            }


            //Inicia con el total de Qty
            View.Model.ManualTrackList = new List<Label>();
            View.BtnAddTrack.IsEnabled = true;

        }


        private void OnAddManualTrackToList(object sender, EventArgs e)
        {

            //Check If Qty is Valid
            int qty;
            if (!int.TryParse(View.TxtQtyTrack.Text, out qty) || int.Parse(View.TxtQtyTrack.Text) <= 0)
            {
                Util.ShowError("Plase enter a valid quantity.");
                return;
            }

            if (qty > View.Model.RemainingQty)
            {
                Util.ShowError("Qty Remaining is " + View.Model.RemainingQty.ToString() + " please fix.");
                return;
            }

            Label curLabel; //Usada para consultar y enviar la informacion del label actual

            //Revisa que hayan datos de track ingresados y sean validos
            foreach (ProductTrackRelation trackRel in View.Model.TrackData)
            {
                if (string.IsNullOrEmpty(trackRel.TempValue))
                {
                    Util.ShowError("Plase enter valid track information.");
                    return;
                }

                //Si en un unique data like a serial debe validar que no sea un dato existente en la DB
                //ni en los ingresado en la lista actual.
                if (trackRel.IsUnique == true)
                {
                    curLabel = new Label { Product = View.Model.Product, Status = new Status { StatusID = EntityStatus.Active } };
                    curLabel.GetType().GetProperty(trackRel.TrackOption.Name).SetValue(curLabel, trackRel.TempValue, null);

                    if (service.GetLabel(curLabel).Count > 0)
                    {
                        Util.ShowError("Product with the " + trackRel.TrackOption.DisplayName + " " + trackRel.TempValue + " already exists.");
                        return;
                    }


                    foreach (Label trkLabel in View.Model.ManualTrackList)

                        if (trkLabel.GetType().GetProperty(trackRel.TrackOption.Name).GetValue(trkLabel, null).ToString() == curLabel.GetType().GetProperty(trackRel.TrackOption.Name).GetValue(curLabel, null).ToString())
                        {
                            Util.ShowError("Product with the " + trackRel.TrackOption.DisplayName + " " + trackRel.TempValue + " already added.");
                            return;
                        }

                }

            }


            //Si es unique pasa la unidad basica a Each, Si no usa la que ingreso el user
            Unit selectedUnit = (View.Model.MaxTrackQty == 1) ? View.Model.Product.BaseUnit : (Unit)View.ComboUnit.SelectedItem;

            for (int i = 0; i < qty; i++)
            {

                curLabel = new Label
                {
                    Product = View.Model.Product,
                    Unit = View.Model.TrackUnit,
                    Bin = View.Model.BinLabel.Bin,
                    CurrQty = 1,
                    IsLogistic = false,
                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                    Node = new Node { NodeID = NodeType.PreLabeled },
                    Printed = false,
                    StartQty = 1,
                    //UnitBaseFactor = ((Unit)View.ComboUnit.SelectedItem).BaseAmount,
                    Status = new Status { StatusID = EntityStatus.Active },
                    Barcode = "",
                    Name = "",
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now
                };

                //Asigna los valores ingresados de las TrackOption al correspodiente campo del label
                foreach (ProductTrackRelation trackRel in View.Model.TrackData)
                    curLabel.GetType().GetProperty(trackRel.TrackOption.Name).SetValue(curLabel, trackRel.TempValue, null);

                View.Model.ManualTrackList.Add(curLabel);
            }

            View.ManualTrackList.Items.Refresh();
            View.Model.RemainingQty -= qty;


            //Ajustando las cantidades
            if (View.Model.MaxTrackQty == 1)
                View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
            else
                View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();


            //Limpiando los Values
            for (int i = 0; i < View.Model.TrackData.Count; i++)
                View.Model.TrackData[i].TempValue = "";

            if (View.Model.RemainingQty <= 0)
            {
                View.BtnAddTrack.IsEnabled = false;

                //Si el ERP requiere de manera obligatoria los tracks habilita el boton
                if (Util.GetConfigOption("RCERPTRACK").Equals("T"))
                    View.BtnTrackReceive.IsEnabled = true;

            }


            //Pone el Focus en el elemento del Grid para entrar el siguiente dato
            View.LvTrackProduct.Focus();

        }


        //Envia los Labels (por receiveLabel) y el producto pendiente (recevieProduct) si remainin es > 0
        private void OnReceiveManualTrack(object sender, EventArgs e)
        {
            if (View.Model.ManualTrackList == null || View.Model.ManualTrackList.Count == 0)
                return;

            try
            {
                ProcessWindow("Receiving Product ...", false);

                View.BtnTrackReceive.IsEnabled = false;

                Label curLabel;
                //Funcion para obtener siguiente Label
                DocumentTypeSequence initSequence = service.GetNextDocSequence(App.curCompany, View.Model.ManualTrackList[0].LabelType);

                //Lo que sea Label lo manda a Crear y luego manda el Label for Receiving Label
                foreach (Label label in View.Model.ManualTrackList)
                {
                    label.LabelCode = (initSequence.NumSequence++).ToString();
                    curLabel = service.SaveLabel(label); //Save label
                    service.ReceiveLabel(View.Model.Document, curLabel, View.Model.BinLabel.Bin, View.Model.Node); //Receive Label
                }

                initSequence.NumSequence++;
                service.UpdateDocumentTypeSequence(initSequence);

                //Lo remaining lo manda Normal
                if (View.Model.RemainingQty > 0)
                {
                    ProcessPickingComponent(View.Model.BinLabel, View.Model.RemainingQty);
                }
                else
                {
                    View.ProcessResult.Text = "Product Picked.";

                    if (showNextTime)
                        showNextTime = UtilWindow.ConfirmResult(View.ProcessResult.Text);
                    //Util.ShowMessage(View.ProcessResult.Text);
                }

                //Reset the Track
                View.Model.ManualTrackList = null;
                View.Model.TrackData = null;
                View.ManualTrackList.Items.Refresh();

                pw.Close();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error receiving. " + ex.Message);
                return;
            }

        }


        private void OnRemoveManualTrack(object sender, EventArgs e)
        {

            if (View.ManualTrackList.SelectedItems == null)
                return;


            foreach (object lineObject in View.ManualTrackList.SelectedItems)
            {
                View.Model.ManualTrackList.Remove((Label)lineObject);
                View.Model.RemainingQty++;
            }

            View.ManualTrackList.Items.Refresh();

            if (View.Model.MaxTrackQty > 1)
                View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();

            View.BtnAddTrack.IsEnabled = true;


            //Si el ERP requiere de manera obligatoria los tracks habilita el boton
            //if (App.IsConnectedToErpInventory)
                //View.BtnTrackReceive.IsEnabled = false;

        }

        */

        private void OnShowReceivingTicket(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow("Generando Documento ... ", false);
                UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.Document.DocID, "", false); //"PDF995"
                pw.Close();
            }
            catch { pw.Close(); }
        }





        private void OnRemoveFromNode(object sender, DataEventArgs<DocumentBalance> e)
        {
            if (View.Model.Document.DocStatus.StatusID == DocStatus.Completed)
                return;

            if (View.Model.Document.DocStatus.StatusID == DocStatus.Cancelled)
                return;

            if (View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
                return;

            if (e.Value == null)
                return;


            DocumentBalance balanceLine = e.Value;
            bool refreshBalance = ((DataGridControl)sender).Name == "dgDocumentBalance" ? true : false;

            if (((DataGridControl)sender).Name == "dgPostingBalance" && balanceLine.QtyPending.Equals(0))
            {
                Util.ShowMessage("None labels available.");
                return;
            }


            try
            {
                IRemoveNodePresenter presenter = container.Resolve<IRemoveNodePresenter>();
                presenter.ParamRecord(balanceLine, this, refreshBalance); //refresBalance=true hace que traiga de nuevo el balance


                View.UcReceivedBal.ShowViewInShell(presenter.View);
                View.PopupReceived.IsOpen = true;
                View.PopupReceived.StaysOpen = true;
            }
            catch (Exception ex)
            {
                View.PopupReceived.IsOpen = false;
                View.PopupReceived.StaysOpen = false;
                Util.ShowError(ex.Message);
            }
        }



        ////Febrero 23 2009 - Seleccion de Bins - JM
        //private void OnLoadBins(object sender, DataEventArgs<string> e)
        //{
        //    View.Model.BinList = service.SearchBin(e.Value);

        //    if (View.Model.BinList == null || View.Model.BinList.Count == 0)
        //        return;

        //    //Cargar la lista de Bins
        //    View.CboBinList.Visibility = Visibility.Visible;
        //    View.CboBinList.IsDropDownOpen = true;

        //    if (View.Model.BinList.Count == 1)
        //    {
        //        View.CboBinList.Visibility = Visibility.Collapsed;
        //        View.BinLocation.Text = View.Model.BinList[0].BinCode;
        //        View.BinLocation.Focus();
        //    }
        //}




        public void SetDocument(Document document)
        {
            //Load Document Calling Document come from other panel.
            LoadDocuments();
            LoadDetails(document);
        }


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore)
        {
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }



        private void OnConfirmOrder(object sender, EventArgs e)
        {
            try
            {
                ProcessWindow("Confirming Order " + View.Model.Document.DocNumber + " ... ", false);

                //Todo Process
                View.Model.Document.ModifiedBy = App.curUser.UserName;
                View.Model.Document.ModDate = DateTime.Now;
                View.Model.Document.Location = App.curLocation;


                View.Model.Document = service.ConfirmKitAssemblyOrder(View.Model.Document, App.curLocation);

                //Send to print Result Labels;
                PrintLabels();


                pw.Close();

                Util.ShowMessage("Orden Confirmada");

                View.BtnConfirmOrder.IsEnabled = false;
            }
            catch
            {
                Util.ShowMessage("Error confirmando el documento.");
                pw.Close();
            }
        }


        private void OnPrintLabels(object sender, EventArgs e)
        {
            PrintLabels();
        }


        private void PrintLabels()
        {
            //if (View.PrinterList.SelectedItem == null)
                //return;


            try
            {
                //string printerName = View.PrinterList.SelectedItem != null ? ((Printer)View.PrinterList.SelectedItem).PrinterName : "";


                NodeTrace patterTrace = new NodeTrace
                {
                    Document = View.Model.Document,
                    Node = new Node { NodeID = NodeType.Stored }, //View.Model.Node,
                    Status = new Status { StatusID = EntityStatus.Active },
                    Label = new Label
                    {
                        Node = new Node { NodeID = NodeType.Stored },
                        Product = View.Model.KitProduct
                    }
                };


                List<Label> labelsToPrint = service.GetNodeTrace(patterTrace)
                    .Select(f => f.Label).Where(f => f.FatherLabel == null && f.Product.PrintLabel == true).ToList();


                if (labelsToPrint == null || labelsToPrint.Count == 0)
                {
                    pw.Close();
                    return;
                }

                /*
                LabelTemplate defTemplate;
                try
                {
                    defTemplate = service.GetLabelTemplate(
                        new LabelTemplate { Header = WmsSetupValues.AssemblyLabelTemplate }).First();
                }
                catch { 
                    Util.ShowError("Label " + WmsSetupValues.AssemblyLabelTemplate + " does not exists, or no label definde for Kit/Assembly.");
                    return;
                }

                ReportMngr.PrintLabelsInBatch(defTemplate, (Printer)View.PrinterList.SelectedItem, labelsToPrint);
                ReportMngr.PrintLabelsInBatch(defTemplate, new Printer { PrinterName = WmsSetupValues.DEFAULT }, labelsToPrint);
                */

                //Usar el Facade para esto.

                
                //Definicion del Template a Imprimier
                string printTemplate;
                try
                {
                    printTemplate = View.Model.KitProduct.DefaultTemplate != null ? View.Model.KitProduct.DefaultTemplate.Header : WmsSetupValues.AssemblyLabelTemplate;
                }
                catch { printTemplate = WmsSetupValues.AssemblyLabelTemplate; }


                service.PrintLabelsFromDevice(WmsSetupValues.DEFAULT, printTemplate, labelsToPrint);

            }

            catch (Exception ex) { pw.Close();  Util.ShowError("Error imprimiendo. " + ex.Message); }

        }


        private void OnRefreshBin(object sender, EventArgs e)
        {
            ShowLabelsAvailable(View.Model.Document);

        }

    }

}