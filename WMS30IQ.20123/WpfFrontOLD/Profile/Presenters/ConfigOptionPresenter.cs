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
using System.Linq;
using WpfFront.Services;

namespace WpfFront.Presenters
{

    public interface IConfigOptionPresenter
    {
        IConfigOptionView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ConfigOptionPresenter : IConfigOptionPresenter
    {
        public IConfigOptionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public ConfigOptionPresenter(IUnityContainer container, IConfigOptionView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ConfigOptionModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<ConfigOptionByCompany>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.FilterBy += new EventHandler<DataEventArgs<ConfigType>>(View_FilterBy);

            ProcessWindow pw = new ProcessWindow("Loading ...");

            if (App.curRol.Rol.RolID == BasicRol.Admin)
                View.Model.EntityList = service.GetConfigOptionByCompany(new ConfigOptionByCompany { Company = App.curCompany });
            else
                View.Model.EntityList = service.GetConfigOptionByCompany(new ConfigOptionByCompany { Company = App.curCompany })
                    .Where(f => f.ConfigOption.IsAdmin == true).ToList();
            
            View.Model.Record = null;

            View.Model.TypeList = service.GetConfigType(new ConfigType());
            View.Model.TypeList.Add(new ConfigType());

            //List Height
            View.ListRecords.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 250;

            pw.Close();

        }



        void View_FilterBy(object sender, DataEventArgs<ConfigType> e)
        {
            View.Model.EntityList = service.GetConfigOptionByCompany(
                new ConfigOptionByCompany
                {
                    Company = App.curCompany,
                    ConfigOption = new ConfigOption { ConfigType = e.Value }
                });

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {
            return;
        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<ConfigOptionByCompany> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            //View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;
        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            //View.BtnDelete.Visibility = Visibility.Hidden;
            View.Model.Record = null;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new ConfigOptionByCompany();
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RowID == 0)
            {
                isNew = true;
                //View.Model.Record.Company = App.curLocation.Company;                
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveConfigOptionByCompany(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateConfigOptionByCompany(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetConfigOptionByCompany(new ConfigOptionByCompany { Company = App.curCompany });

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            

        }


        private void OnDelete(object sender, EventArgs e)
        {
            return;

            //if (View.Model.Record == null)
            //{
            //    Util.ShowError("No record selected.");
            //    return;
            //}

            //try
            //{
            //    service.DeleteConfigOptionByCompany(View.Model.Record);
            //    Util.ShowMessage("Record deleted.");

            //    View.StkEdit.Visibility = Visibility.Hidden;
            //    View.Model.EntityList = service.GetConfigOption(new ConfigOption());
            //}
            //catch(Exception ex)
            //{
            //    Util.ShowError(ex.Message);
            //}



        }


    }
}