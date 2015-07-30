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
using WpfFront.Services;
using System.ComponentModel;
using WpfFront.Presenters;
using System.Windows.Controls.Primitives;


namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for AdminPackagesV2.xaml
    /// </summary>
    public partial class AdminPackagesV2 : UserControl, INotifyPropertyChanged
    {

        IList<DocumentPackage> oriPacks;
        Document curDoc = null;
        Document curPosted = null;
        DocumentPackage newPack;
        bool isChild = false;
        int curPieces = 0;
        DocumentPackage rootPackage = null;


        //Manage for fase 2
        //Level -1, 0, 1, 2 //restrict level 3
        //PackagePath PLT1 # 102047
        //SubSequence
        //CurrentDesc PLT1/BOX1

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


        public AdminPackagesV2()
        {
            InitializeComponent();
            base.DataContext = this;
        }



        internal void LoadPackages(Document document, Document posted)
        {
            //Bloque de los botones si ya es un shipment completado y la opcion de 
            //reorder esta en false

            btnAddSerial.IsEnabled = true;
            btnMove.IsEnabled = true;
            btnMoveSN.IsEnabled = true;
            btnMoveRetail.IsEnabled = true;
            btnNew.IsEnabled = true;
            btnNewBox.IsEnabled = true;
            btnMovePack.IsEnabled = true;


            if (posted != null && Util.GetConfigOption("ALWREORDERPK").Equals("F"))
            {
                btnAddSerial.IsEnabled = false;
                btnMove.IsEnabled = false;
                btnMoveSN.IsEnabled = false;
                btnMoveRetail.IsEnabled = false;
                btnNew.IsEnabled = false;
                btnNewBox.IsEnabled = false;
                btnMovePack.IsEnabled = false;
            }

            WpfFront.WMSBusinessService.Label shipmentLabel;
            btnCreateShipment.Visibility = Visibility.Visible;
            brMove.Visibility = Visibility.Collapsed;


            sourceTree.Items.Refresh();
            destTree.Items.Refresh();

            Packages = null;



            if (posted == null)
            {

                //SEP 20 - 2010 - Jairo Murillo
                //Aqui Revisa si el Shipment number debe ser preasignado
                if (Util.GetConfigOption("PRESHIPMENT").Equals("T"))
                {
                    if (string.IsNullOrEmpty(document.PostingDocument))
                    { //Obtain Sequence                   
                        document.PostingDocument = service.GetNextDocSequence(document.Company, new DocumentType { DocTypeID = SDocType.SalesShipment }).CodeSequence; 
                        service.UpdateDocument(document);
                    }
                    shipmentLabel = new WpfFront.WMSBusinessService.Label { LabelCode = "Root of " + document.PostingDocument };
                }
                else
                    shipmentLabel = new WpfFront.WMSBusinessService.Label { LabelCode = "Root of " + document.DocNumber };

            }
            else //Shipment ya creado y posteado
            {
                btnCreateShipment.Visibility = Visibility.Collapsed;
                shipmentLabel = new WpfFront.WMSBusinessService.Label { LabelCode = "Root of " + posted.DocNumber };
            }


            Packages = new List<DocumentPackage>() { 
                new DocumentPackage { Document = document, 
                    PostingDocument = posted,
                    PostingDate = posted == null ? null : posted.CreationDate,
                    PostingUserName =posted == null ? null : posted.CreatedBy,
                    PackLabel = shipmentLabel,
                    PackDescExt = shipmentLabel.LabelCode,
                    Sequence = 1,
                    PackageType = "R"
                }
            };


            curDoc = document;
            curPosted = posted;

            LoadTrees();
        }



        #region UC MODEL

        private IList<DocumentPackage> packages;
        public IList<DocumentPackage> Packages
        {
            get { return packages; }
            set
            {
                packages = value;
                OnPropertyChanged("Packages");
            }
        }


        private IList<ProductStock> _PackDetails1;
        public IList<ProductStock> PackDetails1
        {
            get { return _PackDetails1; }
            set
            {
                _PackDetails1 = value;
                OnPropertyChanged("PackDetails1");
            }
        }


        private DocumentPackage _curPack;
        public DocumentPackage curPack
        {
            get { return _curPack; }
            set
            {
                _curPack = value;
                OnPropertyChanged("curPack");
            }
        }


        private IList<WpfFront.WMSBusinessService.Label> _PackDetailsSN;
        public IList<WpfFront.WMSBusinessService.Label> PackDetailsSN
        {
            get { return _PackDetailsSN; }
            set
            {
                _PackDetailsSN = value;
                OnPropertyChanged("PackDetailsSN");
            }
        }


        private ShippingPresenter _parentPresenter;
        public ShippingPresenter parentPresenter
        {
            get { return _parentPresenter; }
            set
            {
                _parentPresenter = value;
            }
        }



        #endregion



        private void LoadTrees()
        {
            DocumentPackage target = new DocumentPackage
            {
                //PostingDocument = curPosted != null ? curPosted : new Document { DocID = -1 },
                Document = curDoc,
                ParentPackage = new DocumentPackage { PackID = -1 }
            };

            if (Util.GetConfigOption("ONESHIPMENT").Equals("T") && curPosted == null)
                target.PostingDocument = null;
            else
                target.PostingDocument = curPosted != null ? curPosted : new Document { DocID = -1 };


            IList<DocumentPackage> oriPacks = service.GetDocumentPackage(target);

            if (oriPacks == null || oriPacks.Count == 0)
                return;

            //Solo me trae los que no sean el ROOT y en el document carga el ROOT.
            Packages[0].ChildPackages = oriPacks.Where(f=>f.PackageType != "R").ToList();

            rootPackage = oriPacks.Where(f => f.PackageType == "R").First();

            sourceTree.Items.Refresh();
            destTree.Items.Refresh();
        }


        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (sourceTree.SelectedItem == null)
            {
                Util.ShowError("No source package selected to create the new package inside.");
                return;
            }

            curPack = sourceTree.SelectedItem as DocumentPackage;

            newPack = new DocumentPackage
            {
                ParentPackage = curPack,
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                Document = curPack.Document

            };

            string pkgType = "B";
            if (((Button)sender).Name == "btnNew")
                pkgType = "P";


            //Adicionar el Package a la vista original
            newPack = service.CreateNewPackage(curPack.Document, App.curUser, true, curPack, pkgType);

            newPack.PostingDocument = curPack.PostingDocument;
            newPack.PostingDate = curPack.PostingDate;
            newPack.PostingUserName = curPack.PostingUserName;
            
            //Adicionar info para el arbol en el device
            //Level -1, 0, 1, 2 //restrict level 3
            //PackagePath PLT1 # 102047
            //SubSequence
            //CurrentDesc PLT1/BOX1
            if (newPack.ParentPackage == null)
            {
                newPack.Level = 0;
                newPack.SubSequence = Packages[0].ChildPackages.Count+1;
               
                try
                {
                    if (newPack.PackageType == "P") {
                        newPack.CurrentDesc = "PLT" + newPack.SubSequence.ToString();
                        newPack.PackDescExt = "PLT" + newPack.SubSequence.ToString() + " # " + newPack.PackLabel.LabelCode;
                        
                    }
                    else if (newPack.PackageType == "B") {
                        newPack.CurrentDesc = "BOX" + newPack.SubSequence.ToString();
                        newPack.PackDescExt = "BOX" + newPack.SubSequence.ToString() + " # " + newPack.PackLabel.LabelCode;
                    }
                }
                catch { }
            }
            else
            {
                newPack.Level = newPack.ParentPackage.Level + 1;

                try { newPack.SubSequence = newPack.ParentPackage.ChildPackages.Count + 1; }
                catch { newPack.SubSequence = 1; }

                try { newPack.PackagePath = newPack.ParentPackage.CurrentDesc + " # " + newPack.PackLabel.LabelCode; }
                catch {}

                try
                {
                    if (newPack.PackageType == "B")
                    {
                        newPack.CurrentDesc = newPack.ParentPackage.CurrentDesc + "/BOX" + newPack.SubSequence;
                        newPack.PackDescExt = "BOX" + newPack.SubSequence.ToString() + " # " + newPack.PackLabel.LabelCode;
                    }
                }
                catch { }

            }
                                 

            service.UpdateDocumentPackage(newPack);

            if (curPack.ChildPackages == null)
                curPack.ChildPackages = new List<DocumentPackage>();

            curPack.ChildPackages.Add(newPack);
            sourceTree.Items.Refresh();

            destTree.ItemsSource = sourceTree.ItemsSource;
            destTree.Items.Refresh();

            //Si es una caja, Nivel 2 Inprime el label.
            if (newPack.ParentPackage != null && newPack.ParentPackage.PackID != 0)
            {
                ProcessWindow pw = new ProcessWindow("Printing Package Label ...");
                try
                {
                    service.PrintLabelsFromDevice(WmsSetupValues.DEFAULT, WmsSetupValues.DefaultPackLabelTemplate,
                            new List<WpfFront.WMSBusinessService.Label> { newPack.PackLabel });
                }
                catch { }
                finally { pw.Close(); }
            }


        }



        private void sourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sourceTree.SelectedItem == null)
            {
                sourceTree.Items.Refresh();
                destTree.Items.Refresh();
                return;
            }


            ProcessWindow pw = new ProcessWindow("Loading Packages ...");

            try
            {

                bool canDelete = false;

                curPack = ((DocumentPackage)sourceTree.SelectedItem);
                PackDetailsSN = new List<WpfFront.WMSBusinessService.Label>();
                PackDetails1 = new List<ProductStock>();
                stkOparations.Visibility = stkSN.Visibility = Visibility.Hidden;


                gridPack.Visibility = Visibility.Visible;
                brMove.Visibility = Visibility.Visible;
                btnNew.Visibility = Visibility.Collapsed;
                //btnNewBox.Visibility = Visibility.Collapsed;

                //COMETARIADO PORQUE DEB TRAER EL DEL ROOT CUANDO EL ID SEA CERO
                //if (curPack.PackID == 0)
                //{
                //    gridPack.Visibility = Visibility.Hidden;
                //    brMove.Visibility = Visibility.Hidden;
                //    return;
                //}


                #region cuando es el package normal

                if (curPack.PackID > 0)
                {

                    //Si es un pallet bloquear el move Package
                    btnMovePack.Visibility = Visibility.Visible;
                    if (curPack.ChildPackages != null && curPack.ChildPackages.Count > 0)
                        btnMovePack.Visibility = Visibility.Hidden;



                    PackDetails1 = service.GetLabelStock(curPack.PackLabel);

                    if (curPack.PackLabel != null && curPack.PackLabel.LabelID != 0)
                        PackDetailsSN = service.GetUniqueTrackLabels(new WpfFront.WMSBusinessService.Label
                        {
                            FatherLabel = curPack.PackLabel, //((DocumentPackage)sourceTree.SelectedItem).PackLabel,
                            LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel }
                        });


                    if (PackDetailsSN.Count == 0 && PackDetails1.Count == 0 && (curPack.ChildPackages == null || curPack.ChildPackages.Count == 0))
                        canDelete = true;


                    imgDel.Visibility = Visibility.Collapsed;
                    if (canDelete)
                        imgDel.Visibility = Visibility.Visible;


                    //Si esta cerrado no deja hacer operaciones.
                    stkOparations.Visibility = stkSN.Visibility = Visibility.Visible;
                    if (curPack.IsClosed == true)
                        stkOparations.Visibility = stkSN.Visibility = Visibility.Hidden;


                    curPieces = 0;
                    curPack.Pieces = GetCurrentPieces(curPack);

                    if (curPack.IsClosed != true)
                    {
                        try { UpdatePackages(); }
                        catch { }
                    }

                    destTree.Items.Refresh();

                }

                #endregion

                #region cuando es el package root
                else {

                    //Permite pallet
                    btnNew.Visibility = Visibility.Visible;

                    //if (curPack.ChildPackages != null && curPack.ChildPackages.Count > 0)
                        btnMovePack.Visibility = Visibility.Hidden;

                        PackDetails1 = service.GetLabelStock(rootPackage.PackLabel);

                        if (rootPackage.PackLabel != null && rootPackage.PackLabel.LabelID != 0)

                        PackDetailsSN = service.GetUniqueTrackLabels(
                        new WpfFront.WMSBusinessService.Label
                        {
                            FatherLabel = rootPackage.PackLabel,
                            LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel }
                        });


                    //if (PackDetailsSN.Count == 0 && PackDetails1.Count == 0 && (curPack.ChildPackages == null || curPack.ChildPackages.Count == 0))
                        //canDelete = false;


                    imgDel.Visibility = Visibility.Collapsed;
                    //if (canDelete)
                        //imgDel.Visibility = Visibility.Visible;


                    //Si esta cerrado no deja hacer operaciones.
                    stkOparations.Visibility = stkSN.Visibility = Visibility.Visible;
                    //if (curPack.IsClosed == true)
                        //stkOparations.Visibility = stkSN.Visibility = Visibility.Hidden;


                    curPieces = 0;
                    curPack.Pieces = GetCurrentPieces(rootPackage);

                    //if (curPack.IsClosed != true)
                    //{
                    //    try { UpdatePackages(); }
                    //    catch { }
                    //}

                    destTree.Items.Refresh();
                
                }


                #endregion


            }
            catch { }
            finally { pw.Close(); }
        }



        private int GetCurrentPieces(DocumentPackage pack)
        {

            if (pack.ChildPackages != null && pack.ChildPackages.Count > 0)

                foreach (DocumentPackage dp in pack.ChildPackages)
                    curPieces += GetCurrentPieces(dp);

            curPieces += (int)pack.CalculatedPieces;

            return curPieces;
        }
        




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


        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            ProductStock record;
            curPack = sourceTree.SelectedItem as DocumentPackage;
            newPack = destTree.SelectedItem as DocumentPackage;

            IEnumerable<WpfFront.WMSBusinessService.Label> affectedLabels;
            IList<WpfFront.WMSBusinessService.Label> movedLabels = new List<WpfFront.WMSBusinessService.Label>();


            if (pkgDetails1.SelectedItems == null)
            {
                Util.ShowError("Please select package details to process.");
                return;
            }

            if (newPack == null || newPack.PackID == 0)
            {
                Util.ShowError("Please select a destination package.");
                return;
            }

            if (newPack.PackID == curPack.PackID)
            {
                Util.ShowError("Source and destination package are the same.");
                return;
            }

            if (newPack.Document.DocID != curPack.Document.DocID)
            {
                Util.ShowError("Source and destination document are different");
                LoadTrees();
                return;
            }


            if (newPack.IsClosed == true)
            {
                Util.ShowError("Destination package is closed.");
                return;
            }



            try
            {

                foreach (Object obj in pkgDetails1.SelectedItems)
                {
                    record = obj as ProductStock;

                    //Trae los labels de ese package
                    if (curPack.PackID > 0)

                        affectedLabels = service.GetLabel(
                            new WpfFront.WMSBusinessService.Label
                            {
                                FatherLabel = curPack.PackLabel,
                                Product = record.Product
                            });

                    else
                        affectedLabels = service.GetLabel(
                            new WpfFront.WMSBusinessService.Label
                            {
                                FatherLabel = rootPackage.PackLabel,
                                Product = record.Product
                            });


                    if (affectedLabels.Count() == 0)
                        continue;

                    //Actualizado los Labels Afectados
                    foreach (WpfFront.WMSBusinessService.Label lbl in affectedLabels)
                    {
                        lbl.FatherLabel = newPack.PackLabel;
                        lbl.ModifiedBy = App.curUser.UserName;
                        lbl.ModDate = DateTime.Now;

                        movedLabels.Add(lbl);
                        //service.UpdateLabel(lbl);
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
            PackDetails1 = service.GetLabelStock(curPack.PackLabel);


        }



        private void btnMoveRetail_Click(object sender, RoutedEventArgs e)
        {
            ProductStock record = pkgDetails1.SelectedItem as ProductStock;
            curPack = sourceTree.SelectedItem as DocumentPackage;
            newPack = destTree.SelectedItem as DocumentPackage;

            if (record == null)
            {
                Util.ShowError("Please select a line to process.");
                return;
            }

            if (newPack == null || newPack.PackID == 0 )
            {
                Util.ShowError("Please select a destination package.");
                return;
            }

            if (newPack.PackID == curPack.PackID)
            {
                Util.ShowError("Source and destination package are the same.");
                return;
            }

            if (newPack.Document.DocID != curPack.Document.DocID)
            {
                Util.ShowError("Source and destination document are different");
                LoadTrees();
                return;
            }

            if (newPack.IsClosed == true)
            {
                Util.ShowError("Destination package is closed.");
                return;
            }

            try
            {


                //DEfine si Curpack debe ser el root
                //if (curPack.PackID == 0)
                    //curPack = rootPackage;


                double qty;
                if (!double.TryParse(txtQty.Text, out qty))
                {
                    Util.ShowError("Please enter a valid quantity.");
                    return;
                }

                if (qty > record.Stock)
                {
                    Util.ShowError("Qty to move is greather than available.");
                    txtQty.Text = record.Stock.ToString();
                    return;
                }

                //Mover la cantidad seleccionada de un Paquete a Otro.
                if (newPack.CreatedBy == null)
                    newPack.CreatedBy = App.curUser.UserName;


                if (curPack.PackID == 0)
                    service.MoveQtyBetweenPackages(rootPackage, newPack, record.Product, qty);
                else
                    service.MoveQtyBetweenPackages(curPack, newPack, record.Product, qty);

            }
            catch (Exception ex)
            {
                Util.ShowError("Problem updating packages.\n" + ex.Message);
            }

            //Refreshing the details
            PackDetails1 = service.GetLabelStock(curPack.PackLabel);
            txtQty.Text = "";
        }



        private void btnMovePack_Click(object sender, RoutedEventArgs e)
        {
            //Move a Package to another.
            curPack = sourceTree.SelectedItem as DocumentPackage;
            newPack = destTree.SelectedItem as DocumentPackage;

            if (curPack.PackID == 0)
            {
                Util.ShowError("Please select a valid package.");
                return;
            }


            if (newPack == null)
            {
                Util.ShowError("Please select a destination package.");
                return;
            }

            if (newPack.PackID == curPack.PackID)
            {
                Util.ShowError("Source and destination package are the same.");
                return;
            }

            if (newPack.Document.DocID != curPack.Document.DocID)
            {
                Util.ShowError("Source and destination document are different");
                LoadTrees();
                return;
            }


            if (newPack.IsClosed == true)
            {
                Util.ShowError("Destination package is closed.");
                return;
            }


            isChild = false;

            LookForChild(curPack, newPack);

            if (isChild)
            {
                Util.ShowError("Invalid change. Destination package is child of the source package.");
                return;
            }



            if (newPack.PackID == 0)
            {
                curPack.ParentPackage = null;
                curPack.PackLabel.FatherLabel = null;
            }
            else
            {
                curPack.ParentPackage = newPack;
                curPack.PackLabel.FatherLabel = newPack.PackLabel;
            }

            //Updating the package    
            bool save = true;
            while (save)
            {
                try
                {
                    service.UpdateDocumentPackage(curPack);
                    save = false;
                }
                catch { }
            }

           
            LoadTrees();
        }



        private void LookForChild(DocumentPackage pack, DocumentPackage newPack)
        {
            if (isChild)
                return;

            if (pack.ChildPackages != null && pack.ChildPackages.Count > 0)
            {
                foreach (DocumentPackage dp in pack.ChildPackages)
                {
                    if (dp.PackID == newPack.PackID)
                    {
                        isChild = true;
                        return;
                    }
                    else
                        LookForChild(dp, newPack);
                }
            }
        }



        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //If Print all selected imprime toda la lista, si no imprime el seleccionado.
            ProcessWindow pw = new ProcessWindow("Printing Labels ... ");

            try
            {

                if (ckPrintAll.IsChecked == true)
                {
                    Util.PrintShipmentPackLabels(curPosted);
                    //PrintParentLabel();
                    return;
                }

                //Definicion del Template a Imprimier
                DocumentPackage curPack = ((DocumentPackage)sourceTree.SelectedItem);

                if (curPack == null || curPack.PackLabel.LabelID == 0)
                {
                    Util.ShowError("No Pallet/Box selected.");
                    return;
                }

                if ((curPack.ParentPackage == null || curPack.ParentPackage.PackID == 0)) 
                    //&& curPack.ChildPackages != null && curPack.ChildPackages.Count > 0) //Parent label
                    PrintParentLabel(curPack);

                else
                    //Imprime solo el Label que se selecciono.
                    service.PrintLabelsFromDevice(WmsSetupValues.DEFAULT, WmsSetupValues.DefaultPackLabelTemplate,
                        new List<WpfFront.WMSBusinessService.Label> { curPack.PackLabel });

            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            finally { pw.Close(); }
        }




        private void PrintParentLabel(DocumentPackage parentPack)
        {

            ProcessWindow pw = new ProcessWindow("Printing Label ... ");

            try
            {
                service.PrintLabelsFromDevice(WmsSetupValues.DEFAULT, WmsSetupValues.DefaultPalletLabelTemplate,
                    new List<WpfFront.WMSBusinessService.Label> { parentPack.PackLabel });
            }
            catch { }
            finally { pw.Close(); }

            /*
            DocumentPackage basePack = service.GetDocumentPackage(new DocumentPackage
            {
                PostingDocument = curPosted,
                ParentPackage = new DocumentPackage { PackID = -1 }
            }).First();

            //Acomoda el label del pallet para que salga en el Template de los packages.
            WpfFront.WMSBusinessService.Label parentLabel = basePack.PackLabel;

            parentLabel.LabelCode = parentLabel.Barcode = "SHPMNT " + curPosted.DocNumber;
            parentLabel.Package.Sequence = 0;
            parentLabel.Package.Weight = 0;
            parentLabel.Package.Dimension = "";
            parentLabel.Package.Pieces = 0;
            parentLabel.CurrQty = 0;
            parentLabel.StartQty = 0;
            parentLabel.LabelID = 0;
            parentLabel.Package.PackID = 0;
            */
        }





        private void btnMoveSN_Click(object sender, RoutedEventArgs e)
        {
            //Mueve un serial de una caja a otra.
            curPack = sourceTree.SelectedItem as DocumentPackage;
            newPack = destTree.SelectedItem as DocumentPackage;

            //if (curPack.PackID == 0)
            //{
            //    Util.ShowError("Please select a valid package.");
            //    return;
            //}


            if (newPack == null || newPack.PackID == 0)
            {
                Util.ShowError("Please select a destination package.");
                return;
            }

            if (newPack.PackID == curPack.PackID)
            {
                Util.ShowError("Source and destination package are the same.");
                return;
            }


            if (newPack.Document.DocID != curPack.Document.DocID)
            {
                Util.ShowError("Source and destination document are different");
                LoadTrees();
                return;
            }

            if (newPack.PackID == 0)
            {
                Util.ShowError("Please select a valid destination package.");
                return;
            }

            if (lvSerials.SelectedItem == null)
            {
                Util.ShowError("No serial barcode selected.");
                return;
            }

            if (newPack.IsClosed == true)
            {
                Util.ShowError("Destination package is closed.");
                return;
            }


            //Define si Curpack debe ser el root
            //if (curPack.PackID == 0)
                //curPack = rootPackage;


            IList<WpfFront.WMSBusinessService.Label> movedLabels = new List<WpfFront.WMSBusinessService.Label>();
            
            foreach (WpfFront.WMSBusinessService.Label affectedLabel in lvSerials.SelectedItems)
            {
                affectedLabel.FatherLabel = newPack.PackLabel;
                affectedLabel.ModDate = DateTime.Now;
                affectedLabel.ModifiedBy = App.curUser.UserName;

                movedLabels.Add(affectedLabel);
                PackDetailsSN.Remove(affectedLabel);
            }

            service.UpdatePackageMovedLabels(movedLabels);
            lvSerials.Items.Refresh();

        }


        private void imgDel_Click(object sender, RoutedEventArgs e)
        {
            if (sourceTree.SelectedItem == null)
            {
                Util.ShowError("No package selected.");
                return;
            }

            if (PackDetails1 != null && PackDetails1.Count > 0)
            {
                Util.ShowError("No package selected is not empty.");
                return;
            }

            if (PackDetailsSN != null && PackDetailsSN.Count > 0)
            {
                Util.ShowError("No package selected is not empty.");
                return;
            }

            curPack = sourceTree.SelectedItem as DocumentPackage;

            try
            {
                //Delete Package
                service.DeleteDocumentPackage(curPack);
                try { service.DeleteLabel(curPack.PackLabel); }
                catch { }
            }
            catch { }


            LoadTrees();
        }


        private void btnAddSerial_Click(object sender, RoutedEventArgs e)
        {
            AddSerial();
        }


        private void AddSerial()
        {

            if (string.IsNullOrEmpty(txtUnique.Text))
            {
                Util.ShowError("No valid data.");
                return;
            }


            try
            {
                //Mueve uno de los labels existentes al package indicado.

                //1. Validar package is selected
                //Mueve un serial de una caja a otra.
                newPack = sourceTree.SelectedItem as DocumentPackage;

                if (newPack == null || newPack.PackID == 0)
                {
                    Util.ShowError("Please select a package.");
                    txtUnique.Text = "";
                    return;
                }

                if (newPack.IsClosed == true)
                {
                    Util.ShowError("Package is closed.");
                    return;
                }

                //2. Buscar el S/N - labelcode dentro de el listado de labels del shipment
                WpfFront.WMSBusinessService.Label labelToPack;
                try
                {
                    labelToPack = service.GetNodeTrace(new NodeTrace
                    {
                        Label = new WpfFront.WMSBusinessService.Label { LabelCode = txtUnique.Text },
                        Document = curDoc,
                        PostingDocument = curPosted
                    }).Select(f => f.Label).First();
                }
                catch
                {
                    Util.ShowError("S/N or Barcode not found in the shipment.");
                    txtUnique.Text = "";
                    return;
                }

                //3. Cambiar el Label de posicion al nuevo Package.
                labelToPack.FatherLabel = newPack.PackLabel;
                labelToPack.ModDate = DateTime.Now;
                labelToPack.ModifiedBy = App.curUser.UserName;
                service.UpdateLabel(labelToPack);

                //4. Refresh 
                if (!PackDetailsSN.Any(f => f.LabelID == labelToPack.LabelID))
                {
                    PackDetailsSN.Add(labelToPack);
                    lvSerials.Items.Refresh();
                }

            }
            finally
            {
                txtUnique.Text = "";
                txtUnique.Focus();
            }

        }


        private void txtUnique_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            AddSerial();
        }


        private void UpdatePackages()
        {
            //Actualiza el Valor de los Packages, Weight and Dimension

            curPack.ModDate = DateTime.Now;
            curPack.ModifiedBy = App.curUser.UserName;
            curPack.Weight = double.Parse(txtWeight.Text.ToString());
            curPack.Dimension = txtDim.Text;
            service.UpdateDocumentPackage(curPack);

        }


        private void txtWeight_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePackages();
        }


        private void txtDim_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePackages();
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            newPack = sourceTree.SelectedItem as DocumentPackage;

            if (newPack == null || newPack.PackID == 0)
            {
                return;
            }

            //Manda a cerrar el package y sus hijos
            service.CloseDocumentPackage(newPack);

            stkOparations.Visibility = stkSN.Visibility = Visibility.Hidden;

        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            newPack = sourceTree.SelectedItem as DocumentPackage;

            if (newPack == null || newPack.PackID == 0)
            {
                return;
            }

            newPack.IsClosed = false;
            service.UpdateDocumentPackage(newPack);

            stkOparations.Visibility = stkSN.Visibility = Visibility.Visible;

        }


        private void btnCreateShipment_Click(object sender, RoutedEventArgs e)
        {

            //revisar que no quede producto en ROOT
            IList<ProductStock> lStock = service.GetLabelStock(rootPackage.PackLabel);
            if (lStock.Count > 0)
            {
                Util.ShowError("The Shipment still has product pending of packing.");
                return;
            }

            IEnumerable<DocumentPackage> listPackages = service.GetDocumentPackage(
                    new DocumentPackage
                    {
                        Document = new Document { DocID = rootPackage.Document.DocID },
                        Level = 0,
                        PostingDocument = new Document { DocID = -1 },
                        
                    }
                ).Where(f => f.Weight == 0 && f.PackageType != "R");



            if (listPackages.Count() > 0
                && UtilWindow.ConfirmOK("Some Pallet(s)/Boxe(s) in level 1 appears without weight. Wish you continue anyway?") != true)
                return;


            if (rootPackage.PackLabel != null && rootPackage.PackLabel.LabelID != 0)
            {
                IList<WpfFront.WMSBusinessService.Label> lSN = service.GetUniqueTrackLabels(
                    new WpfFront.WMSBusinessService.Label
                {
                    FatherLabel = rootPackage.PackLabel,
                    LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel }
                });

                if (lSN.Count > 0)
                {
                    Util.ShowError("The Shipment still has product pending of packing.");
                    return;
                }
            }


            try
            {
                parentPresenter.CreateShipment();
            }
            finally
            {
                ((Popup)((Border)((StackPanel)this.Parent).Parent).Parent).IsOpen = false;
            }
            
        }

    }
}