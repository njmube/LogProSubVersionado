using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for C_CasNumberView.xaml
    /// </summary>
    public partial class C_CasNumberView : UserControlBase, IC_CasNumberView
    {
        public C_CasNumberView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<C_CasNumber>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

         public C_CasNumberModel Model
        {
            get
            { return this.DataContext as C_CasNumberModel; }
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


        public TabControl TabMenu
        { get { return this.tabMenu; }
            set { this.tabMenu = value; } 
        }


        public CasNumberRule UC_CasNumRule
        {
            get { return this.ucCasNumberRule; }
            set { this.ucCasNumberRule = value; }
        }

        public TabItem TabRules
        {
            get { return this.tbIRules; }
            set { this.tbIRules = value; }
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
            LoadData(sender, new DataEventArgs<C_CasNumber>((C_CasNumber)dgList.SelectedItem));

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

    }



    public interface IC_CasNumberView
    {
        //Clase Modelo
        C_CasNumberModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        TabControl TabMenu { get; set;}
        CasNumberRule UC_CasNumRule { get; set; }
        TabItem TabRules { get; set; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<C_CasNumber>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
    }
}