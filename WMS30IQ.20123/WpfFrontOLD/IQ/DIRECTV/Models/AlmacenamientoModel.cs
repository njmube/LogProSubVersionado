using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IAlmacenamientoModel
    {
        DataTable ListRecords { get; set; }
        IList<MMaster> ListadoPosiciones { get; set; }

        IList<MMaster> ListadoEstadosPallet { get; set; }
        DataTable ListadoRecibo { get; set; }
        DataTable ListUbicacionesDestino { get; set; }

        DataTable ListPallets_Almacenamiento { get; set; }

        DataTable Listado_PalletSerial { get; set; }

        DataTable List_Nocargue { get; set; }
    }

    public class AlmacenamientoModel : BusinessEntityBase, IAlmacenamientoModel
    {
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

        private IList<MMaster> _ListadoEstadosPallet;
        public IList<MMaster> ListadoEstadosPallet
        {
            get { return _ListadoEstadosPallet; }
            set
            {
                _ListadoEstadosPallet = value;
                OnPropertyChanged("ListadoEstadosPallet");
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

        private DataTable _ListPallets_Almacenamiento;
        public DataTable ListPallets_Almacenamiento
        {
            get { return _ListPallets_Almacenamiento; }
            set
            {
                _ListPallets_Almacenamiento = value;
                OnPropertyChanged("ListPallets_Almacenamiento");
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

        private DataTable _List_Nocargue;
        public DataTable List_Nocargue
        {
            get { return _List_Nocargue; }
            set
            {
                _List_Nocargue = value;
                OnPropertyChanged("List_Nocargue");
            }
        }
    }
}