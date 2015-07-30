// File:    ContactEntityAsociation.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:26
// Purpose: Definition of Class ContactEntityAsociation

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master")]

    public class ContactEntityRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Contact Contact { get; set; }
        [DataMember]
        public virtual ClassEntity ClassEntity { get; set; }
        [DataMember]
        public virtual Int32 EntityRowID { get; set; }
        [DataMember]
        public virtual ContactPosition ContactPosition { get; set; }
        [DataMember]
        public virtual Boolean? IsMain { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ContactEntityRelation castObj = (ContactEntityRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}