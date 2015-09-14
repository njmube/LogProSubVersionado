// File:    ClassEntity.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:23
// Purpose: Definition of Class ClassEntity
// Tipo de Master entity Clasess, Account, Product, Location

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Report;


namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    public class ClassEntity
    {
        [DataMember] 
        public virtual Int16 ClassEntityID { get; set; }
        [DataMember] 
        public virtual String Name { get; set; }
        [DataMember] 
        public virtual Boolean? BlnManageContacts { get; set; }
        [DataMember] 
        public virtual Boolean? BlnManageCriteria { get; set; }
        [DataMember]
        public virtual Boolean? BlnZoneCriteria { get; set; }
        //[DataMember]
        //public virtual IqReportColumn ShorcutColumn { get; set; }
        [DataMember]
        public virtual Int32 ShortcutColumnID { get; set; }

        //[DataMember]
        //public virtual IList<GroupCriteriaRelation> GroupCriteriaRelations { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ClassEntity castObj = (ClassEntity)obj;
            return (castObj != null) &&
                (this.ClassEntityID == castObj.ClassEntityID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ClassEntityID.GetHashCode();
        }

    }
}
