using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //)] //, IsReference=true)]
    public class Rol
    {
        [DataMember]
        public virtual Int16 RolID { get; set; }
        [DataMember]
        public virtual String RolCode { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual Boolean? IsMultiLocation { get; set; }

        //[DataMember]
        public virtual IList<MenuOptionByRol> MenuOptions { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Rol castObj = (Rol)obj;
            return (castObj != null) &&
                (this.RolID == castObj.RolID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RolID.GetHashCode();
        }
    }
}
