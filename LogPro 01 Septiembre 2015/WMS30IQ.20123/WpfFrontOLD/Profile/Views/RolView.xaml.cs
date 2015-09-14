using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for RolView.xaml
    /// </summary>
    public partial class RolView : UserControlBase, IRolView
    {
        public RolView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Rol>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> AddRolMenuOption;

         public RolModel Model
        {
            get
            { return this.DataContext as RolModel; }
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

        public ListView LvAvailablePermission
            {   get { return this.lvDenyPermission; }
                set { this.lvDenyPermission = value; } 
            }

        public ListView LvAssignPermission
        {
            get { return this.lvAllowPermission; }
            set { this.lvAllowPermission = value; }
        }

        
        public TabItem TbRolPerm
        {
            get { return this.tbRolPerm; }
            set { this.tbRolPerm = value; }
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
            LoadData(sender, new DataEventArgs<Rol>((Rol)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (((Button)sender).Name == "btnDelete" && !UtilWindow.ConfirmOK("Really wish to delete the Rol and its permits?") == true)
                return;

            Delete(sender, e);
        }

        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRolMenuOption(this, e);
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAllow.Name)
            {
                this.lvAllowPermission.SelectAll();
                this.lvAllowPermission.Focus();
            }
            else if (((CheckBox)sender).Name == chkSelectDeny.Name)
            {
                this.lvDenyPermission.SelectAll();
                this.lvDenyPermission.Focus();
            }

        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllow.Name)
                this.lvAllowPermission.UnselectAll();

            else if (((CheckBox)sender).Name == chkSelectDeny.Name)
                this.lvDenyPermission.UnselectAll();

        }



    }



    public interface IRolView
    {
        //Clase Modelo
        RolModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        ListView LvAvailablePermission { get; set; }
        ListView LvAssignPermission { get; set; }
        TabItem TbRolPerm { get; set; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Rol>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> AddRolMenuOption;
    }
}