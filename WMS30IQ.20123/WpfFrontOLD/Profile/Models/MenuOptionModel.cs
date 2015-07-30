using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IMenuOptionModel
    {
        SysUser User { get; }
        MenuOption Record { get; set; }
        IList<MenuOption> EntityList { get; set; }
        IList<MenuOptionType> MenuOptionTypeList { get; set; }
        IList<OptionType> OptionType { get; set; }
        MenuOptionExtension RecordExt { get; set; }

    }

    public class MenuOptionModel: BusinessEntityBase, IMenuOptionModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<MenuOption> entitylist;
        public IList<MenuOption> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MenuOption record;
        public MenuOption Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private MenuOptionExtension recordext;
        public MenuOptionExtension RecordExt
        {
            get { return recordext; }
            set
            {
                recordext = value;
                OnPropertyChanged("RecordExt");
            }
        }


        private IList<MenuOptionType> _MenuOptionTypeList;
        public IList<MenuOptionType> MenuOptionTypeList
        {
            get { return _MenuOptionTypeList; }
            set
            {
                _MenuOptionTypeList = value;
                OnPropertyChanged("MenuOptionTypeList");
            }
        }

        private IList<OptionType> _OptionType;
        public IList<OptionType> OptionType
        {
            get { return _OptionType; }
            set
            {
                _OptionType = value;
                OnPropertyChanged("OptionType");
            }
        }

    }
}
