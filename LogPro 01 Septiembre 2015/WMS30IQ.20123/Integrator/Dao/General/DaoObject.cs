using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using Integrator.Dao;
using Entities.General;
using System.Linq;
using System.Collections.Specialized;
using NHibernate.Transform;


namespace Integrator.Dao.General
{
    public class DaoObject : DaoService
    {
        public DaoObject(DaoFactory factory) : base(factory) { }

        public Object Save(Object data)
        {
            return (Object)base.Save(data);
        }


        public Boolean Update(Object data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Object data)
        {
            return base.Delete(data);
        }


        public Object SelectById(Object data)
        {
            return (Object)base.SelectById(data);
        }


        public Object Select(string hql, object fielID)
        {
            try { return SelectObject(hql, fielID).UniqueResult(); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        public override IQuery GetHsql(Object data)
        {
            return null;
        }


        private IQuery SelectObject(string hql, object fieldID)
        {
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id", fieldID });

            IQuery query = Factory.Session.CreateQuery(hql);
            SetParameters(query);
            return query;
        }



        public IList<ShowData> GetCustomList(string databaseObject, string field, string strWhere)
        {
            IList<ShowData> finalResult = new List<ShowData>();

            try
            {
                IQuery query = Factory.Session.CreateSQLQuery("SELECT TOP 10 " + field + ", DOB FROM  " + databaseObject + " " + strWhere); //
                IList<Object[]> result = query.List<Object[]>();


                if (result != null && result.Count > 0)

                    foreach (object[] s in result)
                    {
                        if (!finalResult.Any(f => f.DataKey == s[0].ToString()) && !string.IsNullOrEmpty(s[0].ToString()))
                            finalResult.Add(new ShowData { DataKey = s[0].ToString(), DataValue = s[1].ToString() });
                    }

            }
            catch { }

            return finalResult;
        }



        public IList<ShowData> GetCustomListV2(string sqlQuery)
        {
            IList<ShowData> finalResult = new List<ShowData>();

            try
            {
                IQuery query = Factory.Session.CreateSQLQuery(sqlQuery); //
                IList<Object[]> result = query.List<Object[]>();


                if (result != null && result.Count > 0)

                    foreach (object[] s in result)
                    {
                        if (!finalResult.Any(f => f.DataKey == s[0].ToString()) && !string.IsNullOrEmpty(s[0].ToString()))
                            finalResult.Add(new ShowData { DataKey = s[0].ToString(), DataValue = s[1].ToString() });
                    }

            }
            catch { }

            return finalResult;
        }



        public IList<ShowData> GetLanguage(string langCode)
        {
            //IQuery query = Factory.Session.
                //CreateSQLQuery("SELECT KeyCode as DataKey, [" + langCode + "] as DataValue FROM Language");

            /*string sqlQuery = "SELECT KeyCode as {ShowData.DataKey}, [" + langCode + "] as {ShowData.DataValue} FROM Language"; 

            var result = Factory.Session.CreateSQLQuery(sqlQuery)
                  .AddEntity("ShowData", typeof(ShowData))
                  .List<ShowData>();
             * 
             */

            try
            {
                string sqlQuery = "SELECT KeyCode as DataKey, [" + langCode + "] as DataValue FROM Language";

                var result = Factory.Session.CreateSQLQuery(sqlQuery)
                    .AddScalar("DataKey", NHibernateUtil.String)
                    .AddScalar("DataValue", NHibernateUtil.String)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(ShowData)))
                    .List<ShowData>();

                return result;
            }
            catch {
                return new List<ShowData>();
            }


            /*
            IList<KeyValuePair<string,string>> result = query.List<KeyValuePair<string,string>>();

            var myResult = (from r in result
                 select new ShowData
                 {
                    DataKey = r.Key,
                    DataValue = r.Value
                 }).ToList();

            return myResult;
             * */
        }


        public void PerformSpecificQuery(string sQuery)
        {
            Factory.Session.CreateSQLQuery(sQuery).ExecuteUpdate();
        }


        public object SelectSQL(string hql, object fieldID)
        {
            try { return SelectObjectSQL(hql, fieldID).UniqueResult(); }
            catch { return null; }
        }


        private IQuery SelectObjectSQL(string hql, object fieldID)
        {
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id", fieldID });

            IQuery query = Factory.Session.CreateSQLQuery(hql);
            SetParameters(query);
            return query;
        }

    }
}