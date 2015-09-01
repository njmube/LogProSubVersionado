using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IValidadorSerialesModel
    {

        #region Header

        IList<Location> LocationFromList { get; set; }
        Location LocationFrom { get; set; }
        IList<Bin> BinFromList { get; set; }
        Bin BinFrom { get; set; }
        IList<Location> LocationToList { get; set; }
        Location LocationTo { get; set; }
        IList<BinRoute> BinToList { get; set; }
        Bin BinTo { get; set; }
        IList<ShowData> ListHeaderDataSave { get; set; }
        DataInformation DataInformationHeader { get; set; }
        Document HeaderDocument { get; set; }
        Boolean IsCheckedCommentsSerial { get; set; }

        #endregion

        #region Serial

        IList<DataDefinitionByBin> CamposSeriales { get; set; }
        /// <summary>
        /// ProductoSerial
        /// </summary>
        Product ProductoSerial { get; set; }

        #endregion

        #region Details

        DataTable ListRecords { get; set; }
        IList<Label> ListLabelScann { get; set; }
        IList<DataInformation> ListDataInformation { get; set; }
        IList<ShowData> ListDetailsDataSave { get; set; }
        IList<DataDefinitionByBin> CamposDetails { get; set; }
        String UltimosProcesadosMP { get; set; }

        #endregion

        /// <summary>
        /// Lista de los seriales no cargados.
        /// </summary>
        IList<WMSBusinessService.Label> ListaSerialesNoCargados { get; set; }
    }

    public class ValidadorSerialesModel : BusinessEntityBase, IValidadorSerialesModel
    {
        #region Header

        private IList<Location> _LocationFromList;
        public IList<Location> LocationFromList
        {
            get { return _LocationFromList; }
            set
            {
                _LocationFromList = value;
                OnPropertyChanged("LocationFromList");
            }
        }

        private Location _LocationFrom;
        public Location LocationFrom
        {
            get { return _LocationFrom; }
            set
            {
                _LocationFrom = value;
                OnPropertyChanged("LocationFrom");
            }
        }

        private IList<Bin> _BinFromList;
        public IList<Bin> BinFromList
        {
            get { return _BinFromList; }
            set
            {
                _BinFromList = value;
                OnPropertyChanged("BinFromList");
            }
        }

        private Bin _BinFrom;
        public Bin BinFrom
        {
            get { return _BinFrom; }
            set
            {
                _BinFrom = value;
                OnPropertyChanged("BinFrom");
            }
        }

        private IList<Location> _LocationToList;
        public IList<Location> LocationToList
        {
            get { return _LocationToList; }
            set
            {
                _LocationToList = value;
                OnPropertyChanged("LocationToList");
            }
        }

        private Location _LocationTo;
        public Location LocationTo
        {
            get { return _LocationTo; }
            set
            {
                _LocationTo = value;
                OnPropertyChanged("LocationTo");
            }
        }

        private IList<BinRoute> _BinToList;
        public IList<BinRoute> BinToList
        {
            get { return _BinToList; }
            set
            {
                _BinToList = value;
                OnPropertyChanged("BinToList");
            }
        }

        private Bin _BinTo;
        public Bin BinTo
        {
            get { return _BinTo; }
            set
            {
                _BinTo = value;
                OnPropertyChanged("BinTo");
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

        /// <summary>
        /// ProductoSerial.
        /// </summary>
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


        /// <summary>
        /// Ultimos procesadosMP "variable global.".
        /// </summary>
        private String _UltimosProcesadosMP;
        public String UltimosProcesadosMP
        {
            get { return _UltimosProcesadosMP; }
            set
            {
                _UltimosProcesadosMP = value;
                OnPropertyChanged("UltimosProcesadosMP");
            }
        }

        #endregion

        /// <summary>
        /// Lista de los seriales no cargados.
        /// </summary>
        private IList<WMSBusinessService.Label> listaSerialesNoCargados;
        /// <summary>
        /// Lista de los seriales no cargados.
        /// </summary>
        public IList<WMSBusinessService.Label> ListaSerialesNoCargados
        {
            get { return listaSerialesNoCargados; }
            set
            {
                listaSerialesNoCargados = value;
                OnPropertyChanged("ListaSerialesNoCargados");
            }
        }

    }
}
