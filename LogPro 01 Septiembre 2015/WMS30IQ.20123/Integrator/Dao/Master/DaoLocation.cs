using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoLocation : DaoService
    {
        public DaoLocation(DaoFactory factory) : base(factory) { }

        public Location Save(Location data)
        {
            return (Location)base.Save(data);
        }


        public Boolean Update(Location data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Location data)
        {
            return base.Delete(data);
        }


        public Location SelectById(Location data)
        {
            return (Location)base.SelectById(data);
        }


        public IList<Location> Select(Location data)
        {

                IList<Location> datos = new List<Location>();

                datos = GetHsql(data).List<Location>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Location a    where  ");
            Location location = (Location)data;
            if (location != null)
            {
                Parms = new List<Object[]>();
                if (location.LocationID != 0)
                {
                    sql.Append(" a.LocationID = :id     and   ");
                    Parms.Add(new Object[] { "id", location.LocationID });
                }

                if (location.Company != null && location.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", location.Company.CompanyID });
                }

                if (!String.IsNullOrEmpty(location.Name))
                {
                    sql.Append(" a.Name like :nom     and   ");
                    Parms.Add(new Object[] { "nom", "%" + location.Name + "%" });
                }

                if (!String.IsNullOrEmpty(location.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", location.ErpCode });
                }


                if (!String.IsNullOrEmpty(location.City))
                {
                    sql.Append(" a.City = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", location.City });
                }

                if (!String.IsNullOrEmpty(location.State))
                {
                    sql.Append(" a.State = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", location.State });
                }

                if (!String.IsNullOrEmpty(location.ZipCode))
                {
                    sql.Append(" a.ZipCode = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", location.ZipCode });
                }

                if (!String.IsNullOrEmpty(location.Country))
                {
                    sql.Append(" a.Country = :nom5     and   "); 
                    Parms.Add(new Object[] { "nom5", location.Country });
                }

                if (!String.IsNullOrEmpty(location.ContactPerson))
                {
                    sql.Append(" a.ContactPerson = :nom6     and   "); 
                    Parms.Add(new Object[] { "nom6", location.ContactPerson });
                }

                if (!String.IsNullOrEmpty(location.Phone1))
                {
                    sql.Append(" a.Phone1 = :nom7     and   "); 
                    Parms.Add(new Object[] { "nom7", location.Phone1 });
                }

                if (!String.IsNullOrEmpty(location.Phone2))
                {
                    sql.Append(" a.Phone2 = :nom8     and   "); 
                    Parms.Add(new Object[] { "nom8", location.Phone2 });
                }

                if (!String.IsNullOrEmpty(location.Phone3))
                {
                    sql.Append(" a.Phone3 = :nom9     and   "); 
                    Parms.Add(new Object[] { "nom9", location.Phone3 });
                }

                if (!String.IsNullOrEmpty(location.Email))
                {
                    sql.Append(" a.Email = :nom10     and   "); 
                    Parms.Add(new Object[] { "nom10", location.Email });
                }


                if (location.Status != null && location.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", location.Status.StatusID });
                }


                if (location.IsDefault != null)
                {
                    sql.Append(" a.IsDefault = :nom11     and   ");
                    Parms.Add(new Object[] { "nom11", location.IsDefault });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.LocationID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}