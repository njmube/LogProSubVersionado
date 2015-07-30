using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoNodeExtension : DaoService
    {
        public DaoNodeExtension(DaoFactory factory) : base(factory) { }

        public NodeExtension Save(NodeExtension data)
        {
            return (NodeExtension)base.Save(data);
        }


        public Boolean Update(NodeExtension data)
        {
            return base.Update(data);
        }


        public Boolean Delete(NodeExtension data)
        {
            return base.Delete(data);
        }


        public NodeExtension SelectById(NodeExtension data)
        {
            return (NodeExtension)base.SelectById(data);
        }


        public IList<NodeExtension> Select(NodeExtension data)
        {

                IList<NodeExtension> datos = new List<NodeExtension>();

            try {
                datos = GetHsql(data).List<NodeExtension>();
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
            StringBuilder sql = new StringBuilder("select a from NodeExtension a    where  ");
            NodeExtension nodeextension = (NodeExtension)data;
            if (nodeextension != null)
            {
                Parms = new List<Object[]>();
                if (nodeextension.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", nodeextension.RowID });
                }

                if (nodeextension.Node != null && nodeextension.Node.NodeID != 0)
                {
                    sql.Append(" a.Node.NodeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", nodeextension.Node.NodeID });
                }

                if (!String.IsNullOrEmpty(nodeextension.FieldName))
                {
                    sql.Append(" a.FieldName = :nom     and   ");
                    Parms.Add(new Object[] { "nom", nodeextension.FieldName });
                }

                if (!String.IsNullOrEmpty(nodeextension.FieldType))
                {
                    sql.Append(" a.FieldType = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", nodeextension.FieldType });
                }

                if (nodeextension.Size != 0)
                {
                    sql.Append(" a.Size = :id2     and   ");
                    Parms.Add(new Object[] { "id2", nodeextension.Size });
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