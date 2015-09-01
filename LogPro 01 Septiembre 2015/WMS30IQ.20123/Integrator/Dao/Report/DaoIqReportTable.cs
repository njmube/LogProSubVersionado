using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.Report
{
    public class DaoIqReportTable : DaoService
    {
        public DaoIqReportTable(DaoFactory factory) : base(factory) { }

        public IqReportTable Save(IqReportTable data)
        {
            return (IqReportTable)base.Save(data);
        }


        public Boolean Update(IqReportTable data)
        {
            return base.Update(data);
        }


        public Boolean Delete(IqReportTable data)
        {
            return base.Delete(data);
        }


        public IqReportTable SelectById(IqReportTable data)
        {
            return (IqReportTable)base.SelectById(data);
        }




        public IList<IqReportTable> Select(IqReportTable data)
        {
            IList<IqReportTable> datos = new List<IqReportTable>();
            datos = GetHsql(data).List<IqReportTable>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from IqReportTable a where ");
            IqReportTable iqreporttable = (IqReportTable)data;
            if (iqreporttable != null)
            {
                Parms = new List<Object[]>();
                if (iqreporttable.ReportTableId != 0)
                {
                    sql.Append(" a.ReportTableId = :id     and   ");
                    Parms.Add(new Object[] { "id", iqreporttable.ReportTableId });
                }

                if (!String.IsNullOrEmpty(iqreporttable.Alias))
                {
                    sql.Append(" a.Alias = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", iqreporttable.Alias });
                }

                if (iqreporttable.Secuence != 0)
                {
                    sql.Append(" a.Secuence = :id3 and  ");
                    Parms.Add(new Object[] { "id3", iqreporttable.Secuence });
                }

                if (!String.IsNullOrEmpty(iqreporttable.JoinQuery))
                {
                    sql.Append(" a.JoinQuery = :nom4  and  ");
                    Parms.Add(new Object[] { "nom4", iqreporttable.JoinQuery });
                }

                if (!String.IsNullOrEmpty(iqreporttable.WhereCondition))
                {
                    sql.Append(" a.WhereCondition = :nom5  and  ");
                    Parms.Add(new Object[] { "nom5", iqreporttable.WhereCondition });
                }

                if (iqreporttable.Report != null && iqreporttable.Report.ReportId != 0)
                {
                    sql.Append(" a.Report.ReportId = :idd6     and   ");
                    Parms.Add(new Object[] { "idd6", iqreporttable.Report.ReportId });
                }

                if (iqreporttable.Table != null && iqreporttable.Table.TableId != 0)
                {
                    sql.Append(" a.Table.TableId = :idd7     and   ");
                    Parms.Add(new Object[] { "idd7", iqreporttable.Table.TableId });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ReportTableId asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}