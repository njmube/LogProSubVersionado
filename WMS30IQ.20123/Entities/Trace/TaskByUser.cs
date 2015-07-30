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

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class TaskByUser : Profile.Auditing
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Document TaskDocument { get; set; }
        [DataMember]
        public virtual SysUser User { get; set; }
        [DataMember]
        public virtual String DisplayName {
            get
            { return "(" + TaskDocument.Priority + ") " + TaskDocument.DocNumber +", Assg: " + CreationDate.ToString().Substring(0,10) ; } 
            set { } 
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            TaskByUser castObj = (TaskByUser)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}