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
using WpfFront.Presenters;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class InventoryTaskView : UserControlBase, IInventoryTaskView
    {


        public InventoryTaskView()
        {
            InitializeComponent();
        }



        //Events
        public event EventHandler<EventArgs> ShowRepAndPack;
        public event EventHandler<EventArgs> InvenroryAdj;
        public event EventHandler<EventArgs> ErpConciliation;


        public InventoryTaskModel Model
        {
            get
            { return this.DataContext as InventoryTaskModel; }
            set
            { this.DataContext = value; }

        }


        //Properties
        public ItemsControl UCInfo 
        {
            get { return this.ucInfo; }
            set { this.ucInfo = value; }
        }

        public TextBlock TxtTitle
        {
            get { return this.txtTitle; }
            set { this.txtTitle = value; }
        }


        private void btnRepPack_Click(object sender, RoutedEventArgs e)
        {
            //Replanishment & Packing
            ShowRepAndPack(sender, e);
        }

        private void btnIA_Click(object sender, RoutedEventArgs e)
        {
            InvenroryAdj(sender, e);
        }

        private void btnConcil_Click(object sender, RoutedEventArgs e)
        {
            ErpConciliation(sender, e);
        }



    }



    public interface IInventoryTaskView
    {
        //Clase Modelo
        InventoryTaskModel Model { get; set; }

        ItemsControl UCInfo { get; set; }
        TextBlock TxtTitle { get; set; }


        event EventHandler<EventArgs> ShowRepAndPack;
        event EventHandler<EventArgs> InvenroryAdj;
        event EventHandler<EventArgs> ErpConciliation;
    }
}