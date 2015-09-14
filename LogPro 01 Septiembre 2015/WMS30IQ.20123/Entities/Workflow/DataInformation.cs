using System;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;
using Entities.General;


namespace Entities.Workflow
{

    [Serializable]
    [DataContract(Namespace = "Entities.Workflow", IsReference = true)]

    public class DataInformation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; } //IsHeader=true->document, IsHeader=false->label

        [DataMember]
        public virtual Int32 EntityRowID { get; set; }

        [DataMember]
        public virtual String XmlData { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DataInformation castObj = (DataInformation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
