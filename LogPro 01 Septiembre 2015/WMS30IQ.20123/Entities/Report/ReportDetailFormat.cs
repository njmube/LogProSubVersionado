using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entities.Report
{

    //Detallado del reporte
    [DataContract(Namespace = "Entities.Report")] // 
    public class ReportDetailFormat
    {
        [DataMember]
        public virtual Int32 SeqLine { get; set; }
        [DataMember]
        public virtual String ProductCode { get; set; }
        [DataMember]
        public virtual String ProductDescription { get; set; }
        [DataMember]
        public virtual String Unit { get; set; }
        [DataMember]
        public virtual Double QtyOrder { get; set; }
        [DataMember]
        public virtual Double QtyPending { get; set; }
        [DataMember]
        public virtual Double QtyBO { get; set; }
        [DataMember]
        public virtual Double Weight { get; set; }
        [DataMember]
        public virtual String StockBin { get; set; }
        [DataMember]
        public virtual String BarcodeLabel { get; set; }
        [DataMember]
        public virtual String Date1 { get; set; }
        [DataMember]
        public virtual String Serial { get; set; }
        [DataMember]
        public virtual String Lote { get; set; }
        [DataMember]
        public virtual String PrintLot { get; set; }
        [DataMember]
        public virtual String Notes { get; set; }
        [DataMember]
        public virtual String Printed { get; set; }
        [DataMember]
        public virtual String UserName { get; set; }
        [DataMember]
        public virtual String LogisticNote { get; set; }
        [DataMember]
        public virtual String AccountItemNumber { get; set; }
        [DataMember]
        public virtual String AssignedBins { get; set; }
        [DataMember]
        public virtual String SuggestedBins { get; set; }
        [DataMember]
        public virtual Boolean? IsSubDetail { get; set; }
        [DataMember]
        public virtual Int32 AuxSequence { get; set; }
        [DataMember]
        public virtual String  CreatedBy { get; set; }
        [DataMember]
        public virtual Int32 CustNumber1 { get; set; }
        [DataMember]
        public virtual Int32 Rank { get; set; }
        [DataMember]
        public virtual String OutBin { get; set; }
        [DataMember]
        public virtual String OldestBin { get; set; }
        [DataMember]
        public virtual String AlternProduct { get; set; }

        [DataMember]
        public virtual String Custom1 { get; set; }
        [DataMember]
        public virtual String Custom2 { get; set; }
        [DataMember]
        public virtual String Custom3 { get; set; }


        //For Packages
        [DataMember]
        public virtual String DocNumber { get; set; }
        [DataMember]
        public virtual String ContactPerson { get; set; }
        [DataMember]
        public virtual String ShipAddress { get; set; }
        [DataMember]
        public virtual String Dimension { get; set; }
        [DataMember]
        public virtual Double PackWeight { get; set; }
        [DataMember]
        public virtual Double Pieces { get; set; }
        [DataMember]
        public virtual Double ProductCost { get; set; }
        [DataMember]
        public virtual Double ExtendedCost { get; set; }
        [DataMember]
        public virtual Int32 Sequence { get; set; }
        //[DataMember]
        //public virtual Int32 PackLevel { get; set; } //Define si es pallet, box inside pallet or box alone,.
        //[DataMember]
        //public virtual String PalletNote { get; set; }

    }
}
