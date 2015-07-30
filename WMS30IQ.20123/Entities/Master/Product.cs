// File:    Product.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:01:04
// Purpose: Definition of Class Product

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Process;
using System.Linq;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true
    public class Product : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 ProductID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }

        //Sep 04 3PL
        [DataMember]
        public virtual Contract Contract { get; set; }

        [DataMember]
        public virtual String ProductCode { get; set; }
        [DataMember]
        public virtual String Name { get; set; }
        [DataMember]
        public virtual String Description { get; set; }
        [DataMember]
        public virtual String Comment { get; set; }
        [DataMember]
        public virtual ProductCategory Category { get; set; }
        [DataMember]
        public virtual String UpcCode { get; set; }
        [DataMember]
        public virtual String DefVendorNumber { get; set; }
        [DataMember]
        public virtual String Brand { get; set; }
        [DataMember]
        public virtual String Reference { get; set; }
        [DataMember]
        public virtual String Manufacturer { get; set; }
        [DataMember]
        public virtual Double ProductCost { get; set; }
        [DataMember]
        public virtual Unit BaseUnit { get; set; }
        [DataMember]
        public virtual Unit SaleUnit { get; set; }
        [DataMember]
        public virtual Unit PurchaseUnit { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Boolean? IsKit { get; set; }
        [DataMember]
        public virtual Double MinStock { get; set; }
        [DataMember]
        public virtual Double MaxStock { get; set; }
        [DataMember]
        public virtual Double Weight { get; set; }
        [DataMember]
        public virtual Double Volume { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }
        [DataMember]
        public virtual Boolean? PrintLabel { get; set; }
        [DataMember]
        public virtual LabelTemplate DefaultTemplate { get; set; }
        [DataMember]
        public virtual String AssignedBins { get; set; }
        [DataMember]
        public virtual PickMethod PickMethod { get; set; }
        [DataMember]
        public virtual Int16 CountRank { get; set; }  //1=A(80%), 2=B(15%), 3=C(4%), 4=D(1%)
        [DataMember]
        public virtual Double AvgCost { get; set; }
        [DataMember]
        public virtual Int32 ForecastDemand { get; set; } //Forecast Demand in a Month

        
        [DataMember]
        public virtual String CountRankW { get {
            switch (CountRank)
            {
                case 1: return "A";
                case 2: return "B";
                case 3: return "C";
                case 4: return "D";
            }

            return "";
        
        } set { } }  //1=A(80%), 2=B(15%), 3=C(4%), 4=D(1%)


        [DataMember]
        public virtual String Quality //Ej: Hazmat
        {
            get
            {
                try
                {
                    return ProductTrack.Where(f => f.TrackOption.DataType.DataTypeID == SDataTypes.ProductQuality)
                        .First().TrackOption.Name;
                }
                catch { return ""; }

            }
            set { }
        }


        [DataMember]
        public virtual Boolean IsUniqueTrack
        {
            get
            {
                try
                {
                    return ProductTrack.Any(f => f.TrackOption.IsUnique == true);
                }
                catch { return false; }

            }
            set { }
        } 



        [DataMember]
        public virtual String SugesstedBins { get; set; }
        [DataMember]
        public virtual String FullDesc { 
            get { return this.ProductCode.Trim() + ", "+ this.Name;  }
            set { } 
        }

        [DataMember]
        public virtual Int32 UnitsPerPack { get; set; }

        [DataMember]
        public virtual Int16 ErpTrackOpt { get; set; } //1 - Normal, 2 - Serial, 3 - Lotnumber

        [DataMember]
        public virtual Boolean? IsBinRestricted { get; set; }


        [DataMember]
        public virtual IList<UnitProductRelation> ProductUnits { get; set; }
        
        [DataMember]
        public virtual IList<ProductTrackRelation> ProductTrack { get; set; }

        //[DataMember]
        public virtual IList<ZoneEntityRelation> ProductZones { get; set; }

        [DataMember]
        public virtual IList<ProductAccountRelation> ProductAccounts { get; set; }

        [DataMember]
        public virtual IList<ProductAlternate> AlternProducts { get; set; }

        //[DataMember]
        //public virtual IList<CustomProcessContextByEntity> ProcessContext { get; set; }

        //[DataMember]
        //public virtual IList<KitAssemblyFormula> ProductFormula { get; set; }

        [DataMember]
        public virtual IList<KitAssembly> Kit { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Product castObj = (Product)obj;
            return (castObj != null) &&
                (this.ProductID == castObj.ProductID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.ProductID.GetHashCode();
        }

    }
}