using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Linq;

namespace WpfFront.Models
{
    public interface ICustomProcessModel
    {
        CustomProcess Record { get; set; }
        IList<CustomProcess> EntityList { get; set; }
        IList<Status> StatusList { get; }
        IList<DocumentType> TypeList { get; }
    }

    public class CustomProcessModel: BusinessEntityBase, ICustomProcessModel
    {

        public IList<DocumentType> TypeList { get { return App.DocTypeList.Where(f => f.DocClass.DocClassID == SDocClass.Process).ToList(); } }
        
        public IList<Status> StatusList { get { return App.EntityStatusList;  } }

        private IList<CustomProcess> entitylist;
        public IList<CustomProcess> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcess record;
        public CustomProcess Record
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
