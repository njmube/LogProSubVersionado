using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoTaskDocumentRelation : DaoService
    {
        public DaoTaskDocumentRelation(DaoFactory factory) : base(factory) { }

        public TaskDocumentRelation Save(TaskDocumentRelation data)
        {
            return (TaskDocumentRelation)base.Save(data);
        }


        public Boolean Update(TaskDocumentRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(TaskDocumentRelation data)
        {
            return base.Delete(data);
        }


        public TaskDocumentRelation SelectById(TaskDocumentRelation data)
        {
            return (TaskDocumentRelation)base.SelectById(data);
        }


        public IList<TaskDocumentRelation> Select(TaskDocumentRelation data)
        {

                IList<TaskDocumentRelation> datos = new List<TaskDocumentRelation>();

                try
                {
                    datos = GetHsql(data).List<TaskDocumentRelation>();
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
            StringBuilder sql = new StringBuilder("select a from TaskDocumentRelation a    where  ");
            TaskDocumentRelation taskdocument = (TaskDocumentRelation)data;
            if (taskdocument != null)
            {
                Parms = new List<Object[]>();
                if (taskdocument.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", taskdocument.RowID });
                }


                if (taskdocument.IncludedDoc != null && taskdocument.IncludedDoc.DocID != 0)
                {
                    sql.Append(" a.IncludedDoc.DocID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", taskdocument.IncludedDoc.DocID });
                }

                if (taskdocument.TaskDoc != null && taskdocument.TaskDoc.DocID != 0)
                {
                    sql.Append(" a.TaskDoc.DocID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", taskdocument.TaskDoc.DocID });
                }

                if (taskdocument.TaskDoc != null && taskdocument.TaskDoc.DocType != null && taskdocument.TaskDoc.DocType.DocTypeID != 0)
                {
                    sql.Append(" a.TaskDoc.DocType.DocTypeID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", taskdocument.TaskDoc.DocType.DocTypeID });
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