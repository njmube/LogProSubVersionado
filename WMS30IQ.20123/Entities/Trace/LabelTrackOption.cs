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


namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")] 
    public class LabelTrackOption : Auditing
    {
        [DataMember] 
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Label Label { get; set; }
        [DataMember] 
        public virtual TrackOption TrackOption { get; set; }
        [DataMember] 
        public virtual String TrackValue { get; set; }
 

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            LabelTrackOption castObj = (LabelTrackOption)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
