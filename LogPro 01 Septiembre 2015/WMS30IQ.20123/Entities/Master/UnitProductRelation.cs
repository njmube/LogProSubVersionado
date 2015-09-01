// File:    ProductUnit.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:16:12
// Purpose: Definition of Class ProductUnit
// Unidade psibles para unproducto, desde la unidad basica hasta la unidad de empaque y cual contiene a cual

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true

    public class UnitProductRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Product Product { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; }
        [DataMember]
        public virtual String UnitErpCode { get; set; }
        [DataMember]
        public virtual Double BaseAmount { get; set; }
        [DataMember]
        public virtual Boolean IsBasic { get; set; }
        [DataMember]
        public virtual Double Weight { get; set; }
        [DataMember]
        public virtual Double Volume { get; set; }

        //[DataMember]
        public virtual MeasureUnit WeightUnit { get; set; } //UnitofMeasure
        //[DataMember]
        public virtual MeasureUnit VolumeUnit { get; set; } //UnitofMeasure
        [DataMember]
        public virtual Status Status { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            UnitProductRelation castObj = (UnitProductRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}