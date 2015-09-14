using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoKitAssembly : DaoService
    {
        public DaoKitAssembly(DaoFactory factory) : base(factory) { }

        public KitAssembly Save(KitAssembly data)
        {
            return (KitAssembly)base.Save(data);
        }


        public Boolean Update(KitAssembly data)
        {
            return base.Update(data);
        }


        public Boolean Delete(KitAssembly data)
        {
            return base.Delete(data);
        }


        public KitAssembly SelectById(KitAssembly data)
        {
            return (KitAssembly)base.SelectById(data);
        }


        public IList<KitAssembly> Select(KitAssembly data, int showRegs)
        {
            IList<KitAssembly> datos = new List<KitAssembly>();

            /*
                datos = GetHsql(data).List<KitAssembly>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            */
                try
                {
                    if (showRegs > 0)
                        datos = GetHsql(data).SetMaxResults(showRegs).List<KitAssembly>();
                    else
                        datos = GetHsql(data).List<KitAssembly>();

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
            StringBuilder sql = new StringBuilder("select a from KitAssembly a where  ");
            KitAssembly kitasm = (KitAssembly)data;
            if (kitasm != null)
            {
                Parms = new List<Object[]>();

                if (kitasm.RowID != 0)
                {
                    sql.Append(" a.RowID = :idx     and   ");
                    Parms.Add(new Object[] { "idx", kitasm.RowID });
                }

                if (kitasm.Product != null)
                {
                    if (kitasm.Product.ProductID != 0)
                    {
                        sql.Append(" a.Product.ProductID = :id     and   ");
                        Parms.Add(new Object[] { "id", kitasm.Product.ProductID });
                    }


                    if (!String.IsNullOrEmpty(kitasm.Product.Name))
                    {
                        sql.Append(" a.Product.Name LIKE :idN  OR (a.Product.ProductCode LIKE :idN OR a.Product.Name LIKE :idN )   and   ");
                        Parms.Add(new Object[] { "idN", "%" + kitasm.Product.Name + "%" });
                    }

                    //if (!String.IsNullOrEmpty(kitasm.Product.ProductCode))
                    //{
                    //    sql.Append(" (a.Product.ProductCode LIKE :idpc OR a.Product.Name LIKE :idpc )  and   ");
                    //    Parms.Add(new Object[] { "idpc", "%" + kitasm.Product.ProductCode + "%" });
                    //}

                    if (!String.IsNullOrEmpty(kitasm.Product.ProductCode))
                    {
                        sql.Append(" a.Product.ProductCode = :idpc  and   ");
                        Parms.Add(new Object[] { "idpc", kitasm.Product.ProductCode });
                    }


                    if (kitasm.Product.Company != null && kitasm.Product.Company.CompanyID != 0)
                    {
                        sql.Append(" a.Product.Company.CompanyID = :idCia    and   ");
                        Parms.Add(new Object[] { "idCia", kitasm.Product.Company.CompanyID });
                    }

                }


                if (kitasm.AsmType != 0)
                {
                    sql.Append(" a.AsmType = :id1     and   ");
                    Parms.Add(new Object[] { "id1", kitasm.AsmType });
                }


                if (kitasm.Status != null && kitasm.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", kitasm.Status.StatusID });
                }

                if (kitasm.Method != 0)
                {
                    sql.Append(" a.Method = :id2     and   ");
                    Parms.Add(new Object[] { "id2", kitasm.Method });
                }

                if (kitasm.Unit != null && kitasm.Unit.UnitID != 0)
                {
                    sql.Append(" a.Unit.UnitID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", kitasm.Unit.UnitID });
                }

                if (kitasm.ObsoleteDate != null )
                {
                    sql.Append(" a.ObsoleteDate = :id5     and   ");
                    Parms.Add(new Object[] { "id5", kitasm.ObsoleteDate });
                }


                if (kitasm.EfectiveDate != null)
                {
                    sql.Append(" a.EfectiveDate = :id6    and   ");
                    Parms.Add(new Object[] { "id6", kitasm.EfectiveDate });
                }

                if (kitasm.IsFromErp != null)
                {
                    sql.Append(" a.IsFromErp = :ie11     and   ");
                    Parms.Add(new Object[] { "ie11", kitasm.IsFromErp });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Product.ProductID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}