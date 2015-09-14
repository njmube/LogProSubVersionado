using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Linq;

namespace WpfFront.Models
{
    public interface ICustomProcessRouteModel
    {
        CustomProcessRoute Record { get; set; }
        IList<CustomProcessRoute> EntityList { get; set; }
        IList<Company> CompanyList { get; }
        IList<Status> StatusList { get; }
        IList<DocumentType> TypeList { get; }
        IList<CustomProcess> ProcessFromList { get; set; }
        IList<CustomProcess> ProcessToList { get; set; }

    }

    public class CustomProcessRouteModel: BusinessEntityBase, ICustomProcessRouteModel
    {
        private IList<CustomProcess> _ProcessFromList;
        public IList<CustomProcess> ProcessFromList
        {

            get { return _ProcessFromList; }
            set
            {
                _ProcessFromList = value;
                OnPropertyChanged("ProcessFromList");
            }
        
        }

        private IList<CustomProcess> _ProcessToList;
        public IList<CustomProcess> ProcessToList
        {

            get { return _ProcessToList; }
            set
            {
                _ProcessToList = value;
                OnPropertyChanged("ProcessToList");
            }
        }



        public IList<Status> StatusList { get { return App.EntityStatusList; } }

        public IList<Company> CompanyList { get { return App.CompanyList; } }

        public IList<DocumentType> TypeList { 
            get { return App.DocTypeList.Where(f => f.DocClass.DocClassID == SDocClass.Process).ToList(); 
         } }



        private IList<CustomProcessRoute> entitylist;
        public IList<CustomProcessRoute> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcessRoute record;
        public CustomProcessRoute Record
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
