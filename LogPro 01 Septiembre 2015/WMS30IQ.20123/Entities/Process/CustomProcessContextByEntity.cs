using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;

namespace Entities.Process
{
    //Especificacion del contexto a una entidad en particular
    public class CustomProcessContextByEntity: CustomProcessContext
    {
        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual Int32 EntityRowID { get; set; }
 
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessContextByEntity castObj = (CustomProcessContextByEntity)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
