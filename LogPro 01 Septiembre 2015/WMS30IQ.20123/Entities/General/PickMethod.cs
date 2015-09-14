// File:    MetaStatus.cs
// File:    MetaStatus.cs
// Author:  jairomurillo

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)]
    [Serializable]
    public class PickMethod 
    {
        [DataMember]
        public virtual Int16 MethodID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Boolean? Active { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            PickMethod castObj = (PickMethod)obj;
            return (castObj != null) &&
                (this.MethodID == castObj.MethodID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MethodID.GetHashCode();
        }
    }
}