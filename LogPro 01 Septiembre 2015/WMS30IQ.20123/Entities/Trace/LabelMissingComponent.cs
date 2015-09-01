// File:    ClassEntity.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:23
// Purpose: Definition of Class ClassEntity
// Tipo de Master entity Clasess, Account, Product, Location

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.Profile;
using Entities.General;


namespace Entities.Trace
{
    //Si el label es de un Kit esta clase contiene los productos que le hacen falta.

    [DataContract(Namespace = "Entities.Trace")]
    public class LabelMissingComponent : Auditing
    {
        [DataMember] 
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Label FatherLabel { get; set; }
        [DataMember] 
        public virtual Product Component { get; set; }
        [DataMember] 
        public virtual Double Quantity { get; set; }
        [DataMember]
        public virtual String Notes { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;

            LabelMissingComponent castObj = (LabelMissingComponent)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
