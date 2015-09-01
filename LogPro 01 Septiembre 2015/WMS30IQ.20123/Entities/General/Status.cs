// File:    MetaStatus.cs
// File:    MetaStatus.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:12:01
// Purpose: Definition of Class MetaStatus
// Datos de los status

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General", IsReference = true)] //, IsReference = true
    /// Status para cualquier tipo de entidad del sistema que maneje estados
    public class Status 
    {
        [DataMember]
        public virtual Int32 StatusID { get; set; }
        [DataMember]
        public virtual StatusType StatusType { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        public virtual String Description { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Status castObj = (Status)obj;
            return (castObj != null) &&
                (this.StatusID == castObj.StatusID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.StatusID.GetHashCode();
        }
    }
}