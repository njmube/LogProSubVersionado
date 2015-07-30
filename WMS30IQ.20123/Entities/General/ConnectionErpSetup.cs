using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General", IsReference = true)] 
    public class ConnectionErpSetup
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Int32 ConnectionTypeID { get; set; }

        [DataMember]
        public virtual String EntityCode { get; set; }

        [DataMember]
        public virtual String QueryString { get; set; }

        //[DataMember]
        //public virtual String ReturnQueryString { get; set; }

        public virtual Int16 EntityType { get; set; } //References = 1, Documents = 2


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ConnectionErpSetup castObj = (ConnectionErpSetup)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }

}

