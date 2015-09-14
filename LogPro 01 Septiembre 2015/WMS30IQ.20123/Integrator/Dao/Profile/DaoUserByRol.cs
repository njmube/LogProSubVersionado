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
    public class DaoUserByRol : DaoService
    {
        public DaoUserByRol(DaoFactory factory) : base(factory){ }

        public UserByRol Save(UserByRol data)
        {
            return (UserByRol)base.Save(data);
        }

        public Boolean Update(UserByRol data)
        {
            return base.Update(data);
        }

        public Boolean Delete(UserByRol data)
        {
            return base.Delete(data);
        }

        public UserByRol SelectById(UserByRol data)
        {
            return (UserByRol)base.SelectById(data);
        }

        public IList<UserByRol> Select(UserByRol data)
        {

                IList<UserByRol> datos = new List<UserByRol>();
                datos = GetHsql(data).List<UserByRol>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }

        public override IQuery GetHsql(Object data) 
        {
            StringBuilder sql = new StringBuilder("select a from UserByRol a  where  ");
            UserByRol userbyrol = (UserByRol)data;

            if (userbyrol != null) 
            {
                Parms = new List<Object[]>();

                if (userbyrol.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", userbyrol.RowID });
                }

                if (userbyrol.User != null && userbyrol.User.UserID !=0)
                {
                    sql.Append(" a.User.UserID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", userbyrol.User.UserID });
                }

                if (userbyrol.Rol != null && userbyrol.Rol.RolID != 0)
                {
                    sql.Append(" a.Rol.RolID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", userbyrol.Rol.RolID });
                }

                if (userbyrol.Location != null && userbyrol.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", userbyrol.Location.LocationID });
                }


                if (userbyrol.IsDefault != null)
                {
                    sql.Append(" a.IsDefault = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", userbyrol.IsDefault });
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
