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

    public interface ICustomProcessTransitionPresenter
    {
        ICustomProcessTransitionView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class CustomProcessTransitionPresenter : ICustomProcessTransitionPresenter
    {
        public ICustomProcessTransitionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public CustomProcessTransitionPresenter(IUnityContainer container, ICustomProcessTransitionView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<CustomProcessTransitionModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<CustomProcessTransition>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.LoadContextKey += new EventHandler<EventArgs>(View_LoadContextKey);

            View.Model.EntityList = service.GetCustomProcessTransition(new CustomProcessTransition());
            View.Model.Record = null;

            view.Model.CustomProcessList = service.GetCustomProcess(new CustomProcess());
            View.Model.CustomProcessActivityList = service.GetCustomProcessActivity(new CustomProcessActivity());
            View.Model.CustomProcessNextActivityList = service.GetCustomProcessActivity(new CustomProcessActivity());
        }


        void View_LoadContextKey(object sender, EventArgs e)
        {
            View.Model.ProcessContextList = service.GetCustomProcessContext(new CustomProcessContext { ProcessType = View.Model.Record.Process.ProcessType });

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetCustomProcessTransition(new CustomProcessTransition());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetCustomProcessTransition(new CustomProcessTransition { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<CustomProcessTransition> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            if (View.Model.Record.Process != null)
                View.Model.ProcessContextList = service.GetCustomProcessContext(new CustomProcessContext { ProcessType = View.Model.Record.Process.ProcessType });
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
            View.Model.Record = new CustomProcessTransition { 
                Status = new Status { StatusID = EntityStatus.Active },
                Process = new CustomProcess(),
                CurrentActivity = new CustomProcessActivity()
            };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RowID == 0)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    message = "Record created.";
                    View.Model.Record = service.SaveCustomProcessTransition(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    message = "Record updated.";
                    service.UpdateCustomProcessTransition(View.Model.Record);

                }

                View.Model.EntityList = service.GetCustomProcessTransition(new CustomProcessTransition());

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
                service.DeleteCustomProcessTransition(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetCustomProcessTransition(new CustomProcessTransition());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}