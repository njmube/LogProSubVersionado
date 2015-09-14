using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General")]
    public class LabelTemplate : Profile.Auditing
    {

        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual DocumentType LabelType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Header { get; set; }
        [DataMember]
        public virtual String Body { get; set; }
        [DataMember]
        public virtual String DetailColumns { get; set; }
        [DataMember]
        public virtual String DetailQuery { get; set; }
        [DataMember]
        public virtual String Empty { get; set; }
        [DataMember]
        public virtual Boolean? PrintEmptyLabel { get; set; }
        [DataMember]
        public virtual Boolean? IsPL { get; set; } //dice si el template debe imprimirse a bajo nivel. Y los datos quedan en el PLTemplate
        [DataMember]
        public virtual String PLTemplate { get; set; }
        [DataMember]
        public virtual String PLHeader { get; set; }
        //[DataMember]
        //public virtual DocumentType DocumentType { get; set; }
        [DataMember]
        public virtual Connection DefPrinter { get; set; }
        [DataMember]
        public virtual Boolean? IsUnique { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            LabelTemplate castObj = (LabelTemplate)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }


        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
