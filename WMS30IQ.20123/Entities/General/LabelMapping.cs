using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Master;

namespace Entities.General
{
    
    /// <summary>
    /// Contiene la relacion entre el objeto de label y el dato que debe reemplace en la plantilla
    /// </summary>
    [DataContract(Namespace = "Entities.General")]
    public class LabelMapping : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual DocumentType LabelType { get; set; }
        [DataMember]
        public virtual  Account Account { get; set; } //If Customer Label
        [DataMember]
        public virtual String DataKey { get; set; }
        [DataMember]
        public virtual String DataValue { get; set; }
        [DataMember]
        public virtual String Description { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            LabelMapping castObj = (LabelMapping)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
/*
        PRODUCT LABEL
 *      ----------------------------------------
        PRODUCT, Label.Product.ProductCode
 *      PRODUCTNAME, Label.Product.Name
        UNIT, Label.Unit.Name
 *      UNITBASE, Label.Unit.BaseAmount
        LOTE, Label.LotCode
        EXPDATE, Label.ExpirationDate
 *      SERIAL, Label.SerialNumber
 *      STARQTY,Label.StartQty, or Label.ChildCount (When is Logistic)
 *      PRINT, Label.PrintingLot
 *      NOTES, Label.Notes
 *      PRINTDATE, DateTime.Now()
 *      BARCODE, Label.Barcode
 *      USER, Label.CreatedBy
 
 * 
 *      BIN LABEL
 *      ----------------------------------------
 *      PRINT, Label.PrintingLot
 *      NOTES, Label.Notes
 *      PRINTDATE, DateTime.Now()
 *      BARCODE, Label.Barcode
 *      USER, Label.CreatedBy
*/