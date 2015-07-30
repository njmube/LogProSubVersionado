using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProductAccountRelation : DaoService
    {
        public DaoProductAccountRelation(DaoFactory factory) : base(factory) { }

        public ProductAccountRelation Save(ProductAccountRelation data)
        {
            return (ProductAccountRelation)base.Save(data);
        }


        public Boolean Update(ProductAccountRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProductAccountRelation data)
        {
            return base.Delete(data);
        }


        public ProductAccountRelation SelectById(ProductAccountRelation data)
        {
            return (ProductAccountRelation)base.SelectById(data);
        }


        public IList<ProductAccountRelation> Select(ProductAccountRelation data)
        {

                IList<ProductAccountRelation> datos = new List<ProductAccountRelation>();
                datos = GetHsql(data).List<ProductAccountRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProductAccountRelation a    where  ");
            ProductAccountRelation pVendor = (ProductAccountRelation)data;
            if (pVendor != null)
            {
                Parms = new List<Object[]>();
                if (pVendor.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", pVendor.RowID });
                }

                if (pVendor.AccountType != null && pVendor.AccountType.AccountTypeID != 0)
                {
                    sql.Append(" a.AccountType.AccountTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", pVendor.AccountType.AccountTypeID });
                }

                if (pVendor.Account != null && pVendor.Account.AccountID != 0)
                {
                    sql.Append(" a.Account.AccountID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", pVendor.Account.AccountID });
                }

                if (pVendor.Product != null && pVendor.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", pVendor.Product.ProductID });
                }

                if (!String.IsNullOrEmpty(pVendor.ItemNumber))
                {
                    sql.Append(" (a.ItemNumber = :nom OR a.Code1 = :nom OR a.Code2 = :nom  )  and   ");
                    Parms.Add(new Object[] { "nom",  pVendor.ItemNumber });
                }


                if (!String.IsNullOrEmpty(pVendor.Code1))
                {
                    sql.Append(" a.Code1 = :cod1     and   ");
                    Parms.Add(new Object[] { "cod1", pVendor.Code1 });
                }

                if (!String.IsNullOrEmpty(pVendor.Code2))
                {
                    sql.Append(" a.Code2 = :cod2     and   ");
                    Parms.Add(new Object[] { "cod2", pVendor.Code2 });
                }

                if (pVendor.IsFromErp != null)
                {
                    sql.Append(" a.IsFromErp = :if2     and   ");
                    Parms.Add(new Object[] { "if2", pVendor.IsFromErp });
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