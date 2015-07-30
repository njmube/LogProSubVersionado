using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using Entities.Profile;
using Integrator.Dao;
using Integrator;


namespace BizzLogic.Logic
{
    public class DocumentMngr : BasicMngr
    {
        //private DaoFactory Factory { get; set; }
        private WmsTypes WType { get; set; }
        private Rules Rules { get; set; }


        public DocumentMngr()
        {
            //Factory = new DaoFactory();
            WType = new WmsTypes(Factory);
            Rules = new Rules(Factory);

        }


        public Document CreateNewDocument(Document data, Boolean autocommit)
        {

            //Start Transaction
            Factory.IsTransactional = true;

            //Asigna el consecutivo obtenido a el documento
            if (string.IsNullOrEmpty(data.DocNumber))
                data.DocNumber = GetNextDocSequence(data.Company, data.DocType).CodeSequence;

            //Informacion del Documento
            Account defaultAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
            data.Customer = (data.Customer == null) ? defaultAccount : data.Customer;
            data.Vendor = (data.Vendor == null) ? defaultAccount : data.Vendor;

            data.DocStatus = (data.DocStatus == null) ? WType.GetStatus(new Status { StatusID = DocStatus.New }) : data.DocStatus;
            
            if (data.DocConcept == null)
                data.DocConcept = WType.GetDefaultConcept(data.DocType.DocClass);
            
            data.CreationDate = DateTime.Now;
            data.CreatedBy = (data.CreatedBy != null) ? data.CreatedBy : "";
            data.PickMethod = data.DocType.PickMethod;

            //Salva el documento nuevo y lo retorna
            data = Factory.DaoDocument().Save(data);

            if (autocommit)
                Factory.Commit();

            data.DocumentLines = new List<DocumentLine>();
            return data;

        }



        public Document CreateNewTaskDocument(IList<Document> docList, Document taskDocument)
        {
            Factory.IsTransactional = true;

            //Crea un nuevo documento tipo Task            
            taskDocument = CreateNewDocument(taskDocument, false);

            //Start Transaction

            //Asocia los documentos del Task a TaskDocument
            TaskDocumentRelation taskDocRel;
            foreach (Document curDoc in docList)
            {
                taskDocRel = new TaskDocumentRelation
                {
                    TaskDoc = taskDocument,
                    IncludedDoc = curDoc,
                    CreatedBy = "",
                    CreationDate = DateTime.Now
                };

                Factory.DaoTaskDocumentRelation().Save(taskDocRel);

                //Actualizando el Status del documento a In Process
                curDoc.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
                Factory.DaoDocument().Update(curDoc);

            }


            IList<DocumentLine> taskDocLines = new List<DocumentLine>();
            DocumentLine curLine;

            int i = 1;

            foreach (Object[] obj in Factory.DaoDocument().GetDocumentConsolidation(taskDocument))
            {
                curLine = new DocumentLine();
                curLine.Document = taskDocument;
                curLine.Quantity = (Double)obj[0];
                curLine.QtyCancel = 0;
                curLine.Product = Factory.DaoProduct().SelectById(new Product { ProductID = (int)obj[1] });
                curLine.Unit = Factory.DaoUnit().SelectById(new Unit { UnitID = (int)obj[2] }); ;
                curLine.UnitBaseFactor = (Double)obj[3];
                curLine.LineNumber = i++;
                curLine.CreationDate = DateTime.Now;
                curLine.CreatedBy = taskDocument.CreatedBy;
                curLine.LineStatus = Factory.DaoStatus().SelectById(new Status { StatusID = 101 }); ;
                curLine.Location = taskDocument.Location;
                curLine.IsDebit = false;

                taskDocLines.Add(curLine);
            }

            taskDocument.DocumentLines = taskDocLines;

            //Saving Document task with Lines
            Factory.DaoDocument().Update(taskDocument);

            Factory.Commit();

            return taskDocument;

        }



        //Get document that mach with products of the Receiving Document
        public IList<Document> GetCrossDockSalesOrders(Document data)
        {
            Factory.IsTransactional = true;
            //Sales Doc Type
            DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };

            //Purchase Products
            IList<Product> purchaseProducts = Factory.DaoDocumentLine()
                .Select(new DocumentLine { Document = data} ).Select(f => f.Product).Distinct().ToList();

            //Pending CrossDock List
            IList<Document> listDoc = Factory.DaoDocument().SelectPendingCrossDock(
                new Document { DocType = docType, Company = data.Company }, purchaseProducts).Distinct().ToList();

            Factory.Commit();

            return listDoc;


            ////Document Lines to make the Join
            //IList<DocumentLine> salesLineList = new List<DocumentLine>();
            //foreach (Document doc in salesDocList)
            //{
            //    if (doc.DocumentLines == null)
            //        continue;

            //    foreach (DocumentLine line in doc.DocumentLines)
            //        salesLineList.Add(line);
            //}

            ////Purchase Document Lines
            //IList<DocumentLine> purchaseLineList = Factory.DaoDocument().Select(data).First().DocumentLines;

            //IList<Document> returnList = purchaseLineList
            //    .Join<DocumentLine, DocumentLine, int, Document>
            //    (salesLineList, purchase => purchase.Product.ProductID, sales => sales.Product.ProductID, 
            //    (purchase, sales) => sales.Document ).Distinct().ToList(); 

            ////Join Operation to Get Cross Sales Orders
            //var selectedDocs =
            //        from purchase in purchaseLineList
            //        join sales in salesLineList 
            //        on purchase.Product.ProductID equals sales.Product.ProductID
            //        select sales.Document; //produces flat sequence

            //return selectedDocs as IList<Document>;

            //return returnList;
        }



        //Get the crossdok balace comparar purchase docuemnt against sales documents
        public IList<DocumentBalance> GetCrossDockBalance(DocumentBalance purchaseBalance, IList<Document> salesDocs)
        {

            if (salesDocs == null || salesDocs.Count == 0)
                throw new Exception("No sales document availables for Crossdocking.");


            //Checking if trackoptions are all populated in the receiving document.
            ValidateReceiptDocumentTrackingOptions(purchaseBalance.Document, new Node {NodeID = NodeType.Received }, true);


            Node pickNode = Factory.DaoNode().Select(new Node { NodeID = NodeType.Picked }).First();

            //get Document Balance for the purchase
            IList<DocumentBalance> purchase = Factory.DaoDocumentBalance().GeneralBalance(purchaseBalance, true);

            //En Qty Pending grabamos lo que queda, para procesar 
            //Ese dato es el que disminuye en el cada que se cruza contra una orden
            for (int i = 0; i < purchase.Count; i++)
                purchase[i].QtyPending = purchase[i].QtyProcessed;           

            //Create the acumulator document balance
            IList<DocumentBalance> resultBalance = new List<DocumentBalance>();

            //Recorre la lista de Sales Documents y LLena el Acumulador
            IList<DocumentBalance> curSalesBalance;

            foreach (Document curDoc in salesDocs)
            {
                //Balance del Sales Order
                try
                {
                    curSalesBalance = Factory.DaoDocumentBalance()
                        .GeneralBalance(new DocumentBalance { Node = pickNode, Document = curDoc }, true)
                        .Where(f => f.Unit.BaseAmount == 1).ToList();
                }
                catch { continue; }

                if (curSalesBalance == null || curSalesBalance.Count == 0)
                    continue;


                //Cruce entre el sales order y el purchase order que genera las diferencias
                /*
                curSalesBalance =
                    (from sales in curSalesBalance
                     join purch in purchase on sales.Product.ProductID equals purch.Product.ProductID
                     into gj
                     from sub in gj.DefaultIfEmpty()
                     select new DocumentBalance
                     {
                         Document = sales.Document,
                         Product = sales.Product,
                         Quantity = sales.Quantity, //Added: *sales.Unit.BaseAmount Dec 05/09
                         QtyPending = (sub == null) ? 
                                sales.Quantity : 
                                (sales.Quantity <= sub.QtyPending * sub.Unit.BaseAmount) ?
                                0 : sales.Quantity - sub.QtyPending * sub.Unit.BaseAmount,
                         Unit = sales.Product.BaseUnit,  //Se cambio sales.Unit
                         Notes = (sub == null) ? "Qty not supplied" : (sales.Quantity <= sub.QtyPending * sub.Unit.BaseAmount) ? " OK!" : "Qty not supplied"
                     }).ToList();
                */

                //Added: *sales.Unit.BaseAmount Dec 05/09
                curSalesBalance =
                        (from sales in curSalesBalance
                         join purch in purchase on sales.Product.ProductID equals purch.Product.ProductID
                         into gj
                         from sub in gj.DefaultIfEmpty()
                         select new DocumentBalance
                         {
                             Document = sales.Document,
                             Product = sales.Product,
                             Quantity = sales.Quantity * sales.Unit.BaseAmount,
                             QtyPending = (sub == null) ?
                                    sales.Quantity * sales.Unit.BaseAmount :
                                    (sales.Quantity * sales.Unit.BaseAmount <= sub.QtyPending * sub.Unit.BaseAmount) ?
                                    0 : sales.Quantity * sales.Unit.BaseAmount - sub.QtyPending * sub.Unit.BaseAmount,
                             Unit = sales.Product.BaseUnit,  //Se cambio sales.Unit
                             Notes = (sub == null) ? "Qty not supplied" : (sales.Quantity * sales.Unit.BaseAmount <= sub.QtyPending * sub.Unit.BaseAmount) ? " OK!" : "Qty not supplied"
                         }).ToList();


                purchase = //Adicion de BaseAmount porque sales viene en EACH
                    (from purch in purchase
                     join sales in curSalesBalance on purch.Product.ProductID equals sales.Product.ProductID
                     into gj
                     from sub in gj.DefaultIfEmpty()
                     select new DocumentBalance
                     {
                         Document = purch.Document,
                         Product = purch.Product,
                         Quantity = purch.Quantity,
                         QtyPending = (sub == null) ? purch.QtyPending : (double)((int)((purch.QtyPending*purch.Unit.BaseAmount - sub.QtyProcessed)/purch.Unit.BaseAmount)),
                         Unit = purch.Unit
                     }).ToList();

                ////Lenando el acumulador con el documento de venta actual
                foreach (DocumentBalance curBal in curSalesBalance)
                    resultBalance.Add(curBal);
            }


            ////Lenando el acumulador con el resumen del docuemento de compras
            foreach (DocumentBalance curBal in purchase) {
                if (curBal.QtyPending > 0)
                    curBal.Notes = "Qty remain in PO";
                resultBalance.Add(curBal);
            }

            return resultBalance;
        }



        //Confirm CrossDock Document - Create Task
        public Document ConfirmCrossDockProcess(IList<DocumentBalance> crossDockBalance, string user)
        {
            Document cdDocument = null;

            try
            {
                Factory.IsTransactional = true;

                //1. Crea un Documento de Cross Dock. Y los relaciona con el PO y SO.
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.CrossDock });
                DocumentTypeSequence docSeq = GetNextDocSequence(crossDockBalance[0].Document.Company, docType);

                Account vendor = null;
                try { vendor = crossDockBalance.Where(f => f.Document.DocType.DocClass.DocClassID == SDocClass.Receiving).First().Document.Vendor; }
                catch { }


                //Crear Document header
                cdDocument = new Document
                 {
                     DocNumber = docSeq.CodeSequence,
                     DocType = docType,
                     IsFromErp = false,
                     CrossDocking = true,
                     Date1 = DateTime.Now,
                     CreatedBy = user,
                     Company = crossDockBalance[0].Document.Company,
                     Vendor = vendor
                 };

                CreateNewDocument(cdDocument, false);


                //2. CrossDock Document Lines
                int line = 1;
                foreach (DocumentBalance curBal in crossDockBalance.Where(f=> f.QtyProcessed > 0 &&  f.Document.DocType.DocClass.DocClassID == SDocClass.Shipping))
                {
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = cdDocument,
                        Product = curBal.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = curBal.Unit,
                        Quantity = curBal.QtyProcessed,
                        CreationDate = DateTime.Now,
                        IsDebit = false,
                        LineNumber = line,
                        Location = curBal.Location,
                        UnitBaseFactor = curBal.Unit.BaseAmount,
                        LinkDocNumber = curBal.Document.DocNumber,
                        CreatedBy = user
                    };

                    cdDocument.DocumentLines.Add(docLine);
                    line++;
                }


                Factory.Commit();

                //3. Asocia los documentos que hacen parte del proceso, el purchase y los sales
                TaskDocumentRelation taskDocRel;
                foreach (Document curDoc in crossDockBalance.Select(f=>f.Document).Distinct())
                {
                    taskDocRel = new TaskDocumentRelation();
                    taskDocRel.TaskDoc = cdDocument;
                    taskDocRel.IncludedDoc = curDoc;
                    taskDocRel.CreatedBy = user;
                    taskDocRel.CreationDate = DateTime.Now;

                    Factory.DaoTaskDocumentRelation().Save(taskDocRel);

                    //Actualizando el Status del documento a In Process
                    curDoc.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
                    Factory.DaoDocument().Update(curDoc);
                }


                //Actualiza el Documento de Receiving y lo pone como IsCrossDock = treu
                foreach (Document doc in crossDockBalance.Where(f => f.Document.DocType.DocClass.DocClassID == SDocClass.Receiving).Select(f => f.Document).Distinct())
                {
                    doc.CrossDocking = true;
                    Factory.DaoDocument().Update(doc);
                }


                Factory.Commit();
                return cdDocument;
            }

            catch (Exception ex)
            {
                Factory.Rollback();
                //Factory.DaoDocument().Delete(cdDocument);
                ExceptionMngr.WriteEvent("ConfirmCrossDockDocuments:Doc#" + cdDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw;
            }
        }



        //Asocia los usuarios que van a trabajar en un documento de recibo o despacho
        public void AssociateUserDocument(Document document, IList<SysUser> userList)
        {
            try
            {
                //Start Transaction
                Factory.IsTransactional = true;

                //Asocia los documentos del Task a TaskDocument
                TaskByUser TaskByUserRel;
                foreach (SysUser curUser in userList)
                {
                    TaskByUserRel = new TaskByUser
                    {
                        TaskDocument = document,
                        User = curUser,
                        CreatedBy = "",
                        CreationDate = DateTime.Now
                    };

                    Factory.DaoTaskByUser().Save(TaskByUserRel);
                }

            }

            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("AssociteUserDocument:Doc#" + document.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw;
            }
        }



        public IList<Product> GetDocumentProduct(Document data, Product product)
        {
            try
            {
                //Obteniendo las lineas que no esten New or In process
                IList<Product> productList = Factory.DaoDocumentLine()
                    .Select(new DocumentLine { Document = data,  Product = product })
                    .Where(f => f.LineStatus.StatusID != DocStatus.Cancelled)
                    .Select(f => f.Product).Distinct().Where(f => f.Status.StatusID == EntityStatus.Active).ToList();

                return productList;

            }
            catch
            {
                return null;
            }
        }



        public IList<Unit> GetDocumentUnit(DocumentLine data)
        {
            try
            {
               return Factory.DaoDocumentLine().Select(data)
                    .Where(f => f.LineStatus.StatusID == DocStatus.New || f.LineStatus.StatusID == DocStatus.InProcess)
                    .Select(f=>f.Unit).Distinct().ToList();
            }
            catch { return null; } 
        }


        public bool IsTrackRequiredInDocument(Document document, Node node)
        {
            return Factory.DaoDocument().IsTrackRequiredInDocument(document, node);
        }

        /// <summary>
        /// Create a sales document with the lines entered asociated to the pickers assigned
        /// </summary>
        /// <param name="document"></param>
        /// <param name="dtLines"></param>
        /// <param name="pickers"></param>
        public Document CreateMergedDocument(Document document, IList<DocumentLine> dtLines,
            IList<SysUser> pickers, IList<DocumentAddress> addresses)
        {
            

            if (dtLines == null || dtLines.Count == 0)
                throw new Exception("Document does not contain lines.");

            String flag = "Status";
            IList<DocumentLine> mergedLines = new List<DocumentLine>();

            Status inProcess = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
            Status statusNew = WType.GetStatus(new Status { StatusID = DocStatus.New });
            Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });

            

            Factory.IsTransactional = true;

            try
            {
                flag = "New Document";
                document = CreateNewDocument(document, false);

                

                //OLD DOCUMENTS
                flag = "Update Old Documents";
                foreach (Document doc in dtLines.Select(f=>f.Document).Distinct())
                {
                    doc.DocStatus = inProcess;
                    doc.ModDate = DateTime.Now;
                    doc.ModifiedBy = document.CreatedBy;
                    doc.Comment = "Merged on: " + document.DocNumber + ". " + (string.IsNullOrEmpty(doc.Comment) ? "" : doc.Comment);
                    Factory.DaoDocument().Update(doc);
                }
                
                //OLD LINES
                int z = 1;
                DocumentLine curLine = null;

                flag = "Entering to Lines";

                foreach (DocumentLine line in dtLines)
                {
                    try
                    {
                        flag = "Update Line " + line.LineID.ToString();

                        curLine = Factory.DaoDocumentLine().Select(new DocumentLine { LineID = line.LineID }).First();

                        flag += "Status";

                        curLine.LineStatus = completed;

                        curLine.ModDate = DateTime.Now;

                        flag += "CreatedBy";
                        curLine.ModifiedBy = document.CreatedBy;

                        //Line Quantites Updated (BackOrder / Cancel)
                        flag += "BO";
                        curLine.QtyBackOrder = line.QtyBackOrder;

                        flag += "Cancel";
                        curLine.QtyCancel = line.QtyCancel;

                        flag += "Allocated";
                        curLine.QtyAllocated = line.QtyAllocated;

                        flag += "DocNumber";
                        curLine.PostingDocument = document.DocNumber;

                        Factory.DaoDocumentLine().Update(curLine);

                    }
                    catch (Exception ex) {
                        ExceptionMngr.WriteEvent("CreateMergedDocument:Doc#" + document.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                                ListValues.ErrorCategory.Business);
                    }


                    //Revisar que no haya una linea con el mismo documento y la misma linea en otro merged order
                    // Jul 29 /2010
                    
                    /*
                    try
                    {
                        if (Factory.DaoDocumentLine().Select(
                                new DocumentLine { 
                                        LinkDocLineNumber = line.LineNumber, 
                                        LinkDocNumber = line.Document.DocNumber })
                            .Any(f => f.Document.DocType.DocTypeID == SDocType.MergedSalesOrder
                            && f.LineStatus.StatusID != DocStatus.Cancelled))
                            continue;
                    }
                    catch { }
                    */

                    flag = "Merge Line " + line.LineID.ToString();

                    //MERGED LINES
                    mergedLines.Add(new DocumentLine {
                        LinkDocLineNumber = line.LineNumber,
                        LineNumber = z,
                        LinkDocNumber = line.Document.DocNumber,
                        Document = document,
                        CreatedBy = document.CreatedBy,
                        CreationDate = DateTime.Now,
                        Date1 = document.Date1,
                        IsDebit = false,
                        LineStatus = statusNew,
                        Location = document.Location,
                        Note = line.Note,
                        Product = line.Product,
                        QtyAllocated = line.QtyAllocated,
                        QtyBackOrder = line.QtyBackOrder,
                        QtyCancel = line.QtyCancel,
                        Quantity = line.Quantity,
                        Unit = line.Unit,
                        UnitBaseFactor = line.UnitBaseFactor,
                        UnitPrice = line.UnitPrice,
                        LineDescription = line.LineDescription
                    });

                    z++;
                }


                flag = "After lines";

                document.DocumentLines = mergedLines;

                //ADDRESSES
                for (int i = 0; i < addresses.Count; i++)
                    addresses[i].Document = document;
                document.DocumentAddresses = addresses;

                Factory.DaoDocument().Update(document);


                //Actualizando el Allocation de ProductInventoty
                //Disminuyendo el Qty IN Use para cada producto.
                //TODO: Una variable que permita decir si usar o no allocation.

                /*
                List<int> productList = new List<int>();
                foreach (Int32 p in dtLines.Select(f => f.Product.ProductID).Distinct())
                    productList.Add(p);

                //IList<ProductInventory> productInUseList = Factory.DaoProductInventory()
                    .GetProductInventory(new ProductInventory(), productList);
                

                ProductInventory curPi;
                foreach (DocumentLine line in dtLines)
                {

                    try
                    {
                        curPi = productInUseList.Where(f => f.Product.ProductID == line.Product.ProductID 
                            && f.Document.DocID == line.Document.DocID ).First();

                        curPi.QtyAllocated += line.QtyAllocated;
                        curPi.QtyInUse -= line.QtyAllocated;

                        if (curPi.QtyInUse < 0)
                            curPi.QtyInUse = 0;

                        //curPi.ModDate = DateTime.Now;
                        //curPi.ModifiedBy = document.CreatedBy;
                        //Factory.DaoProductInventory().Update(curPi);
                    }
                    catch (Exception ex) {
                        ExceptionMngr.WriteEvent("ProdutInventory:" + line.Product.ProductCode, ListValues.EventType.Warn, 
                            ex, null, ListValues.ErrorCategory.Business);
                    }                    
                }
                */

                flag = "Commit";

                Factory.Commit();

                flag = "Return";

                return Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();


            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("CreateMergedDocument:Doc#" + document.DocNumber+ ":" + flag, ListValues.EventType.Fatal, ex, null,
                    ListValues.ErrorCategory.Business);
                throw;
            }

        }





        public Document CreateMergedDocumentV2(Document document, IList<DocumentLine> dtLines,
    IList<SysUser> pickers, IList<DocumentAddress> addresses)
        {


            if (dtLines == null || dtLines.Count == 0)
                throw new Exception("Document does not contain lines.");

            String flag = "Status";
            IList<DocumentLine> mergedLines = new List<DocumentLine>();

            Status inProcess = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
            Status statusNew = WType.GetStatus(new Status { StatusID = DocStatus.New });
            Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });



            Factory.IsTransactional = true;

            try
            {
                flag = "New Document";
                if (document.DocID == 0)
                    document = CreateNewDocument(document, false);



                //OLD DOCUMENTS
                flag = "Update Old Documents";
                foreach (Document doc in dtLines.Where(f=> string.IsNullOrEmpty(f.BinAffected)).Select(f => f.Document).Distinct())
                {
                    doc.DocStatus = inProcess;
                    doc.ModDate = DateTime.Now;
                    doc.ModifiedBy = document.CreatedBy;
                    doc.Comment = "Merged on: " + document.DocNumber + ". " + (string.IsNullOrEmpty(doc.Comment) ? "" : doc.Comment);
                    Factory.DaoDocument().Update(doc);
                }

                //OLD LINES
                int z = 1;
                DocumentLine curLine = null;

                flag = "Entering to Lines";

                foreach (DocumentLine line in dtLines)
                {
                    if (!string.IsNullOrEmpty(line.BinAffected))
                    {  //Si no ha sido adicionada antes
                        z++;
                        continue;
                    }

                    try
                    {
                        flag = "Update Line " + line.LineID.ToString();

                        curLine = Factory.DaoDocumentLine().Select(new DocumentLine { LineID = line.LineID }).First();

                        flag += "Status";

                        curLine.LineStatus = completed;

                        curLine.ModDate = DateTime.Now;

                        flag += "CreatedBy";
                        curLine.ModifiedBy = document.CreatedBy;

                        //Line Quantites Updated (BackOrder / Cancel)
                        flag += "BO";
                        curLine.QtyBackOrder = line.QtyBackOrder;

                        flag += "Cancel";
                        curLine.QtyCancel = line.QtyCancel;

                        flag += "Allocated";
                        curLine.QtyAllocated = line.QtyAllocated;

                        flag += "DocNumber";
                        curLine.PostingDocument = document.DocNumber;

                        Factory.DaoDocumentLine().Update(curLine);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("CreateMergedDocument:Doc#" + document.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                                ListValues.ErrorCategory.Business);
                    }



                    flag = "Merge Line " + line.LineID.ToString();

                    //MERGED LINES
                    mergedLines.Add(new DocumentLine
                    {
                        LinkDocLineNumber = line.LineNumber,
                        LineNumber = z,
                        LinkDocNumber = line.Document.DocNumber,
                        Document = document,
                        CreatedBy = document.CreatedBy,
                        CreationDate = DateTime.Now,
                        Date1 = document.Date1,
                        IsDebit = false,
                        LineStatus = statusNew,
                        Location = document.Location,
                        Note = line.Note,
                        Product = line.Product,
                        QtyAllocated = line.QtyAllocated,
                        QtyBackOrder = line.QtyBackOrder,
                        QtyCancel = line.QtyCancel,
                        Quantity = line.Quantity,
                        Unit = line.Unit,
                        UnitBaseFactor = line.UnitBaseFactor,
                        UnitPrice = line.UnitPrice,
                        LineDescription = line.LineDescription,
                        BinAffected = "M"
                    });

                    z++;
                }


                flag = "After lines";

                document.DocumentLines = mergedLines;

                //ADDRESSES
                for (int i = 0; i < addresses.Count; i++)
                    addresses[i].Document = document;
                document.DocumentAddresses = addresses;

                Factory.DaoDocument().Update(document);

                flag = "Commit";

                Factory.Commit();

                flag = "Return";

                return Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();


            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("CreateMergedDocument:Doc#" + document.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                    ListValues.ErrorCategory.Business);
                throw;
            }

        }



        public void CancelMergerOrder(Document document, DocumentLine docLine)
        {

            //Cancelacion del merged order
            //Debe poner en cancelado todas las lineas del documento. 
            //que esten en new y no tengan nada shipped
            DocumentLine oriLine;
            Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

            #region Cancel Complete Document

            if (document != null)
            {
                try
                {

                    Factory.DaoDocument().Update(document);

                    Factory.IsTransactional = true;


                    if (document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                    {

                        IList<DocumentLine> docLinesList = Factory.DaoDocument()
                            .Select(new Document { DocID = document.DocID }).First().DocumentLines;


                        foreach (DocumentLine dl in docLinesList)
                        {
                            dl.ModifiedBy = document.ModifiedBy;
                            dl.ModDate = DateTime.Now;
                            dl.LineStatus = cancelled;
                            Factory.DaoDocumentLine().Update(dl);

                            //Obtiene la linea original.
                            //2. Busca las lineas del merged original
                            oriLine = Factory.DaoDocumentLine().Select(new DocumentLine
                            {
                                Document = new Document
                                {
                                    DocNumber = dl.LinkDocNumber,
                                    Company = document.Company
                                },
                                LineNumber = dl.LinkDocLineNumber

                            }).First();


                            oriLine.LineStatus = new Status { StatusID = DocStatus.New };
                            oriLine.QtyAllocated = 0;
                            oriLine.QtyShipped = 0;
                            oriLine.QtyCancel = 0;
                            oriLine.QtyBackOrder = 0;

                            Factory.DaoDocumentLine().Update(oriLine);
                        }
                    }

                    //Reversar lo piqueado para ese documento
                    ReversePickedProduct(document);


                    Factory.Commit();

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("CancelMergerOrder:Doc#" + document.DocNumber, ListValues.EventType.Fatal, ex, null,
                        ListValues.ErrorCategory.Business);
                    throw;
                }
            }

            #endregion


            #region Cancel Line

            else if (docLine != null)
            {
                try
                {
                    Factory.IsTransactional = true;


                    docLine.ModDate = DateTime.Now;
                    docLine.LineStatus = cancelled;
                    Factory.DaoDocumentLine().Update(docLine);

                    //Obtiene la linea original.
                    //2. Busca las lineas del merged original
                    oriLine = Factory.DaoDocumentLine().Select(new DocumentLine
                    {
                        Document = new Document
                        {
                            DocNumber = docLine.LinkDocNumber,
                            Company = docLine.Document.Company
                        },
                        LineNumber = docLine.LinkDocLineNumber

                    }).First();


                    oriLine.LineStatus = new Status { StatusID = DocStatus.New };
                    oriLine.QtyAllocated = 0;
                    oriLine.QtyShipped = 0;
                    oriLine.QtyCancel = 0;
                    oriLine.QtyBackOrder = 0;
                    Factory.DaoDocumentLine().Update(oriLine);

                    Factory.Commit();

                    //Remove The Original Line from GP
                    SaveUpdateDocumentLine(oriLine, true);

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("CancelMergerOrder:Doc#" + document.DocNumber, ListValues.EventType.Fatal, ex, null,
                        ListValues.ErrorCategory.Business);
                    throw;
                }
            }

            #endregion

        }





        private void ReversePickedProduct(Document data)
        {

            //Node releaseNode = new Node { NodeID = NodeType.Released };
            Node storeNode = new Node { NodeID = NodeType.Stored };
            Bin binRestore = WType.GetBin(new Bin {BinCode = DefaultBin.MAIN, Location = data.Location});

            //Update document status to Cancelled
            Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
            Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });
            Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            //update NodeTrace
            NodeTrace qNodeTrace = new NodeTrace { Document = data };

            //Busca todo los registros de ese documento y los reversa
            IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(qNodeTrace);

            Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

            //Label curLabel;

            foreach (NodeTrace trace in nodeTraceList)
            {
                //Reversa el trace original para poderlo postear nuevamente o reversarlo a stored
                if (trace.Node.NodeID == NodeType.Picked)
                {
                    trace.ModifiedBy = data.ModifiedBy;
                    trace.ModDate = DateTime.Now;
                    trace.Node = voidNode;
                    trace.Comment = "Picked: " + trace.Document.DocNumber + " Reversed";
                    Factory.DaoNodeTrace().Update(trace);

                    // CAA
                    // Vuelve a Stock node
                    SaveNodeTrace(
                        new NodeTrace
                        {
                            Node = storeNode,
                            Document = trace.Document,
                            Label = trace.Label,
                            Quantity = trace.Quantity,
                            IsDebit = trace.IsDebit,
                            CreatedBy = trace.CreatedBy,
                            Status = active,
                            Comment = "Stock: " + trace.Document.DocNumber + " Reversed",
                            CreationDate = DateTime.Now
                        });
                }

                //Recorre los Packages de ese document y reversa los labels HIjos
                //Poner en Void los package Labels de ese documento.
                IList<DocumentPackage> packList = Factory.DaoDocumentPackage().Select(new DocumentPackage { Document = data });
                IList<Label> labelList;
                foreach (DocumentPackage curPack in packList)
                {
                    labelList = Factory.DaoLabel().Select(new Label { FatherLabel = curPack.PackLabel });

                    foreach (Label lbl in labelList)
                    {
                        //Reverse labels to node trace stored
                        lbl.Bin = binRestore;
                        lbl.Node = storeNode;
                        lbl.ModDate = DateTime.Now;
                        lbl.ModifiedBy = data.ModifiedBy;
                        lbl.Status = active;
                        lbl.ShippingDocument = null;
                        lbl.FatherLabel = null;

                        Factory.DaoLabel().Update(lbl);


                        //Reversando los Hijos en caso de que ese label tenga Hijos
                        try
                        {
                            lbl.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = lbl });

                            if (lbl.ChildLabels != null && lbl.ChildLabels.Count > 0)

                                foreach (Label child in lbl.ChildLabels)
                                {
                                    child.Bin = binRestore;
                                    child.Node = storeNode;
                                    child.ModDate = DateTime.Now;
                                    child.ModifiedBy = data.ModifiedBy;
                                    child.Status = active;
                                    child.ShippingDocument = null;

                                    Factory.DaoLabel().Update(child);
                                }
                        }
                        catch { }
                    }

                    curPack.PackLabel.Status = inactive;
                    curPack.PackLabel.Node = voidNode;
                    Factory.DaoLabel().Update(curPack.PackLabel);
                }
            }
        }



        public DocumentLine SaveUpdateDocumentLine(DocumentLine docLine, bool removeLine)
        {
            int lineSeq = 0;
            DocumentLine resulLine = docLine;

            try
            {
                //Manda a crear/actualizar la linea al ERP.
                lineSeq = (new ErpDataMngr()).SaveUpdateErpDocumentLine(docLine, removeLine);

            }
            catch (Exception ex)
            {
                throw new Exception("Line could not be processed in the ERP.\n" + ex.Message);
            }


            //NEW OR UPDATE
            Factory.IsTransactional = true;

            try
            {

                if (removeLine)       
                    Factory.DaoDocumentLine().Delete(docLine);
                
                //Crea/Actuliza la linea en el documento
                else if (docLine.LineID > 0)
                {
                    //Actualiza Cantidades
                    //Qty, QtyCancel, QtyBO, QtyAllocated, Description.
                    Factory.DaoDocumentLine().Update(docLine);

                    //Si es una Merged deb actulizar tambien la linea Original.
                    if (docLine.Document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                    {
                        DocumentLine oriLine = Factory.DaoDocumentLine().Select(new DocumentLine
                            {
                                Document = new Document { DocNumber = docLine.LinkDocNumber, Company = docLine.Document.Company },
                                LineNumber = docLine.LinkDocLineNumber
                            }).First();

                        //Update Qtys
                        oriLine.Quantity = docLine.Quantity;
                        oriLine.QtyCancel = docLine.QtyCancel;
                        oriLine.QtyBackOrder = docLine.QtyBackOrder;

                        Factory.DaoDocumentLine().Update(oriLine);
                    }

                }

                else
                {
                    if (lineSeq == 0)
                        try { lineSeq = Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = docLine.Document.DocID } }).Max(f => f.LineNumber) + 1; }
                        catch { lineSeq = 1; }

                    //Si es una Merged debe crear la linea en el doc original.
                    Document oriDoc = null;
                    if (docLine.Document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                    {
                        oriDoc = Factory.DaoDocument().Select(
                            new Document { DocNumber = docLine.LinkDocNumber, Company = docLine.Document.Company }
                        ).First();
                    }


                    docLine.IsDebit = false;
                    docLine.LineNumber = lineSeq;
                    docLine.Location = docLine.Document.Location;
                    docLine.CreatedBy = docLine.ModifiedBy;
                    docLine.CreationDate = docLine.ModDate = DateTime.Now;
                    docLine.LineStatus = new Status { StatusID = DocStatus.New };
                    docLine.UnitPrice = docLine.Product.ProductCost;
                    docLine.ExtendedPrice = docLine.Product.ProductCost * docLine.Quantity;
                    docLine.LinkDocLineNumber = (oriDoc == null) ? docLine.LinkDocLineNumber : lineSeq;

                    resulLine = Factory.DaoDocumentLine().Save(docLine);
                    resulLine.Note = "NEW";


                    //Si es una Merged debe crear la linea en el doc original.
                    if (oriDoc != null)
                    {
                        //Creando la Linea Original.
                        DocumentLine oriLine = new DocumentLine
                        {
                            LineNumber = lineSeq,
                            Document = oriDoc,
                            CreatedBy = oriDoc.CreatedBy,
                            CreationDate = DateTime.Now,
                            Date1 = oriDoc.Date1,
                            IsDebit = false,
                            LineStatus = new Status { StatusID = DocStatus.InProcess },
                            Location = oriDoc.Location,
                            Note = docLine.Note,
                            Product = docLine.Product,
                            QtyAllocated = docLine.QtyAllocated,
                            QtyBackOrder = docLine.QtyBackOrder,
                            QtyCancel = docLine.QtyCancel,
                            Quantity = docLine.Quantity,
                            Unit = docLine.Unit,
                            UnitPrice = docLine.UnitPrice,
                            ExtendedPrice = docLine.ExtendedPrice,
                            LineDescription = docLine.LineDescription
                        };

                        Factory.DaoDocumentLine().Save(oriLine);
                    }

                }

                Factory.Commit();

                return resulLine;

            }
            catch (Exception ex)
            {

                Factory.Rollback();
                throw new Exception(ex.Message);
            }
        }



        public string CreateMergedDocumentForBackOrders(Document document, IList<DocumentLine> 
            processLines, IList<SysUser> pickers, IList<DocumentAddress> addresses, int process)
        {

            if (processLines == null || processLines.Count == 0)
                throw new Exception("Document does not contain lines.");

                        
            IList<DocumentLine> curLines = null;
            Document soDocument = null;
            string error = "";
            string ok = "";
            string flagz = "Process";
            string erpDoc = "";

            #region Process = 0, ONe Order per Document

            if (process == 0)
            {
                IList<Document> listDoc = processLines.Select(f => f.Document).Distinct().ToList();
               

                    //UN SALES ORDER POR CADA DOCUMENTO DE BO

                foreach (Document curDoc in listDoc)
                {

                    soDocument = new Document
                    {
                        DocType = document.DocType,
                        CreatedBy = document.CreatedBy,
                        CreationDate = DateTime.Now,
                        Location = document.Location,
                        Company = document.Company,
                        IsFromErp = true,
                        CrossDocking = false,
                        Date1 = DateTime.Now,
                        UseAllocation = true,
                        CustPONumber = "",
                        Comment = document.Comment,
                        UserDef1 = document.UserDef1,
                        ErpMaster = curDoc.ErpMaster
                    };


                    try
                    {
                        flagz = "curAddress";

                        IList<DocumentAddress> curAddress = Factory.DaoDocumentAddress()
                            .Select(new DocumentAddress { Document = curDoc });

                        Factory.IsTransactional = true;


                        flagz = "CreateNewDocument";
                        soDocument.Customer = curDoc.Customer;
                        soDocument.CustPONumber = curDoc.DocNumber;
                        soDocument = CreateNewDocument(soDocument, false);


                        //Addresses 
                        soDocument.DocumentAddresses = curAddress;

                        flagz = "ProcessBOLines";
                        curLines = processLines.Where(f => f.Document.DocID == curDoc.DocID).ToList();
                        soDocument = ProcessBOLines(soDocument, curLines);

                        //Send SO to ERP
                        flagz = "CreateSalesOrder";
                        erpDoc = (new ErpDataMngr()).CreateSalesOrder(soDocument, curDoc.UserDef1, "RTP");

                        soDocument.DocNumber = erpDoc;
                        Factory.DaoDocument().Update(soDocument);

                        Factory.Commit();


                        //ELIMINAR EL BO LINE de GP
                        foreach (DocumentLine xLine in curLines)
                            (new ErpDataMngr()).SaveUpdateErpDocumentLine(xLine, true);


                        ok += curDoc.DocNumber + " => " + soDocument.DocNumber + " : Process OK!\n";

                    }
                    catch (Exception ez)
                    {
                        error += curDoc.DocNumber + " => " + soDocument.DocNumber + ": " + flagz+ " "+ ez.Message + "\n";
                        Factory.Rollback();
                    }

                } //End FOREACH


            }
            #endregion

            //Process = 1, One Order per Customer

            if (string.IsNullOrEmpty(error))
                Factory.Commit();
            else
                throw new Exception(ok + error);

            return ok;
        }


        private Document ProcessBOLines(Document soDocument, IList<DocumentLine> curLines)
        {
       
            String flag = "Status";

            try
            {
                IList<DocumentLine> mergedLines = new List<DocumentLine>();

                Status inProcess = new Status { StatusID = DocStatus.InProcess };
                Status statusNew = new Status { StatusID = DocStatus.New };
                Status completed = new Status { StatusID = DocStatus.Completed };

                //OLD DOCUMENTS
                flag = "Update Old Documents";
                foreach (Document doc in curLines.Select(f => f.Document).Distinct())
                {
                    doc.DocStatus = inProcess;
                    doc.ModDate = DateTime.Now;
                    doc.ModifiedBy = soDocument.CreatedBy;
                    doc.Comment = "Processed on: " + soDocument.DocNumber + ". " + (string.IsNullOrEmpty(doc.Comment) ? "" : doc.Comment);
                    Factory.DaoDocument().Update(doc);
                }

                //OLD LINES
                int z = 1;
                DocumentLine curLine = null;

                flag = "Entering to Lines";

                foreach (DocumentLine line in curLines)
                {
                    try
                    {
                        flag = "Update Line " + line.LineID.ToString();

                        curLine = Factory.DaoDocumentLine().Select(new DocumentLine { LineID = line.LineID }).First();

                        flag += "Status";

                        curLine.LineStatus = completed;

                        curLine.ModDate = DateTime.Now;

                        flag += "CreatedBy";
                        curLine.ModifiedBy = soDocument.CreatedBy;

                        flag += "BO";
                        curLine.QtyBackOrder = line.QtyBackOrder;

                        flag += "DocNumber";
                        curLine.PostingDocument = soDocument.DocNumber;

                        Factory.DaoDocumentLine().Update(curLine);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("CreateBODocument:Doc#" + soDocument.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                                ListValues.ErrorCategory.Business);
                    }


                    flag = "New SO Line " + line.LineID.ToString();

                    //MERGED LINES
                    mergedLines.Add(new DocumentLine
                    {
                        LinkDocLineNumber = line.LineNumber,
                        LineNumber = z * 16384,
                        LinkDocNumber = line.Document.DocNumber,
                        Document = soDocument,
                        CreatedBy = soDocument.CreatedBy,
                        CreationDate = DateTime.Now,
                        Date1 = soDocument.Date1,
                        IsDebit = false,
                        LineStatus = statusNew,
                        Location = soDocument.Location,
                        Note = line.Note,
                        Product = line.Product,
                        Quantity = line.QtyBackOrder,
                        Unit = line.Unit,
                        UnitBaseFactor = line.UnitBaseFactor,
                        UnitPrice = line.UnitPrice,
                        LineDescription = line.LineDescription,
                        ExtendedPrice = line.UnitPrice * line.QtyBackOrder
                    });

                    z++;
                }


                flag = "After lines";

                soDocument.DocumentLines = mergedLines;

                Factory.DaoDocument().Update(soDocument);

                return soDocument;
            }
            catch (Exception ez){
                throw new Exception(flag + " " + ez.Message);
            }
        }


        public Document ConsolidateOrdersInNewDocument(Document document, List<Document> docList)
        {
           //Arma un documento con las lineas de todos los otros.


            if (docList == null || docList.Count == 0)
                throw new Exception("Process does not contain document to merge.");

            String flag = "Status";
            IList<DocumentLine> mergedLines = new List<DocumentLine>();
            IList<DocumentLine> dtLines = new List<DocumentLine>();

            Status inProcess = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
            Status statusNew = WType.GetStatus(new Status { StatusID = DocStatus.New });
            Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });



            Factory.IsTransactional = true;

            try
            {
                flag = "New Document";
                if (document.DocID == 0)
                    document = CreateNewDocument(document, false);



                //OLD DOCUMENTS
                flag = "Update Old Documents";
                foreach (Document doc in docList)
                {
                    doc.DocStatus = inProcess;
                    doc.ModDate = DateTime.Now;
                    doc.QuoteNumber = "M";  //Merged
                    doc.Reference = document.DocNumber;
                    doc.ModifiedBy = document.CreatedBy;
                    doc.Notes = "Agrupado: " + document.DocNumber + ". " + (string.IsNullOrEmpty(doc.Comment) ? "" : doc.Comment);
                    Factory.DaoDocument().Update(doc);

                    foreach (DocumentLine dl in Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = doc.DocID}} ))
                        dtLines.Add(dl);
                }

                //OLD LINES
                int z = 1;
                DocumentLine curLine = null;

                flag = "Entering to Lines";

                foreach (DocumentLine line in dtLines)
                {
                    if (!string.IsNullOrEmpty(line.BinAffected))
                    {  //Si no ha sido adicionada antes
                        z++;
                        continue;
                    }

                    try
                    {
                        flag = "Update Line " + line.LineID.ToString();

                        curLine = Factory.DaoDocumentLine().Select(new DocumentLine { LineID = line.LineID }).First();

                        flag += "Status";

                        curLine.LineStatus = completed;

                        curLine.ModDate = DateTime.Now;

                        flag += "CreatedBy";
                        curLine.ModifiedBy = document.CreatedBy;

                        flag += "DocNumber";
                        curLine.PostingDocument = document.DocNumber;

                        Factory.DaoDocumentLine().Update(curLine);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("ConsolidateOrdersInNewDocument:Doc#" + document.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                                ListValues.ErrorCategory.Business);
                    }



                    flag = "Merge Line " + line.LineID.ToString();

                    //MERGED LINES
                    mergedLines.Add(new DocumentLine
                    {
                        LinkDocLineNumber = line.LineNumber,
                        LineNumber = z,
                        LinkDocNumber = line.Document.DocNumber,
                        Document = document,
                        CreatedBy = document.CreatedBy,
                        CreationDate = DateTime.Now,
                        Date1 = document.Date1,
                        IsDebit = false,
                        LineStatus = statusNew,
                        Location = document.Location,
                        Note = line.Note,
                        Product = line.Product,
                        QtyAllocated = line.QtyAllocated,
                        QtyBackOrder = line.QtyBackOrder,
                        QtyCancel = line.QtyCancel,
                        Quantity = line.Quantity,
                        Unit = line.Unit,
                        UnitBaseFactor = line.UnitBaseFactor,
                        UnitPrice = line.UnitPrice,
                        LineDescription = line.LineDescription,
                        BinAffected = "M"
                    });

                    z++;
                }


                flag = "After lines";

                document.DocumentLines = mergedLines;

                Factory.DaoDocument().Update(document);

                flag = "Commit";

                Factory.Commit();

                flag = "Return";

                return Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();


            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ConsolidateOrdersInNewDocument:Doc#" + document.DocNumber + ":" + flag, ListValues.EventType.Fatal, ex, null,
                    ListValues.ErrorCategory.Business);
                throw;
            }
        }
    }
}