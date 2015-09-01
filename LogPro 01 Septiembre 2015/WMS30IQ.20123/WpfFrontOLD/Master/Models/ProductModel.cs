using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Collections;
using System.Linq;
using System.Collections.Specialized;
using WpfFront.Common.Query;

namespace WpfFront.Models
{
    public interface IProductModel
    { 
        Product Record { get; set; }
        IList<Product> EntityList { get; set; }
        IList<ZoneBinRelation> ProductBinRelation { get; set; }
        IList<TrackOption> AvailableTrack { get; set; }
        IList<ProductTrackRelation> AllowTrack { get; set; }
        IList<Unit> AvailableUnits { get; set; }
        IList<UnitProductRelation> AssignedUnits { get; set; }
        Unit UnitGroup { get; set; }
        IList<Unit> ListUnitGroup { get; set; }
        IList<Bin> BinList { get; set; }
        IList<Status> StatusList { get; set; }
        IList<PickMethod> PickMethods { get; set; }
        Hashtable BinDirectionList { get; }
        ProductAlternate CurAltern { get; set; }
        IList<LabelTemplate> TemplateList { get; set; }
        IList<Unit> UnitList { get; set; }
        IList<ProductAccountRelation> ProductVendorRelation { get; set; }
        IList<ProductAccountRelation> ProductCustRelation { get;  }
        StringDictionary ABCRank { get; }
    }

    public class ProductModel: BusinessEntityBase, IProductModel
    {

        public StringDictionary ABCRank { get { return Operators.GetABCRank(); } }

        IList<ProductAccountRelation> _ProductCustRelation;
        public IList<ProductAccountRelation> ProductCustRelation
        {
            get {
                return _ProductCustRelation;
            }
            set
            {
                _ProductCustRelation = value;
                OnPropertyChanged("ProductCustRelation");
            }
        }


        IList<ProductAccountRelation> _ProductVendorRelation;
        public IList<ProductAccountRelation> ProductVendorRelation
        {
            get
            {
                return _ProductVendorRelation;
            }
            set
            {
                _ProductVendorRelation = value;
                OnPropertyChanged("ProductVendorRelation");
            }
        }




        private IList<Unit> _UnitList;
        public IList<Unit> UnitList
        {
            get { return _UnitList; }
            set
            {
                _UnitList = value;
                OnPropertyChanged("UnitList");
            }
        }

        private IList<LabelTemplate> _TemplateList;
        public IList<LabelTemplate> TemplateList
        {
            get { return _TemplateList; }
            set
            {
                _TemplateList = value;
                OnPropertyChanged("TemplateList");
            }
        }

        private ProductAlternate _CurAltern;
        public ProductAlternate CurAltern
        {
            get { return _CurAltern; }
            set
            {
                _CurAltern = value;
                OnPropertyChanged("CurAltern");
            }
        }

        public Hashtable BinDirectionList { get { return App.BinDirectionList; } }


        private IList<Product> entitylist;
        public IList<Product> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Product record;
        public Product Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private IList<ZoneBinRelation> _ProductBinRelation;
        public IList<ZoneBinRelation> ProductBinRelation
        {
            get
            { return _ProductBinRelation; }
            set
            {
                _ProductBinRelation = value;
                OnPropertyChanged("ProductBinRelation");
            }
        }



        private IList<ProductTrackRelation> _AllowTrack;
        public IList<ProductTrackRelation> AllowTrack
        {
            get
            { return _AllowTrack; }
            set
            {
                _AllowTrack = value;
                OnPropertyChanged("AllowTrack");
            }
        }



        private IList<TrackOption> _AvailableTrack;
        public IList<TrackOption> AvailableTrack
        {
            get
            { return _AvailableTrack; }
            set
            {
                _AvailableTrack = value;
                OnPropertyChanged("AvailableTrack");
            }
        }


        //Units


        private IList<Unit> _AvailableUnits;
        public IList<Unit> AvailableUnits
        {
            get
            { return _AvailableUnits; }
            set
            {
                _AvailableUnits = value;
                OnPropertyChanged("AvailableUnits");
            }
        }



        private IList<UnitProductRelation> _AssignedUnits;
        public IList<UnitProductRelation> AssignedUnits
        {
            get
            { return _AssignedUnits; }
            set
            {
                _AssignedUnits = value;
                OnPropertyChanged("AssignedUnits");
            }
        }


        private Unit _UnitGroup;
        public Unit UnitGroup
        {
            get
            { return _UnitGroup; }
            set
            {
                _UnitGroup = value;
                OnPropertyChanged("UnitGroup");
            }
        }



        private IList<Unit> _ListUnitGroup;
        public IList<Unit> ListUnitGroup
        {
            get
            { return _ListUnitGroup; }
            set
            {
                _ListUnitGroup = value;
                OnPropertyChanged("ListUnitGroup");
            }
        }


        private IList<Bin> _BinList;
        public IList<Bin> BinList
        {
            get
            { return _BinList; }
            set
            {
                _BinList = value;
                OnPropertyChanged("BinList");
            }
        }


        private IList<Status> _StatusList;
        public IList<Status> StatusList
        {
            get
            { return _StatusList; }
            set
            {
                _StatusList = value;
                OnPropertyChanged("StatusList");
            }
        }

        private IList<PickMethod> _PickMethod;
        public IList<PickMethod> PickMethods
        {
            get
            { return _PickMethod; }
            set
            {
                _PickMethod = value;
                OnPropertyChanged("PickMethods");
            }
        }

    }
}
