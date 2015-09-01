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
    public class DaoConfigOptionByCompany : DaoService
    {
        public DaoConfigOptionByCompany(DaoFactory factory) : base(factory) { }

        public ConfigOptionByCompany Save(ConfigOptionByCompany data)
        {
            return (ConfigOptionByCompany)base.Save(data);
        }


        public Boolean Update(ConfigOptionByCompany data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ConfigOptionByCompany data)
        {
            return base.Delete(data);
        }


        public ConfigOptionByCompany SelectById(ConfigOptionByCompany data)
        {
            return (ConfigOptionByCompany)base.SelectById(data);
        }

        public IList<ConfigOptionByCompany> Select(ConfigOptionByCompany data)
        {
                IList<ConfigOptionByCompany> datos = new List<ConfigOptionByCompany>();
                datos = GetHsql(data).List<ConfigOptionByCompany>();
                //if (!Factory.IsTransactional)
                    //Factory.Commit();
                return datos;            
        }


        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from ConfigOptionByCompany a  where  ");
            ConfigOptionByCompany configoption = (ConfigOptionByCompany)data;
            
            if (configoption != null)
            {
                Parms = new List<Object[]>();

                if (configoption.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and    ");
                    Parms.Add(new Object[] { "id", configoption.RowID });
                }

                if (configoption.ConfigOption != null && configoption.ConfigOption.ConfigOptionID != 0)
                {
                    sql.Append("a.ConfigOption.ConfigOptionID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", configoption.ConfigOption.ConfigOptionID });
                }

                
                if (!String.IsNullOrEmpty(configoption.Value))
                {
                    sql.Append(" a.Value = :nom     and   ");
                    Parms.Add(new Object[] { "nom", configoption.Value });
                }


                if (configoption.ConfigOption != null)
                {
                    if (!String.IsNullOrEmpty(configoption.ConfigOption.Code))
                    {
                        sql.Append(" a.ConfigOption.Code = :nom1     and   ");
                        Parms.Add(new Object[] { "nom1", configoption.ConfigOption.Code });
                    }

                    if (configoption.ConfigOption.ConfigType != null && configoption.ConfigOption.ConfigType.ConfigTypeID != 0)
                    {
                        sql.Append(" a.ConfigOption.ConfigType.ConfigTypeID = :cfg     and   ");
                        Parms.Add(new Object[] { "cfg", configoption.ConfigOption.ConfigType.ConfigTypeID });
                    }

                    if (configoption.ConfigOption.IsAdmin != null)
                    {
                        sql.Append(" a.ConfigOption.IsAdmin = :adm     and   ");
                        Parms.Add(new Object[] { "adm", configoption.ConfigOption.IsAdmin });
                    }

                }


                if (configoption.Company != null  && configoption.Company.CompanyID != 0)
                {
                    sql.Append("a.Company.CompanyID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", configoption.Company.CompanyID });
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
