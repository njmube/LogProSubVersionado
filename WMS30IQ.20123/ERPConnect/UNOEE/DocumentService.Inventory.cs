using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Master;
using ErpConnect;
using Entities.General;
using Integrator;
using System.Data.SqlClient;
using System.Data;
using Entities;
using Entities.Profile;
using System.Globalization;

namespace ErpConnect.UNOEE
{
    public partial class DocumentService : SQLBase, IDocumentService
    {


        #region IDocumentService Members



        public IList<Document> GetKitAssemblyDocumentsSince(DateTime sinceDate)
        {
            return GetKitAssemblyDocuments(" ISNULL(f350_fecha_ts_creacion,f350_fecha_ts_actualizacion) >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
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

                Query = GetErpQuery("KITDOC");

                //Console.WriteLine(Query);

                DataSet ds = ReturnDataSet(Query, null, "KITDOC", Command.Connection);


                //Console.WriteLine(ds.Tables.Count);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


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
                        tmpData.Date1 = DateTime.Parse(dr["f350_fecha"].ToString()); //Tran date
                        //tmpData.Date2 = DateTime.Parse(dr["BM_Start_Date"].ToString()); //BM_Start_Date
                        //tmpData.Date3 = DateTime.Parse(dr["PSTGDATE"].ToString()); //PSTGDATE
                        tmpData.DocNumber = dr["docnumber"].ToString();
                        tmpData.CreatedBy = dr["f350_usuario_creacion"].ToString();

                        try
                        {
                            tmpData.Location = WType.GetLocation(new Location { Company = CurCompany, ErpCode = dr["cod_bodega"].ToString() });
                        }
                        catch { }

                        tmpData.DocStatus = status;

                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount;
                        tmpData.Customer = defAccount;

                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;
                        tmpData.ErpMaster = int.Parse(dr["f350_rowid"].ToString());

                        tmpData.Company = CurCompany;
                        tmpData.Reference = dr["f350_referencia"].ToString();
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


            Query = GetErpQuery("KITDOC_LINE").Replace("__DOCUMENT", doc.ErpMaster.ToString());

            DataSet ds = ReturnDataSet(Query, null, "KITDOC_LINE", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return null;


            if (ds.Tables[0].Select("rowid=0").Length == 0)
                return null;

            try
            {

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;

                    tmpData.LineNumber = int.Parse(dr["rowid"].ToString()); //curLine++;
                    tmpData.Sequence = tmpData.LineNumber;
                    tmpData.LinkDocLineNumber = int.Parse(dr["row_padre"].ToString());
                    tmpData.Note = dr["type"].ToString();

                    //TODO: Revisar el Status en GP para traer el equivalente
                    tmpData.LineStatus = lineStatus;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["f470_cant_base"].ToString(), ListValues.DoubleFormat());
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;

                    curMaster = "Location";
                    tmpData.Location = doc.Location; //WType.GetLocation(new Location { Company = CurCompany, ErpCode = dr["LOCNCODE"].ToString() });

                    try
                    {
                        curMaster = "Product";
                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["item_id"].ToString() }); ;

                        curMaster = "Unit";
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["unit_id"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

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





        public Document GetAssemblyOrderPostedStatus(Document order)
        {
            throw new NotImplementedException();
        }

        public bool AssemblyOrderWasDeleted(Document order)
        {
            throw new NotImplementedException();
        }

        public string CreateKitAssemblyOrderBasedOnSalesDocument(Document shipment, Product product, double quantity, string sequence)
        {
            throw new NotImplementedException();
        }

        public string CancelKitAssemblyOrderBasedOnSalesDocument(Document data)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetAllLocationTransferDocuments()
        {
            return GetLocationTransferDocuments("");
        }

        public IList<Document> GetLocationTransferDocumentsSince(DateTime sinceDate)
        {
            return GetLocationTransferDocuments(" f450_ts >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<Document> GetLocationTransferDocuments(string sWhere)
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


                sWhere = string.IsNullOrEmpty(sWhere) ? "" : " AND " + sWhere;


                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("TRANSFER");

                DataSet ds = ReturnDataSet(Query, null, "TRANSFER", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                DocumentConcept docConcept = WType.GetDefaultConcept(new DocumentClass { DocClassID = SDocClass.Receiving });
                DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.InTransitShipment });


                Status docStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

                Account defAccount = WType.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT });
                SysUser user = WType.GetUser(new SysUser { UserName = WmsSetupValues.AdminUser });
                Company company = CurCompany; 



                //En el dataset, Tables: 1 - DocumentHeader
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Document();
                        tmpData.Date1 = DateTime.Parse(dr["f450_id_fecha"].ToString());
                        tmpData.DocNumber = dr["numero_doc"].ToString();
                        tmpData.DocStatus = docStatus; //GetReceivingStatus(int.Parse(dr["POSTATUS"].ToString()));
                        tmpData.DocType = docType;
                        tmpData.PickMethod = docType.PickMethod;
                        tmpData.DocConcept = docConcept;
                        tmpData.Vendor = defAccount; //Vendor Account;
                        tmpData.Customer = defAccount;
                        tmpData.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.Reference = dr["f450_docto_alterno"].ToString();

                        tmpData.UserDef1 = dr["co_salida"].ToString(); //CO Origen - SALIDA
                        tmpData.UserDef2 = dr["bodega_salida"].ToString(); //Bodega Origen - SALIDA

                        //try { tmpData.LastChange = DateTime.Parse(dr["f450_ts"].ToString()); }
                        //catch { }

        
                        tmpData.IsFromErp = true;
                        tmpData.CrossDocking = false;

                        tmpData.Location = WType.GetLocation(new Location { ErpCode = dr["bodega_entrada"].ToString(), Company = company }); //location;
                        
                        tmpData.Company = CurCompany;


                        //Asignacion de Lines
                        tmpData.DocumentLines = GetLocationTransferDocumentLines(tmpData, company, dr["f450_rowid_docto"].ToString());
                            
                        if (tmpData.DocumentLines != null && tmpData.DocumentLines.Count > 0)
                        {
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


        private IList<DocumentLine> GetLocationTransferDocumentLines(Document doc, Company company, String docID)
        {

            DocumentLine tmpData;
            IList<DocumentLine> list = new List<DocumentLine>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New });

            int curLine = 0;
            string curMaster = "";


            Query = GetErpQuery("TRANSFER_LINE").Replace("__DOCUMENT", docID);

            DataSet ds = ReturnDataSet(Query, null, "TRANSFER_LINE", Command.Connection);

            if (ds == null || ds.Tables.Count == 0)
                return null;

            try
            {

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new DocumentLine();
                    tmpData.Date1 = doc.Date1;
                    curMaster = "";

                    tmpData.LineNumber = (int)double.Parse(dr["f470_rowid"].ToString());
                    tmpData.Sequence = tmpData.LineNumber;

                    curLine = tmpData.LineNumber;
                    tmpData.Document = doc;
                    tmpData.IsDebit = false;
                    tmpData.Quantity = double.Parse(dr["f470_cantidad"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.CreationDate = DateTime.Now;
                    tmpData.Note = dr["f470_notas"].ToString();
                    tmpData.BinAffected = dr["ubicacion"].ToString();
                    tmpData.PostingUserName = dr["f470_id_un_movto"].ToString();
                    


                    tmpData.LineStatus = lineStatus;

                    //Location de donde proviene la mercancia
                    curMaster = "Location:" + dr["bodega_entrada"].ToString();
                    tmpData.Location = WType.GetLocation(new Location { Company = company, ErpCode = dr["bodega_entrada"].ToString() });

                    //Lcation a donde va la mercancia
                    curMaster = "Location To:" + dr["bodega_salida"].ToString();
                    tmpData.Location2 = WType.GetLocation(new Location { Company = company, ErpCode = dr["bodega_salida"].ToString() });
                    

                    try
                    {
                        curMaster = "Product:" + dr["itemext"].ToString();
                        tmpData.Product = WType.GetProduct(new Product { Company = company, ProductCode = dr["f121_rowid_item"].ToString() });
                        tmpData.LineDescription = tmpData.Product.Name; //dr["ITEMDESC"].ToString();
                        tmpData.AccountItem = dr["itemext"].ToString(); //ITEM EXT

                        curMaster = "Uom:" + dr["f470_id_unidad_medida"].ToString();
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["f470_id_unidad_medida"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });

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





        public void UpdateSalesDocumentBatch(string salesDocument, string batchNumber)
        {
            throw new NotImplementedException();
        }

        public IList<ProductStock> GetErpStock(Entities.General.ProductStock data, bool detailed)
        {
            throw new NotImplementedException();
        }



        public bool SalesOrderWasDeleted(Document curDocument)
        {
            throw new NotImplementedException();
        }



        public bool FulFillMergedSalesDocument(Document ssDocument, IList<DocumentLine> iList, bool fistTimeFulfill, string batchNumber)
        {
            throw new NotImplementedException();
        }


        public int SaveUpdateErpDocumentLine(DocumentLine docLine, bool removeLine)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
 