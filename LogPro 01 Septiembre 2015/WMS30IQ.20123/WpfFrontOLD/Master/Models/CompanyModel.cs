using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ICompanyModel
    {
        SysUser User { get; }
        Company Record { get; set; }
        IList<Company> EntityList { get; set; }
        IList<Status> Status { get; set; }
        IList<Connection> ErpConn { get; set; }
        ImageEntityRelation Logo { get; set; } 
    }

    public class CompanyModel: BusinessEntityBase, ICompanyModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<Company> entitylist;
        public IList<Company> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private Company record;
        public Company Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<Status> _Status;
        public IList<Status> Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }



        private IList<Connection> _ErpConn;
        public IList<Connection> ErpConn
        {
            get { return _ErpConn; }
            set
            {
                _ErpConn = value;
                OnPropertyChanged("ErpConn");
            }
        }

        private ImageEntityRelation logo;
        public ImageEntityRelation Logo
        {
            get { return logo; }
            set
            {
                logo = value;
                OnPropertyChanged("Logo");
            }
        }


        public Connection ErpConnection
        {
            get { return record.ErpConnection; } 
            
            set {
                if (record.ErpConnection == value) return;
                record.ErpConnection = value;

                OnPropertyChanged("Record"); 
            } 
        }


    }
}
