// File:    Bin.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:56
// Purpose: Definition of Class Bin

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]

    public class ShippingMethod : Profile.Auditing
    {  
        [DataMember]
        public virtual Int16 ShpMethodID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }   
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ShippingMethod castObj = (ShippingMethod)obj;
            return (castObj != null) &&
                (this.ShpMethodID == castObj.ShpMethodID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ShpMethodID.GetHashCode();
        }

    }
}