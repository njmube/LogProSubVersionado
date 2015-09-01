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
    public class CustomProcessTransitionByEntity: CustomProcessTransition
    {
        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual Int32 EntityRowID { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessTransitionByEntity castObj = (CustomProcessTransitionByEntity)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }


        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
