// File:    Label.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:46:13
// Purpose: Definition of Class Label

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Master;
using Entities;
using System.Linq;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]
    public class Label : Profile.Auditing
    {

        [DataMember]
        public virtual Int64 LabelID { get; set; }
        [DataMember]
        public virtual DocumentType LabelType { get; set; }
        [DataMember]
        public virtual String LabelCode { get; set; }
        [DataMember]
        public virtual Status Status { get; set; }
        [DataMember]
        public virtual Label FatherLabel { get; set; } //Aplica Cuando la caja esta contenida por un label Superior
        [DataMember]
        public virtual Product Product { get; set; }
        [DataMember]
        public virtual Unit Unit { get; set; } //Unidad de Presentacion
        //[DataMember]
        //public virtual Double UnitBaseFactor { get; set; } //Factor a la Unidad Basica
        [DataMember]
        public virtual String Manufacturer { get; set; }
        [DataMember]
        public virtual Double StartQty { get; set; } //Cantidades Iniciales de Unidad Basica
        [DataMember]
        public virtual Double CurrQty { get; set; } //Cantidades Actuales de Unidad Basica
        [DataMember]
        public virtual Node Node { get; set; }
        [DataMember]
        public virtual Bin Bin { get; set; }
        [DataMember]
        public virtual Bin LastBin { get; set; }
        [DataMember]
        public virtual Boolean? Printed { get; set; } //Dice si el label se comporta como virtal o realmente impreso
        [DataMember]
        public virtual String PrintingLot { get; set; }
        [DataMember]
        public virtual String Notes { get; set; }
        [DataMember]
        public virtual DateTime? ReceivingDate { get; set; }
        [DataMember]
        public virtual Document ReceivingDocument { get; set; }
        [DataMember]
        public virtual Document ShippingDocument { get; set; }
        [DataMember]
        public virtual Label LabelSource { get; set; } //Aplica Cuando la caja se piquea indicando de que label proviene


        [DataMember]
        public virtual Double BaseStartQty
        {
            get
            {
                try { return StartQty * Unit.BaseAmount; }
                catch { return 0; }
            }
            set { }
        }

        [DataMember]
        public virtual Double BaseCurrQty
        {
            get
            {
                try { return CurrQty * Unit.BaseAmount; }
                catch { return 0; }
            }
            set { }
        } 


        [DataMember]
        public virtual DocumentPackage Package
        {
            get
            {
                if (this.DocumentPackages != null && this.DocumentPackages.Count > 0) 
                    return this.DocumentPackages[0]; 
                else 
                    return null;
            }
            set { }
        } //Aplica Cuando la caja se piquea indicando de que label proviene



        [DataMember]
        public virtual IList<DocumentPackage> DocumentPackages { get; set; }

        [DataMember]
        public virtual IList<LabelTrackOption> TrackOptions { get; set; }

        [DataMember]
        public virtual IList<LabelMissingComponent> MissingComponents { get; set; }

        [DataMember]
        public virtual String SerialNumber
        {
            get {
                return LabelCode;
                //return GetTrackValue(STrackOptions.SerialNumber); 
            }
            set { }
        }

        [DataMember]
        public virtual String LotCode
        {
            get { return GetTrackValue(STrackOptions.LotCode); }
            set { }
        }


        [DataMember]
        public virtual DateTime? ExpirationDate
        {
            get
            {
                try { return DateTime.Parse(GetTrackValue(STrackOptions.ExpirationDate)); }
                catch { return null; }
            }
            set { }
        }


        [DataMember]
        public virtual Double StockQty { get; set; } //Mapped to Nhibernate Consulta

        [DataMember]
        public virtual String Name { get { return LabelCode; } set { } }

        //[DataMember]
        public virtual Boolean? IsLogistic { get; set; }


        [DataMember]
        public virtual String SerialMark {
            get
            {
                try
                {
                    if (LabelType.DocTypeID == Entities.LabelType.ProductLabel && Product.IsUniqueTrack)
                        return "S/N";
                }
                catch { }
                return "";
            }
            set { } }

        [DataMember]
        public virtual int ChildCount
        {
            get
            {
                if (this.ChildLabels != null)
                    try { return ChildLabels.Count; }
                    catch { }
                return 0;
            }
            set { }
        }

        [DataMember]
        public virtual String Barcode
        {
            get
            {
                if (LabelType == null)
                    return LabelCode;

                if (LabelType.DocTypeID == Entities.LabelType.ProductLabel || LabelType.DocTypeID == Entities.LabelType.CustomerLabel)
                {

                    try { return long.Parse(LabelCode).ToString().PadLeft(WmsSetupValues.LabelLength, '0'); }
                    catch { return LabelCode; }

                }

                if (LabelType.DocTypeID == Entities.LabelType.UniqueTrackLabel)
                {
                    //Genera el Barcode a partir de su ID, 
                    //si no tiene Serial el label Code se torna igual a Esta Opcion.
                    return '1'+ LabelID.ToString().PadLeft(WmsSetupValues.LabelLength - 1, '0'); 
                }

                return LabelCode;
            }
            set { }
        }

 
        public virtual IList<Label> ChildLabels { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Label castObj = (Label)obj;
            return (castObj != null) &&
                (this.LabelID == castObj.LabelID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.LabelID.GetHashCode();
        }

        private String GetTrackValue(short trackOp)
        {
            if (TrackOptions != null && TrackOptions.Where(f => f.TrackOption.RowID == trackOp).Count() > 0)
                return TrackOptions.Where(f => f.TrackOption.RowID == trackOp).First().TrackValue;
            else
                return "";
        }
    }
}