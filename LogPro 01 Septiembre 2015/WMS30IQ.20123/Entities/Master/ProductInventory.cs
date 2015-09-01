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
using Entities.Trace;

namespace Entities.Master
{
    [DataContract(Namespace = "Entities.Master", IsReference = true)] //, IsReference = true
    public class ProductInventory : Profile.Auditing
    {

        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual Location Location { get; set; }

        [DataMember]
        public virtual Document Document { get; set; }

        [DataMember]
        public virtual Product Product { get; set; }

        [DataMember]
        public virtual Double QtyInStock { get; set; } //Es virtual viene de la DB no se persiste.

        [DataMember]
        //En uso actualmente por el programa pero aun no ha sido confirmado
        public virtual Double QtyInUse { get; set; } //Persiste.

        [DataMember]
        //Cantidad ya confirmada
        public virtual Double QtyAllocated { get; set; }  //Persiste

        [DataMember]
        public virtual Double QtyAvailable
        {
            get
            {
                if (QtyInStock - QtyAllocated - QtyInUse > 0)
                    return QtyInStock - QtyAllocated - QtyInUse;
                else
                    return 0;
            }
            set { }
        }




        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ProductInventory castObj = (ProductInventory)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}