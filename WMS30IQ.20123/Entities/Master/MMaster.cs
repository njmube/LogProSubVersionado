using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

//Guarda los datos de la informacion de maestro a utilizar durante los pricesos, combos y listas.
//Puede salir de otras tablas existentes con lo que solo lo que se necesita es el query a la otra tabla y se define en el metatype

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]

    public class MMaster
    {
        [DataMember]
        public virtual Int32 MetaMasterID { get; set; }

        [DataMember]
        public virtual MType MetaType { get; set; }

        [DataMember]
        public virtual String Code { get; set; }

        [DataMember]
        public virtual String Code2 { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual String DefValue { get; set; }

        [DataMember]
        public virtual Int16 NumOrder { get; set; }

        [DataMember]
        public virtual Boolean? Active { get; set; }

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MMaster castObj = (MMaster)obj;
            return (castObj != null) &&
                (this.MetaMasterID == castObj.MetaMasterID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MetaMasterID.GetHashCode();
        }
    }
}
