using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    /// <summary>
    /// Permite la relacion entre zonas y determinada caracteristica o informacion de producto, cliente, vendor
    /// Tipo de documento entre otros
    /// </summary>
    [DataContract(Namespace = "Entities.Master")] 
    public class ZoneEntityRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Zone Zone { get; set; }
        [DataMember]
        public virtual ClassEntity Entity { get; set; }
        [DataMember]
        public virtual Int32 EntityRowID { get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }
        [DataMember]
        public virtual Boolean? Forced { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ZoneEntityRelation castObj = (ZoneEntityRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
