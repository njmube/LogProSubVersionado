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



namespace ErpConnect.UNOEE
{
    public partial class DocumentService : SQLBase, IDocumentService
    {
        private Company CurCompany;
        private WmsTypes WType;
        private IList<ConnectionErpSetup> ErpSetup;
        private String Query;
        private String Separator;


        public DocumentService(Company factoryCompany)
        {
            CurCompany = factoryCompany;
            WType = new WmsTypes();

            if (ErpSetup == null)
                ErpSetup = WType.GetConnectionErpSetup(new ConnectionErpSetup
                {
                    EntityType = CnnEntityType.Documents,
                    ConnectionTypeID = CnnType.UnoEE
                });

            Separator = GetErpQuery("DSEPARATOR");
        }


        private string GetErpQuery(string entityCode)
        {
            try
            {
                return ErpSetup.Where(f => f.EntityCode == entityCode)
                    .First().QueryString.Replace("__CIA", CurCompany.CompanyID.ToString());
            }
            catch { throw new Exception("Erp Setup Query not defined for " + entityCode); }
        }


        //private string GetErpReturnQuery(string entityCode)
        //{
        //    try
        //    {
        //        return ErpSetup.Where(f => f.EntityCode == entityCode)
        //            .First().ReturnQueryString.Replace("__CIA", CurCompany.CompanyID.ToString());
        //    }
        //    catch { throw new Exception("Erp Setup Return Query not defined for " + entityCode); }
        //}


        public IList<Document> GetAllReceivingDocuments()
        {
            return GetReceivingDocuments("");
        }

        public IList<Document> GetReceivingDocumentsLastXDays(int days)
        {
            return GetReceivingDocuments("DATEDIFF(day,f420_ts,GETDATE()) <= " + days.ToString());
        }

        public IList<Document> GetReceivingDocumentById(string code)
        {
            return GetReceivingDocuments("f420_rowid = '" + code + "'");
        }

        public IList<Document> GetReceivingDocumentsSince(DateTime sinceDate)
        {
            return GetReceivingDocuments(" f420_ts >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }




        public IList<Document> GetReceivingDocuments(string sWhere)
        {
            IList<Document> list = new List<Document>();
            DocumentClass docClass = new DocumentClass();
            Document tmpData = null;

            try
            {

            /*
            401	0	En elaboración
            401	1	Aprobado
            401	2	Parcial
            401	3	Cumplido
            401	9	Anulado
            */

                //Console.WriteLine("Entramos");

                sWhere = string.IsNullOrEmpty(sWhere) ? " t420.f420_ind_estado IN (1,2,3,9) " : " t420.f420_ind_estado IN (1,2,3,9) AND " + sWhere;


                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("PURCHASEORDER");

                //Console.WriteLine(Query);

                DataSet ds = ReturnDataSet(Query, null, "PURCHASEORDER", Command.Connection);


                Console.WriteLine(ds.Tables.Count);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Receiving });
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.PurchaseOrder });

                //Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany; //WType.GetDefaultCompany();
                //Location location = WType.GetDefaultLocation();


                //En el dataset, Tables: 1 - DocumentHeader, 2 - DocumentLine, 3 - DocumentComments
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["f420_fecha"].ToString());
                        //tmpData.Date2 = DateTime.Parse(dr["PRMDATE"].ToString());
                        //tmpData.Date3 = DateTime.Parse(dr["PRMSHPDTE"].ToString());
                        //tmpData.Date4 = DateTime.Parse(dr["REQDATE"].ToString());
                        //tmpData.Date5 = DateTime.Parse(dr["DUEDATE"].ToString()); //USADA PARA NOtification en IMAGE
                        tmpData.DocNumber = dr["ponumber"].ToString();
                        tmpData.DocStatus = GetReceivingStatus(int.Parse(dr["f420_ind_estado"].ToString()));
                        tmpData.DocType = docType;
                        tmpData.PickMethod = docType.PickMethod;
                        tmpData.DocConcept = docConcept;
                        tmpData.QuoteNumber = dr["f420_id_sucursal_prov"].ToString();
                        tmpData.Reference = dr["f420_num_docto_referencia"].ToString();
                        tmpData.SalesPersonName = dr["id_comprador"].ToString();
                        tmpData.UserDef1 = dr["f420_id_moneda_docto"].ToString();


                        //try { tmpData.LastChange = DateTime.Parse(dr["f420_fecha_ts_actualizacion"].ToString()); }
                        //catch { }

                        tmpData.Comment = dr["f420_notas"].ToString();


                        tmpData.Vendor = WType.GetAccount(
                            new Account
                            {
                                AccountCode = dr["id_proveedor"].ToString(),
                                BaseType = new AccountType { AccountTypeID = AccntType.Vendor },
                                Company = company
                            }); //Vendor Account;

                        tmpData.Customer = defAccount;
                        tmpData.CreatedBy = dr["f420_usuario_creacion"].ToString();

                        //try { tmpData.ShippingMethod = WType.GetShippingMethod(new ShippingMethod { ErpCode = dr["SHIPMTHD"].ToString(), Company = company }); }
                        //catch { }

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        //tmpData.Location = WType.GetLocation(new Location { ErpCode = dr["LOCNCODE"].ToString(), Company = company }); //location;
                        tmpData.Company = CurCompany;


                        //Asignacion de Lines
                        tmpData.DocumentLines = GetReceivingDocumentLines(tmpData, company, dr["f420_rowid"].ToString());


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



        private IList<DocumentLine> GetReceivingDocumentLines(Document doc, Company company, string docID)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            //Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

            int curLine = 0;
            string curMaster = "";

            try
            {
                Query = GetErpQuery("PURCHASEORDER_LINE").Replace("__DOCUMENT", docID);

                DataSet ds = ReturnDataSet(Query, null, "PURCHASEORDER_LINE", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;



                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    try { tmpData.Date2 = DateTime.Parse(dr["f421_fecha_entrega"].ToString()); }
                    catch { }

                    //tmpData.Date3 = DateTime.Parse(dr["PRMSHPDTE"].ToString());
                    //tmpData.Date4 = DateTime.Parse(dr["PRMDATE"].ToString());
                    tmpData.LineNumber = int.Parse(dr["f421_rowid"].ToString());
                    tmpData.AccountItem = dr["f421_cod_item_prov"].ToString();
                    tmpData.Sequence = tmpData.LineNumber;

                    curLine = tmpData.LineNumber;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["f421_cant_pedida"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });
                    tmpData.UnitBaseFactor = double.Parse(dr["fact_adicional"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });


                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Status:" + dr["f421_ind_estado"].ToString();
                    tmpData.LineStatus = GetReceivingStatus(int.Parse(dr["f421_ind_estado"].ToString())); //doc.DocStatus;
                    
                    curMaster = "Location:" + dr["cod_bodega"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = company, ErpCode = dr["cod_bodega"].ToString() });

                    try
                    {

                        curMaster = "Product:" + dr["f121_rowid_item"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = dr["f121_rowid_item"].ToString() });
                        tmpData.LineDescription = dr["f120_descripcion"].ToString();

                        curMaster = "Uom:" + dr["f421_id_unidad_medida"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["f421_id_unidad_medida"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

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
                    tmpData.UnitPrice = double.Parse(dr["f421_precio_unitario"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });
                    tmpData.ExtendedPrice = double.Parse(dr["subtotal"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });

                    //SOP POP Link
                    //object[] sop_popLink = GetSOP_POPLink(doc.DocNumber, tmpData.LineNumber, doc.DocType.DocTypeID);
                    //if (sop_popLink != null)
                    //{
                    //    tmpData.LinkDocNumber = sop_popLink[0].ToString();
                    //    tmpData.LinkDocLineNumber = int.Parse(sop_popLink[1].ToString());
                    //}


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




        private Status GetReceivingStatus(int GPStatus)
        {
            /*
            401	0	En elaboración
            401	1	Aprobado
            401	2	Parcial
            401	3	Cumplido
            401	9	Anulado
            */


            if (GPStatus == 3)
                return WType.GetStatus(new Status { StatusID = DocStatus.Completed });
            else if (GPStatus == 9)
                return WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
            else //es nueva (1,2,3)
                return WType.GetStatus(new Status { StatusID = DocStatus.New });
        }




        public string CreatePurchaseOrder(Document document)
        {
            throw new NotImplementedException();
        }


    }
}