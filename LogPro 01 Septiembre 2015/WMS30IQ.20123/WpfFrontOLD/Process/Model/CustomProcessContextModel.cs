using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Linq;

namespace WpfFront.Models
{
    public interface ICustomProcessContextModel
    {
        CustomProcessContext  Record { get; set; }
        IList<CustomProcessContext > EntityList { get; set; }
        IList<Status> StatusList { get; }
        IList<DocumentType> TypeList { get; }

    }

    public class CustomProcessContextModel: BusinessEntityBase, ICustomProcessContextModel
    {
        public IList<Status> StatusList { get { return App.EntityStatusList; } }

        public IList<DocumentType> TypeList { get { return App.DocTypeList.Where(f => f.DocClass.DocClassID == SDocClass.Process).ToList(); } }

        private IList<CustomProcessContext > entitylist;
        public IList<CustomProcessContext > EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcessContext  record;
        public CustomProcessContext  Record
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
