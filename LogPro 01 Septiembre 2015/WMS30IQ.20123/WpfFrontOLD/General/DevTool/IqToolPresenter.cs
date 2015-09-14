using System;
using System.Collections.Generic;
using System.Data;
using Assergs.Windows;
using Microsoft.Practices.Unity; 
using WMComposite.Events;
using WpfFront.Services;
using WpfFront.Views;
using WpfFront.Models;
using WpfFront.WMSBusinessService;
using System.Linq;
using Xceed.Wpf.DataGrid;
using System.Text;
using Xceed.Wpf.DataGrid.Views;
using WpfFront.Common;
using Xceed.Wpf.DataGrid.Settings;
using System.Windows;


namespace WpfFront.Presenters
{
    public class IqToolPresenter : IIqToolPresenter
    {
        private readonly IUnityContainer container;

        private readonly WMSServiceClient service;

        public IIqToolView View { get; set; }

        private ToolWindow window;
        public ToolWindow Window
        {
            get
            {
                return window;
            }
            set
            {
                //if (value != null)
                //    value.IsMaximized = true;
                window = value;
            }
        }


        public IqToolPresenter(IUnityContainer container, IIqToolView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<IqToolModel>();

            View.SelectionData += new EventHandler<DataEventArgs<IqReportTable>>(OnSelectionChanged);

            View.RemoveFromSelected += new EventHandler<DataEventArgs<IqColumn>>(View_RemoveFromSelected);
            View.AddToSelected += new EventHandler<DataEventArgs<IqColumn>>(View_AddToSelected);
            View.UpdateReport += new EventHandler<EventArgs>(View_UpdateReport);


            ProcessWindow pw = new ProcessWindow("Loading Tool ...");

            View.Model.CheckRules();
            Initialize();

            pw.Close();
        }



        void View_UpdateReport(object sender, EventArgs e)
        {
            try { service.UpdateIqReportTable(View.Model.CurTable); }
            catch (Exception ex) { Util.ShowError(ex.Message); }
        }



        //void View_UpdateFilter(object sender, DataEventArgs<IqReportColumn> e)
        //{
        //    View.Model.AllColumns.Where(f => f.ReportColumnId == e.Value.ReportColumnId).First().Options = e.Value.Options;
        //    View.Model.AllColumns = View.Model.AllColumns;
        //}



        void View_AddToSelected(object sender, DataEventArgs<IqColumn> e)
        {
            if (e.Value == null)
                return;

            try
            {
                IqReportColumn rc = new IqReportColumn { 
                    Column = e.Value,
                    Alias = e.Value.Name,
                    CreatedBy = "system",
                    CreationDate = DateTime.Now,
                    ReportTable = View.Model.CurTable                                    
                };

                service.SaveIqReportColumn(rc);
                View.Model.CurTable.ReportColumns = service.GetIqReportColumn(
                    new IqReportColumn
                    {
                        ReportTable = new IqReportTable
                        {
                            ReportTableId = View.Model.CurTable.ReportTableId
                        }
                    }).ToList();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error processing record.\n" + ex.Message);
            }
        }




        void View_RemoveFromSelected(object sender, DataEventArgs<IqColumn> e)
        {
            if (e.Value == null)
                return;

            try
            {
                //e.Value.IsSelected = false;
                //service.DeleteIqReportColumn(e.Value);
            }
            catch (Exception ex)
            {
                Util.ShowError("Error processing record.\n" + ex.Message);
            }
        }


        void View_UpdateData(object sender, DataEventArgs<IqReport> e)
        {
            if (e.Value == null)
                return;

            try
            {
                IqReport data = e.Value;
                data.ModifiedBy = App.curUser.UserName;
                data.ModDate = DateTime.Now;
                service.UpdateIqReport(data);
                Util.ShowMessage("Report Updated.");

            }
            catch (Exception ex)
            {
                Util.ShowError("Error saving the reports.\n" + ex.Message);
            }
        }


        public void Initialize()
        {
            View.Model.ListReportSystems = service.GetIqReport(new IqReport());
       }


        private void OnSave(object sender, DataEventArgs<IqReport> e)
        {
            if (e.Value == null)
                return;


            try
            {
                IqReport data = e.Value;
                data.CreatedBy = App.curUser.UserName;
                data.CreationDate = DateTime.Now;
                data.IsForSystem = false;
                View.Model.ReportSystem = service.SaveIqReport(data);
                View.Model.ListReportSystems = service.GetIqReport(new IqReport());
                Util.ShowMessage("Report Saved.");

            }
            catch (Exception ex)
            {
                Util.ShowError("Error saving the reports.\n" + ex.Message);
            }

        }





        private void OnSelectionChanged(object sender, DataEventArgs<IqReportTable> e)
        {

            if (e.Value == null)
                return;

            IList<IqReportColumn> columnList = new List<IqReportColumn>();
            //foreach (IqReportTable rt in e.Value.ReportTables)
                foreach (IqReportColumn rc in e.Value.ReportColumns)
                    columnList.Add(rc);

                View.Model.AllColumns = service.GetIqColumn(new IqColumn { Table = e.Value.Table });

        }




    }

    public interface IIqToolPresenter
    {
        IIqToolView View { get; set; }

        ToolWindow Window { get; set; }

        void Initialize();
    }




}