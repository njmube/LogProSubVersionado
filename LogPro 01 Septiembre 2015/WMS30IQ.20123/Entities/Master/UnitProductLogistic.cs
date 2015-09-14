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
    [DataContract(Namespace = "Entities.Master")]

    public class UnitProductLogistic : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual UnitProductEquivalence LogisticUnit { get; set; }
        [DataMember]
        public virtual UnitProductEquivalence ContainedUnit { get; set; }
        [DataMember]
        public virtual Int32 AmountOfContained { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            UnitProductLogistic castObj = (UnitProductLogistic)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}