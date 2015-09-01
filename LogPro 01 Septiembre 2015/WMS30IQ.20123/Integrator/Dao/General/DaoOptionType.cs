using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoOptionType : DaoService
    {
        public DaoOptionType(DaoFactory factory) : base(factory) { }

        public OptionType Save(OptionType data)
        {
            return (OptionType)base.Save(data);
        }


        public Boolean Update(OptionType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(OptionType data)
        {
            return base.Delete(data);
        }


        public OptionType SelectById(OptionType data)
        {
            return (OptionType)base.SelectById(data);
        }


        public IList<OptionType> Select(OptionType data)
        {
            IList<OptionType> datos = new List<OptionType>();
            datos = GetHsql(data).List<OptionType>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from OptionType a    where  ");
            OptionType OptionType = (OptionType)data;
            if (OptionType != null)
            {
                Parms = new List<Object[]>();
                if (OptionType.OpTypeID != 0)
                {
                    sql.Append(" a.OpTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", OptionType.OpTypeID });
                }

		        if (!String.IsNullOrEmpty(OptionType.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", OptionType.Name });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.OpTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}