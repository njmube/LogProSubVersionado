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

    public interface IZoneManagerPresenter
    {
       IZoneManagerView View { get; set; }

       ToolWindow Window { get; set; }
    }


    public class ZoneManagerPresenter : IZoneManagerPresenter
    {
        public IZoneManagerView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;

        public ToolWindow Window { get; set; }


        public ZoneManagerPresenter(IUnityContainer container, IZoneManagerView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ZoneManagerModel>();

            //Event Delegate
            View.LoadRecords += new EventHandler<DataEventArgs<Location>>(this.OnLoadRecords);
            View.LoadToAdmin += new EventHandler<DataEventArgs<Zone>>(this.OnLoadToAdmin);
            View.AddBinByUser+= new EventHandler<EventArgs>(this.OnAddBinByUser);
            View.RemoveBinByUser += new EventHandler<EventArgs>(this.OnRemoveBinByUser);
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.AddPicker += new EventHandler<EventArgs>(this.OnAddPicker );
            view.RemovePicker += new EventHandler<EventArgs>(this.OnRemovePicker );
            view.LoadCriterias += new EventHandler<EventArgs>(this.OnLoadCriterias );
            View.LoadSearchCriteria+= new EventHandler<DataEventArgs<string>>(this.OnLoadSearchCriteria );
            view.AddRecord += new EventHandler<EventArgs>(this.OnAddRecord );
            view.RemoveRecord += new EventHandler<EventArgs>(this.OnRemoveRecord);

            View.Model.LocationList = service.GetLocation(new Location { Company = App.curCompany });
            View.Model.Record = null;

        }



        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadRecords(object sender, DataEventArgs<Location> e)
        {
            if (e.Value == null)
                return;

            View.Model.EntityList = service.GetZone(new Zone { Location = e.Value });
            View.ListRecords.Items.Refresh();

            View.StkInfo.Visibility = Visibility.Hidden;

            // limpiamos las grillas
            /*
            View.Model.AllowedList=null;
            View.Model.SubEntityList = null;
            */
        }



        //Carga los datos a Administrar
        private void OnLoadToAdmin(object sender, DataEventArgs<Zone> e)
        {
            if (e.Value == null)
                return;

            View.Model.Record = e.Value;

            View.StkInfo.Visibility = Visibility.Visible;

            // A. Bins
            IList<Bin> bins = service.GetBin(new Bin { Location = e.Value.Location });  // Zone= e.Value
            IList<ZoneBinRelation> bins_zone = service.GetZoneBinRelation(new ZoneBinRelation { Zone = e.Value });

            View.Model.SubEntityList = Bins_availables(bins, bins_zone);
            View.SubEntityList.Items.Refresh();

            View.Model.AllowedList = bins_zone;
            View.AllowedList.Items.Refresh();

            View.TxtSearch.Text = "";

            // B. Pickers                               // App.curLocation
            UserByRol userByRol = new UserByRol { Location = e.Value.Location , Rol = new Rol { RolID = BasicRol.Picker } };
            IList<UserByRol> users = service.GetUserByRol(userByRol);
            IList<ZonePickerRelation> users_zone = service.GetZonePickerRelation(new ZonePickerRelation { Zone = e.Value });

            View.Model.PickerList = Pickers_availables(users, users_zone);
            View.PickerList.Items.Refresh();

            View.Model.PickerListReg = users_zone;
            View.PickerListReg.Items.Refresh();

            // C. Entities (criterias admon)
            View.Model.ClassEntityList = service.GetClassEntity(new ClassEntity { BlnZoneCriteria = true });
            View.CriteriaList.ItemsSource = new List<Object>();
            View.CriteriaListReg.ItemsSource = new List<Object>();
            View.TxtSearchCriteria.Text = "";

        }

        public IList<Bin> Bins_availables(IList<Bin> bins, IList<ZoneBinRelation> bins_zone)
        {
            IList<Bin> bins_availables=bins;
            Bin bin_remove=null;

            foreach (ZoneBinRelation Zonebin in bins_zone)
            {
                // bins_availables.Remove(Zonebin.Bin);
                foreach (Bin bin in bins)
                {
                    if (Zonebin.Bin.BinID == bin.BinID)
                        bin_remove = bin;            
                }
                if (bin_remove != null)
                {
                    bins_availables.Remove(bin_remove);
                    bin_remove = null;
                }
            }
            return bins_availables;
        }

        public IList<UserByRol> Pickers_availables(IList<UserByRol> users, IList<ZonePickerRelation> users_zone)
        {
            IList<UserByRol> users_availables = users;
            UserByRol user_remove = null;

            foreach (ZonePickerRelation ZoneUser in users_zone)
            {
                foreach (UserByRol user in users)
                {
                    if (ZoneUser.Picker.UserID == user.User.UserID)
                        user_remove = user;
                }
                if (user_remove != null)
                {
                    users_availables.Remove(user_remove);
                    user_remove = null;
                }
            }
            return users_availables;
        }

        #region bins

        private void AddBin(Bin bin)
        {
            if (bin == null)
                return;

            try
            {
                ZoneBinRelation zoneBin = new ZoneBinRelation
                {
                    Zone = View.Model.Record,
                    Bin = bin,
                    Rank = 0, //bin.Rank,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                };

               zoneBin = service.SaveZoneBinRelation(zoneBin);
                View.Model.AllowedList.Insert(0, zoneBin);
                View.Model.SubEntityList.Remove(bin);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void OnAddBinByUser(object sender, EventArgs e)
        {
            try
            {
                foreach (Bin selItem in View.SubEntityList.SelectedItems)
                {
                    AddBin(selItem);
                }
                View.AllowedList.Items.Refresh();
                View.SubEntityList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Bin could not be loaded.\n" + ex.Message);
            }

        }
        
        private void RemoveBin(ZoneBinRelation Zonebin)
        {
            if (Zonebin == null)
                return;

            try
            {
                service.DeleteZoneBinRelation(Zonebin);
                View.Model.AllowedList.Remove(Zonebin);
                View.Model.SubEntityList.Add(Zonebin.Bin);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
         

        private void OnRemoveBinByUser(object sender, EventArgs e)
        {
            try
            {
                foreach (ZoneBinRelation selItem in View.AllowedList.SelectedItems)
                {
                    RemoveBin(selItem);
                }
                View.AllowedList.Items.Refresh();
                View.SubEntityList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Bin could not be removed.\n" + ex.Message);
            }

        }

        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {
            /*
            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;
             */

            IList<Bin> bins; 
            IList<ZoneBinRelation> bins_zone = service.GetZoneBinRelation(new ZoneBinRelation { Zone = View.Model.Record });

            if (string.IsNullOrEmpty(e.Value))
            {
                // trae todos los bins del location
                bins = service.GetBin(new Bin { Location = View.Model.Record.Location });
            }
            else
                //Busca por Nombre
                bins = service.GetBin(new Bin { Location = View.Model.Record.Location, BinCode = e.Value });

            View.Model.SubEntityList = Bins_availables(bins, bins_zone);
            View.SubEntityList.Items.Refresh();

        }
        #endregion

        #region Picker

        private void AddOnePicker(UserByRol user)
        {
            if (user == null)
                return;

            try
            {
                ZonePickerRelation zoneUser = new ZonePickerRelation
                {
                    Zone = View.Model.Record,
                    Picker = user.User,
                    Rank = 0, //bin.Rank,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                };

                zoneUser = service.SaveZonePickerRelation(zoneUser);
                View.Model.PickerListReg.Insert(0, zoneUser);
                View.Model.PickerList.Remove(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void OnAddPicker(object sender, EventArgs e)
        {
            try
            {
                foreach (UserByRol selItem in View.PickerList.SelectedItems)
                {
                   AddOnePicker(selItem);
                }
                View.PickerList.Items.Refresh();
                View.PickerListReg.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Picker could not be loaded.\n" + ex.Message);
            }

        }

        private void RemoveOnePicker(ZonePickerRelation ZoneUser)
        {
            if (ZoneUser == null)
                return;

            try
            {
                service.DeleteZonePickerRelation(ZoneUser);
                View.Model.PickerListReg.Remove(ZoneUser);
                View.Model.PickerList.Add(service.GetUserByRol(new UserByRol { User = ZoneUser.Picker, Rol= new Rol { RolID = BasicRol.Picker}, Location= App.curLocation }).First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OnRemovePicker(object sender, EventArgs e)
        {
            try
            {
                foreach (ZonePickerRelation selItem in View.PickerListReg.SelectedItems)
                {
                    RemoveOnePicker(selItem);
                }
                View.PickerListReg.Items.Refresh();
                View.PickerList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Picker could not be removed.\n" + ex.Message);
            }

        }
        #endregion

        #region CriteriasAdmin

        //
        //Carga los datos al seleccionar una entidad de la lista de criterios
        //
        private void OnLoadCriterias(object sender, EventArgs e)
        {
            LoadRecords(0);
            return;
        }

        private void LoadRecords(int type)
        {
            // entidad Base seleccionada
            ClassEntity entitySel = View.ClassEntityCmb.SelectedItem as ClassEntity;
            if (entitySel != null)
            {
                // criterios ya seleccionados para la Entidad Base en la Zona actual
                IList<ZoneEntityRelation> dataList = service.GetZoneEntityRelation(new ZoneEntityRelation { Zone = View.Model.Record, Entity = entitySel });
                List<Object> infoList = new List<object>();

                switch (entitySel.Name)
                {
                    case "Account":
                        if (type == 0)
                            View.CriteriaList.ItemsSource = service.GetCustomerAccount(new Account { Company = App.curCompany });
                        foreach (ZoneEntityRelation zoneEnt in dataList)
                        {
                            infoList.Add(service.GetCustomerAccount(new Account { AccountID = zoneEnt.EntityRowID, Company = App.curCompany }).First());
                        }
                        View.CriteriaListReg.ItemsSource = new List<object>();
                        View.CriteriaListReg.UpdateLayout();
                        View.CriteriaListReg.ItemsSource = infoList;
                        View.CriteriaListReg.UpdateLayout();
                        View.CriteriaListReg.Items.Refresh();
                        break;

                    case "Product":
                        if (type == 0)
                            View.CriteriaList.ItemsSource = service.GetProduct(new Product { Company = App.curCompany });
                        foreach (ZoneEntityRelation zoneEnt in dataList)
                        {
                            infoList.Add(service.GetProduct(new Product { ProductID = zoneEnt.EntityRowID, Company = App.curCompany }).First());
                        }
                        //View.CriteriaListReg.Items.Clear();
                        View.CriteriaListReg.ItemsSource = new List<object>();
                        View.CriteriaListReg.UpdateLayout();
                        View.CriteriaListReg.ItemsSource = infoList;
                        View.CriteriaListReg.UpdateLayout();
                        View.CriteriaListReg.Items.Refresh();
                        break;

                    default: break;
                }
            }
            return;
        }

        private void OnLoadSearchCriteria(object sender, DataEventArgs<string> e)
        {
            
            // entidad Base seleccionada
            ClassEntity entitySel = View.ClassEntityCmb.SelectedItem as ClassEntity;
            if (entitySel != null)
            {
                if (string.IsNullOrEmpty(e.Value))  // trae todos los datos por entidad
                {
                    switch (entitySel.Name)
                    {
                        case "Account":
                            View.CriteriaList.ItemsSource = service.GetCustomerAccount(new Account { Company = App.curCompany });
                            break;
                        case "Product":
                            View.CriteriaList.ItemsSource = service.GetProduct(new Product { Company = App.curCompany });
                            break;
                        default: break;
                    }
                    return;
                }

                if (e.Value.Length < WmsSetupValues.SearchLength)
                    return;

                //Busca por Nombre
                switch (entitySel.Name)
                {
                    case "Account":
                        View.CriteriaList.ItemsSource = service.GetCustomerAccount(new Account { Company = App.curCompany, Name = e.Value });
                        break;
                    case "Product":
                        View.CriteriaList.ItemsSource = service.GetProduct(new Product { Company = App.curCompany, Name = e.Value });
                        break;
                    default: break;
                }
            }
            return;
        }


        private void AddOneRecord(Object record)
        {
            if (record == null)
                return;

            // entidad Base seleccionada
            ClassEntity entitySel = View.ClassEntityCmb.SelectedItem as ClassEntity;
            if (entitySel == null)
                return;

            try
            {
                int entityRowId=0;
                switch (entitySel.Name)
                {
                    case "Account":
                        entityRowId = ((Account)record).AccountID;
                        // el registro ya existe en la lista destino?
                        if (View.CriteriaListReg.ItemsSource.Cast<Account>().Where(f => f.AccountID == entityRowId).Count() != 0)
                            return;  
                        break;
                    case "Product":
                        entityRowId = ((Product)record).ProductID;
                        // el registro ya existe en la lista destino?
                        if (View.CriteriaListReg.ItemsSource.Cast<Product>().Where(f => f.ProductID  == entityRowId).Count() != 0)
                            return;  
                        break;
                    default: break;
                }

                ZoneEntityRelation zoneEnt = new ZoneEntityRelation
                {
                    Zone = View.Model.Record,
                    Entity = entitySel,
                    Rank = 0, 
                    EntityRowID = entityRowId,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                };

                service.SaveZoneEntityRelation(zoneEnt);
                //View.CriteriaListReg.Items.Add(record);
                //View.CriteriaList.Items.Remove(record);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void OnAddRecord(object sender, EventArgs e)
        {
            try
            {
                //List<Object> list = View.CriteriaList.SelectedItems.Cast<Object>().ToList();
                foreach (Object selItem in View.CriteriaList.SelectedItems)  // list
                {
                    AddOneRecord(selItem);
                }
                LoadRecords(1);
                //View.CriteriaList.Items.Refresh();
                //View.CriteriaListReg.Items.Refresh();
            }
            catch (Exception ex)
            {
                Util.ShowError("Record could not be loaded.\n" + ex.Message);
            }
        }

        private void RemoveOneRecord(Object record)
        {
            if (record == null)
                return;

            // entidad Base seleccionada
            ClassEntity entitySel = View.ClassEntityCmb.SelectedItem as ClassEntity;
            if (entitySel == null)
                return;

            try
            {
                int entityRowId = 0;
                switch (entitySel.Name)
                {
                    case "Account":
                        entityRowId = ((Account)record).AccountID;
                        break;
                    case "Product":
                        entityRowId = ((Product)record).ProductID;
                        break;
                    default: break;
                }

                ZoneEntityRelation zoneEnt = service.GetZoneEntityRelation(new ZoneEntityRelation
                {
                    Zone = View.Model.Record,
                    Entity = entitySel,
                    EntityRowID = entityRowId,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Today
                }).First();

                service.DeleteZoneEntityRelation(zoneEnt);
                //View.CriteriaListReg.Items.Remove(record);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void OnRemoveRecord(object sender, EventArgs e)
        {
            try
            {
                foreach (Object selItem in View.CriteriaListReg.SelectedItems) 
                {
                    RemoveOneRecord(selItem);
                }
                LoadRecords(1);

            }
            catch (Exception ex)
            {
                Util.ShowError("Record could not be removed.\n" + ex.Message);
            }
        }

        #endregion

    }
}