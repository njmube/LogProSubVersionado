using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IShippingMethodModel
    {
        ShippingMethod Record { get; set; }
        IList<ShippingMethod> EntityList { get; set; }

    }

    public class ShippingMethodModel: BusinessEntityBase, IShippingMethodModel
    {

        private IList<ShippingMethod> entitylist;
        public IList<ShippingMethod> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private ShippingMethod record;
        public ShippingMethod Record
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
