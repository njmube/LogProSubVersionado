using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Trace
{

    [DataContract(Namespace = "Entities.Trace", IsReference=true)]
    public class NodeRoute : Profile.Auditing
    {
        [DataMember]
        public virtual Int16 RowID { get; set; }
        [DataMember]
        public virtual Node CurNode { get; set; }
        [DataMember]
        public virtual Node NextNode { get; set; }
        [DataMember]
        public virtual Int16 Sequence { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            NodeRoute castObj = (NodeRoute)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
