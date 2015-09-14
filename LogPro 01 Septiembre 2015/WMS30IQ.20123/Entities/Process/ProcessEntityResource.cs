using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Profile;
using Entities.Master;

namespace Entities.Process
{
    /// <summary>
    /// Administra los recursos, templates or file asociados aun proceso por tipo de entidad.
    /// </summary>

    [DataContract(Namespace = "Entities.Process", IsReference = true)]
    public class ProcessEntityResource : Auditing 
    {

        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; } 

        [DataMember]
        public virtual CustomProcess Process { get; set; }  //Process

        [DataMember]
        public virtual Int32 EntityRowID { get; set; } //ID de la entidad producto documento, etc

        [DataMember]
        public virtual LabelTemplate Template { get; set; }

        [DataMember]
        public virtual ImageEntityRelation File { get; set; }


        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual Connection Printer { get; set; }

        [DataMember]
        public virtual String DisplayName
        {
            get
            {
                if (File != null) 
                    return File.ImageName;
                else if (Template != null)
                    return Template.Name;
                else
                    return "";
            }
            set { }
        }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            ProcessEntityResource castObj = (ProcessEntityResource)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }

    }
}
