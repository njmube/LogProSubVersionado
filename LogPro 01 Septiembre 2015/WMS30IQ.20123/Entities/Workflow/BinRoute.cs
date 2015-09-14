using System;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;


namespace Entities.Workflow
{

    [Serializable]
    [DataContract(Namespace = "Entities.Workflow", IsReference = true)]

    public class BinRoute : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Location LocationFrom { get; set; } // Origen

        [DataMember]
        public virtual Bin BinFrom { get; set; }

        [DataMember]
        public virtual Location LocationTo { get; set; }

        [DataMember]
        public virtual Bin BinTo { get; set; }

        [DataMember]
        public virtual Boolean? RequireData { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            BinRoute castObj = (BinRoute)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
