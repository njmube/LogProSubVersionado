using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class ImageEntityRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Byte[] Image { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual Int32 EntityRowID { get; set; }

        [DataMember]
        public virtual String ImageName { get; set; }

        [DataMember]
        public virtual Connection FileType { get; set; }

        [DataMember]
        public virtual String FullDesc
        {
            get
            {
                try { return ImageName + ", Type: " + FileType.Name; }
                catch { return ImageName; }
            }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ImageEntityRelation castObj = (ImageEntityRelation)obj;
            return (castObj != null) &&
               (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
