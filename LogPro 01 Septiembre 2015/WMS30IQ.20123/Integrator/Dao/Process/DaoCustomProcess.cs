using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;
using Entities.Process;
using Entities.Trace;
using Entities.General;
using Integrator.Dao.Trace;

namespace Integrator.Dao.Process
{
    public class DaoCustomProcess : DaoService
    {
        public DaoCustomProcess(DaoFactory factory) : base(factory) { }

        public CustomProcess Save(CustomProcess data)
        {
            return (CustomProcess)base.Save(data);
        }


        public Boolean Update(CustomProcess data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcess data)
        {
            return base.Delete(data);
        }


        public CustomProcess SelectById(CustomProcess data)
        {
            return (CustomProcess)base.SelectById(data);
        }


        public IList<CustomProcess> Select(CustomProcess data)
        {

                IList<CustomProcess> datos = new List<CustomProcess>();

                datos = GetHsql(data).List<CustomProcess>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcess a    where  ");
            CustomProcess CustomProcess = (CustomProcess)data;
            if (CustomProcess != null)
            {
                Parms = new List<Object[]>();
                if (CustomProcess.ProcessID != 0)
                {
                    sql.Append(" a.ProcessID = :id     and   ");
                    Parms.Add(new Object[] { "id", CustomProcess.ProcessID });
                }

                if (CustomProcess.ProcessType != null && CustomProcess.ProcessType.DocTypeID != 0)
                {
                    sql.Append(" a.ProcessType.DocTypeID = :ix2     and   ");
                    Parms.Add(new Object[] { "ix2", CustomProcess.ProcessType.DocTypeID });
                }


                if (!String.IsNullOrEmpty(CustomProcess.Name))
                {
                    sql.Append(" a.Name = :nomCode     and   ");
                    Parms.Add(new Object[] { "nomCode", CustomProcess.Name  });
                }


                if (CustomProcess.Status != null && CustomProcess.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", CustomProcess.Status.StatusID });
                }


                if (CustomProcess.IsSystem != null)
                {
                    sql.Append(" a.IsSystem = :id11     and   ");
                    Parms.Add(new Object[] { "id11", CustomProcess.IsSystem });
                }


            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }




        public IList<int> GetInspectionVendors(Entities.Trace.Document document)
        {
            string sQuery = "SELECT     MIN(Master.Account.AccountID) " +
                        "FROM         Master.Account RIGHT OUTER JOIN Trace.Label INNER JOIN " +
                      "Master.Product ON Trace.Label.ProductID = Master.Product.ProductID LEFT OUTER JOIN " +
                      "Master.ProductAccountRelation ON Master.ProductAccountRelation.AccTypeID = 2 " +
                          "AND Master.Product.ProductID = Master.ProductAccountRelation.ProductID ON " +
                      "Master.Account.AccountID = Master.ProductAccountRelation.AccountID " +
                    "WHERE     (Trace.Label.Notes = 'REPAIR') AND Master.Account.AccountCode NOT LIKE 'W-%' AND Master.Account.CompanyID = :co1 AND Trace.Label.PrintingLot = :id1 " +
                    "GROUP BY Label.ProductID  ";


            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", document.DocNumber });
            Parms.Add(new Object[] { "co1", document.Company.CompanyID });


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            return query.List<int>();
        }



        public IList<ProductStock> GetInspectionVendorStock(Document document, Account account)
        {

            //string sQuery = "SELECT     l.ProductID, l.UnitID, l.BinID, SUM(l.CurrQty) AS Stock, 0 as PackStock " +
            //                "FROM         Master.Account RIGHT OUTER JOIN Trace.Label l INNER JOIN "+
            //                "                      Master.Product ON l.ProductID = Master.Product.ProductID LEFT OUTER JOIN "+
            //                "                      Master.ProductAccountRelation ON Master.ProductAccountRelation.AccTypeID = 2 AND Master.Product.ProductID = Master.ProductAccountRelation.ProductID ON "+
            //                "                      Master.Account.AccountID = Master.ProductAccountRelation.AccountID "+
            //                "WHERE     (l.Notes = 'REPAIR')   AND Master.Account.CompanyID = :co1 AND l.PrintingLot = :id1 AND Master.Account.AccountId = :vd1  AND l.NodeID = :nd1 " +
            //                "GROUP BY l.ProductID, l.UnitID, l.BinID ";


            string sQuery = "SELECT     l.ProductID, l.UnitID, l.BinID, SUM(l.CurrQty) AS Stock, 0 as PackStock " +
                "FROM      Trace.Label l INNER JOIN " +
                "                      Master.Product ON l.ProductID = Master.Product.ProductID "+
                "WHERE     (l.Notes = 'REPAIR')  AND l.PrintingLot = :id1 AND l.NodeID = :nd1 AND Master.Product.CompanyID = :co1 " +
                "GROUP BY l.ProductID, l.UnitID, l.BinID ";

            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", document.DocNumber });
            Parms.Add(new Object[] { "co1", document.Company.CompanyID });
            //Parms.Add(new Object[] { "vd1", document.Vendor.AccountID });
            Parms.Add(new Object[] { "nd1", NodeType.Process });


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            return ret;
        }
    }
}