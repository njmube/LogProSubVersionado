using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;


namespace WpfFront.Models
{
    public interface IDiagnosticoModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        IList<MMaster> ListadoPosicionesCambioUbicacion { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        //Recibo
        IList<MMaster> ListadoPosiciones { get; set; }
        DataTable ListRecordsAddToPallet { get; set; }
        DataTable ListadoRecibo { get; set; }
        DataTable ListadoTecnicoReparacion { get; set; }    
    }

    public class DiagnosticoModel : BusinessEntityBase, IDiagnosticoModel
    {
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

        private IList<MMaster> _ListadoPosicionesCambioUbicacion;
        public IList<MMaster> ListadoPosicionesCambioUbicacion
        {
            get { return _ListadoPosicionesCambioUbicacion; }
            set
            {
                _ListadoPosicionesCambioUbicacion = value;
                OnPropertyChanged("ListadoPosiciones");
            }
        }

        private DataTable _ListRecordsAddToPallet;
        public DataTable ListRecordsAddToPallet
        {
            get { return _ListRecordsAddToPallet; }
            set
            {
                _ListRecordsAddToPallet = value;
                OnPropertyChanged("ListRecordsAddToPallet");
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

        private IList<MMaster> _ListadoPosiciones;
        public IList<MMaster> ListadoPosiciones
        {
            get { return _ListadoPosiciones; }
            set
            {
                _ListadoPosiciones = value;
                OnPropertyChanged("ListadoPosiciones");
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

        private DataTable _ListRecords_1;
        public DataTable ListRecords_1
        {
            get { return _ListRecords_1; }
            set
            {
                _ListRecords_1 = value;
                OnPropertyChanged("ListRecords_1");
            }
        }

        private IList<MMaster> _ListCampo1;
        public IList<MMaster> ListCampo1
        {
            get { return _ListCampo1; }
            set
            {
                _ListCampo1 = value;
                OnPropertyChanged("ListCampo1");
            }
        }

        private DataTable _ListUbicacionesDestino;
        public DataTable ListUbicacionesDestino
        {
            get { return _ListUbicacionesDestino; }
            set
            {
                _ListUbicacionesDestino = value;
                OnPropertyChanged("ListUbicacionesDestino");
            }
        }

        //Recibo
        //private IList<MMaster> _ListadoPosiciones;
        //public IList<MMaster> ListadoPosiciones
        //{
        //    get { return _ListadoPosiciones; }
        //    set
        //    {
        //        _ListadoPosiciones = value;
        //        OnPropertyChanged("ListadoPosiciones");
        //    }
        //}

        private DataTable _ListadoRecibo;
        public DataTable ListadoRecibo
        {
            get { return _ListadoRecibo; }
            set
            {
                _ListadoRecibo = value;
                OnPropertyChanged("ListadoRecibo");
            }
        }

        private DataTable _ListadoTecnicoReparacion;
        public DataTable  ListadoTecnicoReparacion
        {
            get { return _ListadoTecnicoReparacion;}
            set 
            {
                _ListadoTecnicoReparacion = value;
                OnPropertyChanged("ListadoTecnicoReparacion");
            }
        }
    }
}