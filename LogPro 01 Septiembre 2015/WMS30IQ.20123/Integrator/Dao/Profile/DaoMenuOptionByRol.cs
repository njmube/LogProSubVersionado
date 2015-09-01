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
    public class DaoMenuOptionByRol : DaoService
    {
        public DaoMenuOptionByRol(DaoFactory factory) : base(factory){ }

        public MenuOptionByRol Save(MenuOptionByRol data)
        {
            return (MenuOptionByRol)base.Save(data);
        }

        public Boolean Update(MenuOptionByRol data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MenuOptionByRol data)
        {
            return base.Delete(data);
        }

        public MenuOptionByRol SelectById(MenuOptionByRol data)
        {
            return (MenuOptionByRol)base.SelectById(data);
        }

        public IList<MenuOptionByRol> Select(MenuOptionByRol data)
        {

                IList<MenuOptionByRol> datos = new List<MenuOptionByRol>();
                datos = GetHsql(data).List<MenuOptionByRol>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }

        public override IQuery GetHsql(Object data) 
        {
            StringBuilder sql = new StringBuilder("select a from MenuOptionByRol a  where  ");
            MenuOptionByRol menuoptionbyrol = (MenuOptionByRol)data;

            if (menuoptionbyrol != null) 
            {
                Parms = new List<Object[]>();

                if (menuoptionbyrol.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", menuoptionbyrol.RowID });
                }

                if (menuoptionbyrol.MenuOption != null && menuoptionbyrol.MenuOption.MenuOptionID !=0)
                {
                    sql.Append(" a.MenuOption.MenuOptionID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", menuoptionbyrol.MenuOption.MenuOptionID });
                }

                if (menuoptionbyrol.Rol != null && menuoptionbyrol.Rol.RolID != 0)
                {
                    sql.Append(" a.Rol.RolID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", menuoptionbyrol.Rol.RolID });
                }

                if (menuoptionbyrol.Company != null && menuoptionbyrol.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", menuoptionbyrol.Company.CompanyID });
                }

                if (menuoptionbyrol.Status != null && menuoptionbyrol.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID  = :id4     and   ");
                    Parms.Add(new Object[] { "id4", menuoptionbyrol.Status.StatusID });
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
