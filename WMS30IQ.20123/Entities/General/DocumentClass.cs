// File:    ClassEntity.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:23
// Purpose: Definition of Class ClassEntity
//Receiving, Shipping, Task, Storage, Erp

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Report;


namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    [Serializable]
    public class DocumentClass 
    {
        [DataMember]
        public virtual Int16 DocClassID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Boolean? HasAdmin { get; set; }
        [DataMember]
        public virtual String Fields { get; set; }
        //[DataMember]
        //public virtual IList<DocumentType> DocumentTypes { get; set; }
        
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentClass castObj = (DocumentClass)obj;
            return (castObj != null) &&
                (this.DocClassID == castObj.DocClassID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.DocClassID.GetHashCode();
        }

    }
}
