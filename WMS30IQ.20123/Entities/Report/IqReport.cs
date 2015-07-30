using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Entities.Profile;
using Entities.General;

namespace Entities.Report
{
    /// <summary>
    /// Report object for mapped table Reports.
    /// </summary>
    [DataContract]
    public class IqReport : Auditing
    {

        [DataMember]
        public virtual Int32 ReportId { get; set; }

        [DataMember]
        public virtual String Name { get; set; }

        [DataMember]
        public virtual Status Status { get; set; }

        [DataMember]
        public virtual Boolean? IsForSystem { get; set; }

        [DataMember]
        public virtual String QueryString { get; set; }

        [DataMember]
        public virtual String ReportDesc
        {
            get { return Name + " " + ((IsForSystem==true) ? "(System)" : "(Custom by " + this.CreatedBy + ")"); }
            set{}
        
        }

        [DataMember]
        public virtual String Process { get; set; } 
        //Aqui se manejaran los proceso que se pueden ejecutar sobre el reporte
        //Como Print In Batch, Schedule Count, PO Creation y Muchos mas.


        [DataMember]
        public virtual String ProcessControl { get; set; } 
        //Objeto a abrir asociado al reporte

        [DataMember]
        public virtual String ProcessParams { get; set; } 
        //Parametros a pasar al control

        [DataMember]
        public virtual String PermitCode { get; set; } 
        //Permit string - String en el modulo de permisos que indica quien puede ejecutar
        //El shotcut que ofrece el reporte



        [DataMember]
        public virtual String Settings { get; set; } 

        [DataMember]
        public virtual IList<IqReportTable> ReportTables { get; set; }


        public override Int32 GetHashCode()
        {
            return 3 * 3 * this.ReportId.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            IqReport castObj = (IqReport)obj;
            return (castObj != null) && (this.ReportId == castObj.ReportId);
        }
    }
}