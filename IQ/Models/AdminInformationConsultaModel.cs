using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IAdminInformationConsultaModel
    {
        #region Busqueda

        Location RecordCliente { get; set; }
        IList<ClassEntity> SearchTypeList { get; set; }
        IList<DataDefinitionByBin> CamposSeriales { get; set; }

        #endregion

        #region Datos Estaticos

        Label LabelSearch { get; set; }
        Document DocumentSearch { get; set; }
        IList<Bin> BinList { get; set; }
        IList<DataDefinition> SerialesActivosList { get; set; }
        IList<Status> StatusList { get; set; }
        IList<Location> LocationList { get; set; }

        #endregion

        #region Datos Dinamicos

        DataInformation DataInformationSearch { get; set; }
        IList<DataDefinitionByBin> CamposDinamicosEditList { get; set; }

        #endregion

        #region Eventos Botones

        IList<ShowData> ListDataSave { get; set; }

        #endregion
    }

    public class AdminInformationConsultaModel : BusinessEntityBase, IAdminInformationConsultaModel
    {
        #region Busqueda

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

        private IList<ClassEntity> _SearchTypeList;
        public IList<ClassEntity> SearchTypeList
        {
            get { return _SearchTypeList; }
            set
            {
                _SearchTypeList = value;
                OnPropertyChanged("SearchTypeList");
            }
        }

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

        #endregion

        #region Datos Estaticos

        private Label _LabelSearch;
        public Label LabelSearch
        {
            get { return _LabelSearch; }
            set
            {
                _LabelSearch = value;
                OnPropertyChanged("LabelSearch");
            }
        }

        private Document _DocumentSearch;
        public Document DocumentSearch
        {
            get { return _DocumentSearch; }
            set
            {
                _DocumentSearch = value;
                OnPropertyChanged("DocumentSearch");
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

        private IList<DataDefinition> _SerialesActivosList;
        public IList<DataDefinition> SerialesActivosList
        {
            get { return _SerialesActivosList; }
            set
            {
                _SerialesActivosList = value;
                OnPropertyChanged("SerialesActivosList");
            }
        }

        private IList<Status> _StatusList;
        public IList<Status> StatusList
        {
            get { return _StatusList; }
            set
            {
                _StatusList = value;
                OnPropertyChanged("StatusList");
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

        #endregion

        #region Datos Dinamicos

        private DataInformation _DataInformationSearch;
        public DataInformation DataInformationSearch
        {
            get { return _DataInformationSearch; }
            set
            {
                _DataInformationSearch = value;
                OnPropertyChanged("DataInformationSearch");
            }
        }

        private IList<DataDefinitionByBin> _CamposDinamicosEditList;
        public IList<DataDefinitionByBin> CamposDinamicosEditList
        {
            get { return _CamposDinamicosEditList; }
            set
            {
                _CamposDinamicosEditList = value;
                OnPropertyChanged("CamposDinamicosEditList");
            }
        }

        #endregion

        #region Eventos Botones

        private IList<ShowData> _ListDataSave;
        public IList<ShowData> ListDataSave
        {
            get { return _ListDataSave; }
            set
            {
                _ListDataSave = value;
                OnPropertyChanged("ListDataSave");
            }
        }

        #endregion
    }
}
