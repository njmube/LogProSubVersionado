using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;
using Entities.General;

namespace Entities.Workflow
{
    [Serializable]
    [DataContract(Namespace = "Entities.Workflow", IsReference = true)]

    /*
     * Tipo de Dato
        Cliente(Si el cliente es 0, es un dato que se pide para todo el mundo)
        Nombre
        Encabezado / Detalle
        Documento / Label
        Serial1, serial2, serial3. (Definen los tamanos de los datos referentes a los seriales)

     */
    public class DataDefinition : Profile.Auditing
    {
        [DataMember]
        public virtual Guid RowID { get; set; }

        [DataMember]
        public virtual String Code { get; set; }

        [DataMember]
        public virtual String DisplayName { get; set; }

        [DataMember]
        public virtual Boolean? ReadOnly { get; set; }

        //[DataMember]
        //public virtual Boolean? IsUnique { get; set; }

        [DataMember]
        public virtual WFDataType DataType { get; set; }

        [DataMember]
        public virtual Location Location { get; set; } // Si esta en 0 aplica para todos los clientes

        [DataMember]
        public virtual Boolean? IsHeader { get; set; } // 1=Header, 0=Detalle

        [DataMember]
        public virtual ClassEntity Entity { get; set; } // Si es documento, serial o detalle

        [DataMember]
        public virtual Boolean? IsSerial { get; set; } // 1=Si, 0=No

        [DataMember]
        public virtual Boolean? IsRequired { get; set; } // 1=Si, 0=No  ---> Si el tipo de campos es obligatorio o no

        [DataMember]
        public virtual Int32 Size { get; set; } // Tamano de los seriales

        [DataMember]
        public virtual String DefaultValue { get; set; }

        [DataMember]
        public virtual Int16 NumOrder { get; set; }

        [DataMember]
        public virtual MType MetaType { get; set; } //Valor del metatipo si el tipo de dato es combobox


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DataDefinition castObj = (DataDefinition)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
