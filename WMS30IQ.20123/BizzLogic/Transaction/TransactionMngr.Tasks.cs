using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Profile;
using Entities.General;
using Entities.Master;
using Integrator.Dao;
//using System.Transactions;
using Integrator;
//using System.ServiceModel;
using Entities;
using System.Data;
using Entities.Process;


namespace BizzLogic.Logic
{

    public partial class TransactionMngr 
    {

        public void DepurationTasks(Company company)
        {
            Console.WriteLine("Expired Replenish ...");
            DeleteExpiredReplenish(company);

            Console.WriteLine("Custom Process I ...");
            UpdateReceivedOnNode();

            //DeleteExpiredProductInventory();

            Console.WriteLine("Count Schedule ...");
            CreateScheduledCount();

            
            Console.WriteLine("Custom Process II ...");
            SpecificCompanyProcess(company);
            


            //try
            //{
            //    //Eliminacion de Labels que tinene printed = 0 y CurQty = 0, labels en blanco.
            //    Factory.DaoLabel().DeleteEmptyLabels();
            //}
            //catch { }




            /*
            //Obtiene los Labels con current Qty = 0 y en node 4 y los pone inactivos. Tipo 1002
            try {

                Factory.IsTransactional = true;

                IList<Label> labelList = Factory.DaoLabel().Select(
                    new Label
                    {
                        Node = new Node { NodeID = NodeType.Stored },
                        Status = new Status { StatusID = EntityStatus.Active},
                        LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                        CurrQty = -1
                    }).Where(f => f.StockQty <= 0).ToList();

                Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

                //Actualizando los lables encontrados
                foreach (Label lbl in labelList)
                {
                    lbl.Status = inactive;
                    Factory.DaoLabel().Update(lbl);

                    SaveNodeTrace(new NodeTrace{
                        Node = lbl.Node,
                        Label = lbl,
                        Quantity = lbl.CurrQty,
                        IsDebit = false,
                        CreatedBy = WmsSetupValues.SystemUser,
                        Comment = "Inactivated by zero quantity."
                    });
                }

                Factory.Commit();
            }
            catch { Factory.Rollback();  }
            */

            /*
            //Obtiene los Labels con current Qty = 0 y en node 4 y los pone inactivos. Tipo 1005
            try
            {
                Factory.IsTransactional = true;

                IList<Label> labelList = Factory.DaoLabel().Select(
                    new Label
                    {
                        Node = new Node { NodeID = NodeType.Stored },
                        Status = new Status { StatusID = EntityStatus.Active },
                        LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
                        CurrQty = -1
                    });

                Status inactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

                //Actualizando los lables encontrados
                foreach (Label lbl in labelList)
                {
                    lbl.Status = inactive;
                    Factory.DaoLabel().Update(lbl);

                    SaveNodeTrace(new NodeTrace
                    {
                        Node = lbl.Node,
                        Label = lbl,
                        Quantity = lbl.CurrQty,
                        IsDebit = false,
                        CreatedBy = WmsSetupValues.SystemUser,
                        Comment = "Inactivated by zero quantity."
                    });
                }

                Factory.Commit();
            }
            catch { Factory.Rollback(); }
            */

        }

        private void SpecificCompanyProcess(Company company)
        {
            string sQuery = "";
            //procesos Especificos a ejecutar para algunas companias ej Manzo que se deben traer
            //informacion para popular Servicios.

            //Se debe disenar mas elegante, sacando los queries de base de datos
            //Con una frecuenca de ejecucion y ultima ejecucion, next execution
            //Como un Schedule.

            string routineCommand = "";
            try
            {
                routineCommand = Factory.DaoConfigOption().Select(new ConfigOption { Code = "ROUTINES" })
                    .First().DefValue;

                try
                {
                    if (!string.IsNullOrEmpty(routineCommand))
                        Factory.DaoObject().PerformSpecificQuery(routineCommand);
                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("SpecificCompanyProcess: ", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.Business);
                }

            }
            catch { }

            /*
            if (company.Name.Contains("MANZO"))
            {
                try
                {
                    /*sQuery = "UPDATE p SET p.UnitsPerPack = gag.CASESPP " +
                        "FROM Master.Product p INNER JOIN " +
                        "MAN01.dbo.GAGItemMainOP gag ON p.ProductCode = gag.ITEMNUMBER";


                    sQuery = "EXEC [dbo].[spWMSRoutines] 4";
                    Factory.DaoObject().PerformSpecificQuery(sQuery);
                }
                catch (Exception ex) {
                    ExceptionMngr.WriteEvent("SpecificCompanyProcess: ", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.Business);
                }
            }
            */

        }


        private void DeleteExpiredProductInventory()
        {
            //Borrando todas las lineas que no fueron allocated.
            IList<ProductInventory> list = Factory.DaoProductInventory().Select(new ProductInventory());
            foreach (ProductInventory reg in list.Where(f => f.QtyAllocated <= 0 && f.ModDate < DateTime.Now.AddMinutes(-30)))
                Factory.DaoProductInventory().Delete(reg);
        }



        private void UpdateReceivedOnNode()
        {
            //Obtiene los Nodetrace que sean de nodo 4 y que tengan labels en nodo 2, y con documento de posteo existente
            try
            {
                IList<Label> labelList = Factory.DaoNodeTrace().Select(
                    new NodeTrace
                    {
                        Label = new Label { Node = new Node { NodeID = NodeType.Received } },
                        PostingDocument = new Document { DocID = 1 },
                        Node = new Node { NodeID = NodeType.Stored }
                    }).Select(f => f.Label).ToList();

                Node stored = WType.GetNode(new Node { NodeID = NodeType.Stored });

                //Actualizando los lables encontrados
                foreach (Label lbl in labelList)
                {
                    lbl.Node = stored;
                    lbl.Notes += " (2)";
                    Factory.DaoLabel().Update(lbl);
                }
            }
            catch { }

        }



        private void DeleteExpiredReplenish(Company company)
        {

            int expHours;

            try { expHours = int.Parse(GetCompanyOption(company, "RPOEXP")); }
            catch { expHours = WmsSetupValues.ReplenishmentExpHours; }

            IList<Document> list = Factory.DaoDocument().Select(new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.ReplenishPackTask },
                DocStatus = new Status { StatusID = DocStatus.New }
            }).Where(f => f.CreationDate < DateTime.Now.AddHours(-1 * expHours)).ToList();


            Status status = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });

            foreach (Document doc in list)
            {
                doc.DocStatus = status;
                Factory.DaoDocument().Update(doc);
            }

        }



        //Mueve cantidades de un package a otro
        public void MoveQtyBetweenPackages(DocumentPackage curPack, DocumentPackage newPack, 
            Product product, double qty)
        {

            Factory.IsTransactional = true;

            Unit baseUnit = product.BaseUnit;

            try
            {

                DocumentLine line = new DocumentLine
                {
                    Quantity = qty,
                    Product = product,
                    Unit = baseUnit,
                    CreatedBy = newPack.CreatedBy
                };

                #region remove from OLD package
                //#########################################################################
                //Remover la cantidad del paquete origen
                //Saca las cantidades para es BIN y de ese producto.
                IList<Label> labelList = GetPackageLabels(curPack.PackLabel, line);

                Label sourceLabel = null;

                if (labelList.Sum(f => f.BaseCurrQty) < line.Quantity * line.Unit.BaseAmount)
                {
                    Factory.Rollback();
                    throw new Exception("No quantity available for the transaction.");
                }


                //Recorre los labels hasta que termine el saldo y se salga.
                double qtyBalance = line.Quantity * line.Unit.BaseAmount;
                double curQty;
                foreach (Label label in labelList)
                {

                    if (qtyBalance <= 0)
                        break;

                    label.CurrQty = label.BaseCurrQty;
                    label.StartQty = label.BaseStartQty;
                    label.Unit = baseUnit;

                    //Cantidad a Disminuir
                    curQty = qtyBalance > label.CurrQty ? label.CurrQty : qtyBalance;

                    qtyBalance -= curQty;
                   
                    label.CurrQty -= curQty;
                    label.ModDate = DateTime.Now;
                    label.ModifiedBy = line.CreatedBy;
                    Factory.DaoLabel().Update(label);
                    sourceLabel = label;

                }

                #endregion

                #region add to NEW package
                //#########################################################################
                //Adicionar la cantidad al paquete destino
                Label tmpLabel = null;
                Status statusLock = WType.GetStatus(new Status { StatusID = EntityStatus.Locked }); //Active

                DocumentType lblType = new DocumentType { DocTypeID = LabelType.ProductLabel };
                //DocumentTypeSequence initSequence = DocMngr.GetNextDocSequence(curPack.Document.Company, lblType); //Funcion para obtener siguiente Label


                //Salvar con el nuevo status
                tmpLabel = new Label();

                //To Send
                Node node = WType.GetNode(new Node { NodeID = NodeType.Released });
                tmpLabel.Node = curPack.PackLabel.Node;

                tmpLabel.Bin = curPack.PackLabel.Bin;
                tmpLabel.CurrQty = line.Quantity;
                tmpLabel.Product = line.Product;
                tmpLabel.StartQty = line.Quantity;
                tmpLabel.Unit = line.Product.BaseUnit;
                tmpLabel.CreatedBy = line.CreatedBy;

                tmpLabel.Status = statusLock;
                tmpLabel.LabelType = lblType;
                tmpLabel.LabelCode = ""; // initSequence.NumSequence.ToString() + GetRandomHex(line.CreatedBy, initSequence.NumSequence);

                tmpLabel.Printed = false;
                tmpLabel.CreationDate = DateTime.Now;
                tmpLabel.IsLogistic = false;
                tmpLabel.ShippingDocument = curPack.Document;
                tmpLabel.LabelSource = sourceLabel;

                tmpLabel.FatherLabel = newPack.PackLabel;
                tmpLabel = Factory.DaoLabel().Save(tmpLabel);


                #endregion

                Factory.Commit();

            }
            catch  {
                Factory.Rollback();
                throw;
            }
        }



        //Obtienene el listado de labels d eun BIN especifico.
        private IList<Label> GetPackageLabels(Label labelPattern, DocumentLine searchLine)
        {
            //Cuenta el producto necesario a mover al nuevo Bin, si no alcanza tira exception
            Label searchLabel = new Label();

            searchLabel.LabelType = WType.GetLabelType(new DocumentType { DocTypeID = LabelType.ProductLabel });
            searchLabel.Product = searchLine.Product;
            searchLabel.FatherLabel = labelPattern;

            //searchLabel.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            //searchLabel.Node = labelPattern.Node;
            //searchLabel.Bin = labelPattern.Bin;

            //Llamar los labels a afectar
            return Factory.DaoLabel().Select(searchLabel).OrderBy(f => f.ReceivingDate).ToList<Label>();

        }



        public void UpdatePackageMovedLabels(IList<Label> movedLabels)
        {
            foreach (Label lbl in movedLabels)
                Factory.DaoLabel().Update(lbl);
        }


        public void SendProcessNotification(SysUser user, Document document, string process)
        {
            string message = process + " : " + document.DocType.Name;
            message += "\nDocument Number: " + document.DocNumber;
            message += "\nPicker: " + user.UserName;
            message += "\nDate: " + DateTime.Now.ToString();


            //Si es Pickig update SalesPersonName of the SO.
            /*
            try
            {
                document.UserDef1 = user.UserName;
                Factory.DaoDocument().Update(document);
            }
            catch { }
            */

            //1. Obtiene else proceso y sus steps;
            CustomProcess currentProcess;
            try
            {
                currentProcess = Factory.DaoCustomProcess().Select(new CustomProcess { Name = process }).First();
            }
            catch { return; }

            (new ProcessMngr()).EvaluateBasicProcess(currentProcess, user, message);


            //If process has BATCH update the SO Batch,
            if (!string.IsNullOrEmpty(currentProcess.BatchNo))
            {
                if (document.DocType.DocTypeID == SDocType.MergedSalesOrder)
                {
                    foreach (DocumentLine line in Factory.DaoDocumentLine().Select(new DocumentLine { Document = new Document { DocID = document.DocID } }))
                    {
                        (new ErpDataMngr()).UpdateSalesDocumentBatch(document.Company, line.LinkDocNumber, currentProcess.BatchNo);
                    }
                }
                else
                    (new ErpDataMngr()).UpdateSalesDocumentBatch(document.Company, document.DocNumber, currentProcess.BatchNo);
            }



            
        }



        #region Inventory Count



        public void ResetCountedBinTask(BinByTask binTask)
        {
            //Saca el listado delso completado, para cancelarlos.
            IList<BinByTaskExecution> list = Factory.DaoBinByTaskExecution()
                .Select(new BinByTaskExecution { BinTask = binTask })
                .Where(f=>f.Status.StatusID == DocStatus.Completed || f.Status.StatusID == DocStatus.New)
                .ToList(); //, Status = new Status { StatusID = DocStatus.Completed }

            if (list == null || list.Count == 0)
                return;

            Status cancelled = WType.GetStatus(new Status{StatusID  = DocStatus.Cancelled });

            foreach (BinByTaskExecution bte in list)
            {
                bte.Status = cancelled;
                bte.ModDate = DateTime.Now;
                bte.ModifiedBy = binTask.ModifiedBy;
                Factory.DaoBinByTaskExecution().Update(bte);
            }
        }



        public Document ConfirmCountingTaskDocument(Document countTask, IList<CountTaskBalance> taskList, string user)
        {

            Bin NoCountBin = null;
            try { NoCountBin = Factory.DaoBin().Select(new Bin { BinCode = DefaultBin.NOCOUNT, Location = countTask.Location }).First(); }
            catch
            {
                NoCountBin = Factory.DaoBin().Save(new Bin
                {
                    Location = countTask.Location,
                    BinCode = DefaultBin.NOCOUNT,
                    Status = new Status { StatusID = EntityStatus.Active },
                    Rank = 0,
                    CreatedBy = WmsSetupValues.SystemUser,
                    CreationDate = DateTime.Now,
                    IsArea = true,
                    IsFromErp = false,
                    LevelCode = ""
                });
            }

            /* Recibe el balance de conteo y ejecuta los ajuste necesarios 

             LABELS 
             1. Envia los labels Printed que NO fueron reportados a NOCOUNT (label != null && conuted = 0)
             2. Los Labels encontrados son ajustados si hay diferencia  (label != null && Difference != 0)
                2a. A los labels encontrados se les ajusta el BIN y el status
              
             */

            Status posted = Factory.DaoStatus().Select(new Status { StatusID = DocStatus.Posted }).First();
            Status active = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();
            Status inActive = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Inactive }).First();


            // CAA [2010/07/13]  Se activan los bines usados
            if (countTask.Notes == "0")
            {
                IList<BinByTask> binList = Factory.DaoBinByTask().Select(new BinByTask { TaskDocument = countTask });
                foreach (BinByTask bin in binList)
                {
                    try
                    {
                        bin.Bin.Status = active;
                        bin.Bin.Comment = "";
                        Factory.DaoBin().Update(bin.Bin);
                    }
                    catch { }
                }
            }
            else if (countTask.Notes == "1")
            {
                IList<BinByTaskExecution> binListE = Factory.DaoBinByTaskExecution().Select(
                    new BinByTaskExecution { BinTask = new BinByTask { TaskDocument = countTask } });

                foreach (BinByTaskExecution bin in binListE)
                {
                    try
                    {
                        bin.Bin.Status = active;
                        bin.Bin.Comment = "";
                        Factory.DaoBin().Update(bin.Bin);
                    }
                    catch { }
                }
            }


            IList<Document> negativeDocs = new List<Document>();
            DocumentLine addLine;
            Document postiveAdj = null;
            Document negativeAdj = null;
            int positiveLine = 1;

            //Ajustes Positivos en un solo documento
            if (taskList.Any(f => f.Difference > 0))
            {
                //Crear Ajustes de inventario positivos en un solo ajuste.
                postiveAdj = new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                    CreatedBy = user,
                    Location = countTask.Location,
                    Company = countTask.Company,
                    IsFromErp = false,
                    CrossDocking = false,
                    Comment = "CountTask Posting (Postitive Adj) " + countTask.DocNumber + ", " + user,
                    Date1 = DateTime.Now,
                    CustPONumber = countTask.DocNumber,
                    Notes = WmsSetupValues.Counting_Bach
                };

                postiveAdj = DocMngr.CreateNewDocument(postiveAdj, false);
            }
            

            //Casetype
            //3. Label Contado (esperado y no Esperado)
            //4. Label Esperado no Contado Se meuve a No Count


            List<CountTaskBalance> labelsTotal, productTotal;

            try
            {

                try { labelsTotal = taskList.Where(f => f.Label != null && f.Label.LabelID > 0).ToList(); }
                catch { labelsTotal = new List<CountTaskBalance>(); }

                //LABELS
                foreach (CountTaskBalance r in labelsTotal.Where(f => f.CaseType == 4 || f.CaseType == 5 || f.CaseType == 6))
                {
                    
                    //Label Counted - Expected
                    if ((r.CaseType == 4 || r.CaseType == 5) && r.Mark == true)
                        UpdateLabelData(r.Label, r.Bin, countTask.DocNumber, active, user);

                    //Label no counted expected
                    if (r.CaseType == 6 && r.Mark == true)
                        UpdateLabelData(r.Label, NoCountBin, countTask.DocNumber, inActive, user);


                    //Hacer el ajuste de inventario si el label tiene diferencias.
                    if (r.Difference > 0 && r.Mark == true)
                        positiveLine = UpdatePositiveAdj(countTask, r, user, postiveAdj, positiveLine);

                    else if (r.Difference < 0 && r.Mark == true)
                    {
                        negativeAdj = UpdateNegativeAdj(countTask, r, user);
                        if (negativeAdj != null)
                            negativeDocs.Add(negativeAdj);
                    }

                }


                /* PRODUCTO
                 1. Ajusta las diferencias de producto en el mismo BIN (Positivo/Negativo) (Label == null && Expected > 0 && counted > 0)            
                 2. Aumenta el producto Encontrado (Ajuste Positivo) (Label == null && Expected == 0)
                 3. Disminuir el producto no encotrado pero esperado en el bin (Label = null && Expected > 0 && counted ==0)
                 */

                //Casetype
                //1. Producto Suelto en el BIN (contado o no Contado)
                //2. Producto No esperado ajuste positivo

                try { productTotal = taskList.Where(f => f.CaseType == 1 || f.CaseType == 2 || f.CaseType == 3 ).ToList(); }
                catch { productTotal = new List<CountTaskBalance>(); }


                foreach (CountTaskBalance r in productTotal.Where(f => f.Mark == true))
                {
                    if (r.Difference > 0) //r.QtyExpected == 0 && 
                        positiveLine = UpdatePositiveAdj(countTask, r, user, postiveAdj, positiveLine);

                    else if (r.Difference < 0) //r.QtyExpected > 0 && r.QtyCount == 0 && 
                    {
                        negativeAdj = UpdateNegativeAdj(countTask, r, user);
                        if (negativeAdj != null)
                            negativeDocs.Add(negativeAdj);
                    }
                }

                //Factory.Commit();
                Factory.IsTransactional = true;


                //Enviando Ajustes Positivos Al ERP
                if (postiveAdj != null)
                    try { ErpMngr.CreateInventoryAdjustment(postiveAdj, true); }
                    catch { }

                //Enviando Ajustes Negativos Al ERP
                if (negativeDocs.Count > 0)
                {
                    foreach (Document negAdj in negativeDocs)
                        try { ErpMngr.CreateInventoryAdjustment(negAdj, true); }
                        catch (Exception ex)
                        {
                            try
                            {
                                addLine = Factory.DaoDocumentLine().Select(new DocumentLine { Document = negAdj }).First();
                                addLine.Note = WriteLog.GetTechMessage(ex).Substring(0, 255);
                                Factory.DaoDocumentLine().Update(addLine);
                            }
                            catch { }
                        }
                }


                //Factory.IsTransactional = true;


                Status cancell = Factory.DaoStatus().Select(new Status { StatusID = DocStatus.Cancelled }).First();                

                //Cancel the BinExecution Completed Or New
                //Lista de Tareas Ejecutadas.
                //Poner las tareas en status completed.
                IList<BinByTask> executionList = taskList.Select(f=>f.BinByTask).Distinct().ToList();
                IList<BinByTaskExecution> exeChilds;
                foreach (BinByTask btExe in executionList)
                {
                    btExe.Status = posted;
                    btExe.ModDate = DateTime.Now;
                    btExe.ModifiedBy = user;
                    Factory.DaoBinByTask().Update(btExe);

                    //Actualizando los Hijos
                    exeChilds = Factory.DaoBinByTaskExecution().Select(new BinByTaskExecution { BinTask = new BinByTask { RowID = btExe.RowID } });
                    foreach (BinByTaskExecution ch in exeChilds)
                    {
                        ch.Status = posted;
                        ch.ModDate = DateTime.Now;
                        ch.ModifiedBy = user;
                        Factory.DaoBinByTaskExecution().Update(ch);
                    }

                }               

                //Factory.Commit();

                countTask.DocStatus = posted;
                countTask.ModDate = DateTime.Now;
                countTask.ModifiedBy = user;
                countTask.Date2 = DateTime.Now; //Confirmation Date
                Factory.DaoDocument().Update(countTask);


                Factory.Commit();



                return countTask;

            }
            catch (Exception ex)
            {
                Factory.Rollback();

                ExceptionMngr.WriteEvent("ConfirmCountingTaskDocument: " + countTask.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpPosting);
                throw;
            }
        }



        private void UpdateLabelData(Label label, Bin newBin, string notes, Status status, string user)
        {
            label.LastBin = label.Bin;
            label.ModDate = DateTime.Now;
            label.ModifiedBy = user;


            if (!string.IsNullOrEmpty(notes))
                label.Notes = notes;

            if (status != null)
            {
                label.Status = status;

                if (status.StatusID == EntityStatus.Active)
                    label.Node = new Node { NodeID = NodeType.Stored };
            }

            Factory.DaoLabel().Update(label);

            //Para garantizar que la deshabilita asi no pueda cambiar el BIN
            label.Bin = newBin;
            try { Factory.DaoLabel().Update(label); }
            catch { }

            
            //Actualizar los Hijos.
            try
            {
                label.ChildLabels = Factory.DaoLabel().Select(new Label { FatherLabel = label });

                if (label.ChildLabels != null && label.ChildLabels.Count > 0)
                {
                    foreach (Label child in label.ChildLabels)
                    {
                        child.LastBin = label.Bin;                        
                        child.ModDate = DateTime.Now;
                        child.ModifiedBy = user;


                        if (!string.IsNullOrEmpty(notes))
                            child.Notes = notes;

                        if (status != null)
                        {
                            child.Status = status;

                            if (status.StatusID == EntityStatus.Active)
                                label.Node = new Node { NodeID = NodeType.Stored };
                        }

                        Factory.DaoLabel().Update(child);

                        child.Bin = newBin;
                        try { Factory.DaoLabel().Update(child); }
                        catch { }

                    }
                }
             
            }
            catch { }
            
        }



        private Document UpdateNegativeAdj(Document countTask, CountTaskBalance r, string user)
        {

            Label targetLabel;
            if (r.Label != null)
                targetLabel = r.Label;
            else
                targetLabel = Factory.DaoLabel().Select(new Label { Bin = new Bin { BinID = r.Bin.BinID }, LabelType = new DocumentType { DocTypeID = LabelType.BinLocation } }).First();


            //Creando la linea del Ajuste
            DocumentLine addLine = new DocumentLine
            {
                Product = r.Product,
                Quantity = Math.Abs(r.Difference), //Math.Abs(binTask.QtyDiff),
                QtyAllocated = r.QtyExpected,
                ModifiedBy = user,
                CreationDate = DateTime.Now,
                CreatedBy = user,
                ModDate = DateTime.Now,
                Unit = r.Product.BaseUnit,
                LineStatus = new Status { StatusID = DocStatus.New },
                IsDebit = true, //r.Difference < 0 ? true : false,
                UnitBaseFactor = r.Product.BaseUnit.BaseAmount,
                //BinAffected = r.Bin.BinCode + ((r.Label != null) ? " " + r.Label.LabelCode : ""),
                BinAffected = targetLabel.LabelCode,
                Location = countTask.Location,
                Note = r.Bin.BinCode,
                Date1 = DateTime.Now
            };

            //Crear Ajustes de inventario positivos en un solo ajuste.
            Document negativeAdj = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                CreatedBy = user,
                Location = countTask.Location,
                Company = countTask.Company,
                IsFromErp = false,
                CrossDocking = false,
                Comment = "CountTask Posting (Negative Adj) " + countTask.DocNumber + ", " + user,
                Date1 = DateTime.Now,
                CustPONumber = countTask.DocNumber,
                Notes = WmsSetupValues.Counting_Bach
            };

            negativeAdj = DocMngr.CreateNewDocument(negativeAdj, false);
            addLine.Document = negativeAdj;
            addLine.LineNumber = 1;
            addLine = SaveAdjustmentTransaction(addLine, targetLabel, false);

            if (addLine.Note == "Adjust OK.")
                return negativeAdj;
            else
                return null;
        }


        private int UpdatePositiveAdj(Document countTask, CountTaskBalance r, string user, Document posAdj, int curLine)
        {

            Label targetLabel;
            if (r.Label != null)
                targetLabel = r.Label;
            else
                targetLabel = Factory.DaoLabel().Select(new Label { Bin = new Bin { BinID = r.Bin.BinID }, LabelType = new DocumentType { DocTypeID = LabelType.BinLocation } }).First();


            //Creando la linea del Ajuste
            DocumentLine addLine = new DocumentLine
            {
                Product = r.Product,
                Quantity = Math.Abs(r.Difference), //Math.Abs(binTask.QtyDiff),
                QtyAllocated = r.QtyExpected,
                ModifiedBy = user,
                CreationDate = DateTime.Now,
                CreatedBy = user,
                ModDate = DateTime.Now,
                Unit = r.Product.BaseUnit,
                LineStatus = new Status { StatusID = DocStatus.New },
                IsDebit = false, //r.Difference < 0 ? true : false,
                UnitBaseFactor = r.Product.BaseUnit.BaseAmount,
                //BinAffected = r.Bin.BinCode + ((r.Label != null) ? r.Label.LabelCode : ""),
                BinAffected = targetLabel.LabelCode,
                Location = countTask.Location,
                Note = r.Bin.BinCode,
                Date1 = DateTime.Now
            };

            addLine.Document = posAdj;
            addLine.LineNumber = curLine;
            addLine = SaveAdjustmentTransaction(addLine, targetLabel, false);

            if (addLine.Note == "Adjust OK.")
                return curLine + 1;
            else
                return curLine;
        }
      


        public IList<Document> GetTaskByUser(TaskByUser data, Location location)
        {
            IList<Document> list;

            try
            {
                list = Factory.DaoTaskByUser().Select(data).Select(f => f.TaskDocument).ToList();
            }
            catch
            {
                list = new List<Document>();
            }


            list = list.Union<Document>(
                Factory.DaoDocument().SelectPending(new Document
                {
                    Location = location,
                    DocType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Task } }

                }, WmsSetupValues.HistoricDaysToShow, WmsSetupValues.NumRegsDevice)
             ).ToList<Document>();


            return list;
        }


        public void CancelCountingTask(Document countTask, string user)
        {
            try {


                Factory.IsTransactional = true;
                Status active = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();


                //2. Unlock Bins            
                if (countTask.Notes == "0")
                {
                    IList<BinByTask> binList = Factory.DaoBinByTask().Select(new BinByTask { TaskDocument = countTask });

                    foreach (BinByTask bin in binList)
                    {
                        try
                        {
                            bin.Bin.Status = active;
                            bin.Bin.Comment = "";
                            Factory.DaoBin().Update(bin.Bin);
                        }
                        catch { }
                    }

                }
                else if (countTask.Notes == "1")
                {
                    IList<BinByTaskExecution> binList = Factory.DaoBinByTaskExecution().Select(new BinByTaskExecution { BinTask = new BinByTask { TaskDocument = countTask } });

                    foreach (BinByTaskExecution bin in binList)
                    {
                        try
                        {
                            bin.Bin.Status = active;
                            bin.Bin.Comment = "";
                            Factory.DaoBin().Update(bin.Bin);
                        }
                        catch { }
                    }
                }


                //3. Cancell all Execution Tasks
                IList<BinByTaskExecution> binListExe = Factory.DaoBinByTaskExecution().Select(new BinByTaskExecution { BinTask = new BinByTask { TaskDocument = countTask } });
                Status cancelled = Factory.DaoStatus().Select(new Status { StatusID = DocStatus.Cancelled }).First();

                foreach (BinByTaskExecution binExe in binListExe)
                {
                    binExe.Status = cancelled;
                    Factory.DaoBinByTask().Update(binExe);
                }


                Factory.Commit();


                //1. Update the document
                countTask.DocStatus = cancelled;
                Factory.DaoDocument().Update(countTask);

                Factory.Commit();


            }
            catch (Exception ex)
            {
                Factory.Rollback();

                ExceptionMngr.WriteEvent("CancelCountingTask: " + countTask.DocNumber, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.Business);
                throw;
            }
        }


        public void CreateScheduledCount()
        {
            DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Task } };
            docType.DocTypeID = SDocType.CountTask;

            // search for the Scheduled counts...
            IList<CountSchedule> list = Factory.DaoCountSchedule().Select(new CountSchedule { NextDateRun = DateTime.Today, IsDone = false });

            foreach (CountSchedule sch in list)
            {

                Document document = new Document
                {
                    Comment = "Scheduled Counting : "+sch.Title, 
                    DocType = docType,
                    CrossDocking = false,
                    IsFromErp = false,
                    Location = sch.Location, 
                    Company = sch.Location.Company, 
                    Date1 = DateTime.Today,
                    CreationDate = DateTime.Now,
                    CreatedBy = WmsSetupValues.SystemUser,
                    Notes = sch.CountOption.ToString()
                };
                document = DocMngr.CreateNewDocument(document, true);

                // parametros para ejecutar el query
                DataSet paramsQuery = BasicMngr.GetDataSet(sch.Parameters);

                // ejecuta el query q trae los productos/bines 
                DataSet dataSet = Factory.DaoIqReport().GetReportObject(sch.Query, paramsQuery);


                bool useProduct = true;

                if (sch.CountOption == 0) //Only BIN
                    useProduct = false;

                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    //  siempre deben enviar los alias "producto" "binCode" en el reporte !!!
                    Product prod = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(row["Product"].ToString()) && useProduct)
                            prod = Factory.DaoProduct().Select(new Product { ProductCode = row["Product"].ToString() }, 0)[0];
                    }
                    catch { }

                    Bin bin = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(row["BinCode"].ToString()))
                            bin = Factory.DaoBin().Select(new Bin { BinCode = row["BinCode"].ToString() })[0];
                    }
                    catch { }

                    //Crea el BinTask x prod/bin
                    BinByTask binByTask = new BinByTask
                    {
                        CreatedBy = WmsSetupValues.SystemUser,
                        CreationDate = DateTime.Now,
                        Bin = bin,
                        Product = prod,
                        TaskDocument = document,
                        Status = new Status { StatusID = DocStatus.New }
                    };

                    try
                    {
                        Factory.DaoBinByTask().Save(binByTask);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }

                // programamos sgte fecha de conteo (si no se pasa de la fecha final)
                if (sch.NextDateRun.Value.AddDays(double.Parse(sch.RepeatEach.ToString())) <= sch.Finish.Value)
                    sch.NextDateRun = sch.NextDateRun.Value.AddDays(double.Parse(sch.RepeatEach.ToString()));

                else  // ya finaliza el conteo repetitivo
                    sch.IsDone = true;

                sch.ModDate = DateTime.Now;
                sch.ModifiedBy = WmsSetupValues.SystemUser;

                Factory.DaoCountSchedule().Update(sch);
            }
        }


        public Document ProcessNoCount(List<ProductStock> listNoCount, string username, bool erp)
        {
            //Para el listado enviado 
            //1. Pasar los labels de esos productos en NOCOUNT a Printed = false & Status = active                       
            //2. hacer Ajustes Negativos BIN NOCOUNT por las cantidades

            // CAA [2010/06/10]
            // Nueva opción para enviar o nó, al ERP
            string erpText="OnlyWms";
            if (erp)
                erpText = "SentToErp";

            //Crear Ajustes de inventario positivos en un solo ajuste.
            Document negativeAdj = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                CreatedBy = username,
                Location = listNoCount[0].Bin.Location,
                Company = listNoCount[0].Product.Company,
                IsFromErp = false,
                CrossDocking = false,
                Comment = "CountTask Posting (NOCOUT Adj)" + username,
                Date1 = DateTime.Now,
                CustPONumber = "NOCOUNT Adjustment",
                Notes = WmsSetupValues.Counting_Bach,
                Reference = erpText
            };

            negativeAdj = DocMngr.CreateNewDocument(negativeAdj, false);


            Status active = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();

            Node storedNode = Factory.DaoNode().Select(new Node { NodeID = NodeType.Stored }).First();

            IList<Label> affectedLabels;
            int adjLine = 1;

            foreach (ProductStock ps in listNoCount)
            {
                affectedLabels = Factory.DaoLabel().Select(new Label
                {
                    Product = ps.Product,
                    Bin = ps.Bin,
                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel }
                });

                foreach (Label lbl in affectedLabels)
                {
                    lbl.Status = active;
                    lbl.Printed = false;
                    lbl.Node = storedNode;
                    Factory.DaoLabel().Update(lbl);
                }

                //Crear la Negative Lines.
                adjLine = CreateNoCountAdjLines(ps, username, negativeAdj, adjLine);
            }


            ErpMngr.CreateInventoryAdjustment(negativeAdj, true);
            return negativeAdj;

        }



        private int CreateNoCountAdjLines(ProductStock r, string user, Document posAdj, int curLine)
        {

            Label  targetLabel = Factory.DaoLabel().Select(new Label { Bin = new Bin { BinID = r.Bin.BinID }, 
                LabelType = new DocumentType { DocTypeID = LabelType.BinLocation } }).First();

            //Creando la linea del Ajuste
            DocumentLine addLine = new DocumentLine
            {
                Product = r.Product,
                Quantity = r.Stock, //Math.Abs(binTask.QtyDiff),
                ModifiedBy = user,
                CreationDate = DateTime.Now,
                CreatedBy = user,
                ModDate = DateTime.Now,
                Unit = r.Product.BaseUnit,
                LineStatus = new Status { StatusID = DocStatus.New },
                IsDebit = true,
                UnitBaseFactor = r.Product.BaseUnit.BaseAmount,
                BinAffected = r.Bin.BinCode + ((r.Label != null) ? r.Label.LabelCode : ""),
                Location = posAdj.Location,
                Note = r.Bin.BinCode,
                Date1 = DateTime.Now
            };

            addLine.Document = posAdj;
            addLine.LineNumber = curLine;
            addLine = SaveAdjustmentTransaction(addLine, targetLabel, false);

            if (addLine.Note == "Adjust OK.")
                return curLine + 1;
            else
                return curLine;
        }

        public void ProcessNoCountToBin(List<ProductStock> listNoCount, string username, Location location, Bin bin)
        {
            // CAA [2010/06/10]
            //Para el listado enviado 
            //Pasar los labels de esos productos en NOCOUNT a el BIN enviado & Status = active 

            Status active = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Active }).First();
            Bin tmpBin;
            Bin mainBin = Factory.DaoBin().Select(new Bin { BinCode = DefaultBin.MAIN, Location = location }).First();
            
            bool toOriginalBin = false;
            try
            {
                if (GetCompanyOption(location.Company, "RTLSTBIN").Equals("T"))
                    toOriginalBin = true;
            }
            catch { }

            IList<Label> affectedLabels;
            foreach (ProductStock ps in listNoCount)
            {
                affectedLabels = Factory.DaoLabel().Select(new Label
                {
                    Product = ps.Product,
                    Bin = ps.Bin,
                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel }
                });

                foreach (Label lbl in affectedLabels)
                {
                    tmpBin = lbl.Bin;
                    //1. Si viene Bin Tira el label al Bin que viene
                    if (bin != null)
                        lbl.Bin = bin;
                    else if (toOriginalBin)
                        lbl.Bin = lbl.LastBin;

                    if (lbl.Bin == null)
                        lbl.Bin = mainBin;

                    lbl.LastBin = tmpBin;

                    lbl.Status = active;
                    lbl.ModDate = DateTime.Now;
                    lbl.ModifiedBy = username;
                    Factory.DaoLabel().Update(lbl);
                }
            }

        }

        #endregion

    }
}
