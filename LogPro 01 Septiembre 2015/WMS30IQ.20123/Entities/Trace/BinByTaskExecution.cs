// File:    ReceivingTransactionDocument.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:59:14
// Purpose: Definition of Class ReceivingTransactionDocument
// ReceivingTask, PickTicket, InventoryTask

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Profile;
using Entities.Master;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class BinByTaskExecution : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        
        [DataMember]
        public virtual BinByTask BinTask { get; set; }

        [DataMember]
        public virtual Product  Product { get; set; }


        [DataMember]
        public virtual Bin Bin { get; set; }

        [DataMember]
        public virtual Label StockLabel { get; set; }

        [DataMember]
        public virtual Double QtyExpected { get; set; }

        [DataMember]
        public virtual Double QtyCount { get; set; }


        [DataMember]
        public virtual Unit UnitCount { get; set; }


        [DataMember]
        public virtual Double QtyDiff {  //Esta en unidad BASE
            get { return QtyCount * UnitCount.BaseAmount - QtyExpected; }
            set{} 
        }

        [DataMember]
        public virtual Double BaseQtyCount
        {  //Esta en unidad BASE
            get { return QtyCount * UnitCount.BaseAmount; }
            set { }
        }

        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual Int16 BinTaskSequence { get; set; }

        [DataMember]
        public virtual Int16 Sequence { get; set; }

        [DataMember]
        public virtual String CountSession { get; set; }

        [DataMember]
        public virtual String Comment { get; set; }

        [DataMember]
        public virtual Boolean Mark { get; set; }

        [DataMember]
        public virtual String LabelCode { get{

            if (StockLabel == null || StockLabel.LabelID == 0)
                return "UnLabeled";
            else
                return StockLabel.LabelCode;

        } set{} }

        [DataMember]
        public virtual Boolean Posteable { get{

            if (this.QtyDiff == 0)
                return false;

            if (this.Status.StatusID != DocStatus.Completed)
                return false;

            else
                return true;


        } set{} }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;

            BinByTaskExecution castObj = (BinByTaskExecution)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}