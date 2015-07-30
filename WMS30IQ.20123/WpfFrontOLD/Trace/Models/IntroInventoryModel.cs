using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IIntroInventoryModel
    {
        //IList<Label> LabelList { get; set; }
    }

    public class IntroInventoryModel: BusinessEntityBase, IIntroInventoryModel
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
