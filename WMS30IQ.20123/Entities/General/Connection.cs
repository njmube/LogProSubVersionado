using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General")] 
    public class Connection : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ConnectionID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual ConnectionType ConnectionType { get; set; }
        [DataMember]
        public virtual String  CnnString { get; set; }
        [DataMember]
        public virtual String UserName { get; set; }
        [DataMember]
        public virtual String Password { get; set; }
        [DataMember]
        public virtual String Domain { get; set; }
        [DataMember]
        public virtual String UserDef { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Connection castObj = (Connection)obj;
            return (castObj != null) &&
                (this.ConnectionID == castObj.ConnectionID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ConnectionID.GetHashCode();
        }
    }
}
