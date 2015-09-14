using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IInventoryCountModel
    {
        IList<Bin> AvailableBin { get; set; }
        IList<BinByTask> AssignBin { get; set; }
        Document Document { get; set; }
        IList<Document> DocumentList { get; set; }
        IList<BinByTaskExecution> CountExecution { get; set; }
        IList<Status> DocStatus { get; } //set; }
        IList<CountTaskBalance> CountSummary { get; set; }
        IList<ProductCategory> ProductCategories { get; set; }
        IList<CountTaskBalance> CountSummaryX { get; set; }

        IList<Product> AvailableProduct { get; set; }

    }


    public class InventoryCountModel: BusinessEntityBase, IInventoryCountModel
    {

        public int ActualTab=-1;

        private IList<ProductCategory> _ProductCategories;
        public IList<ProductCategory> ProductCategories
        {
            get
            { return _ProductCategories; }
            set
            {
                _ProductCategories = value;
                OnPropertyChanged("ProductCategories");
            }
        }


        private IList<CountTaskBalance> _CountSummary;
        public IList<CountTaskBalance> CountSummary
        {
            get
            { return _CountSummary; }
            set
            {
                _CountSummary = value;
                OnPropertyChanged("CountSummary");
            }
        }

        private IList<CountTaskBalance> _CountSummaryX;
        public IList<CountTaskBalance> CountSummaryX
        {
            get
            { return _CountSummaryX; }
            set
            {
                _CountSummaryX = value;
                OnPropertyChanged("CountSummaryX");
            }
        }



        private IList<BinByTaskExecution> _CountExecution;
        public IList<BinByTaskExecution> CountExecution
        {
            get
            { return _CountExecution; }
            set
            {
                _CountExecution = value;
                OnPropertyChanged("CountExecution");
            }
        }


        private Document _TaskDocument;
        public Document Document
        {
            get
            { return _TaskDocument; }
            set
            {
                _TaskDocument = value;
                OnPropertyChanged("Document");
            }
        }


        private IList<Document> _DocumentList;
        public IList<Document> DocumentList
        {
            get
            { return _DocumentList; }
            set
            {
                _DocumentList = value;
                OnPropertyChanged("DocumentList");
            }
        }


        private IList<Bin> _AvailableBin;
        public IList<Bin> AvailableBin
        {
            get
            { return _AvailableBin; }
            set
            {
                _AvailableBin = value;
                OnPropertyChanged("AvailableBin");
            }
        }


        private IList<Product> _AvailableProduct;
        public IList<Product> AvailableProduct
        {
            get
            { return _AvailableProduct; }
            set
            {
                _AvailableProduct = value;
                OnPropertyChanged("AvailableProduct");
            }
        }




        private IList<BinByTask> _AssignBin;
        public IList<BinByTask> AssignBin
        {
            get
            { return _AssignBin; }
            set
            {
                _AssignBin = value;
                OnPropertyChanged("AssignBin");
            }
        }


        //private IList<Status> docstatus;
        public IList<Status> DocStatus
        {
            get
            { return App.DocStatusList; }
            //set
            //{
            //    docstatus = value;
            //    OnPropertyChanged("DocStatus");
            //}
        }



        private IList<ProductStock> _CountSummaryN;
        public IList<ProductStock> NoCountSummary
        {
            get
            { return _CountSummaryN; }
            set
            {
                _CountSummaryN = value;
                OnPropertyChanged("NoCountSummary");
            }
        }
        

    }
}
