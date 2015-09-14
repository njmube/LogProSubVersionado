// File:    ClassCriteriaData.cs
// Author:  jairomurillo
// Created: viernes, 29 de agosto de 2008 8:34:16
// Purpose: Definition of Class ClassCriteriaData
// Datos que tiene el RowID de la entidad para determinado criterio de clasificacion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General")]

    /// Contiene los detalles de criterio contra las entidades asociadas a ellos
    public class GroupCriteriaRelationData : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual GroupCriteriaRelation CriteriaRel { get; set; }
        [DataMember]
        public virtual GroupCriteriaDetail CriteriaDet { get; set; }
        /// ID de la Entidad que se quiere entrar la info del criterio ej: Producto 'MILK33'
        [DataMember]
        public virtual Int32 EntityRowID { get; set; }
        /// Dato del Criterio para esa entidad
        [DataMember]
        public virtual String CriteriaDetData { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            GroupCriteriaRelationData castObj = (GroupCriteriaRelationData)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}