using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.General;
using Entities.Master;
using Integrator.Dao;
using Entities.Trace;
using Entities;
using Integrator;
using Entities.Profile;
using System.Data;
using Entities.Report;
using System.IO;
using System.Reflection;
using System.Xml;
using ErpConnect;
using System.Data.SqlClient;
using System.Threading;
using Entities.Process;

namespace BizzLogic.Logic
{

    public partial class BasicMngr
    {

        public DaoFactory Factory { get; set; }
        private WmsTypes Wtype { get; set; }

        public BasicMngr()
        {
            Factory = new DaoFactory();
            Wtype = new WmsTypes(Factory);
        }


        public DocumentTypeSequence GetNextDocSequence(Company company, DocumentType docType)
        {

            //crea el objeto de DocumentTypeSequence
            DocumentTypeSequence docSeq = new DocumentTypeSequence();
            docSeq.Company = company;
            docSeq.DocType = docType;

            //Solicita el Consecutivo para el documento
            docSeq = Factory.DaoDocumentTypeSequence().Select(docSeq).First();


            //Check Label Sequence 
            if (docType.DocTypeID == LabelType.ProductLabel)
            {
                long labelSeq = Factory.DaoLabel().SelectSequence();
                if (labelSeq > docSeq.NumSequence)
                    docSeq.NumSequence = labelSeq;
            }




            //Aumenta el consecutivo en Uno y lo guarda
            docSeq.NumSequence++;
            Factory.DaoDocumentTypeSequence().Update(docSeq);

            return docSeq;

        }


        public IList<Unit> GetProductUnit(Product data)
        {
            IList<UnitProductRelation> unitRel = Factory.DaoUnitProductRelation().Select(new UnitProductRelation { Product = data });

            IList<Unit> list = new List<Unit>();
            foreach (UnitProductRelation uRel in unitRel)
                list.Add(uRel.Unit);
            
            return list.OrderBy(f=>f.BaseAmount).ToList();
        }


        public void SaveNodeTrace(NodeTrace nodeTrace)
        {
            //nodeTrace.RowID = 0;
            nodeTrace.CreationDate = DateTime.Now;
            nodeTrace.CreatedBy = (nodeTrace.CreatedBy == null) ? "" : nodeTrace.CreatedBy;
            nodeTrace.FatherLabel = nodeTrace.Label.FatherLabel;

            if (nodeTrace.Unit == null)
                nodeTrace.Unit = nodeTrace.Label.Unit; //Added: Dec05/2009

            if (nodeTrace.Bin == null)
                nodeTrace.Bin = (nodeTrace.Label != null) ? nodeTrace.Label.Bin : null;
            nodeTrace.Status = new Status { StatusID = EntityStatus.Active };

            Factory.DaoNodeTrace().Save(nodeTrace);
           
        }



        //public double GetLabelStockQty(Label label)
        //{
        //    IList<Label> list = Factory.DaoLabel().Select(new Label { FatherLabel = new Label { LabelID = label.LabelID } });
        //    if (list != null && list.Count > 0)
        //        return label.CurrQty + list.Sum(f => f.CurrQty);
        //    else
        //        return label.CurrQty;

        //}


        //Get Product Biz Zone Assigned
        public IList<ZoneBinRelation> GetProductAssignedZone(Product product, Location location)
        {
            //IList<ProductStock> list = new List<ProductStock>();
            //ProductStock curLine;
            //Obtener producto que trae sus zonas related,
            product = Factory.DaoProduct().Select(product,0).FirstOrDefault();
            IList<ZoneBinRelation> list = new List<ZoneBinRelation>();


            //recorrer las zonas t devolver l product Stock
            if (product.ProductZones == null || product.ProductZones.Count == 0)
                return null;

            foreach (ZoneEntityRelation ze in product.ProductZones)
            {
                if (ze.Zone.Bins == null || ze.Zone.Bins.Where(f => f.Zone.Location.LocationID == location.LocationID).Count() == 0)
                    continue;

                foreach (ZoneBinRelation b in ze.Zone.Bins)
                {
                    //curLine = new ProductStock
                    //{
                    //    Bin = b.Bin,
                    //    Product = product,
                    //    Stock = 0,
                    //    Unit = null,
                    //    Zone = b.Zone,
                    //    BinType = b.BinType
                    //};

                    list.Add(b);

                }
            }

            return list;
        }


        //Asigna un Bin a un producto
        public void AssignBinToProduct(Product product, ZoneBinRelation zoneBin)
        {

            Zone zone;
            Factory.IsTransactional = true;

            try
            {

                //1. Crea Label zona const else nombre del producto si no existe
                IList<Zone> zoneList = Factory.DaoZone().Select(new Zone { Location = zoneBin.Bin.Location, Name = product.ProductCode });
                if (zoneList == null || zoneList.Count == 0)
                {
                    Status status = Wtype.GetStatus(new Status { StatusID = EntityStatus.Active });
                    //Create Zone 
                    zone = new Zone
                    {
                        Name = product.ProductCode,
                        CreationDate = DateTime.Now,
                        CreatedBy = zoneBin.CreatedBy,
                        Description = "Zone for Product " + product.ProductCode + ", " + product.Name,
                        Location = zoneBin.Bin.Location,
                        Status = status,
                        IsDefault = false
                    };

                    zone = Factory.DaoZone().Save(zone);

                }
                else
                    zone = zoneList.FirstOrDefault();




                //2. Crear el Zone entity relation
                ZoneEntityRelation zoneEntity = new ZoneEntityRelation
                {
                    Entity = Factory.DaoClassEntity().SelectById(new ClassEntity { ClassEntityID = EntityID.Product }),
                    EntityRowID = product.ProductID,
                    Zone = zone
                };

                //Revisa si existe la relacion Zone Entity
                IList<ZoneEntityRelation> zoneEntityList = Factory.DaoZoneEntityRelation().Select(zoneEntity);

                if (zoneEntityList == null || zoneEntityList.Count == 0)
                {
                    zoneEntity.CreationDate = DateTime.Today;
                    zoneEntity.CreatedBy = zoneBin.CreatedBy;
                    zoneEntity = Factory.DaoZoneEntityRelation().Save(zoneEntity);
                }


                //3. Crea el zone Bin Relation
                //ZoneBinRelation zoneBinRel = new ZoneBinRelation
                //{
                //    Bin = bin,
                //    Zone = zone,
                //    BinType = binDirection
                //};

                zoneBin.Zone = zone;

                //Revisa que no exista la Relacion ZoneBin
                IList<ZoneBinRelation> zoneBinList = Factory.DaoZoneBinRelation().Select(zoneBin);

                if (zoneBinList != null && zoneBinList.Count > 0)
                    throw new Exception("Bin already was assigned to the product.");

                zoneBin.CreationDate = DateTime.Today;
                zoneBin = Factory.DaoZoneBinRelation().Save(zoneBin);

                product = Factory.DaoProduct().SelectById(product);

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                throw new Exception("Error creating Zone. " + ex.Message);
            }
        }


        //Reversar/Anular una transaccion de NodeTrace - pas avoid y el label involucrado a un nodo anterior
        public void ReverseNodeTrace(IList<NodeTrace> nodes, string user, Node labelNode, Bin bin, Status newStatus)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            Factory.IsTransactional = true;

            //labelNode indica el nodo donde debe quedar el label despues de reversado.
            Node voidNode = Factory.DaoNode().SelectById(new Node { NodeID = NodeType.Voided });

            Label label;
            Bin tmpBin;
            Bin mainBin = Wtype.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = nodes[0].Bin.Location });
            IList<Label> packedLabels = new List<Label>();

            bool toOriginalBin = false;
            try
            {
                if (GetCompanyOption(nodes[0].Document.Company, "RTLSTBIN").Equals("T"))
                    toOriginalBin = true; 
            }
            catch { }

            //#si es un Shipment tiene que reverser los Labels sueltos en un pack
            #region Trace
            foreach (NodeTrace nodeTr in nodes)
            {
                try
                {
                    // actualizamos NodeTrace
                    nodeTr.ModDate = DateTime.Now;
                    nodeTr.ModifiedBy = user;
                    nodeTr.Node = voidNode;
                    nodeTr.Comment = "Reversed by User Doc# " + nodeTr.Document.DocNumber;
                    Factory.DaoNodeTrace().Update(nodeTr);
                    
                    // actualizamos el label
                    label = nodeTr.Label;
                    tmpBin = label.Bin;
                    label.Bin = null; //Reseteando lo paar que coloque un bin adecuado

                    if (label.ShippingDocument != null)
                        packedLabels.Add(label);

                    //1. Si viene Bin Tira el restock al Bin que viene
                    if (bin != null && bin.Location.LocationID == nodes[0].Bin.Location.LocationID)
                        label.Bin = bin;

                    else if (toOriginalBin && label.LastBin != null && label.LastBin.Location.LocationID == nodes[0].Bin.Location.LocationID)
                        label.Bin = label.LastBin;

                    if (label.Bin == null)
                        label.Bin = mainBin;

                    label.LastBin = tmpBin;
                    label.FatherLabel = null;

                    if (label.Node.NodeID == NodeType.Picked)
                    {
                        label.ShippingDocument = null;

                        //Adicionado Sep 29 - Vuelve al label del que partio
                        if (label.LabelType.DocTypeID == LabelType.UniqueTrackLabel)
                            label.FatherLabel = label.LabelSource;
                    }

                    else if (label.Node.NodeID == NodeType.Released)
                    {
                        label.Bin = mainBin;
                        label.ShippingDocument = null;
                        //label.Printed = false;

                        //Adicionado Sep 29 - Vuelve al label del que partio
                        if (label.LabelType.DocTypeID == LabelType.UniqueTrackLabel)
                            label.FatherLabel = label.LabelSource;

                    }
                    else if (label.Node.NodeID == NodeType.Received)
                        label.ReceivingDocument = null;

                    // CAA [2010/06/08]
                    // Liberar serial #s 
                    if ((labelNode.NodeID == NodeType.PreLabeled || label.Node.NodeID == NodeType.Received) &&
                        label.LabelType.DocTypeID == LabelType.UniqueTrackLabel)
                    {
                        label.LabelCode = "VOID_" + DateTime.Now.ToString("YMdHms") + "_" + label.LabelCode;
                    }
               
                    label.ModDate = DateTime.Now;
                    label.ModifiedBy = user;
                    label.Node = labelNode;
                    label.Status = newStatus;


                    Factory.DaoLabel().Update(label);


                    //Remueve los hijos de cada Label
                    try
                    {
                        label.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = label });
                        if (label.ChildLabels != null && label.ChildLabels.Count > 0)
                        {
                            foreach (Label lbl in label.ChildLabels)
                            {
                                // CAA [2010/06/08]
                                // Liberar serial #s 
                                if (labelNode.NodeID == NodeType.PreLabeled)
                                    lbl.LabelCode = "VOID_" + DateTime.Now.ToString("YMdHms") + "_" + lbl.LabelCode;

                                lbl.Bin = label.Bin;
                                lbl.LastBin = label.LastBin;

                                lbl.ModDate = DateTime.Now;
                                lbl.ModifiedBy = user;
                                lbl.Node = labelNode;
                                lbl.Status = newStatus;
                                Factory.DaoLabel().Update(lbl);
                            }
                        }
                    }
                    catch { }           


                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ReverseNodeTrace. Doc #" + nodeTr.Document.DocNumber + " NodeTrace: " + nodeTr.RowID.ToString(), ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    throw;
                    //return;
                }
            }
            #endregion

            //Label que no estan en el nodetrace, pero pertenecen al documento.

            Factory.Commit();


            //Remueve los labels que hayan sido empacados
            try
            {
                foreach (Label zLabel in packedLabels)
                {
                    IList<Label> chLabels = Factory.DaoLabel().Select(new Label
                    {
                        LabelSource = zLabel,
                        ShippingDocument = zLabel.ShippingDocument
                    });

                    foreach (Label tmpLabel in chLabels)
                    {
                        tmpLabel.Node = labelNode;

                        tmpLabel.Bin = zLabel.LastBin;
                        tmpLabel.Status = newStatus;
                        tmpLabel.ShippingDocument = null;
                        tmpLabel.FatherLabel = null;

                        Factory.DaoLabel().Update(tmpLabel);
                    }
                }
                
            }
            catch { }

        }



        /// <summary>
        /// Get a property from object using a key value, Es recursivi porque busca las propiedades hijas, 
        /// reemplazando el dato del mapping KEY contra el valor de la proiedad que viene en el Objeto
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetMapPropertyValue(string dataProperty, object obj)
        {
            try
            {
                string result = "";

                if (dataProperty.Equals("DateTime.Now"))
                    return DateTime.Now.ToString();

                if (dataProperty.Equals("DateTime.Today"))
                    return DateTime.Today.ToString("yyyy/MM/dd");

                //Para obener las propiedades hasta el ultimo nivel
                string[] properties = dataProperty.Split('.');

                if (properties.Length < 2)
                    return result;

                if (properties.Length > 2)
                {
                    for (int i = 1; i < properties.Length; i++)
                    {
                        result += properties[i];
                        result = i < properties.Length - 1 ? result + "." : result;
                    }

                    return GetMapPropertyValue(result, obj.GetType().GetProperty(properties[1]).GetValue(obj, null));
                }

                if (obj.GetType().GetProperty(properties[1]).GetValue(obj, null) != null)
                    return obj.GetType().GetProperty(properties[1]).GetValue(obj, null).ToString();

                return result;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// Cra nuevos labels en el sistema para determinados productos, al imprimir o al recibir pro app o device.
        /// </summary>
        /// <param name="logisticUnit"></param>
        /// <param name="data">Document Line con los datos de los labels a crear</param>
        /// <param name="node"></param>
        /// <param name="destLocation"></param>
        /// <param name="logisticFactor"></param>
        /// <param name="printingLot"></param>
        /// <returns></returns>
        public IList<Label> CreateProductLabels(Unit logisticUnit, DocumentLine data, Node node, Bin destLocation,
            double logisticFactor, string printingLot, string comment, DateTime receivingDate)
        {

            //Label Type
            DocumentType labelType = Factory.DaoDocumentType().Select(new DocumentType { DocTypeID = LabelType.ProductLabel }).First();

            //Status
            Status status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();

            bool isTransactional = Factory.IsTransactional;

            if (!isTransactional)
                Factory.IsTransactional = true;


            //Funcion para obtener siguiente Label
            //DocumentTypeSequence initSequence = GetNextDocSequence(data.Document.Company, labelType);
            //long numSeqLabel = initSequence.NumSequence - 1;

            IList<Label> labelResult = new List<Label>();


            Label fatherLabel = null;
            string notes = (data.Document != null && data.Document.DocID > 0) ? "Doc# " + data.Document.DocNumber : "";
            notes = notes + " " + data.Unit.Name;

            //double labelBalance = data.Quantity;
            int numLabels =(int)(data.Quantity/logisticFactor);

            if (numLabels <= 0)
                throw new Exception("No valid amount of labels to print.");


            for (int i = 0; i < numLabels; i++)
            {
                    fatherLabel = new Label();
                    fatherLabel.Node = node;
                    fatherLabel.Bin = destLocation;
                    fatherLabel.Product = data.Product;
                    fatherLabel.CreatedBy = data.CreatedBy;
                    fatherLabel.Status = status;
                    fatherLabel.LabelType = labelType;
                    fatherLabel.CreationDate = DateTime.Now;
                    fatherLabel.Printed = false; //string.IsNullOrEmpty(printingLot) ? false : true;


                    if (data.Product.IsUniqueTrack)
                    {
                        fatherLabel.Unit = data.Product.BaseUnit; //La unidad que tiene ese Label //logisticUnit; 
                        fatherLabel.StartQty = logisticFactor * data.Unit.BaseAmount; //*data.Unit.BaseAmount;
                        fatherLabel.CurrQty = logisticFactor * data.Unit.BaseAmount; //*data.Unit.BaseAmount;
                    }
                    else
                    {
                        fatherLabel.Unit = data.Unit; //La unidad que tiene ese Label //logisticUnit; 
                        fatherLabel.StartQty = logisticFactor; //*data.Unit.BaseAmount;
                        fatherLabel.CurrQty = logisticFactor; //*data.Unit.BaseAmount; 
                    }


                    //fatherLabel.UnitBaseFactor = logisticUnit.BaseAmount;  //Siempres es 1
                    fatherLabel.IsLogistic = false;
                    fatherLabel.FatherLabel = null;
                    fatherLabel.LabelCode = ""; // (numSeqLabel + i).ToString() + GetRandomHex(data.CreatedBy, numSeqLabel + i); // (initSequence.NumSequence + i).ToString();
                    
 
                    fatherLabel.Notes = notes;
                    fatherLabel.PrintingLot = printingLot; //data.Note;
                    fatherLabel.ReceivingDate = receivingDate == null ? DateTime.Now : receivingDate;
                    fatherLabel.ReceivingDocument = data.Document.DocID > 0 ? data.Document : null;

                    fatherLabel = Factory.DaoLabel().Save(fatherLabel);

                    fatherLabel.LabelCode = fatherLabel.LabelID.ToString(); //Added for indentityUSE
                    labelResult.Add(fatherLabel);

                    //Registra el movimiento del nodo

                    SaveNodeTrace(
                        new NodeTrace
                        {
                            Node = node,
                            Document = data.Document.DocID > 0 ? data.Document : null,
                            Label = fatherLabel,
                            Quantity = fatherLabel.CurrQty,
                            IsDebit = false,
                            CreatedBy = data.CreatedBy,
                            Comment = comment
                        }
                    );

            }

            //Ajustando la sequencia
            //initSequence.NumSequence += numLabels;

            //Factory.DaoDocumentTypeSequence().Update(initSequence);

            if (!isTransactional)
                Factory.Commit();

            return labelResult;
        }


        //Dec 04 2009 para recibir seriales de una vez en proceos One By One
        public IList<Label> CreateProductUniqueTrackLabels(DocumentLine data, Node node, Bin destLocation, 
            string printingLot, string comment, DateTime receivingDate)
        {            

            //Label Type
            DocumentType labelType = Factory.DaoDocumentType().Select(new DocumentType { DocTypeID = LabelType.UniqueTrackLabel }).First();

            //Status
            Status status = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();

            IList<Label> labelResult = new List<Label>();

            string notes = (data.Document != null && data.Document.DocID > 0) ? "Doc# " + data.Document.DocNumber : "";
            notes = notes + " " + data.Unit.Name;

            //double labelBalance = data.Quantity;
            int numLabels = (int)data.Quantity;

            if (numLabels <= 0)
                throw new Exception("No valid amount of labels to print.");


            Label fatherLabel = null;

             for (int i = 0; i < numLabels; i++)
            {
                fatherLabel = new Label();
                fatherLabel.Node = node;
                fatherLabel.Bin = destLocation;
                fatherLabel.Product = data.Product;
                fatherLabel.CreatedBy = data.CreatedBy;
                fatherLabel.Status = status;
                fatherLabel.LabelType = labelType;
                fatherLabel.CreationDate = DateTime.Now;
                fatherLabel.Printed = true; 

                fatherLabel.Unit = data.Unit; //La unidad que tiene ese Label //logisticUnit; L
                fatherLabel.IsLogistic = false;
                fatherLabel.FatherLabel = null;
                fatherLabel.LabelCode = Guid.NewGuid().ToString();
                fatherLabel.CurrQty = 1; //*data.Unit.BaseAmount;
                fatherLabel.StartQty = 1; //*data.Unit.BaseAmount;

                fatherLabel.Notes = notes;
                fatherLabel.PrintingLot = printingLot; //data.Note;
                fatherLabel.ReceivingDate = receivingDate == null ? DateTime.Now : receivingDate;
                fatherLabel.ReceivingDocument = data.Document.DocID > 0 ? data.Document : null;

                 //Guarda y Actualiza para poder obtener su serial
                fatherLabel = Factory.DaoLabel().Save(fatherLabel);
                fatherLabel.LabelCode = '1' + fatherLabel.LabelID.ToString().PadLeft(WmsSetupValues.LabelLength-1, '0');
                Factory.DaoLabel().Update(fatherLabel);

                labelResult.Add(fatherLabel);


                //Registra el movimiento del nodo

                SaveNodeTrace(
                    new NodeTrace
                    {
                        Node = node,
                        Document = data.Document.DocID > 0 ? data.Document : null,
                        Label = fatherLabel,
                        Quantity = fatherLabel.CurrQty,
                        IsDebit = false,
                        CreatedBy = data.CreatedBy,
                        Comment = comment
                    }
                );

            }

            return labelResult;
        }


        public string GetAssignedBins(Product product, Location location, short binDirection)
        {

            string result = "";

            if (product == null)
                return result;

            int count = 0;

            if (location == null || product.ProductZones == null || product.ProductZones.Count == 0)
                return result;

            foreach (ZoneEntityRelation ze in product.ProductZones)
            {
                if (ze.Zone.Bins == null || ze.Zone.Bins.Count == 0)
                    continue;

                try
                {
                    foreach (ZoneBinRelation b in ze.Zone.Bins.Where(f => f.Zone.Location.LocationID == location.LocationID).OrderBy(f => f.BinType))
                    {
                        if (b.BinType == 0 || b.BinType == binDirection || binDirection == 0)
                            result += b.Bin.BinCode + ", ";

                        count++;
                        if (count == WmsSetupValues.DefaultBinsToShow)
                            return result;
                    }
                }
                catch { }
            }

            return result;

        }




        public IList<ZoneBinRelation> GetAssignedBinsList(Product product, Location location)
        {

            IList<ZoneBinRelation> result = new List<ZoneBinRelation>();

            if (product == null || location == null)
                return result;

            //if ( product.ProductZones == null || product.ProductZones.Count == 0)
            //    return result;

            IList<ZoneEntityRelation> tmpZoneEntity = Factory.DaoZoneEntityRelation().Select(
                    new ZoneEntityRelation { Entity = new ClassEntity { ClassEntityID = EntityID.Product }, 
                                             EntityRowID = product.ProductID }
                                             );
            if (tmpZoneEntity == null || tmpZoneEntity.Count == 0)
                return result;

            IList<ZoneBinRelation> tmpZoneBin;

            foreach (ZoneEntityRelation ze in tmpZoneEntity)
            {
                /*
                if (ze.Zone.Bins == null || ze.Zone.Bins.Count == 0)
                    continue;

                foreach (ZoneBinRelation b in ze.Zone.Bins.Where(f => f.Zone.Location.LocationID == location.LocationID))
                    result.Add(b);
                */

                tmpZoneBin = Factory.DaoZoneBinRelation().Select(new ZoneBinRelation { Zone = ze.Zone });
                if (tmpZoneBin == null || tmpZoneBin.Count == 0)
                    continue;

                foreach (ZoneBinRelation b in tmpZoneBin.Where(f=> f.Zone.Location.LocationID == location.LocationID))
                    result.Add(b);

            }
            return result.OrderByDescending(f=>f.BinType).ToList();
        }



        public string GetCompanyOption(Company company, string code)
        {
            try
            {
                return Factory.DaoConfigOptionByCompany().Select(new ConfigOptionByCompany
                {
                    Company = company,
                    ConfigOption = new ConfigOption { Code = code }
                }).First().Value;
            }
            catch { return ""; }
        }

        public string GetConfigOption(string code)
        {
            try
            {
                return Factory.DaoConfigOption().Select(new ConfigOption
                {
                    Code = code,
                    ConfigType = new ConfigType { ConfigTypeID = 1 }
                }).First().DefValue;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssDocument"></param>
        /// <param name="qtyType">1-QtyOrder (Order), 2-QtyAllocates (Shipment)</param>
        public void PrintKitAssemblyLabels(Document ssDocument, int qtyType)
        {

            if (!ssDocument.DocumentLines.Any(f => f.Note == "2"))
                return;


            IList<Label> list = new List<Label>();


            double qtyToUse = 0;
            int unitsPerPack = 0;
            int numLabels = 0;

            foreach (DocumentLine line in ssDocument.DocumentLines
                .Where(f => f.Note == "2" && f.Product.Category != null && f.Product.Category.ExplodeKit > 0))
            {
                
                qtyToUse = (qtyType == 1) ? line.Quantity : line.QtyAllocated;
                
                unitsPerPack = (line.Product.UnitsPerPack > 0) ? line.Product.UnitsPerPack : 1;

                numLabels = line.Product.Category.ExplodeKit == ExplodeKit.Caterpillar ? 1 : 2;

                for (int i = 0; i < (qtyToUse * numLabels) / unitsPerPack; i++) //Imprime 2 labels por cada KIT
                    list.Add(new Label
                    {
                        Product = line.Product,
                        LabelCode = line.Product.ProductCode,
                        LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                        CurrQty = unitsPerPack, //1
                        StartQty = unitsPerPack //1
                    });

                // CAA [2010/06/25]
                // Se imprimen los labels de los componentes de CaterpillarKit (los originales NO los substitutos)
                if (line.Product.Category.ExplodeKit == ExplodeKit.CaterpillarKit)
                {
                    Product parentKit;

                    foreach (DocumentLine lineComp in ssDocument.DocumentLines
                        .Where(f => f.Note == "1" && f.LinkDocLineNumber == line.LineNumber ))
                    {

                        try
                        {
                            parentKit = Factory.DaoKitAssemblyFormula().Select(
                                new KitAssemblyFormula { Component = lineComp.Product, DirectProduct = new Product { Category = new ProductCategory { ExplodeKit = ExplodeKit.Caterpillar } } })
                                .Select(f => f.DirectProduct)
                                //.Where(f => f.Category.ExplodeKit == ExplodeKit.Caterpillar)
                                .First();

                            qtyToUse = (qtyType == 1) ? lineComp.Quantity : lineComp.QtyAllocated;

                            unitsPerPack = (parentKit.UnitsPerPack > 0) ? parentKit.UnitsPerPack : 1;

                            numLabels = 1; //Solo Catterpillar components


                            for (int i = 0; i < (qtyToUse * numLabels) / unitsPerPack; i++) //Imprime 2 labels por cada KIT

                                list.Add(new Label
                                {
                                    Product = parentKit,
                                    LabelCode = parentKit.ProductCode,
                                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                                    CurrQty = unitsPerPack, //1
                                    StartQty = unitsPerPack //1
                                });

                        }
                        catch { continue; }

                    }
                }
            }



            if (list.Count > 0)
            {

                //template Generica de Producto
                //Removido oct 27 / 2009
                //LabelTemplate defTemplate = Factory.DaoLabelTemplate().Select(
                    //new LabelTemplate { Header = WmsSetupValues.ProductLabelTemplate }).First();

                LabelTemplate defTemplate = null; //Para que tome el del producto por defecto

                //Path de Applicacion
                string appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);

                //Print Function.
                ReportMngr.PrintLabelsFromFacade(new Printer { PrinterName = WmsSetupValues.DEFAULT },
                    defTemplate, list, appPath);
            }
        }



        public string GetAssignedProducts(Bin bin)
        {

            string result = "";

            if (bin == null)
                return result;

            IList<ZoneBinRelation> zoneList = Factory.DaoZoneBinRelation().Select(new ZoneBinRelation { Bin = bin });


            if (zoneList == null || zoneList.Count == 0)
                return result;

            IList<ZoneEntityRelation> zer = null;

            foreach (ZoneBinRelation ze in zoneList)
            {
                zer = Factory.DaoZoneEntityRelation().Select(
                    new ZoneEntityRelation
                    {
                        Zone = ze.Zone,
                        Entity = new ClassEntity { ClassEntityID = EntityID.Product }
                    });

                if (zer == null || zer.Count == 0)
                    continue;

                foreach (ZoneEntityRelation r in zer)
                {
                    try { result += Factory.DaoProduct().SelectById(new Product { ProductID = r.EntityRowID }).ProductCode + ", "; }
                    catch { }
                }
            }
            return result;

        }



        public object[] GetSuggestedBins(Product product, Location location, PickMethod pickMethod, short binDirection)
        {
            string result1 = "", result2 = "";
            string firstBin = "";
            string oldestBin = "";
            IList<ProductStock> list = Factory.DaoLabel().GetSuggestedBins(product, location, pickMethod);
            IList<Bin> resultBin = new List<Bin>();


            if (list == null || list.Count == 0)
                return new object[] { "NO STOCK", WmsSetupValues.MaxBinRank, firstBin, oldestBin };

            
            //Bin con mercancia mas vieja.
            try
            {
                ProductStock oldest = list.Where(f => f.MinDate != null).OrderBy(f => f.MinDate).First();
                oldestBin += oldest.Bin.BinCode + " (";
                oldestBin += (oldest.PackStock + oldest.Stock).ToString() + ")\n";
            }
            catch { }


            IList<ZoneBinRelation> listBin = GetAssignedBinsList(product, location);


            foreach (ProductStock ps in list.OrderBy(f => f.Bin.Rank))
            {

                if (binDirection == BinType.In_Out)
                {
                    result1 += ps.Bin.BinCode + " (";

                    if (ps.PackStock + ps.Stock > 0)
                        result1 += (ps.PackStock + ps.Stock).ToString();

                    result1 += ")\n";

                    resultBin.Add(ps.Bin);

                    if (string.IsNullOrEmpty(firstBin))
                        firstBin = ps.Bin.BinCode;
                }
                else
                {

                    if (listBin != null && listBin.Any(f => f.Bin.BinID == ps.Bin.BinID && f.BinType != binDirection))
                        continue;

                    if (binDirection == BinType.Exclude_Out && ps.BinType == BinType.Out_Only)
                        continue;

                    if (binDirection == BinType.Exclude_In && ps.BinType == BinType.In_Only)
                        continue;


                    //Si el bin esta en la lista de Zonas Se va para el Result 1
                    if (ps.PackStock + ps.Stock > 0)
                        if (listBin.Any(f => f.Bin.BinID == ps.Bin.BinID))
                        {
                            result1 += ps.Bin.BinCode + " (";
                            result1 += (ps.PackStock + ps.Stock).ToString();
                            result1 += ")\n";

                            if (string.IsNullOrEmpty(firstBin))
                                firstBin = ps.Bin.BinCode;
                        }
                        else //Si no se va para el result 2
                        {
                            result2 += ps.Bin.BinCode + " (";
                            result2 += (ps.PackStock + ps.Stock).ToString();
                            result2 += ")\n";
                        }

                    //Al final sumo result 1 y 2.

                    resultBin.Add(ps.Bin);
                }
            }

            if (string.IsNullOrEmpty(firstBin) && resultBin[0] != null)
                firstBin = resultBin[0].BinCode;

            int resRank = (resultBin[0] != null) ? resultBin[0].Rank : 0;

            return new object[] { result1 + result2, resRank, firstBin, oldestBin };
        }


        public Dictionary<int,object[]> GetDocumentSuggestedBins(Document document, Location location, PickMethod pickMethod, short binDirection)
        {
            string result = "";
            string firstBin = "";
            IList<ProductStock> list = Factory.DaoLabel().GetDocumentSuggestedBins(document, location, pickMethod);
            IList<Bin> resultBin = new List<Bin>();
            Dictionary<int, object[]> resultDictionary = new Dictionary<int, object[]>();


            foreach (Product product in list.Select(p => p.Product).Distinct())
            {

                if (list == null || list.Count == 0)
                {
                    if (!resultDictionary.ContainsKey(product.ProductID))
                        resultDictionary.Add(product.ProductID, new object[] { "NO STOCK", WmsSetupValues.MaxBinRank, firstBin });
                }

                IList<ZoneBinRelation> listBin = GetAssignedBinsList(product, location);


                foreach (ProductStock ps in list.Where(f=>f.Product.ProductID == product.ProductID).OrderBy(f => f.Bin.Rank))
                {

                    if (binDirection == BinType.In_Out)
                    {
                        result += ps.Bin.BinCode + " (";

                        if (ps.PackStock + ps.Stock > 0)
                            result += (ps.PackStock + ps.Stock).ToString();

                        result += ")\n";

                        resultBin.Add(ps.Bin);

                        if (string.IsNullOrEmpty(firstBin))
                            firstBin = ps.Bin.BinCode;
                    }
                    else
                    {

                        if (listBin != null && listBin.Any(f => f.Bin.BinID == ps.Bin.BinID && f.BinType != binDirection))
                            continue;

                        if (binDirection == BinType.Exclude_Out && ps.BinType == BinType.Out_Only)
                            continue;

                        if (binDirection == BinType.Exclude_In && ps.BinType == BinType.In_Only)
                            continue;


                        result += ps.Bin.BinCode + " (";

                        if (ps.PackStock + ps.Stock > 0)
                            result += (ps.PackStock + ps.Stock).ToString();

                        result += ")\n";

                        resultBin.Add(ps.Bin);

                        if (string.IsNullOrEmpty(firstBin))
                            firstBin = ps.Bin.BinCode;
                    }

                    if (!resultDictionary.ContainsKey(product.ProductID))
                        resultDictionary.Add(product.ProductID, new object[] { result, resultBin[0].Rank, firstBin });
                }
               
            }

            return resultDictionary;
        }



        
        public object[] GetFirstSuggestedBin(Product product, Location location, PickMethod pickMethod, short binDirection)
        {
            string result = "";
            IList<ProductStock> list = Factory.DaoLabel().GetSuggestedBins(product, location, pickMethod);
            IList<Bin> resultBin = new List<Bin>();


            if (list == null || list.Count == 0)
                return new object[] { "NO STOCK", WmsSetupValues.MaxBinRank };


            IList<ZoneBinRelation> listBin = GetAssignedBinsList(product, location);


            foreach (ProductStock ps in list.OrderBy(f => f.Bin.Rank))
            {

                if (binDirection == BinType.In_Out)
                {
                    result += ps.Bin.BinCode + " (";

                    if (ps.PackStock + ps.Stock > 0)
                        result += (ps.PackStock + ps.Stock).ToString();

                    result += ")\n";

                    resultBin.Add(ps.Bin);

                    return new object[] { result, resultBin[0].Rank };
                }
                else
                {
                    if (listBin != null && listBin.Any(f => f.Bin.BinID == ps.Bin.BinID && f.BinType != binDirection))
                        continue;

                    result += ps.Bin.BinCode + " (";

                    if (ps.PackStock + ps.Stock > 0)
                        result += (ps.PackStock + ps.Stock).ToString();

                    result += ")\n";

                    resultBin.Add(ps.Bin);

                    return new object[] { result, resultBin[0].Rank };
                }

            }

            return new object[] { "NO STOCK", WmsSetupValues.MaxBinRank };
            
        }
        


        //Devuelve un datatable de una lista de objetos
        public DataTable GetDataTableSchema(String[] cols, string tableName, IList<Object[]> list)
        {
            DataTable dt = new DataTable(tableName);
            for (int i = 0; i < cols.Length; i++)
                dt.Columns.Add(cols[i].Trim());

            DataRow dr;

            //Tabla Principal
            foreach (Object[] objArray in list)
            {
                dr = dt.NewRow();

                for (int i = 0; i < cols.Length; i++)
                    dr[cols[i].Trim()] = objArray[i];

                dt.Rows.Add(dr);
            }

            //DataSet ds = new DataSet("dsResult");
            //ds.Tables.Add(dt);
            return dt;
        }



        public string ConvertDataTableToHtml(DataTable targetTable, string headMsg)
        {
            //Get a worker object. 
            StringBuilder myBuilder = new StringBuilder();

            ////Open tags and write the top portion. 
            //myBuilder.Append("<html xmlns='http://www.w3.org/1999/xhtml'>"); 
            //myBuilder.Append("<head>"); myBuilder.Append("<title>"); 
            //myBuilder.Append("Page-"); myBuilder.Append(Guid.NewGuid().ToString()); 
            //myBuilder.Append("</title>"); myBuilder.Append("</head>"); 
            //myBuilder.Append("<body>"); 
            myBuilder.Append("<br><table border='0' cellpadding='3' cellspacing='1' bgcolor='#dedede'>");
            //Add the headings row.

            if (!string.IsNullOrEmpty(headMsg))
            {
                myBuilder.Append("<tr bgcolor='#ffffbb'><td colspan=" + targetTable.Columns.Count.ToString() + "><b>");
                myBuilder.Append(headMsg);
                myBuilder.Append("</b></td></tr>");
            }

            myBuilder.Append("<tr valign='top'>");

            foreach (DataColumn myColumn in targetTable.Columns)
            {
                myBuilder.Append("<th align='center' valign='top'>");
                myBuilder.Append(myColumn.ColumnName);
                myBuilder.Append("</th>");
            }

            myBuilder.Append("</tr>");

            //Add the data rows. 
            foreach (DataRow myRow in targetTable.Rows)
            {
                myBuilder.Append("<tr bgcolor='#ffffff' valign='top' >");

                foreach (DataColumn myColumn in targetTable.Columns)
                {
                    myBuilder.Append("<td valign='top'>");
                    myBuilder.Append(myRow[myColumn.ColumnName].ToString());
                    myBuilder.Append("</td>");
                }

                myBuilder.Append("</tr>");

            }

            //Close tags. 
            myBuilder.Append("</table><br><br>");

            return myBuilder.ToString();

        }


        //Obtiene el Default Bin de un Producto, de lo contrario lo manda a MAIN.
        public Label GetProductDefaultBinLabel(Product product, Location location, short binDirection)
        {
            IList<ZoneBinRelation> zoneBin = GetProductAssignedZone(product, location);

            if (zoneBin == null || zoneBin.Where(f => f.BinType == binDirection || binDirection == BinType.In_Out || f.BinType == BinType.In_Out).Count() == 0)
                return null; //GetBinLabel(DefaultBin.MAIN, location);

            foreach (ZoneBinRelation z in zoneBin.Where(f => f.BinType == binDirection || binDirection == BinType.In_Out || f.BinType == BinType.In_Out).OrderBy(f => f.Bin.Rank))
                return GetBinLabel(z.Bin.BinCode, location);

            return null; //GetBinLabel(DefaultBin.MAIN, location);
        }


        public Label GetBinLabel(string binCode, Location location)
        {
            return Factory.DaoLabel().Select(
                new Label { LabelCode = binCode, Bin = new Bin { Location = location } }).First();
        }



        //Return a dataset from a XML string  document
        public static DataSet GetDataSet(string xmlData)
        {
            XmlDocument myXmlOut = new XmlDocument();
            myXmlOut.LoadXml(xmlData);

            // convert to dataset in two lines
            DataSet ds = new DataSet();
            ds.ReadXml(new XmlNodeReader(myXmlOut));

            return ds;
        }



        /*
        public void SendMailNotification(string msgCode, Company company, SysUser user)
        {
            string message = "";
            string subject = "";
            string userTo = "";

            switch (msgCode)
            {
                case "PICKING":
                    userTo = GetCompanyOption(company, "MSGPICKTO");
                    if (string.IsNullOrEmpty(userTo))
                    {
                        CreateNotification(subject, message, userTo);
                    }
                    break;

                default:
                    return;
            }
        }


        private void CreateNotification(string subject, SysUser user, string mailTo)
        {
            try
            {
                MessagePool messageRecord = new MessagePool();
                messageRecord.CreatedBy = user.UserName;
                messageRecord.CreationDate = DateTime.Now;
                messageRecord.MailTo = mailTo;
                messageRecord.MailFrom = (String)GetContextValue("EMAILFROM");
                messageRecord.Subject = "WMS Process: " + subject + ", By: " + messageRecord.CreatedBy + ", Date: " + DateTime.Today.ToShortDateString();

                string msg = (String)GetContextValue("RESULTMESSAGE");
                msg = msg.Replace("\n", "<br>");
                msg = "<b>" + messageRecord.Subject + "</b><br><br>" + msg;
                messageRecord.Message = msg;

                Factory.DaoMessagePool().Save(message);
            }
            catch { Factory.Rollback(); }
        }
         * */


        public IList<Unit> GetDocumentProductUnit(Document document, Product product)
        {
            try
            {
                IList<DocumentLine> lines = Factory.DaoDocumentLine().Select(new DocumentLine
                {
                    Document = new Document { DocID = document.DocID },
                    Product = new Product { ProductID = product.ProductID }
                });

                return lines.Select(f => f.Unit).Distinct().ToList();

            }
            catch { return null; }

        }


        public void PersistProductInUse(ProductInventory productInventory)
        {            

            try //NEW
            {
                productInventory.CreationDate = DateTime.Now;
                productInventory.ModDate = DateTime.Now;
                productInventory.ModifiedBy = productInventory.CreatedBy;
                Factory.DaoProductInventory().Save(productInventory);
            }
            catch
            {
                //UPDATE
                double qtyInUse = productInventory.QtyInUse;
                double qtyAllocated = productInventory.QtyAllocated;
                string createdBy = productInventory.CreatedBy;

                productInventory = Factory.DaoProductInventory().Select(productInventory).First();
                productInventory.QtyInUse += qtyInUse;
                if (productInventory.QtyInUse < 0)
                    productInventory.QtyInUse = 0;

                if (productInventory.QtyAllocated < 0)
                    productInventory.QtyAllocated += qtyAllocated;

                if (productInventory.QtyAllocated + productInventory.QtyInUse == 0)
                {
                    Factory.DaoProductInventory().Delete(productInventory);
                    return;
                }

                productInventory.ModDate = DateTime.Now;
                productInventory.ModifiedBy = createdBy;
                Factory.DaoProductInventory().Update(productInventory);
            }

        }

        public void ResetQtyInUse(ProductInventory productInventory)
        {
            //Borrando todas las lineas que no fueron allocated.
            IList<ProductInventory> list = Factory.DaoProductInventory().Select(productInventory);
            foreach (ProductInventory reg in list.Where(f => f.QtyAllocated <= 0))
                Factory.DaoProductInventory().Delete(reg);
        }



        public Boolean ValidateReceiptDocumentTrackingOptions(Document data, Node node, Boolean autoThrow)
        {
            //Hace que traiga las lineas in lazy = true
            Factory.IsTransactional = true;

            //Validate company is linked to ERP
            /*
            if (Factory.DaoConfigOptionByCompany()
                        .Select(new ConfigOptionByCompany
                        {
                            ConfigOption = new ConfigOption { Code = "WITHERP" },
                            Company = data.Company,
                        }
                        ).FirstOrDefault().Value == "F")
                return true;
            */

            //Saco los productos del document que requieren track options
            IList<Product> productList;
            if (data.IsFromErp == true)
                productList = Factory.DaoDocumentLine().Select(new DocumentLine { Document = data })
                    .Select(f => f.Product).Where(f => f.ProductTrack != null && f.ProductTrack.Count > 0)
                    .Distinct().ToList();
            else
                productList = Factory.DaoDocumentBalance().PostingBalance(
                    new DocumentBalance { Document = data, Location = data.Location, Node = node })
                    .Select(f => f.Product).ToList();


            if (productList == null || productList.Count == 0)
                return true; //No tiene track option ninguno de los productos


            //Busca si todos los items tienen el track que necesitan.
            string errorMsg = "";
            NodeTrace pattern = null;
            foreach (Product product in productList)
            {
                if (product.ProductTrack == null || product.ProductTrack.Count == 0) //product.ErpTrackOpt == 0 ||
                    continue;

                //Para las que no son unique //LOTES/FECHAS etc.
                foreach (ProductTrackRelation pt in product.ProductTrack.Where(f => f.TrackOption.IsUnique != true && f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality))
                {
                    if (Factory.DaoNodeTrace().GetRecordWithoutTrackOption(data, pt, node).Count > 0)
                    {
                        //errorMsg += "Product " + pt.Product.Name + " require " + pt.TrackOption.DisplayName + ".\n";
                        errorMsg += "Some " + pt.TrackOption.DisplayName + " for product " + product.ProductCode + " are missing.\n";
                        continue;
                    }
                }

                //Para productos unique, los labels de tipo 1002 deben tener X hijos de tipo 1005
                foreach (ProductTrackRelation pt in product.ProductTrack.Where(f => f.IsUnique == true))
                {
                    pattern = new NodeTrace
                    {
                        Document = data,
                        Label = new Label { Product = product, LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel } },
                        Node = node
                    };


                    if (Factory.DaoNodeTrace().Select(pattern).Select(f => f.Label).Sum(f => f.CurrQty) > 0)
                    {
                        //errorMsg += "Product " + pt.Product.Name + " require " + pt.TrackOption.DisplayName + ".\n";
                        errorMsg += "Some " + pt.TrackOption.DisplayName + " for product " + product.ProductCode + " are missing.\n";
                        continue;
                    }
                }



            }

            if (!string.IsNullOrEmpty(errorMsg))
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception(errorMsg + "Please enter data before process.");
                }
                else
                    return false;


            Factory.Commit();
            Factory.IsTransactional = false;
            return true;
        }


        public static string GetRandomHex(string createdBy, long seq)
        {
            /*
            try
            {
                string data = DateTime.Now.Millisecond.ToString();
                return data.Substring(data.Length - 1, 1);
            }
            catch { return (new Random()).Next(99).Substring(data.Length - 1, 1); }
             */

            //string data = (new Random()).Next(99).ToString();
            //return data.Substring(data.Length - 1, 1);
            string data = (seq % createdBy.Length).ToString();
            return data.Substring(data.Length - 1, 1);

        }


        public void DirectSQLNonQuery(string query, Connection connection)
        {
            SQLBase.ExecuteQuery(query, new SqlConnection(connection.CnnString));
        }


        public DataTable DirectSQLQuery(string query, string swhere, string tableName, Connection connection)
        {
            return SQLBase.ReturnDataTable(query, swhere, tableName, new SqlConnection(connection.CnnString));            
        }

        public static DataSet DirectSQLQueryDS(string query, string swhere, string tableName, Connection connection)
        {
            return SQLBase.ReturnDataSet(query, swhere, tableName, new SqlConnection(connection.CnnString));

        }

        public void IncreaseOption(string option, Company company)
        {
            try
            {
                ConfigOption cfg = Factory.DaoConfigOption().Select(new ConfigOption { Code = option }).First();
                cfg.DefValue = (int.Parse(cfg.DefValue) + 1).ToString();
                Factory.DaoConfigOption().Update(cfg);
            }
            catch { }


            try
            {
                ConfigOptionByCompany cfg = Factory.DaoConfigOptionByCompany().Select(
                    new ConfigOptionByCompany
                    {
                        Company = company,
                        ConfigOption = new ConfigOption { Code = option }
                    }).First();

                cfg.Value = (int.Parse(cfg.Value) + 1).ToString();
                Factory.DaoConfigOptionByCompany().Update(cfg);
            }
            catch { }

        }

        public static void PrintDocumentsInBatch(IList<Document> documentList, string appPath, string printer, CustomProcess process)
        {
            if (string.IsNullOrEmpty(appPath))
                appPath = Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);

            if (string.IsNullOrEmpty(printer))
                printer = WmsSetupValues.DEFAULT;


            //Create el therat desde este punto.
            BatchPrintProcess threadP = new BatchPrintProcess
            {
                DocumentList = documentList.ToList(),
                AppPath = appPath,
                Printer = printer,
                Process = process
            };

            //RptMngr.PrintDocumentsInBatch(documentList, appPath, printer, process);

            Thread th = new Thread(new ParameterizedThreadStart(PrintDocumentsInBatchThread));
            th.Start(threadP);
        }



        private static void PrintDocumentsInBatchThread(Object threatP)
        {
            BatchPrintProcess batch = (BatchPrintProcess)threatP;
            (new ReportMngr()).PrintDocumentsInBatch(batch.DocumentList, batch.AppPath, batch.Printer, batch.Process);
        }


        public void UpdateIsMainProductAccount(ProductAccountRelation data)
        {

            ProductAccountRelation pa = Factory.DaoProductAccountRelation().Select(data).First();
            pa.IsDefault = data.IsDefault;

            if (data.IsDefault == true)
            {
                IList<ProductAccountRelation> list = Factory.DaoProductAccountRelation().Select(
                    new ProductAccountRelation { Product = pa.Product });

                foreach (ProductAccountRelation r in list.Where(f => f.RowID != pa.RowID))
                {
                    r.IsDefault = false;
                    Factory.DaoProductAccountRelation().Update(r);
                }

                //Actulizando el Default del Producto
                Product p = Factory.DaoProduct().Select(new Product {ProductID = pa.Product.ProductID } ,0).First();
                p.UpcCode = pa.Code1;
                p.DefVendorNumber = pa.ItemNumber;
                Factory.DaoProduct().Update(p);

            }
                        
            Factory.DaoProductAccountRelation().Update(pa);


        }

        public AccountAddress SaveUpdateAccountAddres(AccountAddress data, int saveType)
        {

            //Save in the ERP
            if (GetCompanyOption(data.Account.Company, "WITHERPSH").Equals("T"))
            {
                if (saveType == 1)
                {                    
                    data.ErpCode = data.Name.Replace("'", "").Replace(" ", "");

                    if (data.ErpCode.Length > 10)
                        data.ErpCode = data.ErpCode.Substring(0, 10);                                            
                }

                (new ErpDataMngr()).CreateCustomerAddress(data);
            }
            

            if (saveType == 1)
                data = Factory.DaoAccountAddress().Save(data);
            else
                Factory.DaoAccountAddress().Update(data);


            return data;
            
        }


    }
}
