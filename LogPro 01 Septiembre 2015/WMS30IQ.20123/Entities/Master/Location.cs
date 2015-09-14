// File:    Location.cs
// Author:  jairomurillo
// Created: jueves, 21 de agosto de 2008 10:20:54
// Purpose: Definition of Class Location

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;


namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true

    public class Location : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 LocationID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual Boolean? IsDefault { get; set; }
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
        public virtual String BatchNo { get; set; }
        //[DataMember]
        //public virtual IList<Zone> Zones { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Location castObj = (Location)obj;
            return (castObj != null) &&
                (this.LocationID == castObj.LocationID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.LocationID.GetHashCode();
        }

    }
}