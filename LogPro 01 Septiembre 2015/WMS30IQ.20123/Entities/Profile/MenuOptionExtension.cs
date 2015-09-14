using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //, IsReference = true
    public class MenuOptionExtension
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual MenuOption MenuOption { get; set; }
        [DataMember]
        public virtual String Custom1 { get; set; } //Query
        [DataMember]
        public virtual String Custom2 { get; set; } //Query Columns
        [DataMember]
        public virtual String Custom3 { get; set; } //Param and Filters

  
        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MenuOptionExtension castObj = (MenuOptionExtension)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
