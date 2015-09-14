using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class ProductCategory : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 CategoryID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual String ErpCode { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }

        [DataMember]  //Indica si el Kit se debe explosionar o no.
        public virtual Int16 ExplodeKit { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ProductCategory castObj = (ProductCategory)obj;
            return (castObj != null) &&
                (this.CategoryID == castObj.CategoryID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.CategoryID.GetHashCode();
        }
    }
}
