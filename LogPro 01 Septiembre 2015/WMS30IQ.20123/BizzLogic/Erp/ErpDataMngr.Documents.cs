using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Master;
using Entities.Trace;
using Entities.General;
using ErpConnect;
using System.Configuration;
using Integrator;
using Entities;

namespace BizzLogic.Logic
{

    public partial class ErpDataMngr : BasicMngr
    {


        #region ErpDocuments

        public Boolean GetErpAllReceivingDocuments(Company company)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllReceivingDocuments Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);


            Console.WriteLine("Running RecDoc");


            //ProcessDocuments(ErpFactory.Documents().GetAllReceivingDocuments()); //return true;
            ProcessDocuments(ErpFactory.Documents().GetReceivingDocumentsSince(DateTime.Today.AddDays(-1 * this.historicDays)), company);
            ProcessDocuments(ErpFactory.Documents().GetPurchaseReturnsSince(DateTime.Today.AddDays(-1 * this.historicDays)), company);



            return true;

        }


        public Boolean GetErpReceivingDocumentById(Company company, string code)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpReceivingDocumentById Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetReceivingDocumentById(code), company); return true;
        }


        public Boolean GetErpReceivingDocumentsLastXDays(Company company, int days)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpReceivingDocumentsLastXDays Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetReceivingDocumentsLastXDays(days), company); return true;
        }


        public Boolean GetErpReceivingDocumentsSince(Company company, DateTime sinceDate)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpReceivingDocumentsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetReceivingDocumentsSince(sinceDate), company);

            Console.WriteLine("GetPurchaseReturns");
            try { ProcessDocuments(ErpFactory.Documents().GetPurchaseReturnsSince(sinceDate), company); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return true;
        }


        public Boolean GetErpAllShippingDocuments(Company company)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllShippingDocuments Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);


            bool useRemain = GetCompanyOption(company, "USEREMQTY").Equals("T");

            //ProcessDocuments(ErpFactory.Documents().GetAllShippingDocuments()); 
            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(
                    DateTime.Today.AddDays(-1 * this.historicDays), 2, useRemain), company);

            //Si quiere mas documentos manda los otros tipos de documento 2,3,5
            //ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(
            //DateTime.Today.AddDays(-1 * WmsSetupValues.HistoricDays), 6));


            //Back Orders
            if (GetCompanyOption(company, "GETBO").Equals("T"))
                    ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(
                        DateTime.Today.AddDays(-1 * WmsSetupValues.HistoricDays), 5, false), company);

            //RETURNS
            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(
                    DateTime.Today.AddDays(-1 * this.historicDays), 4, false), company);


            return true;
        }


        public Boolean GetErpShippingDocumentById(Company company, string code)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpShippingDocumentById Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            bool useRemain = GetCompanyOption(company, "USEREMQTY").Equals("T");

            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentById(code, 2, useRemain), company); return true;
        }


        public Boolean GetErpShippingDocumentsLastXDays(Company company, int days)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpShippingDocumentsLastXDays Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            bool useRemain = GetCompanyOption(company, "USEREMQTY").Equals("T");

            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsLastXDays(days, 2, useRemain), company); return true;
        }


        public Boolean GetErpShippingDocumentsSince(Company company, DateTime sinceDate)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpShippingDocumentsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            bool useRemain = GetCompanyOption(company, "USEREMQTY").Equals("T");

            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(sinceDate, 2, useRemain), company);
            ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(sinceDate, 4, false), company);

            //Get BackOrders
            if (GetCompanyOption(company, "GETBO").Equals("T"))
            {
                Console.WriteLine("Processing BackOrders");
                ProcessDocuments(ErpFactory.Documents().GetShippingDocumentsSince(sinceDate, 5, false), company);
            }

            return true;
        }



        private void ProcessDocuments(IList<Document> list, Company company)
        {
            if (list == null)
                return;

            Document qDoc;
            DocumentLine curLine;
            Factory.Commit();
            Factory.IsTransactional = true;
            Status cancell = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
            string flag = "";

            //pregunta si sobre escribe las cantidades ya guardadas con las nuevas del ERP
            string overWriteQtys = "T";

            try { overWriteQtys = GetCompanyOption(company, "OVERWQTY"); }
            catch { overWriteQtys = "T"; }



            int i, y;
            foreach (Document e in list)
            {
                try
                {

                    flag = "Document";

                    qDoc = new Document
                    {
                        DocNumber = e.DocNumber,
                        //DocType = new DocumentType { DocTypeID = e.DocType.DocTypeID },
                        Company = new Company { CompanyID = e.Company.CompanyID }
                    };

                    //Evalua si el documento ya existe 
                    IList<Document> exList = Factory.DaoDocument().Select(qDoc);
                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;
                    Factory.Commit();

                    //Si No existe
                    if (exList.Count == 0)
                    {
                        e.CreationDate = DateTime.Now;
                        e.CreatedBy = string.IsNullOrEmpty(e.CreatedBy) ? WmsSetupValues.SystemUser : e.CreatedBy;
                        Factory.DaoDocument().Save(e);
                        Factory.Commit();
                    }
                    else
                    {

                        //Si el documento esta completado no puede ser actualizado por el DEL ERP
                        //13 Oct 2009
                        //if (exList.First().DocStatus.StatusID == DocStatus.Completed)
                        //continue;

                        //Si el last change del document e sdiferente de nulo y no es mayor al ultimo las change
                        if (exList.First().LastChange != null && exList.First().LastChange >= e.LastChange)
                            continue;

                        //Console.WriteLine("Document:" + e.DocNumber);

                        //Valores que no pueden cambiar asi se reciban de nuevo del ERP
                        e.DocID = exList.First().DocID;
                        e.CreationDate = exList.First().CreationDate;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.Priority = exList.First().Priority;
                        e.Notes = exList.First().Notes;
                        e.CrossDocking = exList.First().CrossDocking;

                        if (!string.IsNullOrEmpty(exList.First().Comment))
                            e.Comment = exList.First().Comment;

                        e.PickMethod = exList.First().PickMethod;
                        e.AllowPartial = exList.First().AllowPartial;
                        e.ModDate = DateTime.Now;
                        e.ModifiedBy = e.CreatedBy;

                        //Conserva el status si el actual es mayor al que viene del el ERP.
                        if (exList.First().DocStatus.StatusID > e.DocStatus.StatusID)
                            e.DocStatus = exList.First().DocStatus;


                        flag = "Address";

                        #region DocAddress
                        if (e.DocumentAddresses != null)
                        {
                            //Evaluar los document Address
                            i = 0;
                            DocumentAddress curAddr;
                            foreach (DocumentAddress addr in e.DocumentAddresses)
                            {
                                curAddr = new DocumentAddress();
                                curAddr.Document = new Document { DocID = e.DocID };
                                curAddr.Name = addr.Name;
                                curAddr.DocumentLine = new DocumentLine { LineID = -1 };
                                IList<DocumentAddress> listAddrs = Factory.DaoDocumentAddress().Select(curAddr);
                                Factory.Commit();

                                if (listAddrs.Count > 0)
                                {
                                    e.DocumentAddresses[i].ModDate = DateTime.Now;
                                    e.DocumentAddresses[i].ModifiedBy = WmsSetupValues.SystemUser;
                                    e.DocumentAddresses[i].RowID = listAddrs.First().RowID;
                                    e.DocumentAddresses[i].CreationDate = listAddrs.First().CreationDate;
                                    e.DocumentAddresses[i].CreatedBy = listAddrs.First().CreatedBy;
                                }
                                else
                                {
                                    e.DocumentAddresses[i].CreationDate = DateTime.Now;
                                    e.DocumentAddresses[i].CreatedBy = WmsSetupValues.SystemUser;
                                }

                                i++;
                            }
                        }

                        //Factory.DaoDocument().Update(e);

                        #endregion


                        flag = "Lines";
                        //Evaluar los document Lines
                        #region DocLines

                        if (e.DocumentLines != null)
                        {


                            IList<DocumentLine> currentLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = e.DocID } });

                            //Elimina la lineas que no sean de procesos originale del ERP
                            //Para recrealas en pasos posteriores
                            if (currentLines != null && currentLines.Count > 0)
                            {

                                //foreach (DocumentLine curxLine in currentLines.Where(f=>f.Note != "1" && f.Note != "2" && f.LinkDocLineNumber == 0 ))
                                foreach (DocumentLine curxLine in currentLines.Where(f => f.LinkDocLineNumber <= 0))
                                {
                                    //Borra las lineas que no existan ya y que no sean de tipo kit assembly.
                                    //if (!e.DocumentLines.Any(f => f.LineNumber == curxLine.LineNumber || ((f.Note == "1" || f.Note == "2") && f.LinkDocLineNumber > 0))) 

                                    //Console.WriteLine("\t" + curxLine.LineNumber);
                                    if (!e.DocumentLines.Any(f => f.LineNumber == curxLine.LineNumber))
                                    {
                                        //if (curxLine.Note != "1" && curxLine.Note != "2" && curxLine.LinkDocLineNumber == 0)
                                        Factory.DaoDocumentLine().Delete(curxLine);
                                        //Console.WriteLine("\tDeleted " + curxLine.LineNumber);
                                    }
                                    //curxLine.LineStatus = cancell;
                                    //Factory.DaoDocumentLine().Update(curxLine);
                                }

                                Factory.Commit();
                            }




                            i = 0;
                            IList<DocumentLine> linesToRemove = new List<DocumentLine>();

                            foreach (DocumentLine line in e.DocumentLines)
                            {
                                curLine = new DocumentLine { Document = new Document { DocID = e.DocID }, LineNumber = line.LineNumber };

                                IList<DocumentLine> listLines = Factory.DaoDocumentLine().Select(curLine);
                                Factory.Commit();

                                //Console.WriteLine(e.DocNumber + "," + e.DocID + "," + line.LineNumber + "," + listLines.Count.ToString());

                                if (listLines.Count > 0)
                                {

                                    //if (listLines.First().LineStatus.StatusID == DocStatus.InProcess || listLines.First().LineStatus.StatusID == DocStatus.Completed)
                                    if (listLines.First().LineStatus.StatusID != DocStatus.New)
                                    {
                                        linesToRemove.Add(e.DocumentLines[i]);
                                        i++;
                                        continue;
                                    }

                                    e.DocumentLines[i].ModDate = DateTime.Now;
                                    e.DocumentLines[i].ModifiedBy = WmsSetupValues.SystemUser;
                                    e.DocumentLines[i].LineID = listLines.First().LineID;
                                    e.DocumentLines[i].CreationDate = listLines.First().CreationDate;
                                    e.DocumentLines[i].CreatedBy = listLines.First().CreatedBy;
                                    e.DocumentLines[i].QtyShipped = listLines.First().QtyShipped;
                                    e.DocumentLines[i].LinkDocLineNumber = listLines.First().LinkDocLineNumber;
                                    e.DocumentLines[i].LinkDocNumber = listLines.First().LinkDocNumber;

                                    if (overWriteQtys.Equals("F"))
                                    {
                                        if (e.DocumentLines[i].QtyAllocated > 0 && listLines.First().QtyAllocated == 0)
                                            e.DocumentLines[i].QtyAllocated = listLines.First().QtyAllocated;

                                        if (e.DocumentLines[i].QtyBackOrder > 0 && listLines.First().QtyBackOrder == 0)
                                            e.DocumentLines[i].QtyBackOrder = listLines.First().QtyBackOrder;

                                        if (e.DocumentLines[i].QtyCancel > 0 && listLines.First().QtyCancel == 0)
                                            e.DocumentLines[i].QtyCancel = listLines.First().QtyCancel;
                                    }


                                    #region Document Line Address
                                    //Evaluar los document Line Address
                                    if (line.DocumentLineAddresses != null)
                                    {
                                        y = 0;
                                        DocumentAddress curLineAddr;
                                        foreach (DocumentAddress lineAddr in line.DocumentLineAddresses)
                                        {
                                            curLineAddr = new DocumentAddress();
                                            curLineAddr.Document = new Document { DocID = line.Document.DocID };
                                            curLineAddr.DocumentLine = line;
                                            curLineAddr.Name = lineAddr.Name;
                                            IList<DocumentAddress> listLineAddrs = Factory.DaoDocumentAddress().Select(curLineAddr);
                                            Factory.Commit();

                                            if (listLineAddrs.Count > 0)
                                            {
                                                line.DocumentLineAddresses[y].ModDate = DateTime.Now;
                                                line.DocumentLineAddresses[y].ModifiedBy = WmsSetupValues.SystemUser;
                                                line.DocumentLineAddresses[y].RowID = listLineAddrs.First().RowID;
                                                line.DocumentLineAddresses[y].CreationDate = listLineAddrs.First().CreationDate;
                                                line.DocumentLineAddresses[y].CreatedBy = listLineAddrs.First().CreatedBy;
                                            }
                                            else
                                            {
                                                line.DocumentLineAddresses[y].CreationDate = DateTime.Now;
                                                line.DocumentLineAddresses[y].CreatedBy = WmsSetupValues.SystemUser;
                                            }

                                            y++;
                                        }
                                    }
                                    #endregion

                                }
                                else
                                {
                                    e.DocumentLines[i].CreationDate = DateTime.Now;
                                    e.DocumentLines[i].CreatedBy = WmsSetupValues.SystemUser;
                                }

                                i++;
                            }

                            //Remueve las lineas que no van a ser procesadas.
                            foreach(DocumentLine lr in linesToRemove)                                                            
                                e.DocumentLines.Remove(lr);

                        }
                        #endregion

                        flag = "Update Document";


                        Factory.DaoDocument().Update(e);
                        Factory.Commit();
                    }

                    flag = "Explode Kit";
                    //Incluido Mayo 14 de 2009 Evalua si el documento de Venta tiene lineas de assembly y debe mostrar 
                    //Los componentes
                    //e.DocType.DocClass.DocClassID == SDocClass.Shipping - Removido ON Sep 17/09
                    //Console.WriteLine("\tDocument Before Explode:" + e.DocNumber);
                    if (e.DocType.DocTypeID == SDocType.SalesOrder && GetCompanyOption(e.Company, "SHOWCOMP").Equals("T"))
                    {
                        //Console.WriteLine("\tDocument Explode:" + e.DocNumber);
                        ExplodeKitAssemblyComponents(e, true);
                    }


                    //Incluido Mayo 26 de 2009 Evalua si el documento de Return tiene lineas de assembly y debe mostrar 
                    //Los componentes, pero no recibirlos, recibe el asembli, por eso el parametro en false.
                    if (e.DocType.DocTypeID == SDocType.Return && GetCompanyOption(e.Company, "RETURNCOMP").Equals("T"))
                        ExplodeKitAssemblyComponents(e, false);


                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    if (e.DocType.DocTypeID != SDocType.KitAssemblyTask) //&& !ex.Message.Contains("Problem updating the record.")
                        ExceptionMngr.WriteEvent("ProcessDocuments:" + flag + ":" + e.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }

            }
        }


        /// <summary>
        /// Crea nuevas lineas en el documento de venta con los componentes de los Kit/Assemblies de la orden
        /// </summary>
        /// <param name="document"></param>
        /// <param name="forSale">Indica si el documento va for Sale (false) o for Return (false)
        /// For Sale hace que se desplieguen los componentes y se oculta el Kit, for Return se oucltan los componentes
        /// y se muestra el kit</param>
        private void ExplodeKitAssemblyComponents(Document document, bool forSale)
        {

            //if (document.DocStatus.StatusID == DocStatus.InProcess || document.DocStatus.StatusID == DocStatus.Completed)
            //return;



            DocumentLine curLine;
            string keyKit = "", keyComponent = "";

            if (forSale) { keyComponent = "1"; keyKit = "2"; }
            else { keyComponent = "2"; keyKit = "1"; }


            //1. Borrar las lineas tipo componente de ese documento
            // y Confirma la eliminacion de los componentes para recrearlos
            ///Donde Note="1" y LinkDocLineNumber  > 0
            IList<DocumentLine> delLines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = document.DocID } });

            foreach (DocumentLine delL in delLines.Where(f => f.Note == keyComponent && f.LinkDocLineNumber > 0))
                Factory.DaoDocumentLine().Delete(delL);

            Factory.Commit();


            Factory.IsTransactional = true;


            //2.Obtener la lineas activas donde el producto tenga formulacion        
            //Solo se obtiene los componente del producto donde la cataegoria existe y tenga explodeKit == true - MAXIFORCE.
            IList<DocumentLine> lines = Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = document.DocID } });
            try
            {
                lines = lines.Where(f => f.Product.Kit != null && f.Product.Kit.Count > 0 &&
                    f.Product.Kit[0].ProductFormula != null && f.Product.Kit[0].ProductFormula.Count > 0 && f.LinkDocLineNumber <= 0)
                    .Where(f => f.Product.Category != null && f.Product.Category.ExplodeKit > 0).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kits: " + ex.Message);
                return;
            }

            //Console.WriteLine("\t" + document.DocNumber + ":" + lines.Count.ToString());


            if (lines.Count == 0)
            {
                Console.WriteLine(document.DocNumber + ". 0 Kits");
                return;
            }


            //3.Para cada linea:
            //3.1 Obtener la formula y crear una nueva linea en el documento
            int i = 1;
            int seqLine;
            double QtyBalance = 0;

            foreach (DocumentLine docLine in lines)
            {

                //Console.WriteLine("\t\t" + docLine.Product.ProductCode);

                //Si la opcion de Explode es la #2 (Explode si el componente no tiene el allocated complete)
                //Se verifica que el Allocated sea menor al Ordered - Cancelled para 
                //poder hacer Explode del Kit.

                if (docLine.Product.Category.ExplodeKit == ExplodeKit.IfNotStock || docLine.Product.Category.ExplodeKit == ExplodeKit.Caterpillar)
                {
                    QtyBalance = docLine.Quantity - docLine.QtyCancel - docLine.QtyBackOrder;

                    if (docLine.QtyAllocated >= QtyBalance && QtyBalance > 0)
                        continue;

                    if (forSale) //Solo aplica para los Kit que salen
                        docLine.QtyCancel += QtyBalance;
                }


                seqLine = 0;

                docLine.Sequence = (i * 1000) + seqLine; //Actualiza antes de que cambie el seq
                seqLine++;

                if (docLine.Product.Kit != null && docLine.Product.Kit.Count > 0 &&
                docLine.Product.Kit[0].ProductFormula != null && docLine.Product.Kit[0].ProductFormula.Count > 0)
                {

                    foreach (KitAssemblyFormula formula in docLine.Product.Kit[0].ProductFormula)
                    {
                        curLine = new DocumentLine
                        {
                            Date1 = docLine.Date1,

                            LineNumber = docLine.Sequence + seqLine,
                            Sequence = docLine.Sequence + seqLine,
                            LinkDocLineNumber = docLine.LineNumber,
                            Note = keyComponent, // Se debe recoger, el padre debe ponerse de tipo 2 y LinDocNUmber = -1
                            LineStatus = docLine.LineStatus,
                            Document = document,
                            IsDebit = false,

                            CreatedBy = WmsSetupValues.SystemUser,
                            CreationDate = DateTime.Now,

                            Location = docLine.Location,
                            Unit = formula.Unit,
                            //AccountItem = (docLine.Product.Category.ExplodeKit == null) ? null : docLine.Product.Category.ExplodeKit.ToString() 

                        };

                        //Marca para ver si hace el ajuste o no.
                        if (formula.KitAssembly.Product.Category.ExplodeKit == ExplodeKit.Caterpillar)
                            curLine.AccountItem = ExplodeKit.Caterpillar.ToString();


                        //Junio 16 de 2010 /Jairo Murillo
                        //En lugar de el producto Original su reeemplazo
                        if (formula.KitAssembly.Product.Category.ExplodeKit == ExplodeKit.CaterpillarKit)
                        {
                            if (formula.Component.Category.ExplodeKit == ExplodeKit.Caterpillar)
                                curLine.AccountItem = ExplodeKit.Caterpillar.ToString();

                            curLine.Product = GetPartReplacement(formula.Component, docLine.Quantity * formula.FormulaQty, document.Location);

                            if (curLine.Product.ProductID != formula.Component.ProductID)
                                curLine.LineDescription = "Part replaced instead " + formula.Component.ProductCode;
                        }
                        else
                            curLine.Product = formula.Component;


                        //Solo se setean las cantidades si es para venta
                        if (forSale)
                        {
                            curLine.Quantity = docLine.Quantity * formula.FormulaQty;
                            curLine.QtyCancel = docLine.QtyCancel * formula.FormulaQty;
                            curLine.QtyInvoiced = docLine.QtyInvoiced * formula.FormulaQty;
                            curLine.QtyPending = docLine.QtyPending * formula.FormulaQty;

                            if (docLine.QtyBackOrder > 0)
                                curLine.QtyBackOrder = (docLine.Quantity - docLine.QtyBackOrder) * formula.FormulaQty;
                        }
                        else //En return se recibe el Kit (No los componentes)
                        {
                            //Esto hace que la cantidad se muestre, pero que se anule el balance
                            curLine.Quantity = docLine.Quantity * formula.FormulaQty;
                            //curLine.QtyCancel = docLine.QtyCancel * formula.FormulaQty;
                            curLine.QtyPending = docLine.QtyPending * formula.FormulaQty;
                            curLine.QtyCancel = curLine.Quantity;
                        }

                        //Salvar la Linea
                        //Console.WriteLine("\t Saving: " + curLine.Product.ProductCode);
                        Factory.DaoDocumentLine().Save(curLine);
                        seqLine++;
                    }
                }

                //Update Line
                docLine.Note = keyKit;
                docLine.LinkDocLineNumber = -1;

                //Si no es para venta es deicr un Return debe poner QtyCancel = QtyOrder para neterar el balance.
                //Comentariado en Nov/01/2009
                //if (!forSale)
                //docLine.QtyCancel = docLine.Quantity;


                Factory.DaoDocumentLine().Update(docLine);

                i++;

            }


            Factory.Commit();

        }

        private Product GetPartReplacement(Product part, double quantity, Location location)
        {
            //Si hay stock retorna el mismo Component
            try
            {
                /* ProductStock ps = Factory.DaoLabel().GetStock(
                    new ProductStock { Product = part, Bin = new Bin {Location = location } }).First();
                
                if (ps.Stock + ps.PackStock > quantity)
                    return part; */

                //Revisa si es un componente de caterpillar y envia su componente reemplazo,
                return part.Kit.First().ProductFormula[0].Component;

            }
            catch { }
            return part;
        }



        #endregion




        public Boolean GetErpAllKitAssemblyDocuments(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllKitAssemblyDocuments Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetKitAssemblyDocuments(), company);


            return true;
        }


        public Boolean GetErpKitAssemblyDocumentsSince(Company company, DateTime sinceDate)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllKitAssemblyDocumentsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetKitAssemblyDocumentsSince(sinceDate), company); return true;
        }





        public void GetErpAllLocationTransferDocuments(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllLocationTransferDocuments Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return;
            }

            SetConnectMngr(company);

            //ProcessDocuments(ErpFactory.Documents().GetAllReceivingDocuments()); //return true;
            ProcessDocuments(ErpFactory.Documents().GetLocationTransferDocumentsSince(DateTime.Today.AddDays(-1 * this.historicDays)), company);
            return;
        }


        public void GetErpLocationTransferDocumentsSince(Company company, DateTime sinceDate)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpLocationTransferDocumentsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return;
            }


            SetConnectMngr(company);

            ProcessDocuments(ErpFactory.Documents().GetLocationTransferDocumentsSince(sinceDate), company); return;
        }




        public IList<ProductStock> GetStockComparation(ProductStock data, Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetStockComparation Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return null;
            }
            SetConnectMngr(company);


            bool detailed = false;
            //Obtiene el Stock del ERP para lo mismo loaction - bin
            IList<ProductStock> erpStock = ErpFactory.Documents().GetErpStock(data, detailed);

            if (erpStock == null || erpStock.Count == 0)
                throw new Exception("No ERP Stock to compare.");


            IList<ProductStock> wmsAvailStock = Factory.DaoLabel().GetNodeStock(data,
            new Node { NodeID = NodeType.Stored },
            new Status { StatusID = EntityStatus.Active });

            IList<ProductStock> wmsPickedStock = Factory.DaoLabel().GetNodeStock(data,
            new Node { NodeID = NodeType.Picked },
            new Status { StatusID = EntityStatus.Locked });

            if (wmsAvailStock == null || wmsAvailStock.Count == 0)
                throw new Exception("No WMS Stock to compare.");

            //Adicionando el Picking
            wmsAvailStock = (from avail in wmsAvailStock
                             join pick in wmsPickedStock on avail.Product.ProductID equals pick.Product.ProductID
                             into gj
                             from sub in gj.DefaultIfEmpty()
                             select new ProductStock
                             {
                                 Product = avail.Product,
                                 Stock = avail.Stock,
                                 PackStock = (sub == null) ? 0 : sub.Stock,
                                 Bin = new Bin { Location = data.Bin.Location },
                             }).ToList();


            //Comparando Contra el ERP entragando un solo resultado
            wmsAvailStock = (from erp in erpStock
                             join avail in wmsAvailStock on erp.Product.ProductID equals avail.Product.ProductID
                             into gj
                             from sub in gj.DefaultIfEmpty()
                             select new ProductStock
                             {
                                 Product = erp.Product,
                                 Stock = erp.Stock,  //ERP On Hand
                                 PackStock = (sub == null) ? 0 : sub.Stock,  //WMS Available
                                 MaxStock = (sub == null) ? 0 : sub.PackStock,  //WMS Picking
                                 AuxQty1 = (sub == null) ? 0 : sub.Stock + sub.PackStock,
                                 MinStock = (sub == null) ? -1 * erp.Stock : sub.Stock + sub.PackStock - erp.Stock, //Diference
                                 Bin = new Bin { Location = data.Bin.Location },
                             }).Where(f => f.MinStock != 0).ToList();


            return wmsAvailStock;

        }



        //Obtiene la info del stock del ERP y actualiza el inventario
        //Detallado o global dejando todo en MAIN
        public void UpdateWMSStockFromERP(Company company, bool detailed)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetStockComparation Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return;
            }
            SetConnectMngr(company);


            //bool detailed = true; //indica si obtiene el stock a nivel de BIN
            ProductStock data;
            Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });
            IList<ProductStock> erpStock;


            IList<Location> locationList = Factory.DaoLocation().Select(new Location { Company = company });

            Console.WriteLine("Locations: " + locationList.Count.ToString());

            foreach (Location location in locationList)
            {

                if (location.LocationID == 107 || location.LocationID == 103)
                    continue;

                try
                {
                    data = new ProductStock { Bin = new Bin { Location = location } };
                    erpStock = ErpFactory.Documents().GetErpStock(data, detailed);

                    if (erpStock == null || erpStock.Count == 0)
                        continue;


                    Console.WriteLine("Location: " + location.Name + " => " + erpStock.Count + " records");
                    //Console.ReadKey();

                    foreach (ProductStock ps in erpStock.Where(f => f.Stock > 0))
                        try
                        {
                            UpdateStock(ps, storedNode, "Stock Update From ERP");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("UpdateStock: " + ex.Message);
                        }

                }
                catch (Exception ex)
                { Console.WriteLine("GetErpStock: " + ex.Message); }
            }

        }


        private void UpdateStock(ProductStock ps, Node storedNode, string comment)
        {


            if (!string.IsNullOrEmpty(ps.Bin.BinCode))
            {
                try { ps.Bin = Factory.DaoBin().Select(ps.Bin).First(); }
                catch (Exception ex)
                { //El bin no Existe
                    //Console.WriteLine(ps.Product.ProductCode + " => " + ps.Bin.BinCode);
                    Console.WriteLine(ex.Message);
                    //Console.ReadKey();
                    Status active = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();
                    ps.Bin.CreatedBy = WmsSetupValues.SystemUser;
                    ps.Bin.CreationDate = DateTime.Now;
                    ps.Bin.IsFromErp = true;
                    ps.Bin.Status = active;
                    ps.Bin.Rank = 0;
                    Console.WriteLine("Creating Bin: " + ps.Bin.BinCode);
                    ps.Bin = Factory.DaoBin().Save(ps.Bin);
                }
            }
            else
                ps.Bin = WType.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = ps.Bin.Location });

            //Procesando el Stock.
            DocumentLine line = new DocumentLine
            {
                Product = ps.Product,
                Quantity = ps.Stock,
                Unit = ps.Product.BaseUnit,
                CreatedBy = WmsSetupValues.SystemUser
            };

            (new TransactionMngr()).IncreaseQtyIntoBin(line, storedNode, ps.Bin, comment, true, DateTime.Now, null, null);


        }

        public String CreateSalesOrder(Document document, string docPrefix, string batch)
        {
            SetConnectMngr(document.Company);

            return ErpFactory.Documents().CreateSalesOrder(document, docPrefix, batch);
        }

        public string CreateCustomer(Account customer)
        {
            SetConnectMngr(customer.Company);

            return ErpFactory.Documents().CreateCustomer(customer);
        }

        public string CreateCustomerAddress(AccountAddress address)
        {
            SetConnectMngr(address.Account.Company);

            return ErpFactory.Documents().CreateCustomerAddress(address);
        }

        public string CreatePurchaseOrder(Document document)
        {
            SetConnectMngr(document.Company);

            return ErpFactory.Documents().CreatePurchaseOrder(document);
        }


        internal void UpdateSalesDocumentBatch(Company company, string docNumber, string batchNo)
        {

            {
                //Revisa si se debe enviar el recibo al ERP, es decir si esta en true la opcion de conexion a ERP
                bool ErpConnected = GetCompanyOption(company, "WITHERPSH").Equals("T");

                //Valida que la conexion al ERP exista
                if (ErpConnected)
                {
                    if (company.ErpConnection == null)
                        throw new Exception("Please setup Erp Connection.");

                    SetConnectMngr(company);

                    ErpFactory.Documents().UpdateSalesDocumentBatch(docNumber, batchNo);

                }
            }
        }



        internal int SaveUpdateErpDocumentLine(DocumentLine docLine, bool removeLine)
        {
            //Si el sistema esta conectado a un ERP Crea actualiza la linea en el ERP
            bool ErpConnected = GetCompanyOption(docLine.Document.Company, "WITHERP").Equals("T");

            //Valida que la conexion al ERP exista
            if (ErpConnected)
            {
                if (docLine.Document.Company.ErpConnection == null)
                    throw new Exception("Please setup Erp Connection.");

                SetConnectMngr(docLine.Document.Company);
            }

            if (ErpConnected)
                return ErpFactory.Documents().SaveUpdateErpDocumentLine(docLine, removeLine);

            return 0;

        }

    }
}