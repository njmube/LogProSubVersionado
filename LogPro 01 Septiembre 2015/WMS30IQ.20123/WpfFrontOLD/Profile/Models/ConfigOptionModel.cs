using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IConfigOptionModel
    {
        SysUser User { get; }
        ConfigOptionByCompany Record { get; set; }
        IList<ConfigOptionByCompany> EntityList { get; set; }
        IList<ConfigType> TypeList { get; set; }
        ConfigOption CfgRecord { get; set; } 

    }

    public class ConfigOptionModel: BusinessEntityBase, IConfigOptionModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<ConfigOptionByCompany> entitylist;
        public IList<ConfigOptionByCompany> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private ConfigOptionByCompany record;
        public ConfigOptionByCompany Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private ConfigOption _CfgRecord;
        public ConfigOption CfgRecord
        {
            get { return _CfgRecord; }
            set
            {
                _CfgRecord = value;
                OnPropertyChanged("CfgRecord");
            }
        }



        private IList<ConfigType> typelist;
        public IList<ConfigType> TypeList
        {
            get { return typelist; }
            set
            {
                typelist = value;
                OnPropertyChanged("TypeList");
            }
        }

    }
}
