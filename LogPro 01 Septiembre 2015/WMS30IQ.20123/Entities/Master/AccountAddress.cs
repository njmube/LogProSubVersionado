// File:    CustomerAddress.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:17:04
// Purpose: Definition of Class CustomerAddress

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference=true)]

    public class AccountAddress : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 AddressID { get; set; }
        [DataMember]
        public virtual Account Account { get; set; }
         [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual Boolean? IsMain { get; set; }
        [DataMember]
        public virtual String AddressLine1 { get; set; }
        [DataMember]
        public virtual String AddressLine2 { get; set; }
        [DataMember]
        public virtual String AddressLine3 { get; set; }
        [DataMember]
        public virtual String City { get; set; }
        [DataMember]
        public virtual String State { get; set; }
        [DataMember]
        public virtual String ZipCode { get; set; }
        [DataMember]
        public virtual String Country { get; set; }
        [DataMember]
        public virtual String ContactPerson { get; set; }
        [DataMember]
        public virtual String Phone1 { get; set; }
        [DataMember]
        public virtual String Phone2 { get; set; }
        [DataMember]
        public virtual String Phone3 { get; set; }
        [DataMember]
        public virtual String Email { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }

        [DataMember]
        public virtual ShippingMethod ShpMethod { get; set; }

        [DataMember]
        public virtual String FullDesc { get {

            string fDesc = IsNull(this.Name);
            fDesc += IsNull(AddressLine1) + IsNull(AddressLine2) + IsNull(AddressLine3);
            fDesc += ", " + IsNull(City) + IsNull(State) + IsNull(ZipCode);
            fDesc += ", " + IsNull(Country);
            return fDesc;
        
        } set{} }


        public virtual string IsNull(string data) {
            if (string.IsNullOrEmpty(data))
                return "";
            else
                return data + " ";
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            AccountAddress castObj = (AccountAddress)obj;
            return (castObj != null) &&
                (this.AddressID == castObj.AddressID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.AddressID.GetHashCode();
        }

    }
}