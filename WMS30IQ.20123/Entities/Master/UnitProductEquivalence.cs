// File:    UnitProductEquivalence.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:16:12
// Purpose: Definition of Class UnitProductEquivalence
// Clase para poder pasar la unidad de un producto de un tipo a otro,
// ej KG, a volumen.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master")]

    public class UnitProductEquivalence : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual UnitProductRelation UnitProductRelation { get; set; }
        [DataMember]
        public virtual MeasureUnit EquivMeasureUnit { get; set; }
        [DataMember]
        public virtual Double EquivFactor { get; set; }


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