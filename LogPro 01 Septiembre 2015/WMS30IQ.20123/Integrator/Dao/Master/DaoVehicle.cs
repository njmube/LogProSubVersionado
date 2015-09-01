using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoVehicle : DaoService
    {
        public DaoVehicle(DaoFactory factory) : base(factory) { }

        public Vehicle Save(Vehicle data)
        {
            return (Vehicle)base.Save(data);
        }


        public Boolean Update(Vehicle data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Vehicle data)
        {
            return base.Delete(data);
        }


        public Vehicle SelectById(Vehicle data)
        {
            return (Vehicle)base.SelectById(data);
        }


        public IList<Vehicle> Select(Vehicle data)
        {
                IList<Vehicle> datos = new List<Vehicle>();
                datos = GetHsql(data).List<Vehicle>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Vehicle a    where  ");
            Vehicle vehicle = (Vehicle)data;
            if (vehicle != null)
            {
                Parms = new List<Object[]>();
                if (vehicle.VehicleID != 0)
                {
                    sql.Append(" a.VehicleID = :id     and   ");
                    Parms.Add(new Object[] { "id", vehicle.VehicleID });
                }

                if (vehicle.Account != null && vehicle.Account.AccountID != 0)
                {
                    sql.Append(" a.Account.AccountID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", vehicle.Account.AccountID });
                }

                if (!String.IsNullOrEmpty(vehicle.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", vehicle.ErpCode });
                }

                if (!String.IsNullOrEmpty(vehicle.Plate1))
                {
                    sql.Append(" a.Plate1 = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", vehicle.Plate1 });
                }

                if (!String.IsNullOrEmpty(vehicle.Plate2))
                {
                    sql.Append(" a.Plate2 = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", vehicle.Plate2 });
                }

                if (!String.IsNullOrEmpty(vehicle.Capacity))
                {
                    sql.Append(" a.Capacity = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", vehicle.Capacity });
                }

                if (!String.IsNullOrEmpty(vehicle.ContainerNumber))
                {
                    sql.Append(" a.ContainerNumber = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", vehicle.ContainerNumber });
                }

                if (!String.IsNullOrEmpty(vehicle.ContainerCapacity))
                {
                    sql.Append(" a.ContainerCapacity = :nom5     and   "); 
                    Parms.Add(new Object[] { "nom5", vehicle.ContainerCapacity });
                }

                if (vehicle.Status != null && vehicle.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", vehicle.Status.StatusID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.vehicleID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}