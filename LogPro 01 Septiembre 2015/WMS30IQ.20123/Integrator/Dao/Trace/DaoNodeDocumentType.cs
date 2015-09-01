using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoNodeDocumentType : DaoService
    {
        public DaoNodeDocumentType(DaoFactory factory) : base(factory) { }

        public NodeDocumentType Save(NodeDocumentType data)
        {
            return (NodeDocumentType)base.Save(data);
        }


        public Boolean Update(NodeDocumentType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(NodeDocumentType data)
        {
            return base.Delete(data);
        }


        public NodeDocumentType SelectById(NodeDocumentType data)
        {
            return (NodeDocumentType)base.SelectById(data);
        }


        public IList<NodeDocumentType> Select(NodeDocumentType data)
        {

                IList<NodeDocumentType> datos = new List<NodeDocumentType>();

            try {
                datos = GetHsql(data).List<NodeDocumentType>();
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
            StringBuilder sql = new StringBuilder("select a from NodeDocumentType a    where  ");
            NodeDocumentType nodedocumenttype = (NodeDocumentType)data;
            if (nodedocumenttype != null)
            {
                Parms = new List<Object[]>();
                if (nodedocumenttype.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", nodedocumenttype.RowID });
                }

                if (nodedocumenttype.Node != null && nodedocumenttype.Node.NodeID != 0)
                {
                    sql.Append(" a.Node.NodeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", nodedocumenttype.Node.NodeID });
                }

                if (nodedocumenttype.DocType != null && nodedocumenttype.DocType.DocTypeID != 0)
                {
                    sql.Append(" a.DocType.DocTypeID= :id2     and   ");
                    Parms.Add(new Object[] { "id2", nodedocumenttype.DocType.DocTypeID });
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