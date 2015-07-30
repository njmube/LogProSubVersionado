// File:    ClassCriteriaHdr.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 19:21:07
// Purpose: Definition of Class ClassCriteriaHdr
// Clieterios de clasificacion (clase master)


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true


    /// Encabezados de Criterios de Agruapcion
    public class GroupCriteria : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 GroupCriteriaID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }

        //[DataMember]
        public virtual IList<GroupCriteriaDetail> GroupCriteriaDetails { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            GroupCriteria castObj = (GroupCriteria)obj;
            return (castObj != null) &&
                (this.GroupCriteriaID == castObj.GroupCriteriaID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.GroupCriteriaID.GetHashCode();
        }

    }
}