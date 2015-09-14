using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoGroupCriteriaDetail : DaoService
    {
        public DaoGroupCriteriaDetail(DaoFactory factory) : base(factory) { }

        public GroupCriteriaDetail Save(GroupCriteriaDetail data)
        {
            return (GroupCriteriaDetail)base.Save(data);
        }


        public Boolean Update(GroupCriteriaDetail data)
        {
            return base.Update(data);
        }


        public Boolean Delete(GroupCriteriaDetail data)
        {
            return base.Delete(data);
        }


        public GroupCriteriaDetail SelectById(GroupCriteriaDetail data)
        {
            return (GroupCriteriaDetail)base.SelectById(data);
        }


        public IList<GroupCriteriaDetail> Select(GroupCriteriaDetail data)
        {
           
                IList<GroupCriteriaDetail> datos = new List<GroupCriteriaDetail>();

                datos = GetHsql(data).List<GroupCriteriaDetail>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from GroupCriteriaDetail a    where  ");
            GroupCriteriaDetail groupcriteriadetail = (GroupCriteriaDetail)data;
            if (groupcriteriadetail != null)
            {
                Parms = new List<Object[]>();
                if (groupcriteriadetail.CriteriaDetID != 0)
                {
                    sql.Append(" a.CriteriaDetID = :id     and   ");
                    Parms.Add(new Object[] { "id", groupcriteriadetail.CriteriaDetID });
                }

                if (groupcriteriadetail.GroupCriteria.GroupCriteriaID != 0)
                {
                    sql.Append(" a.GroupCriteria.GroupCriteriaID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", groupcriteriadetail.GroupCriteria.GroupCriteriaID });
                }

                if (!String.IsNullOrEmpty(groupcriteriadetail.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", groupcriteriadetail.Name });
                }

                if (!String.IsNullOrEmpty(groupcriteriadetail.Description))
                {
                    sql.Append(" a.Description = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", groupcriteriadetail.Description });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.CriteriaDetID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}