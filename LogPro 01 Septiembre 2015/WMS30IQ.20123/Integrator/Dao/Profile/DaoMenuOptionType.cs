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
    public class DaoMenuOptionType : DaoService
    {
        public DaoMenuOptionType(DaoFactory factory) : base(factory) { }

        public MenuOptionType Save(MenuOptionType data)
        {
            return (MenuOptionType)base.Save(data);
        }

        public Boolean Update(MenuOptionType data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MenuOptionType data)
        {
            return base.Delete(data);
        }

        public MenuOptionType SelectById(MenuOptionType data)
        {
            return (MenuOptionType)base.SelectById(data);
        }

        public IList<MenuOptionType> Select(MenuOptionType data)
        {

                IList<MenuOptionType> datos = new List<MenuOptionType>();

                datos = GetHsql(data).List<MenuOptionType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }

        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MenuOptionType a    where  ");
            MenuOptionType menuoptiontype = (MenuOptionType)data;
            
            if (menuoptiontype != null)
            {
                Parms = new List<Object[]>();
                
                if(menuoptiontype.MenuOptionTypeID != 0)
                {
                    sql.Append(" a.MenuOptionTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", menuoptiontype.MenuOptionTypeID });
                }

                if (!String.IsNullOrEmpty(menuoptiontype.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", menuoptiontype.Name });
                }

                if (!String.IsNullOrEmpty(menuoptiontype.Url))
                {
                    sql.Append(" a.Url = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", menuoptiontype.Url });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.MenuOptionTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }

    }
}
