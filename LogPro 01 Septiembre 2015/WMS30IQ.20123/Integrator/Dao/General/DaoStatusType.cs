using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoStatusType : DaoService
    {
        public DaoStatusType(DaoFactory factory) : base(factory) { }

        public StatusType Save(StatusType data)
        {
            return (StatusType)base.Save(data);
        }


        public Boolean Update(StatusType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(StatusType data)
        {
            return base.Delete(data);
        }


        public StatusType SelectById(StatusType data)
        {
            return (StatusType)base.SelectById(data);
        }


        public IList<StatusType> Select(StatusType data)
        {
            IList<StatusType> datos = new List<StatusType>();
            datos = GetHsql(data).List<StatusType>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from StatusType a    where  ");
            StatusType statustype = (StatusType)data;
            if (statustype != null)
            {
                Parms = new List<Object[]>();
                if (statustype.StatusTypeID != 0)
                {
                    sql.Append(" a.StatusTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", statustype.StatusTypeID });
                }

		        if (!String.IsNullOrEmpty(statustype.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", statustype.Name });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.statusTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}