// File:    MetaStatus.cs
// File:    MetaStatus.cs
// Author:  jairomurillo

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] 
    
    public class OptionType 
    {
        [DataMember]
        public virtual Int16 OpTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            OptionType castObj = (OptionType)obj;
            return (castObj != null) &&
                (this.OpTypeID == castObj.OpTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.OpTypeID.GetHashCode();
        }
    }
}