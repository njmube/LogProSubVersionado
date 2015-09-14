// File:    MeasureUnitConvertion.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:59
// Purpose: Definition of Class Convertion, Conver unit source to unit detination in the same measure type

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General")]
    public class MeasureUnitConvertion : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual MeasureUnit SourceUnit { get; set; }
        [DataMember]
        public virtual MeasureUnit DestinationUnit { get; set; }
        [DataMember]
        public virtual Double ConvertionFactor { get; set; }
        [DataMember]
        public virtual String Description { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MeasureUnitConvertion castObj = (MeasureUnitConvertion)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}