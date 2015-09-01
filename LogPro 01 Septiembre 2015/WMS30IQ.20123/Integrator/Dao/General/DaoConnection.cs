using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoConnection : DaoService
    {
        public DaoConnection(DaoFactory factory) : base(factory) { }


        public Connection Save(Connection data)
        {
            return (Connection)base.Save(data);
        }


        public Boolean Update(Connection data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Connection data)
        {
            return base.Delete(data);
        }


        public Connection SelectById(Connection data)
        {
            return (Connection)base.SelectById(data);
        }


        public IList<Connection> Select(Connection data)
        {

                IList<Connection> datos = new List<Connection>();

                datos = GetHsql(data).List<Connection>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
  
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Connection a    where  ");
            Connection connection = (Connection)data;
            if (connection != null)
            {
                Parms = new List<Object[]>();
                if (connection.ConnectionID != 0)
                {
                    sql.Append(" a.ConnectionID = :id     and   ");
                    Parms.Add(new Object[] { "id", connection.ConnectionID });
                }


                if (!String.IsNullOrEmpty(connection.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", connection.Name });
                }


                if (connection.ConnectionType != null && connection.ConnectionType.RowID != 0)
                {
                    sql.Append(" a.ConnectionType.RowID = :cid1  and    ");
                    Parms.Add(new Object[] { "cid1", connection.ConnectionType.RowID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.ConnectionID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}