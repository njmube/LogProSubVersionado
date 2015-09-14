using System;
using Integrator.Dao.General;
using Integrator.Dao.Master;
using Integrator.Dao.Trace;
using Integrator.Dao.Profile;
using Integrator.Config;
using NHibernate;
using Integrator.Dao.Report;
using Integrator.Dao.Process;
using System.Data;
using Integrator.Dao.Workflow;


namespace Integrator.Dao
{
    public class DaoFactory
    {
        public String Error { get; set; }

        private ISession session;
        public Boolean IsTransactional = false;


        public DaoFactory() { }


        public ISession Session
        {
            get
            {
                try
                {
                    if (session == null || !session.IsOpen)
                        session = NHibernateHelper.CurrentSession();

                    if (!session.Transaction.IsActive)
                        session.BeginTransaction(IsolationLevel.ReadUncommitted);

                    return session;
                }
                catch (HibernateException e)
                {
                    //Error = e.Message;
                    session.Close();
                    NHibernateHelper.WriteEventLog("Session: " + WriteLog.GetTechMessage(e));
                    throw new Exception(WriteLog.GetTechMessage(e));

                }
            }

     
        }


        public void Commit()
        {
            IsTransactional = false;

            try
            {
                if (session != null && session.Transaction != null && session.Transaction.IsActive)
                    session.Transaction.Commit();

            }
            catch (HibernateException e)
            {
                Rollback();
                //throw new Exception(WriteLog.GetTechMessage(e));
                NHibernateHelper.WriteEventLog("Commit: " + WriteLog.GetTechMessage(e));
            }
            finally
            {
                if (session != null && session.IsOpen)
                    session.Close();
            }

        }




        public void Rollback()
        {
            try
            {
                if (session != null && session.Transaction != null && session.Transaction.IsActive)
                    session.Transaction.Rollback();

                //if (session != null && session.IsOpen)
                //    session.Close();
            }
            catch (Exception e)
            {
                Error = e.Message;
                //throw new Exception(WriteLog.GetTechMessage(e));
                NHibernateHelper.WriteEventLog("Rollback: " + WriteLog.GetTechMessage(e));
            }
            finally
            {
                if (session != null && session.IsOpen)
                    session.Close();
            }
        }


        //General
        public DaoAccountType DaoAccountType() { return new DaoAccountType(this); }
        public DaoClassEntity DaoClassEntity() { return new DaoClassEntity(this); }
        public DaoDocumentClass DaoDocumentClass() { return new DaoDocumentClass(this); }
        public DaoDocumentConcept DaoDocumentConcept() { return new DaoDocumentConcept(this); }
        public DaoDocumentType DaoDocumentType() { return new DaoDocumentType(this); }
        public DaoDocumentTypeSequence DaoDocumentTypeSequence() { return new DaoDocumentTypeSequence(this); }
        public DaoGroupCriteria DaoGroupCriteria() { return new DaoGroupCriteria(this); }
        public DaoGroupCriteriaDetail DaoGroupCriteriaDetail() { return new DaoGroupCriteriaDetail(this); }
        public DaoGroupCriteriaRelation DaoGroupCriteriaRelation() { return new DaoGroupCriteriaRelation(this); }
        public DaoGroupCriteriaRelationData DaoGroupCriteriaRelationData() { return new DaoGroupCriteriaRelationData(this); }
        public DaoMeasureType DaoMeasureType() { return new DaoMeasureType(this); }
        public DaoLogError DaoLogError() { return new DaoLogError(this); }
        public DaoMeasureUnit DaoMeasureUnit() { return new DaoMeasureUnit(this); }
        public DaoMeasureUnitConvertion DaoMeasureUnitConvertion() { return new DaoMeasureUnitConvertion(this); }
        public DaoStatus DaoStatus() { return new DaoStatus(this); }
        public DaoStatusType DaoStatusType() { return new DaoStatusType(this); }
        public DaoLabelMapping DaoLabelMapping() { return new DaoLabelMapping(this); }
        public DaoLabelTemplate DaoLabelTemplate() { return new DaoLabelTemplate(this); }
        public DaoConnection DaoConnection() { return new DaoConnection(this); }
        public DaoConnectionType DaoConnectionType() { return new DaoConnectionType(this); }
        public DaoPickMethod DaoPickMethod() { return new DaoPickMethod(this); }
        public DaoDataType DaoDataType() { return new DaoDataType(this); }
        public DaoOptionType DaoOptionType() { return new DaoOptionType(this); }
        public DaoConnectionErpSetup DaoConnectionErpSetup() { return new DaoConnectionErpSetup(this); }

        //Master
        public DaoAccount DaoAccount() { return new DaoAccount(this); }
        public DaoAccountAddress DaoAccountAddress() { return new DaoAccountAddress(this); }
        public DaoAccountTypeRelation DaoAccountTypeRelation() { return new DaoAccountTypeRelation(this); }
        public DaoBin DaoBin() { return new DaoBin(this); }
        public DaoCompany DaoCompany() { return new DaoCompany(this); }
        public DaoContact DaoContact() { return new DaoContact(this); }
        public DaoContactEntityRelation DaoContactEntityRelation() { return new DaoContactEntityRelation(this); }
        public DaoContactPosition DaoContactPosition() { return new DaoContactPosition(this); }
        public DaoLocation DaoLocation() { return new DaoLocation(this); }
        public DaoProduct DaoProduct() { return new DaoProduct(this); }
        public DaoTerminal DaoTerminal() { return new DaoTerminal(this); }
        public DaoUnit DaoUnit() { return new DaoUnit(this); }
        public DaoUnitProductEquivalence DaoUnitProductEquivalence() { return new DaoUnitProductEquivalence(this);}
        public DaoUnitProductLogistic DaoUnitProductLogistic() { return new DaoUnitProductLogistic(this); }
        public DaoUnitProductRelation DaoUnitProductRelation() { return new DaoUnitProductRelation(this); }
        public DaoVehicle DaoVehicle() { return new DaoVehicle(this); }
        public DaoZone DaoZone() { return new DaoZone(this); }
        public DaoZoneBinRelation DaoZoneBinRelation() { return new DaoZoneBinRelation(this); }
        public DaoZonePickerRelation DaoZonePickerRelation() { return new DaoZonePickerRelation(this); }
        public DaoZoneEntityRelation DaoZoneEntityRelation() { return new DaoZoneEntityRelation(this); }
        public DaoShippingMethod DaoShippingMethod() { return new DaoShippingMethod(this); }
        public DaoProductTrackRelation DaoProductTrackRelation() { return new DaoProductTrackRelation(this); }
        public DaoTrackOption DaoTrackOption() { return new DaoTrackOption(this); }
        public DaoProductCategory DaoProductCategory() { return new DaoProductCategory(this); }
        public DaoProductAccountRelation DaoProductAccountRelation() { return new DaoProductAccountRelation(this); }
        public DaoImageEntityRelation DaoImageEntityRelation() { return new DaoImageEntityRelation(this); }
        public DaoKitAssembly DaoKitAssembly() { return new DaoKitAssembly(this); }
        public DaoKitAssemblyFormula DaoKitAssemblyFormula() { return new DaoKitAssemblyFormula(this); }
        public DaoProductAlternate DaoProductAlternate() { return new DaoProductAlternate(this); }
        public DaoContract DaoContract() { return new DaoContract(this); }
        public DaoProductInventory DaoProductInventory() { return new DaoProductInventory(this); }

        //Trace
        public DaoDocument DaoDocument() { return new DaoDocument(this); }
        public DaoDocumentAddress DaoDocumentAddress() { return new DaoDocumentAddress(this); }
        //public DaoDocumentHistory DaoDocumentHistory() { return new DaoDocumentHistory(this); }
        public DaoDocumentLine DaoDocumentLine() { return new DaoDocumentLine(this); }
        //public DaoDocumentLineHistory DaoDocumentLineHistory() { return new DaoDocumentLineHistory(this); }
        public DaoLabel DaoLabel() { return new DaoLabel(this); }
        //public DaoLabelHistory DaoLabelHistory() { return new DaoLabelHistory(this); }
        public DaoNode DaoNode() { return new DaoNode(this); }
        public DaoNodeDocumentType DaoNodeDocumentType() { return new DaoNodeDocumentType(this); }
        public DaoNodeExtension DaoNodeExtension() { return new DaoNodeExtension(this); }
        public DaoNodeExtensionTrace DaoNodeExtensionTrace() { return new DaoNodeExtensionTrace(this); }
        public DaoNodeRoute DaoNodeRoute() { return new DaoNodeRoute(this); }
        public DaoNodeTrace DaoNodeTrace() { return new DaoNodeTrace(this); }
        //public DaoNodeTraceHistory DaoNodeTraceHistory() { return new DaoNodeTraceHistory(this); }
        public DaoTaskDocumentRelation DaoTaskDocumentRelation() { return new DaoTaskDocumentRelation(this); }
        public DaoDocumentBalance DaoDocumentBalance() { return new DaoDocumentBalance(this); }
        public DaoTaskByUser DaoTaskByUser() { return new DaoTaskByUser(this); }
        public DaoLabelTrackOption DaoLabelTrackOption() { return new DaoLabelTrackOption(this); }
        public DaoLabelMissingComponent DaoLabelMissingComponent() { return new DaoLabelMissingComponent(this); }
        public DaoDocumentPackage DaoDocumentPackage() { return new DaoDocumentPackage(this); }
        public DaoBinByTask DaoBinByTask() { return new DaoBinByTask(this); }
        public DaoBinByTaskExecution DaoBinByTaskExecution() { return new DaoBinByTaskExecution(this); }
        public DaoCountSchedule DaoCountSchedule() { return new DaoCountSchedule(this); }
        public DaoEntityExtraData DaoEntityExtraData() { return new DaoEntityExtraData(this); }


        //Profile
        public DaoRol DaoRol() { return new DaoRol(this); }
        public DaoSysUser DaoSysUser() { return new DaoSysUser(this); }
        public DaoUserTransactionLog DaoUserTransactionLog() { return new DaoUserTransactionLog(this); }
        public DaoConfigOption DaoConfigOption() { return new DaoConfigOption(this); }
        public DaoConfigType DaoConfigType() { return new DaoConfigType(this); }
        public DaoMenuOption DaoMenuOption() { return new DaoMenuOption(this); }
        public DaoMenuOptionByRol DaoMenuOptionByRol() { return new DaoMenuOptionByRol(this); }
        public DaoMenuOptionType DaoMenuOptionType() { return new DaoMenuOptionType(this); }
        public DaoUserByRol DaoUserByRol() { return new DaoUserByRol(this); }
        public DaoConfigOptionByCompany DaoConfigOptionByCompany() { return new DaoConfigOptionByCompany(this); }
        public DaoMenuOptionExtension DaoMenuOptionExtension() { return new DaoMenuOptionExtension(this); }
        

        //Report
        //public DaoReportDocument DaoReportDocument() { return new DaoReportDocument(this); }
        public DaoMessagePool DaoMessagePool() { return new DaoMessagePool(this); }
        public DaoMessageRuleByCompany DaoMessageRuleByCompany() { return new DaoMessageRuleByCompany(this); }
        public DaoMessageRuleExtension DaoMessageRuleExtension() { return new DaoMessageRuleExtension(this); }

        //Inquiry
        //public DaoInquiryReport DaoInquiryReport() { return new DaoInquiryReport(this); }
        //public DaoInquiryTableColumn DaoInquiryTableColumn() { return new DaoInquiryTableColumn(this); }
        //public DaoInquiryType DaoInquiryType() { return new DaoInquiryType(this); }
        //public DaoInquiryReportColumn DaoInquiryTypeReportColumn() { return new DaoInquiryReportColumn(this); }
        //public DaoInquiryReportTableRelation DaoInquiryReportTableRelation() { return new DaoInquiryReportTableRelation(this); }
        public DaoIqColumn DaoIqColumn() { return new DaoIqColumn(this); }
        public DaoIqQueryParameter DaoIqQueryParameter() { return new DaoIqQueryParameter(this); }
        public DaoIqReport DaoIqReport() { return new DaoIqReport(this); }
        public DaoIqReportColumn DaoIqReportColumn() { return new DaoIqReportColumn(this); }

        public DaoIqReportTable DaoIqReportTable() { return new DaoIqReportTable(this); }
        public DaoIqTable DaoIqTable() { return new DaoIqTable(this); }



        //Process
        public DaoCustomProcess DaoCustomProcess() { return new DaoCustomProcess(this); }
        public DaoCustomProcessActivity DaoCustomProcessActivity() { return new DaoCustomProcessActivity(this); }
        public DaoCustomProcessContext DaoCustomProcessContext() { return new DaoCustomProcessContext(this); }
        public DaoCustomProcessContextByEntity DaoCustomProcessContextByEntity() { return new DaoCustomProcessContextByEntity(this); }
        public DaoCustomProcessTransition DaoCustomProcessTransition() { return new DaoCustomProcessTransition(this); }
        public DaoCustomProcessTransitionByEntity DaoCustomProcessTransitionByEntity() { return new DaoCustomProcessTransitionByEntity(this); }
        public DaoCustomProcessRoute DaoCustomProcessRoute() { return new DaoCustomProcessRoute(this); }
        public DaoProcessEntityResource DaoProcessEntityResource() { return new DaoProcessEntityResource(this); }

        //Object
        public DaoObject DaoObject() { return new DaoObject(this); }

        //CUSTOM
        public DaoC_CasNumber DaoC_CasNumber() { return new DaoC_CasNumber(this); }
        public DaoC_CasNumberFormula DaoC_CasNumberFormula() { return new DaoC_CasNumberFormula(this); }
        public DaoMType DaoMType() { return new DaoMType(this); }
        public DaoMMaster DaoMMaster() { return new DaoMMaster(this); }
        public DaoC_CasNumberRule DaoC_CasNumberRule() { return new DaoC_CasNumberRule(this); }

        //Workflow
        public DaoBinRoute DaoBinRoute() { return new DaoBinRoute(this); }
        public DaoDataDefinition DaoDataDefinition() { return new DaoDataDefinition(this); }
        public DaoDataInformation DaoDataInformation() { return new DaoDataInformation(this); }
        public DaoDataDefinitionByBin DaoDataDefinitionByBin() { return new DaoDataDefinitionByBin(this); }
        public DaoWFDataType DaoWFDataType() { return new DaoWFDataType(this); }

    }
}
