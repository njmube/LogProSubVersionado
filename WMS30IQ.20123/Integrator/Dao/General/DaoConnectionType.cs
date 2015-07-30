using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoConnectionType : DaoService
    {
        public DaoConnectionType(DaoFactory factory) : base(factory) { }


        public ConnectionType Save(ConnectionType data)
        {
            return (ConnectionType)base.Save(data);
        }


        public Boolean Update(ConnectionType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ConnectionType data)
        {
            return base.Delete(data);
        }


        public ConnectionType SelectById(ConnectionType data)
        {
            return (ConnectionType)base.SelectById(data);
        }


        public IList<ConnectionType> Select(ConnectionType data)
        {

                IList<ConnectionType> datos = new List<ConnectionType>();

                datos = GetHsql(data).List<ConnectionType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
  
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ConnectionType a where  ");
            ConnectionType ConnectionType = (ConnectionType)data;
            if (ConnectionType != null)
            {
                Parms = new List<Object[]>();
                if (ConnectionType.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", ConnectionType.RowID });
                }

                if (ConnectionType.IsEditable != null)
                {
                    sql.Append(" a.IsEditable = :edt  and    ");
                    Parms.Add(new Object[] { "edt", ConnectionType.IsEditable });
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