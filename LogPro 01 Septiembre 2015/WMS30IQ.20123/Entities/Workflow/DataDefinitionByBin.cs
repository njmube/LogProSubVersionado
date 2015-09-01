using System;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;


namespace Entities.Workflow
{

    [Serializable]
    [DataContract(Namespace = "Entities.Workflow", IsReference = true)]

    public class DataDefinitionByBin : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Bin Bin { get; set; }

        [DataMember]
        public virtual Boolean? EsEditable { get; set; }

        [DataMember]
        public virtual DataDefinition DataDefinition { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DataDefinitionByBin castObj = (DataDefinitionByBin)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
