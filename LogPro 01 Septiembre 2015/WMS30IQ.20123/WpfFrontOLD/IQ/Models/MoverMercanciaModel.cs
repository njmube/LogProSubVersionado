using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IMoverMercanciaModel
    {
        IList<MMaster> ListadoPosiciones { get; set; }
        IList<MMaster> ListadoPosicionesUnionEstibas { get; set; }
        IList<MMaster> ListadoPosicionesCambioUbicacion { get; set; }
        DataTable ListadoProductosActivos { get; set; }
        DataTable ListUbicacionesDestino { get; set; }
        DataTable ListUbicacionesDestino_Recibo { get; set; }

        DataTable ListadoCambioUbicacion { get; set; }
        DataTable ListadoCambioClasificacion { get; set; }
        DataTable Listado_PalletSerial { get; set; }

        DataTable ListadoRecibo { get; set; }
        IList<MMaster> ListadoPosicionesRecibo { get; set; }
        DataTable ListadoUbicacionesDestinoRecibo { get; set; }
        DataTable ListSerialsOneByOne { get; set; }
    }

    public class MoverMercanciaModel : BusinessEntityBase, IMoverMercanciaModel
    {
        private DataTable _ListSerialsOneByOne;
        public DataTable ListSerialsOneByOne
        {
            get { return _ListSerialsOneByOne; }
            set 
            {
                this._ListSerialsOneByOne = value;
                OnPropertyChanged("ListSerialsOneByOne");
            }
        }

        private IList<MMaster> _ListadoPosicionesUnionEstibas;
        public IList<MMaster> ListadoPosicionesUnionEstibas
        {
            get { return _ListadoPosicionesUnionEstibas; }
            set
            {
                _ListadoPosicionesUnionEstibas = value;
                OnPropertyChanged("ListadoPosicionesUnionEstibas");
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

        private DataTable _ListadoProductosActivos;
        public DataTable ListadoProductosActivos
        {
            get { return _ListadoProductosActivos; }
            set
            {
                _ListadoProductosActivos = value;
                OnPropertyChanged("ListadoPosiciones");
            }
        }

        private DataTable _ListUbicacionesDestino_Recibo;
        public DataTable ListUbicacionesDestino_Recibo
        {
            get { return _ListUbicacionesDestino_Recibo; }
            set
            {
                _ListUbicacionesDestino_Recibo = value;
                OnPropertyChanged("ListUbicacionesDestino_Recibo");
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

        private DataTable _ListadoCambioUbicacion;
        public DataTable ListadoCambioUbicacion
        {
            get { return _ListadoCambioUbicacion; }
            set
            {
                _ListadoCambioUbicacion = value;
                OnPropertyChanged("ListadoCambioUbicacion");
            }
        }

        private DataTable _ListadoCambioClasificacion;
        public DataTable ListadoCambioClasificacion
        {
            get { return _ListadoCambioClasificacion; }
            set
            {
                _ListadoCambioClasificacion = value;
                OnPropertyChanged("ListadoCambioClasificacion");
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

        private IList<MMaster> _ListadoPosicionesRecibo;
        public IList<MMaster> ListadoPosicionesRecibo
        {
            get { return _ListadoPosicionesRecibo; }
            set
            {
                _ListadoPosicionesRecibo = value;
                OnPropertyChanged("ListadoPosicionesRecibo");
            }
        }

        private DataTable _ListadoUbicacionesDestinoRecibo;
        public DataTable ListadoUbicacionesDestinoRecibo
        {
            get { return _ListadoUbicacionesDestinoRecibo; }
            set
            {
                _ListadoUbicacionesDestinoRecibo = value;
                OnPropertyChanged("ListadoUbicacionesDestinoRecibo");
            }
        }
    }
}