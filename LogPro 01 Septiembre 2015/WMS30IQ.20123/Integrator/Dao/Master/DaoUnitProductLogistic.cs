using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoUnitProductLogistic : DaoService
    {
        public DaoUnitProductLogistic(DaoFactory factory) : base(factory) { }

        public UnitProductLogistic Save(UnitProductLogistic data)
        {
            return (UnitProductLogistic)base.Save(data);
        }


        public Boolean Update(UnitProductLogistic data)
        {
            return base.Update(data);
        }


        public Boolean Delete(UnitProductLogistic data)
        {
            return base.Delete(data);
        }


        public UnitProductLogistic SelectById(UnitProductLogistic data)
        {
            return (UnitProductLogistic)base.SelectById(data);
        }

        public IList<UnitProductLogistic> Select(UnitProductLogistic data)
        {

            IList<UnitProductLogistic> datos = new List<UnitProductLogistic>();
            datos = GetHsql(data).List<UnitProductLogistic>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }

        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from UnitProductLogistic a    where  ");
            UnitProductLogistic unitproductlogistic = (UnitProductLogistic)data;
            if (unitproductlogistic != null)
            {
                Parms = new List<Object[]>();
                if (unitproductlogistic.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", unitproductlogistic.RowID });
                }

                if(unitproductlogistic.LogisticUnit != null && unitproductlogistic.LogisticUnit.RowID != 0)
                {
                    sql.Append(" a.LogisticUnit.RowID = id1     and    ");
                    Parms.Add(new Object[] { "id1", unitproductlogistic.LogisticUnit.RowID });
                }

                if (unitproductlogistic.ContainedUnit != null && unitproductlogistic.ContainedUnit.RowID != 0)
                {
                    sql.Append(" a.ContainedUnit.RowID = id2     and    ");
                    Parms.Add(new Object[] { "id2", unitproductlogistic.ContainedUnit.RowID });

                }

                if (unitproductlogistic.AmountOfContained != 0)
                {
                    sql.Append(" a.Amount = id3     and    ");
                    Parms.Add(new Object[] { "id3", unitproductlogistic.AmountOfContained });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
        

    }
}
