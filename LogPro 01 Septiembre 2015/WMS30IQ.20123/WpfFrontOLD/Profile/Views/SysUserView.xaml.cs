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
    /// Interaction logic for SysUserView.xaml
    /// </summary>
    public partial class SysUserView : UserControlBase, ISysUserView
    {
        public SysUserView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        //public event EventHandler<DataEventArgs<SysUser>> LoadData;
        public event EventHandler<EventArgs> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<EventArgs> AddRol;


         public SysUserModel Model
        {
            get
            { return this.DataContext as SysUserModel; }
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


        public ComboBox CboLocation
        { get { return this.cboLocation; } }

        public ComboBox CboRol
        { get { return this.cboRol; } }


        public ListView LvRol
        {
            get { return this.lvRol; }
            set { this.lvRol = value; }
        }

        public TabItem TbRol
        {
            get { return this.tbRol; }
            set { this.tbRol = value; }
        }


        public TabControl TbUser
        {
            get { return this.tbUser; }
            set { this.tbUser = value; }
        }


        public StackPanel StkRolForm
        {
            get { return this.stkRolForm; }
            set { this.stkRolForm = value; }
        }

        //public PasswordBox TxtPwd
        //{
        //    get { return this.txtPwd; }
        //    set { this.txtPwd = value; }
        //}

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
            //LoadData(sender, new DataEventArgs<SysUser>((SysUser)dgList.SelectedItem));
            LoadData(sender, e);

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!UtilWindow.ConfirmOK("Really wish to delete the User and its permits?") == true)
                return;

            Delete(sender, e);
        }


        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            {
                this.lvRol.SelectAll();
                this.lvRol.Focus();
            }


        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
                this.lvRol.UnselectAll();

        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }


        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRol(sender, e);
        }






    }



    public interface ISysUserView
    {
        //Clase Modelo
        SysUserModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        ComboBox CboRol { get; }
        ComboBox CboLocation { get; }
        ListView LvRol { get; set; }
        TabItem TbRol { get; set; }
        TabControl TbUser { get; set; }
        StackPanel StkRolForm { get; set; }
        //PasswordBox TxtPwd { get; set; }

        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        //event EventHandler<DataEventArgs<SysUser>> LoadData;
        event EventHandler<EventArgs> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<EventArgs> AddRol;
    }
}