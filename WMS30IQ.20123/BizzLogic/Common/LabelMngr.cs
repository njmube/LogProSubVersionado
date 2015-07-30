using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Master;
using Entities.Trace;
using Entities.General;
using Entities.Profile;
using Entities;
using Integrator;
using System.IO;
using System.Reflection;

namespace BizzLogic.Logic
{
    //Maneja la logica de craeciond e labels e impresion.

    public partial class LabelMngr : BasicMngr
    {
        public WmsTypes WType;
        IList<Label> listOfLabels;

       public LabelMngr()
        {
            Factory = new DaoFactory();
            WType = new WmsTypes(Factory);
        }



       //Obtiene el Stock de un producto, Ubicacion, cantodad segun un metodo (ZONE, LIFO, FIFO) 
       public String GetStrProductStock(ProductStock productStock, PickMethod pickMethod)
       {
           //Query sobre label entity de tipo producto y activa, sacando la unidad base
           //Order tue query over  ZONE, FIFO, LIFO , FEFO segun el PickingMethod

           string result = "";

           //Label label = new Label
           //{
           //    Product = product,
           //    Bin = bin
           //};

           IList<ProductStock> list = Factory.DaoLabel().GetStock(productStock, pickMethod, 20);

           if (list == null || list.Count == 0)
               return "";

           foreach (ProductStock ps in list)
               result += ps.Bin.BinCode + "(" + ps.Stock.ToString() + "), ";

           return result;
       }



       public IList<Label> CreateEmptyPackLabel(Location location, int numLabels, Product product, string printSession,
           SysUser user)
       {
           Factory.IsTransactional = true;

           Node recNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
           Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
           DocumentType labelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });
           Unit logisticUnit = WType.GetUnit(new Unit { Company = location.Company, Name = WmsSetupValues.CustomUnit });
           Bin destLocation = WType.GetBin(new Bin { Location = location, BinCode = DefaultBin.PUTAWAY });


           IList<Label> listLabel = new List<Label>();

           //Generate new logistig labels located in MAIN
           //Labels shouldbe activated the next transaction
           try {
               //Funcion para obtener siguiente Label
               //DocumentTypeSequence initSequence = GetNextDocSequence(location.Company, labelType);
               Label fatherLabel = null;

               for (int i = 0; i < (int)numLabels; i++)
               {

                   fatherLabel = new Label();
                   fatherLabel.Node = recNode;
                   fatherLabel.Bin = destLocation;
                   fatherLabel.Product = product;
                   fatherLabel.CreatedBy = user.UserName;
                   fatherLabel.Status = status;
                   fatherLabel.LabelType = labelType;
                   fatherLabel.CreationDate = DateTime.Now;
                   fatherLabel.Printed = false;

                   fatherLabel.Unit = logisticUnit;
                   fatherLabel.IsLogistic = false;
                   fatherLabel.FatherLabel = null;
                   fatherLabel.LabelCode = ""; //(initSequence.NumSequence + i).ToString() + GetRandomHex(user.UserName, initSequence.NumSequence + i);
                   fatherLabel.CurrQty = 0;
                   //Aqui guardan las logisitcas las hijas que contienen unas vez se imprimen
                   fatherLabel.StartQty = 0;
                   fatherLabel.Notes = "Empty Label";
                   fatherLabel.PrintingLot = printSession;
                   fatherLabel = Factory.DaoLabel().Save(fatherLabel);

                   //Registra el movimiento del nodo
                   fatherLabel.LabelCode = fatherLabel.LabelID.ToString();

                   SaveNodeTrace(
                       new NodeTrace
                       {
                           Node = recNode,
                           Document = null,
                           Label = fatherLabel,
                           Quantity = fatherLabel.CurrQty,
                           IsDebit = false,
                           CreatedBy = user.UserName
                       }
                   );

                   listLabel.Add(fatherLabel);

               }

               //Ajustando la sequencia
               //initSequence.NumSequence += numLabels;

               //Factory.DaoDocumentTypeSequence().Update(initSequence);

               Factory.Commit();

               return listLabel;

           }
           catch (Exception ex)
           {
               Factory.Rollback();
               ExceptionMngr.WriteEvent("CreateEmptyLabel:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
               throw new Exception(WriteLog.GetTechMessage(ex));
           }


       }


       public IList<Label> GenerateLabelsToPrint(IList<DocumentBalance> printList, String printLot, UserByRol userByRol)
       {
           Factory.IsTransactional = true;

           this.listOfLabels = new List<Label>();
           Node node = WType.GetNode(new Node { NodeID = NodeType.PreLabeled });
           Bin bin = WType.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = userByRol.Location });




           try
           {
               foreach (DocumentBalance printLine in printList)
                   ProcessPrintingLine(printLine, printLot, node, bin, userByRol);

               Factory.Commit();
           }
           catch (Exception ex)
           {
               Factory.Rollback();
               ExceptionMngr.WriteEvent("GenerateLabelsToPrint:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
               throw new Exception(WriteLog.GetTechMessage(ex));
           }

           return (this.listOfLabels.Count > 0) ? this.listOfLabels : null;
       }


       private void ProcessPrintingLine(DocumentBalance printLine, String printLot, Node node, Bin bin, UserByRol userByRol)
       {
           //Tipo De impresion
           //1. Normal Imprime standar, sin logistica

           //2. Logistic (Notes tiene data) - imprime normal mas la Logistica
           Unit logisticUnit = null;
           //bool printOnlyLogistic = false;


           if (printLine.Notes != null && printLine.Notes.Contains("Pack"))
           {
               string[] dataLogistic = printLine.Notes.Split(':');

               //El elemento [1] contiene la unidad logistica, si es cero es custom.
               if (dataLogistic[2].Equals(WmsSetupValues.CustomUnit))
               {
                   try
                   {   //trata de encontrar una unidad con ese baseamount
                       logisticUnit = Factory.DaoUnit().Select(
                           new Unit
                           {
                               BaseAmount = printLine.QtyPending,
                               Company = userByRol.Location.Company,
                               ErpCodeGroup = printLine.Unit.ErpCodeGroup
                           }
                        ).First();
                   }
                   catch
                   {
                       //Obtiene la custom
                       logisticUnit = Factory.DaoUnit().SelectById(new Unit { UnitID = int.Parse(dataLogistic[1]) });
                   }
               }
               else  // Si no es Custom
               {
                   logisticUnit = Factory.DaoUnit().SelectById(new Unit { UnitID = int.Parse(dataLogistic[1]) });
               }

           }

           //Se ingresa para poder sacar el dato de la company
           if (printLine.Document == null)
               printLine.Document = new Document { Company = userByRol.Location.Company };

           //CReating Document Line to Send
           DocumentLine prnLine = new DocumentLine
           {
               Product = printLine.Product,
               Document = printLine.Document,
               Unit = printLine.Unit,
               Quantity = printLine.Quantity,
               CreatedBy = userByRol.User.UserName

           };

           if (logisticUnit != null)
                logisticUnit.BaseAmount = printLine.QtyPending;

           //Obteniendo el factor logistico antes de enviar a crear los labels
           double logisticFactor = (logisticUnit != null) ? printLine.QtyPending : 1;
           //Manda a Crear los Labels
           IList<Label> resultList = CreateProductLabels(logisticUnit, prnLine, node, bin, logisticFactor, printLot,"", DateTime.Now)
               .Where(f => f.FatherLabel == null).ToList();

           foreach (Label lbl in resultList)
               this.listOfLabels.Add(lbl);


       }


       public void CloseDocumentPackage(DocumentPackage newPack)
       {
           try
           {

               //Close package and his childs.
               Factory.IsTransactional = true;

               newPack = Factory.DaoDocumentPackage().Select(new DocumentPackage { PackID = newPack.PackID }).First();
               newPack.IsClosed = true;
               Factory.DaoDocumentPackage().Update(newPack);

               foreach (DocumentPackage child in newPack.ChildPackages)
               {
                   child.IsClosed = true;
                   Factory.DaoDocumentPackage().Update(child);
               }

               Factory.Commit();
           }
           catch { Factory.Rollback(); throw; }

       }
    }
}
    