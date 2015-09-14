using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.Report
{
    public class DaoIqTable : DaoService
    {
        public DaoIqTable(DaoFactory factory) : base(factory) { }

        public IqTable Save(IqTable data)
        {
            return (IqTable)base.Save(data);
        }


        public Boolean Update(IqTable data)
        {
            return base.Update(data);
        }


        public Boolean Delete(IqTable data)
        {
            return base.Delete(data);
        }


        public IqTable SelectById(IqTable data)
        {
            return (IqTable)base.SelectById(data);
        }




        public IList<IqTable> Select(IqTable data)
        {
            IList<IqTable> datos = new List<IqTable>();
            datos = GetHsql(data).List<IqTable>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from IqTable a where ");
            IqTable iqtable = (IqTable)data;
            if (iqtable != null)
            {
                Parms = new List<Object[]>();
                if (iqtable.TableId != 0)
                {
                    sql.Append(" a.TableId = :id     and   ");
                    Parms.Add(new Object[] { "id", iqtable.TableId });
                }

                if (!String.IsNullOrEmpty(iqtable.Name))
                {
                    sql.Append(" a.Name = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", iqtable.Name });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.TableId asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}