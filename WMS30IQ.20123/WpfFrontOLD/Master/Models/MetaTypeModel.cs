using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IMetaTypeModel
    {
        MType Record { get; set; }
        IList<MType> EntityList { get; set; }
        IList<MMaster> DetailList { get; set; }
        MMaster DetailRecord { get; set; }
    }

    public class MetaTypeModel: BusinessEntityBase, IMetaTypeModel
    {

        private IList<MType> entitylist;
        public IList<MType> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MType record;
        public MType Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<MMaster> detailList;
        public IList<MMaster> DetailList
        {
            get { return detailList; }
            set
            {
                detailList = value;
                OnPropertyChanged("DetailList");
            }
        }

        private MMaster detailRecord;
        public MMaster DetailRecord
        {
            get { return detailRecord; }
            set
            {
                detailRecord = value;
                OnPropertyChanged("DetailRecord");
            }
        }
    }
}
