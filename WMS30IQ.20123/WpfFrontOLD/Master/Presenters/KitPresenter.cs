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
    public interface IKitPresenter
    {
        IKitView View { get; set; }
        ToolWindow Window { get; set; }
    }

   public class KitPresenter: IKitPresenter
    {
       public IKitView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public KitPresenter(IUnityContainer container, IKitView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<KitModel>();

            //Event Delegate
            
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.LoadData += new EventHandler<DataEventArgs<KitAssembly>>(this.OnLoadData);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.SaveComponent += new EventHandler<EventArgs>(this.OnSaveComponent);
            view.RemoveComponent += new EventHandler<EventArgs>(this.RemoveComponent);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            //ProcessWindow pw = new ProcessWindow("Loading ...");

            // CAA   Trae todos los tipos de Kits....      AsmType = KitType.Custom,
            View.Model.EntityList = service.GetKitAssembly(new KitAssembly { Status = new Status { StatusID = EntityStatus.Active }, Product = new Product { Company = App.curCompany } }, WmsSetupValues.NumRegs).OrderBy(f => f.IsFromErp).ToList();  
            View.Model.Record = null;

            //pw.Close();

        }

        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
            {
                View.Model.EntityList = service.GetKitAssembly(
                    new KitAssembly { Status = new Status { StatusID = EntityStatus.Active }, 
                                      Product = new Product { Company = App.curCompany } }, WmsSetupValues.NumRegs).OrderBy(f => f.IsFromErp).ToList(); 
                return;
            }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetKitAssembly(new KitAssembly 
                    { //AsmType = KitType.Custom, 
                      Status=new Status { StatusID = EntityStatus.Active}, 
                      Product = new Product {  Name = e.Value, Company = App.curCompany }
                    }, WmsSetupValues.NumRegs).OrderBy(f => f.IsFromErp).ToList(); 

            if (View.Model.EntityList.Count == 1)
            {
                View.ListRecords.SelectedIndex = 0;
                LoadKit(View.Model.EntityList[0]);
            }

        }

       
        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<KitAssembly> e)
        {
            if (e.Value == null)
                return;

            LoadKit(e.Value);
        }


        private void LoadKit(KitAssembly kit)
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.StkNewKit.Visibility = Visibility.Collapsed;
            View.Model.Record = kit;
            CleanComponentSection();
            //View.LvFormula.Items.Refresh();
            View.Model.FormulaList = service.GetKitAssemblyFormula(new
            KitAssemblyFormula { KitAssembly = new KitAssembly { Product = View.Model.Record.Product } });

            // si el KIT es ERP no es editable
            // CAA [2010/06/24]
            // Pero si puede borrarse.
            if (kit.AsmType == KitType.ERP)
            {
                View.BrdComponent.Visibility = Visibility.Collapsed;
                View.BtnRemove.Visibility = Visibility.Collapsed;
                View.BtnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                View.BrdComponent.Visibility = Visibility.Visible;
                View.BtnDelete.Visibility = Visibility.Visible;
                View.BtnRemove.Visibility = Visibility.Visible;
            }
        }

        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }

        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Hidden;
            View.StkNewKit.Visibility = Visibility.Visible;
            View.Model.Record = null;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new KitAssembly { };
            View.TxtFatherProduct.Text = "";
            View.TxtFatherProduct.ProductDesc = "";
        }

        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RowID == 0)
            {
                if (View.TxtFatherProduct.Product.ProductID == 0)
                {
                    Util.ShowError("Product is required.");
                    return;
                }
                else  // valida que ya no exista como KIT.
                {
                    if (service.GetKitAssembly(new KitAssembly { Product = View.TxtFatherProduct.Product }, WmsSetupValues.NumRegs).Count > 0)
                    {
                        Util.ShowError("Product is already a kit.");
                        return;
                    }
                }
                isNew = true;
                View.Model.Record.Product = View.TxtFatherProduct.Product;
                View.Model.Record.Unit = View.TxtFatherProduct.Product.BaseUnit; 
                View.Model.Record.AsmType = KitType.Custom;
                View.Model.Record.Status = new Status { StatusID = EntityStatus.Active};
                View.Model.Record.Method = 0;
                View.Model.Record.EfectiveDate = DateTime.Now;  
                View.Model.Record.ObsoleteDate = DateTime.Now;  
                View.Model.Record.CreationDate = DateTime.Now;
                View.Model.Record.CreatedBy = App.curUser.UserName;
                View.Model.Record.IsFromErp = false;

            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveKitAssembly(View.Model.Record);
                    View.Model.EntityList = service.GetKitAssembly(new KitAssembly { Status = new Status { StatusID = EntityStatus.Active }, Product = new Product { Company = App.curCompany } }, WmsSetupValues.NumRegs).OrderBy(f => f.IsFromErp).ToList();
                    //CleanToCreate();
                    LoadKit(View.Model.Record);
                }

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }

        }

        private void CleanComponentSection()
        {
            View.TxtQty.Text = "";
            View.TxtOrder.Text = "";
            View.TxtComponent.Text = "";
            View.TxtComponent.ProductDesc = "";
        }

        private void OnSaveComponent(object sender, EventArgs e)
        {
            string message = "";

            if (View.TxtComponent.Product.ProductID == 0 || string.IsNullOrEmpty(View.TxtQty.Text) || string.IsNullOrEmpty(View.TxtOrder.Text))
            {
                Util.ShowError("Product, Quantity and Order are required.");
                return;
            }
            else  // valida que ya no exista como componente del KIT.
            {
                // if (View.Model.Record.ProductFormula.Where(f => f.Component.ProductID == View.TxtComponent.Product.ProductID).Count() >0)
                if (service.GetKitAssemblyFormula(new KitAssemblyFormula
                {
                    KitAssembly = new KitAssembly { Product = View.Model.Record.Product }
                }).Where(f => f.Component.ProductID == View.TxtComponent.Product.ProductID).Count() > 0)
                {
                    Util.ShowError("Component is already in the kit.");
                    return;
                }
            }

            try
            {

                KitAssemblyFormula kaf = new KitAssemblyFormula();
                kaf.KitAssembly = View.Model.Record;
                kaf.Component = View.TxtComponent.Product;
                kaf.DirectProduct = View.Model.Record.Product;
                kaf.Unit = View.TxtComponent.Product.BaseUnit;
                kaf.Ord = int.Parse(View.TxtOrder.Text);
                kaf.Status = new Status { StatusID = EntityStatus.Active };
                kaf.FormulaQty = float.Parse(View.TxtQty.Text);
                kaf.ScrapPercent = 0;
                kaf.EfectiveDate = DateTime.Now;   
                kaf.ObsoleteDate = DateTime.Now;  
                kaf.CreationDate = DateTime.Now;
                kaf.CreatedBy = App.curUser.UserName;

                message = "Record created.";
                View.Model.FormulaList.Add(service.SaveKitAssemblyFormula(kaf));
                // View.Model.Record.ProductFormula.Add
                View.LvFormula.Items.Refresh();
                CleanComponentSection();

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }

        }

        private void RemoveComponent(object sender, EventArgs e)
        {
            if (View.LvFormula.SelectedItem == null)
                return;

            foreach (KitAssemblyFormula kaf in View.LvFormula.SelectedItems)
            {
                service.DeleteKitAssemblyFormula(kaf);
                View.Model.FormulaList.Remove(kaf);
            }

            View.LvFormula.Items.Refresh();

        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (View.Model.Record == null)
            {
                Util.ShowError("No record selected.");
                return;
            }

            if (UtilWindow.ConfirmOK("Are you sure about delete this kit and all its components?").Value)
            {
                try
                {
                    // 1ero se eliminan los componentes del kit
                    foreach (KitAssemblyFormula kaf in View.LvFormula.Items)
                    {
                        service.DeleteKitAssemblyFormula(kaf);
                    }

                    View.Model.Record.ProductFormula = null;
                    service.DeleteKitAssembly(View.Model.Record);
                    View.StkEdit.Visibility = Visibility.Hidden;
                    View.StkNewKit.Visibility = Visibility.Collapsed;

                    View.Model.EntityList.Remove(View.Model.Record);
                    View.ListRecords.Items.Refresh();

                    Util.ShowMessage("Record deleted.");

                }
                catch (Exception ex)
                {
                    Util.ShowError(ex.Message);
                }
            }
        }

    }
}