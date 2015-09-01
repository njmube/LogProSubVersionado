using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoAccountTypeRelation : DaoService
    {
        public DaoAccountTypeRelation(DaoFactory factory) : base(factory) { }

        public AccountTypeRelation Save(AccountTypeRelation data)
        {
            return (AccountTypeRelation)base.Save(data);
        }


        public Boolean Update(AccountTypeRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(AccountTypeRelation data)
        {
            return base.Delete(data);
        }


        public AccountTypeRelation SelectById(AccountTypeRelation data)
        {
            return (AccountTypeRelation)base.SelectById(data);
        }


        public IList<AccountTypeRelation> Select(AccountTypeRelation data)
        {

                IList<AccountTypeRelation> datos = new List<AccountTypeRelation>();

                datos = GetHsql(data).List<AccountTypeRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from AccountTypeRelation a    where  ");
            AccountTypeRelation accounttyperelation = (AccountTypeRelation)data;
            if (accounttyperelation != null)
            {
                Parms = new List<Object[]>();
                if (accounttyperelation.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", accounttyperelation.RowID });
                }

                if (accounttyperelation.Account != null && accounttyperelation.Account.AccountID != 0)
                {
                    sql.Append(" a.Account.AccountID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", accounttyperelation.Account.AccountID });
                }

                if (accounttyperelation.AccountType != null && accounttyperelation.AccountType.AccountTypeID != 0)
                {
                    sql.Append(" a.AccountType.AccountTypeID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", accounttyperelation.AccountType.AccountTypeID });
                }

                if (!String.IsNullOrEmpty(accounttyperelation.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", accounttyperelation.ErpCode });
                }

                if (accounttyperelation.Status != null && accounttyperelation.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", accounttyperelation.Status.StatusID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public IList<Account> GetAccount(Account data, int accountType)
        {
            StringBuilder sql = new StringBuilder("select a.Account from AccountTypeRelation a  ");

            if (data != null)
            {
                Parms = new List<Object[]>();

                sql.Append(" Where a.AccountType.AccountTypeID = :id2 ");
                Parms.Add(new Object[] { "id2", accountType });

                sql.Append(" And a.Status.StatusID = :id3 ");
                Parms.Add(new Object[] { "id3", EntityStatus.Active });


                if (!String.IsNullOrEmpty(data.Name))
                {
                    sql.Append(" And ( a.Account.Name like :nom OR a.Account.AccountCode like :nom )");
                    Parms.Add(new Object[] { "nom", '%'+data.Name+'%' });
                }

                if (data.Company != null && data.Company.CompanyID != 0)
                {
                    sql.Append(" And a.Account.Company.CompanyID = :id4 ");
                    Parms.Add(new Object[] { "id4", data.Company.CompanyID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" order by a.Account.Name asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query.List<Account>();
        }



    }
}