using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.General;

namespace Entities.Report
{
    [DataContract(Namespace = "Entities.Report")]
    public class MessagePool : Profile.Auditing
    {
        //RowID
        //From
        //    To
        //    Subject
        //        Message
        //    ModifiedDate = sentDate

        //Funcionamiento el sistema de mensajeria consiste en:
        //Mensajes a asociados a una entidad ejemplo documento.
        //Cada mensaje tiene una plantilla ej: Documento enviado al ERP
        //y un mapping que le dice de donde sale cada campo que poblara el mensaje.
        //Asi al hacer un mapping el mensaje queda creado.

        [DataMember]
        public virtual Int32 RowID { get; set; }

        [DataMember]
        public virtual String MailTo { get; set; }

        [DataMember]
        public virtual String MailFrom { get; set; }

        [DataMember]
        public virtual String Subject { get; set; }

        [DataMember]
        public virtual String Message { get; set; }

        [DataMember]
        public virtual Boolean IsHtml { get; set; }

        [DataMember]
        public virtual MessageRuleByCompany Rule { get; set; }

        [DataMember]
        public virtual Boolean AlreadySent { get; set; }

        [DataMember]
        public virtual ClassEntity Entity { get; set; }

        [DataMember]
        public virtual Int32 RecordID { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            MessagePool castObj = (MessagePool)obj;
            return (castObj != null) &&
                (this.RowID == castObj.RowID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.RowID.GetHashCode();
        }
    }
}
