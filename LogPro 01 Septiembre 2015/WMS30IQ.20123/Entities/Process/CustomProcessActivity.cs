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
    //Activida a Ejecutar define el metodo que se debe ejecutar en el administrador de procesos

    public class CustomProcessActivity: Auditing
    {
        [DataMember]
        public virtual Int32 ActivityID { get; set; }

        [DataMember]
        public virtual DocumentType ProcessType { get; set; }  //Quality, Inventory

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual String Description { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual String Method { get; set; }

        [DataMember]
        public virtual Int16 ActivityType { get; set; } //1- Automatic, 2 - Manual



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            CustomProcessActivity castObj = (CustomProcessActivity)obj;
            return (castObj != null) &&
                (this.ActivityID == castObj.ActivityID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ActivityID.GetHashCode();
        }

    }
}
