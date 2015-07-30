using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoAccountType : DaoService
    {
        public DaoAccountType(DaoFactory factory) : base(factory) { }

        public AccountType Save(AccountType data)
        {
            return (AccountType)base.Save(data);
        }


        public Boolean Update(AccountType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(AccountType data)
        {
            return base.Delete(data);
        }


        public AccountType SelectById(AccountType data)
        {
            return (AccountType)base.SelectById(data);
        }


        public IList<AccountType> Select(AccountType data)
        {
  
                IList<AccountType> datos = new List<AccountType>();
                datos = GetHsql(data).List<AccountType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from AccountType a    where  ");
            AccountType accounttype = (AccountType)data;
            if (accounttype != null)
            {
                Parms = new List<Object[]>();
                if (accounttype.AccountTypeID != 0)
                {
                    sql.Append(" a.AccountTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", accounttype.AccountTypeID });
                }

                if (!String.IsNullOrEmpty(accounttype.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", accounttype.Name });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.AccountTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}