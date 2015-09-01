using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //, IsReference = true
    //[KnownType(typeof(NHibernate.Collection.PersistentBag))]
    [KnownType(typeof(MenuOption))]

    public class MenuOptionType 
    {
        [DataMember]
        public virtual Int32 MenuOptionTypeID { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Url { get; set; }
        [DataMember]
        public virtual IList<MenuOption> MenuOptions { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MenuOptionType castObj = (MenuOptionType)obj;
            return (castObj != null) &&
                (this.MenuOptionTypeID == castObj.MenuOptionTypeID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.MenuOptionTypeID.GetHashCode();
        }
    }
}
