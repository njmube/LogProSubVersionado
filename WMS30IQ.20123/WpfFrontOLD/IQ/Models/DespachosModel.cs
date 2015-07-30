using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IDespachosModel
    {
        #region Header

        Location RecordCliente { get; set; }
        IList<Bin> ListBinDespachosAlmacen { get; set; }
        IList<ShowData> ListHeaderDataSave { get; set; }
        DataInformation DataInformationHeader { get; set; }
        Document HeaderDocument { get; set; }
        Boolean IsCheckedCommentsSerial { get; set; }

        #endregion

        #region Serial

        IList<DataDefinitionByBin> CamposSeriales { get; set; }
        IList<Product> ProductosLocation { get; set; }

        #endregion

        #region Details

        DataTable ListRecords { get; set; }
        IList<Label> ListLabelScann { get; set; }
        IList<DataInformation> ListDataInformation { get; set; }
        IList<ShowData> ListDetailsDataSave { get; set; }
        IList<DataDefinitionByBin> CamposDetails { get; set; }

        #endregion
    }

    public class DespachosModel : BusinessEntityBase, IDespachosModel
    {
        #region Header

        private Location _RecordCliente;
        public Location RecordCliente
        {
            get { return _RecordCliente; }
            set
            {
                _RecordCliente = value;
                OnPropertyChanged("RecordCliente");
            }
        }

        private IList<Bin> _ListBinDespachosAlmacen;
        public IList<Bin> ListBinDespachosAlmacen
        {
            get { return _ListBinDespachosAlmacen; }
            set
            {
                _ListBinDespachosAlmacen = value;
                OnPropertyChanged("ListBinDespachosAlmacen");
            }
        }

        private IList<ShowData> _ListHeaderDataSave;
        public IList<ShowData> ListHeaderDataSave
        {
            get { return _ListHeaderDataSave; }
            set
            {
                _ListHeaderDataSave = value;
                OnPropertyChanged("ListHeaderDataSave");
            }
        }

        private DataInformation _DataInformationHeader;
        public DataInformation DataInformationHeader
        {
            get { return _DataInformationHeader; }
            set
            {
                _DataInformationHeader = value;
                OnPropertyChanged("DataInformationHeader");
            }
        }

        private Document _HeaderDocument;
        public Document HeaderDocument
        {
            get { return _HeaderDocument; }
            set
            {
                _HeaderDocument = value;
                OnPropertyChanged("HeaderDocument");
            }
        }

        private Boolean _IsCheckedCommentsSerial;
        public Boolean IsCheckedCommentsSerial
        {
            get { return _IsCheckedCommentsSerial; }
            set
            {
                _IsCheckedCommentsSerial = value;
                OnPropertyChanged("IsCheckedCommentsSerial");
            }
        }

        #endregion

        #region Serial

        private IList<DataDefinitionByBin> _CamposSeriales;
        public IList<DataDefinitionByBin> CamposSeriales
        {
            get { return _CamposSeriales; }
            set
            {
                _CamposSeriales = value;
                OnPropertyChanged("CamposSeriales");
            }
        }

        private IList<Product> _ProductosLocation;
        public IList<Product> ProductosLocation
        {
            get { return _ProductosLocation; }
            set
            {
                _ProductosLocation = value;
                OnPropertyChanged("ProductosLocation");
            }
        }

        #endregion

        #region Details

        private DataTable _ListRecords;
        public DataTable ListRecords
        {
            get { return _ListRecords; }
            set
            {
                _ListRecords = value;
                OnPropertyChanged("ListRecords");
            }
        }

        private IList<Label> _ListLabelScann;
        public IList<Label> ListLabelScann
        {
            get { return _ListLabelScann; }
            set
            {
                _ListLabelScann = value;
                OnPropertyChanged("ListLabelScann");
            }
        }

        private IList<DataInformation> _ListDataInformation;
        public IList<DataInformation> ListDataInformation
        {
            get { return _ListDataInformation; }
            set
            {
                _ListDataInformation = value;
                OnPropertyChanged("ListDataInformation");
            }
        }

        private IList<ShowData> _ListDetailsDataSave;
        public IList<ShowData> ListDetailsDataSave
        {
            get { return _ListDetailsDataSave; }
            set
            {
                _ListDetailsDataSave = value;
                OnPropertyChanged("ListDetailsDataSave");
            }
        }

        private IList<DataDefinitionByBin> _CamposDetails;
        public IList<DataDefinitionByBin> CamposDetails
        {
            get { return _CamposDetails; }
            set
            {
                _CamposDetails = value;
                OnPropertyChanged("CamposDetails");
            }
        }

        #endregion
    }
}
