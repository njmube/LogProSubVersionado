using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IUC_IV_Model
    {
        IList<ProductStock> RepPackList { get; set; }  //lista Comun Usada para manejar los stocks
        IList<ProductStock> OriRepPackList { get; set; }
        Boolean ShowProcess { get; set; }
        IList<Location> LocationList { get; }
        Location CurLocation { get; set; }
        DocumentType DocType { get; set; }
        IList<string> BinList { get; set; }
        IList<string> ProductList { get; set; }
        IList<ShowData> SelectorList { get; }
    }

    public class UC_IV_Model: BusinessEntityBase, IUC_IV_Model
    {

        public IList<Location> LocationList { get { return App.LocationList; } }

         private IList<ShowData> _SelectorList = new List<ShowData>()
        { 
            new ShowData { DataKey = "1", DataValue = "Bin Levels"}, 
            new ShowData { DataKey = "2", DataValue = "Product/Bin Levels" }
        };

        public IList<ShowData> SelectorList
        {
            get
            { return _SelectorList; }
        }


        private IList<string> _BinList;
        public IList<string> BinList
        {
            get
            { return _BinList; }
            set
            {
                _BinList = value;
                OnPropertyChanged("BinList");
            }
        }

        private IList<string> _ProductList;
        public IList<string> ProductList
        {
            get
            { return _ProductList; }
            set
            {
                _ProductList = value;
                OnPropertyChanged("ProductList");
            }
        }

        private Location _CurLocation;
        public Location CurLocation
        {
            get
            { return _CurLocation; }
            set
            {
                _CurLocation = value;
                OnPropertyChanged("CurLocation");
            }
        }

        private IList<ProductStock> _RepPackList;
        public IList<ProductStock> RepPackList
        {
            get
            { return _RepPackList; }
            set
            {
                _RepPackList = value;
                OnPropertyChanged("RepPackList");
            }
        }


        private IList<ProductStock> _OriRepPackList;
        public IList<ProductStock> OriRepPackList
        {
            get
            { return _OriRepPackList; }
            set
            {
                _OriRepPackList = value;
                OnPropertyChanged("OriRepPackList");
            }
        }




        private Boolean _ShowProcess;
        public Boolean ShowProcess
        {
            get
            { return _ShowProcess; }
            set
            {
                _ShowProcess = value;
                OnPropertyChanged("ShowProcess");
            }
        }

        private DocumentType _DocType;
        public DocumentType DocType
        {
            get
            { return _DocType; }
            set
            {
                _DocType = value;
                OnPropertyChanged("DocType");
            }
        }

    }
}
