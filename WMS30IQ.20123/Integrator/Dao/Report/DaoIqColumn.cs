using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.Report
{
    public class DaoIqColumn : DaoService
    {
        public DaoIqColumn(DaoFactory factory) : base(factory) { }

        public IqColumn Save(IqColumn data)
        {
            return (IqColumn)base.Save(data);
        }


        public Boolean Update(IqColumn data)
        {
            return base.Update(data);
        }


        public Boolean Delete(IqColumn data)
        {
            return base.Delete(data);
        }


        public IqColumn SelectById(IqColumn data)
        {
            return (IqColumn)base.SelectById(data);
        }




        public IList<IqColumn> Select(IqColumn data)
        {
            IList<IqColumn> datos = new List<IqColumn>();
            datos = GetHsql(data).List<IqColumn>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from IqColumn a where ");
            IqColumn iqcolumn = (IqColumn)data;
            if (iqcolumn != null)
            {
                Parms = new List<Object[]>();
                if (iqcolumn.ColumnId != 0)
                {
                    sql.Append(" a.ColumnId = :id     and   ");
                    Parms.Add(new Object[] { "id", iqcolumn.ColumnId });
                }

                if (!String.IsNullOrEmpty(iqcolumn.Name))
                {
                    sql.Append(" a.Name = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", iqcolumn.Name });
                }

                if (!String.IsNullOrEmpty(iqcolumn.DbType))
                {
                    sql.Append(" a.DbType = :nom3  and  ");
                    Parms.Add(new Object[] { "nom3", iqcolumn.DbType });
                }

                if (iqcolumn.Table != null && iqcolumn.Table.TableId != 0)
                {
                    sql.Append(" a.Table.TableId = :idt     and   ");
                    Parms.Add(new Object[] { "idt", iqcolumn.Table.TableId });
                }


            }



            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ColumnId asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}