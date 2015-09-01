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
    public class CustomProcess: Auditing
    {

        [DataMember]
        public virtual Int32 ProcessID { get; set; }

        [DataMember]
        public virtual DocumentType ProcessType { get; set; }  //Quality, Inventory

        [DataMember]
        public virtual String Name { get; set; } 

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual Boolean? IsSystem { get; set; }

        [DataMember]
        public virtual Boolean? IsRouted { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual String BatchNo { get; set; }

        [DataMember]
        public virtual Connection Printer { get; set; }


        [DataMember]
        public virtual IList<CustomProcessContextByEntity> ProcessContext { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcess castObj = (CustomProcess)obj;
            return (castObj != null) &&
                (this.ProcessID == castObj.ProcessID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ProcessID.GetHashCode();
        }

    }
}
