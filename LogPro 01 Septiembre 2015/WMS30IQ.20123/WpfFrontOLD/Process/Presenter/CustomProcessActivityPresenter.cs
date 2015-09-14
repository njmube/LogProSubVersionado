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

    public interface ICustomProcessActivityPresenter
    {
        ICustomProcessActivityView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class CustomProcessActivityPresenter : ICustomProcessActivityPresenter
    {
        public ICustomProcessActivityView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public CustomProcessActivityPresenter(IUnityContainer container, ICustomProcessActivityView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<CustomProcessActivityModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<CustomProcessActivity>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetCustomProcessActivity(new CustomProcessActivity());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetCustomProcessActivity(new CustomProcessActivity());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetCustomProcessActivity(new CustomProcessActivity { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<CustomProcessActivity> e)
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
            View.Model.Record = new CustomProcessActivity { ActivityType = ProcessActivityType.Automatic };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.ActivityID == 0)
            {
                isNew = true;       
            }


            if (string.IsNullOrEmpty(View.Model.Record.Name) || View.Model.Record.ProcessType == null || View.Model.Record.Status == null)
            {
                Util.ShowError("Name, Process Type and Status are required.");
                return;
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveCustomProcessActivity(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateCustomProcessActivity(View.Model.Record);

                }

                View.Model.EntityList = service.GetCustomProcessActivity(new CustomProcessActivity());

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
                service.DeleteCustomProcessActivity(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetCustomProcessActivity(new CustomProcessActivity());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}