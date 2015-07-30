using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using System.Linq;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Windows;
using System.Data;

namespace WpfFront.Models
{

    public interface IOrderMngrV2Model 
    {
        IList<Account> CustomerList { get; set; }
        IList<DocumentLine> CurrentDetails { get; set; }
        IList<DocumentLine> SelectedLines { get; set; }
        Account Customer { get; set; }
        IList<ProductStock> DocumentProductStock { get; set; }
        IList<ProductInventory> StockInUse { get; set; }
        
        IList<Product> ItemList { get; set; }
        IList<Document> DocAdminList { get; set; }

        IList<DocumentLine> OrderDetails { get; set; }

        DataTable VendorDetails { get; set; }

    }



    public class OrderMngrV2Model : BusinessEntityBase, IOrderMngrV2Model
    {

        private DataTable _VendorDetails;
        public DataTable VendorDetails
        {
            get
            { return _VendorDetails; }
            set
            {
                _VendorDetails = value;
                OnPropertyChanged("VendorDetails");
            }
        }

        
        private IList<Product> _ItemList;
        public IList<Product> ItemList
        {
            get
            { return _ItemList; }
            set
            {
                _ItemList = value;
                OnPropertyChanged("ItemList");
            }
        }





        private IList<Document> _DocAdminList;
        public IList<Document> DocAdminList
        {
            get
            { return _DocAdminList; }
            set
            {
                _DocAdminList = value;
                OnPropertyChanged("DocAdminList");
            }
        }



        private Account _Customer;
        public Account Customer
        {
            get
            { return _Customer; }
            set
            {
                _Customer = value;
                OnPropertyChanged("Customer");
            }
        }


        private IList<ProductInventory> _StockInUse;
        public IList<ProductInventory> StockInUse
        {
            get
            { return _StockInUse; }
            set
            {
                _StockInUse = value;
                OnPropertyChanged("StockInUse");
            }
        }



        private IList<ShowData> _Voyages;
        public IList<ShowData> Voyages
        {
            get
            { return _Voyages; }
            set
            {
                _Voyages = value;
                OnPropertyChanged("Voyages");
            }
        }





        private IList<ProductStock> _DocumentProductStock;
        public IList<ProductStock> DocumentProductStock
        {
            get
            { return _DocumentProductStock; }
            set
            {
                _DocumentProductStock = value;
                OnPropertyChanged("DocumentProductStock");
            }
        }



        private IList<DocumentLine> _sl;
        public IList<DocumentLine> SelectedLines
        {
            get
            { return _sl; }
            set
            {
                _sl = value;
                OnPropertyChanged("SelectedLines");
            }
        }



        private IList<DocumentLine> _SelectedDocs;
        public IList<DocumentLine> CurrentDetails
        {
            get
            { return _SelectedDocs; }
            set
            {
                _SelectedDocs = value;
                OnPropertyChanged("CurrentDetails");
            }
        }



        private IList<Account> _CustomerList;
        public IList<Account> CustomerList
        {
            get
            {
                return _CustomerList;
            }
            set
            {
                _CustomerList = value;
                OnPropertyChanged("CustomerList");
            }
        }


        private IList<DocumentLine> _OrderDetails;
        public IList<DocumentLine> OrderDetails
        {
            get
            { return _OrderDetails; }
            set
            {
                _OrderDetails = value;
                OnPropertyChanged("OrderDetails");
            }
        }
    }
}