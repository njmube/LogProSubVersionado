using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;
using Entities.Process;

namespace Integrator.Dao.Process
{
    public class DaoCustomProcessContextByEntity : DaoService
    {
        public DaoCustomProcessContextByEntity(DaoFactory factory) : base(factory) { }

        public CustomProcessContextByEntity Save(CustomProcessContextByEntity data)
        {
            return (CustomProcessContextByEntity)base.Save(data);
        }


        public Boolean Update(CustomProcessContextByEntity data)
        {
            return base.Update(data);
        }


        public Boolean Delete(CustomProcessContextByEntity data)
        {
            return base.Delete(data);
        }


        public CustomProcessContextByEntity SelectById(CustomProcessContextByEntity data)
        {
            return (CustomProcessContextByEntity)base.SelectById(data);
        }


        public IList<CustomProcessContextByEntity> Select(CustomProcessContextByEntity data)
        {

                IList<CustomProcessContextByEntity> datos = new List<CustomProcessContextByEntity>();

                datos = GetHsql(data).List<CustomProcessContextByEntity>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from CustomProcessContextByEntity a    where  ");
            CustomProcessContextByEntity custEr = (CustomProcessContextByEntity)data;
            if (custEr != null)
            {
                Parms = new List<Object[]>();
                if (custEr.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", custEr.RowID });
                }


                if (custEr.Entity != null && custEr.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :ie2     and   ");
                    Parms.Add(new Object[] { "ie2", custEr.Entity.ClassEntityID });
                }

                if (custEr.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :ir2     and   ");
                    Parms.Add(new Object[] { "ir2", custEr.EntityRowID });
                }

                if (custEr.Status != null && custEr.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", custEr.Status.StatusID });
                }


            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}