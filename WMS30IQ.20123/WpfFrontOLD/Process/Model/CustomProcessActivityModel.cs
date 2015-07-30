using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Linq;


namespace WpfFront.Models
{
    public interface ICustomProcessActivityModel
    {
        CustomProcessActivity Record { get; set; }
        IList<CustomProcessActivity> EntityList { get; set; }
        IList<Status> StatusList { get; }
        IList<DocumentType> TypeList { get; }
    }

    public class CustomProcessActivityModel: BusinessEntityBase, ICustomProcessActivityModel
    {

        public IList<Status> StatusList { get { return App.EntityStatusList; } }

        public IList<DocumentType> TypeList { get { return App.DocTypeList.Where(f => f.DocClass.DocClassID == SDocClass.Process).ToList(); } }


        private IList<CustomProcessActivity> entitylist;
        public IList<CustomProcessActivity> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcessActivity record;
        public CustomProcessActivity Record
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
