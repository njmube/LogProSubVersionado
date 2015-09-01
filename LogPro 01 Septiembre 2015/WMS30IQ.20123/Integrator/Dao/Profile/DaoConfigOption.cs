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
    public class DaoConfigOption : DaoService
    {
        public DaoConfigOption(DaoFactory factory) : base(factory) { }

        public ConfigOption Save(ConfigOption data)
        {
            return (ConfigOption)base.Save(data);
        }


        public Boolean Update(ConfigOption data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ConfigOption data)
        {
            return base.Delete(data);
        }


        public ConfigOption SelectById(ConfigOption data)
        {
            return (ConfigOption)base.SelectById(data);
        }

        public IList<ConfigOption> Select(ConfigOption data)
        {
                IList<ConfigOption> datos = new List<ConfigOption>();
                datos = GetHsql(data).List<ConfigOption>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;            
        }


        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from ConfigOption a  where  ");
            ConfigOption configoption = (ConfigOption)data;
            
            if (configoption != null)
            {
                Parms = new List<Object[]>();

                if (configoption.ConfigOptionID != 0)
                {
                    sql.Append(" a.ConfigOptionID = :id     and    ");
                    Parms.Add(new Object[] { "id", configoption.ConfigOptionID });
                }

                if(configoption.ConfigType != null && configoption.ConfigType.ConfigTypeID !=0 )
                {
                    sql.Append("a.ConfigType.ConfigTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", configoption.ConfigType.ConfigTypeID });

                }

                
                if (!String.IsNullOrEmpty(configoption.Code))
                {
                    sql.Append(" a.Code = :nom     and   ");
                    Parms.Add(new Object[] { "nom", configoption.Code });
                }

                if (!String.IsNullOrEmpty(configoption.Name))
                {
                    sql.Append(" a.Name = :nom1    and   ");
                    Parms.Add(new Object[] { "nom1", configoption.Name });
                }

                if (!String.IsNullOrEmpty(configoption.Description))
                {
                    sql.Append(" a.Description = :nom3     and");
                    Parms.Add(new Object[] { "nom3", configoption.Description });
                }

                if (configoption.NumOrder != 0)
                {
                    sql.Append(" a.NumOrder = :id2     and    ");
                    Parms.Add(new Object[] { "id2", configoption.NumOrder });
                }

                if (configoption.IsDevice != null)
                {
                    sql.Append(" a.IsDevice = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", configoption.IsDevice });
                }

                 if (configoption.IsAdmin  != null)
                {
                    sql.Append(" a.IsAdmin = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", configoption.IsAdmin });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ConfigOptionID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }

    }
}
