using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.General;
using NHibernate;

namespace Integrator.Dao.General
{
    public class DaoLabelMapping: DaoService
    {
        public DaoLabelMapping(DaoFactory factory) : base(factory) { }

        public LabelMapping Save(LabelMapping data)
        {
            return (LabelMapping)base.Save(data);
        }


        public Boolean Update(LabelMapping data)
        {
            return base.Update(data);
        }


        public Boolean Delete(LabelMapping data)
        {
            return base.Delete(data);
        }


        public LabelMapping SelectById(LabelMapping data)
        {
            return (LabelMapping)base.SelectById(data);
        }


        public IList<LabelMapping> Select(LabelMapping data)
        {

                IList<LabelMapping> datos = new List<LabelMapping>();
                datos = GetHsql(data).List<LabelMapping>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from LabelMapping a    where  ");
            LabelMapping LabelMapping = (LabelMapping)data;
            if (LabelMapping != null)
            {
                Parms = new List<Object[]>();
                if (!String.IsNullOrEmpty(LabelMapping.Description))
                {
                    sql.Append(" a.Description = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", LabelMapping.Description });
                }
                if (LabelMapping.LabelType != null && LabelMapping.LabelType.DocTypeID != 0)
                {
                    sql.Append(" a.LabelType.DocTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", LabelMapping.LabelType.DocTypeID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Description asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
    }
}
