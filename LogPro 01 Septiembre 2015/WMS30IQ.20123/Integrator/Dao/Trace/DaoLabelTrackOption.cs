using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;

namespace Integrator.Dao.Trace
{
    public class DaoLabelTrackOption : DaoService
    {
        public DaoLabelTrackOption(DaoFactory factory) : base(factory) { }

        public LabelTrackOption Save(LabelTrackOption data)
        {
            return (LabelTrackOption)base.Save(data);
        }


        public Boolean Update(LabelTrackOption data)
        {
            return base.Update(data);
        }


        public Boolean Delete(LabelTrackOption data)
        {
            return base.Delete(data);
        }


        public LabelTrackOption SelectById(LabelTrackOption data)
        {
            return (LabelTrackOption)base.SelectById(data);
        }


        public IList<LabelTrackOption> Select(LabelTrackOption data)
        {

                IList<LabelTrackOption> datos = new List<LabelTrackOption>();

            try { 
                datos = GetHsql(data).List<LabelTrackOption>();
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
            StringBuilder sql = new StringBuilder("select a from LabelTrackOption a  where  ");
            LabelTrackOption track = (LabelTrackOption)data;

            if (track != null)
            {
                Parms = new List<Object[]>();

                if (track.RowID != 0)
                {
                    sql.Append(" a.RowID = :id1  and   ");
                    Parms.Add(new Object[] { "id1", track.RowID });
                }

                if (!String.IsNullOrEmpty(track.TrackValue))
                {
                    sql.Append(" a.TrackValue = :nom1  and   ");
                    Parms.Add(new Object[] { "nom1", track.TrackValue });
                }


                if (track.TrackOption != null && track.TrackOption.RowID != 0)
                {
                    sql.Append(" a.TrackOption.RowID = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", track.TrackOption.RowID });
                }


                if (track.Label != null)
                {
                    if (track.Label.Status != null && track.Label.Status.StatusID != 0)
                    {
                        sql.Append(" a.Label.Status.StatusID = :st0     and   ");
                        Parms.Add(new Object[] { "st0", track.Label.Status.StatusID });
                    }

                    if (track.Label.Product != null && track.Label.Product.ProductID != 0)
                    {
                        sql.Append(" a.Label.Product.ProductID = :pr0     and   ");
                        Parms.Add(new Object[] { "pr0", track.Label.Product.ProductID });
                    }

                    if (track.Label.LabelID != 0)
                    {
                        sql.Append(" a.Label.LabelID = :nom9     and   ");
                        Parms.Add(new Object[] { "nom9", track.Label.LabelID });
                    }
                }
            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.RowID ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}