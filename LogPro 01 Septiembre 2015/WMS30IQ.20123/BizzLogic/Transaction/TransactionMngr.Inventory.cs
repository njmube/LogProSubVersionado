using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Profile;
using Entities.General;
using Entities.Master;
using Integrator.Dao;
//using System.Transactions;
using Integrator;
//using System.ServiceModel;
using Entities;
using System.Data;
using Entities.Process;
using System.Linq;

namespace BizzLogic.Logic
{
    public partial class TransactionMngr : BasicMngr
    {
        //Local Properties
        private WmsTypes WType { get; set; }
        private LabelMngr LblMngr { get; set; }
        private DocumentMngr DocMngr { get; set; }
        private ErpDataMngr ErpMngr { get; set; }
        private Rules Rules { get; set; }

        //Constructor
        public TransactionMngr()
        {
            Factory = new DaoFactory();

            WType = new WmsTypes(Factory);
            LblMngr = new LabelMngr();
            DocMngr = new DocumentMngr();
            ErpMngr = new ErpDataMngr();
            Rules = new Rules(Factory);
        }



        #region Inventory Transactions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">Document Line que guarda la informacion de la transaccion</param>
        /// <param name="label">Label involucrado en la transaccion. Puede ser de ubicacion o de producto</param>
        /// <returns></returns>
        /// 
        /*
        public DocumentLine SaveAdjustmentTransaction(DocumentLine data, Label ubicationLabel, bool commitTransaction)
        {

            Rules.ValidateBinStatus(ubicationLabel.Bin, true);

            if (ubicationLabel.LabelType.DocTypeID == LabelType.ProductLabel)
                Rules.ValidateLabelIsActive(ubicationLabel, true);


            if (commitTransaction)
                Factory.IsTransactional = true;

            //Los ajustyes solo pueden salir de producto Almacenado (Stored)
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored }); //Stored

            try
            {

                //Caso de Ajuste negativo
                if (data.IsDebit == true)
                    PerformNegativeAdjustment(data, ubicationLabel, storedNode);

                //Caso ajuste positivo
                else
                    PerformPositiveAdjustment(data, ubicationLabel, storedNode);


                //Salva la linea del documento
                data.CreationDate = DateTime.Now;
                data.BinAffected = ubicationLabel.LabelCode;
                data = Factory.DaoDocumentLine().Save(data);


                if (commitTransaction)
                    Factory.Commit();

                data.Note = "Adjust OK.";

                return data;
            }

            catch (Exception ex)
            {

                if (commitTransaction)
                    Factory.Rollback();

                ExceptionMngr.WriteEvent("SaveAdjustmentTransaction:Line:" + data.LineID, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);

                data.Note = WriteLog.GetTechMessage(ex);
                return data;

            }
        }
        */


        public DocumentLine SaveAdjustmentTransaction(DocumentLine data, Label ubicationLabel,
    bool commitTransaction)
        {

            Rules.ValidateBinStatus(ubicationLabel.Bin, true);

            if (ubicationLabel.LabelType.DocTypeID == LabelType.ProductLabel)
                Rules.ValidateLabelIsActive(ubicationLabel, true);


            if (commitTransaction)
                Factory.IsTransactional = true;

            //Los ajustyes solo pueden salir de producto Almacenado (Stored)
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored }); //Stored

            try
            {

                if (data.Product.ProductTrack.Any(f => f.TrackOption.IsUnique == true) && !string.IsNullOrEmpty(data.AccountItem))
                {
                    //Si es positivo se crea el Track
                    if (data.IsDebit == true)
                    { //Negativo
                        Label lbl = EmptyUniqueTrackLabel(data.Product, data.AccountItem, data.CreatedBy);

                        //Create el Nodetrace.
                        SaveNodeTrace(new NodeTrace
                        {
                            Node = new Node { NodeID = NodeType.Stored },
                            Document = data.Document,
                            Label = lbl,
                            Quantity = 1,
                            IsDebit = true,
                            CreatedBy = data.CreatedBy,
                            Comment = "Serialized (-) Adjustment " + data.Document.DocNumber,
                            Bin = ubicationLabel.Bin,
                            CreationDate = DateTime.Now,
                            Status = new Status { StatusID = EntityStatus.Active },
                            Unit = lbl.Unit,
                            FatherLabel = lbl.FatherLabel
                        });

                    }
                    else
                    { //Si es negativo se ajusta el label. a cero y se bloquea. {
                        Label lblp = CreateUniqueTrackLabel(data.Product, data.AccountItem, ubicationLabel, data.CreatedBy);

                        //Create el Nodetrace.
                        SaveNodeTrace(new NodeTrace
                        {
                            Node = new Node { NodeID = NodeType.Stored },
                            Document = data.Document,
                            Label = lblp,
                            Quantity = 1,
                            IsDebit = false,
                            CreatedBy = data.CreatedBy,
                            Comment = "Serialized (+) Adjustment " + data.Document.DocNumber,
                            Bin = lblp.Bin,
                            CreationDate = DateTime.Now,
                            Status = new Status { StatusID = EntityStatus.Active },
                            Unit = lblp.Unit,
                            FatherLabel = lblp.FatherLabel
                        });
                    }
                }
                //Caso de Ajuste negativo
                else if (data.IsDebit == true)
                    PerformNegativeAdjustment(data, ubicationLabel, storedNode);

                //Caso ajuste positivo
                else
                    PerformPositiveAdjustment(data, ubicationLabel, storedNode);


                //Salva la linea del documento
                data.CreationDate = DateTime.Now;
                data.BinAffected = ubicationLabel.LabelCode;
                data = Factory.DaoDocumentLine().Save(data);


                if (commitTransaction)
                    Factory.Commit();

                data.Note = "Adjust OK.";

                return data;
            }

            catch (Exception ex)
            {

                if (commitTransaction)
                    Factory.Rollback();

                ExceptionMngr.WriteEvent("SaveAdjustmentTransaction:Line:" + data.LineID, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);

                data.Note = WriteLog.GetTechMessage(ex);
                return data;

            }
        }



        private void PerformPositiveAdjustment(DocumentLine data, Label ubicationLabel, Node storedNode)
        {
            //Validar Product Restriction
            Rules.ValidateRestrictedProductInBin(data.Product, ubicationLabel.Bin, true);

            if (Rules.ValidateIsBinLabel(ubicationLabel, false))
                IncreaseQtyIntoBin(data, storedNode, ubicationLabel.Bin, "Positive Adjustment Bin", true,
                    DateTime.Now, null, null);

            else if (Rules.ValidateIsProductLabel(ubicationLabel, false))
                IncreaseQtyIntoLabel(ubicationLabel, data, "Positive Adjustment Label", true, storedNode);

        }




        private void PerformNegativeAdjustment(DocumentLine data, Label ubicationLabel, Node storedNode)
        {

            //SI el ajustes es sobre un Label
            if (Rules.ValidateIsProductLabel(ubicationLabel, false))
                DecreaseQtyFromLabel(ubicationLabel, data, "Negative Adjustment Label", true, storedNode, true);

            else if (Rules.ValidateIsBinLabel(ubicationLabel, false))
                DecreaseQtyFromBin(ubicationLabel, data, "Negative Adjustment Bin", true, storedNode);

        }



        //Verifica si el ajuste tiene todas las condiciones para darse sino devuelve false
        public Boolean CheckAdjustmentLine(DocumentLine data, Label ubicationLabel)
        {

            Rules.ValidateBinStatus(ubicationLabel.Bin, true);

            if (ubicationLabel.LabelType.DocTypeID == LabelType.ProductLabel)
                Rules.ValidateLabelIsActive(ubicationLabel, true);

            //Caso de Ajuste negativo
            if (data.IsDebit == true)
            {

                //LABEL
                if (Rules.ValidateIsProductLabel(ubicationLabel, false) && ubicationLabel.BaseCurrQty < data.Quantity * data.Unit.BaseAmount)
                {
                    Factory.Rollback();
                    throw new Exception("No quantity available for the transaction.\n" +"Product: " + data.Product.FullDesc
                        + ", Qty: " + data.Quantity + " " + data.Unit.Name + ", Bin/Label: " + ubicationLabel.LabelCode);
                }


                //BIN
                if (Rules.ValidateIsBinLabel(ubicationLabel, false))
                {
                    //Saca las cantidades para es BIN y de ese producto.
                    IList<Label> labelList = GetRetailLabels(ubicationLabel, data);

                    if (labelList.Sum(f => f.BaseCurrQty) < data.Quantity * data.Unit.BaseAmount)
                    {
                        Factory.Rollback();
                        throw new Exception("No quantity available for the transaction.\n" + "Product: " + data.Product.FullDesc
                        + ", Qty: " + data.Quantity + " " + data.Unit.Name + ", Bin/Label: " + ubicationLabel.LabelCode);
                    }
                }

            }

            Factory.Commit();
            return true;

        }



        public DocumentLine ChangeProductUbication(Label labelSource, DocumentLine changeLine, Label labelDest,
            string appPath)
        {


            bool destIsLabel = false;


            try
            {
                //Evalua si los labels son iguales
                if (labelSource.LabelID == labelDest.LabelID)
                {
                    //Factory.Rollback();
                    throw new Exception("Source and destination are the same.");
                }

                //Valida si los Bines estas Activos
                Rules.ValidateBinStatus(labelSource.Bin, true);

                Rules.ValidateBinStatus(labelDest.Bin, true);


                //Valida que el label origen este activo
                Rules.ValidateLabelIsActive(labelSource, true);


                //Valida que origen y destino sean de la misma location
                if (GetCompanyOption(changeLine.Product.Company, "WITHERPIN").Equals("T"))
                    Rules.ValidateSameLocation(labelSource, labelDest, true);


                //Valida que el Origen este en el nodo Correcto.
                //Valida que el label este en el nodo stored

                bool AllowToMoveInReceipt = GetCompanyOption(changeLine.Product.Company, "MOVINRECV").Equals("T");

                if (!Rules.ValidateNodeInLabel(labelSource, new Node { NodeID = NodeType.Stored }, false))
                {
                    //Si Permite MOVER en RECIBO
                    if (AllowToMoveInReceipt)
                        if (!Rules.ValidateNodeInLabel(labelSource, new Node { NodeID = NodeType.Received }, false))
                            throw new Exception("Label is not Stored or Received.\nPlease check if the receipt was created correctly.");
                        else
                            throw new Exception("Label is not Stored.\nPlease check if the receipt was created correctly.");
                }

                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(changeLine.Product, labelDest.Bin, true);

                if (Rules.ValidateIsUniqueLabel(labelSource, false))
                    throw new Exception("Product is serialized can not be moved by unit.");


                Rules.ValidateIsUbicationLabel(labelDest, true);

                //Cuando el Destino es un Label Logistico hay validaciones adicionales
                if (Rules.ValidateIsProductLabel(labelDest, false)) //LABEL
                {

                    //Valida el Label de destino si es un product Label
                    if (!Rules.ValidateNodeInLabel(labelDest, new Node { NodeID = NodeType.Stored }, false))
                    {
                        //Si Permite MOVER en RECIBO
                        if (AllowToMoveInReceipt)
                            if (!Rules.ValidateNodeInLabel(labelDest, new Node { NodeID = NodeType.Received }, false))
                                throw new Exception("Label is not Stored or Received.\nPlease check if the receipt was created correctly.");
                            else
                                throw new Exception("Label is not Stored.\nPlease check if the receipt was created correctly.");
                    }
                    
                    //Valida que el label este activo
                    Rules.ValidateLabelIsActive(labelDest, true);

                    //Valida si el label es empty y lo activa y le pone el producto que asociara.
                    if (labelDest.Node.NodeID == NodeType.PreLabeled)
                    {
                        labelDest.Product = (labelDest.Product == null) ? changeLine.Product : labelDest.Product;
                        labelDest.Node = Factory.DaoNode().Select(new Node { NodeID = NodeType.Stored }).First();
                        labelDest.ModifiedBy = WmsSetupValues.SystemUser;
                        labelDest.ModDate = DateTime.Now;

                        Factory.DaoLabel().Update(labelDest);
                    }

                    Rules.ValidateSameProduct(labelDest.Product, changeLine.Product, true);
                    destIsLabel = true;
                }


                Factory.IsTransactional = true;



                //Adicion de cantidades al destino
                IList<Label> labelList = new List<Label>();
                DateTime recDate = DateTime.Now;

                //Segun el destino la operacion es diferente
                if (destIsLabel)
                    IncreaseQtyIntoLabel(labelDest, changeLine, "Moved from " + labelSource.LabelCode + " to " + labelDest.LabelCode, true, labelDest.Node);

                else
                { //si el destino es un Bin, Crea el producto Suelto 
                    Label incLabel = IncreaseQtyIntoBin(changeLine, labelDest.Node, labelDest.Bin, "Moved from " + labelSource.LabelCode + " to " + labelDest.LabelCode,
                        true, recDate, null, null);

                    labelList.Add(incLabel);
                }

                //Validacion del Origen
                //Si es un Bin Revisa el producto suelto del Bin
                if (labelSource.LabelType.DocTypeID == LabelType.BinLocation)
                {
                    IList<Label> tranLabel = DecreaseQtyFromBin(labelSource, changeLine, "", false, null);
                    try { recDate = (DateTime)tranLabel.Where(f => f.ReceivingDate != null).OrderBy(f => f.ReceivingDate).First().ReceivingDate; }
                    catch { recDate = DateTime.Now; }
                }
                else
                { //Si es un label revisa la cantidad del label
                    DecreaseQtyFromLabel(labelSource, changeLine, "", false, null, true);
                    try { recDate = (labelSource.ReceivingDate == null) ? DateTime.Now : (DateTime)labelSource.ReceivingDate; }
                    catch { recDate = DateTime.Now; }
                }


                //Adiciona la Linea al Track de los documentos de Cmabio de Ubicacion Para ese Usuario
                changeLine.BinAffected = labelSource.LabelCode + " to " + labelDest.LabelCode;
                changeLine.IsDebit = false;
                changeLine.LineStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
                changeLine.CreationDate = DateTime.Now;
                changeLine.UnitBaseFactor = changeLine.Unit.BaseAmount;
                changeLine.Location = labelDest.Bin.Location;

                Factory.Commit();

                //Lleva el log de las transacciones de change location en un documento por usuario
                try { AddChangeLineToChangeHistoryDocument(changeLine); }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("Change History", ListValues.EventType.Warn, ex, null, ListValues.ErrorCategory.Business);
                }

                Factory.Commit();


                //Evalua si ese bin tiene algun proceso para evaluar y lo ejecuta. Ejemplo Damage
                if (Rules.ValidateIsProcessBin(labelDest, false))
                    //Evalua Procesos.
                    (new ProcessMngr()).EvaluateCustomProcess(labelDest.Bin, labelList, changeLine, appPath, labelSource);


                changeLine.Note = "Change OK!";
                return changeLine;


            }

            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ChangeProductUbication:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);

                changeLine.Note = WriteLog.GetTechMessage(ex);
                return changeLine;

            }
        }



        private void AddChangeLineToChangeHistoryDocument(DocumentLine changeLine)
        {
            //Obtener el Documento De Historia del Mes Actual para el Usuario.
            Document changeDoc = null;
            int lineNumber = 1;
            try
            {
                //Mira Si existe
                changeDoc = Factory.DaoDocument().Select(
                        new Document
                        {
                            Company = changeLine.Location.Company,
                            DocNumber = DateTime.Today.ToString("yyyyMM") + "_" + changeLine.CreatedBy
                        }
                    ).First();

                lineNumber = Factory.DaoDocumentLine().Select(new DocumentLine { Document = changeDoc }).Count + 1;
            }
            catch
            {
                //Si no lo Crea
                changeDoc = new Document
                 {
                     DocNumber = DateTime.Today.ToString("yyyyMM") + "_" + changeLine.CreatedBy,
                     DocType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.ChangeLocation }),
                     IsFromErp = false,
                     CrossDocking = false,
                     Date1 = DateTime.Now,
                     CreatedBy = changeLine.CreatedBy,
                     Company = changeLine.Location.Company,
                     Location = changeLine.Location
                 };

                changeDoc = DocMngr.CreateNewDocument(changeDoc, false);
            }

            //Salvar la Linea.
            changeLine.Document = changeDoc;
            changeLine.LineNumber = lineNumber;
            Factory.DaoDocumentLine().Save(changeLine);

        }



        public Label ChangeLabelUbication(Label labelSource, Label labelDest, string appPath, SysUser user)
        {

            Factory.IsTransactional = true;

            try
            {

                Rules.ValidateBinStatus(labelSource.Bin, true);

                Rules.ValidateBinStatus(labelDest.Bin, true);


                //Valida que el label este activo
                Rules.ValidateLabelIsActive(labelSource, true);

                //Valida que el label este activo
                Rules.ValidateLabelIsActive(labelDest, true);


                //Validar Product Restriction
                Rules.ValidateRestrictedProductInBin(labelSource.Product, labelDest.Bin, true);

                //Valida que sea el mismo producto en origen y destino
                Rules.ValidateIsSameProduct(labelSource, labelDest, true);


                //Valida que origen y destino sean de la misma location
                if (GetCompanyOption(labelSource.Product.Company, "WITHERPIN").Equals("T"))
                    Rules.ValidateSameLocation(labelSource, labelDest, true);

                bool AllowToMoveInReceipt = GetCompanyOption(labelSource.Product.Company,"MOVINRECV").Equals("T");

                //Valida que el label este en el nodo stored
                if (!Rules.ValidateNodeInLabel(labelSource, new Node { NodeID = NodeType.Stored }, false))
                {
                    //Si Permite MOVER en RECIBO
                    if (AllowToMoveInReceipt)
                        if (!Rules.ValidateNodeInLabel(labelSource, new Node { NodeID = NodeType.Received }, false))
                            throw new Exception("Label is not Stored or Received.\nPlease check if the receipt was created correctly.");                   
                    else
                        throw new Exception("Label is not Stored.\nPlease check if the receipt was created correctly.");
                }
                
                //Para que traiga los Hijos
                try
                {
                    labelSource = Factory.DaoLabel().Select(new Label { LabelID = labelSource.LabelID }).First();
                }
                catch { throw new Exception("Label could not be obtained."); }

                //Evalua que sea una etiqueta de Bin el detino en caso de que el target sea 1002
                if (labelSource.LabelType.DocTypeID == LabelType.ProductLabel)
                    Rules.ValidateIsBinLabel(labelDest, true);

                //Valida que el dato ingresado de origen sea un Label
                Rules.ValidateIsProductLabel(labelSource, true);

                //Evalua si va al mismo Bin
                if (labelDest.LabelType.DocTypeID == LabelType.BinLocation && labelSource.Bin.Equals(labelDest.Bin))
                {
                    Factory.Rollback();
                    throw new Exception("Source and destination are the same.");
                }

                //Pone NULL en el father previendo que lo movieron de un Logistic a un BIN
                //si pasa o otro logistic el father se actualiza posteriormente
                labelSource.FatherLabel = null;
                if (labelSource.LabelType.DocTypeID == LabelType.UniqueTrackLabel)
                    labelSource.FatherLabel = labelDest;


                //Valida si el label es empty y lo activa y le pone el producto que asociara.
                if (labelDest.Node.NodeID == NodeType.PreLabeled)
                {
                    labelDest.Product = (labelDest.Product == null) ? labelSource.Product : labelDest.Product;
                    labelDest.Node = WType.GetNode(new Node { NodeID = NodeType.Stored });
                    labelDest.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
                    labelDest.ModDate = DateTime.Now;
                    labelDest.ModifiedBy = WmsSetupValues.SystemUser;
                    Factory.DaoLabel().Update(labelDest);
                }




                //Si es bin o logistico hace el cambio
                //labelSource.Printed = true;  COmentariado NOv 27/09

                string comment = "Moved from " + labelSource.Bin.BinCode + " to " + labelDest.LabelCode;
                UpdateLocation(labelSource, labelDest);

                SaveNodeTrace(new NodeTrace
                {
                    Node = labelDest.Node,
                    Label = labelSource,
                    Quantity = labelSource.StockQty,
                    IsDebit = false,
                    CreatedBy = user.UserName,
                    Comment = comment
                });


                DocumentLine changeLine = new DocumentLine
                {
                    Product = labelSource.Product,
                    Unit = labelSource.Product.BaseUnit,
                    Quantity = labelSource.StockQty,
                    CreatedBy = user.UserName, //(labelSource.ModifiedBy == null) ? WmsSetupValues.SystemUser : labelSource.ModifiedBy,
                    LineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New }),
                    IsDebit = false,
                    UnitBaseFactor = labelSource.Product.BaseUnit.BaseAmount,
                    BinAffected = labelSource.LabelCode + " to " + labelDest.LabelCode,
                    Location = labelDest.Bin.Location,
                    CreationDate = DateTime.Now
                };

                //Lleva el log de las transacciones de change location en un documento por usuario
                try { AddChangeLineToChangeHistoryDocument(changeLine); }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("Change History", ListValues.EventType.Warn, ex, null, ListValues.ErrorCategory.Business);
                }

                Factory.Commit();


                //Evalua si ese bin tiene algun proceso para evaluar y lo ejecuta. Ejemplo Damage
                if (Rules.ValidateIsProcessBin(labelDest, false))
                {
                    //Evalua Procesos.
                    IList<Label> labelList = new List<Label>();
                    labelList.Add(labelSource);
                    (new ProcessMngr()).EvaluateCustomProcess(labelDest.Bin, labelList, changeLine, appPath, labelSource);
                }

                labelSource.Notes = "Change OK!";
                return labelSource;

            }

            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ChangeLabelUbication:" + labelSource.LabelCode, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);

                labelSource.Notes = WriteLog.GetTechMessage(ex);
                return labelSource;

            }

        }


        public Label ChangeLabelLocationV2(Label labelSource, Label labelDest, Document document)
        {
            Bin TmpBin = labelSource.Bin;

            UpdateLocation(labelSource, labelDest);

            //Create nodetrace to Void transaction
            //Registra el movimiento del nodo
            SaveNodeTrace(
                new NodeTrace
                {
                    Node = new Node { NodeID = NodeType.Stored },
                    Document = document.DocID == 0 ? null : document,
                    Label = labelSource,
                    Quantity = 1,
                    IsDebit = false,
                    CreatedBy = document.CreatedBy,
                    Bin = labelDest.Bin,
                    BinSource = TmpBin
                });
            return labelSource;
        }
        

        public IList<ProductStock> GetReplanishmentList(ProductStock data, Location location, short selector, bool showEmpty, string bin1, string bin2)
        {
            return Factory.DaoLabel().GetReplanishmentList(data, location, selector, showEmpty, bin1, bin2);
        }



        public Document ConfirmKitAssemblyOrder(Document document, Location location)
        {

            if (document.DocStatus.StatusID != DocStatus.New && document.DocStatus.StatusID != DocStatus.InProcess)
                throw new Exception("Document was cancelled or already confirmed.");


            Factory.IsTransactional = true;

            Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

            Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

            //Process Node // El Kit debe quear en Stored Node
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });


            //validando que si exista un Producto a Crear y que tenga cantidad
            IList<DocumentLine> documentLine = Factory.DaoDocumentLine().Select(new DocumentLine { Document = document })
                    .Where(f => f.LinkDocLineNumber == -1).ToList();

            if (documentLine == null || documentLine.Count == 0)
                throw new Exception("Kit/Assembly product not found.");


            try
            {

                //1. Update the old labels to void.
                NodeTrace pattern = new NodeTrace
                {
                    Document = document,
                    Node = new Node { NodeID = NodeType.Process },
                    PostingDocument = new Document { DocID = 0 }, //0 mean Null
                };

                IList<NodeTrace> tracelList = Factory.DaoNodeTrace().Select(pattern);

                IList<Label> ListaComponentes = new List<Label>(); //Contiene los componentes principales para asignar el label code
                IList<Label> ListaFinal = new List<Label>(); //Contiene los labels creados para modificar el label code

                foreach (NodeTrace nodeTrace in tracelList)
                {

                    nodeTrace.PostingDate = DateTime.Now;
                    nodeTrace.PostingDocument = document;
                    nodeTrace.PostingUserName = document.ModifiedBy;
                    nodeTrace.ModifiedBy = document.ModifiedBy;
                    nodeTrace.ModDate = DateTime.Now;



                    //Update Label to Void Node
                    nodeTrace.Label.Node = voidNode;
                    nodeTrace.Label.Status = inactive;
                    nodeTrace.Label.LastBin = nodeTrace.Label.Bin;
                    //nodeTrace.Label.Bin = bin;
                    nodeTrace.Label.CurrQty = 0;

                    Factory.DaoLabel().Update(nodeTrace.Label);

                    //Si el componente es principal, lo adiciono al listado para asignar el label code
                    if (nodeTrace.Label.Product.UpcCode == "10")
                        ListaComponentes.Add(nodeTrace.Label);

                    //Create nodetrace to Void transaction
                    //Registra el movimiento del nodo
                    SaveNodeTrace(
                        new NodeTrace
                        {
                            Node = voidNode,
                            Document = document,
                            Label = nodeTrace.Label,
                            Quantity = nodeTrace.Label.CurrQty,
                            IsDebit = nodeTrace.IsDebit,
                            CreatedBy = document.ModifiedBy,
                            PostingDocument = document,
                            PostingUserName = document.ModifiedBy,
                            Comment = "Decrease Kit/Assembly Component, Order# " + document.DocNumber
                        });

                }

                //2. Increase the inventory with the kit/assembly created.
                //Crear las labels del nuevo producto creado, en que BIN?

                foreach (DocumentLine docLine in documentLine)
                {

                    //Evalua si el producto tiene default BIN y lo crea en ese BIN
                    string kitAsmBin;
                    try { kitAsmBin = GetProductDefaultBinLabel(docLine.Product, location, BinType.Out_Only).Bin.BinCode; }
                    catch
                    {
                        //Bin donde se almacena el new label
                        kitAsmBin = GetCompanyOption(document.Company, "BINKITASM");
                        if (string.IsNullOrEmpty(kitAsmBin))
                            kitAsmBin = DefaultBin.MAIN;

                    }

                    Bin bin = Factory.DaoBin().Select(new Bin { BinCode = kitAsmBin, Location = location })
                        .Where(f=>f.BinCode == kitAsmBin).First();

                    //Create the new labels with new product
                    double logisticFactor = 1; //debe imprimir un label por cada Assembly.
                    ListaFinal = CreateProductLabels(null, docLine, storedNode, bin, logisticFactor, document.DocNumber,
                        "Increase Kit/Assembly Component, Order# " + document.DocNumber, DateTime.Now);

                    //Actualiza el documento con el Bin donde se creo el producto
                    docLine.BinAffected = bin.BinCode;
                    Factory.DaoDocumentLine().Update(docLine);

                    //Actualizo los labelcode con los seriales de los componentes principales
                    int control = 0;
                    foreach (Label Componente in ListaFinal)
                    {
                        Componente.LabelCode = ListaComponentes[control].LabelCode;
                        Factory.DaoLabel().Update(Componente);
                        control++;
                    }
                }

                Factory.Commit();

                //Actualizar el status del documento source a completed
                document.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
                document.Comment = "Order Completed On " + DateTime.Now;
                Factory.DaoDocument().Update(document);

                Factory.Commit();
                return document;
            }

            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ConfirmKitAssemblyOrder:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                return null;
            }
        }



        public Document CreateReplenishOrder(IList<ProductStock> lines, String user, Location location)
        {
            Document prtDoc = null;

            try
            {
                Factory.IsTransactional = true;

                //1. Crea un Documento de Cross Dock. Y los relaciona con el PO y SO.
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.ReplenishPackTask });
                DocumentTypeSequence docSeq = GetNextDocSequence(lines[0].Product.Company, docType);

                Account vendor = null;

                //Crear Document header
                prtDoc = new Document
                 {
                     DocNumber = docSeq.CodeSequence,
                     DocType = docType,
                     IsFromErp = false,
                     CrossDocking = false,
                     Date1 = DateTime.Now,
                     CreatedBy = user,
                     Company = lines[0].Product.Company,
                     Vendor = vendor,
                     Location = location
                 };

                DocMngr.CreateNewDocument(prtDoc, false);


                //2. Document Lines
                int line = 1;
                foreach (ProductStock curLine in lines)
                {
                    DocumentLine docLine = new DocumentLine
                    {
                        Document = prtDoc,
                        Product = curLine.Product,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        Unit = curLine.Unit,
                        Quantity = curLine.PackStock,
                        CreationDate = DateTime.Now,
                        IsDebit = false,
                        LineNumber = line,
                        Location = curLine.Bin.Location,
                        UnitBaseFactor = curLine.Unit.BaseAmount,
                        CreatedBy = user,
                        Note = curLine.Bin.BinCode
                    };

                    prtDoc.DocumentLines.Add(docLine);
                    line++;
                }

                Factory.DaoDocument().Update(prtDoc);
                Factory.Commit();
                return prtDoc;
            }

            catch (Exception ex)
            {
                Factory.Rollback();
                //Factory.DaoDocument().Delete(prtDoc);
                ExceptionMngr.WriteEvent("CreateReplenishOrder:Doc#" + prtDoc.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                throw;
            }
        }



        public Document ConfirmReplenishmentOrder(Document document)
        {

            if (document.DocStatus.StatusID != DocStatus.New && document.DocStatus.StatusID != DocStatus.InProcess)
                throw new Exception("Document was cancelled or already confirmed.");

            Factory.IsTransactional = true;

            Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });

            try
            {

                //1. Update the old labels to void.
                NodeTrace pattern = new NodeTrace
                {
                    Document = document,
                    Node = new Node { NodeID = NodeType.Stored },
                    PostingDocument = new Document { DocID = 0 } //0 mean Null
                };

                IList<NodeTrace> tracelList = Factory.DaoNodeTrace().Select(pattern);

                foreach (NodeTrace nodeTrace in tracelList)
                {

                    nodeTrace.PostingDate = DateTime.Now;
                    nodeTrace.PostingDocument = document;
                    nodeTrace.PostingUserName = document.ModifiedBy;
                    nodeTrace.ModifiedBy = document.ModifiedBy;
                    nodeTrace.ModDate = DateTime.Now;
                    nodeTrace.Label.Status = active;
                    nodeTrace.Node = voidNode;

                    nodeTrace.Label.ModDate = DateTime.Now;
                    nodeTrace.Label.ModifiedBy = WmsSetupValues.SystemUser;
                    Factory.DaoLabel().Update(nodeTrace.Label);


                    //Create nodetrace to Void transaction
                    //Registra el movimiento del nodo
                    if (nodeTrace.IsDebit == false)
                        SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = storedNode,
                                Document = document,
                                Label = nodeTrace.Label,
                                Quantity = nodeTrace.Label.CurrQty,
                                IsDebit = nodeTrace.IsDebit,
                                CreatedBy = document.ModifiedBy,
                                PostingDocument = document,
                                PostingUserName = document.ModifiedBy,
                                Comment = "Confirmed Replenishment, Order# " + document.DocNumber
                            });

                }


                Factory.Commit();

                //Actualizar el status del documento source a completed
                document.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Completed });
                document.Comment = "Order Completed On " + DateTime.Now;
                Factory.DaoDocument().Update(document);

                Factory.Commit();
                return document;
            }

            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("ConfirmReplenishmentOrder:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                return null;
            }
        }



        public void ConsolidateBins(Label source, Label destination, string appPath, SysUser user)
        {

            //Validate not are the same Bin
            Rules.ValidateSameLocation(source, destination, LabelType.BinLocation, true);



            Rules.ValidateBinStatus(source.Bin, true);

            Rules.ValidateBinStatus(destination.Bin, true);

            //Validate Both ar Bin Locations
            Rules.ValidateIsBinLabel(source, true);
            Rules.ValidateIsBinLabel(destination, true);


            //Recorre todo los labels activos del Bin source y les cambia la location al bin destino
            //No lo hace en Bulk, porque se debe registrar en el node trace que se realizo ese movimiento

            Label pattern = new Label
            {
                Status = new Status { StatusID = EntityStatus.Active },
                Bin = source.Bin,
                FatherLabel = new Label { LabelID = -1 }, //Null
                LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel }
            };

            IList<Label> labelList = Factory.DaoLabel().Select(pattern);

            if (labelList == null || labelList.Count == 0)
                return;

            //Le pone else comentario para el facil rastreo
            destination.Notes = "Bin " + source.Bin.BinCode + " to Bin " + destination.Bin.BinCode + " consolidation";
            foreach (Label label in labelList)
                ChangeLabelUbication(label, destination, appPath, user);

        }


        #endregion




        //Ejecuta el cambio de Bin de manera recursiva para un label
        public void UpdateLocation(Label label, Label labelDestination)
        {
            label.ModDate = DateTime.Now;
            label.ModifiedBy = WmsSetupValues.SystemUser;
            label.LastBin = label.Bin;
            label.Bin = labelDestination.Bin;
            label.Node = labelDestination.Node;

            //JM 20nov08 Se elimina para poder tener elementos anidados
            //if (labelDestination.IsLogistic == true)
            //label.FatherLabel = labelDestination;

            Factory.DaoLabel().Update(label);


            IList<Label> childs = Factory.DaoLabel().Select(new Label { FatherLabel = label });

            if (childs != null && childs.Count > 0)
                foreach (Label lbl in childs)
                {
                    lbl.Status = label.Status;
                    UpdateLocation(lbl, labelDestination);
                }
        }



        //Obtiene una lista de labels basado en una patron y una cantidad , activos y de tipo producto.
        //Y producto suelto - NO ETIQUETADO.
        private IList<Label> GetQuantityOfLabels(Label labelPattern, DocumentLine searchLine)
        {

            //Cuenta el producto necesario a mover al nuevo Bin, si no alcanza tira exception
            Label searchLabel = new Label();
            searchLabel.Node = labelPattern.Node;

            if (Rules.ValidateIsBinLabel(labelPattern, false))
            {
                searchLabel.Bin = labelPattern.Bin;
                searchLabel.FatherLabel = new Label(); //Solo puede sacar producto suelto no contenido en unidad logistica
            }
            else if (Rules.ValidateIsProductLabel(labelPattern, false))
                searchLabel.FatherLabel = labelPattern;

            searchLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });
            searchLabel.Product = searchLine.Product;
            searchLabel.Unit = searchLine.Unit;
            searchLabel.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            searchLabel.Printed = false; //Printed in false indica etiqutas virtuales
            searchLabel.IsLogistic = false;


            //Llamar los labels a afectar
            return Factory.DaoLabel().Select(searchLabel).Take(int.Parse(searchLine.Quantity.ToString())).ToList<Label>();

        }



        //Obtienene el listado de labels d eun BIN especifico.
        private IList<Label> GetRetailLabels(Label labelPattern, DocumentLine searchLine)
        {
            //Cuenta el producto necesario a mover al nuevo Bin, si no alcanza tira exception
            Label searchLabel = new Label();
            searchLabel.Node = labelPattern.Node;
            searchLabel.Bin = labelPattern.Bin;
            searchLabel.Product = searchLine.Product;
            searchLabel.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            // CAA [2010/05/28]
            // Validacion adjustments x serial#  (AccountItem guarda el Serial para estos casos) 
            if (searchLine.AccountItem != null && searchLine.AccountItem.Equals("") && !searchLine.AccountItem.Equals("-"))
            {
                searchLabel.LabelCode = searchLine.AccountItem;
                searchLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.UniqueTrackLabel });
                searchLabel.Printed = true; 
            }
            else
            {
                searchLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });
                searchLabel.Printed = false; //Printed in false indica etiqutas virtuales
            }

            //Track Options -Adicionando para que se busque los TrackOptions
            if (labelPattern.TrackOptions != null && labelPattern.TrackOptions.Count > 0)
                searchLabel.TrackOptions = labelPattern.TrackOptions;

            //Llamar los labels a afectar
            return Factory.DaoLabel().Select(searchLabel).OrderBy(f => f.ReceivingDate).ToList<Label>();
        }



        public IList<Label> DecreaseQtyFromBin(Label fromLabel, DocumentLine line, string comment, bool saveTrace, Node node)
        {


            //Saca las cantidades para es BIN y de ese producto.
            IList<Label> labelList = GetRetailLabels(fromLabel, line);

            if (labelList.Sum(f => f.BaseCurrQty) < line.Quantity * line.Unit.BaseAmount)
            {
                Factory.Rollback();
                throw new Exception("No quantity available for the transaction.\n" + "Product: " + line.Product.FullDesc
                        + ", Qty: " + line.Quantity + " " + line.Unit.Name +", Bin/Label: " + fromLabel.LabelCode);
            }


            Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

            //Recorre los labels hasta que termine el saldo y se salga.
            double qtyBalance = line.Quantity * line.Unit.BaseAmount; //Ene 21

            double curQty;
            foreach (Label label in labelList)
            {

                if (qtyBalance <= 0)
                    break;

                //Cantidad a Disminuir
                curQty = qtyBalance > label.BaseCurrQty ? label.BaseCurrQty : qtyBalance;

                qtyBalance -= curQty;

                //Toda transaccion de este tipo convierte el label en Base Amount para poderlo procesar.
                label.StartQty = label.StartQty * label.Unit.BaseAmount;
                label.CurrQty = label.CurrQty * label.Unit.BaseAmount;

                label.Unit = label.Product.BaseUnit;
                label.CurrQty -= curQty;

                if (label.CurrQty <= 0)
                    label.Status = inactive;

                // CAA [2010/05/28]
                // Si tiene serial, al removerse se le actualiza
                if (label.LabelType.DocTypeID == LabelType.UniqueTrackLabel && !label.LabelCode.Equals(""))
                    label.LabelCode = "VOID_" + label.LabelCode;
                // EmptyUniqueTrackLabel(label.Product, label.LabelCode, WmsSetupValues.AdminUser);

                label.ModDate = DateTime.Now;
                label.ModifiedBy = line.CreatedBy;
                Factory.DaoLabel().Update(label);



                if (saveTrace)
                {
                    SaveNodeTrace(new NodeTrace
                    {
                        Node = fromLabel.Node,
                        Document = line.Document,
                        Label = label,
                        Quantity = curQty,
                        IsDebit = true,
                        Comment = comment,
                        CreationDate = DateTime.Now,
                        CreatedBy = line.CreatedBy,
                        Unit = label.Unit

                    });
                }


            }

            return labelList;

        }



        public void DecreaseQtyFromLabel(Label fromLabel, DocumentLine line, string comment,
            bool saveTrace, Node node, bool passToPrint)
        {

            //Revisa si el saldo del label es mayor a lo que solicita el ajuste
            if (fromLabel.BaseCurrQty < line.Quantity * line.Unit.BaseAmount)
            {
                Factory.Rollback();
                throw new Exception("No quantity available for the transaction.\n" + "Product: " + line.Product.FullDesc
                        + ", Qty: " + line.Quantity + " " + line.Unit.Name + ", Bin/Label: " + fromLabel.LabelCode);
            }


            if (fromLabel.Unit.BaseAmount > 1)
            {
                fromLabel.CurrQty = fromLabel.CurrQty * fromLabel.Unit.BaseAmount;
                fromLabel.StartQty = fromLabel.StartQty * fromLabel.Unit.BaseAmount;
                fromLabel.Unit = fromLabel.Product.BaseUnit;
            }

            fromLabel.CurrQty -= line.Quantity * line.Unit.BaseAmount;
            fromLabel.ModDate = DateTime.Now;
            fromLabel.ModifiedBy = line.CreatedBy;

            if (passToPrint)
                fromLabel.Printed = true;

            if (fromLabel.CurrQty <= 0)
                fromLabel.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });


            Factory.DaoLabel().Update(fromLabel);

            if (saveTrace)
            {
                SaveNodeTrace(new NodeTrace
                {
                    Node = fromLabel.Node,
                    Document = line.Document,
                    Label = fromLabel,
                    Quantity = line.Quantity * line.Unit.BaseAmount,
                    IsDebit = true,
                    Comment = comment,
                    CreationDate = DateTime.Now,
                    CreatedBy = line.CreatedBy

                });
            }

        }


        /// <summary>
        /// Aumenta el stock d eun Bin Determinado
        /// </summary>
        /// <param name="line">Product, Unit, Qty a ajustar</param>
        /// <param name="node"></param>
        /// <param name="destBin"></param>
        /// <param name="comment"></param>
        /// <param name="saveTrace"></param>
        /// <returns></returns>
        public Label IncreaseQtyIntoBin(DocumentLine line, Node node, Bin destBin, string comment,
            bool saveTrace, DateTime recDate, IList<LabelTrackOption> trackOptions, Label sourceLabel)
        {


            bool isTransactional = Factory.IsTransactional;

            if (!isTransactional)
                Factory.IsTransactional = true;


            Label tmpLabel = null;
            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active }); //Active

            DocumentType lblType = new DocumentType { DocTypeID = LabelType.ProductLabel };
            //DocumentTypeSequence initSequence = DocMngr.GetNextDocSequence(destBin.Location.Company, lblType); //Funcion para obtener siguiente Label

            //Crear el nuevo label en Node Storage

            DocumentType curType = null;
            if (line.Document != null)
                curType = Factory.DaoDocumentType().Select(line.Document.DocType).First();

            //Salvar con el nuevo status
            //El estatus es Active in printed en 0 (virtual)
            tmpLabel = new Label();

            //To Send
            tmpLabel.Node = node;

            tmpLabel.Bin = destBin;
            tmpLabel.CurrQty = line.Quantity;
            tmpLabel.Product = line.Product;
            tmpLabel.StartQty = line.Quantity;
            tmpLabel.Unit = line.Unit;
            tmpLabel.CreatedBy = line.CreatedBy;

            tmpLabel.Status = status;
            tmpLabel.CreationDate = DateTime.Now;
            tmpLabel.ReceivingDate = (recDate == null) ? DateTime.Now : recDate;
            tmpLabel.IsLogistic = false;

            // CAA [2010/05/28]
            // Validacion adjustments x serial#  (AccountItem guarda el Serial para estos casos) 
            if (line.AccountItem != null && !string.IsNullOrEmpty(line.AccountItem) && !line.AccountItem.Equals("-"))
            {
                tmpLabel.LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel };
                tmpLabel.LabelCode = line.AccountItem;
                tmpLabel.Printed = true;
            }
            else
            {
                tmpLabel.LabelType = lblType;
                tmpLabel.LabelCode = ""; // initSequence.NumSequence.ToString() + GetRandomHex(line.CreatedBy, initSequence.NumSequence);
                tmpLabel.Printed = false;
            }



            if (curType != null)
            {
                if (curType.DocClass.DocClassID == SDocClass.Receiving)
                {
                    tmpLabel.ReceivingDocument = line.Document;
                }

                else if (curType.DocClass.DocClassID == SDocClass.Shipping)
                    tmpLabel.ShippingDocument = line.Document;

            }

            //Adicion de los TRackOptions Oct09/2009
            if (trackOptions != null && trackOptions.Count > 0)
            {
                tmpLabel.TrackOptions = new List<LabelTrackOption>();
                foreach (LabelTrackOption lto in trackOptions)
                {
                    lto.Label = tmpLabel;
                    lto.RowID = 0;
                    lto.CreationDate = DateTime.Now;
                    tmpLabel.TrackOptions.Add(lto);
                }

            }


            //Save the source if come from a Label LabelTypeID = 1002
            //Para poder rastrear el label de donde se saca el producto
            if (sourceLabel != null && sourceLabel.LabelType.DocTypeID == LabelType.ProductLabel)
            {
                tmpLabel.LabelSource = sourceLabel;
                //Confirmando que el producto este OK. Si no pone el del label Source
                //Mayo 10 problema de manzo
                if (tmpLabel.Product.ProductID != sourceLabel.Product.ProductID)
                    tmpLabel.Product = sourceLabel.Product;
            }

            tmpLabel = Factory.DaoLabel().Save(tmpLabel);
            if (tmpLabel.LabelCode.Equals(""))
                tmpLabel.LabelCode = tmpLabel.LabelID.ToString();

            //Registra el movimiento en NodeTrace
            if (saveTrace)
            {
                SaveNodeTrace(new NodeTrace
                {
                    Node = node,
                    Document = line.Document,
                    Label = tmpLabel,
                    Quantity = line.Quantity,
                    IsDebit = false,
                    Comment = comment,
                    CreationDate = DateTime.Now,
                    CreatedBy = line.CreatedBy,
                    Unit = line.Unit
                });
            }


            //Ajustando la sequencia
            //initSequence.NumSequence++;
            //Factory.DaoDocumentTypeSequence().Update(initSequence);

            if (!isTransactional)
                Factory.Commit();

            return tmpLabel;

        }



        public void IncreaseQtyIntoLabel(Label ubicationLabel, DocumentLine data, string comment, bool saveTrace, Node node)
        {

            if (ubicationLabel.Unit.BaseAmount > 1)
            {
                ubicationLabel.CurrQty = ubicationLabel.CurrQty * ubicationLabel.Unit.BaseAmount;
                ubicationLabel.StartQty = ubicationLabel.StartQty * ubicationLabel.Unit.BaseAmount;
                ubicationLabel.Unit = ubicationLabel.Product.BaseUnit;
            }


            ubicationLabel.CurrQty += data.Quantity * data.Unit.BaseAmount;

            ubicationLabel.ModDate = DateTime.Now;
            ubicationLabel.ModifiedBy = data.CreatedBy;
            ubicationLabel.Printed = true;
          
            Factory.DaoLabel().Update(ubicationLabel);

            if (saveTrace)
            {
                SaveNodeTrace(new NodeTrace
                {
                    Node = node,
                    Document = data.Document,
                    Label = ubicationLabel,
                    Quantity = data.Quantity * data.Unit.BaseAmount,
                    IsDebit = false,
                    Comment = comment,
                    CreationDate = DateTime.Now,
                    CreatedBy = data.CreatedBy

                });
            }
        }


        /// <summary>
        /// Replace Part Adjustment
        /// </summary>
        /// <param name="sourceLabel"></param>
        /// <param name="product"></param>
        /// <param name="product2"></param>
        /// <param name="unit"></param>
        /// <param name="unit2"></param>
        /// <param name="qty"></param>
        /// <param name="curUser"></param>
        /// <param name="curLocation"></param>
        public string CreateReplaceAdjustmentDocument(Document curDocument, Label sourceLabel, Product product,
            Product product2, Unit unit, Unit unit2, int qty, SysUser curUser, Location curLocation)
        {

            int step = 0;
            Factory.IsTransactional = true;

            try
            {

                Rules.ValidateBinStatus(sourceLabel.Bin, true);


                //Header del Documento de Ajuste
                curDocument = DocMngr.CreateNewDocument(curDocument, false);
                step = 1; //Creo el header del documento
                DocumentLine curLine;


                //Line a de producto a remover
                curLine = new DocumentLine
                {
                    Product = product,
                    Unit = unit,
                    Quantity = qty,
                    CreatedBy = curUser.UserName,
                    LineStatus = new Status { StatusID = DocStatus.New },
                    IsDebit = true,
                    UnitBaseFactor = unit.BaseAmount,
                    BinAffected = sourceLabel.LabelCode,
                    //// Final Data
                    Document = curDocument,
                    Location = curLocation,
                    LineNumber = 1  //count++,
                };


                //Removiendo el producto Inicial
                curLine = SaveAdjustmentTransaction(curLine, sourceLabel, false);
                if (curLine.Note != "Adjust OK.")
                    throw new Exception(curLine.Note);



                curDocument.DocumentLines = new List<DocumentLine>();
                curDocument.DocumentLines.Add(curLine);

                //Linea de producto a adicionar

                curLine = new DocumentLine
                {
                    Product = product2,
                    Unit = unit2,
                    Quantity = qty,
                    CreatedBy = curUser.UserName,
                    LineStatus = new Status { StatusID = DocStatus.New },
                    IsDebit = false,
                    UnitBaseFactor = unit2.BaseAmount,

                    //// Final Data
                    Document = curDocument,
                    Location = curLocation,
                    BinAffected = sourceLabel.Bin.BinCode,
                    LineNumber = 2  //count++,
                };

                //Creando el nuevo producto
                curLine = SaveAdjustmentTransaction(curLine, sourceLabel.Bin.LabelRef[0], false);
                if (curLine.Note != "Adjust OK.")
                    throw new Exception(curLine.Note);


                curDocument.DocumentLines.Add(curLine);


                step = 2; //Se crearon las lineas

                //Si hay Conexion al ERP Envia el documento de ajuste al ERP
                if (GetCompanyOption(curLocation.Company, "WITHERPIN").Equals("T"))
                    ErpMngr.CreateInventoryAdjustment(curDocument, false);

                Factory.Commit();


                curDocument.DocStatus = new Status { StatusID = DocStatus.Completed };
                Factory.DaoDocument().Update(curDocument);


                Factory.Commit();


                return curDocument.DocNumber;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                //Factory.IsTransactional = false;
                //Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

                //if (step > 0)
                //{
                //    curDocument.DocStatus = cancelled;
                //    curDocument.Comment = "Cancelled: Device, " + ex.Message;

                //    if (curDocument.DocumentLines != null)
                //        for (int x = 0; x < curDocument.DocumentLines.Count; x++)
                //            curDocument.DocumentLines[x].LineStatus = cancelled;

                //    Factory.DaoDocument().Update(curDocument);
                //}


                throw new Exception("Document not created. " + ex.Message);
            }
        }




        public void ProcessKitAssemblyAddRemove(Label label, int operation, Product Component, int qty, SysUser user,
            long labelSourceID)
        {

            Factory.IsTransactional = true;


            Node node = WType.GetNode(new Node { NodeID = NodeType.Stored });
            Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            Status lockStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });

            //Bin donde Sucede la transaccion
            Label binLocation = Factory.DaoLabel().Select(
                        new Label { Bin = label.Bin, LabelType = new DocumentType { DocTypeID = LabelType.BinLocation } }
                    ).First();


            Rules.ValidateBinStatus(binLocation.Bin, true);

            //Documento de Ajuste
            Document curDocument = null;
            int pos = 0; //Posicion de avance del proceso



            #region Operation 1 - Remove Component


            LabelMissingComponent lblMissing = null;
            //IList<Label> labelsCreated = null;


            if (operation == 1) //Remove Component
            {
                try
                {

                    //Unit logisticUnit = null;
                    DocumentLine line = null;

                    //Crear N missing components
                    for (int i = 0; i < qty; i++)
                    {
                        lblMissing = new LabelMissingComponent
                        {
                            FatherLabel = label,
                            Component = Component,
                            CreatedBy = user.UserName,
                            CreationDate = DateTime.Now,
                            Quantity = 1,
                            Status = active
                        };

                        Factory.DaoLabelMissingComponent().Save(lblMissing);
                    }


                    #region Option 1 - Inventory Adjustment

                    //Positive Adjustment Del Componente, Negative del Kit Si es la primera vez,
                    //Si no solo positive del componente
                    // Creando el Ajuste de inventario Adiciona la parte y disminuye el kit, si es la primera pieza.

                    curDocument = new Document
                        {
                            DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                            CreatedBy = user.UserName,
                            Location = label.Bin.Location,
                            Company = Component.Company,
                            IsFromErp = false,
                            CrossDocking = false,
                            Comment = "Kit/Assembly Remove Component " + label.LabelCode + ", " + user.UserName,
                            Notes = "Extract Component " + Component.ProductCode,
                            Date1 = DateTime.Now
                        };

                    curDocument = DocMngr.CreateNewDocument(curDocument, false);

                    //Adiciona la Linea del Componente Removido.
                    line = new DocumentLine
                    {
                        Product = Component,
                        Unit = Component.BaseUnit,
                        Quantity = qty,
                        CreatedBy = user.UserName,
                        Document = curDocument,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        IsDebit = false,
                        UnitBaseFactor = Component.BaseUnit.BaseAmount,
                        BinAffected = binLocation.Bin.BinCode,
                        Location = label.Bin.Location,
                        LineNumber = 1
                    };

                    line = SaveAdjustmentTransaction(line, binLocation, false);
                    if (line.Note != "Adjust OK.")
                        throw new Exception(line.Note);



                    curDocument.DocumentLines.Add(line);


                    //Quiere decir que es la primera vez que se extrae 
                    //entonces hace el ajuste negativo del Kit
                    if (label.Status.StatusID == EntityStatus.Active)
                    {
                        DocumentLine kitAdj = new DocumentLine
                        {
                            Product = label.Product,
                            Unit = label.Product.BaseUnit,
                            Quantity = 1,
                            CreatedBy = user.UserName,
                            LineStatus = new Status { StatusID = DocStatus.New },
                            IsDebit = true,
                            UnitBaseFactor = label.Product.BaseUnit.BaseAmount,
                            BinAffected = binLocation.Bin.BinCode,
                            Location = label.Bin.Location,
                            LineNumber = 2,
                            Document = curDocument
                        };

                        kitAdj = SaveAdjustmentTransaction(kitAdj, label, false);
                        if (kitAdj.Note != "Adjust OK.")
                            throw new Exception(kitAdj.Note);


                        curDocument.DocumentLines.Add(kitAdj);

                    }

                    ErpMngr.CreateInventoryAdjustment(curDocument, false);
                    Factory.Commit();

                    #endregion

                    label.Status = lockStatus;
                    label.ModDate = DateTime.Now;
                    label.ModifiedBy = user.UserName;
                    Factory.DaoLabel().Update(label);

                    Factory.Commit();
                    return;

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    throw new Exception(ex.Message);
                }

            }


            #endregion


            if (operation == 2) //ADD component {
            {
                try
                {

                    Label sourceLabel;
                    try { sourceLabel = Factory.DaoLabel().Select(new Label { LabelID = labelSourceID }).First(); }
                    catch { throw new Exception("Source location not defined."); }

                    Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

                    //Recorrer la lista de Missing Components activos e ir los desactivando.
                    List<LabelMissingComponent> missingComps = label.MissingComponents
                        .Where(f => f.Status.StatusID == EntityStatus.Active && f.Component.ProductID == Component.ProductID)
                        .Take(qty).ToList();

                    if (missingComps == null) //Si no hya nada que ajustar siplemente sale
                        return;

                    //Ajusta los componentes missing indicando que fueron adicionados 
                    foreach (LabelMissingComponent component in missingComps)
                    {
                        component.ModifiedBy = user.UserName;
                        component.ModDate = DateTime.Now;
                        component.Status = inactive;
                        Factory.DaoLabelMissingComponent().Update(component);
                    }


                    //Si el label esta completo. Lo pone el Label en status activo.
                    if (label.MissingComponents.Where(f => f.Status.StatusID == EntityStatus.Active).Count() == 0)
                    {
                        label.Status = active;
                        label.ModDate = DateTime.Now;
                        label.ModifiedBy = user.UserName;
                        Factory.DaoLabel().Update(label);
                    }


                    //Negative Adjustment del componente y Positive Adjustment del Kit solo si el kit se completo.
                    #region Option 2 - Inventory Adjustment

                    // Creando el Ajuste de inventario Adiciona la parte y disminuye el kit, si es la primera pieza.

                    curDocument = new Document
                    {
                        DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                        CreatedBy = user.UserName,
                        Location = label.Bin.Location,
                        Company = Component.Company,
                        IsFromErp = false,
                        CrossDocking = false,
                        Comment = "Kit/Assembly Add Component " + label.LabelCode + ", " + user.UserName,
                        Notes = "Add Component " + Component.ProductCode,
                        Date1 = DateTime.Now
                    };

                    curDocument = DocMngr.CreateNewDocument(curDocument, false);

                    //Adiciona la Linea del Componente Adicionado.
                    DocumentLine addLine = new DocumentLine
                    {
                        Product = Component,
                        Quantity = qty,
                        CreatedBy = user.UserName,
                        Unit = Component.BaseUnit,
                        LineStatus = new Status { StatusID = DocStatus.New },
                        IsDebit = true,
                        UnitBaseFactor = Component.BaseUnit.BaseAmount,
                        BinAffected = binLocation.Bin.BinCode,
                        Location = label.Bin.Location,
                        LineNumber = 1,
                        Document = curDocument
                    };

                    addLine = SaveAdjustmentTransaction(addLine, sourceLabel, false);
                    if (addLine.Note != "Adjust OK.")
                        throw new Exception(addLine.Note);


                    curDocument.DocumentLines.Add(addLine);

                    //Quiere decir que es la primera vez que se extrae 
                    //entonces hace el ajuste negativo del Kit
                    if (label.Status.StatusID == EntityStatus.Active)
                    {
                        DocumentLine kitAdj = new DocumentLine
                        {
                            Product = label.Product,
                            Unit = label.Product.BaseUnit,
                            Quantity = 1,
                            CreatedBy = user.UserName,
                            LineStatus = new Status { StatusID = DocStatus.New },
                            IsDebit = false,
                            UnitBaseFactor = label.Product.BaseUnit.BaseAmount,
                            BinAffected = binLocation.Bin.BinCode,
                            Location = label.Bin.Location,
                            LineNumber = 2,
                            Document = curDocument
                        };

                        kitAdj = SaveAdjustmentTransaction(kitAdj, label, false);
                        if (kitAdj.Note != "Adjust OK.")
                            throw new Exception(kitAdj.Note);


                        curDocument.DocumentLines.Add(kitAdj);

                    }


                    ErpMngr.CreateInventoryAdjustment(curDocument, false);

                    #endregion



                    Factory.Commit();
                    return;

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    throw new Exception(ex.Message);
                }

            }

        }



        public Document EnterInspectionResultByLabel(Document document, Label label, string resultCode, string username)
        {
            try
            {
                Factory.IsTransactional = true;

                label = Factory.DaoLabel().Select(new Label { LabelID = label.LabelID }).First();

                if (label.StockQty <= 0)
                    throw new Exception("Label does not contain quantities.");


                Node processNode = WType.GetNode(new Node { NodeID = NodeType.Process });
                Bin processBin = WType.GetBin(new Bin { BinCode = DefaultBin.PROCESS, Location = label.Bin.Location });
                Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });

                if (document == null)
                {
                    //1. Crea un Documento de Cross Dock. Y los relaciona con el PO y SO.
                    DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.ProcessTask });
                    DocumentTypeSequence docSeq = GetNextDocSequence(label.Product.Company, docType);

                    //Crear Document header
                    document = new Document
                    {
                        DocNumber = docSeq.CodeSequence,
                        DocType = docType,
                        IsFromErp = false,
                        CrossDocking = false,
                        Date1 = DateTime.Now,
                        CreatedBy = username,
                        Company = label.Product.Company,
                        Location = label.Bin.Location
                    };

                    document = DocMngr.CreateNewDocument(document, false);

                }

                //Pasamos el Label al Nodo Process
                label.Node = processNode;
                label.LastBin = label.Bin;
                label.Bin = processBin;
                //Note y Lot Indican que movimiento es.
                label.Notes = resultCode;
                label.PrintingLot = document.DocNumber;
                label.Status = locked;
                label.LabelSource = label.FatherLabel;
                label.FatherLabel = null;
                Factory.DaoLabel().Update(label);


                //Creamos el Node trace para poder indentificar los movimientos
                SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = processNode,
                                Document = document,
                                Label = label,
                                Quantity = label.CurrQty,
                                IsDebit = false,
                                CreatedBy = username,
                                Comment = resultCode,

                            });


                //Mover los hijos de ese label si existen.
                try
                {
                    IList<Label> childLabels = Factory.DaoLabel().Select(new Label { FatherLabel = label });

                    if (childLabels != null && childLabels.Count > 0)
                    {
                        foreach (Label child in childLabels)
                        {
                            //Pasamos el Label al Nodo Process
                            child.Node = processNode;
                            child.LastBin = label.Bin;
                            child.Bin = processBin;
                            //Note y Lot Indican que movimiento es.
                            child.Notes = resultCode;
                            child.PrintingLot = document.DocNumber;
                            child.Status = locked;
                            Factory.DaoLabel().Update(child);
                        }
                    }
                }
                catch { }

                Factory.Commit();
                return document;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                throw new Exception(ex.Message);
            }
        }


        public Document EnterInspectionResultByProduct(Document document, Product product,
            double qtyToPick, string resultCode, Bin bin, string username)
        {
            //IDEM but with product Quantity
            try
            {
                Factory.IsTransactional = true;


                Node processNode = WType.GetNode(new Node { NodeID = NodeType.Process });
                Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
                Bin processBin = WType.GetBin(new Bin { BinCode = DefaultBin.PROCESS, Location = bin.Location });
                Status locked = WType.GetStatus(new Status { StatusID = EntityStatus.Locked });

                Label inspectionBIN = bin.LabelRef[0];

                if (document == null)
                {
                    //1. Crea un Documento de Cross Dock. Y los relaciona con el PO y SO.
                    DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.ProcessTask });
                    DocumentTypeSequence docSeq = GetNextDocSequence(product.Company, docType);

                    //Crear Document header
                    document = new Document
                    {
                        DocNumber = docSeq.CodeSequence,
                        DocType = docType,
                        IsFromErp = false,
                        CrossDocking = false,
                        Date1 = DateTime.Now,
                        CreatedBy = username,
                        Company = product.Company,
                        Location = bin.Location
                    };

                    document = DocMngr.CreateNewDocument(document, false);

                }

                //Pasamos el Label al Nodo Process
                DocumentLine line = new DocumentLine { Document = document, Product = product, CreatedBy = username, 
                    Quantity = qtyToPick, Unit = product.BaseUnit };
                DecreaseQtyFromBin(inspectionBIN, line, "", false, storedNode);

                Label label = IncreaseQtyIntoBin(line, processNode, processBin, "", false, DateTime.Now, null, null);

                label.Node = processNode;
                label.LastBin = label.Bin;
                label.Bin = processBin;
                //Note y Lot Indican que movimiento es.
                label.Notes = resultCode;
                label.PrintingLot = document.DocNumber;
                label.Status = locked;
                Factory.DaoLabel().Update(label);


                //Creamos el Node trace para poder indentificar los movimientos
                SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = processNode,
                                Document = document,
                                Label = label,
                                Quantity = label.CurrQty,
                                IsDebit = false,
                                CreatedBy = username,
                                Comment = resultCode,

                            });


                Factory.Commit();
                return document;

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                throw new Exception(ex.Message);
            }
        }



        public void ConfirmInspectionDocument(Document document, string appPath, string username)
        {
            //Obtener the Inspection Responses
            string responses = WType.GetCompanyOption(document.Company, "INSRESPO").ToString();

            Status posted = WType.GetStatus(new Status { StatusID = DocStatus.Posted });

            if (document.DocStatus.StatusID == DocStatus.Posted)
                throw new Exception("Inspection Document already completed.");


            if (string.IsNullOrEmpty(responses))
                throw new Exception("No Inspection responses defined.");

            // convert to dataset in two lines
            DataSet ds = GetDataSet(responses);
            CustomProcess process = null;
            IList<Label> processLabels = null;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (string.IsNullOrEmpty(dr[0].ToString()))
                    continue;

                //Para Cada tipo de Respuestas Ejecutar el Proceso.
                process = WType.GetCustomProcess(new CustomProcess { Name = dr[0].ToString() });

                //Obtiene los Labels Especificos de cada proceso.
                processLabels = Factory.DaoLabel().Select(
                    new Label
                    {
                        Notes = process.Name,
                        PrintingLot = document.DocNumber,
                        Node = new Node { NodeID = NodeType.Process },
                        FatherLabel = new Label { LabelID = -1 }
                    });


                if (processLabels == null || processLabels.Count == 0)
                    continue;

                //Llama la ejecucion del proceso.
                if (process.Name != "REPAIR")
                    (new ProcessMngr()).EvaluateInspectionProcess(document, process, processLabels, appPath);
                else
                {
                    Document prDocument = Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();
                    EvaluateRepair(prDocument, process, processLabels, appPath);
                }

            }

            document.DocStatus = posted;
            document.ModifiedBy = username;
            document.ModDate = DateTime.Now;
            Factory.DaoDocument().Update(document);

        }


        /// <summary>
        /// Este metodo Crea un documento por cada Vendor de la lista de labels y agrupa los labels en un solo tipo de producto
        /// </summary>
        /// <param name="document"></param>
        /// <param name="processLabels"></param>
        /// <param name="appPath"></param>
        private void EvaluateRepair(Document document, CustomProcess process, IList<Label> processLabels, string appPath)
        {
            //1. Select the list of different vedors for the document
            //IList<int> vendors = Factory.DaoCustomProcess().GetInspectionVendors(document);

            //if (vendors == null || vendors.Count == 0)
            //return;

            IList<ProductStock> stockList;

            //foreach (int vID in vendors.Distinct())
            //{
            //    document.Vendor = WType.GetAccount(new Account { AccountID = vID });

            //2. Select The product Stock data 
            document.DocumentLines = new List<DocumentLine>();

            stockList = Factory.DaoCustomProcess().GetInspectionVendorStock(document, document.Vendor);

            foreach (ProductStock ps in stockList)
            {
                //Arma el LabelList para enviar al proceso
                document.DocumentLines.Add(new DocumentLine
                {
                    Document = document,
                    Product = ps.Product,
                    Unit = ps.Product.BaseUnit,
                    Quantity = ps.Stock
                });
            }

            //Envia al proceso
            try { (new ProcessMngr()).EvaluateInspectionProcess(document, process, processLabels, appPath); }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("EvaluateRepair:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
            }

            //}

        }



        public void ResetInspectionLine(Document document, Label label, string username, bool cancelDocument)
        {
            //reversa el Label al estado de Inspection

            Node storeNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
            Bin bin = WType.GetBin(new Bin { BinCode = DefaultBin.INSPECTION, Location = label.Bin.Location });
            Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            Factory.IsTransactional = true;

            try
            {

                label.Node = storeNode;
                label.Bin = bin;
                //Note y Lot Indican que movimiento es.
                label.Notes = "";
                label.PrintingLot = "";
                label.Status = active;
                label.FatherLabel = label.LabelSource;
                Factory.DaoLabel().Update(label);



                //Creamos el Node trace para poder indentificar los movimientos
                SaveNodeTrace(
                            new NodeTrace
                            {
                                Node = storeNode,
                                Document = document,
                                Label = label,
                                Quantity = label.CurrQty,
                                IsDebit = false,
                                CreatedBy = username,
                                Comment = "Process Reverted",

                            });


                if (cancelDocument)
                {
                    document.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                    document.ModDate = DateTime.Now;
                    document.ModifiedBy = username;
                    Factory.DaoDocument().Update(document);

                }

                Factory.Commit();


            }
            catch (Exception ex)
            {
                Factory.Rollback();
                throw new Exception(ex.Message);
            }

        }

        public IList<DocumentLine> CreateAssemblyOrderLines(Document Document, Product Product, Double Quantity)
        {
            IList<DocumentLine> DocumentLineListSaved;
            DocumentLine DocumentLineSave;
            IList<KitAssemblyFormula> KitFormula;

            try
            {
                //Creo el primer registro referente al kit
                DocumentLineSave = new DocumentLine();
                DocumentLineSave.Document = Document;
                DocumentLineSave.LineNumber = 0;
                DocumentLineSave.LineStatus = WType.GetStatus(new Status { StatusID = 101 });
                DocumentLineSave.Product = Product;
                DocumentLineSave.Quantity = Quantity;
                DocumentLineSave.IsDebit = false;
                DocumentLineSave.Unit = WType.GetUnit(new Unit { UnitID = 2 });
                DocumentLineSave.UnitBaseFactor = 1;
                DocumentLineSave.Location = Document.Location;
                DocumentLineSave.Note = "2";
                DocumentLineSave.LinkDocLineNumber = -1;
                DocumentLineSave.CreatedBy = Document.CreatedBy;
                DocumentLineSave.CreationDate = Document.CreationDate;
                //Guardo el Kit en el DocumentLine
                Factory.DaoDocumentLine().Save(DocumentLineSave);

                //Obtengo la formula del Kit
                KitFormula = Factory.DaoKitAssemblyFormula().Select(new KitAssemblyFormula { KitAssembly = new KitAssembly { Product = Product } });

                //Creo el resto de registros
                int Line = 1;
                foreach (KitAssemblyFormula KitAssemblyProduct in KitFormula)
                {
                    //Creo el DocumentLine con cada KitAssemblyFormula
                    DocumentLineSave = new DocumentLine();
                    DocumentLineSave.Document = Document;
                    DocumentLineSave.LineNumber = Line;
                    DocumentLineSave.LineStatus = WType.GetStatus(new Status { StatusID = 101 });
                    DocumentLineSave.Product = KitAssemblyProduct.Component;
                    DocumentLineSave.Quantity = KitAssemblyProduct.FormulaQty * Quantity;
                    DocumentLineSave.IsDebit = false;
                    DocumentLineSave.Unit = WType.GetUnit(new Unit { UnitID = 2 });
                    DocumentLineSave.UnitBaseFactor = 1;
                    DocumentLineSave.Location = Document.Location;
                    DocumentLineSave.Note = "1";
                    DocumentLineSave.LinkDocLineNumber = 0;
                    DocumentLineSave.CreatedBy = Document.CreatedBy;
                    DocumentLineSave.CreationDate = Document.CreationDate;
                    //Guardo el DocumentLine
                    Factory.DaoDocumentLine().Save(DocumentLineSave);
                    Line++;
                }
            }
            catch { }

            //Obtengo los DocumentLines guardados
            DocumentLineListSaved = Factory.DaoDocumentLine().Select(new DocumentLine { Document = Document }).Where(f => f.LineNumber > 0).ToList();

            return DocumentLineListSaved;
        }

    }
}
