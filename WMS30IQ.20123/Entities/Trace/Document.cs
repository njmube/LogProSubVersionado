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
    public class Document : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 DocID { get; set; }
        [DataMember]
        public virtual DocumentType DocType { get; set; }
        [DataMember]
        public virtual DocumentConcept DocConcept { get; set; }
        [DataMember]
        public virtual String DocNumber { get; set; }

        [DataMember]
        public virtual String Search { get; set; } //Para buscar por cualquier Criterio

        [DataMember]
        public virtual Int16 Priority { get; set; } //Used to require Audit
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }
        [DataMember]
        public virtual Boolean? CrossDocking { get; set; }
        [DataMember]
        public virtual Boolean? UseAllocation { get; set; }
        [DataMember]
        public virtual Int32 ErpMaster { get; set; }
        [DataMember]
        public virtual Account Vendor { get; set; }
        [DataMember]
        public virtual Account Customer { get; set; }
        [DataMember]
        public virtual Status DocStatus { get; set; }
        [DataMember]
        public virtual String SalesPersonName { get; set; }
        [DataMember]
        public virtual String QuoteNumber { get; set; }
        [DataMember]
        public virtual String CustPONumber { get; set; }
        [DataMember]
        public virtual String Comment { get; set; }
        [DataMember]
        public virtual String Notes { get; set; }
        [DataMember]
        public virtual String Reference { get; set; }
        [DataMember]
        public virtual DateTime? Date1 { get; set; } //DOCDATE
        [DataMember]
        public virtual DateTime? Date2 { get; set; } //PO: RequireDate  
        [DataMember]
        public virtual DateTime? Date3 { get; set; } //PO: PromisedShipDate
        [DataMember]
        public virtual DateTime? Date4 { get; set; } //PO: PromisedDate

        [DataMember]
        public virtual DateTime? Date5 { get; set; } //PO: Arrived

        [DataMember]
        public virtual String UserDef1 { get; set; }
        [DataMember]
        public virtual String UserDef2 { get; set; }
        [DataMember]
        public virtual String UserDef3 { get; set; }

        [DataMember]
        public virtual Boolean? Arrived {
            get
            {
                if (Date5 == null) return null;
                return true;
            }
            set { } }

        [DataMember]
        public virtual DateTime? LastChange { get; set; }

        [DataMember]
        public virtual PickMethod PickMethod { get; set; }


        //[DataMember]
        //public virtual SysUser User { get; set; }

        [DataMember]
        public virtual Location Location { get; set; }

        [DataMember]
        public virtual Company Company { get; set; }

        [DataMember]
        public virtual String PostingDocument { get; set; }
        [DataMember]
        public virtual String PostingUserName { get; set; }
        [DataMember]
        public virtual DateTime? PostingDate { get; set; }

        [DataMember]
        public virtual ShippingMethod ShippingMethod { get; set; }

        [DataMember]
        public virtual Boolean? AllowPartial { get; set; }

        [DataMember]
        public virtual IList<TaskByUser> TaskUsers { get; set; }

        [DataMember]
        public virtual String SuggestedUsers { get; set; }

        //[DataMember]
        public virtual IList<DocumentLine> DocumentLines { get; set; }

        //[DataMember]
        public virtual IList<DocumentAddress> DocumentAddresses { get; set; }

        //[DataMember]
        public virtual IList<TaskDocumentRelation> TaskDocuments { get; set; }


        [DataMember]
        public virtual String FullDesc
        {
            get { return this.DocNumber + ", " + this.Date1.Value.ToString("yyyy-MM-dd") + ", " + this.CreatedBy; }
            set { }
        }


        [DataMember]
        public virtual string AssignedUsers
        {
            get
            {
                string result = "";
                if (TaskUsers != null && TaskUsers.Count > 0)
                {
                    result = string.Join(", ", TaskUsers.Select(f => f.User.UserName).ToArray());
                    //foreach (TaskByUser curLine in TaskUsers.OrderBy(f => f.User.UserName))
                    //    result += curLine.User.UserName + ", ";

                    return result;
                }
                else
                    return "Not Assigned";
            }
            set { }
        }



        [DataMember]
        public virtual string ShipAddress
        {
            get
            {
                try { return DocumentAddresses.Where(f => f.AddressType == AddressType.Shipping).First().FullDesc; }
                catch { return ""; }

            }
            set { }
        }


        //WEIGHT - VOLUME
        [DataMember]
        public virtual Double DocWeight { get; set; }

        [DataMember]
        public virtual Double DocVolume { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Document castObj = (Document)obj;
            return (castObj != null) &&
                (this.DocID == castObj.DocID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.DocID.GetHashCode();
        }
    }
}

/*
//SHIPPING 

*/

/* RECEIVING 
   Required Date (La fecha en que espero la mercancia)
   Promised Date (La promesa del vendor)
   Promised Ship Date ()
*/
