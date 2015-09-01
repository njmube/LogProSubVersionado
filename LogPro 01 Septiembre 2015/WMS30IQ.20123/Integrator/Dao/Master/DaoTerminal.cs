using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoTerminal : DaoService
    {
        public DaoTerminal(DaoFactory factory) : base(factory) { }

        public Terminal Save(Terminal data)
        {
            return (Terminal)base.Save(data);
        }


        public Boolean Update(Terminal data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Terminal data)
        {
            return base.Delete(data);
        }


        public Terminal SelectById(Terminal data)
        {
            return (Terminal)base.SelectById(data);
        }


        public IList<Terminal> Select(Terminal data)
        {
                IList<Terminal> datos = new List<Terminal>();

                datos = GetHsql(data).List<Terminal>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Terminal a    where  ");
            Terminal terminal = (Terminal)data;
            if (terminal != null)
            {
                Parms = new List<Object[]>();
                if (terminal.TerminalID != 0)
                {
                    sql.Append(" a.TerminalID = :id     and   ");
                    Parms.Add(new Object[] { "id", terminal.TerminalID });
                }

                if (!string.IsNullOrEmpty(terminal.Name))
                {
                    sql.Append(" a.Name = :id1     and   ");
                    Parms.Add(new Object[] { "id1", terminal.Name });
                }

                if (!string.IsNullOrEmpty(terminal.Description))
                {
                    sql.Append(" a.Description = :id2     and   ");
                    Parms.Add(new Object[] { "id2", terminal.Description });
                }

                if (terminal.Status != null && terminal.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", terminal.Status.StatusID });
                }

                if (terminal.Location != null && terminal.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", terminal.Location.LocationID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.terminalID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}