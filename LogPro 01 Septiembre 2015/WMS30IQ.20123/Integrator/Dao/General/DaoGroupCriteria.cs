using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate; 
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoGroupCriteria : DaoService
    {
        public DaoGroupCriteria(DaoFactory factory) : base(factory) { }

        public GroupCriteria Save(GroupCriteria data)
        {
            return (GroupCriteria)base.Save(data);
        }


        public Boolean Update(GroupCriteria data)
        {
            return base.Update(data);
        }


        public Boolean Delete(GroupCriteria data)
        {
            return base.Delete(data);
        }


        public GroupCriteria SelectById(GroupCriteria data)
        {
            return (GroupCriteria)base.SelectById(data);
        }


        public IList<GroupCriteria> Select(GroupCriteria data)
        {
            IList<GroupCriteria> datos = new List<GroupCriteria>();
            datos = GetHsql(data).List<GroupCriteria>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;

        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from GroupCriteria a    where  ");
            GroupCriteria groupcriteria = (GroupCriteria)data;
            if (groupcriteria != null)
            {
                Parms = new List<Object[]>();
                if (groupcriteria.GroupCriteriaID != 0)
                {
                    sql.Append(" a.GroupCriteriaID = :id     and   ");
                    Parms.Add(new Object[] { "id", groupcriteria.GroupCriteriaID });
                }

                if (groupcriteria.Company != null && groupcriteria.Company.CompanyID != 0)
                {
                    sql.Append(" a.Company.CompanyID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", groupcriteria.Company.CompanyID });
                }

                if (!String.IsNullOrEmpty(groupcriteria.Name))
                {
                    sql.Append(" a.Name like :nom     and   "); 
                    Parms.Add(new Object[] { "nom", "%"+groupcriteria.Name+"%" });
                }

                if (!String.IsNullOrEmpty(groupcriteria.Description))
                {
                    sql.Append(" a.Description like :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", "%" + groupcriteria.Description+"%"});
                }


                if (groupcriteria.Rank != 0)
                {
                    sql.Append(" a.Rank = :id2     and   ");
                    Parms.Add(new Object[] { "id2", groupcriteria.Rank });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.GroupCriteriaID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}