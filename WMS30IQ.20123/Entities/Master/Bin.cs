// File:    Bin.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:11:56
// Purpose: Definition of Class Bin

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Process;
using Entities.Trace;


namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true


    public class Bin : Profile.Auditing
    {  
        [DataMember]
        public virtual Int32 BinID { get; set; }        
        [DataMember]
        public virtual Zone Zone { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }
        [DataMember] 
        public virtual String BinCode { get; set; }
        [DataMember] 
        public virtual String LevelCode { get; set; }
        [DataMember] 
        public virtual String Aisle { get; set; }
        [DataMember]
        public virtual MeasureUnit WeightUnit { get; set; } //UnitofMeasure
        [DataMember]
        public virtual Double WeightCapacity { get; set; }
        [DataMember]
        public virtual MeasureUnit VolumeUnit { get; set; } //UnitofMeasure
        [DataMember] 
        public virtual Double VolumeCapacity { get; set; }
        [DataMember] 
        public virtual String Description { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember] 
        public virtual Int32 Rank { get; set; }
        [DataMember]
        public virtual Boolean? IsArea { get; set; }
        [DataMember]
        public virtual Boolean? IsFromErp { get; set; }
        
        [DataMember]
        public virtual Double UnitCapacity { get; set; }

        [DataMember]
        public virtual String Comment { get; set; }
        
        [DataMember]
        public virtual Double MinUnitCapacity { get; set; }
        
        [DataMember]
        public virtual Double CurrentOcupancy { get; set; }

        [DataMember]
        public virtual String AssignedProducts { get; set; }
        
        [DataMember]
        public virtual String OcupancyRate {
            get
            {
                if (this.CurrentOcupancy > 0 && this.UnitCapacity > 0)
                    return (100 * this.CurrentOcupancy / this.UnitCapacity).ToString("#,###.##") + "%";
                
                return "";

            }
            set { } 
        }

        [DataMember]
        public virtual String Name { 
            get { return BinCode; } 
            set { } }

        [DataMember]
        public virtual CustomProcess Process { get; set; }

        //[DataMember]
        public virtual IList<Label> LabelRef { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Bin castObj = (Bin)obj;
            return (castObj != null) &&
                (this.BinID == castObj.BinID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.BinID.GetHashCode();
        }

    }
}