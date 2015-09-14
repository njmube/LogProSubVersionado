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
using System.Collections.Specialized;
using System.Reflection;
using System.Collections;

namespace WpfFront.Presenters
{


    public interface IInventoryCountPresenter
    {
        IInventoryCountView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class InventoryCountPresenter : IInventoryCountPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        public IInventoryCountView View { get; set; }
        private IList<Bin> oriAvailableBin { get; set; }
        DocumentType docType { get; set; }
        ProcessWindow pw;
        int countType = -1; //0 = Bin , 1 = PRODUCT


        public InventoryCountPresenter(IUnityContainer container, IInventoryCountView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<InventoryCountModel>();


            View.FilterByBin += new EventHandler<DataEventArgs<string>>(View_FilterByBin);
            View.AddToAssigned += new EventHandler<EventArgs>(View_AddToAssigned);
            View.RemoveFromList += new EventHandler<EventArgs>(View_RemoveFromList);
            View.CreateNewTask += new EventHandler<EventArgs>(View_CreateNewTask);
            View.LoadDetails += new EventHandler<DataEventArgs<Document>>(View_LoadDetails);
            View.ShowTicket += new EventHandler<EventArgs>(View_ShowTicket);
            View.ChangeStatus += new EventHandler<EventArgs>(view_ChangeStatus);
            //View.BinTaskSelected += new EventHandler<DataEventArgs<ProductStock>>(View_BinTaskSelected);
            View.ChangeCountedQty += new EventHandler<DataEventArgs<object[]>>(View_ChangeCountedQty);
            View.ConfirmCountTask += new EventHandler<EventArgs>(View_ConfirmCountTask);
            View.CancelTask += new EventHandler<EventArgs>(View_CancelTask);
            View.SearchDocument += new EventHandler<DataEventArgs<string>>(View_SearchDocument);
            View.RefreshDocuments += new EventHandler<EventArgs>(View_RefreshDocuments);
            View.ReloadDocument += new EventHandler<EventArgs>(View_ReloadDocument);
            View.FilterByProduct += new EventHandler<DataEventArgs<Product>>(View_FilterByProduct);
            View.UpdateDocumentOption += new EventHandler<DataEventArgs<int>>(View_UpdateDocumentOption);
            View.ShowInitialTicket += new EventHandler<EventArgs>(View_ShowInitialTicket);
            View.LoadNoCountBalance += new EventHandler<EventArgs>(View_LoadNoCountBalance);
            View.SendAdjustment += new EventHandler<EventArgs>(View_SendAdjustment);
            View.ChangeSendOption += new EventHandler<EventArgs>(OnChangeSendOption);
            view.SelectAll += new EventHandler<EventArgs>(OnSelectAll);
            view.UnSelectAll += new EventHandler<EventArgs>(OnUnSelectAll);


            //DocType
            docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Task } };
            docType.DocTypeID = SDocType.CountTask;

            ProcessWindow pw = new ProcessWindow("Loading Bin List ...");

            //oriAvailableBin = service.GetBin(new Bin { Location = App.curLocation }).OrderBy(f=> f.BinCode).ToList();
            oriAvailableBin = service.GetBin(new Bin()).OrderBy(f => f.BinCode).ToList();

            pw.Close();

            RefreshDocuments();
            
            //Product Categories
            try
            {
                IList<ProductCategory> list = service.GetProductCategory(new ProductCategory());
                list.Add(new ProductCategory { Name = "... Any Category"});
                View.Model.ProductCategories = list.OrderBy(f=>f.Name).ToList();
            }
            catch { }

            // CAA [2010/07/07]  Carga los filtros de busq de bines.
            IqReportColumn rc = new IqReportColumn();
            rc.Alias = "Filter by Bin";
            rc.FilteredValue = "";
            View.BFilters.cboStrComp.SelectedValue = " = _val";
            View.BFilters.RepColumn = rc;
        }



        void View_SendAdjustment(object sender, EventArgs e)
        {
            // CAA [2010/06/04]
            // Opc. para enviar el producto a un Bin  (y validar opc. para enviar al ERP)
            /*
                        0        <ComboBoxItem x:Uid="ComboBoxItem_1" Content="Send Adjustment to WMS and ERP"/>
                        1        <ComboBoxItem x:Uid="ComboBoxItem_2" Content="Send Adjustment only to WMS"/>
                        2        <ComboBoxItem x:Uid="ComboBoxItem_3" Content="Send Product back to bin"/>
             */

            if (View.CboSendOptions.SelectedIndex == -1)
            {
                Util.ShowError("Select an Adjustment option");
                return;
            }

            if (!UtilWindow.ConfirmOK("You have selected this option:\n\n"+View.CboSendOptions.SelectionBoxItem.ToString()+"\nClick OK to continue.").Value)
                return;

            //Counting Task Confirmation
            try
            {
                IEnumerable<ProductStock> list = View.Model.NoCountSummary.Where(f => f.Mark == true);

                if (list == null || list.Count() == 0) // && !View.Model.NoCountSummary.Any(f => f.Mark == true))
                {
                    //pw.Close();
                    Util.ShowError("No records to confirm.");
                    return;
                }

                bool erp=false;
                if (View.CboSendOptions.SelectedIndex == 0)   // sent adjustment to ERP
                {                                             // valida si tiene conex. habilitada.
                    erp = true;
                    if (string.IsNullOrEmpty(Util.GetConfigOption("WITHERPIN")) || !Util.GetConfigOption("WITHERPIN").Equals("T"))
                    {
                        Util.ShowError("ERP Connection does not exist.");
                        return;
                    }
                }

                ProcessWindow pw = new ProcessWindow("Sending NOCOUNT Adjustment ...");
                string result="";
                if (View.CboSendOptions.SelectedIndex == 0 || View.CboSendOptions.SelectedIndex == 1)
                {
                    Document adjNocount = service.ProcessNoCount(list.ToList(), App.curUser.UserName, erp); 
                    result = "NOCOUNT Adjusment # " + adjNocount.DocNumber + " was created.";
                }
                else
                    if (View.CboSendOptions.SelectedIndex == 2)   // adjustment to BIN
                    {
                        service.ProcessNoCountToBin(list.ToList(), App.curUser.UserName, App.curLocation, View.BinRestock.Bin);
                        result = "NOCOUNT Process done.";
                    }

                LoadNoCountBalance();

                pw.Close();

                Util.ShowMessage(result);


            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Sending NOCOUNT Adjustment.\n" + ex.Message);
            }
        }




        void View_LoadNoCountBalance(object sender, EventArgs e)
        {
            LoadNoCountBalance();

        }


        private void LoadNoCountBalance()
        {
            View.Model.NoCountSummary = service.GetNoCountSummary(App.curLocation);

            View.StkAdjustmentOptions.IsEnabled = false;
            // View.BtnCfmNoCount.IsEnabled = false;

            if (View.Model.NoCountSummary != null && View.Model.NoCountSummary.Count > 0)
                View.StkAdjustmentOptions.IsEnabled = true;
                //View.BtnCfmNoCount.IsEnabled = true;
        }



        void View_ShowInitialTicket(object sender, EventArgs e)
        {
            try
            {
                pw = new ProcessWindow("Generating Document ... ");
                UtilWindow.ShowDocument(new LabelTemplate { Header = WmsSetupValues.CountTicketTemplate }, View.Model.Document.DocID, "", false); //"PDF995"
                pw.Close();
            }
            catch { pw.Close(); }
        }


        void View_UpdateDocumentOption(object sender, DataEventArgs<int> e)
        {
            //Retrae el documento para actualizarlo y no cambiarle es Ultimo estado.
            //View.Model.Document = service.GetDocument(new Document { DocID = View.Model.Document.DocID }).First();
            
            countType = int.Parse(e.Value.ToString());

            if (countType == 0)
                ((GridView)View.LvAssigned.View).Columns[0].Width = 80;
            else
                ((GridView)View.LvAssigned.View).Columns[0].Width = 0;

            if (View.Model.Document.Notes != e.Value.ToString())
            {
                View.Model.Document.Notes = e.Value.ToString();
                service.UpdateDocument(View.Model.Document);

                LoadDetails(View.Model.Document);
            }

            
        }


        void View_ReloadDocument(object sender, EventArgs e)
        {
            ProcessWindow pw = new ProcessWindow("Loading Document ...");
            try
            {
                View.Model.Document = service.GetDocument(new Document { DocID = View.Model.Document.DocID }).First();
                LoadDetails(View.Model.Document);
            }
            catch { }
            finally { pw.Close(); }
        }



        void View_RefreshDocuments(object sender, EventArgs e)
        {
            RefreshDocuments();
        }


        private void RefreshDocuments()
        {
            ProcessWindow pw = new ProcessWindow("Loading Counting Tasks ...");

            try
            {                
                View.Model.AvailableBin = oriAvailableBin.Where(f=>f.Location.LocationID == App.curLocation.LocationID).ToList();

                //Document Lis
                View.Model.DocumentList = service.GetPendingDocument(
                        new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation }, 0, 0)
                        .OrderBy(f=>f.ModDate).ToList(); // 

            }
            catch { }
            finally { pw.Close(); }
        }



        void View_SearchDocument(object sender, DataEventArgs<string> e)
        {
            ProcessWindow pw = new ProcessWindow("Searching ...");
            try
            {

                DocumentType docType = new DocumentType { DocTypeID = SDocType.CountTask };

                if (string.IsNullOrEmpty(e.Value))
                {
                    pw.Close();

                    View.Model.DocumentList = service.GetPendingDocument(
                            new Document { DocType = docType, Company = App.curCompany, Location = App.curLocation }, 0, 0); // 

                    return;
                }

                View.Model.DocumentList = service.SearchDocument(e.Value, docType);

                //si encuentra un resultado lo carga
                if (View.Model.DocumentList != null && View.Model.DocumentList.Count == 1)
                {
                    View.DgDocument.SelectedIndex = 0;
                    LoadDetails(View.Model.DocumentList[0]);
                }
 

                pw.Close();
            }
            catch { pw.Close(); }
        }



        void View_CancelTask(object sender, EventArgs e)
        {

            ProcessWindow pw = new ProcessWindow("Cancelling Task " + View.Model.Document.DocNumber + " ...");

            //Counting Task Confirmation
            try
            {
                service.CancelCountingTask(View.Model.Document, App.curUser.UserName);

                Util.ShowMessage("Count Task# " + View.Model.Document.DocNumber + " was Cancelled.");

                View.Model.Document = null;
                View.BtnConfirm.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Util.ShowError("Error Cancelling Task " + View.Model.Document.DocNumber + ".\n" + ex.Message);
            }
            finally
            {
                pw.Close();
            }
        }





        void View_ConfirmCountTask(object sender, EventArgs e)
        {

            ProcessWindow pw = new ProcessWindow("Confirming Task " + View.Model.Document.DocNumber + " ...");

            //Counting Task Confirmation
            try
            {
                IEnumerable<CountTaskBalance> list = View.Model.CountSummary.Where(f => f.Mark == true);

                if (list == null || list.Count() == 0 && !View.Model.CountSummary.Any(f => f.Mark == true))
                {
                        pw.Close();
                        Util.ShowError("No records to confirm.");
                        return;
                }              

                View.Model.Document = service.ConfirmCountingTaskDocument(View.Model.Document, list.ToList(), App.curUser.UserName);


                pw.Close();

                Util.ShowMessage("Count Task# " + View.Model.Document.DocNumber + " was Confirmed.\nPlease see confirmation Document for details.");

                LoadDetails(View.Model.Document);

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error Confirming Task " + View.Model.Document.DocNumber + ".\n" + ex.Message);
            }

        }



        void View_ChangeCountedQty(object sender, DataEventArgs<object[]> e)
        {
            if (e.Value == null)
                return;

            //Change Quantitie.
            BinByTaskExecution bte = new BinByTaskExecution { RowID = int.Parse(e.Value[0].ToString()) };
            bte = service.GetBinByTaskExecution(bte).First();
            bte.QtyCount = double.Parse(e.Value[1].ToString());
            bte.ModDate = DateTime.Now;
            bte.ModifiedBy = App.curUser.UserName;

            //Update the execution
            service.UpdateBinByTaskExecution(bte);

            View.Model.CountSummary = service.GetCountSummary(View.Model.Document, false);

            RefreshCountSummary();
        }



        private void RefreshCountSummary()
        {

            for (int i = 0; i < View.Model.CountSummary.Count; i++)
            {
                //View.Model.CountSummary[i].Difference = View.Model.CountSummary[i]. - View.Model.CountSummary[i].PackStock;
                if (View.Model.CountSummary[i].Difference != 0 || !string.IsNullOrEmpty(View.Model.CountSummary[i].Comment))
                    View.Model.CountSummary[i].Mark = true;
                else
                    View.Model.CountSummary[i].Comment = "OK";
            }

            View.Model.CountSummary = View.Model.CountSummary.OrderBy(f => f.Comment).ToList();
            View.LvSumm.Items.Refresh();
        }



        void view_ChangeStatus(object sender, EventArgs e)
        {
            try
            {
                //View.Model.Document.DocStatus = (Status)View.ComboStatus.SelectedItem;
                View.Model.Document.Date1 = View.TxtSchDate.SelectedDate;
                View.Model.Document.ModifiedBy = App.curUser.UserName;
                View.Model.Document.ModDate = DateTime.Now;
                service.UpdateDocument(View.Model.Document);

                Util.ShowMessage("Document updated.");
            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be updated. \n" + ex.Message);
            }
        }


        void View_ShowTicket(object sender, EventArgs e)
        {
            try
            {
                pw = new ProcessWindow("Generating Document ... ");
                UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.Document.DocID, "", false); //"PDF995"
                /*
                // CAA [2010/07/06]
                // Genera distinto formato de impresión; según el estado del doc.
                // Posted? mostrar todo (summary y detail)...  Si no, solo Summary 
                if (string.IsNullOrEmpty(Util.GetConfigOption("CTREPDETAIL")) || Util.GetConfigOption("CTREPDETAIL").Equals("F"))
                    UtilWindow.ShowDocument(View.Model.Document.DocType.Template, View.Model.Document.DocID, "", false); //"PDF995"
                else
                    if (View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
                        UtilWindow.ShowDocument(new LabelTemplate { Header = WmsSetupValues.CountTicketTemplate_Full }, View.Model.Document.DocID, "", false);
                    else
                        UtilWindow.ShowDocument(new LabelTemplate { Header = WmsSetupValues.CountTicketTemplate_Summary  }, View.Model.Document.DocID, "", false);
                */
                pw.Close();
            }
            catch { pw.Close(); }
        }



        void View_LoadDetails(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            if (View.Model.Document != null && View.Model.Document.DocID == e.Value.DocID)
                return;


            ProcessWindow pw = new ProcessWindow("Loading Document " + e.Value.DocNumber + " ...");

            try
            {
                LoadDetails(e.Value);
                pw.Close();

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Document could not be loaded.\n" + ex.Message);
            }

        }



        void View_CreateNewTask(object sender, EventArgs e)
        {
            //Create the new Count Task y la Selecciona. Igual que create Empty document

            ProcessWindow pw = new ProcessWindow("Creating New Counting Task ...");

            try
            {

                Document document = new Document
                {
                    DocType = docType,
                    CrossDocking = false,
                    IsFromErp = false,
                    Location = App.curLocation,
                    Company = App.curCompany,
                    Date1 = DateTime.Today,                    
                    CreatedBy = App.curUser.UserName
                };

                document = service.CreateNewDocument(document, true);

                View.Model.DocumentList.Add(document);

                View.DgDocument.Items.Refresh();

                pw.Close();
                Util.ShowMessage("Task Document " + document.DocNumber + " created.");


                View.ExpSetup.IsExpanded = true;

                //LoadDetails(document);
                View.CboToDo.IsEnabled = true;
                View.CboToDo.SelectedIndex = -1;
                View.Model.Document = document;

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Document could not be created. \n" + ex.Message);
            }

        }


        private void LoadDetails(Document document)
        {

            ProcessWindow pw = new ProcessWindow("Loading Counting Task ...");

            View.StkTask.Visibility = Visibility.Visible;
            View.BtnConfirm.IsEnabled = true;
            View.BtnRemove.IsEnabled = false;

            View.Model.Document = document;
            //View.TxtSchDate.S


            if (View.Model.Document.DocStatus.StatusID == DocStatus.New)
            {
                View.BtnRemove.IsEnabled = true;
                View.Model.AssignBin = service.GetBinByTask(new BinByTask { TaskDocument = document });

                //Mostrar solo los que no esten assignados.
                if (document.Notes == "0") //BINS
                {
                    View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID).ToList();

                    View.Model.AvailableBin =
                            (from available in View.Model.AvailableBin
                             join assign in View.Model.AssignBin on available.BinID equals assign.Bin.BinID
                             into gj
                             from sub in gj.DefaultIfEmpty()
                             select new Bin
                             {
                                 BinID = available.BinID,
                                 BinCode = available.BinCode,
                                 Rank = (sub == null) ? 1 : 0,
                                 Location = available.Location
                             })
                    .Where(f => f.Rank > 0) //Solo lo que no esten assignados
                    .ToList();
                }
                //else if (document.Notes == "1") //PRODUCTS
                //{
                //    View.Model.AvailableProduct =
                //            (from available in View.Model.AvailableProduct
                //             join assign in View.Model.AvailableProduct on available.ProductID equals assign.ProductID
                //             into gj
                //             from sub in gj.DefaultIfEmpty()
                //             select new Product
                //             {
                //                 ProductID = available.ProductID,
                //                 FullDesc = available.FullDesc,
                //                 ProductCode = available.ProductCode,
                //                 UnitsPerPack = (sub == null) ? 1 : 0 //Marca temporal
                //             })
                //    .Where(f => f.UnitsPerPack > 0) //Solo lo que no esten assignados
                //    .ToList();
                //}
            }



            //View.BtnUpdate.IsEnabled = false;
            //if (View.Model.Document.DocStatus.StatusID == DocStatus.New || View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
            //    View.BtnUpdate.IsEnabled = true;

            //Loading Execution 
            //View.LvExeDetail.Visibility = Visibility.Collapsed;

            LoadExecution();



            //Mostrar u Ocultar Ticket y boton de Update.
            View.BtnUpdate.IsEnabled = false;
            View.BtnTicket.Visibility = Visibility.Collapsed;
            View.BtnTicketList.Visibility = Visibility.Collapsed;
            View.BtnCancel.Visibility = Visibility.Collapsed;

            if (View.Model.Document.DocStatus.StatusID == DocStatus.New || View.Model.Document.DocStatus.StatusID == DocStatus.InProcess)
            {
                View.BtnUpdate.IsEnabled = true;
                View.BtnTicketList.Visibility = Visibility.Visible;
            }

            if (View.Model.Document.DocStatus.StatusID == DocStatus.Posted || View.Model.Document.DocStatus.StatusID == DocStatus.Completed)  
                View.BtnTicket.Visibility =  Visibility.Visible;


            if (View.Model.Document.DocStatus.StatusID != DocStatus.Posted)
                View.BtnCancel.Visibility = Visibility.Visible;


            //Revisa la opcion del documento seleccionada.
            View.StkOptions.Visibility = Visibility.Collapsed;
            View.CboToDo.IsEnabled = true;
            View.CboToDo.SelectedIndex = -1;
            try
            {
                View.CboToDo.SelectedIndex = int.Parse(View.Model.Document.Notes);
                View.CboToDo.IsEnabled = false;
            }
            catch { }


            pw.Close();

        }



        private void LoadExecution()
        {
            View.ExpExe.Visibility = Visibility.Collapsed;


            //Stock = Counted, PackStock = Expected
            if (View.Model.Document.DocStatus.StatusID == DocStatus.Completed || View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
            {

                View.StkConfirm.Visibility = Visibility.Collapsed;

                //
                View.ExpExe.Visibility = Visibility.Visible;
                View.ExpExe.IsExpanded = true;
                View.ExpSetup.IsExpanded = false;


                View.Model.CountSummary = service.GetCountSummary(View.Model.Document, false);

                //Summary    
                // CAA [2010/06/22]
                // cálculo consolidado   [excluye: lo esperado y NO contado]
                View.Model.CountSummaryX =
                       (from ctBalance in View.Model.CountSummary // .Where(f=> f.CaseType!=3 && f.CaseType!=6)
                        group ctBalance by new { ctBinId = ctBalance.Bin.BinID, ctProductId = ctBalance.Product.ProductID }  // , cttype = ((ctBalance.Label == null) ? false : true)
                            into ctb
                            select new CountTaskBalance
                            {
                                Bin = ctb.First().Bin,  // View.Model.CountSummary.Select(f=> f.Bin).Where(f=> f.BinID == ctb.Key.ctBinId).First(),  
                                Product = ctb.First().Product,  // View.Model.CountSummary.Select(f=> f.Product).Where(f=> f.ProductID == ctb.Key.ctProductId).First(),  
                                //Difference = ctb.Sum(f => f.Difference),
                                Difference = ctb.Sum(f => (f.CaseType == 3 || f.CaseType == 6) ? 0 - f.QtyExpected : f.Difference),
                                // QtyCount = ctb.Sum(f => f.QtyCount),
                                QtyCount = ctb.Sum(f => (f.CaseType==3 || f.CaseType==6)?0:f.QtyCount),
                                QtyExpected = ctb.Sum(f => f.QtyExpected)
                                // Mark = ctb.Key.cttype,
                            }).ToList();
                
                if (View.Model.CountSummary != null && View.Model.CountSummary.Count > 0 || View.Model.Document.DocStatus.StatusID == DocStatus.Posted)
                {
                    if (View.Model.Document.DocStatus.StatusID == DocStatus.Completed)
                        View.StkConfirm.Visibility = Visibility.Visible;

                    RefreshCountSummary();
                }
                else
                {
                    Util.ShowError("No differences were found in this task.");
                }
                
            }

            if (View.Model.Document.DocStatus.StatusID == DocStatus.New || View.Model.Document.DocStatus.StatusID == DocStatus.InProcess)
                View.ExpSetup.IsExpanded = true;


        }


        //void View_BinTaskSelected(object sender, DataEventArgs<ProductStock> e)
        //{


        //    if (e.Value == null)
        //        return;

        //    //View.LvExeDetail.Visibility = Visibility.Collapsed;

        //    try
        //    {
        //        //show the info of a Document, Bin, Product Summary Selected.
        //        View.Model.CountExecution = service.GetBinByTaskExecution(
        //            new BinByTaskExecution
        //            {
        //                Product = e.Value.Product,
        //                BinTask = new BinByTask
        //                {
        //                    TaskDocument = View.Model.Document,
        //                    Bin = e.Value.Bin
        //                },
        //                Status = new Status { StatusID = DocStatus.Completed }
        //            }
        //        ).ToList(); //Where(f=>f.QtyCount > 0);

        //        //View.LvExeDetail.Visibility = Visibility.Visible;
        //    }
        //    catch { }

        //}


        void View_RemoveFromList(object sender, EventArgs e)
        {
            // Removing a Bin
            if (((Button)sender).Name == "btnRemove")
                RemoveRecord();

        }


        private void RemoveRecord()
        {
            if (View.LvAssigned.SelectedItems == null || View.LvAssigned.SelectedItems.Count == 0)
                return;

            string msg = "";
            BinByTask bin = null;

            foreach (Object obj in View.LvAssigned.SelectedItems)
            {
                try
                {
                    bin = (BinByTask)obj;

                    //Removerla del Server
                    service.DeleteBinByTask(bin);

                    View.Model.AssignBin.Remove((BinByTask)obj);
                    View.Model.AvailableBin.Insert(0, ((BinByTask)obj).Bin);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Bin: " + bin.Bin.BinCode + ". " + ex.Message;
                }
            }


            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            View.LvAvailable.Items.Refresh();
            View.LvAssigned.Items.Refresh();
            //View.BtnStep1.Visibility = (View.Model.AssignedDocs == null || View.Model.AssignedDocs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;


            if ((View.Model.AssignBin == null || View.Model.AssignBin.Count == 0) && View.CboToDo.IsEnabled == false)
                View.CboToDo.IsEnabled = true;

        }



        void View_AddToAssigned(object sender, EventArgs e)
        {

            //BIN
            if (View.CboToDo.SelectedIndex == 0)
            {

                if (View.LvAvailable.SelectedItems == null)
                    return;

                try
                {
                    foreach (Bin selItem in View.LvAvailable.SelectedItems)
                        AddToAssigned(selItem, null);

                    View.LvAvailable.Items.Refresh();
                }
                catch (Exception ex)
                {
                    Util.ShowError("Bin could not be assigned.\n" + ex.Message);
                }
            }
            else {

                if (View.LvAvailableProd.SelectedItems == null)
                    return;

                try
                {
                    foreach (Product selItem in View.LvAvailableProd.SelectedItems)
                        AddToAssigned(null, selItem);

                    View.LvAvailableProd.Items.Refresh();
                }
                catch (Exception ex)
                {
                    Util.ShowError("Product could not be assigned.\n" + ex.Message);
                }
            
            }

            View.LvAssigned.Items.Refresh();

        }



        private void AddToAssigned(Bin record, Product product)
        {
            if (record == null && product == null)
                return;

            try
            {
                if (View.Model.AssignBin == null)
                    View.Model.AssignBin = new List<BinByTask>();

                //Si ya esta en la lista se devuelve

                if (record != null && View.Model.AssignBin.Where(f=> f.Bin != null && f.Bin.BinID == record.BinID).Count() > 0)
                    return;

                if (product != null && View.Model.AssignBin.Where(f => f.Product != null &&  f.Product.ProductID == product.ProductID).Count() > 0)
                    return;


                //Cuando es un conteo de PRODUCT asocia el BIN MAIN
                if (countType == 1)
                    record = service.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = App.curLocation }).First();


                if (App.curLocation.LocationID != View.Model.Document.Location.LocationID)
                {
                    Util.ShowError("Location for the document " + View.Model.Document.Location.Name + ". Is different to the current location " + App.curLocation.Name);
                    return;
                }


                if (record.Location.LocationID != View.Model.Document.Location.LocationID)
                {
                    Util.ShowError("Location for the document " + View.Model.Document.Location.Name + ". Is different to the bin location " + record.Location.Name);
                    return;
                }



                //Crea el BinTask en el server
                BinByTask binByTask = new BinByTask {
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Bin = record,
                    Product = product,
                    TaskDocument = View.Model.Document,
                    ProductDesc = (countType == 1) ? product.FullDesc : "Any Product",
                    BinDesc = (countType == 0) ? record.BinCode : "Any Bin",
                    Status = new Status { StatusID = DocStatus.New }
                };

                service.SaveBinByTask(binByTask);


                View.Model.AssignBin.Insert(0, binByTask);


                if (View.CboToDo.SelectedIndex == 0)
                    View.Model.AvailableBin.Remove(record);
                else
                    View.Model.AvailableProduct.Remove(product);


                if (View.Model.AssignBin != null && View.Model.AssignBin.Count > 0 && View.CboToDo.IsEnabled == true)
                    View.CboToDo.IsEnabled = false;


                //View.BtnStep1.Visibility = (View.Model.AssignedDocs == null || View.Model.AssignedDocs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        void View_FilterByBin(object sender, DataEventArgs<string> e)
        {
            ProcessWindow pw = new ProcessWindow("Loading Bins ... ");
            //Obtiene el rango de Bins
            String[] binRange = e.Value.Split(':');
            try
            {
                // CAA [2010/07/07] Nuevos filtros de busqueda
                DictionaryEntry operatorItem=(DictionaryEntry)View.BFilters.cboStrComp.SelectedItem;
                switch (operatorItem.Key.ToString())
                {
                    case "between (range)":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID && f.BinCode.ToUpper().CompareTo(binRange[0].ToUpper()) >= 0).ToList();
                        View.Model.AvailableBin = View.Model.AvailableBin.Where(f => f.BinCode.ToUpper().CompareTo(binRange[1].ToUpper()) <= 0).ToList();
                        break;
                    case "endswith":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID  && f.BinCode.ToUpper().EndsWith(binRange[0].ToUpper())).ToList();
                        break;
                    case "startswith":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID  && f.BinCode.ToUpper().StartsWith(binRange[0].ToUpper())).ToList();
                        break;
                    case "contains":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID  &&  f.BinCode.ToUpper().Contains(binRange[0].ToUpper())).ToList();
                        break;
                    case "equal":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => f.Location.LocationID == App.curLocation.LocationID  && f.BinCode.ToUpper().Equals(binRange[0].ToUpper())).ToList();
                        break;
                    case "notcontains":
                        View.Model.AvailableBin = oriAvailableBin.Where(f => !f.BinCode.ToUpper().Contains(binRange[0].ToUpper()) && f.Location.LocationID == App.curLocation.LocationID).ToList();
                        break;
                    default:
                        break;
                        

                }
                 
                //   View.Model.AvailableBin = oriAvailableBin.Where(f => f.BinCode.ToUpper().CompareTo(binRange[0].ToUpper()) >= 0).ToList();
                //   View.Model.AvailableBin = View.Model.AvailableBin.Where(f => f.BinCode.ToUpper().CompareTo(binRange[1].ToUpper()) <= 0).ToList();
                // HideBins();

                View.LvAssigned.Items.Refresh();
                View.LvAvailable.Items.Refresh();

            }
            catch { }
            finally { pw.Close(); }
            
        }

        protected void HideBins()
        {
            if (View.ChkHideBin.IsChecked.Value)
            {
                docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Task } };
                docType.DocTypeID = SDocType.CountTask;

                // DocStatus = DocStatus.Completed,  ????
                Document doc = new Document { 
                    DocType = docType, 
                    Notes = "0", 
                    DocStatus = new Status{StatusID = DocStatus.New}, 
                    Company = App.curCompany, 
                    Location = App.curLocation };

                // recorremos lista de los Bines Pre-seleccionados
                // lista temporal  ???
                IList<Bin> binSelecteds = new List<Bin>();
                foreach (Bin binSel in View.Model.AvailableBin)
                {
                    binSelecteds.Add(binSel);
                }

                foreach (Bin binSel in binSelecteds)
                {
                    if (service.GetBinByTask(new BinByTask
                    {
                        TaskDocument = doc,
                        Status = new Status { StatusID = DocStatus.New }, // y el 103? ??
                        Bin = new Bin { BinID = binSel.BinID }
                    }
                    ).Count() > 0)      // ya existe en otro doc.
                    {
                        View.Model.AvailableBin.Remove(binSel);
                    }
                }
            }
        }

        void View_FilterByProduct(object sender, DataEventArgs<Product> e)
        {
            ProcessWindow pw = new ProcessWindow("Loading Products ... ");

            try
            {
                View.Model.AvailableProduct = service.GetProductApp(e.Value, 300);
               
                View.LvAssigned.Items.Refresh();
                View.LvAvailableProd.Items.Refresh();

            }
            catch { }
            finally { pw.Close(); }
        }

        void OnChangeSendOption(object sender, EventArgs e)
        {
            if (View.CboSendOptions.SelectedIndex == 2)    // send product to some Bin
                View.StkUcBin.Visibility = Visibility.Visible;
            else
                View.StkUcBin.Visibility = Visibility.Collapsed;
        }

        void OnUnSelectAll(object sender, EventArgs e)
        {
            markList(false);
        }

        void OnSelectAll(object sender, EventArgs e)
        {
            markList(true);
        }

        protected void markList (bool mark)
        {
            for (int i = 0; i < View.Model.NoCountSummary.Count; i++)
                View.Model.NoCountSummary[i].Mark = mark;
            View.LvSummNoCount.Items.Refresh();

        }

    }
}
