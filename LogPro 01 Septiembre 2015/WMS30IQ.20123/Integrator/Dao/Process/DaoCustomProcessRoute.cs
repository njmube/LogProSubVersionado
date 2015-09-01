using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;
using Entities.Process;

namespace Integrator.Dao.Process
{
    public class DaoCustomProcessRoute : DaoService
    {
        public DaoCustomProcessRoute(DaoFactory factory) : base(factory) { }

        public CustomProcessRoute Save(CustomProcessRoute data)
        {
            return (CustomProcessRoute)base.Save(data);
        }


        public Boolean Update(CustomProcessRoute data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcessRoute data)
        {
            return base.Delete(data);
        }


        public CustomProcessRoute SelectById(CustomProcessRoute data)
        {
            return (CustomProcessRoute)base.SelectById(data);
        }


        public IList<CustomProcessRoute> Select(CustomProcessRoute data)
        {

                IList<CustomProcessRoute> datos = new List<CustomProcessRoute>();

                datos = GetHsql(data).List<CustomProcessRoute>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcessRoute a    where  ");
            CustomProcessRoute CustomProcessRoute = (CustomProcessRoute)data;
            if (CustomProcessRoute != null)
            {
                Parms = new List<Object[]>();
                if (CustomProcessRoute.RouteID != 0)
                {
                    sql.Append(" a.RouteID = :id     and   ");
                    Parms.Add(new Object[] { "id", CustomProcessRoute.RouteID });
                }

                if (CustomProcessRoute.ProcessType != null && CustomProcessRoute.ProcessType.DocTypeID != 0)
                {
                    sql.Append(" a.ProcessType.DocTypeID = :ix2     and   ");
                    Parms.Add(new Object[] { "ix2", CustomProcessRoute.ProcessType.DocTypeID });
                }


                if (CustomProcessRoute.ProcessFrom != null && CustomProcessRoute.ProcessFrom.ProcessID != 0)
                {
                    sql.Append(" a.ProcessFrom.ProcessID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", CustomProcessRoute.ProcessFrom.ProcessID });
                }

                if (CustomProcessRoute.Status != null && CustomProcessRoute.ProcessTo.ProcessID != 0)
                {
                    sql.Append(" a.ProcessTo.ProcessID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", CustomProcessRoute.ProcessTo.ProcessID });
                }


                if (CustomProcessRoute.Status != null && CustomProcessRoute.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", CustomProcessRoute.Status.StatusID });
                }



            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}