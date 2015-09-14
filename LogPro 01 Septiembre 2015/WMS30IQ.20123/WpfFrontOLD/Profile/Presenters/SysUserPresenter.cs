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

    public interface ISysUserPresenter
    {
        ISysUserView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class SysUserPresenter : ISysUserPresenter
    {
        public ISysUserView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public SysUserPresenter(IUnityContainer container, ISysUserView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<SysUserModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<EventArgs>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.AddRol += new EventHandler<EventArgs>(this.OnAddRol);
            View.RemoveFromList += new EventHandler<EventArgs>(this.OnRemoveFromList);

            ProcessWindow pw = new ProcessWindow("Loading ...");

            View.Model.EntityList = service.GetSysUser(new SysUser());
            //View.Model.Record = null;

            View.Model.LocationList = service.GetLocation(new Location { Company = App.curCompany }).OrderBy(f => f.Name).ToList();
            View.Model.ListRol = service.GetRol(new Rol()).OrderBy(f=>f.Name).ToList();

            pw.Close();

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {
            View.TbRol.IsEnabled = true;

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetSysUser(new SysUser());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetSysUser(new SysUser());


            if (View.Model.Record.UserID == 1 && App.curUser.UserID != 1)
                View.TbRol.IsEnabled = false;

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, EventArgs e)
        {
            if (View.ListRecords.SelectedItem == null)
                return;

            View.TbRol.Visibility = Visibility.Visible;

            View.CboLocation.SelectedIndex = -1;
            View.CboRol.SelectedIndex = -1;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = View.ListRecords.SelectedItem as SysUser;
            View.Model.Record.DecryptPass = ""; // Util.DeCryptPasswd(View.Model.Record.UserName, View.Model.Record.Password);
            LoadRol();


            /*
            //if (e.Value == null)
            //    return;


            View.TbRol.Visibility = Visibility.Visible;


            View.CboLocation.SelectedIndex = -1;
            View.CboRol.SelectedIndex = -1;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;
            View.Model.Record.DecryptPass = Util.DeCryptPasswd(View.Model.Record.UserName, View.Model.Record.Password);
            LoadRol();

            */
        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.TbRol.Visibility = Visibility.Collapsed;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new SysUser();
            View.TbUser.SelectedIndex = 0;
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;


            if (string.IsNullOrEmpty(View.Model.Record.UserName) || string.IsNullOrEmpty(View.Model.Record.FirstName)) //|| string.IsNullOrEmpty(View.Model.Record.DecryptPass)
            {
                Util.ShowError("UserName, First Name & Last Name are required."); 
                return; 
            }


            if (View.Model.Record.UserID == 0)
                isNew = true;            


            try
            {
                if (!string.IsNullOrEmpty(View.Model.Record.DecryptPass))
                    View.Model.Record.Password = Util.CryptPasswd(View.Model.Record.DecryptPass, View.Model.Record.UserName);
                

                if (isNew)   // new
                {
                    message = "Record created.\nPlease setup user permission.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveSysUser(View.Model.Record);

                    View.Model.UserRolList = null;
                    View.TbUser.SelectedIndex = 1;
                    View.TbRol.Visibility = Visibility.Visible;
                    View.TbRol.Focus();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                   
                    bool saved = false;
                    while (!saved)
                    {
                        try { service.UpdateSysUser(View.Model.Record); saved = true; }
                        catch { }
                    }
                }

                

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetSysUser(new SysUser());

                Util.ShowMessage(message);

            }
            catch (Exception ex) {
                try
                {
                    service.UpdateSysUser(View.Model.Record); 
                    Util.ShowMessage(message);
                }
                catch
                {
                    Util.ShowError("User could not be updated. \n" + ex.Message);
                }
            }
            



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

                //Borra las menu Options del Rol y luego el Rol
                UserByRol mr = new UserByRol { User = View.Model.Record };
                foreach (UserByRol mo in service.GetUserByRol(mr))
                    service.DeleteUserByRol(mo);


                service.DeleteSysUser(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetSysUser(new SysUser());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }

        }



        private void OnRemoveFromList(object sender, EventArgs e)
        {
            // Removing a Bin
            if (((Button)sender).Name == "btnRemRol")
                RemoveRol();

        }



        public void RemoveRol()
        {
            UserByRol pUserRol = null;
            string msg = "";

            if (View.LvRol.SelectedItems == null)
                return;

            foreach (Object obj in View.LvRol.SelectedItems)
            {
                //try
                //{
                    pUserRol = (UserByRol)obj;
                    View.Model.UserRolList.Remove((UserByRol)obj);

                    bool deleted = false;
                    while (!deleted)
                    {
                        try
                        {
                            service.DeleteUserByRol(pUserRol);
                            deleted = true;
                        }
                        catch { }
                    }
                
                
                //}

                //catch (Exception ex)
                //{
                //    msg += "Error trying to delete Rol: " + pUserRol.Rol.Name + ". " + ex.Message;
                //}
            }


            if (!string.IsNullOrEmpty(msg))
            {
                Util.ShowError(msg);
            }

            LoadRol();
        }


        //Units
        //Carga las Opciones de Track
        private void LoadRol()
        {

            //Assigned Unit
            View.Model.UserRolList = service.GetUserByRol(
                new UserByRol { User = View.Model.Record });


            //View.StkRolForm.Visibility = Visibility.Visible;

            //if (View.Model.UserRolList.Count > 0)
                //View.StkRolForm.Visibility = Visibility.Collapsed;

            View.LvRol.Items.Refresh();
        }



        private void OnAddRol(object sender, EventArgs e)
        {
            Rol curRol = View.CboRol.SelectedItem as Rol;
            Location curLocation = View.CboLocation.SelectedItem as Location;

            if (curLocation == null || curRol == null)
            {
                Util.ShowError("Please enter Rol and Location.");
                return;
            }

             UserByRol relation = new UserByRol
                {
                    User = View.Model.Record,
                    Location = curLocation               
                };


             IList<UserByRol> list = service.GetUserByRol(relation);
             if (list != null && list.Count > 0)
             {
                 Util.ShowError("Location was already assigned.");
                 return;
             }


            try
            {
                relation.Rol = curRol;
                relation.CreationDate = DateTime.Now;
                relation.CreatedBy = App.curUser.UserName;

                relation = service.SaveUserByRol(relation);
                LoadRol();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}