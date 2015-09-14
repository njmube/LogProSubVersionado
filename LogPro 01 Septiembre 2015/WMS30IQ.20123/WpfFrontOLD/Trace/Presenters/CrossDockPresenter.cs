using System;
using WpfFront.Models;
using WpfFront.Services;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using WMComposite.Regions;


namespace WpfFront.Presenters
{



    public interface ICrossDockPresenter
    {
        ICrossDockView View { get; set; }
        void LoadDocument(Document document, Document taskDoc);
        void SetShowProcess(bool set);
        void ShowProcessPanel();
        ToolWindow Window { get; set; }
    }


    public class CrossDockPresenter : ICrossDockPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        private Bin PutAway;
        private bool AlreadyProcessed = false;
        public bool ShowProcess = true;
        ProcessWindow pw = null;
        public ToolWindow Window { get; set; }

        public ICrossDockView View { get; set; }



        public CrossDockPresenter(IUnityContainer container, ICrossDockView view, IShellPresenter region)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            this.region = region;
            View.Model = this.container.Resolve<CrossDockModel>();

            //Event Delegate
            View.ProcessPending += new EventHandler<DataEventArgs<int>>(OnProcessPending);
            View.RemoveFromList += new EventHandler<EventArgs>(this.OnRemoveFromList);
            View.AddDocumentToAssigned += new EventHandler<EventArgs>(OnAddDocumentToAssigned);
            View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
            View.CrossDockPreview += new EventHandler<EventArgs>(OnCrossDockPreview);
            View.ConfirmCrossDock += new EventHandler<EventArgs>(OnConfirmCrossDock);
            View.SearchHistDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchHistDocument);
            View.LoadDetails += new EventHandler<DataEventArgs<Document>>(OnLoadDetails);
            View.ShowTicket += new EventHandler<EventArgs>(OnShowTicket);
            View.ShowCrossDockDocuments += new EventHandler<EventArgs>(OnShowCrossDockDocuments);

            View.Model.AnyReceived = false;

            PutAway = service.GetBinLocation("", true);


            //Si  hay conexion a ERP se habilita el panel de posting
            //if (App.IsConnectedToErpReceving)
                View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Received }).First();
            //else
                //View.Model.Node = service.GetNode(new Node { NodeID = Common.NodeType.Stored }).First();


            //ShowProcessPanel();
         }


        public void SetShowProcess(bool set)
        {
            ShowProcess = set;
        }

        public void LoadDocument(Document document, Document taskDoc)
        {
            ShowProcess = true;

            View.DgHistList.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 330;

            try
            {

                View.Model.Document = document;

                if (taskDoc != null)
                {
                    AlreadyProcessed = true;
                    ShowProcess = false;
                    ShowProcessPanel();
                    pw.Close();
                    LoadDetails(taskDoc);
                    return;
                }

                pw = new ProcessWindow("Loading data for document " + document.DocNumber);


                //Revisar si ya se realizo una tarea de crossdock sobre ese documento
                TaskDocumentRelation taskRel = new TaskDocumentRelation
                {
                    IncludedDoc = document,
                    TaskDoc = new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } }
                };


                IList<TaskDocumentRelation> listTask = service.GetTaskDocumentRelation(taskRel)
                    .Where(f => f.TaskDoc.DocStatus.StatusID != DocStatus.Cancelled).ToList();
                if (listTask != null && listTask.Count > 0)
                {
                    //Este documento ya fue procesado 
                    AlreadyProcessed = true;
                    ShowProcess = false;
                }


                ShowProcessPanel();

                //Si ya lo proceso se manda a la historia.
                if (AlreadyProcessed)
                {
                    pw.Close();
                    Util.ShowError("Document " + View.Model.Document + " was already processed.");

                    //Load CrossDock History
                    LoadDetails(listTask[0].TaskDoc);

                    return;
                }



                //Load Sales Order Document List
                View.Model.AvailableDocs = service.GetCrossDockSalesOrders(document);
                if (View.Model.AvailableDocs == null || View.Model.AvailableDocs.Count == 0)
                {
                    pw.Close();
                    Util.ShowError("Sales document products do not match with the received products to make Cross Dock.");
                    //lo deja en los historicos de Cross Dock.
                    //View.TbCross.IsEnabled = false;
                    ShowProcess = false;
                    ShowProcessPanel();
                }
                else
                {
                    //Si hay documentos con productos que coincidan
                    LoadSalesDocument();

                    //Cargando el purchasing Document
                    View.Model.DocumentData = Util.ToShowData(document);
                    View.Model.DocumentLines = service.GetDocumentLine(new DocumentLine { Document = document });

                    RefreshBalance(document);
                    pw.Close();
                }


            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error loading document. \n" + ex.Message);
            }
            finally { pw.Close();  }

        }


        public void ShowProcessPanel()
        {
            if (ShowProcess)
            {
                View.TbCross.IsEnabled = true;
                //View.TbHistory.IsEnabled = false;
                View.TbControl.SelectedIndex = 0;
            }
            else
            {

                View.TbCross.IsEnabled = false;
                //View.TbHistory.IsEnabled = true;
                View.TbControl.SelectedIndex = 1;

                //Load Documents
               View.Model.HistoryList = service.GetDocument(new Document
                { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } });

                //Si solo hay un documento lo carga inmediatamente.
               //if (View.Model.HistoryList != null && View.Model.HistoryList.Count == 1)
               //    LoadDetails(View.Model.HistoryList[0]);
            }
        }


        private void LoadSalesDocument()
        {
           
            Document curDoc;

            if (View.Model.AssignedDocs != null)
                foreach (Document document in View.Model.AssignedDocs)
                {
                    curDoc = View.Model.AvailableDocs.Where(f => f.DocID == document.DocID).First();
                    if (curDoc != null)
                        View.Model.AvailableDocs.Remove(curDoc);
                }

            View.LvAvailableDocs.Items.Refresh();
            View.LvAssignedDocs.Items.Refresh();
        }


        private void RefreshBalance(Document document)
        {

            DocumentBalance docBalance = new DocumentBalance
            {
                Document = document,
                Node = View.Model.Node,
                Location = App.curLocation
            };

            //#########Receiving Balance
            if (View.Model.DocumentLines == null || View.Model.DocumentLines.Count == 0)
                View.Model.DocumentBalance = service.GetDocumentBalanceForEmpty(docBalance);
            else
                View.Model.DocumentBalance = service.GetDocumentBalance(docBalance, true);

            View.DgDocumentBalance.Items.Refresh();

            //Mostrar o no el mensaje del balance
            bool showMessage = false;
            foreach (DocumentBalance docBal in View.Model.DocumentBalance)
            {
                if (docBal.QtyPending > 0)
                {
                    showMessage = true;
                    break;
                }
            }

            //Si hay saldo pendiente por recibir.
            View.StkPendingMsg.Visibility = showMessage ? Visibility.Visible : Visibility.Collapsed;


            //Si nada esta recibido, dejar el boton bloqueado.

            if (View.Model.DocumentBalance.Any(f => f.QtyProcessed > 0))
                View.Model.AnyReceived = true;

        }


        private void OnProcessPending(object sender, DataEventArgs<int> e)
        {
            if (e.Value == 0){
                //Ejecuta el Proceso de Put Away de la mercancia Pendiente
                PutAwayPendingQuantities();
            }
            else if (e.Value == 1)
            {
                //Lama al view que maneja el Receiving
                /*
                IReceivingPresenter presenter = container.Resolve<IReceivingPresenter>();
                region.Shell.ShowViewInShell(presenter.View);
                App.curPresenter = typeof(ICrossDockPresenter);
                presenter.SetDocument(View.Model.Document);
                 */


                //Lama al view que maneja el Cross Dock
                IReceivingPresenter presenter = container.Resolve<IReceivingPresenter>();
                presenter.SetDocument(View.Model.Document);

                InternalWindow window = Util.GetInternalWindow(this.Window.Parent, "Receiving Process");
                presenter.Window = window;
                window.GridContent.Children.Add((ReceivingView)presenter.View);
                window.Show();

            }
        }


        private void PutAwayPendingQuantities()
        {
            if (PutAway == null)
            {
                Util.ShowError("Put away location could not be resolved.");
                return;
            }

            try
            {
                pw = new ProcessWindow("Receiving Product ...");
                service.ReceiptAtOnce(View.Model.Document, PutAway, View.Model.Node);
                RefreshBalance(View.Model.Document);
                pw.Close();
                Util.ShowMessage("Pending quantities for document " + View.Model.Document.DocNumber + "  was received.");
                View.Model.AnyReceived = true;
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Product could not be received. \n" + ex.Message);
            }
        }


        private void OnAddDocumentToAssigned(object sender, EventArgs e)
        {

            if (View.LvAvailableDocs.SelectedItems == null)
                return;

            try
            {
                foreach (Document selItem in View.LvAvailableDocs.SelectedItems)
                    AddDocument(selItem);

                View.LvAssignedDocs.Items.Refresh();
                View.LvAvailableDocs.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be assigned.\n" + ex.Message);
            }

        }


        private void AddDocument(Document document)
        {
            if (document == null)
                return;

            try
            {
                if (View.Model.AssignedDocs == null)
                    View.Model.AssignedDocs = new List<Document>();

                View.Model.AssignedDocs.Insert(0, document);
                View.Model.AvailableDocs.Remove(document);
                View.BtnStep1.Visibility = (View.Model.AssignedDocs == null || View.Model.AssignedDocs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private void OnRemoveFromList(object sender, EventArgs e)
        {
            // Removing a Bin
            if (((Button)sender).Name == "btnRemDoc")
                RemoveDocument();

        }


        private void RemoveDocument()
        {
            if (View.LvAssignedDocs.SelectedItems == null || View.LvAssignedDocs.SelectedItems.Count == 0)
                return;

            string msg = "";
            Document document = null;

            foreach (Object obj in View.LvAssignedDocs.SelectedItems)
            {
                try
                {
                    document = (Document)obj;
                    View.Model.AssignedDocs.Remove((Document)obj);
                    View.Model.AvailableDocs.Insert(0,(Document)obj);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Document: " + document.DocNumber + ". " + ex.Message;
                }
            }


            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            View.LvAvailableDocs.Items.Refresh();
            View.LvAssignedDocs.Items.Refresh();
            View.BtnStep1.Visibility = (View.Model.AssignedDocs == null || View.Model.AssignedDocs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;

        }


        private void OnSearchDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Value) || e.Value.Length < WmsSetupValues.SearchLength)
                {
                    View.Model.AvailableDocs = service.GetCrossDockSalesOrders(View.Model.Document);
                    LoadSalesDocument();
                    return;
                }

                View.Model.AvailableDocs = service.GetCrossDockSalesOrders(View.Model.Document)
                    .Where(f=>f.DocNumber.Contains(e.Value) || f.Customer.AccountCode.Contains(e.Value) || f.Customer.Name.Contains(e.Value))
                    .ToList();

                LoadSalesDocument();

            }
            catch { }
        }


        //Encargada de obtener el cruce entre el purchase document y los sales document
        //Involucrados en el proceso cross dock;
        private void OnCrossDockPreview(object sender, EventArgs e)
        {
            View.BtnStep1.IsEnabled = false;


            //Obteniendo el Balance
            DocumentBalance docBalance = new DocumentBalance
            {
                Document = View.Model.Document,
                Node = View.Model.Node,
                Location = App.curLocation
            };

            ProcessWindow pw = new ProcessWindow("");
            pw.Show();

            try
            {

                //Obteniendo el cruce de documentos.
                View.Model.CrossDockBalance = service.GetCrossDockBalance(docBalance, View.Model.AssignedDocs.ToList())
                    .Where(f => !string.IsNullOrEmpty(f.Notes))
                    .OrderBy(f => f.Notes).ToList();

                //Mostrando el Warning de not suppied if apply
                if (View.Model.CrossDockBalance != null && View.Model.CrossDockBalance.Where(f => f.Notes == "Qty not supplied").Count() > 0)
                    View.TxtWarning.Visibility = Visibility.Visible;

                //Ocultando el expander superior
                View.ExpDocs.IsExpanded = false;

                //Visible the second panel
                View.ExpResult.IsExpanded = true;
                View.ExpResult.Visibility = Visibility.Visible;

                pw.Close();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Problem generating the Cross Dock preview.\n"+ ex.Message);
            }

        }


        private void OnConfirmCrossDock(object sender, EventArgs e)
        {
            ProcessWindow pw = null;
            Document crossDockDocument = null;
            Document pReceipt = null;
            View.Model.Document.Location = App.curLocation;
            View.Model.Document.Company = App.curCompany;
            View.Model.Document.ModifiedBy = App.curUser.UserName;
            int step = 0;

            View.BtnStep2.IsEnabled = false;

            try
            {
                //1. Create Cross Dock document
                pw = new ProcessWindow("Creating Cross Dock document ...");

                //Poniendo el Location al documento
                for (int i = 0; i < View.Model.CrossDockBalance.Count; i++)
                    View.Model.CrossDockBalance[i].Location = App.curLocation;

                //Crea un documento con las transacciones finales a mover en despacho.
                crossDockDocument = service.ConfirmCrossDockProcess(View.Model.CrossDockBalance.ToList(), App.curUser.UserName);

                step = 1;

                //Update the purchase documento to Cross = true
                //View.Model.Document.CrossDocking = true;
                //service.UpdateDocument(View.Model.Document);


                pw.Close();
            }
            catch (Exception ex)
            {
                if (step == 1)
                    CancelDocument(crossDockDocument);

                pw.Close();
                Util.ShowError("Problem creating Cross Dock document.\n" + ex.Message);
                return;
            }


            //if (App.IsConnectedToErpReceving)
            //{

            //Este procesos ejecutar.
            //2. Create Purchase Receipt Solo si esta pegado al ERP.

            try
            {

                pw = new ProcessWindow("Creating Purchase Receipt for document " + View.Model.Document.DocNumber + " ...");
                pReceipt = service.CreatePurchaseReceipt(View.Model.Document);

                step = 2;

                //Adicionando un ocmentario de Cross Dock al recibo.
                pReceipt.Comment = "Receipt under Cross Dock process. Document " + crossDockDocument.DocNumber;
                pReceipt.CrossDocking = true;
                service.UpdateDocument(pReceipt);

                step = 3;


                //Adiciona el PR ala relacion del CrossDock.
                TaskDocumentRelation tkDoc = new TaskDocumentRelation
                {
                    CreationDate = DateTime.Now,
                    CreatedBy = App.curUser.UserName,
                    IncludedDoc = pReceipt,
                    TaskDoc = crossDockDocument
                };

                service.SaveTaskDocumentRelation(tkDoc);
                step = 4;

                pw.Close();
            }
            catch (Exception ex)
            {
                CancelDocument(crossDockDocument);

                if (step >= 2)
                    service.ReversePurchaseReceipt(pReceipt); //reversar el recibo

                pw.Close();
                Util.ShowError("Problem creating Purchase Receipt.\n" + ex.Message);
                return;
            }

            //}


            try
            {
                //3. Picking Product Based on CrossDock Preview. - Lo Saca de MAIN or putawayZone
                pw = new ProcessWindow("Allocating sales document quantities ...");
                //IList<DocumentLine> crossLines = service.GetDocumentLine(new DocumentLine { Document = crossDockDocument });

                //Del documento de cross co obtiene solo las lineas de Sales Orders para ser piqueadeas
                //foreach (DocumentLine line in crossLines)
                service.PickCrossDockProduct(View.Model.Document, View.Model.CrossDockBalance.ToList(), App.curUser);

                pw.Close();
            }
            catch (Exception ex)
            {
                pReceipt.Comment = "Cross Dock transaction problem.";
                service.ReversePurchaseReceipt(pReceipt); //reversar el recibo
                CancelDocument(crossDockDocument); //Reversar el Cross Dock
                pw.Close();
                Util.ShowError("Problem allocating sales document quantities.\n" + ex.Message);
                return;
            }


            Util.ShowMessage("Cross Dock Process Complete.");

        }


        private void CancelDocument(Document cdoc)
        {
            cdoc.Comment = "Cross Dock transaction problem";
            cdoc.DocStatus = new Status { StatusID = DocStatus.Cancelled };
            service.UpdateDocument(cdoc);
        }



        ///Metods para soporte de los ducumentos ya procesados
        ///
        private void OnSearchHistDocument(object sender, DataEventArgs<string> e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Value) || e.Value.Length < WmsSetupValues.SearchLength)
                {
                    View.Model.HistoryList = service.GetDocument(new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } });
                    return;
                }

                View.Model.AvailableDocs = service.GetDocument(new Document
                { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } })
                .Where(f => f.DocNumber.Contains(e.Value) || f.Customer.AccountCode.Contains(e.Value) || f.Customer.Name.Contains(e.Value))
                    .ToList();


            }
            catch { }
        }


        private void OnLoadDetails(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            LoadDetails(e.Value);

        }


        public void LoadDetails(Document histDoc)
        {
            try
            {
                pw = new ProcessWindow("Loading Document " + histDoc.DocNumber + " ...");
                View.StkDetail.Visibility = Visibility.Visible;
                View.Model.HistData = Util.ToShowData(histDoc);
                View.Model.HistLines = service.GetDocumentLine(new DocumentLine { Document = histDoc });
                View.Model.HistDoc = histDoc;

                //Loadin Cross Docks
                View.Model.CrossDocs = service.GetTaskDocumentRelation(new TaskDocumentRelation { TaskDoc = View.Model.HistDoc });

                pw.Close();
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Document could not be loaded.\n" + ex.Message);
            }
        }


        private void OnShowTicket(object sender, EventArgs e)
        {
            try
            {
                pw = new ProcessWindow("Generating Document ... ");
                UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.HistDoc.DocID, "", false); //"PDF995"
                pw.Close();
            }
            catch { pw.Close(); }
        }

        private void OnShowCrossDockDocuments(object sender, EventArgs e)
        {
            //Load Documents
            if (View.Model.HistoryList == null || View.Model.HistoryList.Count == 0)
            {
                pw = new ProcessWindow("Loading documents");
                View.Model.HistoryList = service.GetDocument(new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } });
                View.DgHistList.Items.Refresh();
                pw.Close();
            }
        }

    }
}
