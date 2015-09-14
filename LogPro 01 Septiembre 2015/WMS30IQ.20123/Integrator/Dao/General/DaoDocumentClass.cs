using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoDocumentClass : DaoService
    {
        public DaoDocumentClass(DaoFactory factory) : base(factory) { }

        public DocumentClass Save(DocumentClass data)
        {
            return (DocumentClass)base.Save(data);
        }


        public Boolean Update(DocumentClass data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentClass data)
        {
            return base.Delete(data);
        }


        public DocumentClass SelectById(DocumentClass data)
        {
            return (DocumentClass)base.SelectById(data);
        }


        public IList<DocumentClass> Select(DocumentClass data)
        {

                IList<DocumentClass> datos = new List<DocumentClass>();
                datos = GetHsql(data).List<DocumentClass>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
 
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentClass a    where  ");
            DocumentClass documentclass = (DocumentClass)data;
            if (documentclass != null)
            {
                Parms = new List<Object[]>();
                if (documentclass.DocClassID != 0)
                {
                    sql.Append(" a.DocClassID = :id     and   ");
                    Parms.Add(new Object[] { "id", documentclass.DocClassID });
                }

                if (!String.IsNullOrEmpty(documentclass.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", documentclass.Name });
                }

                if (!String.IsNullOrEmpty(documentclass.Description))
                {
                    sql.Append(" a.Description = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", documentclass.Description });
                }

                if (documentclass.HasAdmin != null)
                {
                    sql.Append(" a.hasAdmin = :nom2    and   ");
                    Parms.Add(new Object[] { "nom2", documentclass.HasAdmin });
                }

                if (!String.IsNullOrEmpty(documentclass.Fields))
                {
                    sql.Append(" a.Fields LIKE :nom3   and   ");
                    Parms.Add(new Object[] { "nom3", "%" + documentclass.Fields + "%" });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.DocClassID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}