using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IDataInformationModel
    {
        //Principal
        Location Record { get; set; }
        IList<Location> LocationList { get; set; }
        //DataDefinition
        DataDefinition RecordDataDefinition { get; set; }
        IList<DataDefinition> DataDefinitionList { get; set; }
        IList<WFDataType> WFDataTypeList { get; set; }
        //DataDefinitionByBin
        Bin RecordBin { get; set; }
        IList<Bin> BinList { get; set; }
        IList<DataDefinition> DataDefinitionListNotUse { get; set; }
        IList<DataDefinitionByBin> DataDefinitionByBinListUsed { get; set; }
        //BinRoute
        BinRoute RecordBinRoute { get; set; }
        IList<BinRoute> BinRouteList { get; set; }
        IList<Bin> BinToList { get; set; }
    }

    public class DataInformationModel : BusinessEntityBase, IDataInformationModel
    {
        //Principal
        private Location _Record;
        public Location Record
        {
            get { return _Record; }
            set
            {
                _Record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<Location> _LocationList;
        public IList<Location> LocationList
        {
            get { return _LocationList; }
            set
            {
                _LocationList = value;
                OnPropertyChanged("LocationList");
            }
        }

        //DataDefinition
        private DataDefinition _RecordDataDefinition;
        public DataDefinition RecordDataDefinition
        {
            get { return _RecordDataDefinition; }
            set
            {
                _RecordDataDefinition = value;
                OnPropertyChanged("RecordDataDefinition");
            }
        }

        private IList<DataDefinition> _DataDefinitionList;
        public IList<DataDefinition> DataDefinitionList
        {
            get { return _DataDefinitionList; }
            set
            {
                _DataDefinitionList = value;
                OnPropertyChanged("DataDefinitionList");
            }
        }

        private IList<WFDataType> _WFDataTypeList;
        public IList<WFDataType> WFDataTypeList
        {
            get { return _WFDataTypeList; }
            set
            {
                _WFDataTypeList = value;
                OnPropertyChanged("WFDataTypeList");
            }
        }

        private IList<MType> _MetaTypeList;
        public IList<MType> MetaTypeList
        {
            get { return _MetaTypeList; }
            set
            {
                _MetaTypeList = value;
                OnPropertyChanged("MetaTypeList");
            }
        }

        //DataDefinitionByBin
        private Bin _RecordBin;
        public Bin RecordBin
        {
            get { return _RecordBin; }
            set
            {
                _RecordBin = value;
                OnPropertyChanged("RecordBin");
            }
        }

        private IList<Bin> _BinList;
        public IList<Bin> BinList
        {
            get { return _BinList; }
            set
            {
                _BinList = value;
                OnPropertyChanged("BinList");
            }
        }

        private IList<DataDefinition> _DataDefinitionListNotUse;
        public IList<DataDefinition> DataDefinitionListNotUse
        {
            get { return _DataDefinitionListNotUse; }
            set
            {
                _DataDefinitionListNotUse = value;
                OnPropertyChanged("DataDefinitionListNotUse");
            }
        }

        private IList<DataDefinitionByBin> _DataDefinitionByBinListUsed;
        public IList<DataDefinitionByBin> DataDefinitionByBinListUsed
        {
            get { return _DataDefinitionByBinListUsed; }
            set
            {
                _DataDefinitionByBinListUsed = value;
                OnPropertyChanged("DataDefinitionByBinListUsed");
            }
        }

        //BinRoute
        private BinRoute _RecordBinRoute;
        public BinRoute RecordBinRoute
        {
            get { return _RecordBinRoute; }
            set 
            {
                _RecordBinRoute = value;
                OnPropertyChanged("RecordBinRoute");
            }
        }
        private IList<BinRoute> _BinRouteList;
        public IList<BinRoute> BinRouteList
        {
            get { return _BinRouteList; }
            set
            {
                _BinRouteList = value;
                OnPropertyChanged("BinRouteList");
            }
        }

        private IList<Bin> _BinToList;
        public IList<Bin> BinToList
        {
            get { return _BinToList; }
            set
            {
                _BinToList = value;
                OnPropertyChanged("BinToList");
            }
        }
    }
}
