using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile")]
    public class MenuOptionByRol : Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual MenuOption MenuOption { get; set; }
        [DataMember]
        public virtual Rol Rol { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }

    }
}
