using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;

//Tipos de cuenta : Customer, vendor, Delivery etc. //22wms89

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //)] //, IsReference=true)]
    public class AccountType 
    {

        [DataMember]
        public virtual Int16 AccountTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            AccountType castObj = (AccountType)obj;
            return (castObj != null) &&
                (this.AccountTypeID == castObj.AccountTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.AccountTypeID.GetHashCode();
        }
       
    }
}
