using System;
using System.Collections.Generic;
using System.Text;
using Integrator.Dao;
using NHibernate;
using Integrator.Config;
using Entities.Workflow;

namespace Integrator.Dao.Workflow
{
    public class DaoBinRoute  : DaoService
    {
        public DaoBinRoute (DaoFactory factory) : base(factory) { }

        public BinRoute Save(BinRoute data)
        {
            return (BinRoute)base.Save(data);
        }

        public Boolean Update(BinRoute data)
        {
            return base.Update(data);
        }

        public Boolean Delete(BinRoute data)
        {
            return base.Delete(data);
        }

        public BinRoute SelectById(BinRoute data)
        {
            return (BinRoute)base.SelectById(data);
        }

        public IList<BinRoute> Select(BinRoute data)
        {
            IList<BinRoute> datos = new List<BinRoute>();
            try {
                datos = GetHsql(data).List<BinRoute>();
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
            StringBuilder sql = new StringBuilder("select a from BinRoute  a where ");
            BinRoute BinRoute = (BinRoute)data;
            if (BinRoute != null)
            {
                Parms = new List<Object[]>();
                if (BinRoute.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", BinRoute.RowID });
                }

                if (BinRoute.LocationFrom != null && BinRoute.LocationFrom.LocationID != 0)
                {
                    sql.Append(" a.LocationFrom.LocationID = :idd2     and   ");
                    Parms.Add(new Object[] { "idd2", BinRoute.LocationFrom.LocationID });
                }

                if (BinRoute.BinFrom != null && BinRoute.BinFrom.BinID != 0)
                {
                    sql.Append(" a.BinFrom.BinID = :idd3     and   ");
                    Parms.Add(new Object[] { "idd3", BinRoute.BinFrom.BinID });
                }

                if (BinRoute.LocationTo != null && BinRoute.LocationTo.LocationID != 0)
                {
                    sql.Append(" a.LocationTo.LocationID = :idd4     and   ");
                    Parms.Add(new Object[] { "idd4", BinRoute.LocationTo.LocationID });
                }

                if (BinRoute.BinTo != null && BinRoute.BinTo.BinID != 0)
                {
                    sql.Append(" a.BinTo.BinID = :idd5     and   ");
                    Parms.Add(new Object[] { "idd5", BinRoute.BinTo.BinID });
                }

                if (BinRoute.RequireData != null)
                {
                    sql.Append(" a.RequireData = :idd6     and   ");
                    Parms.Add(new Object[] { "idd6", BinRoute.RequireData });
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