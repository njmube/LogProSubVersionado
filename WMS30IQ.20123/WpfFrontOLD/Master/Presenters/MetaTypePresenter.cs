using System;
using WpfFront.Models;
using WpfFront.Views;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using Assergs.Windows;

namespace WpfFront.Presenters
{

    public interface IMetaTypePresenter
    {
       IMetaTypeView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class MetaTypePresenter : IMetaTypePresenter
    {
        public IMetaTypeView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
	    public ToolWindow Window { get; set; }


        public MetaTypePresenter(IUnityContainer container, IMetaTypeView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MetaTypeModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.NewDetail += new EventHandler<EventArgs>(this.OnNewDet);
            View.LoadData += new EventHandler<DataEventArgs<MType>>(this.OnLoadData);
            view.LoadDataDetail += new EventHandler<DataEventArgs<MMaster>>(this.OnLoadDataDetail);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.SaveDetail += new EventHandler<EventArgs>(this.OnSaveDet);
            View.DeleteDetail += new EventHandler<EventArgs>(this.OnDeleteDet);

            //View.Model.EntityList = service.GetMetaType(new MetaType());
            View.Model.Record = null;
            View.Model.EntityList = service.GetMType(new MType());

        }

        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetMType(new MType());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetMType(new MType { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<MType> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            View.BorderDetail.Visibility = Visibility.Visible;
            View.StkEditDet.Visibility = Visibility.Collapsed;
            View.Model.DetailList = service.GetMMaster(new MMaster { MetaType = e.Value });
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
            View.Model.Record = new MType();

            View.StkEditDet.Visibility = Visibility.Collapsed;
            View.BorderDetail.Visibility = Visibility.Collapsed;
        }


        private void OnSave(object sender, EventArgs e)
        {
            if (View.txtNameType.Text.Equals(""))
            {
                Util.ShowError("Name is required.");
                return;
            }

            string message = "";
            bool isNew = false;

            if (View.Model.Record.MetaTypeID == 0)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveMType(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateMType(View.Model.Record);

                }

                View.Model.EntityList = service.GetMType(new MType());

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
                service.DeleteMType(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.StkEditDet.Visibility = Visibility.Collapsed;
                View.Model.EntityList = service.GetMType(new MType());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


        // Detail section

        private void OnNewDet(object sender, EventArgs e)
        {
            CleanToCreateDetail();
        }


        public void CleanToCreateDetail()
        {
            View.StkEditDet.Visibility = Visibility.Visible;
            View.BtnDeleteDet.Visibility = Visibility.Collapsed;
            View.Model.DetailRecord = null;
            // View.ListRecordsDet.SelectedItem = null;
            View.ListRecordsDet.SelectedIndex = -1;
            View.Model.DetailRecord = new MMaster();

        }


        private void OnSaveDet(object sender, EventArgs e)
        {
            if (View.txtNameMaster.Text.Equals(""))
            {
                Util.ShowError("Name is required.");
                return;
            }

            int res=0;
            if (!int.TryParse(View.txtOrderNum.Text,out res))
            {
                Util.ShowError("Order is numeric.");
                return;
            }

            string message = "";
            bool isNew = false;

            if (View.Model.DetailRecord.MetaMasterID == 0)
            {
                isNew = true;
            }

            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.DetailRecord.MetaType = View.Model.Record;
                    View.Model.DetailRecord = service.SaveMMaster(View.Model.DetailRecord);
                    CleanToCreateDetail();
                }
                else
                {
                    message = "Record updated.";
                    View.StkEditDet.Visibility = Visibility.Collapsed;
                    service.UpdateMMaster(View.Model.DetailRecord);

                }

                View.Model.DetailList = service.GetMMaster(new MMaster { MetaType = View.Model.Record });

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }

        }


        private void OnDeleteDet(object sender, EventArgs e)
        {

            if (View.Model.DetailRecord == null)
            {
                Util.ShowError("No record selected.");
                return;
            }

            try
            {
                service.DeleteMMaster(View.Model.DetailRecord);
                Util.ShowMessage("Record deleted.");

                View.StkEditDet.Visibility = Visibility.Collapsed;
                View.Model.DetailList = service.GetMMaster(new MMaster { MetaType = View.Model.Record });
            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }

        }


        //Carga los datos al seleccionar un registro de la lista detalle
        private void OnLoadDataDetail(object sender, DataEventArgs<MMaster> e)
        {
            if (e.Value == null)
                return;

            View.BtnDeleteDet.Visibility = Visibility.Visible;
            View.StkEditDet.Visibility = Visibility.Visible;
            View.Model.DetailRecord = e.Value;

        }
    }
}