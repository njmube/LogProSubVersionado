using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoTrackOption : DaoService
    {
        public DaoTrackOption(DaoFactory factory) : base(factory) { }

        public TrackOption Save(TrackOption data)
        {
            return (TrackOption)base.Save(data);
        }


        public Boolean Update(TrackOption data)
        {
            return base.Update(data);
        }


        public Boolean Delete(TrackOption data)
        {
            return base.Delete(data);
        }


        public TrackOption SelectById(TrackOption data)
        {
            return (TrackOption)base.SelectById(data);
        }


        public IList<TrackOption> Select(TrackOption data)
        {

                IList<TrackOption> datos = new List<TrackOption>();
                datos = GetHsql(data).List<TrackOption>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from TrackOption a  where  ");
            TrackOption track = (TrackOption)data;

            if (track != null)
            {
                Parms = new List<Object[]>();

                if (track.RowID != 0)
                {
                    sql.Append(" a.RowID = :id1  and   ");
                    Parms.Add(new Object[] { "id1", track.RowID });
                }

                if (!String.IsNullOrEmpty(track.Name))
                {
                    sql.Append(" a.Name = :nom1  and   "); 
                    Parms.Add(new Object[] { "nom1",  track.Name });
                }

                if (!String.IsNullOrEmpty(track.DisplayName))
                {
                    sql.Append(" a.Displayname = :nom2  and   ");
                    Parms.Add(new Object[] { "nom2",  track.DisplayName  });
                }


                if (track.IsUnique != null)
                {
                    sql.Append(" a.IsUnique = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", track.IsUnique });
                }


                if (track.IsSystem != null)
                {
                    sql.Append(" a.IsSystem = :nom10     and   ");
                    Parms.Add(new Object[] { "nom10", track.IsSystem });
                }


                if (track.DataType != null && track.DataType.DataTypeID != 0)
                {
                    sql.Append(" a.DataType.DataTypeID = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", track.DataType.DataTypeID });
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