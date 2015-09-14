using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IZoneManagerModel
    {
        SysUser User { get; }
        Zone Record { get; set; }
        IList<Zone> EntityList { get; set; }       
        IList<Location> LocationList { get; set; }
        IList<Bin> SubEntityList { get; set; } 
        IList<ZoneBinRelation> AllowedList { get; set; }

        IList<UserByRol> PickerList { get; set; }
        IList<ZonePickerRelation> PickerListReg { get; set; }

        IList<ClassEntity> ClassEntityList { get; set; }
        IList<Object> CriteriaList { get; set; }
        IList<Object> CriteriaListReg { get; set; }

    }

    public class ZoneManagerModel : BusinessEntityBase, IZoneManagerModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<Zone> entitylist;
        public IList<Zone> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }


        private IList<Bin> subentitylist;
        public IList<Bin> SubEntityList
        {
            get { return subentitylist; }
            set
            {
                subentitylist = value;
                OnPropertyChanged("SubEntityList");
            }
        }
        
        private Zone record;
        public Zone Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<Location> locationlist;
        public IList<Location> LocationList
        {
            get { return locationlist; }
            set
            {
                locationlist = value;
                OnPropertyChanged("LocationList");
            }
        }

        private IList<ZoneBinRelation> allowedList;
        public IList<ZoneBinRelation> AllowedList
        {
            get { return allowedList; }
            set
            {
                allowedList = value;
                OnPropertyChanged("AllowedList");
            }
        }

        private IList<UserByRol> pickerList;
        public IList<UserByRol> PickerList
        {
            get
            { return pickerList; }
            set
            {
                pickerList = value;
                OnPropertyChanged("PickerList");
            }
        }

        private IList<ZonePickerRelation> pickerListReg;
        public IList<ZonePickerRelation> PickerListReg
        {
            get
            { return pickerListReg; }
            set
            {
                pickerListReg = value;
                OnPropertyChanged("PickerListReg");
            }
        }

        private IList<ClassEntity> classEntityList;
        public IList<ClassEntity> ClassEntityList
        {
            get { return classEntityList; }
            set
            {
                classEntityList = value;
                OnPropertyChanged("ClassEntityList");
            }
        }

        private IList<object> criteriaList;
        public IList<object> CriteriaList
        {
            get
            { return criteriaList; }
            set
            {
                criteriaList = value;
                OnPropertyChanged("CriteriaList");
            }
        }

        private IList<object> criteriaListReg;
        public IList<object> CriteriaListReg
        {
            get
            { return criteriaListReg; }
            set
            {
                criteriaListReg = value;
                OnPropertyChanged("CriteriaListReg");
            }
        }
    }
}
