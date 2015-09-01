using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IBasicSetupModel
    {
       IList<ShowData> OptionList { get;}
    }

    public class BasicSetupModel: BusinessEntityBase, IBasicSetupModel
    {
        private IList<ShowData> list = new List<ShowData> {
        
            new ShowData { DataKey = "Connections", DataValue = "1" },
            new ShowData { DataKey = "Document Numbers", DataValue = "2" },

            new ShowData { DataKey = "Document Concepts", DataValue = "9" },

            new ShowData { DataKey = "Tracking Options", DataValue = "3" },
            new ShowData { DataKey = "Picking Methods", DataValue = "4" },

            new ShowData { DataKey = "Message Rules", DataValue = "6" },
            new ShowData { DataKey = "Templates", DataValue = "7" },
            new ShowData { DataKey = "Template Fields", DataValue = "8" },

            new ShowData { DataKey = "Process Files", DataValue = "10" },

            //new ShowData { DataKey = "WareHouse Process", DataValue = "10" }

            new ShowData{ DataKey = "Shipping Method", DataValue = "11"},
        
        };        

        public IList<ShowData> OptionList
        {
            get { return list; }
        }

    }
}
