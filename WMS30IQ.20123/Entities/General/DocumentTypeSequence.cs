// File:    DocumentTypeSequence.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:59
// Purpose: Definition of Class DocumentTypeSequence 
//Consecutivo de cada tipo de documento.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;


namespace Entities.General
{
    [DataContract(Namespace = "Entities.General")]

    /// Incluye Posting Documents
    public class DocumentTypeSequence : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual DocumentType DocType { get; set; }
        [DataMember]
        public virtual String Prefix { get; set; }
        [DataMember]
        public virtual Int64 NumSequence { get; set; }
        [DataMember]
        public virtual String CodeSequence
        {
            get { return Prefix + NumSequence.ToString().PadLeft(5, '0'); }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentTypeSequence castObj = (DocumentTypeSequence)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}