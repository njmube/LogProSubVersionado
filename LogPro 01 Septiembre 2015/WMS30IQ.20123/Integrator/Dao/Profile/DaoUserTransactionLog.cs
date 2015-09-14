using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Profile;

namespace Integrator.Dao.General
{
    public class DaoUserTransactionLog : DaoService
    {
        public DaoUserTransactionLog(DaoFactory factory) : base(factory) { }

        public UserTransactionLog Save(UserTransactionLog data)
        {
            return (UserTransactionLog)base.Save(data);
        }


        public Boolean Update(UserTransactionLog data)
        {
            return base.Update(data);
        }


        public Boolean Delete(UserTransactionLog data)
        {
            return base.Delete(data);
        }


        public UserTransactionLog SelectById(UserTransactionLog data)
        {
            return (UserTransactionLog)base.SelectById(data);
        }


        public IList<UserTransactionLog> Select(UserTransactionLog data)
        {

                IList<UserTransactionLog> datos = new List<UserTransactionLog>();
                datos = GetHsql(data).List<UserTransactionLog>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from UserTransactionLog a    where  ");
            UserTransactionLog usertransactionlog = (UserTransactionLog)data;
            if (usertransactionlog != null)
            {
                Parms = new List<Object[]>();
                if (usertransactionlog.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", usertransactionlog.RowID });
                }

                if (usertransactionlog.Company != null && usertransactionlog.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", usertransactionlog.Company.CompanyID });
                }

                if (usertransactionlog.Location != null && usertransactionlog.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", usertransactionlog.Location.LocationID });
                }

                if (usertransactionlog.Terminal != null && usertransactionlog.Terminal.TerminalID != 0)
                {
                    sql.Append(" a.Terminal.TerminalID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", usertransactionlog.Terminal.TerminalID });
                }

                if (usertransactionlog.DocType != null && usertransactionlog.DocType.DocTypeID != 0)
                {
                    sql.Append(" a.DocType.DocTypeID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", usertransactionlog.DocType.DocTypeID });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.RowID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}