using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using Entities.Profile;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Integrator;
using Microsoft.Dynamics.GP.eConnect;
using Microsoft.Dynamics.GP.eConnect.Serialization;
using Microsoft.Dynamics.GP.eConnect.MiscRoutines;
using System.Reflection;
using System.Globalization;
using System.Data.SqlClient;



namespace ErpConnect.DynamicsGP
{
    public partial class DocumentService_ec : SQLBase, IDocumentService
    {


        #region PostingDocuments


        #region Receiving 

        /// <summary>
        /// Con un Receiving Task generar el documento de Purchase Receipt a Generar
        /// </summary>
        /// <param name="recivingTask"></param>
        /// <returns></returns>
        public Boolean CreatePurchaseReceipt(Document receipt, IList<NodeTrace> traceList, bool costZero)
        {
            eConnectType eConnect;

            try
            {

                if (receipt.DocumentLines == null || receipt.DocumentLines.Count == 0)
                {
                    ExceptionMngr.WriteEvent("CreatePurchaseReceipt: No lines to process.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreatePurchaseReceipt: No lines to process.");
                }

                if (receipt.Location == null)
                {
                    ExceptionMngr.WriteEvent("CreatePurchaseReceipt: Document Location is missing.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreatePurchaseReceipt: Document Location is missing.");
                }

                taPopRcptLineInsert_ItemsTaPopRcptLineInsert[] docLines =
                    new taPopRcptLineInsert_ItemsTaPopRcptLineInsert[receipt.DocumentLines.Count];

                //Create an object that holds XML node object
                taPopRcptLineInsert_ItemsTaPopRcptLineInsert curLine;
                int i = 1;

                //For trak options
                IList<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert> serialTrack = new List<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert>();
                IList<taPopRcptLotInsert_ItemsTaPopRcptLotInsert> lotTrack = new List<taPopRcptLotInsert_ItemsTaPopRcptLotInsert>();


                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in receipt.DocumentLines)
                {
                    //Debe ser active, para garantizar que no es Misc, o service Item
                    if (dr.Product.Status.StatusID == EntityStatus.Active)
                    {

                        curLine = new taPopRcptLineInsert_ItemsTaPopRcptLineInsert();

                        //Validate Item/Vendor, GP requires that the Vendor has assigned the ItemNumber 
                        ValidateItemAndVendor(receipt.Vendor.AccountCode, dr.Product.ProductCode);

                        //Validate Item/Location, GP requires that the Location has assigned the ItemNumber 
                        ValidateItemAndLocation(receipt.Location.ErpCode, dr.Product.ProductCode);

                        // Populate Lines            
                        curLine.POPRCTNM = receipt.DocNumber;
                        curLine.POPTYPE = GP_DocType.PR_Shipment;
                        curLine.receiptdate = DateTime.Today.ToString("yyyy-MM-dd");
                        curLine.VENDORID = receipt.Vendor.AccountCode;

                        //Si va a Costo Zero el Costo debe ser enviado en el recibo como ZERO
                        if (costZero)
                        {
                            curLine.AUTOCOST = 0;
                            curLine.EXTDCOST = 0;
                            curLine.EXTDCOSTSpecified = true;
                            curLine.UNITCOST = 0;
                            curLine.UNITCOSTSpecified = true;                           
                        }
                        else
                            curLine.AUTOCOST = 1;                       
                        
                        curLine.ITEMNMBR = dr.Product.ProductCode;
                        curLine.VNDITNUM = dr.AccountItem;
                        curLine.LOCNCODE = dr.Location.ErpCode;
                        curLine.RCPTLNNM = i;
                        curLine.UOFM = dr.Unit.ErpCode;
                        curLine.QTYSHPPD = Decimal.Parse(dr.Quantity.ToString());


                        //Organizando los productos con Serial y Lotcode para cada linea del documento
                        if (dr.Product.ErpTrackOpt > 1 &&  dr.Product.ProductTrack != null)

                            foreach (ProductTrackRelation pt in dr.Product.ProductTrack)
                            {
                                if (pt.TrackOption.RowID == 1 && pt.DisplayName == null)  //Serial
                                    serialTrack = GetReceiptLineSerials(serialTrack, traceList.Where(f => f.PostingDocLineNumber == dr.LineNumber).ToList());

                                else if (pt.TrackOption.RowID == 2 && pt.DisplayName == null) //Lot Code
                                    lotTrack = GetReceiptLineLots(lotTrack, traceList.Where(f => f.PostingDocLineNumber == dr.LineNumber).ToList());
                            }


                        if (!string.IsNullOrEmpty(dr.LinkDocNumber))
                        {
                            curLine.PONUMBER = dr.LinkDocNumber;
                            curLine.POLNENUM = dr.LinkDocLineNumber;
                        }

                        docLines[i - 1] = curLine;
                        i++;
                    }
                }

                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                POPReceivingsType docType = new POPReceivingsType();

                //Adicionado Track Lists
                docType.taPopRcptLineInsert_Items = docLines;
                if (lotTrack.Count > 0)
                    docType.taPopRcptLotInsert_Items = lotTrack.ToArray();

                if (serialTrack.Count > 0)
                    docType.taPopRcptSerialInsert_Items = serialTrack.ToArray();

                //Create a taSopHdrIvcInsert XML node object

                taPopRcptHdrInsert docHdr = new taPopRcptHdrInsert();

                //Populate Header   
                docHdr.BACHNUMB = costZero ? "WMS_LANDING" : receipt.Location.ErpCode + "_" + GPBatchNumber.Receipt;

                docHdr.POPRCTNM = receipt.DocNumber;
                docHdr.POPTYPE = GP_DocType.PR_Shipment;
                docHdr.receiptdate = DateTime.Today.ToString("yyyy-MM-dd");
                docHdr.VENDORID = receipt.Vendor.AccountCode;

                //Define el Auto Cost
                docHdr.AUTOCOST = costZero ? 0 : 1;


                docHdr.NOTETEXT = "WMS DOC#: " + receipt.DocNumber;
                docHdr.VNDDOCNM = receipt.Reference;
                docHdr.DUEDATE = (receipt.Date5 == null) ? "" : receipt.Date5.Value.ToString("yyyy-MM-dd");

                docType.taPopRcptHdrInsert = docHdr;

                POPReceivingsType[] docTypeArray = new POPReceivingsType[1];
                docTypeArray[0] = docType;

                //Create an eConnect XML document object and populate its docType property with
                //the docType schema object
                eConnect = new eConnectType();
                eConnect.POPReceivingsType = docTypeArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                return true;
            }

            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("CreatePurchaseReceipt: ", ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }


        private IList<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert> 
            GetReceiptLineSerials(IList<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert> result, IList<NodeTrace> traceList)
        {
            //IList<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert> result = new List<taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert>();

            taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert curSerial;

            //foreach (NodeTrace trace in traceList) {
            //    curSerial = new taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert();
            //    curSerial.ITEMNMBR = trace.Label.Product.ProductCode;
            //    curSerial.POPRCTNM = trace.PostingDocument.DocNumber;
            //    curSerial.RCPTLNNM = trace.PostingDocLineNumber;
            //    curSerial.SERLTNUM = trace.Label.SerialNumber;

            //    result.Add(curSerial);
            //}

            foreach (NodeTrace trace in traceList)
            {
                if (trace.Label.LabelType.DocTypeID == LabelType.UniqueTrackLabel) {
                    curSerial = new taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert();
                    curSerial.ITEMNMBR = trace.Label.Product.ProductCode;
                    curSerial.POPRCTNM = trace.PostingDocument.DocNumber;
                    curSerial.RCPTLNNM = trace.PostingDocLineNumber;
                    curSerial.SERLTNUM = trace.Label.SerialNumber;

                    result.Add(curSerial);
                }


                foreach (Label label in trace.Label.ChildLabels)
                {
                    if (trace.Label.LabelType.DocTypeID != LabelType.UniqueTrackLabel)
                    {
                        curSerial = new taPopRcptSerialInsert_ItemsTaPopRcptSerialInsert();
                        curSerial.ITEMNMBR = label.Product.ProductCode;
                        curSerial.POPRCTNM = trace.PostingDocument.DocNumber;
                        curSerial.RCPTLNNM = trace.PostingDocLineNumber;
                        curSerial.SERLTNUM = label.SerialNumber;

                        result.Add(curSerial);
                    }
                }
            }


            return result;
        }


        private IList<taPopRcptLotInsert_ItemsTaPopRcptLotInsert> 
            GetReceiptLineLots(IList<taPopRcptLotInsert_ItemsTaPopRcptLotInsert> result, IList<NodeTrace> traceList)
        {
            //IList<taPopRcptLotInsert_ItemsTaPopRcptLotInsert> result = new List<taPopRcptLotInsert_ItemsTaPopRcptLotInsert>();

            //Lot Collection
            IDictionary<string, double> lotBalance = new Dictionary<string, double>();


            taPopRcptLotInsert_ItemsTaPopRcptLotInsert curLotCode;

            foreach (NodeTrace trace in traceList)
            {
                if (lotBalance.ContainsKey(trace.Label.LotCode))
                    lotBalance[trace.Label.LotCode] += trace.Quantity*trace.Label.Unit.BaseAmount;
                else
                    lotBalance.Add(trace.Label.LotCode, trace.Quantity * trace.Label.Unit.BaseAmount);

            }


            //Recorre la coleccion de lotBalance para obtener el consolidado y retornarlo
            foreach (string lotCode in lotBalance.Keys)
            {
                if (lotBalance[lotCode] > 0)
                {
                    curLotCode = new taPopRcptLotInsert_ItemsTaPopRcptLotInsert();
                    curLotCode.ITEMNMBR = traceList[0].Label.Product.ProductCode;
                    curLotCode.POPRCTNM = traceList[0].PostingDocument.DocNumber;
                    curLotCode.RCPTLNNM = traceList[0].PostingDocLineNumber;
                    curLotCode.SERLTNUM = lotCode;
                    curLotCode.SERLTQTY = (Decimal)lotBalance[lotCode];
                    result.Add(curLotCode);
                }
            }

                        
            return result;
        }

        //Valida si la relacion Vendor-Item Existe, si no la Crea
        private void ValidateItemAndVendor(string vendorID, string itemNumber)
        {
            try
            {
                //Ask for the vendor Item record
                string sWhere = "VENDORID='" + vendorID + "' AND ITEMNMBR = '" + itemNumber + "'";
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("VendorItem", false, 2, 0, sWhere, true));

                if (ds.Tables.Count == 0)
                    CreateVendorItem(vendorID, itemNumber);

            }
            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("ValidateItemAndVendor: ", ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("ValidateItemAndVendor:" + WriteLog.GetTechMessage(ex));
            }
        }


        //Valida si la relacion Location-Item Existe, si no la Crea
        private void ValidateItemAndLocation(string locationCode, string itemNumber)
        {
            try
            {
                //Ask for the vendor Item record
                string sWhere = "ITEMNMBR = '" + itemNumber + "'";
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("Item", false, 2, 0, sWhere, true));

                //Recorre la tabla 2 - Quantities y Busca la Bodega
                if (ds.Tables[2].Select("LOCNCODE='" + locationCode + "'").Length == 0)
                    CreateLocationItem(locationCode, itemNumber);

            }

            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("ValidateItemAndLocation: ", ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("ValidateItemAndLocation: " + WriteLog.GetTechMessage(ex));
            }

        }


        private void CreateVendorItem(string vendorID, string itemNumber)
        {
            eConnectType eConnect;

            try
            {
                taCreateItemVendors_ItemsTaCreateItemVendors record = new taCreateItemVendors_ItemsTaCreateItemVendors();
                record.ITEMNMBR = itemNumber;
                record.VENDORID = vendorID;

                IVVendorItemType curType = new IVVendorItemType();
                curType.taCreateItemVendors_Items = new taCreateItemVendors_ItemsTaCreateItemVendors[] { record };


                IVVendorItemType[] typeArray = new IVVendorItemType[] { curType };

                eConnect = new eConnectType(); 
                eConnect.IVVendorItemType = typeArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());
            }

            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("CreateVendorItem: ", ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("CreateVendorItem: "+ ex.Message);
            }

        }


        private void CreateLocationItem(string locationCode, string itemNumber)
        {
            eConnectType eConnect;

            try
            {
                taItemSite_ItemsTaItemSite record = new taItemSite_ItemsTaItemSite();
                record.ITEMNMBR = itemNumber;
                record.LOCNCODE = locationCode;
                record.UpdateIfExists = 1;

                IVInventoryItemSiteType curType = new IVInventoryItemSiteType();
                curType.taItemSite_Items = new taItemSite_ItemsTaItemSite[] { record };              

                IVInventoryItemSiteType[] typeArray = new IVInventoryItemSiteType[] { curType };

                eConnect = new eConnectType();
                eConnect.IVInventoryItemSiteType = typeArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());
            }

            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("CreateLocationItem: ", ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("CreateLocationItem: " + ex.Message);
            }

        }


        /// <summary>
        /// Go to the ERP system to get the posted status for the current document, is posted return
        /// the document with posted dates else returns null.
        /// </summary>
        /// <param name="postedReceipt"></param>
        /// <returns></returns>
        public Document GetReceiptPostedStatus(Document postedReceipt)
        {

            try
            {
                string sWhere = "POPRCTNM='" + postedReceipt.DocNumber + "'";

                //En GP la fecha null es 1900-01-01
                DateTime nullGpDate = DateTime.Parse("1900-01-01");

                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSPurchaseReceipt", false, 2, 0, sWhere, true));

                if (ds != null && ds.Tables.Count > 1 &&  ds.Tables[1] != null &&  ds.Tables[1].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[1].Rows[0];

                    //Evaluar en el encabezado si el documento esta posted (fecha y User)
                    DateTime postedDate = DateTime.Parse(dr["POSTEDDT"].ToString());
                    string postedUser = dr["PTDUSRID"].ToString();

                    if (!string.IsNullOrEmpty(postedUser) && postedDate != nullGpDate)
                    {
                        if (postedReceipt.Comment == null) postedReceipt.Comment = "";
                        postedReceipt.Comment = postedReceipt.Comment + " Erp posted date: " + postedDate.ToString() + ", Erp posted user: " + postedUser;
                        return postedReceipt;
                    }
                    else
                        return null;
                }

                //retornar la lista 
                return null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetReceiptPostedStatus", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }



        }

        #endregion




        public Boolean CreateInventoryAdjustment(Document inventoryAdj, short adjType) 
            //adjType: Ajustment or Variance
        {

            eConnectType eConnect;

            try
            {
                taIVTransactionLineInsert_ItemsTaIVTransactionLineInsert[] docLines = 
                    new taIVTransactionLineInsert_ItemsTaIVTransactionLineInsert[inventoryAdj.DocumentLines.Count];

                //Create an object that holds XML node object
                taIVTransactionLineInsert_ItemsTaIVTransactionLineInsert curLine;
                int i = 1;
                int sign;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in inventoryAdj.DocumentLines)
                {
                    curLine = new taIVTransactionLineInsert_ItemsTaIVTransactionLineInsert();

                    // Populate Lines            
                    curLine.ITEMNMBR = dr.Product.ProductCode;
                    curLine.IVDOCNBR = inventoryAdj.DocNumber;
                    curLine.IVDOCTYP = (short)((adjType == GP_DocType.IV_Variance) ? GP_DocType.IV_Variance : GP_DocType.IV_Adjustment);
                    curLine.LNSEQNBR = i;
                    curLine.TRXLOCTN = inventoryAdj.Location.ErpCode;
                    curLine.UOFM = dr.Unit.ErpCode;
                    
                    /*
                    Override quantity flag; to use this element, inventory control must allow for adjustment overrides:
                    0=Reset TRXQTY with quantity available; 
                    1=Override quantity available and use TRXQTY 
                    Se debe configurar en el GP la Opcion en el Inventory Control 
                     */
                    curLine.OverrideQty = 1; 

                    sign = (dr.IsDebit == true) ? -1 : 1;
                    curLine.TRXQTY = sign * Decimal.Parse(dr.Quantity.ToString());
                   
                    docLines[i-1] = curLine;
                    i++;
                }

                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                IVInventoryTransactionType docType = new IVInventoryTransactionType();
                docType.taIVTransactionLineInsert_Items = docLines;

                //Create a taSopHdrIvcInsert XML node object
                taIVTransactionHeaderInsert docHdr = new taIVTransactionHeaderInsert();
                

                //Populate Header  
                //BATCH si hay notes, ahi se pone el BATCH
                if (string.IsNullOrEmpty(inventoryAdj.Notes))
                    docHdr.BACHNUMB = inventoryAdj.Location.ErpCode + "_" + GPBatchNumber.Inventory;
                else
                    docHdr.BACHNUMB = inventoryAdj.Notes;

                docHdr.DOCDATE = ((DateTime)inventoryAdj.Date1).ToString("yyyy-MM-dd"); //DateTime.Today.ToString();
                docHdr.IVDOCNBR = inventoryAdj.DocNumber; //IA00119

                //Determina si crea un adjustment o variance.
                docHdr.IVDOCTYP = (short)((adjType == GP_DocType.IV_Variance) ? GP_DocType.IV_Variance : GP_DocType.IV_Adjustment);
                
                
                docHdr.NOTETEXT = inventoryAdj.Comment;


                docType.taIVTransactionHeaderInsert = docHdr;

                IVInventoryTransactionType[] docTypeArray = new IVInventoryTransactionType[1];
                docTypeArray[0] = docType;

                //Create an eConnect XML document object and populate its docType property with
                //the docType schema object
                eConnect = new eConnectType();
                eConnect.IVInventoryTransactionType = docTypeArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                return true;
            }

            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
                //ExceptionMngr.WriteEvent("CreateInventoryAdjustment:" + inventoryAdj.DocNumber, ListValues.EventType.Error, 
                //    ex, null, ListValues.ErrorCategory.ErpConnection);
                //return false;
            }

        }


        public Boolean CreateLocationTransfer(Document trfDocument)
        {
            eConnectType eConnect;

            Console.WriteLine("In ErpConnect");

            try
            {
                taIVTransferLineInsert_ItemsTaIVTransferLineInsert[] docLines =
                    new taIVTransferLineInsert_ItemsTaIVTransferLineInsert[trfDocument.DocumentLines.Count];

                //Create an object that holds XML node object
                taIVTransferLineInsert_ItemsTaIVTransferLineInsert curLine;
                int i = 1;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in trfDocument.DocumentLines)
                {
                    curLine = new taIVTransferLineInsert_ItemsTaIVTransferLineInsert();

                    // Populate Lines            
                    curLine.ITEMNMBR = dr.Product.ProductCode;
                    curLine.IVDOCNBR = trfDocument.DocNumber;
                    curLine.LNSEQNBR = i;
                    curLine.TRXLOCTN = dr.Location.ErpCode; //ORIGEN
                    curLine.TRNSTLOC = dr.Location2.ErpCode; //DESTINO - ERP CODE
                    curLine.UOFM = dr.Unit.ErpCode;

                    /*
                    Override quantity flag; to use this element, inventory control must allow for adjustment overrides:
                    0=Reset TRXQTY with quantity available; 
                    1=Override quantity available and use TRXQTY 
                    Se debe configurar en el GP la Opcion en el Inventory Control 
                     */

                    curLine.OverrideQty = 1;
                    curLine.TRXQTY = Decimal.Parse(dr.Quantity.ToString());

                    docLines[i - 1] = curLine;
                    i++;
                }

                Console.WriteLine("Lines created");

                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                IVInventoryTransferType docType = new IVInventoryTransferType();
                docType.taIVTransferLineInsert_Items = docLines;

                //Create a taSopHdrIvcInsert XML node object
                taIVTransferHeaderInsert docHdr = new taIVTransferHeaderInsert();


                //Populate Header  
                //BATCH si hay notes, ahi se pone el BATCH
                if (string.IsNullOrEmpty(trfDocument.Notes))
                    docHdr.BACHNUMB = trfDocument.Location.ErpCode + "_" + GPBatchNumber.Inventory;
                else
                    docHdr.BACHNUMB = trfDocument.Notes;

                docHdr.DOCDATE = ((DateTime)trfDocument.Date1).ToString("yyyy-MM-dd"); //DateTime.Today.ToString();
                docHdr.IVDOCNBR = trfDocument.DocNumber; //TRF000X
                docHdr.POSTTOGL = 1;
                docHdr.USRDEFND4 = trfDocument.Comment;
                docHdr.USRDEFND5 = "";
                docType.taIVTransferHeaderInsert = docHdr;

                Console.WriteLine("Header created");

                IVInventoryTransferType[] docTypeArray = new IVInventoryTransferType[1];
                docTypeArray[0] = docType;

                //Create an eConnect XML document object and populate its docType property with
                //the docType schema object
                eConnect = new eConnectType();
                eConnect.IVInventoryTransferType = docTypeArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());
                Console.WriteLine("Send to GP");

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                throw new Exception(WriteLog.GetTechMessage(ex));
                //ExceptionMngr.WriteEvent("CreateInventoryAdjustment:" + inventoryAdj.DocNumber, ListValues.EventType.Error, 
                //    ex, null, ListValues.ErrorCategory.ErpConnection);
                //return false;
            }
        }


        /// <summary>
        /// Go to the ERP system to get the posted status for the current document, is posted return
        /// the document with posted dates else returns null.
        /// </summary>
        /// <param name="postedReceipt"></param>
        /// <returns></returns>
        public Document GetAdjustmentPostedStatus(Document document)
        {

            try
            {
                string sWhere = "IVDOCNBR='" + document.DocNumber + "'";

                //En GP la fecha null es 1900-01-01
                DateTime nullGpDate = DateTime.Parse("1900-01-01");

                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSInventoryAdjustment", false, 2, 0, sWhere, true));

                if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[1].Rows[0];

                    //Evaluar en el encabezado si el documento esta posted (fecha y User)
                    DateTime postedDate = DateTime.Parse(dr["GLPOSTDT"].ToString());
                    //string postedUser = dr["PTDUSRID"].ToString();

                    if (postedDate != nullGpDate)
                    {
                        if (document.Comment == null) document.Comment = "";
                        document.Comment = document.Comment + " Erp posted date: " + postedDate.ToString(); //+", Erp posted user: " + postedUser;
                        return document;
                    }
                    else
                        return null;
                }

                //retornar la lista 
                return null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetAdjustmentPostedStatus", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }



        }



        public Document GetAssemblyOrderPostedStatus(Document order)
        {

            try
            {
                string sWhere = " TRX_ID='" + order.DocNumber + "'";

                //En GP la fecha null es 1900-01-01
                DateTime nullGpDate = DateTime.Parse("1900-01-01");

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                Command.Connection.Open();

                // BM30220 - Document posted
                ds = ReturnDataSet("SELECT * FROM BM30200 WHERE 1=1 ", sWhere, "BM30200", Command.Connection);

                if (ds != null && ds.Tables[0] != null &&  ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];

                    //Evaluar en el encabezado si el documento esta posted (fecha y User)
                    DateTime postedDate = DateTime.Parse(dr["POSTEDDT"].ToString());
                    string postedUser = dr["PTDUSRID"].ToString();

                    if (!string.IsNullOrEmpty(postedUser) && postedDate != nullGpDate)
                    {
                        if (order.Comment == null) order.Comment = "";
                        order.Comment = order.Comment + " Erp posted date: " + postedDate.ToString() + ", Erp posted user: " + postedUser;
                        
                        return order;
                    }
                    else
                        return null;
                }

                //retornar la lista 
                return null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetAssemblyOrderPostedStatus", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }



        }



        public String CreateKitAssemblyOrderBasedOnSalesDocument(Document shipment, Product product, 
            double quantity, string sequence)
        {
            //use DataTable Management to insert Data.
            //Crea el Header en la tabla BM10200
            //Crea el Header en la tabla BM10300

            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            SqlDataAdapter objAdapter;
            SqlCommandBuilder ObjCmdBuilder;
            String today = DateTime.Today.ToString("yyyy-MM-dd");
            string asmOrder = shipment.DocNumber + sequence;

            //CREANDO EL ASM HEADER
            string sqlTableHeader = "BM10200";
            DataSet dsTmp = new DataSet(sqlTableHeader);

            objAdapter = new SqlDataAdapter("SELECT * FROM " + sqlTableHeader + " WHERE 1=2", Command.Connection);
            objAdapter.Fill(dsTmp, sqlTableHeader);


            DataRow objRow = dsTmp.Tables[0].NewRow();

            //Asignando el Header usando los dato del documento
            objRow["TRX_ID"] = asmOrder; //Document number mas ASM Sequence
            objRow["BM_Trx_Status"] = "1";
            
            objRow["BM_Start_Date"] = today;
            objRow["TRXDATE"] = today;
            objRow["REFRENCE"] = shipment.CustPONumber;
            objRow["USER2ENT"] = shipment.CreatedBy;
            objRow["CREATDDT"] = today;
            objRow["PSTGDATE"] = today;
            objRow["USERDEF1"] = "WMSEXPRESS";
            //objRow["BACHNUMB"] = WType.GetCompanyOption(CurCompany, "ASMBACHNUM");


            dsTmp.Tables[0].Rows.Add(objRow);

            //Actualizando el Header
            ObjCmdBuilder = new SqlCommandBuilder(objAdapter);
            objAdapter.Update(dsTmp, sqlTableHeader);




            //CREANDO EL ASM DETAIL
            string sqlTableDetail = "BM10300";
            dsTmp = new DataSet(sqlTableDetail);

            objAdapter = new SqlDataAdapter("SELECT * FROM " + sqlTableDetail + " WHERE 1=2", Command.Connection);
            objAdapter.Fill(dsTmp, sqlTableDetail);


            //Adicionando el Kit/Assemblie - El componente creado
            objRow = dsTmp.Tables[0].NewRow();

            //Asignacion de las lineas del Documento al DataRow de la orden
            objRow["TRX_ID"] = asmOrder;
            objRow["Component_ID"] = 0;
            objRow["Parent_Component_ID"] = -1;
            objRow["ITEMNMBR"] = product.ProductCode;
            objRow["ITEMDESC"] = product.Name;
            objRow["UOFM"] = product.BaseUnit.ErpCode;
            objRow["LOCNCODE"] = shipment.Location.ErpCode;
            objRow["Stock_Quantity"] = 0;  //Stock - Formula
            objRow["Assemble_Quantity"] = quantity;
            objRow["ATYALLOC"] = 0;
            objRow["BM_Stock_Method"] = 3;
            objRow["Cost_Type"] = "1";
            objRow["Design_Quantity"] =0; //formula (unit base)
            objRow["Standard_Quantity"] = quantity;   //formula (unit base))
            objRow["Extended_Standard_Quantity"] =  quantity; //Qty Extended
            objRow["BM_Component_Type"] = 2;  //2 - Main, 1 - Component
            objRow["Lvl"] = 0; //0-Main, 1 - Level1
            objRow["QTYBSUOM"] = 1;
            objRow["INVINDX"] = WType.GetCompanyOption(CurCompany, "ERPINVACCT");
            objRow["Variance_Index"] = WType.GetCompanyOption(CurCompany, "ERPVARACCT");


            dsTmp.Tables[0].Rows.Add(objRow);





            //int i = 1;
            //foreach (KitAssemblyFormula dl in product.ProductFormula)
            //{

            //    objRow = dsTmp.Tables[0].NewRow();

            //    //Asignacion de las lineas del Documento al DataRow de la orden
            //    objRow["TRX_ID"] = asmOrder;
            //    objRow["Component_ID"] = 16384*i;
            //    objRow["Parent_Component_ID"] = 0;
            //    objRow["ITEMNMBR"] = dl.Component.ProductCode;
            //    objRow["ITEMDESC"] = dl.Component.Name;
            //    objRow["UOFM"] = dl.Component.BaseUnit.ErpCode;
            //    objRow["LOCNCODE"] = shipment.Location.ErpCode;
            //    objRow["Stock_Quantity"] = dl.FormulaQty;  //Stock - Formula
            //    objRow["Assemble_Quantity"] = 0;
            //    objRow["ATYALLOC"] = 0;
            //    objRow["BM_Stock_Method"] = 2;
            //    objRow["Cost_Type"] = "1";
            //    objRow["Design_Quantity"] = dl.FormulaQty; //formula (unit base)
            //    objRow["Standard_Quantity"] = dl.FormulaQty;   //formula (unit base))
            //    objRow["Extended_Standard_Quantity"] = dl.FormulaQty*quantity; //Qty Extended
            //    objRow["BM_Component_Type"] = 1;  //2 - Main, 1 - Component
            //    objRow["Lvl"] = 1; //0-Main, 1 - Level1
            //    objRow["QTYBSUOM"] = 1;
            //    //objRow["DECPLQTY"] = "";
            //    //objRow["DECPLCUR"] = "";
            //    //objRow["ITMTRKOP"] = "";

            //    dsTmp.Tables[0].Rows.Add(objRow);
            //    i++;
            //}

            //Actualizando el Detail
            ObjCmdBuilder = new SqlCommandBuilder(objAdapter);
            objAdapter.Update(dsTmp, sqlTableDetail);



            return "";
        }



        #region Shipping 




        public Boolean FulFillSalesDocument(Document ffDocument, string shortAgeOption, bool fistTimeFulfill, string batchNo)
        {

            SqlDataAdapter objAdapter;
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            try
            {


                string GpTable = "SOP10200";
                string SopType = "2";

                string condition = " WHERE SOPTYPE = " + SopType + " AND SOPNUMBE='" + ffDocument.CustPONumber + "'";

                //Llenado los dataset con los registros del sales document 
                DataSet ds = new DataSet(GpTable);
                objAdapter = new SqlDataAdapter("SELECT * FROM SOP10100 " + condition, Command.Connection);
                objAdapter.Fill(ds, GpTable);


                if (ds == null || ds.Tables == null || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Document is not ready to fulfill or was already invoiced.");

                Double QtyFulFi = 0.0, QtyToBalance = 0.0;

                //Aqui se define a donde debe ir el Balance si a BackOrder o a Cancelacion
                shortAgeOption = string.IsNullOrEmpty(shortAgeOption) ? ShortAge.BackOrder : shortAgeOption;

                foreach (DocumentLine dr in ffDocument.DocumentLines)
                {
                    //Hace fullfill solo si fue piqueda o si es un KIT con componentes piqueados.
                    if (dr.Quantity <= 0 && dr.Note != "2")
                        continue;

                    //Ajustando las cantidades a fullfill del kit, verifica primero si hay inventario en GP para poder
                    //Procesar la orden, si no envia una error.
                    if (dr.Note == "2" && dr.QtyAllocated > 0)
                    {
                        if (!ValidateProductInErpInventory(ffDocument.Location.ErpCode, dr.Product.ProductCode, dr.QtyAllocated, Command.Connection))
                            throw new Exception("No stock available to fulfill product: " + dr.Product.FullDesc + ".\nPlease check.");

                        dr.Quantity = dr.QtyAllocated;
                    }


                    //Me dice que linea del documento esta linkeada
                    DataSet dsr;
                    DataRow dLine;
                    string strBalance = "";

                    if (!string.IsNullOrEmpty(dr.LinkDocNumber))
                    {

                        dsr = ReturnDataSet("SELECT * FROM " + GpTable + " WHERE 1=1 ",
                            " SOPTYPE = " + SopType + " AND SOPNUMBE='" + ffDocument.CustPONumber + "' AND LNITMSEQ=" + dr.LinkDocLineNumber,
                            GpTable, Command.Connection);


                        if (dsr == null || dsr.Tables.Count == 0 || dsr.Tables[0].Rows.Count == 0)
                            continue;


                        dLine = dsr.Tables[0].Rows[0];

                        //Al crear una linea se llenan QUANTITY, ATYALLOC, QTYREMAI, QTYTOINV - INFORMATIVO
                        //Al hacer fullfill se Ajustan: Invoice, FulFill, BackOrder (?) or Cancel
                        //                      QUANTITY	ATYALLOC	QTYREMAI	QTYTOINV	FulFill	AtySlCTD	QtyToBAo	QtyPrvINV
                        //Creacion de la Orden	15	        15	        15	        15	        0	    0	        0	        0
                        //Al poner Full Fill	15	        10	        15	        10	        10	    10	        5	        0
                        //Despues de facturar	15	        0	        5	        0	        0	    0	        5	        10

                        if (fistTimeFulfill)
                        {
                            QtyFulFi = dr.Quantity;
                            //Balance para cancelar o para backOrder
                            QtyToBalance = Double.Parse(dLine["QUANTITY"].ToString())
                                - Double.Parse(dLine["QTYPRINV"].ToString()) - QtyFulFi;
                        }
                        else
                        {
                            QtyFulFi = dr.Quantity; //Double.Parse(objDBRow["QTYFULFI"].ToString()) +
                            QtyToBalance = Double.Parse(dLine["QUANTITY"].ToString()) -
                                Double.Parse(dLine["QTYPRINV"].ToString()) - QtyFulFi;
                        }


                        //if (QtyToBalance > 0)
                        //{
                        //el backorder debe pasar al Cero cuando no hay balance.
                        bool useBalance = false;
                        if (shortAgeOption == ShortAge.BackOrder)
                        {
                            strBalance = " QTYTBAOR = @QtyBal, "; //BackOrder Si asi lo indica el setup
                            useBalance = true;
                        }
                        else if (shortAgeOption == ShortAge.Cancel)
                        {
                            strBalance = " QTYCANCE = @QtyBal, ";
                            useBalance = true;
                        }
                        //}


                        //Header
                        Command.CommandText = "UPDATE " + GpTable + " SET QTYFULFI=@qtyFulfi, QTYTOINV=@qtyInv, "
                            + strBalance + " FUFILDAT = @ffDat, ACTLSHIP = @actShip, CONTNBR = @Mark, FAXNUMBR = @Shipment WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber AND LNITMSEQ = @lineSeq ";

                        Command.Parameters.Clear();
                        // Create and prepare an SQL statement.
                        AddParameter("@qtyFulfi", QtyFulFi);
                        AddParameter("@qtyInv", QtyFulFi);
                        AddParameter("@ffDat", DateTime.Today);
                        AddParameter("@actShip", DateTime.Today);   
                        AddParameter("@sopnumber", ffDocument.CustPONumber);
                        AddParameter("@lineSeq", dr.LinkDocLineNumber);
                        AddParameter("@Mark", "WMS_FF");
                        AddParameter("@Shipment", ffDocument.DocNumber);

                        //if (QtyToBalance > 0)
                        if (useBalance)
                            AddParameter("@QtyBal", QtyToBalance);

                        // Call Prepare after setting the Commandtext and Parameters.
                        Command.ExecuteNonQuery();

                    }

                }

                //Manda a BO or Cancel las lineas no procesadas.
                //Abril 9 de 2010
                try
                {
                    //Header
                    if (shortAgeOption == ShortAge.Cancel)
                        Command.CommandText = "UPDATE SOP10200 SET  QTYCANCE = QUANTITY, QTYTOINV = 0, ATYALLOC = 0 WHERE SOPTYPE=2 AND SOPNUMBE = @sopNumbe AND QTYFULFI=0  AND QTYTBAOR = 0 ";
                    else //Back Order
                        Command.CommandText = "UPDATE SOP10200 SET  QTYTBAOR = QUANTITY, QTYTOINV = 0, ATYALLOC = 0 WHERE SOPTYPE=2 AND SOPNUMBE = @sopNumbe AND QTYFULFI=0  AND QTYCANCE = 0 ";


                    Command.Parameters.Clear();
                    // Create and prepare an SQL statement.
                    AddParameter("@sopNumbe", ffDocument.CustPONumber);
                    Command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("Reset BO: " + ffDocument.CustPONumber, ListValues.EventType.Error, ex, null,
                            ListValues.ErrorCategory.ErpConnection);
                }


                //ACTUALIZA EL BATCH SI NO ES NULL
                if (!string.IsNullOrEmpty(batchNo))
                    UpdateSalesDocumentBatch(ffDocument.CustPONumber, batchNo);



                return true;

            }
            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("FulFillSalesDocument: " + ffDocument.CustPONumber, ListValues.EventType.Error, ex, null,
                //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("FulFillSalesDocument:" + WriteLog.GetTechMessage(ex));
            }
            finally {
                if (Command.Connection.State != ConnectionState.Open)
                    Command.Connection.Close();
            }

        }



        public Boolean FulFillMergedSalesDocument(Document ssDocument, IList<DocumentLine> origLines, bool fistTimeFulfill, string batchNumber)
        {

            if (origLines == null || origLines.Count == 0)
                throw new Exception("No lines in original documents.");

            //SqlDataAdapter objAdapter;
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            string GpTable = "SOP10200";
            string SopType = "2";
            string shortAgeOption = ssDocument.DocType.Comment;

            try
            {
                Double QtyFulFi = 0.0, QtyToBalance = 0.0, QtyBo = 0.0, QtyCancel = 0.0;

                DataSet dsr;
                DataRow dLine;
                string strBalance = "";
                DocumentLine curOriLine = null;

                //Aqui se define a donde debe ir el Balance si a BackOrder o a Cancelacion
                foreach (DocumentLine dr in ssDocument.DocumentLines)
                {
                    //Me dice que linea del documento esta linkeada
                    if (string.IsNullOrEmpty(dr.LinkDocNumber))
                        continue;

                    //Obtiene los datos del documento Original Document y Line Number.
                    curOriLine = origLines.Where(f => f.LineNumber == dr.LinkDocLineNumber).First();

                    QtyCancel = curOriLine.QtyCancel;
                    QtyBo = curOriLine.QtyBackOrder;
                    QtyFulFi = dr.Quantity;


                    dsr = ReturnDataSet("SELECT * FROM " + GpTable + " WHERE 1=1 ",
                        " SOPTYPE = " + SopType + " AND SOPNUMBE='" + curOriLine.LinkDocNumber + "' AND LNITMSEQ=" + curOriLine.LinkDocLineNumber,
                        GpTable, Command.Connection);

                    if (dsr == null || dsr.Tables.Count == 0 || dsr.Tables[0].Rows.Count == 0)
                        continue;

                    dLine = dsr.Tables[0].Rows[0];

                    //Balance para cancelar o para backOrder
                    QtyToBalance = Double.Parse(dLine["QUANTITY"].ToString())
                        - Double.Parse(dLine["QTYPRINV"].ToString()) - QtyFulFi - QtyBo - QtyCancel;


                    if (QtyToBalance > 0)
                    {
                        if (shortAgeOption == ShortAge.Cancel)
                            QtyCancel += QtyToBalance;
                        else                            
                            QtyBo += QtyToBalance; //BACKORDER por defecto
                    }


                    //Header
                    /*
                    Command.CommandText = "UPDATE " + GpTable + " SET QTYFULFI=@qtyFulfi, ATYALLOC=@qtyAlloc, QTYTOINV=@qtyInv, "
                        + strBalance + " FUFILDAT = @ffDat, ACTLSHIP = @actShip, QTYTBAOR = @qtyBO,  QTYCANCE = @qtyCancel, CONTNBR = @Mark, FAXNUMBR = @Shipment "
                    + " WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber AND LNITMSEQ = @lineSeq ";
                    */

                    Command.CommandText = "UPDATE " + GpTable + " SET QTYFULFI=@qtyFulfi, QTYSLCTD=@qtySol, QTYTOINV=@qtyInv, ATYALLOC=@qtyAlloc,"
                    + strBalance + " FUFILDAT = @ffDat, ACTLSHIP = @actShip, QTYTBAOR = @qtyBO,  QTYCANCE = @qtyCancel, CONTNBR = @Mark, FAXNUMBR = @Shipment, "
                    + " EXTQTYAL = EXTQTYAL + ATYALLOC WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber AND LNITMSEQ = @lineSeq ";

                    Command.Parameters.Clear();
                    // Create and prepare an SQL statement.
                    AddParameter("@qtyFulfi", QtyFulFi);
                    AddParameter("@qtyInv", QtyFulFi);
                    AddParameter("@qtyAlloc", QtyFulFi);
                    AddParameter("@qtySol", QtyFulFi);
                    AddParameter("@qtyBO", QtyBo);
                    AddParameter("@qtyCancel", QtyCancel);
                    AddParameter("@ffDat", DateTime.Today);
                    AddParameter("@actShip", DateTime.Today);
                    AddParameter("@sopnumber", curOriLine.LinkDocNumber);
                    AddParameter("@lineSeq", curOriLine.LinkDocLineNumber);
                    AddParameter("@Mark", "WMS_FF");
                    AddParameter("@Shipment", ssDocument.DocNumber);

                    // Call Prepare after setting the Commandtext and Parameters.
                    Command.ExecuteNonQuery();


                    //Actualiza las lineas de inventario.
                    Command.Parameters.Clear();
                    Command.CommandText = "UPDATE IV00102 SET ATYALLOC = ATYALLOC + @WMS_QUANTITY_FULFILLED, QTYBKORD = QTYBKORD + @QTYBO  WHERE ITEMNMBR = @ITEMNMBR AND (RCRDTYPE = 1 OR LOCNCODE = @LOCNCODE)";
                    AddParameter("@WMS_QUANTITY_FULFILLED", QtyFulFi);
                    AddParameter("@QTYBO", QtyBo);
                    AddParameter("@ITEMNMBR", dr.Product.ProductCode);
                    AddParameter("@LOCNCODE", ssDocument.Location.ErpCode);                    
                    Command.ExecuteNonQuery();
                    


                }


                //Actualiza las lineas canceladas y las de BO
                /*
                try
                {
                    foreach (DocumentLine origLine in origLines.Where(f => f.QtyCancel > 0 || f.QtyBackOrder > 0))
                    {
                        try
                        {

                            if (origLine.QtyBackOrder > 0 || origLine.QtyCancel > 0)
                            {

                                //Header
                                Command.CommandText = "UPDATE " + GpTable + " SET QTYTBAOR = @qtyBO,  QTYCANCE = @qtyCancel"
                                + " WHERE SOPTYPE = 2 AND QTYFULFI=0, QTYTOINV=0  AND SOPNUMBE = @sopnumber AND LNITMSEQ = @lineSeq AND CONTNBR = @Mark ";

                                Command.Parameters.Clear();
                                // Create and prepare an SQL statement.                       
                                AddParameter("@qtyBO", origLine.QtyBackOrder);
                                AddParameter("@qtyCancel", origLine.QtyCancel);
                                AddParameter("@sopnumber", origLine.Document.DocNumber);
                                AddParameter("@lineSeq", origLine.LineNumber);
                                AddParameter("@Mark", "WMS_FF");

                                // Call Prepare after setting the Commandtext and Parameters.
                                Command.ExecuteNonQuery();
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                */


                //Saca los documentos origen del Merge Order
                foreach (String origDoc in origLines.Select(f => f.LinkDocNumber).Distinct())
                {
                    //ACTUALIZA EL BATCH SI NO ES NULL
                    if (!string.IsNullOrEmpty(batchNumber))
                        UpdateSalesDocumentBatch(origDoc, batchNumber);

                    UpdateSOP10106(origDoc, ssDocument.DocNumber);
                }


                return true;

            }
            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("FulFillSalesDocument: " + ffDocument.CustPONumber, ListValues.EventType.Error, ex, null,
                //ListValues.ErrorCategory.ErpConnection);

                throw new Exception("FulFillMergedSalesDocument:" + WriteLog.GetTechMessage(ex));
            }
            finally
            {
                if (Command.Connection.State != ConnectionState.Open)
                    Command.Connection.Close();
            }
        }


        #region OLD Fulfill
        /*

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ffDocument">New Document create for the fulfill transaction</param>
        /// <param name="curConnection">SQL ERP connection string</param>
        /// <param name="shortAgeOption">Option to do when qty over stock</param>
        /// <returns></returns>
        /// 
         
        public Boolean FulFillSalesDocument(Document ffDocument, string shortAgeOption, bool fistTimeFulfill)
        {

            //En Dynamics GP el sistema para que funcione WMS debe estar en "ALLOCATE BY NONE"

            //Se ejecuta directo a SQL porque el sistema de GP, es muy sensible a la actualizacion de documentos
            //e introduce problemas y modificaciones al modificar el documento completamente usando eConnect o WebServices.

            //Debe actualizar las cantidades fullfilled de las lineas del documento
            //Debe hacerlo directo por SQL sobre la tabla SOP10200 - Detalle de Invoice
            //Debe Actualizar la fecha de fullfill y poner un coment que lo relacione con WMS

            try
            {

                SqlDataAdapter objAdapter;
                this.Connection =  new SqlConnection(ffDocument.Company.ErpConnection.CnnString);
                Command.Connection = this.Connection;
                SqlCommandBuilder ObjCmdBuilder;
                DataRow objDBRow = null;

                string GpTable = "SOP10200";
                string SopType = "2";

                string condition = " WHERE SOPTYPE = " + SopType + " AND SOPNUMBE='" + ffDocument.CustPONumber + "'";

                //Llenado los dataset con los registros del sales document 
                DataSet ds = new DataSet(GpTable);
                objAdapter = new SqlDataAdapter("SELECT * FROM " + GpTable + condition, Connection);
                objAdapter.Fill(ds, GpTable);


                if (ds == null || ds.Tables == null || ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Document is not ready to fulfill or was already invoiced.");


                Double QtyFulFi = 0.0, QtyToInv = 0.0, QtyToBalance = 0.0;

                //Aqui se define a donde debe ir el Balance si a BackOrder o a Cancelacion
                shortAgeOption = string.IsNullOrEmpty(shortAgeOption) ? ShortAge.BackOrder : shortAgeOption;

                foreach (DocumentLine dr in ffDocument.DocumentLines)
                {
                    //Hace fullfill solo si fue piqueda o si es un KIT con componentes piqueados.
                    if (dr.Quantity <= 0 && dr.Note != "2")
                        continue;

                    //Ajustando las cantidades a fullfill del kit, vrifica primero si hay inventario en GP para poder
                    //Procesar la orden, si no envia una error.
                    if (dr.Note == "2" && dr.QtyAllocated > 0 )
                    {
                        if (!ValidateProductInErpInventory(ffDocument.Location.ErpCode, dr.Product.ProductCode, dr.QtyAllocated, this.Connection))
                            throw new Exception("No stock available to fulfill product: " + dr.Product.FullDesc + ".\nPlease check.");

                        dr.Quantity = dr.QtyAllocated;
                    }


                    //Me dice que linea del documento esta linkeada
                    if (!string.IsNullOrEmpty(dr.LinkDocNumber))
                    {

                        try { objDBRow = ds.Tables[0].Select("LNITMSEQ=" + dr.LinkDocLineNumber)[0]; }
                        catch { continue; } //indica que la linea no existe en el doc original. y no se le 
                        //puede hacer fullfill - va a la siguiente.


                        //Al crear una linea se llenan QUANTITY, ATYALLOC, QTYREMAI, QTYTOINV - INFORMATIVO
                        //Al hacer fullfill se Ajustan: Invoice, FulFill, BackOrder (?) or Cancel
                        //                      QUANTITY	ATYALLOC	QTYREMAI	QTYTOINV	FulFill	AtySlCTD	QtyToBAo	QtyPrvINV
                        //Creacion de la Orden	15	        15	        15	        15	        0	    0	        0	        0
                        //Al poner Full Fill	15	        10	        15	        10	        10	    10	        5	        0
                        //Despues de facturar	15	        0	        5	        0	        0	    0	        5	        10

                        if (fistTimeFulfill)
                        {
                            QtyFulFi = dr.Quantity;

                            //Balance para cancelar o para backOrder
                            QtyToBalance = Double.Parse(objDBRow["QUANTITY"].ToString())
                                - Double.Parse(objDBRow["QTYPRINV"].ToString()) - QtyFulFi;

                            objDBRow["QTYFULFI"] = QtyFulFi; //Qty To FulFill
                            objDBRow["QTYTOINV"] = QtyFulFi; //Qty To Facturar
                        }
                        else
                        {
                            QtyFulFi = dr.Quantity; //Double.Parse(objDBRow["QTYFULFI"].ToString()) +

                            //QtyToInv = Double.Parse(objDBRow["QTYTOINV"].ToString()) + dr.Quantity;

                            QtyToBalance = Double.Parse(objDBRow["QUANTITY"].ToString()) -
                                Double.Parse(objDBRow["QTYPRINV"].ToString()) - QtyFulFi;

                            objDBRow["QTYFULFI"] = QtyFulFi; //Qty To FulFill
                            objDBRow["QTYTOINV"] = QtyToInv; //Qty To Facturar
                        }


                        if (QtyToBalance > 0)
                        {
                            if (shortAgeOption == ShortAge.BackOrder)
                                objDBRow["QTYTBAOR"] = QtyToBalance; //BackOrder Si asi lo indica el setup

                            else if (shortAgeOption == ShortAge.Cancel)
                                objDBRow["QTYCANCE"] = QtyToBalance; //Cancel Si asi lo indica el setup

                            //objDBRow["QTYREMAI"] = QtyToBalance;
                        }
                        else
                        {
                            //Pone en cero las canceladas y al back Order
                            objDBRow["QTYTBAOR"] = 0;
                            objDBRow["QTYCANCE"] = 0;
                            //objDBRow["QTYREMAI"] = 0;

                        }


                        objDBRow["FUFILDAT"] = DateTime.Today.ToString("yyyy-MM-dd");
                        objDBRow["ACTLSHIP"] = DateTime.Today.ToString("yyyy-MM-dd"); ;

                        //Persits Line
                        ds.Tables[0].Select("LNITMSEQ=" + dr.LinkDocLineNumber)[0] = objDBRow;
                    }


                    //Si la linea tiene Tracking. (se debe adicionar los tracking asociados)
                    if (dr.Product.ProductTrack != null)
                    {

                        //foreach (ProductTrackRelation pt in dr.Product.ProductTrack)
                        //{

                        //    if (pt.TrackOption.RowID == 1 && pt.DisplayName == null)  //Serial
                        //        serialTrack = GetReceiptLineSerials(serialTrack, traceList.Where(f => f.PostingDocLineNumber == dr.LineNumber).ToList());

                        //    else if (pt.TrackOption.RowID == 2 && pt.DisplayName == null) //Lot Code
                        //        lotTrack = GetReceiptLineLots(lotTrack, traceList.Where(f => f.PostingDocLineNumber == dr.LineNumber).ToList());
                        //}


                        //if (!string.IsNullOrEmpty(dr.LinkDocNumber))
                        //{
                        //    curLine.PONUMBER = dr.LinkDocNumber;
                        //    curLine.POLNENUM = dr.LinkDocLineNumber;
                        //}

                        //docLines[i - 1] = curLine;
                        //i++;
                    }


                }

                ObjCmdBuilder = new SqlCommandBuilder(objAdapter);
                objAdapter.Update(ds, GpTable); //Actualizando la Base de datos con las nuevas cantidades

                return true;

            }
            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("FulFillSalesDocument: " + ffDocument.CustPONumber, ListValues.EventType.Error, ex, null,
                    //ListValues.ErrorCategory.ErpConnection);

                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }

        */

        #endregion



        private bool ValidateProductInErpInventory(string location, string productCode, double quantity, SqlConnection xCnn)
        {
            Command.Connection = (xCnn == null) ? new SqlConnection(CurCompany.ErpConnection.CnnString) : xCnn;
            
            if (Command.Connection.State != ConnectionState.Open)
                Command.Connection.Open();

            // SOP10100 - Sales Order Header
            string sWhere = "ITEMNMBR = '" + productCode + "' AND LOCNCODE = '" + location + "'";
            DataSet ds = ReturnDataSet("SELECT QTYONHND FROM IV00102 WHERE 1 = 1 ", sWhere, "IV00102", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return false; //No Stock

            try
            {
                //Cantidad en inventario.
                Double QtyOnHnd = double.Parse(ds.Tables[0].Rows[0][0].ToString());
                return QtyOnHnd >= quantity ? true : false;

            }
            catch { return false; }

        }



        public string CancelKitAssemblyOrderBasedOnSalesDocument(Document data)
        {
            //use DataTable Management to delete Data.
            //Delete el Header en la tabla BM10200
            //Delete el Detalle en la tabla BM10300
            //Solo hace la Eliminacion si el status es < 3 (Que significa released)

            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            // BM00101 - KitAssemblyHeader
            string sWhere = "TRX_ID LIKE '" + data.DocNumber + "0%'";
            DataSet ds = ReturnDataSet("SELECT * FROM BM10200 WHERE BM_Trx_Status < 3 ", sWhere, "BM10200", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return "No Kit/Assembly Orders to Delete in the ERP.";

            StringBuilder result = new StringBuilder();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {

                result.Append("ASM Order " + dr["TRX_ID"] + ": ");

                try
                {
                    //Detail
                    Command.CommandText = "DELETE FROM BM10300 WHERE TRX_ID LIKE '" + data.DocNumber + "0%'";
                    Command.ExecuteNonQuery();

                    //Header
                    Command.CommandText = "DELETE FROM BM10300 WHERE TRX_ID LIKE '" + data.DocNumber + "0%'";
                    Command.ExecuteNonQuery();

                    result.Append("Deleted OK.\n");
                }
                catch (Exception ex) { 
                   result.Append("Failed, "+WriteLog.GetTechMessage(ex)+"\n"); 
                }
            }

            return result.ToString();

        }



        public void UpdateSalesDocumentBatch(String salesDoc, String batchNumber)
        {

            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();
            Command.CommandType = CommandType.Text;


            //Update BATCH first
            //Command.Parameters.Clear();
            //Command.CommandText = "EXEC HERA.Development.dbo.ICON_CreateSOPBatchIPUR '" + batchNumber + "'";
            //Command.ExecuteNonQuery();


            Command.Parameters.Clear();            

            // SOP10100 - Sales Order Header
            string sWhere = "SOPNUMBE = '" + salesDoc + "'";
            DataSet ds = ReturnDataSet("SELECT * FROM SOP10100 WHERE SOPTYPE = 2 ", sWhere, "SOP10100", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return; //No sales orden in ERP to Update


            //Header
            Command.CommandText = "UPDATE SOP10100 SET FUFILDAT=@ffDate, BACHNUMB=@batch WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber";
            // Create and prepare an SQL statement.
            AddParameter("@ffDate", DateTime.Today.ToString("yyyy-MM-dd"));
            AddParameter("@batch", batchNumber);
            AddParameter("@sopnumber", salesDoc);

            // Call Prepare after setting the Commandtext and Parameters.
            Command.ExecuteNonQuery();


            

            Command.Connection.Close();

        }


        public void UpdateSOP10106(String salesDoc, String batchNumber)
        {

            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();
            Command.Parameters.Clear();


            //Solo para Image
            //Cancell y Back Order.
            //Despues del ajuste que hizo Thomas 23/SEP/2010
            try { ExecuteQuery("UPDATE SOP10200 SET  QTYTBAOR = QTYREMAI, QTYTOINV = 0 WHERE SOPTYPE=2 AND SOPNUMBE = '" + salesDoc + "' AND QTYFULFI=0", Command.Connection); }
            catch { }

            //Limpiado las lineas que no sean procesadas por WMS
            //try { ExecuteQuery("UPDATE SOP10200 SET  ATYALLOC = 0, QTYTOINV = 0, QTYFULFI = 0 WHERE SOPTYPE=2 AND SOPNUMBE = '" + salesDoc + "' AND CONTNBR <> 'WMS_FF'", Command.Connection); }
            //catch { }


            //Depurando posibles Negativos
            //try { ExecuteQuery("UPDATE  SOP10200  set qtyfulfi=0, qtyslctd=0, atyalloc=0, qtytoinv=0 where SOPTYPE=2 and ( qtytoinv < 0 OR atyalloc < 0 OR qtyfulfi < 0)", Command.Connection); }
            //catch { }


            try { ExecuteQuery("INSERT INTO SOP10106 (soptype,sopnumbe,CMMTTEXT) values (2,'" + salesDoc + "','')", Command.Connection); }
            catch { }

            // SOP10100 - Sales Order Header
            string sWhere = "SOPNUMBE = '" + salesDoc + "'";
            DataSet ds = ReturnDataSet("SELECT * FROM SOP10106 WHERE SOPTYPE = 2 ", sWhere, "SOP10106", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return; //No sales orden in ERP to Update


            //Header
            Command.CommandText = "UPDATE SOP10106 SET USRDEF05=@batch WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber";
            // Create and prepare an SQL statement.
            AddParameter("@batch", batchNumber);
            AddParameter("@sopnumber", salesDoc);

            // Call Prepare after setting the Commandtext and Parameters.
            Command.ExecuteNonQuery();
            Command.Connection.Close();

        }



        public Document GetSalesOrderPostedStatus(Document order)
        {

            try
            {
                string sWhere = " SOPNUMBE='" + order.DocNumber + "'";

                //En GP la fecha null es 1900-01-01
                DateTime nullGpDate = DateTime.Parse("1900-01-01");

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                Command.Connection.Open();

                // BM30220 - Document posted
                ds = ReturnDataSet("SELECT * FROM SOP30200 WHERE SOPTYPE=2 ", sWhere, "SOP30200", Command.Connection);

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];

                    //Evaluar en el encabezado si el documento esta posted (fecha y User)
                    DateTime postedDate = DateTime.Parse(dr["POSTEDDT"].ToString());
                    string postedUser = dr["PTDUSRID"].ToString();


                    if (!string.IsNullOrEmpty(postedUser) && postedDate != nullGpDate)
                    {
                        if (order.Comment == null) order.Comment = "";
                        order.Comment = order.Comment + " Erp posted date: " + postedDate.ToString() + ", Erp posted user: " + postedUser;

                        return order;
                    }
                    else
                        return null;
                }

                //retornar la lista 
                return null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetSalesOrderPostedStatus", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }



        }



        public Boolean CreateSalesInvoice()
        {
            throw new Exception("Not implemented.");
        }



        #endregion





        #endregion

    }
}

