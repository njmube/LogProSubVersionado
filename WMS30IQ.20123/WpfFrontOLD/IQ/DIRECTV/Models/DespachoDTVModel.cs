﻿using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IDespachoDTVModel
    {
        Location RecordCliente { get; set; }
        IList<Bin> ListBinEntradaAlmacen { get; set; }
        Document HeaderDocument { get; set; }
        Product ProductoSerial { get; set; }
        DataTable ListRecords { get; set; }
        IList<MMaster> ListCampo1 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        DataTable ListadoProductosDespacho { get; set; }
        DataTable ListadoDespachos { get; set; }
        DataTable Listado_PalletSerial { get; set; }

        DataTable Listado_PalletSerialDespacho { get; set; }
        DataTable ListadoRecibo { get; set; }
    }

    public class DespachoDTVModel : BusinessEntityBase, IDespachoDTVModel
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

        private DataTable _ListadoProductosDespacho;
        public DataTable ListadoProductosDespacho
        {
            get { return _ListadoProductosDespacho; }
            set 
            {
                _ListadoProductosDespacho = value;
                OnPropertyChanged("ListadoProductosDespacho");
            }
        }

        private DataTable _ListadoDespachos;
        public DataTable ListadoDespachos
        {
            get { return _ListadoDespachos; }
            set
            {
                _ListadoDespachos = value;
                OnPropertyChanged("ListadoDespachos");
            }
        }

        private DataTable _Listado_PalletSerial;
        public DataTable Listado_PalletSerial
        {
            get { return _Listado_PalletSerial;}
            set 
            {
                this._Listado_PalletSerial = value;
                OnPropertyChanged("Listado_PalletSerial");
            }
        }

        private DataTable _Listado_PalletSerialDespacho;
        public DataTable Listado_PalletSerialDespacho
        {
            get { return _Listado_PalletSerialDespacho; }
            set
            {
                this._Listado_PalletSerialDespacho = value;
                OnPropertyChanged("Listado_PalletSerialDespacho");
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