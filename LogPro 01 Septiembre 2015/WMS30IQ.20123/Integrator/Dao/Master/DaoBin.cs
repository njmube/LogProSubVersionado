using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoBin : DaoService
    {
        public DaoBin(DaoFactory factory) : base(factory) { }

        public Bin Save(Bin data)
        {
            return (Bin)base.Save(data);
        }


        public Boolean Update(Bin data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Bin data)
        {
            return base.Delete(data);
        }


        public Bin SelectById(Bin data)
        {
            return (Bin)base.SelectById(data);
        }


        public IList<Bin> Select(Bin data)
        {

                IList<Bin> datos = new List<Bin>();

                datos = GetHsql(data).List<Bin>();

                if (!Factory.IsTransactional)
                    Factory.Commit();

                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Bin a    where  ");
            Bin bin = (Bin)data;
            if (bin != null)
            {
                Parms = new List<Object[]>();
                if (bin.BinID != 0)
                {
                    sql.Append(" a.BinID = :id     and   ");
                    Parms.Add(new Object[] { "id", bin.BinID });
                }

                if (bin.Zone != null && bin.Zone.ZoneID != 0)
                {
                    sql.Append(" a.Zone.ZoneID= :id1     and   ");
                    Parms.Add(new Object[] { "id1", bin.Zone.ZoneID });
                }

                if (bin.Location != null && bin.Location.LocationID != 0)
                {
                    sql.Append(" a.Location.LocationID = :loid2     and   ");
                    Parms.Add(new Object[] { "loid2", bin.Location.LocationID });
                }

                if (!String.IsNullOrEmpty(bin.BinCode))
                {
                    //sql.Append(" a.BinCode = :nom     and   "); 
                    //Parms.Add(new Object[] { "nom", bin.BinCode });
                    sql.Append(" (a.BinCode = :nomCode OR a.BinCode LIKE :nomb) and   ");
                    Parms.Add(new Object[] { "nomCode", bin.BinCode });
                    Parms.Add(new Object[] { "nomb", bin.BinCode + '%' });
                }

                if (!String.IsNullOrEmpty(bin.LevelCode))
                {
                    sql.Append(" a.LevelCode = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", bin.LevelCode });
                }

                if (!String.IsNullOrEmpty(bin.Aisle))
                {
                    sql.Append(" a.Aisle = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", bin.Aisle });
                }

                if (!String.IsNullOrEmpty(bin.Description))
                {
                    sql.Append(" a.Description = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", bin.Description });
                }

                if (bin.Status != null && bin.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", bin.Status.StatusID });
                }

                if (bin.Rank != 0)
                {
                    sql.Append(" a.Rank = :id3     and   ");
                    Parms.Add(new Object[] { "id3", bin.Rank });
                }

                if (bin.IsArea == true)
                {
                    sql.Append(" a.IsArea = :id11     and   ");
                    Parms.Add(new Object[] { "id11", true });
                }
                else if (bin.IsArea == false)
                {
                    sql.Append(" a.IsArea = :id11     and   ");
                    Parms.Add(new Object[] { "id11", false });
                }


                if (bin.Process != null && bin.Process.ProcessID != 0)
                {
                    sql.Append(" a.Process.ProcessID = :cp1     and   ");
                    Parms.Add(new Object[] { "cp1", bin.Process.ProcessID });
                }

            }
            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.Rank, a.BinCode asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }



    }
}