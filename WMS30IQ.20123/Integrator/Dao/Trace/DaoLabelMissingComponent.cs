using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoLabelMissingComponent : DaoService
    {
        public DaoLabelMissingComponent(DaoFactory factory) : base(factory) { }

        public LabelMissingComponent Save(LabelMissingComponent data)
        {
            return (LabelMissingComponent)base.Save(data);
        }


        public Boolean Update(LabelMissingComponent data)
        {
            return base.Update(data);
        }


        public Boolean Delete(LabelMissingComponent data)
        {
            return base.Delete(data);
        }


        public LabelMissingComponent SelectById(LabelMissingComponent data)
        {
            return (LabelMissingComponent)base.SelectById(data);
        }


        public IList<LabelMissingComponent> Select(LabelMissingComponent data)
        {

                IList<LabelMissingComponent> datos = new List<LabelMissingComponent>();
                try
                {
                    datos = GetHsql(data).List<LabelMissingComponent>();
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
            StringBuilder sql = new StringBuilder("select a from LabelMissingComponent a  where  ");
            LabelMissingComponent label = (LabelMissingComponent)data;

            if (label != null)
            {
                Parms = new List<Object[]>();

                if (label.RowID != 0)
                {
                    sql.Append(" a.RowID = :id1  and   ");
                    Parms.Add(new Object[] { "id1", label.RowID });
                }

                if (label.FatherLabel != null && label.FatherLabel.LabelID != 0)
                {
                    sql.Append(" a.FatherLabel.LabelID = :nom9     and   ");
                    Parms.Add(new Object[] { "nom9", label.FatherLabel.LabelID });
                }

                if (label.Status != null && label.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :st11    and   ");
                    Parms.Add(new Object[] { "st11", label.Status.StatusID });
                }


                if (label.Component != null && label.Component.ProductID != 0)
                {
                    sql.Append(" a.Component.ProductID = :nom11    and   ");
                    Parms.Add(new Object[] { "nom11", label.Component.ProductID });
                }

                if (!String.IsNullOrEmpty(label.Notes))
                {
                    sql.Append(" a.Notes = :nom1  and   ");
                    Parms.Add(new Object[] { "nom1", label.Notes });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}