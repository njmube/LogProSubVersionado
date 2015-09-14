using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;


namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class MType 
    {
        [DataMember]
        public virtual Int32 MetaTypeID { get; set; }
        [DataMember]
        public virtual String Code { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MType castObj = (MType)obj;
            return (castObj != null) &&
                (this.MetaTypeID == castObj.MetaTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MetaTypeID.GetHashCode();
        }
    }
}
