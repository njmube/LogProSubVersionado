using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoUnit : DaoService
    {
        public DaoUnit(DaoFactory factory) : base(factory) { }

        public Unit Save(Unit data)
        {
            return (Unit)base.Save(data);
        }


        public Boolean Update(Unit data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Unit data)
        {
            return base.Delete(data);
        }


        public Unit SelectById(Unit data)
        {
            return (Unit)base.SelectById(data);
        }


        public IList<Unit> Select(Unit data)
        {

                IList<Unit> datos = new List<Unit>();
                datos = GetHsql(data).List<Unit>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
           
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Unit a    where  ");
            Unit unit = (Unit)data;
            if (unit != null)
            {
                Parms = new List<Object[]>();
                if (unit.UnitID != 0)
                {
                    sql.Append(" a.UnitID = :id     and   ");
                    Parms.Add(new Object[] { "id", unit.UnitID });
                }

                if (unit.Company != null && unit.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", unit.Company.CompanyID });
                }

                if (!String.IsNullOrEmpty(unit.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", unit.Name });
                }


                if (!String.IsNullOrEmpty(unit.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", unit.ErpCode });
                }

                if (!String.IsNullOrEmpty(unit.ErpCodeGroup))
                {
                    sql.Append(" a.ErpCodeGroup = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", unit.ErpCodeGroup });
                }

                if (!String.IsNullOrEmpty(unit.Description))
                {
                    sql.Append(" a.Description = :nom3    and   "); 
                    Parms.Add(new Object[] { "nom3", unit.Description });
                }

                if (unit.BaseAmount > 0)
                {
                    sql.Append(" a.BaseAmount = :u3    and   ");
                    Parms.Add(new Object[] { "u3", unit.BaseAmount });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.UnitID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}