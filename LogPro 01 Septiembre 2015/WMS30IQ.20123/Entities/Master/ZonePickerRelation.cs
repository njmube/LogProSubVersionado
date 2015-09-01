using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Profile;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master")] //, IsReference = true
    public class ZonePickerRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Zone Zone { get; set; }
        [DataMember]
        public virtual SysUser Picker {get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ZonePickerRelation castObj = (ZonePickerRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
