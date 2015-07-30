using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Master;
using Entities.Trace;
using Entities.General;
using Integrator;
using Entities;
using Entities.Profile;

namespace BizzLogic.Logic
{
    /// <summary>
    /// Clase encargada de procesar los datos que arman los Headers y Bodies de impresion para plantillas 
    /// especificas de la clase PrintingTemplate y mapenadolas con LabelMapping.
    /// </summary>
    public partial class LabelMngr : BasicMngr
    {

 
        public String GetReplacedTemplateForLabels(IList<Label> labelList, LabelTemplate template, string printLot)
        {
            string result = "";

            //Obteniendo el listado de TAGS que se deben reemplazar en el template
            IList<LabelMapping> labelmappings = Factory.DaoLabelMapping().Select(
                new LabelMapping { LabelType = template.LabelType });

            //Reemplazando el Header
            if (template.PLHeader != null)
                result += ReplaceTemplate(labelmappings, template.PLHeader, labelList[0]) + Environment.NewLine;

            //Reemplazando el Body
            if (template.PLTemplate != null)
            {
                foreach (Label label in labelList)
                    result += ReplaceTemplate(labelmappings, template.PLTemplate, label) + Environment.NewLine;
            }

            return result;
        }



        /// <summary>
        /// Replace template parameters with object properties
        /// </summary>
        /// <param name="labels">Contains mapping of parameters to properties</param>
        /// <param name="template">Contains the template</param>
        /// <param name="obj">Contains the values to print</param>
        /// <returns></returns>
        private string ReplaceTemplate(IList<LabelMapping> labelMap, String templateData, Object obj)
        {
            for (int i = 0; i < labelMap.Count; i++)
                templateData = templateData.Replace(labelMap[i].DataKey, GetMapPropertyValue(labelMap[i].DataValue, obj));

            return templateData;
        }



  
        #region Obsolete

        //Process the lines to Print - OBSOLETE
        public String GetReplacedTemplate(IList<DocumentBalance> printList, LabelTemplate template, string printLot, UserByRol userByRol)
        {
            Factory.IsTransactional = true;

            string result = "";
            Node node = WType.GetNode(new Node { NodeID = NodeType.PreLabeled });
            Bin bin = WType.GetBin(new Bin { BinCode = DefaultBin.MAIN, Location = userByRol.Location });



            try
            {
                foreach (DocumentBalance printLine in printList)
                {
                    result += ProcessPrintingLine(printLine, template, printLot, node, bin, userByRol);

                    if (template.PrintEmptyLabel == true)
                        result += template.Empty;
                }

                Factory.Commit();

            }
            catch (Exception ex)
            {
                Factory.Rollback();
                ExceptionMngr.WriteEvent("GetReplacedTemplate:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.ErpConnection);
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

            return result;
        }



        /// <summary>
        /// Get Print file string to print
        /// </summary>
        /// <param name="labels">List of labels to print</param>
        /// <param name="template">Template to use for the printing</param>
        /// <returns></returns>
        public String ProcessPrintingLine(DocumentBalance printLine, LabelTemplate template,
            String printLot, Node node, Bin bin, UserByRol userByRol)
        {
            string result = "";
            Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active }); //Active


            //Obteniendo el listado de TAGS que se deben reemplazar en el template
            IList<LabelMapping> labelmappings = Factory.DaoLabelMapping().Select(
                new LabelMapping { LabelType = template.LabelType });


            //Template base
            //int i;
            IList<Label> labelList = new List<Label>();


            //Tipo De impresion
            //1. Normal Imprime standar, sin logistica

            //2. Logistic (Notes tiene data) - imprime normal mas la Logistica
            Unit logisticUnit = null;


            if (printLine.Notes != null && printLine.Notes.Contains("Logistic"))
            {
                string[] dataLogistic = printLine.Notes.Split(':');
                //El primer elemento contiene la unidad logistica.
                logisticUnit = Factory.DaoUnit().SelectById(new Unit { UnitID = int.Parse(dataLogistic[1]) });

                //3. Only print Logistic (notes tiene "ONLYPACK") - no imprime la normal (si las crea), solo imprime las logisticas
                //if (printLine.Notes.Contains("ONLYPACK"))
                    //printOnlyLogistic = true;
            }

            //CReating Document Line to Send
            DocumentLine prnLine = new DocumentLine
            {
                Product = printLine.Product,
                Document = printLine.Document,
                Unit = printLine.Unit,
                Quantity = printLine.Quantity

            };

            //Crea las etiquetas de la cantidad de producto a recibir Logisticas y sus Hijas
            double logisticFactor = (logisticUnit != null) ? (double)(logisticUnit.BaseAmount / printLine.Unit.BaseAmount) : 1;

            labelList = CreateProductLabels(logisticUnit, prnLine, node, bin, logisticFactor, printLot,"", DateTime.Now)
                .Where(f=>f.FatherLabel == null).ToList();



            //Reemplazando el Header
            if (template.Header != null)
                result += ReplaceTemplate(labelmappings, template.Header, labelList[0]) + Environment.NewLine;

            //Reemplazando el Body
            if (template.Body != null)
            {
                foreach (Label label in labelList)
                    result += ReplaceTemplate(labelmappings, template.Body, label) + Environment.NewLine;
            }

            return result;
        }





        #endregion

    }
}
