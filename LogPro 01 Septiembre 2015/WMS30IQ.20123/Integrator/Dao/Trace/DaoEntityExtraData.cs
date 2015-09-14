using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;
using Entities.Trace;

namespace Integrator.Dao.Master
{
    public class DaoEntityExtraData : DaoService
    {
        public DaoEntityExtraData(DaoFactory factory) : base(factory) { }

        public EntityExtraData Save(EntityExtraData data)
        {
            return (EntityExtraData)base.Save(data);
        }


        public Boolean Update(EntityExtraData data)
        {
            return base.Update(data);
        }


        public Boolean Delete(EntityExtraData data)
        {
            return base.Delete(data);
        }


        public EntityExtraData SelectById(EntityExtraData data)
        {
            return (EntityExtraData)base.SelectById(data);
        }


        public IList<EntityExtraData> Select(EntityExtraData data)
        {

            IList<EntityExtraData> datos = new List<EntityExtraData>();
            datos = GetHsql(data).List<EntityExtraData>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from EntityExtraData a    where  ");
            EntityExtraData imgRel = (EntityExtraData)data;
            if ( imgRel != null)
            {
                Parms = new List<Object[]>();


                if (imgRel.Entity != null && imgRel.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", imgRel.Entity.ClassEntityID });
                }

                if (imgRel.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", imgRel.EntityRowID });
                }


                if (imgRel.RowID != 0)
                {
                    sql.Append(" a.RowID = :idr2     and   ");
                    Parms.Add(new Object[] { "id2r", imgRel.RowID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}
