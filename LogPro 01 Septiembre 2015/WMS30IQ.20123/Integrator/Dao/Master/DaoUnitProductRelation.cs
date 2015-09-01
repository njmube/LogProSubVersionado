using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoUnitProductRelation : DaoService
    {
        public DaoUnitProductRelation(DaoFactory factory) : base(factory) { }

        public UnitProductRelation Save(UnitProductRelation data)
        {
            return (UnitProductRelation)base.Save(data);
        }


        public Boolean Update(UnitProductRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(UnitProductRelation data)
        {
            return base.Delete(data);
        }


        public UnitProductRelation SelectById(UnitProductRelation data)
        {
            return (UnitProductRelation)base.SelectById(data);
        }


        public IList<UnitProductRelation> Select(UnitProductRelation data)
        {

            IList<UnitProductRelation> datos = new List<UnitProductRelation>();

            datos = GetHsql(data).List<UnitProductRelation>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from UnitProductRelation a    where  ");
            UnitProductRelation unitproductrelation = (UnitProductRelation)data;
            if (unitproductrelation != null)
            {
                Parms = new List<Object[]>();
                if (unitproductrelation.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", unitproductrelation.RowID });
                }

                if (unitproductrelation.Product != null && unitproductrelation.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", unitproductrelation.Product.ProductID });
                }

                if (unitproductrelation.Product != null && !String.IsNullOrEmpty(unitproductrelation.Product.ProductCode))
                {
                    sql.Append(" a.Product.ProductCode = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", unitproductrelation.Product.ProductCode });
                }

                if (unitproductrelation.Unit != null && unitproductrelation.Unit.UnitID != 0)
                {
                    sql.Append(" a.Unit.UnitID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", unitproductrelation.Unit.UnitID });
                }

                if (!String.IsNullOrEmpty(unitproductrelation.UnitErpCode))
                {
                    sql.Append(" a.UnitErpCode = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", unitproductrelation.UnitErpCode });
                }

                if (unitproductrelation.Status != null && unitproductrelation.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", unitproductrelation.Status.StatusID });
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