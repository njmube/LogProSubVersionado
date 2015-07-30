using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities.Master;
using Entities.Trace;
using Entities.Process;

namespace Entities.General
{
    //USada en presentacion WPF para desplegar Valores de un Objeto
    [DataContract(Namespace = "Entities.General")]
   public class ShowData
   {
       [DataMember]
       public String DataKey { get; set; }
       [DataMember]
       public String DataValue { get; set; }

       public override Boolean Equals(object obj)
       {
           if ((obj == null) || (obj.GetType() != this.GetType())) return false;
           ShowData castObj = (ShowData)obj;
           return (castObj != null) &&
               (this.DataKey == castObj.DataKey);
       }

       public override Int32 GetHashCode()
       {
           return 9 * 3 * this.DataKey.GetHashCode();
       }
   }


    [DataContract(Namespace = "Entities.General")]
    public class ProcessResponse
    {
        [DataMember]
        public virtual Int32 MessageID { get; set; }
        [DataMember]
        public virtual String Message { get; set; }
    }


   [DataContract(Namespace = "Entities.General")]
   public class ProductStock
   {
       [DataMember]
       public virtual Product Product { get; set; }

       [DataMember]
       public virtual Unit Unit { get; set; }

       [DataMember]
       public virtual Bin Bin { get; set; }

       [DataMember]
       public virtual Int16 BinType { get; set; }

       [DataMember]
       public virtual Zone Zone { get; set; }

       [DataMember]
       public virtual Label Label { get; set; }

       [DataMember]
       public virtual Double Stock { get; set; }

       [DataMember]
       public virtual Double PackStock { get; set; }

       [DataMember]
       public virtual DateTime? MaxDate { get; set; }

       [DataMember]
       public virtual DateTime? MinDate { get; set; }

       [DataMember]
       public virtual Double MinStock { get; set; }

       [DataMember]
       public virtual Double MaxStock { get; set; }

       [DataMember]
       public virtual String BinTypeDesc { 
           get { 
               if (this.BinType == 1) return "In only";
               if (this.BinType == 2) return "Out only";
               return "In/Out";
           } 
           set { } }

       [DataMember]
       public virtual Boolean Mark { get; set; }

       [DataMember]
       public virtual Double AuxQty1 { get; set; }

       [DataMember]
       public virtual Double FullStock
       {
           get { try { return PackStock + Stock; } catch { return 0; } }
           set { }
       }

   }


   [DataContract(Namespace = "Entities.General")]
   public class Printer
   {
       [DataMember]
       public String PrinterName { get; set; }
       [DataMember]
       public String PrinterPath { get; set; }
   }


   public class BatchPrintProcess
   {
      // documentList, appPath, printer, process
       public List<Document> DocumentList { get; set; }
       public string AppPath { get; set; }
       public string Printer { get; set; }
       public CustomProcess Process { get; set; }
   }

}
