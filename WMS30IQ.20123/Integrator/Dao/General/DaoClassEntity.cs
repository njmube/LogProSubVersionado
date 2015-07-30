using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoClassEntity : DaoService
    {
        public DaoClassEntity(DaoFactory factory) : base(factory) { }

        public ClassEntity Save(ClassEntity data)
        {
            return (ClassEntity)base.Save(data);
        }


        public Boolean Update(ClassEntity data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ClassEntity data)
        {
            return base.Delete(data);
        }


        public ClassEntity SelectById(ClassEntity data)
        {
            return (ClassEntity)base.SelectById(data);
        }


        public IList<ClassEntity> Select(ClassEntity data)
        {

                IList<ClassEntity> datos = new List<ClassEntity>();

                datos = GetHsql(data).List<ClassEntity>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
  
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ClassEntity a    where  ");
            ClassEntity classentity = (ClassEntity)data;
            if (classentity != null)
            {
                Parms = new List<Object[]>();
                if (classentity.ClassEntityID != 0)
                {
                    sql.Append(" a.ClassEntityID = :id     and   ");
                    Parms.Add(new Object[] { "id", classentity.ClassEntityID });
                }

                if (!String.IsNullOrEmpty(classentity.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", classentity.Name });
                }

                if (classentity.BlnManageContacts != null)
                {
                    sql.Append(" a.BlnManageContacts = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", classentity.BlnManageContacts });
                }

                if (classentity.BlnManageCriteria != null)
                {
                    sql.Append(" a.BlnManageCriteria = :nom2     and   ");
                    Parms.Add(new Object[] { "nom2", classentity.BlnManageCriteria });
                }

                if (classentity.BlnZoneCriteria != null)
                {
                    sql.Append(" a.BlnZoneCriteria = :nom3     and   ");
                    Parms.Add(new Object[] { "nom3", classentity.BlnZoneCriteria  });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.ClassEntityID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}