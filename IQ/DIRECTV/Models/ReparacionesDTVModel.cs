using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IReparacionesDTVModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }
        DataTable ListadoRecibo { get; set; }
        IList<MMaster> ListadoMotivos { get; set; }  
        DataTable ListReparaciones { get; set; }

        //Asignacion
        DataTable ListadoAsignacion { get; set; }
        DataTable ListadoEquiposEstiba { get; set; }
        IList<SysUser> ListadoTecnicos { get; set; }

        DataTable Listado_PalletSerial { get; set; }
    }

    public class ReparacionesDTVModel : BusinessEntityBase, IReparacionesDTVModel
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

        private IList<MMaster> _ListadoFallaDiagnostico;
        public IList<MMaster> ListadoFallaDiagnostico
        {
            get { return _ListadoFallaDiagnostico; }
            set
            {
                _ListadoFallaDiagnostico = value;
                OnPropertyChanged("ListadoFallaDiagnostico");
            }
        }

            private IList<MMaster> _ListadoMotivos;
            public IList<MMaster> ListadoMotivos
            {
                get { return _ListadoMotivos; }
                set
                {
                    _ListadoMotivos = value;
                    OnPropertyChanged("ListadoMotivos");
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
        private DataTable _ListReparaciones;
        public DataTable ListReparaciones
        {
            get { return _ListReparaciones; }
            set
            {
                _ListReparaciones = value;
                OnPropertyChanged("ListReparaciones");
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

        //Asignacion
        private DataTable _ListadoAsignacion;
        public DataTable ListadoAsignacion
        {
            get { return _ListadoAsignacion; }
            set
            {
                _ListadoAsignacion = value;
                OnPropertyChanged("ListadoAsignacion");
            }
        }

        private DataTable _ListadoEquiposEstiba;
        public DataTable ListadoEquiposEstiba
        {
            get { return _ListadoEquiposEstiba; }
            set
            {
                _ListadoEquiposEstiba = value;
                OnPropertyChanged("ListadoEquiposEstiba");
            }
        }

        private IList<SysUser> _ListadoTecnicos;
        public IList<SysUser> ListadoTecnicos
        {
            get { return _ListadoTecnicos; }
            set
            {
                _ListadoTecnicos = value;
                OnPropertyChanged("ListadoTecnicos");
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
    }
}