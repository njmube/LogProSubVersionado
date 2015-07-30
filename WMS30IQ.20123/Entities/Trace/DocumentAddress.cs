// File:    ShippingDocumentHdr.cs
// Author:  jairomurillo
// Created: jueves, 28 de agosto de 2008 8:28:49
// Purpose: Definition of Class ShippingDocumentHdr

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.General;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace", IsReference=true)]

    public class DocumentAddress : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Document Document { get; set; }
        [DataMember]
        public virtual DocumentLine DocumentLine { get; set; }
        [DataMember]
        public virtual Int16 AddressType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String AddressLine1 { get; set; }
        [DataMember]
        public virtual String AddressLine2 { get; set; }
        [DataMember]
        public virtual String AddressLine3 { get; set; }
        [DataMember]
        public virtual String City { get; set; }
        [DataMember]
        public virtual String State { get; set; }
        [DataMember]
        public virtual String ZipCode { get; set; }
        [DataMember]
        public virtual String Country { get; set; }
        [DataMember]
        public virtual String ContactPerson { get; set; }
        [DataMember]
        public virtual String Phone1 { get; set; }
        [DataMember]
        public virtual String Phone2 { get; set; }
        [DataMember]
        public virtual String Phone3 { get; set; }
        [DataMember]
        public virtual String Email { get; set; }
        [DataMember]
        public virtual ShippingMethod ShpMethod { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }


        [DataMember]
        public virtual String FullDesc
        {
            get
            {

                string fDesc = IsNull(this.Name);
                fDesc += IsNull(AddressLine1) + IsNull(AddressLine2) + IsNull(AddressLine3);
                fDesc += ", " + IsNull(City) + IsNull(State) + IsNull(ZipCode);
                fDesc += ", " + IsNull(Country);
                return fDesc;

            }
            set { }
        }

        public virtual string IsNull(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";
            else
                return data + " ";
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DocumentAddress castObj = (DocumentAddress)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.Document.GetHashCode();
        }
    }
}

/*
//SHIPPING 
[DataMember]
public virtual DateTime deliveryDate { get; set; }  
[DataMember]
public virtual Int32 estShipDate { get; set; }
[DataMember]
public virtual DateTime shipDate { get; set; }
[DataMember]
public virtual DateTime docDate { get; set; }
*/

/* RECEIVING 
[DataMember]
public virtual DateTime deliveryDate { get; set; }
[DataMember]
public virtual DateTime shippingDate { get; set; }
[DataMember]
public virtual DateTime requestDate { get; set; }
[DataMember]
public virtual DateTime docDate { get; set; }
*/
