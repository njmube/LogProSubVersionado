using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;

namespace Entities.Report
{
    /// <summary>
    /// Column object for mapped table Columns.
    /// </summary>
    [DataContract]
    public class IqColumn : Auditing
    {

        [DataMember]
        public virtual Int32 ColumnId { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual String DbType { get; set; }

        [DataMember]
        public virtual IqTable Table { get; set; }

        [DataMember]
        public virtual IList<IqReportColumn> ReportColumns { get; set; }

        //[DataMember]
        //public virtual Auditory Auditory { get; set; }

        public override Int32 GetHashCode()
        {
            return 3 * 3 * this.ColumnId.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            IqColumn castObj = (IqColumn)obj;
            return (castObj != null) && (this.ColumnId == castObj.ColumnId);
        }
    }
}