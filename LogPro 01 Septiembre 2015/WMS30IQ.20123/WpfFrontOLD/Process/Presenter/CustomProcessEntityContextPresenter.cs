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

namespace WpfFront.Presenters
{

    public interface ICustomProcessContextByEntityPresenter
    {
        ICustomProcessContextByEntityView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class CustomProcessContextByEntityPresenter : ICustomProcessContextByEntityPresenter
    {
        public ICustomProcessContextByEntityView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public CustomProcessContextByEntityPresenter(IUnityContainer container, ICustomProcessContextByEntityView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<CustomProcessContextByEntityModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<CustomProcessContextByEntity>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetCustomProcessContextByEntity(new CustomProcessContextByEntity());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetCustomProcessContextByEntity(new CustomProcessContextByEntity());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetCustomProcessContextByEntity(new CustomProcessContextByEntity { ContextKey = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<CustomProcessContextByEntity> e)
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
            View.Model.Record = new CustomProcessContextByEntity();
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record == null)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveCustomProcessContextByEntity(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateCustomProcessContextByEntity(View.Model.Record);

                }

                View.Model.EntityList = service.GetCustomProcessContextByEntity(new CustomProcessContextByEntity());

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
                service.DeleteCustomProcessContextByEntity(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetCustomProcessContextByEntity(new CustomProcessContextByEntity());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}