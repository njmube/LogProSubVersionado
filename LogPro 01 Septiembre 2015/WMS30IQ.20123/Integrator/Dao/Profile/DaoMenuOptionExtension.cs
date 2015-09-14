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
    public class DaoMenuOptionExtension : DaoService
    {
        public DaoMenuOptionExtension(DaoFactory factory) : base(factory){}

        public MenuOptionExtension Save(MenuOptionExtension data)
        {
            return (MenuOptionExtension)base.Save(data);
        }

        public Boolean Update(MenuOptionExtension data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MenuOptionExtension data)
        {
            return base.Delete(data);
        }

        public MenuOptionExtension SelectById(MenuOptionExtension data) 
        {
            return (MenuOptionExtension)base.SelectById(data);
        }

        public IList<MenuOptionExtension> Select(MenuOptionExtension data)
        {

                IList<MenuOptionExtension> datos = new List<MenuOptionExtension>();
                datos = GetHsql(data).List<MenuOptionExtension>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }

        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from MenuOptionExtension a  where  ");
            MenuOptionExtension MenuOptionExtension = (MenuOptionExtension)data;

            if (MenuOptionExtension != null)
            {
                Parms = new List<Object[]>();

                if (MenuOptionExtension.RowID != 0) 
                {
                    sql.Append(" a.RowID = :id     and    ");
                    Parms.Add(new Object[] { "id", MenuOptionExtension.RowID });
                }

                if (MenuOptionExtension.MenuOption != null && MenuOptionExtension.MenuOption.MenuOptionID != 0)
                {
                    sql.Append(" a.MenuOption.MenuOptionID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", MenuOptionExtension.MenuOption.MenuOptionID });
                }

             
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public IList<Object[]> GetReportObject(string selectQuery, IList<String> rpParams)
        {
            StringBuilder sql = new StringBuilder(selectQuery);

            if (rpParams != null && rpParams.Count > 0)
            {
                Parms = new List<Object[]>();

                for (int i = 0; i < rpParams.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rpParams[i]))
                    {
                        sql.Append(" And " + rpParams[i].Split(':')[0] + " Like :p" + i.ToString());
                        Parms.Add(new Object[] { "p" + i.ToString(), rpParams[i].Split(':')[1] + "%" });
                    }

                }
            }

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            return query.List<Object[]>();
        }


    }
}
