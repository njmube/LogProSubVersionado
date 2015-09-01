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

    public interface IMenuOptionPresenter
    {
        IMenuOptionView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class MenuOptionPresenter : IMenuOptionPresenter
    {
        public IMenuOptionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public MenuOptionPresenter(IUnityContainer container, IMenuOptionView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MenuOptionModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<MenuOption>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            ProcessWindow pw = new ProcessWindow("Loading ...");

            View.Model.EntityList = service.GetMenuOption(new MenuOption());
            View.Model.Record = null;

            View.Model.MenuOptionTypeList = service.GetMenuOptionType(new MenuOptionType());
            View.Model.OptionType = service.GetOptionType(new OptionType());

            pw.Close();
        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetMenuOption(new MenuOption());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetMenuOption(new MenuOption { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<MenuOption> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            try { View.Model.RecordExt = service.GetMenuOptionExtension(new MenuOptionExtension { MenuOption = e.Value }).First(); }
            catch { View.Model.RecordExt = new MenuOptionExtension { MenuOption = e.Value }; }

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
            View.Model.Record = new MenuOption { CreatedBy = App.curUser.UserName,
                                                CreationDate = DateTime.Now,
                                                MenuOptionType = new MenuOptionType(),
                                                OptionType = new OptionType() };

            View.Model.RecordExt = new MenuOptionExtension();

        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.MenuOptionID == 0)
            {
                isNew = true;
                ////View.Model.Record.Company = App.curLocation.Company;                
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveMenuOption(View.Model.Record);
                    View.Model.RecordExt.MenuOption = View.Model.Record;
                    View.Model.RecordExt = service.SaveMenuOptionExtension(View.Model.RecordExt);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateMenuOption(View.Model.Record);

                    if (View.Model.RecordExt == null || View.Model.RecordExt.RowID == 0)
                    {
                        View.Model.RecordExt.MenuOption = View.Model.Record;
                        service.SaveMenuOptionExtension(View.Model.RecordExt);
                    }
                    else
                    {
                        service.UpdateMenuOptionExtension(View.Model.RecordExt);
                    }
                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetMenuOption(new MenuOption());

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
                service.DeleteMenuOption(View.Model.Record);

               if (View.Model.RecordExt == null || View.Model.RecordExt.RowID == 0)
                   service.DeleteMenuOptionExtension(View.Model.RecordExt);

                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetMenuOption(new MenuOption());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}