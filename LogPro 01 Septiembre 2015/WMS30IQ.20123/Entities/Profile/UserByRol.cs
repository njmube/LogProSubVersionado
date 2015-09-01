using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)]
    public class UserByRol : Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual SysUser User { get; set; }
        [DataMember]
        public virtual Rol Rol { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        [DataMember]
        public virtual Boolean? IsDefault { get; set; }
        [DataMember]
        public virtual Contract Contract { get; set; }

    }
}
