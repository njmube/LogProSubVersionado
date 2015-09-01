using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Profile;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true

    public class TrackOption : Auditing
    {
        [DataMember]
        public virtual Int16   RowID { get; set; } //Nombre del Attributo en la clase Label

        [DataMember]
        public virtual String Name { get; set; } //Nombre del Attributo en la clase Label

        [DataMember]
        public virtual Boolean? IsUnique { get; set; }

        [DataMember]
        public virtual String DisplayName { get; set; } //Despliegue generico global (eg. SerialNumber, LotCode)

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual DataType DataType { get; set; }

        [DataMember]
        public virtual Boolean? Active { get; set; }

        [DataMember]
        public virtual Boolean? IsSystem { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            TrackOption castObj = (TrackOption)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
