using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Entities;
using Entities.General;
using Entities.Master;
using Entities.Profile;
using Entities.Report;
using Entities.Trace;
using BizzLogic.Logic;
using System.Threading;
using Entities.Process;
using System.IO;
using System.Reflection;
using Integrator.Dao;
using Integrator;
using System.Collections.Specialized;
using Entities.Workflow;




namespace Facade
{
    public class Control
    {
        //Properties
        private DaoFactory Factory { get; set; }
        private DocumentMngr DocMngr { get; set; }
        private LabelMngr LabelMngr { get; set; }
        private TransactionMngr TranMngr { get; set; }
        private ErpDataMngr ErpMngr { get; set; }
        private ReportMngr RptMngr { get; set; }
        private BasicMngr BasicMngr { get; set; }
        private MessageMngr MsgMngr { get; set; }
        private WmsTypes WType { get; set; }


        //Constructor
        public Control()
        {
            Factory = new DaoFactory();
            DocMngr = new DocumentMngr();
            LabelMngr = new LabelMngr();
            TranMngr = new TransactionMngr();
            ErpMngr = new ErpDataMngr();
            RptMngr = new ReportMngr();
            BasicMngr = new BasicMngr();
            MsgMngr = new MessageMngr();
            WType = new WmsTypes();
        }




        #region ------ Basic/Master Manager  -------


        public string GetCompanyOption(Company company, string code)
        {
            return BasicMngr.GetCompanyOption(company, code);
        }

        public IList<Unit> GetProductUnit(Product data)
        { return BasicMngr.GetProductUnit(data); }

        public IList<Account> GetVendorAccount(Account data)
        { return Factory.DaoAccountTypeRelation().GetAccount(data, AccntType.Vendor)
            .Take(WmsSetupValues.NumRegs).ToList(); }

        public IList<Account> GetCustomerAccount(Account data)
        {
            return Factory.DaoAccountTypeRelation().GetAccount(data, AccntType.Customer)
            .Take(WmsSetupValues.NumRegs).ToList();
        }

        public DocumentTypeSequence GetNextDocSequence(Company company, DocumentType docType)
        {
            return BasicMngr.GetNextDocSequence(company, docType);
        }


        public void AssignBinToProduct(Product product, ZoneBinRelation zoneBin)
        {
            BasicMngr.AssignBinToProduct(product, zoneBin);
        }


        public IList<Label> CreateProductLabels(Unit logisticUnit, DocumentLine data, Node node, Bin destLocation,
            double logisticFactor, string printingLot, DateTime receivingDate )
        {
            return BasicMngr.CreateProductLabels(logisticUnit, data, node, destLocation, logisticFactor, printingLot, "", receivingDate);
        }



        public IList<Product> GetProduct(Product data)
        {
            data.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            return Factory.DaoProduct().Select(data,0).ToList();
        }

        public IList<Product> GetProductApp(Product data, int showRecords)
        {
            data.Status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            return Factory.DaoProduct().Select(data, showRecords).ToList();
        }


        public Product SaveProduct(Product data) { return Factory.DaoProduct().Save(data); }
        public void UpdateProduct(Product data) { Factory.DaoProduct().Update(data); }
        public void DeleteProduct(Product data) { Factory.DaoProduct().Delete(data); }


        public IList<TrackOption> GetTrackOption(TrackOption data) { return Factory.DaoTrackOption().Select(data); }
        public TrackOption SaveTrackOption(TrackOption data) { return Factory.DaoTrackOption().Save(data); }
        public void DeleteTrackOption(TrackOption data) { Factory.DaoTrackOption().Delete(data); }
        public void UpdateTrackOption(TrackOption data) { Factory.DaoTrackOption().Update(data); }


        public IList<ProductTrackRelation> GetProductTrackRelation(ProductTrackRelation data) { return Factory.DaoProductTrackRelation().Select(data); }
        public ProductTrackRelation SaveProductTrackRelation(ProductTrackRelation data) { return Factory.DaoProductTrackRelation().Save(data); }
        public void DeleteProductTrackRelation(ProductTrackRelation data) { Factory.DaoProductTrackRelation().Delete(data); }
        public void UpdateProductTrackRelation(ProductTrackRelation data) { Factory.DaoProductTrackRelation().Update(data); }


        public IList<Account> GetAccount(Account data) { return Factory.DaoAccount().Select(data); }
        public Account SaveAccount(Account data) { return Factory.DaoAccount().Save(data); }
        public void UpdateAccount(Account data) { Factory.DaoAccount().Update(data); }
        public void DeleteAccount(Account data) { Factory.DaoAccount().Delete(data); }


        public IList<AccountAddress> GetAccountAddress(AccountAddress data) { return Factory.DaoAccountAddress().Select(data); }
        
        public AccountAddress SaveAccountAddress(AccountAddress data) {
            return BasicMngr.SaveUpdateAccountAddres(data, 1); //1 Save
        }
        public void UpdateAccountAddress(AccountAddress data)
        {
             BasicMngr.SaveUpdateAccountAddres(data, 2); //2 Update
        }
        public void DeleteAccountAddress(AccountAddress data) { Factory.DaoAccountAddress().Delete(data); }


        public IList<AccountTypeRelation> GetAccountTypeRelation(AccountTypeRelation data) { return Factory.DaoAccountTypeRelation().Select(data); }
        public AccountTypeRelation SaveAccountTypeRelation(AccountTypeRelation data) { return Factory.DaoAccountTypeRelation().Save(data); }
        public void UpdateAccountTypeRelation(AccountTypeRelation data) { Factory.DaoAccountTypeRelation().Update(data); }
        public void DeleteAccountTypeRelation(AccountTypeRelation data) { Factory.DaoAccountTypeRelation().Delete(data); }


        public IList<Bin> GetBin(Bin data) { return Factory.DaoBin().Select(data); }
        public Bin SaveBin(Bin data) { return Factory.DaoBin().Save(data); }
        public void UpdateBin(Bin data) { Factory.DaoBin().Update(data); }
        public void DeleteBin(Bin data) { Factory.DaoBin().Delete(data); }


        public IList<Company> GetCompany(Company data) { return Factory.DaoCompany().Select(data); }
        public Company SaveCompany(Company data) { return Factory.DaoCompany().Save(data); }
        public void UpdateCompany(Company data) { Factory.DaoCompany().Update(data); }
        public void DeleteCompany(Company data) { Factory.DaoCompany().Delete(data); }


        public IList<C_CasNumber> GetC_CasNumber(C_CasNumber data) { return Factory.DaoC_CasNumber().Select(data); }
        public C_CasNumber SaveC_CasNumber(C_CasNumber data) { return Factory.DaoC_CasNumber().Save(data); }
        public void UpdateC_CasNumber(C_CasNumber data) { Factory.DaoC_CasNumber().Update(data); }
        public void DeleteC_CasNumber(C_CasNumber data) { Factory.DaoC_CasNumber().Delete(data); }

        public IList<C_CasNumberFormula> GetC_CasNumberFormula(C_CasNumberFormula data) { return Factory.DaoC_CasNumberFormula().Select(data); }
        public C_CasNumberFormula SaveC_CasNumberFormula(C_CasNumberFormula data) { return Factory.DaoC_CasNumberFormula().Save(data); }
        public void UpdateC_CasNumberFormula(C_CasNumberFormula data) { Factory.DaoC_CasNumberFormula().Update(data); }
        public void DeleteC_CasNumberFormula(C_CasNumberFormula data) { Factory.DaoC_CasNumberFormula().Delete(data); }

        public IList<C_CasNumberRule> GetC_CasNumberRule(C_CasNumberRule data) { return BasicMngr.GetCasNumberRule(data); }
        public C_CasNumberRule SaveC_CasNumberRule(C_CasNumberRule data) { return Factory.DaoC_CasNumberRule().Save(data); }
        public void UpdateC_CasNumberRule(C_CasNumberRule data) { Factory.DaoC_CasNumberRule().Update(data); }
        public void DeleteC_CasNumberRule(C_CasNumberRule data) { Factory.DaoC_CasNumberRule().Delete(data); }



        public IList<Contact> GetContact(Contact data) { return Factory.DaoContact().Select(data); }
        public Contact SaveContact(Contact data) { return Factory.DaoContact().Save(data); }
        public void UpdateContact(Contact data) { Factory.DaoContact().Update(data); }
        public void DeleteContact(Contact data) { Factory.DaoContact().Delete(data); }


        public IList<ContactEntityRelation> GetContactEntityRelation(ContactEntityRelation data) { return Factory.DaoContactEntityRelation().Select(data); }
        public ContactEntityRelation SaveContactEntityRelation(ContactEntityRelation data) { return Factory.DaoContactEntityRelation().Save(data); }
        public void UpdateContactEntityRelation(ContactEntityRelation data) { Factory.DaoContactEntityRelation().Update(data); }
        public void DeleteContactEntityRelation(ContactEntityRelation data) { Factory.DaoContactEntityRelation().Delete(data); }


        public IList<ContactPosition> GetContactPosition(ContactPosition data) { return Factory.DaoContactPosition().Select(data); }
        public ContactPosition SaveContactPosition(ContactPosition data) { return Factory.DaoContactPosition().Save(data); }
        public void UpdateContactPosition(ContactPosition data) { Factory.DaoContactPosition().Update(data); }
        public void DeleteContactPosition(ContactPosition data) { Factory.DaoContactPosition().Delete(data); }


        public IList<Location> GetLocation(Location data) { return Factory.DaoLocation().Select(data); }
        public Location SaveLocation(Location data) { return Factory.DaoLocation().Save(data); }
        public void UpdateLocation(Location data) { Factory.DaoLocation().Update(data); }
        public void DeleteLocation(Location data) { Factory.DaoLocation().Delete(data); }


        public IList<ShippingMethod> GetShippingMethod(ShippingMethod data) { return Factory.DaoShippingMethod().Select(data); }
        public ShippingMethod SaveShippingMethod(ShippingMethod data) { return Factory.DaoShippingMethod().Save(data); }
        public void UpdateShippingMethod(ShippingMethod data) { Factory.DaoShippingMethod().Update(data); }
        public void DeleteShippingMethod(ShippingMethod data) { Factory.DaoShippingMethod().Delete(data); }


        public IList<Terminal> GetTerminal(Terminal data) { return Factory.DaoTerminal().Select(data); }
        public Terminal SaveTerminal(Terminal data) { return Factory.DaoTerminal().Save(data); }
        public void UpdateTerminal(Terminal data) { Factory.DaoTerminal().Update(data); }
        public void DeleteTerminal(Terminal data) { Factory.DaoTerminal().Delete(data); }


        public IList<Unit> GetUnit(Unit data) { return Factory.DaoUnit().Select(data); }
        public Unit SaveUnit(Unit data) { return Factory.DaoUnit().Save(data); }
        public void UpdateUnit(Unit data) { Factory.DaoUnit().Update(data); }
        public void DeleteUnit(Unit data) { Factory.DaoUnit().Delete(data); }


        public IList<UnitProductEquivalence> GetUnitProductEquivalence(UnitProductEquivalence data) { return Factory.DaoUnitProductEquivalence().Select(data); }
        public UnitProductEquivalence SaveUnitProductEquivalence(UnitProductEquivalence data) { return Factory.DaoUnitProductEquivalence().Save(data); }
        public void UpdateUnitProductEquivalence(UnitProductEquivalence data) { Factory.DaoUnitProductEquivalence().Update(data); }
        public void DeleteUnitProductEquivalence(UnitProductEquivalence data) { Factory.DaoUnitProductEquivalence().Delete(data); }


        public IList<UnitProductLogistic> GetUnitProductLogistic(UnitProductLogistic data) { return Factory.DaoUnitProductLogistic().Select(data); }
        public UnitProductLogistic SaveUnitProductLogistic(UnitProductLogistic data) { return Factory.DaoUnitProductLogistic().Save(data); }
        public void UpdateUnitProductLogistic(UnitProductLogistic data) { Factory.DaoUnitProductLogistic().Update(data); }
        public void DeleteUnitProductLogistic(UnitProductLogistic data) { Factory.DaoUnitProductLogistic().Delete(data); }


        public IList<UnitProductRelation> GetUnitProductRelation(UnitProductRelation data) { return Factory.DaoUnitProductRelation().Select(data); }
        public UnitProductRelation SaveUnitProductRelation(UnitProductRelation data) { return Factory.DaoUnitProductRelation().Save(data); }
        public void UpdateUnitProductRelation(UnitProductRelation data) { Factory.DaoUnitProductRelation().Update(data); }
        public void DeleteUnitProductRelation(UnitProductRelation data) { Factory.DaoUnitProductRelation().Delete(data); }


        public IList<Vehicle> GetVehicle(Vehicle data) { return Factory.DaoVehicle().Select(data); }
        public Vehicle SaveVehicle(Vehicle data) { return Factory.DaoVehicle().Save(data); }
        public void UpdateVehicle(Vehicle data) { Factory.DaoVehicle().Update(data); }
        public void DeleteVehicle(Vehicle data) { Factory.DaoVehicle().Delete(data); }


        public IList<Zone> GetZone(Zone data) { return Factory.DaoZone().Select(data); }
        public Zone SaveZone(Zone data) { return Factory.DaoZone().Save(data); }
        public void UpdateZone(Zone data) { Factory.DaoZone().Update(data); }
        public void DeleteZone(Zone data) { Factory.DaoZone().Delete(data); }



        public IList<ZoneBinRelation> GetZoneBinRelation(ZoneBinRelation data) { return Factory.DaoZoneBinRelation().Select(data); }
        public ZoneBinRelation SaveZoneBinRelation(ZoneBinRelation data) { return Factory.DaoZoneBinRelation().Save(data); }
        public void UpdateZoneBinRelation(ZoneBinRelation data) { Factory.DaoZoneBinRelation().Update(data); }
        public void DeleteZoneBinRelation(ZoneBinRelation data) { Factory.DaoZoneBinRelation().Delete(data); }

        public IList<ZonePickerRelation> GetZonePickerRelation(ZonePickerRelation data) { return Factory.DaoZonePickerRelation().Select(data); }
        public ZonePickerRelation SaveZonePickerRelation(ZonePickerRelation data) { return Factory.DaoZonePickerRelation().Save(data); }
        public void UpdateZonePickerRelation(ZonePickerRelation data) { Factory.DaoZonePickerRelation().Update(data); }
        public void DeleteZonePickerRelation(ZonePickerRelation data) { Factory.DaoZoneBinRelation().Delete(data); }

        public IList<ZoneEntityRelation> GetZoneEntityRelation(ZoneEntityRelation data) { return Factory.DaoZoneEntityRelation().Select(data); }
        public ZoneEntityRelation SaveZoneEntityRelation(ZoneEntityRelation data) { return Factory.DaoZoneEntityRelation().Save(data); }
        public void UpdateZoneEntityRelation(ZoneEntityRelation data) { Factory.DaoZoneEntityRelation().Update(data); }
        public void DeleteZoneEntityRelation(ZoneEntityRelation data) { Factory.DaoZoneEntityRelation().Delete(data); }


        public IList<AccountType> GetAccountType(AccountType data) { return Factory.DaoAccountType().Select(data); }
        public AccountType SaveAccountType(AccountType data) { return Factory.DaoAccountType().Save(data); }
        public void UpdateAccountType(AccountType data) { Factory.DaoAccountType().Update(data); }
        public void DeleteAccountType(AccountType data) { Factory.DaoAccountType().Delete(data); }


        public IList<ClassEntity> GetClassEntity(ClassEntity data) { return Factory.DaoClassEntity().Select(data); }
        public ClassEntity SaveClassEntity(ClassEntity data) { return Factory.DaoClassEntity().Save(data); }
        public void UpdateClassEntity(ClassEntity data) { Factory.DaoClassEntity().Update(data); }
        public void DeleteClassEntity(ClassEntity data) { Factory.DaoClassEntity().Delete(data); }


        public IList<DocumentClass> GetDocumentClass(DocumentClass data) { 
            //Factory.IsTransactional = true;
            IList<DocumentClass> list = Factory.DaoDocumentClass().Select(data);
            
            //foreach (DocumentClass docClass in list)
            //    docClass.Reports.ToList();

            return list; 
        }
        public DocumentClass SaveDocumentClass(DocumentClass data) { return Factory.DaoDocumentClass().Save(data); }
        public void UpdateDocumentClass(DocumentClass data) { Factory.DaoDocumentClass().Update(data); }
        public void DeleteDocumentClass(DocumentClass data) { Factory.DaoDocumentClass().Delete(data); }


        public IList<DocumentConcept> GetDocumentConcept(DocumentConcept data) { return Factory.DaoDocumentConcept().Select(data); }
        public DocumentConcept SaveDocumentConcept(DocumentConcept data) { return Factory.DaoDocumentConcept().Save(data); }
        public void UpdateDocumentConcept(DocumentConcept data) { Factory.DaoDocumentConcept().Update(data); }
        public void DeleteDocumentConcept(DocumentConcept data) { Factory.DaoDocumentConcept().Delete(data); }


        public IList<DocumentType> GetDocumentType(DocumentType data) { return Factory.DaoDocumentType().Select(data); }
        public DocumentType SaveDocumentType(DocumentType data) { return Factory.DaoDocumentType().Save(data); }
        public void UpdateDocumentType(DocumentType data) { Factory.DaoDocumentType().Update(data); }
        public void DeleteDocumentType(DocumentType data) { Factory.DaoDocumentType().Delete(data); }


        public IList<DocumentTypeSequence> GetDocumentTypeSequence(DocumentTypeSequence data) { return Factory.DaoDocumentTypeSequence().Select(data); }
        public DocumentTypeSequence SaveDocumentTypeSequence(DocumentTypeSequence data) { return Factory.DaoDocumentTypeSequence().Save(data); }
        public void UpdateDocumentTypeSequence(DocumentTypeSequence data) { Factory.DaoDocumentTypeSequence().Update(data); }
        public void DeleteDocumentTypeSequence(DocumentTypeSequence data) { Factory.DaoDocumentTypeSequence().Delete(data); }


        public IList<GroupCriteria> GetGroupCriteria(GroupCriteria data) { return Factory.DaoGroupCriteria().Select(data); }
        public GroupCriteria SaveGroupCriteria(GroupCriteria data) { return Factory.DaoGroupCriteria().Save(data); }
        public void UpdateGroupCriteria(GroupCriteria data) { Factory.DaoGroupCriteria().Update(data); }
        public void DeleteGroupCriteria(GroupCriteria data) { Factory.DaoGroupCriteria().Delete(data); }


        public IList<GroupCriteriaDetail> GetGroupCriteriaDetail(GroupCriteriaDetail data) { return Factory.DaoGroupCriteriaDetail().Select(data); }
        public GroupCriteriaDetail SaveGroupCriteriaDetail(GroupCriteriaDetail data) { return Factory.DaoGroupCriteriaDetail().Save(data); }
        public void UpdateGroupCriteriaDetail(GroupCriteriaDetail data) { Factory.DaoGroupCriteriaDetail().Update(data); }
        public void DeleteGroupCriteriaDetail(GroupCriteriaDetail data) { Factory.DaoGroupCriteriaDetail().Delete(data); }


        public IList<GroupCriteriaRelation> GetGroupCriteriaRelation(GroupCriteriaRelation data) { return Factory.DaoGroupCriteriaRelation().Select(data); }
        public GroupCriteriaRelation SaveGroupCriteriaRelation(GroupCriteriaRelation data) { return Factory.DaoGroupCriteriaRelation().Save(data); }
        public void UpdateGroupCriteriaRelation(GroupCriteriaRelation data) { Factory.DaoGroupCriteriaRelation().Update(data); }
        public void DeleteGroupCriteriaRelation(GroupCriteriaRelation data) { Factory.DaoGroupCriteriaRelation().Delete(data); }


        public IList<GroupCriteriaRelationData> GetGroupCriteriaRelationData(GroupCriteriaRelationData data) { return Factory.DaoGroupCriteriaRelationData().Select(data); }
        public GroupCriteriaRelationData SaveGroupCriteriaRelationData(GroupCriteriaRelationData data) { return Factory.DaoGroupCriteriaRelationData().Save(data); }
        public void UpdateGroupCriteriaRelationData(GroupCriteriaRelationData data) { Factory.DaoGroupCriteriaRelationData().Update(data); }
        public void DeleteGroupCriteriaRelationData(GroupCriteriaRelationData data) { Factory.DaoGroupCriteriaRelationData().Delete(data); }


        public IList<LabelMapping> GetLabelMapping(LabelMapping data) { return Factory.DaoLabelMapping().Select(data); }
        public LabelMapping SaveLabelMapping(LabelMapping data) { return Factory.DaoLabelMapping().Save(data); }
        public void UpdateLabelMapping(LabelMapping data) { Factory.DaoLabelMapping().Update(data); }
        public void DeleteLabelMapping(LabelMapping data) { Factory.DaoLabelMapping().Delete(data); }


         public LabelTemplate SaveLabelTemplate(LabelTemplate data) { return Factory.DaoLabelTemplate().Save(data); }
        public void UpdateLabelTemplate(LabelTemplate data) { Factory.DaoLabelTemplate().Update(data); }
        public void DeleteLabelTemplate(LabelTemplate data) { Factory.DaoLabelTemplate().Delete(data); }


        public IList<LogError> GetLogError(LogError data) { return Factory.DaoLogError().Select(data); }
        public LogError SaveLogError(LogError data) { return Factory.DaoLogError().Save(data); }
        public void UpdateLogError(LogError data) { Factory.DaoLogError().Update(data); }
        public void DeleteLogError(LogError data) { Factory.DaoLogError().Delete(data); }


        public IList<MeasureType> GetMeasureType(MeasureType data) { return Factory.DaoMeasureType().Select(data); }
        public MeasureType SaveMeasureType(MeasureType data) { return Factory.DaoMeasureType().Save(data); }
        public void UpdateMeasureType(MeasureType data) { Factory.DaoMeasureType().Update(data); }
        public void DeleteMeasureType(MeasureType data) { Factory.DaoMeasureType().Delete(data); }


        public IList<MeasureUnit> GetMeasureUnit(MeasureUnit data) { return Factory.DaoMeasureUnit().Select(data); }
        public MeasureUnit SaveMeasureUnit(MeasureUnit data) { return Factory.DaoMeasureUnit().Save(data); }
        public void UpdateMeasureUnit(MeasureUnit data) { Factory.DaoMeasureUnit().Update(data); }
        public void DeleteMeasureUnit(MeasureUnit data) { Factory.DaoMeasureUnit().Delete(data); }


        public IList<MeasureUnitConvertion> GetMeasureUnitConvertion(MeasureUnitConvertion data) { return Factory.DaoMeasureUnitConvertion().Select(data); }
        public MeasureUnitConvertion SaveMeasureUnitConvertion(MeasureUnitConvertion data) { return Factory.DaoMeasureUnitConvertion().Save(data); }
        public void UpdateMeasureUnitConvertion(MeasureUnitConvertion data) { Factory.DaoMeasureUnitConvertion().Update(data); }
        public void DeleteMeasureUnitConvertion(MeasureUnitConvertion data) { Factory.DaoMeasureUnitConvertion().Delete(data); }


        public IList<Status> GetStatus(Status data) { return Factory.DaoStatus().Select(data); }
        public Status SaveStatus(Status data) { return Factory.DaoStatus().Save(data); }
        public void UpdateStatus(Status data) { Factory.DaoStatus().Update(data); }
        public void DeleteStatus(Status data) { Factory.DaoStatus().Delete(data); }


        public IList<StatusType> GetStatusType(StatusType data) { return Factory.DaoStatusType().Select(data); }
        public StatusType SaveStatusType(StatusType data) { return Factory.DaoStatusType().Save(data); }
        public void UpdateStatusType(StatusType data) { Factory.DaoStatusType().Update(data); }
        public void DeleteStatusType(StatusType data) { Factory.DaoStatusType().Delete(data); }


        public IList<ProductCategory> GetProductCategory(ProductCategory data) { return Factory.DaoProductCategory().Select(data); }
        public ProductCategory SaveProductCategory(ProductCategory data) { return Factory.DaoProductCategory().Save(data); }
        public void DeleteProductCategory(ProductCategory data) { Factory.DaoProductCategory().Delete(data); }
        public void UpdateProductCategory(ProductCategory data) { Factory.DaoProductCategory().Update(data); }




        public Document SaveDocument(Document data) { return Factory.DaoDocument().Save(data); }
        public void DeleteDocument(Document data) { Factory.DaoDocument().Delete(data); }


        public IList<DocumentAddress> GetDocumentAddress(DocumentAddress data) { return Factory.DaoDocumentAddress().Select(data); }
        public DocumentAddress SaveDocumentAddress(DocumentAddress data) { return Factory.DaoDocumentAddress().Save(data); }
        public void UpdateDocumentAddress(DocumentAddress data) { Factory.DaoDocumentAddress().Update(data); }
        public void DeleteDocumentAddress(DocumentAddress data) { Factory.DaoDocumentAddress().Delete(data); }


        //public IList<DocumentHistory> GetDocumentHistory(DocumentHistory data) { return Factory.DaoDocumentHistory().Select(data); }
        //public DocumentHistory SaveDocumentHistory(DocumentHistory data) { return Factory.DaoDocumentHistory().Save(data); }
        //public void UpdateDocumentHistory(DocumentHistory data) { Factory.DaoDocumentHistory().Update(data); }
        //public void DeleteDocumentHistory(DocumentHistory data) { Factory.DaoDocumentHistory().Delete(data); }


        public DocumentLine SaveDocumentLine(DocumentLine data) {

            //Averigua primero el siguiente document Line de se documento.
            if (data.LineNumber == 0)
                data.LineNumber = Factory.DaoDocument().GetNexDocLineNumber(data.Document);

            return Factory.DaoDocumentLine().Save(data); 

        }
        public void UpdateDocumentLine(DocumentLine data) { Factory.DaoDocumentLine().Update(data); }
        public void DeleteDocumentLine(DocumentLine data) { Factory.DaoDocumentLine().Delete(data); }


        //public IList<DocumentLineHistory> GetDocumentLineHistory(DocumentLineHistory data) { return Factory.DaoDocumentLineHistory().Select(data); }
        //public DocumentLineHistory SaveDocumentLineHistory(DocumentLineHistory data) { return Factory.DaoDocumentLineHistory().Save(data); }
        //public void UpdateDocumentLineHistory(DocumentLineHistory data) { Factory.DaoDocumentLineHistory().Update(data); }
        //public void DeleteDocumentLineHistory(DocumentLineHistory data) { Factory.DaoDocumentLineHistory().Delete(data); }


        public void UpdateLabel(Label data) { Factory.DaoLabel().Update(data); }
        public void DeleteLabel(Label data) { Factory.DaoLabel().Delete(data); }


        //public IList<LabelHistory> GetLabelHistory(LabelHistory data) { return Factory.DaoLabelHistory().Select(data); }
        //public LabelHistory SaveLabelHistory(LabelHistory data) { return Factory.DaoLabelHistory().Save(data); }
        //public void UpdateLabelHistory(LabelHistory data) { Factory.DaoLabelHistory().Update(data); }
        //public void DeleteLabelHistory(LabelHistory data) { Factory.DaoLabelHistory().Delete(data); }


        public IList<Node> GetNode(Node data) { return Factory.DaoNode().Select(data); }
        public Node SaveNode(Node data) { return Factory.DaoNode().Save(data); }
        public void UpdateNode(Node data) { Factory.DaoNode().Update(data); }
        public void DeleteNode(Node data) { Factory.DaoNode().Delete(data); }


        public IList<NodeDocumentType> GetNodeDocumentType(NodeDocumentType data) { return Factory.DaoNodeDocumentType().Select(data); }
        public NodeDocumentType SaveNodeDocumentType(NodeDocumentType data) { return Factory.DaoNodeDocumentType().Save(data); }
        public void UpdateNodeDocumentType(NodeDocumentType data) { Factory.DaoNodeDocumentType().Update(data); }
        public void DeleteNodeDocumentType(NodeDocumentType data) { Factory.DaoNodeDocumentType().Delete(data); }


        public IList<NodeExtension> GetNodeExtension(NodeExtension data) { return Factory.DaoNodeExtension().Select(data); }
        public NodeExtension SaveNodeExtension(NodeExtension data) { return Factory.DaoNodeExtension().Save(data); }
        public void UpdateNodeExtension(NodeExtension data) { Factory.DaoNodeExtension().Update(data); }
        public void DeleteNodeExtension(NodeExtension data) { Factory.DaoNodeExtension().Delete(data); }


        public IList<NodeExtensionTrace> GetNodeExtensionTrace(NodeExtensionTrace data) { return Factory.DaoNodeExtensionTrace().Select(data); }
        public NodeExtensionTrace SaveNodeExtensionTrace(NodeExtensionTrace data) { return Factory.DaoNodeExtensionTrace().Save(data); }
        public void UpdateNodeExtensionTrace(NodeExtensionTrace data) { Factory.DaoNodeExtensionTrace().Update(data); }
        public void DeleteNodeExtensionTrace(NodeExtensionTrace data) { Factory.DaoNodeExtensionTrace().Delete(data); }


        public IList<NodeRoute> GetNodeRoute(NodeRoute data) { return Factory.DaoNodeRoute().Select(data); }
        public NodeRoute SaveNodeRoute(NodeRoute data) { return Factory.DaoNodeRoute().Save(data); }
        public void UpdateNodeRoute(NodeRoute data) { Factory.DaoNodeRoute().Update(data); }
        public void DeleteNodeRoute(NodeRoute data) { Factory.DaoNodeRoute().Delete(data); }


        public IList<NodeTrace> GetNodeTrace(NodeTrace data) { return Factory.DaoNodeTrace().Select(data); }
        public NodeTrace SaveNodeTrace(NodeTrace data) { return Factory.DaoNodeTrace().Save(data); }
        public void UpdateNodeTrace(NodeTrace data) { Factory.DaoNodeTrace().Update(data); }
        public void DeleteNodeTrace(NodeTrace data) { Factory.DaoNodeTrace().Delete(data); }


        //public IList<NodeTraceHistory> GetNodeTraceHistory(NodeTraceHistory data) { return Factory.DaoNodeTraceHistory().Select(data); }
        //public NodeTraceHistory SaveNodeTraceHistory(NodeTraceHistory data) { return Factory.DaoNodeTraceHistory().Save(data); }
        //public void UpdateNodeTraceHistory(NodeTraceHistory data) { Factory.DaoNodeTraceHistory().Update(data); }
        //public void DeleteNodeTraceHistory(NodeTraceHistory data) { Factory.DaoNodeTraceHistory().Delete(data); }


        public IList<TaskDocumentRelation> GetTaskDocumentRelation(TaskDocumentRelation data) { return Factory.DaoTaskDocumentRelation().Select(data); }
        public TaskDocumentRelation SaveTaskDocumentRelation(TaskDocumentRelation data) { return Factory.DaoTaskDocumentRelation().Save(data); }
        public void UpdateTaskDocumentRelation(TaskDocumentRelation data) { Factory.DaoTaskDocumentRelation().Update(data); }
        public void DeleteTaskDocumentRelation(TaskDocumentRelation data) { Factory.DaoTaskDocumentRelation().Delete(data); }


        public IList<Document> GetTaskByUser(TaskByUser data, Location location) 
        {
            return TranMngr.GetTaskByUser(data, location);
        }

        public TaskByUser SaveTaskByUser(TaskByUser data) { return Factory.DaoTaskByUser().Save(data); }
        public void UpdateTaskByUser(TaskByUser data) { Factory.DaoTaskByUser().Update(data); }
        public void DeleteTaskByUser(TaskByUser data) { Factory.DaoTaskByUser().Delete(data); }

        public IList<Connection> GetConnection(Connection data) { return Factory.DaoConnection().Select(data); }
        public Connection SaveConnection(Connection data) { return Factory.DaoConnection().Save(data); }
        public void UpdateConnection(Connection data) {  Factory.DaoConnection().Update(data); }
        public void DeleteConnection(Connection data) { Factory.DaoConnection().Delete(data); }


        public IList<ProductAccountRelation> GetProductAccountRelation(ProductAccountRelation data) { return Factory.DaoProductAccountRelation().Select(data); }
        public ProductAccountRelation SaveProductAccountRelation(ProductAccountRelation data) { return Factory.DaoProductAccountRelation().Save(data); }
        public void DeleteProductAccountRelation(ProductAccountRelation data) { Factory.DaoProductAccountRelation().Delete(data); }
        public void UpdateProductAccountRelation(ProductAccountRelation data) { Factory.DaoProductAccountRelation().Update(data); }
        public void UpdateIsMainProductAccount(ProductAccountRelation data) { BasicMngr.UpdateIsMainProductAccount(data); }


        public IList<ImageEntityRelation> GetImageEntityRelation(ImageEntityRelation data) { return Factory.DaoImageEntityRelation().Select(data); }
        public ImageEntityRelation SaveImageEntityRelation(ImageEntityRelation data) { return Factory.DaoImageEntityRelation().Save(data); }
        public void DeleteImageEntityRelation(ImageEntityRelation data) { Factory.DaoImageEntityRelation().Delete(data); }
        public void UpdateImageEntityRelation(ImageEntityRelation data) { Factory.DaoImageEntityRelation().Update(data); }

        public IList<PickMethod> GetPickMethod(PickMethod data) { return Factory.DaoPickMethod().Select(data); }
        public PickMethod SavePickMethod(PickMethod data) { return Factory.DaoPickMethod().Save(data); }
        public void DeletePickMethod(PickMethod data) { Factory.DaoPickMethod().Delete(data); }
        public void UpdatePickMethod(PickMethod data) { Factory.DaoPickMethod().Update(data); }


        public IList<LabelTrackOption> GetLabelTrackOption(LabelTrackOption data) { return Factory.DaoLabelTrackOption().Select(data); }
        public LabelTrackOption SaveLabelTrackOption(LabelTrackOption data) { return Factory.DaoLabelTrackOption().Save(data); }
        public void DeleteLabelTrackOption(LabelTrackOption data) { Factory.DaoLabelTrackOption().Delete(data); }
        public void UpdateLabelTrackOption(LabelTrackOption data) { Factory.DaoLabelTrackOption().Update(data); }


        public IList<LabelMissingComponent> GetLabelMissingComponent(LabelMissingComponent data) { return Factory.DaoLabelMissingComponent().Select(data); }
        public LabelMissingComponent SaveLabelMissingComponent(LabelMissingComponent data) { return Factory.DaoLabelMissingComponent().Save(data); }
        public void DeleteLabelMissingComponent(LabelMissingComponent data) { Factory.DaoLabelMissingComponent().Delete(data); }
        public void UpdateLabelMissingComponent(LabelMissingComponent data) { Factory.DaoLabelMissingComponent().Update(data); }


        public IList<MenuOptionExtension> GetMenuOptionExtension(MenuOptionExtension data) { return Factory.DaoMenuOptionExtension().Select(data); }
        public MenuOptionExtension SaveMenuOptionExtension(MenuOptionExtension data) { return Factory.DaoMenuOptionExtension().Save(data); }
        public void DeleteMenuOptionExtension(MenuOptionExtension data) { Factory.DaoMenuOptionExtension().Delete(data); }
        public void UpdateMenuOptionExtension(MenuOptionExtension data) { Factory.DaoMenuOptionExtension().Update(data); }

        public IList<MessageRuleExtension> GetMessageRuleExtension(MessageRuleExtension data) { return Factory.DaoMessageRuleExtension().Select(data); }
        public MessageRuleExtension SaveMessageRuleExtension(MessageRuleExtension data) { return Factory.DaoMessageRuleExtension().Save(data); }
        public void DeleteMessageRuleExtension(MessageRuleExtension data) { Factory.DaoMessageRuleExtension().Delete(data); }
        public void UpdateMessageRuleExtension(MessageRuleExtension data) { Factory.DaoMessageRuleExtension().Update(data); }

        public IList<ProductAlternate> GetProductAlternate(ProductAlternate data) { return Factory.DaoProductAlternate().Select(data); }
        public ProductAlternate SaveProductAlternate(ProductAlternate data) { return Factory.DaoProductAlternate().Save(data); }
        public void DeleteProductAlternate(ProductAlternate data) { Factory.DaoProductAlternate().Delete(data); }
        public void UpdateProductAlternate(ProductAlternate data) { Factory.DaoProductAlternate().Update(data); }


        public IList<DocumentPackage> GetDocumentPackage(DocumentPackage data) { return Factory.DaoDocumentPackage().Select(data); }
        public DocumentPackage SaveDocumentPackage(DocumentPackage data) { return Factory.DaoDocumentPackage().Save(data); }
        public void DeleteDocumentPackage(DocumentPackage data)
        {
            try
            {
                //Revisa que ese package no tenga hijos
                if (data.PackLabel.LabelID > 0)
                {
                    IList<Label> lblList = Factory.DaoLabel()
                        .Select(new Label
                        {
                            LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                            FatherLabel = new Label { LabelID = data.PackLabel.LabelID }
                        });

                    if (lblList == null || lblList.Count == 0)
                        Factory.DaoDocumentPackage().Delete(data);

                }
            }
            catch { }

        }

        public void UpdateDocumentPackage(DocumentPackage data) { Factory.DaoDocumentPackage().Update(data); }


        public IList<OptionType> GetOptionType(OptionType data) { return Factory.DaoOptionType().Select(data); }
        //public OptionType SaveOptionType(OptionType data) { return Factory.DaoOptionType().Save(data); }
        //public void DeleteOptionType(OptionType data) { Factory.DaoOptionType().Delete(data); }
        //public void UpdateOptionType(OptionType data) { Factory.DaoOptionType().Update(data); }


        public IList<ConnectionType> GetConnectionType(ConnectionType data) { return Factory.DaoConnectionType().Select(data); }

        public IList<DataType> GetDataType(DataType data) { return Factory.DaoDataType().Select(data); }


        public IList<Contract> GetContract(Contract data) { return Factory.DaoContract().Select(data); }
        public Contract SaveContract(Contract data) { return Factory.DaoContract().Save(data); }
        public void DeleteContract(Contract data) { Factory.DaoContract().Delete(data); }
        public void UpdateContract(Contract data) { Factory.DaoContract().Update(data); }


        public IList<ProcessEntityResource> GetProcessEntityResource(ProcessEntityResource data) { return Factory.DaoProcessEntityResource().Select(data); }
        public ProcessEntityResource SaveProcessEntityResource(ProcessEntityResource data) { return Factory.DaoProcessEntityResource().Save(data); }
        public void DeleteProcessEntityResource(ProcessEntityResource data) { Factory.DaoProcessEntityResource().Delete(data); }
        public void UpdateProcessEntityResource(ProcessEntityResource data) { Factory.DaoProcessEntityResource().Update(data); }


        public IList<ProductInventory> GetProductInventory(ProductInventory data) { return Factory.DaoProductInventory().Select(data); }
        public ProductInventory SaveProductInventory(ProductInventory data) { return Factory.DaoProductInventory().Save(data); }
        public void DeleteProductInventory(ProductInventory data) { Factory.DaoProductInventory().Delete(data); }
        public void UpdateProductInventory(ProductInventory data) { Factory.DaoProductInventory().Update(data); }
        public IList<ProductInventory> GetProductInventoryByProduct(ProductInventory productInventory, List<int> productList)
        { return Factory.DaoProductInventory().GetProductInventory(productInventory, productList); }

        public void PersistProductInUse(ProductInventory productInventory) { BasicMngr.PersistProductInUse(productInventory); }

        public void ResetQtyInUse(ProductInventory productInventory) { BasicMngr.ResetQtyInUse(productInventory); }

        public IList<ProductStock> GetProductInUseForMerged(List<int> list, Location location)
        {
            return Factory.DaoDocumentLine().GetProductInUseForMerged(list, location);
        }



        public IList<CountSchedule> GetCountSchedule(CountSchedule data) { return Factory.DaoCountSchedule().Select(data); }
        public CountSchedule SaveCountSchedule(CountSchedule data) { return Factory.DaoCountSchedule().Save(data); }
        public void DeleteCountSchedule(CountSchedule data) { Factory.DaoCountSchedule().Delete(data); }
        public void UpdateCountSchedule(CountSchedule data) { Factory.DaoCountSchedule().Update(data); }


        public IList<MType> GetMType(MType data) { return Factory.DaoMType().Select(data); }
        public MType SaveMType(MType data) { return Factory.DaoMType().Save(data); }
        public void UpdateMType(MType data) { Factory.DaoMType().Update(data); }
        public void DeleteMType(MType data) { Factory.DaoMType().Delete(data); }



        public IList<MMaster> GetMMaster(MMaster data) { return Factory.DaoMMaster().Select(data); }
        public MMaster SaveMMaster(MMaster data) { return Factory.DaoMMaster().Save(data); }
        public void UpdateMMaster(MMaster data) { Factory.DaoMMaster().Update(data); }
        public void DeleteMMaster(MMaster data) { Factory.DaoMMaster().Delete(data); }


        #endregion


        #region ------ Document Manager  -------

        //Usado para obtener documentos a procesar, debe pasarse el filtro que se deb manejar
        public IList<Document> GetDocument(Document document)
        {
            return Factory.DaoDocument().Select(document).Take(WmsSetupValues.NumRegs).ToList();
        }


        public Document CreateNewDocument(Document data, Boolean autocommit)
        { return DocMngr.CreateNewDocument(data, autocommit); }


        public Boolean UpdateDocument(Document data)
        { return Factory.DaoDocument().Update(data); }


        public Document CreateNewDocumentTask(IList<Document> docList, Document taskDoc)
        { return DocMngr.CreateNewTaskDocument(docList, taskDoc); }


        public IList<DocumentBalance> GetDocumentBalance(DocumentBalance docBalance, bool isCrossDock)
        { return Factory.DaoDocumentBalance().GeneralBalance(docBalance, isCrossDock); }


        public bool IsOrderBalanceCompleted(DocumentBalance docBal)
        {
            return Factory.DaoDocumentBalance().IsOrderBalanceCompleted(docBal); 
        }


        public IList<Document> GetCrossDockSalesOrders(Document data)
        {
            return DocMngr.GetCrossDockSalesOrders(data);
        }


        //Obtiene las diferencias entre el docuento de recibo y los documentos de despacho asociados al crossdock
        public IList<DocumentBalance> GetCrossDockBalance(DocumentBalance purchaseBalance, IList<Document> salesDocs)
        { return DocMngr.GetCrossDockBalance(purchaseBalance, salesDocs); }


        public Document ConfirmCrossDockProcess(IList<DocumentBalance> crossDockBalance, string user)
        { return DocMngr.ConfirmCrossDockProcess(crossDockBalance, user); }


        public Boolean AssociateUserDocument(Document document, IList<SysUser> userList)
        { DocMngr.AssociateUserDocument(document, userList); return true; }


        public IList<DocumentLine> GetDocumentLine(DocumentLine data)
        { return Factory.DaoDocumentLine().Select(data); }


        public IList<Product> GetDocumentProduct(Document data, Product product)
        { return DocMngr.GetDocumentProduct(data, product); }


        public IList<Unit> GetDocumentUnit(DocumentLine data)
        { return DocMngr.GetDocumentUnit(data); }

        public IList<Document> GetPendingDocument(Document document, int daysAgo, int records)
        { return Factory.DaoDocument().SelectPending(document, daysAgo, records); }

        public IList<DocumentBalance> GetDocumentPostingBalance(DocumentBalance docBalance)
        { return Factory.DaoDocumentBalance().PostingBalance(docBalance); }

        public IList<DocumentBalance> GetDocumentBalanceForEmpty(DocumentBalance docBalance)
        { return Factory.DaoDocumentBalance().DocumentBalanceForEmpty(docBalance); }


        public IList<ProductStock> GetDocumentStock(Document document, bool printed)
        { return Factory.DaoLabel().GetDocumentStock(document, printed); }


        //Get Product Biz Zone Assigned
        public IList<ZoneBinRelation> GetProductAssignedZone(Product product, Location location)
        { return BasicMngr.GetProductAssignedZone(product, location); }

        public String GetAssignedBins(Product product, Location location, short binDirection)
        { return BasicMngr.GetAssignedBins(product, location, binDirection); }

        public String GetAssignedProducts(Bin bin)
        { return BasicMngr.GetAssignedProducts(bin); }

        public IList<ZoneBinRelation> GetAssignedBinsList(Product product, Location location)
        { return BasicMngr.GetAssignedBinsList(product, location); }


        public IList<ProductStock> GetStock(ProductStock data, PickMethod pickMethod, int records)
        {
            return Factory.DaoLabel().GetStock(data, pickMethod, records);
        }

        public object[] GetSuggestedBins(Product product, Location location, PickMethod pickMethod,  short binDirection)
        {
            return BasicMngr.GetSuggestedBins(product, location, pickMethod, binDirection);
        }

        public IList<ProductStock> GetBinStock(ProductStock data)
        {
            return Factory.DaoLabel().GetStock(data);
        }

        //public IList<Account> GetDocumentAccount(Document document, short accountType, bool pending)
        //{
        //    return Factory.DaoDocument().GetDocumentAccount(document, accountType, pending);
        //}

        public IList<ShowData> GetDocumentAccount(Document document, short accountType, bool pending)
        {
            return Factory.DaoDocument().GetDocumentAccount(document, accountType, pending);
        }


        public DocumentLine SaveUpdateDocumentLine(DocumentLine docLine, bool removeLine)
        {
            return DocMngr.SaveUpdateDocumentLine(docLine, removeLine);
        }



        #endregion


        #region ------ Label Manager  -------

        public IList<Label> GetLabel(Label data)
        {
            //Factory.IsTransactional = true;
            return Factory.DaoLabel().Select(data);
        }

        public Label SaveLabel(Label data)
        { return Factory.DaoLabel().Save(data); }


        public IList<DocumentType> GetLabelType()
        {
            Factory.IsTransactional = true;
            return Factory.DaoDocumentType().Select(
                new DocumentType()).Where(f => f.DocClass.DocClassID == SDocClass.Label
              ).ToList();
        }

        public IList<ProductStock> GetProductStock(ProductStock productStock, PickMethod pickMethod)
        { return Factory.DaoLabel().GetStock(productStock, pickMethod, 20); }


        public IList<ProductStock> GetLabelStock(Label fatherLabel)
        { return Factory.DaoLabel().GetStock(fatherLabel); }


        public String GetStrProductStock(ProductStock productStock, PickMethod pickMethod)
        { return LabelMngr.GetStrProductStock(productStock, pickMethod); }



        public IList<Label> GetDocumentLabelAvailable(Document data, Label searchLabel)
        {
            if (data.DocType != null && data.DocType.DocTypeID == SDocType.WarehouseTransferReceipt)
            {
                //Get the posting shipment
                Document shipment;
                try
                {
                    shipment = Factory.DaoDocument().Select(
                        new Document { DocNumber = data.CustPONumber, Company = data.Company }).First();
                }
                catch {
                    return null;   
                }

                return Factory.DaoLabel().GetDocumentLabelAvailableFromTransfer(data, shipment, null);
            }
            else
                return Factory.DaoLabel().GetDocumentLabelAvailable(data, searchLabel); 
        }


        public IList<Label> CreateEmptyLabel(Location location, int numLabels, Product product, string printSession, 
            SysUser user) {
                return LabelMngr.CreateEmptyPackLabel(location, numLabels, product, printSession, user);
        }


        public Double SelectCurrentQty(Label label, Product product, bool includeLabeled)
        {
            return Factory.DaoLabel().SelectCurrentQty(label, product, includeLabeled);
        }
        

        public DocumentPackage CreateNewPackage(Document document, SysUser picker, bool isOpen, DocumentPackage parent, string packageType)
        {
            return TranMngr.CreateNewPackage(document, picker, isOpen, parent, packageType);
        }


        public void MoveQtyBetweenPackages(DocumentPackage curPack, DocumentPackage newPack, 
            Product product, double qty)
        {
            TranMngr.MoveQtyBetweenPackages(curPack, newPack, product, qty);
        }


        public void UpdatePackageMovedLabels(IList<Label> movedLabels)
        {
            TranMngr.UpdatePackageMovedLabels(movedLabels);
        }


        public IList<Label> GetUniqueTrackLabels(Label label)
        {
            return Factory.DaoLabel().GetUniqueTrackLabels(label);
        }


        public void CloseDocumentPackage(DocumentPackage newPack)
        {
            LabelMngr.CloseDocumentPackage(newPack);
        }



        #endregion


        #region ------ Profile Manager  -------

        //public IList<SysUser> GetUser(SysUser data) 
        //    { return Factory.DaoSysUser().Select(data); }

        //LDAP - Autentication
        public SysUser UserAuthentication(SysUser data)
        {
            LDAPAuth ldap = new LDAPAuth();
            return ldap.IsValidUser(data);
        }

        public SysUser UserAuthenticationDevice(SysUser data)
        {
            LDAPAuth ldap = new LDAPAuth();
            return ldap.IsValidUserDevice(data);
        }


        public IList<ShowData> GetDomainList()
        {
            return LDAPAuth.GetDomainList();
        }

        public IList<ShowData> GetCustomersList()
        {
            return LDAPAuth.GetCustomersList();
        }

        public IList<ConfigOption> GetConfigOption(ConfigOption data) { return Factory.DaoConfigOption().Select(data); }
        public ConfigOption SaveConfigOption(ConfigOption data) { return Factory.DaoConfigOption().Save(data); }
        public void UpdateConfigOption(ConfigOption data) { Factory.DaoConfigOption().Update(data); }
        public void DeleteConfigOption(ConfigOption data) { Factory.DaoConfigOption().Delete(data); }


        public IList<ConfigType> GetConfigType(ConfigType data) { return Factory.DaoConfigType().Select(data); }
        public ConfigType SaveConfigType(ConfigType data) { return Factory.DaoConfigType().Save(data); }
        public void UpdateConfigType(ConfigType data) { Factory.DaoConfigType().Update(data); }
        public void DeleteConfigType(ConfigType data) { Factory.DaoConfigType().Delete(data); }


        public IList<MenuOption> GetMenuOption(MenuOption data) { return Factory.DaoMenuOption().Select(data); }
        public MenuOption SaveMenuOption(MenuOption data) { return Factory.DaoMenuOption().Save(data); }
        public void UpdateMenuOption(MenuOption data) { Factory.DaoMenuOption().Update(data); }
        public void DeleteMenuOption(MenuOption data) { Factory.DaoMenuOption().Delete(data); }


        public IList<MenuOptionByRol> GetMenuOptionByRol(MenuOptionByRol data) {
            Factory.IsTransactional = true;
            return Factory.DaoMenuOptionByRol().Select(data); 
        }
        public MenuOptionByRol SaveMenuOptionByRol(MenuOptionByRol data) { return Factory.DaoMenuOptionByRol().Save(data); }
        public void UpdateMenuOptionByRol(MenuOptionByRol data) { Factory.DaoMenuOptionByRol().Update(data); }
        public void DeleteMenuOptionByRol(MenuOptionByRol data) { Factory.DaoMenuOptionByRol().Delete(data); }


        public IList<MenuOptionType> GetMenuOptionType(MenuOptionType data) { return Factory.DaoMenuOptionType().Select(data); }
        public MenuOptionType SaveMenuOptionType(MenuOptionType data) { return Factory.DaoMenuOptionType().Save(data); }
        public void UpdateMenuOptionType(MenuOptionType data) { Factory.DaoMenuOptionType().Update(data); }
        public void DeleteMenuOptionType(MenuOptionType data) { Factory.DaoMenuOptionType().Delete(data); }


        public IList<Rol> GetRol(Rol data) {
            Factory.IsTransactional = true;
            return Factory.DaoRol().Select(data); 
        }
        public Rol SaveRol(Rol data) { return Factory.DaoRol().Save(data); }
        public void UpdateRol(Rol data) { Factory.DaoRol().Update(data); }
        public void DeleteRol(Rol data) { Factory.DaoRol().Delete(data); }


        public IList<SysUser> GetSysUser(SysUser data) { return Factory.DaoSysUser().Select(data); }
        public SysUser SaveSysUser(SysUser data) { return Factory.DaoSysUser().Save(data); }
        public void UpdateSysUser(SysUser data) { Factory.DaoSysUser().Update(data); }
        public void DeleteSysUser(SysUser data) { Factory.DaoSysUser().Delete(data); }


        public IList<UserByRol> GetUserByRol(UserByRol data) {
            Factory.IsTransactional = true;
            return Factory.DaoUserByRol().Select(data);
        }

        public UserByRol SaveUserByRol(UserByRol data) { return Factory.DaoUserByRol().Save(data); }
        public void UpdateUserByRol(UserByRol data) { Factory.DaoUserByRol().Update(data); }
        public void DeleteUserByRol(UserByRol data) { Factory.DaoUserByRol().Delete(data); }


        public IList<UserTransactionLog> GetUserTransactionLog(UserTransactionLog data) { return Factory.DaoUserTransactionLog().Select(data); }
        public UserTransactionLog SaveUserTransactionLog(UserTransactionLog data) { return Factory.DaoUserTransactionLog().Save(data); }
        public void UpdateUserTransactionLog(UserTransactionLog data) { Factory.DaoUserTransactionLog().Update(data); }
        public void DeleteUserTransactionLog(UserTransactionLog data) { Factory.DaoUserTransactionLog().Delete(data); }


        public IList<ConfigOptionByCompany> GetConfigOptionByCompany(ConfigOptionByCompany data) { return Factory.DaoConfigOptionByCompany().Select(data); }
        public ConfigOptionByCompany SaveConfigOptionByCompany(ConfigOptionByCompany data) { return Factory.DaoConfigOptionByCompany().Save(data); }
        public void DeleteConfigOptionByCompany(ConfigOptionByCompany data) { Factory.DaoConfigOptionByCompany().Delete(data); }
        public void UpdateConfigOptionByCompany(ConfigOptionByCompany data) { Factory.DaoConfigOptionByCompany().Update(data); }


        #endregion


        #region ------ Transaction Manager  -------

        public DocumentLine SaveAdjustmentTransaction(DocumentLine data, Label label, bool commitTransaction)
        {
            return TranMngr.SaveAdjustmentTransaction(data, label, commitTransaction);
        }

        public Boolean CheckAdjustmentLine(DocumentLine data, Label label)
        {
            return TranMngr.CheckAdjustmentLine(data, label);
        }


        public Label ChangeLabelUbication(Label labelSource, Label labelDest, string appPath, SysUser user)
        {
            return TranMngr.ChangeLabelUbication(labelSource, labelDest, appPath, user);
        }

        public Label ChangeLabelLocationV2(Label labelSource, Label labelDest, Document document)
        {
            return TranMngr.ChangeLabelLocationV2(labelSource, labelDest, document);
        }

        public DocumentLine ChangeProductUbication(Label labelSource, DocumentLine changeLine, Label labelDest, string appPath)
        {
            return TranMngr.ChangeProductUbication(labelSource, changeLine, labelDest, appPath);
        }

        public Boolean ReceiveProduct(DocumentLine receivingLine, Unit logisticUnit, Bin destLocation, Node recNode)
        {
            TranMngr.ReceiveProduct(receivingLine, logisticUnit, destLocation, recNode);
            return true;
        }

        public void ReceiveLabels(Document document, IList<Label> labels, Bin destLocation, Node recNode, Boolean defaultBin, 
            Location location)
        {
            TranMngr.ReceiveLabels(document, labels, destLocation, recNode, defaultBin, location);
        }


        public Label ReceiveLabel(Document document, Label label, Bin destLocation, Node recNode)
        {
            if (document.DocType != null && document.DocType.DocTypeID == SDocType.WarehouseTransferReceipt && document.IsFromErp != true)
                return TranMngr.ReceiveLabelForTransfer(document, label, destLocation, recNode);

            else
                return TranMngr.ReceiveLabel(document, label, destLocation, recNode);
        }

        public void ReceiptAtOnce(Document document, Bin destLocation, Node recNode)
        {
            if (document.DocType != null && document.DocType.DocTypeID == SDocType.WarehouseTransferReceipt && document.IsFromErp != true)
                TranMngr.ReceiptAtOnceForTransfer(document, destLocation, recNode);

            else
                TranMngr.ReceiptAtOnce(document, destLocation, recNode);
        }
        

        public Boolean PickProduct(DocumentLine line, Label sourceLocation, Node node, Label packageLabel, SysUser picker, Bin binDest) {
            TranMngr.PickProduct(line, sourceLocation, node, packageLabel, picker, binDest);
            return true;
        }

        public Label PickLabel(Document document, Label label, Node node, Label packageLabel, SysUser picker, Bin destBin)
        {
            return TranMngr.PickLabel(document, label, node, packageLabel, picker, destBin);
        }

        public void PickAtOnce(Document document, Label sourceLocation, Node node, SysUser picker) {
            TranMngr.PickAtOnce(document, sourceLocation, node, picker);
        }

        public void PickCrossDockProduct(Document purchase, IList<DocumentBalance> crossDockBalance, SysUser picker)
        {
            TranMngr.PickCrossDockProduct(purchase, crossDockBalance, picker);
        }


        public void ReverseReceiptNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, DocumentType docType)
        {
            TranMngr.ReverseReceiptNodeTraceByLabels(nodeTraceList, user, docType);
        }

        public void ReversePickingNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, Bin restockBin)
        {
            TranMngr.ReversePickingNodeTraceByLabels(nodeTraceList, user, restockBin);
        }



        public void ReverseReceiptNodeTraceByQty(DocumentBalance docBalance, int quantity, SysUser user)
        {
            TranMngr.ReverseReceiptNodeTraceByQty(docBalance, quantity, user);
        }

        public Document ConfirmKitAssemblyOrder(Document data, Location location)
        {
            return TranMngr.ConfirmKitAssemblyOrder(data, location);
        }

        public IList<ProductStock> GetReplanishmentList(ProductStock data, Location location, short selector, bool showEmpty, string bin1, string bin2)
        {
            return TranMngr.GetReplanishmentList(data, location, selector, showEmpty, bin1, bin2);
        }

        public Document CreateReplenishOrder(IList<ProductStock> lines, String user, Location location)
        {
            return TranMngr.CreateReplenishOrder(lines, user, location);
        }


        public Document ConfirmReplenishmentOrder(Document data)
        {
            return TranMngr.ConfirmReplenishmentOrder(data);
        }

        public void ConsolidateBins(Label source, Label destination, string appPath, SysUser user) 
        {
            TranMngr.ConsolidateBins(source,destination,appPath, user);
        }

        public void DepurationTasks(Company company)
        {
            TranMngr.DepurationTasks(company);
        }

        public IList<Label> DecreaseQtyFromBin(Label fromLabel, DocumentLine line, string comment, bool saveTrace, Node node)
        {
            return TranMngr.DecreaseQtyFromBin(fromLabel, line, comment, saveTrace, node);
        }

        public void DecreaseQtyFromLabel(Label fromLabel, DocumentLine line, string comment, bool saveTrace, Node node, bool passToPrint)
        {
            TranMngr.DecreaseQtyFromLabel(fromLabel, line, comment, saveTrace, node, passToPrint);
        }

        public Label IncreaseQtyIntoBin(DocumentLine line, Node node, Bin destBin, string comment, bool saveTrace, DateTime recDate, IList<LabelTrackOption> trackOptions, Label sourceLabel)
        {
            return TranMngr.IncreaseQtyIntoBin(line, node, destBin, comment, saveTrace, recDate, trackOptions, sourceLabel);
        }


        public void IncreaseQtyIntoLabel(Label ubicationLabel, DocumentLine data, string comment, bool saveTrace, Node node)
        {
            TranMngr.IncreaseQtyIntoLabel(ubicationLabel, data, comment, saveTrace, node);
        }



        public string CreateReplaceAdjustmentDocument(Document curDocument, Label sourceLabel, Product product, Product product2, Unit unit, Unit unit2, int qty, SysUser curUser, Location curLocation)
        {
           return TranMngr.CreateReplaceAdjustmentDocument(curDocument, sourceLabel, product, product2, unit, 
               unit2, qty, curUser, curLocation);
        }

        public void ProcessKitAssemblyAddRemove(Label label, int operation, Product Component, int qty, SysUser user, long labelSourceID)
        {
            TranMngr.ProcessKitAssemblyAddRemove(label, operation, Component, qty, user, labelSourceID);
        }



        public IList<CountTaskBalance> GetCountSummary(Document document, bool summary)
        {
            return Factory.DaoBinByTaskExecution().GetCountSummary(document, summary);
        }

        public Document ProcessNoCount(List<ProductStock> listNoCount, string username, bool erp)
        {
            return TranMngr.ProcessNoCount(listNoCount, username, erp);
        }

        public void ProcessNoCountToBin(List<ProductStock> listNoCount, string username, Location location, Bin bin)
        {
            TranMngr.ProcessNoCountToBin(listNoCount, username,location,bin);
        }

        public IList<ProductStock> GetNoCountSummary(Location location)
        {
            return Factory.DaoBinByTaskExecution().GetNoCountSummary(location);
        }




        public void ResetCountedBinTask(BinByTask binTask)
        {
            TranMngr.ResetCountedBinTask(binTask);
        }

        public Document ConfirmCountingTaskDocument(Document countTask, IList<CountTaskBalance> taskList, string user)
        {
            return TranMngr.ConfirmCountingTaskDocument(countTask, taskList, user);
        }


        public void CancelCountingTask(Document countTask, string user)
        {
            TranMngr.CancelCountingTask(countTask, user);
        }

        
        public Label PickProductWithTrack(Document document, Label label, double qtyToPick, Node node, SysUser picker, Label packLabel)
        {
            return TranMngr.PickProductWithTrack(document, label, qtyToPick, node, picker, packLabel);
        }


        public bool ReceiveReturn(Document document, IList<ProductStock> retProduct, SysUser sysUser, double retTotalQty, Node recNode)
        {
            return TranMngr.ReceiveReturn(document, retProduct, sysUser, retTotalQty, recNode);
        }

        public Boolean ReceiveProductAsUnique(DocumentLine line, Bin bin, Node node)
        {
            return TranMngr.ReceiveProductAsUnique(line, bin, node);
        }


        public Label CreateUniqueTrackLabel(Label fatherLabel, string labelCode)
        {
            return TranMngr.CreateUniqueTrackLabel(fatherLabel, labelCode);
        }

        public Label CreateUniqueTrackLabel(Product product, string labelCode, Label destLabel, string user)
        {
            return TranMngr.CreateUniqueTrackLabel(product, labelCode, destLabel, user);
        }


        public void EmptyUniqueTrackLabel(Product product, string labelCode, string user)
        {
            TranMngr.EmptyUniqueTrackLabel(product, labelCode, user);
        }


        public Label UpdateUniqueTrackLabel(Label fatherLabel, string labelCode)
        {
            return TranMngr.UpdateUniqueTrackLabel(fatherLabel, labelCode);
        }


        public Label UpdateLabelTracking(Label label, TrackOption trackOpt, string trackValue, string user)
        {
            return TranMngr.UpdateLabelTracking(label, trackOpt, trackValue, user);
        }


        public void ReceiptAcknowledge(Document document, double numLabels, SysUser sysUser, string appPath)
        {
            TranMngr.ReceiptAcknowledge(document, numLabels, sysUser, appPath);
        }


        public Document CreateMergedDocument(Document document, IList<DocumentLine> dtLines, IList<SysUser> pickers,
            IList<DocumentAddress> addresses)
        {
            return DocMngr.CreateMergedDocument(document, dtLines, pickers, addresses);
        }


        public Document CreateMergedDocumentV2(Document document, IList<DocumentLine> dtLines, IList<SysUser> pickers,
    IList<DocumentAddress> addresses)
        {
            return DocMngr.CreateMergedDocumentV2(document, dtLines, pickers, addresses);
        }


        public Document ConsolidateOrdersInNewDocument(Document document, List<Document> docList)
        {
            return DocMngr.ConsolidateOrdersInNewDocument(document, docList);
        }



        public string CreateMergedDocumentForBackOrders(Document document, IList<DocumentLine> dtLines, IList<SysUser> pickers,
    IList<DocumentAddress> addresses, int process)
        {
            return DocMngr.CreateMergedDocumentForBackOrders(document, dtLines, pickers, addresses, process);
        }


        public void CancelMergerOrder(Document document, DocumentLine docLine)
        {
            DocMngr.CancelMergerOrder(document, docLine);
        }


        public IList<ProductStock> GetDocumentProductStock(Document document, Product product)
        {
            return Factory.DaoDocument().GetDocumentProductStock(document, product);
        }


        public Label PickUniqueLabel(Document document, Node node, Product product, string serialLabel, SysUser picker, Label packLabel)
        {
            return TranMngr.PickUniqueLabel(document, node, product, serialLabel, picker, packLabel);
        }

        public void UnPickUniqueLabel(Document document, Label label, SysUser picker)
        {
            TranMngr.UnPickUniqueLabel(document, label, picker);
        }

        public void ConfirmPicking(Document document, string user)
        {
            TranMngr.ConfirmPicking(document, user);
        }


        #endregion





        #region ------ ErpData Manager  -------

        public void TestConnection(Company data)
        { ErpMngr.TestConnection(data); }

        //References
        public void GetErpAllUnits(Company company)
        { ErpMngr.GetErpAllUnits(company); }

        public void GetErpLocations(Company company)
        { ErpMngr.GetErpAllLocations(company); }

        public void GetErpLocationsSince(Company company, DateTime sinceDate)
        { ErpMngr.GetErpLocationsSince(company, sinceDate); }

        public void GetErpShippingMethods(Company company)
        { ErpMngr.GetErpAllShippingMethods(company); }



        public void GetErpAllProducts(Company company)
        { ErpMngr.GetErpAllProducts(company); }

        public void GetErpProductsLastXDays(Company company, int days)
        { ErpMngr.GetErpProductsLastXDays(company, days); }

        public void GetErpProductById(Company company, string code)
        { ErpMngr.GetErpProductById(company, code); }

        public void GetErpProductsSince(Company company, DateTime sinceDate)
        { ErpMngr.GetErpProductsSince(company, sinceDate); }



        public void GetErpProductCategories(Company company)
        { ErpMngr.GetErpAllCategories(company); }


        public void GetErpProductsByQuery(Company company, string sWhere)
        { ErpMngr.GetErpProductsByQuery(company, sWhere); }


        public void GetErpAllAccounts(Company company)
        { ErpMngr.GetErpAllAccounts(company); }

        public void GetErpAccountsLastXDays(Company company, int days)
        { ErpMngr.GetErpAccountsLastXDays(company, days); }

        public void GetErpAccountById(Company company, string code)
        { ErpMngr.GetErpAccountById(company, code); }


        public void GetErpAccountsSince(Company company, DateTime sinceDate)
        { ErpMngr.GetErpAccountsSince(company, sinceDate); }



        //Documents
        public void GetErpAllReceivingDocuments(Company company)
        { ErpMngr.GetErpAllReceivingDocuments(company); }

        public void GetErpReceivingDocumentsLastXDays(Company company, int days)
        { ErpMngr.GetErpReceivingDocumentsLastXDays(company, days); }

        public void GetErpReceivingDocumentById(Company company, string code)
        { ErpMngr.GetErpReceivingDocumentById(company, code); }

        public void GetErpReceivingDocumentsSince(Company company, DateTime sinceDate)
        { ErpMngr.GetErpReceivingDocumentsSince(company, sinceDate); }


        public void GetErpAllShippingDocuments(Company company)
        { ErpMngr.GetErpAllShippingDocuments(company); }

        public void GetErpShippingDocumentsLastXDays(Company company, int days)
        { ErpMngr.GetErpShippingDocumentsLastXDays(company, days); }

        public void GetErpShippingDocumentById(Company company, string code)
        { ErpMngr.GetErpShippingDocumentById(company, code); }

        public void GetErpShippingDocumentsSince(Company company, DateTime sinceDate)
        { ErpMngr.GetErpShippingDocumentsSince(company, sinceDate); }


        //Transfers

        public void GetErpAllLocationTransferDocuments(Company company)
        {
            ErpMngr.GetErpAllLocationTransferDocuments(company);
        }

        public void GetErpLocationTransferDocumentsSince(Company company, DateTime sinceDate)
        {
            ErpMngr.GetErpLocationTransferDocumentsSince(company, sinceDate);
        }


        //Posting
        public void CreateInventoryAdjustment(Document data, bool reload)
        { ErpMngr.CreateInventoryAdjustment(data, reload); }


        public void ReverseInventoryAdjustment(Document data)
        {
            ErpMngr.ReverseInventoryAdjustment(data);
        }

        public Document CreatePurchaseReceipt(Document data)
        {
            return ErpMngr.CreatePurchaseReceipt(data);            
        }


        public void ReversePurchaseReceipt(Document data)
        {
            TranMngr.ReversePurchaseReceipt(data);
        }

        public void ReverseShipmentDocument(Document data, Bin binRestore)
        {
            ErpMngr.ReverseShipmentDocument(data, binRestore);
        }

        public void UpdateErpPostedProcess(Company company) 
        { 
            //This process mus be used with Thread
            Thread th = new Thread(new ParameterizedThreadStart(UpdatePostedProcessThread));
            th.Start(company);     
        }
        

        public void UpdatePostedProcessThread(object data)
        {
            //Llamado a la funcion de logica (Facade) que ejecuta el proceso
            ErpMngr.UpdatePostedProcess((Company)data);
        }


        public Document CreateShipmentDocument(Document data)
        { return ErpMngr.CreateShipmentDocument(data); }


        public IList<ProductStock> GetStockComparation(ProductStock data, bool detailed, Company company)
        {
                return ErpMngr.GetStockComparation(data, company);
        }


        public void UpdateWMSStockFromERP(Company company, bool detailed)
        {
            ErpMngr.UpdateWMSStockFromERP(company, detailed);
        }


        public String CreateSalesOrder(Document document, string docPrefix, string batch)
        {
            return ErpMngr.CreateSalesOrder(document, docPrefix,  batch);
        }

        public String CreateCustomer(Account customer)
        {
            return ErpMngr.CreateCustomer(customer);
        }

        public String CreateCustomerAddress(AccountAddress address)
        {
            return ErpMngr.CreateCustomerAddress(address);
        }


        public String CreatePurchaseOrder(Document document)
        {
            return ErpMngr.CreatePurchaseOrder(document);
        }

        public void FullfilSalesOrder(Document document)
        {
            ErpMngr.FullfilSalesOrder(document);
        }


        public void ReSendInventoryAdjustmentToERP(Document document)
        {
            ErpMngr.ReSendInventoryAdjustmentToERP(document);
        }



        #endregion


        #region ------ Print Manager -------

        //public String GetReplacedTemplate(IList<DocumentBalance> printList, LabelTemplate template, String printLot, UserByRol userByRol)
        //{
        //    return LabelMngr.GetReplacedTemplate(printList, template, printLot, userByRol);
        //}

        public IList<LabelTemplate> GetLabelTemplate(LabelTemplate data)
        {
            return Factory.DaoLabelTemplate().Select(data);
        }

        ////23 Marzo - 09, Jairo Murillo Print labels already received
        public String GetReplacedTemplateForLabels(IList<Label> labelList, LabelTemplate template, string printLot)
        {
            return LabelMngr.GetReplacedTemplateForLabels(labelList, template, printLot);
        }

        //10 Marzo /09 : Jairo Murillo
        public IList<Label> GenerateLabelsToPrint(IList<DocumentBalance> printList, String printLot, UserByRol userByRol)
        {
            return LabelMngr.GenerateLabelsToPrint(printList, printLot, userByRol);
        }


        public void PrintLabelsFromDevice(string printer, string rdlTemplateName, IList<Label> listOfLabels, string appPath)
        {
            LabelTemplate defTemplate = null;
            Printer objPrinter = null;

            //Si el path viene Empty.
            if (string.IsNullOrEmpty(appPath))
                appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);

            try {

                if (printer.Equals(WmsSetupValues.DEFAULT))
                    objPrinter = new Printer { PrinterName = printer };
                else
                {
                    Connection cnnPrinter = Factory.DaoConnection().Select(new Connection { Name = printer }).First();
                    objPrinter = new Printer { PrinterName = cnnPrinter.Name, PrinterPath = cnnPrinter.CnnString };
                }
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(rdlTemplateName))
                    defTemplate = Factory.DaoLabelTemplate().Select(
                        new LabelTemplate { Header = rdlTemplateName }).First();
            }
            catch { }

            ReportMngr.PrintLabelsFromFacade(objPrinter, defTemplate, listOfLabels, appPath);
        }


        public int PrintPackageLabels(Document shipment, string appPath, string printer, string labelcode)
        {
            if (string.IsNullOrEmpty(appPath))
                appPath = Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location), WmsSetupValues.WebServer);

            if (string.IsNullOrEmpty(printer))
                printer = WmsSetupValues.DEFAULT;

            if (shipment != null || shipment.DocID != 0 || !string.IsNullOrEmpty(labelcode))
                return RptMngr.PrintPackageLabels(shipment, printer, appPath, labelcode);

            else
                return 0;

        }



        public void PrintDocumentsInBatch(IList<Document> documentList, string appPath, string printer, CustomProcess process)
        {
            BasicMngr.PrintDocumentsInBatch(documentList, appPath, printer, process);
        }



        #endregion


        #region ------ Report Manager  -------

        public ReportHeaderFormat GetReportInformation(Document data, string template)
        { return RptMngr.GetReportInformation(data, template); }

        public void ProcessMessageRules()
        {
            MsgMngr.ProcessMessageRules();
        }

        public DataSet GetReportObject(MenuOption option, IList<String> rpParams, Location location)
        {
            return RptMngr.GetReportObject(option, rpParams, location);
        }


        public IList<MessageRuleByCompany> GetMessageRuleByCompany(MessageRuleByCompany data) { return Factory.DaoMessageRuleByCompany().Select(data); }
        public MessageRuleByCompany SaveMessageRuleByCompany(MessageRuleByCompany data) { return Factory.DaoMessageRuleByCompany().Save(data); }
        public void DeleteMessageRuleByCompany(MessageRuleByCompany data) { Factory.DaoMessageRuleByCompany().Delete(data); }
        public void UpdateMessageRuleByCompany(MessageRuleByCompany data) { Factory.DaoMessageRuleByCompany().Update(data); }


        //public IList<ReportDocument> GetReportDocument(ReportDocument data) { return Factory.DaoReportDocument().Select(data); }
        //public ReportDocument SaveReportDocument(ReportDocument data) { return Factory.DaoReportDocument().Save(data); }
        //public void DeleteReportDocument(ReportDocument data) { Factory.DaoReportDocument().Delete(data); }
        //public void UpdateReportDocument(ReportDocument data) { Factory.DaoReportDocument().Update(data); }

        public IList<IqColumn> GetIqColumn(IqColumn data) { return Factory.DaoIqColumn().Select(data); }
        public IqColumn SaveIqColumn(IqColumn data) { return Factory.DaoIqColumn().Save(data); }
        public void DeleteIqColumn(IqColumn data) { Factory.DaoIqColumn().Delete(data); }
        public void UpdateIqColumn(IqColumn data) { Factory.DaoIqColumn().Update(data); }

        public IList<IqReport> GetIqReport(IqReport data) { return Factory.DaoIqReport().Select(data); }
        public IqReport SaveIqReport(IqReport data) { return Factory.DaoIqReport().Save(data); }
        public void DeleteIqReport(IqReport data) { Factory.DaoIqReport().Delete(data); }
        public void UpdateIqReport(IqReport data) { Factory.DaoIqReport().Update(data); }


        public IList<IqReportColumn> GetIqReportColumn(IqReportColumn data) { return Factory.DaoIqReportColumn().Select(data); }
        public IqReportColumn SaveIqReportColumn(IqReportColumn data) { return Factory.DaoIqReportColumn().Save(data); }
        public void DeleteIqReportColumn(IqReportColumn data) { Factory.DaoIqReportColumn().Delete(data); }
        public void UpdateIqReportColumn(IqReportColumn data) { Factory.DaoIqReportColumn().Update(data); }


        //public IList<IqReportSetting> GetIqReportSetting(IqReportSetting data) { return Factory.DaoIqReportSetting().Select(data); }
        //public IqReportSetting SaveIqReportSetting(IqReportSetting data) { return Factory.DaoIqReportSetting().Save(data); }
        //public void DeleteIqReportSetting(IqReportSetting data) { Factory.DaoIqReportSetting().Delete(data); }
        //public void UpdateIqReportSetting(IqReportSetting data) { Factory.DaoIqReportSetting().Update(data); }


        public IList<IqReportTable> GetIqReportTable(IqReportTable data) { return Factory.DaoIqReportTable().Select(data); }
        public IqReportTable SaveIqReportTable(IqReportTable data) { return Factory.DaoIqReportTable().Save(data); }
        public void DeleteIqReportTable(IqReportTable data) { Factory.DaoIqReportTable().Delete(data); }
        public void UpdateIqReportTable(IqReportTable data) { Factory.DaoIqReportTable().Update(data); }


        public IList<IqTable> GetIqTable(IqTable data) { return Factory.DaoIqTable().Select(data); }
        public IqTable SaveIqTable(IqTable data) { return Factory.DaoIqTable().Save(data); }
        public void DeleteIqTable(IqTable data) { Factory.DaoIqTable().Delete(data); }
        public void UpdateIqTable(IqTable data) { Factory.DaoIqTable().Update(data); }


        public IList<BinByTaskExecution> GetBinByTaskExecution(BinByTaskExecution data) { return Factory.DaoBinByTaskExecution().Select(data); }
        public BinByTaskExecution SaveBinByTaskExecution(BinByTaskExecution data) { return Factory.DaoBinByTaskExecution().Save(data); }
        public void DeleteBinByTaskExecution(BinByTaskExecution data) { Factory.DaoBinByTaskExecution().Delete(data); }
        public void UpdateBinByTaskExecution(BinByTaskExecution data) { Factory.DaoBinByTaskExecution().Update(data); }


        public IList<BinByTask> GetBinByTask(BinByTask data) { return Factory.DaoBinByTask().Select(data); }
        public BinByTask SaveBinByTask(BinByTask data) { return Factory.DaoBinByTask().Save(data); }
        public void DeleteBinByTask(BinByTask data) { Factory.DaoBinByTask().Delete(data); }
        public void UpdateBinByTask(BinByTask data) { Factory.DaoBinByTask().Update(data); }


        public IList<EntityExtraData> GetEntityExtraData(EntityExtraData data) { return Factory.DaoEntityExtraData().Select(data); }
        public EntityExtraData SaveEntityExtraData(EntityExtraData data) { return Factory.DaoEntityExtraData().Save(data); }
        public void DeleteEntityExtraData(EntityExtraData data) { Factory.DaoEntityExtraData().Delete(data); }
        public void UpdateEntityExtraData(EntityExtraData data) { Factory.DaoEntityExtraData().Update(data); }


        public DataSet GetIqReportDataSet(string dataQuery,  DataSet rpParams)
        {

            return Factory.DaoIqReport().GetReportObject(dataQuery, rpParams);
        }


        #endregion


        #region ------ Workflow Manager -------

        public IList<BinRoute> GetBinRoute(BinRoute data) { return Factory.DaoBinRoute().Select(data); }
        public BinRoute SaveBinRoute(BinRoute data) { return Factory.DaoBinRoute().Save(data); }
        public void UpdateBinRoute(BinRoute data) { Factory.DaoBinRoute().Update(data); }
        public void DeleteBinRoute(BinRoute data) { Factory.DaoBinRoute().Delete(data); }

        public IList<DataDefinition> GetDataDefinition(DataDefinition data) { return Factory.DaoDataDefinition().Select(data); }
        public DataDefinition SaveDataDefinition(DataDefinition data) { return Factory.DaoDataDefinition().Save(data); }
        public void UpdateDataDefinition(DataDefinition data) { Factory.DaoDataDefinition().Update(data); }
        public void DeleteDataDefinition(DataDefinition data) { Factory.DaoDataDefinition().Delete(data); }

        public IList<DataInformation> GetDataInformation(DataInformation data) { return Factory.DaoDataInformation().Select(data); }
        public DataInformation SaveDataInformation(DataInformation data) { data.ModTerminal = "T"; return Factory.DaoDataInformation().Save(data); }
        public void UpdateDataInformation(DataInformation data) { data.ModTerminal = "T"; Factory.DaoDataInformation().Update(data); }
        public void DeleteDataInformation(DataInformation data) { Factory.DaoDataInformation().Delete(data); }

        public IList<DataDefinitionByBin> GetDataDefinitionByBin(DataDefinitionByBin data) { return Factory.DaoDataDefinitionByBin().Select(data); }
        public DataDefinitionByBin SaveDataDefinitionByBin(DataDefinitionByBin data) { return Factory.DaoDataDefinitionByBin().Save(data); }
        public void UpdateDataDefinitionByBin(DataDefinitionByBin data) { Factory.DaoDataDefinitionByBin().Update(data); }
        public void DeleteDataDefinitionByBin(DataDefinitionByBin data) { Factory.DaoDataDefinitionByBin().Delete(data); }

        public IList<WFDataType> GetWFDataType(WFDataType data) { return Factory.DaoWFDataType().Select(data); }
        public WFDataType SaveWFDataType(WFDataType data) { return Factory.DaoWFDataType().Save(data); }
        public void UpdateWFDataType(WFDataType data) { Factory.DaoWFDataType().Update(data); }
        public void DeleteWFDataType(WFDataType data) { Factory.DaoWFDataType().Delete(data); }

        #endregion


        #region ----------------- KitAssembly ---------------------

        public void GetErpAllKitAssembly(Company company)
        {
            ErpMngr.GetErpAllKitAssembly(company);
        }

        public void GetErpAllKitAssemblyDocuments(Company company)
        {
            ErpMngr.GetErpAllKitAssemblyDocuments(company);
        }

        public void GetErpKitAssemblySince(Company company, DateTime since)
        {
            ErpMngr.GetErpKitAssemblySince(company, since);
        }

        public void GetErpKitAssemblyDocumentsSince(Company company, DateTime since)
        {
            ErpMngr.GetErpKitAssemblyDocumentsSince(company, since);
        }

        public IList<KitAssembly> GetKitAssembly(KitAssembly data, int showRegs) { return Factory.DaoKitAssembly().Select(data, showRegs); }
        public KitAssembly SaveKitAssembly(KitAssembly data) { return Factory.DaoKitAssembly().Save(data); }
        public void DeleteKitAssembly(KitAssembly data) { Factory.DaoKitAssembly().Delete(data); }
        public void UpdateKitAssembly(KitAssembly data) { Factory.DaoKitAssembly().Update(data); }


        public IList<KitAssemblyFormula> GetKitAssemblyFormula(KitAssemblyFormula data) { return Factory.DaoKitAssemblyFormula().Select(data); }
        public KitAssemblyFormula SaveKitAssemblyFormula(KitAssemblyFormula data) { return Factory.DaoKitAssemblyFormula().Save(data); }
        public void DeleteKitAssemblyFormula(KitAssemblyFormula data) { Factory.DaoKitAssemblyFormula().Delete(data); }
        public void UpdateKitAssemblyFormula(KitAssemblyFormula data) { Factory.DaoKitAssemblyFormula().Update(data); }

        //Febrero 15 de 2011
        //Jorge Armando Ortega
        public IList<DocumentLine> CreateAssemblyOrderLines(Document Document, Product Product, Double Quantity) { return TranMngr.CreateAssemblyOrderLines(Document, Product, Quantity); }


        #endregion


        #region ----------------- Process  ---------------------

        public IList<CustomProcess> GetCustomProcess(CustomProcess data) { return Factory.DaoCustomProcess().Select(data); }
        public CustomProcess SaveCustomProcess(CustomProcess data) { return Factory.DaoCustomProcess().Save(data); }
        public void DeleteCustomProcess(CustomProcess data) { Factory.DaoCustomProcess().Delete(data); }
        public void UpdateCustomProcess(CustomProcess data) { Factory.DaoCustomProcess().Update(data); }

        public IList<CustomProcessContext> GetCustomProcessContext(CustomProcessContext data) { return Factory.DaoCustomProcessContext().Select(data); }
        public CustomProcessContext SaveCustomProcessContext(CustomProcessContext data) { return Factory.DaoCustomProcessContext().Save(data); }
        public void DeleteCustomProcessContext(CustomProcessContext data) { Factory.DaoCustomProcessContext().Delete(data); }
        public void UpdateCustomProcessContext(CustomProcessContext data) { Factory.DaoCustomProcessContext().Update(data); }

        public IList<CustomProcessContextByEntity> GetCustomProcessContextByEntity(CustomProcessContextByEntity data) { return Factory.DaoCustomProcessContextByEntity().Select(data); }
        public CustomProcessContextByEntity SaveCustomProcessContextByEntity(CustomProcessContextByEntity data) { return Factory.DaoCustomProcessContextByEntity().Save(data); }
        public void DeleteCustomProcessContextByEntity(CustomProcessContextByEntity data) { Factory.DaoCustomProcessContextByEntity().Delete(data); }
        public void UpdateCustomProcessContextByEntity(CustomProcessContextByEntity data) { Factory.DaoCustomProcessContextByEntity().Update(data); }

        public IList<CustomProcessRoute> GetCustomProcessRoute(CustomProcessRoute data) { return Factory.DaoCustomProcessRoute().Select(data); }
        public CustomProcessRoute SaveCustomProcessRoute(CustomProcessRoute data) { return Factory.DaoCustomProcessRoute().Save(data); }
        public void DeleteCustomProcessRoute(CustomProcessRoute data) { Factory.DaoCustomProcessRoute().Delete(data); }
        public void UpdateCustomProcessRoute(CustomProcessRoute data) { Factory.DaoCustomProcessRoute().Update(data); }


        public IList<CustomProcessActivity> GetCustomProcessActivity(CustomProcessActivity data) { return Factory.DaoCustomProcessActivity().Select(data); }
        public CustomProcessActivity SaveCustomProcessActivity(CustomProcessActivity data) { return Factory.DaoCustomProcessActivity().Save(data); }
        public void DeleteCustomProcessActivity(CustomProcessActivity data) { Factory.DaoCustomProcessActivity().Delete(data); }
        public void UpdateCustomProcessActivity(CustomProcessActivity data) { Factory.DaoCustomProcessActivity().Update(data); }


        public IList<CustomProcessTransition> GetCustomProcessTransition(CustomProcessTransition data) { return Factory.DaoCustomProcessTransition().Select(data); }
        public CustomProcessTransition SaveCustomProcessTransition(CustomProcessTransition data) { return Factory.DaoCustomProcessTransition().Save(data); }
        public void DeleteCustomProcessTransition(CustomProcessTransition data) { Factory.DaoCustomProcessTransition().Delete(data); }
        public void UpdateCustomProcessTransition(CustomProcessTransition data) { Factory.DaoCustomProcessTransition().Update(data); }

        public IList<CustomProcessTransitionByEntity> GetCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data) { return Factory.DaoCustomProcessTransitionByEntity().Select(data); }
        public CustomProcessTransitionByEntity SaveCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data) { return Factory.DaoCustomProcessTransitionByEntity().Save(data); }
        public void DeleteCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data) { Factory.DaoCustomProcessTransitionByEntity().Delete(data); }
        public void UpdateCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data) { Factory.DaoCustomProcessTransitionByEntity().Update(data); }


        public void EvaluateBasicProcess(CustomProcess processName, SysUser user, string message)
        {
            (new ProcessMngr()).EvaluateBasicProcess(processName, user, message);
        }


        public string ProcessFile(CustomProcess process, string stream, SysUser user)
        {
            return (new ProcessMngr()).ProcessFile(process, stream, user);
        }


        #endregion


        //Object

        #region  objects

        public object GetObject(string hql, object fieldID)
        {
            Object obj = Factory.DaoObject().Select(hql, fieldID);

            //if (obj.GetType() == typeof(Label))
                //return (Label)obj;

            return obj;
        }


        public string GetObject(string hql, object fieldID, bool nativeSql)
        {
            Object obj = Factory.DaoObject().SelectSQL(hql, fieldID);

            //if (obj.GetType() == typeof(Label))
            //return (Label)obj;

            return obj == null ? "" : obj.ToString();
        }


        public object SaveObject(object businessObj)
        {
            return Factory.DaoObject().Save(businessObj);
        }

        public object UpdateObject(object businessObj)
        {
            return Factory.DaoObject().Update(businessObj);
        }

        public object DeleteObject(object businessObj)
        {
            return Factory.DaoObject().Delete(businessObj);
        }

        public void DirectSQLNonQuery(string query, Connection connection)
        {
            BasicMngr.DirectSQLNonQuery(query, connection);
        }

        public DataTable DirectSQLQuery(string query, string swhere, string tableName, Connection connection)
        {
            return BasicMngr.DirectSQLQuery(query, swhere, tableName, connection);
        }

        public DataSet DirectSQLQueryDS(string query, string swhere, string tableName, Connection connection)
        {
            return BasicMngr.DirectSQLQueryDS(query, swhere, tableName, connection);
        }

        public IList<ShowData> GetLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
                langCode = Factory.DaoConfigOption().Select(new ConfigOption { Code = "LANGUAGE" }).First().DefValue;

            return Factory.DaoObject().GetLanguage(langCode);
        }

        #endregion


        public void SendProcessNotification(SysUser user, Document document, string process)
        {
            TranMngr.SendProcessNotification(user, document, process);
        }



        public Document EnterInspectionResultByLabel(Document document, Label label, string resultCode, string username)
        {
            return TranMngr.EnterInspectionResultByLabel(document, label, resultCode, username);
        }

        public Document EnterInspectionResultByProduct(Document document, Product product, double qtyToPick, string resultCode, Bin bin, string username)
        {
            return TranMngr.EnterInspectionResultByProduct(document, product, qtyToPick, resultCode,  bin, username);
        }

        public bool IsTrackRequiredInDocument(Document document, Node node)
        {
            return DocMngr.IsTrackRequiredInDocument(document, node);
        }

        public void ConfirmInspectionDocument(Document document, string appPath, string username)
        {
            TranMngr.ConfirmInspectionDocument(document, appPath, username);
        }

        public void ResetInspectionLine(Document document, Label label, string username, bool cancelDocument)
        {
            TranMngr.ResetInspectionLine(document, label, username, cancelDocument);
        }



        public IList<Unit> GetDocumentProductUnit(Document document, Product product)
        {
            return BasicMngr.GetDocumentProductUnit(document, product);
            //return GetProductUnit(product);
        }

        public IList<ShowData> GetCustomList(string databaseObject, string field, string strWhere)
        {
            return Factory.DaoObject().GetCustomList(databaseObject, field, strWhere);
        }


        public IList<ShowData> GetCustomListV2(string sqlQuery)
        {
            return Factory.DaoObject().GetCustomListV2(sqlQuery);
        }


        #region PUBLIC RULES

        public Boolean ValidateReceiptDocumentTrackingOptions(Document data, Node node, Boolean autoThrow)
        {
            return BasicMngr.ValidateReceiptDocumentTrackingOptions(data, node, autoThrow);
        }

        #endregion


        public void PrintKitAssemblyLabels(Document ssDocument, int qtyType)
        {
            ssDocument.DocumentLines = Factory.DaoDocumentLine().Select(
                new DocumentLine { Document = new Document { DocID = ssDocument.DocID } });

            // el documento debe tener lineas 
            if (ssDocument.DocumentLines == null || ssDocument.DocumentLines.Count == 0)
                return;
            else
                BasicMngr.PrintKitAssemblyLabels(ssDocument, qtyType);
        }



        public void CreateLocationTransfer(Document trfDoc)
        {
            ErpMngr.CreateLocationTransfer(trfDoc);
        }

        public void RunDataBaseRoutines(Company company)
        {
            WorkFlowRuntime.DataBaseRoutines();
        }
    }
}
