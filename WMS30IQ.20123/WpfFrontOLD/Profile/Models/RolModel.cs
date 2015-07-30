using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IRolModel
    {
        Rol Record { get; set; }
        IList<Rol> EntityList { get; set; }
        IList<MenuOption> AvailablePermission { get; set; }
        IList<MenuOptionByRol> AssignPermission { get; set; }
        Company Company { get; }

    }

    public class RolModel: BusinessEntityBase, IRolModel
    {

        public Company Company { get { return App.curCompany; } }

        private IList<Rol> entitylist;
        public IList<Rol> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Rol record;
        public Rol Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<MenuOptionByRol> _AssignPermission;
        public IList<MenuOptionByRol> AssignPermission
        {
            get
            { return _AssignPermission; }
            set
            {
                _AssignPermission = value;
                OnPropertyChanged("AssignPermission");
            }
        }



        private IList<MenuOption> _AvailablePermission;
        public IList<MenuOption> AvailablePermission
        {
            get
            { return _AvailablePermission; }
            set
            {
                _AvailablePermission = value;
                OnPropertyChanged("AvailablePermission");
            }
        }


    }
}
