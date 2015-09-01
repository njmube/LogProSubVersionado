using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoGroupCriteriaRelationData : DaoService
    {
        public DaoGroupCriteriaRelationData(DaoFactory factory) : base(factory) { }

        public GroupCriteriaRelationData Save(GroupCriteriaRelationData data)
        {
            return (GroupCriteriaRelationData)base.Save(data);
        }


        public Boolean Update(GroupCriteriaRelationData data)
        {
            return base.Update(data);
        }


        public Boolean Delete(GroupCriteriaRelationData data)
        {
            return base.Delete(data);
        }


        public GroupCriteriaRelationData SelectById(GroupCriteriaRelationData data)
        {
            return (GroupCriteriaRelationData)base.SelectById(data);
        }


        public IList<GroupCriteriaRelationData> Select(GroupCriteriaRelationData data)
        {
         
                IList<GroupCriteriaRelationData> datos = new List<GroupCriteriaRelationData>();
 
                datos = GetHsql(data).List<GroupCriteriaRelationData>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from GroupCriteriaRelationData a    where  ");
            GroupCriteriaRelationData groupcriteriarelationdata = (GroupCriteriaRelationData)data;
            if (groupcriteriarelationdata != null)
            {
                Parms = new List<Object[]>();
                if (groupcriteriarelationdata.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", groupcriteriarelationdata.RowID });
                }

                if (groupcriteriarelationdata.CriteriaRel != null && groupcriteriarelationdata.CriteriaRel.RowID != 0)
                {
                    sql.Append(" a.CriteriaRel.RowID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", groupcriteriarelationdata.CriteriaRel.RowID });
                }

                if (groupcriteriarelationdata.CriteriaDet != null && groupcriteriarelationdata.CriteriaDet.CriteriaDetID != 0)
                {
                    sql.Append(" a.CriteriaDet.CriteriaDetID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", groupcriteriarelationdata.CriteriaDet.CriteriaDetID });
                }

                if (groupcriteriarelationdata.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", groupcriteriarelationdata.EntityRowID });
                }

                if (!String.IsNullOrEmpty(groupcriteriarelationdata.CriteriaDetData))
                {
                    sql.Append(" a.CriteriaDetData = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", groupcriteriarelationdata.CriteriaDetData });
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