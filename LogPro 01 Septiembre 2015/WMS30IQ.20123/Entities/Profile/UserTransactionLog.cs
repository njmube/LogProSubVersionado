// File:    UserTransactionLog.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:12:01
// Purpose: Definition of Class UserTransactionLog

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.Master;
using Entities.General;


namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile")]

    public class UserTransactionLog : Auditing
    {
        [DataMember]
        public virtual Int64 RowID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }
        [DataMember]
        public virtual Location Location { get; set; }

        [DataMember]
        public virtual Terminal Terminal { get; set; }
        [DataMember]
        public virtual DocumentType DocType { get; set; }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            UserTransactionLog castObj = (UserTransactionLog)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}