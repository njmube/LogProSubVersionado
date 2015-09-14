using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;


namespace WpfFront.Models
{
    public interface IEntradaAlmacenV2Model
    {
        DataTable ListRecords { get; set; }

        DataTable List_Nocargue { get; set; }

        DataTable List_NocargueAlert { get; set; }

        //DataTable ListRecords_Alertas { get; set; }
        IList<Product> ListadoProductos { get; set; }
        //DataTable ListadoProductos { get; set; }
        //ComboBox Detalles
        IList<MMaster> ListadoOrigen { get; set; }
        IList<MMaster> ListadoCiudades { get; set; }
        IList<MMaster> ListadoAliado { get; set; }
        IList<MMaster> ListadoCodigoSAP { get; set; }
        IList<MMaster> ListadoEstadoRR { get; set; }
        IList<MMaster> ListadoTipoREC { get; set; }
        IList<MMaster> ListadoCentros { get; set; }
        IList<MMaster> ListadoFamilias { get; set; }

        IList<MMaster> ListadoPreaTipoRecoleccion { get; set; }
        IList<MMaster> ListadoPreaTipoOrigen { get; set; }

    }

    public class EntradaAlmacenV2Model : BusinessEntityBase, IEntradaAlmacenV2Model
    {
        private DataTable _ListRecords, _ListRecordsAlertas, _ListNoCargue, _ListNoCargueAlert;
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

        public DataTable ListRecords_Alertas
        {
            get { return _ListRecordsAlertas; }
            set
            {
                _ListRecordsAlertas = value;
                OnPropertyChanged("ListRecords");
            }
        }

        public DataTable List_Nocargue
        {
            get { return _ListNoCargue; }
            set
            {
                _ListNoCargue = value;
                OnPropertyChanged("ListRecords");
            }
        }

        public DataTable List_NocargueAlert
        {
            get { return _ListNoCargueAlert; }
            set
            {
                _ListNoCargueAlert = value;
                OnPropertyChanged("ListRecords");
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

        //private DataTable _ListadoProductos;
        //public DataTable ListadoProductos
        //{
        //    get { return _ListadoProductos; }
        //    set
        //    {
        //        _ListadoProductos = value;
        //        OnPropertyChanged("ListadoProductos");
        //    }
        //}

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

        private IList<MMaster> _ListadoPreaTipoRecoleccion;
        public IList<MMaster> ListadoPreaTipoRecoleccion
        {
            get { return _ListadoPreaTipoRecoleccion; }
            set
            {
                _ListadoPreaTipoRecoleccion = value;
                OnPropertyChanged("ListadoPreaTipoRecoleccion");
            }
        }

        private IList<MMaster> _ListadoPreaTipoOrigen;
        public IList<MMaster> ListadoPreaTipoOrigen
        {
            get { return _ListadoPreaTipoOrigen; }
            set
            {
                _ListadoPreaTipoOrigen = value;
                OnPropertyChanged("ListadoPreaTipoOrigen");
            }
        }
    }
}