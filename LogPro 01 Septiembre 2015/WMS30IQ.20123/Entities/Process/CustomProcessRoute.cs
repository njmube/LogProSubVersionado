using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;
using Entities.Master;

namespace Entities.Process
{
    public class CustomProcessRoute: Auditing
    {
        [DataMember]
        public virtual Int32 RouteID { get; set; }

        [DataMember]
        public virtual Company Company { get; set; }

        [DataMember]
        public virtual DocumentType ProcessType { get; set; }  //Quality, Inventory

        [DataMember]
        public virtual CustomProcess ProcessFrom { get; set; }

        [DataMember]
        public virtual CustomProcess ProcessTo { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessRoute castObj = (CustomProcessRoute)obj;
            return (castObj != null) &&
                (this.RouteID == castObj.RouteID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RouteID.GetHashCode();
        }

    }
}
