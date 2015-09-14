using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Report;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Entities.Trace;
using Integrator;
using Entities;
using Entities.Master;
using System.Reflection;
using Entities.General;
using Microsoft.Reporting.WinForms;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using Entities.Process;
using UtilTool.PrintFile;

namespace BizzLogic.Logic
{
    /// <summary>
    /// Maneja el modulo de despliegue de reportes documentos - Tickets y otros
    /// </summary>
    public partial class ReportMngr : BasicMngr
    {

        public ReportHeaderFormat GetReportInformation(Document document, string template)
        {
            DataSet result = new DataSet();

            // quitamos temporalmente el location del Documento, para q no busque por ese filtro
            Location loc_doc = document.Location;
            document.Location = null;

            //Obtiene los datos del documento a procesar cuando el reporte es de tipo documento
            document = Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First(); //.SelectById(document);
            document.Company = Factory.DaoCompany().SelectById(document.Company);

            document.Location = loc_doc;
            //Convert Object To DataSet


            if (template == WmsSetupValues.HAZMAT_REPORT)
                return ProcessHeaderAndDetailsForHAZMAT(document);
            
            else
            { //DEFAULT

                //Si es Shipment hace un tratamiento especial del document
                if (document.DocType.DocTypeID == SDocType.SalesShipment)
                    return ProcessHeaderAndDetailsForShipment(document);

                else if (document.DocType.DocTypeID == SDocType.CountTask)
                    return ProcessHeaderAndDetailsForCounting(document);

                else
                    return ProcessHeaderAndDetails(document);
            }

        }




        private ReportHeaderFormat ProcessHeaderAndDetailsForHAZMAT(Document document)
        {

            ReportHeaderFormat header = ProcessHeader(document);


            #region Map Details for each document Line
            // 
            //ReportDetailFormat detail;
            IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();
            ReportDetailFormat detail;

            //TODO: Incluir Filtro por bodega zona en este punto para solo obtener los detalles del filtro
            int sequence = 1;

            IList<DocumentLine> hazmatLines = Factory.DaoDocumentLine()
                .Select(new DocumentLine { Document = new Document { DocID = document.DocID } });

            bool isHazmat = false;
            foreach (DocumentLine line in hazmatLines)
                if (line.Product.ProductTrack.Any(f => f.TrackOption.Name == "HAZMAT"))
                {
                    isHazmat = true;
                    line.Note = "HAZMAT";
                }
                

            if (!isHazmat)   //SI NO ES HAZMAT SALE
                return null;

            int qtyOrder;
            foreach (Product product in hazmatLines.Where(f=>f.Note=="HAZMAT").Select(f=>f.Product).Distinct())
            {
                detail = new ReportDetailFormat();

                detail.SeqLine = sequence++;
                detail.ProductDescription = string.IsNullOrEmpty(product.Comment) ? "NO HAZMAT COMMENT, PLEASE GO TO PRODUCT CARD" : product.Comment;
                detail.ProductDescription = product.Name + "\n" + detail.ProductDescription;
                detail.ProductCode = product.ProductCode;
                

                qtyOrder = (int)hazmatLines.Where(f=>f.Product.ProductID == product.ProductID).Sum(f=>f.Quantity);
                detail.QtyOrder = (product.UnitsPerPack > 0) ? qtyOrder / product.UnitsPerPack : qtyOrder;

                //Peso por caja
                detail.PackWeight  = product.Weight * detail.QtyOrder;

                // CAA [2010/05/07]
                // Se agrega el campo de peso (individual)
                detail.Weight = product.Weight;

                if (detail.ProductDescription.Contains("Not Regulated"))
                    detail.Notes = "";
                else
                    detail.Notes = "XX";

                detailList.Add(detail);

                header.TotalCases += detail.QtyOrder;
                header.TotalWeight += detail.PackWeight;

            }

            #endregion


            header.ReportDetails = detailList.ToList();
            return header;

        }




        private ReportHeaderFormat ProcessHeaderAndDetailsForCounting(Document document)
        {
            ReportHeaderFormat header = ProcessHeader(document);

            //Consolida todos los ajuste realizados para Confirmar la Tarea de Inventario
            //Y los muestra como Documento.
            IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();
            ReportDetailFormat detail;

            int seq = 1;
            if (document.DocStatus.StatusID == DocStatus.New || document.DocStatus.StatusID == DocStatus.InProcess)
            {
                //Si no esta posteada muestar la Executon task

                //Pattern
                IList<ProductStock> listCompleted = Factory.DaoBinByTask().GetCountInitialSummary(document);

                //Para que el doc no salga en blanco
                if (listCompleted == null || listCompleted.Count == 0) 
                    detailList.Add(new ReportDetailFormat());


                foreach (ProductStock record in listCompleted)
                {
                    detail = new ReportDetailFormat();

                    if (record.Product != null)
                    {
                        detail.ProductCode = record.Product.ProductCode;
                        detail.ProductDescription = record.Product.Name;
                    }
                    else
                    {
                        detail.ProductCode = "";
                        detail.ProductDescription = "";
                    }

                    detail.StockBin = record.Bin.BinCode;
                    detail.SeqLine = seq++;
                    detail.Rank = record.Bin.Rank;

                    detail.QtyOrder = record.Stock - record.PackStock; //DIFF
                    detail.QtyPending = record.PackStock; //Expected
                    try { detail.ProductCost = record.Product.ProductCost; }
                    catch { }
                    try { detail.ExtendedCost = record.Product.ProductCost * (record.Stock - record.PackStock); }
                    catch { }
                    detail.Date1 = record.MinDate != null ? record.MinDate.Value.ToString() : "";
                    detail.CreatedBy = document.CreatedBy;
                    detail.DocNumber = document.DocNumber;

                    detail.BarcodeLabel = record.Label != null ? "Barcode: " + record.Label.LabelCode : "";
                    // CAA [2010/07/01] tipo de conteo
                    detail.Rank = 0;


                    detailList.Add(detail);

                    var sortedProducts = from p in detailList orderby p.Rank select p;
                    header.ReportDetails = sortedProducts.ToList();

                }


            }


            else if (document.DocStatus.StatusID == DocStatus.Completed)
            {
                //Si no esta posteada muestar la Executon task

                //Pattern
                IList<CountTaskBalance> listCompleted = Factory.DaoBinByTaskExecution().GetCountSummary(document, false);

                // CAA [2010/07/09]
                // Excluye los registros buenos...  (se ocultan en el reporte)
                bool hide = false;
                try{
                    if (GetConfigOption("COUNTHIDE").Equals("T"))
                    {
                        //listCompleted = listCompleted.Where(f=> f.Difference!=0 || !string.IsNullOrEmpty(f.Comment)).ToList();
                        hide = true;
                    }
                }
                catch {}

                //Para que el doc no salga en blanco
                if (listCompleted == null || listCompleted.Count == 0)
                    detailList.Add(new ReportDetailFormat());


                foreach (CountTaskBalance record in listCompleted)
                {
                    detail = new ReportDetailFormat();
                    detail.ProductCode = record.Product.ProductCode;
                    detail.ProductDescription = record.Product.Name;
                    detail.StockBin = record.Bin.BinCode;
                    detail.QtyOrder = record.Difference; //DIFF
                    detail.QtyPending = record.QtyExpected; //Expected
                    detail.ProductCost = record.Product.ProductCost;
                    detail.ExtendedCost = record.Product.ProductCost * (detail.QtyOrder);
                    detail.Date1 = DateTime.Today.ToString();
                    detail.CreatedBy = document.CreatedBy;
                    detail.DocNumber = document.DocNumber;
                    detail.Notes = record.Comment;

                    detail.BarcodeLabel = record.Label != null ? "Barcode: " + record.Label.LabelCode : "";
                    // CAA [2010/07/01] tipo de conteo
                    detail.Rank = record.CaseType;

                    // Se ocultará en el reporte
                    if (hide && (record.Difference==0 && string.IsNullOrEmpty(record.Comment)))
                        detail.Custom1 = "T";

                    detailList.Add(detail);
                }

                header.ReportDetails = detailList.OrderBy(f => f.StockBin).ToList();

            }
            else if (document.DocStatus.StatusID == DocStatus.Posted)
            {
                //Si la tarea esta posteada muestra los ajustes de inventario.
                DocumentLine patternLine = new DocumentLine { 
                    Document = new Document { CustPONumber = document.DocNumber, Company = document.Company }
                };
                
                IList<DocumentLine> lines = Factory.DaoDocumentLine().Select(patternLine);

                // CAA [2010/07/09] Se incluyen los labels de el bin NoCount.
                IList<Label> labelsNoCount = Factory.DaoLabel().Select(new Label { Bin = new Bin { BinCode = DefaultBin.NOCOUNT }, Notes = document.DocNumber });

                //para que el doc no salga en blanco
                if ((lines == null || lines.Count == 0) && (labelsNoCount == null || labelsNoCount.Count == 0))
                    detailList.Add(new ReportDetailFormat());


                int sing = 0;
                foreach (DocumentLine record in lines)
                {
                    sing = record.IsDebit == true ? -1 : 1;

                    detail = new ReportDetailFormat();
                    detail.ProductCode = record.Product.ProductCode;
                    detail.ProductDescription = record.Product.Name;
                    detail.StockBin = record.BinAffected;
                    detail.QtyOrder = sing * record.Quantity; //DIFF
                    detail.QtyPending = record.QtyAllocated;
                    detail.ProductCost = record.Product.ProductCost;
                    detail.ExtendedCost = record.Product.ProductCost * record.Quantity * sing;
                    detail.Date1 = record.Date1 != null ? record.Date1.Value.ToString() : "";
                    detail.DocNumber = record.Document.DocNumber + ", " + record.Document.Comment;

                    detail.BarcodeLabel = "";
                    // CAA [2010/07/01] tipo de conteo
                    detail.Rank = 0;

                    detailList.Add(detail);
                }

                // NoCount Labels
                foreach (Label record in labelsNoCount)
                {
                    //sing = record.IsDebit == true ? -1 : 1;

                    detail = new ReportDetailFormat();
                    detail.ProductCode = record.Product.ProductCode;
                    detail.ProductDescription = record.Product.Name;
                    detail.StockBin = record.Bin.BinCode;
                    detail.QtyOrder = 0-record.CurrQty;  //DIFF
                    detail.QtyPending = record.CurrQty ; 
                    detail.ProductCost = record.Product.ProductCost;
                    detail.ExtendedCost= record.Product.ProductCost * record.CurrQty * -1;
                    detail.Date1 = record.ModDate != null ? record.ModDate.Value.ToString() : "";
                    detail.DocNumber = document.DocNumber;

                    detail.BarcodeLabel = "Barcode: " + record.LabelCode;
                    // CAA [2010/07/01] tipo de conteo
                    detail.Rank = 0;

                    detailList.Add(detail);
                }

                header.ReportDetails = detailList.OrderBy(f => f.StockBin).ToList();
            }



            return header;

        }



        /// <summary>
        /// Shipment document debe manejar los packages en el DEtail, the Header es el mismo
        /// que los documentos standar
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private ReportHeaderFormat ProcessHeaderAndDetailsForShipment(Document document)
        {

            ReportHeaderFormat header = ProcessHeader(document);

           

            #region Map Details for each document Line

            // 
            //ReportDetailFormat detail;
            IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();

            //TODO: Incluir Filtro por bodega zona en este punto para solo obtener los detalles del filtro
            //int sequence = 1, subSequence = 1;
            double totWeight = 0, totCases = 0, allCases = 0, totPallet = 0, totQtyOrder = 0, totProductWeight = 0;
            double totExtendedPrice = 0;

            //Obteniendo los Packages de ese Documento.
            IList<DocumentPackage> packList = Factory.DaoDocumentPackage().Select(
                new DocumentPackage { PostingDocument = document });

            IList<Label> containedLabels;
            ReportDetailFormat detail;
            IList<ReportDetailFormat> packDetail; 
            int level = -1; // 0 = Box, 1 = pallet

            foreach (DocumentPackage pkg in packList)
            {

                header.Date2 = pkg.CreationDate.ToString();

                //Obtiene los labels Contenidos en ese package                
                containedLabels = Factory.DaoLabel().Select(new Label { FatherLabel = pkg.PackLabel })
                    .Where(f => f.StockQty > 0).ToList();
                //.Where(f=>f.CurrQty > 0).ToList();

                //Guarda los detalles de cada paquete
                packDetail = new List<ReportDetailFormat>();


                //Adicion manejo de Pallets and cases.
                /*
                if (pkg.ChildPackages == null || pkg.ChildPackages.Count == 0)
                {
                    if (pkg.ParentPackage == null)
                    {
                        level = 0; //Single box
                        totCases++;
                    }
                    else
                        level = 1; //Box inside pallet

                    allCases++;
                }
                else
                {
                    level = 2; //Pallet
                    totPallet++;
                }
                */

                if (pkg.PackageType == "P")
                {
                    level = 2; //Pallet
                    totPallet++;
                }
                else if (pkg.PackageType == "B")
                {
                    level = 1; 

                    if (pkg.ParentPackage == null)
                    {
                        level = 0; //Single box
                        totCases++;
                    }

                    allCases++;
                }
                else { allCases++; }



                //foreach (Label curLabel in containedLabels)
                foreach (Product curLabel in containedLabels.Select(f => f.Product).Distinct())
                {

                    detail = new ReportDetailFormat();


                    //Grouped By Pack - Label
                    detail.BarcodeLabel = pkg.PackLabel.Barcode;
                    detail.PackWeight = pkg.Weight;
                    detail.Dimension = pkg.Dimension;
                    detail.Pieces = pkg.Pieces;
                    detail.CreatedBy = pkg.CreatedBy;


                    //Map Data
                    detail.ProductCode = curLabel.ProductCode;
                    detail.ProductDescription = curLabel.Name;
                    detail.Unit = curLabel.BaseUnit.Name;
                    detail.ProductCost = curLabel.ProductCost;
                    //CUSTOM
                    detail.Custom1 = curLabel.Manufacturer;
                    detail.Custom2 = curLabel.Reference;


                    //Agrupo por producto dentro del package. para mostrar una sola linea
                    detail.Weight = curLabel.Weight * containedLabels.Where(f => f.Product.ProductCode == curLabel.ProductCode).Sum(f => f.StockQty * f.Unit.BaseAmount); //CurrQty
                    detail.QtyOrder = containedLabels.Where(f => f.Product.ProductCode == curLabel.ProductCode).Sum(f => f.StockQty * f.Unit.BaseAmount);
                    totQtyOrder += containedLabels.Where(f => f.Product.ProductCode == curLabel.ProductCode).Sum(f => f.StockQty * f.Unit.BaseAmount);
                    totProductWeight += detail.Weight;

                    totExtendedPrice += detail.QtyOrder * curLabel.ProductCost;

                    //Descripcion del pallet
                    //detail.PackLevel = 0;
                    if (level == 0)
                    {
                        //detail.PalletNote = "";
                        detail.LogisticNote = "SINGLE BOX " + pkg.PackLabel.LabelCode;
                    }

                    else if (level == 1)
                    {
                        //CASE INSIDE PALLET
                        //detail.PalletNote = "PALLET " + GetParentPallet(pkg);
                        detail.LogisticNote = "PALLET " + GetParentPallet(pkg) + " >> BOX " + pkg.PackLabel.LabelCode; //"PL " + pkg.ParentPackage.Sequence.ToString() + "/" + pkg.PackLabel.LabelCode;
                    }

                    else
                    {
                        //detail.PalletNote = "PALLET " + pkg.PackLabel.LabelCode;
                        detail.LogisticNote = "PALLET " + pkg.PackLabel.LabelCode + " >> W/OUT BOX";
                    }


                    //IList<ProductAccountRelation> acctItem = null;

                    ////Customer Item Number
                    //if (document.Customer.AccountCode != WmsSetupValues.DEFAULT)
                    //{
                    //    acctItem = curLabel.Product.ProductAccounts.Where(f => f.Account.AccountID == document.Customer.AccountID).ToList();
                    //    if (acctItem != null && acctItem.Count() > 0)
                    //        detail.AccountItemNumber = acctItem[0].ItemNumber;
                    //}

                    //if (detail.AccountItemNumber == null)
                    //    detail.AccountItemNumber = "";

                    //Adicinando el Detalle                    
                    packDetail.Add(detail);
                }

                //Recorre los detalles de cada Package para saber si es un detalle de Kit lo que hace que
                //Adicione un Detalle nuevo (Padre)
                foreach (ReportDetailFormat kitDetail in GetDetailsWithKitAssembly(packDetail, document, pkg))
                    detailList.Add(kitDetail);


                totWeight += pkg.Weight;
                //totCases++;

            }

            #endregion


            #region Shipment Totals
            //En un Shipment enviar el CustPOUmber
            try
            {
                header.OrigNumber = Factory.DaoDocument().Select(new Document { DocNumber = document.CustPONumber, Company = document.Company }).First().CustPONumber;
            }
            catch { }

            // Totals
            try { header.TotalExtended = double.Parse(totExtendedPrice.ToString("###,###.##")); } //totExtendedPrice; 
            catch { }

            header.TotalCases = totCases;
            header.TotalPallets = totPallet;
            header.AllCases = allCases;
            header.TotalWeight = totWeight > 0 ? totWeight : totProductWeight;
            header.TotalQtyOrder = totQtyOrder;


            header.ReportDetails = detailList.OrderBy(f => f.AuxSequence).ToList();

            #endregion

            return header;
        }



        private string GetParentPallet(DocumentPackage pkg)
        {
            if (pkg.ParentPackage == null)
                return pkg.PackLabel.LabelCode;

            return GetParentPallet(pkg.ParentPackage);
        }



        private ReportHeaderFormat ProcessHeader(Document document)
        {
            ReportHeaderFormat header = new ReportHeaderFormat();

            #region Map Document Header

            header.DocumentName = document.DocType.Name;
            header.DocumentNumber = document.DocNumber;
            header.OrigNumber = document.QuoteNumber; //document.ErpMaster.ToString();
            header.CustPONumber = document.CustPONumber;
            header.Reference = document.Reference;
            header.UserDef1 = document.UserDef1;
            header.UserDef2 = document.UserDef2;
            header.UserDef3 = document.UserDef3;


            // Data Section
            header.Date1 = (document.Date1 == null) ? "" : document.Date1.Value.ToShortDateString();
            header.Date2 = (document.Date2 == null) ? "" : document.Date2.Value.ToString();
            header.Date3 = (document.Date3 == null) ? "" : document.Date3.Value.ToShortDateString();
            header.Date4 = (document.Date4 == null) ? "" : document.Date4.Value.ToShortDateString(); 


            try
            {
                header.Warehouse = string.IsNullOrEmpty(document.Location.ErpCode)
                    ? document.Location.Name
                    : document.Location.ErpCode;
            }
            catch { }

            header.Vendor = document.Vendor.Name;
            header.VendorAccount = document.Vendor.AccountCode;
            header.Customer = document.Customer.Name;
            header.CustomerAccount = document.Customer.AccountCode;
            header.FilterBy = document.CreatedBy; //document.User.UserName;
            header.Notes = document.Comment;
            header.Comment = document.Notes;
            header.Printed = DateTime.Today.ToString("MM/dd/yyyy hh:mm:ss");
            
            

            header.Corporate_Name = document.Company.Name;
            header.Corporate_Line1 = document.Company.AddressLine1;
            header.Corporate_Line2 = document.Company.AddressLine2;
            header.Corporate_Line3 = document.Company.AddressLine3;
            header.Corporate_Line4 = document.Company.City + ", " + document.Company.State + " " + document.Company.ZipCode;
            header.Corporate_Line5 = document.Company.Country;
            header.Corporate_Line6 = document.Company.ContactPerson;
            header.CreatedBy = document.CreatedBy;
            header.PickMethod = (document.PickMethod != null) ? document.PickMethod.Name : "";

            //Company Logo
            IList<ImageEntityRelation> img = null;

            try
            {
                img = Factory.DaoImageEntityRelation().Select(new ImageEntityRelation
                    {
                        EntityRowID = document.Company.CompanyID,
                        ImageName = "LOGO",
                        Entity = new ClassEntity { ClassEntityID = EntityID.Company }
                    });
            }
            catch { }

            header.Image1 = (img != null && img.Count > 0) ? img.First().Image : null;

            header.Barcode = document.DocNumber;


            DocumentAddress BillTo_address = null, ShipTo_address = null;


            #region Addresses 

            try
            {
                // BillTo Section
                Document addDoc;
                // header.BillTo_Name = document.Customer.AccountAddresses.ElementAt(0).Name;
                if (document.DocType.DocTypeID == SDocType.SalesShipment)
                {
                    addDoc = Factory.DaoDocument().Select(
                        new Document { DocNumber = document.CustPONumber, Company = document.Company }).First();

                    header.ShipVia = (addDoc.ShippingMethod != null) ? addDoc.ShippingMethod.ErpCode : "";
                }
                else
                {
                    addDoc = document;
                    header.ShipVia = (document.ShippingMethod != null) ? document.ShippingMethod.ErpCode : "";
                }


                BillTo_address = Factory.DaoDocumentAddress().Select(
                        new DocumentAddress
                        {
                            Document = addDoc,
                            AddressType = AddressType.Billing
                        })
                        .Where(f => f.DocumentLine == null).OrderByDescending(f=>f.RowID).First();

                header.BillTo_Name = BillTo_address.Name;
                
                header.BillTo_Line1 = BillTo_address.AddressLine1 + "\n" + BillTo_address.AddressLine2;
                if (!string.IsNullOrEmpty(BillTo_address.AddressLine3))
                    header.BillTo_Line1 += "\n" + BillTo_address.AddressLine3;


                header.BillTo_Line2 = BillTo_address.City + ", " + BillTo_address.State + " " + BillTo_address.ZipCode;
                header.BillTo_Line3 = BillTo_address.Country;
                header.BillTo_Line4 = BillTo_address.Phone1 + " " + BillTo_address.Phone2 + " " + BillTo_address.Phone3 
                    + " " + (BillTo_address.Email == null ? "" : BillTo_address.Email);

            }
            catch { }

            try
            {
                try
                {

                    Document saddDoc;
                    if (document.DocType.DocTypeID == SDocType.SalesShipment)
                    {
                        saddDoc = Factory.DaoDocument().Select(
                                 new Document { DocNumber = document.CustPONumber, Company = document.Company }).First();
                    }
                    else
                        saddDoc = document;



                    ShipTo_address = Factory.DaoDocumentAddress().Select(
                        new DocumentAddress
                        {
                            Document = saddDoc,
                            AddressType = AddressType.Shipping
                        })
                        .Where(f => f.DocumentLine == null).OrderByDescending(f => f.RowID).First();
                }
                catch { ShipTo_address = BillTo_address; }

                header.ShipTo_Name = ShipTo_address.Name;
                
                header.ShipTo_Line1 = ShipTo_address.AddressLine1 + "\n" + ShipTo_address.AddressLine2;
                if (!string.IsNullOrEmpty(ShipTo_address.AddressLine3))
                    header.ShipTo_Line1 += "\n" + ShipTo_address.AddressLine3;
                
                
                header.ShipTo_Line2 = ShipTo_address.City + ", " + ShipTo_address.State + " " + ShipTo_address.ZipCode;
                
                header.ShipTo_Line3 = ShipTo_address.Country;
                header.ShipTo_Line4 = ShipTo_address.Phone1 + " " + ShipTo_address.Phone2 + " " +  ShipTo_address.Phone3 
                    + " "+ (ShipTo_address.Email == null ? "" : ShipTo_address.Email);

            }
            catch { }


            #endregion




            return header;

            #endregion End Header Section

        }



        private IList<ReportDetailFormat> GetDetailsWithKitAssembly(IList<ReportDetailFormat> pkgDetails,
            Document shpDocument, DocumentPackage pkg)
        {

            ReportDetailFormat detail = null;
            DocumentLine curLine = null;
            int kitLine;
            //IList<DocumentLine> processeKits = new List<DocumentLine>();


            //Inner join del documento contra las los pkgdetails, obteniendo los documentlines.
            IList<DocumentLine> shpLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = shpDocument });

            //Obtiene las lineas del documento que son tocadas por los detalles del paquete a procesar.
            shpLines =
                    (from sales in shpLines
                     join pack in pkgDetails on sales.Product.ProductCode equals pack.ProductCode
                     select sales).Distinct().ToList();

            //Lleva el conteo d ela sequencia para cada Kit
            Dictionary<DocumentLine, int> countKit = new Dictionary<DocumentLine,int>();
            int nextKit = 1;
            int countComponent = 1;

            //Recorre los componentes para encontrar su Kit/Assembly Padre.
            foreach (DocumentLine dl in shpLines.Where(f => f.Note == "1").OrderBy(f => f.LinkDocLineNumber))
            {
                try
                {
                    //Entrega la linea del KIT/ASSEMBLY en el Sales Order
                    kitLine = Factory.DaoDocumentLine().Select(
                        new DocumentLine
                        {
                            LineNumber = dl.LinkDocLineNumber,
                            Document = new Document { DocNumber = dl.LinkDocNumber, Company = dl.Document.Company }
                        }
                        ).First().LinkDocLineNumber;

                    curLine = Factory.DaoDocumentLine().Select(
                        new DocumentLine
                        {
                            LineNumber = kitLine,
                            Document = new Document { DocNumber = dl.LinkDocNumber, Company = dl.Document.Company }
                        }
                         ).First();


                    //revisa si ese kit aun no ha sido procesado. Si fue procesado va al siguiente
                    if (countKit.Where(f => f.Key.LineNumber == curLine.LineNumber).Count() > 0)
                    {
                        //A los paquetes que tiene ese producto se les pone subdetail
                        foreach (ReportDetailFormat pkgDet in pkgDetails.Where(f => f.ProductCode == dl.Product.ProductCode))
                        {
                            pkgDet.IsSubDetail = true;
                            pkgDet.AuxSequence = countKit[curLine] + countComponent++;
                            pkgDet.Custom1 = curLine.Product.Category.ExplodeKit.ToString(); //Adicionado para Maxiforce caterpillar
                        }

                        continue;
                    }
                    else
                    {
                        //processeKits.Add(curLine);
                        countKit.Add(curLine, 1000 * nextKit++);
                    }

                    //A los paquetes que tiene ese producto se les pone subdetail
                    foreach (ReportDetailFormat pkgDet in pkgDetails.Where(f => f.ProductCode == dl.Product.ProductCode))
                    {
                        pkgDet.IsSubDetail = true;
                        pkgDet.AuxSequence = countKit[curLine] + countComponent++;
                        pkgDet.Custom1 = curLine.Product.Category.ExplodeKit.ToString(); //Adicionado para Maxiforce caterpillar
                    }                    
                                       

                }
                catch { continue; }



                //Crea una linea para el documento de shipment
                detail = new ReportDetailFormat
                {
                    //Grouped By Pack - Label
                    BarcodeLabel = pkg.PackLabel.Barcode,
                    PackWeight = pkg.Weight,
                    Dimension = pkg.Dimension,

                    //Map Data
                    ProductCode = curLine.Product.ProductCode,
                    ProductDescription = curLine.Product.Name,
                    Unit = curLine.Unit.Name,
                    CreatedBy = pkg.CreatedBy,
                    IsSubDetail = false,
                    AuxSequence = countKit[curLine]

                };


                IList<ProductAccountRelation> acctItem = null;

                //Customer Item Number
                if (shpDocument.Customer.AccountCode != WmsSetupValues.DEFAULT)
                {
                    acctItem = curLine.Product.ProductAccounts.Where(f => f.Account.AccountID == shpDocument.Customer.AccountID).ToList();
                    if (acctItem != null && acctItem.Count() > 0)
                        detail.AccountItemNumber = acctItem[0].ItemNumber;
                }

                if (detail.AccountItemNumber == null)
                    detail.AccountItemNumber = "";


                pkgDetails.Add(detail);
            }

            return pkgDetails;
        }




        /// <summary>
        /// Se encarga de serializar el objeto headr y el detalle en un dataset que pueda se deplegado
        /// por el reporting service
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private ReportHeaderFormat ProcessHeaderAndDetails(Document document)
        {
            ReportHeaderFormat header = ProcessHeader(document);


            #region Map Details for each document Line
            // 
            //ReportDetailFormat detail;
            IList<ReportDetailFormat> detailList = new List<ReportDetailFormat>();

            //TODO: Incluir Filtro por bodega zona en este punto para solo obtener los detalles del filtro
            int sequence = 1, subSequence = 1;

            double totWeight = 0, totCases = 0, totQtyOrder = 0, totQtyPending = 0;
            
            short binDirection = BinType.In_Out; //Bin Direction to use

           if (document.DocType.DocTypeID == SDocType.ReplenishPackTask)
                    binDirection = BinType.In_Only;


            //Gettting document balance para qty pending,
            IList<DocumentBalance> docBalance = null;
            if (document.DocType.DocClass.DocClassID == SDocClass.Receiving)
            {
                docBalance = Factory.DaoDocumentBalance().DetailedBalance(new DocumentBalance
                {
                    Document = document,
                    Node = new Node { NodeID = NodeType.Received }
                }, false);
            }
            else if (document.DocType.DocClass.DocClassID == SDocClass.Shipping)
            {
                docBalance = Factory.DaoDocumentBalance().DetailedBalance(new DocumentBalance
                {
                    Document = document,
                    Node = new Node { NodeID = NodeType.Picked }
                }, document.CrossDocking == true ? true : false);
            }


            double kitTotalOrder = 0;
            foreach (DocumentLine dLine in Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = document.DocID } }).OrderBy(f => f.LineNumber))
            {

                //Adicionado en Dec 30 2009.
                if (dLine.Product.Status.StatusID != EntityStatus.Active)
                    continue;

                //if (dLine.LineStatus.StatusID == DocStatus.Cancelled)
                    //continue;

                if (dLine.LineStatus.StatusID != DocStatus.New && dLine.LineStatus.StatusID != DocStatus.InProcess)
                    continue;

                if (dLine.Quantity <= 0 && dLine.LineNumber != 0)
                    continue;



                //Product To build, el primero en la lista
                if (string.IsNullOrEmpty(header.ProductToBuild))
                {
                    header.ProductToBuild = dLine.Product.ProductCode + ", " + dLine.Product.Name;

                    try
                    {
                        header.Barcode = GetAssignedBinsList(dLine.Product, document.Location).First().Bin.BinCode;
                    }
                    catch { header.Barcode = ""; }
                }


                //Si es el primer item del asembly se sale.
                if (document.DocType.DocTypeID == SDocType.KitAssemblyTask && dLine.LineNumber == 0)
                {
                    kitTotalOrder = dLine.Quantity;
                    continue;
                }




                IList<ReportDetailFormat> evaluatedLines = EvaluateLine(dLine, document, docBalance, binDirection, 1);


                foreach (ReportDetailFormat detail in evaluatedLines)
                {

                    if (detail.IsSubDetail != true)
                    {
                        detail.SeqLine = sequence++;
                        totWeight += detail.Weight;
                        totCases++; // += detail.Cases;
                        totQtyOrder += detail.QtyOrder;
                        totQtyPending += detail.QtyPending;
                        subSequence = 1;
                    }
                    else
                        detail.SeqLine = subSequence++;

                    detailList.Add(detail);

                }


            }

            #endregion

            // Totals
            header.TotalCases = totCases;
            header.TotalWeight = totWeight;
            header.TotalQtyOrder = (kitTotalOrder > 0) ? kitTotalOrder : totQtyOrder;
            header.TotalQtyPending = totQtyPending;


            if (document.DocType.DocTypeID == SDocType.SalesOrder || 
                document.DocType.DocTypeID == SDocType.WarehouseTransferShipment ||
                document.DocType.DocTypeID == SDocType.MergedSalesOrder)
            {
                //header.ReportDetails = detailList.OrderBy(f => f.Rank).ToList();

                //Si tiene componente organiza por componentes y luego por Rank
                if (detailList.Any(f=>f.IsSubDetail == true)) {
                
                    var sortedProducts = from p in detailList orderby p.CustNumber1, p.IsSubDetail, p.Rank select p;
                    header.ReportDetails = sortedProducts.ToList();
                
                }
                else { //Si no, solo por Rank. Aqui vienen las reglas de SORT
                
                    var sortedProducts = from p in detailList orderby p.Rank select p;
                    header.ReportDetails = sortedProducts.ToList();

                }

                
            }
            else
                header.ReportDetails = detailList.OrderBy(f => f.AuxSequence).ToList();


            return header;

        }



        private IList<ReportDetailFormat> EvaluateLine(DocumentLine dLine, Document document, IList<DocumentBalance> docBalance,
            short binDirection, short level)
        {

            IList<ReportDetailFormat> returnList = new List<ReportDetailFormat>();
            ReportDetailFormat detail = new ReportDetailFormat();

            if (level > 2)
                return null;

            if (GetCompanyOption(document.Company, "SHOWBO").Equals("F") && dLine.Quantity - dLine.QtyBackOrder - dLine.QtyCancel <= 0)
                return returnList;
            

            //El  qty pending debe salir del balance. que se obtuvo arriba.
            if (docBalance != null && docBalance.Count > 0)
            {
                try { detail.QtyPending = docBalance.Where(f => f.DocumentLine.LineNumber == dLine.LineNumber).First().QtyPending; }
                catch { detail.QtyPending = detail.QtyOrder; }
            }

            if (document.DocType.DocClass.DocClassID == SDocClass.Shipping && GetCompanyOption(document.Company, "SHOWBO").Equals("F") && detail.QtyPending <= 0)
                return returnList;



            //Map Data
            detail.ProductCode = dLine.Product.ProductCode;

            if (string.IsNullOrEmpty(dLine.LineDescription))
                detail.ProductDescription = dLine.Product.Name;
            else
                detail.ProductDescription = dLine.LineDescription;
            
            detail.AlternProduct = "";
            try
            {
                if (dLine.Product.AlternProducts != null && dLine.Product.AlternProducts[0] != null)
                    detail.AlternProduct = dLine.Product.AlternProducts[0].AlternProduct.ProductCode;
            }
            catch {  }

            detail.Unit = dLine.Unit.Name;
            detail.Notes = dLine.Note;
            detail.AuxSequence = dLine.Sequence;
            detail.CreatedBy = dLine.CreatedBy; 
           

            //CUSTOM
            detail.Custom1 = dLine.Product.Manufacturer;
            detail.Custom2 = dLine.Product.Reference;


            //Definir si es un subdetail
            //detail.IsSubDetail = (dLine.LinkDocLineNumber > 0) ? true : false;
            detail.IsSubDetail = (dLine.Note == "1") ? true : false;

            if (detail.IsSubDetail == true)
                detail.CustNumber1 = dLine.LinkDocLineNumber;
            else
                detail.CustNumber1 = dLine.LineNumber;


            IList<ProductAccountRelation> acctItem = null;

 
            try { detail.AssignedBins = GetAssignedBins(dLine.Product, dLine.Location, binDirection); }
            catch { }

            if (document.DocType.UseStock == true)
            {
                try
                {

                    object[] objBins;

                    if (document.DocType.DocClass.DocClassID == SDocClass.Shipping)
                        objBins = GetSuggestedBins(dLine.Product, dLine.Location, document.PickMethod, BinType.Out_Only); //binDirection

                    else if (document.DocType.DocTypeID == SDocType.ReplenishPackTask)
                    {
                        objBins = GetSuggestedBins(dLine.Product, dLine.Location, document.PickMethod, BinType.Out_Only); //binDirection

                        
                        string[] xBins = objBins[0].ToString().Split("\n".ToCharArray());                        
                        string binResult = "";

                        for (int i = 0; i < xBins.Length; i++)
                            if (!xBins[i].Contains(dLine.Note.Trim()))
                                binResult += xBins[i] + "\n";

                        objBins[0] = binResult;
                        //objBins[2] = objBins[2].ToString().Replace(dLine.Note, "").ToString();
                    }
                    else
                        objBins = GetSuggestedBins(dLine.Product, dLine.Location, document.PickMethod, BinType.In_Out);

                    detail.OutBin = objBins[2].ToString();

                    detail.Rank = objBins[1] != null ? (Int32)objBins[1] : WmsSetupValues.MaxBinRank; //Rank del OutBin  

                    detail.SuggestedBins = objBins[0].ToString();

                    detail.OldestBin = objBins[3].ToString();

                }
                catch { }
            }


            //detail.QtyPending = dLine.QtyPending;
            detail.StockBin = dLine.Location.Name;
            detail.Weight = dLine.Product.Weight * dLine.Quantity;
            detail.Pieces = dLine.Product.UnitsPerPack > 0 ? dLine.Product.UnitsPerPack : 1;
            //detail.Cases = dLine.Quantity;


            //Si es un assembly order solo muestra cantidad para las lineas con Notes = "2" 
            //que significan que son items a piquear., del resto no.
            //if (document.DocType.DocTypeID == SDocType.KitAssemblyTask)
                //detail.QtyOrder = (dLine.Note != null && dLine.Note == "1") ? dLine.Quantity : 0;
            //else
                detail.QtyOrder = dLine.Quantity - dLine.QtyCancel;
                detail.QtyBO = dLine.QtyBackOrder;


            //Customer Item Number
            if (document.Customer.AccountCode != WmsSetupValues.DEFAULT)
            {
                acctItem = dLine.Product.ProductAccounts.Where(f => f.Account.AccountID == document.Customer.AccountID).ToList();
                if (acctItem != null && acctItem.Count() > 0)
                    detail.AccountItemNumber = acctItem[0].ItemNumber;
            }


            //Vendor Item Number
            if (document.Vendor.AccountCode != WmsSetupValues.DEFAULT)
            {
                acctItem = dLine.Product.ProductAccounts.Where(f => f.Account.AccountID == document.Vendor.AccountID).ToList();
                if (acctItem != null && acctItem.Count() > 0)
                    detail.AccountItemNumber = acctItem[0].ItemNumber;
            }

            if (detail.AccountItemNumber == null)
                detail.AccountItemNumber = "";


            //Aasignacion del Detalle Principal.
            returnList.Add(detail);


            ///--- FORMULACION A NIVEL DE DOCUMENTO
            ////Evalua si el producto tiene formula para meter los subitems y ademas si se pinden mostrar los subitems
            //if (dLine.Product.ProductFormula != null && dLine.Product.ProductFormula.Count > 0
            //    && GetCompanyOption(document.Company, "SHOWCOMP").Equals("T") && level == 1  
            //    && document.DocType.DocClass.DocClassID == SDocClass.Shipping )
            //{
            //    IList<ReportDetailFormat> subDetails = null;
            //    DocumentLine subLine;
            //    foreach (KitAssemblyFormula formula in dLine.Product.ProductFormula)
            //    {
            //        subLine = new DocumentLine { 
            //            Product = formula.Component, 
            //            Unit = formula.Unit, Note = "",  
            //            Location = dLine.Location, 
            //            Quantity = formula.FormulaQty * dLine.Quantity,
            //            QtyPending = formula.FormulaQty * detail.QtyPending    
            //        };

            //        subDetails = EvaluateLine(subLine, document, null, binDirection, 2);

            //        foreach (ReportDetailFormat sub in subDetails)
            //        {
            //            sub.IsSubDetail = true;
            //            returnList.Add(sub);

            //        }
            //    }
            //}

            return returnList;

        }



        //public void PrintShipmentDocs(Document shipment, string printer, string appPath)
        //{
        //    shipment = Factory.DaoDocument().Select(new Document { DocID = shipment.DocID }).First();

        //    //Inicializando variables usadas en la impresion
        //    m_streams = new Dictionary<LabelTemplate, IList<Stream>>();
        //    m_currentPageIndex = new Dictionary<LabelTemplate, int>();
        //    AppPath = appPath;

        //    //Obtiene los documentos que se deben imprimir para el Shipment y los manda a imprimir
        //    PrintDocument(shipment, shipment.DocType.Template, appPath);
        //}



        public void PrintDocumentsInBatch(IList<Document> documentList, string appPath, string printer, CustomProcess process)
        {
            Document curDoc;
            //Inicializando variables usadas en la impresion
            m_streams = new Dictionary<LabelTemplate, IList<Stream>>();
            m_currentPageIndex = new Dictionary<LabelTemplate, int>();
            AppPath = appPath;

            //Factory.IsTransactional = true;

            Console.WriteLine("Document List");
            foreach (Document document in documentList)
            {
                curDoc = Factory.DaoDocument().Select(new Document { DocID = document.DocID }).First();

                Console.WriteLine("Print " + curDoc.DocType.Name);
                //Obtiene los documentos que se deben imprimir para el Shipment y los manda a imprimir
                PrintDocument(curDoc, curDoc.DocType.Template, appPath, process);
                Thread.Sleep(3000);

                if (process != null)
                    PrintProcessDocuments(curDoc, process, appPath);

            }

            //Factory.Commit();
        }


        /// <summary>
        /// Recorre el Documento Obtiene los procesos para el tipo de Documento, Cliente, Vendor, Product
        /// </summary>
        /// <param name="curDoc"></param>
        /// <param name="process"></param>
        private void PrintProcessDocuments(Document curDoc, CustomProcess process, string appPath)
        {
            //Recorre los procesos para cada una de la entidades descritas.

            //Productos.            
            IEnumerable<Product> productList = Factory.DaoDocumentLine()
                .Select(new DocumentLine { Document = new Document { DocID = curDoc.DocID } })
                .Select(f => f.Product).Distinct();

            Console.WriteLine("Products");
            foreach (Product product in productList)
                ProcessEntity(EntityID.Product, product.ProductID, process, appPath, curDoc);


            //Imprime el HAZMAT si hay producto con Hazmat.
            try
            {
                LabelTemplate hazmatTpl = Factory.DaoLabelTemplate().Select(
                        new LabelTemplate { Header = WmsSetupValues.HAZMAT_REPORT }).First();


                foreach (Product prd in productList)
                {
                    if (prd.ProductTrack.Any(f => f.TrackOption.Name == "HAZMAT"))
                    {
                        PrintDocument(curDoc, hazmatTpl, appPath, process);
                        Thread.Sleep(3000);
                        break;
                    }
                }
            }
            catch { }

            //Procesnado los documentos para el tipo de Documento
            Console.WriteLine("Document Type");
            ProcessEntity(EntityID.DocumentType, curDoc.DocType.DocTypeID, process, appPath, curDoc);

        }



        private void ProcessEntity(short entity, int entityRow, CustomProcess process, string appPath, Document document)
        {

            //Seleccionando los Files para ese proceso.
            IEnumerable<ProcessEntityResource>  resourceList = 
            Factory.DaoProcessEntityResource().Select(new ProcessEntityResource
            {
                Entity = new ClassEntity { ClassEntityID = entity },
                EntityRowID = entityRow,
                Process = process
            });


            ProcessEntityResource curResource;
            Console.WriteLine("Process for " + entity.ToString());

            foreach (ProcessEntityResource resource in resourceList)
            {

                //Usa Cur REsource en lugar del anterior
                curResource = Factory.DaoProcessEntityResource().Select(
                    new ProcessEntityResource { RowID = resource.RowID }).First();

                //Ejecutar la impresion global en un Hilo     
                if (curResource.Template != null)
                {
                    //Si es un Hazmat revisa si el documento contiene HAZMAT
                    //if (curResource.Template.Header == WmsSetupValues.HAZMAT_REPORT && !document.QuoteNumber.Equals("Y"))
                        //continue;

                    PrintDocument(document, curResource.Template, appPath, process);
                }


                else if (curResource.File != null)
                {

                    if (process.Printer != null)
                        curResource.Printer = curResource.Printer;

                    //Save the File printed for Document to show Later. //Solo si no existe.
                    try
                    {
                        Factory.DaoProcessEntityResource().Save(
                            new ProcessEntityResource
                            {
                                Entity = new ClassEntity { ClassEntityID = EntityID.Document },
                                EntityRowID = document.DocID,
                                CreatedBy = WmsSetupValues.SystemUser,
                                CreationDate = DateTime.Now,
                                File = curResource.File,
                                Process = process,
                                Status = new Status { StatusID = EntityStatus.Active }
                            });
                    }
                    catch {
 
                    }


                    //Thread th = new Thread(new ParameterizedThreadStart(PrintFileThread));
                    //th.Start(curResource);
                    /*
                    try { PrintFileThread(curResource); }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("PrintFileThread: " + curResource.File.ImageName,
                            ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                    */


                }

                Thread.Sleep(4000);

            }
        }


        private void PrintFileThread(Object resource)
        {

            PrintFileFactory printFactory;
            printFactory = PrintFileFactory.getFactory((ProcessEntityResource)resource);
            printFactory.PrintFile();

        }



        public void PrintDocument(Document document, LabelTemplate report, string appPath, CustomProcess process)
        {
            curTemplate = report;

            try
            {

                if (curTemplate.DefPrinter == null)
                    throw new Exception("No printer defined for report " + report.Name +".");

                //Save the File printed for Document to show Later. //Solo si no existe.
                try
                {
                    Factory.DaoProcessEntityResource().Save(
                        new ProcessEntityResource
                        {
                            Entity = new ClassEntity { ClassEntityID = EntityID.Document },
                            EntityRowID = document.DocID,
                            CreatedBy = WmsSetupValues.SystemUser,
                            CreationDate = DateTime.Now,
                            Process = process,
                            Status = new Status { StatusID = EntityStatus.Active },
                            Template = report
                        });
                }
                catch { }


                //Usa la default Printer del template
                usePrinter = new Printer { PrinterName = curTemplate.DefPrinter.Name, PrinterPath = curTemplate.DefPrinter.CnnString };              

                //Ejecutar la impresion global en un Hilo            
                //Thread th = new Thread(new ParameterizedThreadStart(PrintDocumentThread));
                //th.Start(document);
                PrintDocumentThread(document);




            }
            catch (Exception ex)
            {
                //throw new Exception("Report could not be printed: " + report.Name + ", " + ex.Message);
                ExceptionMngr.WriteEvent("Report could not be printed: " + report.Name + ", " + ex.Message,
                            ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
            }

        }


        private void PrintDocumentThread(Object document)
        {

            //Report File exists
            string reportPath = Path.Combine(AppPath, WmsSetupValues.RdlTemplateDir + "\\" + curTemplate.Header);

            if (!File.Exists(reportPath))
                throw new Exception("Report file does not exists.");


            try
            {
                //Rendering Report
                localReport = new LocalReport();
                localReport.EnableExternalImages = true;
                localReport.ExecuteReportInCurrentAppDomain(System.Reflection.Assembly.GetExecutingAssembly().Evidence);
                localReport.AddTrustedCodeModuleInCurrentAppDomain("Barcode, Version=1.0.5.40001, Culture=neutral, PublicKeyToken=6dc438ab78a525b3");
                localReport.AddTrustedCodeModuleInCurrentAppDomain("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                localReport.EnableExternalImages = true;
                localReport.ReportPath = reportPath;


                //Obteniendo la informacion del Reporte (DataSet)
                ReportHeaderFormat rptHdr = GetReportInformation((Document)document, curTemplate.Header);
                DataSet ds = GetReportDataset(rptHdr);

                localReport.DataSources.Add(new ReportDataSource("Header", ds.Tables["ReportHeaderFormat"]));
                localReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["ReportDetailFormat"]));
            }
            catch { return; }


            //Print Report
            //Proceso de Creacion de archivos 
            m_streams.Add(curTemplate, new List<Stream>());

            m_currentPageIndex.Add(curTemplate, 0);

            Export(localReport, curTemplate, "IMAGE");  //1 - Document, 2 -  Label


            m_currentPageIndex[curTemplate] = 0;

            //Ejecutar la impresion global en un Hilo            
            Thread th = new Thread(new ParameterizedThreadStart(Print));
            th.Start(curTemplate.DefPrinter.CnnString);

            //Print(curTemplate.DefPrinter.CnnString);
        }



        //Entrega el dataset de un ReportHeader
        public DataSet GetReportDataset(ReportHeaderFormat header)
        {
            //Add Header to DataSet
            DataSet dh = new DataSet("Header");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ReportHeaderFormat));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, header);
            StringReader reader = new StringReader(writer.ToString());
            dh.ReadXml(reader);
            //dh.Tables[0].TableName = "Header";
            //dh.Tables[2].TableName = "Details";


            ////Add Detail List to DataSet
            //DataSet dd = new DataSet("Details");
            //xmlSerializer = new XmlSerializer(header.ReportDetails.ToArray().GetType()); 
            //writer = new StringWriter();
            //xmlSerializer.Serialize(writer, header.ReportDetails.ToArray());
            //reader = new StringReader(writer.ToString());
            //dd.ReadXml(reader);
            //dd.Tables[0].TableName = "Details";

            //dd.Tables.Add(dh.Tables[0].Copy()); //Adicionamos el header a los details
            return dh;
        }

    }
}
