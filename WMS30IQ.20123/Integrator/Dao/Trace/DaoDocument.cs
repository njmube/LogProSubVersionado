using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.Master;
using Entities.General;
using Entities.Report;
using System.Linq;
using System.Data;


namespace Integrator.Dao.Trace
{
    public class DaoDocument : DaoService
    {
        public DaoDocument(DaoFactory factory) : base(factory) { }

        public Document Save(Document data)
        {
            return (Document)base.Save(data);
        }


        public Boolean Update(Document data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Document data)
        {
            return base.Delete(data);
        }


        public Document SelectById(Document data)
        {
            return (Document)base.SelectById(data);
        }


        public IList<Document> Select(Document data)
        {

                IList<Document> datos = new List<Document>();
                try
                {
                    datos = GetHsql(data).List<Document>();
                    //if (!Factory.IsTransactional)
                    //    Factory.Commit();
                }
                            
                catch (Exception e)
                {
                    NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
                }
                return datos;
           
        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Document a    where  ");
            Document document = (Document)data;
            if (document != null)
            {
                Parms = new List<Object[]>();

                if (document.DocID != 0)
                {
                    sql.Append(" a.DocID = :id     and   ");
                    Parms.Add(new Object[] { "id", document.DocID });
                }

		        if (document.DocType != null && document.DocType.DocTypeID != 0)
                {
                    sql.Append(" a.DocType.DocTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", document.DocType.DocTypeID });

                }

                if (document.DocType != null && document.DocType.DocClass != null && document.DocType.DocClass.DocClassID != 0)
                {
                    sql.Append(" a.DocType.DocClass.DocClassID = :id15     and   ");
                    Parms.Add(new Object[] { "id15", document.DocType.DocClass.DocClassID });
                }

                if (document.DocConcept != null && document.DocConcept.DocConceptID != 0)
                {
                    sql.Append(" a.DocConcept.DocConceptID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", document.DocConcept.DocConceptID });
                }

                if (!String.IsNullOrEmpty(document.DocNumber))
                {
                    sql.Append(" (a.DocNumber = :nomr Or a.DocNumber like :nom Or a.DocNumber like :nomx)   and ");
                    Parms.Add(new Object[] { "nom", document.DocNumber + "%" });
                    Parms.Add(new Object[] { "nomx", "%" + document.DocNumber });
                    Parms.Add(new Object[] { "nomr", document.DocNumber });
                }


                if (!String.IsNullOrEmpty(document.Search))
                {
                    sql.Append(" (a.DocNumber = :nomr Or a.DocNumber like :nomz  OR a.Reference LIKE :nomz OR a.Comment LIKE :nomz OR a.Notes LIKE :nomz OR a.CustPONumber LIKE :nomz OR a.QuoteNumber LIKE :nomz)   and ");
                    //Parms.Add(new Object[] { "nom", document.Search + "%" });
                    //Parms.Add(new Object[] { "nomx", "%" + document.Search });
                    Parms.Add(new Object[] { "nomr", document.Search });
                    Parms.Add(new Object[] { "nomz", "%" + document.Search + "%" });
                }


                if (document.ErpMaster != 0)
                {
                    sql.Append(" a.ErpMaster = :id3     and   ");
                    Parms.Add(new Object[] { "id3", document.ErpMaster });
                }

                if (document.Customer != null && document.Customer.AccountID != 0)
                {
                    sql.Append(" a.Customer.AccountID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", document.Customer.AccountID });
                }

                if (document.Vendor != null && document.Vendor.AccountID != 0)
                {
                    sql.Append(" a.Vendor.AccountID = :id5     and   ");
                    Parms.Add(new Object[] { "id5", document.Vendor.AccountID });
                }


                if (document.Customer != null && !string.IsNullOrEmpty(document.Customer.Name))
                {
                    sql.Append(" a.Customer.Name like :id20     and   ");
                    Parms.Add(new Object[] { "id20", "%"+document.Customer.Name+"%" });
                }

                if (document.Vendor != null && !string.IsNullOrEmpty(document.Vendor.Name))
                {
                    sql.Append(" a.Vendor.Name like :id21    and   ");
                    Parms.Add(new Object[] { "id21", "%" + document.Vendor.Name + "%" });
                }


                if (document.DocStatus != null && document.DocStatus.StatusID != 0)
                {
                   
                    //if (document.DocStatus.StatusID == DocStatus.PENDING)
                    //{
                    //    Console.WriteLine(document.DocStatus.StatusID.ToString() + " " + DocStatus.PENDING.ToString());
                    //    sql.Append(" and ( a.DocStatus.StatusID = :idx2  Or a.DocStatus.StatusID = :idx3) ");
                    //    Parms.Add(new Object[] { "idx2", DocStatus.New });
                    //    Parms.Add(new Object[] { "idx3", DocStatus.InProcess });
                    //}
                    //else
                    //{
                        sql.Append(" a.DocStatus.StatusID = :id6     and   ");
                        Parms.Add(new Object[] { "id6", document.DocStatus.StatusID });
                    //}

                }

                if (!String.IsNullOrEmpty(document.SalesPersonName))
                {
                    sql.Append(" a.SalesPersonName = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", document.SalesPersonName });
                }

                if (!String.IsNullOrEmpty(document.QuoteNumber))
                {
                    sql.Append(" a.QuoteNumber = :id7     and   ");
                    Parms.Add(new Object[] { "id7", document.QuoteNumber });
                }

                if (!String.IsNullOrEmpty(document.CustPONumber))
                {
                    sql.Append(" a.CustPONumber = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", document.CustPONumber });
                }

                if (!String.IsNullOrEmpty(document.Reference))
                {
                    sql.Append(" a.Reference = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", document.Reference });
                }


                if (!String.IsNullOrEmpty(document.Comment))
                {
                    sql.Append(" a.Comment Like :nom11     and   ");
                    Parms.Add(new Object[] { "nom11", '%' + document.Comment + '%' });
                }

                if (!String.IsNullOrEmpty(document.Notes))
                {
                    sql.Append(" a.Notes Like :nom199     and   ");
                    Parms.Add(new Object[] { "nom199", document.Notes + '%' });
                }


                if (document.Date1 != null)
                {
                    sql.Append(" a.Date1 = :nom4     and   ");
                    Parms.Add(new Object[] { "nom4", document.Date1 });
                }

                if (document.Date2 != null)
                {
                    sql.Append(" a.Date2 = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", document.Date2 });
                }

                if (document.Date3 != null)
                {
                    sql.Append(" a.Date3 = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", document.Date3 });
                }

                if (document.Date4 != null)
                {
                    sql.Append(" a.Date4 = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", document.Date4 });
                }

                if (document.Date5 != null)
                {
                    sql.Append(" a.Date5 = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", document.Date5 });
                }



                if (document.PickMethod != null && document.PickMethod.MethodID != 0)
                {
                    sql.Append(" a.PickMethod.MethodID = :ip3    and   ");
                    Parms.Add(new Object[] { "ip3", document.PickMethod.MethodID });
                }

                //if (document.User != null && document.User.UserID != 0)
                //{
                //    sql.Append(" a.User.UserID = :id8   and   ");
                //    Parms.Add(new Object[] { "id8", document.User.UserID });
                //}

                if (document.Location != null && document.Location.LocationID != 0)
                {
                    sql.Append(" (a.Location.LocationID = :id9 OR a.Location.LocationID Is Null)    and   ");
                    Parms.Add(new Object[] { "id9", document.Location.LocationID });
                }


                if (document.Company != null && document.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id96     and   ");
                    Parms.Add(new Object[] { "id96", document.Company.CompanyID });
                }


                if (document.IsFromErp != null)
                {
                    sql.Append(" a.IsFromErp = :nom9     and   ");
                    Parms.Add(new Object[] { "nom9", document.IsFromErp });
                }



                if (document.CrossDocking != null)
                {
                    sql.Append(" a.CrossDocking = :nom10     and   ");
                    Parms.Add(new Object[] { "nom10", document.CrossDocking });
                }


                if (!string.IsNullOrEmpty(document.PostingDocument))
                {
                    if (document.PostingDocument == "0")
                        sql.Append(" a.PostingDocument is null and   ");
                    else if (document.PostingDocument == "1")
                        sql.Append(" a.PostingDocument is not null and   ");
                    else
                    {
                        sql.Append(" a.PostingDocument = :nom30    and   ");
                        Parms.Add(new Object[] { "nom30", document.PostingDocument });
                    }
                }


                if (document.ShippingMethod != null && document.ShippingMethod.ShpMethodID != 0)
                {
                    sql.Append(" a.ShippingMethod.ShpMethodID = :id96     and   ");
                    Parms.Add(new Object[] { "id96", document.ShippingMethod.ShpMethodID });
                }


             }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.DocID desc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());

            //Console.WriteLine(sql.ToString());

            SetParameters(query);
            return query;
        }


        public IList<Object[]> GetDocumentConsolidation(Document taskDoc)
        {
            int i = 1;
            IList<TaskDocumentRelation> taskDocRel = Factory.DaoTaskDocumentRelation().Select(new TaskDocumentRelation { TaskDoc = taskDoc });
            //Consolidar las Lineas de cada documento asociado a la tarea

            StringBuilder sql = new StringBuilder("select Sum(a.Quantity - a.QtyCancel), a.Product.ProductID, a.Unit.UnitID, a.UnitBaseFactor from DocumentLine a  where  ");

            if (taskDocRel != null && taskDocRel.Count > 0)
            {
                Parms = new List<Object[]>();
                
                foreach (TaskDocumentRelation t in taskDocRel)
                {
                    string sOR = (i > 1) ? " or " : " ( " ;
  
                    sql.Append(sOR + " a.Document.DocID = :id" + i.ToString());
                    Parms.Add(new Object[] { "id" + i.ToString(), t.IncludedDoc.DocID });

                    i++;
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" ) and a.LineStatus.StatusID != " + DocStatus.Cancelled.ToString() + " group by a.Product, a.Unit, a.UnitBaseFactor "); 
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);


             return   query.List<Object[]>();

        }


        public IList<Document> SelectPending(Document document, int daysAgo, int records)
        {

            daysAgo = daysAgo == 0 ? WmsSetupValues.HistoricDaysToShow : daysAgo;
            records = records == 0 ? WmsSetupValues.NumRegs : records;



            StringBuilder sql = new StringBuilder("select a from Document a  where 1=1  "); //a.PostingDocument is null

            if (document != null)
            {
                Parms = new List<Object[]>();


                if (document.DocType != null && document.DocType.DocTypeID != 0)
                {
                    sql.Append(" and a.DocType.DocTypeID = :id1   ");
                    Parms.Add(new Object[] { "id1", document.DocType.DocTypeID });

                }

                if (document.DocType != null && document.DocType.DocClass != null && document.DocType.DocClass.DocClassID != 0)
                {
                    sql.Append(" and a.DocType.DocClass.DocClassID = :id15  ");
                    Parms.Add(new Object[] { "id15", document.DocType.DocClass.DocClassID });
                }


                if (document.Customer != null && document.Customer.AccountID != 0)
                {
                    sql.Append(" and a.Customer.AccountID = :idc4     ");
                    Parms.Add(new Object[] { "idc4", document.Customer.AccountID });
                }


                if (document.Vendor != null && document.Vendor.AccountID != 0)
                {
                    sql.Append(" and a.Vendor.AccountID = :idv5      ");
                    Parms.Add(new Object[] { "idv5", document.Vendor.AccountID });
                }

                if (document.Date1 != null) //Date1 Contiene la fecha de Rerefencia
                {
                    sql.Append(" and ( a.Date2  <= :nom4  Or  a.Date4 <= :nom4 )  ");
                    Parms.Add(new Object[] { "nom4", document.Date1 }); //DateTime.Today
                }


                if (document.Date2 != null) 
                {
                    sql.Append(" and a.Date2  = :nomd2 ");
                    Parms.Add(new Object[] { "nomd2", document.Date2 });
                }


                if (document.ShippingMethod != null && document.ShippingMethod.ShpMethodID != 0)
                {
                    sql.Append(" and a.ShippingMethod.ShpMethodID = :id96  ");
                    Parms.Add(new Object[] { "id96", document.ShippingMethod.ShpMethodID });
                }


                if (!string.IsNullOrEmpty(document.UserDef1))
                {
                    sql.Append(" and a.UserDef1 = :usd1 ");
                    Parms.Add(new Object[] { "usd1", document.UserDef1 });
                }


                if (document.Location != null && document.Location.LocationID != 0)
                {
                    sql.Append(" and  (a.Location.LocationID = :id9 OR a.Location.LocationID Is Null) ");
                    Parms.Add(new Object[] { "id9", document.Location.LocationID });
                }

                if (document.Arrived != null)
                {
                    if (document.Arrived == false)
                        sql.Append(" and a.Date5 is null ");
                    else
                        sql.Append(" and a.Date5 is not null ");                    
                }


                sql.Append(" and a.Date1  >= :dtx4 ");
                Parms.Add(new Object[] { "dtx4", DateTime.Today.AddDays(-1*daysAgo) });

                sql.Append(" and ( a.DocStatus.StatusID = :id2  Or a.DocStatus.StatusID = :id3 Or a.DocStatus.StatusID = :id4 ) ");
                Parms.Add(new Object[] { "id2", DocStatus.New });
                Parms.Add(new Object[] { "id3", DocStatus.InProcess });
                Parms.Add(new Object[] { "id4", DocStatus.Completed });

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("  order by a.DocStatus.StatusID, a.Date1 desc");

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query.SetMaxResults(records).List<Document>();
        }


        public IList<Document> SelectPendingCrossDock(Document document, IList<Product> purchaseProduct)
        {
            //AND l.Unit.BaseAmount = 1 agregada Dec 07/2009 solo permitir para crossdock documento en venta basica.
            StringBuilder sql = new StringBuilder("select a from Document a , DocumentLine l where a.PostingDocument is null and a.DocID=l.Document.DocID AND l.Unit.BaseAmount = 1 ");

            if (document != null)
            {
                Parms = new List<Object[]>();


                if (document.DocType != null && document.DocType.DocTypeID != 0)
                {
                    sql.Append(" and a.DocType.DocTypeID = :id1   ");
                    Parms.Add(new Object[] { "id1", document.DocType.DocTypeID });

                }

                if (document.DocType != null && document.DocType.DocClass != null && document.DocType.DocClass.DocClassID != 0)
                {
                    sql.Append(" and a.DocType.DocClass.DocClassID = :id15  ");
                    Parms.Add(new Object[] { "id15", document.DocType.DocClass.DocClassID });
                }

                if (document.Date1 != null) //Date1 Contiene la fecha de Rerefencia
                {
                    sql.Append(" and ( a.Date2  <= :nom4  Or  a.Date4 <= :nom4 )  ");
                    Parms.Add(new Object[] { "nom4", DateTime.Now });
                }

                sql.Append(" and ( a.DocStatus.StatusID = :id2  Or a.DocStatus.StatusID = :id3 ) ");
                Parms.Add(new Object[] { "id2", DocStatus.New });
                Parms.Add(new Object[] { "id3", DocStatus.InProcess });

                int i = 0;
                sql.Append(" and ( 1=2 ");
                foreach (Product product in purchaseProduct)
                {
                    sql.Append(" Or l.Product.ProductID = :prd"+i.ToString()+" ");
                    Parms.Add(new Object[] { "prd" + i, product.ProductID });
                    i++;
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(")  order by a.Date2 asc ");

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query.List<Document>();
        }


        public IList<Int32> SelectRulesMessage(Document document, MessageRuleByCompany rule)
        {
            string sQuery = "";

                sQuery = "select DocID from Trace.Document a left outer join Report.MessagePool m on a.DocID = m.RecordID "+
                   " and m.EntityID = :idx1 and m.RuleID  = :rule Where  " +
                   "  ( a.CreationDate >= :dtm1 or a.ModDate  >= :dtm1 ) and m.RecordID is null ";


            StringBuilder sql = new StringBuilder(sQuery);

            if (document != null)
            {
                Parms = new List<Object[]>();
                Parms.Add(new Object[] { "dtm1", DateTime.Today.AddDays(-5) }); //Limita a enviar solo los del dia
                Parms.Add(new Object[] { "idx1", EntityID.Document }); 
                Parms.Add(new Object[] { "rule", rule.RowID }); //Rule a ejecutar


                if (document.DocType != null && document.DocType.DocTypeID != 0)
                {
                    sql.Append(" and a.DocTypeID = :id1   ");
                    Parms.Add(new Object[] { "id1", document.DocType.DocTypeID });

                }

                if (document.DocStatus != null && document.DocStatus.StatusID != 0)
                {
                    sql.Append(" and a.DocStatusID = :id3   ");
                    Parms.Add(new Object[] { "id3", document.DocStatus.StatusID });

                }

                //sql.Append(" and ( a.DocStatusID != :id2) ");
                //Parms.Add(new Object[] { "id2", DocStatus.Cancelled });

            }

            sql = new StringBuilder(sql.ToString());

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);
            return query.List<Int32>();

        }



        public bool IsTrackRequiredInDocument(Document document, Node node)
        {

            string sQuery = "";
            Parms = new List<Object[]>();

            if (document.DocType.DocTypeID != SDocType.ReceivingTask)
            {
                sQuery = "SELECT  COUNT(l.LineID) " +
                      "FROM         Trace.DocumentLine l INNER JOIN " +
                      "Master.ProductTrackRelation pt ON l.ProductID = pt.ProductID INNER JOIN " +
                      "Master.TrackOption t ON pt.TrackOptionID = t.RowID " +
                      "WHERE  l.DocID = :id3 AND  (l.LineStatusID = :id1 ) AND (t.DataTypeID <> :id2 )";

                Parms.Add(new Object[] { "id1", DocStatus.New }); //Limita a enviar solo los del dia
            }
            else
            {
                sQuery = "select COUNT(l.ProductID) " +
                "from Trace.NodeTrace n INNER JOIN Trace.Label l ON n.LabelID = l.LabelID " +
                "INNER JOIN Master.ProductTrackRelation pt ON l.ProductID = pt.ProductID " +
                "INNER JOIN Master.TrackOption t ON pt.TrackOptionID = t.RowID " +
                "WHERE   (t.DataTypeID <> :id2 ) AND n.DocID = :id3 "; //AND n.NodeID = :id4

                //Parms.Add(new Object[] { "id4", node.NodeID });
            }



            StringBuilder sql = new StringBuilder(sQuery);

            if (document != null)
            {
                Parms.Add(new Object[] { "id2", SDataTypes.ProductQuality });
                Parms.Add(new Object[] { "id3", document.DocID });
                
            }

            sql = new StringBuilder(sql.ToString());

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            int trackLines = 0;
            try
            {
                trackLines = int.Parse(query.UniqueResult().ToString());
                return trackLines > 0 ? true : false;
            }
            catch { return false; }

        }



        public IList<ShowData> GetDocumentAccount(Document document, short accType, bool pending)
        {
            StringBuilder sql = new StringBuilder("select ");

            if (document != null)
            {
                Parms = new List<Object[]>();


                if (accType == AccntType.Customer)
                    sql.Append(" a.Customer.AccountID, Concat(a.Customer.Name, ' | ', Cast(Count(Distinct a.DocID) as String), ' Orders | since ', Cast(MIN(a.Date2) as String))   from Document a INNER JOIN a.DocumentLines l  Where a.Customer.AccountCode != :def "); //a.PostingDocument is null

                if (accType == AccntType.Vendor)
                    sql.Append(" a.Vendor.AccountID, Concat(a.Vendor.Name, ' | ', Cast(Count(Distinct a.DocID) as String),  ' Documents | since ', Cast(MIN(a.Date1) as String))  from Document a  INNER JOIN a.DocumentLines l where a.Vendor.AccountCode != :def ");

                Parms.Add(new Object[] { "def", WmsSetupValues.DEFAULT });



                if (document.DocType != null && document.DocType.DocTypeID != 0)
                {
                    sql.Append(" and a.DocType.DocTypeID = :id1   ");
                    Parms.Add(new Object[] { "id1", document.DocType.DocTypeID });
                }


                if (document.DocType != null && document.DocType.DocClass != null && document.DocType.DocClass.DocClassID != 0)
                {
                    sql.Append(" and a.DocType.DocClass.DocClassID = :id15  ");
                    Parms.Add(new Object[] { "id15", document.DocType.DocClass.DocClassID });
                }

                if (document.Date1 != null) //Date1 Contiene la fecha de Rerefencia
                {
                    sql.Append(" and ( a.Date2  <= :nom4  Or  a.Date4 <= :nom4 )  ");
                    Parms.Add(new Object[] { "nom4", DateTime.Today });
                }

                if (document.Location != null && document.Location.LocationID != 0)
                {
                    sql.Append(" and  (a.Location.LocationID = :id9 OR a.Location.LocationID Is Null) ");
                    Parms.Add(new Object[] { "id9", document.Location.LocationID });
                }

                if (document.Arrived != null)
                {
                    if (document.Arrived == false)
                        sql.Append(" and a.Date5 is null ");
                    else
                        sql.Append(" and a.Date5 is not null ");
                }


                //sql.Append(" and a.Date1  >= :dtx4 ");
                //Parms.Add(new Object[] { "dtx4", DateTime.Today.AddDays(-1 * daysAgo) });

                if (pending)
                {
                    sql.Append(" and ( a.DocStatus.StatusID = :id2 OR a.DocStatus.StatusID = :idx  ) "); //Or a.DocStatus.StatusID = :id3 Or a.DocStatus.StatusID = :id4 
                    sql.Append(" and (l.LineStatus.StatusID = :id2  OR (l.LineStatus.StatusID = :idx AND (l.QtyBackOrder > 0  ))) "); //OR l.QtyCancel > 0
                    //sql.Append(" and (l.Quantity - l.QtyShipped > 0  ) ");

                    Parms.Add(new Object[] { "id2", DocStatus.New });
                    Parms.Add(new Object[] { "idx", DocStatus.InProcess });
                    //Parms.Add(new Object[] { "id4", DocStatus.Completed });
                }

            }

            if (accType == AccntType.Customer)
                sql.Append(" GROUP BY a.Customer.AccountID, a.Customer.Name "); //a.PostingDocument is null

            if (accType == AccntType.Vendor)
                sql.Append(" GROUP BY  a.Vendor.AccountID, a.Vendor.Name ");


            sql.Append(" ORDER BY a.Customer.Name ");
            


            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);

            IList<Object[]> result = query.List<Object[]>();

            if (result == null || result.Count == 0)
                return new List<ShowData>();
            else
            {
                IList<ShowData> retList = new List<ShowData>();

                foreach (object[] obj in result) {
                    retList.Add(new ShowData { DataKey = obj[0].ToString(), 
                        DataValue = obj[1].ToString().Substring(0,obj[1].ToString().Length - 8) });
                }

                return retList;
            }


        }



        /// <summary>
        /// Entrega lo que esta pendiente por recibir de un Documento contra un NodeTrace En unidad Basica
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IList<ProductStock> GetDocumentProductStock(Document document, Product product)
        {
            string sQuery = "";
            Parms = new List<Object[]>();

            string strDoc = "";

            if (document.DocID != 0)
            {
                strDoc = "AND d.DocID = :id0";
                Parms.Add(new Object[] { "id0", document.DocID });
            }

            sQuery = " SELECT     stk.ProductID, p.BaseUnitID, 0, SUM(ISNULL(stk.SingleQuantity,0) + ISNULL(stk.PackQuantity,0)) * u.BaseAmount AS Quantity, 0, null, null " +
                     " FROM  vwStockSummary AS stk INNER JOIN Master.Product p  " +
                     " ON ISNULL(stk.LevelCode,'') <> 'NOUSE' AND p.ProductID = stk.ProductID INNER JOIN Master.Unit u ON " +
                     " u.UnitID = stk.UnitID AND p.ProductID IN (select distinct ProductID From Trace.DocumentLine l " +
                     " INNER JOIN Trace.Document d ON d.DociD = l.DocID AND d.DocTypeID = :idt1 __STRCUST " +
                     " WHERE (d.DocStatusID = :ds1   OR  d.DocStatusID = :dsp1) AND ( LineStatusID = :id3 OR LineStatusID = :is3 ) " + strDoc + " ) " +
                     " WHERE (stk.StatusID = :id1) AND (stk.LocationID = :id2)   AND (stk.NodeID = :id4)  ";

            StringBuilder sql = new StringBuilder(sQuery);


            Parms.Add(new Object[] { "id1", EntityStatus.Active });
            Parms.Add(new Object[] { "id2", document.Location.LocationID });
            Parms.Add(new Object[] { "id3", DocStatus.New });
            Parms.Add(new Object[] { "is3", DocStatus.InProcess });
            Parms.Add(new Object[] { "id4", NodeType.Stored });

            if (document.Customer != null && document.Customer.AccountID != 0)
            {
                sql = sql.Replace("__STRCUST", " AND d.CustomerID = :idc0 ");
                Parms.Add(new Object[] { "idc0", document.Customer.AccountID });
            }
            else
                sql = sql.Replace("__STRCUST", "");
            

            if (product != null && product.ProductID != 0)
            {
                sql.Append(" AND p.ProductID = :p0 ");
                Parms.Add(new Object[] { "p0", product.ProductID });
            }

            Parms.Add(new Object[] { "idt1", document.DocType.DocTypeID });
            Parms.Add(new Object[] { "ds1", DocStatus.New });
            Parms.Add(new Object[] { "dsp1", DocStatus.InProcess });



            sql.Append("   GROUP BY stk.LocationID, stk.ProductID, u.BaseAmount, p.BaseUnitID");

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }


        public int GetNexDocLineNumber(Document document)
        {
            string sQuery = "";

            sQuery = "select Isnull(Max(LineNumber),0) from Trace.DocumentLine l Where l.DocID = :doc ";


            StringBuilder sql = new StringBuilder(sQuery);

            if (document != null)
            {
                Parms = new List<Object[]>();
                Parms.Add(new Object[] { "doc", document.DocID }); //Limita a enviar solo los del dia
            }

            sql = new StringBuilder(sql.ToString());

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);
            return query.UniqueResult<Int32>() + 1;
        }
    }
}