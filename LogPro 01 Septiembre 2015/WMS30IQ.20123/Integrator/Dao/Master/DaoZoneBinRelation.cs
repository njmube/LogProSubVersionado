using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoZoneBinRelation: DaoService
    {
        public DaoZoneBinRelation(DaoFactory factory) : base(factory) { }

        public ZoneBinRelation Save(ZoneBinRelation data)
        {
            return (ZoneBinRelation)base.Save(data);
        }


        public Boolean Update(ZoneBinRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ZoneBinRelation data)
        {
            return base.Delete(data);
        }


        public ZoneBinRelation SelectById(ZoneBinRelation data)
        {
            return (ZoneBinRelation)base.SelectById(data);
        }


        public IList<ZoneBinRelation> Select(ZoneBinRelation data)
        {

            IList<ZoneBinRelation> datos = new List<ZoneBinRelation>();
            datos = GetHsql(data).List<ZoneBinRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ZoneBinRelation a    where  ");
            ZoneBinRelation zoneRel = (ZoneBinRelation)data;
            if ( zoneRel != null)
            {
                Parms = new List<Object[]>();
                if ( zoneRel.Zone != null && zoneRel.Zone.ZoneID != 0 )
                {
                    sql.Append(" a.Zone.ZoneID = :id     and   ");
                    Parms.Add(new Object[] { "id", zoneRel.Zone.ZoneID });
                }

                if (zoneRel.Bin != null && zoneRel.Bin.BinID != 0)
                {
                    sql.Append(" a.Bin.BinID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", zoneRel.Bin.BinID });
                }

                if (zoneRel.BinType != 0)
                {
                    sql.Append(" a.BinType = :id2     and   ");
                    Parms.Add(new Object[] { "id2", zoneRel.BinType });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Rank asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}