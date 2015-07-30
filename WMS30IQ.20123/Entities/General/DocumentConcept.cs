using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Trace;


namespace Entities.General
{

    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    public class DocumentConcept
    {
        [DataMember]
        public virtual Int16 DocConceptID { get; set; }
        [DataMember]
        public virtual DocumentClass DocClass { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        //public virtual IList<Document> Documents { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentConcept castObj = (DocumentConcept)obj;
            return (castObj != null) &&
                (this.DocConceptID == castObj.DocConceptID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.DocConceptID.GetHashCode();
        }
    }
}