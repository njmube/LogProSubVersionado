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
    public class DaoMessageRuleExtension : DaoService
    {
        public DaoMessageRuleExtension(DaoFactory factory) : base(factory){}

        public MessageRuleExtension Save(MessageRuleExtension data)
        {
            return (MessageRuleExtension)base.Save(data);
        }

        public Boolean Update(MessageRuleExtension data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MessageRuleExtension data)
        {
            return base.Delete(data);
        }

        public MessageRuleExtension SelectById(MessageRuleExtension data) 
        {
            return (MessageRuleExtension)base.SelectById(data);
        }

        public IList<MessageRuleExtension> Select(MessageRuleExtension data)
        {

                IList<MessageRuleExtension> datos = new List<MessageRuleExtension>();
                datos = GetHsql(data).List<MessageRuleExtension>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }

        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from MessageRuleExtension a  where  ");
            MessageRuleExtension MessageRuleExtension = (MessageRuleExtension)data;

            if (MessageRuleExtension != null)
            {
                Parms = new List<Object[]>();

                if (MessageRuleExtension.RowID != 0) 
                {
                    sql.Append(" a.RowID = :id     and    ");
                    Parms.Add(new Object[] { "id", MessageRuleExtension.RowID });
                }

                if (MessageRuleExtension.Rule != null && MessageRuleExtension.Rule.RowID != 0)
                {
                    sql.Append(" a.Rule.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", MessageRuleExtension.Rule.RowID });
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
