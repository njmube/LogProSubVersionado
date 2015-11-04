using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.IQ.Models
{
    public interface IConsultaTrackingModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        DataTable ListMovimientos { get; set; }
        DataTable ListadoEquipos { get; set; }

        DataTable ListadoRecibo { get; set; }

        IList<MMaster> ListadoModelos { get; set; }
        IList<MMaster> ListadoMotScrap { get; set; }
        IList<MMaster> ListadoCiudades { get; set; }
        IList<MMaster> ListadoEstatusLogPro { get; set; }
        IList<MMaster> ListadoOrigen { get; set; }
        IList<MMaster> ListadoTipoOrigen { get; set; }
        IList<MMaster> ListadoTipoREC { get; set; }
        IList<MMaster> ListadoEstadoRR { get; set; }
        IList<MMaster> ListadoTipoDev { get; set; }
        IList<MMaster> ListadoCausaIngEq { get; set; }
        IList<Product> ListadoProductos { get; set; }
        IList<MMaster> ListadoEstMaterial { get; set; }
        IList<MMaster> ListadoFallas { get; set; }
        IList<MMaster> ListadoSINO { get; set; }
        IList<MMaster> ListadoPosiciones { get; set; }

    }
    public class ConsultaTrackingModel : BusinessEntityBase, IConsultaTrackingModel
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

        private IList<MMaster> _ListadoTipoOrigen;
        public IList<MMaster> ListadoTipoOrigen
        {
            get { return _ListadoTipoOrigen; }
            set
            {
                _ListadoTipoOrigen = value;
                OnPropertyChanged("ListadoTipoOrigen");
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

        private IList<MMaster> _ListadoFallas;
        public IList<MMaster> ListadoFallas
        {
            get { return _ListadoFallas; }
            set
            {
                _ListadoFallas = value;
                OnPropertyChanged("ListadoFallas");
            }
        }

        private IList<MMaster> _ListadoMotScrap;
        public IList<MMaster> ListadoMotScrap
        {
            get { return _ListadoMotScrap; }
            set
            {
                _ListadoMotScrap = value;
                OnPropertyChanged("ListadoMotScrap");
            }
        }

        private IList<MMaster> _ListadoEstMaterial;
        public IList<MMaster> ListadoEstMaterial
        {
            get { return _ListadoEstMaterial; }
            set
            {
                _ListadoEstMaterial = value;
                OnPropertyChanged("ListadoEstMaterial");
            }
        }

        private IList<MMaster> _ListadoEstatusLogPro;
        public IList<MMaster> ListadoEstatusLogPro
        {
            get { return _ListadoEstatusLogPro; }
            set
            {
                _ListadoEstatusLogPro = value;
                OnPropertyChanged("ListadoEstatusLogPro");
            }
        }

        private IList<MMaster> _ListadoTipoDev;
        public IList<MMaster> ListadoTipoDev
        {
            get { return _ListadoTipoDev; }
            set
            {
                _ListadoTipoDev = value;
                OnPropertyChanged("ListadoTipoDev");
            }
        }

        private IList<MMaster> _ListadoSINO;
        public IList<MMaster> ListadoSINO
        {
            get { return _ListadoSINO; }
            set
            {
                _ListadoSINO = value;
                OnPropertyChanged("ListadoSINO");
            }
        }

        private IList<MMaster> _ListadoCausaIngEq;
        public IList<MMaster> ListadoCausaIngEq
        {
            get { return _ListadoCausaIngEq; }
            set
            {
                _ListadoCausaIngEq = value;
                OnPropertyChanged("ListadoCausaIngEq");
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

        private IList<MMaster> _ListadoModelos;
        public IList<MMaster> ListadoModelos
        {
            get { return _ListadoModelos; }
            set
            {
                _ListadoModelos = value;
                OnPropertyChanged("ListadoModelos");
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

        private DataTable _ListMovimientos;
        public DataTable ListMovimientos
        {
            get { return _ListMovimientos; }
            set
            {
                _ListMovimientos = value;
                OnPropertyChanged("ListMovimientos");
            }
        }

        private DataTable _ListadoEquipos;
        public DataTable ListadoEquipos
        {
            get { return _ListadoEquipos; }
            set
            {
                _ListadoEquipos = value;
                OnPropertyChanged("ListadoEquipos");
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
    }
}
