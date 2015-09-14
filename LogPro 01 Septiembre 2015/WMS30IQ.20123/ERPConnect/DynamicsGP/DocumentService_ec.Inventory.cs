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


        #region Kit/Assembly Documents


        public IList<Document> GetKitAssemblyDocumentsSince(DateTime sinceDate)
        {
            return GetKitAssemblyDocuments(" MODIFDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        public IList<Document> GetKitAssemblyDocuments()
        {
            return GetKitAssemblyDocuments("");
        }

        private IList<Document> GetKitAssemblyDocuments(String sWhere)
        {

            //retorna la lista de Documentos de Assembly

            Document tmpData = null;

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);               

                // BM00101 - KitAssemblyHeader
                DataSet ds = ReturnDataSet("SELECT h.*,d.LOCNCODE FROM BM10200 h INNER JOIN BM10300 d ON h.TRX_ID=d.TRX_ID AND d.Parent_Component_ID=-1 WHERE h.BM_Trx_Status=3 AND h.USERDEF1 <> 'WMSEXPRESS'", sWhere, "BM10200", Command.Connection);


                if (ds == null || ds.Tables.Count == 0)
                    return null;


                List<Document> list = new List<Document>();
                Status status = WType.GetStatus(new Status { StatusID = DocStatus.New });
                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany;
                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Inventory });
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.KitAssemblyTask });


                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["TRXDATE"].ToString()); //Tran date
                        tmpData.Date2 = DateTime.Parse(dr["BM_Start_Date"].ToString()); //BM_Start_Date
                        tmpData.Date3 = DateTime.Parse(dr["PSTGDATE"].ToString()); //PSTGDATE
                        tmpData.DocNumber = dr["TRX_ID"].ToString();
                        tmpData.CreatedBy = dr["USER2ENT"].ToString();

                        try
                        {
                            tmpData.Location = WType.GetLocation(new Location { Company = CurCompany, ErpCode = dr["LOCNCODE"].ToString() });
                        }
                        catch { }

                        tmpData.DocStatus = status;

                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount;
                        tmpData.Customer = defAccount;

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        tmpData.Company = CurCompany;
                        tmpData.Reference = dr["REFRENCE"].ToString();
                        tmpData.DocType = docType;
                        tmpData.PickMethod = docType.PickMethod;

                        //Asignacion de Lines - Seguen el tipo de orden
                        tmpData.DocumentLines = GetKitAssemblyDocumentLines(tmpData);

                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0) 
                            list.Add(tmpData);


                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetKitAssemblyDocuments: " + tmpData.DocNumber, ListValues.EventType.Error, ex, null,
                            ListValues.ErrorCategory.ErpConnection);
                    }
                }


                return list;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetKitAssemblyDocuments:", ListValues.EventType.Error, ex, null,
                    ListValues.ErrorCategory.ErpConnection);

                return null;
            }

        }



        private IList<DocumentLine> GetKitAssemblyDocumentLines(Document doc)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });


            int curLine = 1;
            string curMaster = "";

            // BM10300 - KitAssembly Document Lines
            DataSet ds = ReturnDataSet("SELECT * FROM BM10300 WHERE 1=1 ", "TRX_ID='"+doc.DocNumber+"'", "BM10300", Command.Connection);


            if (ds == null || ds.Tables.Count == 0)
                return null;


            try
            {

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;

                    tmpData.LineNumber = int.Parse(dr["Component_ID"].ToString()); //curLine++;
                    tmpData.Sequence = tmpData.LineNumber;
                    tmpData.LinkDocLineNumber = int.Parse(dr["Parent_Component_ID"].ToString());
                    tmpData.Note = dr["BM_Component_Type"].ToString();

                    //TODO: Revisar el Status en GP para traer el equivalente
                    tmpData.LineStatus = lineStatus;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["Extended_Standard_Quantity"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Location";
                    tmpData.Location = WType.GetLocation(new Location { Company = CurCompany, ErpCode = dr["LOCNCODE"].ToString() });

                    try
                    {
                        curMaster = "Product";
                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() }); ;

                        curMaster = "Unit";
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetKitAssemblyDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                        ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                        continue;
                    //{
                    //    //Pone el Default Product
                    //    tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = WmsSetupValues.DEFAULT });
                    //    tmpData.LineDescription = "Unknown: " + dr["ITEMNMBR"].ToString() + ", " + dr["ITEMDESC"].ToString();

                    //    curMaster = "Unit";
                    //    tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString() });

                    }

                    list.Add(tmpData);
                }

                return (list.Count > 0) ? list : null;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetKitAssemblyDocumentLines: " + doc.DocNumber + "," + curLine.ToString() + "," + curMaster,
                    ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                throw;
                //return null;
            }
        }



        public Boolean AssemblyOrderWasDeleted(Document order)
        {
            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                Command.Connection.Open();

                string sWhere = " TRX_ID='" + order.DocNumber + "'";

                ds = ReturnDataSet("SELECT * FROM BM10200 WHERE 1=1 ", sWhere, "BM10200", Command.Connection);

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    return false;

            /*  YA POSTEADAS 
                ds = ReturnDataSet("SELECT * FROM BM30200 WHERE 1=1 ", sWhere, "BM30200", Command.Connection);

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    return false;
             */
 


                return true;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("AssemblyOrderWasDeleted", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return false;
            }
        }





        #endregion



        public IList<ProductStock> GetErpStock(ProductStock data, bool detailed)
        {
            IList<ProductStock> result = new List<ProductStock>();

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                Command.Connection.Open();

                string sQuery, table, sWhere="";
                if (detailed)
                {
                    sQuery = "SELECT * FROM IV00112 WHERE (QUANTITY > 0 OR ATYALLOC > 0) ";  //IV00112 Tabla detallada de produto por BIN
                    table = "IV00112";
                }
                else
                {
                    sQuery = "SELECT * FROM IV00102 WHERE RCRDTYPE = 2 AND QTYONHND > 0 ";
                    table = "IV00102";
                }


                if (data.Product != null && data.Product.ProductID != 0)
                    sWhere += " AND ITEMNMBR = '" + data.Product.ProductCode + "'";


                if (data.Bin != null && data.Bin.BinID != 0 && detailed)
                    sWhere += " AND BIN = '" + data.Bin.BinCode + "'";


                if (data.Bin != null && data.Bin.Location != null && data.Bin.Location.LocationID != 0)
                    sWhere += " AND LOCNCODE = '" + data.Bin.Location.ErpCode + "'";

                ds = ReturnDataSet(sQuery + sWhere, "", table, Command.Connection);

                if (ds == null || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
                    return null;


                //Creado el Ilist De ProductStock
                ProductStock record;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {


                        record = new ProductStock();
                        record.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() }); ;

                        if (detailed)
                        {
                            record.Bin = new Bin { BinCode = dr["BIN"].ToString(), Location = data.Bin.Location };
                            record.Stock = double.Parse(dr["QUANTITY"].ToString());
                            record.PackStock = double.Parse(dr["ATYALLOC"].ToString());
                        }
                        else
                        {
                            record.Bin = new Bin { BinCode = DefaultBin.MAIN, Location = data.Bin.Location };
                            record.Stock = double.Parse(dr["QTYONHND"].ToString());
                            record.PackStock = double.Parse(dr["ATYALLOC"].ToString());
                        }

                        result.Add(record);
                    }
                    catch { }
                }




                return result;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetErpStock", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }


        public Boolean SalesOrderWasDeleted(Document order)
        {
            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                Command.Connection.Open();

                //string sWhere = " SOPNUMBE='" + order.DocNumber + "'";

                ds = ReturnDataSet("SELECT SOPNUMBE FROM SOP10100 WHERE SOPTYPE IN (2,4) AND VOIDSTTS = 0 AND SOPNUMBE='" + order.DocNumber + "'", "", "SOP10100", Command.Connection);

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    return false;

                //AND VOIDSTTS = 0
                ds = ReturnDataSet("SELECT SOPNUMBE FROM SOP30200 WHERE SOPTYPE IN (2,4) AND SOPNUMBE='" + order.DocNumber + "'", "", "SOP30200", Command.Connection);

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    return true;


                return false;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("SalesOrderWasDeleted", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return false;
            }
        }




    }
}