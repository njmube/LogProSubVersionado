using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;


namespace Entities.General
{
    [DataContract(Namespace = "Entities.General", IsReference = true)] //)] //, IsReference=true)]

    public class DataType 
    {

        [DataMember]
        public virtual Int16 DataTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            DataType castObj = (DataType)obj;
            return (castObj != null) &&
                (this.DataTypeID == castObj.DataTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.DataTypeID.GetHashCode();
        }
       
    }
}
