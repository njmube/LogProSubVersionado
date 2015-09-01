using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoMeasureUnitConvertion : DaoService
    {
        public DaoMeasureUnitConvertion(DaoFactory factory) : base(factory) { }

        public MeasureUnitConvertion Save(MeasureUnitConvertion data)
        {
            return (MeasureUnitConvertion)base.Save(data);
        }


        public Boolean Update(MeasureUnitConvertion data)
        {
            return base.Update(data);
        }


        public Boolean Delete(MeasureUnitConvertion data)
        {
            return base.Delete(data);
        }


        public MeasureUnitConvertion SelectById(MeasureUnitConvertion data)
        {
            return (MeasureUnitConvertion)base.SelectById(data);
        }


        public IList<MeasureUnitConvertion> Select(MeasureUnitConvertion data)
        {

                IList<MeasureUnitConvertion> datos = new List<MeasureUnitConvertion>();
                datos = GetHsql(data).List<MeasureUnitConvertion>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MeasureUnitConvertion a    where  ");
            MeasureUnitConvertion measureunitconvertion = (MeasureUnitConvertion)data;
            if (measureunitconvertion != null)
            {
                Parms = new List<Object[]>();
                if (measureunitconvertion.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", measureunitconvertion.RowID });
                }

                if (measureunitconvertion.DestinationUnit.MeasureUnitID != 0)
                {
                    sql.Append(" a.DestinationUnit.MeasureUnitID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", measureunitconvertion.DestinationUnit.MeasureUnitID });
                }

                if ( measureunitconvertion.SourceUnit.MeasureUnitID != 0)
                {
                    sql.Append(" a.SourceUnit.MeasureUnitID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", measureunitconvertion.SourceUnit.MeasureUnitID });
                }

                if (!String.IsNullOrEmpty(measureunitconvertion.Description))
                {
                    sql.Append(" a.Description = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", measureunitconvertion.Description });
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