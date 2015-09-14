using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IEmpaqueDTVModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        //Recibo
        IList<MMaster> ListadoPosiciones { get; set; }
        DataTable ListadoRecibo { get; set; }

        DataTable ListPallets_Empaque { get; set; }
        DataTable ListCajas_Empaque { get; set; }
        DataTable ListSeriales_Empaque { get; set; }
        DataTable Listado_PalletSerial { get; set; }

        IList<MMaster> ListCambioEstado { get; set; }
        DataTable ListadoCalidad { get; set; }
    }

    public class EmpaqueDTVModel : BusinessEntityBase, IEmpaqueDTVModel
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

        private DataTable _ListPallets_Empaque;
        public DataTable ListPallets_Empaque
        {
            get { return _ListPallets_Empaque; }
            set
            {
                _ListPallets_Empaque = value;
                OnPropertyChanged("ListPallets_Empaque");
            }
        }

        private DataTable _ListCajas_Empaque;
        public DataTable ListCajas_Empaque
        {
            get { return _ListCajas_Empaque; }
            set
            {
                _ListCajas_Empaque = value;
                OnPropertyChanged("ListCajas_Empaque");
            }
        }

        private DataTable _ListSeriales_Empaque;
        public DataTable ListSeriales_Empaque
        {
            get { return _ListSeriales_Empaque; }
            set
            {
                _ListSeriales_Empaque = value;
                OnPropertyChanged("ListSeriales_Empaque");
            }
        }

        private DataTable _Listado_PalletSerial;
        public DataTable Listado_PalletSerial
        {
            get { return _Listado_PalletSerial; }
            set
            {
                _Listado_PalletSerial = value;
                OnPropertyChanged("Listado_PalletSerial");
            }
        }

        private IList<MMaster> _ListCambioEstado;
        public IList<MMaster> ListCambioEstado
        {
            get { return _ListCambioEstado; }
            set
            {
                _ListCambioEstado = value;
                OnPropertyChanged("ListCambioEstado");
            }
        }

        private DataTable _ListadoCalidad;
        public DataTable ListadoCalidad
        {
            get { return _ListadoCalidad; }
            set
            {
                _ListadoCalidad = value;
                OnPropertyChanged("ListadoCalidad");
            }
        }

        private DataTable _Listado_PalletSerialCalidad;
        public DataTable Listado_PalletSerialCalidad
        {
            get { return _Listado_PalletSerialCalidad; }
            set
            {
                _Listado_PalletSerialCalidad = value;
                OnPropertyChanged("Listado_PalletSerialCalidad");
            }
        }
    }
}