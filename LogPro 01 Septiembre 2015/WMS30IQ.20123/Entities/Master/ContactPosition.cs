// File:    ContactPosition.cs
// Author:  Mauricio Escobar
// Created: Lunes, 1 de Septimebre de 2008 14:23:26
// Purpose: Definition of Class ContactPosition
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference=true)]
    public class ContactPosition : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ContactPositionID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        //public virtual Status Status { get; set; }
        //[DataMember]
        //public virtual IList<ContactEntityRelation> ContactEntityRelations { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ContactPosition castObj = (ContactPosition)obj;
            return (castObj != null) &&
                (this.ContactPositionID == castObj.ContactPositionID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ContactPositionID.GetHashCode();
        }

    }
}
