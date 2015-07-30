using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface ISmartCardProdDTVModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }

        DataTable ListRecordsReciclaje { get; set; }

        DataTable ListRecords_Asignada { get; set; }

        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        List<string> test { get; set; }

        DataTable ListadoRecibo { get; set; }
        IList<MMaster> ListEstados { get; set; }
    }

    public class SmartCardProdDTVModel : BusinessEntityBase, ISmartCardProdDTVModel
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

        private IList<MMaster> _ListEstados;
        public IList<MMaster> ListEstados
        {
            get { return _ListEstados; }
            set
            {
                _ListEstados = value;
                OnPropertyChanged("ListEstados");
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


        private List<string> _test;
        public List<string> test
        {
            get { return _test; }
            set
            {
                _test = value;
                OnPropertyChanged("cb_Ubicacion");
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

        private DataTable _ListRecords_Asignada;
        public DataTable ListRecords_Asignada
        {
            get { return _ListRecords_Asignada; }
            set
            {
                _ListRecords_Asignada = value;
                OnPropertyChanged("ListRecords_Asignada");
            }
        }

        private DataTable _ListRecordsReciclaje;
        public DataTable ListRecordsReciclaje
        {
            get { return _ListRecordsReciclaje; }
            set
            {
                _ListRecordsReciclaje = value;
                OnPropertyChanged("ListRecordsReciclaje");
            }
        }
    }
}