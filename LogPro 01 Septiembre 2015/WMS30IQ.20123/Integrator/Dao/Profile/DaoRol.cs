using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Profile;

namespace Integrator.Dao.Profile
{
    public class DaoRol : DaoService
    {
        public DaoRol(DaoFactory factory) : base(factory) { }

        public Rol Save(Rol data)
        {
            return (Rol)base.Save(data);
        }


        public Boolean Update(Rol data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Rol data)
        {
            return base.Delete(data);
        }


        public Rol SelectById(Rol data)
        {
            return (Rol)base.SelectById(data);
        }


        public IList<Rol> Select(Rol data)
        {

                IList<Rol> datos = new List<Rol>();

                datos = GetHsql(data).List<Rol>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Rol a    where  ");
            Rol rol = (Rol)data;
            if (rol != null)
            {
                Parms = new List<Object[]>();
                if (rol.RolID != 0)
                {
                    sql.Append(" a.RolID = :id     and   ");
                    Parms.Add(new Object[] { "id", rol.RolID });
                }

                if (!String.IsNullOrEmpty(rol.RolCode))
                {
                    sql.Append(" a.RolCode = :nom     and   ");
                    Parms.Add(new Object[] { "nom", rol.RolCode });
                }

                if (!String.IsNullOrEmpty(rol.Name))
                {
                    sql.Append(" a.Name = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", rol.Name });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.RolID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}