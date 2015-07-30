using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoNode : DaoService
    {
        public DaoNode(DaoFactory factory) : base(factory) { }

        public Node Save(Node data)
        {
            return (Node)base.Save(data);
        }


        public Boolean Update(Node data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Node data)
        {
            return base.Delete(data);
        }


        public Node SelectById(Node data)
        {
            return (Node)base.SelectById(data);
        }


        public IList<Node> Select(Node data)
        {
                IList<Node> datos = new List<Node>();

            try {
                datos = GetHsql(data).List<Node>();
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
            StringBuilder sql = new StringBuilder("select a from Node a    where  ");
            Node node = (Node)data;
            if (node != null)
            {
                Parms = new List<Object[]>();
                if (node.NodeID != 0)
                {
                    sql.Append(" a.NodeID = :id     and   ");
                    Parms.Add(new Object[] { "id", node.NodeID });
                }



                //if (node.Location != null && node.Location.LocationID != 0)
                //{
                //    sql.Append(" a.Location.LocationID = :id2     and   ");
                //    Parms.Add(new Object[] { "id2", node.Location.LocationID });
                //}

                if (!String.IsNullOrEmpty(node.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", node.Name });
                }

                if (!String.IsNullOrEmpty(node.Description))
                {
                    sql.Append(" a.Description = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", node.Description });
                }

                if (node.IsBasic != null)
                {
                    sql.Append(" a.IsBasic = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", node.IsBasic + "%" });
                }

                if (node.NodeSeq != 0)
                {
                    sql.Append(" a.XOrder = :id3     and   ");
                    Parms.Add(new Object[] { "id3", node.NodeSeq });
                }

                if (node.RequireDocID != null)
                {
                    sql.Append(" a.RequireDocID = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", node.RequireDocID + "%" });
                }






            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.NodeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}