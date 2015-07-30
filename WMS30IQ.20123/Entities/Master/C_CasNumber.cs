
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
    public class C_CasNumber : Profile.Auditing
    {

        [DataMember]
        public virtual Int32 CasNumberID { get; set; }

        [DataMember]
        public virtual String Code { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual String FullDesc
        {
            get { return this.Code.Trim() + ", " + this.Name; }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            C_CasNumber castObj = (C_CasNumber)obj;
            return (castObj != null) &&
                (this.CasNumberID == castObj.CasNumberID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.CasNumberID.GetHashCode();
        }

    }
}
