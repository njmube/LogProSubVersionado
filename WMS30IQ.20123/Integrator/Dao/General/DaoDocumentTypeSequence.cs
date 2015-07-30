using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoDocumentTypeSequence : DaoService
    {
        public DaoDocumentTypeSequence(DaoFactory factory) : base(factory) { }

        public DocumentTypeSequence Save(DocumentTypeSequence data)
        {
            return (DocumentTypeSequence)base.Save(data);
        }


        public Boolean Update(DocumentTypeSequence data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentTypeSequence data)
        {
            return base.Delete(data);
        }


        public DocumentTypeSequence SelectById(DocumentTypeSequence data)
        {
            return (DocumentTypeSequence)base.SelectById(data);
        }


        public IList<DocumentTypeSequence> Select(DocumentTypeSequence data)
        {

                IList<DocumentTypeSequence> datos = new List<DocumentTypeSequence>();
                datos = GetHsql(data).List<DocumentTypeSequence>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentTypeSequence a    where  ");
            DocumentTypeSequence documenttypesequence = (DocumentTypeSequence)data;
            if (documenttypesequence != null)
            {
                Parms = new List<Object[]>();
                if (documenttypesequence.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", documenttypesequence.RowID });
                }

                if (documenttypesequence.Company != null  && documenttypesequence.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", documenttypesequence.Company.CompanyID });
                }

                if (documenttypesequence.DocType != null && documenttypesequence.DocType.DocTypeID != 0)
                {
                    sql.Append(" a.DocType.DocTypeID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", documenttypesequence.DocType.DocTypeID });
                }

                if (!String.IsNullOrEmpty(documenttypesequence.Prefix))
                {
                    sql.Append(" a.Prefix = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", documenttypesequence.Prefix });
                }

                if (documenttypesequence.NumSequence != 0)
                {
                    sql.Append(" a.NumSequence = :id3     and   ");
                    Parms.Add(new Object[] { "id3", documenttypesequence.NumSequence });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}