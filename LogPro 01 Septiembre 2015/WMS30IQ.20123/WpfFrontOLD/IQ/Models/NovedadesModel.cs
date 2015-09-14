using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface INovedadesModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }

        DataTable ListadoPrealertas { get; set; }
        DataTable ListadoNovedades { get; set; }
        DataTable ListadoNovedadesTipoA { get; set; }
        DataTable ListadoNovedadesTipoB { get; set; }
        DataTable ListadoArchivos { get; set; }
    }

    public class NovedadesModel : BusinessEntityBase, INovedadesModel
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


        private DataTable _ListadoPrealertas;
        public DataTable ListadoPrealertas
        {
            get { return _ListadoPrealertas; }
            set
            {
                _ListadoPrealertas = value;
                OnPropertyChanged("ListadoPrealertas");
            }
        }

        private DataTable _ListadoNovedades;
        public DataTable ListadoNovedades
        {
            get { return _ListadoNovedades; }
            set
            {
                _ListadoNovedades = value;
                OnPropertyChanged("ListadoNovedades");
            }
        }

        private DataTable _ListadoNovedadesTipoA;
        public DataTable ListadoNovedadesTipoA
        {
            get { return _ListadoNovedadesTipoA; }
            set
            {
                _ListadoNovedadesTipoA = value;
                OnPropertyChanged("ListadoNovedadesTipoA");
            }
        }

        private DataTable _ListadoNovedadesTipoB;
        public DataTable ListadoNovedadesTipoB
        {
            get { return _ListadoNovedadesTipoB; }
            set
            {
                _ListadoNovedadesTipoB = value;
                OnPropertyChanged("ListadoNovedadesTipoB");
            }
        }

        private DataTable _ListadoArchivos;
        public DataTable ListadoArchivos
        {
            get { return _ListadoArchivos; }
            set
            {
                _ListadoArchivos = value;
                OnPropertyChanged("ListadoArchivos");
            }
        }
    }
}