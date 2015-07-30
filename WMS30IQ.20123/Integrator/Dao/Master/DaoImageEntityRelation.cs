using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoImageEntityRelation : DaoService
    {
        public DaoImageEntityRelation(DaoFactory factory) : base(factory) { }

        public ImageEntityRelation Save(ImageEntityRelation data)
        {
            return (ImageEntityRelation)base.Save(data);
        }


        public Boolean Update(ImageEntityRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ImageEntityRelation data)
        {
            return base.Delete(data);
        }


        public ImageEntityRelation SelectById(ImageEntityRelation data)
        {
            return (ImageEntityRelation)base.SelectById(data);
        }


        public IList<ImageEntityRelation> Select(ImageEntityRelation data)
        {

            IList<ImageEntityRelation> datos = new List<ImageEntityRelation>();
            datos = GetHsql(data).List<ImageEntityRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ImageEntityRelation a    where  ");
            ImageEntityRelation imgRel = (ImageEntityRelation)data;
            if ( imgRel != null)
            {
                Parms = new List<Object[]>();
                if (!string.IsNullOrEmpty(imgRel.ImageName))
                {
                    sql.Append(" a.ImageName = :id     and   ");
                    Parms.Add(new Object[] { "id", imgRel.ImageName });
                }

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
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}
