using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoGroupCriteriaRelation : DaoService
    {
        public DaoGroupCriteriaRelation(DaoFactory factory) : base(factory) { }

        public GroupCriteriaRelation Save(GroupCriteriaRelation data)
        {
            return (GroupCriteriaRelation)base.Save(data);
        }


        public Boolean Update(GroupCriteriaRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(GroupCriteriaRelation data)
        {
            return base.Delete(data);
        }


        public GroupCriteriaRelation SelectById(GroupCriteriaRelation data)
        {
            return (GroupCriteriaRelation)base.SelectById(data);
        }


        public IList<GroupCriteriaRelation> Select(GroupCriteriaRelation data)
        {
                IList<GroupCriteriaRelation> datos = new List<GroupCriteriaRelation>();
                datos = GetHsql(data).List<GroupCriteriaRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
          
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from GroupCriteriaRelation a    where  ");
            GroupCriteriaRelation groupcriteriarelation = (GroupCriteriaRelation)data;
            if (groupcriteriarelation != null)
            {
                Parms = new List<Object[]>();
                if (groupcriteriarelation.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", groupcriteriarelation.RowID });
                }

                if (groupcriteriarelation.Company != null && groupcriteriarelation.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", groupcriteriarelation.Company.CompanyID });
                }

                if (groupcriteriarelation.GroupCriteria != null && groupcriteriarelation.GroupCriteria.GroupCriteriaID != 0)
                {
                    sql.Append(" a.GroupCriteria.GroupCriteria.GroupCriteriaID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", groupcriteriarelation.GroupCriteria.GroupCriteriaID });
                }

                if (groupcriteriarelation.ClassEntity != null && groupcriteriarelation.ClassEntity.ClassEntityID != 0)
                {
                    sql.Append(" a.ClassEntity.ClassEntityID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", groupcriteriarelation.ClassEntity.ClassEntityID });
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