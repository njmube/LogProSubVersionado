using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.Trace;


/* Esta entidad entrega las direfencias  de inventario encontradas en un conteo */

namespace Entities.Trace
{
    public class CountTaskBalance
    {
        [DataMember]
        public virtual Document CountTask { get; set; }

        [DataMember]
        public virtual BinByTask BinByTask { get; set; }

        [DataMember]
        public virtual Bin Bin { get; set; }

        [DataMember]
        public virtual Label Label { get; set; }

        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual Double QtyCount { get; set; }

        [DataMember]
        public virtual Unit UnitCount { get; set; }

        [DataMember]
        public virtual Double QtyExpected { get; set; }

        [DataMember]
        public virtual Unit UnitExpected { get; set; }

        [DataMember]
        public virtual Double Difference
        {
            get { return QtyCount * UnitCount.BaseAmount - QtyExpected*UnitExpected.BaseAmount; }
            set { }
        }

        [DataMember]
        public virtual Boolean   Mark { get; set; }

        [DataMember]
        public virtual Int32 CaseType { get; set; } //Indica si es un registrio contado o es un Missing

        [DataMember]
        public virtual String QtyCountDesc { get { return QtyCount.ToString() + " " + UnitCount.Name; } set{} }

        [DataMember]
        public virtual String QtyExpectedDesc { get { return QtyExpected.ToString() + " " + UnitExpected.Name; } set { } }

        [DataMember]
        public virtual String LabelCode
        {
            get
            {
                if (Label != null) return Label.LabelCode; else return ""; //"UNLABELED";
            }
            set { }
        }

        [DataMember]
        public virtual String Sign
        {
            get
            {
                if (Difference > 0) return "[ + ]";
                if (Difference < 0) return "[ - ]";
                return "";
            }
            set { }
        }


        [DataMember]
        public virtual String Comment
        {
            get
            {
                switch (CaseType)
                {
                    /*
                    case 1:
                        if (QtyCount == 0) return "No counted. " + Sign; //Expected/
                        break;

                    case 2: return "Not expected. " + Sign;

                    case 3:
                        if (Label.Bin != null && Label.Bin.BinID != Bin.BinID)
                            return "Not expected. Move to " + Bin.BinCode + ". " + Sign; //Barcode not Expected. 
                        break;

                    case 4: return "No counted. Move to NOCOUNT. " + Sign; //Label Expected/
                     */
 
                    case 1:
                        if (QtyCount == 0) return "No counted. " + Sign; //Expected/
                        break;

                    case 2: return "Not expected. " + Sign;

                    case 3: return "No counted. " + Sign;

                    case 4:
                        if (QtyCount == 0) return "No counted. " + Sign;
                        break;

                    case 5:
                        if (Label.Bin != null && Label.Bin.BinID != Bin.BinID)
                            return "Not expected. Move to " + Bin.BinCode + ". " + Sign; 
                        break;

                    case 6:
                            return "Not Counted. Move to NOCOUNT. " + Sign;

                }

                return Sign;

            }
            set { }
        }
    }
}
    



