using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Entities.General;
using Entities.Master;
using Integrator.Config;




namespace Integrator.Dao.Master
{
    public class DaoMType : DaoService
    {
        public DaoMType(DaoFactory factory) : base(factory) { }

        public MType Save(MType data)
        {
            return (MType)base.Save(data);
        }

        public Boolean Update(MType data)
        {
            return base.Update(data);
        }

        public Boolean Delete(MType data)
        {
            return base.Delete(data);
        }

        public MType SelectById(MType data)
        {
            return (MType)base.SelectById(data);
        }

        public IList<MType> Select(MType data)
        {

                IList<MType> datos = new List<MType>();
            try { 
                datos = GetHsql(data).List<MType>();
                if (!Factory.IsTransactional)
                    Factory.Commit();

            }
            catch (Exception e)
            {
                NHibernateHelper.WriteEventLog(WriteLog.GetTechMessage(e));
            }
                return datos;
           
        }

        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from MType a    where  ");
            MType MetaType = (MType)data;
            
            if (MetaType != null)
            {
                Parms = new List<Object[]>();
                
                if(MetaType.MetaTypeID != 0)
                {
                    sql.Append(" a.MetaTypeID = :id     and   ");
                    Parms.Add(new Object[] { "id", MetaType.MetaTypeID });
                }

 
                if (!String.IsNullOrEmpty(MetaType.Name))
                {
                    sql.Append(" a.Name LIKE :nom     and   ");
                    Parms.Add(new Object[] { "nom", "%" + MetaType.Name + "%" });
                }

                if (!String.IsNullOrEmpty(MetaType.Code))
                {
                    sql.Append(" a.Code = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", MetaType.Code });
                }



            }

            sql = new StringBuilder(sql.ToString());
            sql.Append("1=1 order by a.MetaTypeID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }

    }
}
