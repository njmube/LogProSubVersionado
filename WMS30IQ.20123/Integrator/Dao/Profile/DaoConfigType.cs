using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Profile;


namespace Integrator.Dao.Profile
{
    public class DaoConfigType : DaoService
    {   
        public DaoConfigType(DaoFactory factory) : base(factory) { }

        public ConfigType Save(ConfigType data)
        {
            return (ConfigType)base.Save(data);
        }


        public Boolean Update(ConfigType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ConfigType data)
        {
            return base.Delete(data);
        }


        public ConfigType SelectById(ConfigType data)
        {
            return (ConfigType)base.SelectById(data);
        }


        public IList<ConfigType> Select(ConfigType data)
        {

                IList<ConfigType> datos = new List<ConfigType>();
                datos = GetHsql(data).List<ConfigType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }

        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ConfigType a    where  ");
            ConfigType configtype = (ConfigType)data;
            if (configtype != null)
            {
                Parms = new List<Object[]>();

                if (configtype.ConfigTypeID != 0)
                {
                    sql.Append(" a.ConfigTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", configtype.ConfigTypeID });
                }

                if (!String.IsNullOrEmpty(configtype.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", configtype.Name });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ConfigTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
    }
}
