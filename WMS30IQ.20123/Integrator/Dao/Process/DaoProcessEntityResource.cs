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
    public class DaoProcessEntityResource : DaoService
    {
        public DaoProcessEntityResource(DaoFactory factory) : base(factory) { }

        public ProcessEntityResource Save(ProcessEntityResource data)
        {
            return (ProcessEntityResource)base.Save(data);
        }


        public Boolean Update(ProcessEntityResource data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProcessEntityResource data)
        {
            return base.Delete(data);
        }


        public ProcessEntityResource SelectById(ProcessEntityResource data)
        {
            return (ProcessEntityResource)base.SelectById(data);
        }


        public IList<ProcessEntityResource> Select(ProcessEntityResource data)
        {

                IList<ProcessEntityResource> datos = new List<ProcessEntityResource>();

                datos = GetHsql(data).List<ProcessEntityResource>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProcessEntityResource a    where  ");
            ProcessEntityResource ProcessEntityResource = (ProcessEntityResource)data;
            if (ProcessEntityResource != null)
            {
                Parms = new List<Object[]>();
                if (ProcessEntityResource.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", ProcessEntityResource.RowID });
                }

                if (ProcessEntityResource.Entity != null && ProcessEntityResource.Entity.ClassEntityID != 0)
                {
                    sql.Append(" a.Entity.ClassEntityID = :ix2     and   ");
                    Parms.Add(new Object[] { "ix2", ProcessEntityResource.Entity.ClassEntityID });
                }


                if (ProcessEntityResource.Process != null && ProcessEntityResource.Process.ProcessID != 0)
                {
                    sql.Append(" a.Process.ProcessID = :ip2     and   ");
                    Parms.Add(new Object[] { "ip2", ProcessEntityResource.Process.ProcessID });
                }



                if (ProcessEntityResource.Status != null && ProcessEntityResource.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", ProcessEntityResource.Status.StatusID });
                }


                if (ProcessEntityResource.EntityRowID != 0)
                {
                    sql.Append(" a.EntityRowID = :id11     and   ");
                    Parms.Add(new Object[] { "id11", ProcessEntityResource.EntityRowID });
                }


                if (ProcessEntityResource.Template != null && ProcessEntityResource.Template.RowID != 0)
                {
                    sql.Append(" a.Template.RowID  = :idx2     and   ");
                    Parms.Add(new Object[] { "idx2", ProcessEntityResource.Template.RowID });
                }


                if (ProcessEntityResource.File != null && ProcessEntityResource.File.RowID != 0)
                {
                    sql.Append(" a.File.RowID  = :idf2     and   ");
                    Parms.Add(new Object[] { "idf2", ProcessEntityResource.File.RowID });
                }


                //if (ProcessEntityResource.ResourceType != 0)
                //{
                //    sql.Append(" a.ResourceType = :id12     and   ");
                //    Parms.Add(new Object[] { "id12", ProcessEntityResource.ResourceType });
                //}



                //if (ProcessEntityResource.ResourceID != 0)
                //{
                //    sql.Append(" a.ResourceID = :id13     and   ");
                //    Parms.Add(new Object[] { "id13", ProcessEntityResource.ResourceID });
                //}

            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}