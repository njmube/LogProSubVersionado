using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.Trace
{

    [DataContract(Namespace = "Entities.Trace", IsReference = true)]
    public class Node : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 NodeID { get; set; }
        //[DataMember]
        //public virtual Location Location { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Boolean? IsBasic { get; set; } //si es uno de los nodos principales del sistema
        [DataMember]
        public virtual Int16 NodeSeq { get; set; }
        [DataMember]
        public virtual Boolean? RequireDocID { get; set; }

        ////[DataMember]
        //public virtual IList<NodeExtension> NodeExtensions { get; set; }

        ////[DataMember]
        //public virtual IList<Label> Labels { get; set; }
        ////[DataMember]
        //public virtual IList<LabelHistory> LabelsHistory { get; set; }
        ////[DataMember]
        //public virtual IList<NodeTrace> NodeTraces { get; set; }
        ////[DataMember]
        //public virtual IList<NodeTraceHistory> NodeTracesHistory { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Node castObj = (Node)obj;
            return (castObj != null) &&
                (this.NodeID == castObj.NodeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.NodeID.GetHashCode();
        }
    }
}
