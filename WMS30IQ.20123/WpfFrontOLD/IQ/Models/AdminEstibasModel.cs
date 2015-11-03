using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IAdminEstibasModel
    {
        DataTable ListadoPallets { get; set; }
        DataTable ListadoSerialesXPallet { get; set; }
        IList<MMaster> ListEstado { get; set; }
        IList<MMaster> ListPosiciones { get; set; }
        IList<MMaster> ListadoPosicionesUnionEstibas { get; set; }
        DataTable ListSerialsOneByOne { get; set; }
    }

    public class AdminEstibasModel : BusinessEntityBase, IAdminEstibasModel
    {
        private DataTable _ListadoPallets;
        public DataTable ListadoPallets
        {
            get { return _ListadoPallets; }
            set
            {
                this._ListadoPallets = value;
                OnPropertyChanged("ListadoPallets");
            }
        }

        private DataTable _ListadoSerialesXPallet;
        public DataTable ListadoSerialesXPallet
        {
            get { return _ListadoSerialesXPallet; }
            set
            {
                this._ListadoSerialesXPallet = value;
                OnPropertyChanged("ListadoSerialesXPallet");
            }
        }
    
        private IList<MMaster> _ListEstado;
        public IList<MMaster> ListEstado
        {
            get { return _ListEstado; }
            set
            {
                _ListEstado = value;
                OnPropertyChanged("ListEstado");
            }
        }

        private IList<MMaster> _ListPosiciones;
        public IList<MMaster> ListPosiciones
        {
            get { return _ListPosiciones; }
            set
            {
                _ListPosiciones = value;
                OnPropertyChanged("ListPosiciones");
            }
        }

        #region Union de estibas y adicion de estibas 1 a 1

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
        #endregion
    }
}
