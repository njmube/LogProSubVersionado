using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.Windows.Forms.Integration;
//using DataDynamics.Analysis.Windows.Forms;
//using DataDynamics.Analysis.Windows.Forms.DataSources;
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
    public partial class BasicSetupView : UserControlBase, IBasicSetupView
    {

        public event EventHandler<DataEventArgs<int>> LoadView;


        public BasicSetupModel Model
        {
            get
            { return this.DataContext as BasicSetupModel; }
            set
            { this.DataContext = value; }

        }

        //Properties
        public ItemsControl UCInfo
        {
            get { return this.ucInfo; }
            set { this.ucInfo = value; }
        }


        public BasicSetupView()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            LoadView(sender, new DataEventArgs<int>(int.Parse(btn.CommandParameter.ToString())));

        }

        private void Intro_Loaded(object sender, RoutedEventArgs e)
        {
            LoadView(sender, new DataEventArgs<int>(1));
        }

    }


    public interface IBasicSetupView
    {
        ItemsControl UCInfo { get; set; }
        BasicSetupModel Model { get; set; }

        event EventHandler<DataEventArgs<int>> LoadView;
    }

}