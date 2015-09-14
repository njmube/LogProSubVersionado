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
using System.Windows.Controls.Primitives;

namespace WpfFront.Presenters
{

    public interface IPackageAdminPresenter
    {
        IPackageAdminView View { get; set; }
        ToolWindow Window { get; set; }
        void LoadPackages();

    }
    

    public class PackageAdminPresenter : IPackageAdminPresenter
    {
        public IPackageAdminView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        bool allowUnpick = false;



        public PackageAdminPresenter(IUnityContainer container, IPackageAdminView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<PackageAdminModel>();

            View.LoadPackDetails += new EventHandler<DataEventArgs<DocumentPackage>>(View_LoadPackDetails);
            View.LoadPackDetails2 += new EventHandler<DataEventArgs<DocumentPackage>>(View_LoadPackDetails2);
            View.MoveRetail += new EventHandler<EventArgs>(View_MoveRetail);
            View.MoveSelected += new EventHandler<EventArgs>(View_MoveSelected);
            View.UnPickSelected += new EventHandler<EventArgs>(View_UnPickSelected);

            //if (Util.GetConfigOption("ALLOWUNPICK").Equals("T"))
                //View.BtnUnpick.Visibility = Visibility.Visible;

        }




        void View_UnPickSelected(object sender, EventArgs e)
        {

            if (View.LvDetails1.SelectedItems == null)
            {
                Util.ShowError("No detail selected");
                return;
            }


            ProductStock record = null;
            DocumentPackage curPack = View.CboPack1.SelectedItem as DocumentPackage;
            double unPickQty = 0;
            Document shipmentDoc = null;

            IEnumerable<NodeTrace> affectedLabels;
            List<NodeTrace> movedLabels = new List<NodeTrace>();

            try
            {

                foreach (Object obj in View.LvDetails1.SelectedItems)
                {
                    record = obj as ProductStock;

                    //Trae los labels de ese package
                    affectedLabels = service.GetNodeTrace(
                        new NodeTrace { 
                            Label = new Label {Product = record.Product, FatherLabel = curPack.PackLabel }, 
                            Node = new Node {NodeID = NodeType.Released},
                            PostingDocument = View.Model.Document
                        });


                    if (affectedLabels.Count() == 0)
                        continue;


                    //Actualizado los Labels Afectados
                    foreach (NodeTrace lbl in affectedLabels)
                    {
                        movedLabels.Add(lbl);
                        unPickQty += lbl.Quantity;

                        if (shipmentDoc == null)
                            shipmentDoc = lbl.PostingDocument;

                    }

                }

                //Nuevo servicio del 22 de oct de 2009
                service.ReversePickingNodeTraceByLabels(movedLabels, App.curUser, null);

            }
            catch (Exception ex)
            {
                Util.ShowError("Problem unpicking product.\n" + ex.Message);
            }

            //Create Document Line with Unpick Product 
            try {
                service.SaveDocumentLine(new DocumentLine
                {
                    Document = shipmentDoc,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Date1 = DateTime.Now,
                    IsDebit = true,
                    LineStatus = new Status { StatusID = EntityStatus.Active },
                    Note = "Unpicked product after shipment creation.",
                    Product = record.Product,
                    Quantity = unPickQty,
                    Unit = record.Unit,
                    Location = shipmentDoc.Location                    
               });
            }
            catch { }

       


            //Refreshing the details
            View.Model.PackDetails1 = service.GetLabelStock(curPack.PackLabel);
            Util.ShowMessage("Product UnPicked.");
        }



        void View_MoveSelected(object sender, EventArgs e)
        {
            ProductStock record;
            DocumentPackage curPack = View.CboPack1.SelectedItem as DocumentPackage;
            DocumentPackage newPack = View.CboPack2.SelectedItem as DocumentPackage;

            IEnumerable<Label> affectedLabels;
            IList<Label> movedLabels = new List<Label>();

            try
            {

                foreach (Object obj in View.LvDetails1.SelectedItems)
                {
                    record = obj as ProductStock;

                    //Trae los labels de ese package
                    affectedLabels = service.GetLabel(
                        new Label { FatherLabel = curPack.PackLabel, Product = record.Product });
                        //.Where(f => f.Product.ProductID == record.Product.ProductID);


                    if (affectedLabels.Count() == 0)
                        continue;

                    //Significa que es un package nuevo y hay que crearlo.
                    if (newPack.PackID == 0)
                        newPack = CreateNewPackage(curPack);


                    //Actualizado los Labels Afectados
                    foreach (Label lbl in affectedLabels)
                    {
                        lbl.FatherLabel = newPack.PackLabel;
                        lbl.ModifiedBy = App.curUser.UserName;
                        lbl.ModDate = DateTime.Now;
                        movedLabels.Add(lbl);
                    }

                }

                //Nuevo servicio del 22 de oct de 2009
                service.UpdatePackageMovedLabels(movedLabels);


            }
            catch (Exception ex)
            {
                Util.ShowError("Problem updating packages.\n" + ex.Message);
            }

            //Refreshing the details
            View.Model.PackDetails1 = service.GetLabelStock(curPack.PackLabel);
            View.Model.PackDetails2 = service.GetLabelStock(newPack.PackLabel);

        }



        private DocumentPackage CreateNewPackage(DocumentPackage curPack)
        {
            DocumentPackage newPack = service.CreateNewPackage(curPack.Document, App.curUser, true, null, "B");
            //DocumentPackage newPack = newPackLabel.DocumentPackages[0];
            newPack.PostingDocument = curPack.PostingDocument;
            newPack.PostingDate = curPack.PostingDate;
            newPack.PostingUserName = curPack.PostingUserName;
            service.UpdateDocumentPackage(newPack);

            //Add new Pack tho the list.
            View.Model.Packages2.Add(newPack);
            View.Model.Packages.Add(newPack);
            View.CboPack1.Items.Refresh();
            View.CboPack2.Items.Refresh();
            View.CboPack2.SelectedIndex = View.Model.Packages2.Count - 1;

            return newPack;
        }



        void View_MoveRetail(object sender, EventArgs e)
        {

            ProductStock record = View.LvDetails1.SelectedItem as ProductStock;
            DocumentPackage curPack = View.CboPack1.SelectedItem as DocumentPackage;
            DocumentPackage newPack = View.CboPack2.SelectedItem as DocumentPackage;

            try
            {


                double qty;
                if (!double.TryParse(View.TxtQty.Text, out qty))
                {
                    Util.ShowError("Please enter a valid quantity.");
                    return;
                }

                if (qty > record.Stock)
                {
                    Util.ShowError("Qty to move is greather than available.");
                    View.TxtQty.Text = record.Stock.ToString();
                    return;
                }



                //Significa que es un package nuevo y hay que crearlo.
                if (newPack.PackID == 0)
                    newPack = CreateNewPackage(curPack);

                //Mover la cantidad seleccionada de un Paquete a Otro.
                service.MoveQtyBetweenPackages(curPack, newPack, record.Product, qty);


            }
            catch (Exception ex)
            {
                Util.ShowError("Problem updating packages.\n" + ex.Message);
            }

            //Refreshing the details
            View.Model.PackDetails1 = service.GetLabelStock(curPack.PackLabel);
            View.Model.PackDetails2 = service.GetLabelStock(newPack.PackLabel);

        }



        public void LoadPackages()
        {
            //Load Document Packages
            View.Model.Packages = service.GetDocumentPackage(new DocumentPackage { PostingDocument = View.Model.Document });

            if (View.Model.Packages != null && View.Model.Packages.Count > 0)
            {
                View.CboPack1.SelectedIndex = 0;
                LoadPackageDetails(View.Model.Packages[0]);
            }
        }


        void View_LoadPackDetails(object sender, DataEventArgs<DocumentPackage> e)
        {
            if (e.Value == null)
                return;

            LoadPackageDetails(e.Value);
        }


        private void LoadPackageDetails(DocumentPackage docPackage)
        {
            View.Model.PackDetails1 = service.GetLabelStock(docPackage.PackLabel);
            View.Model.Packages2 = View.Model.Packages.Where(f => f.PackID != docPackage.PackID).ToList();

            View.Model.Packages2.Add(new DocumentPackage { PackDesc = "New Package ...", PackLabel = new Label() });
            View.CboPack2.Items.Refresh();
        }


        void View_LoadPackDetails2(object sender, DataEventArgs<DocumentPackage> e)
        {
            View.Model.PackDetails2 = service.GetLabelStock(e.Value.PackLabel);
        }
       
    }
}
