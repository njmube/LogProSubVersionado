using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProductCategory : DaoService
    {
        public DaoProductCategory(DaoFactory factory) : base(factory) { }

        public ProductCategory Save(ProductCategory data)
        {
            return (ProductCategory)base.Save(data);
        }


        public Boolean Update(ProductCategory data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProductCategory data)
        {
            return base.Delete(data);
        }


        public ProductCategory SelectById(ProductCategory data)
        {
            return (ProductCategory)base.SelectById(data);
        }


        public IList<ProductCategory> Select(ProductCategory data)
        {

                IList<ProductCategory> datos = new List<ProductCategory>();
                datos = GetHsql(data).List<ProductCategory>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProductCategory a    where  ");
            ProductCategory category = (ProductCategory)data;
            if (category != null)
            {
                Parms = new List<Object[]>();
                if (category.CategoryID != 0)
                {
                    sql.Append(" a.CategoryID = :id     and   ");
                    Parms.Add(new Object[] { "id", category.CategoryID });
                }

                if (category.Company != null && category.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", category.Company.CompanyID });
                }

                if (!String.IsNullOrEmpty(category.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom     and   ");
                    Parms.Add(new Object[] { "nom",  category.ErpCode });
                }

                if (!String.IsNullOrEmpty(category.Name))
                {
                    sql.Append(" a.Name like :nom1   and   "); 
                    Parms.Add(new Object[] { "nom1", "%"+ category.Name + "%" });
                }

                if (!String.IsNullOrEmpty(category.Description))
                {
                    sql.Append(" a.Description like :nom2  and   ");
                    Parms.Add(new Object[] { "nom2", "%" + category.Description + "%" });
                }
 
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Name asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}