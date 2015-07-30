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

    public class BinByTask : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Document TaskDocument { get; set; }

        [DataMember]
        public virtual Bin  Bin { get; set; }

        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual Int16 Sequence { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }


        [DataMember]
        public virtual string ProductDesc
        {
            get { if (Product != null) return Product.FullDesc; return "Any Product"; }
            set { }
        }

        [DataMember]
        public virtual string BinDesc
        {
            get { if (Bin != null) return Bin.BinCode; return "Any Bin"; }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;

            BinByTask castObj = (BinByTask)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}