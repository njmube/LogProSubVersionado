// File:    Zone.cs
// Author:  jairomurillo
// Created: jueves, 21 de agosto de 2008 10:20:56
// Purpose: Definition of Class Zone

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true
    public class Zone : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ZoneID { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        [DataMember]
        public virtual Contract Contract { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual String StoreConditions { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Zone FatherZone { get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }

        [DataMember]
        public virtual Boolean? IsDefault { get; set; }

//        [DataMember]
        public virtual IList<ZoneBinRelation> Bins { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Zone castObj = (Zone)obj;
            return (castObj != null) &&
                (this.ZoneID == castObj.ZoneID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ZoneID.GetHashCode();
        }
    }
}