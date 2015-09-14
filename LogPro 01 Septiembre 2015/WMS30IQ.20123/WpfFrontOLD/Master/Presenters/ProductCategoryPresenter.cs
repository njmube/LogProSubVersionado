using System;
using WpfFront.Models;
using WpfFront.Views;
//using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using WpfFront.WMSBusinessService;
using WMComposite.Events;
using Assergs.Windows;

namespace WpfFront.Presenters
{

    public interface IProductCategoryPresenter
    {
       IProductCategoryView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class ProductCategoryPresenter : IProductCategoryPresenter
    {
        public IProductCategoryView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        public ProductCategoryPresenter(IUnityContainer container, IProductCategoryView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ProductCategoryModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<ProductCategory>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetProductCategory(new ProductCategory());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetProductCategory(new ProductCategory());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetProductCategory(new ProductCategory { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<ProductCategory> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;
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
            View.Model.Record = new ProductCategory();
            View.Model.Record.IsFromErp = false;
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.CategoryID == 0)
            {
                isNew = true;
                View.Model.Record.Company = App.curLocation.Company;                
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveProductCategory(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateProductCategory(View.Model.Record);
                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetProductCategory(new ProductCategory());

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            
        }


        private void OnDelete(object sender, EventArgs e)
        {

            if (View.Model.Record.CategoryID == 0)
            {
                Util.ShowError("No record selected.");
                return;
            }

            try
            {
                service.DeleteProductCategory(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetProductCategory(new ProductCategory());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}