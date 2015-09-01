using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IInventoryTaskModel
    {
        //IList<Label> LabelList { get; set; }
    }

    public class InventoryTaskModel: BusinessEntityBase, IInventoryTaskModel
    {
                
        //private IList<Label> _LabelList;
        //public IList<Label> LabelList
        //{
        //    get
        //    { return _LabelList; }
        //    set
        //    {
        //        _LabelList = value;
        //        OnPropertyChanged("LabelList");
        //    }
        //}

    }
}
