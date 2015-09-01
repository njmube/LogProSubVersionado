using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.Report
{
    public class DaoMessageRuleByCompany : DaoService
    {
        public DaoMessageRuleByCompany(DaoFactory factory) : base(factory) { }

        public MessageRuleByCompany Save(MessageRuleByCompany data)
        {
            return (MessageRuleByCompany)base.Save(data);
        }


        public Boolean Update(MessageRuleByCompany data)
        {
            return base.Update(data);
        }


        public Boolean Delete(MessageRuleByCompany data)
        {
            return base.Delete(data);
        }


        public MessageRuleByCompany SelectById(MessageRuleByCompany data)
        {
            return (MessageRuleByCompany)base.SelectById(data);
        }

        public IList<MessageRuleByCompany> Select(MessageRuleByCompany data)
        {
                IList<MessageRuleByCompany> datos = new List<MessageRuleByCompany>();
                datos = GetHsql(data).List<MessageRuleByCompany>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;            
        }


        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from MessageRuleByCompany a  where  ");
            MessageRuleByCompany option = (MessageRuleByCompany)data;
            
            if (option != null)
            {
                Parms = new List<Object[]>();

                if (option.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and    ");
                    Parms.Add(new Object[] { "id", option.RowID });
                }

                if (option.Template != null && option.Template.RowID != 0)
                {
                    sql.Append("a.Template.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", option.Template.RowID });
                }

                
                if (option.Company != null  && option.Company.CompanyID != 0)
                {
                    sql.Append("a.Company.CompanyID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", option.Company.CompanyID });
                }

                if (option.Entity != null && option.Entity.ClassEntityID != 0)
                {
                    sql.Append("a.Entity.ClassEntityID = :id12     and   ");
                    Parms.Add(new Object[] { "id12", option.Entity.ClassEntityID });
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
