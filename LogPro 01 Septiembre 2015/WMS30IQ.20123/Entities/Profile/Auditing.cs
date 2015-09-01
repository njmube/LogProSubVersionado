using System;
using System.Runtime.Serialization;

namespace Entities.Profile
{
    [Serializable]
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //,IsReference=true
    public class Auditing
    {
        [DataMember]
        public virtual String CreatedBy { get; set; }
        [DataMember]
        public virtual DateTime? CreationDate { get; set; }
        [DataMember]
        public virtual String CreTerminal { get; set; }
        [DataMember]
        public virtual String ModifiedBy { get; set; }
        [DataMember]
        public virtual DateTime? ModDate { get; set; }
        //[DataMember]
        public virtual String ModTerminal { get; set; }

    }
}
