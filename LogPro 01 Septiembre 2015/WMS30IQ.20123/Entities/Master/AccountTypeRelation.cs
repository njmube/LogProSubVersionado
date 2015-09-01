using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;


namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master")]
    public class AccountTypeRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Account Account { get; set; }
        [DataMember]
        public virtual AccountType AccountType { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            AccountTypeRelation castObj = (AccountTypeRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
