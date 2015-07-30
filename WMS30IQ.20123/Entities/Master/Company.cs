
// File:    Company.cs
// Author:  jairomurillo
// Created: jueves, 21 de agosto de 2008 10:20:45
// Purpose: Definition of Class Company

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;
using Entities.Trace;


namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true
    public class Company : Profile.Auditing
    {

        [DataMember]
        public virtual Int16 CompanyID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String Email { get; set; }
        [DataMember]
        public virtual String ContactPerson { get; set; }
        //[DataMember]
        public virtual String Website { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Boolean? IsDefault { get; set; }
        [DataMember]
        public virtual Connection ErpConnection { get; set; }
        [DataMember]
        public virtual DateTime? LastUpdate { get; set; }
        //[DataMember]
        public virtual String AddressLine1 { get; set; }
        //[DataMember]
        public virtual String AddressLine2 { get; set; }
        //[DataMember]
        public virtual String AddressLine3 { get; set; }
        //[DataMember]
        public virtual String City { get; set; }
        //[DataMember]
        public virtual String State { get; set; }
        //[DataMember]
        public virtual String ZipCode { get; set; }
        //[DataMember]
        public virtual String Country { get; set; }
        //[DataMember]
        public virtual String WebURL { get; set; }
        //[DataMember]
        public virtual String ResourcesDiskPath { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Company castObj = (Company)obj;
            return (castObj != null) &&
                (this.CompanyID == castObj.CompanyID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.CompanyID.GetHashCode();
        }

    }
}
