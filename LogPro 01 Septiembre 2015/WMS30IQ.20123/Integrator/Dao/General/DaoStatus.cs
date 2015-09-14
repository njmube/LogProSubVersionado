using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoStatus : DaoService
    {
        public DaoStatus(DaoFactory factory) : base(factory) { }

        public Status Save(Status data)
        {
            return (Status)base.Save(data);
        }


        public Boolean Update(Status data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Status data)
        {
            return base.Delete(data);
        }


        public Status SelectById(Status data)
        {
            return (Status)base.SelectById(data);
        }


        public IList<Status> Select(Status data)
        {

                IList<Status> datos = new List<Status>();
                datos = GetHsql(data).List<Status>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Status a    where  ");
            Status status = (Status)data;
            if (status != null)
            {
                Parms = new List<Object[]>();
                if (status.StatusID != 0)
                {
                    sql.Append(" a.StatusID = :id     and   ");
                    Parms.Add(new Object[] { "id", status.StatusID });
                }

                if (status.StatusType != null && status.StatusType.StatusTypeID != 0)
                {
                    sql.Append(" a.StatusType.StatusTypeID  = :id1     and   ");
                    Parms.Add(new Object[] { "id1", status.StatusType.StatusTypeID });
                }

                if (!String.IsNullOrEmpty(status.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", status.Name });
                }

                if (!String.IsNullOrEmpty(status.Description))
                {
                    sql.Append(" a.Description = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", status.Description });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.StatusID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}