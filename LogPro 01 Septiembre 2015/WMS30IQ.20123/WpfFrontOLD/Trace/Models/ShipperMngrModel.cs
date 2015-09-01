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

    public interface IShipperMngrModel 
    {
        IList<ShowData> CustomerList { get; set; }
        DataTable OrdersData { get; set; }
        IList<DocumentLine> CurrentDetails { get; set; }
        IList<DocumentLine> OrdersDetail { get; set; }
        IList<AccountAddress> CustomerAddress { get; set; }
        IList<DocumentLine> SelectedLines { get; set; }
        IList<SysUser> Pickers { get; set; }
        Account Customer { get; set; }
        IList<ProductStock> DocumentProductStock { get; set; }
        IList<ProductInventory> StockInUse { get; set; }
        String DocNumber { get; set; }
        int DocID { get; set; }

        IList<ShowData> Voyages { get; set; }
        IList<ShowData> Exit_IPS { get; set; }
        //IList<ShowData> Ports { get; set; }

    }



    public class ShipperMngrModel : BusinessEntityBase, IShipperMngrModel
    {


        private String _DocNumber;
        public String DocNumber
        {
            get
            { return _DocNumber; }
            set
            {
                _DocNumber = value;
                OnPropertyChanged("DocNumber");
            }
        }

        private int _DocID;
        public int DocID
        {
            get
            { return _DocID; }
            set
            {
                _DocID = value;
                OnPropertyChanged("DocID");
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



        //private IList<ShowData> _Ports;
        //public IList<ShowData> Ports
        //{
        //    get
        //    { return _Ports; }
        //    set
        //    {
        //        _Ports = value;
        //        OnPropertyChanged("Ports");
        //    }
        //}



        private IList<ShowData> _Exit_IPS;
        public IList<ShowData> Exit_IPS
        {
            get
            { return _Exit_IPS; }
            set
            {
                _Exit_IPS = value;
                OnPropertyChanged("Exit_IPS");
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



        private IList<AccountAddress> _CustomerAddress;
        public IList<AccountAddress> CustomerAddress
        {
            get
            { return _CustomerAddress; }
            set
            {
                _CustomerAddress = value;
                OnPropertyChanged("CustomerAddress");
            }
        }

        private DataTable _OrdersData;
        public DataTable OrdersData
        {
            get
            { return _OrdersData; }
            set
            {
                _OrdersData = value;
                OnPropertyChanged("OrdersData");
            }
        }


        private IList<DocumentLine> _OrdersDetail;
        public IList<DocumentLine> OrdersDetail
        {
            get
            { return _OrdersDetail; }
            set
            {
                _OrdersDetail = value;
                OnPropertyChanged("OrdersDetail");
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


        private IList<SysUser> _Pikers;
        public IList<SysUser> Pickers
        {
            get
            { return _Pikers; }
            set
            {
                _Pikers = value;
                OnPropertyChanged("Pickers");
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



        private IList<ShowData> _CustomerList;
        public IList<ShowData> CustomerList
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
    }
}