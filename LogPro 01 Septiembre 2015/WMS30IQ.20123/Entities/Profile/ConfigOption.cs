using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //, IsReference = true
    public class ConfigOption : Auditing
    {
        [DataMember]
        public virtual Int32 ConfigOptionID { get; set; }
        [DataMember]
        public virtual ConfigType ConfigType { get; set; }

        [DataMember]
        public virtual String Code { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String DefValue { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual DataType DataType { get; set; }
        [DataMember]
        public virtual Int32 Length { get; set; }
        [DataMember]
        public virtual Int16 NumOrder { get; set; }
        [DataMember]
        public virtual Boolean? IsDevice { get; set; }
        [DataMember]
        public virtual Boolean? IsAdmin { get; set; }
        //[DataMember]
        //public virtual IList<ConfigOptionByCompany> OptionByCompany { get; set; }

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ConfigOption castObj = (ConfigOption)obj;
            return (castObj != null) &&
                (this.ConfigOptionID == castObj.ConfigOptionID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ConfigOptionID.GetHashCode();
        }
    }
}
