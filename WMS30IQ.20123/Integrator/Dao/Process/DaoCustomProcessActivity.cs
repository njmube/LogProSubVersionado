using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Process;

namespace Integrator.Dao.Process
{
    public class DaoCustomProcessActivity : DaoService
    {
        public DaoCustomProcessActivity(DaoFactory factory) : base(factory) { }

        public CustomProcessActivity Save(CustomProcessActivity data)
        {
            return (CustomProcessActivity)base.Save(data);
        }


        public Boolean Update(CustomProcessActivity data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcessActivity data)
        {
            return base.Delete(data);
        }


        public CustomProcessActivity SelectById(CustomProcessActivity data)
        {
            return (CustomProcessActivity)base.SelectById(data);
        }




        public IList<CustomProcessActivity> Select(CustomProcessActivity data)
        {
            IList<CustomProcessActivity> datos = new List<CustomProcessActivity>();
            datos = GetHsql(data).List<CustomProcessActivity>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcessActivity a where ");
            CustomProcessActivity customprocessactivity = (CustomProcessActivity)data;
            if (customprocessactivity != null)
            {
                Parms = new List<Object[]>();
                if (customprocessactivity.ActivityID != 0)
                {
                    sql.Append(" a.ActivityID = :id     and   ");
                    Parms.Add(new Object[] { "id", customprocessactivity.ActivityID });
                }

                if (!String.IsNullOrEmpty(customprocessactivity.Name))
                {
                    sql.Append(" a.Name like :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", "%" + customprocessactivity.Name + "%" });
                }

                if (customprocessactivity.Status != null && customprocessactivity.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :idd4     and   ");
                    Parms.Add(new Object[] { "idd4", customprocessactivity.Status.StatusID });
                }

                if (!String.IsNullOrEmpty(customprocessactivity.Method))
                {
                    sql.Append(" a.Method = :nom5  and  ");
                    Parms.Add(new Object[] { "nom5", customprocessactivity.Method });
                }

                if (customprocessactivity.ProcessType != null && customprocessactivity.ProcessType.DocTypeID != 0)
                {
                    sql.Append(" a.ProcessType.DocTypeID = :ix2     and   ");
                    Parms.Add(new Object[] { "ix2", customprocessactivity.ProcessType.DocTypeID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ActivityID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}