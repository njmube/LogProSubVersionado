using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoMeasureUnit : DaoService
    {
        public DaoMeasureUnit(DaoFactory factory) : base(factory) { }

        public MeasureUnit Save(MeasureUnit data)
        {
            return (MeasureUnit)base.Save(data);
        }


        public Boolean Update(MeasureUnit data)
        {
            return base.Update(data);
        }


        public Boolean Delete(MeasureUnit data)
        {
            return base.Delete(data);
        }


        public MeasureUnit SelectById(MeasureUnit data)
        {
            return (MeasureUnit)base.SelectById(data);
        }


        public IList<MeasureUnit> Select(MeasureUnit data)
        {
            IList<MeasureUnit> datos = new List<MeasureUnit>();

            datos = GetHsql(data).List<MeasureUnit>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MeasureUnit a    where  ");
            MeasureUnit measureunit = (MeasureUnit)data;
            if (measureunit != null)
            {
                Parms = new List<Object[]>();
                if (measureunit.MeasureUnitID != 0)
                {
                    sql.Append(" a.MeasureUnitID = :id     and   ");
                    Parms.Add(new Object[] { "id", measureunit.MeasureUnitID });
                }

                if (measureunit.MeasureType != null && measureunit.MeasureType.MeasureTypeID != 0)
                {
                    sql.Append(" a.MeasureType.MeasureTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", measureunit.MeasureType.MeasureTypeID });
                }

                if (!String.IsNullOrEmpty(measureunit.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", measureunit.Name });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.measureUnitID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}