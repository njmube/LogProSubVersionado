// File:    Node.cs
// File:    Node.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:12:00
// Purpose: Definition of Class LabelTransaction

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Master;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class NodeTrace : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Node Node { get; set; }
        [DataMember]
        public virtual Document Document { get; set; }
        [DataMember]
        public virtual DocumentLine DocumentLine { get; set; }
        [DataMember]
        public virtual Bin Bin { get; set; }
        [DataMember]
        public virtual Label Label { get; set; }
        [DataMember]
        public virtual Label FatherLabel { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; }
        [DataMember]
        public virtual Double Quantity { get; set; }
        [DataMember]
        public virtual Boolean? IsDebit { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual String Comment { get; set; }
        [DataMember]
        public virtual Document PostingDocument { get; set; }
        [DataMember]
        public virtual Int32 PostingDocLineNumber { get; set; }
        [DataMember]
        public virtual String PostingUserName { get; set; }
        [DataMember]
        public virtual DateTime? PostingDate { get; set; }
        [DataMember]
        public virtual Bin BinSource { get; set; }

        //[DataMember]
        //public virtual IList<Label> Labels { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            NodeTrace castObj = (NodeTrace)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}

/*
 * XSD DE NODEBASE Y NODE EXTENSION
 * 
<?xml version="1.0" standalone="yes"?>
<root>
  <xs:schema id="root" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xs:element name="root" msdata:IsDataSet="true" msdata:Locale="en-US">
      <xs:complexType>
        <xs:choice minOccurs="0" maxOccurs="unbounded">
          <xs:element name="NodeBase">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="RowID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" msdata:AutoIncrement="true" msdata:AutoIncrementSeed="1" msdata:AutoIncrementStep="1" />
                <xs:element name="docID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" />
                <xs:element name="docTypeID" type="xs:int" msdata:DataType="System.Int16" minOccurs="0" />
                <xs:element name="companyID" type="xs:int" msdata:DataType="System.Int16" minOccurs="0" />
                <xs:element name="locationID" type="xs:int" msdata:DataType="System.Int16" minOccurs="0" />
                <xs:element name="binID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" />
                <xs:element name="labelID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" />
                <xs:element name="qtyDebit" type="xs:double" msdata:DataType="System.Double" minOccurs="0" />
                <xs:element name="qtyCredit" type="xs:double" msdata:DataType="System.Double" minOccurs="0" />
                <xs:element name="statusID" type="xs:int" msdata:DataType="System.Int16" minOccurs="0" />
                <xs:element name="comment" type="xs:string" minOccurs="0" msdata:DataType="System.String" />
                <xs:element name="postingDocumentID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" />
                <xs:element name="postingDate" type="xs:date" msdata:DataType="System.DateTime" minOccurs="0" />
                <xs:element name="userName" type="xs:string"  msdata:DataType="System.String" minOccurs="0" />
				<xs:element name="recordDate" type="xs:date" msdata:DataType="System.DateTime" minOccurs="0" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
			<xs:element name="NodeExtension">
				<xs:complexType>
					<xs:sequence>
						<xs:element name="RowID" type="xs:int" msdata:DataType="System.Int32" minOccurs="0" />
					</xs:sequence>
				</xs:complexType>
			</xs:element>
		</xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
</root>
*/