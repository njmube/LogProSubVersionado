using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;
using System.Data;
using System.Linq;

namespace Integrator.Dao.Report
{
    public class DaoIqReport : DaoService
    {
        public DaoIqReport(DaoFactory factory) : base(factory) { }

        public IqReport Save(IqReport data)
        {
            return (IqReport)base.Save(data);
        }


        public Boolean Update(IqReport data)
        {
            return base.Update(data);
        }


        public Boolean Delete(IqReport data)
        {
            return base.Delete(data);
        }


        public IqReport SelectById(IqReport data)
        {
            return (IqReport)base.SelectById(data);
        }




        public IList<IqReport> Select(IqReport data)
        {
            IList<IqReport> datos = new List<IqReport>();

            try { 
            datos = GetHsql(data).List<IqReport>();
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
            StringBuilder sql = new StringBuilder("select a from IqReport a where ");
            IqReport iqreport = (IqReport)data;
            if (iqreport != null)
            {
                Parms = new List<Object[]>();

                if (iqreport.ReportId != 0)
                {
                    sql.Append(" a.ReportId = :id     and   ");
                    Parms.Add(new Object[] { "id", iqreport.ReportId });
                }

                if (!String.IsNullOrEmpty(iqreport.Name))
                {
                    sql.Append(" a.Name = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", iqreport.Name });
                }

                if (iqreport.IsForSystem != null)
                {
                    sql.Append(" a.IsForSystem = :nom3  and  ");
                    Parms.Add(new Object[] { "nom3", iqreport.IsForSystem });
                }

                if (iqreport.Status != null && iqreport.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :stid     and   ");
                    Parms.Add(new Object[] { "stid", iqreport.Status.StatusID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.ReportId asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public DataSet GetReportObject(string dataQuery, DataSet rpParams)
        {

            StringBuilder sql = new StringBuilder(dataQuery);
            Parms = GetParams(rpParams);

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());

            if (Parms.Count > 0)
                SetParameters(query);

            IList<Object[]> list = query.List<Object[]>();

            if (list == null || list.Count == 0)
                return null;

            //Obtaining Columns
            IList<string> cols = GetColumnList(dataQuery);

            DataTable dt = GetDataTableSchema(cols, "dt0", list);

            DataSet ds = new DataSet("dsResult");
            ds.Tables.Add(dt);
            return ds;
        }


        private IList<object[]> GetParams(DataSet rpParams)
        {
            IList<object[]> rp = new List<object[]>();

            if (rpParams == null)
                return rp;

            try
            {
                foreach (DataRow dr in rpParams.Tables[0].Rows)
                    rp.Add(new object[] { dr[0], dr[1] });

            }
            catch { return rp; }
            return rp;

        }



        private IList<string> GetColumnList(string dataQuery)
        {
            dataQuery = dataQuery.Substring(0, dataQuery.IndexOf("FROM"));
            
            string[] step1 = dataQuery.Replace("Select", "").Split(',');

            IList<string> list = new  List<string>();

            for (int i = 0; i < step1.Length; i++)
            {
                try
                {
                    if (step1[i].IndexOf(" as ") > 0)
                        list.Add(step1[i].Substring(step1[i].IndexOf(" as ") + 4).Trim());
                }
                catch { }
            }

            return list;
        }



        private DataTable GetDataTableSchema(IList<string> cols, string tableName, IList<object[]> list)
        {
            DataTable dt = new DataTable(tableName);
            
            DataColumn dc = null;
            for (int i = 0; i < cols.Count; i++)
            {
                try
                {
                    dc = new DataColumn(cols[i].Trim(), list.First()[i].GetType());
                    dt.Columns.Add(dc);
                }
                catch
                {
                    dt.Columns.Add(cols[i].Trim());       
                }
            }


            DataRow dr;

            //Tabla Principal
            foreach (Object[] objArray in list)
            {
                dr = dt.NewRow();

                for (int i = 0; i < cols.Count; i++)
                {
                    try { dr[cols[i].Trim()] = objArray[i]; }
                    catch { }
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

    }
}