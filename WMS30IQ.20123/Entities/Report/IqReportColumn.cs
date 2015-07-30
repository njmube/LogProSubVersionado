using System;
using System.Runtime.Serialization;
using Entities.Profile;


namespace Entities.Report
{
    /// <summary>
    /// ReportColumn object for mapped table ReportColumns.
    /// </summary>
    [DataContract]
    public class IqReportColumn : Auditing
    {

        [DataMember]
        public virtual Int32 ReportColumnId { get; set; }

        [DataMember]
        public virtual Boolean IsSelected { get; set; }

        [DataMember]
        public virtual String Alias { get; set; }

        [DataMember]
        public virtual Boolean? IsFiltered { get; set; }

        [DataMember]
        public virtual String FilteredValue { get; set; }

        [DataMember]
        public virtual String FilterOperator { get; set; }

        [DataMember]
        public virtual Boolean? IsAggregate { get; set; }

        [DataMember]
        public virtual String AggregateValue { get; set; }

        [DataMember]
        public virtual Int16 NumOrder { get; set; }

        [DataMember]
        public virtual String BaseWhere { get; set; }

        [DataMember]
        public virtual Boolean? IsCalculated { get; set; }

        [DataMember]
        public virtual String ColumnFormula { get; set; }

        [DataMember]
        public virtual String Options {
            get
            {
                if (IsAggregate == true)
                    return "A: " + this.AggregateValue + "(_val)";

                if (IsFiltered == true && !string.IsNullOrEmpty(FilteredValue))
                    return "F: " + this.FilterOperator + " _val:" + this.FilteredValue;

                return "";
            }
            set {} }


        [DataMember]
        public virtual String OptionsDesc { get; set; }

        [DataMember]
        public virtual IqColumn Column { get; set; }

        [DataMember]
        public virtual IqReportTable ReportTable { get; set; }



        public override Int32 GetHashCode()
        {
            return 3 * 3 * this.ReportColumnId.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            IqReportColumn castObj = (IqReportColumn)obj;
            return (castObj != null) && (this.ReportColumnId == castObj.ReportColumnId);
        }
    }
}