using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoDocumentAddress : DaoService
    {
        public DaoDocumentAddress(DaoFactory factory) : base(factory) { }

        public DocumentAddress Save(DocumentAddress data)
        {
            return (DocumentAddress)base.Save(data);
        }


        public Boolean Update(DocumentAddress data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DocumentAddress data)
        {
            return base.Delete(data);
        }


        public DocumentAddress SelectById(DocumentAddress data)
        {
            return (DocumentAddress)base.SelectById(data);
        }


        public IList<DocumentAddress> Select(DocumentAddress data)
        {
                IList<DocumentAddress> datos = new List<DocumentAddress>();

                datos = GetHsql(data).List<DocumentAddress>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DocumentAddress a    where  ");
            DocumentAddress documentaddress = (DocumentAddress)data;
            if (documentaddress != null)
            {

                Parms = new List<Object[]>();
                if (documentaddress.RowID != 0)
                {
                    sql.Append(" a.RowID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", documentaddress.RowID });
                }

                Parms = new List<Object[]>();



                if (documentaddress.Document != null && documentaddress.Document.DocID != 0)
                {
                    sql.Append(" a.Document.DocID = :id     and   ");
                    Parms.Add(new Object[] { "id", documentaddress.Document.DocID });
                }

                //DocNumber
                if (documentaddress.Document != null &&  !string.IsNullOrEmpty(documentaddress.Document.DocNumber))
                {
                    sql.Append(" a.Document.DocNumber = :idn  and   ");
                    Parms.Add(new Object[] { "idn", documentaddress.Document.DocNumber });
                }

                //Document - Company
                if (documentaddress.Document != null && documentaddress.Document.Company != null && documentaddress.Document.Company.CompanyID != 0)
                {
                    sql.Append(" a.Document.Company.CompanyID = :idc    and   ");
                    Parms.Add(new Object[] { "idc", documentaddress.Document.Company.CompanyID });
                }


                if (documentaddress.DocumentLine != null )
                {
                    if (documentaddress.DocumentLine.LineID != 0)
                    {
                        sql.Append(" a.DocumentLine.LineID = :id3     and   ");
                        Parms.Add(new Object[] { "id3", documentaddress.DocumentLine.LineID });
                    }

                    if (documentaddress.DocumentLine.LineID < 0)
                    {
                        sql.Append(" a.DocumentLine.LineID IS NULL    and   ");
                    }


                    if (documentaddress.DocumentLine.LineNumber != 0)
                    {
                        sql.Append(" a.DocumentLine.LineNumber = :idn3     and   ");
                        Parms.Add(new Object[] { "idn3", documentaddress.DocumentLine.LineNumber });
                    }
                }

		        if (documentaddress.AddressType != 0)
                {
                    sql.Append(" a.AddressType = :id1     and   ");
                    Parms.Add(new Object[] { "id1", documentaddress.AddressType });
                }

                if (!String.IsNullOrEmpty(documentaddress.Name))
                {
                    sql.Append(" a.Name = :nm1    and   ");
                    Parms.Add(new Object[] { "nm1", documentaddress.Name });
                }


                if (!String.IsNullOrEmpty(documentaddress.AddressLine1))
                {
                    sql.Append(" a.AddressLine1 = :nom     and   ");
                    Parms.Add(new Object[] { "nom", documentaddress.AddressLine1 });
                }

                if (!String.IsNullOrEmpty(documentaddress.AddressLine2))
                {
                    sql.Append(" a.AddressLine2 = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", documentaddress.AddressLine2 });
                }

                if (!String.IsNullOrEmpty(documentaddress.AddressLine3))
                {
                    sql.Append(" a.AddressLine3 = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", documentaddress.AddressLine3 });
                }

                if (!String.IsNullOrEmpty(documentaddress.City))
                {
                    sql.Append(" a.City = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", documentaddress.City });
                }

                if (!String.IsNullOrEmpty(documentaddress.State))
                {
                    sql.Append(" a.State = :nom4     and   ");
                    Parms.Add(new Object[] { "nom4", documentaddress.State });
                }

                if (!String.IsNullOrEmpty(documentaddress.ZipCode))
                {
                    sql.Append(" a.ZipCode = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", documentaddress.ZipCode });
                }

                if (!String.IsNullOrEmpty(documentaddress.ContactPerson))
                {
                    sql.Append(" a.ContactPerson = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", documentaddress.ContactPerson });
                }

                if (!String.IsNullOrEmpty(documentaddress.Country))
                {
                    sql.Append(" a.Phone = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", documentaddress.Country });
                }

                if (documentaddress.ShpMethod != null && documentaddress.ShpMethod.ShpMethodID != 0)
                {
                    sql.Append(" a.DocumentLine.LineID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", documentaddress.DocumentLine.LineID });
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