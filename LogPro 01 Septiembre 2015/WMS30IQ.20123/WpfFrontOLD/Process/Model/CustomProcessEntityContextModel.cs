using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ICustomProcessContextByEntityModel
    {
        CustomProcessContextByEntity Record { get; set; }
        IList<CustomProcessContextByEntity> EntityList { get; set; }

    }

    public class CustomProcessContextByEntityModel: BusinessEntityBase, ICustomProcessContextByEntityModel
    {

        private IList<CustomProcessContextByEntity> entitylist;
        public IList<CustomProcessContextByEntity> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcessContextByEntity record;
        public CustomProcessContextByEntity Record
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
