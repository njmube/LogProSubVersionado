using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoContactPosition : DaoService
    {
        public DaoContactPosition(DaoFactory factory) : base(factory) { }

        public ContactPosition Save(ContactPosition data)
        {
            return (ContactPosition)base.Save(data);
        }


        public Boolean Update(ContactPosition data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ContactPosition data)
        {
            return base.Delete(data);
        }


        public ContactPosition SelectById(ContactPosition data)
        {
            return (ContactPosition)base.SelectById(data);
        }


        public IList<ContactPosition> Select(ContactPosition data)
        {

                IList<ContactPosition> datos = new List<ContactPosition>();

                datos = GetHsql(data).List<ContactPosition>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ContactPosition a    where  ");
            ContactPosition contactposition = (ContactPosition)data;
            if (contactposition != null)
            {
                Parms = new List<Object[]>();
                if (contactposition.ContactPositionID != 0)
                {
                    sql.Append(" a.ContactPositionID = :id     and   ");
                    Parms.Add(new Object[] { "id", contactposition.ContactPositionID });
                }

                if (!String.IsNullOrEmpty(contactposition.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", contactposition.Name });
                }

                //if (contactposition.Status != null && contactposition.Status.StatusID != 0)
                //{
                //    sql.Append(" a.Status.StatusID= :id1     and   ");
                //    Parms.Add(new Object[] { "id1", contactposition.Status.StatusID });
                //}



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.contactPositionID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}