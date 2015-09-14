using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


    /// <summary>
    /// The meta-model (and associated XPDL) assumes a number of standard data types 
    /// (string, reference, integer, float, date/time, etc.); such data types are relevant to data fields, 
    /// system or environmental data or participant data. Expressions may be formed using such data types to 
    /// support conditional evaluations and assignment of new values to data fields. Data types may be extended using 
    /// an XML schema or a reference to data defined in an external source.
    /// </summary>
    /// 
    namespace Entities.Workflow
    {
        [Serializable]
        [DataContract(Namespace = "Entities.Workflow", IsReference = true)] 

        public class WFDataType
        {
            [DataMember]
            public virtual Int16 DataTypeID { get; set; }
            
            [DataMember]
            public virtual String Name { get; set; }

            [DataMember]
            public virtual String Description { get; set; }
            
            [DataMember]
            public virtual Boolean? IsBasic { get; set; } //Indica si es un tipo basico o un tipo complejo

            [DataMember]
            public virtual String UIControl { get; set; } //Namespace del UserControl or XML

            [DataMember]
            public virtual String BaseType { get; set; } //Tipo Base para conversion del valor entregado Cast()

            [DataMember]
            public virtual String UIListControl { get; set; } //Tipo de Dato



            public override Boolean Equals(object obj)
            {
                if ((obj == null) || (obj.GetType() != this.GetType())) return false;
                WFDataType castObj = (WFDataType)obj;
                return (castObj != null) &&
                    (this.DataTypeID == castObj.DataTypeID);
            }

            public override Int32 GetHashCode()
            {
                return 9 * 3 * this.DataTypeID.GetHashCode();
            }
        }
    }



/*
<xsd:enumeration value="STRING"/> 
 * <xsd:enumeration value="FLOAT"/> 
 * <xsd:enumeration value="INTEGER"/> 
 * <xsd:enumeration value="REFERENCE"/> 
 * <xsd:enumeration value="DATETIME"/> 
 * <xsd:enumeration value="DATE"/> 
 * <xsd:enumeration value="TIME"/> 
 * <xsd:enumeration value="BOOLEAN"/> 
 * <xsd:enumeration value="PERFORMER"/>
*/

/*
<xsd:element ref="xpdl:BasicType"/> 
 * <xsd:element ref="xpdl:DeclaredType"/> 
 * <xsd:element ref="xpdl:SchemaType"/> 
 * <xsd:element ref="xpdl:ExternalReference"/> 
 * <xsd:element ref="xpdl:RecordType"/> 
 * <xsd:element ref="xpdl:UnionType"/> 
 * <xsd:element ref="xpdl:EnumerationType"/> 
 * <xsd:element ref="xpdl:ArrayType"/> 
 * <xsd:element ref="xpdl:ListType"/>
*/