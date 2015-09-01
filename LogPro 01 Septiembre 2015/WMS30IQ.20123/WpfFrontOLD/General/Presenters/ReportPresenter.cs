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
using Microsoft.Reporting.WinForms;
using System.Data;
using System.IO;
using System.Reflection;
using WpfFront.Common.UserControls;

namespace WpfFront.Presenters
{

    public interface IReportPresenter
    {
       IReportView View { get; set; }
       ToolWindow Window { get; set; }
    }

    public class ReportPresenter : IReportPresenter
    {
        public IReportView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }



        public ReportPresenter(IUnityContainer container, IReportView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ReportModel>();

            //Event Delegate


            LoadUrlReport();

        }


        private void LoadUrlReport()
        {
            String rptUrl = Util.GetConfigOption("RSURL");
            View.ReportContainer.ScrollBarsEnabled = false;
            View.ReportContainer.Navigate(rptUrl);
            View.ReportContainer.Refresh();
            

        }



    }
}