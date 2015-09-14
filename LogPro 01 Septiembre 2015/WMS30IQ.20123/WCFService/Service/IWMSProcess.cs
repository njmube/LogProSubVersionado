using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.ServiceModel;
using Entities.Master;
using Entities.General;
using Entities.Trace;
using Entities.Profile;
using Integrator;
using WcfExtensions;
using Entities.Report;
using System.Data;
using Entities.Process;
using System.Collections.Specialized;
using Entities.Workflow;

namespace WcfService
{
    //, CallbackContract = typeof(IWMSProcessCallback) - Esto hace que el servicio sea Dual
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ExceptionMarshallingBehavior]
    //[DataContractFormat(Style = OperationFormatStyle.Rpc)]
    [UseNetDataContractSerializer]

    interface IWMSProcess
    {

        //[OperationContract]
        //string MyOperation1(string arg1, int arg2);

        #region //------ Custom Operations  -------


        [OperationContract]
        IList<Account> GetVendorAccount(Account data);

        [OperationContract]
        IList<Account> GetCustomerAccount(Account data);

        [OperationContract]
        IList<Unit> GetProductUnit(Product data);

        [OperationContract]
        IList<ZoneBinRelation> GetProductAssignedZone(Product product, Location location);

        [OperationContract]
        void AssignBinToProduct(Product product, ZoneBinRelation zoneBin);

        [OperationContract]
        IList<ProductStock> GetProductStock(ProductStock productStock, PickMethod pickMethod);

        [OperationContract]
        IList<Product> GetProductApp(Product data, int showRecords);

        [OperationContract]
        IList<ShowData> GetCustomList(string databaseObject, string field, string strWhere);

        [OperationContract]
        IList<ShowData> GetCustomListV2(string sqlQuery);

        #endregion



        #region //------ Document Manager Operations -------

        [OperationContract]
        IList<Document> GetDocument(Document document);

        [OperationContract]
        Boolean UpdateDocument(Document data);

        [OperationContract]
        Document CreateNewDocument(Document data, Boolean autocommit);

        [OperationContract]
        IList<DocumentBalance> GetCrossDockBalance(DocumentBalance purchaseBalance, IList<Document> salesDocs);

        [OperationContract]
        IList<Document> GetCrossDockSalesOrders(Document data);

        [OperationContract]
        Document ConfirmCrossDockProcess(IList<DocumentBalance> crossDockBalance, string user);


        [OperationContract]
        IList<DocumentLine> GetDocumentLine(DocumentLine data);

        [OperationContract]
        IList<Product> GetDocumentProduct(Document data, Product product);

        [OperationContract]
        IList<DocumentBalance> GetDocumentBalance(DocumentBalance docBalance, bool isCrossDock);

        [OperationContract]
        IList<Unit> GetDocumentUnit(DocumentLine data);

        [OperationContract]
        IList<Document> GetPendingDocument(Document document, int daysAgo, int records);

        [OperationContract]
        IList<DocumentBalance> GetDocumentPostingBalance(DocumentBalance docBalance);

        [OperationContract]
        IList<DocumentBalance> GetDocumentBalanceForEmpty(DocumentBalance docBalance);

        [OperationContract]
        DocumentTypeSequence GetNextDocSequence(Company company, DocumentType docType);

        [OperationContract]
        IList<ProductStock> GetDocumentStock(Document document, bool printed);

        //[OperationContract]
        //IList<Account> GetDocumentAccount(Document document, short accountType, bool pending);

        [OperationContract]
        IList<ShowData> GetDocumentAccount(Document document, short accountType, bool pending);

        [OperationContract]
        DocumentLine SaveUpdateDocumentLine(DocumentLine docLine, bool removeLine);

        #endregion



        #region  // ------ Label Manager Operations  -------

        [OperationContract]
        IList<Label> GetLabel(Label data);

        [OperationContract]
        Label SaveLabel(Label data);

        [OperationContract]
        IList<DocumentType> GetLabelType();

        [OperationContract]
        IList<Label> GetDocumentLabelAvailable(Document data, Label searchLabel);

        [OperationContract]
        IList<ProductStock> GetBinStock(ProductStock data);

        [OperationContract]
        IList<ProductStock> GetLabelStock(Label fatherLabel);

        [OperationContract]
        DocumentPackage CreateNewPackage(Document document, SysUser picker, bool isOpen, DocumentPackage parent, 
            String packageType);

        [OperationContract]
        void MoveQtyBetweenPackages(DocumentPackage curPack, DocumentPackage newPack, Product product, double qty);

        [OperationContract]
        IList<Label> CreateEmptyLabel(Location location, int numLabels, Product product, string printSession, SysUser user);

        [OperationContract]
        IList<Label> GetUniqueTrackLabels(Label label);

        [OperationContract]
        void CloseDocumentPackage(DocumentPackage newPack);

        #endregion



        #region //------ Profile Manager Operations  -------

        [OperationContract]
        SysUser UserAuthentication(SysUser data);

        [OperationContract]
        IList<ShowData> GetDomainList();

        [OperationContract]
        IList<ShowData> GetCustomersList();

        [OperationContract]
        IList<ConfigOption> GetConfigOption(ConfigOption data);
        [OperationContract]
        ConfigOption SaveConfigOption(ConfigOption data);
        [OperationContract]
        void DeleteConfigOption(ConfigOption data);
        [OperationContract]
        void UpdateConfigOption(ConfigOption data);

        [OperationContract]
        IList<ConfigType> GetConfigType(ConfigType data);
        [OperationContract]
        ConfigType SaveConfigType(ConfigType data);
        [OperationContract]
        void DeleteConfigType(ConfigType data);
        [OperationContract]
        void UpdateConfigType(ConfigType data);

        [OperationContract]
        IList<MenuOption> GetMenuOption(MenuOption data);
        [OperationContract]
        MenuOption SaveMenuOption(MenuOption data);
        [OperationContract]
        void DeleteMenuOption(MenuOption data);
        [OperationContract]
        void UpdateMenuOption(MenuOption data);

        [OperationContract]
        IList<MenuOptionByRol> GetMenuOptionByRol(MenuOptionByRol data);
        [OperationContract]
        MenuOptionByRol SaveMenuOptionByRol(MenuOptionByRol data);
        [OperationContract]
        void DeleteMenuOptionByRol(MenuOptionByRol data);
        [OperationContract]
        void UpdateMenuOptionByRol(MenuOptionByRol data);

        [OperationContract]
        IList<MenuOptionType> GetMenuOptionType(MenuOptionType data);
        [OperationContract]
        MenuOptionType SaveMenuOptionType(MenuOptionType data);
        [OperationContract]
        void DeleteMenuOptionType(MenuOptionType data);
        [OperationContract]
        void UpdateMenuOptionType(MenuOptionType data);

        [OperationContract]
        IList<Rol> GetRol(Rol data);
        [OperationContract]
        Rol SaveRol(Rol data);
        [OperationContract]
        void DeleteRol(Rol data);
        [OperationContract]
        void UpdateRol(Rol data);

        [OperationContract]
        IList<SysUser> GetSysUser(SysUser data);
        [OperationContract]
        SysUser SaveSysUser(SysUser data);
        [OperationContract]
        void DeleteSysUser(SysUser data);
        [OperationContract]
        void UpdateSysUser(SysUser data);

        [OperationContract]
        IList<ConfigOptionByCompany> GetConfigOptionByCompany(ConfigOptionByCompany data);
        [OperationContract]
        ConfigOptionByCompany SaveConfigOptionByCompany(ConfigOptionByCompany data);
        [OperationContract]
        void DeleteConfigOptionByCompany(ConfigOptionByCompany data);
        [OperationContract]
        void UpdateConfigOptionByCompany(ConfigOptionByCompany data);


        [OperationContract]
        IList<UserByRol> GetUserByRol(UserByRol data);
        [OperationContract]
        UserByRol SaveUserByRol(UserByRol data);
        [OperationContract]
        void DeleteUserByRol(UserByRol data);
        [OperationContract]
        void UpdateUserByRol(UserByRol data);

        #endregion



        #region //------ Transaction Manager Operations  -------


        [OperationContract]
        DocumentLine SaveAdjustmentTransaction(DocumentLine data, Label label, bool commitTransaction);

        [OperationContract]
        Boolean CheckAdjustmentLine(DocumentLine data, Label label);

        [OperationContract]
        Label ChangeLabelUbication(Label labelSource, Label labelDest, string appPath, SysUser user);

        [OperationContract]
        Label ChangeLabelLocationV2(Label labelSource, Label labelDest, Document document);

        [OperationContract]
        DocumentLine ChangeProductUbication(Label labelSource, DocumentLine changeLine, Label labelDest, string appPath);

        [OperationContract]
        Boolean ReceiveProduct(DocumentLine receivingLine, Unit logisticUnit, Bin destLocation, Node recNode);

        [OperationContract]
        void ReceiveLabels(Document document, IList<Label> labels, Bin destLocation, Node recNode, Boolean defaultBin,
            Location location);


        [OperationContract]
        Label ReceiveLabel(Document document, Label label, Bin destLocation, Node recNode);

        [OperationContract]
        void ReceiptAtOnce(Document document, Bin destLocation, Node recNode);

        [OperationContract]
        Boolean PickProduct(DocumentLine line, Label sourceLocation, Node node, Label packageLabel,
            SysUser picker, Bin binDest);

        [OperationContract]
        Label PickLabel(Document document, Label label, Node node, Label packageLabel,
            SysUser picker, Bin binDest);

        [OperationContract]
        void PickAtOnce(Document document, Label sourceLocation, Node node, SysUser picker);

        [OperationContract]
        void PickCrossDockProduct(Document purchase, IList<DocumentBalance> crossDockBalance, SysUser picker);

        [OperationContract]
        void ReverseReceiptNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, DocumentType docType);

        [OperationContract]
        void ReversePickingNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, Bin restockBin);


        [OperationContract]
        void ReverseReceiptNodeTraceByQty(DocumentBalance docBalance, int quantity, SysUser user);

        [OperationContract]
        Document ConfirmKitAssemblyOrder(Document data, Location location);

        [OperationContract]
        IList<ProductStock> GetReplanishmentList(ProductStock data, Location location, short selector, bool showEmpty, string bin1, string bin2);

        [OperationContract]
        Document CreateReplenishOrder(IList<ProductStock> lines, String user, Location location);

        [OperationContract]
        IList<ProductStock> GetStockComparation(ProductStock data, bool detailed, Company company);

        [OperationContract]
        IList<CountTaskBalance> GetCountSummary(Document document, bool summary);

        [OperationContract]
        Document ConfirmCountingTaskDocument(Document countTask, IList<CountTaskBalance> taskList, string user);

        [OperationContract]
        void CancelCountingTask(Document countTask, string user);


        [OperationContract]
        Label PickProductWithTrack(Document document, Label label, double qtyToPick, Node node, SysUser picker, Label packLabel);

        [OperationContract]
        bool ReceiveReturn(Document document, IList<ProductStock> retProduct, SysUser sysUser, double retTotalQty, Node recNode);

        [OperationContract]
        void UpdatePackageMovedLabels(IList<Label> movedLabels);

        [OperationContract]
        Label CreateUniqueTrackLabel(Label fatherLabel, string labelCode);

        [OperationContract]
        Label UpdateLabelTracking(Label label, TrackOption trackOpt, string trackValue, string user);


        [OperationContract]
        void ReceiptAcknowledge (Document document, double numLabels, SysUser sysUser, string appPath);


        [OperationContract]
        Document CreateMergedDocument(Document document, IList<DocumentLine> dtLines, IList<SysUser> pickers,
            IList<DocumentAddress> addresses);



        [OperationContract]
        Document CreateMergedDocumentV2(Document document, IList<DocumentLine> dtLines, IList<SysUser> pickers,
            IList<DocumentAddress> addresses);

        [OperationContract]
        Document ConsolidateOrdersInNewDocument(Document document, List<Document> docList);


        [OperationContract]
        string CreateMergedDocumentForBackOrders(Document document, IList<DocumentLine> dtLines, 
            IList<SysUser> pickers, IList<DocumentAddress> addresses, int process);


        [OperationContract]
        void CancelMergerOrder(Document document, DocumentLine docLine);



        [OperationContract]
        IList<ProductStock> GetDocumentProductStock(Document document, Product product);

        [OperationContract]
        Label PickUniqueLabel(Document document, Node node, Product product, string serialLabel, SysUser picker, Label packLabel);

        [OperationContract]
        void UnPickUniqueLabel(Document document, Label label, SysUser picker);

        [OperationContract]
        string ProcessFile(CustomProcess process, string stream, SysUser user);


        [OperationContract]
        void ConfirmPicking(Document document, string user);

        [OperationContract]
        Document ProcessNoCount(List<ProductStock> list, string username, bool erp);

        [OperationContract]
        void ProcessNoCountToBin(List<ProductStock> listNoCount, string username, Location location, Bin bin);
        

        [OperationContract]
        IList<ProductStock> GetNoCountSummary(Location location);

        [OperationContract]
        void ReSendInventoryAdjustmentToERP(Document document);


        #endregion



        #region  //-------- Printing Manager Operations --------

        //[OperationContract]
        //String GetReplacedTemplate(IList<DocumentBalance> printList, LabelTemplate template, String printLot, 
        //    UserByRol userByRol);

        [OperationContract]
        IList<LabelTemplate> GetLabelTemplate(LabelTemplate data);

        [OperationContract]
        String GetReplacedTemplateForLabels(IList<Label> labelList, LabelTemplate template, string printLot);

        [OperationContract]
        IList<Label> GenerateLabelsToPrint(IList<DocumentBalance> printList, String printLot, UserByRol userByRol);

        //[OperationContract]
        //void PrintShipmentDocs(Document shipment, string appPath, string printer);

        [OperationContract]
        int PrintPackageLabels(Document shipment, string appPath, string printer, string labelcode);

        [OperationContract]
        void PrintLabelsFromDevice(string printer, string rdlTemplateName, IList<Label> listOfLabels, string appPath);

        [OperationContract]
        void PrintDocumentsInBatch(IList<Document> documentList, string appPath, string printer, CustomProcess process);

        [OperationContract]
        void PrintKitAssemblyLabels(Document ssDocument, int qtyType);

        #endregion


        #region  //--------- ERP Process Operations -------------

        [OperationContract]
        void CreateInventoryAdjustment(Document data, bool reload);

        [OperationContract]
        void ReverseInventoryAdjustment(Document data);

        [OperationContract]
        Document CreatePurchaseReceipt(Document data);

        [OperationContract]
        void ReversePurchaseReceipt(Document data);

        [OperationContract]
        void UpdateErpPostedProcess(Company company);

        [OperationContract]
        Document CreateShipmentDocument(Document data);

        [OperationContract]
        void ReverseShipmentDocument(Document data, Bin binRestore);

        [OperationContract]
        void FullfilSalesOrder(Document document);

        #endregion



        #region //------ (Basic) Master Module Operations  -------


        [OperationContract]
        IList<Account> GetAccount(Account data);
        [OperationContract]
        Account SaveAccount(Account data);
        [OperationContract]
        void DeleteAccount(Account data);
        [OperationContract]
        void UpdateAccount(Account data);

        [OperationContract]
        IList<AccountAddress> GetAccountAddress(AccountAddress data);
        [OperationContract]
        AccountAddress SaveAccountAddress(AccountAddress data);
        [OperationContract]
        void DeleteAccountAddress(AccountAddress data);
        [OperationContract]
        void UpdateAccountAddress(AccountAddress data);

        [OperationContract]
        IList<AccountTypeRelation> GetAccountTypeRelation(AccountTypeRelation data);
        [OperationContract]
        AccountTypeRelation SaveAccountTypeRelation(AccountTypeRelation data);
        [OperationContract]
        void DeleteAccountTypeRelation(AccountTypeRelation data);
        [OperationContract]
        void UpdateAccountTypeRelation(AccountTypeRelation data);

        [OperationContract]
        IList<Bin> GetBin(Bin data);
        [OperationContract]
        Bin SaveBin(Bin data);
        [OperationContract]
        void DeleteBin(Bin data);
        [OperationContract]
        void UpdateBin(Bin data);

        [OperationContract]
        IList<Company> GetCompany(Company data);
        [OperationContract]
        Company SaveCompany(Company data);
        [OperationContract]
        void DeleteCompany(Company data);
        [OperationContract]
        void UpdateCompany(Company data);


        [OperationContract]
        IList<C_CasNumber> GetC_CasNumber(C_CasNumber data);
        [OperationContract]
        C_CasNumber SaveC_CasNumber(C_CasNumber data);
        [OperationContract]
        void DeleteC_CasNumber(C_CasNumber data);
        [OperationContract]
        void UpdateC_CasNumber(C_CasNumber data);


        [OperationContract]
        IList<C_CasNumberFormula> GetC_CasNumberFormula(C_CasNumberFormula data);
        [OperationContract]
        C_CasNumberFormula SaveC_CasNumberFormula(C_CasNumberFormula data);
        [OperationContract]
        void DeleteC_CasNumberFormula(C_CasNumberFormula data);
        [OperationContract]
        void UpdateC_CasNumberFormula(C_CasNumberFormula data);



        [OperationContract]
        IList<C_CasNumberRule> GetC_CasNumberRule(C_CasNumberRule data);
        [OperationContract]
        C_CasNumberRule SaveC_CasNumberRule(C_CasNumberRule data);
        [OperationContract]
        void UpdateC_CasNumberRule(C_CasNumberRule data);
        [OperationContract]
        void DeleteC_CasNumberRule(C_CasNumberRule data);



        [OperationContract]
        IList<Contact> GetContact(Contact data);
        [OperationContract]
        Contact SaveContact(Contact data);
        [OperationContract]
        void DeleteContact(Contact data);
        [OperationContract]
        void UpdateContact(Contact data);

        [OperationContract]
        IList<ContactEntityRelation> GetContactEntityRelation(ContactEntityRelation data);
        [OperationContract]
        ContactEntityRelation SaveContactEntityRelation(ContactEntityRelation data);
        [OperationContract]
        void DeleteContactEntityRelation(ContactEntityRelation data);
        [OperationContract]
        void UpdateContactEntityRelation(ContactEntityRelation data);

        [OperationContract]
        IList<ContactPosition> GetContactPosition(ContactPosition data);
        [OperationContract]
        ContactPosition SaveContactPosition(ContactPosition data);
        [OperationContract]
        void DeleteContactPosition(ContactPosition data);
        [OperationContract]
        void UpdateContactPosition(ContactPosition data);

        [OperationContract]
        IList<Location> GetLocation(Location data);
        [OperationContract]
        Location SaveLocation(Location data);
        [OperationContract]
        void DeleteLocation(Location data);
        [OperationContract]
        void UpdateLocation(Location data);

        [OperationContract]
        IList<Product> GetProduct(Product data);
        [OperationContract]
        Product SaveProduct(Product data);
        [OperationContract]
        void DeleteProduct(Product data);
        [OperationContract]
        void UpdateProduct(Product data);


        [OperationContract]
        IList<TrackOption> GetTrackOption(TrackOption data);
        [OperationContract]
        TrackOption SaveTrackOption(TrackOption data);
        [OperationContract]
        void DeleteTrackOption(TrackOption data);
        [OperationContract]
        void UpdateTrackOption(TrackOption data);

        [OperationContract]
        IList<ProductTrackRelation> GetProductTrackRelation(ProductTrackRelation data);
        [OperationContract]
        ProductTrackRelation SaveProductTrackRelation(ProductTrackRelation data);
        [OperationContract]
        void DeleteProductTrackRelation(ProductTrackRelation data);
        [OperationContract]
        void UpdateProductTrackRelation(ProductTrackRelation data);


        [OperationContract]
        IList<ShippingMethod> GetShippingMethod(ShippingMethod data);
        [OperationContract]
        ShippingMethod SaveShippingMethod(ShippingMethod data);
        [OperationContract]
        void DeleteShippingMethod(ShippingMethod data);
        [OperationContract]
        void UpdateShippingMethod(ShippingMethod data);

        [OperationContract]
        IList<Terminal> GetTerminal(Terminal data);
        [OperationContract]
        Terminal SaveTerminal(Terminal data);
        [OperationContract]
        void DeleteTerminal(Terminal data);
        [OperationContract]
        void UpdateTerminal(Terminal data);

        [OperationContract]
        IList<Unit> GetUnit(Unit data);
        [OperationContract]
        Unit SaveUnit(Unit data);
        [OperationContract]
        void DeleteUnit(Unit data);
        [OperationContract]
        void UpdateUnit(Unit data);

        [OperationContract]
        IList<UnitProductEquivalence> GetUnitProductEquivalence(UnitProductEquivalence data);
        [OperationContract]
        UnitProductEquivalence SaveUnitProductEquivalence(UnitProductEquivalence data);
        [OperationContract]
        void DeleteUnitProductEquivalence(UnitProductEquivalence data);
        [OperationContract]
        void UpdateUnitProductEquivalence(UnitProductEquivalence data);

        [OperationContract]
        IList<UnitProductLogistic> GetUnitProductLogistic(UnitProductLogistic data);
        [OperationContract]
        UnitProductLogistic SaveUnitProductLogistic(UnitProductLogistic data);
        [OperationContract]
        void DeleteUnitProductLogistic(UnitProductLogistic data);
        [OperationContract]
        void UpdateUnitProductLogistic(UnitProductLogistic data);

        [OperationContract]
        IList<UnitProductRelation> GetUnitProductRelation(UnitProductRelation data);
        [OperationContract]
        UnitProductRelation SaveUnitProductRelation(UnitProductRelation data);
        [OperationContract]
        void DeleteUnitProductRelation(UnitProductRelation data);
        [OperationContract]
        void UpdateUnitProductRelation(UnitProductRelation data);

        [OperationContract]
        IList<Vehicle> GetVehicle(Vehicle data);
        [OperationContract]
        Vehicle SaveVehicle(Vehicle data);
        [OperationContract]
        void DeleteVehicle(Vehicle data);
        [OperationContract]
        void UpdateVehicle(Vehicle data);

        [OperationContract]
        IList<Zone> GetZone(Zone data);
        [OperationContract]
        Zone SaveZone(Zone data);
        [OperationContract]
        void DeleteZone(Zone data);
        [OperationContract]
        void UpdateZone(Zone data);

        [OperationContract]
        IList<ZoneBinRelation> GetZoneBinRelation(ZoneBinRelation data);
        [OperationContract]
        ZoneBinRelation SaveZoneBinRelation(ZoneBinRelation data);
        [OperationContract]
        void DeleteZoneBinRelation(ZoneBinRelation data);
        [OperationContract]
        void UpdateZoneBinRelation(ZoneBinRelation data);

        [OperationContract]
        IList<ZonePickerRelation> GetZonePickerRelation(ZonePickerRelation data);
        [OperationContract]
        ZonePickerRelation SaveZonePickerRelation(ZonePickerRelation data);
        [OperationContract]
        void DeleteZonePickerRelation(ZonePickerRelation data);
        [OperationContract]
        void UpdateZonePickerRelation(ZonePickerRelation data);

        [OperationContract]
        IList<ZoneEntityRelation> GetZoneEntityRelation(ZoneEntityRelation data);
        [OperationContract]
        ZoneEntityRelation SaveZoneEntityRelation(ZoneEntityRelation data);
        [OperationContract]
        void DeleteZoneEntityRelation(ZoneEntityRelation data);
        [OperationContract]
        void UpdateZoneEntityRelation(ZoneEntityRelation data);


        [OperationContract]
        IList<ProductAccountRelation> GetProductAccountRelation(ProductAccountRelation data);
        [OperationContract]
        ProductAccountRelation SaveProductAccountRelation(ProductAccountRelation data);
        [OperationContract]
        void DeleteProductAccountRelation(ProductAccountRelation data);
        [OperationContract]
        void UpdateProductAccountRelation(ProductAccountRelation data);
        [OperationContract]
        void UpdateIsMainProductAccount(ProductAccountRelation data);


        [OperationContract]
        IList<ImageEntityRelation> GetImageEntityRelation(ImageEntityRelation data);
        [OperationContract]
        ImageEntityRelation SaveImageEntityRelation(ImageEntityRelation data);
        [OperationContract]
        void DeleteImageEntityRelation(ImageEntityRelation data);
        [OperationContract]
        void UpdateImageEntityRelation(ImageEntityRelation data);


        [OperationContract]
        IList<DocumentPackage> GetDocumentPackage(DocumentPackage data);
        [OperationContract]
        DocumentPackage SaveDocumentPackage(DocumentPackage data);
        [OperationContract]
        void DeleteDocumentPackage(DocumentPackage data);
        [OperationContract]
        void UpdateDocumentPackage(DocumentPackage data);

        [OperationContract]
        IList<Contract> GetContract(Contract data);
        [OperationContract]
        Contract SaveContract(Contract data);
        [OperationContract]
        void DeleteContract(Contract data);
        [OperationContract]
        void UpdateContract(Contract data);


        [OperationContract]
        IList<ProcessEntityResource> GetProcessEntityResource(ProcessEntityResource data);
        [OperationContract]
        ProcessEntityResource SaveProcessEntityResource(ProcessEntityResource data);
        [OperationContract]
        void DeleteProcessEntityResource(ProcessEntityResource data);
        [OperationContract]
        void UpdateProcessEntityResource(ProcessEntityResource data);


        [OperationContract]
        IList<ProductCategory> GetProductCategory(ProductCategory data);
        [OperationContract]
        ProductCategory SaveProductCategory(ProductCategory data);
        [OperationContract]
        void DeleteProductCategory(ProductCategory data);
        [OperationContract]
        void UpdateProductCategory(ProductCategory data);


        [OperationContract]
        IList<ProductInventory> GetProductInventory(ProductInventory data);
        [OperationContract]
        ProductInventory SaveProductInventory(ProductInventory data);
        [OperationContract]
        void DeleteProductInventory(ProductInventory data);
        [OperationContract]
        void UpdateProductInventory(ProductInventory data);
        [OperationContract]
        IList<ProductInventory> GetProductInventoryByProduct(ProductInventory productInventory, List<int> productList);
        [OperationContract]
        void PersistProductInUse(ProductInventory productInventory);
        [OperationContract]
        void ResetQtyInUse(ProductInventory productInventory);

        [OperationContract]
        IList<ProductStock> GetProductInUseForMerged(List<int> list, Location location);


        [OperationContract]
        IList<KitAssembly> GetKitAssembly(KitAssembly data, int showRegs);
        [OperationContract]
        KitAssembly SaveKitAssembly(KitAssembly data);
        [OperationContract]
        void DeleteKitAssembly(KitAssembly data);
        [OperationContract]
        void UpdateKitAssembly(KitAssembly data);

        [OperationContract]
        IList<KitAssemblyFormula> GetKitAssemblyFormula(KitAssemblyFormula data);
        [OperationContract]
        KitAssemblyFormula SaveKitAssemblyFormula(KitAssemblyFormula data);
        [OperationContract]
        void DeleteKitAssemblyFormula(KitAssemblyFormula data);
        [OperationContract]
        void UpdateKitAssemblyFormula(KitAssemblyFormula data);


        [OperationContract]
        IList<MType> GetMType(MType data);
        [OperationContract]
        MType SaveMType(MType data);
        [OperationContract]
        void UpdateMType(MType data);
        [OperationContract]
        void DeleteMType(MType data);


        [OperationContract]
        IList<MMaster> GetMMaster(MMaster data);
        [OperationContract]
        MMaster SaveMMaster(MMaster data);
        [OperationContract]
        void UpdateMMaster(MMaster data);
        [OperationContract]
        void DeleteMMaster(MMaster data);


        [OperationContract]
        IList<EntityExtraData> GetEntityExtraData(EntityExtraData data);
        [OperationContract]
        EntityExtraData SaveEntityExtraData(EntityExtraData data);
        [OperationContract]
        void DeleteEntityExtraData(EntityExtraData data);
        [OperationContract]
        void UpdateEntityExtraData(EntityExtraData data);




        #endregion



        #region //------ (Basic) General Module Operations

        [OperationContract]
        IList<AccountType> GetAccountType(AccountType data);
        [OperationContract]
        AccountType SaveAccountType(AccountType data);
        [OperationContract]
        void DeleteAccountType(AccountType data);
        [OperationContract]
        void UpdateAccountType(AccountType data);


        [OperationContract]
        IList<ClassEntity> GetClassEntity(ClassEntity data);
        [OperationContract]
        ClassEntity SaveClassEntity(ClassEntity data);
        [OperationContract]
        void DeleteClassEntity(ClassEntity data);
        [OperationContract]
        void UpdateClassEntity(ClassEntity data);

        [OperationContract]
        IList<DocumentClass> GetDocumentClass(DocumentClass data);
        [OperationContract]
        DocumentClass SaveDocumentClass(DocumentClass data);
        [OperationContract]
        void DeleteDocumentClass(DocumentClass data);
        [OperationContract]
        void UpdateDocumentClass(DocumentClass data);

        [OperationContract]
        IList<DocumentConcept> GetDocumentConcept(DocumentConcept data);
        [OperationContract]
        DocumentConcept SaveDocumentConcept(DocumentConcept data);
        [OperationContract]
        void DeleteDocumentConcept(DocumentConcept data);
        [OperationContract]
        void UpdateDocumentConcept(DocumentConcept data);

        [OperationContract]
        IList<DocumentType> GetDocumentType(DocumentType data);
        [OperationContract]
        DocumentType SaveDocumentType(DocumentType data);
        [OperationContract]
        void DeleteDocumentType(DocumentType data);
        [OperationContract]
        void UpdateDocumentType(DocumentType data);


        [OperationContract]
        IList<DocumentTypeSequence> GetDocumentTypeSequence(DocumentTypeSequence data);
        [OperationContract]
        DocumentTypeSequence SaveDocumentTypeSequence(DocumentTypeSequence data);
        [OperationContract]
        void DeleteDocumentTypeSequence(DocumentTypeSequence data);
        [OperationContract]
        void UpdateDocumentTypeSequence(DocumentTypeSequence data);


        [OperationContract]
        IList<GroupCriteria> GetGroupCriteria(GroupCriteria data);
        [OperationContract]
        GroupCriteria SaveGroupCriteria(GroupCriteria data);
        [OperationContract]
        void DeleteGroupCriteria(GroupCriteria data);
        [OperationContract]
        void UpdateGroupCriteria(GroupCriteria data);


        [OperationContract]
        IList<GroupCriteriaDetail> GetGroupCriteriaDetail(GroupCriteriaDetail data);
        [OperationContract]
        GroupCriteriaDetail SaveGroupCriteriaDetail(GroupCriteriaDetail data);
        [OperationContract]
        void DeleteGroupCriteriaDetail(GroupCriteriaDetail data);
        [OperationContract]
        void UpdateGroupCriteriaDetail(GroupCriteriaDetail data);


        [OperationContract]
        IList<GroupCriteriaRelation> GetGroupCriteriaRelation(GroupCriteriaRelation data);
        [OperationContract]
        GroupCriteriaRelation SaveGroupCriteriaRelation(GroupCriteriaRelation data);
        [OperationContract]
        void DeleteGroupCriteriaRelation(GroupCriteriaRelation data);
        [OperationContract]
        void UpdateGroupCriteriaRelation(GroupCriteriaRelation data);


        [OperationContract]
        IList<GroupCriteriaRelationData> GetGroupCriteriaRelationData(GroupCriteriaRelationData data);
        [OperationContract]
        GroupCriteriaRelationData SaveGroupCriteriaRelationData(GroupCriteriaRelationData data);
        [OperationContract]
        void DeleteGroupCriteriaRelationData(GroupCriteriaRelationData data);
        [OperationContract]
        void UpdateGroupCriteriaRelationData(GroupCriteriaRelationData data);


        [OperationContract]
        IList<LabelMapping> GetLabelMapping(LabelMapping data);
        [OperationContract]
        LabelMapping SaveLabelMapping(LabelMapping data);
        [OperationContract]
        void DeleteLabelMapping(LabelMapping data);
        [OperationContract]
        void UpdateLabelMapping(LabelMapping data);


        [OperationContract]
        LabelTemplate SaveLabelTemplate(LabelTemplate data);
        [OperationContract]
        void DeleteLabelTemplate(LabelTemplate data);
        [OperationContract]
        void UpdateLabelTemplate(LabelTemplate data);


        [OperationContract]
        IList<LogError> GetLogError(LogError data);
        [OperationContract]
        LogError SaveLogError(LogError data);
        [OperationContract]
        void DeleteLogError(LogError data);
        [OperationContract]
        void UpdateLogError(LogError data);


        [OperationContract]
        IList<MeasureType> GetMeasureType(MeasureType data);
        [OperationContract]
        MeasureType SaveMeasureType(MeasureType data);
        [OperationContract]
        void DeleteMeasureType(MeasureType data);
        [OperationContract]
        void UpdateMeasureType(MeasureType data);


        [OperationContract]
        IList<MeasureUnit> GetMeasureUnit(MeasureUnit data);
        [OperationContract]
        MeasureUnit SaveMeasureUnit(MeasureUnit data);
        [OperationContract]
        void DeleteMeasureUnit(MeasureUnit data);
        [OperationContract]
        void UpdateMeasureUnit(MeasureUnit data);


        [OperationContract]
        IList<MeasureUnitConvertion> GetMeasureUnitConvertion(MeasureUnitConvertion data);
        [OperationContract]
        MeasureUnitConvertion SaveMeasureUnitConvertion(MeasureUnitConvertion data);
        [OperationContract]
        void DeleteMeasureUnitConvertion(MeasureUnitConvertion data);
        [OperationContract]
        void UpdateMeasureUnitConvertion(MeasureUnitConvertion data);




        [OperationContract]
        IList<Status> GetStatus(Status data);
        [OperationContract]
        Status SaveStatus(Status data);
        [OperationContract]
        void DeleteStatus(Status data);
        [OperationContract]
        void UpdateStatus(Status data);


        [OperationContract]
        IList<StatusType> GetStatusType(StatusType data);
        [OperationContract]
        StatusType SaveStatusType(StatusType data);
        [OperationContract]
        void DeleteStatusType(StatusType data);
        [OperationContract]
        void UpdateStatusType(StatusType data);


        [OperationContract]
        IList<Connection> GetConnection(Connection data);
        [OperationContract]
        Connection SaveConnection(Connection data);
        [OperationContract]
        void DeleteConnection(Connection data);
        [OperationContract]
        void UpdateConnection(Connection data);


        [OperationContract]
        IList<MenuOptionExtension> GetMenuOptionExtension(MenuOptionExtension data);
        [OperationContract]
        MenuOptionExtension SaveMenuOptionExtension(MenuOptionExtension data);
        [OperationContract]
        void DeleteMenuOptionExtension(MenuOptionExtension data);
        [OperationContract]
        void UpdateMenuOptionExtension(MenuOptionExtension data);



        [OperationContract]
        IList<MessageRuleExtension> GetMessageRuleExtension(MessageRuleExtension data);
        [OperationContract]
        MessageRuleExtension SaveMessageRuleExtension(MessageRuleExtension data);
        [OperationContract]
        void DeleteMessageRuleExtension(MessageRuleExtension data);
        [OperationContract]
        void UpdateMessageRuleExtension(MessageRuleExtension data);


        [OperationContract]
        IList<OptionType> GetOptionType(OptionType data);

        [OperationContract]
        IList<ConnectionType> GetConnectionType(ConnectionType data);

        [OperationContract]
        void TestConnection(Company data);


        [OperationContract]
        IList<MessageRuleByCompany> GetMessageRuleByCompany(MessageRuleByCompany data);
        [OperationContract]
        MessageRuleByCompany SaveMessageRuleByCompany(MessageRuleByCompany data);
        [OperationContract]
        void DeleteMessageRuleByCompany(MessageRuleByCompany data);
        [OperationContract]
        void UpdateMessageRuleByCompany(MessageRuleByCompany data);


        [OperationContract]
        IList<ProductAlternate> GetProductAlternate(ProductAlternate data);
        [OperationContract]
        ProductAlternate SaveProductAlternate(ProductAlternate data);
        [OperationContract]
        void DeleteProductAlternate(ProductAlternate data);
        [OperationContract]
        void UpdateProductAlternate(ProductAlternate data);




        [OperationContract]
        IList<CountSchedule> GetCountSchedule(CountSchedule data);
        [OperationContract]
        CountSchedule SaveCountSchedule(CountSchedule data);
        [OperationContract]
        void DeleteCountSchedule(CountSchedule data);
        [OperationContract]
        void UpdateCountSchedule(CountSchedule data);


        #endregion



        #region //-------- (Basic) Trace Module Operation


        [OperationContract]
        Document SaveDocument(Document data);
        [OperationContract]
        void DeleteDocument(Document data);


        [OperationContract]
        IList<DocumentAddress> GetDocumentAddress(DocumentAddress data);
        [OperationContract]
        DocumentAddress SaveDocumentAddress(DocumentAddress data);
        [OperationContract]
        void DeleteDocumentAddress(DocumentAddress data);
        [OperationContract]
        void UpdateDocumentAddress(DocumentAddress data);


        //[OperationContract]
        //IList<DocumentHistory> GetDocumentHistory(DocumentHistory data);
        //[OperationContract]
        //DocumentHistory SaveDocumentHistory(DocumentHistory data);
        //[OperationContract]
        //void DeleteDocumentHistory(DocumentHistory data);
        //[OperationContract]
        //void UpdateDocumentHistory(DocumentHistory data);


        [OperationContract]
        DocumentLine SaveDocumentLine(DocumentLine data);
        [OperationContract]
        void DeleteDocumentLine(DocumentLine data);
        [OperationContract]
        void UpdateDocumentLine(DocumentLine data);


        //[OperationContract]
        //IList<DocumentLineHistory> GetDocumentLineHistory(DocumentLineHistory data);
        //[OperationContract]
        //DocumentLineHistory SaveDocumentLineHistory(DocumentLineHistory data);
        //[OperationContract]
        //void DeleteDocumentLineHistory(DocumentLineHistory data);
        //[OperationContract]
        //void UpdateDocumentLineHistory(DocumentLineHistory data);


        [OperationContract]
        void DeleteLabel(Label data);
        [OperationContract]
        void UpdateLabel(Label data);

        //[OperationContract]
        //IList<LabelHistory> GetLabelHistory(LabelHistory data);
        //[OperationContract]
        //LabelHistory SaveLabelHistory(LabelHistory data);
        //[OperationContract]
        //void DeleteLabelHistory(LabelHistory data);
        //[OperationContract]
        //void UpdateLabelHistory(LabelHistory data);

        [OperationContract]
        IList<Node> GetNode(Node data);
        [OperationContract]
        Node SaveNode(Node data);
        [OperationContract]
        void DeleteNode(Node data);
        [OperationContract]
        void UpdateNode(Node data);

        [OperationContract]
        IList<NodeDocumentType> GetNodeDocumentType(NodeDocumentType data);
        [OperationContract]
        NodeDocumentType SaveNodeDocumentType(NodeDocumentType data);
        [OperationContract]
        void DeleteNodeDocumentType(NodeDocumentType data);
        [OperationContract]
        void UpdateNodeDocumentType(NodeDocumentType data);

        [OperationContract]
        IList<NodeExtension> GetNodeExtension(NodeExtension data);
        [OperationContract]
        NodeExtension SaveNodeExtension(NodeExtension data);
        [OperationContract]
        void DeleteNodeExtension(NodeExtension data);
        [OperationContract]
        void UpdateNodeExtension(NodeExtension data);

        [OperationContract]
        IList<NodeExtensionTrace> GetNodeExtensionTrace(NodeExtensionTrace data);
        [OperationContract]
        NodeExtensionTrace SaveNodeExtensionTrace(NodeExtensionTrace data);
        [OperationContract]
        void DeleteNodeExtensionTrace(NodeExtensionTrace data);
        [OperationContract]
        void UpdateNodeExtensionTrace(NodeExtensionTrace data);

        [OperationContract]
        IList<NodeRoute> GetNodeRoute(NodeRoute data);
        [OperationContract]
        NodeRoute SaveNodeRoute(NodeRoute data);
        [OperationContract]
        void DeleteNodeRoute(NodeRoute data);
        [OperationContract]
        void UpdateNodeRoute(NodeRoute data);

        [OperationContract]
        IList<NodeTrace> GetNodeTrace(NodeTrace data);
        [OperationContract]
        NodeTrace SaveNodeTrace(NodeTrace data);
        [OperationContract]
        void DeleteNodeTrace(NodeTrace data);
        [OperationContract]
        void UpdateNodeTrace(NodeTrace data);

        //[OperationContract]
        //IList<NodeTraceHistory> GetNodeTraceHistory(NodeTraceHistory data);
        //[OperationContract]
        //NodeTraceHistory SaveNodeTraceHistory(NodeTraceHistory data);
        //[OperationContract]
        //void DeleteNodeTraceHistory(NodeTraceHistory data);
        //[OperationContract]
        //void UpdateNodeTraceHistory(NodeTraceHistory data);

        [OperationContract]
        IList<TaskDocumentRelation> GetTaskDocumentRelation(TaskDocumentRelation data);
        [OperationContract]
        TaskDocumentRelation SaveTaskDocumentRelation(TaskDocumentRelation data);
        [OperationContract]
        void DeleteTaskDocumentRelation(TaskDocumentRelation data);
        [OperationContract]
        void UpdateTaskDocumentRelation(TaskDocumentRelation data);

        [OperationContract]
        IList<Document> GetTaskByUser(TaskByUser data, Location location);
        [OperationContract]
        TaskByUser SaveTaskByUser(TaskByUser data);
        [OperationContract]
        void DeleteTaskByUser(TaskByUser data);
        [OperationContract]
        void UpdateTaskByUser(TaskByUser data);

        [OperationContract]
        IList<PickMethod> GetPickMethod(PickMethod data);
        [OperationContract]
        PickMethod SavePickMethod(PickMethod data);
        [OperationContract]
        void DeletePickMethod(PickMethod data);
        [OperationContract]
        void UpdatePickMethod(PickMethod data);


        [OperationContract]
        IList<LabelTrackOption> GetLabelTrackOption(LabelTrackOption data);
        [OperationContract]
        LabelTrackOption SaveLabelTrackOption(LabelTrackOption data);
        [OperationContract]
        void DeleteLabelTrackOption(LabelTrackOption data);
        [OperationContract]
        void UpdateLabelTrackOption(LabelTrackOption data);

        [OperationContract]
        IList<DataType> GetDataType(DataType data);



        [OperationContract]
        IList<BinByTaskExecution> GetBinByTaskExecution(BinByTaskExecution data);
        [OperationContract]
        BinByTaskExecution SaveBinByTaskExecution(BinByTaskExecution data);
        [OperationContract]
        void DeleteBinByTaskExecution(BinByTaskExecution data);
        [OperationContract]
        void UpdateBinByTaskExecution(BinByTaskExecution data);


        [OperationContract]
        IList<BinByTask> GetBinByTask(BinByTask data);
        [OperationContract]
        BinByTask SaveBinByTask(BinByTask data);
        [OperationContract]
        void DeleteBinByTask(BinByTask data);
        [OperationContract]
        void UpdateBinByTask(BinByTask data);

        //Febrero 15 de 2011
        //Jorge Armando Ortega
        [OperationContract]
        IList<DocumentLine> CreateAssemblyOrderLines(Document Document, Product Product, Double Quantity);



        #endregion



        #region Object

        [OperationContract]
        string GetObject(string hql, object fieldID, bool asString);

        [OperationContract]
        void DirectSQLNonQuery(string query, Connection localSQL);

        [OperationContract]
        DataTable DirectSQLQuery(string query, string swhere, string tableName, Connection connection);

        [OperationContract]
        DataSet DirectSQLQueryDS(string query, string swhere, string tableName, Connection connection);

        [OperationContract]
        IList<ShowData> GetLanguage(string langCode);

        #endregion



        #region //-------- Report Manager
        [OperationContract]
        ReportHeaderFormat GetReportInformation(Document data, String template);
        [OperationContract]
        DataSet GetReportObject(MenuOption option, IList<String> rpParams, Location location);


        //[OperationContract]
        //IList<ReportDocument> GetReportDocument(ReportDocument data);
        //[OperationContract]
        //ReportDocument SaveReportDocument(ReportDocument data);
        //[OperationContract]
        //void DeleteReportDocument(ReportDocument data);
        //[OperationContract]
        //void UpdateReportDocument(ReportDocument data);


        [OperationContract]
        IList<IqColumn> GetIqColumn(IqColumn data);
        [OperationContract]
        IqColumn SaveIqColumn(IqColumn data);
        [OperationContract]
        void DeleteIqColumn(IqColumn data);
        [OperationContract]
        void UpdateIqColumn(IqColumn data);

        [OperationContract]
        IList<IqReport> GetIqReport(IqReport data);
        [OperationContract]
        IqReport SaveIqReport(IqReport data);
        [OperationContract]
        void DeleteIqReport(IqReport data);
        [OperationContract]
        void UpdateIqReport(IqReport data);


        [OperationContract]
        IList<IqReportColumn> GetIqReportColumn(IqReportColumn data);
        [OperationContract]
        IqReportColumn SaveIqReportColumn(IqReportColumn data);
        [OperationContract]
        void DeleteIqReportColumn(IqReportColumn data);
        [OperationContract]
        void UpdateIqReportColumn(IqReportColumn data);


        //[OperationContract]
        //IList<IqReportSetting> GetIqReportSetting(IqReportSetting data);
        //[OperationContract]
        //IqReportSetting SaveIqReportSetting(IqReportSetting data);
        //[OperationContract]
        //void DeleteIqReportSetting(IqReportSetting data);
        //[OperationContract]
        //void UpdateIqReportSetting(IqReportSetting data);


        [OperationContract]
        IList<IqReportTable> GetIqReportTable(IqReportTable data);
        [OperationContract]
        IqReportTable SaveIqReportTable(IqReportTable data);
        [OperationContract]
        void DeleteIqReportTable(IqReportTable data);
        [OperationContract]
        void UpdateIqReportTable(IqReportTable data);


        [OperationContract]
        IList<IqTable> GetIqTable(IqTable data);
        [OperationContract]
        IqTable SaveIqTable(IqTable data);
        [OperationContract]
        void DeleteIqTable(IqTable data);
        [OperationContract]
        void UpdateIqTable(IqTable data);


        [OperationContract]
        DataSet GetIqReportDataSet(string dataQuery, DataSet rpParams);



        #endregion



        #region //-------- Workflow Operations -------

        [OperationContract]
        IList<BinRoute> GetBinRoute(BinRoute data);
        [OperationContract]
        BinRoute SaveBinRoute(BinRoute data);
        [OperationContract]
        void DeleteBinRoute(BinRoute data);
        [OperationContract]
        void UpdateBinRoute(BinRoute data);

        [OperationContract]
        IList<DataDefinition> GetDataDefinition(DataDefinition data);
        [OperationContract]
        DataDefinition SaveDataDefinition(DataDefinition data);
        [OperationContract]
        void DeleteDataDefinition(DataDefinition data);
        [OperationContract]
        void UpdateDataDefinition(DataDefinition data);

        [OperationContract]
        IList<DataInformation> GetDataInformation(DataInformation data);
        [OperationContract]
        DataInformation SaveDataInformation(DataInformation data);
        [OperationContract]
        void DeleteDataInformation(DataInformation data);
        [OperationContract]
        void UpdateDataInformation(DataInformation data);

        [OperationContract]
        IList<DataDefinitionByBin> GetDataDefinitionByBin(DataDefinitionByBin data);
        [OperationContract]
        DataDefinitionByBin SaveDataDefinitionByBin(DataDefinitionByBin data);
        [OperationContract]
        void DeleteDataDefinitionByBin(DataDefinitionByBin data);
        [OperationContract]
        void UpdateDataDefinitionByBin(DataDefinitionByBin data);

        [OperationContract]
        IList<WFDataType> GetWFDataType(WFDataType data);
        [OperationContract]
        WFDataType SaveWFDataType(WFDataType data);
        [OperationContract]
        void DeleteWFDataType(WFDataType data);
        [OperationContract]
        void UpdateWFDataType(WFDataType data);



        #endregion



        #region //------------------------ Process


        [OperationContract]
        IList<CustomProcess> GetCustomProcess(CustomProcess data);
        [OperationContract]
        CustomProcess SaveCustomProcess(CustomProcess data);
        [OperationContract]
        void DeleteCustomProcess(CustomProcess data);
        [OperationContract]
        void UpdateCustomProcess(CustomProcess data);


        [OperationContract]
        IList<CustomProcessContext> GetCustomProcessContext(CustomProcessContext data);
        [OperationContract]
        CustomProcessContext SaveCustomProcessContext(CustomProcessContext data);
        [OperationContract]
        void DeleteCustomProcessContext(CustomProcessContext data);
        [OperationContract]
        void UpdateCustomProcessContext(CustomProcessContext data);

        [OperationContract]
        IList<CustomProcessContextByEntity> GetCustomProcessContextByEntity(CustomProcessContextByEntity data);
        [OperationContract]
        CustomProcessContextByEntity SaveCustomProcessContextByEntity(CustomProcessContextByEntity data);
        [OperationContract]
        void DeleteCustomProcessContextByEntity(CustomProcessContextByEntity data);
        [OperationContract]
        void UpdateCustomProcessContextByEntity(CustomProcessContextByEntity data);


        [OperationContract]
        IList<CustomProcessActivity> GetCustomProcessActivity(CustomProcessActivity data);
        [OperationContract]
        CustomProcessActivity SaveCustomProcessActivity(CustomProcessActivity data);
        [OperationContract]
        void DeleteCustomProcessActivity(CustomProcessActivity data);
        [OperationContract]
        void UpdateCustomProcessActivity(CustomProcessActivity data);


        [OperationContract]
        IList<CustomProcessRoute> GetCustomProcessRoute(CustomProcessRoute data);
        [OperationContract]
        CustomProcessRoute SaveCustomProcessRoute(CustomProcessRoute data);
        [OperationContract]
        void DeleteCustomProcessRoute(CustomProcessRoute data);
        [OperationContract]
        void UpdateCustomProcessRoute(CustomProcessRoute data);


        [OperationContract]
        IList<CustomProcessTransition> GetCustomProcessTransition(CustomProcessTransition data);
        [OperationContract]
        CustomProcessTransition SaveCustomProcessTransition(CustomProcessTransition data);
        [OperationContract]
        void DeleteCustomProcessTransition(CustomProcessTransition data);
        [OperationContract]
        void UpdateCustomProcessTransition(CustomProcessTransition data);



        [OperationContract]
        IList<CustomProcessTransitionByEntity> GetCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data);
        [OperationContract]
        CustomProcessTransitionByEntity SaveCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data);
        [OperationContract]
        void DeleteCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data);
        [OperationContract]
        void UpdateCustomProcessTransitionByEntity(CustomProcessTransitionByEntity data);



        #endregion

    }
    
}
