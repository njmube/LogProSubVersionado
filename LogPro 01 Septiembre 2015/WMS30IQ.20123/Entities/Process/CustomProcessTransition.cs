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
    //Encargada de Definir La ruta de actividades del proceso hasta llegar la final de la ejecucion
    public class CustomProcessTransition: Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual CustomProcess Process { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual Int16 Sequence { get; set; }

        [DataMember]
        public virtual CustomProcessActivity CurrentActivity { get; set; }

        [DataMember]
        public virtual CustomProcessContext ResultContextKey { get; set; }

        [DataMember]
        public virtual String ResultValue { get; set; }

        [DataMember]
        public virtual CustomProcessActivity NextActivity { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

      



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessTransition castObj = (CustomProcessTransition)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
