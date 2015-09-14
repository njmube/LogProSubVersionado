using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoZonePickerRelation: DaoService
    {
        public DaoZonePickerRelation(DaoFactory factory) : base(factory) { }

        public ZonePickerRelation Save(ZonePickerRelation data)
        {
            return (ZonePickerRelation)base.Save(data);
        }


        public Boolean Update(ZonePickerRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ZonePickerRelation data)
        {
            return base.Delete(data);
        }


        public ZonePickerRelation SelectById(ZonePickerRelation data)
        {
            return (ZonePickerRelation)base.SelectById(data);
        }


        public IList<ZonePickerRelation> Select(ZonePickerRelation data)
        {

            IList<ZonePickerRelation> datos = new List<ZonePickerRelation>();
            datos = GetHsql(data).List<ZonePickerRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ZonePickerRelation a    where  ");
            ZonePickerRelation zoneRel = (ZonePickerRelation)data;
            if ( zoneRel != null)
            {
                Parms = new List<Object[]>();
                if ( zoneRel.Zone != null && zoneRel.Zone.ZoneID != 0 )
                {
                    sql.Append(" a.Zone.ZoneID = :id     and   ");
                    Parms.Add(new Object[] { "id", zoneRel.Zone.ZoneID });
                }

                if (zoneRel.Picker != null && zoneRel.Picker.UserID != 0)
                {
                    sql.Append(" a.Picker.UserID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", zoneRel.Picker.UserID });
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