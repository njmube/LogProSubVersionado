using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Entities.Master;
using Integrator.Config;




namespace Integrator.Dao.Master
{
    public class DaoMMaster : DaoService
    {
        public DaoMMaster(DaoFactory factory) : base(factory){}

        public MMaster Save(MMaster data)
        {
            return (MMaster)base.Save(data);
        }

        public Boolean Update(MMaster data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MMaster data)
        {
            return base.Delete(data);
        }

        public MMaster SelectById(MMaster data) 
        {
            return (MMaster)base.SelectById(data);
        }

        public IList<MMaster> Select(MMaster data)
        {

            IList<MMaster> datos = new List<MMaster>();

            try
            {
                datos = GetHsql(data).List<MMaster>();
                if (!Factory.IsTransactional)
                    Factory.Commit();

            }
            catch (Exception e)
            {
                NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
            }

            return datos;

        }

        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from MMaster a  where  ");
            MMaster MetaMaster = (MMaster)data;

            if (MetaMaster != null)
            {
                Parms = new List<Object[]>();

                if (MetaMaster.MetaMasterID != 0) 
                {
                    sql.Append(" a.MetaMasterID = :id     and    ");
                    Parms.Add(new Object[] { "id", MetaMaster.MetaMasterID });
                }

                if (!String.IsNullOrEmpty(MetaMaster.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", MetaMaster.Name });
                }

                if (!String.IsNullOrEmpty(MetaMaster.Code))
                {
                    sql.Append(" a.Code = :nom1    and   ");
                    Parms.Add(new Object[] { "nom1", MetaMaster.Code });
                }


                if (!String.IsNullOrEmpty(MetaMaster.Code2))
                {
                    sql.Append(" a.Code2 = :noc1    and   ");
                    Parms.Add(new Object[] { "noc1", MetaMaster.Code2 });
                }


                if (MetaMaster.MetaType != null )
                {

                    if (MetaMaster.MetaType.MetaTypeID != 0)
                    {
                        sql.Append(" a.MetaType.MetaTypeID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", MetaMaster.MetaType.MetaTypeID });
                    }


                    if (!String.IsNullOrEmpty(MetaMaster.MetaType.Code))
                    {
                        sql.Append(" a.MetaType.Code = :icd1     and   ");
                        Parms.Add(new Object[] { "icd1", MetaMaster.MetaType.Code });
                    }
                }

                if (MetaMaster.NumOrder != 0)
                {
                    sql.Append(" a.NumOrder = :id2     and    ");
                    Parms.Add(new Object[] { "id2", MetaMaster.NumOrder });
                }

                if (MetaMaster.Active != null)
                {
                    sql.Append(" a.Active = :ia2     and    ");
                    Parms.Add(new Object[] { "ia2", MetaMaster.Active });
                }


                if (!String.IsNullOrEmpty(MetaMaster.DefValue))
                {
                    sql.Append(" a.DefValue = :nom14    and   ");
                    Parms.Add(new Object[] { "nom14", MetaMaster.DefValue });
                }


            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.Name asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }
    }
}
