using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoPickMethod : DaoService
    {
        public DaoPickMethod(DaoFactory factory) : base(factory) { }

        public PickMethod Save(PickMethod data)
        {
            return (PickMethod)base.Save(data);
        }


        public Boolean Update(PickMethod data)
        {
            return base.Update(data);
        }


        public Boolean Delete(PickMethod data)
        {
            return base.Delete(data);
        }


        public PickMethod SelectById(PickMethod data)
        {
            return (PickMethod)base.SelectById(data);
        }


        public IList<PickMethod> Select(PickMethod data)
        {
            IList<PickMethod> datos = new List<PickMethod>();
            datos = GetHsql(data).List<PickMethod>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from PickMethod a    where  ");
            PickMethod PickMethod = (PickMethod)data;
            if (PickMethod != null)
            {
                Parms = new List<Object[]>();
                if (PickMethod.MethodID != 0)
                {
                    sql.Append(" a.MethodID = :id     and   ");
                    Parms.Add(new Object[] { "id", PickMethod.MethodID });
                }

		        if (!String.IsNullOrEmpty(PickMethod.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", PickMethod.Name });
                }

                if (PickMethod.Active != null)
                {
                    sql.Append(" a.Active = :act and ");
                    Parms.Add(new Object[] { "act", PickMethod.Active });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.MethodID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}