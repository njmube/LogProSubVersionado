using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    //[DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class KitAssemblyFormula : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        //[DataMember]
        //public virtual Product Product { get; set; }
        [DataMember]
        public virtual KitAssembly KitAssembly { get; set; }
        [DataMember]
        public virtual Int32 Ord { get; set; }
        [DataMember]
        public virtual Product Component { get; set; }
        [DataMember]
        public virtual Double FormulaQty { get; set; }
        [DataMember]
        public virtual Double FormulaPercent { get; set; }
        [DataMember]
        public virtual Double ScrapPercent { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; }
        [DataMember]
        public virtual DateTime? EfectiveDate { get; set; }
        [DataMember]
        public virtual DateTime? ObsoleteDate { get; set; }
        
        //Permite hacer calculos cuando se esten manipulando los assemblies
        [DataMember]        
        public virtual Double BalanceQty { get; set; }

        [DataMember]
        public virtual Product DirectProduct { get; set; }
        

    }
}
