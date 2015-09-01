using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;
using Entities.Master;

namespace Entities.Report
{
    /*
     * En esta entidad la idea es configurar una especie de triggers sobre las entidades
     * y enviar notificaciones a los afectado segun el evenyto creacion, update, delete.
     * or cambio de status
     * */

    [DataContract(Namespace = "Entities.Report")]
    public class MessageRuleByCompany
    {
        [DataMember]
        public virtual Int32 RowID { get; set; }
        [DataMember]
        public virtual Company Company { get; set; }

        [DataMember]
        public virtual String RuleName { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual String StrAttrib1 { get; set; }
        [DataMember]
        public virtual String StrAttrib2 { get; set; }
        [DataMember]
        public virtual String StrAttrib3 { get; set; }

        [DataMember]
        public virtual String IntAttrib1 { get; set; }
        [DataMember]
        public virtual String IntAttrib2 { get; set; }
        [DataMember]
        public virtual String IntAttrib3 { get; set; }


        [DataMember]
        public virtual DateTime? StartDate { get; set; }
        [DataMember]
        public virtual DateTime? EndDate { get; set; }
        [DataMember]
        public virtual DateTime? LastUpdate { get; set; }

        [DataMember]
        public virtual LabelTemplate Template { get; set; }

        [DataMember]
        public virtual String MailFrom { get; set; }
        [DataMember]
        public virtual String MailTo { get; set; }
        [DataMember]
        public virtual Boolean IsHtml { get; set; }
        [DataMember]
        public virtual Int16 RuleType { get; set; }  //1. Create, 2. Update, 3.Delete
        //[DataMember]
        public virtual IList<MessageRuleExtension> RuleExtensions { get; set; }
        [DataMember]
        public virtual Boolean? Active { get; set; }
        [DataMember]
        public virtual String Files { get; set; }

        //Adicionada para Business Alert
        [DataMember]
        public virtual DateTime? NextRunTime { get; set; } //La proxima vez que debe correr
        [DataMember]
        public virtual Int32 FrequencyNumber { get; set; } //Cuando o cada cuanto se ejecuta
        [DataMember]
        public virtual Int32 FrequencyType { get; set; } //Dias (86400), Hour (3600), Minutes (60), Seconds (1)


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MessageRuleByCompany castObj = (MessageRuleByCompany)obj;
            return (castObj != null) && (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
