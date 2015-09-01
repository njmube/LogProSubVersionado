using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface INoProcesablesModel
    {
        DataTable ListRecords { get; set; }
        IList<MMaster> ListadoPosiciones { get; set; }
        DataTable ListRecords2 { get; set; }
        DataTable ListUbicacionesDestino { get; set; }
    }

    public class NoProcesablesModel : BusinessEntityBase, INoProcesablesModel
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

        private DataTable _ListRecords2;
        public DataTable ListRecords2
        {
            get { return _ListRecords2; }
            set
            {
                _ListRecords2 = value;
                OnPropertyChanged("ListRecords2");
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
    }
}