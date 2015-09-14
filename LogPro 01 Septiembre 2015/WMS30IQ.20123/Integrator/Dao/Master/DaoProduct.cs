using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProduct : DaoService
    {
        public DaoProduct(DaoFactory factory) : base(factory) { }

        public Product Save(Product data)
        {
            return (Product)base.Save(data);
        }


        public Boolean Update(Product data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Product data)
        {
            return base.Delete(data);
        }


        public Product SelectById(Product data)
        {
            return (Product)base.SelectById(data);
        }


        public IList<Product> Select(Product data, int showRegs)
        {

                IList<Product> datos = new List<Product>();

                try
                {
                    if (showRegs > 0)
                        datos = GetHsql(data).SetMaxResults(showRegs).List<Product>();
                    else
                        datos = GetHsql(data).List<Product>();

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
            StringBuilder sql = new StringBuilder("select a from Product a   ");
            Product product = (Product)data;


            if (product != null)
            {
                Parms = new List<Object[]>();


                if (product.ProductAccounts != null && product.ProductAccounts.Count > 0)
                {
                    sql.Append(" LEFT OUTER JOIN a.ProductAccounts as prodAccount "); //where a.ProductID =  prodAccount.Product.ProductID ");

                    ProductAccountRelation par = product.ProductAccounts[0];

                    sql.Append(" WHERE (( 1=1 AND prodAccount.RowID is not null ");

                    //Solo se condiciona si pasa un account.
                    if (par.Account != null && par.Account.AccountCode != WmsSetupValues.DEFAULT)
                    {
                        sql.Append(" AND prodAccount.AccountType.AccountTypeID = :act");
                        Parms.Add(new Object[] { "act", par.AccountType.AccountTypeID });

                        sql.Append(" AND prodAccount.Account.AccountID  = :acc");
                        Parms.Add(new Object[] { "acc", par.Account.AccountID });
                    }

                    sql.Append(" AND (prodAccount.ItemNumber = :itm" + " OR prodAccount.Code1 = :itm" + " OR prodAccount.Code2 = :itm" + ")) ");
                    Parms.Add(new Object[] { "itm", par.ItemNumber });


                    if (!String.IsNullOrEmpty(product.ProductCode))
                    {
                        sql.Append(" OR (a.ProductCode = :nom  OR a.UpcCode = :nom OR a.DefVendorNumber = :nom )) and ");
                        Parms.Add(new Object[] { "nom", product.ProductCode });
                    }

                    else
                        sql.Append(" ) and   ");

                }
                else
                {
                    sql.Append(" where ");

                    if (!String.IsNullOrEmpty(product.ProductCode))
                    {
                        sql.Append(" (a.ProductCode = :nom  OR a.UpcCode = :nom )  and   ");
                        Parms.Add(new Object[] { "nom", product.ProductCode });
                    }


                }



                if (product.ProductID != 0)
                {
                    sql.Append(" a.ProductID = :id     and   ");
                    Parms.Add(new Object[] { "id", product.ProductID });
                }

                if (product.Company != null && product.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", product.Company.CompanyID });
                }


                if (product.Contract != null && product.Contract.ContractID != 0)
                {
                    sql.Append(" a.Contract.ContractID  = :cot1     and   ");
                    Parms.Add(new Object[] { "cot", product.Contract.ContractID });
                }



                if (!String.IsNullOrEmpty(product.Name))
                {
                    sql.Append(" (a.ProductCode like :nom1 Or  a.UpcCode = :nomp  ");

                    if (product.Name.Length >= 3)
                    {
                        sql.Append(" Or a.Name like :nom2 ");
                        Parms.Add(new Object[] { "nom2", "%" + product.Name + "%" });
                    }

                    sql.Append(" ) and   ");

                    Parms.Add(new Object[] { "nom1", product.Name + "%" });
                    Parms.Add(new Object[] { "nomp", product.Name });

                }


                if (!String.IsNullOrEmpty(product.Description))
                {
                    sql.Append(" a.Description like :nom2  and   ");
                    Parms.Add(new Object[] { "nom2", "%" + product.Description + "%" });
                }

                if (!String.IsNullOrEmpty(product.UpcCode))
                {
                    sql.Append(" a.UpcCode = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", product.UpcCode });
                }

                if (!String.IsNullOrEmpty(product.Brand))
                {
                    sql.Append(" a.Brand = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", product.Brand });
                }

                if (!String.IsNullOrEmpty(product.Reference))
                {
                    sql.Append(" a.Reference LIKE :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", "%" + product.Reference + "%" });
                }

                if (!String.IsNullOrEmpty(product.Manufacturer))
                {
                    sql.Append(" a.Manufacturer = :nom6     and   "); 
                    Parms.Add(new Object[] { "nom6", product.Manufacturer });
                }

                if (product.BaseUnit != null && product.BaseUnit.UnitID != 0)
                {
                    sql.Append(" a.BaseUnit.UnitID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", product.BaseUnit.UnitID });
                }

                if (product.Status != null && product.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", product.Status.StatusID });
                }


                if (product.DefaultTemplate != null && product.DefaultTemplate.RowID != 0)
                {
                    sql.Append(" a.DefaultTemplate.RowID = :idt3     and   ");
                    Parms.Add(new Object[] { "idt3", product.DefaultTemplate.RowID });
                }


                if (product.IsKit == true)
                {
                    sql.Append(" a.IsKit = :nom9     and   ");
                    Parms.Add(new Object[] { "nom9", true });
                }
                else if (product.IsKit == false)
                {
                    sql.Append(" a.IsKit = :nom9     and   ");
                    Parms.Add(new Object[] { "nom9", false });
                }


                if (product.Category != null && product.Category.CategoryID != 0)
                {
                    sql.Append(" a.Category.CategoryID = :pc1     and   ");
                    Parms.Add(new Object[] { "pc1", product.Category.CategoryID });
                }


                if (product.PickMethod != null && product.PickMethod.MethodID != 0)
                {
                    sql.Append(" a.PickMethod.MethodID = :ip3    and   ");
                    Parms.Add(new Object[] { "ip3", product.PickMethod.MethodID });
                }

                if (product.CountRank  != 0)
                {
                    sql.Append(" a.CountRank = :icr3    and   ");
                    Parms.Add(new Object[] { "icr3", product.CountRank });
                }
 
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Name asc ");

            //Console.WriteLine(sql.ToString());

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}