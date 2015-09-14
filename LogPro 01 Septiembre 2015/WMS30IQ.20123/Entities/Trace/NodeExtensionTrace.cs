// File:    Node.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 14:12:00
// Purpose: Definition of Class LabelTransaction

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Master;

namespace Entities.Trace
{
    [DataContract(Namespace = "Entities.Trace")]

    public class NodeExtensionTrace : Profile.Auditing
    {
        [DataMember]
        public virtual NodeTrace RowID { get; set; }

        [DataMember]
        public virtual NodeExtension NodeExtension { get; set; }

        [DataMember]
        public virtual String Data { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            NodeExtensionTrace castObj = (NodeExtensionTrace)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}