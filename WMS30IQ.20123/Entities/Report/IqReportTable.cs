using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;

namespace Entities.Report
{
    /// <summary>
    /// ReportTable object for mapped table ReportTables.
    /// </summary>
    [DataContract]
    public class IqReportTable : Auditing
    {

        [DataMember]
        public virtual Int32 ReportTableId { get; set; }

        [DataMember]
        public virtual String Alias { get; set; }

        [DataMember]
        public virtual Int16 Secuence { get; set; }

        [DataMember]
        public virtual String JoinQuery { get; set; }

        [DataMember]
        public virtual String WhereCondition { get; set; }

        [DataMember]
        public virtual IqReport Report { get; set; }

        [DataMember]
        public virtual IqTable Table { get; set; }


        [DataMember]
        public virtual Int16 NumLevel { get; set; }


        [DataMember]
        public virtual IList<IqReportColumn> ReportColumns { get; set; }

        //[DataMember]
        //public virtual Auditory Auditory { get; set; }

        public override Int32 GetHashCode()
        {
            return 3 * 3 * this.ReportTableId.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            IqReportTable castObj = (IqReportTable)obj;
            return (castObj != null) && (this.ReportTableId == castObj.ReportTableId);
        }
    }
}