using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoDocumentConcept : DaoService
    {
        public DaoDocumentConcept(DaoFactory factory) : base(factory) { }

        public DocumentConcept Save(DocumentConcept data)
        {
            return (DocumentConcept)base.Save(data);
        }


        public Boolean Update(DocumentConcept data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentConcept data)
        {
            return base.Delete(data);
        }


        public DocumentConcept SelectById(DocumentConcept data)
        {
            return (DocumentConcept)base.SelectById(data);
        }


        public IList<DocumentConcept> Select(DocumentConcept data)
        {

            IList<DocumentConcept> datos = new List<DocumentConcept>();
            datos = GetHsql(data).List<DocumentConcept>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;
        }
     



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentConcept a    where  ");
            DocumentConcept docconcept = (DocumentConcept)data;
            if (docconcept != null)
            {
                Parms = new List<Object[]>();
                if (docconcept.DocConceptID != 0)
                {
                    sql.Append(" a.DocConceptID = :id     and   ");
                    Parms.Add(new Object[] { "id", docconcept.DocConceptID });
                }

                if (docconcept.DocClass != null && docconcept.DocClass.DocClassID != 0)
                {
                    sql.Append(" a.DocClass.DocClassID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", docconcept.DocClass.DocClassID });
                }


                if (!String.IsNullOrEmpty(docconcept.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", docconcept.Name });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.DocConceptID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}