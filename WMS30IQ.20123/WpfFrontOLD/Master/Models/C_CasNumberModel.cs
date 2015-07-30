using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IC_CasNumberModel
    {
        C_CasNumber Record { get; set; }
        IList<C_CasNumber> EntityList { get; set; }

    }

    public class C_CasNumberModel: BusinessEntityBase, IC_CasNumberModel
    {

        private IList<C_CasNumber> entitylist;
        public IList<C_CasNumber> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private C_CasNumber record;
        public C_CasNumber Record
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
