using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Report;
using Integrator.Dao;
using Entities;
using Entities.Trace;
using Entities.General;
using UtilTool;
using System.Data;
using Integrator;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace BizzLogic.Logic
{
    public class MessageMngr : BasicMngr
    {

        //1. Recorre la reglas de envio de mails configuradas en la clase Report.MessageRuleByCompany
        //2. Segun las reglas activas Recorre la entidad afectada con la condicion de los Atributos
        //3. Obtiene los registros y hace el reemplazo de la plantilla
        //4. Envia el mail.

        public MessageMngr()
        {
            Factory = new DaoFactory();
        }


        public void ProcessMessageRules()
        {

            IList<MessageRuleByCompany> ruleList = Factory.DaoMessageRuleByCompany().Select(
                new MessageRuleByCompany()
                ).Where(f => f.Active != false).ToList();

            //Revisa solo las activas
            foreach (MessageRuleByCompany rule in ruleList)
            {
                try
                {

                    switch (rule.Entity.ClassEntityID)
                    {
                        case EntityID.Document:
                            ProcessDocuments(rule);
                            break;

                        case EntityID.LogError:
                            ProcessLogErrors(rule);
                            break;

                        case EntityID.BusinessAlert:
                            ProcessBusinessAlert(rule);
                            break;
                    }

                }
                catch { }
            }


            //Envia los mensajes pendientes.
            SendPendingMessages();

        }



        private void ProcessBusinessAlert(MessageRuleByCompany rule)
        {
            //Console.WriteLine("Entre BA");

            try
            {

                //Si se vencio el next runtime lo ejecute
                if (rule.NextRunTime != null && rule.NextRunTime > DateTime.Now)
                    return;


                //Sace el data set y guarda el resultado en Message Pool
                MessagePool msg = new MessagePool
                {
                    CreationDate = DateTime.Now,
                    CreatedBy = WmsSetupValues.SystemUser
                };


                //Console.WriteLine("Get Message");
                msg.Message = GetBusinessAlertMessage(rule);

                //Console.WriteLine("Message: " + msg.Message);

                if (string.IsNullOrEmpty(msg.Message))
                    return;

                msg.Entity = new ClassEntity { ClassEntityID = EntityID.BusinessAlert };
                msg.IsHtml = rule.IsHtml;
                msg.MailFrom = rule.MailFrom;
                msg.MailTo = rule.MailTo;
                msg.Subject = rule.Template.Name;

                msg.AlreadySent = false;
                msg.Rule = rule;


                //Save the record.
                //Console.WriteLine("Saving BA:");
                Factory.DaoMessagePool().Save(msg);


                //Setea el next runtime
                rule.NextRunTime = DateTime.Now.AddSeconds(rule.FrequencyNumber * rule.FrequencyType);
                rule.LastUpdate = DateTime.Now;
                Factory.DaoMessageRuleByCompany().Update(rule);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                ExceptionMngr.WriteEvent(" BusinessAlert:" + rule.RuleName, ListValues.EventType.Fatal, ex, null,
                                ListValues.ErrorCategory.Messages);
            }
        }




        private string GetBusinessAlertMessage(MessageRuleByCompany rule)
        {
            IList<Object[]> objList = Factory.DaoMenuOptionExtension().GetReportObject(rule.Template.DetailQuery, new List<String>()); //Query

            //Creado el objeto a desplegar
            DataTable dt = GetDataTableSchema(rule.Template.DetailColumns.Split(','), "dt0", objList);

            if (dt == null || dt.Rows.Count == 0)
                return "";

          return ConvertDataTableToHtml(dt, "");

        }




        private void ProcessLogErrors(MessageRuleByCompany rule)
        {

            if (string.IsNullOrEmpty(rule.MailTo)) //Sale si no hay a quien enviarle
                return;


            if (rule.RuleType == 1)
            {
                //Created 
                LogError pattern = new LogError();


                if (!string.IsNullOrEmpty(rule.IntAttrib1))
                    pattern.Category = rule.IntAttrib1.Split('=')[1];


                //Obteniendo la lista de documentos que coinciden
                IList<Int64> listObj = Factory.DaoLogError().SelectRulesMessage((LogError)pattern, rule);


                if (listObj == null || listObj.Count == 0)
                    return;


                LogError data = null;
                foreach (Int64 rID in listObj)
                {

                    data = Factory.DaoLogError().SelectById(new LogError { LogErrorID = rID });
                    ProcessMessageObject(data, (Int32)rID, "", rule, EntityID.LogError);

                }
            }

        }


        /// <summary>
        /// Cunado la regla aplica sobre documentos
        /// </summary>
        /// <param name="rule"></param>
        private void ProcessDocuments(MessageRuleByCompany rule)
        {

            if (string.IsNullOrEmpty(rule.MailTo)) //Sale si no hay a quien enviarle
                return;

            if (rule.RuleType == 1)
            { 
                //Created 
                Document pattern = new Document();
                pattern.Company = rule.Company;

                if (!string.IsNullOrEmpty(rule.IntAttrib1))
                {
                    pattern.DocType = new DocumentType();
                    pattern.DocType.DocTypeID = short.Parse(rule.IntAttrib1.Split('=')[1]);
                }

                if (!string.IsNullOrEmpty(rule.IntAttrib2))
                {
                    pattern.DocStatus = new Status();
                    pattern.DocStatus.StatusID = short.Parse(rule.IntAttrib2.Split('=')[1]);
                }

                //Obteniendo la lista de documentos que coinciden
                IList<Int32> listObj = Factory.DaoDocument().SelectRulesMessage((Document)pattern, rule);


                if (listObj == null || listObj.Count == 0)
                    return;


                Document doc = null;

                foreach (Int32 docID in listObj)
                {
                    doc = Factory.DaoDocument().SelectById(new Document { DocID = docID });
                    ProcessMessageObject(doc, docID, doc.DocNumber, rule, EntityID.Document);
                }
            }
        }



        private void ProcessMessageObject(Object obj, Int32 rID, string rCode, MessageRuleByCompany rule, short EntityTypeID)
        {
            try
            {

                //lista de Mappings
                IList<LabelMapping> listMap = Factory.DaoLabelMapping().Select(
                    new LabelMapping { LabelType = new DocumentType { DocTypeID = LabelType.Message } });


                if (listMap != null && listMap.Count > 0)
                {
                    MessagePool msg = GenerateMessage(listMap, rule.Template, obj);

                    if (!string.IsNullOrEmpty(rule.Template.DetailQuery))
                        msg.Message += AddDocumentDetails(rule, rID, rCode);

                    //Revisa si hay rule extension que adicionan informacion al mensaje                        
                    if (rule.RuleExtensions != null && rule.RuleExtensions.Count > 0)
                        msg.Message += AddRuleExtension(rule, rID);

                    //Campos Faltantes
                    msg.RecordID = rID;
                    msg.Entity = new ClassEntity { ClassEntityID = EntityTypeID };
                    msg.IsHtml = rule.IsHtml;
                    msg.MailFrom = rule.MailFrom;
                    msg.MailTo = rule.MailTo;

                    //Si el objeto es un documento de tipo sales Shipment y esta la
                    //Variable de sent notification to customers se va el mail.
                    if (obj.GetType().Equals(typeof(Document)))
                        msg.MailTo +=  GetAccountAddress((Document)obj);


                    msg.AlreadySent = false;
                    msg.Rule = rule;

                    //Save the record.
                    Factory.DaoMessagePool().Save(msg);
                }

            }
            catch { }
        }



        private string GetAccountAddress(Document document)
        {
            try
            {
                if (GetCompanyOption(document.Company, "SHIPNOTE").Equals("T"))
                {
                    if (!string.IsNullOrEmpty(document.Customer.UserDefine1))
                        return ";" + document.Customer.UserDefine1;
                    else
                        return "";
                }
            }
            catch { }
            return "";
        }
        


        private string AddDocumentDetails(MessageRuleByCompany rule, Int32 ID, String Code)
        {
            string message = "";
            DataTable dt;
            string Query;


            Query = rule.Template.DetailQuery.Replace("__PARAM1", ID.ToString());
            Query = Query.Replace("__CODE1", Code);
            IList<Object[]> objList = Factory.DaoMenuOptionExtension().GetReportObject(Query, new List<String>()); //Query

            //Creado el objeto a desplegar
            dt = GetDataTableSchema(rule.Template.DetailColumns.Split(','), "dt0", objList);

            if (dt == null || dt.Rows.Count == 0)
                return message;

            message += ConvertDataTableToHtml(dt, "Document Details");

            return message;
        }




        private string AddRuleExtension(MessageRuleByCompany rule, Int32 ID)
        {
            string message = "";
            DataTable dt;
            string Query;

            foreach (MessageRuleExtension ruleExt in rule.RuleExtensions)
            {
                Query = ruleExt.Custom2.Replace("__PARAM1", ID.ToString());
                IList<Object[]> objList = Factory.DaoMenuOptionExtension().GetReportObject(Query, new List<String>()); //Query

                //Creado el objeto a desplegar
                dt = GetDataTableSchema(ruleExt.Custom3.Split(','), "dt0", objList);

                if (dt == null || dt.Rows.Count == 0)
                    continue;

                message += ruleExt.Custom2;
                message += ConvertDataTableToHtml(dt,"");
            }

            return message;
        }



        private MessagePool GenerateMessage(IList<LabelMapping> labelMap, 
            LabelTemplate templateData, Object obj)
        {
            //Header - Subject
            string subject = templateData.Header;
            string body = templateData.Body;
            for (int i = 0; i < labelMap.Count; i++)
            {
                subject = subject.Replace(labelMap[i].DataKey, GetMapPropertyValue(labelMap[i].DataValue, obj));
                body = body.Replace(labelMap[i].DataKey, GetMapPropertyValue(labelMap[i].DataValue, obj).Replace("\n","<br>")); //Para fit in html
            }


            return new MessagePool
            {
                Message = body,
                Subject = subject,
                CreationDate = DateTime.Now,
                CreatedBy = WmsSetupValues.SystemUser
            };

        }



        private void SendPendingMessages()
        {
            IList<MessagePool> msgList = Factory.DaoMessagePool().Select(new MessagePool { AlreadySent = false });

            if (msgList == null || msgList.Count == 0)
                return;

            ///Console.WriteLine(msgList.Count.ToString() + " messages to be send.");

            foreach (MessagePool msg in msgList)
            {
                //if (MailSender.SendEmail(msg))
                //{
                //    msg.AlreadySent = true;
                //    msg.ModifiedBy = WmsSetupValues.SystemUser;
                //    msg.ModDate = DateTime.Now;

                //    try { Factory.DaoMessagePool().Update(msg); }
                //    catch (Exception ex) {
                //        ExceptionMngr.WriteEvent("SendEmail:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Messages);
                //    }
                //}

                try
                {
                    // CAA [2010/07/27] Procesar adjuntos del correo
                    string[] adj = Attachements(msg);

                    if (MailSender.SendEmail(msg,adj))
                    {

                        msg.AlreadySent = true;
                        msg.ModifiedBy = WmsSetupValues.SystemUser;
                        msg.ModDate = DateTime.Now;

                        Factory.DaoMessagePool().Update(msg);
                    }

                }
                catch (Exception ex)
                {
                    ExceptionMngr.WriteEvent("SendEmail:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Messages);
                }
            }
        }


        private string[] Attachements(MessagePool message)
        {
            string[] results= new string[0]; 
            try
            {
                LocalReport localReport;
                // "E:\\GagPrjs\\wms30_\\WpfFrontOLD\\bin\\Debug"; //
                string appPath =  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);
                
                string[] files = message.Rule.Files.Split(',');
                results = new string[files.Count()];
                int cont = 0;

                // ciclo de attachements asociados al correo programado
                foreach (string templateId in files)
                {
                    localReport = new LocalReport();
                    LabelTemplate template = Factory.DaoLabelTemplate().Select(new LabelTemplate { RowID = int.Parse(templateId) }).First();
                    string reportPath = Path.Combine(appPath, WmsSetupValues.RdlTemplateDir + "\\" + template.Header);
                    localReport.ReportPath = reportPath;

                    //Obteniendo la informacion del Reporte (DataSet)
                    ReportMngr repMng = new ReportMngr();
                    Document doc = Factory.DaoDocument().Select(new Document { DocID = message.RecordID }).First();

                    ReportHeaderFormat rptHdr = repMng.GetReportInformation(doc, template.Header);
                    if (rptHdr.ReportDetails == null || rptHdr.ReportDetails.Count == 0)
                    {
                        rptHdr.ReportDetails.Add(new ReportDetailFormat { });
                    }

                    DataSet ds = GetReportDataset(rptHdr);
                    localReport.DataSources.Add(new ReportDataSource("Header", ds.Tables["ReportHeaderFormat"]));
                    localReport.DataSources.Add(new ReportDataSource("Details", ds.Tables["ReportDetailFormat"]));

                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    string reportType = "PDF";

                    //The DeviceInfo settings should be changed based on the reportType
                    //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
                    string deviceInfo =
                    "<DeviceInfo>" +
                    "  <OutputFormat>" + reportType + "</OutputFormat>" +
                    "  <PageWidth>10in</PageWidth>" +
                    "  <PageHeight>11in</PageHeight>" +
                    "  <MarginTop>0.2in</MarginTop>" +
                    "  <MarginLeft>1.0in</MarginLeft>" +
                    "  <MarginRight>0.5in</MarginRight>" +
                    "  <MarginBottom>0.2in</MarginBottom>" +
                    "</DeviceInfo>";

                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    //Render the report
                    renderedBytes = localReport.Render(
                        reportType,
                        deviceInfo,
                        // null,
                        out mimeType,
                        out encoding,
                        out fileNameExtension,
                        out streams,
                        out warnings);

                    string path = Path.Combine(appPath, WmsSetupValues.PrintReportDir + "\\" + doc.DocNumber + "_" + template.Name + "." + reportType);
                    BinaryWriter bn = new BinaryWriter(File.Open(path, FileMode.Create));
                    bn.Write(renderedBytes);
                    bn.Flush();
                    bn.Close();

                    results[cont++] = path;
                }
            }
            catch { }
            return results;
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
            return dh;
        }

    }
}