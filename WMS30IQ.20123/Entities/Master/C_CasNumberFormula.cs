
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;
using Entities.Trace;


namespace Entities.Master
{
    //Clase creada para el proyecto de ANDES CHEMICAL

    [DataContract(Namespace = "Entities.Master", IsReference = true)] 
    public class C_CasNumberFormula : Profile.Auditing
    {

        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual C_CasNumber CasNumberComponent { get; set; }

        [DataMember]
        public virtual Double Percent { get; set; }
      

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            C_CasNumberFormula castObj = (C_CasNumberFormula)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
