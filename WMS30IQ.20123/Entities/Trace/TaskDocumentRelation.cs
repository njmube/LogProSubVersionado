// File:    ReceivingTransactionDocument.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:59:14
// Purpose: Definition of Class ReceivingTransactionDocument
// ReceivingTask, PickTicket, InventoryTask

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class TaskDocumentRelation : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Document TaskDoc { get; set; }
        [DataMember]
        public virtual Document IncludedDoc { get; set; }



        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            TaskDocumentRelation castObj = (TaskDocumentRelation)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}