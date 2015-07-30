using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IMoverMercanciaDTVModel
    {
        IList<MMaster> ListadoPosiciones { get; set; }
        DataTable ListadoPosicionesOcupadas { get; set; }
        IList<MMaster> ListadoPosicionesCambioUbicacion { get; set; }
        DataTable ListadoProductosActivos { get; set; }
        DataTable ListUbicacionesDestino { get; set; }
        DataTable ListadoCambioUbicacion { get; set; }
        DataTable ListadoCambioClasificacion { get; set; }
        DataTable Listado_PalletSerial { get; set; }
    }

    public class MoverMercanciaDTVModel : BusinessEntityBase, IMoverMercanciaDTVModel
    {
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

        private DataTable _ListadoPosicionesOcupadas;
        public DataTable ListadoPosicionesOcupadas
        {
            get { return _ListadoPosicionesOcupadas; }
            set
            {
                _ListadoPosicionesOcupadas = value;
                OnPropertyChanged("ListadoPosicionesOcupadas");
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

        private DataTable _listadoProducosActivos;
        public DataTable ListadoProductosActivos
        {
            get { return _listadoProducosActivos; }
            set
            {
                _listadoProducosActivos = value;
                OnPropertyChanged("ListadoProductosActivos");
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