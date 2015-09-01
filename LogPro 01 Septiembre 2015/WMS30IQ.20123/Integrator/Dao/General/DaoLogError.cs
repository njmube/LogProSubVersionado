using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoLogError : DaoService
    {
        public DaoLogError(DaoFactory factory) : base(factory) { }

        public LogError Save(LogError data)
        {
            return (LogError)base.Save(data);
        }


        public Boolean Update(LogError data)
        {
            return base.Update(data);
        }


        public Boolean Delete(LogError data)
        {
            return base.Delete(data);
        }


        public LogError SelectById(LogError data)
        {
            return (LogError)base.SelectById(data);
        }


        public IList<LogError> Select(LogError data)
        {

                IList<LogError> datos = new List<LogError>();
                datos = GetHsql(data).List<LogError>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from LogError a    where  ");
            LogError logerror = (LogError)data;
            if (logerror != null)
            {
                Parms = new List<Object[]>();
                if (logerror.LogErrorID != 0)
                {
                    sql.Append(" a.LogErrorID = :id     and   ");
                    Parms.Add(new Object[] { "id", logerror.LogErrorID });
                }

		        if (!String.IsNullOrEmpty(logerror.Category))
                {
                    sql.Append(" a.Category = :nom     and   ");
                    Parms.Add(new Object[] { "nom", logerror.Category });
                }

                if (!String.IsNullOrEmpty(logerror.UserError))
                {
                    sql.Append(" a.UserError = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", logerror.UserError });
                }

                if (!String.IsNullOrEmpty(logerror.CreatedBy))
                {
                    sql.Append(" a.CreatedBy = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", logerror.CreatedBy });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.LogErrorID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



        public IList<long> SelectRulesMessage(LogError data, Entities.Report.MessageRuleByCompany rule)
        {
            string sQuery = "";

            sQuery = "select LogErrorID from General.LogError a left outer join Report.MessagePool m on a.LogErrorID = m.RecordID " +
               " and m.EntityID = :idx1 and m.RuleID  = :rule Where  " +
               "  ( a.CreationDate >= :dtm1 or a.ModDate  >= :dtm1 ) and m.RecordID is null ";


            StringBuilder sql = new StringBuilder(sQuery);

            if (data != null)
            {
                Parms = new List<Object[]>();
                Parms.Add(new Object[] { "dtm1", DateTime.Today.AddDays(-5) }); //Limita a enviar solo los del dia
                Parms.Add(new Object[] { "idx1", EntityID.LogError });
                Parms.Add(new Object[] { "rule", rule.RowID }); //Rule a ejecutar



                if (! String.IsNullOrEmpty(data.Category))
                {
                    sql.Append(" and a.Category = :id3   ");
                    Parms.Add(new Object[] { "id3", data.Category });

                }


            }

            sql = new StringBuilder(sql.ToString());

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);
            return query.List<long>();
        }
    }
}