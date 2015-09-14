using System;
using WpfFront.Models;
using WpfFront.Views;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using WMComposite.Events;
using Assergs.Windows;
using System.Linq;

namespace WpfFront.Presenters
{

    public interface IShippingMethodPresenter
    {
       IShippingMethodView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class ShippingMethodPresenter : IShippingMethodPresenter
    {
        public IShippingMethodView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
	    public ToolWindow Window { get; set; }


        public ShippingMethodPresenter(IUnityContainer container, IShippingMethodView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ShippingMethodModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<ShippingMethod>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetShippingMethod(new ShippingMethod());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetShippingMethod(new ShippingMethod());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetShippingMethod(new ShippingMethod { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<ShippingMethod> e)
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
            View.ListRecords.SelectedItem = -1;
            View.Model.Record = new ShippingMethod { Company = App.curCompany, IsFromErp = false };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.ShpMethodID == 0)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record = service.SaveShippingMethod(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    View.Model.Record.ModDate = DateTime.Now;
                    service.UpdateShippingMethod(View.Model.Record);

                }

                View.Model.EntityList = service.GetShippingMethod(new ShippingMethod());

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
                service.DeleteShippingMethod(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetShippingMethod(new ShippingMethod());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}