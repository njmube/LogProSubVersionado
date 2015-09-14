using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities;
using Integrator;
using Entities.General;
using Entities.Master;
using System.Reflection;
using System.IO;
using Entities.Process;
using Integrator.Dao;
using Entities.Report;
using System.Runtime.Serialization.Formatters.Binary;
using Entities.Profile;
using System.Data;
using ErpConnect;
using System.Data.SqlClient;


namespace BizzLogic.Logic
{

    //Procesos basicos Internos.
    //Ejecutados por transferencia a Bines de Proceso
    //O Ejecuciones de proceso basicas.

    public class ProcessMngr : BasicMngr
    {

        //Local Properties
        private DocumentMngr DocMngr { get; set; }
        private TransactionMngr TranMngr { get; set; }
        private WmsTypes WType { get; set; }
        private ErpDataMngr ErpMngr { get; set; }
        private CustomProcess currentProcess { get; set; }
        private Document tmpDocument { get; set; }
        string maiLine = "-----------------------------------------------------------------------\n";

        //Contexto
        IList<CustomProcessContext> pContext = null;
        string ApplicationPath {get; set;}

        //Constructor
        public ProcessMngr()
        {
            Factory = new DaoFactory();
            DocMngr = new DocumentMngr();
            TranMngr = new TransactionMngr();
            WType = new WmsTypes(Factory);
            ErpMngr = new ErpDataMngr();

        }



        public void EvaluateInspectionProcess(Document document, CustomProcess process, IList<Label> labelList, string appPath)
        {

            if (process == null || process.Status.StatusID != EntityStatus.Active)
                return;

            //1. Obtiene else proceso y sus steps;
            currentProcess = process;
            ApplicationPath = appPath;
            tmpDocument = document;

            IList<CustomProcessTransition> processActivityList = Factory.DaoCustomProcessTransition()
                .Select(new CustomProcessTransition { Process = currentProcess });

            if (processActivityList == null || processActivityList.Count == 0)
                return;

            //2. Instancia el Contexto para ese proceso en un Hahs Table
            pContext = Factory.DaoCustomProcessContext().Select(new CustomProcessContext());
            pContext.Where(f => f.ContextKey == "LABELLIST").First().Value = labelList;
            pContext.Where(f => f.ContextKey == "LOCATION").First().Value = document.Location;
            pContext.Where(f => f.ContextKey == "CREATEDBY").First().Value = document.CreatedBy;
            pContext.Where(f => f.ContextKey == "COMPANY").First().Value = document.Company;
            pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value = "";
            pContext.Where(f => f.ContextKey == "PROCESSBIN").First().Value = null;
            pContext.Where(f => f.ContextKey == "BACHNUM").First().Value = "";



            //Evalua el contexto especifico del proceso o de las entidades involucradas
            //Segun prioridad
            if (currentProcess.ProcessContext != null && currentProcess.ProcessContext.Count > 0)
                foreach (CustomProcessContextByEntity conByEnt in currentProcess.ProcessContext)
                {
                    try { pContext.Where(f => f.ContextKey == conByEnt.ContextKey).First().Value = GetContextEntityValue(conByEnt); }
                    catch { }
                }



            //3. Recorre las actividades del proceso.
            MethodInfo method;
            foreach (CustomProcessTransition step in processActivityList.Where(f => f.CurrentActivity.Status.StatusID == EntityStatus.Active).OrderBy(f => f.Sequence))
            {
                method = this.GetType().GetMethod(step.CurrentActivity.Method);
                method.Invoke(this, null);
            }


        }



        public void EvaluateCustomProcess(Bin bin, IList<Label> labelList, DocumentLine docLine, string appPath, Label labelSource)
        {

            if (bin.Process == null || bin.Process.Status.StatusID != EntityStatus.Active)
                return;

            //1. Obtiene else proceso y sus steps;
            currentProcess = bin.Process;
            ApplicationPath = appPath;

            IList<CustomProcessTransition> processActivityList = Factory.DaoCustomProcessTransition()
                .Select(new CustomProcessTransition { Process = currentProcess });

            if (processActivityList == null || processActivityList.Count == 0)
                return;

            //2. Instancia el Contexto para ese proceso en un Hahs Table
            pContext = Factory.DaoCustomProcessContext().Select(new CustomProcessContext());
            pContext.Where(f => f.ContextKey == "LABELLIST").First().Value = labelList;
            pContext.Where(f => f.ContextKey == "LOCATION").First().Value = bin.Location;
            pContext.Where(f => f.ContextKey == "BINDIRECTION").First().Value = BinType.Out_Only;
            pContext.Where(f => f.ContextKey == "CREATEDBY").First().Value = docLine.CreatedBy;
            pContext.Where(f => f.ContextKey == "PROCESSBIN").First().Value = bin;
            pContext.Where(f => f.ContextKey == "DOCUMENTLINE").First().Value = docLine;
            pContext.Where(f => f.ContextKey == "COMPANY").First().Value = bin.Location.Company;
            pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value = "";
            pContext.Where(f => f.ContextKey == "LABELSOURCE").First().Value = labelSource;
            pContext.Where(f => f.ContextKey == "BACHNUM").First().Value = "";


            //Evalua el contexto especifico del proceso o de las entidades involucradas
            //Segun prioridad
            if (currentProcess.ProcessContext != null && currentProcess.ProcessContext.Count > 0)
                foreach (CustomProcessContextByEntity conByEnt in currentProcess.ProcessContext)
                {
                    try { pContext.Where(f => f.ContextKey == conByEnt.ContextKey).First().Value = GetContextEntityValue(conByEnt); }
                    catch { }
                }



            //3. Recorre las actividades del proceso.
            MethodInfo method;
            foreach (CustomProcessTransition step in processActivityList.Where(f=>f.CurrentActivity.Status.StatusID == EntityStatus.Active ).OrderBy(f=>f.Sequence))
            {
                method = this.GetType().GetMethod(step.CurrentActivity.Method);
                method.Invoke(this, null);
            }


        }


        /// <summary>
        /// Usado para enviar Notificaciones Basicas, Picking Mails Etc.
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="labelList"></param>
        /// <param name="docLine"></param>
        /// <param name="appPath"></param>
        /// <param name="labelSource"></param>
        public void EvaluateBasicProcess(CustomProcess process, SysUser user, string message)
        {

            currentProcess = process;

            IList<CustomProcessTransition> processActivityList = Factory.DaoCustomProcessTransition()
                .Select(new CustomProcessTransition { Process = currentProcess });

            if (processActivityList == null || processActivityList.Count == 0)
                return;

            //2. Instancia el Contexto para ese proceso en un Hahs Table
            pContext = Factory.DaoCustomProcessContext().Select(new CustomProcessContext());
            pContext.Where(f => f.ContextKey == "CREATEDBY").First().Value = user.UserName;
            pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value = message;


            //Evalua el contexto especifico del proceo o de las entidades involucradas
            //Segun prioridad
            if (currentProcess.ProcessContext != null && currentProcess.ProcessContext.Count > 0)
                foreach (CustomProcessContextByEntity conByEnt in currentProcess.ProcessContext)
                {
                    try { pContext.Where(f => f.ContextKey == conByEnt.ContextKey).First().Value = GetContextEntityValue(conByEnt); }
                    catch { }
                }



            //3. Recorre las actividades del proceso.
            MethodInfo method;
            foreach (CustomProcessTransition step in processActivityList.Where(f => f.CurrentActivity.Status.StatusID == EntityStatus.Active).OrderBy(f => f.Sequence))
            {
                method = this.GetType().GetMethod(step.CurrentActivity.Method);
                method.Invoke(this, null);
            }


        }





        #region Auxilar Methods


        private Object GetContextEntityValue(CustomProcessContextByEntity conByEnt)
        {

            switch (conByEnt.ContextDataType)
            {
                case "Entities.General.LabelTemplate":
                    return Factory.DaoLabelTemplate().Select(
                        new LabelTemplate { RowID = int.Parse(conByEnt.ContextBasicValue) }).First();

                case "Entities.Master.Bin":
                    return Factory.DaoBin().Select(
                        new Bin { BinID = int.Parse(conByEnt.ContextBasicValue) }).First();

                case "Entities.General.Status":
                    return Factory.DaoStatus().Select(
                        new Status { StatusID = int.Parse(conByEnt.ContextBasicValue) }).First();

                case "System.Int32":
                        return Int32.Parse(conByEnt.ContextBasicValue);

                default:
                        return conByEnt.ContextBasicValue;
            }

        }


        #region serialization

        /*
        private object GetObjectByID(string objectID, string internalType)
        {
            switch (internalType)
            {
                case "Entities.General.LabelTemplate":
                    return typeof(LabelTemplate);

                case "Entities.General.Status":
                    return typeof(Status);

                default:
                    return null;
            }
        }

        private object CastBasicType(string type, string value)
        {
            if (type.Equals("System.Int32"))
                return Int32.Parse(value);
            else
                return value;
        }


        private object castObject(Type t, object curVal)
        {
            object result;
            // Grabbing the type that has the static generic method   
            Type typeGeneric = typeof(DynamicCast);

            // Grabbing the specific static method   
            MethodInfo methodInfo = typeGeneric.GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);

            // Binding the method info to generic arguments   
            Type[] genericArguments = new Type[] { t };
            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);

            // Simply invoking the method and passing parameters   
            // The null parameter is the object to call the method from. Since the method is   
            // static, pass null.   
            try
            {
                result = genericMethodInfo.Invoke(null, new object[] { curVal });
                return result;
            }
            catch { return null; }
        }


        private Type GetInternalType(string internalType)
        {
            switch (internalType)
            {
                case "Entities.General.LabelTemplate":
                    return typeof(LabelTemplate);

                case "Entities.General.Status":
                    return typeof(Status);

                default:
                    return null;
            }
        }


        //Otiene el objeto Serailiado de Base de Datos y lo trae para serializar.
        private Object GetDeserializedObject(Byte[] serializedObject)
        {
            MemoryStream s = new MemoryStream(serializedObject);
            BinaryFormatter b = new BinaryFormatter();
            return b.Deserialize(s);
        }


        //Serialinza un objeto
        private Byte[] SerializeObject(Object obj)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, obj);

            //Pasar la serializacion a un Byte Array
            int smLength = Convert.ToInt32(m.Length);
            byte[] byteArray = new byte[smLength];
            m.Read(byteArray, 0, smLength);
            return byteArray;

        }
        */

#endregion



        private Object GetContextValue(string contextKey)
        {
            return pContext.Where(f => f.ContextKey.Equals(contextKey)).First().Value;
        }


        //private void UpdateLabelFather(Label label, Label father)
        //{
        //    label.FatherLabel = father;
        //    Factory.DaoLabel().Update(label);

        //    if (label.ChildLabels != null)
        //        foreach (Label lbl in label.ChildLabels)
        //            UpdateLabelFather(lbl, father);
        //}


        private void AddCompletedActivityMessage(string activity, string message)
        {
            string result = "Activity: " + activity + " [COMPLETED OK].\n";
            result += maiLine;
            result += message + "\n\n\n";
            pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value += result;
        }


        private void AddFailedActivityMessage(string activity, string message)
        {
            string result = "Activity: " + activity + " [FAILED].\n";
            result += maiLine;
            result += message + "\n\n\n";

            pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value += result;
        }

        #endregion




        # region Process Activities


        public void ChangeProductToDefaultBin()
        {
            try
            {
                IList<Label> curLabelList = (List<Label>)GetContextValue("LABELLIST");
                Location location = (Location)GetContextValue("LOCATION");
                short binDirection = (short)GetContextValue("BINDIRECTION");
                Label lblSource = (Label)GetContextValue("LABELSOURCE");


                if (curLabelList == null || curLabelList.Count == 0)
                    return;

                Label newLocation = null;
                string msgResult = "";

                newLocation = GetProductDefaultBinLabel(curLabelList[0].Product, location, binDirection);

                if (newLocation == null)
                    throw new Exception("No default Bin defined for product " + curLabelList[0].Product.FullDesc);


                foreach (Label label in curLabelList)
                    TranMngr.UpdateLocation(label, newLocation);

                msgResult += curLabelList.Sum(f=>f.CurrQty).ToString() + " units of (" + curLabelList[0].Product.FullDesc + ") moved from [" + lblSource.Barcode + "] to [" + newLocation.Barcode + "]\n";

                AddCompletedActivityMessage("ChangeProductToDefaultBin", msgResult);

            }
            catch (Exception ex)
            {
                AddFailedActivityMessage("ChangeProductToDefaultBin", WriteLog.GetTechMessage(ex));
            }

        }


        ////GLOBAL: Evalua en que estado debe quedar el producto despues del proceso
        //public virtual Status ProductStatus { get; set; }
        public void UpdateProductStatus()
        {

            IList<Label> curLabelList = (List<Label>)GetContextValue("LABELLIST");


            if (curLabelList == null || curLabelList.Count == 0)
                return;


            Status productStatus = (Status)GetContextValue("PRODUCTSTATUS");

            //Cambiando el Status
            if (productStatus != null)
            {
                try
                {
                    //Saca los que tengan status diferente al nuevo
                    foreach (Label label in curLabelList.Where(f => f.Status.StatusID != productStatus.StatusID))
                        UpdateStatus(label, productStatus);

                    string msgResult = curLabelList.Count().ToString() + " labels of (" + curLabelList[0].Product.FullDesc + ") updated to (" + productStatus.Name + ")";

                    AddCompletedActivityMessage("UpdateProductStatus", msgResult);

                }
                catch (Exception ex) {
                    AddFailedActivityMessage("UpdateProductStatus", WriteLog.GetTechMessage(ex));
                }
            }

        }



        private void UpdateStatus(Label label, Status status)
        {
            label.Status = status;
            label.ModDate = DateTime.Now;
            label.ModifiedBy = WmsSetupValues.SystemUser;
            Factory.DaoLabel().Update(label);

            IList<Label> childs = Factory.DaoLabel().Select(new Label { FatherLabel = new Label { LabelID = label.LabelID } });
            if (childs != null && childs.Count > 0)
                foreach (Label lbl in childs)
                    UpdateStatus(lbl, status);
        }



        ////GLOBAL: Evalua si hay ajustenegativo para ese proceso
        //public virtual Boolean? NegativeAdjustment { get; set; }
        public void CreateNegativeAdjustment()
        {
            Bin processBin = (Bin)GetContextValue("PROCESSBIN");
            IList<Label> adjLabels = (List<Label>)GetContextValue("LABELLIST");
            DocumentLine docLine = (DocumentLine)GetContextValue("DOCUMENTLINE");
            string createdby = (string)GetContextValue("CREATEDBY");
            string bachNum = (string)GetContextValue("BACHNUM");


            if (adjLabels == null || adjLabels.Count == 0)
                return;


            int step = 0;

            //Obteniendo el nodo vois para salvar en el
            Node voidNode = WType.GetNode(new Node { NodeID = NodeType.Voided });

            Document curDocument = new Document
            {
                DocType = new DocumentType { DocTypeID = SDocType.InventoryAdjustment },
                //DocConcept = WType.GetDocumentConcept(new DocumentConcept { DocConceptID = ConceptType.Damage_Scrap }),
                CreatedBy = createdby,
                Location = processBin.Location,
                Company = processBin.Location.Company,
                IsFromErp = false,
                CrossDocking = false,
                Comment = "Adjustment for Process " + processBin.Process.Name,
                Date1 = DateTime.Now,
                Notes = bachNum
            };


            try
            {
                //Header del Documento de Ajuste
                curDocument = DocMngr.CreateNewDocument(curDocument, true);
                step = 1; //Creo el header del documento

                //Creo la linea del documento
                docLine.Document = curDocument;
                docLine.Location = processBin.Location;
                docLine.BinAffected = processBin.BinCode;
                docLine.LineNumber = 1;
                docLine.IsDebit = true;
                curDocument.DocumentLines = new List<DocumentLine>();
                curDocument.DocumentLines.Add(docLine);
                Factory.DaoDocument().Update(curDocument);
                step = 2; //Creo las lineas

                if (Factory.IsTransactional)
                    Factory.Commit();


                //Actualizo los labels para que aparezcan los nodetrace del ajuste
                IList<Label> childs;
                foreach (Label curLabel in adjLabels)
                {
                    //Salvar con el nuevo status
                    curLabel.Node = voidNode;
                    curLabel.ModDate = DateTime.Now;
                    curLabel.ModifiedBy = curDocument.CreatedBy;
                    
                    curLabel.CurrQty = 0; //Add to enforce the void

                    Factory.DaoLabel().Update(curLabel);

                    SaveNodeTrace(new NodeTrace
                    {
                        Node = voidNode,
                        Document = curDocument,
                        Label = curLabel,
                        Quantity = curLabel.CurrQty,
                        IsDebit = true,
                        Comment = "Negative Adjustment (Process)"
                    });


                    //Child Labels. Les hace lo mismo que al principal.
                    childs = Factory.DaoLabel().Select(new Label { FatherLabel = new Label { LabelID = curLabel.LabelID } });

                    if (childs != null && childs.Count > 0)
                        foreach (Label lbl in childs)
                        {
                            //Salvar con el nuevo status
                            lbl.Node = voidNode;
                            lbl.ModDate = DateTime.Now;
                            lbl.ModifiedBy = curDocument.CreatedBy;

                            Factory.DaoLabel().Update(lbl);
                        }

                }

                
                if (Factory.IsTransactional)
                    Factory.Commit();

                //Si hay Conexion al ERP Envia el documento de ajuste al ERP
                if (GetCompanyOption(processBin.Location.Company, "WITHERPIN").Equals("T"))
                    ErpMngr.CreateInventoryAdjustment(curDocument, false);
                else
                {
                    curDocument.DocStatus = new Status { StatusID = DocStatus.Completed };
                    Factory.DaoDocument().Update(curDocument);
                }


                if (Factory.IsTransactional)
                    Factory.Commit();

                AddCompletedActivityMessage("CreateNegativeAdjustment", "Document "+ curDocument.DocNumber +" was created.");

            }
            catch (Exception ex)
            {

                if (Factory.IsTransactional)
                    Factory.Rollback();

                if (step > 0)
                {
                    curDocument.DocStatus = WType.GetStatus(new Status { StatusID = DocStatus.Cancelled });
                    curDocument.Comment = "Cancelled: " + ex.Message;
                    Factory.DaoDocument().Update(curDocument);
                }

                AddFailedActivityMessage("CreateNegativeAdjustment", WriteLog.GetTechMessage(ex));
            }

        }



        ////GLOBAL: Evalua si el proceso realiza impresion de labels
        //public virtual Boolean? PrintLabels { get; set; }
        public void PrintProductLabels()
        {

            //Console.WriteLine("Entre a Print !");

            try
            {
                IList<Label> labelList = (List<Label>)GetContextValue("LABELLIST");

                if (labelList == null || labelList.Count == 0)
                    return;

                string createdby = (string)GetContextValue("CREATEDBY");
                Product product = labelList[0].Product;

                try //try para evitar errores de formato
                {
                    if (product.UnitsPerPack > 0)
                        pContext.Where(f => f.ContextKey == "UNITSPERLABEL").First().Value = product.UnitsPerPack;

                }
                catch { }


                //Template asociado al producto.
                if (product.DefaultTemplate != null)
                    pContext.Where(f => f.ContextKey == "LABELTEMPLATE").First().Value = product.DefaultTemplate;


                //Si el producto tiene definido el proceso reajusta los valores de las variables. LABELTEMPLATE, UNITSPERLABEL, PRINTER
                /*
                if (product.ProcessContext != null && product.ProcessContext.Where(f => f.ProcessType.DocTypeID == currentProcess.ProcessType.DocTypeID).Count() > 0)
                {
                    IList<CustomProcessContextByEntity> productContextList = product.ProcessContext
                        .Where(f => f.ProcessType.DocTypeID == currentProcess.ProcessType.DocTypeID).ToList();

                    object temp;
                    foreach (CustomProcessContextByEntity conByEnt in productContextList)
                    {
                        temp = GetContextEntityValue(conByEnt);
                        if (temp != null)
                            pContext.Where(f => f.ContextKey == conByEnt.ContextKey).First().Value = GetContextEntityValue(conByEnt);
                    }

                }
                 */


                string printPath;
                if (string.IsNullOrEmpty(ApplicationPath))
                    printPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                       WmsSetupValues.WebServer);
                else
                    printPath = ApplicationPath;




                //El template, la impresora , units per label deben evaluarse primero en el producto,
                //Luego en el proceso.
                Printer printer;
                LabelTemplate template;
                int unitsPerLabel;
                try { unitsPerLabel = (int)GetContextValue("UNITSPERLABEL"); }
                catch { throw new Exception("Units per Pack not defined for the product " + product.Name + "."); }


                try { template = (LabelTemplate)GetContextValue("LABELTEMPLATE"); }
                catch { throw new Exception("No label template defined for the process."); }


                Company company = (Company)GetContextValue("COMPANY");
                Status pStatus = labelList[0].Status; //(Status)GetContextValue("PRODUCTSTATUS");

                try { printer = new Printer { PrinterName = template.DefPrinter.Name, PrinterPath = template.DefPrinter.CnnString }; }
                catch { throw new Exception("No printer defined for template " + template.Name + "."); }


                //Si el pack es mayor que 1, crea las nuevas logisticas donde quedara el producto.
                if (template.IsUnique == true)
                {
                    labelList = RepackProduct(labelList, company, createdby, unitsPerLabel, pStatus);
                    //Modificando el valor del Label List en el contexto
                    pContext.Where(f => f.ContextKey == "LABELLIST").First().Value = labelList;
                }

                else
                {
                    //Template Generico

                    double qtyUnits = labelList.Sum(f => f.CurrQty);

                    //Crear labels genericos sacando la razon de labels to print
                    int genericToPrint = (qtyUnits % unitsPerLabel) == 0
                        ? (int)qtyUnits / unitsPerLabel
                        : (int)qtyUnits / unitsPerLabel + 1;

                    IList<Label> genericList = new List<Label>();
                    double packQty = 0;

                    for (int z = 0; z < genericToPrint; z++)
                    {
                        //labelList[0].CurrQty = (unitsPerLabel <= qtyUnits) ? unitsPerLabel : qtyUnits;
                        //labelList[0].StartQty = (unitsPerLabel <= qtyUnits) ? unitsPerLabel : qtyUnits;

                        packQty = (unitsPerLabel <= qtyUnits) ? unitsPerLabel : qtyUnits;

                        genericList.Add(
                            new Label
                                {
                                    Product = labelList[0].Product,
                                    LabelCode = labelList[0].Product.ProductCode,
                                    LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                                    CurrQty = packQty,
                                    StartQty = packQty
                                });

                        qtyUnits -= packQty;
                    }

                    labelList = genericList;


                    //El label inicial queda como printed = 0;
                    foreach (Label lbl in (List<Label>)GetContextValue("LABELLIST"))
                    {
                        lbl.Printed = false;
                        Factory.DaoLabel().Update(lbl);
                    }


                }

                //Console.WriteLine("Before Print !");

                ReportMngr.PrintLabelsFromFacade(printer, template, labelList, printPath);

                string msgResult = labelList.Count().ToString() + " labels of (" + labelList[0].Product.FullDesc + ") was printed.";

                AddCompletedActivityMessage("PrintProductLabels", msgResult);

            }
            catch (Exception ex)
            {
                AddFailedActivityMessage("PrintProductLabels", WriteLog.GetTechMessage(ex));
            }

        }



        private IList<Label> RepackProduct(IList<Label> labelList, Company company,
            string createdby, int unitsPerLabel, Status productStatus)
        {
            //Result
            IList<Label> labelResult = new List<Label>();


            DocumentType labelType = Factory.DaoDocumentType().Select(new DocumentType { DocTypeID = LabelType.ProductLabel }).First();

            //Funcion para obtener siguiente Label
            //DocumentTypeSequence initSequence = GetNextDocSequence(company, labelType);

            Label fatherLabel = null;
            string notes = "Process " + currentProcess.Name;


            Unit logisticUnit = null;
            try { logisticUnit = labelList[0].Product.ProductUnits.Select(f => f.Unit).Where(f => f.BaseAmount == unitsPerLabel).First(); }
            catch { logisticUnit = Factory.DaoUnit().Select(new Unit { Name = WmsSetupValues.CustomUnit, Company = company }).First(); }

            //Factor
            int logisticFactor = unitsPerLabel;
            double labelBalance = labelList.Sum(f => f.CurrQty); //labelList.Count;

            //Crear labels genericos sacando la razon de labels to print
            int labelsToPrint = (labelBalance % unitsPerLabel) == 0
                ? (int)labelBalance / unitsPerLabel
                : (int)labelBalance / unitsPerLabel + 1;

            //label temporal para sacar la informacion basica
            Label tmpLabel = labelList[0];

            for (int z = 0; z < labelsToPrint; z++)
            {

                fatherLabel = new Label();
                fatherLabel.Node = tmpLabel.Node;
                fatherLabel.Bin = tmpLabel.Bin;
                fatherLabel.Product = tmpLabel.Product;
                fatherLabel.CreatedBy = createdby;
                fatherLabel.Status = productStatus;
                fatherLabel.LabelType = labelType;
                fatherLabel.CreationDate = DateTime.Now;
                fatherLabel.Printed = false;
                fatherLabel.ReceivingDate = labelList[0].ReceivingDate;


                fatherLabel.Unit = logisticUnit;
                fatherLabel.IsLogistic = false;
                fatherLabel.FatherLabel = null;
                fatherLabel.LabelCode = ""; // (initSequence.NumSequence + z).ToString() + GetRandomHex(createdby, initSequence.NumSequence + z);
                fatherLabel.CurrQty = (logisticFactor <= labelBalance) ? logisticFactor : labelBalance;
                //Aqui guardan las logisitcas las hijas que contienen unas vez se imprimen
                fatherLabel.StartQty = (logisticFactor <= labelBalance) ? logisticFactor : labelBalance;

                fatherLabel.Notes = notes;

                fatherLabel = Factory.DaoLabel().Save(fatherLabel);
                fatherLabel.LabelCode = fatherLabel.LabelID.ToString();
                labelResult.Add(fatherLabel);

                //Registra el movimiento del nodo

                SaveNodeTrace(
                    new NodeTrace
                    {
                        Node = tmpLabel.Node,
                        Document = null,
                        Label = fatherLabel,
                        Quantity = fatherLabel.CurrQty,
                        IsDebit = false,
                        CreatedBy = createdby,
                        Comment = "Process: Repack Product"
                    }
                );


                //UpdateLabelFather(tmpLabel, fatherLabel);
                labelBalance -= fatherLabel.CurrQty;  //Disminuye el Balance
            }

            //Ajustando la sequencia
            //initSequence.NumSequence += labelsToPrint;

            //Factory.DaoDocumentTypeSequence().Update(initSequence);


            //Debe Poner en Void Los labels Originales Que fueron Reempacados.
            Node voidNode = Factory.DaoNode().Select(new Node {NodeID = NodeType.Voided }).First();
            Status inactive = Factory.DaoStatus().Select(new Status { StatusID = EntityStatus.Inactive }).First();
            foreach (Label oldLabel in labelList)
            {
                oldLabel.Node = voidNode;
                oldLabel.Notes = "Voided by Repack Process.";
                oldLabel.ModifiedBy = createdby;
                oldLabel.ModDate = DateTime.Now;
                oldLabel.Status = inactive;
                oldLabel.CurrQty = 0; //Add to enforce the void
                Factory.DaoLabel().Update(oldLabel);
            }

            return labelResult;

        }



        public void SendConfirmationMail()
        {
            try
            {
                MessagePool message = new MessagePool();
                message.CreatedBy = (String)GetContextValue("CREATEDBY");
                message.CreationDate = DateTime.Now;
                message.MailTo = (String)GetContextValue("EMAILTO");
                message.MailFrom = (String)GetContextValue("EMAILFROM");
                message.Subject = "WMS Process: " + currentProcess.Name + ", By: " + message.CreatedBy + ", Date: " + DateTime.Today.ToShortDateString();
                
                string msg = (String)GetContextValue("RESULTMESSAGE");
                msg = msg.Replace("\n","<br>");
                msg = "<b>"+message.Subject+"</b><br><br>" + msg;
                message.Message = msg;
                
                Factory.DaoMessagePool().Save(message);
            }
            catch { Factory.Rollback(); }
        }



        public void MoveToStockBin()
        {
            Factory.IsTransactional = true;

            try
            {
                IList<Label> curLabelList = (List<Label>)GetContextValue("LABELLIST");
                Location location = (Location)GetContextValue("LOCATION");
                Bin processBin = (Bin)GetContextValue("PROCESSBIN");
                Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                if (curLabelList == null || curLabelList.Count == 0)
                    return;

                Label newLocation = null;
                string msgResult = "";
                string strDetails = "\n";

                try
                {
                    newLocation = processBin.LabelRef[0];
                }
                catch { }


                Document docTranfer = null;
                int numLine = 1;

                foreach (Label label in curLabelList)
                {
                    if (label.Bin.Location.LocationID == newLocation.Bin.Location.LocationID)
                    {

                        label.Status = active;
                        TranMngr.UpdateLocation(label, newLocation);
                        strDetails += label.LabelCode + ", " + label.Product.FullDesc + "\n";
                    }

                    else //Inventory Adjustment Aumentando la nuev, Disminuye la Vieja.
                    {
                        if (docTranfer == null)
                        {
                            docTranfer = new Document
                            {
                                Location = label.Bin.Location, //Origen
                                DocType = new DocumentType { DocTypeID = SDocType.WarehouseTransferReceipt },
                                IsFromErp = false,
                                CrossDocking = false,
                                Date1 = DateTime.Now,
                                CreatedBy = (String)GetContextValue("CREATEDBY"),
                                Company = label.Bin.Location.Company,
                                Comment = "Tranfer by Process " + currentProcess.Name,
                                UseAllocation = false,
                                Notes = "TRFTO" + currentProcess.Name
                            };

                            docTranfer = DocMngr.CreateNewDocument(docTranfer, false);
                            docTranfer.DocumentLines = new List<DocumentLine>();                            
                        }

                        //Tranfer lines                
                        DocumentLine trfLine = new DocumentLine
                            {
                                Product = label.Product,
                                Unit = label.Product.BaseUnit,
                                Quantity = label.BaseCurrQty,
                                CreatedBy = docTranfer.CreatedBy, //(labelSource.ModifiedBy == null) ? WmsSetupValues.SystemUser : labelSource.ModifiedBy,
                                LineStatus = WType.GetStatus(new Status { StatusID = DocStatus.New }),
                                IsDebit = false,
                                UnitBaseFactor = label.Product.BaseUnit.BaseAmount,
                                BinAffected = label.Bin.BinCode + " in " + label.Bin.Location.ErpCode + " to " + newLocation.Bin.BinCode + " in " + newLocation.Bin.Location.ErpCode,
                                Location = label.Bin.Location, //FROM
                                Location2 = newLocation.Bin.Location,
                                CreationDate = DateTime.Now,
                                Document = docTranfer,
                                LineNumber = numLine++,
                                Date1 = docTranfer.Date1
                            };

                        docTranfer.DocumentLines.Add(trfLine);

                        label.Status = active;
                        TranMngr.UpdateLocation(label, newLocation);
                        strDetails += "TRF: " + label.LabelCode + ", " + label.Product.FullDesc + "\n";

                    }
                }


                //Si hay documento para un transfer manda a crear el Transfer
                if (docTranfer != null)
                    try
                    {
                        Factory.DaoDocument().Update(docTranfer);

                        if (docTranfer.DocumentLines.Count > 0)
                        {
                            ErpMngr.CreateLocationTransfer(docTranfer);
                            msgResult += "Tranfer Document " + docTranfer.DocNumber + " Created OK.\n\n";
                        }
                        else                        
                            msgResult += "Error: Tranfer Document " + docTranfer.DocNumber + " Does not contain lines.\n\n";
                        
                    }
                    catch (Exception ex)
                    { msgResult += "Tranfer Document " + docTranfer.DocNumber + " Failed.\n" + ex.Message + "\n"; }



                msgResult += curLabelList.Count.ToString() + " " + currentProcess.Name + " label(s) moved to [" + newLocation.Barcode + "]\n";
                msgResult += strDetails;



                AddCompletedActivityMessage("MoveToStockBin", msgResult);

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                AddFailedActivityMessage("MoveToStockBin", WriteLog.GetTechMessage(ex));
            }

        }



        public void MoveToDamageBin()
        {
            try
            {
                IList<Label> curLabelList = (List<Label>)GetContextValue("LABELLIST");
                Location location = (Location)GetContextValue("LOCATION");
                Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
                Node storedNode = WType.GetNode(new Node { NodeID = NodeType.Stored });

                if (curLabelList == null || curLabelList.Count == 0)
                    return;

                Label newLocation = null;
                string msgResult = "";
                string strDetails = "\n";

                try
                {
                    newLocation = WType.GetBin(new Bin { BinCode = DefaultBin.DAMAGE, Location = location }).LabelRef[0];
                }
                catch { }

                string notes = "";
                foreach (Label label in curLabelList)
                {
                    label.Status = active;
                    label.Node = storedNode;
                    strDetails += label.Barcode + ", " + label.Product.FullDesc + ": ";
                    notes = TranMngr.ChangeLabelUbication(label, newLocation, ApplicationPath, new SysUser { UserName = WmsSetupValues.SystemUser }).Notes;
                    strDetails +=  notes + "\n";
                }

                msgResult += curLabelList.Count.ToString() + " " + currentProcess.Name + " label(s) moved to [" + newLocation.Barcode + "]\n";
                msgResult += strDetails;

                AddCompletedActivityMessage("MoveToDamageBin", msgResult);

            }
            catch (Exception ex)
            {
                AddFailedActivityMessage("MoveToDamageBin", WriteLog.GetTechMessage(ex));
            }
        }



        public void ImageRepairProcess()
        {

            Factory.IsTransactional = true;
            string msgResult = "";
            Account vendor = null;
            string vendorCode = "";
            string custCode = "";
            string Flag = "Inicio";

            try
            {

                Location location = (Location)GetContextValue("LOCATION");
                String createdBy = pContext.Where(f => f.ContextKey == "CREATEDBY").First().Value.ToString();
                Account customer = null;

                //1. Create Vendor as Customer
                //Account customer = Factory.DaoAccount().Select(new Account { AccountID = tmpDocument.Vendor.AccountID }).First();
                //customer.AccountAddresses = Factory.DaoAccountAddress().Select(new AccountAddress { Account = new Account { AccountID = tmpDocument.Vendor.AccountID } });

                try
                {
                    vendorCode = Factory.DaoConfigOptionByCompany().Select(new ConfigOptionByCompany
                        {
                            Company = location.Company,
                            ConfigOption = new ConfigOption { Code = "REPVENDOR" }
                        }).First().Value;
                }
                catch {
                    AddFailedActivityMessage("ImageRepairProcess", "No vendor defined for Repair. Setup Config variable REPVENDOR." );
                    return;
                }


                try
                {
                    custCode = Factory.DaoConfigOptionByCompany().Select(new ConfigOptionByCompany
                    {
                        Company = location.Company,
                        ConfigOption = new ConfigOption { Code = "REPCUST" }
                    }).First().Value;
                }
                catch
                {
                    AddFailedActivityMessage("ImageRepairProcess", "No customer defined for Repair. Setup Config variable REPCUST.");
                    return;
                }



                //VENDOR
                vendor = Factory.DaoAccount().Select(new Account {
                    AccountCode = vendorCode,
                    BaseType = new AccountType { AccountTypeID = AccntType.Vendor }
                }).First();
                
                //CUSTOMER
                try
                {
                   customer = Factory.DaoAccount().Select(new Account
                    {
                        AccountCode = custCode,
                        BaseType = new AccountType { AccountTypeID = AccntType.Customer }
                    }).First();
                }
                catch
                {
                    AddFailedActivityMessage("ImageRepairProcess", "Customer " + custCode  + " not defined for Repair. Please create it in the ERP firts.");
                    return;
                }


                try
                {
                    customer.AccountAddresses = Factory.DaoAccountAddress().Select(
                        new AccountAddress { Account = new Account { AccountID = customer.AccountID } });
                }
                catch { }


                Flag = "Document";


                try { pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value += "Processing " + tmpDocument.DocNumber + "\n"; }
                catch { }


                //2. Enlist the base Document
                Document document = new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.ProcessTask },
                    CreatedBy = createdBy,
                    Location = location,
                    Company = location.Company,
                    IsFromErp = false,
                    CrossDocking = false,
                    Comment = "",
                    Date1 = DateTime.Now,
                    Vendor = vendor,
                    Customer = customer,
                    CustPONumber = tmpDocument.DocNumber
                };

                document.DocumentLines = new List<DocumentLine>();

                Flag = "Document Lines ";

                if (tmpDocument.DocumentLines == null || tmpDocument.DocumentLines.Count == 0)                
                    tmpDocument.DocumentLines = Factory.DaoDocumentLine().Select(
                        new DocumentLine { Document = new Document { DocID = tmpDocument.DocID } });


                Flag = "Entering to Document Lines ";

                    foreach (DocumentLine line in tmpDocument.DocumentLines)
                    {
                        document.DocumentLines.Add(new DocumentLine
                        {
                            Document = document,
                            Product = line.Product,
                            Unit = line.Product.BaseUnit,
                            Quantity = line.Quantity
                        });
                    }


                //3. Create the PO
                Flag = "Creating PO";
                String poNumber = ErpMngr.CreatePurchaseOrder(document);                
                msgResult += "\nPurchase Order: <b>" + poNumber + "</b>";


                //4. Create the SO
                Flag = "Creating SO";
                string repairSOID = "";
                try
                {
                    repairSOID = Factory.DaoConfigOptionByCompany().Select(new ConfigOptionByCompany
                    {
                        Company = location.Company,
                        ConfigOption = new ConfigOption { Code = "REPSOID" }
                    }).First().Value;
                }
                catch
                {
                    AddFailedActivityMessage("ImageRepairProcess", "No SOID Defined. Setup Config variable REPSOID.");
                    return;
                }

                document.CustPONumber = poNumber;
                //Location REPAIR
                document.Location = Factory.DaoLocation().Select(new Location { LocationID = 114 }).First(); //new Location { LocationID = 114 }; //114 = REPAIR
                Flag = "Creating SO - ERP";
                string soNumber = ErpMngr.CreateSalesOrder(document, repairSOID, "WMS_REPAIR_ORD");

                msgResult += "\nSales Order:<b>" + soNumber + "</b>\n";


                Flag = "Final";

                msgResult = "Repair Transaction for Vendor <b>" + vendor.FullDesc + "</b> was Created.\n" + msgResult;
                AddCompletedActivityMessage("ImageRepairProcess", msgResult);

                Factory.Commit();

            }

            catch (Exception ex)
            {
                Factory.Rollback();
                AddFailedActivityMessage("ImageRepairProcess", "Vendor <b>"+ vendor.FullDesc + "</b>\n" + msgResult + "Process Failed:" + Flag + " - " + tmpDocument.DocNumber + " \n" + WriteLog.GetTechMessage(ex));
            }

        }



        public void ImageVendorProcess()
        {
            //Enviar 1 Email por cada producto.
            //Agrupa pro producto y envia un mail con las cantidades al destinatario.
            string mailMsg = "";

            try { pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value 
                += "Processing " + tmpDocument.DocNumber + "\n"; }
            catch { }


            IList<Label> curLabelList = (List<Label>)GetContextValue("LABELLIST");
            foreach (Product product in curLabelList.Select(f => f.Product).Distinct())
            {
                mailMsg = "Product to sent to Vendor:\n";

                foreach(Label lbl in curLabelList.Where(f=>f.Product.ProductID == product.ProductID)) 
                    mailMsg += lbl.Barcode + ", " + lbl.Product.FullDesc + "\n";

                //Enviar el mail para ese producto.
                pContext.Where(f => f.ContextKey == "RESULTMESSAGE").First().Value += mailMsg;
                SendConfirmationMail();

            }
            
        }


        /*

    public Document CreateProcessDocument(DocumentLine curLine, Location location, String createdby)
    {
    Document prtDoc = null;

    try
    {
      Factory.IsTransactional = true;

      //1. Crea un Documento de Cross Dock. Y los relaciona con el PO y SO.
      DocumentType docType = WType.GetDocumentType(new DocumentType { DocTypeID = SDocType.WareHouseProcess });
      DocumentTypeSequence docSeq = GetNextDocSequence(location.Company, docType);

      Account vendor = null;

      //Crear Document header
      prtDoc = new Document
      {
          DocNumber = docSeq.CodeSequence,
          DocType = docType,
          IsFromErp = false,
          CrossDocking = true,
          Date1 = DateTime.Now,
          CreatedBy = createdby,
          Company = curLine.Product.Company,
          Vendor = vendor
      };

      DocMngr.CreateNewDocument(prtDoc, false);


      //2. Document Lines
      int lineNumber = 1;

          DocumentLine docLine = new DocumentLine
          {
              Document = prtDoc,
              Product = curLine.Product,
              LineStatus = new Status { StatusID = DocStatus.New },
              Unit = curLine.Unit,
              Quantity = curLine.Quantity,
              CreationDate = DateTime.Now,
              IsDebit = false,
              LineNumber = lineNumber,
              Location = location,
              UnitBaseFactor = curLine.Unit.BaseAmount,
              CreatedBy = createdby
          };

          prtDoc.DocumentLines.Add(docLine);


      Factory.Commit();
      return prtDoc;
    }

    catch (Exception ex)
    {
      Factory.Rollback();
      Factory.DaoDocument().Delete(prtDoc);
      ExceptionMngr.WriteEvent("CreateProcessDocument:Doc#" + prtDoc.DocNumber, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
      throw;
    }
    }


    */

        #endregion

        public string ProcessFile(CustomProcess process, string stream, SysUser user)
        {
            try
            {
                string strTable = "TMP_" + process.ProcessID;
                //1. Convierte el file en un DataTable
                DataSet ds = ConvertToDataSet(stream, strTable, process.BatchNo, false);

                //Connect to Database SQL
                SQLBase sql = new SQLBase();
                sql.Connection = new SqlConnection(process.Printer.CnnString); //Printeres un Connection
                sql.SaveDataSet(ds, strTable, user.UserName);

                return "";

            }
            catch (Exception ex) {
                return ex.Message;
            }

        }


        public static DataSet ConvertToDataSet(string strFile, string TableName, string delimiter, bool useHeader)
        {

            if (string.IsNullOrEmpty(delimiter))
                delimiter = "\t";
            
            //The DataSet to Return
            DataSet result = new DataSet();

            //Add the new DataTable to the RecordSet
            result.Tables.Add(TableName); //TableName

            //Open the file in a stream reader.
            //StreamReader s = new StreamReader(File);


            //Split off each row at the Carriage Return/Line Feed
            //Default line ending in most windows exports.  
            //You may have to edit this to match your particular file.
            //This will work for Excel, Access, etc. default exports.
            string[] rows = strFile.Split("\n".ToCharArray());


            //Split the first line into the columns       
            string[] columns = rows[0].Split(delimiter.ToCharArray());

            //Cycle the colums, adding those that don't exist yet 
            //and sequencing the one that do.;
            for (int i =0; i < columns.Length; i++)
                result.Tables[TableName].Columns.Add("col_"+i.ToString());
           

            //Read the rest of the data in the file.        
            //string AllData = s.ReadToEnd();

            //Now add each row to the DataSet        
            foreach (string r in rows)
            {
                //Split the row at the delimiter.
                string[] items = r.Trim().Split(delimiter.ToCharArray());

                //Add the item
                result.Tables[TableName].Rows.Add(items);
            }

            //Return the imported data.        
            return result;
        }


    }
}