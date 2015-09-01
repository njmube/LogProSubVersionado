using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IUnitModel
    {
        Company CurCompany { get; }
        Unit Record { get; set; }
        IList<Unit> EntityList { get; set; }
        IList<Unit> UnitGroupList { get; set; }

    }

    public class UnitModel: BusinessEntityBase, IUnitModel
    {

        public Company CurCompany { get { return App.curCompany; } }

        private IList<Unit> entitylist;
        public IList<Unit> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Unit record;
        public Unit Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private IList<Unit> unitgroup;
        public IList<Unit> UnitGroupList
        {
            get { return unitgroup; }
            set
            {
                unitgroup = value;
                OnPropertyChanged("UnitGroupList");
            }
        }

    }
}
