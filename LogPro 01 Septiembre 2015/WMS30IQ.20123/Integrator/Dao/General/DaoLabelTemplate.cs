using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Entities.General;

namespace Integrator.Dao.General
{
    public class DaoLabelTemplate : DaoService
    {

        public DaoLabelTemplate(DaoFactory factory) : base(factory) { }

        public LabelTemplate Save(LabelTemplate data)
        {
            return (LabelTemplate)base.Save(data);
        }


        public Boolean Update(LabelTemplate data)
        {
            return base.Update(data);
        }


        public Boolean Delete(LabelTemplate data)
        {
            return base.Delete(data);
        }


        public LabelTemplate SelectById(LabelTemplate data)
        {
            return (LabelTemplate)base.SelectById(data);
        }


        public IList<LabelTemplate> Select(LabelTemplate data)
        {

                IList<LabelTemplate> datos = new List<LabelTemplate>();
                datos = GetHsql(data).List<LabelTemplate>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;

        }
            
        public override IQuery GetHsql(object data)
        {
            StringBuilder sql = new StringBuilder("select a from LabelTemplate a    where  ");
            LabelTemplate printingTemplate = (LabelTemplate)data;
            if (printingTemplate != null)
            {
                Parms = new List<Object[]>();
                if (printingTemplate.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", printingTemplate.RowID });
                }
                if (printingTemplate.LabelType != null && printingTemplate.LabelType.DocTypeID != 0)
                {
                    sql.Append(" a.LabelType.DocTypeID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", printingTemplate.LabelType.DocTypeID });
                }

                if (printingTemplate.LabelType != null && printingTemplate.LabelType.DocClass != null && printingTemplate.LabelType.DocClass.DocClassID != 0)
                {
                    sql.Append(" a.LabelType.DocClass.DocClassID = :id8     and   ");
                    Parms.Add(new Object[] { "id8", printingTemplate.LabelType.DocClass.DocClassID });
                }


                if (!String.IsNullOrEmpty(printingTemplate.Name))
                {
                    sql.Append(" a.Name = :nom     and   ");
                    Parms.Add(new Object[] { "nom", printingTemplate.Name });
                }

                if (!String.IsNullOrEmpty(printingTemplate.Header))
                {
                    sql.Append(" a.Header = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", printingTemplate.Header });
                }


                if (printingTemplate.DefPrinter != null && printingTemplate.DefPrinter.ConnectionID != 0)
                {
                    sql.Append(" a.DefPrinter.ConnectionID = :ip8     and   ");
                    Parms.Add(new Object[] { "ip8", printingTemplate.DefPrinter.ConnectionID });
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
