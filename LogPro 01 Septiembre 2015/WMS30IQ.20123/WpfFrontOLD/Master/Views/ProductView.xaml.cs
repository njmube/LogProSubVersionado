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
    /// Interaction logic for ProductView.xaml
    /// </summary>
    public partial class ProductView : UserControlBase, IProductView
    {
        public ProductView()
        {
            InitializeComponent();
            //txtAssigBin.imgLoad.Visibility = Visibility.Collapsed;
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<Product>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;
        public event EventHandler<DataEventArgs<Bin>> AssignBinToProduct;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<EventArgs> AddProductTrackOption;
        public event EventHandler<EventArgs> AddProductUnit;
        public event EventHandler<EventArgs> LoadUnitsFromGroup;
        //public event EventHandler<DataEventArgs<string>> LoadBins;
        public event EventHandler<DataEventArgs<object>> UnSetRequired;
        public event EventHandler<DataEventArgs<object>> SetRequired;
        public event EventHandler<DataEventArgs<ZoneBinRelation>> UpdateBinToProduct;
        public event EventHandler<EventArgs> AddAlternateProduct;
        public event EventHandler<DataEventArgs<ProductAccountRelation>> AddProductAccount;
        public event EventHandler<DataEventArgs<object>> SetIsMain;
        public event EventHandler<DataEventArgs<object>> UnSetIsMain;
        public event EventHandler<DataEventArgs<object>> UpdateProductAccount;



         public ProductModel Model
        {
            get
            { return this.DataContext as ProductModel; }
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

        public ListView LvStock
        {
            get { return this.lvBinStock; }
            set { this.lvBinStock = value; }
        }

        public StackPanel StkLocations
        {
            get { return this.stkLocations; }
            set { this.stkLocations = value; }
        }

        public ListView LvAvailableTrack
        {
            get { return this.lvDenyTrack; }
            set { this.lvDenyTrack = value; }
        }

        public ListView LvAssignedTrack
        {
            get { return this.lvAllowTrack; }
            set { this.lvAllowTrack = value; }
        }

        public ListView LvAvailableUnit
        {
            get { return this.lvAvailableUnits; }
            set { this.lvAvailableUnits = value; }
        }

        public ListView LvAssignedUnit
        {
            get { return this.lvAssignedUnits; }
            set { this.lvAssignedUnits = value; }
        }

        public Grid DgUnits
        {
            get { return this.dgUnits; }
            set { this.dgUnits = value; }
        }

        public ComboBox CboUnitGroup {             
            get { return this.cboUnitGroup; }
            set { this.cboUnitGroup = value; }
        }

        public ComboBox CboUnidadBase
        {
            get { return this.UnidadBase; }
            set { this.UnidadBase = value; }
        }

        //public ComboBox CboBinList
        //{
        //    get { return this.cboBin; }
        //    set { this.cboBin = value; }
        //}

        public BinLocation TxtAssigBin
        {
            get { return this.txtAssigBin; }
            set { this.txtAssigBin = value; }
        }

        public ComboBox CboPickMethod
        {
            get { return this.cboPickM; }
            set { this.cboPickM = value; }
        }

        public ComboBox CboBinDirection
        {
            get { return this.cboBinDirection; }
            set { this.cboBinDirection = value; }
        }

        public TextBox MinStock
        {
            get { return this.txtMinStock; }
            set { this.txtMinStock = value; }
        }

        public TextBox MaxStock
        {
            get { return this.txtMaxStock; }
            set { this.txtMaxStock = value; }
        }

        public ListView LvAlternProducts
        {
            get { return this.lvAlternateProducts; }
            set { this.lvAlternateProducts = value; }
        }

        public SearchProduct TxtAltProduct
        {
            get { return this.txtAltProduct; }
        }

        public StackPanel StkAltern
        {
            get { return this.stkAltern; }
        }

        public TabItem TbItmCasN
        {
            get { return this.tbiCasN; }
            set { this.tbiCasN = value; }
        }

        //Product Account Vendor

        public ListView LvVendorProducts
        {
            get { return this.lvVendorProduct; }
            set { this.lvVendorProduct = value; }
        }


        public SearchAccount TxtVendor
        {
            get { return this.txtVendor; }
            set { this.txtVendor = value; }
        }

        public TextBox TxtVendCode1
        {
            get { return this.txtVendorCode1; }
            set { this.txtVendorCode1 = value; }
        }

        public TextBox TxtVendCode2
        {
            get { return this.txtVendorCode2; }
            set { this.txtVendorCode2 = value; }
        }

        public TextBox TxtVendItem
        {
            get { return this.txtVendorItem; }
            set { this.txtVendorItem = value; }
        }


        //Product Account Customer

        public ListView LvCustProducts
        {
            get { return this.lvCustProduct; }
            set { this.lvCustProduct = value; }
        }


        public SearchAccount TxtCustomer
        {
            get { return this.txtCust; }
            set { this.txtCust = value; }
        }

        public TextBox TxtCustCode1
        {
            get { return this.txtCustCode1; }
            set { this.txtCustCode1 = value; }
        }

        public TextBox TxtCustCode2
        {
            get { return this.txtCustCode2; }
            set { this.txtCustCode2 = value; }
        }


        public CheckBox ChkIsMain
        {
            get { return this.chkMain; }
            set { this.chkMain = value; }
        }

        public TextBox TxtCustItem
        {
            get { return this.txtCustItem; }
            set { this.txtCustItem = value; }
        }


        public ProcessFile UcProcessFile
        {
            get { return this.ucProcessFile; }
            set { this.ucProcessFile = value; }
        }

        /*[Jorge Armando Ortega (Octubre 21/2010)]*/
        public CasNumberFormula UcCasNumberFormula
        {
            get { return this.ucCasNumberFormula; }
            set { this.ucCasNumberFormula = value; }
        }
        /****/

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
            LoadData(sender, new DataEventArgs<Product>((Product)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        private void btnAssignBin_Click(object sender, RoutedEventArgs e)
        {
            AssignBinToProduct(sender, new DataEventArgs<Bin>(txtAssigBin.Bin));

            txtAssigBin.Text = txtMaxStock.Text = txtMinStock.Text = "";            
            cboBinDirection.SelectedIndex = -1;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }


        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
            {
                this.lvBinStock.SelectAll();
                this.lvBinStock.Focus();
            }

            if (((CheckBox)sender).Name == chkSelectAlternate.Name)
            {
                this.lvAlternateProducts.SelectAll();
                this.lvAlternateProducts.Focus();
            }

            if (((CheckBox)sender).Name == chkSelectAllVendors.Name)
            {
                this.lvVendorProduct.SelectAll();
                this.lvVendorProduct.Focus();
            }

            if (((CheckBox)sender).Name == chkSelectAllCustomers.Name)
            {
                this.lvCustProduct.SelectAll();
                this.lvCustProduct.Focus();
            }

        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

            if (((CheckBox)sender).Name == chkSelectAllLines.Name)
                this.lvBinStock.UnselectAll();

            if (((CheckBox)sender).Name == chkSelectAlternate.Name)
                this.lvAlternateProducts.UnselectAll();

            if (((CheckBox)sender).Name == chkSelectAllVendors.Name)
                this.lvVendorProduct.UnselectAll();

            if (((CheckBox)sender).Name == chkSelectAllCustomers.Name)
                this.lvCustProduct.UnselectAll();


        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddProductTrackOption(this, e);
        }



        private void btnAddUnit_Click(object sender, RoutedEventArgs e)
        {
            AddProductUnit(this, e);
        }


        private void cboUnitGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUnitsFromGroup(this, e);
        }


        //private void cboBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Bin bin = ((ComboBox)sender).SelectedItem as Bin;

        //    if (bin == null)
        //        return;

        //    txtAssigBin.Text = bin.BinCode;
        //    cboBin.Visibility = Visibility.Collapsed;
        //    txtAssigBin.Focus();

        //}

        //private void txtBinLabel_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //Search for a Bin
        //    LoadBins(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        //}

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetRequired(sender, new DataEventArgs<object>(((CheckBox)sender).CommandParameter));
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UnSetRequired(sender, new DataEventArgs<object>(((CheckBox)sender).CommandParameter));
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateBinToProduct(sender, new DataEventArgs<ZoneBinRelation>((ZoneBinRelation)lvBinStock.SelectedItem));
            lvBinStock.SelectedItem = null;

        }

        private void btnAddAltern_Click(object sender, RoutedEventArgs e)
        {
            AddAlternateProduct(sender, e);
        }

        private void btnAddVendor_Click(object sender, RoutedEventArgs e)
        {
            //if (this.txtVendor.Account == null)
                //return;

            //if (string.IsNullOrEmpty(txtVendorItem.Text))
                //return;

            ProductAccountRelation pa = new ProductAccountRelation
            {
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                Account = this.txtVendor.Account,
                AccountType = new AccountType { AccountTypeID = AccType.Vendor },
                IsFromErp = false,
                ItemNumber = this.txtVendorItem.Text,
                Product = this.Model.Record,
                Code1 = this.txtVendorCode1.Text,
                Code2 = this.txtVendorCode2.Text,
                IsDefault = this.chkMain.IsChecked
            };


            AddProductAccount(sender, new DataEventArgs<ProductAccountRelation>(pa));
        }

        private void btnAddCust_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtCust.Account == null)
                return;

            if (string.IsNullOrEmpty(txtCustItem.Text))
                return;

            ProductAccountRelation pa = new ProductAccountRelation
            {
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                Account = this.txtCust.Account,
                AccountType = new AccountType { AccountTypeID = AccType.Customer },
                IsFromErp = false,
                ItemNumber = this.txtCustItem.Text,
                Product = this.Model.Record,
                Code1 = this.txtCustCode1.Text,
                Code2 = this.txtCustCode2.Text
            };

            AddProductAccount(sender, new DataEventArgs<ProductAccountRelation>(pa));
        }

        private void chkRestrict_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void chkMain_Checked(object sender, RoutedEventArgs e)
        {
            SetIsMain(sender, new DataEventArgs<object>(((CheckBox)sender).CommandParameter));
        }

        private void chkMain_Unchecked(object sender, RoutedEventArgs e)
        {
            UnSetIsMain(sender, new DataEventArgs<object>(((CheckBox)sender).CommandParameter));
        }

        private void itmN_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateProductAccount(sender, new DataEventArgs<object>(((TextBox)sender).Tag));
        }

        #endregion

    }

    public interface IProductView
    {
        //Clase Modelo
        ProductModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        ListView LvStock { get; set; }
        StackPanel StkLocations { get; set; }
        ListView LvAvailableTrack { get; set; }
        ListView LvAssignedTrack { get; set; }
        ListView LvAvailableUnit { get; set; }
        ListView LvAssignedUnit { get; set; }
        Grid DgUnits { get; set; }
        ComboBox CboUnitGroup { get; set; }
        ComboBox CboUnidadBase { get; set; }
        //ComboBox CboBinList { get; set; }
        BinLocation TxtAssigBin { get; set; }
        ComboBox CboPickMethod { get; set; }
        ComboBox CboBinDirection { get; set; }
        TextBox MaxStock { get; set; }
        TextBox MinStock { get; set; }
        ListView LvAlternProducts { get; set; }
        SearchProduct TxtAltProduct { get; }
        StackPanel StkAltern { get; }
        ListView LvVendorProducts { get; set; }

        SearchAccount TxtVendor{ get; set; }
         TextBox TxtVendCode1{ get; set; }
         TextBox TxtVendCode2{ get; set; }
         TextBox TxtVendItem{ get; set; }

         ListView LvCustProducts { get; set; }
         SearchAccount TxtCustomer { get; set; }
         TextBox TxtCustCode1 { get; set; }
         TextBox TxtCustCode2 { get; set; }
         TextBox TxtCustItem { get; set; }
         ProcessFile UcProcessFile { get; set; }
         CheckBox ChkIsMain { get; set; }


         CasNumberFormula UcCasNumberFormula { get; set; }
         TabItem TbItmCasN { get; set; }
        



        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<Product>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
        event EventHandler<DataEventArgs<Bin>> AssignBinToProduct;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<EventArgs> AddProductTrackOption;
        event EventHandler<EventArgs> AddProductUnit;
        event EventHandler<EventArgs> LoadUnitsFromGroup;
        //event EventHandler<DataEventArgs<string>> LoadBins;
        event EventHandler<DataEventArgs<object>> UnSetRequired;
        event EventHandler<DataEventArgs<object>> SetRequired;
        event EventHandler<DataEventArgs<ZoneBinRelation>> UpdateBinToProduct;
        event EventHandler<EventArgs> AddAlternateProduct;
        event EventHandler<DataEventArgs<ProductAccountRelation>> AddProductAccount;
        event EventHandler<DataEventArgs<object>> SetIsMain;
        event EventHandler<DataEventArgs<object>> UnSetIsMain;
        event EventHandler<DataEventArgs<object>> UpdateProductAccount;

    }
}