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
        
        private StringWriter swriter;
        private DataSet ds;
        private WmsTypes WType;
        private Company CurCompany;


        public DocumentService_ec(Company factoryCompany)
        {
            // initialization 
            swriter = new StringWriter();
            WType = new WmsTypes();
            ds = new DataSet();
            CurCompany = factoryCompany;
        }




        #region ReceiptDocuments

        public IList<Document> GetAllReceivingDocuments() { return GetReceivingDocuments(""); }


        public IList<Document> GetReceivingDocumentsLastXDays(int days)
        {
            return GetReceivingDocuments("DATEDIFF(day,DEX_ROW_TS,GETDATE()) <= " + days.ToString());
        }


        public IList<Document> GetReceivingDocumentById(string code) { return GetReceivingDocuments("PONUMBER = '" + code + "'"); }


        public IList<Document> GetReceivingDocumentsSince(DateTime sinceDate)
        {
            return GetReceivingDocuments(" DEX_ROW_TS >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        public IList<Document> GetPurchaseReturnsSince(DateTime sinceDate)
        {
            return GetPurchaseReturns(" (MODIFDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "' OR CREATDDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "')", 7); //7 Inventory w/Credit Return.
        }


        public IList<Document> GetReceivingDocuments(string sWhere)
        {
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;

            try
            {
                sWhere = string.IsNullOrEmpty(sWhere) ? "POSTATUS IN (1,2,3,4,5,6)" : "POSTATUS IN (1,2,3,4,5,6) AND " + sWhere;
                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("Purchase_Order_Transaction", false, 2, 0, sWhere, true));

                if (ds.Tables.Count == 0)
                    return null;

                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Receiving });
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.PurchaseOrder });

                //Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

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
                        tmpData.Date2 = DateTime.Parse(dr["PRMDATE"].ToString());
                        tmpData.Date3 = DateTime.Parse(dr["PRMSHPDTE"].ToString());
                        tmpData.Date4 = DateTime.Parse(dr["REQDATE"].ToString());
                        //tmpData.Date5 = DateTime.Parse(dr["DUEDATE"].ToString()); //USADA PARA NOtification en IMAGE
                        tmpData.DocNumber = dr["PONUMBER"].ToString();
                        tmpData.DocStatus = GetReceivingStatus(int.Parse(dr["POSTATUS"].ToString()));
                        tmpData.DocType = docType;
                        tmpData.PickMethod = docType.PickMethod;
                        tmpData.DocConcept = docConcept;
                        try { tmpData.CustPONumber = dr["POPCONTNUM"].ToString(); }
                        catch { }


                        tmpData.LastChange = GetDocumentLastChange("POP10100", "PONUMBER", dr["PONUMBER"].ToString());

                        try
                        {
                            tmpData.Comment = GetDocumentNotesPurchase("", dr["COMMNTID"].ToString(), dr["PONUMBER"].ToString());
                        }
                        catch { }



                        tmpData.Vendor = WType.GetAccount(
                            new Account
                            {
                                AccountCode = dr["VENDORID"].ToString(),
                                BaseType = new AccountType { AccountTypeID = AccntType.Vendor },
                                Company = company
                            }); //Vendor Account;

                        tmpData.Customer = defAccount;
                        tmpData.CreatedBy = dr["USER2ENT"].ToString();
                        try { tmpData.ShippingMethod = WType.GetShippingMethod(new ShippingMethod { ErpCode = dr["SHIPMTHD"].ToString(), Company = company }); }
                        catch { }

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        //tmpData.Location = WType.GetLocation(new Location { ErpCode = dr["LOCNCODE"].ToString(), Company = company }); //location;
                        tmpData.Company = CurCompany;


                        //Asignacion de Lines
                        tmpData.DocumentLines = GetReceivingDocumentLines(tmpData, company,
                            ds.Tables[2].Select("PONUMBER='" + dr["PONUMBER"].ToString() + "'"));


                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0)
                        {
                            if (tmpData.Location == null)
                                tmpData.Location = tmpData.DocumentLines[0].Location;
                            list.Add(tmpData);
                        }

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetReceiptDocuments: " + tmpData.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetReceiptDocuments", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }

        }



        private Status GetReceivingStatus(int GPStatus)
        {
            if (GPStatus == 4 || GPStatus == 5)
                return WType.GetStatus(new Status { StatusID = DocStatus.Completed });
            else if (GPStatus == 6)
                return WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
            else //es nueva (1,2,3)
                return WType.GetStatus(new Status { StatusID = DocStatus.New });
        }



        private IList<DocumentLine> GetReceivingDocumentLines(Document doc, Company company, DataRow[] dLines)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            //Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

            int curLine = 0;
            string curMaster = "";

            try
            {

                foreach (DataRow dr in dLines)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    tmpData.Date2 = DateTime.Parse(dr["REQDATE"].ToString());
                    tmpData.Date3 = DateTime.Parse(dr["PRMSHPDTE"].ToString());
                    tmpData.Date4 = DateTime.Parse(dr["PRMDATE"].ToString());
                    tmpData.LineNumber = int.Parse(dr["ORD"].ToString());
                    tmpData.AccountItem = dr["VNDITNUM"].ToString();
                    tmpData.Sequence = tmpData.LineNumber;

                    curLine = tmpData.LineNumber;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["QTYORDER"].ToString(), ListValues.DoubleFormat());
                    tmpData.QtyCancel = double.Parse(dr["QTYCANCE"].ToString(), ListValues.DoubleFormat());
                    tmpData.QtyPending = double.Parse(dr["QTYUNCMTBASE"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Status:" + dr["POLNESTA"].ToString();
                    tmpData.LineStatus = GetReceivingStatus(int.Parse(dr["POLNESTA"].ToString())); //doc.DocStatus;
                    curMaster = "Location:" + dr["LOCNCODE"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = company, ErpCode = dr["LOCNCODE"].ToString() });

                    try
                    {
                        curMaster = "Product:" + dr["ITEMNMBR"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = dr["ITEMNMBR"].ToString() });
                        tmpData.LineDescription = dr["ITEMDESC"].ToString();

                        curMaster = "Uom:" + dr["UOFM"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetReceiptDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                        ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                        continue;

                        //    //Pone el Default Product
                        //    tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = WmsSetupValues.DEFAULT });
                        //    tmpData.LineDescription = "Unknown: " + dr["ITEMNMBR"].ToString() + ", " + dr["ITEMDESC"].ToString();

                        //    curMaster = "Uom:" + dr["UOFM"].ToString();
                        //    tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString() });

                    }

                    //Unit - Price, Cost
                    tmpData.UnitCost = double.Parse(dr["UNITCOST"].ToString(), ListValues.DoubleFormat());
                    tmpData.ExtendedCost = double.Parse(dr["EXTDCOST"].ToString(), ListValues.DoubleFormat());

                    //SOP POP Link
                    object[] sop_popLink = GetSOP_POPLink(doc.DocNumber, tmpData.LineNumber, doc.DocType.DocTypeID);
                    if (sop_popLink != null)
                    {
                        tmpData.LinkDocNumber = sop_popLink[0].ToString();
                        tmpData.LinkDocLineNumber = int.Parse(sop_popLink[1].ToString());
                    }


                    list.Add(tmpData);
                }

                return (list.Count > 0) ? list : null;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetReceiptDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                    ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }



        public void ReceiptReturn(Document prDocument, IList<Label> listofReturn)
        {

            if (listofReturn == null || listofReturn.Count == 0)
                return;

            Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
            Command.Connection.Open();

            // SOP10100 - Sales Order Header
            string sWhere = "SOPNUMBE = '" + prDocument.CustPONumber + "'";
            DataSet ds = ReturnDataSet("SELECT * FROM SOP10100 WHERE SOPTYPE = 4 ", sWhere, "SOP10100", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return; //No sales orden in ERP to Update


            Command.Parameters.Clear();
            Command.CommandText = "UPDATE SOP10200 SET QTYDMGED=0,QTYONHND=0  WHERE SOPTYPE = 4 AND SOPNUMBE = @sopnumber";
            // Create and prepare an SQL statement.
            AddParameter("@sopnumber", prDocument.CustPONumber);
            Command.ExecuteNonQuery();


            string LineNumber;
            foreach (Product product in listofReturn.Select(f => f.Product).Distinct())
            {

                LineNumber = "";

                //Cantidades a Setear en los document Lines
                //QTYDMGED
                Double qtyDamage = 0;
                try { qtyDamage = listofReturn.Where(f => f.Bin.BinCode == DefaultBin.DAMAGE && f.Product.ProductID == product.ProductID).Sum(f => f.CurrQty); }
                catch { }

                //QTYONHND
                Double qtyOnHnd = 0;
                try { qtyOnHnd = listofReturn.Where(f => f.Bin.BinCode == DefaultBin.RETURN && f.Product.ProductID == product.ProductID).Sum(f => f.CurrQty); }
                catch { }

                //Obtener el LineNUmber del producto a Actualizar.
                LineNumber = ReturnScalar("SELECT TOP 1 LNITMSEQ FROM SOP10200 WHERE SOPTYPE = 4 AND ITEMNMBR='" + product.ProductCode + "' AND SOPNUMBE ='" + prDocument.CustPONumber + "'", "", Command.Connection);


                if (Command.Connection.State != ConnectionState.Open)
                    Command.Connection.Open();

                if (!string.IsNullOrEmpty(LineNumber))
                {
                    //Header
                    Command.Parameters.Clear();
                    Command.CommandText = "UPDATE SOP10200 SET QTYDMGED=@qtyDamage,QTYONHND=@qtyOnHnd  WHERE SOPTYPE = 4 AND ITEMNMBR=@product AND SOPNUMBE = @sopnumber AND LNITMSEQ = @Line";
                    // Create and prepare an SQL statement.
                    AddParameter("@qtyDamage", qtyDamage);
                    AddParameter("@product", product.ProductCode);
                    AddParameter("@sopnumber", prDocument.CustPONumber);
                    AddParameter("@qtyOnHnd", qtyOnHnd);
                    AddParameter("@Line", int.Parse(LineNumber));

                    // Call Prepare after setting the Commandtext and Parameters.
                    Command.ExecuteNonQuery();
                }
            }


            Command.Connection.Close();

        }



        public IList<Document> GetPurchaseReturns(string sWhere, int docType)
        {
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;
            string pos = "0";

            try
            {
                //Lamar los documents que necesita del Erp usando econnect
                sWhere = string.IsNullOrEmpty(sWhere) ? "POPTYPE IN (7,6,5,4)" : "POPTYPE IN (7,6,5,4) AND " + sWhere;
                string xmlData = DynamicsGP_ec.RetreiveData("PO_Receiving_Transaction", false, 2, 0, sWhere, true);

                pos = "1";

                int rem = 0x02;
                xmlData = xmlData.Replace((char)rem, ' ');
                ds = DynamicsGP_ec.GetDataSet(xmlData);

                pos = "2";


                Console.WriteLine("\t" + ds.Tables.Count);

                if (ds.Tables.Count == 0)
                    return null;


                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Shipping });

                //Definiendo los tipos de documento de return
                DocumentType prtType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.PurchaseReturn });


                //Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });
                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany; // WType.GetDefaultCompany();
                Status cancelled = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });


                //Console.WriteLine(ds.GetXml());

                //En el dataset, Tables: 1 - DocumentHeader, 2 - DocumentLine, 3 - DocumentComments
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    try
                    {

                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["receiptdate"].ToString());
                        tmpData.Date2 = DateTime.Parse(dr["DUEDATE"].ToString());


                        tmpData.DocNumber = dr["POPRCTNM"].ToString();
                        tmpData.DocStatus = GetShippingStatus(0);

                        //try
                        //{
                        //    tmpData.Comment = GetDocumentNotes(dr["NOTEINDX"].ToString(), dr["COMMNTID"].ToString());
                        //}
                        //catch { }

                        tmpData.CreatedBy = dr["USER2ENT"].ToString();
                        tmpData.CustPONumber = dr["VNDDOCNM"].ToString();

                        tmpData.DocConcept = docConcept;

                        tmpData.Vendor = tmpData.Customer = WType.GetAccount(new Account
                        {
                            AccountCode = dr["VENDORID"].ToString(),
                            BaseType = new AccountType { AccountTypeID = AccntType.Vendor },
                            Company = company
                        });

                        //Console.WriteLine("1");

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

                        tmpData.Company = CurCompany;

                        tmpData.Reference = dr["REFRENCE"].ToString();
                        tmpData.Notes = dr["BACHNUMB"].ToString();

                        //Console.WriteLine("2");

                        //Asignacion de Address
                        //tmpData.DocumentAddresses = GetPurchaseReturnsAddress(tmpData, null, dr);

                        DocumentAddress billAddress = null;
                        try { billAddress = GetBillAddress(tmpData, "", dr["VENDORID"].ToString(), AccntType.Vendor); }
                        catch { }

                        tmpData.DocumentAddresses = new List<DocumentAddress>();

                        if (billAddress != null)
                            tmpData.DocumentAddresses.Add(billAddress);


                        tmpData.DocType = prtType;
                        try { tmpData.PickMethod = prtType.PickMethod; }
                        catch { }

                        //Console.WriteLine("3");

                        //Asignacion de Lines - Seguen el tipo de orden
                        tmpData.DocumentLines = GetPurchaseReturnsLines(tmpData, ds.Tables[2].Select("POPRCTNM='" + dr["POPRCTNM"].ToString() + "'"),
                            ds.Tables[3]);


                        //Console.WriteLine("4");

                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0)
                        {
                            if (tmpData.Location == null)
                                tmpData.Location = tmpData.DocumentLines[0].Location;

                            list.Add(tmpData);
                        }

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetPurchaseReturns: " + tmpData.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }

                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetPurchaseReturns:" + pos + ":", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }

        }


        private IList<DocumentLine> GetPurchaseReturnsLines(Document doc, DataRow[] dLines, DataTable dtQty)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });


            int curLine = 0;
            string curMaster = "";
            DataRow[] qtyReceipt;

            try
            {                

                foreach (DataRow dr in dLines)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";


                    tmpData.LineNumber = int.Parse(dr["RCPTLNNM"].ToString());
                    tmpData.Sequence = tmpData.LineNumber;
                    curLine = tmpData.LineNumber;

                    try
                    {
                        qtyReceipt = dtQty.Select("RCPTLNNM = '" + dr["RCPTLNNM"].ToString() + "' AND POPRCTNM='" + dr["POPRCTNM"].ToString() + "'");

                        //Console.WriteLine("RCPTLNNM = " + dr["RCPTLNNM"].ToString() + " AND POPRCTNM='" + dr["POPRCTNM"].ToString() + "'" + qtyReceipt.Length.ToString());

                        tmpData.Quantity = double.Parse(qtyReceipt[0]["QTYRESERVED"].ToString(), ListValues.DoubleFormat());
                    }
                    catch (Exception ez) {

                        ExceptionMngr.WriteEvent("QTYRESERVED: " + dr["POPRCTNM"].ToString() + ","+ dr["RCPTLNNM"].ToString(),
                            ListValues.EventType.Error, ez, null, ListValues.ErrorCategory.ErpConnection);

                        tmpData.Quantity = 0; 
                    }


                    //TODO: Revisar el Status en GP para traer el equivalente
                    tmpData.LineStatus = GetShippingStatus(0);
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Location:" + dr["LOCNCODE"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = CurCompany, ErpCode = dr["LOCNCODE"].ToString() });

                    try
                    {
                        curMaster = "Product:" + dr["ITEMNMBR"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() }); ;
                        tmpData.LineDescription = dr["ITEMDESC"].ToString();

                        curMaster = "Uom:" + dr["UOFM"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetPurchaseReturnsLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                        ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                        continue;

                        //{
                        //    //Pone el Default Product
                        //    tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = WmsSetupValues.DEFAULT });
                        //    tmpData.LineDescription = "Unknown: " + dr["ITEMNMBR"].ToString() + ", " + dr["ITEMDESC"].ToString();

                        //    curMaster = "Uom:" + dr["UOFM"].ToString();
                        //    tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString() });

                    }




                    //Manage Prices
                    tmpData.UnitPrice = double.Parse(dr["UNITCOST"].ToString(), ListValues.DoubleFormat());
                    tmpData.ExtendedPrice = double.Parse(dr["EXTDCOST"].ToString(), ListValues.DoubleFormat());

                    //Asignacion de Address
                    //tmpData.DocumentLineAddresses = GetShippingDocumentAddress(tmpData.Document, tmpData, dr);

                    list.Add(tmpData);
                }

                return (list.Count > 0) ? list : null;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetPurchaseReturnsLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                    ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }




        #endregion



        public String CreatePurchaseOrder(Document document)
        {
            string Flag = "";



            if (document.DocumentLines == null || document.DocumentLines.Count <= 0)
                throw new Exception("Purchase order does not contains lines.");


            try
            {

            string POPNumber;
            //string DocPrefix = "SWEB"; //ej: SWEB o como se deban crear las ordenes
            //string Batch = "BATCH"; //Numero de Batch de las ordenes a Crear

            // Next consecutive for a P.O.
            Flag = "PO Sequence";

            GetNextDocNumbers nextPopNumber = new GetNextDocNumbers();
            POPNumber = nextPopNumber.GetNextPONumber(GetNextDocNumbers.IncrementDecrement.Increment,
                CurCompany.ErpConnection.CnnString);
 
                taPoLine_ItemsTaPoLine[] docLines = new taPoLine_ItemsTaPoLine[document.DocumentLines.Count];

                //Create an object that holds XML node object
                taPoLine_ItemsTaPoLine curLine;
                int i = 1;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in document.DocumentLines)
                {
                    //Debe ser active, para garantizar que no es Misc, o service Item
                    if (dr.Product.Status.StatusID == EntityStatus.Active)
                    {

                        curLine = new taPoLine_ItemsTaPoLine();

                        //Validate Item/Vendor, GP requires that the Vendor has assigned the ItemNumber 
                        ValidateItemAndVendor(document.Vendor.AccountCode, dr.Product.ProductCode);


                        //Validate Item/Location, GP requires that the Location has assigned the ItemNumber 
                        ValidateItemAndLocation(document.Location.ErpCode, dr.Product.ProductCode);

                        // Populate Lines      
                        Flag = "Line Info";
                        curLine.PONUMBER = POPNumber;
                        curLine.POTYPE = GP_DocType.PO_Standard;
                        curLine.VENDORID = document.Vendor.AccountCode;
                        curLine.QUANTITY = (decimal)dr.Quantity;
                        curLine.QUANTITYSpecified = true;
                        curLine.REQDATE = DateTime.Today.ToString("yyyy-MM-dd");
                        curLine.ITEMNMBR = dr.Product.ProductCode;
                        curLine.LOCNCODE = document.Location.ErpCode;
                        curLine.ORD = i;
                        curLine.UOFM = dr.Unit.ErpCode;
                        curLine.POLNESTA = 1; //NEW


                        docLines[i - 1] = curLine;
                        i++;
                    }
                }

                //Create a SOPTransactionType schema object and populate its taSopLineIvcInsert_Items poperty
                POPTransactionType docType = new POPTransactionType();

                //Adicionado Track Lists
                docType.taPoLine_Items = docLines;

                //Create a taSopHdrIvcInsert XML node object
                taPoHdr docHdr = new taPoHdr();

                //Populate Header   
                Flag = "Header Info";

                docHdr.PONUMBER = POPNumber;
                docHdr.POSTATUS = 1; //NEW
                docHdr.POTYPE = GP_DocType.PO_Standard;
                docHdr.REQDATE = DateTime.Today.ToString("yyyy-MM-dd");
                docHdr.VENDORID = document.Vendor.AccountCode;
                docHdr.NOTETEXT = document.Comment;

                docType.taPoHdr = docHdr;

                POPTransactionType[] docTypeArray = new POPTransactionType[1];
                docTypeArray[0] = docType;

                //Create an eConnect XML document object and populate its docType property with
                //the docType schema object
                Flag = "eConnect";

                eConnectType eConnect = new eConnectType();
                eConnect.POPTransactionType = docTypeArray;


                //Serialize the XML document to the file
                XmlSerializer serializer = new XmlSerializer(typeof(eConnectType));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, eConnect);

                DynamicsGP_ec.SendData(writer.ToString());

                return POPNumber;
            }

            catch (Exception ex)
            {
                //ExceptionMngr.WriteEvent("CreatePurchaseOrder: ", ListValues.EventType.Error, ex, null,
                //    ListValues.ErrorCategory.ErpConnection);

                throw new Exception(Flag + ". " + WriteLog.GetTechMessage(ex));
            }
        }



        #region IDocumentService Members


        public void CreateTransferReceipt(Document prDocument, IList<NodeTrace> traceList)
        {
            return;
        }

        #endregion
    }
}