// File:    Vehicle.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:59:55
// Purpose: Definition of Class Vehicle
// Vehiculos asociados a aun comopania

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true

    public class Vehicle : Profile.Auditing
    {
        [DataMember]
        public virtual Int16 VehicleID { get; set; }
        [DataMember]
        public virtual Account Account { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Plate1 { get; set; }
        [DataMember]
        public virtual String Plate2 { get; set; }
        [DataMember]
        public virtual String Capacity { get; set; }
        [DataMember]
        public virtual String ContainerNumber { get; set; }
        [DataMember]
        public virtual String ContainerCapacity { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Vehicle castObj = (Vehicle)obj;
            return (castObj != null) &&
                (this.VehicleID == castObj.VehicleID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.VehicleID.GetHashCode();
        }

    }
}