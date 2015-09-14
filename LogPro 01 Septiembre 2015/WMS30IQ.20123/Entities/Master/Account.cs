// File:    Customer.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:00:57
// Purpose: Definition of Class Customer

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //)] //, IsReference=true)]

    public class Account : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 AccountID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }

        [DataMember]
        public virtual Contract FatherContract { get; set; }

        [DataMember]
        public virtual String AccountCode { get; set; }
        [DataMember]
        public virtual AccountType BaseType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Phone { get; set; }
        [DataMember]
        public virtual String ContactPerson { get; set; }
        [DataMember]
        public virtual String Email { get; set; }
        [DataMember]
        public virtual String WebSite { get; set; }
        [DataMember]
        public virtual String UserDefine1 { get; set; }
        [DataMember]
        public virtual String UserDefine2 { get; set; }
        [DataMember]
        public virtual String UserDefine3 { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }
        [DataMember]
        public virtual String FullDesc
        {
            get { return this.AccountCode.Trim() + ", " + this.Name; }
            set { }
        }

        //[DataMember]
        public virtual IList<AccountAddress> AccountAddresses { get; set; }

        //[DataMember]
        public virtual IList<AccountTypeRelation> AccountTypes { get; set; }

        //[DataMember]
        //public virtual IList<Vehicle> Vehicles { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Account castObj = (Account)obj;
            return (castObj != null) &&
                (this.AccountID == castObj.AccountID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.AccountID.GetHashCode();
        }


    }
}