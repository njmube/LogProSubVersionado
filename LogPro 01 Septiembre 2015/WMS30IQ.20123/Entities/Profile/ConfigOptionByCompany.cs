using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile")]
    public class ConfigOptionByCompany : Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual ConfigOption ConfigOption { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual String Value { get; set; }

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ConfigOptionByCompany castObj = (ConfigOptionByCompany)obj;
            return (castObj != null) && (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
