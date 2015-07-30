using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Data;



namespace WpfFront.Models
{
    public interface IImpresionEtiquetasModel
    {
        #region Busqueda

        Location RecordCliente { get; set; }
        IList<MMaster> ListadoEtiquetas { get; set; }
        DataTable ListaEquipos { get; set; }
        DataTable ListaEquiposAuxiliar { get; set; }

        #endregion

        #region Eventos Botones

        #endregion
    }

    public class ImpresionEtiquetasModel : BusinessEntityBase, IImpresionEtiquetasModel
    {
        #region Busqueda

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

        private IList<MMaster> _ListadoEtiquetas;
        public IList<MMaster> ListadoEtiquetas
        {
            get { return _ListadoEtiquetas; }
            set
            {
                _ListadoEtiquetas = value;
                OnPropertyChanged("ListadoEtiquetas");
            }
        }

        private DataTable _ListaEquipos;
        public DataTable ListaEquipos
        {
            get { return _ListaEquipos; }
            set 
            {
                _ListaEquipos = value;
                OnPropertyChanged("ListaEquipos");
            }
        }

        private DataTable _ListaEquiposAuxiliar;
        public DataTable ListaEquiposAuxiliar
        {
            get { return _ListaEquiposAuxiliar; }
            set
            {
                _ListaEquiposAuxiliar = value;
                OnPropertyChanged("ListaEquiposAuxiliar");
            }
        }

        #endregion

        #region Eventos Botones

        #endregion
    }
}
