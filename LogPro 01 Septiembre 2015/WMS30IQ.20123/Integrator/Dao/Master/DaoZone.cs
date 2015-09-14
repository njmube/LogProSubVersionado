using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoZone : DaoService
    {
        public DaoZone(DaoFactory factory) : base(factory) { }

        public Zone Save(Zone data)
        {
            return (Zone)base.Save(data);
        }


        public Boolean Update(Zone data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Zone data)
        {
            return base.Delete(data);
        }


        public Zone SelectById(Zone data)
        {
            return (Zone)base.SelectById(data);
        }


        public IList<Zone> Select(Zone data)
        {

                IList<Zone> datos = new List<Zone>();
                datos = GetHsql(data).List<Zone>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Zone a    where  ");
            Zone zone = (Zone)data;
            if (zone != null)
            {
                Parms = new List<Object[]>();
                if (zone.ZoneID != 0)
                {
                    sql.Append(" a.ZoneID = :id     and   ");
                    Parms.Add(new Object[] { "id", zone.ZoneID });
                }

                if (zone.Location != null && zone.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", zone.Location.LocationID });
                }

                if (zone.Contract != null && zone.Contract.ContractID != 0)
                {
                    sql.Append(" a.Contract.ContractID  = :cot1     and   ");
                    Parms.Add(new Object[] { "cot", zone.Contract.ContractID });
                }


                //if (zone.Company != null && zone.Company.CompanyID != 0)
                //{
                //    sql.Append(" a.Company.CompanyID = :id2     and   ");
                //    Parms.Add(new Object[] { "id2", zone.Company.CompanyID });
                //}

                if (!String.IsNullOrEmpty(zone.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", zone.Name });
                }

                if (!String.IsNullOrEmpty(zone.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", zone.ErpCode });
                }

                if (!String.IsNullOrEmpty(zone.Description))
                {
                    sql.Append(" a.Description = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", zone.Description });
                }

                if (!String.IsNullOrEmpty(zone.StoreConditions))
                {
                    sql.Append(" a.StoreConditions = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", zone.StoreConditions });
                }

                if (zone.Status != null && zone.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", zone.Status.StatusID });
                }

                if (zone.IsDefault != null)
                {
                    sql.Append(" a.IsDefault = :nom11     and   ");
                    Parms.Add(new Object[] { "nom11", zone.IsDefault });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.ZoneID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}