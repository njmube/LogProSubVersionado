using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.General
{
    public class DaoMessagePool : DaoService
    {
        public DaoMessagePool(DaoFactory factory) : base(factory) { }

        public MessagePool Save(MessagePool data)
        {
            return (MessagePool)base.Save(data);
        }


        public Boolean Update(MessagePool data)
        {
            return base.Update(data);
        }


        public Boolean Delete(MessagePool data)
        {
            return base.Delete(data);
        }


        public MessagePool SelectById(MessagePool data)
        {
            return (MessagePool)base.SelectById(data);
        }


        public IList<MessagePool> Select(MessagePool data)
        {
  
                IList<MessagePool> datos = new List<MessagePool>();
                datos = GetHsql(data).List<MessagePool>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MessagePool a  where  ");
            MessagePool repdoc = (MessagePool)data;
            if (repdoc != null)
            {
                Parms = new List<Object[]>();
                if (repdoc.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", repdoc.RowID });
                }

                if (repdoc.Rule != null && repdoc.Rule.RowID != 0)
                {
                    sql.Append(" a.Rule.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", repdoc.Rule.RowID });
                }

                if (repdoc.AlreadySent == true)
                {
                    sql.Append(" a.AlreadySent = :id2     and   ");
                    Parms.Add(new Object[] { "id2", true });
                }
                else if (repdoc.AlreadySent == false)
                {
                    sql.Append(" a.AlreadySent = :id2     and   ");
                    Parms.Add(new Object[] { "id2", false });
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