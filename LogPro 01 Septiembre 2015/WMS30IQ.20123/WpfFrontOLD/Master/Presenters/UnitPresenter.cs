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

    public interface IUnitPresenter
    {
       IUnitView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class UnitPresenter : IUnitPresenter
    {
        public IUnitView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private bool OnlyGroups = false;
        public ToolWindow Window { get; set; }


        public UnitPresenter(IUnityContainer container, IUnitView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<UnitModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Unit>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.ShowOnlyGroups += new EventHandler<EventArgs>(OnShowOnlyGroups);


            ProcessWindow pw = new ProcessWindow("Loading ...");

            View.Model.EntityList = service.GetUnit(new Unit { Company = App.curCompany });
            View.Model.Record = null;
            View.TxtUnitGroup.IsEnabled = false;

            //List Height
            View.ListRecords.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 250;

            //loading Units
            View.Model.UnitGroupList = service.GetUnit(new Unit { Company = App.curCompany,  BaseAmount = 1 }).Where(f=>f.BaseAmount==1).ToList();

            pw.Close();
        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetUnit(new Unit { Company = App.curCompany });
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetUnit(new Unit {Company = App.curCompany, Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Unit> e)
        {
            if (e.Value == null)
                return;



            //Show Text, Hide Combo
            View.CboUnitGroup.Visibility = Visibility.Collapsed;
            View.TxtUnitGroup.Visibility = Visibility.Visible;


            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            //Bloqueo de campos
            if (View.Model.Record.BaseAmount == 1.0 || View.Model.Record.IsFromErp == true)
                //Solo se pueden crear, nada de eliminar
                View.StkButtons.Visibility = Visibility.Collapsed;
            else
                View.StkButtons.Visibility = Visibility.Visible;           

        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();

            //Show Combo, Hide texbox
            if (OnlyGroups)
            {
                View.CboUnitGroup.Visibility = Visibility.Collapsed;
                View.TxtUnitGroup.Visibility = Visibility.Visible;
            }
            else
            {
                View.CboUnitGroup.Visibility = Visibility.Visible;
                View.TxtUnitGroup.Visibility = Visibility.Collapsed;
            }
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.Model.Record = null;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new Unit();
            View.TxtUnitGroup.IsEnabled = true;
            View.StkButtons.Visibility = Visibility.Visible;

            //Evaluar si va a crear una unidad nueva o un Unit Group Nuevo
            if (OnlyGroups)
            {
                View.TxtBaseAmount.Text = "1";
                View.Model.Record.BaseAmount = 1;
                View.TxtBaseAmount.IsEnabled = false;
            }
            else
                View.TxtBaseAmount.IsEnabled = true;

        }



        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.UnitID == 0)
            {
                isNew = true;
                View.Model.Record.Company = App.curLocation.Company;
                View.Model.Record.CreationDate = DateTime.Now;
                View.Model.Record.CreatedBy = App.curUser.UserName;
                View.Model.Record.IsFromErp = false;
            }


            try
            {
                string checkResult = ValidateData(View.Model.Record);

                //si hay un error de validacion
                if (!string.IsNullOrEmpty(checkResult))
                {
                    Util.ShowError(checkResult);
                    return;
                }

                if (isNew)   // new
                {
                    if (View.CboUnitGroup.SelectedItem == null && OnlyGroups == false)
                    {
                        Util.ShowError("Please enter a Unit Group.");
                        return;
                    }

                    Unit grpUnit = null;
                    if (!OnlyGroups)
                    {
                        grpUnit = (Unit)View.CboUnitGroup.SelectedItem;
                        View.Model.Record.ErpCodeGroup = grpUnit.ErpCodeGroup;
                    }

                    message = "Record created.";

                    View.Model.Record = service.SaveUnit(View.Model.Record);
                    CleanToCreate();
                    View.Model.UnitGroupList = service.GetUnit(new Unit {Company = App.curCompany, BaseAmount = 1 }).Where(f => f.BaseAmount == 1).ToList();
                    View.CboUnitGroup.Items.Refresh();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateUnit(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetUnit(new Unit{Company = App.curCompany});

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            
        }


        public string ValidateData(Unit unit)
        {
            //valida que los datos a ingresar sean correctos

            if (OnlyGroups)
            { //Valida que el Grupo no exista ya en el sistema
                if (View.Model.EntityList.Where(f => f.ErpCodeGroup == unit.ErpCodeGroup).Count() > 0)
                    return "Unit Group " + unit.ErpCodeGroup + " already exists.";

            }
            else
            {
                //El base amount debe ser mayor que 1
                if (unit.BaseAmount <= 1)
                    return "Base Amount must be greater than 1.";

                //Valida que exista el unit group
                //if (View.Model.EntityList.Where(f => f.ErpCodeGroup == unit.ErpCodeGroup).Count() == 0)
                    //return "Unit Group "+ unit.ErpCodeGroup +" does not exists.";

                Unit grpUnit = (Unit)View.CboUnitGroup.SelectedItem;

                if (grpUnit == null || View.Model.EntityList.Where(f => f.ErpCodeGroup == grpUnit.ErpCodeGroup).Count() == 0)
                    return "Unit Group " + unit.ErpCodeGroup + " does not exists.";

                if (View.Model.EntityList.Where(f => f.Name == View.Model.Record.Name && f.ErpCodeGroup == grpUnit.ErpCodeGroup).Count() > 0)
                    return "Unit Name " + unit.ErpCodeGroup + " already exists.";

            }

            return "";
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
                service.DeleteUnit(View.Model.Record);
                

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetUnit(new Unit { Company = App.curCompany });
                Util.ShowMessage("Record deleted.");

            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


        private void OnShowOnlyGroups(object sender, EventArgs e)
        {
            //Los grupos son las que tienen unidad Base = 1.0
            if (((CheckBox)sender).IsChecked == true)
            {
                View.Model.EntityList = service.GetUnit(new Unit { Company = App.curCompany }).Where(f => f.BaseAmount == 1.0).ToList();
                OnlyGroups = true;               

            }
            else
            {
                View.Model.EntityList = service.GetUnit(new Unit { Company = App.curCompany });
                OnlyGroups = false;
            }

            View.StkEdit.Visibility = Visibility.Collapsed;
            View.Model.Record = null;
        }
    }
}