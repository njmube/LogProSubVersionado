using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.General
{
    public class LogError : Profile.Auditing
    {
        [DataMember]
        public virtual Int64 LogErrorID { get; set; }
        [DataMember]
        public virtual String Category { get; set; }
        [DataMember]
        public virtual String UserError { get; set; }
        [DataMember]
        public virtual String TechError { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            LogError castObj = (LogError)obj;
            return (castObj != null) &&
                (this.LogErrorID == castObj.LogErrorID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.LogErrorID.GetHashCode();
        }

    }
}
