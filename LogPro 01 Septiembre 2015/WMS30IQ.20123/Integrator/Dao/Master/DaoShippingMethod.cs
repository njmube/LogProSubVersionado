using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoShippingMethod : DaoService
    {
        public DaoShippingMethod(DaoFactory factory) : base(factory) { }

        public ShippingMethod Save(ShippingMethod data)
        {
            return (ShippingMethod)base.Save(data);
        }


        public Boolean Update(ShippingMethod data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ShippingMethod data)
        {
            return base.Delete(data);
        }


        public ShippingMethod SelectById(ShippingMethod data)
        {
            return (ShippingMethod)base.SelectById(data);
        }


        public IList<ShippingMethod> Select(ShippingMethod data)
        {

                IList<ShippingMethod> datos = new List<ShippingMethod>();

                datos = GetHsql(data).List<ShippingMethod>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ShippingMethod a    where  ");
            ShippingMethod sh = (ShippingMethod)data;
            if (sh != null)
            {
                Parms = new List<Object[]>();
                if (sh.ShpMethodID != 0)
                {
                    sql.Append(" a.ShpMethodID = :id     and   ");
                    Parms.Add(new Object[] { "id", sh.ShpMethodID });
                }

                if (sh.Company != null && sh.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID= :id1     and   ");
                    Parms.Add(new Object[] { "id1", sh.Company.CompanyID });
                }

                if (!String.IsNullOrEmpty(sh.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", sh.Name });
                }

                if (!String.IsNullOrEmpty(sh.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", sh.ErpCode });
                }

            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.ShpMethodID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}