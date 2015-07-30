using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.Master;
using Entities.General;


namespace Integrator.Dao.Trace
{
    public class DaoDocumentPackage : DaoService
    {
        public DaoDocumentPackage(DaoFactory factory) : base(factory) { }

        public DocumentPackage Save(DocumentPackage data)
        {
            return (DocumentPackage)base.Save(data);
        }


        public Boolean Update(DocumentPackage data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentPackage data)
        {
            return base.Delete(data);
        }


        public DocumentPackage SelectById(DocumentPackage data)
        {
            return (DocumentPackage)base.SelectById(data);
        }


        public IList<DocumentPackage> Select(DocumentPackage data)
        {

                IList<DocumentPackage> datos = new List<DocumentPackage>();

            try {
                datos = GetHsql(data).List<DocumentPackage>();
                //if (!Factory.IsTransactional)
                //    Factory.Commit();

            }
            catch (Exception e)
            {
                NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
            }

                return datos;
           
        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentPackage a    where  ");
            DocumentPackage docPack = (DocumentPackage)data;
            if (docPack != null)
            {
                Parms = new List<Object[]>();

                if (docPack.PackID != 0)
                {
                    sql.Append(" a.PackID = :id     and   ");
                    Parms.Add(new Object[] { "id", docPack.PackID });
                }

                if (docPack.Document != null && docPack.Document.DocID != 0)
                {
                    sql.Append(" a.Document.DocID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", docPack.Document.DocID });

                }

                if (docPack.PackLabel != null)
                {
                    if (docPack.PackLabel.LabelID != 0)
                    {
                        sql.Append(" a.PackLabel.LabelID = :id15     and   ");
                        Parms.Add(new Object[] { "id15", docPack.PackLabel.LabelID });
                    }

                    if (!string.IsNullOrEmpty(docPack.PackLabel.LabelCode))
                    {
                        sql.Append(" a.PackLabel.LabelCode = :ilc5     and   ");
                        Parms.Add(new Object[] { "ilc5", docPack.PackLabel.LabelCode });
                    }
                }


                if (docPack.Picker != null && docPack.Picker.UserID != 0)
                {
                    sql.Append(" a.Picker.UserID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", docPack.Picker.UserID });
                }

                if (docPack.Sequence  != 0)
                {
                    sql.Append(" a.Sequence = :iss2     and   ");
                    Parms.Add(new Object[] { "iss2", docPack.Sequence });
                }

                if (docPack.Level != 0)
                {
                    sql.Append(" a.Level = :ilx2     and   ");
                    Parms.Add(new Object[] { "ilx2", docPack.Level });
                }

                if (docPack.ParentPackage != null )
                {

                    if (docPack.ParentPackage.PackID > 0)
                    {
                        sql.Append(" a.ParentPackage.PackID = :ipk2     and   ");
                        Parms.Add(new Object[] { "ipk2", docPack.ParentPackage.PackID });
                    }

                    if (docPack.ParentPackage.PackID < 0)
                        sql.Append(" a.ParentPackage.PackID  IS NULL  and   ");
                    
                }


                if (docPack.PostingDocument != null && docPack.PostingDocument.DocID != 0)
                {
                    if (docPack.PostingDocument.DocID > 0)
                    {
                        sql.Append(" a.PostingDocument.DocID = :id5     and   ");
                        Parms.Add(new Object[] { "id5", docPack.PostingDocument.DocID });
                    }

                    if (docPack.PostingDocument.DocID < 0)
                        sql.Append(" a.PostingDocument.DocID is null  and   ");
                    
                }



             }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.PackID ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}