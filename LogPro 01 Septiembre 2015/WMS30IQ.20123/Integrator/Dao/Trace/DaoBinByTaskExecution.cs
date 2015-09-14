using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using System.Linq;

namespace Integrator.Dao.Trace
{
    public class DaoBinByTaskExecution : DaoService
    {
        public DaoBinByTaskExecution(DaoFactory factory) : base(factory) { }

        public BinByTaskExecution Save(BinByTaskExecution data)
        {
            return (BinByTaskExecution)base.Save(data);
        }


        public Boolean Update(BinByTaskExecution data)
        {
            return base.Update(data);
        }


        public Boolean Delete(BinByTaskExecution data)
        {
            return base.Delete(data);
        }


        public BinByTaskExecution SelectById(BinByTaskExecution data)
        {
            return (BinByTaskExecution)base.SelectById(data);
        }


        public IList<BinByTaskExecution> Select(BinByTaskExecution data)
        {
            IList<BinByTaskExecution> datos = new List<BinByTaskExecution>();

            try {
            datos = GetHsql(data).List<BinByTaskExecution>();
            if (!Factory.IsTransactional)
                Factory.Commit();

                            }
                catch (Exception e)
                {
                    NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
                }

            return datos;
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from BinByTaskExecution a  where  ");
            BinByTaskExecution binTask = (BinByTaskExecution)data;
            if (binTask != null)
            {
                Parms = new List<Object[]>();
                if (binTask.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", binTask.RowID });
                }


                if (binTask.Status != null && binTask.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :idd5     and   ");
                    Parms.Add(new Object[] { "idd5", binTask.Status.StatusID });
                }

                if (binTask.Sequence > 0)
                {
                    sql.Append(" a.Sequence = :idd6   and   ");
                    Parms.Add(new Object[] { "idd6", binTask.Sequence });
                }


                if (!string.IsNullOrEmpty(binTask.CountSession))
                {
                    sql.Append(" a.CountSession = :idcs   and   ");
                    Parms.Add(new Object[] { "idcs", binTask.CountSession });
                }


                if (binTask.Product != null && binTask.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :ip5     and   ");
                    Parms.Add(new Object[] { "ip5", binTask.Product.ProductID });
                }


                //Adicionado en FEB 25 /2010
                if (binTask.Bin != null && binTask.Bin.BinID != 0)
                {
                    sql.Append(" a.Bin.BinID = :ib5     and   ");
                    Parms.Add(new Object[] { "ib5", binTask.Bin.BinID });
                }


                if (binTask.UnitCount != null && binTask.UnitCount.UnitID != 0)
                {
                    sql.Append(" a.UnitCount.UnitID = :ibu5     and   ");
                    Parms.Add(new Object[] { "ibu5", binTask.UnitCount.UnitID });
                }


                if (binTask.StockLabel != null && binTask.StockLabel.LabelID != 0)
                {
                    if (binTask.StockLabel.LabelID > 0)
                    {
                        sql.Append(" a.StockLabel.LabelID = :lb5     and   ");
                        Parms.Add(new Object[] { "lb5", binTask.StockLabel.LabelID });
                    }
                    else 
                        sql.Append(" a.StockLabel.LabelID IS NULL    and   ");
                }


                if (binTask.BinTask != null )
                {
                    if (binTask.BinTask.RowID != 0)
                    {
                        sql.Append(" a.BinTask.RowID = :idd1  and  ");
                        Parms.Add(new Object[] { "idd1", binTask.BinTask.RowID });
                    }


                    if (binTask.BinTask.Product != null && binTask.BinTask.Product.ProductID != 0)
                    {
                        sql.Append(" a.BinTask.Product.ProductID = :ip4  and  ");
                        Parms.Add(new Object[] { "ip4", binTask.BinTask.Product.ProductID });
                    }


                    if (binTask.BinTask.Bin != null && binTask.BinTask.Bin.BinID != 0)
                    {
                        sql.Append(" a.BinTask.Bin.BinID = :idd4  and  ");
                        Parms.Add(new Object[] { "idd4", binTask.BinTask.Bin.BinID });
                    }

                    if (binTask.BinTask.Bin != null && !String.IsNullOrEmpty(binTask.BinTask.Bin.BinCode))
                    {
                        sql.Append(" a.BinTask.Bin.BinCode = :idd2  and  ");
                        Parms.Add(new Object[] { "idd2", binTask.BinTask.Bin.BinCode });
                    }

                    if (binTask.BinTask.TaskDocument != null && binTask.BinTask.TaskDocument.DocID != 0)
                    {
                        sql.Append(" a.BinTask.TaskDocument.DocID = :idd3     and   ");
                        Parms.Add(new Object[] { "idd3", binTask.BinTask.TaskDocument.DocID });
                    }

                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.BinTask.Bin.BinCode, a.Sequence Desc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


        /*
        public IList<ProductStock> GetCountSummary(Document data)
        {
            //Stock = Counted, PackStock = Expected


            string initQuery = "SELECT     Trace.BinByTaskExecution.ProductID, Master.Product.BaseUnitID, Trace.BinByTaskExecution.BinID, SUM(Trace.BinByTaskExecution.QtyCount*u.BaseAmount) AS Counted, " +
                        "ISNULL(summ.SingleQuantity, 0) + ISNULL(summ.PackQuantity, 0) AS QtyExpected, Max(Trace.BinByTaskExecution.CreationDate) as MaxDate,  Min(Trace.BinByTaskExecution.CreationDate) as MinDate " +
                        "FROM         Trace.BinByTaskExecution INNER JOIN " +
                        "                      Trace.BinByTask ON Trace.BinByTaskExecution.BinByTaskID = Trace.BinByTask.RowID INNER JOIN " +
                        "                      Master.Unit u ON Trace.BinByTaskExecution.UnitCountID = u.UnitID INNER JOIN " +
                        "                      Master.Product ON Trace.BinByTaskExecution.ProductID = Master.Product.ProductID LEFT OUTER JOIN " +
                        "                      vwStockSummary AS summ ON Master.Product.ProductID = summ.ProductID AND Trace.BinByTaskExecution.BinID = summ.BinID AND (summ.NodeID = :nid) AND (summ.StatusID = :six) " +
                        "WHERE     (Trace.BinByTaskExecution.StatusID = :sid2)  AND Trace.BinByTask.TaskDocumentID = :doc " +
                        "GROUP BY Trace.BinByTaskExecution.ProductID, Master.Product.BaseUnitID, Trace.BinByTaskExecution.BinID, summ.SingleQuantity, summ.PackQuantity ";
                //"ORDER BY Trace.BinByTask.BinID ";



            initQuery += "UNION SELECT     ISNULL(Trace.BinByTaskExecution.ProductID,summ.ProductID) as ProductID, Master.Product.BaseUnitID, summ.BinID, SUM(ISNULL(Trace.BinByTaskExecution.QtyCount,0)*u.BaseAmount) AS Counted, " +
                        "ISNULL(summ.SingleQuantity, 0) + ISNULL(summ.PackQuantity, 0) AS QtyExpected, Max(Trace.BinByTaskExecution.CreationDate) as MaxDate,  Min(Trace.BinByTaskExecution.CreationDate) as MinDate " +

                        "FROM     vwStockSummary AS summ " +
                        "INNER JOIN Master.Product ON summ.ProductID = Master.Product.ProductID " +
                        "LEFT OUTER JOIN Trace.BinByTask ON Trace.BinByTask.BinID = summ.BinID " +
                        "LEFT OUTER JOIN  Trace.BinByTaskExecution ON Trace.BinByTaskExecution.StatusID = :sid2 AND Trace.BinByTaskExecution.ProductID = summ.ProductID AND Trace.BinByTask.RowID = Trace.BinByTaskExecution.BinByTaskID " +
                        "LEFT OUTER JOIN  Master.Unit u ON Trace.BinByTaskExecution.UnitCountID = u.UnitID " +

                        "WHERE summ.NodeID = :nid AND (summ.StatusID = :six) AND Trace.BinByTask.TaskDocumentID = :doc " +
                        "AND Trace.BinByTask.BinID IN (SELECT DISTINCT BinID FROM Trace.BinByTaskExecution "+
                                "   WHERE Trace.BinByTaskExecution.BinByTaskID IN  " +
                                "   (SELECT RowID FROM Trace.BinByTask WHERE TaskDocumentID = :doc) "+
                                " AND Trace.BinByTaskExecution.StatusID = :sid2) "+
                        "GROUP BY Trace.BinByTaskExecution.ProductID, Master.Product.BaseUnitID, Trace.BinByTask.BinID, summ.SingleQuantity, summ.PackQuantity, summ.ProductID, summ.BinID " +
                        "HAVING ISNULL(summ.SingleQuantity, 0) + ISNULL(summ.PackQuantity, 0) > 0 " + //AND SUM(ISNULL(Trace.BinByTaskExecution.QtyCount,0)) > 0 " +
                        "ORDER BY 3 ";


            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "nid", NodeType.Stored });
            Parms.Add(new Object[] { "six", EntityStatus.Active });
            Parms.Add(new Object[] { "doc", data.DocID });
            //Parms.Add(new Object[] { "sid2", DocStatus.Completed });
            Parms.Add(new Object[] { "sid2", data.DocStatus.StatusID });


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }*/


        public IList<CountTaskBalance> GetCountSummary(Document data, bool summary)
        {
            string initQuery = "";

            /*
            if (summary)
            {
                return null; 
                                
            }
            else
            {
             */
                if (data.Notes == "0")
                    initQuery = "SELECT  TaskDocumentID, RowID, BinID, LabelID, ProductID, QtyExpected, UnitExpID, QtyCount, UnitCountID, CaseType " +
                                      "FROM         vwCountBalanceByBin WHERE     (TaskDocumentID = :doc) AND (StatusID = :sid2)";
                else
                    initQuery = "SELECT  TaskDocumentID, RowID, BinID, LabelID, ProductID, QtyExpected, UnitExpID, QtyCount, UnitCountID, CaseType " +
                                      "FROM         vwCountBalanceByBinProduct WHERE     (TaskDocumentID = :doc) AND (StatusID = :sid2)";

            // }


            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            //Parms.Add(new Object[] { "nid", NodeType.Stored });
            //Parms.Add(new Object[] { "six", EntityStatus.Active });
            Parms.Add(new Object[] { "doc", data.DocID });

            if (!summary)
                Parms.Add(new Object[] { "sid2", data.DocStatus.StatusID });


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<CountTaskBalance> ret = GetCountBalanceObject(query.List<Object[]>());

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }


        private IList<CountTaskBalance> GetCountBalanceObject(IList<Object[]> retList)
        {
            if (retList == null || retList.Count == 0)
                return new List<CountTaskBalance>();

            IList<CountTaskBalance> retBalance = new List<CountTaskBalance>();
            CountTaskBalance curBalance;

            Document document = Factory.DaoDocument().SelectById(new Document { DocID = (int)retList[0][0] });

            foreach (Object[] obj in retList)
            {
               
                curBalance = new CountTaskBalance();
                //TaskDocumentID, BinID, LabelID, ProductID, QtyExpected, UnitExpID, QtyCount, UnitCountID
               
                curBalance.CountTask = document;

                curBalance.BinByTask = Factory.DaoBinByTask().Select(new BinByTask { RowID = (int)obj[1] }).First();

                curBalance.Bin = Factory.DaoBin().Select(new Bin { BinID = (int)obj[2] }).First();

                if ((long)obj[3] > 0)
                    curBalance.Label = Factory.DaoLabel().Select(new Label { LabelID = (long)obj[3] }).First();
                
                curBalance.Product = Factory.DaoProduct().SelectById(new Product { ProductID = (int)obj[4] });
                
                curBalance.QtyExpected = Double.Parse(obj[5].ToString());

                try { curBalance.UnitExpected = Factory.DaoUnit().SelectById(new Unit { UnitID = (int)obj[6] }); }
                catch { }
                
                curBalance.QtyCount = Double.Parse(obj[7].ToString());

                try { curBalance.UnitCount = Factory.DaoUnit().SelectById(new Unit { UnitID = (int)obj[8] }); }
                catch { }

                curBalance.CaseType = int.Parse(obj[9].ToString());

                retBalance.Add(curBalance);
            }

            return retBalance;
        }


        public IList<ProductStock> GetNoCountSummary(Location location)
        {
            //Stock = Counted, PackStock = Expected


            string initQuery = "SELECT     ProductID, UnitID, BinID, SUM(SingleQuantity + PackQuantity) AS Counted, 0 AS QtyExpected, MAX(MaxDate) AS MaxDate, MIN(MinDate) "
                      +"AS MinDate FROM         vwStockSummary WHERE (BinCode = :bin) AND (LocationID = :loc) "
                + "GROUP BY ProductID, UnitID, BinID HAVING SUM(SingleQuantity + PackQuantity) > 0 ";
  

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            //Parms.Add(new Object[] { "nid", NodeType.Stored });
            //Parms.Add(new Object[] { "six", EntityStatus.Active });
            Parms.Add(new Object[] { "loc", location.LocationID });
            Parms.Add(new Object[] { "bin", "NOCOUNT" });


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }
    }
}