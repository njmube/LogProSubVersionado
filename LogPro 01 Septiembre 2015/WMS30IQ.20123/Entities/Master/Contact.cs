// File:    Contact.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:25
// Purpose: Definition of Class Contact

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference= true)]

    public class Contact : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ContactID { get; set; }
        [DataMember]
        public virtual Account Account { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String IdNumber { get; set; } //Like NIT
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String Phone1 { get; set; }
        [DataMember]
        public virtual String Phone2 { get; set; }
        [DataMember]
        public virtual String Phone3 { get; set; }
        [DataMember]
        public virtual String Fax { get; set; }
        [DataMember]
        public virtual String Email { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Contact castObj = (Contact)obj;
            return (castObj != null) &&
                (this.ContactID == castObj.ContactID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ContactID.GetHashCode();
        }

    }
}