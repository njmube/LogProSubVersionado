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

    public interface IRolPresenter
    {
        IRolView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class RolPresenter : IRolPresenter
    {
        public IRolView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public RolPresenter(IUnityContainer container, IRolView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<RolModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Rol>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.AddRolMenuOption += new EventHandler<EventArgs>(OnAddRolMenuOption);

            View.Model.EntityList = service.GetRol(new Rol());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetRol(new Rol());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetRol(new Rol { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Rol> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            View.TbRolPerm.Visibility = Visibility.Visible;
            LoadPermissions();
                        
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
            View.Model.Record = new Rol();
            View.TbRolPerm.Visibility = Visibility.Collapsed;
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RolID == 0)
            {
                isNew = true;            
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveRol(View.Model.Record);
                    View.TbRolPerm.Visibility = Visibility.Visible;
                    View.TbRolPerm.Focus();
                    //CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateRol(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetRol(new Rol());

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            



        }


        private void OnDelete(object sender, EventArgs e)
        {

            // Removing a Permission
            if (((Button)sender).Name == "btnRemPermission")
                RemovePermission();


            // Removing a Rol
            if (((Button)sender).Name == "btnDelete")
                RemoveRol();   
         
        }


        // Remove Permission

        private void RemovePermission()
        {
            if (View.LvAssignPermission.SelectedItem == null)
                return;

            MenuOptionByRol menuOptbyRol = null;
            string msg = "";


            foreach(Object obj in View.LvAssignPermission.SelectedItems)
            {
                try
                {
                    menuOptbyRol = (MenuOptionByRol)obj;
                    View.Model.AssignPermission.Remove(menuOptbyRol);
                    service.DeleteMenuOptionByRol(menuOptbyRol);
                }

                catch (Exception ex)
                {
                    msg += "Error trying to delete Menu Option: " + menuOptbyRol.MenuOption.Name + ". " + ex.Message;
                }
            }

            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            LoadPermissions();
        }


        // Removing Rols

        private void RemoveRol()
        {
            if (View.Model.Record == null)
            {
                Util.ShowError("No record selected.");
                return;
            }

            try
            {
                //Borra las menu Options del Rol y luego el Rol
                MenuOptionByRol mr = new MenuOptionByRol { Rol = View.Model.Record, Company = App.curCompany };
                foreach (MenuOptionByRol mo in service.GetMenuOptionByRol(mr))
                    service.DeleteMenuOptionByRol(mo);

                service.DeleteRol(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetRol(new Rol());
            }
            catch
            {
                Util.ShowError("Record could not be deleted.\n");
            }
        
        }


        //Carga las Opciones de Track
        private void LoadPermissions()
        {
            //Available Permissions
            View.Model.AvailablePermission = service.GetMenuOption(
                new MenuOption()).OrderBy(f=>f.MenuOptionType.MenuOptionTypeID).ToList(); 

            //Assigned Permissions
            View.Model.AssignPermission = service.GetMenuOptionByRol(
                new MenuOptionByRol { Rol = View.Model.Record,
                                      Company = App.curCompany
                                    }).OrderBy(f=>f.MenuOption.MenuOptionType.MenuOptionTypeID).ToList();

            //TrackOption curTrack;
            MenuOption curMenuOption;
            
            foreach (MenuOption menuOp in View.Model.AssignPermission.Select(f => f.MenuOption))
            {
                curMenuOption = View.Model.AvailablePermission.Where(f => f.MenuOptionID == menuOp.MenuOptionID).First();
                if (curMenuOption != null)
                    View.Model.AvailablePermission.Remove(curMenuOption);
            }

            View.LvAvailablePermission.Items.Refresh();
            View.LvAssignPermission.Items.Refresh();
        }



        private void OnAddRolMenuOption(object sender, EventArgs e)
        {
            if (View.LvAvailablePermission.SelectedItems == null || View.LvAvailablePermission.SelectedItems.Count == 0)
                return;

            try
            {
                foreach (MenuOption selItem in View.LvAvailablePermission.SelectedItems)
                {
                    AddMenuOption(selItem);
                }
                View.LvAvailablePermission.Items.Refresh();
                View.LvAssignPermission.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Menu Option could not be assigned.\n" + ex.Message);
            }
        }


        private void AddMenuOption(MenuOption menuOption)
        {
            if (menuOption == null)
                return;

            try
            {
                MenuOptionByRol relation = new MenuOptionByRol                             
                {
                    Rol = View.Model.Record,
                    MenuOption = menuOption,
                    Company = App.curCompany,
                    //ModDate = DateTime.Today,
                    //ModifiedBy = App.curUser.UserName,
                    Status = new Status{ StatusID = EntityStatus.Active },
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today                    
                };

                relation = service.SaveMenuOptionByRol(relation);

                if (View.Model.AssignPermission == null)
                    View.Model.AssignPermission = new List<MenuOptionByRol>();

                View.Model.AssignPermission.Insert(0, relation);

                View.Model.AvailablePermission.Remove(menuOption);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


    }
}