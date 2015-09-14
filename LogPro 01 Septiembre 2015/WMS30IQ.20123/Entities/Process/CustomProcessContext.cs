using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;
using Entities.Master;

namespace Entities.Process
{
    // Controla las variables a usar dentro del proceso (Entrada y Salida) y su tipo de dato,
    // Ahi se incluye el envio de mail y los labels a manipular. 

    public class CustomProcessContext : Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual DocumentType ProcessType { get; set; }  //Quality, Inventory

        [DataMember]
        public virtual String ContextKey { get; set; }

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual Object Value { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual String ContextDataType { get; set; }

        [DataMember]
        public virtual String ContextBasicValue { get; set; }

        [DataMember]
        public virtual Boolean IsInternal { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessContext castObj = (CustomProcessContext)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
