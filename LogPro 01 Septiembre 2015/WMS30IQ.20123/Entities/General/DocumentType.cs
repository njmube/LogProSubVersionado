// File:    DocumentType.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:59
// Purpose: Definition of Class DocumentType
// Dependiendo de la clase el tipo de docuemento purchase order etc

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Trace;
using Entities.Report;

namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    [Serializable]
    public class DocumentType 
    {
        [DataMember]
        public virtual Int16 DocTypeID { get; set; }
        [DataMember]
        public virtual DocumentClass DocClass { get; set; }
        [DataMember]
        public virtual StatusType StatusType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String DefPrefix { get; set; }
        [DataMember]
        public virtual PickMethod PickMethod  { get; set; }        
        [DataMember]
        public virtual LabelTemplate Template { get; set; }
        [DataMember]
        public virtual String Comment { get; set; }
        [DataMember]
        public virtual String Sign { get; set; }
        [DataMember]
        public virtual Boolean UseStock { get; set; }
        [DataMember]
        public virtual String ErpSetup { get; set; }


        //[DataMember]
        //public virtual IList<Document> Documents { get; set; }

        //[DataMember]
        //public virtual IList<Label> Labels { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentType castObj = (DocumentType)obj;
            return (castObj != null) &&
                (this.DocTypeID == castObj.DocTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.DocTypeID.GetHashCode();
        }
    }
}