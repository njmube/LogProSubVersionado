using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class KitAssembly: Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Product Product { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; }
        [DataMember]
        public virtual Int16 AsmType { get; set; }  //1-Kit, 2-Assembly
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Int16 Method { get; set; }
        [DataMember]
        public virtual DateTime? EfectiveDate { get; set; }
        [DataMember]
        public virtual DateTime? ObsoleteDate { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }

        [DataMember]
        public virtual IList<KitAssemblyFormula> ProductFormula { get; set; }
    }
}
