using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)]
    public class ProductAccountRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        
        [DataMember]
        public virtual AccountType AccountType { get; set; } //Customer (1), Vendor (2)
        
        [DataMember]
        public virtual Account Account { get; set; }
        
        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual String ItemNumber { get; set; }

        
        [DataMember]
        public virtual String Code1 { get; set; } //For alter upc vendor codes

        
        [DataMember]
        public virtual String Code2 { get; set; } //For alter upc vendor codes


        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }


        [DataMember]
        public virtual Boolean? IsDefault { get; set; }

        
        //ANDES - features
        //Se adicionan varios campos para soportar las definciones

        [DataMember]
        public virtual Double ShipWeight { get; set; }


        [DataMember]
        public virtual Double NetWeight { get; set; }  


        [DataMember]
        public virtual Unit PackUnit { get; set; }  


        [DataMember]
        public virtual Double CubFeet { get; set; }




        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ProductAccountRelation castObj = (ProductAccountRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
