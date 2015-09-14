using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoContactEntityRelation : DaoService
    {
        public DaoContactEntityRelation(DaoFactory factory) : base(factory) { }

        public ContactEntityRelation Save(ContactEntityRelation data)
        {
            return (ContactEntityRelation)base.Save(data);
        }


        public Boolean Update(ContactEntityRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ContactEntityRelation data)
        {
            return base.Delete(data);
        }


        public ContactEntityRelation SelectById(ContactEntityRelation data)
        {
            return (ContactEntityRelation)base.SelectById(data);
        }


        public IList<ContactEntityRelation> Select(ContactEntityRelation data)
        {
            IList<ContactEntityRelation> datos = new List<ContactEntityRelation>();

            datos = GetHsql(data).List<ContactEntityRelation>();
            if (!Factory.IsTransactional)
                Factory.Commit();
            return datos;
        }


        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ContactEntityRelation a    where  ");
            ContactEntityRelation contactentityrelation = (ContactEntityRelation)data;
            if (contactentityrelation != null)
            {
                Parms = new List<Object[]>();
                if (contactentityrelation.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", contactentityrelation.RowID });
                }

                if (contactentityrelation.Contact != null && contactentityrelation.Contact.ContactID != 0)
                {
                    sql.Append(" a.Contact.ContactID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", contactentityrelation.Contact.ContactID });
                }

                if (contactentityrelation.ClassEntity != null && contactentityrelation.ClassEntity.ClassEntityID != 0)
                {
                    sql.Append(" a.classEntity.ClassEntityID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", contactentityrelation.ClassEntity.ClassEntityID });
                }

                if (contactentityrelation.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", contactentityrelation.EntityRowID });
                }

                if (contactentityrelation.ContactPosition != null && contactentityrelation.ContactPosition.ContactPositionID != 0)
                {
                    sql.Append(" a.ContactPosition.ContactPositionID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", contactentityrelation.ContactPosition.ContactPositionID });
                }

                if (contactentityrelation.IsMain != null)
                {
                    sql.Append(" upper(a.IsMain) like :nom     and   ");
                    Parms.Add(new Object[] { "nom", contactentityrelation.IsMain + "%" });
                }

                if (contactentityrelation.Status != null && contactentityrelation.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id5     and   ");
                    Parms.Add(new Object[] { "id5", contactentityrelation.Status.StatusID });
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