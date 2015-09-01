using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProductAlternate : DaoService
    {
        public DaoProductAlternate(DaoFactory factory) : base(factory) { }

        public ProductAlternate Save(ProductAlternate data)
        {
            return (ProductAlternate)base.Save(data);
        }


        public Boolean Update(ProductAlternate data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProductAlternate data)
        {
            return base.Delete(data);
        }


        public ProductAlternate SelectById(ProductAlternate data)
        {
            return (ProductAlternate)base.SelectById(data);
        }


        public IList<ProductAlternate> Select(ProductAlternate data)
        {

                IList<ProductAlternate> datos = new List<ProductAlternate>();
                datos = GetHsql(data).List<ProductAlternate>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProductAlternate a  where  ");
            ProductAlternate pAltern = (ProductAlternate)data;
           
            if (pAltern != null)
            {
                Parms = new List<Object[]>();

                if (pAltern.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", pAltern.RowID });
                }


                if (pAltern.AlternProduct != null && pAltern.AlternProduct.ProductID != 0)
                {
                    sql.Append(" a.AlternProduct.ProductID  = :id2     and   ");
                    Parms.Add(new Object[] { "id2", pAltern.AlternProduct.ProductID });
                }


                if (pAltern.IsFromErp != null)
                {
                    sql.Append(" a.IsFromErp  = :idx2     and   ");
                    Parms.Add(new Object[] { "idx2", pAltern.IsFromErp });
                }

                if (pAltern.Product != null && pAltern.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", pAltern.Product.ProductID });
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