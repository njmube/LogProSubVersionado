using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IBinModel
    {
        Bin Record { get; set; }
        IList<Bin> EntityList { get; set; }
        IList<Location> LocationList { get; }
        IList<CustomProcess> ProcessList { get; }
        IList<Status> StatusList { get; }
        IList<Status> SearchStatusList { get; }
    }

    public class BinModel: BusinessEntityBase, IBinModel
    {

        private IList<Bin> entitylist;
        public IList<Bin> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Bin record;
        public Bin Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        public IList<Location> LocationList
        { get { return App.LocationList; }  }

        public IList<Status> StatusList
        { get { return App.EntityStatusList; } }

        public IList<CustomProcess> ProcessList
        { get { return App.CustomProcessList; } }

        // lista de status, con opción de seleccionar vacío en la búsqueda.
        public IList<Status> SearchStatusList
        { get {
            IList<Status> tmpStatus = App.EntityStatusList;
            tmpStatus.Add(new Status { StatusID=-1, Name="" });
            return tmpStatus; } }
    }
}
