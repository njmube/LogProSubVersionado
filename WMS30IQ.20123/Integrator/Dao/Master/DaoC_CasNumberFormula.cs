using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoC_CasNumberFormula : DaoService
    {
        public DaoC_CasNumberFormula(DaoFactory factory) : base(factory) { }

        public C_CasNumberFormula Save(C_CasNumberFormula data)
        {
            return (C_CasNumberFormula)base.Save(data);
        }


        public Boolean Update(C_CasNumberFormula data)
        {
            return base.Update(data);
        }


        public Boolean Delete(C_CasNumberFormula data)
        {
            return base.Delete(data);
        }


        public C_CasNumberFormula SelectById(C_CasNumberFormula data)
        {
            return (C_CasNumberFormula)base.SelectById(data);
        }


        public IList<C_CasNumberFormula> Select(C_CasNumberFormula data)
        {

                IList<C_CasNumberFormula> datos = new List<C_CasNumberFormula>();

                datos = GetHsql(data).List<C_CasNumberFormula>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from C_CasNumberFormula a    where  ");
            C_CasNumberFormula C_CasNumberFormula = (C_CasNumberFormula)data;
            if (C_CasNumberFormula != null)
            {
                Parms = new List<Object[]>();
                if (C_CasNumberFormula.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", C_CasNumberFormula.RowID });
                }

                if (C_CasNumberFormula.Product != null && C_CasNumberFormula.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :idp1     and   ");
                    Parms.Add(new Object[] { "idp1", C_CasNumberFormula.Product.ProductID });
                }

                if (C_CasNumberFormula.CasNumberComponent != null && C_CasNumberFormula.CasNumberComponent.CasNumberID != 0)
                {
                    sql.Append(" a.CasNumberComponent.CasNumberID = :idc1     and   ");
                    Parms.Add(new Object[] { "idc1", C_CasNumberFormula.CasNumberComponent.CasNumberID });
                }

                if (C_CasNumberFormula.CasNumberComponent != null && !string.IsNullOrEmpty(C_CasNumberFormula.CasNumberComponent.Code))
                {
                    sql.Append(" a.CasNumberComponent.Code = :idz1     and   ");
                    Parms.Add(new Object[] { "idz1", C_CasNumberFormula.CasNumberComponent.Code });
                }

                if (C_CasNumberFormula.Product != null &&  !string.IsNullOrEmpty(C_CasNumberFormula.Product.ProductCode))
                {
                    sql.Append(" a.Product.ProductCode = :idy1     and   ");
                    Parms.Add(new Object[] { "idy1", C_CasNumberFormula.Product.ProductCode });
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