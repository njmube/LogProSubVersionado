using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoDataType : DaoService
    {
        public DaoDataType(DaoFactory factory) : base(factory) { }

        public DataType Save(DataType data)
        {
            return (DataType)base.Save(data);
        }


        public Boolean Update(DataType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DataType data)
        {
            return base.Delete(data);
        }


        public DataType SelectById(DataType data)
        {
            return (DataType)base.SelectById(data);
        }


        public IList<DataType> Select(DataType data)
        {
            IList<DataType> datos = new List<DataType>();
            datos = GetHsql(data).List<DataType>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from DataType a    where  ");
            DataType DataType = (DataType)data;
            if (DataType != null)
            {
                Parms = new List<Object[]>();
                if (DataType.DataTypeID != 0)
                {
                    sql.Append(" a.DataTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", DataType.DataTypeID });
                }

		        if (!String.IsNullOrEmpty(DataType.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", DataType.Name });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.DataTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}