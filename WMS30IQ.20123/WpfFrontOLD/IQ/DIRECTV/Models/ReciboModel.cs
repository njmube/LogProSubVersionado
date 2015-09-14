using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IReciboModel
    {
        DataTable ListRecords { get; set; }
        IList<Product> ListadoProductos { get; set; }
        IList<MMaster> ListadoModelosDescripcion { get; set; }
        DataTable List_Nocargue { get; set; }

        DataTable List_NocarguePrea { get; set; }
        DataTable ListPrealerta { get; set; }

    }

    public class ReciboModel : BusinessEntityBase, IReciboModel
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

        private IList<MMaster> _ListadoModelosDescripcion;
        public IList<MMaster> ListadoModelosDescripcion
        {
            get { return _ListadoModelosDescripcion; }
            set
            {
                _ListadoModelosDescripcion = value;
                OnPropertyChanged("ListadoModelosDescripcion");
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

        private DataTable _List_NocarguePrea;
        public DataTable List_NocarguePrea
        {
            get { return _List_NocarguePrea; }
            set
            {
                _List_NocarguePrea = value;
                OnPropertyChanged("List_NocarguePrea");
            }
        }

        private DataTable _ListPrealerta;
        public DataTable ListPrealerta
        {
            get { return _ListPrealerta; }
            set
            {
                _ListPrealerta = value;
                OnPropertyChanged("ListPrealerta");
            }
        }
        
    }
}