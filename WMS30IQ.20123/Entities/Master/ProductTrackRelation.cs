using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]

    public class ProductTrackRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Product Product { get; set; }

        //Label.SerialNumber, Label.LotCode, Label.UserDef1 ...
        [DataMember]
        public virtual TrackOption TrackOption { get; set; } //Este y producto hacen la llave unica.
        
        [DataMember]
        public virtual String DisplayName { get; set; }

        [DataMember]
        public virtual Boolean? IsUnique { get; set; }

        [DataMember]
        public virtual Boolean? IsRequired { get; set; }


        [DataMember]
        public virtual String TempValue { get; set; }

        [DataMember]
        public virtual String TempQty { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ProductTrackRelation castObj = (ProductTrackRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
