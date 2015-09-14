using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoAccount : DaoService
    {
        public DaoAccount(DaoFactory factory) : base(factory) { }

        public Account Save(Account data)
        {
            return (Account)base.Save(data);
        }


        public Boolean Update(Account data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Account data)
        {
            return base.Delete(data);
        }


        public Account SelectById(Account data)
        {
            return (Account)base.SelectById(data);
        }


        public IList<Account> Select(Account data)
        {

                IList<Account> datos = new List<Account>();
                datos = GetHsql(data).SetMaxResults(WmsSetupValues.NumRegs).List<Account>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Account a    where  ");
            Account account = (Account)data;
            if (account != null)
            {
                Parms = new List<Object[]>();
                if (account.AccountID != 0)
                {
                    sql.Append(" a.AccountID = :id     and   ");
                    Parms.Add(new Object[] { "id", account.AccountID });
                }

                if (account.Company != null && account.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", account.Company.CompanyID });
                }



                if (account.FatherContract != null && account.FatherContract.ContractID != 0)
                {
                    sql.Append(" a.FatherContract.ContractID = :icd1     and   ");
                    Parms.Add(new Object[] { "icd1", account.FatherContract.ContractID });
                }



                if (account.BaseType != null && account.BaseType.AccountTypeID != 0)
                {
                    sql.Append(" a.BaseType.AccountTypeID = :idt1     and   ");
                    Parms.Add(new Object[] { "idt1", account.BaseType.AccountTypeID });
                }


                if (!String.IsNullOrEmpty(account.Name))
                {
                    sql.Append(" (a.AccountCode = :ncx ");
                    Parms.Add(new Object[] { "ncx", account.Name });

                    if (account.Name.Length > 3)
                    {
                        sql.Append(" Or a.Name  like :nom ) and ");
                        Parms.Add(new Object[] { "nom", "%" + account.Name + "%" });
                    }
                    else
                        sql.Append(" ) and ");  
                    
                }


                if (!String.IsNullOrEmpty(account.AccountCode))
                {
                    sql.Append(" a.AccountCode = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", account.AccountCode });
                }

                if (!String.IsNullOrEmpty(account.Phone))
                {
                    sql.Append(" a.Phone = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", account.Phone });
                }

                if (!String.IsNullOrEmpty(account.ContactPerson))
                {
                    sql.Append(" a.ContactPerson = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", account.ContactPerson });
                }

                if (!String.IsNullOrEmpty(account.Email))
                {
                    sql.Append(" a.Email = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", account.Email });
                }

                if (!String.IsNullOrEmpty(account.WebSite))
                {
                    sql.Append(" a.WebSite = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", account.WebSite });
                }


                if (account.UserDefine1 != null)
                {
                    sql.Append(" a.UserDefine1 = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", account.UserDefine1 });  
                }
                
                if (account.UserDefine2 != null)
                {
                    sql.Append(" a.UserDefine2 = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", account.UserDefine2 });
                }

                if (account.UserDefine3 != null)
                {
                    sql.Append(" a.UserDefine3 = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", account.UserDefine3 });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.AccountID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}