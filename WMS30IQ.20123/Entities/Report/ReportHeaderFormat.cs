using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Report
{
    //Encabezado del reporte
    [DataContract(Namespace = "Entities.Report")]  
    public class ReportHeaderFormat
    {
        [DataMember]
        public virtual String DocumentName { get; set; }
        [DataMember]
        public virtual String DocumentNumber { get; set; }
        [DataMember]
        public virtual String OrigNumber { get; set; }
        [DataMember]
        public virtual String CustPONumber { get; set; }

        [DataMember]
        public virtual String Warehouse { get; set; }
        [DataMember]
        public virtual String Vendor { get; set; }
        [DataMember]
        public virtual String VendorAccount { get; set; }
        [DataMember]
        public virtual String Customer { get; set; }
        [DataMember]
        public virtual String CustomerAccount { get; set; }
        [DataMember]
        public virtual String Notes { get; set; }
        [DataMember]
        public virtual String Printed { get; set; }
        [DataMember]
        public virtual String FilterBy { get; set; }
        [DataMember]
        public virtual String ShipVia { get; set; }

        //Custom fileds
        [DataMember]
        public virtual String Reference { get; set; }
        //Custom fileds
        [DataMember]
        public virtual String Comment { get; set; }
        [DataMember]
        public virtual String UserDef1 { get; set; }
        [DataMember]
        public virtual String UserDef2 { get; set; }
        [DataMember]
        public virtual String UserDef3 { get; set; }



        [DataMember]
        public virtual String Corporate_Name { get; set; }
        [DataMember]
        public virtual String Corporate_Line1 { get; set; }
        [DataMember]
        public virtual String Corporate_Line2 { get; set; }
        [DataMember]
        public virtual String Corporate_Line3 { get; set; }
        [DataMember]
        public virtual String Corporate_Line4 { get; set; }
        [DataMember]
        public virtual String Corporate_Line5 { get; set; }
        [DataMember]
        public virtual String Corporate_Line6 { get; set; } //Phones, Contact Person


        [DataMember]
        public virtual String BillTo_Name { get; set; }
        [DataMember]
        public virtual String BillTo_Line1 { get; set; }
        [DataMember]
        public virtual String BillTo_Line2 { get; set; }
        [DataMember]
        public virtual String BillTo_Line3 { get; set; }
        [DataMember]
        public virtual String BillTo_Line4 { get; set; }
        [DataMember]
        public virtual String BillTo_Line5 { get; set; }

        [DataMember]
        public virtual String ShipTo_Name { get; set; }
        [DataMember]
        public virtual String ShipTo_Line1 { get; set; }
        [DataMember]
        public virtual String ShipTo_Line2 { get; set; }
        [DataMember]
        public virtual String ShipTo_Line3 { get; set; }
        [DataMember]
        public virtual String ShipTo_Line4 { get; set; }
        [DataMember]
        public virtual String ShipTo_Line5 { get; set; }

        [DataMember]
        public virtual String Date1 { get; set; }
        [DataMember]
        public virtual String Date2 { get; set; }
        [DataMember]
        public virtual String Date3 { get; set; }
        [DataMember]
        public virtual String Date4 { get; set; }

        [DataMember]
        public virtual Byte[] Image1 { get; set; }
        [DataMember]
        public virtual String Image2 { get; set; }
        [DataMember]
        public virtual String Image3 { get; set; }
        [DataMember]
        public virtual String Barcode { get; set; }

        // Totals
        [DataMember]
        public virtual Double TotalWeight { get; set; }
        [DataMember]
        public virtual Double TotalPallets { get; set; }
        [DataMember]
        public virtual Double TotalCases { get; set; }
        [DataMember]
        public virtual Double TotalQtyOrder { get; set; }
        [DataMember]
        public virtual Double TotalQtyPending { get; set; }
        [DataMember]
        public virtual String CreatedBy { get; set; }
        [DataMember]
        public virtual String ProductToBuild { get; set; }
        [DataMember]
        public virtual String PickMethod { get; set; }
        [DataMember]
        public virtual Double AllCases { get; set; }
        [DataMember]
        public virtual Double TotalExtended { get; set; }

        [DataMember]
        public virtual List<ReportDetailFormat> ReportDetails { get; set; }


    }

}
