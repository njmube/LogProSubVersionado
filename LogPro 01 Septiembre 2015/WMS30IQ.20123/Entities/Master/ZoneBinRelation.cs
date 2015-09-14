using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master")] 
    public class ZoneBinRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Zone Zone { get; set; }
        [DataMember]
        public virtual Bin Bin { get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }       
        [DataMember]
        public virtual Int16 BinType { get; set; } //Si es un bin de In/Out
        [DataMember]
        public virtual Double UnitCapacity { get; set; }
        [DataMember]
        public virtual Double MinUnitCapacity { get; set; }
        [DataMember]
        public virtual Double CurrentOcupancy { get; set; }

        [DataMember]
        public virtual String BinTypeDesc
        {
            get
            {
                if (this.BinType == 1) return "In only";
                if (this.BinType == 2) return "Out only";
                return "In/Out";
            }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ZoneBinRelation castObj = (ZoneBinRelation)obj;
            return (castObj != null) &&
               (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
