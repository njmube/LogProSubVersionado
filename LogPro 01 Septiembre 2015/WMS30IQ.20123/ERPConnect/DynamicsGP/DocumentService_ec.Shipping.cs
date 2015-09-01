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


        public object[] GetSOP_POPLink(string document, int numLine, short docType)
        {
            DataSet ds = null;
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

            if (docType == SDocType.SalesOrder)
 
                ds = ReturnDataSet("SELECT PONUMBER,ORD,CASE WHEN QTYONPO = 0 THEN 'Received' ELSE '' END FROM SOP60100 WHERE SOPTYPE = 2 ", "SOPNUMBE = '"+ document +"' AND LNITMSEQ = " + numLine, "SOP60100", Command.Connection);


            else if (docType == SDocType.PurchaseOrder)

                ds = ReturnDataSet("SELECT SOPNUMBE,LNITMSEQ, CASE WHEN QTYONPO = 0 THEN 'Received' ELSE '' END FROM SOP60100 WHERE SOPTYPE = 2 ", "PONUMBER = '" + document + "' AND  ORD = " + numLine, "SOP60100", Command.Connection);


            if (ds == null || ds.Tables.Count == 0)
                return null;

            try
            {
                DataRow result = ds.Tables[0].Rows[0];
                return new object[] { result[0], result[1] };
            }
            catch { return null;  }


        }


        
        #region ShippingDocuments


        public IList<Document> GetAllShippingDocuments(int docType, bool useRemain) { return GetShippingDocuments("", docType, useRemain); }


        public IList<Document> GetShippingDocumentsLastXDays(int days, int docType, bool useRemain)
        {
            return GetShippingDocuments("DATEDIFF(day,DEX_ROW_TS,GETDATE()) <= " + days.ToString(), docType, useRemain);
        }


        public IList<Document> GetShippingDocumentById(string code, int docType, bool useRemain)
        { return GetShippingDocuments("SOPNUMBE = '" + code + "'", docType, useRemain); }


        public IList<Document> GetShippingDocumentsSince(DateTime sinceDate, int docType, bool userRemain)
        {
            //return GetShippingDocuments(" MODIFDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "'", docType);
            return GetShippingDocuments(" DEX_ROW_TS >= '" + sinceDate.ToString("yyyy-MM-dd HH:mm:ss") + "'", docType, userRemain);
        }


        public IList<Document> GetShippingDocuments(string sWhere, int docType, bool useRemain)
        {
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;
            string pos = "0";

            try
            {
                //Lamar los documents que necesita del Erp usando econnect
                sWhere = string.IsNullOrEmpty(sWhere) ? "SOPTYPE IN (" + docType + ")" : "SOPTYPE IN (" + docType + ") AND " + sWhere;
                string xmlData = DynamicsGP_ec.RetreiveData("Sales_Transaction", false, 2, 0, sWhere, true);

                pos = "1";

                int rem = 0x02;
                xmlData = xmlData.Replace((char)rem,' ');
                ds = DynamicsGP_ec.GetDataSet(xmlData);

                pos = "2";
                    

                if (ds.Tables.Count == 0)
                    return null;


                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Shipping } );
                
                //Definiendo los tipos de documento de shipping
                DocumentType soType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.SalesOrder });
                DocumentType siType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.SalesInvoice });
                DocumentType bkType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.BackOrder });
                DocumentType returnType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.Return });


                //Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });
                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany; // WType.GetDefaultCompany();
                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

                //En el dataset, Tables: 1 - DocumentHeader, 2 - DocumentLine, 3 - DocumentComments
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    try
                    {

                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["DOCDATE"].ToString());
                        tmpData.Date2 = DateTime.Parse(dr["ReqShipDate"].ToString());
                        tmpData.Date3 = DateTime.Parse(dr["ACTLSHIP"].ToString());
                        tmpData.Date4 = DateTime.Parse(dr["BACKDATE"].ToString());
                        tmpData.Date5 = DateTime.Parse(dr["DUEDATE"].ToString());
                        
                        tmpData.DocNumber = dr["SOPNUMBE"].ToString();
                        tmpData.ErpMaster = int.Parse(dr["MSTRNUMB"].ToString());
                        tmpData.DocStatus = GetShippingStatus(0);

                        //Oct 24 - Manejo de la sincronizacion traer cada ves menos records, solo lo nuevo
                        //tmpData.LastChange = DateTime.Parse(dr["DEX_ROW_TS"].ToString());
                        tmpData.LastChange = GetDocumentLastChange("SOP10100", "SOPNUMBE", dr["SOPNUMBE"].ToString());

                        try
                        {
                            tmpData.Comment = GetDocumentNotes(dr["NOTEINDX"].ToString(), dr["COMMNTID"].ToString());
                        }
                        catch { }



                        //LAs ordenes con status void en GP, salen como canceladas.
                        try
                        {
                            if (int.Parse(dr["VOIDSTTS"].ToString()) > 0)
                                tmpData.DocStatus = cancelled;
                        }
                        catch { }

                        tmpData.CreatedBy = dr["USER2ENT"].ToString();
                        tmpData.CustPONumber = dr["CSTPONBR"].ToString();

                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount;
                        Location location = WType.GetLocation(new Location { ErpCode = dr["LOCNCODE"].ToString(), Company = company });
                        tmpData.Customer = WType.GetAccount(new Account { 
                            AccountCode = dr["CUSTNMBR"].ToString(), 
                            BaseType = new AccountType { AccountTypeID = AccntType.Customer },  
                            Company = company });
                        try
                        {
                            if (!string.IsNullOrEmpty(dr["SHIPMTHD"].ToString()))
                                tmpData.ShippingMethod = WType.GetShippingMethod(
                                    new ShippingMethod { ErpCode = dr["SHIPMTHD"].ToString(), Company = company });
                        }
                        catch { }
                        //tmpData.User = user;

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        tmpData.Location = location;
                        tmpData.Company = CurCompany;



                        tmpData.Reference = dr["REFRENCE"].ToString();
                        tmpData.Notes = dr["BACHNUMB"].ToString();

                        //Asignacion de Address
                        tmpData.DocumentAddresses = GetShippingDocumentAddress(tmpData, null, dr);

                        DocumentAddress billAddress = null;
                        if (!string.IsNullOrEmpty(dr["PRBTADCD"].ToString()))
                            billAddress = GetBillAddress(tmpData, dr["PRBTADCD"].ToString(), dr["CUSTNMBR"].ToString(), AccntType.Customer);

                        if (billAddress != null)
                            tmpData.DocumentAddresses.Add(billAddress);

                        switch (dr["SOPTYPE"].ToString())
                        {
                            case "2":
                                tmpData.DocType = soType;
                                tmpData.PickMethod = soType.PickMethod;
                                break;

                            case "3":
                                tmpData.DocType = siType;
                                tmpData.PickMethod = siType.PickMethod;
                                break;

                            case "6":
                                tmpData.DocType = siType;
                                tmpData.PickMethod = siType.PickMethod;
                                break;


                            case "5":
                                tmpData.DocType = bkType;
                                tmpData.PickMethod = bkType.PickMethod;
                                tmpData.UserDef1 = dr["USDOCID1"].ToString();
                                break;


                            case "4":  //RETURN en realidad es un Receiving Document
                                tmpData.DocType = returnType;
                                //tmpData.Customer = defAccount;
                                tmpData.Vendor = WType.GetAccount(new Account { AccountCode = dr["CUSTNMBR"].ToString(), BaseType = new AccountType { AccountTypeID = AccntType.Customer }, Company = company });
                                tmpData.PickMethod = returnType.PickMethod;
                                break;
                            
                        }

                        //Asignacion de Lines - Seguen el tipo de orden
                        tmpData.DocumentLines = GetShippingDocumentLines(tmpData, ds.Tables["Line"].Select("SOPNUMBE='" + dr["SOPNUMBE"].ToString() + "'"), useRemain);

                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0)
                            list.Add(tmpData);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetShippingDocuments: " + tmpData.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                    
                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetShippingDocuments:" + pos + ":" , ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }

        }


        private IList<DocumentLine> GetShippingDocumentLines(Document doc, DataRow[] dLines, bool useRemain)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });


            int curLine = 0;
            string curMaster = "";


            try
            {

                foreach (DataRow dr in dLines)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    tmpData.Date2 = DateTime.Parse(dr["ReqShipDate"].ToString());
                    tmpData.Date3 = DateTime.Parse(dr["ACTLSHIP"].ToString());
                    tmpData.Date4 = DateTime.Parse(dr["FUFILDAT"].ToString());

                    tmpData.LineNumber = int.Parse(dr["LNITMSEQ"].ToString());
                    tmpData.Sequence = tmpData.LineNumber;
                    curLine = tmpData.LineNumber;

                    //TODO: Revisar el Status en GP para traer el equivalente
                    tmpData.LineStatus = GetShippingStatus(0);
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    
                    if (useRemain)
                        tmpData.Quantity = double.Parse(dr["QTYREMAI"].ToString(), ListValues.DoubleFormat());
                    else
                        tmpData.Quantity = double.Parse(dr["QUANTITY"].ToString(), ListValues.DoubleFormat());

                    tmpData.QtyCancel = double.Parse(dr["QTYCANCE"].ToString(), ListValues.DoubleFormat());
                    tmpData.QtyBackOrder = double.Parse(dr["QTYTBAOR"].ToString(), ListValues.DoubleFormat());
                    tmpData.QtyPending = tmpData.Quantity - tmpData.QtyCancel - double.Parse(dr["QTYPRINV"].ToString(), ListValues.DoubleFormat());
                    tmpData.QtyAllocated = double.Parse(dr["ATYALLOC"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Location:" + dr["LOCNCODE"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = doc.Location.Company, ErpCode = dr["LOCNCODE"].ToString() });
                    
                    try
                    {
                        curMaster = "Product:" + dr["ITEMNMBR"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = doc.Location.Company, ProductCode = dr["ITEMNMBR"].ToString() }); 
                        tmpData.LineDescription = dr["ITEMDESC"].ToString();

                        curMaster = "Uom:" + dr["UOFM"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });


                    }
                    catch
                    {
                        //Pone el Default Product
                        tmpData.Product = WType.GetProduct(new Product { Company = doc.Location.Company, ProductCode = WmsSetupValues.DEFAULT });
                        tmpData.LineDescription = "Unknown: " + dr["ITEMNMBR"].ToString() + ", " + dr["ITEMDESC"].ToString();

                        curMaster = "Uom:" + dr["UOFM"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString() });

                    }

                    //Manage Prices
                    curMaster = "Prices Product:" + dr["ITEMNMBR"].ToString();
                    tmpData.UnitPrice = double.Parse(dr["UNITPRCE"].ToString(), ListValues.DoubleFormat());
                    tmpData.ExtendedPrice = double.Parse(dr["XTNDPRCE"].ToString(), ListValues.DoubleFormat());

                    tmpData.UnitCost = double.Parse(dr["UNITCOST"].ToString(), ListValues.DoubleFormat());
                    tmpData.ExtendedCost = double.Parse(dr["EXTDCOST"].ToString(), ListValues.DoubleFormat());

                    //SOP POP Link
                    try
                    {
                        object[] sop_popLink = GetSOP_POPLink(doc.DocNumber, tmpData.LineNumber, doc.DocType.DocTypeID);
                        if (sop_popLink != null)
                        {
                            tmpData.LinkDocNumber = sop_popLink[0].ToString();
                            tmpData.LinkDocLineNumber = int.Parse(sop_popLink[1].ToString());
                            tmpData.BinAffected = sop_popLink[2].ToString(); //Indica si la linea fue recibida o no.
                        }
                    }
                    catch { }

                    //Asignacion de Address
                    curMaster = "Address Doc:" + doc.DocNumber;
                    tmpData.DocumentLineAddresses = GetShippingDocumentAddress(tmpData.Document, tmpData, dr);
                    

                    list.Add(tmpData);
                }

                return (list.Count > 0) ? list : null;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetShippingDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                    ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }


        private IList<DocumentAddress> GetShippingDocumentAddress(Document doc, DocumentLine docLine, DataRow dr)
        {

            IList<DocumentAddress> list = new List<DocumentAddress>();
            Boolean pass = false;

            if (docLine == null)
                pass = true;
            else if (doc.DocumentAddresses != null && doc.DocumentAddresses.Count > 0 && !doc.DocumentAddresses[0].AddressLine1.Equals(dr["ADDRESS1"].ToString()))
                pass = true;

            try
            {
                //Verificacion solo para Document Lines
                if (pass)
                {

                    DocumentAddress spAddr = new DocumentAddress();
                    spAddr.Document = doc;
                    spAddr.DocumentLine = (docLine == null) ? null : docLine;
                    spAddr.Name = dr["ShipToName"].ToString();
                    spAddr.ErpCode = dr["PRSTADCD"].ToString();
                    spAddr.AddressLine1 = dr["ADDRESS1"].ToString();
                    spAddr.AddressLine2 = dr["ADDRESS2"].ToString();
                    spAddr.AddressLine3 = dr["ADDRESS3"].ToString();
                    spAddr.City = dr["CITY"].ToString();
                    spAddr.State = dr["STATE"].ToString();
                    spAddr.ZipCode = dr["ZIPCODE"].ToString();
                    spAddr.Country = dr["COUNTRY"].ToString();

                    if (docLine == null)
                    {
                        spAddr.Phone1 = dr["PHNUMBR1"].ToString();
                        spAddr.Phone2 = dr["PHNUMBR2"].ToString();
                    }
                    else
                    {
                        spAddr.Phone1 = dr["PHONE1"].ToString();
                        spAddr.Phone2 = dr["PHONE2"].ToString();
                    }
                    spAddr.Phone3 = dr["FAXNUMBR"].ToString();
                    spAddr.ContactPerson = dr["CNTCPRSN"].ToString(); 
                    spAddr.AddressType = AddressType.Shipping;
                    spAddr.Email = "";
                    spAddr.ShpMethod = WType.GetShippingMethod(new ShippingMethod { Company = doc.Company, ErpCode = dr["SHIPMTHD"].ToString() }); ;
                    spAddr.CreationDate = DateTime.Now;
                    spAddr.CreatedBy = WmsSetupValues.SystemUser;

                    if (!(string.IsNullOrEmpty(dr["ShipToName"].ToString()) && string.IsNullOrEmpty(spAddr.AddressLine1 = dr["ADDRESS1"].ToString())))
                        list.Add(spAddr);

                }

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetShippingDocumentAddress" + doc.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
            }

            return list;
        }


        private string GetDocumentNotes(string NoteIndex, string CommID)
        {
            string res = "";
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

            //Se quito por Maxiforce, si un cliente nuevo lo pide se debe poner en el Setup.
            //Se adiciono Maxiforce.
            if (!string.IsNullOrEmpty(NoteIndex))
            res =  ReturnScalar("SELECT TXTFIELD FROM SY03900 WHERE NOTEINDX=" + NoteIndex, "", Command.Connection);

            if (!string.IsNullOrEmpty(CommID))
                res += " " + ReturnScalar("SELECT COMMNTID + '. ' + ISNULL(CMMTTEXT,'') FROM SY04200 WHERE COMMNTID='" + CommID + "'", "", Command.Connection);
            
            return res.Trim();
        }



        private string GetDocumentNotesPurchase(string NoteIndex, string CommID, string docNumber)
        {
            string res = "";
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

            //Se quito por Maxiforce, si un cliente nuevo lo pide se debe poner en el Setup.
            //Se adiciono Maxiforce.
            if (!string.IsNullOrEmpty(NoteIndex))
                res = ReturnScalar("SELECT TXTFIELD FROM SY03900 WHERE NOTEINDX=" + NoteIndex, "", Command.Connection);

            if (!string.IsNullOrEmpty(CommID))
                res += " " + ReturnScalar("SELECT COMMNTID + '. ' + ISNULL(CMMTTEXT,'') FROM SY04200 WHERE COMMNTID='" + CommID + "'", "", Command.Connection);


            res += " " + ReturnScalar("SELECT ISNULL(CMMTTEXT,'') FROM POP10150 WHERE POPNUMBE='" + docNumber + "'", "", Command.Connection);


            return res.Trim();
        }



        private DateTime? GetDocumentLastChange(string Table, string docColumn, string docNumber)
        {
            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                string lastChange = ReturnScalar("SELECT DEX_ROW_TS FROM " + Table + " WHERE " + docColumn + "='" + docNumber + "'", "", Command.Connection);

                return DateTime.Parse(lastChange);
            }
            catch { return null;  } 
        }


        private Status GetShippingStatus(int GPStatus)
        {
            //if (GPStatus == 4)
            //    return WType.GetStatus(new Status { StatusID = DocStatus.Completed });
            //if (GPStatus == 5 || GPStatus == 6)
            //    return WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
            //else //es nueva (1,2,3)
                return WType.GetStatus(new Status { StatusID = DocStatus.New });
        }


        private DocumentAddress GetBillAddress(Document doc, string addressCode, string customerCode, short AccnType)
        {
            DocumentAddress billAddress = null;

            AccountAddress accountAddress = new AccountAddress
            {
                Account = new Account { AccountCode = customerCode, BaseType = new AccountType { AccountTypeID = AccnType } },
                ErpCode = addressCode
            };

            accountAddress = WType.GetAccountAddress(accountAddress);

            if (accountAddress != null)
            {
                billAddress = new DocumentAddress();
                billAddress.Document = doc;
                billAddress.DocumentLine = null;
                billAddress.Name = accountAddress.Name;
                billAddress.AddressLine1 = accountAddress.AddressLine1;
                billAddress.AddressLine2 = accountAddress.AddressLine2;
                billAddress.AddressLine3 = accountAddress.AddressLine3;
                billAddress.City = accountAddress.City;
                billAddress.State = accountAddress.State;
                billAddress.ZipCode = accountAddress.ZipCode;
                billAddress.Country = accountAddress.Country;
                billAddress.Phone1 = accountAddress.Phone1;
                billAddress.Phone2 = accountAddress.Phone2;
                billAddress.Phone3 = accountAddress.Phone3;
                billAddress.ContactPerson = accountAddress.ContactPerson;
                billAddress.AddressType = AddressType.Billing;
                billAddress.Email = accountAddress.Email;
                billAddress.CreationDate = DateTime.Now;
                billAddress.CreatedBy = WmsSetupValues.SystemUser;
                billAddress.ErpCode = addressCode;
            }

            return billAddress;
        }


        public String CreateSalesOrder(Document document, string docPrefix, string batch)
        {

            if (document.DocumentLines == null || document.DocumentLines.Count <= 0)
                throw new Exception("Sales order does not contains lines.");

            string SOPNumber;
            //string DocPrefix = "SWEB"; //ej: SWEB o como se deban crear las ordenes
            //string Batch = "BATCH"; //Numero de Batch de las ordenes a Crear

            try
            {

                taSopLineIvcInsert_ItemsTaSopLineIvcInsert[] salesLine = new taSopLineIvcInsert_ItemsTaSopLineIvcInsert[document.DocumentLines.Count];
                //Create an object that holds XML node object
                taSopLineIvcInsert_ItemsTaSopLineIvcInsert LineItem; // = new taSopLineIvcInsert_ItemsTaSopLineIvcInsert();
                int lineSeq = 0;
                Decimal subTotal = 0;

                // Next consecutive for a S.O.
                GetSopNumber mySopNumber = new GetSopNumber();
                SOPNumber = mySopNumber.GetNextSopNumber(2, docPrefix, CurCompany.ErpConnection.CnnString);


                Console.WriteLine("1");

                foreach (DocumentLine dl in document.DocumentLines)
                {
                    LineItem = new taSopLineIvcInsert_ItemsTaSopLineIvcInsert();

                    // Populate            
                    LineItem.CUSTNMBR = dl.Document.Customer.AccountCode;
                    LineItem.SOPNUMBE = SOPNumber;
                    LineItem.SOPTYPE = 2;
                    LineItem.DOCID = docPrefix;   // SWEB   STDORD
                    LineItem.ITEMNMBR = dl.Product.ProductCode;
                    LineItem.UOFM = dl.Unit.ErpCode;
                    LineItem.UNITPRCE = (decimal)dl.UnitPrice;
                    LineItem.XTNDPRCE = (decimal)dl.ExtendedPrice;
                    LineItem.LOCNCODE = document.Location.ErpCode;
                    LineItem.DOCDATE = DateTime.Now.ToShortDateString();
                    LineItem.PRCLEVEL = "STANDARD";

                    // quantities
                    LineItem.QUANTITY = (decimal)dl.Quantity;

                    salesLine[lineSeq++] = LineItem;
                    subTotal += (decimal)dl.ExtendedPrice;
                }


                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                SOPTransactionType salesOrder = new SOPTransactionType();
                salesOrder.taSopLineIvcInsert_Items = salesLine;

                //Create a taSopHdrIvcInsert XML node object
                taSopHdrIvcInsert salesHdr = new taSopHdrIvcInsert();

                //Populate the properties of the taSopHdrIvcInsert XML node object           

                salesHdr.SOPTYPE = 2;
                salesHdr.SOPNUMBE = SOPNumber;
                salesHdr.DOCID = docPrefix;
                salesHdr.BACHNUMB = batch; // "B2BSO";
                salesHdr.LOCNCODE = document.Location.ErpCode;
                salesHdr.DOCDATE = DateTime.Now.ToShortDateString();
                salesHdr.SUBTOTAL = subTotal;
                salesHdr.DOCAMNT = subTotal;
                
                salesHdr.CUSTNMBR = document.Customer.AccountCode;
                salesHdr.CUSTNAME = document.Customer.Name;

                //if (document.ErpMaster > 0)
                    //salesHdr.MSTRNUMB = document.ErpMaster;

                //BILLING
                try { salesHdr.PRBTADCD = document.Customer.AccountAddresses[0].ErpCode; }
                catch
                {
                    salesHdr.PRBTADCD = document.DocumentAddresses
                        .Where(f => f.AddressType == AddressType.Billing).First().ErpCode;
                }

                //SHIPPING
                try { salesHdr.PRSTADCD = document.Customer.AccountAddresses[0].ErpCode; }
                catch
                {
                    salesHdr.PRBTADCD = document.DocumentAddresses
                        .Where(f => f.AddressType == AddressType.Shipping).First().ErpCode;
                }

                salesHdr.CSTPONBR = document.CustPONumber;

                salesOrder.taSopHdrIvcInsert = salesHdr;

                SOPTransactionType[] salesOrderArray = new SOPTransactionType[1];
                salesOrderArray[0] = salesOrder;

                //Create an eConnect XML document object and populate its SOPTransactionType property with
                //the SOPTransactionType schema object
                eConnectType eConnect = new eConnectType();
                eConnect.SOPTransactionType = salesOrderArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                //mySopNumber.Dispose();

                return SOPNumber;

            }
            
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("CreateSalesOrder", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                throw;
            }
        }


        public String CreateCustomer(Account customer)
        {
            try {
                // datos de la address
                taUpdateCreateCustomerRcd record = new taUpdateCreateCustomerRcd
                {
                    CUSTNAME = customer.Name,
                    CUSTNMBR = customer.AccountCode,
                    //CNTCPRSN = customer.ContactPerson,
                    //PHNUMBR1 = customer.Phone,
                    USERDEF1 = customer.UserDefine1,
                    USERDEF2 = customer.UserDefine2,
                    USRDEFND3 = customer.UserDefine3,
                    COMMENT1 = customer.UserDefine1,
                    UpdateIfExists = 0                    
                };

                RMCustomerMasterType eType = new RMCustomerMasterType();
                eType.taUpdateCreateCustomerRcd = record;

                RMCustomerMasterType[] arrCustTypes = { eType };

                //Create an eConnect XML document object and populate 
                eConnectType eConnect = new eConnectType();
                eConnect.RMCustomerMasterType = arrCustTypes;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());
                
                if (customer.AccountAddresses != null)
                    foreach (AccountAddress address in customer.AccountAddresses)
                        CreateCustomerAddress(address);


                return customer.AccountCode;

            }

            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("CreateCustomer", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                throw;
            }
        }


        public string CreateCustomerAddress(AccountAddress address)  // string path
        {
            try
            {
                // datos de la address
                taCreateCustomerAddress_ItemsTaCreateCustomerAddress add = new taCreateCustomerAddress_ItemsTaCreateCustomerAddress();
                
                add.CUSTNMBR = address.Account.AccountCode;
                add.ADRSCODE = address.ErpCode;
                add.ADDRESS1 = address.AddressLine1;
                add.ADDRESS2 = address.AddressLine2;
                add.ADDRESS3 = address.AddressLine3;
                add.CNTCPRSN = address.ContactPerson;
                add.PHNUMBR1 = address.Phone1;
                add.PHNUMBR2 = address.Phone2;
                add.PHNUMBR3 = address.Phone3;
                add.COUNTRY = address.Country;
                add.STATE = address.State;
                add.CITY = address.City;
                add.ZIPCODE = address.ZipCode;
                add.UpdateIfExists = 1;  //Update If Exists

                RMCustomerAddressType addType = new RMCustomerAddressType();
                addType.taCreateCustomerAddress_Items = new taCreateCustomerAddress_ItemsTaCreateCustomerAddress[] { add };

                RMCustomerAddressType[] arrAddTypes = { addType };

                //Create an eConnect XML document object and populate 
                eConnectType eConnect = new eConnectType();
                eConnect.RMCustomerAddressType = arrAddTypes;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString()); 
                
                return address.Account.AccountCode;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("CreateCustomerAddress", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                throw;
            }

            
        }





        public int SaveUpdateErpDocumentLine(DocumentLine docLine, bool removeLine)
        {
            try
            {
                if (docLine.Document.DocType.DocTypeID == SDocType.SalesOrder || docLine.Document.DocType.DocTypeID == SDocType.BackOrder)
                {
                    if (removeLine)
                        return RemoveSalesOrderLine(docLine, false);

                    //Crea un sales order line en GP. Para el documento generado
                    else if (docLine.LineID == 0)
                        return CreateSalesOrderLine(docLine, false);

                    else
                        return UpdateSalesOrderLine(docLine, false);
                }


                if (docLine.Document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                {
                    //if (removeLine)
                        //return RemoveSalesOrderLine(docLine, true);

                    //Crea un sales order line en GP. Para el documento generado
                    //else 
                    if (docLine.LineID == 0)
                        return CreateSalesOrderLine(docLine, true);

                    else
                        return UpdateSalesOrderLine(docLine, true);
                }


                return 0;

            }
            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
                //ExceptionMngr.WriteEvent("CreateInventoryAdjustment:" + inventoryAdj.DocNumber, ListValues.EventType.Error, 
                //    ex, null, ListValues.ErrorCategory.ErpConnection);
                //return false;
            }
        }



        private int UpdateSalesOrderLine(DocumentLine docLine, bool isMerged)
        {
            //Utiliza SQL Directo para actualizar la linea en GP.
            //Cantidades basicamente.

            //SqlDataAdapter objAdapter;
            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            //Header
            Command.CommandText = "UPDATE SOP10200 SET QUANTITY = @quantity, QTYTBAOR = @qtyBO,  QTYCANCE = @qtyCancel, "
            + "QTYTOINV=@qtyInv, ATYALLOC=@qtyAlloc, QTYREMAI=@qtyRem, QTYFULFI=@qtyFulfi,  ITEMDESC = @itemDesc WHERE SOPTYPE = 2 AND SOPNUMBE = @sopnumber AND LNITMSEQ = @lineSeq ";

            Command.Parameters.Clear();
            // Create and prepare an SQL statement.
            AddParameter("@qtyBO", docLine.QtyBackOrder);
            AddParameter("@quantity", docLine.Quantity);
            AddParameter("@qtyInv", docLine.Quantity - (docLine.QtyBackOrder + docLine.QtyCancel)); //docLine.Quantity - (docLine.QtyBackOrder + docLine.QtyCancel));
            AddParameter("@qtyCancel", docLine.QtyCancel);
            
            if (!string.IsNullOrEmpty(docLine.LineDescription))
                AddParameter("@itemDesc", docLine.LineDescription);
            else
                AddParameter("@itemDesc", docLine.Product.Name);

            AddParameter("@qtyAlloc", 0); //docLine.Quantity - (docLine.QtyBackOrder + docLine.QtyCancel));
            AddParameter("@qtyRem", docLine.Quantity - (docLine.QtyBackOrder + docLine.QtyCancel));
            AddParameter("@qtyFulfi", 0); //docLine.Quantity - (docLine.QtyBackOrder + docLine.QtyCancel));

            if (isMerged)
            {
                AddParameter("@sopnumber", docLine.LinkDocNumber);
                AddParameter("@lineSeq", docLine.LinkDocLineNumber);
            }
            else
            {
                AddParameter("@sopnumber", docLine.Document.DocNumber);
                AddParameter("@lineSeq", docLine.LineNumber);
            }

            // Call Prepare after setting the Commandtext and Parameters.
            Command.ExecuteNonQuery();

            return docLine.LineNumber;

        }



        private int RemoveSalesOrderLine(DocumentLine docLine, bool isMerged)
        {
            try {

                //Create an object that holds XML node object
                taSopLineDelete curLine = new taSopLineDelete();

                // Populate Lines            
                if (docLine.Document.DocType.DocTypeID == SDocType.SalesOrder)
                    curLine.SOPTYPE = GP_DocType.SO_Order;
                else if (docLine.Document.DocType.DocTypeID == SDocType.BackOrder)
                    curLine.SOPTYPE = GP_DocType.SO_BackOrder;


                curLine.ITEMNMBR = docLine.Product.ProductCode;
                curLine.DeleteType = 1;

                curLine.SOPNUMBE = docLine.Document.DocNumber;
                curLine.LNITMSEQ = docLine.LineNumber;


                //if (isMerged)
                //{
                //    curLine.SOPNUMBE = docLine.Document.DocNumber;
                //    curLine.LNITMSEQ = docLine.LineNumber;
                //}
                //else
                //{
                //    curLine.SOPNUMBE = docLine.Document.DocNumber;
                //    curLine.LNITMSEQ = docLine.LineNumber;
                //}


                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                SOPDeleteLineType record = new SOPDeleteLineType();
                record.taSopLineDelete = curLine;

                SOPDeleteLineType[] rArray = new SOPDeleteLineType[]{ record };

                //Create an eConnect XML document object and populate its SOPTransactionType property with
                //the SOPTransactionType schema object
                eConnectType eConnect = new eConnectType();
                eConnect.SOPDeleteLineType = rArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }



        private int CreateSalesOrderLine(DocumentLine docLine, bool isMerged)
        {
            string flag = "Data Definition";

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                //Create an object that holds XML node object
                taSopLineIvcInsert_ItemsTaSopLineIvcInsert curLine = new taSopLineIvcInsert_ItemsTaSopLineIvcInsert();


                string document = (isMerged) ? docLine.LinkDocNumber : docLine.Document.DocNumber;
                string note = docLine.Note == null ? "" : docLine.Note;

                curLine.SOPTYPE = GP_DocType.SO_Order;
                curLine.CUSTNMBR = docLine.Document.Customer.AccountCode;
                curLine.DOCDATE = ((DateTime)docLine.Document.Date1).ToString("yyyy-MM-dd");
                curLine.SOPNUMBE = document;

                flag = "Item Definition";
                curLine.ITEMDESC = string.IsNullOrEmpty(docLine.LineDescription) ? docLine.Product.Name : docLine.LineDescription;
                curLine.ITEMNMBR = docLine.Product.ProductCode;
                curLine.QUANTITY = (decimal)docLine.Quantity;                
                curLine.UOFM = docLine.Unit.ErpCode;

                curLine.QTYCANCE = (decimal)docLine.QtyCancel;
                curLine.QTYTBAOR = (decimal)docLine.QtyBackOrder;
                curLine.UpdateIfExists = 1;
                //curLine.LNITMSEQ = 0;
                curLine.CMMTTEXT = note;
                curLine.ALLOCATE = 0;
                curLine.LOCNCODE = docLine.Location.ErpCode;

                string docID = ReturnScalar("SELECT DOCID FROM SOP10100 WHERE SOPNUMBE='" + document + "' AND SOPTYPE=2", "", Command.Connection);
                curLine.DOCID = docID;
                

                string xGuid = Guid.NewGuid().ToString();
                curLine.CNTCPRSN = xGuid;

                flag = "eConnect Definition";

                taSopLineIvcInsert_ItemsTaSopLineIvcInsert[] erpLines =
                    new taSopLineIvcInsert_ItemsTaSopLineIvcInsert[] { curLine };


                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                SOPTransactionType salesOrder = new SOPTransactionType();
                salesOrder.taSopLineIvcInsert_Items = erpLines;


                //SALES ORDER HEADER

                //Create a taSopHdrIvcInsert XML node object
                taSopHdrIvcInsert salesHdr = new taSopHdrIvcInsert();

                //Populate the properties of the taSopHdrIvcInsert XML node object           

                salesHdr.SOPTYPE = 2;
                salesHdr.SOPNUMBE = document;
                salesHdr.DOCID = ReturnScalar("SELECT DOCID FROM SOP10100 WHERE SOPNUMBE='" + document + "' AND SOPTYPE=2", "", Command.Connection);
                salesHdr.BACHNUMB = ReturnScalar("SELECT BACHNUMB FROM SOP10100 WHERE SOPNUMBE='" + document + "' AND SOPTYPE=2", "", Command.Connection); // "B2BSO";
                salesHdr.LOCNCODE = docLine.Document.Location.ErpCode;
                salesHdr.DOCDATE = ((DateTime)docLine.Document.Date1).ToString("yyyy-MM-dd");
                salesHdr.UpdateExisting = 1;
                salesHdr.CUSTNMBR = docLine.Document.Customer.AccountCode;
                salesHdr.CSTPONBR = docLine.Document.CustPONumber;                

                salesOrder.taSopHdrIvcInsert = salesHdr;

                //SALES ORDER HEADER


                SOPTransactionType[] salesOrderArray = new SOPTransactionType[1];
                salesOrderArray[0] = salesOrder;

                //Create an eConnect XML document object and populate its SOPTransactionType property with
                //the SOPTransactionType schema object
                eConnectType eConnect = new eConnectType();
                eConnect.SOPTransactionType = salesOrderArray;

                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                //Obterne el line ID creado
                flag = "Line Number";

                string  newLine;
                newLine = ReturnScalar("SELECT MAX(LNITMSEQ) FROM SOP10200 WHERE SOPNUMBE='" + document + "' AND CNTCPRSN ='" + xGuid + "'", "", Command.Connection);
                return int.Parse(newLine);


            }
            catch (Exception ex)
            {
                throw new Exception(flag + ". " +WriteLog.GetTechMessage(ex));
            }

        }




        #endregion





        #region Transfer Documents


        public IList<Document> GetAllLocationTransferDocuments()
        {
            return GetLocationTransferDocuments("");
        }


        public IList<Document> GetLocationTransferDocumentsSince(DateTime sinceDate)
        {
            return GetLocationTransferDocuments(" DEX_ROW_TS >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
            //return GetLocationTransferDocuments("");
        }


        private IList<Document> GetLocationTransferDocuments(string sWhere)
        {
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;

            try
            {
                sWhere = string.IsNullOrEmpty(sWhere) ? "TrxState IN (1) AND IVDOCTYP = 3 " : "TrxState IN (1)  AND IVDOCTYP = 3 AND " + sWhere;
                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSInventoryTransfer", false, 2, 0, sWhere, true));

                if (ds.Tables.Count == 0)
                    return null;

                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Shipping });
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.WarehouseTransferShipment });

                Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany; //WType.GetDefaultCompany();
                //Location location = WType.GetDefaultLocation();

                //En el dataset, Tables: 1 - DocumentHeader, 2 - DocumentLine, 3 - DocumentComments
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["DOCDATE"].ToString());
                        tmpData.DocNumber = dr["IVDOCNBR"].ToString();
                        tmpData.DocStatus = docStatus; //GetReceivingStatus(int.Parse(dr["POSTATUS"].ToString()));
                        tmpData.DocType = docType;
                        tmpData.PickMethod = docType.PickMethod;
                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount; //Vendor Account;
                        tmpData.Customer = defAccount;
                        tmpData.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.Reference = dr["BACHNUMB"].ToString();

                        tmpData.LastChange = GetDocumentLastChange("ReqIVHeaderView", "IVDOCNBR", dr["IVDOCNBR"].ToString());

                        //try { tmpData.ShippingMethod = WType.GetShippingMethod(new ShippingMethod { ErpCode = dr["SHIPMTHD"].ToString(), Company = company }); }
                        //catch { }

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        //tmpData.Location = WType.GetLocation(new Location { ErpCode = dr["LOCNCODE"].ToString(), Company = company }); //location;
                        tmpData.Company = CurCompany;


                        //Asignacion de Lines
                        tmpData.DocumentLines = GetLocationTransferDocumentLines(tmpData, company,
                            ds.Tables[2].Select("IVDOCNBR='" + dr["IVDOCNBR"].ToString() + "'"));

                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0) {
                            //El location debe ser el de la primera linea
                            if (tmpData.Location == null)
                                tmpData.Location = tmpData.DocumentLines[0].Location;
                            list.Add(tmpData);
                        }

                    }
                    catch (Exception ex)
                    {

                        ExceptionMngr.WriteEvent("GetLocationTransferDocuments: " + tmpData.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetLocationTransferDocuments", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }

        }


        private IList<DocumentLine> GetLocationTransferDocumentLines(Document doc, Company company, DataRow[] dLines)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

            int curLine = 0;
            string curMaster = "";

            try
            {

                foreach (DataRow dr in dLines)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    tmpData.LineNumber = (int)double.Parse(dr["LNSEQNBR"].ToString());
                    tmpData.Sequence = tmpData.LineNumber;

                    curLine = tmpData.LineNumber;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["TRXQTY"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    tmpData.LineStatus = lineStatus;

                    //Location de donde proviene la mercancia
                    curMaster = "Location:" + dr["TRXLOCTN"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = company, ErpCode = dr["TRXLOCTN"].ToString() });

                    //Lcation a donde va la mercancia
                    curMaster = "Location To:" + dr["TRNSTLOC"].ToString();
                    tmpData.Location2 = WType.GetLocation(new Location { Company = company, ErpCode = dr["TRNSTLOC"].ToString() });

                    try
                    {
                        tmpData.UnitCost = double.Parse(dr["UNITCOST"].ToString(), ListValues.DoubleFormat());
                        tmpData.ExtendedCost = tmpData.UnitCost * tmpData.Quantity;
                    }
                    catch { }



                    try
                    {
                        curMaster = "Product:" + dr["ITEMNMBR"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = dr["ITEMNMBR"].ToString() });
                        tmpData.LineDescription = tmpData.Product.Name; //dr["ITEMDESC"].ToString();

                        curMaster = "Uom:" + dr["UOFM"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetLocationTransferDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                        ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                        continue;

                    //    //Pone el Default Product
                    //    tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = WmsSetupValues.DEFAULT });
                    //    tmpData.LineDescription = "Unknown: " + dr["ITEMNMBR"].ToString();

                    //    curMaster = "Uom:" + dr["UOFM"].ToString();
                    //    tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString() });

                    }




                    list.Add(tmpData);
                }

                return (list.Count > 0) ? list : null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetTransferDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                    ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }





        #endregion




    }
}