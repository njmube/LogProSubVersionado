using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.Master;

namespace Integrator.Dao.Trace
{
    public class DaoNodeTrace : DaoService
    {
        public DaoNodeTrace(DaoFactory factory) : base(factory) { }

        public NodeTrace Save(NodeTrace data)
        {
            return (NodeTrace)base.Save(data);
        }


        public Boolean Update(NodeTrace data)
        {
            return base.Update(data);
        }


        public Boolean Delete(NodeTrace data)
        {
            return base.Delete(data);
        }


        public NodeTrace SelectById(NodeTrace data)
        {
            return (NodeTrace)base.SelectById(data);
        }


        public IList<NodeTrace> Select(NodeTrace data)
        {
                IList<NodeTrace> datos = GetHsql(data).List<NodeTrace>();

            try {
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
            StringBuilder sql = new StringBuilder("select a from NodeTrace a    where  ");
            NodeTrace nodetrace = (NodeTrace)data;
            if (nodetrace != null)
            {
                Parms = new List<Object[]>();
                if (nodetrace.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", nodetrace.RowID });
                }

                if (nodetrace.Node != null && nodetrace.Node.NodeID != 0)
                {
                    sql.Append(" a.Node.NodeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", nodetrace.Node.NodeID });
                }

                if (nodetrace.Document != null && nodetrace.Document.DocID != 0)
                {
                    sql.Append(" a.Document.DocID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", nodetrace.Document.DocID });
                }


                if (nodetrace.Bin != null && nodetrace.Bin.BinID != 0)
                {
                    sql.Append(" a.Bin.BinID = :id5     and   ");
                    Parms.Add(new Object[] { "id5", nodetrace.Bin.BinID });
                }

                if (nodetrace.BinSource != null && nodetrace.BinSource.BinID != 0)
                {
                    sql.Append(" a.BinSource.BinID = :bs1     and   ");
                    Parms.Add(new Object[] { "bs1", nodetrace.BinSource.BinID });
                }

                if (nodetrace.IsDebit != null)
                {
                    sql.Append(" a.IsDebit = :ids5     and   ");
                    Parms.Add(new Object[] { "ids5", nodetrace.IsDebit });
                }


                if (nodetrace.PostingDocLineNumber != 0)
                {
                    sql.Append(" a.PostingDocLineNumber = :ipl5     and   ");
                    Parms.Add(new Object[] { "ipl5", nodetrace.PostingDocLineNumber });
                }


                //LABEL

                if (nodetrace.Label != null)
                {


                    if (!String.IsNullOrEmpty(nodetrace.Label.LabelCode))
                    {
                        sql.Append(" (a.Label.LabelCode = :nom OR a.Label.LabelID = :lb1d ) and ");

                          long barcode;
                          if (Int64.TryParse(nodetrace.Label.LabelCode, out barcode))
                            Parms.Add(new Object[] { "nom", barcode.ToString() });
                        else
                              Parms.Add(new Object[] { "nom", nodetrace.Label.LabelCode });

                        try
                        {
                            if (nodetrace.Label.LabelCode.StartsWith("1") && nodetrace.Label.LabelCode.Length == WmsSetupValues.LabelLength)
                                Parms.Add(new Object[] { "lb1d", Int64.Parse(nodetrace.Label.LabelCode.Substring(1, WmsSetupValues.LabelLength - 1)) });
                            else
                                Parms.Add(new Object[] { "lb1d", 0 });
                        }
                        catch { Parms.Add(new Object[] { "lb1d", 0 }); }

                    }


                    if (nodetrace.Label.FatherLabel != null && nodetrace.Label.FatherLabel.LabelID != 0)
                    {
                        if (nodetrace.Label.FatherLabel.LabelID > 0)
                        {
                            sql.Append(" a.Label.FatherLabel.LabelID = :if6     and   ");
                            Parms.Add(new Object[] { "if6", nodetrace.Label.FatherLabel.LabelID });
                        }
                        else
                            sql.Append(" a.Label.FatherLabel.LabelID is null  and   ");

                    }

                    if (nodetrace.Label.LabelID != 0)
                    {
                        sql.Append(" a.Label.LabelID = :id6     and   ");
                        Parms.Add(new Object[] { "id6", nodetrace.Label.LabelID });
                    }


                    if (!string.IsNullOrEmpty(nodetrace.Label.PrintingLot))
                    {
                        sql.Append(" a.Label.PrintingLot = :pl01    and   ");
                        Parms.Add(new Object[] { "pl01", nodetrace.Label.PrintingLot });
                    }


                    if (nodetrace.Label.IsLogistic != null)
                    {
                        sql.Append(" a.Label.IsLogistic = :id26     and   ");
                        Parms.Add(new Object[] { "id26", nodetrace.Label.IsLogistic });
                    }


                    if (nodetrace.Label.Product != null && nodetrace.Label.Product.ProductID != 0)
                    {
                        sql.Append(" a.Label.Product.ProductID = :id10     and   ");
                        Parms.Add(new Object[] { "id10", nodetrace.Label.Product.ProductID });

                    }

                    if (nodetrace.Label.Node != null && nodetrace.Label.Node.NodeID != 0)
                    {
                        sql.Append(" a.Label.Node.NodeID = :nid10     and   ");
                        Parms.Add(new Object[] { "nid10", nodetrace.Label.Node.NodeID });

                    }

                    if (nodetrace.Label.Status != null && nodetrace.Label.Status.StatusID != 0)
                    {
                        sql.Append(" a.Label.Status.StatusID = :sid10     and   ");
                        Parms.Add(new Object[] { "sid10", nodetrace.Label.Status.StatusID });

                    }

                    if (nodetrace.Label.Unit != null && nodetrace.Label.Unit.UnitID != 0)
                    {
                        sql.Append(" a.Label.Unit.UnitID = :id11     and   ");
                        Parms.Add(new Object[] { "id11", nodetrace.Label.Unit.UnitID });

                    }

                    if (nodetrace.Label.Printed != null)
                    {
                        sql.Append(" a.Label.Printed = :id12     and   ");
                        Parms.Add(new Object[] { "id12", nodetrace.Label.Printed });

                    }


                    if (nodetrace.Label.Node != null && nodetrace.Label.Node.NodeID != 0)
                    {
                        sql.Append(" a.Label.Node.NodeID = :nt11     and   ");
                        Parms.Add(new Object[] { "nt11", nodetrace.Label.Node.NodeID });

                    }

                    if (nodetrace.Label.LabelType != null && nodetrace.Label.LabelType.DocTypeID != 0)
                    {
                        sql.Append(" a.Label.LabelType.DocTypeID = :lx11     and   ");
                        Parms.Add(new Object[] { "lx11", nodetrace.Label.LabelType.DocTypeID });

                    }


                }



                if (nodetrace.Status != null && nodetrace.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id7     and   ");
                    Parms.Add(new Object[] { "id7", nodetrace.Status.StatusID });
                }

                if (!String.IsNullOrEmpty(nodetrace.Comment))
                {
                    sql.Append(" a.Comment = :nomz     and   ");
                    Parms.Add(new Object[] { "nomz", nodetrace.Comment });
                }

                if (nodetrace.PostingDocument != null)
                {

                    if (nodetrace.PostingDocument.DocID <= 0)
                        sql.Append(" a.PostingDocument.DocID is null and ");

                    else if (nodetrace.PostingDocument.DocID == 1)
                        sql.Append(" a.PostingDocument.DocID IS NOT NULL and ");

                    else
                    {
                        sql.Append(" a.PostingDocument.DocID = :dom2  and ");
                        Parms.Add(new Object[] { "dom2", nodetrace.PostingDocument.DocID });
                    }
                }
 

                if (nodetrace.PostingDate != null)
                {
                    sql.Append(" a.PostingDate like :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", nodetrace.PostingDate + "%" });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 "); //order by a.RowID asc 
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


        public IList<long> GetRecordWithoutTrackOption(Document document, ProductTrackRelation productTrack, Node node)
        {
            string sqlbase = "select l.LabelID from Trace.NodeTrace n INNER JOIN Trace.Document d ON n.DocID = d.DocID AND n.DocID = :id2 "
            + " INNER JOIN Trace.Label l ON n.LabelID = l.LabelID AND l.ProductID = :id6"
            + " LEFT OUTER JOIN Trace.LabelTrackOption t ON l.LabelId = t.LabelID AND t.TrackOptionID = :id8"
            + " WHERE n.NodeID = :nd1  AND (t.TrackValue IS NULL OR LTRIM(RTRIM(t.TrackValue)) = '')";
            
            StringBuilder sql = new StringBuilder(sqlbase);

            if (productTrack != null)
            {
                Parms = new List<Object[]>();

                Parms.Add(new Object[] { "nd1", node.NodeID });


                if (document != null && document.DocID != 0)
                    Parms.Add(new Object[] { "id2", document.DocID });

                Parms.Add(new Object[] { "id6", productTrack.Product.ProductID });
                Parms.Add(new Object[] { "id8", productTrack.TrackOption.RowID });

            }

            sql = new StringBuilder(sql.ToString());
            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);
            return query.SetMaxResults(1).List<long>();

        }

    }
}