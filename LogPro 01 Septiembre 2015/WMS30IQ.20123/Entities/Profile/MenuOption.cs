using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //, IsReference = true
    public class MenuOption : Auditing
    {
        [DataMember]
        public virtual Int32 MenuOptionID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Url { get; set; }
        [DataMember]
        public virtual String Icon { get; set; }
        [DataMember]
        public virtual MenuOptionType MenuOptionType { get; set; }
        [DataMember]
        public virtual Int16 NumOrder { get; set; }
        [DataMember]
        public virtual Boolean? Active { get; set; }
        [DataMember]
        public virtual OptionType OptionType { get; set; }

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MenuOption castObj = (MenuOption)obj;
            return (castObj != null) &&
                (this.MenuOptionID == castObj.MenuOptionID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MenuOptionID.GetHashCode();
        }
    }
}
