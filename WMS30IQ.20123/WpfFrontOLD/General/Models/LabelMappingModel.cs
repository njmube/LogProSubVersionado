using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ILabelMappingModel
    {
        LabelMapping Record { get; set; }
        IList<LabelMapping> EntityList { get; set; }
        IList<DocumentType> LabelTypeList { get; set; }
        IList<Account> AccountList { get; set; }

    }

    public class LabelMappingModel: BusinessEntityBase, ILabelMappingModel
    {

        private IList<LabelMapping> entitylist;
        public IList<LabelMapping> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private LabelMapping record;
        public LabelMapping Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<DocumentType> labelTypeList;
        public IList<DocumentType> LabelTypeList
        {
            get { return labelTypeList; }
            set 
            {
                labelTypeList = value;
                OnPropertyChanged("LabelTypeList");
            }
        }

        private IList<Account> accountList;
        public IList<Account> AccountList
        {
            get { return accountList; }
            set
            {
                accountList = value;
                OnPropertyChanged("AccountList");
            }
        }

    }
}
