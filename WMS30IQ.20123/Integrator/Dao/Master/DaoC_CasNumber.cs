using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoC_CasNumber : DaoService
    {
        public DaoC_CasNumber(DaoFactory factory) : base(factory) { }

        public C_CasNumber Save(C_CasNumber data)
        {
            return (C_CasNumber)base.Save(data);
        }


        public Boolean Update(C_CasNumber data)
        {
            return base.Update(data);
        }


        public Boolean Delete(C_CasNumber data)
        {
            return base.Delete(data);
        }


        public C_CasNumber SelectById(C_CasNumber data)
        {
            return (C_CasNumber)base.SelectById(data);
        }


        public IList<C_CasNumber> Select(C_CasNumber data)
        {

                IList<C_CasNumber> datos = new List<C_CasNumber>();

                try
                {
                    datos = GetHsql(data).SetMaxResults(WmsSetupValues.NumRegs).List<C_CasNumber>();
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
            StringBuilder sql = new StringBuilder("select a from C_CasNumber a    where  ");
            C_CasNumber C_CasNumber = (C_CasNumber)data;
            if (C_CasNumber != null)
            {
                Parms = new List<Object[]>();
                if (C_CasNumber.CasNumberID != 0)
                {
                    sql.Append(" a.CasNumberID = :id     and   ");
                    Parms.Add(new Object[] { "id", C_CasNumber.CasNumberID });
                }

                if (!String.IsNullOrEmpty(C_CasNumber.Name))
                {
                    sql.Append(" (a.Name like :nom  OR a.Code like :nomz)  and   "); 
                    Parms.Add(new Object[] { "nom", "%" + C_CasNumber.Name + "%" });
                    Parms.Add(new Object[] { "nomz",  C_CasNumber.Name + "%" });
                }

                if (!String.IsNullOrEmpty(C_CasNumber.Code))
                {
                    sql.Append(" a.Code = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", C_CasNumber.Code });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.CasNumberID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);            
            return query;
        }


    }
}