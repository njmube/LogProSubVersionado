using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.Trace;

namespace Entities.Trace
{
    public class DocumentBalance
    {
        [DataMember]
        public virtual Document Document { get; set; }
        [DataMember]
        public virtual DocumentLine DocumentLine { get; set; }
        [DataMember]
        public virtual Node Node { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        [DataMember]
        public virtual Product Product { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; }
        [DataMember]
        public virtual Double Quantity { get; set; }
        [DataMember]
        public virtual Double QtyPending { get; set; }
        [DataMember]
        public virtual Double QtyProcessed
        {
            get { return Quantity - QtyPending; }
            set { }
        }


        [DataMember]
        public virtual Double BaseQuantity         {
            get { return Quantity * Unit.BaseAmount; }
            set { }
        }

        [DataMember]
        public virtual Double BaseQtyPending         {
            get { return QtyPending * Unit.BaseAmount; }
            set { }
        }

        [DataMember]
        public virtual Double BaseQtyProcessed
        {
            get { return QtyProcessed * Unit.BaseAmount; }
            set { }
        }



        [DataMember]  //25 Nov 09 / JM / Manejo de Precios de Venta
        public virtual Double UnitPrice { get; set; }

        [DataMember]
        //Sirve para almacenar informacion relevante al balance en ese momento 
        public virtual String Notes { get; set; }

        [DataMember]
        public virtual String DefaultProductBins //Los bin(s) por defecto de un producto                                    
        {
            get
            {
                return GetBins();
            }
            set { }
        }



        private string GetBins()
        {
            string result = "";
            int count = 0;

            if (Location == null || Product.ProductZones == null || Product.ProductZones.Count == 0)
                return result;

            foreach (ZoneEntityRelation ze in Product.ProductZones)
            {
                if (ze.Zone.Bins == null || ze.Zone.Bins.Where(f => f.Zone.Location.LocationID == Location.LocationID).Count() == 0)
                    break;

                foreach (ZoneBinRelation b in ze.Zone.Bins)
                {
                    result += b.Bin.BinCode + ", ";
                    count++;
                    if (count == WmsSetupValues.DefaultBinsToShow)
                        return result;
                }
            }
            return result;
        }
    }
}
    



