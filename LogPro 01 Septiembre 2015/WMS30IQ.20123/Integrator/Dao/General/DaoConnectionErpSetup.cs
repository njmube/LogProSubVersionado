using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoConnectionErpSetup : DaoService
    {
        public DaoConnectionErpSetup(DaoFactory factory) : base(factory) { }


        public ConnectionErpSetup Save(ConnectionErpSetup data)
        {
            return (ConnectionErpSetup)base.Save(data);
        }


        public Boolean Update(ConnectionErpSetup data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ConnectionErpSetup data)
        {
            return base.Delete(data);
        }


        public ConnectionErpSetup SelectById(ConnectionErpSetup data)
        {
            return (ConnectionErpSetup)base.SelectById(data);
        }


        public IList<ConnectionErpSetup> Select(ConnectionErpSetup data)
        {

                IList<ConnectionErpSetup> datos = new List<ConnectionErpSetup>();

                datos = GetHsql(data).List<ConnectionErpSetup>();
                //if (!Factory.IsTransactional)
                    //Factory.Commit();
                return datos;
  
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ConnectionErpSetup a where  ");
            ConnectionErpSetup ConnectionErpSetup = (ConnectionErpSetup)data;
            if (ConnectionErpSetup != null)
            {
                Parms = new List<Object[]>();
                if (ConnectionErpSetup.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", ConnectionErpSetup.RowID });
                }

                if (ConnectionErpSetup.ConnectionTypeID != 0)
                {
                    sql.Append(" a.ConnectionTypeID = :edt  and    ");
                    Parms.Add(new Object[] { "edt", ConnectionErpSetup.ConnectionTypeID });
                }

                if (!string.IsNullOrEmpty(ConnectionErpSetup.EntityCode))
                {
                    sql.Append(" a.EntityCode = :idc     and   ");
                    Parms.Add(new Object[] { "idc", ConnectionErpSetup.EntityCode });
                }

                if (ConnectionErpSetup.EntityType != 0)
                {
                    sql.Append(" a.EntityType = :ide     and   ");
                    Parms.Add(new Object[] { "ide", ConnectionErpSetup.EntityType });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}