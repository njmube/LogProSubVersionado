using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoProductTrackRelation : DaoService
    {
        public DaoProductTrackRelation(DaoFactory factory) : base(factory) { }

        public ProductTrackRelation Save(ProductTrackRelation data)
        {
            return (ProductTrackRelation)base.Save(data);
        }


        public Boolean Update(ProductTrackRelation data)
        {
            return base.Update(data);
        }


        public Boolean Delete(ProductTrackRelation data)
        {
            return base.Delete(data);
        }


        public ProductTrackRelation SelectById(ProductTrackRelation data)
        {
            return (ProductTrackRelation)base.SelectById(data);
        }


        public IList<ProductTrackRelation> Select(ProductTrackRelation data)
        {

                IList<ProductTrackRelation> datos = new List<ProductTrackRelation>();
                datos = GetHsql(data).List<ProductTrackRelation>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from ProductTrackRelation a  where  ");
            ProductTrackRelation productTrack = (ProductTrackRelation)data;

            if (productTrack != null)
            {
                Parms = new List<Object[]>();
                if (productTrack.RowID != 0)
                {
                    sql.Append(" a.RowID = :id     and   ");
                    Parms.Add(new Object[] { "id", productTrack.RowID });
                }

                if (productTrack.TrackOption != null && !string.IsNullOrEmpty(productTrack.TrackOption.Name))
                {
                    sql.Append(" a.TrackOption.Name = :idn     and   ");
                    Parms.Add(new Object[] { "idn", productTrack.TrackOption.Name });
                }

                if (productTrack.Product != null && productTrack.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", productTrack.Product.ProductID });
                }

                if (productTrack.TrackOption != null && productTrack.TrackOption.RowID != 0)
                {
                    sql.Append(" a.TrackOption.RowID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", productTrack.TrackOption.RowID });
                }


                if (!String.IsNullOrEmpty(productTrack.DisplayName))
                {
                    sql.Append(" a.DisplayName = :nom1  and   ");
                    Parms.Add(new Object[] { "nom1", "%" + productTrack.DisplayName + "%" });
                }



                if (productTrack.IsRequired == true)
                {
                    sql.Append(" a.IsRequired = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", true});
                }
                else if (productTrack.IsRequired == false)
                {
                    sql.Append(" a.IsRequired = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", false });
                }


                if (productTrack.IsUnique == true)
                {
                    sql.Append(" a.IsUnique = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", true });
                }
                else if (productTrack.IsUnique == false)
                {
                    sql.Append(" a.IsUnique = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", false });
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