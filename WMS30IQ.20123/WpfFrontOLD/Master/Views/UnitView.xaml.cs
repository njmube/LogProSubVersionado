using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for UnitView.xaml
    /// </summary>
    public partial class UnitView : UserControlBase, IUnitView
    {
        public UnitView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Unit>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> ShowOnlyGroups;

         public UnitModel Model
        {
            get
            { return this.DataContext as UnitModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

         public DataGridControl ListRecords 
            { get { return this.dgList; } }

        public StackPanel StkEdit 
            { get { return this.stkEdit; } }

        public Button BtnDelete
            { get { return this.btnDelete; } }


        public TextBox TxtUnitGroup
        { get { return this.txtUnitGroup; }
            set
            { this.txtUnitGroup = value; }
        }

        public TextBox TxtBaseAmount
        { get { return this.txtBaseAmount; }
            set
            { this.txtBaseAmount = value; }
        }

        public StackPanel StkButtons
        {
            get { return this.stkButtons; }
            set
            { this.stkButtons = value; }
        }

        public ComboBox CboUnitGroup
        {
            get { return this.cboUnitGroup; }
            set
            { this.cboUnitGroup = value; }
        }

        #endregion




        #region ViewEvents

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSearch(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
        }


        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<Unit>((Unit)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        #endregion

        private void btnAdmGroup_Click(object sender, RoutedEventArgs e)
        {
           ShowOnlyGroups(sender, e);
        }




    }



    public interface IUnitView
    {
        //Clase Modelo
        UnitModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        TextBox TxtBaseAmount { get; set; }
        TextBox TxtUnitGroup { get; set; }
        StackPanel StkButtons { get; set; }
        ComboBox CboUnitGroup { get; set; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Unit>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> ShowOnlyGroups;

    }
}