using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoKitAssemblyFormula : DaoService
    {
        public DaoKitAssemblyFormula(DaoFactory factory) : base(factory) { }

        public KitAssemblyFormula Save(KitAssemblyFormula data)
        {
            return (KitAssemblyFormula)base.Save(data);
        }


        public Boolean Update(KitAssemblyFormula data)
        {
            return base.Update(data);
        }


        public Boolean Delete(KitAssemblyFormula data)
        {
            return base.Delete(data);
        }


        public KitAssemblyFormula SelectById(KitAssemblyFormula data)
        {
            return (KitAssemblyFormula)base.SelectById(data);
        }


        public IList<KitAssemblyFormula> Select(KitAssemblyFormula data)
        {
                IList<KitAssemblyFormula> datos = new List<KitAssemblyFormula>();

                datos = GetHsql(data).List<KitAssemblyFormula>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }




        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from KitAssemblyFormula a   where  ");
            KitAssemblyFormula kitasm = (KitAssemblyFormula)data;
            if (kitasm != null)
            {
                Parms = new List<Object[]>();
                if (kitasm.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", kitasm.RowID });
                }

                if (kitasm.KitAssembly != null )
                {
                    if (kitasm.KitAssembly.RowID != 0)
                    {
                        sql.Append(" a.KitAssembly.RowID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", kitasm.KitAssembly.RowID });
                    }

                    if (kitasm.KitAssembly.Product != null && kitasm.KitAssembly.Product.ProductID != 0)
                    {
                        sql.Append(" a.KitAssembly.Product.ProductID = :idp1     and   ");
                        Parms.Add(new Object[] { "idp1", kitasm.KitAssembly.Product.ProductID });
                    }

                    if (kitasm.KitAssembly.Product != null && ! string.IsNullOrEmpty(kitasm.KitAssembly.Product.ProductCode))
                    {
                        sql.Append(" a.KitAssembly.Product.ProductCode = :idpc     and   ");
                        Parms.Add(new Object[] { "idpc", kitasm.KitAssembly.Product.ProductCode });
                    }
                }

                if (kitasm.DirectProduct != null)
                {
                    if (kitasm.DirectProduct.ProductID != 0)
                    {
                        sql.Append(" a.DirectProduct.ProductID = :ipz1     and   ");
                        Parms.Add(new Object[] { "ipz1", kitasm.DirectProduct.ProductID });
                    }

                    if (kitasm.DirectProduct.Category != null && kitasm.DirectProduct.Category.ExplodeKit != 0)
                    {
                        sql.Append(" a.DirectProduct.Category.ExplodeKit = :ih1     and   ");
                        Parms.Add(new Object[] { "ih1", kitasm.DirectProduct.Category.ExplodeKit });
                    }


                }


                if (kitasm.Component != null && kitasm.Component.ProductID != 0)
                {
                    sql.Append(" a.Component.ProductID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", kitasm.Component.ProductID });
                }


                if (kitasm.Status != null && kitasm.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", kitasm.Status.StatusID });
                }


                if (kitasm.Unit != null && kitasm.Unit.UnitID != 0)
                {
                    sql.Append(" a.Unit.UnitID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", kitasm.Unit.UnitID });
                }

                if (kitasm.ObsoleteDate != null)
                {
                    sql.Append(" a.ObsoleteDate = :id5     and   ");
                    Parms.Add(new Object[] { "id5", kitasm.ObsoleteDate });
                }


                if (kitasm.EfectiveDate != null)
                {
                    sql.Append(" a.EfectiveDate = :id6    and   ");
                    Parms.Add(new Object[] { "id6", kitasm.EfectiveDate });
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