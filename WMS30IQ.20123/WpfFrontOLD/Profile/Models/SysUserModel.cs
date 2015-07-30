using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ISysUserModel
    {
        SysUser Record { get; set; }
        IList<SysUser> EntityList { get; set; }
        IList<UserByRol> UserRolList { get; set; }
        IList<Location> LocationList { get; set; }
        IList<Rol> ListRol { get; set; }
    
    }

    public class SysUserModel: BusinessEntityBase, ISysUserModel
    {

        private IList<SysUser> entitylist;
        public IList<SysUser> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private SysUser record;
        public SysUser Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private IList<Location> _location;
        public IList<Location> LocationList
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged("LocationList");
            }
        }

        private IList<Rol> _ListRol;
        public IList<Rol> ListRol
        {
            get { return _ListRol; }
            set
            {
                _ListRol = value;
                OnPropertyChanged("ListRol");
            }
        }


        private IList<UserByRol> _UserRolList;
        public IList<UserByRol> UserRolList
        {
            get { return _UserRolList; }
            set
            {
                _UserRolList = value;
                OnPropertyChanged("UserRolList");
            }
        }
    }
}
