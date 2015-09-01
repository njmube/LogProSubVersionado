using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IBasicProcessModel
    {
       IList<ShowData> OptionList { get;}
    }

    public class BasicProcessModel: BusinessEntityBase, IBasicProcessModel
    {
        private IList<ShowData> list = new List<ShowData> {
        
            new ShowData { DataKey = "System Process", DataValue = "1" },
            new ShowData { DataKey = "Process Activities", DataValue = "2" },
            new ShowData { DataKey = "Process Contexts", DataValue = "3" },
            new ShowData { DataKey = "Process Routes", DataValue = "6" },
            new ShowData { DataKey = "Activity Transitions", DataValue = "4" },
            //new ShowData { DataKey = "Contexts By Entity", DataValue = "5" },

       
        };        

        public IList<ShowData> OptionList
        {
            get { return list; }
        }

    }
}
