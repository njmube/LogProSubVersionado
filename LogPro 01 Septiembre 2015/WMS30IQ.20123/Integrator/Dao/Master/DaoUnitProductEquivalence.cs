using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoUnitProductEquivalence : DaoService
    {
        public DaoUnitProductEquivalence(DaoFactory factory) : base(factory) { }

        public UnitProductEquivalence Save(UnitProductEquivalence data)
        {
            return (UnitProductEquivalence)base.Save(data);
        }


        public Boolean Update(UnitProductEquivalence data)
        {
            return base.Update(data);
        }


        public Boolean Delete(UnitProductEquivalence data)
        {
            return base.Delete(data);
        }


        public UnitProductEquivalence SelectById(UnitProductEquivalence data)
        {
            return (UnitProductEquivalence)base.SelectById(data);
        }


        public IList<UnitProductEquivalence> Select(UnitProductEquivalence data)
        {

                IList<UnitProductEquivalence> datos = new List<UnitProductEquivalence>();
                datos = GetHsql(data).List<UnitProductEquivalence>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from UnitProductEquivalence a    where  ");
            UnitProductEquivalence unitproductequivalence = (UnitProductEquivalence)data;
            if (unitproductequivalence != null)
            {
                Parms = new List<Object[]>();
                if (unitproductequivalence.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", unitproductequivalence.RowID });
                }

                if (unitproductequivalence.UnitProductRelation != null && unitproductequivalence.UnitProductRelation.RowID != 0)
                {
                    sql.Append(" a.UnitProductRelation.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", unitproductequivalence.UnitProductRelation.RowID });
                }

                if (unitproductequivalence.EquivMeasureUnit != null && unitproductequivalence.EquivMeasureUnit.MeasureUnitID != 0)
                {
                    sql.Append(" a.EquivMeasureUnit.MeasureUnitID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", unitproductequivalence.EquivMeasureUnit.MeasureUnitID });
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