// File:    MeasureUnit.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:59
// Purpose: Definition of Class Measure Unit Weight: Lbs, Kg, Ton, Length: Mtr. Cm, Volume: Onza, cc, mm, Capacity)

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    public class MeasureUnit : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 MeasureUnitID { get; set; }
        [DataMember]
        public virtual MeasureType MeasureType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MeasureUnit castObj = (MeasureUnit)obj;
            return (castObj != null) &&
                (this.MeasureUnitID == castObj.MeasureUnitID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MeasureUnitID.GetHashCode();
        }
    }
}