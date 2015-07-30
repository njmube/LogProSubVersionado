using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using Integrator.Dao;
using Entities.Workflow;
using Integrator.Config;

namespace Integrator.Dao.Workflow
{
    public class DaoWFDataType : DaoService
    {
        public DaoWFDataType(DaoFactory factory) : base(factory) { }

        public WFDataType Save(WFDataType data)
        {
            return (WFDataType)base.Save(data);
        }


        public Boolean Update(WFDataType data)
        {
            return base.Update(data);
        }


        public Boolean Delete(WFDataType data)
        {
            return base.Delete(data);
        }


        public WFDataType SelectById(WFDataType data)
        {
            return (WFDataType)base.SelectById(data);
        }


        public IList<WFDataType> Select(WFDataType data)
        {
            IList<WFDataType> datos = new List<WFDataType>();
            try { 
            datos = GetHsql(data).List<WFDataType>();
            if (!Factory.IsTransactional)
                Factory.Commit();

            }
            catch (Exception e)
            {
                NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
            }
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from WFDataType a    where  ");
            WFDataType DataType = (WFDataType)data;
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

                if (DataType.IsBasic != null)
                {
                    sql.Append(" a.IsBasic = :nomf     and   ");
                    Parms.Add(new Object[] { "nomf", DataType.IsBasic });
                }

                if (!String.IsNullOrEmpty(DataType.UIListControl))
                {
                    sql.Append(" a.UIListControl = :nom1    and   ");
                    Parms.Add(new Object[] { "nom1", DataType.UIListControl });
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