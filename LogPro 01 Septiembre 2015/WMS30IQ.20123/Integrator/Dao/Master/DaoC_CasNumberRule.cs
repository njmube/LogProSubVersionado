using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoC_CasNumberRule : DaoService
    {
        public DaoC_CasNumberRule(DaoFactory factory) : base(factory) { }

        public C_CasNumberRule Save(C_CasNumberRule data)
        {
            return (C_CasNumberRule)base.Save(data);
        }


        public Boolean Update(C_CasNumberRule data)
        {
            return base.Update(data);
        }


        public Boolean Delete(C_CasNumberRule data)
        {
            return base.Delete(data);
        }


        public C_CasNumberRule SelectById(C_CasNumberRule data)
        {
            return (C_CasNumberRule)base.SelectById(data);
        }


        public IList<C_CasNumberRule> Select(C_CasNumberRule data)
        {

                IList<C_CasNumberRule> datos = new List<C_CasNumberRule>();

                datos = GetHsql(data).List<C_CasNumberRule>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from C_CasNumberRule a    where  ");

            C_CasNumberRule C_CasNumberRule = (C_CasNumberRule)data;

            if (C_CasNumberRule != null)
            {
                Parms = new List<Object[]>();
                if (C_CasNumberRule.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", C_CasNumberRule.RowID });
                }

                if (C_CasNumberRule.CasNumber != null && C_CasNumberRule.CasNumber.CasNumberID != 0)
                {
                    sql.Append(" a.CasNumber.CasNumberID = :idp1     and   ");
                    Parms.Add(new Object[] { "idp1", C_CasNumberRule.CasNumber.CasNumberID });
                }

                if (C_CasNumberRule.Rule != null )
                {
                    if (C_CasNumberRule.Rule.MetaMasterID != 0)
                    {
                        sql.Append(" a.Rule.MetaMasterID = :idc1     and   ");
                        Parms.Add(new Object[] { "idc1", C_CasNumberRule.Rule.MetaMasterID });
                    }

                    if (C_CasNumberRule.Rule.MetaType != null)
                    {
                        if (C_CasNumberRule.Rule.MetaType.MetaTypeID != 0)
                        {
                            sql.Append(" a.Rule.MetaType.MetaTypeID = :itt1     and   ");
                            Parms.Add(new Object[] { "itt1", C_CasNumberRule.Rule.MetaType.MetaTypeID });
                        }

                        if (!string.IsNullOrEmpty(C_CasNumberRule.Rule.MetaType.Code))
                        {
                            sql.Append(" a.Rule.MetaType.Code= :itc1     and   ");
                            Parms.Add(new Object[] { "itc1", C_CasNumberRule.Rule.MetaType.Code });
                        }

                    }
                }

                if (! string.IsNullOrEmpty(C_CasNumberRule.RuleValue))
                {
                    sql.Append(" a.RuleValue = :idz1     and   ");
                    Parms.Add(new Object[] { "idz1", C_CasNumberRule.RuleValue });
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