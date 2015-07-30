using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities;
using Integrator;
using Entities.General;
using Entities.Master;
using Entities.Profile;


namespace BizzLogic.Logic
{
    public partial class TransactionMngr
    {
        
        
        #region Picking/Shipping Transactions


        /// <summary>
        /// Recolecta= producto, sin etiqueta (recibo manual) toma de los labels virtuales para cada unidad basica de producto
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sourceLocation"></param>
        /// <param name="node"></param>
        /// <param name="packageLabel"></param>
        /// <param name="picker"></param>
        public void PickProduct(DocumentLine line, Label sourceLocation, Node destNode, Label packageLabel, SysUser picker, Bin destBin)
        {

            Factory.IsTransactional = true;

            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });

            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });
            DocumentType labelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });

            if (destBin == null)
                destBin = Rules.GetBinForNode(destNode, sourceLocation.Bin.Location);
            

            try
            {

                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(line.Document, true);

                Rules.ValidateBinStatus(sourceLocation.Bin, true);

                //Validar si las locations son iguales
                Rules.ValidatePickLocation(line.Document.Location, sourceLocation.Bin.Location, true);


                if (sourceLocation.LabelType.DocTypeID == LabelType.ProductLabel)
                {
                    //Valida que este activo
                    Rules.ValidateActiveStatus(sourceLocation.Status, true);

                    //Validar que no este vod
                    Rules.ValidateVoided(sourceLocation.Node, true);
                }



                if (line.Document.DocType.DocTypeID != SDocType.PickTicket)
                {
                    //Valida si el producto esta en ese documento
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = line.Document,
                        Product = line.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        CreatedBy = picker.UserName
                    };

                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por procesar
                    Rules.ValidateBalanceQuantityInDocument(docLine, destNode, true, false);
                }


                //Evaluacion de tipo de source, Bin or Label
                DateTime recDate = DateTime.Now;
                if (Rules.ValidateIsBinLabel(sourceLocation, false))
                {
                    IList<Label> tranLabel = DecreaseQtyFromBin(sourceLocation, line, "Picking Source Product", true, storedNode);
                    try
                    {
                        recDate = (DateTime)tranLabel.Where(f => f.ReceivingDate != null).OrderBy(f => f.ReceivingDate).First().ReceivingDate;
                    }
                    catch { recDate = DateTime.Now; }
                }

                    //SI el ajustes es sobre un Label
                else if (Rules.ValidateIsProductLabel(sourceLocation, false))
                {

                    DecreaseQtyFromLabel(sourceLocation, line, "Picking Source Product", true, storedNode, true);
                    try { recDate = (sourceLocation.ReceivingDate == null) ? DateTime.Now : (DateTime)sourceLocation.ReceivingDate; }
                    catch { recDate = DateTime.Now; }
                }


                //Creando el package para ingresar la mercancia.
                if (packageLabel != null)
                {
                    line.Document.Location = sourceLocation.Bin.Location;  //Revalidando que el location sea correcto

                    try { packageLabel = GetPackageLabel(packageLabel, line.Document, picker).PackLabel; }
                    catch (Exception ex)
                    {
                        Factory.Rollback();
                        throw new Exception("Package label could not be created.\n" + ex.Message);
                    }
                }


                //Increasing the Record of Product on Dest Bin.
                Label pickedLabel = IncreaseQtyIntoBin(line, destNode, destBin, "Picking Dest Product", true, recDate, null, sourceLocation);
                pickedLabel.FatherLabel = packageLabel;
                pickedLabel.Status = locked;
                pickedLabel.ShippingDocument = line.Document;
                Factory.DaoLabel().Update(pickedLabel);

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("PickProduct:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }


        /// <summary>
        /// Obtiene el package label donde el producto va a quedar almacenado,
        /// -1 Default package solo un packaga para toda la orden
        /// 0 New Package para esa Orden (En estado Open)
        /// -2 New Package that get the inventory package to shipping package leave it Close
        /// #number Number of package already Open.
        /// </summary>
        /// <param name="packageLabel"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private DocumentPackage GetPackageLabel(Label packageLabel, Document document, SysUser picker)
        {
            switch (packageLabel.LabelID)
            {
                //Default one package in the document.
                case -1:

                    IList<DocumentPackage> packList = Factory.DaoDocumentPackage().Select(
                        new DocumentPackage
                        {
                            Document = document,
                            PostingDocument = new Document { DocID = -1 },
                            ParentPackage = new DocumentPackage { PackID = -1},
                        }); //-1 indica que es NULL

                    if (packList != null && packList.Count() > 0)
                    {
                        packList.First().EndTime = DateTime.Now;
                        Factory.DaoDocumentPackage().Update(packList.First());
                        return packList.First(); //Devuelve el label abierto.
                    }

                    // Si no devuelve el Label Nuevo
                    return CreateNewPackage(document, picker, true, null, "R"); //Root


                case 0:
                    return CreateNewPackage(document, picker, true, null, null);


                case -2:
                    return CreateNewPackage(document, picker, false, null, null);
            }


            //Actualiza el tiempo de Finaliacion antes de salir
            try {
                packageLabel = Factory.DaoLabel().SelectById(packageLabel);
                packageLabel.DocumentPackages[0].EndTime = DateTime.Now;
                Factory.DaoDocumentPackage().Update(packageLabel.DocumentPackages[0]);
            }
            catch {}

            return packageLabel.DocumentPackages[0];
        }



        /// <summary>
        /// Crea a new label package for a specific document, this package will contain product picked for the order.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="picker"></param>
        /// <returns></returns>
        public DocumentPackage CreateNewPackage(Document document, SysUser picker, bool isOpen, 
            DocumentPackage parent, string packageType)
        {

            Factory.IsTransactional = true;

            Node node = WType.GetNode(new Node { NodeID = NodeType.Picked });
            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            DocumentType labelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.CustomerLabel });
            Unit logisticUnit = WType.GetUnit(new Unit { Company = document.Company, Name = WmsSetupValues.CustomUnit });
            Bin destLocation = WType.GetBin(new Bin { Location = document.Location, BinCode = DefaultBin.PICKING });

            int sequence = Factory.DaoDocumentPackage().Select(new DocumentPackage
            {
                Document = document,
                //PostingDocument = new Document {DocID = -1 }
            }).Count + 1;

            //Generate new logistig labels located in MAIN
            //Labels shouldbe activated the next transaction
            try
            {
                //Funcion para obtener siguiente Label
                //DocumentTypeSequence initSequence = GetNextDocSequence(document.Company, labelType);                

                Label packLabel = new Label();
                packLabel.Node = node;
                packLabel.Bin = destLocation;
                packLabel.CreatedBy = picker.UserName;
                packLabel.Status = status;
                packLabel.LabelType = labelType;
                packLabel.CreationDate = DateTime.Now;
                packLabel.Printed = false;
                packLabel.Unit = logisticUnit;
                packLabel.IsLogistic = true;
                packLabel.LabelCode = ""; // initSequence.NumSequence.ToString() + GetRandomHex(picker.UserName, initSequence.NumSequence);
                packLabel.Notes = "Package label for Document # " + document.DocNumber;
                packLabel.ShippingDocument = document;

                //Added on 14/ENE/09
                if (parent != null && parent.PackLabel != null && parent.PackLabel.LabelID != 0)
                {
                    try { packLabel.FatherLabel = parent.PackLabel; }
                    catch { }
                }



                //Creado el document Package Asociado al Label
                DocumentPackage docPack = new DocumentPackage
                {
                    Document = document,
                    CreatedBy = picker.UserName,
                    CreationDate = DateTime.Now,
                    IsClosed = !isOpen,
                    PackLabel = packLabel,
                    Picker = picker,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    Sequence = (short)sequence,
                    Dimension = "",
                    ShipToName = document.Customer.Name,
                    //Added on 14/ENE/09
                    ParentPackage = (parent != null && parent.PackID != 0) ? parent : null,
                    PackageType = packageType
                };

                //Address Line for package 16/oct/09

                DocumentAddress ShipTo_address = null;
                try
                {

                    ShipTo_address = Factory.DaoDocumentAddress().Select(
                        new DocumentAddress
                        {
                            Document = document,
                            AddressType = AddressType.Shipping
                        })
                        .Where(f => f.DocumentLine == null).First();

                    docPack.AddressLine1 = ShipTo_address.AddressLine1 + " " + ShipTo_address.AddressLine2;
                    docPack.AddressLine2 = ShipTo_address.City + ", " + ShipTo_address.State + " " + ShipTo_address.ZipCode;
                    docPack.AddressLine3 = ShipTo_address.Country;

                }
                catch { }

                packLabel.DocumentPackages = new List<DocumentPackage> { docPack };
                packLabel = Factory.DaoLabel().Save(packLabel);
                packLabel.LabelCode = packLabel.LabelID.ToString();

                //Registra el movimiento del nodo

                SaveNodeTrace(
                    new NodeTrace
                    {
                        Node = node,
                        Document = document,
                        Label = packLabel,
                        Quantity = packLabel.CurrQty,
                        IsDebit = false,
                        CreatedBy = picker.UserName
                    }
                );

                //initSequence.NumSequence;
                //Factory.DaoDocumentTypeSequence().Update(initSequence);
                Factory.Commit();


                //actualizando el documento
                try
                {
                    if (string.IsNullOrEmpty(document.UserDef3))
                    {
                        //document.UserDef3 = picker.UserName;
                        Factory.DaoDocument().Update(document);
                    }
                }
                catch { }

                return docPack;
            }
            catch { throw; }
        }


        /// <summary>
        /// Recibe producto, con etiqueta (recibo capturado por scanner generalmente)
        /// </summary>
        /// <param name="document">Task Document in Process</param>
        /// <param name="label">Label de transaccion </param>
        public Label PickLabel(Document document, Label label, Node node, Label packageLabel, SysUser picker, Bin destBin)
        {
            Factory.IsTransactional = true;

            //Node node = WType.GetNode(new Node { NodeID = NodeType.Picked });

            try
            {
                if (label.LabelID == 0)
                {
                    try { label = Factory.DaoLabel().Select(label).First(); }
                    catch { throw new Exception("Label " + label.LabelCode + " does not exists."); }
                }

                //Check if already picked
                if (label.Node.NodeID == NodeType.Picked || label.Node.NodeID == NodeType.Released)
                    throw new Exception("Label " + label.LabelCode + " already picked.");


                if (destBin == null)
                    destBin = Rules.GetBinForNode(node, label.Bin.Location);

                Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });


                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(document, true);

                //Valida si el label es un label de producto, 
                //TODO: alarma cuand o suceda el evento de que no es un label de producto
                Rules.ValidateIsProductLabel(label, true);

                Rules.ValidateBinStatus(label.Bin, true);

                //Valida si el status es Activo
                Rules.ValidateActiveStatus(label.Status, true);

                //Valida si el label esta en el nodo que debe estar (Ruta de Nodos)
                //TODO: alarma cuand o suceda el evento de que no es un label de producto
                Rules.ValidateNodeRoute(label, node, true);

                //revisa si el label tiene zero Qty
                Rules.ValidateLabelQuantity(label, true);


                //Validar si las locations son iguales
                Rules.ValidatePickLocation(document.Location, label.Bin.Location, true);


                //label.ChildLabels = Factory.DaoLabel().Select(
                       // new Label { FatherLabel = label, Status = new Status { StatusID = EntityStatus.Active } });


                if (document.IsFromErp == true)
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
                        Quantity = label.StockQty, //label.CurrQty,
                        CreatedBy = document.ModifiedBy
                    };

                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por recibir
                    //Calcula el Current Qty y Valida si esa cantidad aun esta pendiente en el documento
                    //Double quantity = (label.IsLogistic == true) ? Factory.DaoLabel().SelectCurrentQty(label, null, true) : label.CurrQty;                    
                    Rules.ValidateBalanceQuantityInDocument(docLine, node, true, false);
                }


                if (packageLabel != null)
                {
                    try { packageLabel = GetPackageLabel(packageLabel, document, picker).PackLabel; }
                    catch (Exception ex)
                    {
                        Factory.Rollback();
                        throw new Exception("Package label could not be created.\n" + ex.Message);
                    }

                }

                //Actualiza Label with new data
                label.Node = node;
                label.LastBin = label.Bin;
                label.Bin = destBin;
                label.ModifiedBy = picker.UserName;
                label.ModDate = DateTime.Now;
                label.ShippingDocument = document;
                label.Printed = true;
                label.Status = locked;

                //Registra el movimiento del label en el nodo
                SaveNodeTrace(new NodeTrace
                {
                    Node = node,
                    Document = document,
                    Label = label,
                    Quantity = label.CurrQty,
                    IsDebit = false
                });

                label.LabelSource = label.FatherLabel;
                label.FatherLabel = packageLabel;
                Factory.DaoLabel().Update(label);


                //Actualiza Los Hijos (si existen)
                try
                {
                    label.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = label });

                    if (label.ChildLabels != null && label.ChildLabels.Count > 0)
                        foreach (Label curLabel in label.ChildLabels)
                        {
                            curLabel.Node = node;
                            curLabel.LastBin = label.LastBin;
                            curLabel.Bin = label.Bin;
                            curLabel.ModifiedBy = picker.UserName;
                            curLabel.ModDate = DateTime.Now;
                            curLabel.ShippingDocument = document;
                            curLabel.Status = locked;

                            SaveNodeTrace(new NodeTrace
                            {
                                Node = node,
                                Document = document,
                                Label = curLabel,
                                Quantity = curLabel.CurrQty,
                                IsDebit = false,
                                CreatedBy = document.CreatedBy
                            });

                            Factory.DaoLabel().Update(curLabel);
                        }
                }
                catch { }


                Factory.Commit();
                return label;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("PickLabel:", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }



        public void PickAtOnce(Document document, Label sourceLocation, Node node, SysUser picker)
        {
            Factory.IsTransactional = true;
            //Node node = WType.GetNode(new Node { NodeID = NodeType.Picked });

            DocumentBalance docBal = new DocumentBalance
            {
                Document = document,
                Node = node
            };

            IList<DocumentBalance> balanceList = Factory.DaoDocumentBalance().BalanceByUnit(docBal);

            //Recorre las lineas del documento y las pickea usando PickProduct, pero solo si el balance 
            //existe en la location indicada para todo lo pendiente.

            if (balanceList == null || balanceList.Count == 0)
                throw new Exception("Document " + document.DocNumber + " not contains product pending to pick.");


            DocumentLine curLine;
            string fullExistence = "";

            foreach (DocumentBalance line in balanceList.Where(f=>f.QtyPending > 0))
            {
                //Define Document, Product, Unit and Qty to send to receiving transaction
                curLine = new DocumentLine
                {
                    Document = document,
                    Product = line.Product,
                    Unit = line.Unit,
                    Quantity = line.QtyPending,
                    CreatedBy = picker.UserName
                };

                fullExistence += CheckForStockInLocation(curLine, sourceLocation);

            }


            //Si alguno no tiene existencia no puede ejecutar el PickAtOnce
            if (!string.IsNullOrEmpty(fullExistence))
            {
                ExceptionMngr.WriteEvent("PickAtOnce:", ListValues.EventType.Error, null, null, ListValues.ErrorCategory.Business);
                throw new Exception(fullExistence);
            }


            //Ejecutando el Picking despues de que se confirma la existencia
            foreach (DocumentBalance line in balanceList)
            {
                //Define Document, Product, Unit and Qty to send to receiving transaction
                curLine = new DocumentLine
                {
                    Document = document,
                    Product = line.Product,
                    Unit = line.Unit,
                    Quantity = line.QtyPending
                };

                Label packageLabel = new Label { LabelID = -1 };
                PickProduct(curLine, sourceLocation, node, packageLabel, picker, null);
            }
        }



        private string CheckForStockInLocation(DocumentLine line, Label sourceLocation)
        {
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
            //Toma las etiquetas No printed del sourceLocation
            //Obtiene los labels que va a mover

            if (!Rules.ValidateBinStatus(sourceLocation.Bin, false))
                return "Bin affected: "+sourceLocation.Bin.BinCode + " is currently " + sourceLocation.Bin.Status.Name;

            //Label sourceLabel = new Label { Bin = sourceLocation, LabelType = new DocumentType { DocTypeID = LabelType.BinLocation } };

            IList<Label> labelList = GetQuantityOfLabels(sourceLocation, line);

            if (labelList.Sum(f=>f.BaseCurrQty) < line.Quantity*line.Unit.BaseAmount)
                return "No quantity available in the bin " + sourceLocation.Name + " for product " + line.Product.Name + ".\n";


            return "";
        }



        public void PickCrossDockProduct(Document purchase, IList<DocumentBalance> crossDockBalance, SysUser picker)
        {
            Factory.IsTransactional = true;

            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
            Node node = WType.GetNode(new Node { NodeID = NodeType.Picked });
            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            DocumentType labelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });

            Bin pickingBin = WType.GetBin(new Bin { Location = purchase.Location, BinCode = DefaultBin.PICKING });

            Dictionary<Document,Label> packageLabel = new Dictionary<Document,Label>();
            Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });

           
            try
            {
                //Solo toma las lineas de sales y deja quietas las del docuemnto de purchasing
                foreach (DocumentBalance line in crossDockBalance.Where(f => f.Document.DocType.DocClass.DocClassID == SDocClass.Shipping))
                {
                    //Valida si el documento no es nulo
                    Rules.ValidateDocument(line.Document, true);


                    if (line.Document.DocType.DocTypeID != SDocType.PickTicket)
                    {
                        //Valida si el producto esta en ese documento
                        DocumentLine docLine = new DocumentLine
                        {
                            Document = line.Document,
                            Product = line.Product,
                            LineStatus = new Status { StatusID = DocStatus.New },
                            Unit = line.Unit,
                            Quantity = line.QtyProcessed //Quatity processed que es la que hace el cruce con el CrossDock
                        };

                        Rules.ValidateProductInDocument(docLine, true);

                        //Valida si hay saldo pendiente por procesar
                        try { Rules.ValidateBalanceQuantityInDocument(docLine, node, true, true); }
                        catch { continue; }
                    }

                    //Toma el producto del nodetrace
                    //Obtiene los labels que va a mover
                    NodeTrace sourceTrace = new NodeTrace
                    {
                        Document = purchase,
                        //Dec 7/09 no se puede forzar a que sea la misma unidad del balance //Unit = line.Unit,
                        Label = new Label { Product = line.Product, Node = storedNode, Status = status },
                        Status = status,
                        Node = storedNode
                    };

                    //Obtiene las transacciones del node trace para ese documento especifico de Purchase.                
                    IList<Label> labelList = Factory.DaoNodeTrace().Select(sourceTrace).Select(f => f.Label)
                        //.Take(int.Parse(line.QtyProcessed.ToString()))
                        .ToList();


                    if (labelList.Sum(f=>f.CurrQty*f.Unit.BaseAmount) < line.QtyProcessed * line.Unit.BaseAmount)
                    {
                        Factory.Rollback();
                        throw new Exception("No quantity available in the purchase document " + purchase.DocNumber
                            + " for product " + line.Product.FullDesc + ".\nQty Available: " + labelList.Sum(f => f.CurrQty * f.Unit.BaseAmount).ToString()
                            + " Qty Requested: " + (line.QtyProcessed * line.Unit.BaseAmount).ToString()+" in Doc# " + line.Document.DocNumber );
                    }


                    //Package Label para el Despacho
                    if (!packageLabel.ContainsKey(line.Document))
                        packageLabel.Add(line.Document, GetPackageLabel(new Label { LabelID = -1 }, line.Document, picker).PackLabel);


                    //Debe piquear la cantidad necesaria para suplir el SO con los labels 
                    //recibidos.

                    double crQtyBal = line.QtyProcessed*line.Unit.BaseAmount; //LLevada  a la unidad basica

                    foreach (Label curLabel in labelList)
                    {
                        if (crQtyBal <= 0)
                            break;

                        //Si el Qty del label menor que lo pendiente mando todo el label
                        if (curLabel.CurrQty*curLabel.Unit.BaseAmount <= crQtyBal)
                        {
                            //Si el destino es logitico lo hace su padre, si no es producto suelto en BIN
                            curLabel.ModifiedBy = purchase.ModifiedBy;
                            curLabel.Node = node;
                            curLabel.LastBin = curLabel.Bin;
                            curLabel.Bin = pickingBin;
                            curLabel.ModDate = DateTime.Now;

                            curLabel.FatherLabel = null;
                            curLabel.FatherLabel = packageLabel[line.Document];
                            curLabel.Status = locked;

                            SaveNodeTrace(new NodeTrace
                            {
                                Node = node,
                                Label = curLabel,
                                Quantity = curLabel.CurrQty,
                                Bin = pickingBin,
                                IsDebit = false,
                                CreatedBy = purchase.ModifiedBy,
                                Document = line.Document,
                                // 07 Marzo 2009
                                // En el comenttario se pone el # del PO de cross dock, 
                                // este dato sirve en caso de reversion del crossdock process
                                Comment = purchase.DocNumber
                            });

                           
                            curLabel.ShippingDocument = line.Document;
                            Factory.DaoLabel().Update(curLabel);
                            crQtyBal -= curLabel.CurrQty * curLabel.Unit.BaseAmount;
                        }

                        //Si no: disminuyo el Qty del label y hago un Increase
                        else
                        {
                            //Si el destino es logitico lo hace su padre, si no es producto suelto en BIN
                            curLabel.ModifiedBy = purchase.ModifiedBy;
                            curLabel.ModDate = DateTime.Now;
                            curLabel.CurrQty -= crQtyBal;
                            Factory.DaoLabel().Update(curLabel);

                            //Increase The Pick Node

                            Node pickNode = WType.GetNode(new Node { NodeID = NodeType.Picked });

                            DocumentLine crdLine = new DocumentLine {
                                Document = line.Document,
                                Product = line.Product,
                                Quantity = crQtyBal,
                                CreatedBy = purchase.ModifiedBy
                            };

                            Label pickedLabel = IncreaseQtyIntoBin(crdLine, pickNode, Rules.GetBinForNode(pickNode, purchase.Location), 
                                "Picking Dest", true, DateTime.Now, null, curLabel);
                           
                            pickedLabel.FatherLabel = packageLabel[line.Document];
                            pickedLabel.Status = locked;
                            pickedLabel.ShippingDocument = line.Document;
                            
                            Factory.DaoLabel().Update(pickedLabel);

                            //Factory.Commit();
                        }

                    }



                    /*
                    //Acutualiza la ubicacion Nuevo Bin
                    foreach (Label curLabel in labelList)
                    {
                        //Si el destino es logitico lo hace su padre, si no es producto suelto en BIN
                        curLabel.ModifiedBy = purchase.ModifiedBy;
                        curLabel.Node = node;
                        curLabel.LastBin = curLabel.Bin;
                        curLabel.Bin = pickingBin;

                        SaveNodeTrace(new NodeTrace
                        {
                            Node = node,
                            Label = curLabel,
                            Quantity = curLabel.CurrQty,
                            Bin = pickingBin,
                            IsDebit = false,
                            CreatedBy = purchase.ModifiedBy,
                            Document = line.Document,
                            // 07 Marzo 2009
                            // En el comenttario se pone el # del PO de cross dock, 
                            // este dato sirve en caso de reversion del crossdock process
                            Comment = purchase.DocNumber
                        });

                        curLabel.FatherLabel = null;
                        Factory.DaoLabel().Update(curLabel);
                    }
                     */

                }

                Factory.Commit();


                //Actualiza el estado de los documentos de shiiping a CrossDock
                foreach (Document doc in crossDockBalance.Where(f => f.Document.DocType.DocClass.DocClassID == SDocClass.Shipping).Select(f => f.Document).Distinct())
                {
                    doc.CrossDocking = true;
                    Factory.DaoDocument().Update(doc);
                }

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("PickCrossDockProduct:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }


        /// <summary>
        /// Reversa lista de NodeTrace picked (labels selected)
        /// </summary>
        /// <param name="nodes"></param>
        public void ReversePickingNodeTraceByLabels(List<NodeTrace> nodes, SysUser user, Bin restockBin)
        {
            //Node a donde regresa el producto
            Node labelNode = Factory.DaoNode().SelectById(new Node { NodeID = NodeType.Stored });
            Status status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();

            ReverseNodeTrace(nodes, user.UserName, labelNode, restockBin, status);
        }




        /// <summary>
        /// Piquea un producto a una orden teneindo en cuanta las tracking options.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="qtyToPick"></param>
        /// <param name="node"></param>
        /// <param name="picker"></param>
        public Label PickProductWithTrack(Document document, Label label, double qtyToPick, Node destNode, SysUser picker, 
            Label packLabel)
        {
            //Debe piquear de producto suelto, teniendo en cuanta el Track del Label.
            Factory.IsTransactional = true;

            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });

            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });
            //DocumentType labelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });

            Bin destBin = Rules.GetBinForNode(destNode, document.Location);


            try
            {

                //Valida si el docuemnto no es nulo
                Rules.ValidateDocument(document, true);

                Rules.ValidateBinStatus(label.Bin, true);


                //Valida si el producto esta en ese documento
                DocumentLine docLine = new DocumentLine
                {
                    Document = document,
                    Product = label.Product,
                    LineStatus = new Status { StatusID = DocStatus.New },
                    Unit = label.Unit,
                    Quantity = qtyToPick,
                    CreatedBy = picker.UserName
                };


                if (document.DocType.DocTypeID != SDocType.PickTicket)
                {
                    Rules.ValidateProductInDocument(docLine, true);

                    //Valida si hay saldo pendiente por procesar
                    Rules.ValidateBalanceQuantityInDocument(docLine, destNode, true, false);
                }


                //Evaluacion de tipo de source, Bin or Label
                DateTime recDate = DateTime.Now;
                //if (Rules.ValidateIsBinLabel(sourceLocation, false))
                //{
                IList<Label> tranLabel = DecreaseQtyFromBin(label, docLine, "Picking Source Track", true, storedNode);
                    try
                    {
                        recDate = (DateTime)tranLabel.Where(f => f.ReceivingDate != null).OrderBy(f => f.ReceivingDate).First().ReceivingDate;
                    }
                    catch { recDate = DateTime.Now; }
                //}

                //    //SI el ajustes es sobre un Label
                //else if (Rules.ValidateIsProductLabel(sourceLocation, false))
                //{
                //    DecreaseQtyFromLabel(sourceLocation, line, "Picking Source", true, storedNode);
                //    try { recDate = (sourceLocation.ReceivingDate == null) ? DateTime.Now : (DateTime)sourceLocation.ReceivingDate; }
                //    catch { recDate = DateTime.Now; }
                //}


                //Creando el package para ingresar la mercancia.
                    if (packLabel == null)
                        packLabel = new Label { LabelID = -1 };

                document.Location = label.Bin.Location;  //Revalidando que el location sea correcto
                try
                {
                    packLabel = GetPackageLabel(packLabel, document, picker).PackLabel;
                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    throw new Exception("Package label could not be created.\n" + ex.Message);
                }


                //Increasing the Record of Product on Dest Bin.
                //Oct 09 /2009 Se adiciona el track option.
                Label pickedLabel = IncreaseQtyIntoBin(docLine, destNode, destBin, "Picking Dest Track", true, 
                    recDate, label.TrackOptions, label);

                pickedLabel.FatherLabel = packLabel;
                pickedLabel.Status = locked;
                pickedLabel.ShippingDocument = document;
                Factory.DaoLabel().Update(pickedLabel);

                Factory.Commit();
                return pickedLabel;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("PickProductWithTrack:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }



        public Label PickUniqueLabel(Document document, Node node, Product product, string serialLabel, SysUser picker, Label packLabel)
        {            
            //Valida si el label piquedo esta disponible y pertenece al producto y luego lo manda a PickLabel
            Label label = new Label
            {
                LabelCode = serialLabel,
                //LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
                LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                Product = new Product { ProductID = product.ProductID }
            };           


            try
            {
                label = Factory.DaoLabel().Select(label).First();
            }
            catch { throw new Exception("Barcode " + serialLabel + " does not exists for product " + product.FullDesc + "."); }


            return PickLabel(document, label, node, packLabel, picker, null);

        }


        public void UnPickUniqueLabel(Document document, Label label, SysUser picker)
        {
            Node storedNode = WType.GetNode(new Node{ NodeID = NodeType.Stored});
            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            IList<NodeTrace> nodeList = Factory.DaoNodeTrace().Select(
                new NodeTrace { 
                    Document = document, Label = label, Node = label.Node 
                });

            if (nodeList == null || nodeList.Count == 0)
                return;

            ReverseNodeTrace(nodeList, picker.UserName, storedNode, label.LastBin, status);
        }


        //Hecho para maxiforce, manejo de Caterpillar MAY/12/2010
        public void ConfirmPicking(Document document, string user)
        {
            //Revisar que el ajuste no se ha enviado antes.
            if (document.Reference == "CAT")
                throw new Exception("This document was confirmed before.");

            //Revisar la order y detectar si tiene componentes o kits de caterpillar en su balance.
            IList<DocumentBalance> list = Factory.DaoDocumentBalance().PostingBalance( //PostingBalance
                new DocumentBalance
                {
                    Document = document,
                    Node = new Node { NodeID = NodeType.Picked },
                    Location = document.Location
                });

            if (list == null || list.Count == 0)
                return;

            Product parentKit;
            IList<DocumentLine> adjLines = new List<DocumentLine>();
            DocumentLine addLine;
            int lineSeq = 1;

            IList<DocumentLine> docLines = Factory.DaoDocumentLine().Select(new DocumentLine { Note = "1", 
                Document = new Document { DocID = document.DocID }});

            foreach (DocumentBalance r in list)
            {

                //revisa que ese producto este manejado como componente Notes = "1" en las lineas.
                //if (!docLines.Any(f => f.Product.ProductID == r.Product.ProductID && f.AccountItem == ExplodeKit.Caterpillar.ToString()))
                if (!docLines.Any(f => f.Product.ProductID == r.Product.ProductID && f.AccountItem == ExplodeKit.Caterpillar.ToString()))
                    continue;

               
                try
                {

                    //revisa si es un componente caterpillar
                    parentKit = Factory.DaoKitAssemblyFormula().Select(
                        new KitAssemblyFormula { Component = r.Product })
                        .Select(f => f.KitAssembly.Product).Where(f => f.Category.ExplodeKit == ExplodeKit.Caterpillar)
                        .First();


                    foreach (DocumentLine zLine in docLines.Where(f => f.Product.ProductID == r.Product.ProductID && f.AccountItem == ExplodeKit.Caterpillar.ToString()))
                    {

                        //Creando la linea del Ajuste Negativo COMPONENTE
                        adjLines.Add(new DocumentLine
                         {
                             Product = r.Product,
                             Quantity = zLine.Quantity - zLine.QtyCancel - zLine.QtyBackOrder, //r.QtyPending,
                             CreationDate = DateTime.Now,
                             CreatedBy = user,
                             Unit = r.Product.BaseUnit,
                             LineStatus = new Status { StatusID = DocStatus.New },
                             IsDebit = true,
                             UnitBaseFactor = r.Product.BaseUnit.BaseAmount,
                             Location = document.Location,
                             Date1 = DateTime.Now,
                             LineNumber = lineSeq++
                         });


                        //Creando la linea del Ajuste POsitivo KIT
                        adjLines.Add(new DocumentLine
                        {
                            Product = parentKit,
                            Quantity = zLine.Quantity - zLine.QtyCancel - zLine.QtyBackOrder, //r.QtyPending,
                            CreationDate = DateTime.Now,
                            CreatedBy = user,
                            Unit = parentKit.BaseUnit,
                            LineStatus = new Status { StatusID = DocStatus.New },
                            IsDebit = false,
                            UnitBaseFactor = parentKit.BaseUnit.BaseAmount,
                            Location = document.Location,
                            Date1 = DateTime.Now,
                            LineNumber = lineSeq++
                        });

                    }

                }
                catch { continue; }
            }


            if (adjLines.Count == 0)
                return;


            Factory.IsTransactional = true;

            try
            {
                //Crear Ajustes de inventario positivos en un solo ajuste.
                Document adjustment = new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                    CreatedBy = user,
                    Location = document.Location,
                    Company = document.Company,
                    IsFromErp = false,
                    CrossDocking = false,
                    Comment = "Caterpillar Replace " + document.DocNumber + ", " + user,
                    Date1 = DateTime.Now,
                    CustPONumber = document.DocNumber,
                    Notes = (string.IsNullOrEmpty(document.Location.ErpCode) ? "" : document.Location.ErpCode) + " CAT REPLACE"
                };

                adjustment = DocMngr.CreateNewDocument(adjustment, false);

               foreach (DocumentLine l in adjLines) {
                   l.Document = adjustment;
                   adjustment.DocumentLines.Add(l);
               }

               Factory.DaoDocument().Update(adjustment);

               (new ErpDataMngr()).CreateInventoryAdjustment(adjustment, false);
               

               Factory.Commit();

            
               //Actualiza un dato en el documento qu eindica que ya el ajuste se envio
               Document doc = Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();
               doc.Reference = "CAT";
               Factory.DaoDocument().Update(doc);

                
            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ConfirmPicking:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }



        #endregion


    }
}
