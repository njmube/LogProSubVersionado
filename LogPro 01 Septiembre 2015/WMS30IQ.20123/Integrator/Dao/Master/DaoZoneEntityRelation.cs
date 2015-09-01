using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoZoneEntityRelation : DaoService
    {
        public DaoZoneEntityRelation(DaoFactory factory) : base(factory) { }

        public ZoneEntityRelation Save(ZoneEntityRelation data)
        {
            return (ZoneEntityRelation)base.Save(data);
        }


        public Boolean Update(ZoneEntityRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ZoneEntityRelation data)
        {
            return base.Delete(data);
        }


        public ZoneEntityRelation SelectById(ZoneEntityRelation data)
        {
            return (ZoneEntityRelation)base.SelectById(data);
        }


        public IList<ZoneEntityRelation> Select(ZoneEntityRelation data)
        {

            IList<ZoneEntityRelation> datos = new List<ZoneEntityRelation>();
            datos = GetHsql(data).List<ZoneEntityRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ZoneEntityRelation a    where  ");
            ZoneEntityRelation zoneRel = (ZoneEntityRelation)data;
            if ( zoneRel != null)
            {
                Parms = new List<Object[]>();
                if ( zoneRel.Zone != null && zoneRel.Zone.ZoneID != 0 )
                {
                    sql.Append(" a.Zone.ZoneID = :id     and   ");
                    Parms.Add(new Object[] { "id", zoneRel.Zone.ZoneID });
                }

                if (zoneRel.Entity  != null && zoneRel.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", zoneRel.Entity.ClassEntityID });
                }

                if (zoneRel.EntityRowID!= 0)
                {
                    sql.Append(" a.EntityRowID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", zoneRel.EntityRowID });
                }


                if (zoneRel.Forced != null)
                {
                    sql.Append(" a.Forced = :idf     and   ");
                    Parms.Add(new Object[] { "idf", zoneRel.Forced });
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
