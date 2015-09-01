using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Profile;

namespace Integrator.Dao.Profile
{
    public class DaoSysUser : DaoService
    {
        public DaoSysUser(DaoFactory factory) : base(factory) { }

        public SysUser Save(SysUser data)
        {
            return (SysUser)base.Save(data);
        }


        public Boolean Update(SysUser data)
        {
            return base.Update(data);
        }


        public Boolean Delete(SysUser data)
        {
            return base.Delete(data);
        }


        public SysUser SelectById(SysUser data)
        {
            return (SysUser)base.SelectById(data);
        }


        public IList<SysUser> Select(SysUser data)
        {

                IList<SysUser> datos = new List<SysUser>();
                datos = GetHsql(data).List<SysUser>();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from SysUser a  where  ");
            SysUser user = (SysUser)data;
            if (user != null)
            {
                Parms = new List<Object[]>();



                if (user.UserID != 0)
                {
                    sql.Append(" a.UserID = :usr     and   ");
                    Parms.Add(new Object[] { "usr", user.UserID });
                }


                if (!String.IsNullOrEmpty(user.UserName))
                {
                    sql.Append(" Lower(a.UserName) = :nom     and   ");
                    Parms.Add(new Object[] { "nom", user.UserName.ToLower() });
                }

                if (!String.IsNullOrEmpty(user.Password))
                {
                    sql.Append(" a.Password = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", user.Password });
                }

                if (!String.IsNullOrEmpty(user.FirstName))
                {
                    sql.Append(" a.FirstName = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", user.FirstName });
                }

                if (!String.IsNullOrEmpty(user.LastName))
                {
                    sql.Append(" a.LastName = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", user.LastName });
                }

                if (!String.IsNullOrEmpty(user.Phone))
                {
                    sql.Append(" a.Phone = :nom4     and   ");
                    Parms.Add(new Object[] { "nom4", user.Phone });
                }

                if (!String.IsNullOrEmpty(user.Email))
                {
                    sql.Append(" a.Email = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", user.Email });
                }

                if (user.IsSuperUser == true)
                {
                    sql.Append(" a.IsSuperUser = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", true });
                }

                else if (user.IsSuperUser == false)
                {
                    sql.Append(" a.IsSuperUser = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", false });
                }

                if (!String.IsNullOrEmpty(user.LastSession))
                {
                    sql.Append(" a.LastSession = :n4     and   ");
                    Parms.Add(new Object[] { "n4", user.LastSession });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.UserID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}