using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.Master;
using Entities.General;


namespace Integrator.Dao.Trace
{
    public class DaoDocumentLine : DaoService
    {
        public DaoDocumentLine(DaoFactory factory) : base(factory) { }

        public DocumentLine Save(DocumentLine data)
        {
            return (DocumentLine)base.Save(data);
        }


        public Boolean Update(DocumentLine data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentLine data)
        {
            return base.Delete(data);
        }


        public DocumentLine SelectById(DocumentLine data)
        {
            return (DocumentLine)base.SelectById(data);
        }


        public IList<DocumentLine> Select(DocumentLine data)
        {

                IList<DocumentLine> datos = new List<DocumentLine>();

                try { datos = GetHsql(data).List<DocumentLine>();
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
            StringBuilder sql = new StringBuilder("select a from DocumentLine a  ");
            DocumentLine documentline = (DocumentLine)data;
            if (documentline != null)
            {
                Parms = new List<Object[]>();


                if (documentline.Product != null && documentline.Product.ProductAccounts != null && documentline.Product.ProductAccounts.Count > 0)
                {
                    sql.Append(" LEFT OUTER JOIN a.Product.ProductAccounts as prodAccount WHERE prodAccount.RowID is not null "); //where a.Product.ProductID =  prodAccount.Product.ProductID ");

                    ProductAccountRelation par = documentline.Product.ProductAccounts[0];

                    if (par.Account != null && par.Account.AccountCode != WmsSetupValues.DEFAULT)
                    {
                        if (par.AccountType.AccountTypeID == AccntType.Customer)
                            sql.Append(" AND prodAccount.Account.AccountID = a.Document.Customer.AccountID ");
                        else if (par.AccountType.AccountTypeID == AccntType.Vendor)
                            sql.Append(" AND prodAccount.Account.AccountID = a.Document.Vendor.AccountID ");
                    }

                    sql.Append(" AND  1=1 ");

                    //Solo se condiciona si pasa un account.
                    if (par.Account != null && par.Account.AccountCode != WmsSetupValues.DEFAULT)
                    {
                        sql.Append(" AND prodAccount.AccountType.AccountTypeID = :act");
                        Parms.Add(new Object[] { "act", par.AccountType.AccountTypeID });

                        sql.Append(" AND prodAccount.Account.AccountID  = :acc");
                        Parms.Add(new Object[] { "acc", par.Account.AccountID });
                    }



                    sql.Append(" AND ((prodAccount.ItemNumber = :itm" + " OR prodAccount.Code1 = :itm" + " OR prodAccount.Code2 = :itm" + ") ");
                    Parms.Add(new Object[] { "itm", par.ItemNumber });

                    


                    if (!String.IsNullOrEmpty(documentline.Product.ProductCode))
                    {
                        sql.Append(" OR (a.Product.ProductCode = :nom  OR a.Product.UpcCode = :nom )) and ");
                        Parms.Add(new Object[] { "nom", documentline.Product.ProductCode });
                    }

                    else
                        sql.Append(" ) and   ");

                }
                else
                {
                    sql.Append(" where ");

                    if (documentline.Product != null && !String.IsNullOrEmpty(documentline.Product.ProductCode))
                    {
                        sql.Append(" (a.Product.ProductCode = :pc4  OR a.Product.UpcCode = :pc4 )  and   ");
                        Parms.Add(new Object[] { "pc4", documentline.Product.ProductCode });
                    }


                }


                if (documentline.Product != null)
                {
                    if (documentline.Product.ProductID != 0)
                    {
                        sql.Append(" a.Product.ProductID = :id4     and   ");
                        Parms.Add(new Object[] { "id4", documentline.Product.ProductID });
                    }

                    //if (!string.IsNullOrEmpty(documentline.Product.ProductCode))
                    //{
                    //    sql.Append(" a.Product.ProductCode = :pc4     and   ");
                    //    Parms.Add(new Object[] { "pc4", documentline.Product.ProductCode });
                    //}

                }





                if (documentline.LineID != 0)
                {
                    sql.Append(" a.LineID = :id     and   ");
                    Parms.Add(new Object[] { "id", documentline.LineID });
                }


                if (documentline.Document != null)
                {

                    if (documentline.Document.DocID != 0)
                    {
                        sql.Append(" a.Document.DocID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", documentline.Document.DocID });
                    }

                    if (!string.IsNullOrEmpty(documentline.Document.DocNumber))
                    {
                        sql.Append(" a.Document.DocNumber = :idn     and   ");
                        Parms.Add(new Object[] { "idn", documentline.Document.DocNumber });
                    }

                    if (!string.IsNullOrEmpty(documentline.Document.Notes))
                    {
                        sql.Append(" a.Document.Notes = :in1     and   ");
                        Parms.Add(new Object[] { "in1", documentline.Document.Notes });
                    }

                    if (documentline.Document.DocType != null && documentline.Document.DocType.DocTypeID != 0)
                    {
                        sql.Append(" a.Document.DocType.DocTypeID = :dt1     and   ");
                        Parms.Add(new Object[] { "dt1", documentline.Document.DocType.DocTypeID });
                    }

                    if (documentline.Document.DocStatus != null && documentline.Document.DocStatus.StatusID != 0)
                    {

                        if (documentline.Document.DocStatus.StatusID == DocStatus.PENDING)
                        {
                            sql.Append(" (a.Document.DocStatus.StatusID = :ds1   OR  a.Document.DocStatus.StatusID = :dsp1) and   ");
                            Parms.Add(new Object[] { "ds1", DocStatus.New });
                            Parms.Add(new Object[] { "dsp1", DocStatus.InProcess });
                        }
                        else
                        {
                            sql.Append(" a.Document.DocStatus.StatusID = :ds1     and   ");
                            Parms.Add(new Object[] { "ds1", documentline.Document.DocStatus.StatusID });
                        }

                    }

                    if (documentline.Document.Company != null && documentline.Document.Company.CompanyID != 0)
                    {
                        sql.Append(" a.Document.Company.CompanyID = :dco1     and   ");
                        Parms.Add(new Object[] { "dco1", documentline.Document.Company.CompanyID });
                    }

                    if (!string.IsNullOrEmpty(documentline.Document.CustPONumber))
                    {
                        sql.Append(" a.Document.CustPONumber = :custpo     and   ");
                        Parms.Add(new Object[] { "custpo", documentline.Document.CustPONumber });
                    }

                    if (documentline.Document.Customer != null && documentline.Document.Customer.AccountID != 0)
                    {
                        sql.Append(" a.Document.Customer.AccountID = :cu1     and   ");
                        Parms.Add(new Object[] { "cu1", documentline.Document.Customer.AccountID });
                    }

                    if (documentline.Document.Vendor != null && documentline.Document.Vendor.AccountID != 0)
                    {
                        sql.Append(" a.Document.Vendor.AccountID = :ve1     and   ");
                        Parms.Add(new Object[] { "ve1", documentline.Document.Vendor.AccountID });
                    }

                    if (documentline.Document.Location != null && documentline.Document.Location.LocationID != 0)
                    {
                        sql.Append(" a.Document.Location.LocationID = :lxc1     and   ");
                        Parms.Add(new Object[] { "lxc1", documentline.Document.Location.LocationID });
                    }

                }

                if (documentline.LineNumber != 0)
                {
                    sql.Append(" a.LineNumber = :id2     and   ");
                    Parms.Add(new Object[] { "id2", documentline.LineNumber });
                }


                if (documentline.LinkDocLineNumber != 0)
                {
                    sql.Append(" a.LinkDocLineNumber = :idl2     and   ");
                    Parms.Add(new Object[] { "idl2", documentline.LinkDocLineNumber });
                }


                if (!string.IsNullOrEmpty(documentline.LinkDocNumber))
                {
                    sql.Append(" a.LinkDocNumber = :idn     and   ");
                    Parms.Add(new Object[] { "idn", documentline.LinkDocNumber });
                }


                if (documentline.LineStatus != null && documentline.LineStatus.StatusID != 0)
                {
                    if (documentline.LineStatus.StatusID == DocStatus.PENDING)
                    {
                        sql.Append(" (a.LineStatus.StatusID = :ds1   OR  a.LineStatus.StatusID = :dsp1) and   ");
                        Parms.Add(new Object[] { "ds1", DocStatus.New });
                        Parms.Add(new Object[] { "dsp1", DocStatus.InProcess });
                    }
                    else
                    {
                        sql.Append(" a.LineStatus.StatusID = :id3     and   ");
                        Parms.Add(new Object[] { "id3", documentline.LineStatus.StatusID });
                    }

                }


                       





                if (documentline.IsDebit != null)
                {
                    sql.Append(" a.IsDebit = :nom1  and   ");
                    Parms.Add(new Object[] { "nom1", documentline.IsDebit });
                }


                if (documentline.Unit != null && documentline.Unit.UnitID != 0)
                {
                    sql.Append(" a.Unit.UnitID = :id5     and   ");
                    Parms.Add(new Object[] { "id5", documentline.Unit.UnitID });
                }

                if (documentline.Date1 != null)
                {
                    sql.Append(" a.Date1 = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", documentline.Date1 + "%" });
                }

                if (documentline.Date2 != null)
                {
                    sql.Append(" a.Date2 = :nom4     and   ");
                    Parms.Add(new Object[] { "nom4", documentline.Date2 + "%" });
                }

                if (documentline.Date3 != null)
                {
                    sql.Append(" a.Date3 = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", documentline.Date3 + "%" });
                }

                if (documentline.Date4 != null)
                {
                    sql.Append(" a.Date4 = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", documentline.Date4 + "%" });
                }

                if (documentline.Date5 != null)
                {
                    sql.Append(" a.Date5 = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", documentline.Date5 + "%" });
                }



                if (documentline.Location != null && documentline.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id6     and   ");
                    Parms.Add(new Object[] { "id6", documentline.Location.LocationID });
                }


                if (documentline.Location2 != null && documentline.Location2.LocationID != 0)
                {
                    sql.Append(" a.Location2.LocationID = :idl6     and   ");
                    Parms.Add(new Object[] { "idl6", documentline.Location2.LocationID });
                }


                if (!string.IsNullOrEmpty(documentline.Note))
                {
                    sql.Append(" a.Note = :nom11     and   ");
                    Parms.Add(new Object[] { "nom11", documentline.Note });
                }

                if (!string.IsNullOrEmpty(documentline.CreatedBy))
                {
                    sql.Append(" a.CreatedBy = :nom81     and   ");
                    Parms.Add(new Object[] { "nom81", documentline.CreatedBy });
                }


            }
            
            //Console.WriteLine(sql.ToString());

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.Sequence ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public IList<ProductStock> GetProductInUseForMerged(List<int> list, Location location)
        {
            //Obtiene la lineas que tengan cantidad QtyAllocated - Shipped > 0
            //Y que esten en status 101,102

            //Sale solo product, y del nodo stored
            string initQuery = " SELECT     l.ProductID, p.BaseUnitID, null, SUM(ISNULL(l.QtyAllocated, 0) - ISNULL(trf.QtyShipped, 0)) AS Stock, 0 AS PackStock, NULL, NULL " +
                " FROM         Trace.DocumentLine AS l INNER JOIN Master.Product AS p ON l.ProductID = p.ProductID INNER JOIN  " +
                " Trace.[Document] AS d ON l.DocID = d.DocID  AND d.LocationID = :loc1 LEFT OUTER JOIN " +
                " (SELECT     l.LineNumber, x.DocNumber, l.ProductID, SUM(ISNULL(l.QtyShipped, 0)) AS QtyShipped  " +
                " FROM          Trace.DocumentLine AS l INNER JOIN  " +
                " Trace.[Document] AS x ON x.DocID = l.DocID " +
                " WHERE  x.DocTypeiD in (201) " +
                " GROUP BY l.LineNumber, x.DocNumber, l.ProductID  " +
                " HAVING      (SUM(l.QtyShipped) > 0)) AS trf ON l.LinkDocLineNumber = trf.LineNumber AND l.LinkDocNumber = trf.DocNumber  AND l.ProductID = trf.ProductID " +
                " WHERE     (l.LineStatusID IN (101, 102)) AND (d.DocTypeID IN (206)) ";





            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "loc1", location.LocationID }); //Location

            if (list != null && list.Count > 0)
            {
                sql.Append(" AND l.ProductID IN (0 ");

                foreach (int pID in list)
                {
                    sql.Append(", " + ":p" + pID.ToString());
                    Parms.Add(new Object[] { "p"+pID.ToString(), pID });
                }
                sql.Append(" )");
            }


            sql = new StringBuilder(sql.ToString());
            sql.Append(" GROUP BY l.ProductID, p.BaseUnitID HAVING (SUM(ISNULL(l.QtyAllocated, 0) - ISNULL(trf.QtyShipped, 0)) > 0)");


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = DaoLabel.GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }


    }
}