using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IEntradaAlmacenModel
    {
        #region Header

        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        IList<ShowData> ListHeaderDataSave { get; set; }
        DataInformation DataInformationHeader { get; set; }
        Document HeaderDocument { get; set; }
        Boolean IsCheckedCommentsSerial { get; set; }

        #endregion

        #region Serial

        IList<DataDefinitionByBin> CamposSeriales { get; set; }
        //IList<Product> ProductosLocation { get; set; }
        Product ProductoSerial { get; set; }

        #endregion

        #region Details

        DataTable ListRecords { get; set; }
        IList<ShowData> ListDetailsDataSave { get; set; }
        IList<DataDefinitionByBin> CamposDetails { get; set; }
        String UltimosProcesados { get; set; }

        #endregion

        #region Nuevas Lineas

        IList<WMSBusinessService.Label> ListaSerialesNoCargados { get; set; }

        #endregion

    }

    public class EntradaAlmacenModel : BusinessEntityBase, IEntradaAlmacenModel
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

        private IList<Bin> _ListBinEntradaAlmacen;
        public IList<Bin> ListBinEntradaAlmacen
        {
            get { return _ListBinEntradaAlmacen; }
            set
            {
                _ListBinEntradaAlmacen = value;
                OnPropertyChanged("ListBinEntradaAlmacen");
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

        /*private IList<Product> _ProductosLocation;
        public IList<Product> ProductosLocation
        {
            get { return _ProductosLocation; }
            set
            {
                _ProductosLocation = value;
                OnPropertyChanged("ProductosLocation");
            }
        }*/

        private Product _ProductoSerial;
        public Product ProductoSerial
        {
            get { return _ProductoSerial; }
            set
            {
                _ProductoSerial = value;
                OnPropertyChanged("ProductoSerial");
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

        /// <summary>
        /// Ultimos procesados "variable global.".
        /// </summary>
        private String _UltimosProcesados;
        public String UltimosProcesados
        {
            get { return _UltimosProcesados; }
            set
            {
                _UltimosProcesados = value;
                OnPropertyChanged("UltimosProcesados");
            }
        }

        #endregion

        #region Nuevas Lineas

        private IList<WMSBusinessService.Label> listaSerialesNoCargados;
        public IList<WMSBusinessService.Label> ListaSerialesNoCargados
        {
            get { return listaSerialesNoCargados; }
            set
            {
                listaSerialesNoCargados = value;
                OnPropertyChanged("ListaSerialesNoCargados");
            }
        }


        #endregion
    }
}