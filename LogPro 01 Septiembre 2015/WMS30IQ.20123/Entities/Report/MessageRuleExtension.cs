using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Report
{
    [DataContract(Namespace = "Entities.Report", IsReference = true)] //, IsReference = true
    public class MessageRuleExtension
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual MessageRuleByCompany Rule { get; set; }
        [DataMember]
        public virtual String Custom1 { get; set; } //table comment
        [DataMember]
        public virtual String Custom2 { get; set; } //Query
        [DataMember]
        public virtual String Custom3 { get; set; } //Query Columns

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MessageRuleExtension castObj = (MessageRuleExtension)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
