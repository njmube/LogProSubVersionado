using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IActualizacionRRModel
    {
        DataTable ListRecords { get; set; }
        DataTable List_Nocargue { get; set; }
        DataTable List_Cuarentena { get; set; }
        DataTable ListRecordsRep { get; set; }
        DataTable ListRecordsRepDB { get; set; }
        DataTable ListRecordsSAP { get; set; }
        DataTable ListRecordsSAP_Serial { get; set; }
        IList<Product> ListadoProductos { get; set; }

        //ComboBox Detalles
        IList<MMaster> ListadoOrigen { get; set; }
        IList<MMaster> ListadoCiudades { get; set; }
        IList<MMaster> ListadoAliado { get; set; }
        IList<MMaster> ListadoCodigoSAP { get; set; }
        IList<MMaster> ListadoEstadoRR { get; set; }
        IList<MMaster> ListadoTipoREC { get; set; }
        IList<MMaster> ListadoCentros { get; set; }
        IList<MMaster> ListadoFamilias { get; set; }
    }

    public class ActualizacionRRModel : BusinessEntityBase, IActualizacionRRModel
    {
        private DataTable _ListRecords;
        private DataTable _ListRecordsDialog;
        public DataTable ListRecords
        {
            get { return _ListRecords; }
            set
            {
                _ListRecords = value;
                OnPropertyChanged("ListRecords");
            }
        }

        private DataTable _List_Nocargue;
        public DataTable List_Nocargue
        {
            get { return _List_Nocargue; }
            set
            {
                _List_Nocargue = value;
                OnPropertyChanged("List_Nocargue");
            }
        }

        private DataTable _List_Cuarentena;
        public DataTable List_Cuarentena
        {
            get { return _List_Cuarentena; }
            set
            {
                _List_Cuarentena = value;
                OnPropertyChanged("List_Cuarentena");
            }
        }

        private DataTable _ListRecordsRep;
        public DataTable ListRecordsRep
        {
            get { return _ListRecordsRep; }
            set
            {
                _ListRecordsRep = value;
                OnPropertyChanged("ListRecordsRep");
            }
        }

        private DataTable _ListRecordsRepDB;
        public DataTable ListRecordsRepDB
        {
            get { return _ListRecordsRepDB; }
            set
            {
                _ListRecordsRepDB = value;
                OnPropertyChanged("ListRecordsRepDB");
            }
        }

        private DataTable _ListRecordsSAP;
        public DataTable ListRecordsSAP
        {
            get { return _ListRecordsSAP; }
            set
            {
                _ListRecordsSAP = value;
                OnPropertyChanged("ListRecordsSAP");
            }
        }

        private DataTable _ListRecordsSAP_Serial;
        public DataTable ListRecordsSAP_Serial
        {
            get { return _ListRecordsSAP_Serial; }
            set
            {
                _ListRecordsSAP_Serial = value;
                OnPropertyChanged("ListRecordsSAP_Serial");
            }
        }

        private IList<Product> _ListadoProductos;
        public IList<Product> ListadoProductos
        {
            get { return _ListadoProductos; }
            set
            {
                _ListadoProductos = value;
                OnPropertyChanged("ListadoProductos");
            }
        }

        //ComboBox Detalles
        private IList<MMaster> _ListadoOrigen;
        public IList<MMaster> ListadoOrigen
        {
            get { return _ListadoOrigen; }
            set
            {
                _ListadoOrigen = value;
                OnPropertyChanged("ListadoOrigen");
            }
        }

        private IList<MMaster> _ListadoCiudades;
        public IList<MMaster> ListadoCiudades
        {
            get { return _ListadoCiudades; }
            set
            {
                _ListadoCiudades = value;
                OnPropertyChanged("ListadoCiudades");
            }
        }

        private IList<MMaster> _ListadoAliado;
        public IList<MMaster> ListadoAliado
        {
            get { return _ListadoAliado; }
            set
            {
                _ListadoAliado = value;
                OnPropertyChanged("ListadoAliado");
            }
        }

        private IList<MMaster> _ListadoCodigoSAP;
        public IList<MMaster> ListadoCodigoSAP
        {
            get { return _ListadoCodigoSAP; }
            set
            {
                _ListadoCodigoSAP = value;
                OnPropertyChanged("ListadoCodigoSAP");
            }
        }

        private IList<MMaster> _ListadoEstadoRR;
        public IList<MMaster> ListadoEstadoRR
        {
            get { return _ListadoEstadoRR; }
            set
            {
                _ListadoEstadoRR = value;
                OnPropertyChanged("ListadoEstadoRR");
            }
        }

        private IList<MMaster> _ListadoTipoREC;
        public IList<MMaster> ListadoTipoREC
        {
            get { return _ListadoTipoREC; }
            set
            {
                _ListadoTipoREC = value;
                OnPropertyChanged("ListadoTipoREC");
            }
        }

        private IList<MMaster> _ListadoCentros;
        public IList<MMaster> ListadoCentros
        {
            get { return _ListadoCentros; }
            set
            {
                _ListadoCentros = value;
                OnPropertyChanged("ListadoCentros");
            }
        }

        private IList<MMaster> _ListadoFamilias;
        public IList<MMaster> ListadoFamilias
        {
            get { return _ListadoFamilias; }
            set
            {
                _ListadoFamilias = value;
                OnPropertyChanged("ListadoFamilias");
            }
        }
    }
}