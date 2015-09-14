using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Master;
using Entities.General;
using ErpConnect.DynamicsGP;
using Entities;
using Entities.Profile;
using System.Data;
using Integrator;
using System.Data.SqlClient;
using System.Globalization;


namespace ErpConnect.UNOEE
{
    public partial class DocumentService : SQLBase, IDocumentService
    {



        public IList<Document> GetAllShippingDocuments(int docType, bool userRemain)
        {
            //return GetShippingDocuments("", docType, userRemain);
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentsLastXDays(int days, int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentById(string code, int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentsSince(DateTime sinceDate, int docType, bool userRemain)
        {
            return GetShippingDocuments(" ISNULL(f430_fecha_ts_actualizacion,f430_fecha_ts_creacion) >= '" + sinceDate.ToString("yyyy-MM-dd HH:mm:ss") + "'", docType, userRemain);
        }

      
        public IList<Document> GetShippingDocuments(string sWhere, int docType, bool useRemain)
        {
            
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;
            string pos = "0";


            try
            {
                sWhere = ""; 
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("SALESORDER");

                //Console.WriteLine(Query);

                DataSet ds = ReturnDataSet(Query, null, "SALESORDER", Command.Connection);


                pos = "1";


                if (ds.Tables.Count == 0)
                    return null;


                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Shipping });

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
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {

                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["f430_id_fecha"].ToString());
                        tmpData.Date2 = DateTime.Parse(dr["f430_fecha_entrega"].ToString());
                        tmpData.Date3 = DateTime.Parse(dr["f430_fecha_ts_cumplido"].ToString());


                        tmpData.DocNumber = dr["documento"].ToString();
                        tmpData.ErpMaster = int.Parse(dr["f430_rowid"].ToString());
                        tmpData.DocStatus = GetShippingStatus(0);
                        tmpData.Comment = dr["f430_notas"].ToString();
                        tmpData.SalesPersonName = dr["f200_razon_social"].ToString();

                        //LAs ordenes con status void en GP, salen como canceladas.
                        try
                        {
                            if (int.Parse(dr["f430_ind_estado"].ToString()) == 9) //9 Anulado.
                                tmpData.DocStatus = cancelled;
                        }
                        catch { }

                        tmpData.CreatedBy = dr["f430_usuario_creacion"].ToString();
                        tmpData.CustPONumber = dr["f430_num_docto_referencia"].ToString();

                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount;

                        tmpData.Customer = WType.GetAccount(
                            new Account
                            {
                                AccountCode = dr["id_cliente"].ToString(),
                                BaseType = new AccountType { AccountTypeID = AccntType.Customer },
                                Company = company
                            }); 
                        try
                        {
                            if (!string.IsNullOrEmpty(dr["id_ruta"].ToString()))
                                tmpData.ShippingMethod = WType.GetShippingMethod(
                                    new ShippingMethod { ErpCode = dr["id_ruta"].ToString(), Company = company });
                        }
                        catch { }
                        //tmpData.User = user;

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;
                        tmpData.Company = CurCompany;

                        tmpData.Reference = dr["f430_referencia"].ToString();
                        //tmpData.Notes = dr["BACHNUMB"].ToString();

                        //Asignacion de Address
                        tmpData.DocumentAddresses = GetShippingDocumentAddress(tmpData, null, dr);

                        DocumentAddress billAddress = null;
                        if (!string.IsNullOrEmpty(dr["f430_id_sucursal_fact"].ToString()))
                            billAddress = GetBillAddress(tmpData, dr["f430_id_sucursal_fact"].ToString(), dr["id_cliente"].ToString(), AccntType.Customer);

                        if (billAddress != null)
                            tmpData.DocumentAddresses.Add(billAddress);

                        tmpData.DocType = soType;
                        tmpData.PickMethod = soType.PickMethod;

                        //Asignacion de Lines - Seguen el tipo de orden
                        tmpData.DocumentLines = GetShippingDocumentLines(tmpData, company, dr["f430_rowid"].ToString(), useRemain);


                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0)
                        {
                            if (tmpData.Location == null)
                                tmpData.Location = tmpData.DocumentLines[0].Location;
                            list.Add(tmpData);
                        }

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
                ExceptionMngr.WriteEvent("GetShippingDocuments:" + pos + ":", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }

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




        private IList<DocumentLine> GetShippingDocumentLines(Document doc, Company company, string docID, bool useRemain)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });


            int curLine = 0;
            string curMaster = "";


            try
            {
                Query = GetErpQuery("SALESORDER_LINE").Replace("__DOCUMENT", docID);

                DataSet ds = ReturnDataSet(Query, null, "SALESORDER_LINE", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;



                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    try { tmpData.Date2 = DateTime.Parse(dr["f431_fecha_entrega"].ToString()); }
                    catch { }
                    try { tmpData.Date3 = DateTime.Parse(dr["f431_fecha_cumplido"].ToString()); }
                    catch { }

                    tmpData.LineNumber = int.Parse(dr["f431_rowid"].ToString());
                    tmpData.Sequence = tmpData.LineNumber;
                    curLine = tmpData.LineNumber;

                    //TODO: Revisar el Status en GP para traer el equivalente
                    tmpData.LineStatus = GetShippingStatus(0);
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;

                    if (useRemain)
                        tmpData.Quantity = double.Parse(dr["f431_cant_facturada_base"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });
                    else
                        tmpData.Quantity = double.Parse(dr["f431_cant_facturada_base"].ToString(), ListValues.DoubleFormat());

                    //tmpData.QtyCancel = double.Parse(dr["QTYCANCE"].ToString(), ListValues.DoubleFormat());
                    //tmpData.QtyBackOrder = double.Parse(dr["QTYTBAOR"].ToString(), ListValues.DoubleFormat());
                    //tmpData.QtyPending = tmpData.Quantity - tmpData.QtyCancel - double.Parse(dr["QTYPRINV"].ToString(), ListValues.DoubleFormat());
                    //tmpData.QtyAllocated = double.Parse(dr["ATYALLOC"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Location:" + dr["cod_bodega"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = company, ErpCode = dr["cod_bodega"].ToString() });

                    try
                    {
                        curMaster = "Product:" + dr["f121_rowid_item"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = dr["f121_rowid_item"].ToString() });
                        tmpData.LineDescription = dr["f120_descripcion"].ToString();

                        curMaster = "Uom:" + dr["f431_id_unidad_medida"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["f431_id_unidad_medida"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

                    }
                    catch
                    {
                        //Pone el Default Product
                        tmpData.Product = WType.GetProduct(new Product { Company = doc.Location.Company, ProductCode = WmsSetupValues.DEFAULT });
                        tmpData.LineDescription = "Unknown: " + dr["f121_rowid_item"].ToString() + ", " + dr["f120_descripcion"].ToString();

                        curMaster = "Uom:" + dr["f431_id_unidad_medida"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["f431_id_unidad_medida"].ToString() });

                    }

                    //Manage Prices
                    curMaster = "Prices Product:" + dr["f121_rowid_item"].ToString();
                    tmpData.UnitPrice = double.Parse(dr["f431_precio_unitario_base"].ToString(), ListValues.DoubleFormat());
                    tmpData.ExtendedPrice = double.Parse(dr["subtotal"].ToString(), ListValues.DoubleFormat());

                    //Asignacion de Address
                    curMaster = "Address Doc:" + doc.DocNumber;
                    //tmpData.DocumentLineAddresses = GetShippingDocumentAddress(tmpData.Document, tmpData, dr);


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
            else if (doc.DocumentAddresses != null && doc.DocumentAddresses.Count > 0 && !doc.DocumentAddresses[0].AddressLine1.Equals(dr["direccion1"].ToString()))
                pass = true;

            try
            {
                //Verificacion solo para Document Lines
                if (pass)
                {

                    DocumentAddress spAddr = new DocumentAddress();
                    spAddr.Document = doc;
                    spAddr.DocumentLine = (docLine == null) ? null : docLine;
                    spAddr.Name = dr["f215_descripcion"].ToString();
                    spAddr.ErpCode = dr["f215_id"].ToString();
                    spAddr.AddressLine1 = dr["direccion1"].ToString();
                    spAddr.AddressLine2 = dr["direccion2"].ToString();
                    spAddr.AddressLine3 = dr["direccion3"].ToString();
                    spAddr.City = dr["ciudad"].ToString();
                    spAddr.State = dr["depto"].ToString();
                    spAddr.ZipCode = dr["cod_postal"].ToString();
                    spAddr.Country = dr["pais"].ToString();

                    if (docLine == null)
                    {
                        spAddr.Phone1 = dr["telefono"].ToString();
                        spAddr.Phone2 = dr["fax"].ToString();
                    }
                    else
                    {
                        spAddr.Phone1 = dr["telefono"].ToString();
                        spAddr.Phone2 = dr["fax"].ToString();
                    }

                    //spAddr.Phone3 = dr["FAXNUMBR"].ToString();
                    spAddr.ContactPerson = dr["contacto"].ToString();
                    spAddr.AddressType = AddressType.Shipping;
                    spAddr.Email = "";
                    //spAddr.ShpMethod = WType.GetShippingMethod(new ShippingMethod { Company = doc.Company, ErpCode = dr["SHIPMTHD"].ToString() }); ;
                    spAddr.CreationDate = DateTime.Now;
                    spAddr.CreatedBy = WmsSetupValues.SystemUser;

                    if (!(string.IsNullOrEmpty(dr["f215_descripcion"].ToString()) && string.IsNullOrEmpty(spAddr.AddressLine1 = dr["direccion1"].ToString())))
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


        private Status GetShippingStatus(int p)
        {
            return WType.GetStatus(new Status { StatusID = DocStatus.New });
        }



        public string CreateSalesOrder(Document document, string docPrefix, string batch)
        {
            throw new NotImplementedException();
        }

        public string CreateCustomer(Account customer)
        {
            throw new NotImplementedException();
        }

        public string CreateCustomerAddress(AccountAddress address)
        {
            throw new NotImplementedException();
        }


        public bool CreateSalesInvoice()
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetPurchaseReturnsSince(DateTime sinceDate)
        {
            return null;
        }




        #region IDocumentService Members


        public bool CreateLocationTransfer(Document docTranfer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
 