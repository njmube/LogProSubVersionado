using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Process;
using System.Linq;

namespace Integrator.Dao.Process
{
    public class DaoCustomProcessTransitionByEntity : DaoService
    {
        public DaoCustomProcessTransitionByEntity(DaoFactory factory) : base(factory) { }

        public CustomProcessTransitionByEntity Save(CustomProcessTransitionByEntity data)
        {
            return (CustomProcessTransitionByEntity)base.Save(data);
        }


        public Boolean Update(CustomProcessTransitionByEntity data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcessTransitionByEntity data)
        {
            return base.Delete(data);
        }


        public CustomProcessTransitionByEntity SelectById(CustomProcessTransitionByEntity data)
        {
            return (CustomProcessTransitionByEntity)base.SelectById(data);
        }


        public IList<CustomProcessTransitionByEntity> Select(CustomProcessTransitionByEntity data)
        {
            IList<CustomProcessTransitionByEntity> datos = new List<CustomProcessTransitionByEntity>();
            datos = GetHsql(data).List<CustomProcessTransitionByEntity>();
            if (!Factory.IsTransactional)
                Factory.Commit();

            try { datos = datos.Where(f => f.GetType() == typeof(CustomProcessTransitionByEntity)).ToList(); }
            catch { }
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcessEntityTransition a where ");
            CustomProcessTransitionByEntity transition = (CustomProcessTransitionByEntity)data;
            if (transition != null)
            {
                Parms = new List<Object[]>();
                if (transition.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", transition.RowID });
                }

                if (!String.IsNullOrEmpty(transition.Name))
                {
                    sql.Append(" a.Name = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", transition.Name });
                }

                if (transition.CurrentActivity != null && transition.CurrentActivity.ActivityID != 0)
                {
                    sql.Append(" a.CurrentActivity.ActivityID = :idd3     and   ");
                    Parms.Add(new Object[] { "idd3", transition.CurrentActivity.ActivityID });
                }


                if (transition.ResultContextKey != null && !string.IsNullOrEmpty(transition.ResultContextKey.ContextKey))
                {
                    sql.Append(" a..ResultContextKey.ContextKey = :ick3     and   ");
                    Parms.Add(new Object[] { "ick3", transition.ResultContextKey.ContextKey });
                }

                if (!String.IsNullOrEmpty(transition.ResultValue))
                {
                    sql.Append(" a.ResultValue = :nom5  and  ");
                    Parms.Add(new Object[] { "nom5", transition.ResultValue });
                }

                if (transition.NextActivity != null && transition.NextActivity.ActivityID != 0)
                {
                    sql.Append(" a.NextActivity.ActivityID = :idd6     and   ");
                    Parms.Add(new Object[] { "idd6", transition.NextActivity.ActivityID });
                }

                if (transition.Status != null && transition.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :idd7     and   ");
                    Parms.Add(new Object[] { "idd7", transition.Status.StatusID });
                }


                if (transition.Entity != null && transition.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :ie2     and   ");
                    Parms.Add(new Object[] { "ie2", transition.Entity.ClassEntityID });
                }

                if (transition.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :ir2     and   ");
                    Parms.Add(new Object[] { "ir2", transition.EntityRowID });
                }


                if (transition.Process != null && transition.Process.ProcessID != 0)
                {
                    sql.Append(" a.Process.ProcessID = :ip7     and   ");
                    Parms.Add(new Object[] { "ip7", transition.Process.ProcessID });

                    if (transition.Process.ProcessType != null && transition.Process.ProcessType.DocTypeID != 0)
                    {
                        sql.Append(" a.Process.ProcessType.DocTypeID = :id7     and   ");
                        Parms.Add(new Object[] { "id7", transition.Process.ProcessType.DocTypeID });
                    }

                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.Process.Name, a.Sequence ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
    }
}