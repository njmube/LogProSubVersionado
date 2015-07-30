using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;

namespace Entities.Report
{
    /// <summary>
    /// Table object for mapped table Tables.
    /// </summary>
    [DataContract]
    public class IqTable : Auditing
    {

        [DataMember]
        public virtual Int32 TableId { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual IList<IqColumn> Columns { get; set; }

        [DataMember]
        public virtual IList<IqReportTable> ReportTables { get; set; }

        //[DataMember]
        //public virtual Auditory Auditory { get; set; }

        public override Int32 GetHashCode()
        {
            return 3 * 3 * this.TableId.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            IqTable castObj = (IqTable)obj;
            return (castObj != null) && (this.TableId == castObj.TableId);
        }
    }
}