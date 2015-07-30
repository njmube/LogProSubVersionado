using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoCountSchedule : DaoService
    {
        public DaoCountSchedule(DaoFactory factory) : base(factory) { }

        public CountSchedule Save(CountSchedule data)
        {
            return (CountSchedule)base.Save(data);
        }


        public Boolean Update(CountSchedule data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CountSchedule data)
        {
            return base.Delete(data);
        }


        public CountSchedule SelectById(CountSchedule data)
        {
            return (CountSchedule)base.SelectById(data);
        }


        public IList<CountSchedule> Select(CountSchedule data)
        {
            IList<CountSchedule> datos = new List<CountSchedule>();

            try {
                datos = GetHsql(data).List<CountSchedule>();
            if (!Factory.IsTransactional)
                Factory.Commit();


                            }
                catch (Exception e)
                {
                    NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
                }


            return datos;
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CountSchedule a    where  ");
            CountSchedule countSchedule = (CountSchedule)data;
            if (countSchedule != null)
            {
                Parms = new List<Object[]>();
                if (countSchedule.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", countSchedule.RowID });
                }

                if (countSchedule.RepeatEach != 0)
                {
                    sql.Append(" a.RepeatEach = :id1     and   ");
                    Parms.Add(new Object[] { "id1", countSchedule.RepeatEach });
                }

                if (countSchedule.Start != null)
                {
                    sql.Append(" a.Start = :id2     and   ");
                    Parms.Add(new Object[] { "id2", countSchedule.Start });
                }

                if (countSchedule.Finish != null)
                {
                    sql.Append(" a.Finish = :id3     and   ");
                    Parms.Add(new Object[] { "id3", countSchedule.Finish });
                }

                if (countSchedule.NextDateRun != null)
                {
                    sql.Append(" a.NextDateRun = :id4     and   ");
                    Parms.Add(new Object[] { "id4", countSchedule.NextDateRun });
                }

                if (countSchedule.IsDone != null)
                {
                    sql.Append(" a.IsDone = :id5     and   ");
                    Parms.Add(new Object[] { "id5", countSchedule.IsDone });
                }

                //if (!string.IsNullOrEmpty(countSchedule.Title))
                //{
                //    sql.Append(" a.Title like :id6  and   ");
                //    Parms.Add(new Object[] { "id6", "%" + countSchedule.Title + "%" });
                //}

                if (countSchedule.Location != null && countSchedule.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :loc  and   ");
                    Parms.Add(new Object[] { "loc", countSchedule.Location.LocationID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.NextDateRun asc");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}