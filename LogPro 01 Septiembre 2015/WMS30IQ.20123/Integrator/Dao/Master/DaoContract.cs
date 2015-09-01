using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoContract : DaoService
    {
        public DaoContract(DaoFactory factory) : base(factory) { }

        public Contract Save(Contract data)
        {
            return (Contract)base.Save(data);
        }


        public Boolean Update(Contract data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Contract data)
        {
            return base.Delete(data);
        }


        public Contract SelectById(Contract data)
        {
            return (Contract)base.SelectById(data);
        }


        public IList<Contract> Select(Contract data)
        {

                IList<Contract> datos = new List<Contract>();
                datos = GetHsql(data).List<Contract>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Contract a    where  ");
            Contract Contract = (Contract)data;
            if (Contract != null)
            {
                Parms = new List<Object[]>();
                if (Contract.ContractID != 0)
                {
                    sql.Append(" a.ContractID = :id     and   ");
                    Parms.Add(new Object[] { "id", Contract.ContractID });
                }

                if (Contract.Account != null && Contract.Account.AccountID != 0)
                {
                    sql.Append(" a.Account.AccountID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", Contract.Account.AccountID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.ContractID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}