using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IReparacionesModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        IList<MMaster> ListadoPosicionesCambioUbicacion { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        DataTable ListRecords_1 { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }
        DataTable ListadoRecibo { get; set; }

        //Asignacion
        DataTable ListadoAsignacion { get; set; }
        DataTable ListadoEquiposEstiba { get; set; }
        IList<MMaster> ListadoMotivos { get; set; }

        IList<MMaster> ListadoEstadoFinal { get; set; }
        IList<SysUser> ListadoTecnicos { get; set; }
        DataTable ListadoTecnicosReparacion { get; set; }
        DataTable ListRecordsAddToPallet { get; set; }
        DataTable ListReparaciones { get; set; }
        DataTable ListEquiposReparadosByUser { get; set; }

    }

    public class ReparacionesModel : BusinessEntityBase, IReparacionesModel
    {
        private DataTable _ListEquiposReparadosByUser;
        public DataTable ListEquiposReparadosByUser
        {
            get { return this._ListEquiposReparadosByUser; }
            set 
            {
                this._ListEquiposReparadosByUser = value;
                OnPropertyChanged("ListEquiposReparadosByUser");
            }
        }

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

        private IList<MMaster> _ListadoEstadoFinal;
        public IList<MMaster> ListadoEstadoFinal
        {
            get { return _ListadoEstadoFinal; }
            set
            {
                _ListadoEstadoFinal = value;
                OnPropertyChanged("ListadoEstadoFinal");
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

        private DataTable _ListadoTecnicosReparacion;
        public DataTable ListadoTecnicosReparacion
        {
            get { return _ListadoTecnicosReparacion; }
            set
            {
                _ListadoTecnicosReparacion = value;
                OnPropertyChanged("ListadoTecnicosReparacion");
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

    }
}