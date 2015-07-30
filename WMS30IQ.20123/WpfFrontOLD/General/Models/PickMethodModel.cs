using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IPickMethodModel
    {
        SysUser User { get; }
        PickMethod Record { get; set; }
        IList<PickMethod> EntityList { get; set; }

    }

    public class PickMethodModel: BusinessEntityBase, IPickMethodModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<PickMethod> entitylist;
        public IList<PickMethod> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private PickMethod record;
        public PickMethod Record
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
