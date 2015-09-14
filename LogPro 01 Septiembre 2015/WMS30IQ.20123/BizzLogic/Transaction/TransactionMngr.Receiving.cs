using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Entities.General;
using Entities.Master;
using Entities.Trace;
using Entities.Profile;
using Integrator;
using System.IO;
using System.Reflection;

namespace BizzLogic.Logic
{

    public partial class TransactionMngr
    {

        #region Receiving Transactions



        /// <summary>
        /// Recibe producto, sin etiqueta (recibo manual) crea labels virtuales para cada unidad basica de producto
        /// </summary>
        /// <param name="document">Task Document in Process</param>
        /// <param name="product"></param>
        /// <param name="logisticUnit">Unidad Logistica Que contiene las packUnits</param>
        /// <param name="packUnit">Unidad minima no necesariamente la unidad basica</param>
        /// <param name="quantity">Cantidad de unidades minimas</param>
        public void ReceiveProduct(DocumentLine receivingLine, Unit logisticUnit, Bin destLocation, Node recNode)
        {        

            Factory.IsTransactional = true;

            try
            {

                //Se debe ejecutar proceso para saber si la company permite 
                //recibir producto no existente en el documento, Si es receiving Task se permiten 
                //hacer un bypass a ambas reglas

                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(receivingLine.Document, true);

                //status del Bin donde se Recibira.
                Rules.ValidateBinStatus(destLocation, true);

                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(receivingLine.Product, destLocation, true);


                if (receivingLine.Document.DocType.DocTypeID != SDocType.ReceivingTask)
                {
                    //Valida si el producto esta en ese documento
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = receivingLine.Document,
                        Product = receivingLine.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = receivingLine.Unit,
                        Quantity = receivingLine.Quantity
                    };

                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por recibir
                    Rules.ValidateBalanceQuantityInDocument(docLine, recNode, true, false);
                }


                //Obteniendo el factor logistico antes de enviar a crear los labels
                double logisticFactor = (logisticUnit != null) ? receivingLine.QtyPending : 1;
                //double logisticFactor = (logisticUnit != null) ? (double)(logisticUnit.BaseAmount / receivingLine.Unit.BaseAmount) : 0;

                //Manda a Crear los Labels
                CreateProductLabels(logisticUnit, receivingLine, recNode, destLocation, logisticFactor, receivingLine.Note,"", DateTime.Now);


                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiveProduct:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }

        /// <summary>
        /// Realiza el ReceiveLabels masivamente
        /// </summary>
        /// <param name="document"></param>
        /// <param name="labels"></param>
        /// <param name="destLocation"></param>
        /// <param name="recNode"></param>
        /// <param name="defaultBin"></param>
        /// <param name="location"></param>

        public void ReceiveLabels(Document document, IList<Label> labels, Bin destLocation, Node recNode, Boolean defaultBin, Location location)
        {
            Bin destiny;
            foreach (Label label in labels)
            {
                // bines del producto
                IList<ZoneBinRelation> result = GetAssignedBinsList(label.Product, location);

                // usa el Bin default  o el enviado por parametro
                if (defaultBin && result.Count>0)
                    destiny = result[0].Bin;
                else
                    destiny = destLocation;

                if (document.DocType != null && document.DocType.DocTypeID == SDocType.WarehouseTransferReceipt)
                    ReceiveLabelForTransfer(document, label, destiny, recNode);

                else
                    ReceiveLabel(document, label, destiny, recNode);
            }
             
        }

        /// <summary>
        /// Recibe producto, con etiqueta (recibo capturado por scanner generalmente)
        /// </summary>
        /// <param name="document">Task Document in Process</param>
        /// <param name="label">Label de transaccion </param>
        public Label ReceiveLabel(Document document, Label label, Bin destLocation, Node recNode)
        {

            try
            {
                if (label.LabelID == 0)
                {
                    try { 
                        IList<Label> labelList = Factory.DaoLabel().Select(label); 

                        //Revisa que solo un label sea entregado.
                        if (labelList != null && labelList.Count > 1 )
                            throw new Exception("Label " + label.LabelCode + " exists more than once.\nPlease check it.");

                        label = labelList.First();

                        }
                    catch { 

                        throw new Exception("Label " + label.LabelCode + " does not exists."); 
                    }
                }
                else
                    //Trae de nuevo le label para cargar los childs en caso de que sea logistica
                    label = Factory.DaoLabel().SelectById(new Label {LabelID = label.LabelID });


                Factory.IsTransactional = true;

                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(document, true);

                Rules.ValidateBinStatus(destLocation, true);

                //Valida si el status es Activo
                Rules.ValidateActiveStatus(label.Status, true);

                //Valida si el label es un label de producto, 
                //TODO: alarma cuand o suceda el evento de que no es un label de producto
                Rules.ValidateIsProductLabel(label, true);

                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(label.Product, destLocation, true);


                //Valida si el label esta en el nodo que debe estar (Ruta de Nodos)
                //TODO: alarma cuand o suceda el evento de que no es un label de producto
                Rules.ValidateNodeRoute(label, recNode, true);


                if (document.DocType.DocTypeID != SDocType.ReceivingTask)
                {
                    //Valida si el producto esta en ese documento
                    //Se debe ejecutar proceso para saber si la company permite 
                    //recibir producto no existente en el docuemnto
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = document,
                        Product = label.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = label.Unit,
                        Quantity = 0
                    };

                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por recibir
                    //Calcula el Current Qty y Valida si esa cantidad aun esta pendiente en el documento
                    //Double quantity = (label.IsLogistic == true) ? Factory.DaoLabel().CurrentQty(label) : label.CurrQty * label.UnitBaseFactor;
                    docLine.Quantity = Factory.DaoLabel().SelectCurrentQty(label, null, true);
                    
                    Rules.ValidateBalanceQuantityInDocument(docLine, recNode, true, false);

                }


                //Actualiza Label with new data
                label.Node = recNode;
                label.Bin = destLocation;
                label.ModifiedBy = document.ModifiedBy;
                label.ModDate = DateTime.Now;
                label.ReceivingDate = DateTime.Now;
                label.ReceivingDocument = document;
                label.Printed = true;

                //Si el label estaba contenido en una logistica, al recibilo solo quiere decir que lo extrae de la logistica
                label.FatherLabel = null; 


                Factory.DaoLabel().Update(label);

                //Registra el movimiento del label en el nodo
                SaveNodeTrace(new NodeTrace
                {
                    Node = recNode,
                    Document = document,
                    Label = label,
                    Quantity = label.CurrQty,
                    IsDebit = false,
                    CreatedBy = document.ModifiedBy,
                    Comment = "Receiving "+ document.DocNumber
                });


                //Actualiza Los Hijos (si existen)
                try
                {
                    label.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = label });
                    if (label.ChildLabels != null && label.ChildLabels.Count > 0)

                        foreach (Label curLabel in label.ChildLabels)
                        {
                            curLabel.Node = recNode;
                            curLabel.Bin = label.Bin;
                            curLabel.ModifiedBy = document.ModifiedBy;
                            curLabel.ModDate = DateTime.Now;
                            Factory.DaoLabel().Update(curLabel);

                            SaveNodeTrace(new NodeTrace
                            {
                                Node = recNode,
                                Document = document,
                                Label = curLabel,
                                Quantity = curLabel.CurrQty,
                                IsDebit = false,
                                CreatedBy = document.ModifiedBy,
                                Comment = "Receiving " + document.DocNumber
                            });
                        }
                }
                catch { }


                Factory.Commit();
                return label;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiveLabel:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }



        public Label ReceiveLabelForTransfer(Document document, Label label, Bin destLocation, Node recNode)
        {
            try
            {
                Factory.IsTransactional = true;

                Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //Trae de nuevo le label para cargar los childs en caso de que sea logistica
                label = Factory.DaoLabel().SelectById(new Label { LabelID = label.LabelID });                

                //////////////////////////////////////////////////////
                //Check if label is in the pending List
                Document shipment;
                try
                { shipment = Factory.DaoDocument().Select( new Document { DocNumber = document.CustPONumber, Company = document.Company }).First(); }
                catch
                { throw new Exception("Shipment transfer " + document.CustPONumber + " does not exists."); }

                //List of Pending Labels
                IList<Label> balanceList = Factory.DaoLabel().GetDocumentLabelAvailableFromTransfer(document, shipment, label);

                if (!balanceList.Any(f=>f.LabelID == label.LabelID))
                    throw new Exception("Label [" + label.LabelCode +"] is not in the list of labels to be received.");

                //////////////////////////////////////////////////


                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(document, true);

                Rules.ValidateBinStatus(destLocation, true);


                //Valida si el label es un label de producto, 
                Rules.ValidateIsProductLabel(label, true);

                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(label.Product, destLocation, true);

                //Valida si el producto esta en ese documento
                //Se debe ejecutar proceso para saber si la company permite 
                //recibir producto no existente en el docuemnto
                DocumentLine docLine = new DocumentLine
                {
                    Document = document,
                    Product = label.Product,
                    LineStatus = new Status { StatusID = DocStatus.New },
                    Unit = label.Unit
                };

                Rules.ValidateProductInDocument(docLine, true);

                //Valida si hay saldo pendiente por recibir
                docLine.Quantity = Factory.DaoLabel().SelectCurrentQty(label, null, true);
                Rules.ValidateBalanceQuantityInDocument(docLine, recNode, true, false);


                //Actualiza Label with new data
                label.Node = recNode;
                label.Bin = destLocation;
                label.ModifiedBy = document.ModifiedBy;
                label.ModDate = DateTime.Now;
                label.ReceivingDate = DateTime.Now;
                label.ReceivingDocument = document;
                label.Status = active;
                //Si el label estaba contenido en una logistica, al recibilo solo quiere decir que lo extrae de la logistica
                label.FatherLabel = null;

                Factory.DaoLabel().Update(label);

                //Registra el movimiento del label en el nodo
                SaveNodeTrace(new NodeTrace
                {
                    Node = recNode,
                    Document = document,
                    Label = label,
                    Quantity = label.CurrQty,
                    IsDebit = false,
                    CreatedBy = document.ModifiedBy,
                    Comment = "Transfer " + document.DocNumber
                });


                //Actualiza Los Hijos (si existen)
                try
                {
                    label.ChildLabels = Factory.DaoLabel().Select(new Label {FatherLabel = label});

                    if (label.ChildLabels != null && label.ChildLabels.Count > 0)
                        foreach (Label curLabel in label.ChildLabels)
                        {
                            curLabel.Node = recNode;
                            curLabel.Bin = label.Bin;

                            curLabel.ModifiedBy = document.ModifiedBy;
                            curLabel.ModDate = DateTime.Now;
                            Factory.DaoLabel().Update(curLabel);

                            SaveNodeTrace(new NodeTrace
                            {
                                Node = recNode,
                                Document = document,
                                Label = curLabel,
                                Quantity = curLabel.CurrQty,
                                IsDebit = false,
                                CreatedBy = document.ModifiedBy,
                                Comment = "Transfer " + document.DocNumber
                            });
                        }

                }
                catch { }

                Factory.Commit();
                return label;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiveLabelForTransfer:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }


        /// <summary>
        /// Recibe un documento de recibo por completo, (lo que tenga pendiente por recibr)
        /// </summary>
        /// <param name="document"></param>
        /// <param name="destLocation"></param>
        public void ReceiptAtOnce(Document document, Bin destLocation, Node recNode)
        {
            Factory.IsTransactional = true;

            //Valida si el docuemnto no es nulo
            Rules.ValidateDocument(document, true);


            //Node recNode = WType.GetNode(new Node { NodeID = NodeType.Stored });

            DocumentBalance docBal = new DocumentBalance
            {
                Document = document,
                Node = recNode
            };

            IList<DocumentBalance> balanceList = Factory.DaoDocumentBalance().BalanceByUnit(docBal);

            //Recorre las lineas del documento y las recibe usando ReceiveProduct
            if (balanceList == null || balanceList.Count == 0)
                throw new Exception("Document " + document.DocNumber + " not contains product to receive.");


            DocumentLine curLine;
            foreach (DocumentBalance line in balanceList.Where(f=>f.QtyPending > 0))
            {
                //Define Document, Product, Unit and Qty to send to receiving transaction
                curLine = new DocumentLine
                {
                    Document = document,
                    Product = line.Product,
                    Unit = line.Unit,
                    Quantity = line.QtyPending,
                    QtyPending = line.QtyPending, //Logistic factor
                    CreatedBy = document.ModifiedBy,
                    Note = "" //document.Notes
                };

                ReceiveProduct(curLine, new Unit(), destLocation, recNode);
            }

        }




        public void ReceiptAtOnceForTransfer(Document document, Bin destLocation, Node recNode)
        {
            Factory.IsTransactional = true;

            //Valida si el docuemnto no es nulo
            Rules.ValidateDocument(document, true);



            //Get the posting shipment
            Document shipment;
            try
            {
                shipment = Factory.DaoDocument().Select(
                    new Document { DocNumber = document.CustPONumber, Company = document.Company }).First();
            }
            catch
            {
                throw new Exception("Shipment transfer " + document.CustPONumber + " does not exists.");
            }

            IList<Label> balanceList = Factory.DaoLabel().GetDocumentLabelAvailableFromTransfer(document, shipment, null);

            //Recorre las lineas del documento y las recibe usando ReceiveProduct
            if (balanceList == null || balanceList.Count == 0)
                throw new Exception("Document " + document.DocNumber + " does not contain product to receive.");

            foreach (Label label in balanceList)
                ReceiveLabelForTransfer(document, label, destLocation, recNode);
        }

        /// <summary>
        /// Reversa lista de NodeTrace recibidos (labels selected)
        /// </summary>
        /// <param name="nodes"></param>
        public void ReverseReceiptNodeTraceByLabels(List<NodeTrace> nodes, SysUser user, DocumentType docType)
        {
            Node labelNode;
            Status status;
            //This option is used when label was pre-printed.
            //Node a donde regresa el producto
            if (docType.DocTypeID == SDocType.WarehouseTransferReceipt)
            {
                labelNode = Factory.DaoNode().SelectById(new Node { NodeID = NodeType.Released });
                status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Locked }).First();
            }
            else
            {
                labelNode = Factory.DaoNode().SelectById(new Node { NodeID = NodeType.PreLabeled });
                status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();
            }

            ReverseNodeTrace(nodes, user.UserName, labelNode, null, status);
        }

        
        /// <summary>
        /// Reversa lista de NodeTrace (qty selected)
        /// </summary>
        /// <param name="nodes"></param>
        /// OBSOLETE
        public void ReverseReceiptNodeTraceByQty(DocumentBalance docBalance, int quantity, SysUser user)
        {
            List<NodeTrace> nodes = Factory.DaoNodeTrace().Select(
                new NodeTrace { 
                    Document = docBalance.Document, 
                    Node = docBalance.Node, 
                    Label = new Label { 
                        Product = docBalance.Product, 
                        Printed = false, 
                        Unit = docBalance.Unit } }).Take(quantity).ToList();


            if (quantity > nodes.Count)
                throw new Exception("There are not enough quantities to complete the transaction");

            Node labelNode = Factory.DaoNode().Select(new Node { NodeID = NodeType.Voided }).First();
            ReverseNodeTrace(nodes, user.UserName, labelNode, null, null); //0 = Manual retail product.
        }
        


        //Permite reversar un documento PR que fue posteado en el ERP, 
        //solo se reversa si en el ERP no lo ha posteado
        public void ReversePurchaseReceipt(Document data)
        {
            Factory.IsTransactional = true;

            //if (data.Company.ErpConnection == null)
            //    throw new Exception("Please setup Erp Connection.");

            //SetConnectMngr(data.Company);

            Node recNode = new Node { NodeID = NodeType.Received };
            Node storedNode = new Node { NodeID = NodeType.Stored };

            try
            {


                //Update document status to Cancelled
                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });


                //Cross Dock 7 - Marzo - 09
                ReverseCrossDockProcess(data);

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
                    if (trace.Node.NodeID == NodeType.Stored)
                    {
                        trace.Node = voidNode;
                        trace.Status = inactive;
                        trace.ModDate = DateTime.Now;
                        trace.ModifiedBy = data.ModifiedBy;
                        trace.Comment = "Stored: " + trace.PostingDocument.DocNumber + " Reversed";
                        Factory.DaoNodeTrace().Update(trace);
                        /*
                        SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = voidNode,
                                Document = trace.Document,
                                Label = trace.Label,
                                Quantity = trace.Quantity,
                                IsDebit = trace.IsDebit,
                                CreatedBy = trace.CreatedBy,
                                PostingDocument = trace.PostingDocument,
                                PostingUserName = trace.PostingUserName,
                                Status = inactive,
                                Comment = "Receipt " + trace.PostingDocument.DocNumber + " Reversed",
                                ModDate = DateTime.Now,
                                ModifiedBy = data.ModifiedBy,
                                PostingDate = trace.PostingDate,
                            });
                         */
                    }

                    //Reversa el trace original para poderlo postear nuevamente
                    if (trace.Node.NodeID == NodeType.Received)
                    {
                        trace.DocumentLine = null;
                        trace.PostingDate = null;
                        trace.PostingDocument = null;
                        trace.PostingUserName = null;
                        trace.ModifiedBy = data.ModifiedBy;
                        trace.ModDate = DateTime.Now;

                        Factory.DaoNodeTrace().Update(trace);


                        //Reverse labels to node trace received
                        curLabel = trace.Label;
                        curLabel.Node = recNode;

                        try { curLabel.Notes += "Receipt " + trace.PostingDocument.DocNumber + " Reversed"; }
                        catch { }
                        
                        curLabel.ModDate = DateTime.Now;
                        curLabel.ModifiedBy = data.ModifiedBy;

                        Factory.DaoLabel().Update(curLabel);

                    }


                }

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReversePurchaseReceipt #" + data.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Persistence);
                throw;
                //return;
            }
        }


        //This method revisa si el recibo pertenece a aun procesos de cross dock
        //y Anula el docuemnto de crossdock y sus lineas, y Unpick las cantidades
        //piqueadas para los documentos de ventas
        private void ReverseCrossDockProcess(Document receipt)
        {

            if (receipt.CrossDocking != true)
                return;

            try
            {
                Factory.IsTransactional = true;

                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

                TaskDocumentRelation taskRel = new TaskDocumentRelation
                {
                    IncludedDoc = receipt,
                    TaskDoc = new Document { DocType = new DocumentType { DocTypeID = SDocType.CrossDock } }
                };


                IList<TaskDocumentRelation> listTask = Factory.DaoTaskDocumentRelation().Select(taskRel)
                    .Where(f => f.TaskDoc.DocStatus.StatusID != DocStatus.Cancelled).ToList();

                //Cuando no tiene docuemnto asociado
                if (listTask == null || listTask.Count == 0)
                    return;

                //Si tiene docuemnto asociado continua.
                //1. Cancela el documento cross dock y sus lineas.
                Document crossDockDocument = listTask[0].TaskDoc;
                crossDockDocument.DocStatus = cancelled; 
                WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                crossDockDocument.Comment += "\nDocument cancelled due the reversion of receipt " + receipt.DocNumber;
                crossDockDocument.ModifiedBy = receipt.ModifiedBy;
                crossDockDocument.ModDate = DateTime.Now;

                foreach (DocumentLine line in crossDockDocument.DocumentLines)
                {
                    line.LineStatus = cancelled;
                    Factory.DaoDocumentLine().Update(line);
                }

                //Actualizando el documento
                Factory.DaoDocument().Update(crossDockDocument);

                //Reversando las cantidades piqeuadas para suplir los documentos de ventas.
                //Obtiene las cantidades que fueron piquedas por cada liena de cada documento de vantas

                //Node traces que fueron afectados con ese recibo.
                NodeTrace sourceTrace = new NodeTrace
                {
                    Node = new Node { NodeID = NodeType.Picked },
                    Status = new Status { StatusID = EntityStatus.Active },
                    Comment = receipt.CustPONumber
                };

                IList<NodeTrace> nodes = Factory.DaoNodeTrace().Select(sourceTrace);

                Node labelNode = Factory.DaoNode().Select(new Node { NodeID = NodeType.Stored }).First();

                //revesar todo lo piqueado a main
                Bin bin = WType.GetBin(new Bin { Location = receipt.Location, BinCode = DefaultBin.PUTAWAY });

                Status status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();

                ReverseNodeTrace(nodes, receipt.ModifiedBy, labelNode, bin, status);

                Factory.Commit();

            }
            catch (Exception ex)
            {

                Factory.Rollback();

                ExceptionMngr.WriteEvent("ReverseCrossDockProcess:Doc#" + receipt.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw;

            }


        }




        //
        /// <summary>
        ///Receive Returns, primero recibe normal la cantidad total y luego saca de ese label todo lo que o sea onHand.
        /// Deja el mismo receiving document para poder crear el documento a postear en el ERP.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="retProduct"></param>
        /// <param name="sysUser"></param>
        /// <param name="retTotalQty"></param>
        /// <returns></returns>
        public bool ReceiveReturn(Document document, IList<ProductStock> retProduct, 
            SysUser sysUser, double retTotalQty, Node recNode)
        {

            Factory.IsTransactional = true;

            try
            {

                DocumentLine line = new DocumentLine
                {
                    Document = document,
                    Product = retProduct.First().Product, //View.ComboProduct.SelectedItem,
                    Unit = retProduct.First().Unit,
                    Quantity = retTotalQty,
                    QtyPending = retTotalQty,
                    QtyAllocated = 1,
                    CreatedBy = sysUser.UserName
                };

                //RETURN
                Bin binDest = WType.GetBin(new Bin { BinCode = DefaultBin.RETURN, Location = retProduct.First().Bin.Location });
                //retProduct.Where(f => f.Bin.BinCode == DefaultBin.RETURN).Select(f => f.Bin).First();

                ReceiveProduct(line, new Unit(), binDest, recNode);

                //Mueve las cantidades diferentes del Bin Return a el Bin Correcto.
                Label label = Factory.DaoLabel().Select(new Label { ReceivingDocument = document, 
                    Product = retProduct.First().Product,
                    Status = new Status { StatusID = EntityStatus.Active }, Node = recNode }).First();
                DocumentLine tmpLine = null;

                foreach (ProductStock ps in retProduct.Where(f=>f.Bin.BinCode != DefaultBin.RETURN))
                {
                    tmpLine = new DocumentLine { Quantity = ps.Stock, Document = document, 
                        Product = ps.Product, CreatedBy = sysUser.UserName, Unit = ps.Unit };
                    
                    //Disminuye el Label Original
                    DecreaseQtyFromLabel(label, tmpLine, "Decreased in return " + document.DocNumber, false, recNode, false);
                    label.StartQty = label.CurrQty;

                    //Crea un Lable Nuevo en el bin de destino.
                    IncreaseQtyIntoBin(tmpLine, recNode, ps.Bin, "Decreased in return " + document.DocNumber + ", Bin: " + ps.Bin.BinCode, true, DateTime.Now, null, label);
                }

                //Reversado el Node trace del label original al la cantidad final.
                try {
                    NodeTrace ndtrace = Factory.DaoNodeTrace().Select(new NodeTrace { Label = label, Document = document }).First();

                    if (label.CurrQty == 0)
                    {//Lo elimina porque la QTY es cero.
                        Factory.DaoNodeTrace().Delete(ndtrace);
                        Factory.DaoLabel().Delete(label);
                    }
                    else
                    {
                        ndtrace.Quantity = label.CurrQty;
                        Factory.DaoNodeTrace().Update(ndtrace);
                        Factory.DaoLabel().Update(label);
                    }
                }
                catch { }



                return true;
            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiveReturn:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }



        #endregion


        #region Receiving With TrackOptions


        public Label CreateUniqueTrackLabel(Label fatherLabel, string labelCode)
        {

            //Chequear que no exista un labelcode igual para el mismo producto. En estado Activo.
            if (labelCode != WmsSetupValues.AutoSerialNumber)
            {
                if (Factory.DaoLabel().Select(new Label { LabelCode = labelCode.Trim(), Product = fatherLabel.Product,
                    Node = new Node { NodeID = NodeType.Stored }, 
                    Status = new Status { StatusID = EntityStatus.Active } }).Count() > 0)

                    throw new Exception("Track# [" + labelCode + "] already exists.");
            }


            Factory.IsTransactional = true;

            //Crea un label a partir de otro, poniendol eun labelcode diferente
            Label tmpFather = Factory.DaoLabel().Select(new Label { LabelID = fatherLabel.LabelID }).First();


            //Si es una label pre impreso se pasa a stored por que e sun INitial Inventory Label.
            if (tmpFather.Node.NodeID == NodeType.PreLabeled)
                tmpFather.Node = new Node { NodeID = NodeType.Stored };


            Label uniqueLabel = fatherLabel;
            uniqueLabel.StartQty = 1;
            uniqueLabel.CurrQty = 1;
            uniqueLabel.LabelID = 0;
            uniqueLabel.LabelCode = labelCode == WmsSetupValues.AutoSerialNumber ? Guid.NewGuid().ToString() : labelCode.Trim();
            uniqueLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.UniqueTrackLabel });
            uniqueLabel.Bin = fatherLabel.Bin;
            uniqueLabel.Printed = true;
            uniqueLabel.Unit = uniqueLabel.Product.BaseUnit; //Garantiza que el unique sea de unidad basica.


            uniqueLabel = Factory.DaoLabel().Save(uniqueLabel);

            if (labelCode == WmsSetupValues.AutoSerialNumber)
            {
                uniqueLabel.LabelCode = '1' + uniqueLabel.LabelID.ToString().PadLeft(WmsSetupValues.LabelLength - 1, '0');
                Factory.DaoLabel().Update(uniqueLabel);
            }


            if (tmpFather.Unit.BaseAmount > 1)
            {
                tmpFather.StartQty = tmpFather.StartQty * tmpFather.Unit.BaseAmount;
                tmpFather.CurrQty = tmpFather.CurrQty * tmpFather.Unit.BaseAmount;
                tmpFather.Unit = tmpFather.Product.BaseUnit;
            }

            if (tmpFather.CurrQty > 0)
                tmpFather.CurrQty--;

            Factory.DaoLabel().Update(tmpFather);


            uniqueLabel.FatherLabel = tmpFather;
            Factory.DaoLabel().Update(uniqueLabel);


            Factory.Commit();

            return uniqueLabel;

        }


        public Label CreateUniqueTrackLabel(Product product, string labelCode, Label destLabel, string user)
        {

            //Chequear que no exista un labelcode igual para el mismo producto. En estado Activo.
            if (labelCode != WmsSetupValues.AutoSerialNumber)
            {
                if (Factory.DaoLabel().Select(new Label { LabelCode = labelCode.Trim(), Product = product,
                        Node = new Node { NodeID = NodeType.Stored },
                        Status = new Status { StatusID = EntityStatus.Active }
                }).Count() > 0)
                    throw new Exception("Track# [" + labelCode + "] already exists.");
            }


            Factory.IsTransactional = true;

            //BIN


            Label uniqueLabel = new Label { };
            uniqueLabel.StartQty = 1;
            uniqueLabel.CurrQty = 1;
            uniqueLabel.LabelID = 0;
            uniqueLabel.LabelCode = labelCode == WmsSetupValues.AutoSerialNumber ? Guid.NewGuid().ToString() : labelCode.Trim();
            uniqueLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.UniqueTrackLabel });
            uniqueLabel.Bin = destLabel.Bin;
            uniqueLabel.Printed = true;
            uniqueLabel.Unit = product.BaseUnit; //Garantiza que el unique sea de unidad basica.
            uniqueLabel.CreatedBy = user;
            uniqueLabel.CreationDate = DateTime.Now;
            uniqueLabel.Notes = "Adjusmet Inventory Serial";
            uniqueLabel.Node = new Node { NodeID = NodeType.Stored };
            uniqueLabel.Product = product;
            uniqueLabel.Status = new Status { StatusID = EntityStatus.Active };


            if (destLabel.LabelType.DocTypeID == LabelType.ProductLabel)
                uniqueLabel.FatherLabel = destLabel;
            

            uniqueLabel = Factory.DaoLabel().Save(uniqueLabel);

            if (labelCode == WmsSetupValues.AutoSerialNumber)
            {
                uniqueLabel.LabelCode = '1' + uniqueLabel.LabelID.ToString().PadLeft(WmsSetupValues.LabelLength - 1, '0');
                Factory.DaoLabel().Update(uniqueLabel);
            }


            Factory.Commit();

            return uniqueLabel;
        }




        public Label EmptyUniqueTrackLabel(Product product, string labelCode, string user)
        {
            //Chequear que no exista un labelcode igual para el mismo producto. En estado Activo.
            try
            {

                Label lbl = Factory.DaoLabel().Select(new Label
                {
                    LabelCode = labelCode.Trim(),
                    Product = product,
                    Node = new Node { NodeID = NodeType.Stored },
                    Status = new Status { StatusID = EntityStatus.Active }
                }).First();

                lbl.CurrQty = 0;
                lbl.LabelCode = "VOID_" + DateTime.Now.ToString("YMdHms") + lbl.LabelCode;
                lbl.ModDate = DateTime.Now;
                lbl.Node = WType.GetNode(new Node { NodeID = NodeType.Voided });
                lbl.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

                Factory.DaoLabel().Update(lbl);
                return lbl;

            }
            catch
            {
                throw new Exception("Track # [" + labelCode + "] does not exists or is inactive.");
            }


        }



        public Label UpdateUniqueTrackLabel(Label uniqueLabel, string labelCode)
        {
            //Chequear que no exista un labelcode igual para el mismo producto. En estado Activo.
            if (labelCode != WmsSetupValues.AutoSerialNumber)
            {
                if (Factory.DaoLabel().Select(new Label
                {
                    LabelCode = labelCode.Trim(),
                    Product = uniqueLabel.Product,
                    Node = new Node { NodeID = NodeType.Stored },
                    Status = new Status { StatusID = EntityStatus.Active }
                }).Count() > 0)
                    throw new Exception("Track# [" + labelCode + "] already exists.");

                uniqueLabel.LabelCode = labelCode.Trim();
                Factory.DaoLabel().Update(uniqueLabel);
            }

            return uniqueLabel;
        }


        public Label UpdateLabelTracking(Label label, TrackOption trackOpt, string trackValue, string user)
        {

            IList<LabelTrackOption> trackList = (label.TrackOptions == null || label.TrackOptions.Count == 0) 
                ? new List<LabelTrackOption>() : label.TrackOptions; ;


            if (label.TrackOptions == null || label.TrackOptions.Where(f => f.TrackOption.RowID == trackOpt.RowID).Count() == 0)
            {
                LabelTrackOption curTrack = new LabelTrackOption
                {
                    Label = label,
                    CreatedBy = user,
                    CreationDate = DateTime.Now,
                    TrackOption = trackOpt,
                    TrackValue = trackValue
                };

                trackList.Add(curTrack);

            }
            else //Si ya existe un registro con la informacion para ese trackoption
            {
                trackList.Where(f => f.TrackOption.RowID == trackOpt.RowID).First().Label = label;
                trackList.Where(f => f.TrackOption.RowID == trackOpt.RowID).First().CreatedBy = user;
                trackList.Where(f => f.TrackOption.RowID == trackOpt.RowID).First().CreationDate = DateTime.Now;
                trackList.Where(f => f.TrackOption.RowID == trackOpt.RowID).First().TrackOption = trackOpt;
                trackList.Where(f => f.TrackOption.RowID == trackOpt.RowID).First().TrackValue = trackValue;
            }


            //Adicionando las track options
            label.TrackOptions = trackList.ToList();

            //Update Label
            label.ModDate = DateTime.Now;
            label.ModifiedBy = user;
            Factory.DaoLabel().Update(label);

            return label;
        }


        /// <summary>
        /// Crea un numero de labels definido para el numero de paquetes recibidos de un PO
        /// Envia el mail de notificacion.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="numLabels"></param>
        /// <param name="sysUser"></param>
        public void ReceiptAcknowledge(Document document, double numLabels, SysUser sysUser, string appPath)
        {
            //Crear el documento de Mail para enviar el Mensaje.
            SendProcessNotification(sysUser, document, BasicProcess.ReceiptAcknowledge);

            //Cambiar las fechas de arrive del PO que llego
            document.Date5 = DateTime.Now;
            Factory.DaoDocument().Update(document);


            //Manadar a Imprimir los N Labels de Acknolegement.
            if (string.IsNullOrEmpty(appPath))
                 appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);

            LabelTemplate template = Factory.DaoLabelTemplate().Select(new LabelTemplate { Header = WmsSetupValues.DefTpl_DocumentLabel }).First();


            IList<Label> labelsToPrintX = new List<Label>();
            for (int z = 1; z <= (int)numLabels; z += 1)
            {
                labelsToPrintX.Add(
                    new Label
                    {
                        LabelCode = document.DocNumber,                        
                        LabelType = new DocumentType { DocTypeID = LabelType.CustomerLabel },
                        CurrQty = z,
                        StartQty = numLabels,
                        CreatedBy = sysUser.UserName
                    });
            }

            ReportMngr.PrintLabelsFromFacade(new Printer { PrinterName = WmsSetupValues.DEFAULT },
                template, labelsToPrintX, appPath);

        }




        public bool ReceiveProductAsUnique(DocumentLine receivingLine, Bin destLocation, Node recNode)
        {
            Factory.IsTransactional = true;

            try
            {
                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(receivingLine.Document, true);

                //status del Bin donde se Recibira.
                Rules.ValidateBinStatus(destLocation, true);

                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(receivingLine.Product, destLocation, true);


                if (receivingLine.Document.DocType.DocTypeID != SDocType.ReceivingTask)
                {
                    //Valida si el producto esta en ese documento
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = receivingLine.Document,
                        Product = receivingLine.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = receivingLine.Unit,
                        Quantity = receivingLine.Quantity
                    };

                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por recibir
                    Rules.ValidateBalanceQuantityInDocument(docLine, recNode, true, false);
                }


                //Manda a Crear los Labels
                CreateProductUniqueTrackLabels(receivingLine, recNode, destLocation, receivingLine.Note, "", DateTime.Now);


                Factory.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ReceiveProductAsUnique:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }






       #endregion


    }
}
