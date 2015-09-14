using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoAccountAddress : DaoService
    {
        public DaoAccountAddress(DaoFactory factory) : base(factory) { }

        public AccountAddress Save(AccountAddress data)
        {
            return (AccountAddress)base.Save(data);
        }


        public Boolean Update(AccountAddress data)
        {
            return base.Update(data);
        }


        public Boolean Delete(AccountAddress data)
        {
            return base.Delete(data);
        }


        public AccountAddress SelectById(AccountAddress data)
        {
            return (AccountAddress)base.SelectById(data);
        }


        public IList<AccountAddress> Select(AccountAddress data)
        {

                IList<AccountAddress> datos = new List<AccountAddress>();
                datos = GetHsql(data).List<AccountAddress>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from AccountAddress a    where  ");

            AccountAddress accountaddress = (AccountAddress)data;

            if (accountaddress != null)
            {
                Parms = new List<Object[]>();
                if (accountaddress.AddressID != 0)
                {
                    sql.Append(" a.AddressID = :id     and   ");
                    Parms.Add(new Object[] { "id", accountaddress.AddressID });
                }

                if (accountaddress.Account != null)
                {
                    if (accountaddress.Account.AccountID != 0)
                    {
                        sql.Append(" a.Account.AccountID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", accountaddress.Account.AccountID });
                    }

                    if (!string.IsNullOrEmpty(accountaddress.Account.AccountCode))
                    {
                        sql.Append(" a.Account.AccountCode = :iac     and   ");
                        Parms.Add(new Object[] { "iac", accountaddress.Account.AccountCode });
                    }

                    if (accountaddress.Account.BaseType != null && accountaddress.Account.BaseType.AccountTypeID != 0)
                    {
                        sql.Append(" a.Account.BaseType.AccountTypeID = :iatc     and   ");
                        Parms.Add(new Object[] { "iatc", accountaddress.Account.BaseType.AccountTypeID });
                    }
                }


                if (!String.IsNullOrEmpty(accountaddress.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", accountaddress.Name });
                }

                if (!String.IsNullOrEmpty(accountaddress.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", accountaddress.ErpCode });
                }

                if (accountaddress.IsMain != null)
                {
                    sql.Append(" upper(a.IsMain) like :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", accountaddress.IsMain + "%" });
                }

                /*
                if (!String.IsNullOrEmpty(accountaddress.AddressLine1))
                {
                    sql.Append(" a.AddressLine1 = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", accountaddress.AddressLine1 });
                }

                if (!String.IsNullOrEmpty(accountaddress.AddressLine2))
                {
                    sql.Append(" a.AddressLine2 = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", accountaddress.AddressLine2 });
                }

                if (!String.IsNullOrEmpty(accountaddress.AddressLine3))
                {
                    sql.Append(" a.AddressLine3 = :nom5     and   "); 
                    Parms.Add(new Object[] { "nom5", accountaddress.AddressLine3 });
                }
                 */

                if (!String.IsNullOrEmpty(accountaddress.City))
                {
                    sql.Append(" a.City = :nom6     and   "); 
                    Parms.Add(new Object[] { "nom6", accountaddress.City });
                }

                if (!String.IsNullOrEmpty(accountaddress.State))
                {
                    sql.Append(" a.State = :nom6     and   "); 
                    Parms.Add(new Object[] { "nom6", accountaddress.State });
                }

                if (!String.IsNullOrEmpty(accountaddress.ZipCode))
                {
                    sql.Append(" a.ZipCode = :nom7     and   "); 
                    Parms.Add(new Object[] { "nom7", accountaddress.ZipCode });
                }

                if (!String.IsNullOrEmpty(accountaddress.Country))
                {
                    sql.Append(" a.Country = :nom7     and   "); 
                    Parms.Add(new Object[] { "nom7", accountaddress.Country });
                }

                /*
                if (!String.IsNullOrEmpty(accountaddress.Phone1))
                {
                    sql.Append(" a.Phone1 = :nom8     and   "); 
                    Parms.Add(new Object[] { "nom8", accountaddress.Phone1 });
                }

                if (!String.IsNullOrEmpty(accountaddress.Phone2))
                {
                    sql.Append(" a.Phone2 = :nom9     and   "); 
                    Parms.Add(new Object[] { "nom9", accountaddress.Phone2 });
                }

                if (!String.IsNullOrEmpty(accountaddress.Phone3))
                {
                    sql.Append(" a.Phone3 = :nom10     and   "); 
                    Parms.Add(new Object[] { "nom10", accountaddress.Phone3 });
                }

                 
                if (!String.IsNullOrEmpty(accountaddress.Email))
                {
                    sql.Append(" a.Email = :nom11     and   "); 
                    Parms.Add(new Object[] { "nom11", accountaddress.Email });
                }
                */

                if (accountaddress.Status != null && accountaddress.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", accountaddress.Status.StatusID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.AddressID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}