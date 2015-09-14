using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Master;
using Entities.General;
using Integrator.Dao;
using Integrator;
using Entities;
using Entities.Profile;



namespace BizzLogic
{
    public class Rules
    {
        //Properties
        private DaoFactory Factory { get; set; }

        //Constructor
        public Rules(DaoFactory factory)
        { Factory = factory; }


        public Boolean ValidateProductInDocument(DocumentLine data, Boolean autoThrow)
        {
            //Valida si el producto esta en ese documento
            DocumentLine tmpLine = new DocumentLine
            {
                Document = new Document { DocID = data.Document.DocID }, //data.Document,
                Product = new Product { ProductID = data.Product.ProductID },
                LineStatus = data.LineStatus
            };


            if (Factory.DaoDocumentLine().Select(tmpLine).Count == 0)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Product " + tmpLine.Product.Name + " does not exists in document " + tmpLine.Document.DocNumber + ".");
                }
                else
                    return false;
            }
            return true;
        }



        public Boolean ValidateBalanceQuantityInDocument(DocumentLine validationLine,
            Node node,   Boolean autoThrow, bool isCrossDock)
        //bool isCrossDock, Adicionada en SEP/17/2009 para manejar el balance en cross dock sin tener en cuenta BO Quantities
        {

            DocumentBalance docBal = new DocumentBalance
            {
                Document = validationLine.Document,
                Product = validationLine.Product,
                Unit = validationLine.Unit,
                Node = node
            };

            docBal = Factory.DaoDocumentBalance().GeneralBalance(docBal, isCrossDock).First();

            //Multiplica la cantidad por la unidad base para hacer la resta
            //La unidad del documento de Balance es la unidad Basica
            if (docBal == null || docBal.BaseQtyPending < validationLine.Quantity * validationLine.Unit.BaseAmount)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    //throw new Exception("Current balance for document is " + (docBal.QtyPending / validationLine.Unit.BaseAmount).ToString() + " and you are trying to receive " + ((int)validationLine.Quantity).ToString() + ".");
                    throw new Exception("Current balance for product " + docBal.Product.ProductCode + " in document "
                        + docBal.Document.DocNumber +" is " + docBal.QtyPending.ToString() + " (" + docBal.Unit.Name + ")"
                        + " and you are trying to process " + ((int)validationLine.Quantity).ToString() + " (" + docBal.Unit.Name + ").");
                    
                }
                else
                    return false;
            }

            return true;
        }



        public Boolean ValidateActiveStatus(Status data, Boolean autoThrow)
        {
            if (data.StatusID == EntityStatus.Inactive)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Record appears as inactive.");
                }
                else
                    return false;
            }
            return true;
        }



        public Boolean ValidateLabelIsActive(Label data, Boolean autoThrow)
        {
            if (data.Status.StatusID != EntityStatus.Active)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Label " + data.Name +" appears as "+data.Status.Name+".");
                }
                else
                    return false;
            }
            return true;
        }


        public Boolean ValidateIsProductLabel(Label data, Boolean autoThrow)
        {
            if (data.LabelType.DocTypeID != LabelType.ProductLabel && data.LabelType.DocTypeID != LabelType.UniqueTrackLabel)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Label  " + data.LabelCode + " is not a Product Label.");
                }
                else
                    return false;
            }

            return true;

        }



        public Boolean ValidateIsBinLabel(Label data, Boolean autoThrow)
        {
            if (data.LabelType.DocTypeID != LabelType.BinLocation)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Location " + data.LabelCode + " is not a Bin.");
                }
                else
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Retorna execpcion Si no es una etiqueta logistica y tampoco es un Bin
        /// </summary>
        /// <param name="data"></param>
        /// <param name="autoThrow"></param>
        /// <returns></returns>
        public Boolean ValidateIsUbicationLabel(Label data, Boolean autoThrow)
        {
            if (data.LabelType.DocTypeID != LabelType.BinLocation && data.LabelType.DocTypeID != LabelType.ProductLabel)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Label " + data.LabelCode + " is not a Bin or Product Label.");
                }
                else
                    return false;
            }

            return true;

        }


        public Boolean ValidateNodeInLabel(Label label, Node node, Boolean autoThrow)
        {
            if (label.Node.NodeID != node.NodeID)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Label " + label.LabelCode + " is not in a valid Node.");
                }
                else
                    return false;
            }

            return true;
        }


        public Boolean ValidateSameProduct(Product product1, Product product2, Boolean autoThrow)
        {
            if (!product1.Equals(product2))
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Transaction between differen products " + product1.Name + ", " + product2.Name + " not allowed.");
                }
                else
                    return false;
            }

            return true;
        }


        public Boolean ValidateSameLocation(Label source, Label destination, short locationType, Boolean autoThrow)
        {
            if (locationType == LabelType.BinLocation && source.Bin.BinID == destination.Bin.BinID)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Bin Locations are the same.");
                }
                else
                    return false;
            }


            if (locationType == LabelType.BinLocation && source.LabelID == destination.LabelID)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Label Locations are the same.");
                }
                else
                    return false;
            }


            return true;
        }


        public Boolean ValidateNodeRoute(Label label, Node node, Boolean autoThrow)
        {
            NodeRoute nodeRoute = new NodeRoute
            {
                NextNode = node,
                CurNode = label.Node
            };

            if (Factory.DaoNodeRoute().Select(nodeRoute).Count == 0)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Transaction from " + label.Node.Name + " to " + node.Name + " not allowed.");
                }
                else
                    return false;
            }

            return true;
        }


        public Boolean ValidateDocument(Document data, Boolean autoThrow)
        {
            if (data == null)
            {
                if (autoThrow)
                {
                    Factory.Rollback();
                    throw new Exception("Document required to execute the process.");
                }
                else
                    return false;
            }

            return true;

        }



        public Bin GetBinForNode(Node node, Location location)
        {
            switch (node.NodeID)
            {
                case NodeType.Picked:
                    return Factory.DaoBin().Select(new Bin { Location = location, BinCode = DefaultBin.PICKING }).First();
                
                case NodeType.Process:
                    return Factory.DaoBin().Select(new Bin { Location = location, BinCode = DefaultBin.PROCESS }).First();

                default:
                    return Factory.DaoBin().Select(new Bin { Location = location, BinCode = DefaultBin.MAIN }).First();

            }

        }


        public Boolean ValidateIsProcessBin(Label data, Boolean autoThrow)
        {
            if (data.LabelType.DocTypeID == LabelType.BinLocation && data.Bin.Process != null 
                && data.Bin.Process.Status.StatusID == EntityStatus.Active)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Bin " + data.Bin.BinCode + " is not a process bin or the process is inactive.");
            }
            else
                return false;
        }



        public Boolean ValidateBinStatus(Bin bin, bool autoThrow)
        {
            if (bin.Status.StatusID == EntityStatus.Active)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Bin affected: " + bin.BinCode + " is currently " + bin.Status.Name + ".");
            }
            else
                return false;
        }

        internal Boolean ValidateLabelQuantity(Label label, bool autoThrow)
        {
            if (label.StockQty > 0)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Label has zero quantiy.");
            }
            else
                return false; 
        }



        internal Boolean ValidateRestrictedProductInBin(Product product, Bin destBin, bool autoThrow)
        {
            if (product.IsBinRestricted != true)
                return true;

            //Allow if dest is a Special Bin
            if (destBin.IsArea == true)
                return true;

            //Validate product is restricted.
            IList<ZoneEntityRelation> list = Factory.DaoZoneEntityRelation().Select(
                new ZoneEntityRelation
                {
                    Entity = new ClassEntity { ClassEntityID = EntityID.Product },
                    EntityRowID = product.ProductID
                });

            if (list == null || list.Count == 0)
                return true;

            //Validate is dest in one of the allowed Bins
            foreach (ZoneEntityRelation zer in list)
                if (zer.Zone.Bins != null && zer.Zone.Bins.Any(f => f.Bin.BinID == destBin.BinID))
                    return true;


            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Product " + product.FullDesc + " is not allowed in bin " + destBin.BinCode);
            }
            else
                return false;
        }



        internal Boolean ValidateIsSameProduct(Label labelSource, Label labelDest, bool autoThrow)
        {
            if (labelDest.LabelType.DocTypeID == LabelType.BinLocation)
                return true;

            if (labelDest.Product.ProductID == labelSource.Product.ProductID)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Destination has different product.\n"+labelDest.Product.FullDesc);
            }
            else
                return false; 
        }

        internal Boolean ValidateVoided(Node node, bool autoThrow)
        {
            if (node.NodeID != NodeType.Voided)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Label appears as voided.");
            }
            else
                return false;
        }

        internal Boolean ValidateIsUniqueLabel(Label labelSource, bool autoThrow)
        {
            if (labelSource.LabelType.DocTypeID == LabelType.UniqueTrackLabel)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Product is not serialized.");
            }
            else
                return false;
        }

        internal Boolean ValidateSameLocation(Label labelSource, Label labelDest, bool autoThrow)
        {
            if (labelDest.Bin.Location.LocationID == labelSource.Bin.Location.LocationID)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Location/Site between source/destination are different.");
            }
            else
                return false;
        }


        internal Boolean ValidatePickLocation(Location docLocation, Location pickLocation, bool autoThrow)
        {
            if (docLocation.LocationID == pickLocation.LocationID)
                return true;

            if (autoThrow)
            {
                Factory.Rollback();
                throw new Exception("Pick Location is different to Document Location. " + "Document: " + docLocation.ErpCode + ", Pick: " + pickLocation.ErpCode);
            }
            else
                return false;
        }


    }
}
