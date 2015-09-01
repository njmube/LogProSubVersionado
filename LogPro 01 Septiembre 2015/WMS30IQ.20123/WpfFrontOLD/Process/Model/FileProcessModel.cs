using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IFileProcessModel
    {

        IList<CustomProcess> EntityList { get; set; }
    }

    public class FileProcessModel: BusinessEntityBase, IFileProcessModel
    {

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



    }
}
