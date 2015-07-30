using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.General;

namespace Integrator.Dao.Trace
{
    public class DaoBinByTask : DaoService
    {
        public DaoBinByTask(DaoFactory factory) : base(factory) { }

        public BinByTask Save(BinByTask data)
        {
            return (BinByTask)base.Save(data);
        }


        public Boolean Update(BinByTask data)
        {
            return base.Update(data);
        }


        public Boolean Delete(BinByTask data)
        {
            return base.Delete(data);
        }


        public BinByTask SelectById(BinByTask data)
        {
            return (BinByTask)base.SelectById(data);
        }


        public IList<BinByTask> Select(BinByTask data)
        {
            IList<BinByTask> datos = new List<BinByTask>();

            try {
            datos = GetHsql(data).List<BinByTask>();
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
            StringBuilder sql = new StringBuilder("select a from BinByTask a    where  ");
            BinByTask binTask = (BinByTask)data;
            if (binTask != null)
            {
                Parms = new List<Object[]>();
                if (binTask.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", binTask.RowID });
                }


                if (binTask.Bin != null && binTask.Bin.BinID != 0)
                {
                    sql.Append(" a.Bin.BinID = :idd1  and  ");
                    Parms.Add(new Object[] { "idd1", binTask.Bin.BinID });
                }


                if (binTask.Status != null && binTask.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :ids5     and   ");
                    Parms.Add(new Object[] { "ids5", binTask.Status.StatusID });
                }

                if (binTask.Bin != null && !String.IsNullOrEmpty(binTask.Bin.BinCode))
                {
                    sql.Append(" a.Bin.BinCode = :idd2  and  ");
                    Parms.Add(new Object[] { "idd2", binTask.Bin.BinCode });
                }

                if (binTask.TaskDocument != null && binTask.TaskDocument.DocID != 0)
                {
                    sql.Append(" a.TaskDocument.DocID = :idd3     and   ");
                    Parms.Add(new Object[] { "idd3", binTask.TaskDocument.DocID });
                }

                if (binTask.Product != null && binTask.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :idd4     and   ");
                    Parms.Add(new Object[] { "idd4", binTask.Product.ProductID });
                }

            }

            sql = new StringBuilder(sql.ToString());

            sql.Append(" 1=1 order by a.Bin.BinCode"); //.BinCode asc


            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }




        public IList<ProductStock> GetCountInitialSummary(Document data)
        {
           

            StringBuilder sql = new StringBuilder("");
            Parms = new List<Object[]>();


             string initQuery = "";

            if (data.Notes == "0") //Conteo por BIN
                initQuery = "SELECT     ProductID, NULL, Trace.BinByTask.BinID, 0, 0, NULL, NULL " +
                               "FROM         Trace.BinByTask INNER JOIN Master.Bin b ON b.BinID = Trace.BinByTask.BinID " +
                               "WHERE    Trace.BinByTask.TaskDocumentID = :doc ORDER BY b.Rank";

            else if (data.Notes == "1") //Conteo por Producto
            {
                initQuery = "SELECT     Trace.BinByTask.ProductID, NULL, stk.BinID, 0, 0, NULL, NULL " +
                            "FROM         Trace.BinByTask INNER JOIN " +
                            "vwStockSummary AS stk ON Trace.BinByTask.ProductID = stk.ProductID " +
                            "WHERE    Trace.BinByTask.TaskDocumentID = :doc AND (stk.LocationID = :loc) "+
                            "AND (stk.StatusID = :six) AND (stk.NodeID = :nid)  AND (stk.SingleQuantity > 0 Or stk.PackQuantity > 0) ORDER BY stk.BinID  ";

                Parms.Add(new Object[] { "nid", NodeType.Stored });
                Parms.Add(new Object[] { "six", EntityStatus.Active });
                Parms.Add(new Object[] { "loc", data.Location.LocationID });
            }

            sql.Append(initQuery);


        

 

            Parms.Add(new Object[] { "doc", data.DocID });

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }





    }


    
       


}