using System;
using WpfFront.Models;
using WpfFront.Views;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;

namespace WpfFront.Presenters
{

    public interface IC_CasNumberPresenter
    {
        IC_CasNumberView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class C_CasNumberPresenter : IC_CasNumberPresenter
    {
        public IC_CasNumberView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
	    public ToolWindow Window { get; set; }


        public C_CasNumberPresenter(IUnityContainer container, IC_CasNumberView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<C_CasNumberModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<C_CasNumber>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.EntityList = service.GetC_CasNumber(new C_CasNumber());
            View.Model.Record = new C_CasNumber();

            View.TabRules.IsEnabled = false;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetC_CasNumber(new C_CasNumber());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetC_CasNumber(new C_CasNumber { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<C_CasNumber> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.TabMenu.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            //Cargando las Rules
            View.UC_CasNumRule.LoadData(View.Model.Record, true);
            View.TabRules.IsEnabled = true;

        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.TabMenu.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.TabRules.IsEnabled = false;

            View.Model.Record = new C_CasNumber();
            View.ListRecords.SelectedIndex = -1;
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.CasNumberID == 0)
            {
                isNew = true;       
            }


            try
            {

                if (string.IsNullOrEmpty(View.Model.Record.Code) || string.IsNullOrEmpty(View.Model.Record.Name))
                {
                    Util.ShowError("Cas# and Description are required.");
                    return;
                }


                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveC_CasNumber(View.Model.Record);
                    CleanToCreate();
                    View.TabRules.IsEnabled = true;
                    View.UC_CasNumRule.LoadData(View.Model.Record, true);

                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateC_CasNumber(View.Model.Record);

                }

                View.Model.EntityList = service.GetC_CasNumber(new C_CasNumber());

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
                service.DeleteC_CasNumber(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.TabMenu.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetC_CasNumber(new C_CasNumber());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}