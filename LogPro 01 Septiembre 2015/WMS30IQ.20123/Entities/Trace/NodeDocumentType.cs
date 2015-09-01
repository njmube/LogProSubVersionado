//Asocia los documentos que pueden ser procesados en determinado Nodo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Trace
{

    [DataContract(Namespace = "Entities.Trace", IsReference = true)] //)] //, IsReference=true)]
    public class NodeDocumentType : Profile.Auditing
    {
        [DataMember]
        public virtual Int16 RowID { get; set; }
        [DataMember]
        public virtual Node Node { get; set; }
        [DataMember]
        public virtual DocumentType DocType { get; set; }

 
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            NodeDocumentType castObj = (NodeDocumentType)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
