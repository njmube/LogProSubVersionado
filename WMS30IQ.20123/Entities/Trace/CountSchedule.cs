using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class CountSchedule : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        [DataMember]
        public virtual String Title { get; set; }
        [DataMember]
        public virtual DateTime? Start { get; set; }
        [DataMember]
        public virtual DateTime? Finish { get; set; }
        [DataMember]
        public virtual DateTime? NextDateRun { get; set; }
        [DataMember]
        public virtual String Query { get; set; }
        [DataMember]
        public virtual String Parameters { get; set; }
        [DataMember]
        public virtual Boolean? IsDone { get; set; }
        [DataMember]
        public virtual Int32 RepeatEach { get; set; }
        [DataMember]
        public virtual Int32 CountOption { get; set; } //0-BIN,1-PRODUCT,2-BIN,PRODUCT
        //Sirve para saber cuales son las condiciones del conteo.

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;

            CountSchedule castObj = (CountSchedule)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}