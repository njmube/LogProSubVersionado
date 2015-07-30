// File:    ShippingDocumentHdr.cs
// Author:  jairomurillo
// Created: jueves, 28 de agosto de 2008 8:28:49
// Purpose: Definition of Class Document
//This Class receive document & Tasks

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;
using Entities.Profile;
using System.ComponentModel;
using System.Linq;


namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace", IsReference = true)] //, IsReference = true
    public class DocumentPackage : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 PackID { get; set; }
        [DataMember]
        public virtual Document Document { get; set; }
        [DataMember]
        public virtual Label PackLabel { get; set; }

        [DataMember]
        public virtual DocumentPackage ParentPackage { get; set; }

        //Saber si es el package por defecto. 23 SEP 2009
        [DataMember]
        public virtual Boolean IsRoot
        {
            get { return (Sequence == 1) ? true : false; }
            set { }
        }

        [DataMember]
        public virtual Boolean? IsClosed { get; set; }

        [DataMember]
        public virtual SysUser Picker { get; set; }

        [DataMember]
        public virtual String Notes { get; set; }

        [DataMember]
        public virtual String ShipToName { get; set; }

        [DataMember]
        public virtual String AddressLine1 { get; set; }

        [DataMember]
        public virtual String AddressLine2 { get; set; }

        [DataMember]
        public virtual String AddressLine3 { get; set; }

        [DataMember]
        public virtual DateTime? StartTime { get; set; } //Picking Time

        [DataMember]
        public virtual DateTime? EndTime { get; set; } //Picking Time

        [DataMember]
        public virtual DateTime? StartTimePacking { get; set; } //Packing Time

        [DataMember]
        public virtual DateTime? EndTimePacking { get; set; } //Packing Time

        [DataMember]
        public virtual Document PostingDocument { get; set; }

        [DataMember]
        public virtual String PostingUserName { get; set; }

        [DataMember]
        public virtual DateTime? PostingDate { get; set; }

        [DataMember]
        public virtual Double Weight  { get; set; }
        
        [DataMember]
        public virtual Int32 Sequence { get; set; }


        [DataMember]
        public virtual Int32 SubSequence { get; set; } 

        [DataMember]
        public virtual String Dimension { get; set; }
        
        [DataMember]
        public virtual Int32 Pieces { get; set; }

        [DataMember]
        public virtual String PackageType { get; set; }

        [DataMember]
        public virtual String PackagePath { get; set; }

        [DataMember]
        public virtual Double CalculatedPieces { get; set; }


        [DataMember]
        public virtual String  PackDesc { 
            get {
                return "Pack# " + this.Sequence.ToString() + " (Code: " + this.PackLabel.LabelCode + ") "; //- " + ((this.IsOpen != false) ? "Open" : "Closed");            
            }                     
            set{} }


        [DataMember]
        public virtual String PackDescExt
        {
            get
            {
                if (PackageType == "P")
                    return "PLT" + SubSequence.ToString() + " # " + this.PackLabel.LabelCode;
                if (PackageType == "R")
                    return "Root of " + (string.IsNullOrEmpty(this.Document.PostingDocument) ? Document.DocNumber : Document.PostingDocument);

                return "BOX" + SubSequence.ToString() + " # " + this.PackLabel.LabelCode;
            }
            set { }
        }



        [DataMember]
        public virtual String RLevel
        {
            get { return "L" + (Level + 1).ToString(); }
            set { }
        }


        [DataMember]
        public virtual IList<DocumentPackage> ChildPackages { get; set; }


        //Para Guardar Auditoria.

        [DataMember]
        public virtual Int32 Level { get; set; } //LEVEL

        [DataMember]
        public virtual String CurrentDesc { get; set; }

        [DataMember]
        public virtual String AuditedBy { get; set; }

        [DataMember]
        public virtual DateTime? AuditDate { get; set; }

        [DataMember]
        public virtual String AuditStatus { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentPackage castObj = (DocumentPackage)obj;
            return (castObj != null) &&
                (this.PackID == castObj.PackID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.PackID.GetHashCode();
        }
    }
}
