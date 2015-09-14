using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IReceivingCubeModel
    {
        //IList<Label> LabelList { get; set; }
    }

    public class ReceivingCubeModel: BusinessEntityBase, IReceivingCubeModel
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
