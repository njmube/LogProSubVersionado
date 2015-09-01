using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Report;
using System.Data;
using System.Data.SqlClient;

namespace Integrator.Dao.Report
{
    public class DaoIqQueryParameter : DaoService
    {
        public DaoIqQueryParameter(DaoFactory factory) : base(factory) { }

        //public DataSet SelectAll(IqQueryParameter data)
        //{
        //    DataSet dataSet = Factory.SelectDataSet(data.QueryString, new List<SqlParameter>());
        //    Factory.Commit();
        //    return dataSet;
        //}


        public override IQuery GetHsql(object data)
        {
            throw new NotImplementedException();
        }
    }
}