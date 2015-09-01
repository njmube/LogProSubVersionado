// File:    ClassCriteriaDetail.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 19:21:35
// Purpose: Definition of Class ClassCriteriaDetail

//Detllae de los criterios de clasificacion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General")]


    /// Detalles de criterios de agrupacion
    public class GroupCriteriaDetail : Profile.Auditing
    {

        [DataMember]
        public virtual Int32 CriteriaDetID { get; set; }
        [DataMember]
        public virtual GroupCriteria GroupCriteria { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }

        //[DataMember]
        public virtual IList<GroupCriteriaRelationData> GroupCriteriaRelDatas { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            GroupCriteriaDetail castObj = (GroupCriteriaDetail)obj;
            return (castObj != null) &&
                (this.CriteriaDetID == castObj.CriteriaDetID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.CriteriaDetID.GetHashCode();
        }
    }
}