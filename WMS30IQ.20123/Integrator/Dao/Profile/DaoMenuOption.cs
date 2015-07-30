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
    public class DaoMenuOption : DaoService
    {
        public DaoMenuOption(DaoFactory factory) : base(factory){}

        public MenuOption Save(MenuOption data)
        {
            return (MenuOption)base.Save(data);
        }

        public Boolean Update(MenuOption data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MenuOption data)
        {
            return base.Delete(data);
        }

        public MenuOption SelectById(MenuOption data) 
        {
            return (MenuOption)base.SelectById(data);
        }

        public IList<MenuOption> Select(MenuOption data)
        {

                IList<MenuOption> datos = new List<MenuOption>();
                datos = GetHsql(data).List<MenuOption>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }

        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from MenuOption a  where  ");
            MenuOption menuoption = (MenuOption)data;

            if (menuoption != null)
            {
                Parms = new List<Object[]>();

                if (menuoption.MenuOptionID != 0) 
                {
                    sql.Append(" a.MenuOptionID = :id     and    ");
                    Parms.Add(new Object[] { "id", menuoption.MenuOptionID });
                }

                if (!String.IsNullOrEmpty(menuoption.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", menuoption.Name });
                }

                if (!String.IsNullOrEmpty(menuoption.Url))
                {
                    sql.Append(" a.Url = :nom1    and   ");
                    Parms.Add(new Object[] { "nom1", menuoption.Url });
                }

                if (menuoption.MenuOptionType != null && menuoption.MenuOptionType.MenuOptionTypeID != 0)
                {
                    sql.Append(" a.MenuOptionType.MenuOptionTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", menuoption.MenuOptionType.MenuOptionTypeID });
                }

                if (menuoption.NumOrder != 0)
                {
                    sql.Append(" a.NumOrder = :id2     and    ");
                    Parms.Add(new Object[] { "id2", menuoption.NumOrder });
                }

                if (menuoption.Active != null)
                {
                    sql.Append(" a.Active = :ia2     and    ");
                    Parms.Add(new Object[] { "ia2", menuoption.Active });
                }


                if (menuoption.OptionType != null && menuoption.OptionType.OpTypeID != 0)
                {
                    sql.Append(" a.OptionType.OpTypeID = :io2     and    ");
                    Parms.Add(new Object[] { "io2", menuoption.OptionType.OpTypeID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.MenuOptionID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
    }
}
