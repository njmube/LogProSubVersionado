using System;
using System.Collections.Generic;
using System.Text;
using Integrator.Dao;
using NHibernate;
using Integrator.Config;
using Entities.Workflow;

namespace Integrator.Dao.Workflow
{
    public class DaoDataDefinitionByBin  : DaoService
    {
        public DaoDataDefinitionByBin (DaoFactory factory) : base(factory) { }

        public DataDefinitionByBin  Save(DataDefinitionByBin  data)
        {
            return (DataDefinitionByBin )base.Save(data);
        }


        public Boolean Update(DataDefinitionByBin  data)
        {
            return base.Update(data);
        }


        public Boolean Delete(DataDefinitionByBin  data)
        {
            return base.Delete(data);
        }


        public DataDefinitionByBin  SelectById(DataDefinitionByBin  data)
        {
            return (DataDefinitionByBin )base.SelectById(data);
        }




        public IList<DataDefinitionByBin > Select(DataDefinitionByBin  data)
        {
            IList<DataDefinitionByBin > datos = new List<DataDefinitionByBin >();
            try { 
            datos = GetHsql(data).List<DataDefinitionByBin >();
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
            StringBuilder sql = new StringBuilder("select a from DataDefinitionByBin  a where ");
            DataDefinitionByBin  DataDefinitionByBin  = (DataDefinitionByBin )data;
            if (DataDefinitionByBin != null)
            {
                Parms = new List<Object[]>();
                if (DataDefinitionByBin.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", DataDefinitionByBin.RowID });
                }

                if (DataDefinitionByBin.Bin != null)
                {
                    if (DataDefinitionByBin.Bin.BinID != 0)
                    {
                        sql.Append(" a.Bin.BinID = :nom1  and  ");
                        Parms.Add(new Object[] { "nom1", DataDefinitionByBin.Bin.BinID });
                    }

                    if (!String.IsNullOrEmpty(DataDefinitionByBin.Bin.BinCode))
                    {
                        sql.Append(" a.Bin.BinCode = :nom2  and  ");
                        Parms.Add(new Object[] { "nom2", DataDefinitionByBin.Bin.BinCode });
                    }

                    if (DataDefinitionByBin.Bin.Location != null && DataDefinitionByBin.Bin.Location.LocationID != 0)
                    {
                        sql.Append(" a.Bin.Location.LocationID = :nom3  and  ");
                        Parms.Add(new Object[] { "nom3", DataDefinitionByBin.Bin.Location.LocationID });
                    }
                }

                if (DataDefinitionByBin.DataDefinition != null)
                {
                    if (DataDefinitionByBin.DataDefinition.RowID != Guid.Empty)
                    {
                        sql.Append(" a.DataDefinition.RowID = :nom4  and  ");
                        Parms.Add(new Object[] { "nom4", DataDefinitionByBin.DataDefinition.RowID });
                    }

                    if (DataDefinitionByBin.DataDefinition.IsHeader != null)
                    {
                        sql.Append(" a.DataDefinition.IsHeader = :nom5  and  ");
                        Parms.Add(new Object[] { "nom5", DataDefinitionByBin.DataDefinition.IsHeader });
                    }

                    if (DataDefinitionByBin.DataDefinition.IsSerial != null)
                    {
                        sql.Append(" a.DataDefinition.IsSerial = :nom6  and  ");
                        Parms.Add(new Object[] { "nom6", DataDefinitionByBin.DataDefinition.IsSerial });
                    }

                    if (DataDefinitionByBin.DataDefinition.Location != null && DataDefinitionByBin.DataDefinition.Location.LocationID != 0)
                    {
                        sql.Append(" a.DataDefinition.Location.LocationID = :nom7  and  ");
                        Parms.Add(new Object[] { "nom7", DataDefinitionByBin.DataDefinition.Location.LocationID });
                    }
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