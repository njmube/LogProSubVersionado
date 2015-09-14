using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;
using Entities.Process;
using System.Linq;

namespace Integrator.Dao.Master
{
    public class DaoCustomProcessContext : DaoService
    {
        public DaoCustomProcessContext(DaoFactory factory) : base(factory) { }

        public CustomProcessContext Save(CustomProcessContext data)
        {
            return (CustomProcessContext)base.Save(data);
        }


        public Boolean Update(CustomProcessContext data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcessContext data)
        {
            return base.Delete(data);
        }


        public CustomProcessContext SelectById(CustomProcessContext data)
        {
            return (CustomProcessContext)base.SelectById(data);
        }


        public IList<CustomProcessContext> Select(CustomProcessContext data)
        {

                IList<CustomProcessContext> datos = new List<CustomProcessContext>();

                datos = GetHsql(data).List<CustomProcessContext>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                try { datos = datos.Where(f => f.GetType() == typeof(CustomProcessContext)).ToList(); }
                catch { }
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcessContext a    where  ");
            CustomProcessContext custEr = (CustomProcessContext)data;
            if (custEr != null)
            {
                Parms = new List<Object[]>();
                if (custEr.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", custEr.RowID });
                }

                if (!string.IsNullOrEmpty(custEr.ContextKey))
                {
                    sql.Append(" a.ContextKey like :ir2     and   ");
                    Parms.Add(new Object[] { "ir2", "%" + custEr.ContextKey + "%" });
                }

                if (custEr.Status != null && custEr.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", custEr.Status.StatusID });
                }

                if (custEr.ProcessType != null && custEr.ProcessType.DocTypeID != 0)
                {
                    sql.Append(" a.ProcessType.DocTypeID = :ix2     and   ");
                    Parms.Add(new Object[] { "ix2", custEr.ProcessType.DocTypeID });
                }

            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}