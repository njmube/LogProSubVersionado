using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Collections.Specialized;

namespace WpfFront.Models
{
    public interface IMessageRuleByCompanyModel
    {
        SysUser User { get; }
        MessageRuleByCompany Record { get; set; }
        IList<MessageRuleByCompany> EntityList { get; set; }
        IList<Company> CompanyList { get; }
        IList<ClassEntity> ClassEntityList { get; set; }
        IList<LabelTemplate> LabelTemplateList { get; set; }
    }

    public class MessageRuleByCompanyModel: BusinessEntityBase, IMessageRuleByCompanyModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<MessageRuleByCompany> entitylist;
        public IList<MessageRuleByCompany> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MessageRuleByCompany record;
        public MessageRuleByCompany Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        public IList<Company> CompanyList
        {
            get { return App.CompanyList; }
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

        private IList<LabelTemplate> labelTemplateList;
        public IList<LabelTemplate> LabelTemplateList
        {
            get { return labelTemplateList; }
            set 
            {
                labelTemplateList = value;
                OnPropertyChanged("LabelTemplateList");
            }
        }


        private StringDictionary _FreqType = null;
        public StringDictionary FreqType
        {
            //Dias (86400), Hour (3600), Minutes (60), Seconds (1)

            get
            {
                if (_FreqType == null)
                {
                    _FreqType = new StringDictionary();
                    //_FreqType.Add("1", "Seconds");
                    _FreqType.Add("60", "Minutes");
                    _FreqType.Add("3600", "Hours");
                    _FreqType.Add("86400", "Days");
                }

                return _FreqType;

            }
            set
            {
                _FreqType = value;
                OnPropertyChanged("FreqType");
            }
        }


    }
}
