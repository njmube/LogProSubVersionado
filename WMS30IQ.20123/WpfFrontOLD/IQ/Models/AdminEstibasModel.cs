using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using WpfFront.WMSBusinessService;
using System.Data;

namespace WpfFront.Models
{
    public interface IAdminEstibasModel
    {
        DataTable ListSerialsOneByOne { get; set; }
        IList<MMaster> ListadoPosicionesUnionEstibas { get; set; }
    }

    public class AdminEstibasModel : BusinessEntityBase, IAdminEstibasModel
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
    }
}
