using System;
using System.Collections.Generic;
using System.Text;
using Integrator.Dao;
using NHibernate;
using Integrator.Config;
using Entities.Workflow;

namespace Integrator.Dao.Workflow
{
    public class DaoDataInformation  : DaoService
    {
        public DaoDataInformation (DaoFactory factory) : base(factory) { }

        public DataInformation  Save(DataInformation  data)
        {
            return (DataInformation )base.Save(data);
        }


        public Boolean Update(DataInformation  data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DataInformation  data)
        {
            return base.Delete(data);
        }


        public DataInformation  SelectById(DataInformation  data)
        {
            return (DataInformation )base.SelectById(data);
        }




        public IList<DataInformation > Select(DataInformation  data)
        {
            IList<DataInformation > datos = new List<DataInformation >();
            try { 
            datos = GetHsql(data).List<DataInformation >();
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
            StringBuilder sql = new StringBuilder("select a from DataInformation  a where ");
            DataInformation  DataInformation  = (DataInformation )data;
            if (DataInformation != null)
            {
                Parms = new List<Object[]>();
                if (DataInformation.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", DataInformation.RowID });
                }

                if (DataInformation.Entity != null && DataInformation.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :nom1  and  ");
                    Parms.Add(new Object[] { "nom1", DataInformation.Entity.ClassEntityID });
                }

                if (DataInformation.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", DataInformation.EntityRowID });
                }

                if (!String.IsNullOrEmpty(DataInformation.XmlData))
                {
                    sql.Append(" a.XmlData = :nom3  and  ");
                    Parms.Add(new Object[] { "nom3", DataInformation.XmlData });
                }

                if (!String.IsNullOrEmpty(DataInformation.ModTerminal))
                {
                    sql.Append(" a.ModTerminal = :mt3  and  ");
                    Parms.Add(new Object[] { "mt3", DataInformation.ModTerminal });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1  "); //order by a.RowID desc
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            //query.SetMaxResults(300);
            return query;
        }


    }
}