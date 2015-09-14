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
using System.Windows;
using System.Collections.Specialized;
using System.Reflection;

namespace WpfFront.Presenters
{


    public interface IChangeLocationPresenter
    {
        IChangeLocationView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class ChangeLocationPresenter : IChangeLocationPresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public IChangeLocationView View { get; set; }
        private bool showNextTime = true;
        public ToolWindow Window { get; set; }

        public ChangeLocationPresenter(IUnityContainer container, IChangeLocationView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ChangeLocationModel>();

            //Event Delegate

            View.LoadBins += new EventHandler<EventArgs>(View_LoadSourceContent);
            View.LoadBinsD += new EventHandler<EventArgs>(View_LoadDestination);
            //View.SearchDocument += new EventHandler<DataEventArgs<string>>(this.OnSearchDocument);
            View.MoveAll += new EventHandler<EventArgs>(View_MoveAll);
            View.MoveSelected += new EventHandler<EventArgs>(View_MoveSelected);
            View.MoveRetail += new EventHandler<EventArgs>(View_MoveRetail);

            //View.BinLocation.Focus();

        }

        void View_MoveRetail(object sender, EventArgs e)
        {
            MoveProduct();
        }


        void View_LoadSourceContent(object sender, EventArgs e)
        {
            LoadSourceContent(false);
        }


        private void LoadSourceContent(bool forced)
        {

            if (View.Model.SourceLocation != null && View.BinLocation.Text == View.Model.SourceLocation.Barcode && !forced)
                return;

            View.StkLabel.Visibility = Visibility.Collapsed;
            View.StkRetail.Visibility = Visibility.Collapsed;
            View.BtnMoveLabel.Visibility = Visibility.Collapsed;
            View.StkQtyRetail.Visibility = Visibility.Collapsed;
            View.Model.WithContent = false;
            View.Model.LabelsToProcess = new List<Label>();            


            //Obtiene lo labels sin padre y el producto suelto que este en el BIN

            //Obtener el Bin
            View.Model.SourceLocation = service.GetLocationData(View.BinLocation.Text, false);

            if (View.Model.SourceLocation == null)
            {
                Util.ShowError(Util.GetResourceLanguage("BIN/LABEL_SOURCE") + " " + View.BinLocation.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                View.BinLocation.Text = "";
                return;
            }


            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("LOADING_STOCK_FOR") + View.BinLocation.Text + " ...");


            //Carga los labels contenidos en el Bin o el labels
            LoadSourceLabels();


            //Obtener el producto Suelto solo cuando es tipo 
            if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.BinLocation)
                LoadRetailStock();


            if (View.Model.WithContent == false)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("THE_LOCATION") + View.BinLocation.Text + Util.GetResourceLanguage("DOES_NOT_CONTAIN_STOCK"));
            }
            else
            {
                View.BrDestination.Visibility = Visibility.Visible;
                //View.BinLocationD.Focus();
                pw.Close();
            }
        }


        private void LoadRetailStock()
        {
            View.Model.LinesToProcess = service.GetBinStock(new ProductStock { Bin = View.Model.SourceLocation.Bin })
                .Where(f => f.Stock > 0).OrderBy(f => f.Product.ProductCode).ToList();

            if (View.Model.LinesToProcess != null && View.Model.LinesToProcess.Count > 0)
            {
                View.StkRetail.Visibility = Visibility.Visible;
                View.StkQtyRetail.Visibility = Visibility.Visible;
                View.Model.WithContent = true;
            }
        }


        private void LoadSourceLabels() {

            View.Model.LinesToProcess = new List<ProductStock>();

            if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.BinLocation)
            {

                //Obtener el producto empcadado en cajas
                Label patternLabel = new Label
                {
                    Bin = View.Model.SourceLocation.Bin,
                    //IsLogistic = true,
                    Printed = true,
                    FatherLabel = new Label { LabelID = -1},
                    Status = new Status { StatusID = EntityStatus.Active },
                    Node = new Node { NodeID = NodeType.Stored },
                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel }
                };


                View.Model.LabelsToProcess = service.GetLabel(patternLabel)
                    .Where(f => f.StockQty > 0).OrderByDescending(f => f.LabelID).ToList();

            }

            else if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel) //Cuando tiene Seriales
            {

                //Obtener el producto empcadado en cajas
                Label patternLabel = new Label
                {
                    Status = new Status { StatusID = EntityStatus.Active },
                    Node = new Node { NodeID = NodeType.Stored },
                    FatherLabel = View.Model.SourceLocation
                };

                View.Model.LabelsToProcess = service.GetLabel(patternLabel)
                    .Where(f => f.CurrQty > 0).OrderByDescending(f => f.LabelID).ToList();

            }




            //Muestra El contenido interno del label -- Para moverlo como retail
            if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel && View.Model.SourceLocation.StockQty > 0 ) //&& View.Model.WithContent
            {
                View.Model.WithContent = true;

               //Si es un label, se muestra a si mismo para poder meoverse completo.
                View.Model.LabelsToProcess.Add(View.Model.SourceLocation);
                View.LvLabelsToMove.Items.Refresh();

                if (View.Model.SourceLocation.CurrQty > 0)
                {

                    ProductStock ps = new ProductStock
                    {
                        Product = View.Model.SourceLocation.Product,
                        Stock = View.Model.SourceLocation.StockQty,
                        Unit = View.Model.SourceLocation.Product.BaseUnit
                    };

                    View.Model.LinesToProcess.Add(ps);
                    View.LvProductToMove.Items.Refresh();
                }
                
                View.StkRetail.Visibility = Visibility.Visible;
                View.StkQtyRetail.Visibility = Visibility.Visible;
                

            }


            if (View.Model.LabelsToProcess != null && View.Model.LabelsToProcess.Count > 0)
            {
                View.StkLabel.Visibility = Visibility.Visible;
                View.BtnMoveLabel.Visibility = Visibility.Visible;
                View.Model.WithContent = true;
            }




        }


        //private void OnSearchDocument(object sender, DataEventArgs<string> e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(e.Value))
        //        {
        //            //Load Document List
        //            DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Receiving } };
        //            View.Model.DocumentList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany }, 0, 0); //service.GetReceivingDocument(WmsSetupValues.NumRegs);

        //            //View.Model.DocumentList = service.GetReceivingDocument(WmsSetupValues.NumRegs);
        //            return;
        //        }

        //        View.Model.DocumentList = service.SearchDocument(e.Value);

        //        //if (View.DocumentList.SelectedIndex == -1)
        //        //    View.Model.LinesToProcess = null;

        //    }
        //    catch { }
        //}


        private void LoadDestination()
        {

            if (View.Model.DestLocation != null && View.BinLocationD.Text == View.Model.DestLocation.Barcode)
                return;



            View.BtnMoveAll.Visibility = Visibility.Collapsed;

            //Obtener el Bin
            View.Model.DestLocation = service.GetLocationData(View.BinLocationD.Text, false);
            if (View.Model.DestLocation == null)
            {
                Util.ShowError(Util.GetResourceLanguage("BIN/LABEL_DESTINATION") + " " + View.BinLocationD.Text + Util.GetResourceLanguage("IS_NOT_VALID"));
                View.BinLocationD.Text = "";
                View.BrMove.Visibility = Visibility.Collapsed;
                View.StkMovedData.Visibility = Visibility.Hidden;
                View.BinLocationD.Focus();
                return;
            }

            if (View.Model.SourceLocation.LabelID == View.Model.DestLocation.LabelID)
            {
                Util.ShowError(Util.GetResourceLanguage("SOURCE_AND_DESTINATION_ARE_THE_SAME"));
                View.BinLocationD.Text = "";
                View.BinLocationD.Focus();
                return;
            }


            View.BrMove.Visibility = Visibility.Visible;
            View.StkMovedData.Visibility = Visibility.Visible;
            View.Model.WithDest = true;
            View.Model.LabelsMoved = new List<Label>();
            View.Model.LinesMoved = new List<ProductStock>();


            //Si el destino es un BIN y el Origen es un bin deja realizar el BIN Consolidation
            if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.BinLocation
                && View.Model.SourceLocation.LabelType.DocTypeID == View.Model.DestLocation.LabelType.DocTypeID)
            {
                View.BtnMoveAll.Visibility = Visibility.Visible;

                if (View.Model.DestLocation.Bin.Process != null)
                {
                    Util.ShowMessage(Util.GetResourceLanguage("THE_BIN") + View.Model.DestLocation.LabelCode + Util.GetResourceLanguage("IS_A_PROCESS_BIN_THE_PROCESS") + " [" + View.Model.DestLocation.Bin.Process.Name + "] " + Util.GetResourceLanguage("WILL_BE_EXECUTED_AFTER_TRANSACTION_BE_COMPLETED"));
                }
            }

        }


        void View_LoadDestination(object sender, EventArgs e)
        {
            LoadDestination();
        }


        void View_MoveSelected(object sender, EventArgs e)
        {
            if (View.LvLabelsToMove.SelectedItem == null)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_RECORDS_IN_THE_LABELED_PRODUCT_LIST"));
                return;
            }

            if (View.Model.SourceLocation.LabelID == View.Model.DestLocation.LabelID)
            {
                Util.ShowError(Util.GetResourceLanguage("SOURCE_AND_DESTINATION_ARE_THE_SAME"));
                View.BinLocationD.Text = "";
                View.BinLocationD.Focus();
                View.Model.DestLocation = null;
                return;
            }

            //if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel &&
            //    View.Model.DestLocation.LabelType.DocTypeID != LabelType.BinLocation)
            //{
            //    Util.ShowError("Label can be moved only to a Bin.\n" + View.Model.DestLocation.Barcode + " is not a Bin.");
            //    return;
            //}


            MovingLabeledProduct();

            ResetToNext();

            if (showNextTime)
                showNextTime = UtilWindow.ConfirmResult("Process Completed.");
         


        }



        private void MovingLabeledProduct()
        {
                        //recorre la lista de labels y producto seleccionado y lo mueve al destino
            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("MOVING_LABELED_PRODUCT"));
            string error = "";
            Label curLabel = null;

            foreach (Label label in View.LvLabelsToMove.SelectedItems)
            {
                try {
                
                    label.ModifiedBy = App.curUser.UserName;
                    curLabel = service.ChangeLabelUbication(label, View.Model.DestLocation);

                    if (!curLabel.Notes.Contains("Change OK!"))
                        error += label.LabelCode + ": " + curLabel.Notes + "\n";
                        //error += "On label " + label.LabelCode + " " + curLabel.Notes + "\n";
                   
                }
                catch (Exception ex)
                {
                    error += Util.GetResourceLanguage("FOR_LABEL") + label.LabelCode + ": " + ex.Message + "\n"; ;
                    continue;
                }

                //Add in Moved List
                View.Model.LabelsMoved.Add(label);

                //Remove from Original List
                View.Model.LabelsToProcess.Remove(label);
            }


            //refresh
            //View.LvLabelsToMove.Items.Refresh();
            //View.LvLabelsMoved.Items.Refresh();

            //recarga los source labels
            //if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel)
                //LoadSourceLabels();


            //ResetToNext();

            pw.Close();


            if (!string.IsNullOrEmpty(error))
                Util.ShowError(Util.GetResourceLanguage("PROCESS_COMPLETED_WITH_ERRORS") + "\n\n" + error);

       
        }


        void View_MoveAll(object sender, EventArgs e)
        {

            if (View.Model.SourceLocation.LabelID == View.Model.DestLocation.LabelID)
            {
                Util.ShowError(Util.GetResourceLanguage("SOURCE_AND_DESTINATION_ARE_THE_SAME"));
                View.BinLocationD.Text = "";
                View.BinLocationD.Focus();
                return;
            }


            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("MOVING_ALL_BIN_STOCK_TO") + View.Model.DestLocation.Barcode + " ... ");

            //Selecting all Labels
            if (View.Model.LabelsToProcess != null && View.Model.LabelsToProcess.Count > 0)
            {
                View.LvLabelsToMove.SelectAll();
                MovingLabeledProduct();
            }

            if (View.Model.LinesToProcess != null && View.Model.LinesToProcess.Count > 0)
            {
                foreach (ProductStock record in View.Model.LinesToProcess)
                    //Moviendo el producto suelto.
                    MovingReatilProduct(record, record.Stock, true);
                
            }

            //refresh
            //LoadRetailStock();
            //View.LvProductToMove.Items.Refresh();
            //View.LvProductMoved.Items.Refresh();

            pw.Close();

            ResetToNext();

            if (showNextTime)           
                showNextTime = UtilWindow.ConfirmResult("Process Completed.");
            
            
        }


        private void MoveProduct()
        {

            if (View.LvProductToMove.SelectedItem == null)
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_SELECT_A_RECORD_IN_THE_UNLABELED_PRODUCT_LIST"));
                return;
            }

            if (View.Model.SourceLocation.LabelID == View.Model.DestLocation.LabelID)
            {
                Util.ShowError(Util.GetResourceLanguage("SOURCE_AND_DESTINATION_ARE_THE_SAME"));
                View.BinLocationD.Text = "";
                View.BinLocationD.Focus();
                return;
            }

            ProductStock record = View.LvProductToMove.SelectedItem as ProductStock;

            double qty;
            if (!double.TryParse(View.TxtQty.Text, out qty))
            {
                Util.ShowError(Util.GetResourceLanguage("PLEASE_ENTER_A_VALID_QUANTITY"));
                return;
            }

            if (qty > record.Stock)
            {
                Util.ShowError(Util.GetResourceLanguage("QTY_TO_MOVE_IS_GREATHER_THAN_AVAILABLE"));
                View.TxtQty.Text = record.Stock.ToString();
                return;
            }

            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("MOVING_UNLABELED_PRODUCT"));

            MovingReatilProduct(record, qty, false);


            View.LvProductToMove.SelectedItem = null;

            //recarga los source labels
            if (View.Model.SourceLocation.LabelType.DocTypeID == LabelType.ProductLabel)
                LoadSourceLabels();

            //refresh
            View.LvProductToMove.Items.Refresh();
            View.LvProductMoved.Items.Refresh();


            pw.Close();


            ResetToNext();

            if (showNextTime)
                showNextTime = UtilWindow.ConfirmResult("Process Completed.");



            
            
        }


        private void ResetToNext()
        {
            View.Model.DestLocation = null;
            View.BinLocationD.Text = "";
            View.TxtQty.Text = "";
            View.BrMove.Visibility = Visibility.Collapsed;
            View.StkMovedData.Visibility = Visibility.Hidden;
            View.BinLocationD.Focus();
            
            //recarga el source
            LoadSourceContent(true);
            
        }


        private void MovingReatilProduct(ProductStock record, double qty, bool moveall)
        {


            DocumentLine line = new DocumentLine
            {
                Quantity = qty,
                Product = record.Product,
                Unit = record.Unit,
                CreatedBy = App.curUser.UserName
            };

            try
            {
                line = service.ChangeProductUbication(View.Model.SourceLocation, line, View.Model.DestLocation);

                if (!line.Note.Contains("Change OK!"))
                {
                    Util.ShowError(Util.GetResourceLanguage("TRANSACTION_ERROR") + "\n" + line.Note);
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("TRANSACTION_ERROR") + "\n" + ex.Message);
                ResetToNext();
                return;
            }

            //Add in Moved List
            ProductStock newRecord = new ProductStock { Bin = record.Bin, Product = record.Product, Unit = record.Unit };
            newRecord.Stock = line.Quantity;
            View.Model.LinesMoved.Add(newRecord);

            if (!moveall)
            {
                //Remove from Original List
                View.Model.LinesToProcess.Remove(record);
                record.Stock -= line.Quantity;
                if (record.Stock > 0)
                    View.Model.LinesToProcess.Add(record);
            }




       }
    }
}

