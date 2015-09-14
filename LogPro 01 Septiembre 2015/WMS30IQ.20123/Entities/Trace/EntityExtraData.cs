using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace", IsReference = true)]
    public class EntityExtraData : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual Int32 EntityRowID { get; set; }

        [DataMember]
        public virtual String XmlData { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            EntityExtraData castObj = (EntityExtraData)obj;
            return (castObj != null) &&
               (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
