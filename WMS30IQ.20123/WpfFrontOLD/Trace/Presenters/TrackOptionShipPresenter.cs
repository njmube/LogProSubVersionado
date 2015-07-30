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
//using System.Windows.Controls.Primitives;

namespace WpfFront.Presenters
{

    public interface ITrackOptionShipPresenter
    {
        ITrackOptionShipView View { get; set; }
        void SetupManualTrackOption();
        ToolWindow Window { get; set; }
    }
    

    public class TrackOptionShipPresenter : ITrackOptionShipPresenter
    {
        public ITrackOptionShipView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        ProcessWindow pw = null;
        public ToolWindow Window { get; set; }


        public TrackOptionShipPresenter(IUnityContainer container, ITrackOptionShipView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            //this.region = region;
            View.Model = this.container.Resolve<TrackOptionShipModel>();

            //Event Delegate
            View.LoadQuantity += new EventHandler<DataEventArgs<Label>>(View_LoadQuantity);
            View.PickToList += new EventHandler<EventArgs>(View_PickToList);
            View.LoadSetup += new EventHandler<EventArgs>(View_LoadSetup);

            View.PickUniqueLabel += new EventHandler<DataEventArgs<string>>(View_PickUniqueLabel);
            View.RemoveUniqueTrack += new EventHandler<EventArgs>(View_RemoveUniqueTrack);

        }




        void View_LoadSetup(object sender, EventArgs e)
        {

            //Seting Values
            View.Model.TrackData = View.Model.Product.ProductTrack
                .Where(f=>f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).ToList();
            View.PickQty.Text = "";

            try
            {
                //Debe sacar del balance del documento para ese producto the QTY Remain y la unidad.
                DocumentBalance prdBal = service.GetDocumentBalance(
                    new DocumentBalance
                    {
                        Product = View.Model.Product,
                        Document = View.Model.Document,
                        Node = View.Model.Node
                    }, false).First();

                View.Model.CurQtyPending = (int)prdBal.QtyPending;
                View.Model.CurUnit = prdBal.Unit;
            }
            catch { }
            //View.Model.QtyToTrack = View.Model.CurQtyPending;

            SetupManualTrackOption();
        }



        //23 mArzo 09 - se separo comp un procedo atomico,para reuso.
        //permite cargar las opciones de track del producto actual, cuando elproducto esta
        //recibido carga las opciones de los recibidos.
        //APLica solo para Picking 12/SEP/2009
        public void SetupManualTrackOption()
        {

            if (View.Model.Product == null) //Solo controla el producto en picking
            {
                Util.ShowError("Product not Selected.");
                return;
            }

            ProcessWindow("Loading Tracking Options ...", false);

            try
            {
                View.StkNonUnique.Visibility = Visibility.Collapsed;
                View.StkUnique.Visibility = Visibility.Collapsed;

                View.Model.TrackData = View.Model.Product.ProductTrack
                    .Where(f => f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).ToList();

                View.Model.RemainingQty = View.Model.CurQtyPending;


                //SI ES UNICO 
                if (View.Model.Product.ProductTrack.Any(f => f.IsUnique == true))
                {
                    View.StkUnique.Visibility = Visibility.Visible;
                    View.Model.MaxTrackQty = 1;
                    LoadCurrentLabels();
                }
                else
                {
                    View.StkNonUnique.Visibility = Visibility.Visible;

                    //Boton de Pick
                    ResetPickBtn();

                    //Cargando la lista de disponibles
                    LoadLabelsAvailable();


                    //Load Label List Columns
                    IList<ProductTrackRelation> listTrack;

                    //1. Se arma el subcontrol que pide dato y el listview que muestra la info.

                    System.Windows.Controls.GridViewColumn[] grid =
                        new System.Windows.Controls.GridViewColumn[((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Count];
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.CopyTo(grid, 0);

                    int curColumn = 0;
                    int offset = 2; //Numero de columnas fijas
                    //Arma le grid que pedira los datos de Tracking
                    foreach (System.Windows.Controls.GridViewColumn col in grid)
                    {

                        //si es el barcode pero estamos adicionando se remueve el barcode de las columnas
                        if (col.Header.ToString().Contains("Bin"))
                            continue;


                        //si es la columan qty, continua porque siempre se meustra
                        if (col.Header.ToString().Contains("Quantity"))
                            continue;


                        listTrack = View.Model.TrackData; //.Where(f => f.TrackOption.Name == col.Header.ToString()).ToList();


                        if (curColumn < View.Model.TrackData.Count)
                        {
                            //Cambia el Header  y el with
                            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Header = listTrack[curColumn].TrackOption.DisplayName;

                            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Width = 130;

                        }
                        else  //Si ya estan todos los elementos del trackData Remueve las demas columnas.
                            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Remove(col);


                        curColumn++;
                    }


                    ////2. Detectanto si hay alguna track unique que restrinja el maximo qty a entrar en 1 each.
                    //for (int i = 0; i < View.Model.TrackData.Count; i++)
                    //    if (View.Model.TrackData[i].IsUnique == true)
                    //    {
                    //        View.Model.MaxTrackQty = 1;
                    //        break;
                    //    }

                }


                pw.Close();
            }
            catch { pw.Close(); }


        }




        #region NON UNIQUE PRODUCT

        void View_PickToList(object sender, EventArgs e)
        {
            //Validar que no supere el remaining
            if (View.Model.PickQuantity > View.Model.RemainingQty)
            {
                Util.ShowError("Qty Remaining is lower than Qty Requested.");
                return;
            }

            double qtyToPick = 0;

            double.TryParse(View.PickQty.Text, out qtyToPick);

            PickProduct(qtyToPick);

        }


        private void PickProduct(Double qtyToPick)
        {
            ProcessWindow("Picking Product ...", false);

            try
            {
                Label label = View.ManualTrackList.SelectedItem as Label;

                Label pickedLabel = service.PickProductWithTrack(View.Model.Document, label, qtyToPick,
                    View.Model.Node, App.curUser, View.Model.FatherPresenter.View.Model.CurPackage);

                if (View.Model.ManualTrackPicked == null)
                    View.Model.ManualTrackPicked = new List<Label>();

                View.Model.ManualTrackPicked.Add(pickedLabel);

                //Dismiuir el remainig
                View.Model.RemainingQty -= (int)qtyToPick;


                //limpiar Boxes
                View.PickQty.Text = "";
                ((Label)View.ManualTrackList.SelectedItem).CurrQty -= qtyToPick;

                ResetPickBtn();
                AfterPicking();
            }

            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
                return;
            }
            finally { pw.Close();  }
        }


        private void AfterPicking()
        {
            View.Model.FatherPresenter.refreshBalance = true;
        } 


        //Pone la Cantidad En el campo a piquear
        void View_LoadQuantity(object sender, DataEventArgs<Label> e)
        {
            if (e.Value == null)
                return;

            if (e.Value.CurrQty <= 0)
                return;

            Label lbl = e.Value;

            if (lbl.CurrQty < View.Model.RemainingQty)
                View.PickQty.Text = lbl.CurrQty.ToString();
            else
                View.PickQty.Text = View.Model.RemainingQty.ToString();


        }


        /// <summary>
        /// Obtiene los labels disponibles de ese producto
        /// </summary>
        private void LoadLabelsAvailable()
        {

            Label searchLabel = new Label
            {
                Node = new Node { NodeID = NodeType.Stored },
                Status = new Status { StatusID = EntityStatus.Active },
                Bin = new Bin { Location = App.curLocation },
                Product = View.Model.Product,
                Printed = false
            };

            View.Model.ManualTrackList = service.GetLabel(searchLabel);

        }


        private void ResetPickBtn()
        {
            View.BtnPick.IsEnabled = false;
            if (View.Model.RemainingQty > 0)
                View.BtnPick.IsEnabled = true;
        }


        #endregion


        #region UNIQUE PRODUCT


        void View_PickUniqueLabel(object sender, DataEventArgs<string> e)
        {
            ProcessWindow("Picking Product ...", false);

            //Piquear el label unico.
            try
            {
                Label pickedLabel = service.PickUniqueLabel(View.Model.Document, View.Model.Node,
                    View.Model.Product, e.Value, App.curUser, View.Model.FatherPresenter.View.Model.CurPackage);

                View.Model.RemainingQty -= (int)pickedLabel.StockQty;
                LoadCurrentLabels();

                /*
                if (pickedLabel != null && pickedLabel.Count > 0)
                {

                    if (View.Model.UniqueTrackList == null)
                        View.Model.UniqueTrackList = new List<Label>();

                    foreach (Label lbl in pickedLabel)
                        View.Model.UniqueTrackList.Add(lbl);

                    View.Model.RemainingQty -= pickedLabel.Count;
                }
                View.UniqueTrackList.Items.Refresh();
                */

                AfterPicking();

            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
                return;
            }
            finally { View.TxtUnique.Text = ""; pw.Close(); }



        }



        void View_RemoveUniqueTrack(object sender, EventArgs e)
        {
            if (View.UniqueTrackList.SelectedItems == null)
                return;

            ProcessWindow("Remove Picking ...", false);

            try
            {

                foreach (object lineObject in View.UniqueTrackList.SelectedItems)
                {
                    View.Model.UniqueTrackList.Remove((Label)lineObject);

                    service.UnPickUniqueLabel(View.Model.Document, (Label)lineObject, App.curUser);

                    View.Model.RemainingQty++;
                }

                View.UniqueTrackList.Items.Refresh();
                AfterPicking();
            }
            finally { pw.Close(); }
        }



        private void LoadCurrentLabels()
        {
            View.Model.UniqueTrackList = service.GetLabel(new Label
            {
                Product = View.Model.Product,
                ShippingDocument = View.Model.Document,
                LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
                Node = new Node { NodeID = NodeType.Picked }
            });

            View.UniqueTrackList.Items.Refresh();

        }


        #endregion


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore)
        {
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }

    }
}
