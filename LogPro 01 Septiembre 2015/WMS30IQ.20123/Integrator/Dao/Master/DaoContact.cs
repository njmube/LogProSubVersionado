using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoContact : DaoService
    {
        public DaoContact(DaoFactory factory) : base(factory) { }

        public Contact Save(Contact data)
        {
            return (Contact)base.Save(data);
        }


        public Boolean Update(Contact data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Contact data)
        {
            return base.Delete(data);
        }


        public Contact SelectById(Contact data)
        {
            return (Contact)base.SelectById(data);
        }


        public IList<Contact> Select(Contact data)
        {

                IList<Contact> datos = new List<Contact>();

                datos = GetHsql(data).List<Contact>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Contact a    where  ");
            Contact contact = (Contact)data;
            if (contact != null)
            {
                Parms = new List<Object[]>();
                if (contact.ContactID != 0)
                {
                    sql.Append(" a.ContactID = :id     and   ");
                    Parms.Add(new Object[] { "id", contact.ContactID });
                }

                if (contact.Account != null && contact.Account.AccountID != 0)
                {
                    sql.Append(" a.Account.AccountID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", contact.Account.AccountID });
                }

                if (!String.IsNullOrEmpty(contact.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", contact.Name });
                }

                if (!String.IsNullOrEmpty(contact.IdNumber))
                {
                    sql.Append(" a.IdNumber = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", contact.IdNumber });
                }

                if (!String.IsNullOrEmpty(contact.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", contact.ErpCode });
                }

                if (!String.IsNullOrEmpty(contact.Phone1))
                {
                    sql.Append(" a.Phone1 = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", contact.Phone1 });
                }

                if (!String.IsNullOrEmpty(contact.Phone2))
                {
                    sql.Append(" a.Phone2 = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", contact.Phone2 });
                }

                if (!String.IsNullOrEmpty(contact.Phone3))
                {
                    sql.Append(" a.Phone3 = :nom5     and   "); 
                    Parms.Add(new Object[] { "nom5", contact.Phone3 });
                }

                if (!String.IsNullOrEmpty(contact.Fax))
                {
                    sql.Append(" a.Fax = :nom6     and   "); 
                    Parms.Add(new Object[] { "nom6", contact.Fax });
                }

                if (!String.IsNullOrEmpty(contact.Email))
                {
                    sql.Append(" a.Email = :nom7     and   "); 
                    Parms.Add(new Object[] { "nom7", contact.Email });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.contactID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}