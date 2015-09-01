using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Master;
using Entities.Trace;
using Entities.General;
using ErpConnect;
using System.Configuration;
using Integrator;
using Entities;
using Entities.Profile;
using System.IO;
using System.Reflection;

namespace BizzLogic.Logic
{

    public partial class ErpDataMngr : BasicMngr
    {


        //Actualiza el status de los recipts que han sido procesados en el ERP, para que no puedn ase reversados.
        public ProcessResponse UpdatePostedProcess(Company company)
        {

            if (company == null)
                return null;

            if (company.ErpConnection == null)
                return null;


            SetConnectMngr(company);

            try
            {
                //PR's  
                Console.WriteLine("Purchase Receipts");
                UpdatePostedReceipts();                
            }
            catch (Exception ex)
            { ExceptionMngr.WriteEvent("UpdatePostedReceipts " + company.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business); }


            try
            {
                //Update Inventory Adjustments
                Console.WriteLine("Inventory Adjustements");
                UpdateInventoryAdjustments();
            }
            catch (Exception ex)
            { ExceptionMngr.WriteEvent("UpdateInventoryAdjustments " + company.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business); }



            //Revisa si puede hacer fulfull a un docuemtn de cross dock
            try                
            {

                if (GetCompanyOption(company, "WITHERPSH").Equals("T"))
                {
                    Console.WriteLine("Cross Dock");
                    FulfillCrossDockSalesDocuments();
                }
            
            }
            catch (Exception ex)
            { ExceptionMngr.WriteEvent("FulfillCrossDockSalesDocuments " + company.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business); }


            try
            {
                //Sales Orders
                Console.WriteLine("Sales Orders");
                UpdatePostedSalesOrders();
            }
            catch (Exception ex)
            { ExceptionMngr.WriteEvent("UpdatePostedSalesOrders " + company.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business); }



            try
            {
                //Assembly Orders
                Console.WriteLine("Assembly Orders");
                UpdatePostedAssemblyOrders();
            }
            catch (Exception ex)
            { ExceptionMngr.WriteEvent("UpdatePostedAssemblyOrders " + company.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business); }



            return new ProcessResponse { MessageID = 0, Message = "" };

        }



        private void UpdatePostedSalesOrders()
        {
            Status posted = WType.GetStatus(new Status { StatusID = DocStatus.Posted });

            Document document; //for the foreach
            Document pattern = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder },
                DocStatus = new Status { StatusID = DocStatus.Completed }
            };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);
            IList<Document> shipmentList;

            foreach (Document curDocument in unPostedList)
            {
                try
                {
                    //Obtiene si esta posteado o no
                    document = ErpFactory.Documents().GetSalesOrderPostedStatus(curDocument);

                    if (document != null)
                    {
                        document.DocStatus = posted;
                        Factory.DaoDocument().Update(document);


                        shipmentList = Factory.DaoDocument().Select(new Document { CustPONumber = document.DocNumber, Company = document.Company });

                        foreach (Document shp in shipmentList)
                        {
                            document.DocStatus = posted;
                            Factory.DaoDocument().Update(document);
                        }


                    }
                }
                catch { }
            }



            /*
            Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

          
            #region Sales Orders

            pattern = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder }, //,
                DocStatus = new Status { StatusID = DocStatus.New }
            };

            //Lista de Ordenes nuevos que hay que revisar si se eliminaron
            IList<Document> newList = Factory.DaoDocument().Select(pattern);

            Console.WriteLine("Documents");
            foreach (Document curDocument in newList)
            {
               
                try
                {
                    
                    if (ErpFactory.Documents().SalesOrderWasDeleted(curDocument))
                    {
                        Console.WriteLine("Doc Deleted: " + curDocument.DocNumber);
                        curDocument.DocStatus = cancelled;
                        Factory.DaoDocument().Update(curDocument);
                        
                    }
                }
                catch (Exception ex) {
                    ExceptionMngr.WriteEvent("SalesOrderWasDeleted:Doc#" + curDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                 //throw new Exception(ex.Message);             
                }
            }



            pattern = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.SalesOrder }, //,
                DocStatus = new Status { StatusID = DocStatus.InProcess }
            };

            //Lista de Ordenes nuevos que hay que revisar si se eliminaron
            newList = Factory.DaoDocument().Select(pattern);

            //Console.WriteLine("Documents InP");
            foreach (Document curDocument in newList)
            {

                try
                {
                    
                    if (ErpFactory.Documents().SalesOrderWasDeleted(curDocument))
                    {
                        //Console.WriteLine("Doc: " + curDocument.DocNumber);
                        curDocument.DocStatus = cancelled;
                        Factory.DaoDocument().Update(curDocument);

                    }
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("SalesOrderWasDeleted:Doc#" + curDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                    //throw new Exception(ex.Message);             
                }
            }

            #endregion
            



            #region Returns

            //RETURNS
            pattern = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.Return }, //,
                DocStatus = new Status { StatusID = DocStatus.New }
            };

            //Lista de Ordenes nuevos que hay que revisar si se eliminaron
            //IList<Document> newList = Factory.DaoDocument().Select(pattern);
            newList = Factory.DaoDocument().Select(pattern);



            foreach (Document curDocument in newList)
            {
                try
                {
                    if (ErpFactory.Documents().SalesOrderWasDeleted(curDocument))
                    {
                        curDocument.DocStatus = cancelled;
                        Factory.DaoDocument().Update(curDocument);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("SalesOrderWasDeleted:Doc#" + curDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                    //throw new Exception(ex.Message);             
                }
            }


            pattern = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.Return }, //,
                DocStatus = new Status { StatusID = DocStatus.InProcess }
            };

            //Lista de Ordenes nuevos que hay que revisar si se eliminaron
            //IList<Document> newList = Factory.DaoDocument().Select(pattern);
            newList = Factory.DaoDocument().Select(pattern);



            foreach (Document curDocument in newList)
            {
                try
                {
                    if (ErpFactory.Documents().SalesOrderWasDeleted(curDocument))
                    {
                        curDocument.DocStatus = cancelled;
                        Factory.DaoDocument().Update(curDocument);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("SalesOrderWasDeleted:Doc#" + curDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                    //throw new Exception(ex.Message);             
                }
            }




            #endregion
             * */


        }




        #region Inventory


        public Boolean CreateInventoryAdjustment(Document inventoryAdj, bool reload)
        {

            

            // CAA [2010/06/10]
            // Nueva opción para enviar o nó, al ERP [viene en el Doc.ref en el caso de NoCount Adjustments]
            bool ErpConnected = false;

            if (!string.IsNullOrEmpty(inventoryAdj.Reference) && inventoryAdj.Reference.Equals("SentToErp"))
                ErpConnected = true;

            else  if (!string.IsNullOrEmpty(inventoryAdj.Reference) && inventoryAdj.Reference.Equals("OnlyWms"))
                ErpConnected = false;
           else
                //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
                ErpConnected = GetCompanyOption(inventoryAdj.Company, "WITHERPIN").Equals("T");


            short adjType = 1; //1 = Adjustment 2 = Variance
            try { adjType = short.Parse(GetCompanyOption(inventoryAdj.Company, "ADJTYPE")); }
            catch { adjType = 1; }


            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (inventoryAdj.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(inventoryAdj.Company);
            }


            //Detalles del nodetrace
            //LIsta de Nodos de la transaccion
            IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(new NodeTrace { Document = new Document { DocID = inventoryAdj.DocID } });

            //Trae el documento completo de nuevo, para traer sus linea e info adicional 
            //que puede no llegar de la vista
            if (reload)
                inventoryAdj = Factory.DaoDocument().Select(new Document { DocID = inventoryAdj.DocID }).First();                           


            if (inventoryAdj.DocumentLines == null || inventoryAdj.DocumentLines.Count == 0)
                throw new Exception("Document " + inventoryAdj.DocNumber + " does not have lines.");


            Factory.IsTransactional = true;

            try
            {

                //Actualiza los nodos con el doc de Posteo
                foreach (NodeTrace nodeTrace in nodeTraceList)
                {
                    nodeTrace.PostingDocument = inventoryAdj;
                    nodeTrace.PostingDate = DateTime.Now;
                    nodeTrace.PostingUserName = "";
                    Factory.DaoNodeTrace().Update(nodeTrace);
                }


                for (int i = 0; i < inventoryAdj.DocumentLines.Count; i++)
                {
                    inventoryAdj.DocumentLines[i].PostingDocument = inventoryAdj.DocNumber;
                    inventoryAdj.DocumentLines[i].PostingDate = DateTime.Now;
                }



                inventoryAdj.PostingDocument = inventoryAdj.DocNumber;
                inventoryAdj.PostingDate = DateTime.Now;
                Factory.DaoDocument().Update(inventoryAdj);

                //Factory.Commit();

                //El documento de ajuste de inventario queda con el mismo numero que en WMS
                //Enviar el documento al ERP          
                string withError = "";
                if (ErpConnected)
                {
                    try { ErpFactory.Documents().CreateInventoryAdjustment(inventoryAdj, adjType); }
                    catch (Exception ex)
                    {
                        withError = ex.Message;
                        inventoryAdj.Comment += ex.Message;
                    }
                }

                //Cambia el Status del Documento - Actualizar el documento como Completed
                if (string.IsNullOrEmpty(withError))
                {
                    Factory.Commit();
                    inventoryAdj.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
                    Factory.DaoDocument().Update(inventoryAdj);

                }
                else
                {
                    Factory.Rollback();
                    inventoryAdj.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.NotCompleted });
                    Factory.DaoDocument().Update(inventoryAdj);

                    throw new Exception(withError);
                }



            }
            catch (Exception ex)
            {
                if (Factory.IsTransactional)
                    Factory.Rollback();

                //inventoryAdj.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                //Factory.DaoDocument().Update(inventoryAdj);

                ExceptionMngr.WriteEvent("CreateInventoryAdjustment:Doc#" + inventoryAdj.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                throw new Exception(ex.Message);
            }

            return true;

        }





        public void ReSendInventoryAdjustmentToERP(Document document)
        {
            //Si el sistema esta conectado a un ERP Crea actualiza la linea en el ERP
            bool ErpConnected = GetCompanyOption(document.Company, "WITHERPIN").Equals("T");

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (document.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(document.Company);
            }

            if (!ErpConnected)
                throw new Exception("No Erp Connection.");


            short adjType = 1; //1 = Adjustment 2 = Variance
            try { adjType = short.Parse(GetCompanyOption(document.Company, "ADJTYPE")); }
            catch { adjType = 1; }

            Document reloadedDoc = Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();

            ErpFactory.Documents().CreateInventoryAdjustment(reloadedDoc, adjType);
            reloadedDoc.DocStatus = new Status { StatusID = DocStatus.Completed };
            try { reloadedDoc.QuoteNumber = "Resend by " + document.ModifiedBy + ", " + DateTime.Now.ToString(); }
            catch { }

            Factory.DaoDocument().Update(reloadedDoc);

        }



        //Permite reversar un documento IA que fue posteado en el ERP, 
        //solo se reversa si en el ERP no lo ha posteado
        public void ReverseInventoryAdjustment(Document data)
        {

            //if (data.Company.ErpConnection == null)
            //    throw new Exception("Please setup Erp Connection.");

            Factory.IsTransactional = true;

            //SetConnectMngr(data.Company);

            Node storedNode = new Node { NodeID = NodeType.Stored };
            Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

            try
            {


                //Update document status to Cancelled
                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });
                Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                data.DocStatus = cancelled;
                Factory.DaoDocument().Update(data);

                //Pasa las lineas del documento a Cancelled                
                IList<DocumentLine> docLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = data });

                foreach (DocumentLine dl in docLines)
                {
                    dl.LineStatus = cancelled;
                    Factory.DaoDocumentLine().Update(dl);
                }

                //update NodeTrace
                NodeTrace qNodeTrace = new NodeTrace { PostingDocument = data };

                //Busca todo los registros de ese documento y los reversa
                IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(qNodeTrace);


                Label curLabel;

                foreach (NodeTrace trace in nodeTraceList)
                {

                    //Reverse labels que se generaron con el ajuste, si son negativos 
                    //Pasan de nuevo a stored, si son positivos pasan a void.
                    curLabel = trace.Label;
                    curLabel.Node = (trace.IsDebit == true) ? storedNode : voidNode;
                    curLabel.CurrQty = (trace.IsDebit == true) ? curLabel.CurrQty : 0;
                    curLabel.Status = (trace.IsDebit == true) ? active : inactive;
                    curLabel.ModDate = DateTime.Now;
                    curLabel.ModifiedBy = data.ModifiedBy;
                    Factory.DaoLabel().Update(curLabel);


                    //Crear un trace que tenga la transaccion del posting eliminado en el nodo void
                    //Registra el movimiento del nodo
                    SaveNodeTrace(
                        new NodeTrace
                        {
                            Node = (trace.IsDebit == true) ? storedNode : voidNode,
                            Document = trace.Document,
                            Label = trace.Label,
                            Quantity = trace.Quantity,
                            IsDebit = !trace.IsDebit,
                            CreatedBy = trace.CreatedBy,
                            PostingDocument = trace.PostingDocument,
                            PostingUserName = trace.PostingUserName,
                            Status = active, // inactive,
                            Comment = trace.PostingDocument.DocNumber + " Reversed",
                            ModDate = DateTime.Now,
                            ModifiedBy = data.ModifiedBy,
                            PostingDate = trace.PostingDate,
                        });

                    //Factory.DaoNodeTrace().Delete(trace);


                }

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReverseInventoryAdjustment #" + data.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Persistence);
                throw;
                //return;
            }
        }



        private void UpdateInventoryAdjustments()
        {
            Status posted = WType.GetStatus(new Status { StatusID = DocStatus.Posted });
            DocumentType docType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment };

            Document document; //for the foreach
            Document pattern = new Document { DocType = docType, DocStatus = new Status { StatusID = DocStatus.Completed } };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);

            foreach (Document curDocument in unPostedList)
            {
                try
                {
                    //Obtiene si esta posteado o no
                    document = ErpFactory.Documents().GetAdjustmentPostedStatus(curDocument);

                    if (document != null)
                    {
                        document.DocStatus = posted;
                        Factory.DaoDocument().Update(document);
                    }
                }
                catch { }
            }
        }



        private void UpdatePostedAssemblyOrders()
        {
            Status posted = WType.GetStatus(new Status { StatusID = DocStatus.Posted });
            DocumentType docType = new DocumentType { DocTypeID = SDocType.KitAssemblyTask };

            Document document; //for the foreach
            Document pattern = new Document
            {
                DocType = docType,
                DocStatus = new Status { StatusID = DocStatus.Completed }
            };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);

            foreach (Document curDocument in unPostedList)
            {
                try
                {
                    //Obtiene si esta posteado o no
                    document = ErpFactory.Documents().GetAssemblyOrderPostedStatus(curDocument);

                    if (document != null)
                    {
                        document.DocStatus = posted;
                        Factory.DaoDocument().Update(document);
                    }
                }
                catch { }
            }


           pattern = new Document
            {
                DocType = docType,
                DocStatus = new Status { StatusID = DocStatus.New }
            };


            //Lista de Ordenes nuevos que hay que revisar si se eliminaron
           IList<Document> newList = Factory.DaoDocument().Select(pattern);
           Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

           foreach (Document curDocument in newList)
           {
               try
               {
                   if (ErpFactory.Documents().AssemblyOrderWasDeleted(curDocument))
                   {
                       curDocument.DocStatus = cancelled;
                       Factory.DaoDocument().Update(curDocument);
                   }
               }
               catch { }
           }


        }



        public bool CreateLocationTransfer(Document docTranfer)
        {

            //JM 09 Oct 2010
            bool ErpConnected = GetCompanyOption(docTranfer.Company, "WITHERPIN").Equals("T"); 

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (docTranfer.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(docTranfer.Company);
            }


            Console.WriteLine("in Facade");


            Factory.IsTransactional = true;

            docTranfer = Factory.DaoDocument().Select(new Document { DocID = docTranfer.DocID }).First();

            Console.WriteLine("transact");

            try
            {


                for (int i = 0; i < docTranfer.DocumentLines.Count; i++)
                {
                    docTranfer.DocumentLines[i].PostingDocument = docTranfer.DocNumber;
                    docTranfer.DocumentLines[i].PostingDate = DateTime.Now;
                }



                docTranfer.PostingDocument = docTranfer.DocNumber;
                docTranfer.PostingDate = DateTime.Now;
                Factory.DaoDocument().Update(docTranfer);

                //Factory.Commit();

                //El documento de ajuste de inventario queda con el mismo numero que en WMS
                //Enviar el documento al ERP          
                string withError = "";
                Console.WriteLine("calling Erp");

                if (ErpConnected)
                {
                    Console.WriteLine("Inside Erp Calling");

                    try { ErpFactory.Documents().CreateLocationTransfer(docTranfer); }
                    catch (Exception ex)
                    {
                        withError = ex.Message;
                        docTranfer.Comment += ex.Message;
                    }
                }

                //Cambia el Status del Documento - Actualizar el documento como Completed
                if (string.IsNullOrEmpty(withError))
                {
                    Factory.Commit();

                    docTranfer.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
                    Factory.DaoDocument().Update(docTranfer);

                }
                else
                {
                    Factory.Rollback();

                    docTranfer.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.NotCompleted });
                    Factory.DaoDocument().Update(docTranfer);
                }



            }
            catch (Exception ex)
            {
                if (Factory.IsTransactional)
                    Factory.Rollback();

                ExceptionMngr.WriteEvent("CreateLocationTransfer:Doc#" + docTranfer.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                throw new Exception(ex.Message);
            }

            return true;
        }


        #endregion



        #region Receiving

        //Purchase receipit creation for ERP
        public Document CreatePurchaseReceipt(Document receivingTask)
        {            

            //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
            bool ErpConnected = GetCompanyOption(receivingTask.Company, "WITHERP").Equals("T");

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (receivingTask.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(receivingTask.Company);

            }


           //Valida si las tracking options del documento estan incluidas
            if (GetCompanyOption(receivingTask.Company, "RCERPTRACK").Equals("T"))
                ValidateReceiptDocumentTrackingOptions(receivingTask, new Node { NodeID = NodeType.Received }, true);           

            //Tipo de docuemnto purchase receipt
            DocumentType docType;
            if (receivingTask.DocType.DocTypeID == SDocType.PurchaseOrder || receivingTask.DocType.DocTypeID == SDocType.ReceivingTask)
                docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.PurchaseReceipt });
            
            else if (receivingTask.DocType.DocTypeID == SDocType.InTransitShipment)
                docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.InTransitReception });
            
            else
                docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.ReceiptConfirmation });
            

            DocumentTypeSequence docSeq = GetNextDocSequence(receivingTask.Company, docType);

            Document prDocument = null;

            //Almacena los Nodetrace a procesar y Arma las lineas segun los nodetrace
            IList<NodeTrace> traceList = null;


            Factory.IsTransactional = true;

            try
            {

                //Crear Document header
                prDocument = new Document
                 {
                     DocNumber = docSeq.CodeSequence,
                     Location = receivingTask.Location,
                     DocType = docType,
                     IsFromErp = false,
                     CrossDocking = false,
                     Vendor = receivingTask.Vendor,
                     Reference = receivingTask.Reference,
                     Date5 = receivingTask.Date5,
                     Date1 = DateTime.Now,
                     CustPONumber = receivingTask.DocNumber,
                     CreatedBy = receivingTask.ModifiedBy,
                     Company = receivingTask.Company,
                     Comment = "Receipt for Doc# " + receivingTask.DocNumber,
                     SalesPersonName = receivingTask.SalesPersonName,
                     QuoteNumber = receivingTask.QuoteNumber,
                     UserDef1 = receivingTask.UserDef1,
                     UserDef2 = receivingTask.UserDef2                     
                 };

                DocMngr.CreateNewDocument(prDocument, false);


                Object[] prResult = new Object[2];

                if (receivingTask.IsFromErp == true)
                    prResult = ReceiptLinesForErpDocument(receivingTask, prDocument);

                else
                    prResult = ReceiptLinesForTask(receivingTask, prDocument);


                //Info to update records after receipt creation in ERP
                prDocument.DocumentLines = (List<DocumentLine>)prResult[0]; //Armar las lineas del documento de Recibo
                traceList = (List<NodeTrace>)prResult[1];


                //Si el documento no tienen lineas se sale
                if (prDocument.DocumentLines == null || prDocument.DocumentLines.Count == 0)
                //{
                    //Factory.Rollback();

                    //ExceptionMngr.WriteEvent("CreatePurchaseReceipt:Doc#" + receivingTask.DocNumber + ". Document does not contain lines.",
                        //ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.ErpPosting);
                    throw new Exception("Document does not contain lines.");
                //}


                //Ajustando los nodetrace y los labels procesados
                Node stodeNode = Factory.DaoNode().Select(new Node { NodeID = NodeType.Stored }).First();
                ProcessReceivingTransactionTrace(traceList, prDocument, receivingTask, stodeNode);
                //Factory.Commit();


                #region ERP PROCESS

                //Enviar el documento de recibo al ERP si hay conexion con ERP configurada y solo is es un PO
                if (ErpConnected && receivingTask.DocType.DocTypeID == SDocType.PurchaseOrder)
                    ErpFactory.Documents().CreatePurchaseReceipt(prDocument, traceList, false);

                else if (ErpConnected && receivingTask.DocType.DocTypeID == SDocType.ReceivingTask)
                {
                    //Evalua el Zero Cost  Nov 23 de 2009
                    bool costZero = GetCompanyOption(receivingTask.Company, "RTZERO").Equals("T");
                    ErpFactory.Documents().CreatePurchaseReceipt(prDocument, traceList, costZero);
                }

                //Process Return to ERP     
                else if (ErpConnected && receivingTask.DocType.DocTypeID == SDocType.Return)
                {
                    prDocument.Comment = "Return Receipt for Doc# " + receivingTask.DocNumber;

                    IList<Label> listofReturn = Factory.DaoLabel().Select(new Label
                    {
                        ReceivingDocument = receivingTask,
                        Status = new Status { StatusID = EntityStatus.Active },
                        Node = new Node { NodeID = NodeType.Stored }
                    });

                    ErpFactory.Documents().ReceiptReturn(prDocument, listofReturn);
                }

                //Warehouise Transit Receipt FROM ERP - Caso DICERMEX
                else if (ErpConnected && receivingTask.DocType.DocTypeID == SDocType.InTransitShipment)
                    ErpFactory.Documents().CreateTransferReceipt(prDocument, traceList);


                #endregion


                //Commit
                Factory.Commit();

                 //Se dejan los documentos en InProcess cuando se posteen pasan a Completed
                Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
                Status inProcess = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });

                try
                {
                    //Update Receipt Document
                    prDocument.DocStatus = completed;
                    Factory.DaoDocument().Update(prDocument);
                    Factory.Commit();
                }
                catch //(Exception ex)
                {
                    //ExceptionMngr.WriteEvent("CreatePurchaseReceipt:Doc#" + prDocument.DocNumber, ListValues.EventType.Fatal,
                    //    ex, null, ListValues.ErrorCategory.ErpPosting);
                }


                //Update the Receiving Doc as In Process
                try
                {
                    if (receivingTask.Date5 == null)
                        receivingTask.Date5 = DateTime.Now;

                    receivingTask.DocStatus = completed;
                    Factory.DaoDocument().Update(receivingTask);
                }
                catch //(Exception ex)
                {
                    //ExceptionMngr.WriteEvent("CreatePurchaseReceipt:Doc#" + receivingTask.DocNumber, ListValues.EventType.Fatal,
                    //  ex, null, ListValues.ErrorCategory.ErpPosting);
                }


                //Commit
                Factory.Commit();

                //Mayo 26 de 2009 - JM, Opcion de ajuste de inventario para RETURN.
                //Lo que hace es disminuir un KIT del inventario y aumentar sus componentes
                /*
                 * COMENTARIADO 3/NOV/2009 - El Ajuste se hace manual
                if (receivingTask.DocType.DocTypeID == SDocType.Return && GetCompanyOption(prDocument.Company, "IVKITRET").Equals("T"))
                    CreateKitAssemblyComponentsAdjustment(prDocument, ErpConnected);
                */
                //Commit
                //Factory.Commit();

                //Print Document INn Batch.
                if (GetCompanyOption(receivingTask.Company, "RECTKT").Equals("T"))
                    try { BasicMngr.PrintDocumentsInBatch(new List<Document> { prDocument }, null, null, null); }
                    catch { }


                return prDocument;

            }
            catch (Exception ex)
            {
                Factory.Rollback();

                ExceptionMngr.WriteEvent("CreatePurchaseReceipt:Doc#" + receivingTask.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);

                //TODO: Ajusta la secuencia para reusar el numero
                //docSeq.NumSequence--;
                //Factory.DaoDocumentTypeSequence().Update(docSeq);
                //Factory.DaoDocument().Delete(prDocument);
                //ReverseProcessTransactionTrace(traceList);

                //Factory.IsTransactional = false;

                //Reversar el proceso que se hizo para los labels si se alcanzo a hacer.
                
                throw;
            }

        }


        //Crea un ajuste de INventario Cuando un Return Contiene Kits
        private void CreateKitAssemblyComponentsAdjustment(Document prDocument, bool ErpConnected)
        {
            Factory.IsTransactional = true;
            TransactionMngr tranMngr = new TransactionMngr();

            //Leer las lineas del Recibo que sean Kits
            IList<DocumentLine> kitLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = prDocument });

            kitLines = kitLines.Where(f => f.Product.Kit != null && f.Product.Kit.Count > 0 &&
                f.Product.Kit[0].ProductFormula != null && f.Product.Kit[0].ProductFormula.Count > 0).ToList();

            if (kitLines == null || kitLines.Count == 0)
                return;

            try
            {
                //Armar el Header del Ajuste
                Document curDocument = new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                    DocConcept = prDocument.DocConcept,
                    CreatedBy = prDocument.CreatedBy,
                    Location = prDocument.Location,
                    Company = prDocument.Company,
                    IsFromErp = false,
                    CrossDocking = false,
                    Comment = "Automatic Adjustment for " + prDocument.CustPONumber,
                    Date1 = DateTime.Now
                };
                DocMngr.CreateNewDocument(curDocument, false);

                //Return  Location
                Label sourceBinLocaton = GetBinLabel(DefaultBin.RETURN, prDocument.Location);
                DocumentLine adjKit = null;
                DocumentLine adjComponent = null;

                //Armar las lineas de los ajuste
                int count = 1;
                foreach (DocumentLine dl in kitLines)
                {
                    //Lineas de Ajuste Negativo
                    adjKit = new DocumentLine
                    {
                        Product = dl.Product,
                        Unit = dl.Product.BaseUnit,
                        Quantity = dl.Quantity,
                        CreatedBy = dl.CreatedBy,
                        LineStatus = dl.LineStatus,
                        IsDebit = true,
                        UnitBaseFactor = dl.Product.BaseUnit.BaseAmount,
                        Document = curDocument,
                        Location = curDocument.Location,
                        LineNumber = count++
                    };

                    adjKit = tranMngr.SaveAdjustmentTransaction(adjKit, sourceBinLocaton, false);

                    if (adjComponent.Note != "Adjust OK.")
                        throw new Exception(adjComponent.Note);



                    //Lineas de Ajuste Positivo

                    if (dl.Product.Kit != null && dl.Product.Kit.Count > 0 &&
                         dl.Product.Kit[0].ProductFormula != null && dl.Product.Kit[0].ProductFormula.Count > 0)
                    {


                        foreach (KitAssemblyFormula formula in dl.Product.Kit[0].ProductFormula)
                        {
                            //Lineas de Ajuste Positivo
                            adjComponent = new DocumentLine
                            {
                                Product = formula.Component,
                                Unit = formula.Component.BaseUnit,
                                Quantity = dl.Quantity * formula.FormulaQty,
                                CreatedBy = dl.CreatedBy,
                                LineStatus = dl.LineStatus,
                                IsDebit = false,
                                UnitBaseFactor = formula.Component.BaseUnit.BaseAmount,
                                Document = curDocument,
                                Location = curDocument.Location,
                                LineNumber = count++
                            };

                            adjComponent = tranMngr.SaveAdjustmentTransaction(adjComponent, sourceBinLocaton, false);

                            if (adjComponent.Note != "Adjust OK.")
                                throw new Exception(adjComponent.Note);

                        }
                    }
                }

                //MAndar el ajuste al ERP. Si hay conexion con el ERP.
                if (ErpConnected)
                    CreateInventoryAdjustment(curDocument,false);

                Factory.Commit();

            }
            catch (Exception ex) {                 
                Factory.Rollback();
                ExceptionMngr.WriteEvent("CreateKitAssemblyComponentsAdjustment", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);

            }

        }



        //Cuando las lineas no requieren de hacer link con un Documento en el ERP
        private Object[] ReceiptLinesForTask(Document receivingTask, Document prDocument)
        {
            Object[] result = new Object[2];

            try
            {
                Node recNode = new Node { NodeID = NodeType.Received };
                //Node storedNode = new Node { NodeID = NodeType.Stored };

                //Armar las lineas del documento de Recibo
                IList<DocumentLine> prDocLines = new List<DocumentLine>();

                //Obtiene los nodetrace records a recorrer, 
                //los que no han sido posteados para esa tarea de recibo
                NodeTrace qNodeTrace = new NodeTrace { Document = receivingTask, PostingDocument = new Document { DocID = 0 }, Node = recNode };
                IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(qNodeTrace);
                Status status = WType.GetStatus(new Status { StatusID = DocStatus.New });

                DocumentLine rpLine;
                Double line = 0;

                //Leva el conteo del acumulado a recibir
                IDictionary<UnitProductRelation, Double[]> receiveBalance = new Dictionary<UnitProductRelation, Double[]>();
                UnitProductRelation curUnitProduct;

                //Consolidar las lineas de la misma unidad y hacer una sola linea
                foreach (NodeTrace nodeTrace in nodeTraceList.Where(f=>f.Label.StockQty > 0))
                {
                    //Armando el consolidado
                    curUnitProduct = Factory.DaoUnitProductRelation().Select(
                        new UnitProductRelation
                        {
                            Unit = nodeTrace.Label.Unit,
                            Product = nodeTrace.Label.Product
                        }).First();

                    if (receiveBalance.ContainsKey(curUnitProduct)) 
                        receiveBalance[curUnitProduct][1] += nodeTrace.Quantity;
                    else
                    {
                        line += 1;
                        receiveBalance.Add(curUnitProduct, new Double[2]{line, nodeTrace.Quantity});
                    }
                                            

                    //Update Node Trace
                    nodeTrace.Document = receivingTask;
                    nodeTrace.PostingDate = DateTime.Now;
                    nodeTrace.PostingDocument = prDocument;
                    nodeTrace.PostingDocLineNumber = (Int32)line;
                    nodeTrace.PostingUserName = prDocument.CreatedBy;
                }



                //Recorre la coleccion de balance para adicionar las demas lineas del recibo
                foreach (UnitProductRelation unitProductRel in receiveBalance.Keys)
                {
                    //Si hay cantidad (elemento 1)
                    if (receiveBalance[unitProductRel][1] > 0)
                    {

                        //Crea una linea para el documento de recibo obteniedo la info del balance
                        rpLine = new DocumentLine
                        {
                            Product = unitProductRel.Product,
                            Quantity = receiveBalance[unitProductRel][1],
                            Unit = unitProductRel.Unit,
                            Document = prDocument,
                            CreationDate = DateTime.Now,
                            IsDebit = false,
                            LineNumber = (Int32)receiveBalance[unitProductRel][0],
                            LineStatus = status,
                            Location = prDocument.Location,
                            UnitBaseFactor = unitProductRel.Unit.BaseAmount,
                            CreatedBy = prDocument.CreatedBy
                        };

                        prDocLines.Add(rpLine);
                    }
                }

                result[0] = prDocLines;
                result[1] = nodeTraceList;

                return result;

            }
            catch (Exception ex)
            {
                //Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiptLinesForTask", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                return null;
                //throw;
            }
        }


        //Cuando las lineas deben hacer link con un documento del ERP, solo se permite que el Uom Coincida
        //Con el del documento

        private Object[] ReceiptLinesForErpDocument(Document receivingTask, Document prDocument)
        {
            Object[] result = new Object[2];

            try
            {

                Node recNode = new Node { NodeID = NodeType.Received };
                Node storedNode = new Node { NodeID = NodeType.Stored };
                DocumentBalance docBal = new DocumentBalance { Document = receivingTask, Node = storedNode };

                //Obtiene el balance linea por linea, Es decir el cruce contra lo ya posteado, porque la linea se
                //asigna una vez se postea, lo que no tiene linea no aparece en el balance
                //es decir el saldo que queda por llenar de cada linea de documento
                IList<DocumentBalance> balanceList = Factory.DaoDocumentBalance().DetailedBalance(docBal, false);
                balanceList = balanceList.OrderBy(f => f.DocumentLine.LineNumber).ToList();  //Ordernada por LineNumber

                //Armar las lineas del documento de Recibo
                IList<DocumentLine> prDocLines = new List<DocumentLine>();
                NodeTrace qNodeTrace;
                DocumentLine rpLine;
                int line = 1;
                IEnumerable<NodeTrace> nodeTraceSameUnit;
                IList<NodeTrace> allNodeTrace = new List<NodeTrace>();
                Dictionary<long, double> acumLines = new Dictionary<long, double>();

                Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });
                //Label curLabel;


                //Obtienen los saldos pendientes de lo que se recibio para ese documento.
                nodeTraceSameUnit =  Factory.DaoNodeTrace().Select(
                    new NodeTrace
                    {
                        Document = receivingTask,
                        Node = recNode,
                        PostingDocument = new Document { DocID = 0 }, //0 mean Null
                        //Label = new Label { Product = balance.Product, Unit = balance.Unit }
                    }).Where(f => f.Quantity > 0);


                //Recorre los NodeTrace en Receiving que tengan saldo y tengan balance.
                double traceBalance;
                double receivedLineQty;
                int xLine = 0;
                foreach (NodeTrace curTrace in nodeTraceSameUnit.Where(f => f.Label.Product != null).OrderBy(f => f.Label.Product.ProductID))
                {
                    traceBalance = curTrace.Quantity * curTrace.Unit.BaseAmount;

                    //Saca todas las lineas del mismos producto que tenga la misma unidad y tengan saldo pendiente.
                    foreach (DocumentBalance curline in balanceList
                        .Where(f => f.Product.ProductID == curTrace.Label.Product.ProductID
                            //&& (f.Unit.UnitID == curTrace.Label.Unit.UnitID || f.Unit.BaseAmount == curTrace.Label.Unit.BaseAmount)
                            && f.QtyPending > 0))
                    {

                        //Para el consecutivo de la linea
                        if (!acumLines.ContainsKey(curline.DocumentLine.LineID))
                            xLine++;


                        //Si el valor de la cantidad del label actual es mayor o igual a la pendiente.
                        if (traceBalance >= curline.BaseQtyPending) //(traceBalance >= curline.QtyPending)
                        {
                            traceBalance -= curline.BaseQtyPending;
                            receivedLineQty = curline.BaseQtyPending;
                            curline.QtyPending = 0;
                        }
                        else //Si es menor
                        {
                            curline.QtyPending -= traceBalance / curline.Unit.BaseAmount; //nodeTraceSameUnit.Sum(f => f.Quantity);
                            receivedLineQty = traceBalance; //nodeTraceSameUnit.Sum(f => f.Quantity);
                            traceBalance = 0;
                        }


                        //Creado los nuevos traces en el STORED.
                        curTrace.DocumentLine = curline.DocumentLine;
                        curTrace.PostingDate = DateTime.Now;
                        curTrace.PostingDocument = prDocument;
                        curTrace.PostingDocLineNumber = line;
                        curTrace.PostingUserName = prDocument.CreatedBy;
                        curTrace.ModifiedBy = prDocument.CreatedBy;
                        curTrace.ModDate = DateTime.Now;

                        //Nuevo node para el caso de Receiving.
                        Factory.DaoNodeTrace().Update(curTrace);
                        //allNodeTrace.Add(curTrace);

                        if (receivedLineQty > 0)
                            allNodeTrace.Add(new NodeTrace
                            {
                                Node = curTrace.Node,
                                Document = curTrace.Document,
                                Label = curTrace.Label,
                                Quantity = receivedLineQty,
                                IsDebit = curTrace.IsDebit,
                                CreatedBy = curTrace.CreatedBy,
                                PostingDocument = curTrace.PostingDocument,
                                PostingDocLineNumber = xLine,
                                PostingUserName = prDocument.CreatedBy,
                                Bin = curTrace.Bin,
                                CreationDate = DateTime.Now,
                                PostingDate = DateTime.Now,
                                Status = curTrace.Status,
                                Unit = curline.Product.BaseUnit, //curline.Unit,
                                FatherLabel = curTrace.Label.FatherLabel
                            });


                        //Crea el acumulador para cada linea.
                        if (acumLines.ContainsKey(curline.DocumentLine.LineID))
                            acumLines[curline.DocumentLine.LineID] += receivedLineQty;
                        else
                            acumLines.Add(curline.DocumentLine.LineID, receivedLineQty);

                    }

                    if (traceBalance <= 0)
                        continue;
                    else //Reportar que hubo Overreceived.
                    {
                        ExceptionMngr.WriteEvent("ReceiptLinesForErpDocument: Overreceived: " + curTrace.Document.DocNumber
                            + ", Product: " + curTrace.Label.Product.Name,
                            ListValues.EventType.Warn, null, null,
                            ListValues.ErrorCategory.Business);

                    }
                }


                //Para todos los saldos de las lineas.
                //Crea las lineas respectivas.
                DocumentLine oriLine;
                foreach (long lineID in acumLines.Keys)
                {

                    oriLine = balanceList.Where(f => f.DocumentLine.LineID == lineID).Select(f => f.DocumentLine).First();

                    //Crea una linea para el documento de fullfilment
                    rpLine = new DocumentLine
                    {
                        Product = oriLine.Product,
                        AccountItem = oriLine.AccountItem,
                        Quantity = acumLines[lineID] / oriLine.Unit.BaseAmount,  //nodeTraceSameUnit.Sum(f => f.Quantity),
                        Unit = oriLine.Unit,
                        Document = prDocument,
                        CreationDate = DateTime.Now,
                        IsDebit = false,
                        LineNumber = line,
                        LineStatus = lineStatus,
                        Location = prDocument.Location,
                        UnitBaseFactor = oriLine.Unit.BaseAmount,
                        LinkDocNumber = oriLine.Document.DocNumber,
                        LinkDocLineNumber = oriLine.LineNumber,
                        CreatedBy = prDocument.CreatedBy,
                        Note = oriLine.Note, //If Note=2 indica que es un componente
                        Sequence = oriLine.Sequence,
                        Date1 = DateTime.Now,
                        //UnitPrice = oriLine.UnitPrice,
                        //ExtendedPrice = oriLine.UnitPrice * acumLines[lineID] * oriLine.Unit.BaseAmount,
                        UnitCost = oriLine.UnitCost,
                        ExtendedCost = oriLine.UnitCost * acumLines[lineID] * oriLine.Unit.BaseAmount,
                        Date2 = oriLine.Date2, //Pone las fechas en el recibo, siver para las fechas de entrega Date2 en ENTERPRISE
                        QtyOnHand = oriLine.UnitBaseFactor,
                        BinAffected = oriLine.BinAffected,
                        PostingUserName = oriLine.PostingUserName
                    };

                    


                    prDocLines.Add(rpLine);
                    line++;
                }



                result[0] = prDocLines;
                result[1] = allNodeTrace;

                return result;
            }
            catch (Exception ex)
            {
                //Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiptLinesForErpDocument", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                //throw;
                return null;
            }
        }


        /*
        private Object[] ReceiptLinesForErpDocument(Document receivingTask, Document prDocument)
        {
            Object[] result = new Object[2];

            try
            {

                Node recNode = new Node { NodeID = NodeType.Received };
                Node storedNode = new Node { NodeID = NodeType.Stored };
                DocumentBalance docBal = new DocumentBalance { Document = receivingTask, Node = storedNode };

                //Obtiene el balance linea por linea, Es decir el cruce contra lo ya posteado, porque la linea se
                //asigna una vez se postea, lo que no tiene linea no aparece en el balance
                //es decir el saldo que queda por llenar de cada linea de documento
                IList<DocumentBalance> balanceList = Factory.DaoDocumentBalance().DetailedBalance(docBal, false);
                balanceList = balanceList.OrderBy(f => f.DocumentLine.LineNumber).ToList();  //Ordernada por LineNumber

                //Armar las lineas del documento de Recibo
                IList<DocumentLine> prDocLines = new List<DocumentLine>();
                NodeTrace qNodeTrace;
                DocumentLine rpLine;
                int line = 1;
                IList<NodeTrace> nodeTraceSameUnit;
                IList<NodeTrace> allNodeTrace = new List<NodeTrace>();

                Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });
                //Label curLabel;


                //Recorre los saldos pendientes para ese documento
                foreach (DocumentBalance balance in balanceList.Where(b => b.QtyPending > 0))
                {

                    qNodeTrace = new NodeTrace
                    {
                        Document = receivingTask,
                        Node = recNode,
                        PostingDocument = new Document { DocID = 0 }, //0 mean Null
                        Label = new Label { Product = balance.Product, Unit = balance.Unit }
                    };

                    //PASO 1 : Primero buscar los nodetrace que coincidan con la unidad necesitada y traer los pendientes
                    nodeTraceSameUnit = Factory.DaoNodeTrace().Select(qNodeTrace).ToList(); //.Take(int.Parse(balance.QtyPending.ToString()))

                    //Si se obtienen registros
                    if (nodeTraceSameUnit.Count > 0)
                    {

                        //1.1 Actualiza los que encontro
                        foreach (NodeTrace traceSame in nodeTraceSameUnit)
                        {
                            traceSame.DocumentLine = balance.DocumentLine;
                            traceSame.PostingDate = DateTime.Now;
                            traceSame.PostingDocument = prDocument;
                            traceSame.PostingDocLineNumber = line;
                            traceSame.PostingUserName = prDocument.CreatedBy;
                            traceSame.ModifiedBy = prDocument.CreatedBy;
                            traceSame.ModDate = DateTime.Now;

                            allNodeTrace.Add(traceSame);

                        }


                        //1.2 Disminuye el balance
                        balance.QtyPending -= nodeTraceSameUnit.Sum(f => f.Quantity);


                        //Crea una linea para el documento de recibo
                        rpLine = new DocumentLine
                        {
                            Product = balance.Product,
                            Quantity = nodeTraceSameUnit.Sum(f => f.Quantity),
                            Unit = balance.Unit,
                            Document = prDocument,
                            CreationDate = DateTime.Now,
                            IsDebit = false,
                            LineNumber = line,
                            LineStatus = lineStatus,
                            Location = prDocument.Location,
                            UnitBaseFactor = balance.Unit.BaseAmount,
                            LinkDocNumber = balance.Document.DocNumber,
                            LinkDocLineNumber = balance.DocumentLine.LineNumber,
                            CreatedBy = prDocument.CreatedBy,
                            UnitPrice = balance.UnitPrice,
                            ExtendedPrice = balance.UnitPrice * nodeTraceSameUnit.Sum(f => f.Quantity) * balance.Unit.BaseAmount
                        };

                        prDocLines.Add(rpLine);
                        line++;

                    }

                    //1.4 Si encontro todos los que necesitaba se sale a la siguiente linea
                    if (balance.QtyPending <= 0)
                        continue;

                }

                result[0] = prDocLines;
                result[1] = allNodeTrace;

                return result;
            }
            catch (Exception ex)
            {
                //Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiptLinesForErpDocument", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                //throw;
                return null;
            }                             
                 
        }
        */

        //Actualiza el nodetrace y crea los Nuevos Registros de trace
        private void ProcessReceivingTransactionTrace(IList<NodeTrace> traceList, Document postedDocument, 
            Document taskDocument, Node node)
        {
            if (traceList == null)
                return;

            Label curLabel;
            Status activeStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            foreach (NodeTrace nodeTrace in traceList)
            {
                //Actualiza el trace actual
                //Factory.DaoNodeTrace().Update(nodeTrace);

                //Pasa los labels de ese nodeTrace a Storedb
                //Registra el movimiento del nodo
                SaveNodeTrace(
                    new NodeTrace
                    {
                        Node = node,
                        Document = taskDocument,
                        Label = nodeTrace.Label,
                        Quantity = nodeTrace.Label.StockQty, //GetLabelStockQty(new Label { LabelID = nodeTrace.Label.LabelID }), //nodeTrace.Label.CurrQty
                        IsDebit = nodeTrace.IsDebit,
                        CreatedBy = postedDocument.CreatedBy,
                        PostingDocument = postedDocument,
                        PostingDocLineNumber = nodeTrace.PostingDocLineNumber,
                        PostingUserName = postedDocument.CreatedBy
                    });

                //Factory.Commit();

                //Pasa los labels de ese nodeTrace a Stored
                curLabel = nodeTrace.Label;
                curLabel.Node = node;
                curLabel.ModDate = DateTime.Now;
                curLabel.Status = activeStatus;
                curLabel.ModifiedBy = postedDocument.CreatedBy;

                if (curLabel.LabelCode.StartsWith("VOID_"))
                    curLabel.LabelCode = curLabel.LabelCode.Replace("VOID_","");

                Factory.DaoLabel().Update(curLabel);


                //Modificando las hijas
                try
                {
                    curLabel.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = curLabel });

                    if (curLabel.ChildLabels != null && curLabel.ChildLabels.Count > 0)

                        foreach (Label child in curLabel.ChildLabels)
                        {
                            child.Node = node;
                            child.ModDate = DateTime.Now;
                            child.ModifiedBy = child.FatherLabel.CreatedBy;
                            child.Status = activeStatus;

                            Factory.DaoLabel().Update(child);
                        }
                }
                catch { }

            }
        }



        //Actualiza el nodetrace y crea los Nuevos Registros de trace
        private void ProcessShippingTransactionTrace(IList<NodeTrace> traceList, Document postedDocument,
            Document taskDocument, Node node)
        {
            if (traceList == null)
                return;

            Label curLabel;

            foreach (NodeTrace nodeTrace in traceList)
            {
                //Actualiza el trace actual
                //Factory.DaoNodeTrace().Update(nodeTrace);

                //Pasa los labels de ese nodeTrace a Storedb
                //Registra el movimiento del nodo
                SaveNodeTrace(
                    new NodeTrace
                    {
                        Node = node,
                        Document = taskDocument,
                        Label = nodeTrace.Label,
                        Quantity = nodeTrace.Quantity, //GetLabelStockQty(new Label { LabelID = nodeTrace.Label.LabelID }), //nodeTrace.Label.CurrQty
                        IsDebit = nodeTrace.IsDebit,
                        CreatedBy = postedDocument.CreatedBy,
                        PostingDocument = postedDocument,
                        PostingDocLineNumber = nodeTrace.PostingDocLineNumber,
                        PostingUserName = postedDocument.CreatedBy
                    });

                //Factory.Commit();

                //Pasa los labels de ese nodeTrace a Stored o Node defined.
                curLabel = nodeTrace.Label;
                curLabel.Node = node;
                curLabel.ModDate = DateTime.Now;
                curLabel.ModifiedBy = postedDocument.CreatedBy;

                Factory.DaoLabel().Update(curLabel);


                //Modificando las hijas
                try
                {
                    curLabel.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = curLabel });
                    if (curLabel.ChildLabels != null && curLabel.ChildLabels.Count > 0)

                        foreach (Label child in curLabel.ChildLabels)
                        {
                            child.Node = node;
                            child.ModDate = DateTime.Now;
                            child.ModifiedBy = child.FatherLabel.CreatedBy;

                            Factory.DaoLabel().Update(child);
                        }
                }
                catch { }

            }


            #region Updating QtyShipped - JM Ene21/2010
            //Adicionando el ShippedQty to the Original Order Qher is a MergedOrder
            if (taskDocument.DocType.DocTypeID == SDocType.MergedSalesOrder)
            {              
                try
                {
                    //DocumentLine mergedLine;
                    DocumentLine oriLine;
                    //ProductInventory prInventory;

                    //1. Toma las lineas del Shipped
                    foreach (DocumentLine shipLine in postedDocument.DocumentLines)
                    {
                        try
                        {
                            //2. Busca las lineas del merged y luego la original
                            /*
                            mergedLine = Factory.DaoDocumentLine().Select(new DocumentLine
                            {
                                Document = new Document { DocNumber = shipLine.LinkDocNumber, Company = postedDocument.Company },
                                LineNumber = shipLine.LinkDocLineNumber
                            }).First();
                            */
                            //mergedLine = taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber).First();


                            //2. Busca las lineas del merged original
                            oriLine = Factory.DaoDocumentLine().Select(new DocumentLine
                            {
                                Document = new Document { 
                                    DocNumber = taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber)
                                    .First().LinkDocNumber, 
                                    Company = postedDocument.Company },
                               
                                LineNumber = taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber)
                                .First().LinkDocLineNumber

                            }).First();

                            //3. Actualiza el QtyShipped.
                            oriLine.QtyShipped += shipLine.Quantity;
                            oriLine.ModDate = DateTime.Now;
                            oriLine.ModifiedBy = postedDocument.CreatedBy;

                            if (oriLine.QtyBackOrder > 0)
                                oriLine.LineStatus = new Status { StatusID = DocStatus.InProcess };

                            if (oriLine.Quantity - oriLine.QtyShipped < 0)
                                oriLine.LineStatus = new Status { StatusID = DocStatus.Completed };

                            Factory.DaoDocumentLine().Update(oriLine);


                            //4. Actualiza el QtyShipped En el Merged.
                            /*
                            try
                            {

                                taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber).First().QtyShipped += shipLine.Quantity;
                                taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber).First().ModDate = DateTime.Now;
                                taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber).First().ModifiedBy = postedDocument.CreatedBy;
                                Factory.DaoDocumentLine().Update(taskDocument.DocumentLines.Where(f => f.LineNumber == shipLine.LinkDocLineNumber).First());
                                 
                                mergedLine.QtyShipped += shipLine.Quantity;
                                mergedLine.ModDate = DateTime.Now;
                                mergedLine.ModifiedBy = postedDocument.CreatedBy;
                                Factory.DaoDocumentLine().Update(mergedLine);
                             

                            }
                            catch { }
                             **/

                            //Borrar esa cantidad de lo allocated para ese documento y ese producto.
                            /*
                            try
                            {
                                prInventory = Factory.DaoProductInventory().Select(
                                    new ProductInventory { Document = oriLine.Document, Product = oriLine.Product }).First();

                                if (prInventory.QtyAllocated - shipLine.Quantity <= 0 && prInventory.QtyInUse == 0)
                                    Factory.DaoProductInventory().Delete(prInventory);
                                else
                                {
                                    prInventory.QtyAllocated -= shipLine.Quantity;
                                    if (prInventory.QtyAllocated < 0)
                                        prInventory.QtyAllocated = 0;
                                    Factory.DaoProductInventory().Update(prInventory);
                                }
                            }
                            catch { }
                            */


                        }
                        catch (Exception ex)
                        {
                            ExceptionMngr.WriteEvent("Updating ShipQty:" + postedDocument.DocNumber + ", Line:" + shipLine.LineNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                            continue;
                        }

                    }
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("Updating ShipQty:" + postedDocument.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                }

            }

            #endregion

        }



        //Reversa el proceso para el node trace cuando falle, double check
        private void ReverseProcessTransactionTrace(IList<NodeTrace> traceList)
        {
            if (traceList == null)
                return;

            foreach (NodeTrace nodeTrace in traceList)
            {
                nodeTrace.DocumentLine = null;
                nodeTrace.PostingDate = null;
                nodeTrace.PostingDocument = null;
                nodeTrace.PostingDocLineNumber = 0;
                nodeTrace.PostingUserName = null;

                //Actualiza el trace actual
                Factory.DaoNodeTrace().Update(nodeTrace);

            }
        }



        private void UpdatePostedReceipts()
        {
            Status posted = WType.GetStatus(new Status { StatusID = DocStatus.Posted });
            DocumentType docType = new DocumentType { DocTypeID = SDocType.PurchaseReceipt };

            Document document; //for the foreach
            Document pattern = new Document { DocType = docType, DocStatus = new Status { StatusID = DocStatus.Completed } };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);

            foreach (Document curDocument in unPostedList)
            {
                try
                {
                    //Obtiene si esta posteado o no
                    document = ErpFactory.Documents().GetReceiptPostedStatus(curDocument);

                    if (document != null)
                    {
                        document.DocStatus = posted;
                        Factory.DaoDocument().Update(document);
                    }
                }
                catch (Exception ex) {
                    ExceptionMngr.WriteEvent("UpdatePostedReceipts:Doc#" + curDocument.DocNumber, ListValues.EventType.Fatal, ex, null, 
                        ListValues.ErrorCategory.Business);
                }
            }
        }



        #endregion



        #region ##### Shipping Process


        public Document CreateShipmentDocument(Document shipTask)
        {
            

            //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
            bool ErpConnected = GetCompanyOption(shipTask.Company, "WITHERPSH").Equals("T");
            bool existsPreviowsShipment = false;
            Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (shipTask.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(shipTask.Company);
            }


            string docNumberSeq = null;
            Document ssDocument = null;
            //Obtiene los Nodetrace a procesar y Arma las lineas segun los nodetrace
            IList<NodeTrace> traceList  = null;
            Connection curConnection;
          

            DocumentType docType = new DocumentType { DocTypeID = SDocType.SalesShipment };
            bool fistTimeFulfill = true;

            //ADICION Marzo 9 de 2010 //Si la variable ONESHIP = 'T' debe utilizar el mismo Shipment
            //si ya existe y no crear uno nuevo 
            try
            {
                if (GetCompanyOption(shipTask.Company, "ONESHIPMENT").Equals("T"))
                {
                    ssDocument = Factory.DaoDocument().Select(new Document
                    {
                        Company = shipTask.Company,
                        CustPONumber = shipTask.DocNumber,
                        DocStatus = new Status { StatusID = DocStatus.Completed }
                    }).First();

                    existsPreviowsShipment = true;
                }
            }
            catch { };


            if (ssDocument == null)
            {
                //Se obtinen el numero de fullfill documents para este sales docuemnt.
                //Porque si es primera vez el tratameinto es diferente.
                try
                {
                    fistTimeFulfill = Factory.DaoDocument().Select(new Document
                    {
                        DocType = docType,
                        CustPONumber = shipTask.DocNumber,
                        Company = shipTask.Company,
                        DocStatus = new Status { StatusID = DocStatus.Completed }
                    }).Count > 0 ? false : true;
                }
                catch { fistTimeFulfill = false; }


                //Tipo de docuemnto Sales Shipment, en que legaliza el piqeuo y salida de la mercancia
                if (string.IsNullOrEmpty(shipTask.PostingDocument))
                    docNumberSeq = GetNextDocSequence(shipTask.Company, docType).CodeSequence;
                else
                    docNumberSeq = shipTask.PostingDocument; //21 SEP Cuando ya el shipment tiene numero preasignado.
            }


            //Refresh Document Lines
            shipTask.DocumentLines = Factory.DaoDocumentLine().Select(
                new DocumentLine { Document = new Document { DocID = shipTask.DocID } });


            Factory.IsTransactional = true;

            try
            {
               

                #region Doc Creation

                if (ssDocument == null)
                {
                    //Crear Document header
                    ssDocument = new Document
                    {
                        DocNumber = docNumberSeq, //docSeq.CodeSequence,
                        Location = shipTask.Location,
                        DocType = docType,
                        IsFromErp = false,
                        CrossDocking = false,
                        Customer = shipTask.Customer,
                        Reference = shipTask.Reference,
                        Date1 = DateTime.Now,
                        CustPONumber = shipTask.DocNumber,
                        CreatedBy = shipTask.ModifiedBy,
                        Company = shipTask.Company,
                        Comment = "Shipment for Doc# " + shipTask.DocNumber,
                        UseAllocation = shipTask.UseAllocation,
                        Date3 = shipTask.Date3,
                        Date4 = shipTask.Date4,
                        QuoteNumber = shipTask.CustPONumber,
                        Notes = shipTask.Notes,
                        UserDef1 = shipTask.UserDef1,
                        UserDef2 = shipTask.UserDef2,
                        UserDef3 = shipTask.UserDef3

                    };
                    DocMngr.CreateNewDocument(ssDocument, false);

                }
                else if (existsPreviowsShipment)
                {
                    Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });                    
                    Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
                    Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

                    //Remueve todo los datos de informacion de posteo de los labels
                    //para hacer Repost
                    IList<NodeTrace> nodeShipment = Factory.DaoNodeTrace().Select(new NodeTrace { PostingDocument = ssDocument });
                    foreach (NodeTrace ndShip in nodeShipment)
                    {

                        if (ndShip.Node.NodeID == NodeType.Released)
                        {

                            ndShip.Node = voidNode;
                            ndShip.Status = inactive;
                            ndShip.ModDate = DateTime.Now;
                            Factory.DaoNodeTrace().Update(ndShip);
                        }

                        if (ndShip.Node.NodeID == NodeType.Picked)
                        {
                            ndShip.DocumentLine = null;
                            ndShip.PostingDate = null;
                            ndShip.PostingDocument = null;
                            ndShip.PostingUserName = null;
                            ndShip.PostingDocLineNumber = 0;
                            ndShip.ModDate = DateTime.Now;
                        }

                        Factory.DaoNodeTrace().Update(ndShip);
                    }

                    //DELETE the Shipment Document Lines
                    if (ssDocument != null && ssDocument.DocID > 0)
                        foreach (DocumentLine shLine in Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = ssDocument.DocID } }))
                            Factory.DaoDocumentLine().Delete(shLine);

                }


                Console.WriteLine("0. Start");

                Object[] ssResult = new Object[2];

                if (shipTask.IsFromErp == true)
                    ssResult = SalesLinesForErpDocument(shipTask, ssDocument);

                else
                    ssResult = SalesLinesForTask(shipTask, ssDocument);


                Console.WriteLine("1. Getting SalesLines");

                //Info to update records after receipt creation in ERP
                ssDocument.DocumentLines = (List<DocumentLine>)ssResult[0]; //Armar las lineas del documento de Recibo
                traceList = (List<NodeTrace>)ssResult[1];

                //Si el documento no tienen lineas se sale
                if (ssDocument.DocumentLines == null || ssDocument.DocumentLines.Count == 0)
                    //ExceptionMngr.WriteEvent("CreateShipmentDocument:Doc#" + shipTask.DocNumber + ". Document does not contain lines.", 
                        //ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.ErpPosting);
                    throw new Exception("Document does not contain lines.");


                #endregion


                #region Additional Process & Packages


                Console.WriteLine("2. Kits");

                //Mayo 19 de 2009 - Jairo Murillo
                //Si la opcion SHOWCOMP esta activa y si es un documento del ERP. debe acomodarse el 
                //Shipment con los Kits y ordenar la sequence.
                if (GetCompanyOption(shipTask.Company, "SHOWCOMP").Equals("T") && shipTask.IsFromErp == true)
                    ssDocument.DocumentLines = GetLinesWithKitAssemblyHeaders(ssDocument.DocumentLines);


                string batchNumber="";

                //El proceso de Fullfilment se hace solo para sales orders
                if (ErpConnected && shipTask.DocType.DocTypeID == SDocType.SalesOrder)
                {
                    //Enviar el documento de shipping al ERP
                    curConnection = ssDocument.Company.ErpConnection;

                    //Evaluar si debe crear/full fill order en el ERP
                    if (GetCompanyOption(shipTask.Company, "CREERPSH").Equals("T"))
                    {
                        //Evaular si debe Update BATCH
                        if (GetCompanyOption(shipTask.Company, "UPDSHBATCH").Equals("T"))
                            batchNumber = string.IsNullOrEmpty(shipTask.Location.BatchNo) ? "" : shipTask.Location.BatchNo;


                        Console.WriteLine("3. In GP");
                        string shortAge = GetCompanyOption(shipTask.Company, "SHORTAGE");
                        ErpFactory.Documents().FulFillSalesDocument(ssDocument, shortAge, fistTimeFulfill, batchNumber);
                    }

  
                }
                else if (ErpConnected && shipTask.DocType.DocTypeID == SDocType.MergedSalesOrder) //FULLFILMENT of Merged Sales Ordes
                {
                    //Enviar el documento de shipping al ERP
                    curConnection = ssDocument.Company.ErpConnection;

                    //Evaluar si debe crear/full fill order en el ERP
                    if (GetCompanyOption(shipTask.Company, "CREERPSH").Equals("T"))
                    {                        
                        if (GetCompanyOption(shipTask.Company, "UPDSHBATCH").Equals("T"))
                            batchNumber = string.IsNullOrEmpty(shipTask.Location.BatchNo) ? "" : shipTask.Location.BatchNo;

                        ErpFactory.Documents().FulFillMergedSalesDocument(ssDocument, shipTask.DocumentLines, fistTimeFulfill, batchNumber);

                    }

                    try { ssDocument.QuoteNumber = GetHazMat(ssDocument.DocumentLines); }
                    catch { }
                }


                Console.WriteLine("4. After GP");

                //Se dejan los documentos en InProcess cuando se posteen pasan a Completed
                Status inProcess = WType.GetStatus(new Status { StatusID = DocStatus.InProcess });
                Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });


                //Ajustando los nodetrace y los labels procesados
                ProcessShippingTransactionTrace(traceList, ssDocument, shipTask, new Node { NodeID = NodeType.Released });


                //Put the posting Document to the Document Packages
                IList<DocumentPackage> packs = Factory.DaoDocumentPackage().Select(
                    new DocumentPackage
                    {
                        Document = new Document { DocID = shipTask.DocID }, 
                      PostingDocument = new Document { DocID = -1} }); //-1 equals NULL


                foreach (DocumentPackage pk in packs)
                {
                    //Si el pack no tiene labels contenidos se borra el pack y su label.
                    /*
                    if (Factory.DaoLabel().Select(new Label { FatherLabel = pk.PackLabel }).Count == 0)
                    {
                        try
                        {
                            Factory.DaoLabel().Update(pk.PackLabel);
                        }
                        catch { }

                        pk.PackLabel.Status = inactive;
                        Factory.DaoLabel().Update(pk.PackLabel);
                        continue;
                    }
                    */

                    pk.PostingDate = DateTime.Now;
                    pk.PostingDocument = ssDocument;
                    pk.PostingUserName = ssDocument.CreatedBy;
                    Factory.DaoDocumentPackage().Update(pk);
                }


                // Evalua sobre el documento si hay que hacer procesos adicionales ejemplo en el Documento WareHouse Transfer se debe crear el docuemto de recibo 
                // Para la Bodega que recibe la mercancia.
                if (shipTask.DocType.DocTypeID == SDocType.WarehouseTransferShipment)
                {
                    Location destLocation = Factory.DaoDocumentLine().Select(new DocumentLine { Document = shipTask }).First().Location2;
                    ssDocument.Comment += CreateWareHouseTransferReceiptOrder(ssDocument, destLocation);
                    //Factory.DaoDocument().Update(ssDocument);
                }


                //Mira si debe inprimir Generic Labels for Kit Assembly - Maxiforce.
                //Cuando se implemente Procesos de documento este Bloque pasara al proceso de 
                //Shipment      

                //Comentado para Maxiforce el 13 de Mayo de 2010
                //try
                //{
                //    if (GetCompanyOption(shipTask.Company, "PRTKTLBL").Equals("T"))
                //        PrintKitAssemblyLabels(ssDocument, 2);
                //}
                //catch { }



                #endregion


                //Creacion del Assembly Order en caso de que haya conexion al ERP y la opcion
                // ERPASMORDER este en True paar esa compania
                // 14 Mayo de 2007
                // COMENTARIADO EL 3 Junio de 2009 - Se quita el envio a GP
                /* if (GetCompanyOption(ssDocument.Company, "SHOWCOMP").Equals("T") && GetCompanyOption(ssDocument.Company, "ERPASMORDER").Equals("T"))
                    CreateKitAssemblyOrderBasedOnSalesDocument(shipTask, ssDocument); */


                ////15 jun 2010
                ////Aumentar el valor de COUNTRAND
                //DocMngr.IncreaseOption("COUNTRAND", shipTask.Company);

                //try
                //{ //Aumentar el VAlor de COUNTAMOUNT
                //    if (ssDocument.DocumentLines.Sum(f=>f.Quantity*f.Unit.BaseAmount*f.Product.ProductCost)
                //        > double.Parse(GetCompanyOption(shipTask.Company, "AMOUNTOVER")))
                //        //> double.Parse(Factory.DaoConfigOption().Select(new ConfigOption { Code = "AMOUNTOVER" }).First().DefValue))
                    
                //        DocMngr.IncreaseOption("COUNTAMOUNT", shipTask.Company);
                //}
                //catch { }

                Factory.Commit();

                ssDocument.DocStatus = completed;
                Factory.DaoDocument().Update(ssDocument);

                try
                {
                    //Update the Shipping Doc as In Process
                    if (shipTask.Arrived == true)
                        shipTask.DocStatus = completed; 
                    else
                        shipTask.DocStatus = inProcess; 

                    Factory.DaoDocument().Update(shipTask);
                }
                catch { }


                //Commit
                Factory.Commit();

                return ssDocument;


            }
            catch (Exception ex)
            {

                Factory.Rollback();

                //Ajusta la secuencia para reusar el numero
                //docSeq.NumSequence--;
                //Factory.DaoDocumentTypeSequence().Update(docSeq);
                //Factory.DaoDocument().Delete(ssDocument);
                //ReverseProcessTransactionTrace(traceList);

                ExceptionMngr.WriteEvent("CreateShipmentDocument:Doc#" + shipTask.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpPosting);
                throw;
            }

        }


        public void FullfilSalesOrder(Document ssDocument)
        {

            //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
            bool ErpConnected = GetCompanyOption(ssDocument.Company, "WITHERPSH").Equals("T");
            Connection curConnection;

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (ssDocument.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(ssDocument.Company);
            }


            //Enviar el documento de shipping al ERP
            curConnection = ssDocument.Company.ErpConnection;

            //Evaular si debe Update BATCH
            string batchNumber = "";
            if (GetCompanyOption(ssDocument.Company, "UPDSHBATCH").Equals("T"))
                batchNumber = string.IsNullOrEmpty(ssDocument.Location.BatchNo) ? "" : ssDocument.Location.BatchNo;

            string shortAge = GetCompanyOption(ssDocument.Company, "SHORTAGE");

            ErpFactory.Documents().FulFillSalesDocument(ssDocument, shortAge, true, batchNumber);


        }



        private string GetHazMat(IList<DocumentLine> iList)
        {
            //Lee las lineas del documento y dice si tiene un HAZMAT // IMAGE Service 9/FEB/10
            if (iList == null || iList.Count == 0)
                return "N";
            else
                foreach (Product p in iList.Select(f => f.Product))
                {
                    if (p.ProductTrack.Any(f=>f.TrackOption.Name == "HAZMAT"))
                        return "Y";
                }

            return "N";
        }

/*
        private void DeleteNodeTrace(Label packLabel)
        {
            IList<NodeTrace> traceList = Factory.DaoNodeTrace().Select(new NodeTrace { Label = packLabel });

            foreach (NodeTrace nTrace in traceList)
                Factory.DaoNodeTrace().Delete(nTrace);
        }
        */



        private string CreateWareHouseTransferReceiptOrder(Document ssDocument, Location destLocation)
        {

            DocumentTypeSequence docSeq = null;

            //1. Create the Document Head
            DocumentType docType = new DocumentType { DocTypeID = SDocType.WarehouseTransferReceipt };
            docSeq = GetNextDocSequence(ssDocument.Company, docType);

            Document whTranferDoc = new Document
            {
                DocNumber = docSeq.CodeSequence,
                DocType = docType,
                Location = destLocation,
                IsFromErp = false,
                CrossDocking = false,
                Customer = ssDocument.Customer,
                Reference = ssDocument.CustPONumber,
                Date1 = DateTime.Now,
                CustPONumber = ssDocument.DocNumber,
                CreatedBy = ssDocument.CreatedBy,
                Company = ssDocument.Company,
                Comment = "Warehose Receipt for Doc# " + ssDocument.CustPONumber + ", Ship# " + ssDocument.DocNumber

            };
            whTranferDoc = DocMngr.CreateNewDocument(whTranferDoc, false);


            //2. Create Document Lines (Idem to Shipment but changing Warehouse)
            IList<DocumentLine> docLines = new List<DocumentLine>();
            DocumentLine transferLine;
            foreach (DocumentLine dl in ssDocument.DocumentLines)
            {
                transferLine = new DocumentLine
                {
                    Document = whTranferDoc,
                    LineNumber = dl.LineNumber,
                    LineStatus = dl.LineStatus,
                    Product = dl.Product,
                    IsDebit = dl.IsDebit,
                    Quantity = dl.Quantity,
                    Unit = dl.Unit,
                    UnitBaseFactor = dl.UnitBaseFactor,
                    Date1 = dl.Date1,
                    Date2 = DateTime.Now,
                    CreatedBy = ssDocument.CreatedBy,
                    CreationDate = DateTime.Now,
                    Location = destLocation, //Location DEST
                    Location2 = ssDocument.Location, //Location FROM,
                    UnitCost = dl.UnitCost,
                    ExtendedCost = dl.ExtendedCost
                };
                docLines.Add(transferLine);
            }


            whTranferDoc.DocumentLines = docLines;

            //Salvando el Docuemnto
            Factory.DaoDocument().Save(whTranferDoc);
            return " Receipt Transfer Doc# " + whTranferDoc.DocNumber;

        }
    





        private IList<DocumentLine> GetLinesWithKitAssemblyHeaders(IList<DocumentLine> shpLines)
        {
            DocumentLine ssLine = null;
            DocumentLine curLine = null;
            int kitLine;
            IList<DocumentLine> processeKits = new List<DocumentLine>();

            //Recorre los componentes para encontrar su Kit/Assembly Padre.
            foreach (DocumentLine dl in shpLines.Where(f => f.Note == "1").OrderBy(f=>f.LinkDocLineNumber))
            {
                try
                {
                    //Entrega la linea del KIT/ASSEMBLY en el Sales Order
                    kitLine = Factory.DaoDocumentLine().Select(
                        new DocumentLine
                        {
                            LineNumber = dl.LinkDocLineNumber,
                            Document = new Document { DocNumber = dl.LinkDocNumber, Company = dl.Document.Company }}
                        ).First().LinkDocLineNumber;

                    curLine = Factory.DaoDocumentLine().Select(
                        new DocumentLine {
                            LineNumber = kitLine,
                            Document = new Document { DocNumber = dl.LinkDocNumber, Company = dl.Document.Company }}
                         ).First();

                    //revisa si ese kit aun no ha sido procesado. Si fue procesado va al siguiente
                    if (processeKits.Where(f => f.LineNumber == curLine.LineNumber).Count() > 0)
                        continue;

                    processeKits.Add(curLine);
                }
                catch { continue; }


                int line = shpLines.Select(f=>f.LineNumber).Max() + 1;
                Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

                //Crea una linea para el documento de shipment
                ssLine = new DocumentLine
                {
                    Product = curLine.Product,
                    Quantity = 0, 
                    QtyAllocated = curLine.Quantity, //Guarda la cantidad a enviar al ERP para el Fulfill.
                    Unit = curLine.Unit,
                    Document = dl.Document,
                    CreationDate = DateTime.Now,
                    IsDebit = false,
                    LineNumber = line,
                    LineStatus = lineStatus,
                    Location = dl.Document.Location,
                    UnitBaseFactor = dl.Unit.BaseAmount,
                    LinkDocNumber = curLine.Document.DocNumber,
                    LinkDocLineNumber = curLine.LineNumber,
                    CreatedBy = dl.CreatedBy,
                    Note = "2",
                    Sequence = curLine.Sequence
                };

                shpLines.Add(ssLine);
                line++;
            }

            return shpLines;
        }



        public void CreateKitAssemblyOrderBasedOnSalesDocument(Document shipTask, Document shipment)
        {

            IEnumerable<DocumentLine> lines = Factory.DaoDocumentLine().Select(new DocumentLine{ Document = shipTask });

            //Revisa si el documento tiene productos kit/assembly, si no se sale.
            lines = lines.Where(f => f.Note == "2" && f.LinkDocLineNumber <= 0);

            if (lines.Count() <= 0)
                return;


            try
            {
                //Reseteando la conexion al ERP
                SetConnectMngr(shipTask.Company);

                //Otiene la lista de los producto y la cantidad ordenada
                IEnumerable<ProductStock> productList =
                    (from kit in lines select new ProductStock { Product = kit.Product, Stock = kit.Quantity - kit.QtyCancel });


                //Obtiene La formulacion de cada assembly en el documento y lo envia al ERP
                double quantity = 0;
                int sequence = 0;
                foreach (Product product in productList.Select(f => f.Product).Distinct())
                {
                    //Obteniendo la formula del Assembly
                    try
                    {
                        sequence++;
                        //Obteniedo la cantidad a crear
                        quantity = productList.Where(f => f.Product.ProductID == product.ProductID).Sum(f => f.Stock);

                        //Enviando el Kit/Asm al ERP
                        ErpFactory.Documents().CreateKitAssemblyOrderBasedOnSalesDocument(
                            shipment, product, quantity, sequence.ToString().PadLeft(3, '0'));

                    }

                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("SOAsmOrderLine:Doc#" + shipTask.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                        continue;
                    }


                }

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("SOAsmOrder:Doc#" + shipTask.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
            }

            

        }


        //Cuando las lineas no requieren de hacer link con un Documento en el ERP
        private Object[] SalesLinesForTask(Document shipTask, Document ssDocument)
        {
            Object[] result = new Object[2];

            try
            {
                //Node releaseNode = new Node { NodeID = NodeType.Released };
                Node pickNode = new Node { NodeID = NodeType.Picked };


                //Armar las lineas del documento de Recibo
                IList<DocumentLine> ssDocLines = new List<DocumentLine>();

                //Obtiene los nodetrace records a recorrer, 
                //los que no han sido posteados para esa tarea de recibo
                NodeTrace qNodeTrace = new NodeTrace { Document = shipTask, PostingDocument = new Document { DocID = 0 }, Node = pickNode };
                IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(qNodeTrace);
                Status status = WType.GetStatus(new Status { StatusID = DocStatus.New });

                DocumentLine ssLine;
                Double line = 0;

                //Leva el conteo del acumulado a recibir
                IDictionary<UnitProductRelation, Double[]> shipmentBalance = new Dictionary<UnitProductRelation, Double[]>();
                UnitProductRelation curUnitProduct;

                //Consolidar las lineas de la misma unidad y hacer una sola linea
                foreach (NodeTrace nodeTrace in nodeTraceList)
                {
                    if (nodeTrace.Label.Product == null)
                        continue;

                    //Armando el consolidado
                    curUnitProduct = Factory.DaoUnitProductRelation().Select(
                        new UnitProductRelation
                        {
                            Unit = nodeTrace.Label.Unit,
                            Product = nodeTrace.Label.Product
                        }).First();

                    if (shipmentBalance.ContainsKey(curUnitProduct))
                        shipmentBalance[curUnitProduct][1] += nodeTrace.Quantity;
                    else
                    {
                        line += 1;
                        shipmentBalance.Add(curUnitProduct, new Double[2] { line, nodeTrace.Quantity });
                    }


                    //Update Node Trace
                    nodeTrace.Document = shipTask;
                    nodeTrace.PostingDate = DateTime.Now;
                    nodeTrace.PostingDocument = ssDocument;
                    nodeTrace.PostingDocLineNumber = (Int32)line;
                    nodeTrace.PostingUserName = ssDocument.CreatedBy;
                    nodeTrace.ModifiedBy = ssDocument.CreatedBy;
                    nodeTrace.ModDate = DateTime.Now;

                    Factory.DaoNodeTrace().Update(nodeTrace);
                }



                //Recorre la coleccion de balance para adicionar las demas lineas del recibo
                foreach (UnitProductRelation unitProductRel in shipmentBalance.Keys)
                {
                    //Si hay cantidad (elemento 1)
                    if (shipmentBalance[unitProductRel][1] > 0)
                    {

                        //Crea una linea para el documento de recibo obteniedo la info del balance
                        ssLine = new DocumentLine
                        {
                            Product = unitProductRel.Product,
                            Quantity = shipmentBalance[unitProductRel][1],
                            Unit = unitProductRel.Unit,
                            Document = ssDocument,
                            CreationDate = DateTime.Now,
                            IsDebit = false,
                            LineNumber = (Int32)shipmentBalance[unitProductRel][0],
                            LineStatus = status,
                            Location = ssDocument.Location,
                            UnitBaseFactor = unitProductRel.Unit.BaseAmount,
                            CreatedBy = ssDocument.CreatedBy,
                            Date1 = DateTime.Now
                        };

                        ssDocLines.Add(ssLine);
                    }
                }

                result[0] = ssDocLines;
                result[1] = nodeTraceList;

                return result;

            }
            catch (Exception ex)
            {
                //Factory.Rollback();
                ExceptionMngr.WriteEvent("SalesLinesForTask", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                //throw;
                return null;
            }
        }


        //Cuando las lineas deben hacer link con un documento del ERP, solo se permite que el Uom Coincida
        //Con el del documento
        private Object[] SalesLinesForErpDocument(Document shipTask, Document ssDocument)
        {
            Object[] result = new Object[2];

            try
            {

                Node releaseNode = new Node { NodeID = NodeType.Released };
                Node pickNode = new Node { NodeID = NodeType.Picked };
                DocumentBalance docBal = new DocumentBalance { Document = shipTask, Node = releaseNode };

                //Obtiene el balance linea por linea, Es decir el cruce contra lo ya Released, porque la linea se
                //asigna una vez se postea, lo que no tiene linea no aparece en el balance
                //es decir el saldo que queda por llenar de cada linea de documento
                IList<DocumentBalance> balanceList = Factory.DaoDocumentBalance().DetailedBalance(docBal, shipTask.CrossDocking == true ? true : false);
                balanceList = balanceList.OrderBy(f => f.DocumentLine.LineNumber).ToList();  //Ordernada por LineNumber

                Console.WriteLine("Balance List");

                //Armar las lineas del documento de Shipment
                IList<DocumentLine> ssDocLines = new List<DocumentLine>();
                //NodeTrace qNodeTrace;
                DocumentLine ssLine;
                int line = 1;
                IEnumerable<NodeTrace> nodeTraceSameUnit;
                IList<NodeTrace> allNodeTrace = new List<NodeTrace>();
                Dictionary<long, double> acumLines = new Dictionary<long, double>();


                Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

                //Obtienen los saldos pendientes de lo que se piqueo para ese documento.
                nodeTraceSameUnit = nodeTraceSameUnit = Factory.DaoNodeTrace().Select(
                    new NodeTrace
                    {
                        Document = shipTask,
                        Node = pickNode,
                        PostingDocument = new Document { DocID = 0 }, //0 mean Null
                        //Label = new Label { Product = balance.Product, Unit = balance.Unit }
                    }).Where(f => f.Quantity > 0);


                //Recorre los NodeTrace en Picking que tengan saldo y tengan balance.
                double traceBalance;
                double shipLineQty;
                int xLine = 0;


                Console.WriteLine("Before Foreach");

                foreach (NodeTrace curTrace in nodeTraceSameUnit.Where(f => f.Label.Product != null).OrderBy(f => f.Label.Product.ProductID))
                {
                    traceBalance = curTrace.Quantity * curTrace.Unit.BaseAmount; 

                    //Aqui en shipping el despacho es en EA asi que se deben procescar todas las lineas del nodetrace
                    //En EA
                    foreach (DocumentBalance curline in balanceList
                        .Where(f => f.Product.ProductID == curTrace.Label.Product.ProductID
                            //&& (f.Unit.UnitID == curTrace.Label.Unit.UnitID || f.Unit.BaseAmount == curTrace.Label.Unit.BaseAmount)
                            && f.QtyPending > 0))
                    {

                        //Para el consecutivo de la linea
                        if (!acumLines.ContainsKey(curline.DocumentLine.LineID))
                            xLine++;


                        //Si el valor de la cantidad del label actual es mayor o igual a la pendiente.
                        if (traceBalance >= curline.BaseQtyPending)
                        {
                            traceBalance -= curline.BaseQtyPending;
                            shipLineQty = curline.BaseQtyPending;
                            curline.QtyPending = 0;
                        }
                        else //Si es menor
                        {
                            curline.QtyPending -= traceBalance/curline.Unit.BaseAmount; //nodeTraceSameUnit.Sum(f => f.Quantity);
                            shipLineQty = traceBalance; // nodeTraceSameUnit.Sum(f => f.Quantity);
                            traceBalance = 0;
                        }


                        //Creado los nuevos traces en el RELEASE.
                        curTrace.DocumentLine = curline.DocumentLine;
                        curTrace.PostingDate = DateTime.Now;
                        curTrace.PostingDocument = ssDocument;
                        curTrace.PostingDocLineNumber = xLine;
                        curTrace.PostingUserName = ssDocument.CreatedBy;
                        curTrace.ModifiedBy = ssDocument.CreatedBy;
                        curTrace.ModDate = DateTime.Now;

                        //Update Anterior.
                        Factory.DaoNodeTrace().Update(curTrace);

                        //Nuevo node para el caso de Shipping.
                        if (shipLineQty > 0)
                            allNodeTrace.Add( new NodeTrace
                            {
                                Node = curTrace.Node,
                                Document = curTrace.Document,
                                Label = curTrace.Label,
                                Quantity = shipLineQty,
                                IsDebit = curTrace.IsDebit,
                                CreatedBy = curTrace.CreatedBy,
                                PostingDocument = curTrace.PostingDocument,
                                PostingDocLineNumber = xLine,
                                PostingUserName = ssDocument.CreatedBy,
                                Bin = curTrace.Bin,
                                CreationDate = DateTime.Now,
                                PostingDate = DateTime.Now,
                                Status = curTrace.Status,
                                Unit = curline.Product.BaseUnit,
                                FatherLabel = curTrace.Label.FatherLabel
                            });


                        //Crea el acumulador para cada linea.
                        if (acumLines.ContainsKey(curline.DocumentLine.LineID))
                            acumLines[curline.DocumentLine.LineID] += shipLineQty;
                        else
                            acumLines.Add(curline.DocumentLine.LineID, shipLineQty);

                    }

                    if (traceBalance <= 0)
                        continue;
                    else //Reportar que hubo overshipment.
                    {
                        ExceptionMngr.WriteEvent("SalesLinesForErpDocument: Overshipment: " + curTrace.Document.DocNumber
                            + ", Product: " + curTrace.Label.Product.Name,
                            ListValues.EventType.Warn, null, null,
                            ListValues.ErrorCategory.Business);

                    }

                }


                //Para todos los saldos de las lineas.
                //Crea las lineas respectivas.
                DocumentLine oriLine;
                foreach (long lineID in acumLines.Keys)
                {

                    oriLine = balanceList.Where(f => f.DocumentLine.LineID == lineID).Select(f => f.DocumentLine).First();

                    //Crea una linea para el documento de fullfilment
                    ssLine = new DocumentLine
                    {
                        Product = oriLine.Product,
                        Quantity = acumLines[lineID] / oriLine.Unit.BaseAmount, //acumLines[lineID],  //nodeTraceSameUnit.Sum(f => f.Quantity),
                        Unit = oriLine.Unit,
                        Document = ssDocument,
                        CreationDate = DateTime.Now,
                        IsDebit = false,
                        LineNumber = line,
                        LineStatus = lineStatus,
                        Location = ssDocument.Location,
                        UnitBaseFactor = oriLine.Unit.BaseAmount,
                        LinkDocNumber = oriLine.Document.DocNumber,
                        LinkDocLineNumber = oriLine.LineNumber,
                        CreatedBy = ssDocument.CreatedBy,
                        Note = oriLine.Note, //If Note=2 indica que es un componente
                        Sequence = oriLine.Sequence,
                        Date1 = DateTime.Now,
                        UnitPrice = oriLine.UnitPrice,
                        ExtendedPrice = oriLine.UnitPrice * acumLines[lineID] * oriLine.Unit.BaseAmount,
                        UnitCost = oriLine.UnitCost,
                        ExtendedCost = oriLine.UnitCost * acumLines[lineID] * oriLine.Unit.BaseAmount
                    };

                    ssDocLines.Add(ssLine);
                    line++;
                }


                result[0] = ssDocLines;
                result[1] = allNodeTrace;

                return result;

            }
            catch (Exception ex)
            {
                //Factory.Rollback();
                ExceptionMngr.WriteEvent("SalesLinesForErpDocument", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                //throw;
                return null;
            }




            //Recorre los saldos pendientes para ese documento
            /*
            double curQty = 0;
            foreach (DocumentBalance balance in balanceList.Where(b => b.QtyPending > 0))
            {

                qNodeTrace = new NodeTrace
                {
                    Document = shipTask,
                    Node = pickNode,
                    PostingDocument = new Document { DocID = 0 }, //0 mean Null
                    Label = new Label { Product = balance.Product, Unit = balance.Unit }
                };

                //PASO 1 : Primero buscar los nodetrace que coincidan con la unidad necesitada y traer los pendientes
                nodeTraceSameUnit = Factory.DaoNodeTrace().Select(qNodeTrace).ToList(); //.Take(int.Parse(balance.QtyPending.ToString()))

                //Si se obtienen registros
                //if (nodeTraceSameUnit.Count > 0)
                if (nodeTraceSameUnit.Sum(f => f.Quantity) > 0)
                {

                    //1.1 Actualiza los que encontro
                    foreach (NodeTrace traceSame in nodeTraceSameUnit)
                    {
                        traceSame.DocumentLine = balance.DocumentLine;
                        traceSame.PostingDate = DateTime.Now;
                        traceSame.PostingDocument = ssDocument;
                        traceSame.PostingDocLineNumber = line;
                        traceSame.PostingUserName = ssDocument.CreatedBy;
                        traceSame.ModifiedBy = ssDocument.CreatedBy;
                        traceSame.ModDate = DateTime.Now;

                        allNodeTrace.Add(traceSame);

                    }


                    //1.2 Disminuye el balance
                    //Ajuste para que trabaje bien cuando hay items repetidos en una misma orden
                    if (nodeTraceSameUnit.Sum(f => f.Quantity) <= balance.QtyPending)
                    {
                        balance.QtyPending -= nodeTraceSameUnit.Sum(f => f.Quantity);
                        curQty = nodeTraceSameUnit.Sum(f => f.Quantity);
                    }
                    else
                    {
                        curQty = balance.QtyPending;
                        balance.QtyPending = 0;
                    }


                    //Crea una linea para el documento de fullfilment
                    ssLine = new DocumentLine
                    {
                        Product = balance.Product,
                        Quantity = curQty,  //nodeTraceSameUnit.Sum(f => f.Quantity),
                        Unit = balance.Unit,
                        Document = ssDocument,
                        CreationDate = DateTime.Now,
                        IsDebit = false,
                        LineNumber = line,
                        LineStatus = lineStatus,
                        Location = ssDocument.Location,
                        UnitBaseFactor = balance.Unit.BaseAmount,
                        LinkDocNumber = balance.Document.DocNumber,
                        LinkDocLineNumber = balance.DocumentLine.LineNumber,
                        CreatedBy = ssDocument.CreatedBy,
                        Note = balance.DocumentLine.Note, //If Note=2 indica que es un componente
                        Sequence = balance.DocumentLine.Sequence,
                        Date1 = DateTime.Now,
                        UnitPrice = balance.UnitPrice,
                        ExtendedPrice = balance.UnitPrice * curQty * balance.Unit.BaseAmount
                    };

                    ssDocLines.Add(ssLine);
                    line++;

                }

                //1.4 Si encontro todos los que necesitaba se sale a la siguiente linea
                if (balance.QtyPending <= 0)
                    continue;

            }

            result[0] = ssDocLines;
            result[1] = allNodeTrace;

            return result;
        }
        catch (Exception ex)
        {
            //Factory.Rollback();
            ExceptionMngr.WriteEvent("SalesLinesForErpDocument", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
            //throw;
            return null;
        }
             * */
        }



        private void FulfillCrossDockSalesDocuments()
        {
            //Este proceso aplica solo cuando hay conexion con el ERP hace lo siguiente:
            //1. Recorre los documentos de cross dock en status New.
            //2. Revisa que el Purchase Receipt Created no haya sido cancelado
            //3. Hace el fullfill de las cantidades en el ERP.

            Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
            DocumentType docType = new DocumentType { DocTypeID = SDocType.CrossDock };

            Document pattern = new Document { DocType = docType, 
                DocStatus = new Status { StatusID = DocStatus.New } };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);

            //1. Recorre los documentos de cross dock en status New.
            foreach (Document curDocument in unPostedList)
            {
                //Check if receipt was posted.
                if (!curDocument.TaskDocuments.Select(f => f.IncludedDoc).Where(f => f.DocType.DocTypeID == SDocType.PurchaseReceipt)
                    .Any(f => f.DocStatus.StatusID == DocStatus.Posted))
                    continue;

                //COmentado en Oct 09 /2009
                //Si el documento PR asociado no ha sido completado (posteado) va la siguiente
                //if (curDocument.TaskDocuments.Select(f => f.IncludedDoc)
                //    .Where(f => f.DocType.DocTypeID == SDocType.PurchaseReceipt).First()
                //    .DocStatus.StatusID != DocStatus.Posted)
                //    continue;

                try
                {
                    //3. Hace el fullfill de las cantidades en el ERP para cada documento relacionado
                    foreach (Document shDocument in curDocument.TaskDocuments.Select(f => f.IncludedDoc)
                    .Distinct().Where(f => f.DocType.DocClass.DocClassID == SDocClass.Shipping))
                    {
                        try { 
                            CreateShipmentDocument(shDocument); 
                        }

                        catch (Exception ex)
                        {
                            ExceptionMngr.WriteEvent("CrossDock:CreateShipmentDocument: " + curDocument.DocNumber, ListValues.EventType.Fatal,
                                ex, null, ListValues.ErrorCategory.Business);
                        }
                    }

                    curDocument.DocStatus = completed;
                    Factory.DaoDocument().Update(curDocument);

                }
                catch (Exception ex) {

                    ExceptionMngr.WriteEvent("FulfillCrossDockSalesDocuments: " + curDocument.DocNumber, ListValues.EventType.Fatal, 
                        ex, null, ListValues.ErrorCategory.Business);
                }
            }

        }


        private void UpdatePostedInvoices()
        {
            Status completed = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
            DocumentType docType = new DocumentType { DocTypeID = SDocType.SalesShipment };

            Document document; //for the foreach
            Document pattern = new Document { DocType = docType, DocStatus = new Status { StatusID = DocStatus.InProcess } };

            //Obtiene la lista de los pendientes
            IList<Document> unPostedList = Factory.DaoDocument().Select(pattern);

            foreach (Document curDocument in unPostedList)
            {
                try
                {
                    //Obtiene si esta posteado o no la factura relacionada a ese shipment
                    document = null; // ErpFactory.Documents().GetReceiptPostedStatus(curDocument); //Cambiar a Invoice

                    if (document != null)
                    {
                        document.DocStatus = completed;
                        Factory.DaoDocument().Update(document);
                    }
                }
                catch { }
            }
        }




        //Permite reversar un documento que fue creado y enviado al ERP
        //solo se reversa si en el ERP no lo ha posteado
        public void ReverseShipmentDocument(Document data, Bin binRestore)
        {
            Factory.IsTransactional = true;

            //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
            bool ErpConnected = GetCompanyOption(data.Company, "WITHERPSH").Equals("T");

            Node releaseNode = new Node { NodeID = NodeType.Released };
            Node storeNode = new Node { NodeID = NodeType.Stored };

            try
            {


                //Update document status to Cancelled
                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });
                Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });


                //Cancelacion del Assembly Order en caso de que haya conexion al ERP y la opcion
                // ERPASMORDER este en True para esa compania
                // 22 Mayo de 2007, Mande el resultado a Notes para poder enviarlo en el mail
                // COMENTARIADO EL 3 Junio de 2009 - Se quita el envio a GP
                /*
                if (GetCompanyOption(data.Company, "SHOWCOMP").Equals("T") && GetCompanyOption(data.Company, "ERPASMORDER").Equals("T"))
                {
                    SetConnectMngr(data.Company);
                    data.Comment += "\n\n" + ErpFactory.Documents().CancelKitAssemblyOrderBasedOnSalesDocument(data);
                }
                */

                data.DocStatus = cancelled;
                Factory.DaoDocument().Update(data);

                //Pasa las lineas del documento a Cancelled                
                IList<DocumentLine> docLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = data });

                foreach (DocumentLine dl in docLines)
                {
                    dl.LineStatus = cancelled;
                    Factory.DaoDocumentLine().Update(dl);
                }

                //update NodeTrace
                NodeTrace qNodeTrace = new NodeTrace { PostingDocument = data };

                //Busca todo los registros de ese documento y los reversa
                IList<NodeTrace> nodeTraceList = Factory.DaoNodeTrace().Select(qNodeTrace);

                Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

                Label curLabel;

                foreach (NodeTrace trace in nodeTraceList)
                {

                    //Crear un trace que tenga la transaccion del posting eliminado en el nodo void
                    //Registra el movimiento del nodo
                    if (trace.Node.NodeID == NodeType.Released)
                    {

                        trace.Node = voidNode;
                        trace.Status = inactive;
                        trace.ModDate = DateTime.Now;
                        trace.ModifiedBy = data.ModifiedBy;
                        trace.Comment = "Released: " + trace.PostingDocument.DocNumber + " Reversed";
                        Factory.DaoNodeTrace().Update(trace);
                        
                        SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = storeNode,
                                Document = trace.Document,
                                Label = trace.Label,
                                Quantity = trace.Quantity,
                                IsDebit = trace.IsDebit,
                                CreatedBy = trace.CreatedBy,
                                //PostingDocument = trace.PostingDocument,
                                //PostingUserName = trace.PostingUserName,
                                Status = active,
                                Comment = "Stock: " + trace.PostingDocument.DocNumber + " Reversed",
                                CreationDate= DateTime.Now,
                                //ModifiedBy = data.ModifiedBy,
                                //PostingDate = trace.PostingDate,
                            });
                        
                    }


                    //Reversa el trace original para poderlo postear nuevamente o reversarlo a stored
                    if (trace.Node.NodeID == NodeType.Picked)
                    {
                        //trace.DocumentLine = null;
                        //trace.PostingDate = null;
                        //trace.PostingDocument = null;
                        //trace.PostingUserName = null;
                        trace.ModifiedBy = data.ModifiedBy;
                        trace.ModDate = DateTime.Now;
                        trace.Node = voidNode;
                        trace.Comment = "Picked: " + trace.PostingDocument.DocNumber + " Reversed";
                        Factory.DaoNodeTrace().Update(trace);
                    }

                    //Recorre los Packages de ese shipment y reversa los labels HIjos
                    //Poner en Void los package Labels de ese documento.
                    IList<DocumentPackage> packList = Factory.DaoDocumentPackage().Select(new DocumentPackage { PostingDocument = data });
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

                

                #region Restoring QtyShipped - JM Ene21/2010
                //reversando el ShippedQty to the Original Order Qher is a MergedOrder

                try
                {
                    Document mergedDoc = Factory.DaoDocument().Select(new Document { DocNumber = data.CustPONumber, Company = data.Company }).First();

                    mergedDoc.PostingDocument = "";
                    mergedDoc.Priority = 0;
                    Factory.DaoDocument().Update(mergedDoc);

                    if (mergedDoc.DocType.DocTypeID == SDocType.MergedSalesOrder)
                    {

                        DocumentLine mergedLine;
                        DocumentLine oriLine;

                        //1. Toma las lineas del Shipped
                        foreach (DocumentLine shipLine in docLines)
                        {
                            try
                            {
                                //2. Busca las lineas del merged y luego la original
                                mergedLine = Factory.DaoDocumentLine().Select(new DocumentLine
                                {
                                    Document = new Document { DocNumber = shipLine.LinkDocNumber, Company = data.Company },
                                    LineNumber = shipLine.LinkDocLineNumber
                                }).First();


                                //2. Busca las lineas del merged original
                                oriLine = Factory.DaoDocumentLine().Select(new DocumentLine
                                {
                                    Document = new Document { DocNumber = mergedLine.LinkDocNumber, Company = data.Company },
                                    LineNumber = mergedLine.LinkDocLineNumber
                                }).First();

                                //3. Actualiza el QtyShipped.
                                oriLine.QtyShipped -= shipLine.Quantity;
                                oriLine.ModDate = DateTime.Now;
                                oriLine.QtyBackOrder = 0;
                                oriLine.QtyAllocated = 0;
                                oriLine.QtyCancel = 0;
                                //
                                Factory.DaoDocumentLine().Update(oriLine);
                            }
                            catch (Exception ex)
                            {
                                ExceptionMngr.WriteEvent("Updating ShipQty:" + data.DocNumber + ", Line:" + shipLine.LineNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                                continue;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("Updating ShipQty:" + data.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                }


                #endregion



                Factory.Commit();



            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReverseSalesShipment #" + data.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Persistence);
                throw;
            }
        }







        #endregion




    }
}