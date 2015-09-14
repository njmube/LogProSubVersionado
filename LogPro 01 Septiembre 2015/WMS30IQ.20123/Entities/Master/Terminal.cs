// File:    Terminal.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:57:21
// Purpose: Definition of Class Terminal

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true

    public class Terminal : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 TerminalID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        //[DataMember]
        //public virtual Company Company { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Terminal castObj = (Terminal)obj;
            return (castObj != null) &&
                (this.TerminalID == castObj.TerminalID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.TerminalID.GetHashCode();
        }
    }
}