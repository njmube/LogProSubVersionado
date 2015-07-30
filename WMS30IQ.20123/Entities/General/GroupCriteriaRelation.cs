// File:    ClassCriteriaRelation.cs
// Author:  jairomurillo
// Created: jueves, 28 de agosto de 2008 8:20:15
// Purpose: Definition of Class ClassCriteriaRelation
//Asociacion de las clases a los cliterios de clasificacion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General")]

    /// Contiene los detalles de criterio contra las entidades asociadas a ellos
    public class GroupCriteriaRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual GroupCriteria GroupCriteria { get; set; }
        [DataMember]
        public virtual ClassEntity ClassEntity { get; set; }
        //[DataMember]
        public virtual IList<GroupCriteriaRelationData> GroupCriteriaRelDatas { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            GroupCriteriaRelation castObj = (GroupCriteriaRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}