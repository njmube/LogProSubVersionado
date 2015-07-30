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

namespace WpfFront.Presenters
{

    public interface IProductPresenter
    {
        IProductView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ProductPresenter : IProductPresenter
    {
        public IProductView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        public ProductPresenter(IUnityContainer container, IProductView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ProductModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Product>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.AssignBinToProduct += new EventHandler<DataEventArgs<Bin>>(OnAssignBinToProduct);
            View.RemoveFromList += new EventHandler<EventArgs>(this.OnRemoveFromList);
            View.AddProductTrackOption += new EventHandler<EventArgs>(OnAddProductTrackOption);
            View.AddProductUnit += new EventHandler<EventArgs>(OnAddProductUnit);
            View.LoadUnitsFromGroup += new EventHandler<EventArgs>(OnLoadUnitsFromGroup);
            //View.LoadBins += new EventHandler<DataEventArgs<string>>(OnLoadBins);
            View.SetRequired += new EventHandler<DataEventArgs<object>>(View_SetRequired);
            View.UnSetRequired += new EventHandler<DataEventArgs<object>>(View_UnSetRequired);
            View.UpdateBinToProduct += new EventHandler<DataEventArgs<ZoneBinRelation>>(View_UpdateBinToProduct);
            View.AddAlternateProduct += new EventHandler<EventArgs>(View_AddAlternateProduct);
            View.AddProductAccount += new EventHandler<DataEventArgs<ProductAccountRelation>>(View_AddProductAccount);
            View.UpdateProductAccount += new EventHandler<DataEventArgs<object>>(View_UpdateProductAccount);
            //View.SetIsMain += new EventHandler<DataEventArgs<object>>(View_SetIsMain);
            //View.UnSetIsMain += new EventHandler<DataEventArgs<object>>(View_UnSetIsMain);


            ProcessWindow pw = new ProcessWindow("Loading ...");

            View.Model.EntityList = service.GetProductApp(new Product { Company = App.curCompany, Reference = App.curLocation.LocationID.ToString() }, 25);
            View.Model.Record = null;
            View.Model.StatusList = App.EntityStatusList;
            //Load Pick Methods
            View.Model.PickMethods = App.PickMethodList;
            view.Model.TemplateList = service.GetLabelTemplate(new LabelTemplate { LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel } });
            view.Model.TemplateList.Add(new LabelTemplate());

            //List Height
            View.ListRecords.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 250;
            View.Model.CurAltern = new ProductAlternate();

            //USE CASN
            if (Util.GetConfigOption("USECASN").Equals("T")) 
                View.TbItmCasN.Visibility = Visibility.Visible;
            

            pw.Close();

        }



        void View_AddProductAccount(object sender, DataEventArgs<ProductAccountRelation> e)
        {
            //Crear un objeto de tipo product Account Relation adicionarselo al producto y darle Update.
            if (e.Value == null)
                return;


            if (string.IsNullOrEmpty(e.Value.ItemNumber) && string.IsNullOrEmpty(e.Value.Code1) && string.IsNullOrEmpty(e.Value.Code2))
            {
                Util.ShowError("UPC or Item Number or Alternative Code is required.");
                return;
            }



            try
            {
                if (e.Value.Account == null)
                    e.Value.Account = service.GetAccount(new Account{AccountCode = WmsSetupValues.DEFAULT}).First();    
            

                //Check if no existe ya el UPCcode para otro producto.
                IList<ProductAccountRelation> list;
                if (!string.IsNullOrEmpty(e.Value.ItemNumber))
                {
                    list = service.GetProductAccountRelation(new ProductAccountRelation { ItemNumber = e.Value.ItemNumber });
                    if (list != null && list.Count > 0)
                    {
                        Util.ShowError("Item " + list[0].Product.FullDesc + " with the same code already exists.");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(e.Value.Code1))
                {
                    list = service.GetProductAccountRelation(new ProductAccountRelation { ItemNumber = e.Value.Code1 });
                    if (list != null && list.Count > 0)
                    {
                        Util.ShowError("Item " + list[0].Product.FullDesc + " with the same code already exists.");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(e.Value.Code2))
                {
                    list = service.GetProductAccountRelation(new ProductAccountRelation { ItemNumber = e.Value.Code2 });
                    if (list != null && list.Count > 0)
                    {
                        Util.ShowError("Item " + list[0].Product.FullDesc + " with the same code already exists.");
                        return;
                    }
                }


                ProductAccountRelation newPa = service.SaveProductAccountRelation(e.Value);

                //if (newPa.IsDefault == true)
                    //service.UpdateIsMainProductAccount(newPa);  
                

            }
            catch (Exception ex)
            {
                Util.ShowError("Error saving the record. Account could already exists for this product." + ex.Message );
                return;
            }
            finally
            {
                if (e.Value.AccountType.AccountTypeID == AccType.Vendor)
                {
                    View.TxtVendor.Account = null;
                    View.TxtVendor.Text = "";
                    View.TxtVendCode1.Text = "";
                    View.TxtVendCode2.Text = "";
                    View.TxtVendItem.Text = "";
                }

                if (e.Value.AccountType.AccountTypeID == AccType.Customer)
                {
                    View.TxtCustomer.Account = null;
                    View.TxtCustomer.Text = "";
                    View.TxtCustCode1.Text = "";
                    View.TxtCustCode2.Text = "";
                    View.TxtCustItem.Text = "";
                }

                View.ChkIsMain.IsChecked = false;

            }


            if (e.Value.AccountType.AccountTypeID == AccType.Vendor)
            {

                View.Model.Record.ProductAccounts = service.GetProductAccountRelation(
                 new ProductAccountRelation { Product = View.Model.Record }).ToList();

                try
                {
                    View.Model.ProductVendorRelation = View.Model.Record.ProductAccounts
                        .Where(f => f.AccountType.AccountTypeID == AccType.Vendor || f.Account.AccountCode == WmsSetupValues.DEFAULT).ToList();
                }
                catch { View.Model.ProductVendorRelation = null; }

                View.LvVendorProducts.Items.Refresh();
            }


            else if (e.Value.AccountType.AccountTypeID == AccType.Customer)
            {
                View.Model.Record.ProductAccounts = service.GetProductAccountRelation(
                            new ProductAccountRelation { Product = View.Model.Record }).ToList();

                try
                {
                    View.Model.ProductCustRelation = View.Model.Record.ProductAccounts
                        .Where(f => f.AccountType.AccountTypeID == AccType.Customer || f.Account.AccountCode == WmsSetupValues.DEFAULT ).ToList();
                }
                catch { View.Model.ProductCustRelation = null; }

                View.LvCustProducts.Items.Refresh();
            }

        }



        void View_AddAlternateProduct(object sender, EventArgs e)
        {

            if (View.TxtAltProduct.Product == null)
            {
                Util.ShowError("No altern product associated.");
                return;
            }

            if (View.TxtAltProduct.Product.ProductID == View.Model.Record.ProductID)
            {
                Util.ShowError("Product and Altern product are the same.");
                return;
            }


            try
            {
                View.Model.CurAltern.AlternProduct = View.TxtAltProduct.Product;
                View.Model.CurAltern.Product = View.Model.Record;
                View.Model.CurAltern.CreatedBy = App.curUser.UserName;
                View.Model.CurAltern.CreationDate = DateTime.Now;

                //service.SaveProductAlternate(View.Model.CurAltern);

                if (View.Model.Record.AlternProducts == null)
                    View.Model.Record.AlternProducts = new List<ProductAlternate>();

                View.Model.Record.AlternProducts.Add(View.Model.CurAltern);
                View.LvAlternProducts.Items.Refresh();
                
                service.UpdateProduct(View.Model.Record);
                View.Model.Record = service.GetProduct(View.Model.Record).First();

                ShowProductAlterns();

            }
            catch (Exception ex)
            { Util.ShowError("Problem adding altern product.\n" + ex.Message); }

        }


        void ShowProductAlterns()
        {
            View.StkAltern.Visibility = Visibility.Collapsed;
            if (View.Model.Record.AlternProducts != null && View.Model.Record.AlternProducts.Count > 0)
                View.StkAltern.Visibility = Visibility.Visible;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
            {
                View.Model.EntityList = service.GetProductApp(new Product { Company = App.curCompany, Reference = App.curLocation.LocationID.ToString() }, WmsSetupValues.NumRegs);
                return;
            }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetProductApp(new Product { Name = e.Value, Company = App.curCompany, Reference = App.curLocation.LocationID.ToString() }, WmsSetupValues.NumRegs);

            if (View.Model.EntityList.Count == 1)
            {
                View.ListRecords.SelectedIndex = 0;
                LoadProduct(View.Model.EntityList[0]);
            }
        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Product> e)
        {
            if (e.Value == null)
                return;

            LoadProduct(e.Value);
        }


        private void LoadProduct(Product product)
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = product;
            View.Model.UnitGroup = null;
            View.Model.UnitList = product.ProductUnits.Select(f => f.Unit).ToList();

            try { ShowProductStock(); }
            catch { }
            try { LoadTrackOptions(); }
            catch { }
            try { LoadUnits(); }
            catch { }
            try { ShowProductAlterns(); }
            catch { }

            try
            {
                if (Util.GetConfigOption("USECASN").Equals("T"))
                {
                    View.UcCasNumberFormula.Product = View.Model.Record;
                    View.UcCasNumberFormula.LoadExistingList();
                }
            }
            catch { }

            //Load Product Account For Vendor.
            View.Model.ProductVendorRelation = service.GetProductAccountRelation(
                new ProductAccountRelation { Product = View.Model.Record, 
                    AccountType = new AccountType { AccountTypeID = AccType.Vendor } });


            //Load Product Account For Vendor.
            View.Model.ProductCustRelation = service.GetProductAccountRelation(
                new ProductAccountRelation
                {
                    Product = View.Model.Record,
                    AccountType = new AccountType { AccountTypeID = AccType.Customer }
                });


            LoadResources(); //Files Templates

        }

        private void LoadResources()
        {
            View.UcProcessFile.LoadResources(EntityID.Product, View.Model.Record.ProductID);
        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.Model.Record = null;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new Product { Status = new Status() };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;
            View.Model.Record.PickMethod = (PickMethod)View.CboPickMethod.SelectedItem;
            //Asigno el id del location en el campo reference del product
            if (View.Model.Record.Reference == null)
                View.Model.Record.Reference = App.curLocation.LocationID.ToString();
            //Asigno el valor de la unidad
            if (View.CboUnidadBase.SelectedItem != null)
            {
                View.Model.Record.BaseUnit = (Unit)View.CboUnidadBase.SelectedItem;
            }

            if (View.Model.Record.ProductID == 0)
            {
                isNew = true;
                View.Model.Record.Company = App.curLocation.Company;
                View.Model.Record.CreationDate = DateTime.Now;
                View.Model.Record.CreatedBy = App.curUser.UserName;
                View.Model.Record.IsFromErp = false;
                View.Model.Record.IsKit = false;

            }


            try
            {

                if (string.IsNullOrEmpty(View.Model.Record.ProductCode) ||  string.IsNullOrEmpty(View.Model.Record.Name) || View.Model.Record.Status.StatusID == 0)
                {
                    Util.ShowError("ProductCode, Name and Status are required.");
                    return;
                }


                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveProduct(View.Model.Record);
                    View.Model.EntityList = service.GetProduct(new Product { Company = App.curCompany, Reference = App.curLocation.LocationID.ToString() });
                    CleanToCreate();
                }
                else
                {
                    if (View.Model.Record.DefaultTemplate != null && View.Model.Record.DefaultTemplate.RowID == 0)
                        View.Model.Record.DefaultTemplate = null;

                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    message = "Record updated.";
                    service.UpdateProduct(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                //View.Model.EntityList = service.GetProduct(new Product());

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }




        }


        private void OnDelete(object sender, EventArgs e)
        {

            if (View.Model.Record == null)
            {
                Util.ShowError("No record selected.");
                return;
            }

            try
            {
                service.DeleteProduct(View.Model.Record);
                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetProduct(new Product { Company = App.curCompany, Reference = App.curLocation.LocationID.ToString() });

                Util.ShowMessage("Record deleted.");

            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }


        private void ShowProductStock()
        {
            //Show Product Stock
            View.StkLocations.Visibility = Visibility.Collapsed;
            View.Model.ProductBinRelation = null;
            View.Model.ProductBinRelation = service.GetProductAssignedZone(View.Model.Record, App.curLocation);

            if (View.Model.ProductBinRelation != null && View.Model.ProductBinRelation.Count > 0)
                View.StkLocations.Visibility = Visibility.Visible;
        }


        private void OnAssignBinToProduct(object sender, DataEventArgs<Bin> e)
        {
            if (e.Value == null)
                return;

            Bin assignLocation = e.Value;

            //try
            //{ assignLocation = service.GetBin(new Bin { BinCode = e.Value, Location = App.curLocation }).First(); }
            //catch { assignLocation = null; }

            //if (assignLocation == null)
            //{
            //    Util.ShowError("Bin " + e.Value + " is not valid.");
            //    return;
            //}

            //Assign Bin to Product
            try
            {
                assignLocation.ModifiedBy = App.curUser.UserName;

                int binDirection = View.CboBinDirection.SelectedItem == null ? 0 : (int)View.CboBinDirection.SelectedValue;

                ZoneBinRelation zonBin = new ZoneBinRelation { 
                        Bin = assignLocation, 
                        BinType = (short)binDirection, 
                        CreatedBy = App.curUser.UserName,
                    };

                try { zonBin.MinUnitCapacity = Double.Parse(View.MinStock.Text); } catch {}
                try { zonBin.UnitCapacity = Double.Parse(View.MaxStock.Text); } catch { }
                
                service.AssignBinToProduct(View.Model.Record, zonBin);
                
                ShowProductStock();
            }
            catch (Exception ex)
            {
                Util.ShowError("Bin could not be assigned.\n" + ex.Message);
            }


        }


        private void OnRemoveFromList(object sender, EventArgs e)
        {
            // Removing a Bin
            if (((Button)sender).Name == "btnRemBin")
                RemoveBin();

            // Removing a Track Option
            if (((Button)sender).Name == "btnRemTrack")
                RemoveTrackOption();

            // Removing a Unit
            if (((Button)sender).Name == "btnRemUnit")
                RemoveUnit();

            // Removing a Alternate product
            if (((Button)sender).Name == "btnRemAlt")
                RemoveAlternProduct();


            // Removing a Alternate product
            if (((Button)sender).Name == "btnRemVendor")
                RemoveAccountProduct(AccType.Vendor);

            // Removing a Alternate product
            if (((Button)sender).Name == "btnRemCust")
                RemoveAccountProduct(AccType.Customer);

        }


        private void RemoveAccountProduct(short accType)
        {
            if (accType == AccType.Vendor)
            {
                if (View.LvVendorProducts.SelectedItem == null)
                    return;

                foreach (ProductAccountRelation pa in View.LvVendorProducts.SelectedItems)
                {
                    //ProductAlternate pAltern = View.LvAlternProducts.SelectedItem as ProductAlternate;
                    service.DeleteProductAccountRelation(pa);
                    View.Model.Record.ProductAccounts.Remove(pa);
                }

                View.Model.ProductVendorRelation = View.Model.Record.ProductAccounts
                        .Where(f => f.AccountType.AccountTypeID == AccType.Vendor).ToList();

                View.LvVendorProducts.Items.Refresh();
            }

            if (accType == AccType.Customer)
            {
                if (View.LvCustProducts.SelectedItem == null)
                    return;

                foreach (ProductAccountRelation pa in View.LvCustProducts.SelectedItems)
                {
                    //ProductAlternate pAltern = View.LvAlternProducts.SelectedItem as ProductAlternate;
                    service.DeleteProductAccountRelation(pa);
                    View.Model.Record.ProductAccounts.Remove(pa);
                }

                View.Model.ProductCustRelation = View.Model.Record.ProductAccounts
                        .Where(f => f.AccountType.AccountTypeID == AccType.Customer).ToList();

                View.LvCustProducts.Items.Refresh();
            }

        }




        private void RemoveAlternProduct()
        {
            if (View.LvAlternProducts.SelectedItem == null)
                return;

            foreach (ProductAlternate pAltern in View.LvAlternProducts.SelectedItems)
            {
                //ProductAlternate pAltern = View.LvAlternProducts.SelectedItem as ProductAlternate;
                service.DeleteProductAlternate(pAltern);
                View.Model.Record.AlternProducts.Remove(pAltern);
            }

            View.LvAlternProducts.Items.Refresh();
            ShowProductAlterns();

        }






        private void RemoveBin()
        {

            if (View.LvStock.SelectedItems == null)
                return;

            ZoneBinRelation zBin = null;
            //ZoneBinRelation proStock = null;
            string msg = "";

            foreach (Object obj in View.LvStock.SelectedItems)
            {
                try
                {
                    zBin = (ZoneBinRelation)obj;
                    View.Model.ProductBinRelation.Remove(zBin);

                    zBin = service.GetZoneBinRelation(new ZoneBinRelation { Bin = zBin.Bin, Zone = zBin.Zone }).First();

                    service.DeleteZoneBinRelation(zBin);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Bin: " + zBin.Bin + ". " + ex.Message;
                }
            }

            View.LvStock.Items.Refresh();

            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            ShowProductStock();
        }


        private void RemoveTrackOption()
        {
            if (View.LvAssignedTrack.SelectedItems == null)
                return;

            ProductTrackRelation pTrack = null;
            string msg = "";



            foreach (Object obj in View.LvAssignedTrack.SelectedItems)
            {
                try
                {
                    pTrack = (ProductTrackRelation)obj;
                    View.Model.AllowTrack.Remove((ProductTrackRelation)obj);
                    service.DeleteProductTrackRelation(pTrack);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Track Option: " + pTrack.TrackOption.DisplayName + ". " + ex.Message;
                }
            }


            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            LoadTrackOptions();
        }


        private void RemoveUnit()
        {
            UnitProductRelation pUnit = null;
            string msg = "";

            if (View.LvAssignedUnit.SelectedItems == null)
                return;

            foreach (Object obj in View.LvAssignedUnit.SelectedItems)
            {
                try
                {
                    pUnit = (UnitProductRelation)obj;
                    View.Model.AssignedUnits.Remove((UnitProductRelation)obj);
                    service.DeleteUnitProductRelation(pUnit);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Unit: " + pUnit.Unit.Name + ". " + ex.Message;
                }
            }


            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            LoadUnits();
        }


        //Carga las Opciones de Track
        private void LoadTrackOptions()
        {
            //Avaliable Track
            View.Model.AvailableTrack = service.GetTrackOption(
                new TrackOption { Active = true }).Where(f => (!string.IsNullOrEmpty(f.DisplayName))).ToList();

            //Assigned Track
            View.Model.AllowTrack = service.GetProductTrackRelation(
                new ProductTrackRelation { Product = View.Model.Record });

            TrackOption curTrack;
            foreach (TrackOption trackOp in View.Model.AllowTrack.Select(f => f.TrackOption))
            {
                try { curTrack = View.Model.AvailableTrack.Where(f => f.RowID == trackOp.RowID).First(); }
                catch { curTrack = null; }

                if (curTrack != null)
                    View.Model.AvailableTrack.Remove(curTrack);
            }

            View.LvAvailableTrack.Items.Refresh();
            View.LvAssignedTrack.Items.Refresh();


        }


        private void OnAddProductTrackOption(object sender, EventArgs e)
        {
            if (View.LvAvailableTrack.SelectedItems == null || View.LvAvailableTrack.SelectedItems.Count == 0)
                return;

            try
            {
                foreach (TrackOption selItem in View.LvAvailableTrack.SelectedItems)
                {
                    AddTrackOption(selItem);
                }
                View.LvAssignedTrack.Items.Refresh();
                View.LvAvailableTrack.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Track Option could not be assigned.\n" + ex.Message);
            }

        }


        private void AddTrackOption(TrackOption trackOption)
        {
            if (trackOption == null)
                return;

            try
            {
                ProductTrackRelation relation = new ProductTrackRelation
                {
                    Product = View.Model.Record,
                    TrackOption = trackOption,
                    IsRequired = false,
                    IsUnique = (trackOption.IsUnique == null) ? false : trackOption.IsUnique,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                };

                relation = service.SaveProductTrackRelation(relation);

                if (View.Model.AllowTrack == null)
                    View.Model.AllowTrack = new List<ProductTrackRelation>();

                View.Model.AllowTrack.Insert(0, relation);

                View.Model.AvailableTrack.Remove(trackOption);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        //Units
        //Carga las Opciones de Track
        private void LoadUnits()
        {

            View.DgUnits.Visibility = View.CboUnitGroup.Visibility = Visibility.Collapsed;

            //Assigned Unit
            View.Model.AssignedUnits = service.GetUnitProductRelation(
                new UnitProductRelation { Product = View.Model.Record });

            //Avaliable Unit

            //Si el producto no tiene asignado unidades lista los ERPGROUP (baseamount==1)
            //y luego si muestra las unidades
            if (View.Model.AssignedUnits == null || View.Model.AssignedUnits.Count == 0)
            {
                View.CboUnitGroup.Visibility = Visibility.Visible;

                View.Model.ListUnitGroup = service.GetUnit(new Unit { Company = App.curCompany })
                    .Where(f => f.BaseAmount == 1.0).ToList();


            }
            else
            {
                //Si el producto tiene asignado unidades, toma la primera para sacar el ERPGROUP  
                View.DgUnits.Visibility = Visibility.Visible;

                //unit Group
                View.Model.UnitGroup = View.Model.AssignedUnits.First().Unit;

                View.Model.AvailableUnits = service.GetUnit(new Unit { Company = App.curCompany })
                    .Where(f => f.ErpCodeGroup == View.Model.UnitGroup.ErpCodeGroup).ToList();

                Unit curUnit = null;
                foreach (Unit unit in View.Model.AssignedUnits.Select(f => f.Unit))
                {
                    try { curUnit = View.Model.AvailableUnits.Where(f => f.UnitID == unit.UnitID).First(); }
                    catch { break; }

                    if (curUnit != null)
                        View.Model.AvailableUnits.Remove(curUnit);
                }
            }


            View.LvAvailableUnit.Items.Refresh();
            View.LvAssignedUnit.Items.Refresh();


        }


        private void OnAddProductUnit(object sender, EventArgs e)
        {
            if (View.LvAvailableUnit.SelectedItems == null || View.LvAvailableUnit.SelectedItems.Count==0)
                return;

            try
            {
                foreach (Unit selItem in View.LvAvailableUnit.SelectedItems)
                {
                    AddUnit(selItem);
                }
                View.LvAvailableUnit.Items.Refresh();
                View.LvAssignedUnit.Items.Refresh();
                View.CboUnitGroup.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.ShowError("Unit could not be assigned.\n" + ex.Message);
            }

        }


        private void AddUnit(Unit unit)
        {
            if (unit == null)
                return;

            try
            {
                UnitProductRelation relation = new UnitProductRelation
                {
                    Product = View.Model.Record,
                    Unit = unit,
                    BaseAmount = unit.BaseAmount,
                    IsBasic = (unit.BaseAmount == 1) ? true : false,
                    Status = new Status { StatusID = EntityStatus.Active },
                    UnitErpCode = (unit.ErpCode == null) ? "" : unit.ErpCode,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                    
                };

                relation = service.SaveUnitProductRelation(relation);

                if (View.Model.AssignedUnits == null)
                    View.Model.AssignedUnits = new List<UnitProductRelation>();

                View.Model.AssignedUnits.Insert(0, relation);
                View.Model.AvailableUnits.Remove(unit);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private void OnLoadUnitsFromGroup(object sender, EventArgs e)
        {
            if (View.CboUnitGroup.SelectedItem == null)
                return;

            View.Model.UnitGroup = (Unit)View.CboUnitGroup.SelectedItem;

            View.DgUnits.Visibility = Visibility.Visible;

            View.Model.AvailableUnits = service.GetUnit(new Unit { Company = App.curCompany })
                .Where(f => f.ErpCodeGroup == View.Model.UnitGroup.ErpCodeGroup).ToList();

            View.LvAvailableUnit.Items.Refresh();
            View.LvAssignedUnit.Items.Refresh();
        }


        //Febrero 23 2009 - Seleccion de Bins - JM
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
        //        View.TxtAssigBin.Text = View.Model.BinList[0].BinCode;
        //        View.TxtAssigBin.Focus();
        //    }
        //}




        private void View_SetRequired(object sender, DataEventArgs<object> e)
        {
            UpdateTrackRequired(true, short.Parse(e.Value.ToString()));

        }

        private void View_UnSetRequired(object sender, DataEventArgs<object> e)
        {
            UpdateTrackRequired(false, short.Parse(e.Value.ToString()));
        }


        private void UpdateTrackRequired(bool required, short trackOpID)
        {
            try
            {

                //Traer le ProducTrack
                IList<ProductTrackRelation> ptr = service.GetProductTrackRelation(
                    new ProductTrackRelation
                    {
                        Product = View.Model.Record,
                        TrackOption = new TrackOption { RowID = trackOpID }
                    });

                if (ptr == null || ptr.Count == 0)
                    return;

                ptr.First().IsRequired = required;
                ptr.First().ModDate = DateTime.Now;
                ptr.First().ModifiedBy = App.curUser.UserName;

                service.UpdateProductTrackRelation(ptr.First());
            }
            catch { }
        }

        //JairoM Abril 24 de 2009 - update bin relation desde la lista
        void View_UpdateBinToProduct(object sender, DataEventArgs<ZoneBinRelation> e)
        {
            try
            {
                if (e.Value == null)
                    return;

                e.Value.ModDate = DateTime.Now;
                e.Value.ModifiedBy = App.curUser.UserName;

                service.UpdateZoneBinRelation(e.Value);

            }
            catch (Exception ex)
            {
                Util.ShowError("Problem updating record.\n" + ex.Message);
            }

        }


        /*
        void View_UnSetIsMain(object sender, DataEventArgs<object> e)
        {
            //En el objeto viene el Row ID a Setear.
            if (e.Value == null)
                return;

            ProductAccountRelation pa = new ProductAccountRelation
            {
                RowID = int.Parse(e.Value.ToString()),
                IsDefault = false
            };
            service.UpdateIsMainProductAccount(pa);    

        }



        void View_SetIsMain(object sender, DataEventArgs<object> e)
        {
            //En el objeto viene el Row ID a Setear.
            if (e.Value == null)
                return;


            ProductAccountRelation pa = new ProductAccountRelation
            {
                RowID = int.Parse(e.Value.ToString()),
                IsDefault = true
            };

            service.UpdateIsMainProductAccount(pa);    


        }
        */
    

        void View_UpdateProductAccount(object sender, DataEventArgs<object> e)
        {
            //En el objeto viene el Row ID a Setear.
            if (e.Value == null)
                return;


            try
            {
                //Obtiene el Registro
                ProductAccountRelation pa = View.Model.ProductVendorRelation
                    .Where(f => f.RowID == int.Parse(e.Value.ToString())).First();

                pa.ModDate = DateTime.Now;
                pa.ModifiedBy = App.curUser.UserName;
                service.UpdateProductAccountRelation(pa);

                //if (pa.IsDefault == true)
                //{
                //    View.Model.Record.UpcCode = pa.Code1;
                //    View.Model.Record.DefVendorNumber = pa.ItemNumber;
                //    View.Model.Record.ModDate = DateTime.Now;
                //    View.Model.Record.ModifiedBy = App.curUser.UserName;
                //    service.UpdateProduct(View.Model.Record);

                //}

            }
            catch { }

        }



    }
}