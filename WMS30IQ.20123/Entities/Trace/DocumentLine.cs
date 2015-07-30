// File:    ShippingDocumentLine.cs
// Author:  jairomurillo
// Created: jueves, 28 de agosto de 2008 8:28:49
// Purpose: Definition of Class ShippingDocumentLine

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class DocumentLine : Profile.Auditing
    {
        [DataMember]
        public virtual Int64 LineID { get; set; }
        [DataMember]
        public virtual Document Document { get; set; }
        [DataMember]
        public virtual Int32 LineNumber { get; set; }
        [DataMember]
        public virtual Status LineStatus { get; set; }

        [DataMember]
        public virtual String AccountItem { get; set; } //VendorItemNumber or CustomerItem Number

        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual String LineDescription { get; set; }

        [DataMember]
        public virtual Boolean? IsDebit { get; set; }

        [DataMember]
        public virtual String Sign { get { return (IsDebit == true) ? "-" : "+"; } set { } }

        [DataMember]
        public virtual Double Quantity { get; set; }
        [DataMember]
        public virtual Double QtyCancel { get; set; }
        [DataMember]
        public virtual Double QtyPending { get; set; }
        [DataMember]
        public virtual Double QtyInvoiced { get; set; }
        [DataMember]
        public virtual Double QtyAllocated { get; set; }

        [DataMember]
        public virtual Double QtyBackOrder { get; set; }

        [DataMember]
        public virtual Double QtyShipped { get; set; }

        [DataMember]
        public virtual Double QtyPendingShip  //Lo que ya se piqueo y se fue.
        {
            get { return Quantity - QtyShipped; }
            set{} 
        }

        //For Mergerd order
        [DataMember]
        public virtual Double QtyOnHand { get; set; }
        [DataMember]
        public virtual Double QtyAvailable { get; set; }
        //[DataMember]
        //public virtual Double QtyOrdPend { get; set; }



        [DataMember]
        public virtual Unit Unit { get; set; }

        [DataMember]
        public virtual Double UnitBaseFactor { get; set; }
        [DataMember]
        public virtual DateTime? Date1 { get; set; }
        [DataMember]
        public virtual DateTime? Date2 { get; set; }
        [DataMember]
        public virtual DateTime? Date3 { get; set; }
        [DataMember]
        public virtual DateTime? Date4 { get; set; }
        [DataMember]
        public virtual DateTime? Date5 { get; set; }
        
        [DataMember]
        public virtual Location Location { get; set; }

        [DataMember]
        public virtual Location Location2 { get; set; } //Used in transaction between warehouses (To/From Warehouse)

        [DataMember]
        public virtual String Note { get; set; } //Component Type for Assembly Doc

        [DataMember]
        public virtual String LinkDocNumber { get; set; }

        [DataMember]
        public virtual Int32 LinkDocLineNumber { get; set; }  //Component Parent in Assembly Doc

        [DataMember]
        public virtual String PostingDocument { get; set; }
        [DataMember]
        public virtual String PostingUserName { get; set; }
        [DataMember]
        public virtual DateTime? PostingDate { get; set; }
        [DataMember]
        public virtual String BinAffected { get; set; }
        [DataMember]
        public virtual Int32 Sequence { get; set; }

        //Nov 25 2009 - Prices
        [DataMember]
        public virtual Double UnitPrice { get; set; }

        [DataMember]
        public virtual Double ExtendedPrice { get; set; }

        [DataMember]
        public virtual Double UnitCost { get; set; }

        [DataMember]
        public virtual Double ExtendedCost { get; set; }

        [DataMember]
        public virtual String KitNote
        {
            get
            {
                if (this.Note == "2") return "Kit/Assembly";

                if (this.Note == "1") return "Component";

                if (string.IsNullOrEmpty(LinkDocNumber))
                    return LinkDocNumber;

                return "";
            }
            set { }
        }

        //WEIGHT - VOLUME
        [DataMember]
        public virtual Double LineWeight { get; set; }

        [DataMember]
        public virtual Double LineVolume { get; set; }


        //[DataMember]
        public virtual IList<DocumentAddress> DocumentLineAddresses { get; set; }
 
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentLine castObj = (DocumentLine)obj;
            return (castObj != null) &&
                (this.LineID == castObj.LineID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.LineID.GetHashCode();
        }
    }
}