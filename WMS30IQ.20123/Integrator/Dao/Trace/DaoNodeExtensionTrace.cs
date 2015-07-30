using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoNodeExtensionTrace : DaoService
    {
        public DaoNodeExtensionTrace(DaoFactory factory) : base(factory) { }

        public NodeExtensionTrace Save(NodeExtensionTrace data)
        {
            return (NodeExtensionTrace)base.Save(data);
        }


        public Boolean Update(NodeExtensionTrace data)
        {
            return base.Update(data);
        }


        public Boolean Delete(NodeExtensionTrace data)
        {
            return base.Delete(data);
        }


        public NodeExtensionTrace SelectById(NodeExtensionTrace data)
        {
            return (NodeExtensionTrace)base.SelectById(data);
        }


        public IList<NodeExtensionTrace> Select(NodeExtensionTrace data)
        {

                IList<NodeExtensionTrace> datos = new List<NodeExtensionTrace>();

            try {

                datos = GetHsql(data).List<NodeExtensionTrace>();
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
            StringBuilder sql = new StringBuilder("select a from NodeExtensionTrace a    where  ");
            NodeExtensionTrace nodeextensiontrace = (NodeExtensionTrace)data;
            if (nodeextensiontrace != null)
            {
                Parms = new List<Object[]>();

                if (nodeextensiontrace.RowID.RowID != 0)
                {
                    sql.Append(" a.RowID.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", nodeextensiontrace.RowID.RowID });
                }

                if (nodeextensiontrace.NodeExtension != null && nodeextensiontrace.NodeExtension.RowID != 0)
                {
                    sql.Append(" a.NodeExtension.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", nodeextensiontrace.NodeExtension.RowID });
                }

                if (!String.IsNullOrEmpty(nodeextensiontrace.Data))
                {
                    sql.Append(" a.Data = :nom     and   ");
                    Parms.Add(new Object[] { "nom", nodeextensiontrace.Data });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.RowID.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}