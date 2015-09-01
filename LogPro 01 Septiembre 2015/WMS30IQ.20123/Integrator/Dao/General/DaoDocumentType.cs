using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoDocumentType : DaoService
    {
        public DaoDocumentType(DaoFactory factory) : base(factory) { }

        public DocumentType Save(DocumentType data)
        {
            return (DocumentType)base.Save(data);
        }


        public Boolean Update(DocumentType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentType data)
        {
            return base.Delete(data);
        }


        public DocumentType SelectById(DocumentType data)
        {
            return (DocumentType)base.SelectById(data);
        }


        public IList<DocumentType> Select(DocumentType data)
        {
            IList<DocumentType> datos = new List<DocumentType>();

            datos = GetHsql(data).List<DocumentType>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentType a    where  ");
            DocumentType documenttype = (DocumentType)data;
            if (documenttype != null)
            {
                Parms = new List<Object[]>();
                if (documenttype.DocTypeID != 0)
                {
                    sql.Append(" a.DocTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", documenttype.DocTypeID });
                }

                if (documenttype.DocClass != null)
                {
                    if (documenttype.DocClass.DocClassID != 0)
                    {
                        sql.Append(" a.DocClass.DocClassID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", documenttype.DocClass.DocClassID });
                    }

                    if (documenttype.DocClass.HasAdmin != null)
                    {
                        sql.Append(" a.DocClass.HasAdmin = :idd2   and   ");
                        Parms.Add(new Object[] { "idd2", documenttype.DocClass.HasAdmin });
                    }
                }

                if (documenttype.StatusType != null && documenttype.StatusType.StatusTypeID != 0)
                {
                    sql.Append(" a.StatusType.StatusTypeID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", documenttype.StatusType.StatusTypeID });
                }

                if (documenttype.PickMethod != null && documenttype.PickMethod.MethodID != 0)
                {
                    sql.Append(" a.PickMethod.MethodID = :id3    and   ");
                    Parms.Add(new Object[] { "id3", documenttype.PickMethod.MethodID });
                }


                if (!String.IsNullOrEmpty(documenttype.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", documenttype.Name });
                }

                if (documenttype.Template != null && documenttype.Template.RowID != 0)
                {
                    sql.Append(" a.Template.RowID = :id8     and   ");
                    Parms.Add(new Object[] { "id8", documenttype.Template.RowID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.DocTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}