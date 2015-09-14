using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProductInventory : DaoService
    {
        public DaoProductInventory(DaoFactory factory) : base(factory) { }

        public ProductInventory Save(ProductInventory data)
        {
            return (ProductInventory)base.Save(data);
        }


        public Boolean Update(ProductInventory data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProductInventory data)
        {
            return base.Delete(data);
        }


        public ProductInventory SelectById(ProductInventory data)
        {
            return (ProductInventory)base.SelectById(data);
        }


        public IList<ProductInventory> Select(ProductInventory data)
        {

                IList<ProductInventory> datos = new List<ProductInventory>();

                try
                {
                    datos = GetHsql(data).List<ProductInventory>();

                    if (!Factory.IsTransactional)
                        Factory.Commit();
                }
                            
                catch (Exception e)
                {
                    NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
                }


                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProductInventory a  where ");
            ProductInventory ProductInventory = (ProductInventory)data;


            if (ProductInventory != null)
            {
                Parms = new List<Object[]>();



                if (ProductInventory.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", ProductInventory.RowID });
                }

                if (ProductInventory.Location != null && ProductInventory.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", ProductInventory.Location.LocationID });
                }


                if (ProductInventory.Product != null && ProductInventory.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID  = :cot1     and   ");
                    Parms.Add(new Object[] { "cot1", ProductInventory.Product.ProductID });
                }

                if (ProductInventory.Document != null && ProductInventory.Document.DocID != 0)
                {
                    sql.Append(" a.Document.DocID = :doc     and   ");
                    Parms.Add(new Object[] { "doc", ProductInventory.Document.DocID });
                }


                if (!string.IsNullOrEmpty(ProductInventory.CreatedBy))
                {
                    sql.Append(" a.ModifiedBy = :mod     and   ");
                    Parms.Add(new Object[] { "mod", ProductInventory.CreatedBy });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public IList<ProductInventory> GetProductInventory(ProductInventory productInventory, List<int> productList)
        {
            StringBuilder sql = new StringBuilder("select a from ProductInventory a   Where ");


            if (productInventory != null)
            {
                Parms = new List<Object[]>();



                if (productInventory.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", productInventory.RowID });
                }

                if (productInventory.Location != null && productInventory.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", productInventory.Location.LocationID });
                }


                if (productInventory.Product != null && productInventory.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID  = :cot     and   ");
                    Parms.Add(new Object[] { "cot", productInventory.Product.ProductID });
                }


                if (productInventory.Document != null && productInventory.Document.DocID != 0)
                {
                    sql.Append(" a.Document.DocID = :doc     and   ");
                    Parms.Add(new Object[] { "doc", productInventory.Document.DocID });
                }


                if (productList != null && productList.Count > 0)
                {
                    sql.Append(" a.Product.ProductID  IN ( 0 ");

                    int i = 0;
                    foreach (int p in productList)
                    {
                        sql.Append(", :px" + i.ToString());
                        Parms.Add(new Object[] { "px" + i.ToString(), p });
                        i++;
                    }

                    sql.Append(") and ");
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query.List<ProductInventory>();
        }


    }
}