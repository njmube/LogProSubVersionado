using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Master
{
    public class Contract : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ContractID { get; set; }
        [DataMember]
        public virtual Account Account { get; set; }

        //Fechas de Contrato
        //Modalidad


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Contract castObj = (Contract)obj;
            return (castObj != null) &&
                (this.ContractID == castObj.ContractID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ContractID.GetHashCode();
        }
    }
}
