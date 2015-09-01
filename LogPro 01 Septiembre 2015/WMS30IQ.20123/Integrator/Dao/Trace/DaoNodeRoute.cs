using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoNodeRoute : DaoService
    {
        public DaoNodeRoute(DaoFactory factory) : base(factory) { }

        public NodeRoute Save(NodeRoute data)
        {
            return (NodeRoute)base.Save(data);
        }


        public Boolean Update(NodeRoute data)
        {
            return base.Update(data);
        }


        public Boolean Delete(NodeRoute data)
        {
            return base.Delete(data);
        }


        public NodeRoute SelectById(NodeRoute data)
        {
            return (NodeRoute)base.SelectById(data);
        }


        public IList<NodeRoute> Select(NodeRoute data)
        {

                IList<NodeRoute> datos = new List<NodeRoute>();

                datos = GetHsql(data).List<NodeRoute>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from NodeRoute a    where  ");
            NodeRoute noderoute = (NodeRoute)data;
            if (noderoute != null)
            {
                Parms = new List<Object[]>();
                if (noderoute.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", noderoute.RowID });
                }

                if (noderoute.NextNode != null && noderoute.NextNode.NodeID != 0)
                {
                    sql.Append(" a.NextNode.NodeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", noderoute.NextNode.NodeID });
                }

                if (noderoute.CurNode != null && noderoute.CurNode.NodeID != 0)
                {
                    sql.Append(" a.CurNode.NodeID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", noderoute.CurNode.NodeID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}