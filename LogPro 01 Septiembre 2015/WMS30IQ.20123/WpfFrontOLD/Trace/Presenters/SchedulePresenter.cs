using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;
using System.Data;

namespace WpfFront.Presenters
{
    public interface ISchedulePresenter
    {
        IScheduleView View { get; set; }
        ToolWindow Window { get; set; }
    }

    public class SchedulePresenter : ISchedulePresenter
    {
        public IScheduleView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public SchedulePresenter(IUnityContainer container, IScheduleView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ScheduleModel>();

            //Event Delegate
            View.LoadData += new EventHandler<DataEventArgs<CountSchedule>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            // ProcessWindow pw = new ProcessWindow("Loading ...");

            load_list();
            View.Model.Record = null;

            //pw.Close();
        }

        private void load_list()
        {
            View.Model.EntityList = service.GetCountSchedule(new CountSchedule { IsDone=false });
        }
        
        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<CountSchedule> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            // convierte los params de búsq. del query en una cadena
            DataSet paramsQuery = Util.GetDataSet(View.Model.Record.Parameters);
            string cond = "";
            foreach (DataTable tbl in paramsQuery.Tables)
            {
                foreach (DataRow row in tbl.Rows)
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(row[2].ToString()))
                            cond += row[2].ToString() + "\n";
                    }
                    catch { continue; }
                }
            }
            View.Model.Conditions = cond;

            
            //Bloqueo de campos...  segun si las fechas ya caducaron
            if (View.TxtSchDateFrom.SelectedDate < DateTime.Today)
                View.TxtSchDateFrom.IsEnabled = false;
            //View.StkButtons.Visibility = Visibility.Collapsed;
            else
                View.TxtSchDateFrom.IsEnabled = true;

            if (View.TxtSchDateTo.SelectedDate < DateTime.Today)
                View.TxtSchDateTo.IsEnabled = false;
            else
                View.TxtSchDateTo.IsEnabled = true;
            
            /*
            if (View.TxtSchNextDate.SelectedDate < DateTime.Today)
                View.TxtSchNextDate.IsEnabled = false;
            else
                View.TxtSchNextDate.IsEnabled = true;
            */
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            if (View.Model.Record.RowID == 0)
            {
                return;
            }

            try
            {
                message = "Record updated.";
                View.Model.Record.ModDate = DateTime.Now;
                View.Model.Record.ModifiedBy = App.curUser.UserName;
                service.UpdateCountSchedule(View.Model.Record);
                    
                load_list();

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
                service.DeleteCountSchedule(View.Model.Record);

                View.StkEdit.Visibility = Visibility.Hidden;
                load_list();
                //View.Model.EntityList = service.GetCountSchedule(new CountSchedule{ });
                Util.ShowMessage("Record deleted.");

            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }

        }
          
    }
}
