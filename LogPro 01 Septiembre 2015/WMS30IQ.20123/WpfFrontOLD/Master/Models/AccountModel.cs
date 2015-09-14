using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IAccountModel
    {
        Account Record { get; set; }
        IList<Account> EntityList { get; set; }
        AccountAddress RecordAddress { get; set; }
        //IList<AccountAddress> AccountAddressList { get; set; }

    }

    public class AccountModel: BusinessEntityBase, IAccountModel
    {

        private IList<Account> entitylist;
        public IList<Account> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Account record;
        public Account Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private AccountAddress _RecordAddress;
        public AccountAddress RecordAddress
        {
            get { return _RecordAddress; }
            set
            {
                _RecordAddress = value;
                OnPropertyChanged("RecordAddress");
            }
        }

        //private IList<AccountAddress> _AccountAddressList;
        //public IList<AccountAddress> AccountAddressList
        //{
        //    get { return _AccountAddressList; }
        //    set
        //    {
        //        _AccountAddressList = value;
        //        OnPropertyChanged("AccountAddressList");
        //    }
        //}

    }
}
