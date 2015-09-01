// File:    Unit.cs
// Author:  jairomurillo
// Created: viernes, 29 de agosto de 2008 8:05:49
// Purpose: Definition of Class Unit

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true
    public class Unit : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 UnitID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String ErpCodeGroup { get; set; }
        //[DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Double BaseAmount { get; set; }  
        //[DataMember]
        public virtual MeasureUnit MeasureUnit { get; set; }
        //[DataMember]
        public virtual Double MeasureQuantity { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Unit castObj = (Unit)obj;
            return (castObj != null) &&
                (this.UnitID == castObj.UnitID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.UnitID.GetHashCode();
        }

    }
}