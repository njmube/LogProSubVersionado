using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using Integrator.Config;
using Entities.Workflow;

namespace Integrator.Dao.Workflow
{
    public class DaoDataDefinition : DaoService
    {
        public DaoDataDefinition(DaoFactory factory) : base(factory) { }

        public DataDefinition Save(DataDefinition data)
        {
            return (DataDefinition)base.Save(data);
        }


        public Boolean Update(DataDefinition data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DataDefinition data)
        {
            return base.Delete(data);
        }


        public DataDefinition SelectById(DataDefinition data)
        {
            return (DataDefinition)base.SelectById(data);
        }




        public IList<DataDefinition> Select(DataDefinition data)
        {
            IList<DataDefinition> datos = new List<DataDefinition>();
            try { 
            datos = GetHsql(data).List<DataDefinition>();
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
            StringBuilder sql = new StringBuilder("select a from DataDefinition a where ");
            DataDefinition DataDefinition = (DataDefinition)data;
            if (DataDefinition != null)
            {
                Parms = new List<Object[]>();
                if (DataDefinition.RowID != Guid.Empty)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", DataDefinition.RowID });
                }

                if (!String.IsNullOrEmpty(DataDefinition.Code))
                {
                    sql.Append(" a.Code LIKE :code1    and   ");
                    Parms.Add(new Object[] { "code1", DataDefinition.Code });
                }

                if (!String.IsNullOrEmpty(DataDefinition.DisplayName))
                {
                    sql.Append(" a.DisplayName LIKE :nom1  and  ");
                    Parms.Add(new Object[] { "nom1", "%" + DataDefinition.DisplayName + "%" });
                }

                if (DataDefinition.ReadOnly != null)
                {
                    sql.Append(" a.ReadOnly = :nom2  and  ");
                    Parms.Add(new Object[] { "nom2", DataDefinition.ReadOnly });
                }

                if (DataDefinition.DataType != null && DataDefinition.DataType.DataTypeID != 0)
                {
                    sql.Append(" a.DataType.DataTypeID = :nom3 and  ");
                    Parms.Add(new Object[] { "nom3", DataDefinition.DataType.DataTypeID });
                }

                if (DataDefinition.Location != null && DataDefinition.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :nom4 and  ");
                    Parms.Add(new Object[] { "nom4", DataDefinition.Location.LocationID });
                }

                if (DataDefinition.IsHeader != null)
                {
                    sql.Append(" a.IsHeader = :nom5 and  ");
                    Parms.Add(new Object[] { "nom5", DataDefinition.IsHeader });
                }

                if (DataDefinition.Entity != null && DataDefinition.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :nom6 and  ");
                    Parms.Add(new Object[] { "nom6", DataDefinition.Entity.ClassEntityID });
                }

                if (DataDefinition.IsSerial != null)
                {
                    sql.Append(" a.IsSerial = :nom7 and  ");
                    Parms.Add(new Object[] { "nom7", DataDefinition.IsSerial });
                }

                if (DataDefinition.IsRequired != null)
                {
                    sql.Append(" a.IsRequired = :nom8 and  ");
                    Parms.Add(new Object[] { "nom8", DataDefinition.IsRequired });
                }

                if (DataDefinition.Size != 0)
                {
                    sql.Append(" a.Size = :nom9 and  ");
                    Parms.Add(new Object[] { "nom9", DataDefinition.Size });
                }

                if (!String.IsNullOrEmpty(DataDefinition.DefaultValue))
                {
                    sql.Append(" a.DefaultValue = :nom10 and  ");
                    Parms.Add(new Object[] { "nom10", DataDefinition.DefaultValue });
                }

                if (DataDefinition.NumOrder != 0)
                {
                    sql.Append(" a.NumOrder = :nom11 and  ");
                    Parms.Add(new Object[] { "nom11", DataDefinition.NumOrder });
                }

                if (DataDefinition.MetaType != null && DataDefinition.MetaType.MetaTypeID != 0)
                {
                    sql.Append(" a.MetaType.MetaTypeID = :nom12 and  ");
                    Parms.Add(new Object[] { "nom12", DataDefinition.MetaType.MetaTypeID });
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