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

    public interface ICustomProcessRoutePresenter
    {
       ICustomProcessRouteView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class CustomProcessRoutePresenter : ICustomProcessRoutePresenter
    {
        public ICustomProcessRouteView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public CustomProcessRoutePresenter(IUnityContainer container, ICustomProcessRouteView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<CustomProcessRouteModel>();


            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<CustomProcessRoute>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetCustomProcessRoute(new CustomProcessRoute());
            View.Model.Record = null;

            IList<CustomProcess> list = service.GetCustomProcess(new CustomProcess());
             View.Model.ProcessFromList = list;
             View.Model.ProcessToList = list;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetCustomProcessRoute(new CustomProcessRoute());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetCustomProcessRoute(new CustomProcessRoute());

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<CustomProcessRoute> e)
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
            View.Model.Record = new CustomProcessRoute { Company = App.curCompany };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RouteID == 0)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveCustomProcessRoute(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateCustomProcessRoute(View.Model.Record);

                }

                View.Model.EntityList = service.GetCustomProcessRoute(new CustomProcessRoute());

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
                service.DeleteCustomProcessRoute(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetCustomProcessRoute(new CustomProcessRoute());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}