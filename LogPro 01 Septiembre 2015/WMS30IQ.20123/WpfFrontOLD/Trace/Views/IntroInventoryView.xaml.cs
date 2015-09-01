using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.Windows.Forms.Integration;
using DataDynamics.Analysis.Windows.Forms;
using DataDynamics.Analysis.Windows.Forms.DataSources;
using System.IO;
using System.Reflection;
using System.Xml;
using DataDynamics.Analysis;
using DataDynamics.Analysis.Layout;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class IntroInventoryView : UserControlBase, IIntroInventoryView
    {

        private readonly PivotView pivotView = new PivotView();

        public IntroInventoryView()
        {
            InitializeComponent();
        }


        public IntroInventoryModel Model
        {
            get
            { return this.DataContext as IntroInventoryModel; }
            set
            { this.DataContext = value; }

        }


        private void Intro_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Util.GetConfigOption("CNNSTR")))
            {
                Util.ShowMessage("No database is configured.");
                return;
            }

            LoadPivot();
        }


        private void LoadPivot()
        {
            try
            {
                //Create a Windows Forms Host to host a form
                WindowsFormsHost host = new WindowsFormsHost();

                host.HorizontalAlignment = HorizontalAlignment.Stretch;
                host.VerticalAlignment = VerticalAlignment.Stretch;


                pivotView.Width = 1000;
                pivotView.Height = 570;
                pivotView.AppearanceSettings.SchemaVisible = true;
                pivotView.AppearanceSettings.ExpandGlyphs = ExpandGlyphsStyle.On;
                pivotView.AutoRefreshGrid = true;
                pivotView.CardLayout.DisplayPanels = false;


                RdDataSource dataSource = new RdDataSource();
                pivotView.DataSource = dataSource;

                string qString = Util.GetConfigOption("CNNSTR");
                dataSource.ConnectionString = qString;
                //dataSource.ConnectionString = "Data Source=CRMES;Initial Catalog=WMS30;User Id=sa;Password=2289";

                dataSource.ConnectionType = DataDynamics.Analysis.DataSources.ConnectionType.Sql;
                dataSource.CustomSchemaFile = "inventory.schema";
                dataSource.QueryString = "SELECT * FROM dbo.vwCurrentInventory";
                dataSource.Connect();
                LoadPivotLayout();

                host.Child = pivotView;
                gridP.Children.Add(host);


                pivotView.Run();
            }
            catch (Exception ex)
            {
                Util.ShowMessage("Problem loading the cube. " + ex.Message);
                return;
            }

        }


        private void LoadPivotLayout() {

            string filename = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "InventorySettings.xml");

            if (!File.Exists(filename))
                return;
            using (XmlReader reader = XmlReader.Create(filename))
            {
                pivotView.Read(reader, PersistSettings.Layout | PersistSettings.CardLayout);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filename = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "InventorySettings.xml");

            using (XmlWriter writer = XmlWriter.Create(filename))
            {
                pivotView.Write(writer, PersistSettings.CardLayout | PersistSettings.Layout | PersistSettings.Permissions);
            }
        }


    }



    public interface IIntroInventoryView
    {
        //Clase Modelo
        IntroInventoryModel Model { get; set; }

    }
}