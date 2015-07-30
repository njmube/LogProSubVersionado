using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General", IsReference = true)] 
    public class ConnectionType
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual Boolean? IsEditable { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ConnectionType castObj = (ConnectionType)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }

}

