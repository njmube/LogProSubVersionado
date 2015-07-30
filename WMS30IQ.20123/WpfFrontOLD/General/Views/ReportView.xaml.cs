using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.Windows.Forms.Integration;
using System.IO;
using System.Reflection;
using System.Xml;
//using DataDynamics.Analysis;
//using DataDynamics.Analysis.Layout;
using WpfFront.Common;
using WpfFront.Presenters;
using Microsoft.Practices.Unity;



namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class ReportView : UserControlBase, IReportView
    {

        private System.Windows.Forms.WebBrowser pivotView = new System.Windows.Forms.WebBrowser();


        public ReportView()
        {
            InitializeComponent();


            #region Windows Form Host

            //Create a Windows Forms Host to host a form
            WindowsFormsHost host = new WindowsFormsHost();

            //Report ddimensions
            host.HorizontalAlignment = HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;


            //Add the component to the host
            host.Child = pivotView;
            gridP.Children.Add(host);

            pivotView.Width = (int)SystemParameters.FullPrimaryScreenWidth - 40;
            pivotView.Height = (int)SystemParameters.FullPrimaryScreenHeight - 20;


            #endregion

        }


        public ReportModel Model
        {
            get
            { return this.DataContext as ReportModel; }
            set
            { this.DataContext = value; }

        }

        //Properties
        public System.Windows.Forms.WebBrowser ReportContainer
        {
            get { return this.pivotView; }
            set { this.pivotView = value; }
        }




    }


    public interface IReportView
    {
        System.Windows.Forms.WebBrowser ReportContainer { get; set; }
        ReportModel Model { get; set; }
    }



}