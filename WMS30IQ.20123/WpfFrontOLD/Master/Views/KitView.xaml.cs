using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common.UserControls;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for KitView.xaml
    /// </summary>
    public partial class KitView : UserControlBase, IKitView
    {
        public KitView()
        {
            InitializeComponent();
        }

        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<KitAssembly>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> SaveComponent;
        public event EventHandler<EventArgs> RemoveComponent;

        #region Properties

        public KitModel Model
        {
            get { return this.DataContext as KitModel; }
            set { this.DataContext = value; }
        }

        public DataGridControl ListRecords
        { get { return this.dgList; } }

        public StackPanel StkNewKit
        {
            get { return this.stkNewKit; }
            set { this.stkNewKit = value; }
        }

        public StackPanel StkEdit
        {
            get { return this.stkEdit; }
            set { this.stkEdit = value; }
        }

        public Border BrdComponent
        {
            get { return this.brdComponent; }
            set { this.brdComponent = value; }
        }

        public Button BtnDelete
        {
            get { return this.btnDelete; }
        }

        public Button BtnRemove
        {
            get { return this.btnRemove; }
        }

        public SearchProduct TxtFatherProduct
        {
            get { return this.txtFatherProduct; }
        }

        public SearchProduct TxtComponent { get { return this.txtComponent; } }
        public TextBox TxtQty { get { return this.txtQty; } }
        public TextBox TxtOrder { get { return this.txtOrder; } }
        public ListView LvFormula { get { return this.lvFormula ; } }

        # endregion 


        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<KitAssembly>((KitAssembly)dgList.SelectedItem));
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSearch(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnSaveComponent_Click(object sender, RoutedEventArgs e)
        {
            SaveComponent(sender, e);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveComponent(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }
    }

    public interface IKitView
    {
        //Clase Modelo
        KitModel Model { get; set; }
        StackPanel StkNewKit { get; set; }
        StackPanel StkEdit { get; set; }
        Border BrdComponent { get; set; }
        DataGridControl ListRecords { get; }
        SearchProduct TxtFatherProduct { get; }
        SearchProduct TxtComponent { get; }
        TextBox TxtQty { get; }
        TextBox TxtOrder { get; }
        ListView LvFormula { get; }
        Button BtnDelete { get; }
        Button BtnRemove { get; }

        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<KitAssembly>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> SaveComponent;
        event EventHandler<EventArgs> RemoveComponent;
    }
}
