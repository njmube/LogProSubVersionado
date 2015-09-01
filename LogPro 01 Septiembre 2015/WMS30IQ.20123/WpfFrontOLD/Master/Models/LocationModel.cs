using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ILocationModel
    {
        IList<Company> CompanyList { get; }
        IList<Status> StatusList { get; }
        Location Record { get; set; }
        IList<Location> EntityList { get; set; }

    }

    public class LocationModel: BusinessEntityBase, ILocationModel
    {

        public IList<Company> CompanyList { get { return App.CompanyList; } }
        public IList<Status> StatusList { get { return App.EntityStatusList; } }

        private IList<Location> entitylist;
        public IList<Location> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Location record;
        public Location Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


    }
}
