// File:    MeasureType.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:59
// Purpose: Definition of Class (Measure Type Weight, Length, Volume, Capacity)

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    public class MeasureType 
    {
        [DataMember]
        public virtual Int16 MeasureTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        //public virtual IList<MeasureUnit> MeasureUnits { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MeasureType castObj = (MeasureType)obj;
            return (castObj != null) &&
                (this.MeasureTypeID == castObj.MeasureTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MeasureTypeID.GetHashCode();
        }
    }
}