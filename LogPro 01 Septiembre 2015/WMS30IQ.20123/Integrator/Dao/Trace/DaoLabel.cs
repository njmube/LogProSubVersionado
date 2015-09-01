using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using System.Linq;


namespace Integrator.Dao.Trace
{
    public class DaoLabel : DaoService
    {
        public DaoLabel(DaoFactory factory) : base(factory) { }

        public Label Save(Label data)
        {
            return (Label)base.Save(data);
        }


        public Boolean Update(Label data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Label data)
        {
            return base.Delete(data);
        }


        public Label SelectById(Label data)
        {
            return (Label)base.SelectById(data);
        }


        public IList<Label> Select(Label data)
        {
            IList<Label> datos = new List<Label>();

            try
            {
                datos = GetHsql(data).List<Label>();
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
            StringBuilder sql = new StringBuilder("select a from Label a  ");


            Label label = (Label)data;
            if (label != null)
            {
                Parms = new List<Object[]>();


                if (label.TrackOptions != null && label.TrackOptions.Count > 0)
                {
                    sql.Append(" inner join a.TrackOptions as track where ");

                    int i = 1;
                    foreach (LabelTrackOption trackO in label.TrackOptions)
                    {
                        sql.Append(" ( track.TrackOption.RowID = :toid"+ i.ToString()+" and track.TrackValue = :toval"+ i.ToString() + " ) and   ");
                        Parms.Add(new Object[] { "toid" + i.ToString(), trackO.TrackOption.RowID });
                        Parms.Add(new Object[] { "toval" + i.ToString(), trackO.TrackValue });
                        i++;
                    }
                }
                else
                {
                    sql.Append(" where ");
                }


                if (label.LabelID != 0)
                {
                    sql.Append(" a.LabelID = :id     and   ");
                    Parms.Add(new Object[] { "id", label.LabelID });
                }

                if (label.LabelType != null && label.LabelType.DocTypeID != 0)
                {
                    if (label.LabelType.DocTypeID == LabelType.ProductLabel)
                    {
                        sql.Append(" (a.LabelType.DocTypeID = :id1 OR a.LabelType.DocTypeID = :idl1 ) and   ");
                        Parms.Add(new Object[] { "id1", LabelType.ProductLabel });
                        Parms.Add(new Object[] { "idl1", LabelType.UniqueTrackLabel });
                    }
                    else
                    {
                        sql.Append(" a.LabelType.DocTypeID = :id1     and   ");
                        Parms.Add(new Object[] { "id1", label.LabelType.DocTypeID });
                    }
                }



                if (!String.IsNullOrEmpty(label.LabelCode))
                {
                    sql.Append(" (a.LabelCode = :nom OR a.LabelID = :lb1d OR a.LabelCode = :noms ) and ");

                    //Adicionar las track option que sean unicas como serial etc.
                    //IList<TrackOption> listTrack = Factory.DaoTrackOption().Select(new TrackOption { IsUnique = true });
                    //foreach (TrackOption track in listTrack)
                    //    sql.Append(" Or a." + track.Name +" = :nom ");                    

                    ////sql.Append(") and ");


                    long barcode;
                    if (Int64.TryParse(label.LabelCode, out barcode))
                        Parms.Add(new Object[] { "nom", barcode.ToString() });
                    else
                        Parms.Add(new Object[] { "nom", label.LabelCode });

                    Parms.Add(new Object[] { "noms", label.LabelCode });

                    try {
                        if (label.LabelCode.StartsWith("1") && label.LabelCode.Length == WmsSetupValues.LabelLength)
                            Parms.Add(new Object[] { "lb1d", Int64.Parse(label.LabelCode.Substring(1, WmsSetupValues.LabelLength - 1)) });
                        else
                            Parms.Add(new Object[] { "lb1d", 0 });
                    }
                    catch { Parms.Add(new Object[] { "lb1d", 0 }); }

                }


                if (!String.IsNullOrEmpty(label.Manufacturer))
                {
                    sql.Append(" a.Manufacturer = :nom1     and   ");
                    Parms.Add(new Object[] { "nom1", label.Manufacturer });
                }


                if (label.Status != null && label.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id2     and   ");
                    Parms.Add(new Object[] { "id2", label.Status.StatusID });
                }

                if (label.FatherLabel != null && label.FatherLabel.LabelID > 0)
                {
                    sql.Append(" a.FatherLabel.LabelID = :id3     and   ");
                    Parms.Add(new Object[] { "id3", label.FatherLabel.LabelID });
                }
                else if (label.FatherLabel != null && label.FatherLabel.LabelID <= 0)
                {
                    sql.Append(" a.FatherLabel.LabelID is null  and   ");
                }


                if (label.Product != null && label.Product.ProductID != 0)
                {
                    sql.Append(" a.Product.ProductID = :id4     and   ");
                    Parms.Add(new Object[] { "id4", label.Product.ProductID });
                }

                if (label.Unit != null && label.Unit.UnitID != 0)
                {
                    sql.Append(" a.Unit.UnitID = :id5     and   ");
                    Parms.Add(new Object[] { "id5", label.Unit.UnitID });
                }


                if (label.Node != null && label.Node.NodeID != 0)
                {
                    sql.Append(" a.Node.NodeID = :id7     and   ");
                    Parms.Add(new Object[] { "id7", label.Node.NodeID });
                }

                if (label.Bin != null && label.Bin.BinID != 0)
                {
                    sql.Append(" a.Bin.BinID = :id8     and   ");
                    Parms.Add(new Object[] { "id8", label.Bin.BinID });
                }


                if (label.Bin != null && label.Bin.Location != null && label.Bin.Location.LocationID != 0)
                {
                    sql.Append(" a.Bin.Location.LocationID = :id10     and   ");
                    Parms.Add(new Object[] { "id10", label.Bin.Location.LocationID });
                }


                if (label.Bin != null && !String.IsNullOrEmpty(label.Bin.BinCode))
                {
                    sql.Append(" a.Bin.BinCode = :id9   and   ");
                    Parms.Add(new Object[] { "id9", label.Bin.BinCode });
                }

                if (label.Printed != null)
                {
                    sql.Append(" a.Printed = :nom6     and   ");
                    Parms.Add(new Object[] { "nom6", label.Printed });
                }


                if (!String.IsNullOrEmpty(label.PrintingLot))
                {
                    sql.Append(" a.PrintingLot = :nom9     and   ");
                    Parms.Add(new Object[] { "nom9", label.PrintingLot });
                }


                if (!String.IsNullOrEmpty(label.Notes))
                {
                    sql.Append(" a.Notes = :notes     and   ");
                    Parms.Add(new Object[] { "notes", label.Notes });
                }

                if (label.IsLogistic == true)
                {
                    sql.Append(" a.IsLogistic = :nom7     and   ");
                    Parms.Add(new Object[] { "nom7", true });
                }

                else if (label.IsLogistic == false)
                {
                    sql.Append(" a.IsLogistic = :nom8     and   ");
                    Parms.Add(new Object[] { "nom8", false });
                }


                else if (label.CurrQty < 0)
                    sql.Append(" a.CurrQty <= 0     and   ");
                

                if (label.ReceivingDocument != null && label.ReceivingDocument.DocID != 0)
                {
                    sql.Append(" a.ReceivingDocument.DocID = :idr8     and   ");
                    Parms.Add(new Object[] { "idr8", label.ReceivingDocument.DocID });
                }

                if (label.ShippingDocument != null && label.ShippingDocument.DocID != 0)
                {
                    sql.Append(" a.ShippingDocument.DocID = :ids8     and   ");
                    Parms.Add(new Object[] { "ids8", label.ShippingDocument.DocID });
                }

                if (label.LabelSource != null && label.LabelSource.LabelID != 0)
                {
                    sql.Append(" a.LabelSource.LabelID = :ils     and   ");
                    Parms.Add(new Object[] { "ils", label.LabelSource.LabelID });
                }


            }

            sql = new StringBuilder(sql.ToString());
            //sql.Append("1=1 order by a.LabelID asc ");
            sql.Append("1=1 order by a.LabelType.DocTypeID ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


        public long SelectSequence()
        {
            //string sQuery = "select IsNull(Max(Cast(l.LabelCode as Numeric)),1) from Trace.Label l Where l.LabelTypeID = :id1 ";
            string sQuery = "select IsNull(Max(Cast(LEFT(l.LabelCode,LEN(l.Labelcode)-1) as Numeric)),1) from Trace.Label l Where l.LabelTypeID = :id1 AND ISNUMERIC(LEFT(l.LabelCode,LEN(l.Labelcode)-1)) = 1 ";

            StringBuilder sql = new StringBuilder();
            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "id1", LabelType.ProductLabel });

            IQuery query = Factory.Session.CreateSQLQuery(sQuery);
            SetParameters(query);

            return long.Parse(query.UniqueResult().ToString());
        }

        /// <summary>
        /// Obtiene current qty de unidad basica o logistica
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Double SelectCurrentQty(Label data, Product product, bool includeLabeled)
        {

            //Dice si se debeincluir tambien en el inventario el producto marcado.

            StringBuilder sql = new StringBuilder("select Sum(l.CurrQty*l.Unit.BaseAmount) from Label l Where l.Status.StatusID = :id4 and l.Node.NodeID = :nd0 ");
            Parms = new List<Object[]>();

            if (data.Bin != null && data.Bin.BinID != 0 && data.LabelType.DocTypeID == LabelType.BinLocation)
            {
                sql.Append(" and l.Bin.BinID = :id2 ");
                Parms.Add(new Object[] { "id2", data.Bin.BinID });

                if (!includeLabeled)
                { //Solo cuenta lo que este UnLabeled en el BIN
                    sql.Append(" and l.Printed = :fl1 "); //FatherLabel.LabelID is null
                    Parms.Add(new Object[] { "fl1", false });
                }

            }
            else if (data.LabelID != 0 && (data.LabelType.DocTypeID == LabelType.ProductLabel || data.LabelType.DocTypeID == LabelType.UniqueTrackLabel))
            {
                sql.Append(" and (l.LabelID = :id1 OR l.FatherLabel.LabelID = :id1)");
                Parms.Add(new Object[] { "id1", data.LabelID });
            }

            if (product != null && product.ProductID != 0)
            {
                sql.Append(" and  l.Product.ProductID = :id3 ");
                Parms.Add(new Object[] { "id3", product.ProductID });
            }

            Parms.Add(new Object[] { "id4", EntityStatus.Active });
            Parms.Add(new Object[] { "nd0", NodeType.Stored });

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);

            try { return Double.Parse(query.UniqueResult().ToString(), ListValues.DoubleFormat()); }
            catch { return 0; }
        }


        /// <summary>
        /// Retorna una lista de objetos que contienen las ubicaciones de producto en bodega, 
        /// producto,cantidad en unidad base
        /// </summary>
        /// <param name="data"></param>
        /// <param name="records">Numero de registros a devolver</param>
        /// <returns></returns>
        public IList<ProductStock> GetStock(ProductStock data, PickMethod pickMethod, int records)
        {
            //Sale solo product, y del nodo stored
            string initQuery = "select p.ProductID, p.BaseUnitID, b.BinID, SUM(CASE WHEN l.Printed = 0 THEN l.CurrQty*u.BaseAmount ELSE 0 END) AS Stock,"
            + " SUM(CASE WHEN l.Printed = 1 THEN l.CurrQty*u.BaseAmount ELSE 0 END) as PackStock, Max(l.CreationDate) as MaxDate, Min(l.CreationDate) as MinDate " +
                " from Trace.Label l inner join Master.Product p on l.ProductID = p.ProductID " +
                " Inner Join Master.Unit u On l.UnitID = u.UnitID Inner Join Master.Bin b On l.BinID = b.BinID " +
                " where l.CurrQty > 0 AND (l.LabelTypeID = :lt1 OR l.LabelTypeID = :lsn1)  AND l.NodeID = :nd1 AND l.StatusID = :st1 ";

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "lt1", LabelType.ProductLabel });
            Parms.Add(new Object[] { "lsn1", LabelType.UniqueTrackLabel });
            Parms.Add(new Object[] { "nd1", NodeType.Stored });
            Parms.Add(new Object[] { "st1", EntityStatus.Active });


            if (data.Product != null && data.Product.ProductID != 0)
            {
                sql.Append(" and p.ProductID = :id1 ");
                Parms.Add(new Object[] { "id1", data.Product.ProductID });
            }


            if (data.Bin != null)
            {
                if (data.Bin.BinID != 0)
                {
                    sql.Append(" and b.BinID = :id8 ");
                    Parms.Add(new Object[] { "id8", data.Bin.BinID });
                }

                if (!string.IsNullOrEmpty(data.Bin.BinCode))
                {
                    sql.Append(" and b.BinCode = :idz2 ");
                    Parms.Add(new Object[] { "idz2", data.Bin.BinCode });
                }

                if (data.Bin.Location != null && data.Bin.Location.LocationID != 0)
                {
                    sql.Append(" and b.LocationID = :id10 ");
                    Parms.Add(new Object[] { "id10", data.Bin.Location.LocationID });
                }

            }


            sql = new StringBuilder(sql.ToString());
            sql.Append(" group by  p.ProductID, p.BaseUnitID, b.BinID ");


            //Order By Picking Method

            if (pickMethod == null || pickMethod.MethodID == PickingMethod.FIFO)
                sql.Append(" order by  MinDate");

            else if (pickMethod.MethodID == PickingMethod.LIFO)
                sql.Append(" order by  MaxDate DESC");

            //else if (pickMethod.MethodID == PickingMethod.ZONE)
            //sql.Append(" order by  b.ZoneID, b.Rank");

            //else if (pickMethod == PickingMethod.FEFO)
            //    sql.Append(" order by  ExpirationDate DESC");




            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            query.SetMaxResults(records);

            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }


        /// <summary>
        /// Entrega lo que esta pendiente por recibir de un Documento contra un NodeTrace
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IList<Label> GetDocumentLabelAvailable(Document document, Label searchLabel)
        {

            StringBuilder sql = new StringBuilder("select distinct l from Label l ");

            Parms = new List<Object[]>();


            if (document != null && document.DocID != 0)
            {
                sql.Append(", DocumentLine dl Where l.Product.ProductID = dl.Product.ProductID And dl.Document.DocID = :id1 And l.CurrQty > 0  ");
                Parms.Add(new Object[] { "id1", document.DocID });
            }
            else
                sql.Append(" Where l.CurrQty > 0 ");


            if (searchLabel.Node != null && searchLabel.Node.NodeID != 0)
            {
                sql.Append(" And l.Node.NodeID = :id2 ");
                Parms.Add(new Object[] { "id2", searchLabel.Node.NodeID });
            }

            if (document.Location != null && document.Location.LocationID != 0)
            {
                sql.Append(" And l.Bin.Location.LocationID = :id10   ");
                Parms.Add(new Object[] { "id10", document.Location.LocationID });
            }


            if (searchLabel.Printed != null)
            {
                sql.Append(" And l.Printed = :nom6 ");
                Parms.Add(new Object[] { "nom6", searchLabel.Printed });
            }



            sql.Append(" AND l.Status.StatusID = :id3 order by l.LabelID desc"); // 
            Parms.Add(new Object[] { "id3", EntityStatus.Active });


            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            query.SetMaxResults(WmsSetupValues.NumRegs);

            return query.List<Label>();
        }


        public IList<ProductStock> GetDocumentStock(Document document, bool printed)
        {
            string initQuery = "SELECT l.ProductID, l.UnitID, bin.BinID, SUM(CASE WHEN l.Printed = 0 THEN l.CurrQty ELSE 0 END) AS Stock,"
            + " SUM(CASE WHEN l.Printed = 1 THEN l.CurrQty ELSE 0 END) as PackStock, Max(l.CreationDate) as MaxDate, Min(l.CreationDate) as MinDate "
                        + "FROM Trace.DocumentLine AS dl INNER JOIN "
                        + "Trace.Label AS l ON dl.ProductID = l.ProductID INNER JOIN "
                        + "(SELECT BinID FROM  Master.Bin) AS bin ON l.BinID = bin.BinID And l.NodeID = :nd1 And l.StatusID = :sd1 ";

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();


            Parms.Add(new Object[] { "nd1", NodeType.Stored }); //Node Stored
            Parms.Add(new Object[] { "sd1", EntityStatus.Active }); //Active

            if (document != null && document.DocID != 0)
            {
                sql.Append(" and dl.DocID = :id1 ");
                Parms.Add(new Object[] { "id1", document.DocID });
            }


            //if (printed == true)
            //{
            //    sql.Append(" AND l.Printed = :id2 ");
            //    Parms.Add(new Object[] { "id2", true });
            //}
            //else if (printed == false)
            //{
            //    sql.Append(" AND l.Printed = :id2 ");
            //    Parms.Add(new Object[] { "id2", false });
            //}

            sql.Append("GROUP BY bin.BinID, l.ProductID, l.UnitID ORDER BY l.ProductID, Stock desc");

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);


            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }


        public IList<ProductStock> GetSuggestedBins(Product product, Location location, PickMethod pickMethod)
        {

            string initQuery = "SELECT l.ProductID, p.BaseUnitID, l.BinID, SUM(l.SingleQuantity * u.BaseAmount) AS Stock,  SUM(l.PackQuantity*u.BaseAmount) as PackStock, l.MaxDate, l.MinDate, b.Rank "
             + " FROM vwStockSummary AS l INNER JOIN Master.Unit u ON u.UnitID = l.UnitID INNER JOIN Master.Product p ON p.ProductID = l.ProductID INNER JOIN Master.Bin b ON b.BinID = l.BinID "
            + " Where l.NodeID = :nid And l.StatusID = :sid AND ISNULL(b.LevelCode,'') != 'NOUSE' ";


            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();


            Parms.Add(new Object[] { "nid", NodeType.Stored });
            Parms.Add(new Object[] { "sid", EntityStatus.Active });

            if (product != null && product.ProductID != 0)
            {
                sql.Append(" and l.ProductID = :id1 ");
                Parms.Add(new Object[] { "id1", product.ProductID });
            }

            if (location != null && location.LocationID != 0)
            {
                sql.Append(" and l.LocationID = :id2 ");
                Parms.Add(new Object[] { "id2", location.LocationID });
            }

            sql.Append(" GROUP by   l.ProductID, p.BaseUnitID, l.BinID, l.MaxDate, l.MinDate, b.Rank");
            sql.Append(" HAVING SUM(l.SingleQuantity * u.BaseAmount) > 0 OR SUM(l.PackQuantity * u.BaseAmount) > 0 ");


            if (pickMethod == null || pickMethod.MethodID == PickingMethod.FIFO)
                sql.Append(" order by  l.MinDate");

            else if (pickMethod.MethodID == PickingMethod.LIFO)
                sql.Append(" order by  l.MaxDate DESC");

            else if (pickMethod.MethodID == PickingMethod.ZONE)
                sql.Append(" order by b.Rank"); //l.BinID

            //else if (pickMethod == PickingMethod.FEFO)
            //sql.Append(" order by  ExpirationDate DESC");


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            query.SetMaxResults(WmsSetupValues.DefaultBinsToShow);

            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }


        public static IList<ProductStock> GetProductStockObject(IList<Object[]> retList, DaoFactory factory)
        {
            if (retList == null || retList.Count == 0)
                return new List<ProductStock>();

            IList<ProductStock> ret = new List<ProductStock>();
            ProductStock curStock;


            foreach (Object[] obj in retList)
            {
                try
                {
                    curStock = new ProductStock();

                    try { curStock.Product = factory.DaoProduct().SelectById(new Product { ProductID = (int)obj[0] }); }
                    catch { }

                    try { curStock.Unit = factory.DaoUnit().SelectById(new Unit { UnitID = (int)obj[1] }); }
                    catch { }

                    try { curStock.Bin = (obj[2] != null && (int)obj[2] != 0) ? factory.DaoBin().SelectById(new Bin { BinID = (int)obj[2] }) : null; }
                    catch { }

                    try { curStock.Stock = Double.Parse(obj[3].ToString()); }
                    catch { }

                    try { curStock.PackStock = Double.Parse(obj[4].ToString()); }
                    catch { }
                    
                    try { curStock.MaxDate = obj[5] != null ? (DateTime?)DateTime.Parse(obj[5].ToString()) : null; }
                    catch { }

                    try { curStock.MinDate = obj[6] != null ? (DateTime?)DateTime.Parse(obj[6].ToString()) : null; }
                    catch { }

                    try { curStock.MinStock = obj[7] != null ? Double.Parse(obj[7].ToString()) : 0; }
                    catch { }

                    try { curStock.MaxStock = obj[8] != null ? Double.Parse(obj[8].ToString()) : 0; }
                    catch { }

                    try { curStock.AuxQty1 = obj[9] != null ? Double.Parse(obj[9].ToString()) : 0; }
                    catch { }

                    ret.Add(curStock);
                }
                catch { }
            }

            return ret;
        }


        public IList<ProductStock> GetReplanishmentList(ProductStock data, Location location, short selector, bool showEmpty, string bin1, string bin2)
        {

            //string initQuery = "SELECT l.ProductID, l.UnitID, l.BinID, l.SingleQuantity + l.PackQuantity AS CurrQty, "+
            //            " 0 as PackStock, l.MaxDate, l.MinDate " +
            //            "FROM         Master.ZoneBinRelation AS zbr INNER JOIN "+
            //            "Master.ZoneEntityRelation AS zer ON zbr.ZoneID = zer.ZoneID INNER JOIN " +
            //            "vwStockSummary AS l ON zer.EntityRowID = l.ProductID AND zbr.BinID = l.BinID AND zer.EntityID = :ip1 Where 1=1 ";

            string initQuery = "";

            if (selector == 1)
            {
                //Obtiene el max Stock o el Min Stock del Bin.
                initQuery = "SELECT zer.EntityRowID, p.BaseUnitID, b.BinID, ISNULL(OutStock.CurrQty,0) AS CurrQty, " +
                            "0 as PackStock, InStock.MaxDate, InStock.MinDate, ISNULL(b.MinBasicUnitcapacity,0) as MinStock,  ISNULL(b.BasicUnitcapacity,0) as MaxStock, InStock.CurrQty  - ISNULL(OutStock.CurrQty,0) " +
                            "FROM    Master.Bin b INNER JOIN Master.ZoneBinRelation AS zbr ON zbr.BinID = b.BinID INNER JOIN " +
                            "Master.ZoneEntityRelation AS zer ON zbr.ZoneID = zer.ZoneID AND zer.EntityID = :ip1 " +
                            "INNER JOIN Master.Product as p on p.ProductID = zer.entityrowid " +

                            //Muestra solo lo que tenga Stock en IN
                            "INNER JOIN (  " +
                            "SELECT l.ProductID, SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) AS CurrQty, MAX(l.MaxDate) as MaxDate, MIN(l.MinDate) as MinDate " +
                            "FROM  vwStockSummary AS l INNER JOIN Master.Product as p on p.ProductID = l.ProductID WHERE l.NodeID = :nid And l.StatusID = :sid  and l.LocationID = :id3 " +
                            "GROUP BY l.ProductID HAVING  SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) > 0 ) InStock " +
                            "ON InStock.ProductID = zer.EntityRowID " +

                            "LEFT OUTER JOIN  vwStockSummary AS l ON zer.EntityRowID = l.ProductID AND l.LocationID = :id3 " + //
                            //Out Stock
                            "LEFT OUTER JOIN ( SELECT " +
                                "zer.EntityRowID, SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) AS CurrQty " +
                                "FROM    Master.Bin b INNER JOIN Master.ZoneBinRelation AS zbr ON zbr.BinID = b.BinID INNER JOIN " +
                                "Master.ZoneEntityRelation AS zer ON zbr.ZoneID = zer.ZoneID AND " +
                                "zer.EntityID = :ip1 INNER JOIN  vwStockSummary AS l ON b.BinId = l.BinID AND zer.EntityRowID = l.ProductID " +
                                "WHERE  l.NodeID = :nid And l.LocationID = :id3 AND l.StatusID = :sid AND ( zbr.BinType = 2 ) GROUP BY  zer.EntityRowID) OutStock " +
                                "ON OutStock.EntityRowID = zer.EntityRowID " +

                            " WHERE l.NodeID = :nid And l.StatusID = :sid ";

                if (!showEmpty)
                {
                    initQuery += " AND ISNULL(b.BasicUnitcapacity,0) - ISNULL(OutStock.CurrQty,0) > 0 " +
                        " AND InStock.CurrQty  - ISNULL(OutStock.CurrQty,0) > 0 ";
                }



            }
            else
            {
                //Query Basado en los datos seteados en el BIN para Cada Producto (Relacion, Zona Producto).
                initQuery = "SELECT zer.EntityRowID, p.BaseUnitID, b.BinID, ISNULL(OutStock.CurrQty,0) AS CurrQty, " +
                            "0 as PackStock, InStock.MaxDate, InStock.MinDate, ISNULL(zbr.MinUnitcapacity,0) as MinStock,  ISNULL(zbr.Unitcapacity,0) as MaxStock, InStock.CurrQty  - ISNULL(OutStock.CurrQty,0) " +
                            "FROM    Master.Bin b INNER JOIN Master.ZoneBinRelation AS zbr ON zbr.BinID = b.BinID INNER JOIN " +
                            "Master.ZoneEntityRelation AS zer ON zbr.ZoneID = zer.ZoneID AND zer.EntityID = :ip1 " +
                            //--"INNER JOIN  vwStockSummary AS l ON zer.EntityRowID = l.ProductID AND l.LocationID = :id3 " + //b.BinId = l.BinID AND
                            "INNER JOIN Master.Product as p on p.ProductID = zer.entityrowid " +

                            //Muestra solo lo que tenga Stock en IN
                            "INNER JOIN (  " +
                            "SELECT l.ProductID, SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) AS CurrQty,  MAX(l.MaxDate) as MaxDate, MIN(l.MinDate) as MinDate  " +
                            "FROM  vwStockSummary AS l INNER JOIN Master.Product as p on p.ProductID = l.ProductID WHERE l.NodeID = :nid And l.StatusID = :sid  and l.LocationID = :id3 " +
                            "GROUP BY l.ProductID HAVING  SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) > 0 ) InStock " +
                            "ON InStock.ProductID = zer.EntityRowID " +

                            "LEFT OUTER JOIN  vwStockSummary AS l ON zer.EntityRowID = l.ProductID AND l.LocationID = :id3 " + //

                            //Out Stock
                            "LEFT OUTER JOIN ( SELECT " +
                                "zer.EntityRowID, SUM(ISNULL(l.SingleQuantity,0) + ISNULL(l.PackQuantity,0)) AS CurrQty " +
                                "FROM    Master.Bin b INNER JOIN Master.ZoneBinRelation AS zbr ON zbr.BinID = b.BinID INNER JOIN " +
                                "Master.ZoneEntityRelation AS zer ON zbr.ZoneID = zer.ZoneID AND " +
                                "zer.EntityID = :ip1 INNER JOIN  vwStockSummary AS l ON b.BinId = l.BinID AND zer.EntityRowID = l.ProductID " +
                                "WHERE  l.NodeID = :nid And l.LocationID = :id3 And l.StatusID = :sid AND ( zbr.BinType = 2 ) GROUP BY  zer.EntityRowID) OutStock " +
                                "ON OutStock.EntityRowID = zer.EntityRowID " +

                            " WHERE l.NodeID = :nid And l.StatusID = :sid ";

                if (!showEmpty)
                {
                    initQuery += " AND ISNULL(zbr.Unitcapacity,0) - ISNULL(OutStock.CurrQty,0) > 0 " +
                    " AND InStock.CurrQty  - ISNULL(OutStock.CurrQty,0) > 0 ";
                }




            }

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();



            Parms.Add(new Object[] { "ip1", EntityID.Product });
            Parms.Add(new Object[] { "nid", NodeType.Stored });
            Parms.Add(new Object[] { "sid", EntityStatus.Active });



            if (data.Product != null && data.Product.ProductID != 0)
            {
                sql.Append(" and zer.EntityRowID = :id1 ");
                Parms.Add(new Object[] { "id1", data.Product.ProductID });
            }

            if (data.Bin != null && data.Bin.BinID != 0)
            {
                sql.Append(" and b.BinID = :id2 ");
                Parms.Add(new Object[] { "id2", data.Bin.BinID });
            }

            if (location != null && location.LocationID != 0)
            {
                sql.Append(" and b.LocationID = :id3 ");
                Parms.Add(new Object[] { "id3", location.LocationID });
            }

            if (data.BinType != 0)
            {
                sql.Append(" and (zbr.BinType = :id4 )"); //zbr.BinType = 0 Or 
                Parms.Add(new Object[] { "id4", data.BinType });
            }


            //Nov 3 - adicion de rango de Bines
            //############################################################################
            if (data.Product != null && !string.IsNullOrEmpty(data.Product.ProductCode))
            {

                sql.Append(" And (p.ProductCode like :nom1 ");

                if (data.Product.ProductCode.Length >= 3)
                {
                    sql.Append(" Or p.Name like :nom2 ");
                    Parms.Add(new Object[] { "nom2", "%" + data.Product.ProductCode + "%" });
                }

                sql.Append(" ) ");

                Parms.Add(new Object[] { "nom1", data.Product.ProductCode + "%" });

            }


            if (!string.IsNullOrEmpty(bin1))
            {
                sql.Append(" and b.BinCode >= :ib1 ");
                Parms.Add(new Object[] { "ib1", bin1 });
            }


            if (!string.IsNullOrEmpty(bin2))
            {
                sql.Append(" and b.BinCode <= :ib2 ");
                Parms.Add(new Object[] { "ib2", bin2 });
            }
            //############################################################################


            //sql.Append(" order by  b.ZoneID, b.Rank");

            if (selector == 1)
                sql.Append(" GROUP BY zer.EntityRowID, p.BaseUnitID, b.BinID, b.MinBasicUnitcapacity, b.BasicUnitcapacity, InStock.CurrQty, OutStock.CurrQty, InStock.MaxDate, InStock.MinDate ");
            else
                sql.Append(" GROUP BY zer.EntityRowID, p.BaseUnitID, b.BinID, zbr.MinUnitcapacity, zbr.Unitcapacity, InStock.CurrQty, OutStock.CurrQty, InStock.MaxDate, InStock.MinDate ");




            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            //query.SetMaxResults(WmsSetupValues.);

            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }


        public IList<ProductStock> GetStock(ProductStock data)
        {

            string initQuery = "SELECT l.ProductID, l.UnitID, l.BinID, l.SingleQuantity AS Stock,  l.PackQuantity as PackStock, l.MaxDate, l.MinDate "
             + "FROM vwStockSummary AS l Where (l.SingleQuantity > 0 Or l.PackQuantity > 0)  And l.NodeID = :nid And l.StatusID = :sid ";

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "nid", NodeType.Stored });
            Parms.Add(new Object[] { "sid", EntityStatus.Active });


            if (data.Product != null && data.Product.ProductID != 0)
            {
                sql.Append(" and l.ProductID = :id1 ");
                Parms.Add(new Object[] { "id1", data.Product.ProductID });
            }


            if (data.Bin != null && data.Bin.BinID != 0)
            {
                sql.Append(" and l.BinID = :id2 ");
                Parms.Add(new Object[] { "id2", data.Bin.BinID });
            }

            if (data.Bin != null && data.Bin.Location != null && data.Bin.Location.LocationID != 0)
            {
                sql.Append(" and l.LocationID = :id3 ");
                Parms.Add(new Object[] { "id3", data.Bin.Location.LocationID });
            }


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }



        public void DeleteEmptyLabels()
        {
            Factory.IsTransactional = true;

            string delQuery = "Select l From Label l WHERE l.Printed = :pn and l.CurrQty = 0";

            Parms = new List<Object[]>();
            Parms.Add(new Object[] { "pn", false });

            IQuery query = Factory.Session.CreateQuery(delQuery);
            SetParameters(query);

            //Recorre los labels
            foreach (Label lbl in query.List<Label>())
                try
                {
                    Factory.DaoLabel().Delete(lbl);
                }
                catch { }

            Factory.Commit();
            Factory.IsTransactional = false;


        }



        public IList<ProductStock> GetStock(Label data)
        {
            //Sale solo product, y del nodo stored
            string initQuery = "select p.ProductID, p.BaseUnitID, 0 as BinID, SUM(l.CurrQty * u.BaseAmount) AS Stock,"
            + " SUM(CASE WHEN l.Printed = 1 THEN l.CurrQty * u.BaseAmount ELSE 0 END) as PackStock, Max(l.CreationDate) as MaxDate, Min(l.CreationDate) as MinDate " +
                " from Trace.Label l inner join Master.Product p on l.ProductID = p.ProductID " +
                " Inner Join Master.Unit u On l.UnitID = u.UnitID  " +
                " Where l.CurrQty > 0 AND l.LabelTypeID = :lt1 AND l.FatherLabelID = :fid  ";

           // initQuery += " UNION  select p.ProductID, u.UnitID, 0 as BinID, 0 AS Stock, "
           //+ " l.CurrQty as PackStock, l.CreationDate as MaxDate, l.CreationDate as MinDate  "
           //+ "       from Trace.Label l inner join Master.Product p on l.ProductID = p.ProductID  "
           //      + " Inner Join Master.Unit u On p.BaseUnitID = u.UnitID  "
           //      + " Where l.CurrQty > 0 AND l.LabelTypeID = :lx1 AND l.FatherLabelID = :fid  ";


            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "lt1", LabelType.ProductLabel });
            //Parms.Add(new Object[] { "lx2", LabelType.UniqueTrackLabel });
            Parms.Add(new Object[] { "fid", data.LabelID });


            if (data.Product != null )
            {
                if (data.Product.ProductID != 0)
                {
                    sql.Append(" and p.ProductID = :id1 ");
                    Parms.Add(new Object[] { "id1", data.Product.ProductID });
                }

                if (!string.IsNullOrEmpty(data.Product.ProductCode))
                {
                    sql.Append(" and p.ProductCode = :ipc1 ");
                    Parms.Add(new Object[] { "ipc1", data.Product.ProductCode });
                }
            }


            sql = new StringBuilder(sql.ToString());
            sql.Append(" group by  p.ProductID, p.BaseUnitID");


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;

        }



        //public void ConsolidateBins(Label source, Label destination)
        //{
        //    string updQuery = "UPDATE Trace.Label Set BinID = :destination WHERE BinID = :source ";

        //    StringBuilder sql = new StringBuilder(updQuery);
        //    Parms = new List<Object[]>();

        //    Parms.Add(new Object[] { "source", source.Bin.BinID });
        //    Parms.Add(new Object[] { "destination", destination.Bin.BinID });


        //    IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
        //    SetParameters(query);
        //    query.ExecuteUpdate();
        //}


        public IList<Label> GetDocumentLabelAvailableFromTransfer(Document document, Document shipment, Label label)
        {

            string squery = "select distinct l from Label l, DocumentLine dl, NodeTrace nt Where " +
            " l.Product.ProductID = dl.Product.ProductID And dl.Document.DocID = :id1 And l.CurrQty > 0 And " +
            " l.Node.NodeID = nt.Node.NodeID AND l.LabelID = nt.Label.LabelID";

            StringBuilder sql = new StringBuilder(squery);

            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "id1", document.DocID });

            sql.Append(" And l.Node.NodeID = :id2 ");
            Parms.Add(new Object[] { "id2", NodeType.Released });

            sql.Append(" And nt.PostingDocument.DocID = :id4 ");
            Parms.Add(new Object[] { "id4", shipment.DocID });

            if (label != null && label.LabelID != 0)
            {
                sql.Append(" And l.LabelID = :ill4 ");
                Parms.Add(new Object[] { "ill4", label.LabelID });
            }


            sql.Append(" AND l.Status.StatusID = :id3 order by l.LabelID desc"); // 
            Parms.Add(new Object[] { "id3", EntityStatus.Locked });


            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            //query.SetMaxResults(WmsSetupValues.NumRegs);

            IList<Label> returnList = query.List<Label>();

            return returnList;

        }


        /// <summary>
        /// Detailed Obtiene el detalle por Bin/Producto el normal obtiene el producto por location,
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public IList<ProductStock> GetDetailedNodeStock(ProductStock data, Node node, Status status)
        {

            string initQuery = "SELECT l.ProductID, l.UnitID, l.BinID, l.SingleQuantity AS Stock,  l.PackQuantity as PackStock, l.MaxDate, l.MinDate "
             + "FROM vwStockSummary AS l Where (l.SingleQuantity > 0 Or l.PackQuantity > 0)  And l.NodeID = :nid And l.StatusID = :sid ";

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "nid", node.NodeID });
            Parms.Add(new Object[] { "sid", status.StatusID });


            if (data.Product != null && data.Product.ProductID != 0)
            {
                sql.Append(" and l.ProductID = :id1 ");
                Parms.Add(new Object[] { "id1", data.Product.ProductID });
            }


            if (data.Bin != null && data.Bin.BinID != 0)
            {
                sql.Append(" and l.BinID = :id2 ");
                Parms.Add(new Object[] { "id2", data.Bin.BinID });
            }

            if (data.Bin != null && data.Bin.Location != null && data.Bin.Location.LocationID != 0)
            {
                sql.Append(" and l.LocationID = :id3 ");
                Parms.Add(new Object[] { "id3", data.Bin.Location.LocationID });
            }



            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }


        public IList<ProductStock> GetNodeStock(ProductStock data, Node node, Status status)
        {

            string initQuery = "SELECT l.ProductID, l.UnitID, 0 as BinID, Sum(l.SingleQuantity) AS Stock,  Sum(l.PackQuantity) as PackStock, "
             + " Max(l.MaxDate) as MaxDate, Min(l.MinDate) as MinDate "
             + " FROM vwStockSummary AS l Where (l.SingleQuantity > 0 Or l.PackQuantity > 0)  And l.NodeID = :nid And l.StatusID = :sid ";

            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();

            Parms.Add(new Object[] { "nid", node.NodeID });
            Parms.Add(new Object[] { "sid", status.StatusID });


            if (data.Product != null && data.Product.ProductID != 0)
            {
                sql.Append(" and l.ProductID = :id1 ");
                Parms.Add(new Object[] { "id1", data.Product.ProductID });
            }


            if (data.Bin != null && data.Bin.BinID != 0)
            {
                sql.Append(" and l.BinID = :id2 ");
                Parms.Add(new Object[] { "id2", data.Bin.BinID });
            }

            if (data.Bin != null && data.Bin.Location != null && data.Bin.Location.LocationID != 0)
            {
                sql.Append(" and l.LocationID = :id3 ");
                Parms.Add(new Object[] { "id3", data.Bin.Location.LocationID });
            }


            sql.Append(" Group By l.ProductID, l.UnitID ");

            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);


            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }


        public IList<ProductStock> GetDocumentSuggestedBins(Document document, Location location, PickMethod pickMethod)
        {
            string initQuery = "SELECT l.ProductID, l.UnitID, l.BinID, l.SingleQuantity AS Stock,  l.PackQuantity as PackStock, l.MaxDate, l.MinDate "
             + "FROM vwStockSummary AS l INNER JOIN Trace.DocumentLine dl ON dl.ProductID = l.ProductID "+
             " Where (l.SingleQuantity > 0 Or l.PackQuantity > 0) And l.NodeID = :nid And l.StatusID = :sid ";


            StringBuilder sql = new StringBuilder(initQuery);
            Parms = new List<Object[]>();


            Parms.Add(new Object[] { "nid", NodeType.Stored });
            Parms.Add(new Object[] { "sid", EntityStatus.Active });

            if (document != null && document.DocID != 0)
            {
                sql.Append(" and dl.DocID = :id1 ");
                Parms.Add(new Object[] { "id1", document.DocID });
            }

            if (location != null && location.LocationID != 0)
            {
                sql.Append(" and l.LocationID = :id2 ");
                Parms.Add(new Object[] { "id2", location.LocationID });
            }




            if (pickMethod == null || pickMethod.MethodID == PickingMethod.FIFO)
                sql.Append(" order by  l.MinDate");

            else if (pickMethod.MethodID == PickingMethod.LIFO)
                sql.Append(" order by  l.MaxDate DESC");

            else if (pickMethod.MethodID == PickingMethod.ZONE)
                sql.Append(" order by  l.BinID");

            //else if (pickMethod == PickingMethod.FEFO)
            //sql.Append(" order by  ExpirationDate DESC");


            IQuery query = Factory.Session.CreateSQLQuery(sql.ToString());
            SetParameters(query);

            query.SetMaxResults(WmsSetupValues.DefaultBinsToShow);

            IList<ProductStock> ret = GetProductStockObject(query.List<Object[]>(), Factory);

            if (!Factory.IsTransactional)
                Factory.Commit();

            return ret;
        }



        public IList<Label> GetUniqueTrackLabels(Label searchLabel)
        {

            StringBuilder sql = new StringBuilder("select l from Label l  where 1=1 ");

            Parms = new List<Object[]>();

            if (searchLabel.LabelType != null && searchLabel.LabelType.DocTypeID != 0)
            {
                sql.Append(" And l.LabelType.DocTypeID = :id3 ");
                Parms.Add(new Object[] { "id3", searchLabel.LabelType.DocTypeID });
            }
            else {
                sql.Append(" And (l.LabelType.DocTypeID = :id3 OR l.LabelType.DocTypeID = :idx3)");
                Parms.Add(new Object[] { "id3", LabelType.UniqueTrackLabel });
                Parms.Add(new Object[] { "idx3", LabelType.ProductLabel });
            }

            if (searchLabel.Printed != null)
            {
                sql.Append(" And l.Printed = :p3 ");
                Parms.Add(new Object[] { "p3", searchLabel.Printed });
            }

            sql.Append(" And (l.FatherLabel.LabelID = :id2 OR l.FatherLabel.LabelID in (select x.LabelID from Label x where x.FatherLabel.LabelID = :id2 And x.LabelType = :id4 )) ");
            Parms.Add(new Object[] { "id2", searchLabel.FatherLabel.LabelID });
            Parms.Add(new Object[] { "id4", LabelType.ProductLabel });
           

            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            //query.SetMaxResults(WmsSetupValues.NumRegs);

            return query.List<Label>();
        }
    }
}

