// File:    MetaStatus.cs
// File:    MetaStatus.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:12:01
// Purpose: Definition of Class MetaStatus
// Manejo de status para todas las entidades del sistema se deben definir los posible status

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Entities.General
{
    [Serializable]
    [DataContract(Namespace = "Entities.General", IsReference = true)] //
    /// Status para cualquier tipo de entidad del sistema que maneje estados
    public class StatusType 
    {
        [DataMember]
        public virtual Int16 StatusTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        //[DataMember]
        //public virtual IList<Status> Status { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            StatusType castObj = (StatusType)obj;
            return (castObj != null) &&
                (this.StatusTypeID == castObj.StatusTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.StatusTypeID.GetHashCode();
        }
    }
}