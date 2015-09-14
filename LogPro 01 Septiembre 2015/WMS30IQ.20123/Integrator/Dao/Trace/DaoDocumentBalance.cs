using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.Master;
using Entities.General;
using System.Linq;




namespace Integrator.Dao.Trace
{
    public class DaoDocumentBalance : DaoService
    {

        public DaoDocumentBalance(DaoFactory factory) : base(factory) { }


        public override IQuery GetHsql(Object data)
        {
            return null;
        }


        /// <summary>
        /// Entrega lo que esta pendiente por recibir de un Documento contra un NodeTrace En unidad Basica
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IList<DocumentBalance> GeneralBalance(DocumentBalance docBalance, bool isCrossDock)
        {
            string sQuery = "";
            if (isCrossDock) //No se debe tener en cuenta el back order
                sQuery = "select doc.docID, cast(0 as bigint) as Line, node.NodeID, product.ProductID, unit.UnitID, "
                + " Sum(l.Quantity - l.QtyCancel) as Quantity, "
                + " Sum(l.Quantity - l.QtyCancel) - Isnull(n.Quantity/unit.BaseAmount,0) as QtyPending, Max(l.UnitPrice) as UnitPrice, Min(l.Sequence) as Seq ";

            else
                sQuery = "select doc.docID, cast(0 as bigint) as Line, node.NodeID, product.ProductID, unit.UnitID, "
                + " Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder) as Quantity, "
                + " Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder) - Isnull(n.Quantity/unit.BaseAmount,0) as QtyPending, Max(l.UnitPrice) as UnitPrice, Min(l.Sequence) as Seq ";

                

            sQuery +=  " from Trace.DocumentLine as l "
            + " INNER JOIN Trace.Document doc ON l.DocID = doc.DocID "
            + " INNER JOIN Trace.Node node ON node.NodeID = :id2 "
            + " INNER JOIN Master.Product product ON l.ProductID = product.ProductID "
            + " INNER JOIN Master.Unit unit ON l.UnitID = unit.UnitID LEFT OUTER JOIN  "
            + " ( select lb.ProductID, Isnull(Sum(x.Quantity * ISNULL(u.BaseAmount,1)),0) as Quantity from Trace.NodeTrace x  "
            + " INNER JOIN Trace.Label lb ON x.LabelID = lb.LabelID LEFT JOIN Master.Unit u ON u.UnitID = x.UnitID " //lb.UnitID
            + " Where x.NodeID = :id2 and x.DocID = :id1 AND x.Quantity > 0 AND x.IsDebit = 0 "
            + " Group BY lb.ProductID ) as n  On l.ProductID = n.ProductID Where l.DocID = :id1 ";
  

            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });

            if (docBalance.Node != null && docBalance.Node.NodeID != 0)
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            else if (docBalance.Node != null && !string.IsNullOrEmpty(docBalance.Node.Name))
            {
               docBalance.Node = Factory.DaoNode().Select(docBalance.Node).First();
               Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            }

            if (docBalance != null)
            {

               if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and l.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }

               sql.Append(" and ( l.LineStatusID = :id4  Or l.LineStatusID = :id5 ) ");
               Parms.Add(new Object[] { "id4", DocStatus.New });
               Parms.Add(new Object[] { "id5", DocStatus.InProcess });

               sql.Append(" and product.StatusID = :id6 ");
               Parms.Add(new Object[] { "id6", EntityStatus.Active });

               sql.Append(" Group By doc.docID, node.NodeID, product.ProductID, unit.UnitID, unit.BaseAmount, n.Quantity"); //unit.BaseAmount,
               sql.Append(" Order By Seq");

            }
            
            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<DocumentBalance> retBalance = GetBalanceObject(query.List<Object[]>(), docBalance.Location);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance;
        }


        private IList<DocumentBalance> GetBalanceObject(IList<Object[]> retList, Location location)
        {
            if (retList == null || retList.Count == 0)
                return new List<DocumentBalance>();

            IList<DocumentBalance> retBalance = new List<DocumentBalance>();
            DocumentBalance curBalance;

            Document document = Factory.DaoDocument().SelectById(new Document { DocID = (int)retList[0][0] });
            Node node = Factory.DaoNode().SelectById(new Node { NodeID = (int)(int)retList[0][2] });

            foreach (Object[] obj in retList) {
                
                curBalance = new DocumentBalance();

                curBalance.Document = document;
                curBalance.DocumentLine = ((Int64)obj[1] == 0) ? null : Factory.DaoDocumentLine().SelectById(new DocumentLine { LineID = (Int64)obj[1] });
                curBalance.Node = node;
                curBalance.Product = Factory.DaoProduct().SelectById(new Product { ProductID = (int)obj[3] });
                curBalance.Unit = Factory.DaoUnit().SelectById(new Unit { UnitID = (int)obj[4] });
                curBalance.Quantity = Double.Parse(obj[5].ToString());
                curBalance.QtyPending = Double.Parse(obj[6].ToString());

                try { curBalance.UnitPrice = Double.Parse(obj[7].ToString()); }
                catch { }

                curBalance.Location = location;

                retBalance.Add(curBalance);
            }

            return retBalance;
        }


        public IList<DocumentBalance> DetailedBalance(DocumentBalance docBalance, bool isCrossDock)
        {
            string sQuery = "";

            if (isCrossDock) //No se debe tener en cuenta el back order
                sQuery = "SELECT  l.DocID, l.LineID, node.NodeID, l.ProductID, l.UnitID, SUM(l.Quantity - l.QtyCancel)  AS Quantity, " +
                "SUM(l.Quantity - l.QtyCancel)  - ISNULL(n.Quantity/u.BaseAmount,0) AS QtyPending, Max(l.UnitPrice) as UnitPrice ";

            else
                sQuery = "SELECT  l.DocID, l.LineID, node.NodeID, l.ProductID, l.UnitID, SUM(l.Quantity - l.QtyCancel - l.QtyBackOrder) AS Quantity, " +
                "SUM(l.Quantity - l.QtyCancel - l.QtyBackOrder) - ISNULL(n.Quantity/u.BaseAmount,0) AS QtyPending, Max(l.UnitPrice) as UnitPrice ";


            sQuery +=    "FROM  Trace.DocumentLine AS l "+
	            "INNER JOIN Master.Product AS p ON l.ProductID = p.ProductID "+
	            "INNER JOIN Master.Unit AS u ON l.UnitID = u.UnitID "+
                "INNER JOIN Trace.Node node ON node.NodeID = :id2 " +
	            "LEFT OUTER JOIN ( "+
			    "        SELECT  lbl.ProductID, ISNULL(SUM(x.Quantity * ISNULL(un.BaseAmount,1)) ,0) AS Quantity, x.DocLineID "+
			    "        FROM          Trace.NodeTrace AS x "+
			    "        INNER JOIN Trace.Label AS lbl ON x.LabelID = lbl.LabelID "+
			    "        LEFT OUTER JOIN Master.Unit AS un ON lbl.UnitID = un.UnitID "+
                "        WHERE   x.NodeID = :id2  AND x.DocID = :id1 And x.Quantity > 0 " +
                "        GROUP BY lbl.ProductID, x.DocLineID  " +
	            ") AS n "+
            "ON n.ProductID = l.ProductID AND  l.LineID = n.DocLineID "+
            "WHERE     (l.DocID = :id1 ) ";

            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });
            Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });

            if (docBalance != null)
            {

                if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and p.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }

                sql.Append(" and ( l.LineStatusID = :id4  Or l.LineStatusID = :id5 ) ");
                Parms.Add(new Object[] { "id4", DocStatus.New });
                Parms.Add(new Object[] { "id5", DocStatus.InProcess });

                sql.Append(" and p.StatusID = :id6 ");
                Parms.Add(new Object[] { "id6", EntityStatus.Active });

                sql.Append("GROUP BY l.DocID, l.LineID, node.NodeID, l.ProductID, l.UnitID, u.BaseAmount, n.Quantity ");

            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<DocumentBalance> retBalance = GetBalanceObject(query.List<Object[]>(), docBalance.Location);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance;
        }


        ///// <summary>
        ///// Entrega el Balance entre un docuemnto de receiving y uno o varios documentos de shipping
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        ///// 
        //public IList<DocumentBalance> CrossDockBalance(Document receivingTask, IList<Document> shipDocs, Product product)
        //{

        //    string sQuery = "select l.Document, 0 DocumentLine, null, l.Product, l.Product.BaseUnit as Unit, "
        //    + " Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder)*l.Unit.BaseAmount) as Quantity, Sum((l.Quantity - l.QtyCancel - l.QtyBackOrder)*l.Unit.BaseAmount) "
        //    + " - Sum((sd.Quantity - sd.QtyCancel - sd.QtyBackOrder)*sd.Unit.BaseAmount) as QtyPending  from DocumentLine l Left Outer Join "
        //    + " DocumentLine sd ON l.Product.ProductID = sd.Product.ProductID Where l.Document.DocID = :id1 ";

        //    StringBuilder sql = new StringBuilder(sQuery);
            
        //    Parms = new List<Object[]>();
        //    Parms.Add(new Object[] { "id1", receivingTask.DocID });

        //    //Ciclo para los shipping Documents
        //    int i = 1;
        //    foreach (Document doc in shipDocs)
        //    {
        //        string sOR = (i > 1) ? " or " : " ( ";

        //        sql.Append(sOR + " sd.Document.DocID = :id" + i.ToString());
        //        Parms.Add(new Object[] { "id" + i.ToString(), doc.DocID });
        //        i++;
        //    }

        //    sql.Append(" and ( l.LineStatusID = :id4  Or l.LineStatusID = :id5 ) ");
        //    Parms.Add(new Object[] { "id4", DocStatus.New });
        //    Parms.Add(new Object[] { "id5", DocStatus.InProcess });

        //    sql.Append(" and l.Product.StatusID = :id6 ");
        //    Parms.Add(new Object[] { "id6", EntityStatus.Active });


        //    if (product != null && product.ProductID != 0)
        //    {
        //        sql.Append(" and l.Product.ProductID = :id3 ");
        //        Parms.Add(new Object[] { "id3", product.ProductID });
        //    }

        //    sql.Append("Group by l.Document, n.Node, l.Product, n.Label.Product.BaseUnit");


        //    IQuery query = Factory.Session.CreateQuery(sql.ToString());
        //    SetParameters(query);

        //    if (!Factory.IsTransactional)
        //        Factory.Commit();

        //    return query.List<DocumentBalance>();
        //}


        /// <summary>
        /// Entrega lo que esta pendiente por recibir de un Documento contra un NodeTrace En unidad del documento
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IList<DocumentBalance> BalanceByUnit(DocumentBalance docBalance)
        {

            string sQuery = "select doc.docID, cast(0 as bigint) as Line, node.NodeID, product.ProductID, l.UnitID, "
             + "Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder) as Quantity, "
             + "Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder) - Isnull(n.Quantity / unit.BaseAmount,0) as QtyPending, Max(l.UnitPrice) as UnitPrice "
             +"from Trace.DocumentLine as l "
             +"INNER JOIN Trace.Document doc ON l.DocID = doc.DocID "
             +"INNER JOIN Master.Product product ON l.ProductID = product.ProductID "
             +"INNER JOIN Master.Unit unit ON l.UnitID = unit.UnitID "
             +"INNER JOIN Trace.Node node ON node.NodeID = :id2 "
             +"LEFT OUTER JOIN  "
             +"( select l.ProductID, l.UnitID, Isnull(Sum(x.Quantity * ISNULL(u.BaseAmount,1)),0) as Quantity from Trace.NodeTrace x  "
             +"INNER JOIN Trace.Label l ON x.LabelID = L.LabelID LEFT OUTER JOIN Master.Unit u ON u.UnitID = l.UnitID "
             + "Where x.NodeID = :id2 and x.DocID = :id1 And x.Quantity > 0 "
             + "Group BY l.ProductID, l.UnitID ) as n  On n.ProductID = l.ProductID and n.UnitID = l.UnitID Where l.DocID = :id1";


            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });

            if (docBalance.Node != null && docBalance.Node.NodeID != 0)
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            else if (docBalance.Node != null && !string.IsNullOrEmpty(docBalance.Node.Name))
            {
                docBalance.Node = Factory.DaoNode().Select(docBalance.Node).First();
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            }

            if (docBalance != null)
            {

                if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and l.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }

                sql.Append(" and ( l.LineStatusID = :id4  Or l.LineStatusID = :id5 ) ");
                Parms.Add(new Object[] { "id4", DocStatus.New });
                Parms.Add(new Object[] { "id5", DocStatus.InProcess });

                sql.Append(" and product.StatusID = :id6 ");
                Parms.Add(new Object[] { "id6", EntityStatus.Active });

                sql.Append(" Group By doc.docID, node.NodeID, product.ProductID, l.UnitID,  unit.BaseAmount, n.Quantity");
            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<DocumentBalance> retBalance = GetBalanceObject(query.List<Object[]>(), docBalance.Location);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance;
        }



        /// <summary>
        /// Entrega lo que esta pendiente por recibir de un Documento contra un NodeTrace En unidad Basica
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IList<DocumentBalance> PostingBalance(DocumentBalance docBalance)
        {

            string sQuery = "select n.docID, cast(0 as bigint) as Line, n.NodeID, n.ProductID, n.UnitID, "
            +" Isnull(n.Quantity,0) as Quantity, Isnull(unpost.Quantity,0) as QtyPending, 0 as UnitPrice  "
            +"  From ( select x.DocID, l.ProductID, l.UnitID, x.NodeID, Isnull(Sum(x.Quantity),0) as Quantity from Trace.NodeTrace x  "
             +" INNER JOIN Trace.Label l ON l.LabelID = x.LabelID  "
            +"  Where x.NodeID = :id2 and x.DocID = :id1 and  x.Quantity > 0  "
			+"  Group BY l.ProductID, l.UnitID, x.DocID, x.NodeID ) as n INNER JOIN   "
            +"  ( select l.ProductID, l.UnitID, Isnull(Sum(x.Quantity),0) as Quantity from Trace.NodeTrace x "
            +"  INNER JOIN Trace.Label l ON l.LabelID = x.LabelID  "
            +"  Where x.NodeID = :id2 and x.DocID = :id1 And x.PostingDocumentID IS NULL and  x.Quantity > 0  "
            + "  Group BY l.ProductID, l.UnitID ) as unpost  On  n.ProductID = unpost.ProductID  "
            +"  AND n.UnitID = unpost.UnitID Where n.DocID = :id1";


            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });

            if (docBalance.Node != null && docBalance.Node.NodeID != 0)
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            else if (docBalance.Node != null && !string.IsNullOrEmpty(docBalance.Node.Name))
            {
                docBalance.Node = Factory.DaoNode().Select(docBalance.Node).First();
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            }

            if (docBalance != null)
            {

                if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and n.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }

                sql.Append(" Group By n.docID, n.NodeID, n.productid, n.unitid, n.Quantity, unpost.Quantity");
            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<DocumentBalance> retBalance = GetBalanceObject(query.List<Object[]>(), docBalance.Location);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance;
        }



        public IList<DocumentBalance> DocumentBalanceForEmpty(DocumentBalance docBalance)
        {

            string sQuery = "select n.DocID, cast(0 as bigint) as Line, n.NodeID, l.ProductID, l.UnitID, "
            + " Isnull(Sum(n.Quantity),0) as Quantity, 0.0 as QtyPending, 0 as UnitPrice "
            + " from Trace.NodeTrace n INNER JOIN Trace.Label l ON n.LabelID = l.LabelID "
            + " Where n.NodeID = :id2 and n.DocID = :id1 And n.Quantity > 0 ";


            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });

            if (docBalance.Node != null && docBalance.Node.NodeID != 0)
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            else if (docBalance.Node != null && !string.IsNullOrEmpty(docBalance.Node.Name))
            {
                docBalance.Node = Factory.DaoNode().Select(docBalance.Node).First();
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });
            }

            if (docBalance != null)
            {

                if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and n.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }


                sql.Append(" Group By n.docID, n.NodeID, l.ProductID, l.UnitID");
            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<DocumentBalance> retBalance = GetBalanceObject(query.List<Object[]>(), docBalance.Location);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance;
        }



        public bool IsOrderBalanceCompleted(DocumentBalance docBalance)
        {

            string
                sQuery = "select product.ProductID, Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder)*unit.BaseAmount - Isnull(n.Quantity,0) as QtyPending "
            + " from Trace.DocumentLine as l "
            + " INNER JOIN Trace.Document doc ON l.DocID = doc.DocID "
            + " INNER JOIN Trace.Node node ON node.NodeID = :id2 "
            + " INNER JOIN Master.Product product ON l.ProductID = product.ProductID "
            + " INNER JOIN Master.Unit unit ON l.UnitID = unit.UnitID LEFT OUTER JOIN  "
            + " ( select l.ProductID, Isnull(Sum(x.Quantity * ISNULL(u.BaseAmount,1)),0) as Quantity from Trace.NodeTrace x  "
            + " INNER JOIN Trace.Label l ON x.LabelID = l.LabelID LEFT JOIN Master.Unit u ON u.UnitID = x.UnitID "
            + " Where x.NodeID = :id2 and x.DocID = :id1 AND x.Quantity > 0 AND x.IsDebit = 0 "
            + " Group BY l.ProductID ) as n  On l.ProductID = n.ProductID Where l.DocID = :id1 AND l.LineNumber > 0 ";


            StringBuilder sql = new StringBuilder(sQuery);
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", docBalance.Document.DocID });

            if (docBalance.Node != null && docBalance.Node.NodeID != 0)
                
                Parms.Add(new Object[] { "id2", docBalance.Node.NodeID });


            if (docBalance != null)
            {

                if (docBalance.Product != null && docBalance.Product.ProductID != 0)
                {
                    sql.Append(" and l.ProductID = :id3 ");
                    Parms.Add(new Object[] { "id3", docBalance.Product.ProductID });
                }

                sql.Append(" and ( l.LineStatusID = :id4  Or l.LineStatusID = :id5 ) ");
                Parms.Add(new Object[] { "id4", DocStatus.New });
                Parms.Add(new Object[] { "id5", DocStatus.InProcess });

                sql.Append(" and product.StatusID = :id6 ");
                Parms.Add(new Object[] { "id6", EntityStatus.Active });

                sql.Append(" Group By  product.ProductID,n.Quantity, unit.BaseAmount ");
                sql.Append(" Having Sum(l.Quantity - l.QtyCancel - l.QtyBackOrder) - Isnull(n.Quantity,0) > 0");

            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            IList<Object[]> retBalance = query.List<Object[]>();

            if (!Factory.IsTransactional)
                Factory.Commit();

            return retBalance.Count > 0 ? false : true ;
        }


    }
}
      
