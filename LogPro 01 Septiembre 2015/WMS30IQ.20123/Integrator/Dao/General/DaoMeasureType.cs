using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoMeasureType : DaoService
    {
        public DaoMeasureType(DaoFactory factory) : base(factory) { }

        public MeasureType Save(MeasureType data)
        {
            return (MeasureType)base.Save(data);
        }


        public Boolean Update(MeasureType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(MeasureType data)
        {
            return base.Delete(data);
        }


        public MeasureType SelectById(MeasureType data)
        {
            return (MeasureType)base.SelectById(data);
        }


        public IList<MeasureType> Select(MeasureType data)
        {

                IList<MeasureType> datos = new List<MeasureType>();
                datos = GetHsql(data).List<MeasureType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MeasureType a    where  ");
            MeasureType measuretype = (MeasureType)data;
            if (measuretype != null)
            {
                Parms = new List<Object[]>();
                if (measuretype.MeasureTypeID != 0)
                {
                    sql.Append(" a.MeasureTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", measuretype.MeasureTypeID });
                }

		        if (!String.IsNullOrEmpty(measuretype.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", measuretype.Name });
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.measureTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}