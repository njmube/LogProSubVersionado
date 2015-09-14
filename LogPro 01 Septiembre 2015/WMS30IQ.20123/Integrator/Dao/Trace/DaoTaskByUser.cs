using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoTaskByUser : DaoService
    {
        public DaoTaskByUser(DaoFactory factory) : base(factory) { }

        public TaskByUser Save(TaskByUser data)
        {
            return (TaskByUser)base.Save(data);
        }


        public Boolean Update(TaskByUser data)
        {
            return base.Update(data);
        }


        public Boolean Delete(TaskByUser data)
        {
            return base.Delete(data);
        }


        public TaskByUser SelectById(TaskByUser data)
        {
            return (TaskByUser)base.SelectById(data);
        }


        public IList<TaskByUser> Select(TaskByUser data)
        {
            IList<TaskByUser> datos = new List<TaskByUser>();

            try {
            datos = GetHsql(data).List<TaskByUser>();
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
            StringBuilder sql = new StringBuilder("select a from TaskByUser a    where  ");
            TaskByUser taskUser = (TaskByUser)data;
            if (taskUser != null)
            {
                Parms = new List<Object[]>();
                if (taskUser.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", taskUser.RowID });
                }


                if (taskUser.User != null && taskUser.User.UserID != 0)
                {
                    sql.Append(" a.User.UserID = :idd1  and  ");
                    Parms.Add(new Object[] { "idd1", taskUser.User.UserID });
                }

                if (taskUser.User != null && !String.IsNullOrEmpty(taskUser.User.UserName))
                {
                    sql.Append(" a.User.UserName = :idd2  and  ");
                    Parms.Add(new Object[] { "idd2", taskUser.User.UserName });
                }

                if (taskUser.TaskDocument != null && taskUser.TaskDocument.DocID != 0)
                {
                    sql.Append(" a.TaskDocument.DocID = :idd3     and   ");
                    Parms.Add(new Object[] { "idd3", taskUser.TaskDocument.DocID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.TaskDocument.Priority asc, a.CreationDate asc");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}