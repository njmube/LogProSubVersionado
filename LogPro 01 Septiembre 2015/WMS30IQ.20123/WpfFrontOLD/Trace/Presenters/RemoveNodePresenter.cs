using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace WpfFront.Presenters
{

    public interface IRemoveNodePresenter
    {
        IRemoveNodeView View { get; set; }
        void ParamRecord(DocumentBalance doc, Object parent, bool reload); //Receiving
        ToolWindow Window { get; set; }

    }
    

    public class RemoveNodePresenter : IRemoveNodePresenter
    {
        public IRemoveNodeView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }



        public RemoveNodePresenter(IUnityContainer container, IRemoveNodeView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            //this.region = region;
            View.Model = this.container.Resolve<RemoveNodeModel>();

            //Event Delegate
            View.RemovePrinted  += new EventHandler<EventArgs>(this.OnRemovePrinted);
            View.RemoveManual  += new EventHandler<EventArgs>(this.OnRemoveManual);
        }



        //Recibe el Balance que debe ser removido del nodo

        public void ParamRecord(DocumentBalance actualRecord, Object parent, bool reload)
        {
            try
            {
                //reload pregunta si debe irse a preguntar por el balance para ese producto nuevamente.
                if (reload)
                {
                    try
                    {
                        View.Model.Record = service.GetDocumentPostingBalance(actualRecord)
                            .Where(f => f.Product.ProductID == actualRecord.Product.ProductID)
                            .First();

                    }
                    catch
                    {
                        throw new Exception(Util.GetResourceLanguage("NO_QUANTITIES_AVAILABLE_TO_REVERSE"));                    
                    }
                }
                else
                {
                    View.Model.Record = actualRecord;
                }


                View.Model.ParentWindow = parent;
                LoadTransactions(View.Model.Record);
            }
            catch (Exception ex)
            {
                throw new Exception(Util.GetResourceLanguage("PROBLEM_LOADING_BALANCE") + "\n" + ex.Message);
            }

        }


        //Carga los movimientos a los que se hacen referencia
        private void LoadTransactions(DocumentBalance actualRecord)
        {
            //Solo Muestra Labesl con cantidades fijas a remover
            View.BrdManual.Visibility = Visibility.Collapsed;


            NodeTrace pattern = new NodeTrace
                    {                        
                        Node = actualRecord.Node,
                        Label = new WpfFront.WMSBusinessService.Label
                        {
                            Product = actualRecord.Product
                            //Printed = true,
                            //Unit = actualRecord.Unit
                        }
                    };

            //Cuando se desea cancelar la linea de un Shipment.
            //JM - Marzo 9 / 2010
            if (actualRecord.DocumentLine != null && actualRecord.DocumentLine.LineNumber > 0)
            {
                pattern.PostingDocLineNumber = actualRecord.DocumentLine.LineNumber;
                pattern.PostingDocument = actualRecord.Document;
            }
            else
            {
                pattern.PostingDocument = new Document { DocID = -1 };
                pattern.Document = actualRecord.Document;
            }


            // lista de labels printed
            View.Model.LstPrinted = service.GetNodeTrace(pattern).ToList();

            View.ListPrinted.Items.Refresh();

            // qtys manuales y printed
            View.Model.QtyPrintedOld = View.Model.LstPrinted.Sum(f => f.Quantity);//Sum(f=>f.Label.CurrQty);
            View.Model.QtyManualOld = int.Parse(actualRecord.QtyPending.ToString()) - View.Model.LstPrinted.Count();

            // solo si hay disponibilidad se habilitan las tablas
            //if (View.Model.QtyManualOld == 0)
                //View.BrdManual.Visibility = Visibility.Collapsed;

            if (View.Model.QtyPrintedOld == 0)
                View.BrdPrinted.Visibility = Visibility.Collapsed;


            //Si el documento es de receiving oculta el Bin de restock
            if (View.Model.Record.Document.DocType.DocClass.DocClassID == SDocClass.Receiving)
                View.StkUcBin.Visibility = Visibility.Collapsed;
        }


        //Permite remover lo label printed
        private void OnRemovePrinted(object sender, EventArgs e)
        {
            if (View.ListPrinted.SelectedItems.Count == 0)
            {
                Util.ShowError(Util.GetResourceLanguage("NO_LABEL_SELECTED_TO_REMOVE"));
                return;
            }

            try
            {
                // armamos lista de NodeTrace selected
                List<NodeTrace> nodeTraceList = new List<NodeTrace>();
                foreach (NodeTrace nodeTr in View.ListPrinted.SelectedItems)
                    nodeTraceList.Add(nodeTr);

                //Evaluar que tipo de Documento es y mandalo a la funcion adecuada
                switch (View.Model.Record.Document.DocType.DocClass.DocClassID)
                {

                    case SDocClass.Receiving:
                        service.ReverseReceiptNodeTraceByLabels(nodeTraceList, App.curUser, View.Model.Record.Document.DocType);
                        ClearLabelsByReverse();
                        break;

                    case SDocClass.Shipping:
                        service.ReversePickingNodeTraceByLabels(nodeTraceList, App.curUser, View.RestockBin.Bin);
                        break;


                    case SDocClass.Task:
                        service.ReversePickingNodeTraceByLabels(nodeTraceList, App.curUser, View.RestockBin.Bin);                        
                        break;

                    case SDocClass.Posting:

                        if (View.Model.Record.Document.DocType.DocTypeID == SDocType.SalesShipment)
                            service.ReversePickingNodeTraceByLabels(nodeTraceList, App.curUser, View.RestockBin.Bin);

                        //Create the Adjust to the Shipment Document
                        AdjustShipmentDocumentByReverse(View.Model.Record, nodeTraceList.Select(f=>f.Label).ToList());

                        break;


                    default:
                        return;

                }


                // refrescamos información local
                View.Model.Record.QtyPending -= nodeTraceList.Sum(f => f.Label.CurrQty);

                RefreshParentBalance();
                ParamRecord(View.Model.Record, View.Model.ParentWindow, false);
                Util.ShowMessage(Util.GetResourceLanguage("LABELS_REMOVED_SUCESSFULLY"));

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("AN_ERROR_HAS_OCURRED") + ": " + ex.Message);
                return;
            }
        }


        private void ClearLabelsByReverse()
        {
            IList<WpfFront.WMSBusinessService.Label> labelsToClear = service.GetLabel(new WpfFront.WMSBusinessService.Label
            {
                LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
                Node = new Node { NodeID = NodeType.PreLabeled }
            });

            foreach (WpfFront.WMSBusinessService.Label lbl in labelsToClear)
                try { service.DeleteLabel(lbl); }
                catch { }

        }


        private void AdjustShipmentDocumentByReverse(DocumentBalance actualRecord, List<WpfFront.WMSBusinessService.Label> labelList)
        {
            //Create Document Line with Unpick Product 
            try
            {
                service.SaveDocumentLine(new DocumentLine
                {
                    Document = actualRecord.Document,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Date1 = DateTime.Now,
                    IsDebit = true,
                    LineStatus = new Status { StatusID = EntityStatus.Active },
                    Note = "Unpicked product after shipment creation.",
                    Product = actualRecord.Product,
                    Quantity = labelList.Sum(f=>f.BaseCurrQty),
                    Unit = actualRecord.Product.BaseUnit,
                    Location = actualRecord.Location
                });
            }
            catch { }
        }


        private void OnRemoveManual(object sender, EventArgs e)
        {
            if (View.TxtQtyManualNew.Text.Equals(""))
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_ENTER_THE_QUANTITY_TO_REMOVE"));
                return;
            }

            int qtyToRemove=0;
            if (!int.TryParse(View.TxtQtyManualNew.Text, out qtyToRemove))
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_ENTER_A_VALID_QUANTITY"));
                return;
            }

            if (qtyToRemove > View.Model.QtyManualOld )
            {
                Util.ShowError(Util.GetResourceLanguage("QUANTITY_TO_REMOVE_CAN_NOT_BE_GREATER_THAN_AVAILABLE"));
                return;
            }
            try
            {
                service.ReverseReceiptNodeTraceByQty(View.Model.Record, qtyToRemove, App.curUser);
                Util.ShowMessage(Util.GetResourceLanguage("QUANTITY_REMOVED_SUCESSFULLY"));

                // refrescamos información
                View.Model.Record.QtyPending -= qtyToRemove;


                //Evaluacion del Tipo de Padre
                RefreshParentBalance();
                ParamRecord(View.Model.Record,View.Model.ParentWindow, false);

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("AN_ERROR_HAS_OCURRED") + ": " + ex.Message);
                return;
            }
            
        }



        private void RefreshParentBalance()
        {
            // refrescamos información de la ventana padre principal
            switch (View.Model.ParentWindow.GetType().Name)
            {
                case "ReceivingPresenter":
                    ((ReceivingPresenter)View.Model.ParentWindow).RefreshBalance(View.Model.Record.Document);
                    ((ReceivingPresenter)View.Model.ParentWindow).RefreshProductList();
                    break;

                case "KitAssemblyPresenter":
                    ((KitAssemblyPresenter)View.Model.ParentWindow).RefreshBalance(View.Model.Record.Document);
                    ((KitAssemblyPresenter)View.Model.ParentWindow).RefreshProductList();
                    break;


                case "ShippingPresenter":
                    ((ShippingPresenter)View.Model.ParentWindow).RefreshBalance(View.Model.Record.Document);
                    ((ShippingPresenter)View.Model.ParentWindow).RefreshProductList();
                    break;
            }
        }
       
    }
}
