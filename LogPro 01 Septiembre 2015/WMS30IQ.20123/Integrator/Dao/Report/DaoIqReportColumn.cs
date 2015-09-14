using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;

namespace Integrator.Dao.Report
{
    public class DaoIqReportColumn : DaoService
    {
        public DaoIqReportColumn(DaoFactory factory) : base(factory) { }

        public IqReportColumn Save(IqReportColumn data)
        {
            return (IqReportColumn)base.Save(data);
        }


        public Boolean Update(IqReportColumn data)
        {
            return base.Update(data);
        }


        public Boolean Delete(IqReportColumn data)
        {
            return base.Delete(data);
        }


        public IqReportColumn SelectById(IqReportColumn data)
        {
            return (IqReportColumn)base.SelectById(data);
        }




        public IList<IqReportColumn> Select(IqReportColumn data)
        {
            IList<IqReportColumn> datos = new List<IqReportColumn>();
            datos = GetHsql(data).List<IqReportColumn>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from IqReportColumn a where ");
            IqReportColumn iqreportcolumn = (IqReportColumn)data;
            if (iqreportcolumn != null)
            {
                Parms = new List<Object[]>();
                if (iqreportcolumn.ReportColumnId != 0)
                {
                    sql.Append(" a.ReportColumnId = :id     and   ");
                    Parms.Add(new Object[] { "id", iqreportcolumn.ReportColumnId });
                }

                //if (iqreportcolumn.IsSelected != null)
                //{
                    sql.Append(" a.IsSelected = :nom2 and  ");
                    Parms.Add(new Object[] { "nom2", iqreportcolumn.IsSelected });
                //}

                if (!String.IsNullOrEmpty(iqreportcolumn.Alias))
                {
                    sql.Append(" a.Alias = :nom3  and  ");
                    Parms.Add(new Object[] { "nom3", iqreportcolumn.Alias });
                }

                if (iqreportcolumn.IsFiltered != null)
                {
                    sql.Append(" a.IsFiltered = :nom4 and  ");
                    Parms.Add(new Object[] { "nom4", iqreportcolumn.IsFiltered });
                }

                if (!String.IsNullOrEmpty(iqreportcolumn.FilteredValue))
                {
                    sql.Append(" a.FilteredValue = :nom5  and  ");
                    Parms.Add(new Object[] { "nom5", iqreportcolumn.FilteredValue });
                }

                if (iqreportcolumn.Column != null && iqreportcolumn.Column.ColumnId != 0)
                {
                    sql.Append(" a.Column.ColumnId = :idd6     and   ");
                    Parms.Add(new Object[] { "idd6", iqreportcolumn.Column.ColumnId });
                }

                if (iqreportcolumn.ReportTable != null && iqreportcolumn.ReportTable.ReportTableId != 0)
                {
                    sql.Append(" a.ReportTable.ReportTableId = :idd7     and   ");
                    Parms.Add(new Object[] { "idd7", iqreportcolumn.ReportTable.ReportTableId });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ReportColumnId asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}