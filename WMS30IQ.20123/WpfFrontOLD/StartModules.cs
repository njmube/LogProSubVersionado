using WpfFront.Presenters;
using WpfFront.Models;
using WpfFront.Services;
using WpfFront.Views;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using WMComposite.Modularity;
using WMComposite.Regions;
using System.Collections.ObjectModel;
using Assergs.Windows;
using WpfFront.IQ.Models;
using WpfFront.IQ.Presenters;
using WpfFront.IQ.Views;

namespace WpfFront
{
    public class StartModules : IModule
    {
        private readonly IUnityContainer container;
        private readonly IShellPresenter regionManager;

        public StartModules(IUnityContainer container, IShellPresenter regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
            moduleRegions = new ObservableCollection<ModuleRegion>();
        }

        public void Initialize()
        {
            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP31-C4MY0-XX36W-LGNA";
            //Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP35-3WKND-74T4J-3N2A";
            //V3.5
            //DGP35-3WKND-74T4J-3N2A : PRO
            //DGF35-U7K5D-U4T45-6N2A: STD

            RegisterViewsAndServices();
            regionManager.Shell.ShowAllContent(false);
            regionManager.Shell.Model.Modules.Add(this);
            regionManager.Shell.ShowAllContent(true);
        }



        private ObservableCollection<ModuleRegion> moduleRegions;

        public ObservableCollection<ModuleRegion> ModuleRegions
        {
            get
            {
                if (moduleRegions == null)
                    moduleRegions = new ObservableCollection<ModuleRegion>();
                return moduleRegions;
            }
        }


        protected void RegisterViewsAndServices()
        {
            //container.RegisterType<IWcfWMSProcess, WcfWMSService>();

            container.RegisterType<IOrderMngrV2View, OrderMngrV2View>();
            container.RegisterType<IOrderMngrV2Presenter, OrderMngrV2Presenter>();
            container.RegisterType<IOrderMngrV2Model, OrderMngrV2Model>();

            container.RegisterType<IBookingMngrView, BookingMngrView>();
            container.RegisterType<IBookingMngrPresenter, BookingMngrPresenter>();
            container.RegisterType<IBookingMngrModel, BookingMngrModel>();

            container.RegisterType<IShipRouteView, ShipRouteView>();
            container.RegisterType<IShipRoutePresenter, ShipRoutePresenter>();
            container.RegisterType<IShipRouteModel, ShipRouteModel>();

            container.RegisterType<IDocumentManagerView, DocumentManagerView>();
            container.RegisterType<IDocumentManagerPresenter, DocumentManagerPresenter>();
            container.RegisterType<IDocumentManagerModel, DocumentManagerModel>();

            container.RegisterType<IKitView, KitView>();
            container.RegisterType<IKitPresenter, KitPresenter>();
            container.RegisterType<IKitModel, KitModel>();

            container.RegisterType<IQueriesView, QueriesView>();
            container.RegisterType<IQueriesPresenter, QueriesPresenter>();
            container.RegisterType<IQueriesModel, QueriesModel>();


            container.RegisterType<IIntroView, IntroView>();
            container.RegisterType<IIntroPresenter, IntroPresenter>();
            container.RegisterType<IIntroModel, IntroModel>();


            container.RegisterType<IProductView, ProductView>();
            container.RegisterType<IProductPresenter, ProductPresenter>();
            container.RegisterType<IProductModel, ProductModel>();

            container.RegisterType<IProductCategoryView, ProductCategoryView>();
            container.RegisterType<IProductCategoryPresenter, ProductCategoryPresenter>();
            container.RegisterType<IProductCategoryModel, ProductCategoryModel>();


            container.RegisterType<IPrintingView, PrintingView>();
            container.RegisterType<IPrintingPresenter, PrintingPresenter>();
            container.RegisterType<IPrintingModel, PrintingModel>();

            container.RegisterType<IReceivingView, ReceivingView>();
            container.RegisterType<IReceivingPresenter, ReceivingPresenter>();
            container.RegisterType<IReceivingModel, ReceivingModel>();

            // CAA
            container.RegisterType<IRemoveNodeView, RemoveNodeView>();
            container.RegisterType<IRemoveNodePresenter, RemoveNodePresenter>();
            container.RegisterType<IRemoveNodeModel, RemoveNodeModel>();
            //

            container.RegisterType<IChangeLocationView, ChangeLocationView>();
            container.RegisterType<IChangeLocationPresenter, ChangeLocationPresenter>();
            container.RegisterType<IChangeLocationModel, ChangeLocationModel>();


            container.RegisterType<IInventoryAdjustmentView, InventoryAdjustmentView>();
            container.RegisterType<IInventoryAdjustmentPresenter, InventoryAdjustmentPresenter>();
            container.RegisterType<IInventoryAdjustmentModel, InventoryAdjustmentModel>();


            container.RegisterType<IIntroInventoryView, IntroInventoryView>();
            container.RegisterType<IIntroInventoryPresenter, IntroInventoryPresenter>();
            container.RegisterType<IIntroInventoryModel, IntroInventoryModel>();

            container.RegisterType<IShippingView, ShippingView>();
            container.RegisterType<IShippingPresenter, ShippingPresenter>();
            container.RegisterType<IShippingModel, ShippingModel>();

            container.RegisterType<ICrossDockView, CrossDockView>();
            container.RegisterType<ICrossDockPresenter, CrossDockPresenter>();
            container.RegisterType<ICrossDockModel, CrossDockModel>();


            container.RegisterType<IZoneManagerView, ZoneManagerView>();
            container.RegisterType<IZoneManagerPresenter, ZoneManagerPresenter>();
            container.RegisterType<IZoneManagerModel, ZoneManagerModel>();

            container.RegisterType<IShippingConsoleView, ShippingConsoleView>();
            container.RegisterType<IShippingConsolePresenter, ShippingConsolePresenter>();
            container.RegisterType<IShippingConsoleModel, ShippingConsoleModel>();


            container.RegisterType<IReceivingCubeView, ReceivingCubeView>();
            container.RegisterType<IReceivingCubePresenter, ReceivingCubePresenter>();
            container.RegisterType<IReceivingCubeModel, ReceivingCubeModel>();

            //Febrero 15 de 2011
            //Jorge Armando Ortega
            container.RegisterType<IKitAssemblyV2View, KitAssemblyV2View>();
            container.RegisterType<IKitAssemblyV2Presenter, KitAssemblyV2Presenter>();
            container.RegisterType<IKitAssemblyV2Model, KitAssemblyV2Model>();

            //Marzo 11 de 2009
            container.RegisterType<IKitAssemblyView, KitAssemblyView>();
            container.RegisterType<IKitAssemblyPresenter, KitAssemblyPresenter>();
            container.RegisterType<IKitAssemblyModel, KitAssemblyModel>();

            container.RegisterType<IInventoryTaskView, InventoryTaskView>();
            container.RegisterType<IInventoryTaskPresenter, InventoryTaskPresenter>();
            container.RegisterType<IInventoryTaskModel, InventoryTaskModel>();


            container.RegisterType<IBasicSetupView, BasicSetupView>();
            container.RegisterType<IBasicSetupPresenter, BasicSetupPresenter>();
            container.RegisterType<IBasicSetupModel, BasicSetupModel>();


            container.RegisterType<IBasicProcessView, BasicProcessView>();
            container.RegisterType<IBasicProcessPresenter, BasicProcessPresenter>();
            container.RegisterType<IBasicProcessModel, BasicProcessModel>();


            //Este modelo tienen varias vistas que comprten un solo modelo y un solo presenter.
            //manejan todas las taresa de inventario
            container.RegisterType<IUC_IV_Presenter, UC_IV_Presenter>();
            container.RegisterType<IUC_IV_Model, UC_IV_Model>();
            //Views
            container.RegisterType<IUC_IV_Replanish_PackingView, UC_IV_Replanish_PackingView>();


            container.RegisterType<ITrackOptionView, TrackOptionView>();
            container.RegisterType<ITrackOptionPresenter, TrackOptionPresenter>();
            container.RegisterType<ITrackOptionModel, TrackOptionModel>();


            container.RegisterType<ITrackOptionShipView, TrackOptionShipView>();
            container.RegisterType<ITrackOptionShipPresenter, TrackOptionShipPresenter>();
            container.RegisterType<ITrackOptionShipModel, TrackOptionShipModel>();


            container.RegisterType<IAdminTrackOptionView, AdminTrackOptionView>();
            container.RegisterType<IAdminTrackOptionPresenter, AdminTrackOptionPresenter>();
            container.RegisterType<IAdminTrackOptionModel, AdminTrackOptionModel>();

            container.RegisterType<IPickMethodView, PickMethodView>();
            container.RegisterType<IPickMethodPresenter, PickMethodPresenter>();
            container.RegisterType<IPickMethodModel, PickMethodModel>();

            container.RegisterType<IReportView, ReportView>();
            container.RegisterType<IReportPresenter, ReportPresenter>();
            container.RegisterType<IReportModel, ReportModel>();


            container.RegisterType<IPackageAdminView, PackageAdminView>();
            container.RegisterType<IPackageAdminPresenter, PackageAdminPresenter>();
            container.RegisterType<IPackageAdminModel, PackageAdminModel>();


            container.RegisterType<IIqToolView, IqToolView>();
            container.RegisterType<IIqToolPresenter, IqToolPresenter>();
            container.RegisterType<IIqToolModel, IqToolModel>();


            container.RegisterType<IInventoryCountView, InventoryCountView>();
            container.RegisterType<IInventoryCountPresenter, InventoryCountPresenter>();
            container.RegisterType<IInventoryCountModel, InventoryCountModel>();


            container.RegisterType<IShipperMngrView, ShipperMngrView>();
            container.RegisterType<IShipperMngrPresenter, ShipperMngrPresenter>();
            container.RegisterType<IShipperMngrModel, ShipperMngrModel>();


            container.RegisterType<IScheduleView, ScheduleView>();
            container.RegisterType<ISchedulePresenter, SchedulePresenter>();
            container.RegisterType<IScheduleModel, ScheduleModel>();

            container.RegisterType<IFileProcessView, FileProcessView>();
            container.RegisterType<IFileProcessPresenter, FileProcessPresenter>();
            container.RegisterType<IFileProcessModel, FileProcessModel>();

            #region Register - IQ

            container.RegisterType<IEntradaAlmacenView, EntradaAlmacenView>();
            container.RegisterType<IEntradaAlmacenPresenter, EntradaAlmacenPresenter>();
            container.RegisterType<IEntradaAlmacenModel, EntradaAlmacenModel>();

            container.RegisterType<IEntradaAlmacenV2View, EntradaAlmacenV2View>();
            container.RegisterType<IEntradaAlmacenV2Presenter, EntradaAlmacenV2Presenter>();
            container.RegisterType<IEntradaAlmacenV2Model, EntradaAlmacenV2Model>();

            //MODULO INTERMEDIO DIRECTV - 23 DE FEBRERO DE 2015//////////////////////////////////////////////////
            container.RegisterType<IActualizacionRRView, ActualizacionRRView>();
            container.RegisterType<IActualizacionRRPresenter, ActualizacionRRPresenter>();
            container.RegisterType<IActualizacionRRModel, ActualizacionRRModel>();
            //MODULO INTERMEDIO DIRECTV - 23 DE FEBRERO DE 2015//////////////////////////////////////////////////

            //MODULO 23 DE MAYO DE 2015//////////////////////////////////////////////////
            container.RegisterType<INovedadesView, NovedadesView>();
            container.RegisterType<INovedadesPresenter, NovedadesPresenter>();
            container.RegisterType<INovedadesModel, NovedadesModel>();
            //MODULO 23 DE MAYO DE 2015//////////////////////////////////////////////////

            //MODULO INTERMEDIO CLARO - 18 DE FEBRERO DE 2015//////////////////////////////////////////////////
            container.RegisterType<IConfirmacionIntermediaView, ConfirmacionIntermediaView>();
            container.RegisterType<IConfirmacionIntermediaPresenter, ConfirmacionIntermediaPresenter>();
            container.RegisterType<IConfirmacionIntermediaModel, ConfirmacionIntermediaModel>();
            //MODULO INTERMEDIO CLARO - 18 DE FEBRERO DE 2015//////////////////////////////////////////////////

            container.RegisterType<IAdministradorView, AdministradorView>();
            container.RegisterType<IAdministradorPresenter, AdministradorPresenter>();
            container.RegisterType<IAdministradorModel, AdministradorModel>();

            //MODULO ADMINISTRADOR SERIALES Y ESTIBAS CLARO - 28 DE OCTUBRE DE 2015//////////////////////////////////////////////////
            container.RegisterType<IAdminEstibasView, AdminEstibasView>();
            container.RegisterType<IAdminEstibasPresenter, AdminEstibasPresenter>();
            container.RegisterType<IAdminEstibasModel, AdminEstibasModel>();
            //MODULO ADMINISTRADOR SERIALES Y ESTIBAS CLARO - 28 DE OCTUBRE DE 2015//////////////////////////////////////////////////

            container.RegisterType<IGeneradorEstibasView, GeneradorEstibasView>();
            container.RegisterType<IGeneradorEstibasPresenter, GeneradorEstibasPresenter>();
            container.RegisterType<IGeneradorEstibasModel, GeneradorEstibasModel>();

            container.RegisterType<IBodegasView, BodegasView>();
            container.RegisterType<IBodegasPresenter, BodegasPresenter>();
            container.RegisterType<IBodegasModel, BodegasModel>();

            container.RegisterType<IMoverMercanciaView, MoverMercanciaView>();
            container.RegisterType<IMoverMercanciaPresenter, MoverMercanciaPresenter>();
            container.RegisterType<IMoverMercanciaModel, MoverMercanciaModel>();

            container.RegisterType<INoProcesablesView, NoProcesablesView>();
            container.RegisterType<INoProcesablesPresenter, NoProcesablesPresenter>();
            container.RegisterType<INoProcesablesModel, NoProcesablesModel>();

            container.RegisterType<IDiagnosticoView, DiagnosticoView>();
            container.RegisterType<IDiagnosticoPresenter, DiagnosticoPresenter>();
            container.RegisterType<IDiagnosticoModel, DiagnosticoModel>();

            container.RegisterType<IEnsambleView, EnsambleView>();
            container.RegisterType<IEnsamblePresenter, EnsamblePresenter>();
            container.RegisterType<IEnsambleModel, EnsambleModel>();

            container.RegisterType<IEtiquetadosView, EtiquetadosView>();
            container.RegisterType<IEtiquetadosPresenter, EtiquetadosPresenter>();
            container.RegisterType<IEtiquetadosModel, EtiquetadosModel>();

            container.RegisterType<IVerificacionView, VerificacionView>();
            container.RegisterType<IVerificacionPresenter, VerificacionPresenter>();
            container.RegisterType<IVerificacionModel, VerificacionModel>();

            container.RegisterType<IReparacionesView, ReparacionesView>();
            container.RegisterType<IReparacionesPresenter, ReparacionesPresenter>();
            container.RegisterType<IReparacionesModel, ReparacionesModel>();

            container.RegisterType<IEmpaqueView, EmpaqueView>();
            container.RegisterType<IEmpaquePresenter, EmpaquePresenter>();
            container.RegisterType<IEmpaqueModel, EmpaqueModel>();

            container.RegisterType<IDespachoView, DespachoView>();
            container.RegisterType<IDespachoPresenter, DespachoPresenter>();
            container.RegisterType<IDespachoModel, DespachoModel>();

            container.RegisterType<IMoverProductoView, MoverProductoView>();
            container.RegisterType<IMoverProductoPresenter, MoverProductoPresenter>();
            container.RegisterType<IMoverProductoModel, MoverProductoModel>();

            //container.RegisterType<IConfirmarReciboView, ConfirmarReciboView>();
            //container.RegisterType<IConfirmarReciboPresenter, ConfirmarReciboPresenter>();
            //container.RegisterType<IConfirmarReciboModel, ConfirmarReciboModel>();

            container.RegisterType<IDataInformationView, DataInformationView>();
            container.RegisterType<IDataInformationPresenter, DataInformationPresenter>();
            container.RegisterType<IDataInformationModel, DataInformationModel>();

            container.RegisterType<IDespachosView, DespachosView>();
            container.RegisterType<IDespachosPresenter, DespachosPresenter>();
            container.RegisterType<IDespachosModel, DespachosModel>();

            container.RegisterType<IAdminInformationView, AdminInformationView>();
            container.RegisterType<IAdminInformationPresenter, AdminInformationPresenter>();
            container.RegisterType<IAdminInformationModel, AdminInformationModel>();

            container.RegisterType<IImpresionEtiquetasView, ImpresionEtiquetasView>();
            container.RegisterType<IImpresionEtiquetasPresenter, ImpresionEtiquetasPresenter>();
            container.RegisterType<IImpresionEtiquetasModel, ImpresionEtiquetasModel>();

            container.RegisterType<IAdminInformationConsultaView, AdminInformationConsultaView>();
            container.RegisterType<IAdminInformationConsultaPresenter, AdminInformationConsultaPresenter>();
            container.RegisterType<IAdminInformationConsultaModel, AdminInformationConsultaModel>();

            container.RegisterType<IValidadorSerialesView, ValidadorSerialesView>();
            container.RegisterType<IValidadorSerialesPresenter, ValidadorSerialesPresenter>();
            container.RegisterType<IValidadorSerialesModel, ValidadorSerialesModel>();

            #endregion


            #region Register - Master


            container.RegisterType<IBinView, BinView>();
            container.RegisterType<IBinPresenter, BinPresenter>();
            container.RegisterType<IBinModel, BinModel>();


            container.RegisterType<ICompanyView, CompanyView>();
            container.RegisterType<ICompanyPresenter, CompanyPresenter>();
            container.RegisterType<ICompanyModel, CompanyModel>();

            container.RegisterType<IConnectionView, ConnectionView>();
            container.RegisterType<IConnectionPresenter, ConnectionPresenter>();
            container.RegisterType<IConnectionModel, ConnectionModel>();

            container.RegisterType<ILocationView, LocationView>();
            container.RegisterType<ILocationPresenter, LocationPresenter>();
            container.RegisterType<ILocationModel, LocationModel>();

            container.RegisterType<ICustomProcessView, CustomProcessView>();
            container.RegisterType<ICustomProcessPresenter, CustomProcessPresenter>();
            container.RegisterType<ICustomProcessModel, CustomProcessModel>();

            container.RegisterType<IUnitView, UnitView>();
            container.RegisterType<IUnitPresenter, UnitPresenter>();
            container.RegisterType<IUnitModel, UnitModel>();

            container.RegisterType<IAccountView, AccountView>();
            container.RegisterType<IAccountPresenter, AccountPresenter>();
            container.RegisterType<IAccountModel, AccountModel>();

            container.RegisterType<IC_CasNumberView, C_CasNumberView>();
            container.RegisterType<IC_CasNumberPresenter, C_CasNumberPresenter>();
            container.RegisterType<IC_CasNumberModel, C_CasNumberModel>();

            container.RegisterType<IMetaTypeView, MetaTypeView>();
            container.RegisterType<IMetaTypePresenter, MetaTypePresenter>();
            container.RegisterType<IMetaTypeModel, MetaTypeModel>();

            #endregion


            #region Register - General


            //container.RegisterType<IClassEntityView, ClassEntityView>();
            //container.RegisterType<IClassEntityPresenter, ClassEntityPresenter>();
            //container.RegisterType<IClassEntityModel, ClassEntityModel>();


            //container.RegisterType<IDocumentClassView, DocumentClassView>();
            //container.RegisterType<IDocumentClassPresenter, DocumentClassPresenter>();
            //container.RegisterType<IDocumentClassModel, DocumentClassModel>();


            container.RegisterType<IDocumentConceptView, DocumentConceptView>();
            container.RegisterType<IDocumentConceptPresenter, DocumentConceptPresenter>();
            container.RegisterType<IDocumentConceptModel, DocumentConceptModel>();


            //container.RegisterType<IDocumentTypeView, DocumentTypeView>();
            //container.RegisterType<IDocumentTypePresenter, DocumentTypePresenter>();
            //container.RegisterType<IDocumentTypeModel, DocumentTypeModel>();


            container.RegisterType<IDocumentTypeSequenceView, DocumentTypeSequenceView>();
            container.RegisterType<IDocumentTypeSequencePresenter, DocumentTypeSequencePresenter>();
            container.RegisterType<IDocumentTypeSequenceModel, DocumentTypeSequenceModel>();

            //container.RegisterType<IReportDocumentView, ReportDocumentView>();
            //container.RegisterType<IReportDocumentPresenter, ReportDocumentPresenter>();
            //container.RegisterType<IReportDocumentModel, ReportDocumentModel>();


            //container.RegisterType<IGroupCriteriaView, GroupCriteriaView>();
            //container.RegisterType<IGroupCriteriaPresenter, GroupCriteriaPresenter>();
            //container.RegisterType<IGroupCriteriaModel, GroupCriteriaModel>();


            //container.RegisterType<IGroupCriteriaDetailView, GroupCriteriaDetailView>();
            //container.RegisterType<IGroupCriteriaDetailPresenter, GroupCriteriaDetailPresenter>();
            //container.RegisterType<IGroupCriteriaDetailModel, GroupCriteriaDetailModel>();


            //container.RegisterType<IGroupCriteriaRelationView, GroupCriteriaRelationView>();
            //container.RegisterType<IGroupCriteriaRelationPresenter, GroupCriteriaRelationPresenter>();
            //container.RegisterType<IGroupCriteriaRelationModel, GroupCriteriaRelationModel>();


            //container.RegisterType<IGroupCriteriaRelationDataView, GroupCriteriaRelationDataView>();
            //container.RegisterType<IGroupCriteriaRelationDataPresenter, GroupCriteriaRelationDataPresenter>();
            //container.RegisterType<IGroupCriteriaRelationDataModel, GroupCriteriaRelationDataModel>();


            container.RegisterType<ILabelMappingView, LabelMappingView>();
            container.RegisterType<ILabelMappingPresenter, LabelMappingPresenter>();
            container.RegisterType<ILabelMappingModel, LabelMappingModel>();


            container.RegisterType<ILabelTemplateView, LabelTemplateView>();
            container.RegisterType<ILabelTemplatePresenter, LabelTemplatePresenter>();
            container.RegisterType<ILabelTemplateModel, LabelTemplateModel>();


            //container.RegisterType<IMeasureTypeView, MeasureTypeView>();
            //container.RegisterType<IMeasureTypePresenter, MeasureTypePresenter>();
            //container.RegisterType<IMeasureTypeModel, MeasureTypeModel>();


            //container.RegisterType<IMeasureUnitView, MeasureUnitView>();
            //container.RegisterType<IMeasureUnitPresenter, MeasureUnitPresenter>();
            //container.RegisterType<IMeasureUnitModel, MeasureUnitModel>();


            //container.RegisterType<IMeasureUnitConvertionView, MeasureUnitConvertionView>();
            //container.RegisterType<IMeasureUnitConvertionPresenter, MeasureUnitConvertionPresenter>();
            //container.RegisterType<IMeasureUnitConvertionModel, MeasureUnitConvertionModel>();

            container.RegisterType<IMessageRuleByCompanyView, MessageRuleByCompanyView>();
            container.RegisterType<IMessageRuleByCompanyPresenter, MessageRuleByCompanyPresenter>();
            container.RegisterType<IMessageRuleByCompanyModel, MessageRuleByCompanyModel>();

            container.RegisterType<IShippingMethodView, ShippingMethodView>();
            container.RegisterType<IShippingMethodPresenter, ShippingMethodPresenter>();
            container.RegisterType<IShippingMethodModel, ShippingMethodModel>();

            //container.RegisterType<IStatusView, StatusView>();
            //container.RegisterType<IStatusPresenter, StatusPresenter>();
            //container.RegisterType<IStatusModel, StatusModel>();


            //container.RegisterType<IStatusTypeView, StatusTypeView>();
            //container.RegisterType<IStatusTypePresenter, StatusTypePresenter>();
            //container.RegisterType<IStatusTypeModel, StatusTypeModel>();

            #endregion


            #region Register - Profile


            container.RegisterType<IConfigOptionView, ConfigOptionView>();
            container.RegisterType<IConfigOptionPresenter, ConfigOptionPresenter>();
            container.RegisterType<IConfigOptionModel, ConfigOptionModel>();


            //container.RegisterType<IConfigTypeView, ConfigTypeView>();
            //container.RegisterType<IConfigTypePresenter, ConfigTypePresenter>();
            //container.RegisterType<IConfigTypeModel, ConfigTypeModel>();


            container.RegisterType<IMenuOptionView, MenuOptionView>();
            container.RegisterType<IMenuOptionPresenter, MenuOptionPresenter>();
            container.RegisterType<IMenuOptionModel, MenuOptionModel>();


            //container.RegisterType<IMenuOptionByRolView, MenuOptionByRolView>();
            //container.RegisterType<IMenuOptionByRolPresenter, MenuOptionByRolPresenter>();
            //container.RegisterType<IMenuOptionByRolModel, MenuOptionByRolModel>();


            //container.RegisterType<IMenuOptionTypeView, MenuOptionTypeView>();
            //container.RegisterType<IMenuOptionTypePresenter, MenuOptionTypePresenter>();
            //container.RegisterType<IMenuOptionTypeModel, MenuOptionTypeModel>();


            container.RegisterType<IRolView, RolView>();
            container.RegisterType<IRolPresenter, RolPresenter>();
            container.RegisterType<IRolModel, RolModel>();


            container.RegisterType<ISysUserView, SysUserView>();
            container.RegisterType<ISysUserPresenter, SysUserPresenter>();
            container.RegisterType<ISysUserModel, SysUserModel>();






            #endregion


            #region Register - Trace

            /*
            container.RegisterType<IDocumentView, DocumentView>();
            container.RegisterType<IDocumentPresenter, DocumentPresenter>();
            container.RegisterType<IDocumentModel, DocumentModel>();


            container.RegisterType<IDocumentAddressView, DocumentAddressView>();
            container.RegisterType<IDocumentAddressPresenter, DocumentAddressPresenter>();
            container.RegisterType<IDocumentAddressModel, DocumentAddressModel>();


            container.RegisterType<IDocumentLineView, DocumentLineView>();
            container.RegisterType<IDocumentLinePresenter, DocumentLinePresenter>();
            container.RegisterType<IDocumentLineModel, DocumentLineModel>();



            container.RegisterType<ILabelView, LabelView>();
            container.RegisterType<ILabelPresenter, LabelPresenter>();
            container.RegisterType<ILabelModel, LabelModel>();


            container.RegisterType<INodeView, NodeView>();
            container.RegisterType<INodePresenter, NodePresenter>();
            container.RegisterType<INodeModel, NodeModel>();


            container.RegisterType<INodeDocumentTypeView, NodeDocumentTypeView>();
            container.RegisterType<INodeDocumentTypePresenter, NodeDocumentTypePresenter>();
            container.RegisterType<INodeDocumentTypeModel, NodeDocumentTypeModel>();


            container.RegisterType<INodeExtensionView, NodeExtensionView>();
            container.RegisterType<INodeExtensionPresenter, NodeExtensionPresenter>();
            container.RegisterType<INodeExtensionModel, NodeExtensionModel>();


            container.RegisterType<INodeExtensionTraceView, NodeExtensionTraceView>();
            container.RegisterType<INodeExtensionTracePresenter, NodeExtensionTracePresenter>();
            container.RegisterType<INodeExtensionTraceModel, NodeExtensionTraceModel>();


            container.RegisterType<INodeRouteView, NodeRouteView>();
            container.RegisterType<INodeRoutePresenter, NodeRoutePresenter>();
            container.RegisterType<INodeRouteModel, NodeRouteModel>();



            container.RegisterType<ITaskDocumentRelationView, TaskDocumentRelationView>();
            container.RegisterType<ITaskDocumentRelationPresenter, TaskDocumentRelationPresenter>();
            container.RegisterType<ITaskDocumentRelationModel, TaskDocumentRelationModel>();


            container.RegisterType<ITaskByUserView, TaskByUserView>();
            container.RegisterType<ITaskByUserPresenter, TaskByUserPresenter>();
            container.RegisterType<ITaskByUserModel, TaskByUserModel>();

            */
            #endregion


            #region Register Process


            container.RegisterType<ICustomProcessView, CustomProcessView>();
            container.RegisterType<ICustomProcessPresenter, CustomProcessPresenter>();
            container.RegisterType<ICustomProcessModel, CustomProcessModel>();


            container.RegisterType<ICustomProcessActivityView, CustomProcessActivityView>();
            container.RegisterType<ICustomProcessActivityPresenter, CustomProcessActivityPresenter>();
            container.RegisterType<ICustomProcessActivityModel, CustomProcessActivityModel>();


            container.RegisterType<ICustomProcessRouteView, CustomProcessRouteView>();
            container.RegisterType<ICustomProcessRoutePresenter, CustomProcessRoutePresenter>();
            container.RegisterType<ICustomProcessRouteModel, CustomProcessRouteModel>();


            container.RegisterType<ICustomProcessContextView, CustomProcessContextView>();
            container.RegisterType<ICustomProcessContextPresenter, CustomProcessContextPresenter>();
            container.RegisterType<ICustomProcessContextModel, CustomProcessContextModel>();


            container.RegisterType<ICustomProcessContextByEntityView, CustomProcessContextByEntityView>();
            container.RegisterType<ICustomProcessContextByEntityPresenter, CustomProcessContextByEntityPresenter>();
            container.RegisterType<ICustomProcessContextByEntityModel, CustomProcessContextByEntityModel>();


            container.RegisterType<ICustomProcessTransitionView, CustomProcessTransitionView>();
            container.RegisterType<ICustomProcessTransitionPresenter, CustomProcessTransitionPresenter>();
            container.RegisterType<ICustomProcessTransitionModel, CustomProcessTransitionModel>();


            #endregion


            //Opcion de Menu comodin pra poder obtener el execute;
            /*
            Menu menuChild = new Menu();
            menuChild.Name = "";
            menuChild.Options = new List<MenuModule>();
            menuChild.Options.Add(new MenuModule
            {
                Name = "",
                Module = this,
                PresenterType = null
            });
            Menus.Add(menuChild);
            */



            IList<ModuleRegion> menuOptions = Util.GetMenuOptionsV2(this);

            foreach (ModuleRegion modreg in menuOptions)
                moduleRegions.Add(modreg);

            /*
            ModuleRegion moduleQueries = new ModuleRegion();
            moduleQueries.Name = "List of Reports";
            moduleQueries.Description = "En este modulo usted podra ejecutar los diferentes reportes de la aplicación";
            moduleQueries.Options.Add(new ModuleSubmenu() { Name = "Consultas", Module = this, PresenterType = typeof(PrintingPresenter), Image = GetImage("/WpfFront;component/Images/Icons/48x48/Bin.png") });
            
            moduleRegions.Add(moduleQueries);

            moduleQueries.Name = "List of Reports 2 Old";
            moduleQueries.Description = "En este modulo usted podra ejecutar los diferentes reportes de la aplicación";
            moduleQueries.Options.Add(new ModuleSubmenu() { Name = "Consultas", Module = this, PresenterType = typeof(ChangeLocationPresenter), Image = GetImage("/WpfFront;component/Images/Icons/48x48/Bin.png") });
            
            moduleRegions.Add(moduleQueries);
            */

        }


        #region IModule Members Presenter


        public Object Execute(Type typePresenter, ToolWindow window)
        {
            switch (typePresenter.Name)
            {

                #region Execute - IQ

                case "MoverProductoPresenter":
                    MoverProductoPresenter MoverProducto = container.Resolve<MoverProductoPresenter>();
                    MoverProducto.Window = window;
                    return MoverProducto.View;

                case "EntradaAlmacenPresenter":
                    EntradaAlmacenPresenter EntradaAlmacen = container.Resolve<EntradaAlmacenPresenter>();
                    EntradaAlmacen.Window = window;
                    return EntradaAlmacen.View;

                case "EntradaAlmacenV2Presenter":
                    EntradaAlmacenV2Presenter EntradaAlmacenV2 = container.Resolve<EntradaAlmacenV2Presenter>();
                    EntradaAlmacenV2.Window = window;
                    return EntradaAlmacenV2.View;

                ////////// 18 DE FEBRERO DE 2015 /////////////
                case "ConfirmacionIntermediaPresenter":
                    ConfirmacionIntermediaPresenter ConfirmacionIntermedia = container.Resolve<ConfirmacionIntermediaPresenter>();
                    ConfirmacionIntermedia.Window = window;
                    return ConfirmacionIntermedia.View;
                ////////// 18 DE FEBRERO DE 2015 /////////////

                ////////// 23 DE FEBRERO DE 2015 /////////////
                case "ActualizacionRRPresenter":
                    ActualizacionRRPresenter ActualizacionRRPresenter = container.Resolve<ActualizacionRRPresenter>();
                    ActualizacionRRPresenter.Window = window;
                    return ActualizacionRRPresenter.View;
                ////////// 23 DE FEBRERO DE 2015 /////////////

                ////////// 25 DE MAYO DE 2015 /////////////
                case "NovedadesPresenter":
                    NovedadesPresenter NovedadesPresenterP = container.Resolve<NovedadesPresenter>();
                    NovedadesPresenterP.Window = window;
                    return NovedadesPresenterP.View;
                ////////// 23 DE MAYO DE 2015 /////////////

                //MODULO ADMINISTRADOR SERIALES Y ESTIBAS CLARO - 28 DE OCTUBRE DE 2015//////////////////////////////////////////////////
                case "AdministradorPresenter":
                    AdministradorPresenter Administrador = container.Resolve<AdministradorPresenter>();
                    Administrador.Window = window;
                    return Administrador.View;
                //MODULO ADMINISTRADOR SERIALES Y ESTIBAS CLARO - 28 DE OCTUBRE DE 2015//////////////////////////////////////////////////

                case "AdminEstibasPresenter":
                    AdminEstibasPresenter AdminEstibas = container.Resolve<AdminEstibasPresenter>();
                    AdminEstibas.Window = window;
                    return AdminEstibas.View;

                case "GeneradorEstibasPresenter":
                    GeneradorEstibasPresenter GeneradorEstibas = container.Resolve<GeneradorEstibasPresenter>();
                    GeneradorEstibas.Window = window;
                    return GeneradorEstibas.View;

                case "BodegasPresenter":
                    BodegasPresenter Bodegas = container.Resolve<BodegasPresenter>();
                    Bodegas.Window = window;
                    return Bodegas.View;

                case "MoverMercanciaPresenter":
                    MoverMercanciaPresenter MoverMercancia = container.Resolve<MoverMercanciaPresenter>();
                    MoverMercancia.Window = window;
                    return MoverMercancia.View;

                //case "ConfirmarReciboPresenter":
                //    ConfirmarReciboPresenter ConfirmarRecibo = container.Resolve<ConfirmarReciboPresenter>();
                //    ConfirmarRecibo.Window = window;
                //    return ConfirmarRecibo.View;

                case "NoProcesablesPresenter":
                    NoProcesablesPresenter NoProcesables = container.Resolve<NoProcesablesPresenter>();
                    NoProcesables.Window = window;
                    return NoProcesables.View;

                case "DiagnosticoPresenter":
                    DiagnosticoPresenter Diagnostico = container.Resolve<DiagnosticoPresenter>();
                    Diagnostico.Window = window;
                    return Diagnostico.View;

                case "EnsamblePresenter":
                    EnsamblePresenter Ensamble = container.Resolve<EnsamblePresenter>();
                    Ensamble.Window = window;
                    return Ensamble.View;

                case "EtiquetadosPresenter":
                    EtiquetadosPresenter Etiquetados = container.Resolve<EtiquetadosPresenter>();
                    Etiquetados.Window = window;
                    return Etiquetados.View;

                case "VerificacionPresenter":
                    VerificacionPresenter Verificacion = container.Resolve<VerificacionPresenter>();
                    Verificacion.Window = window;
                    return Verificacion.View;

                case "ReparacionesPresenter":
                    ReparacionesPresenter Reparaciones = container.Resolve<ReparacionesPresenter>();
                    Reparaciones.Window = window;
                    return Reparaciones.View;

                case "EmpaquePresenter":
                    EmpaquePresenter Empaque = container.Resolve<EmpaquePresenter>();
                    Empaque.Window = window;
                    return Empaque.View;

                case "DespachoPresenter":
                    DespachoPresenter Despacho = container.Resolve<DespachoPresenter>();
                    Despacho.Window = window;
                    return Despacho.View;

                case "DataInformationPresenter":
                    DataInformationPresenter DataInformation = container.Resolve<DataInformationPresenter>();
                    DataInformation.Window = window;
                    return DataInformation.View;

                case "DespachosPresenter":
                    DespachosPresenter Despachos = container.Resolve<DespachosPresenter>();
                    Despachos.Window = window;
                    return Despachos.View;

                case "AdminInformationPresenter":
                    AdminInformationPresenter AdminInformation = container.Resolve<AdminInformationPresenter>();
                    AdminInformation.Window = window;
                    return AdminInformation.View;

                case "ImpresionEtiquetasPresenter":
                    ImpresionEtiquetasPresenter ImpresionEtiquetas = container.Resolve<ImpresionEtiquetasPresenter>();
                    ImpresionEtiquetas.Window = window;
                    return ImpresionEtiquetas.View;

                case "AdminInformationConsultaPresenter":
                    AdminInformationConsultaPresenter AdminInformationConsulta = container.Resolve<AdminInformationConsultaPresenter>();
                    AdminInformationConsulta.Window = window;
                    return AdminInformationConsulta.View;

                case "ValidadorSerialesPresenter":
                    ValidadorSerialesPresenter ValidadorSerialesConsulta = container.Resolve<ValidadorSerialesPresenter>();
                    ValidadorSerialesConsulta.Window = window;
                    return ValidadorSerialesConsulta.View;

                #endregion

                case "OrderMngrV2Presenter":
                    OrderMngrV2Presenter presenOM = container.Resolve<OrderMngrV2Presenter>();
                    presenOM.Window = window;
                    return presenOM.View;

                case "BookingMngrPresenter":
                    BookingMngrPresenter presenterb = container.Resolve<BookingMngrPresenter>();
                    presenterb.Window = window;
                    return presenterb.View;

                case "ShipRoutePresenter":
                    ShipRoutePresenter presenter31 = container.Resolve<ShipRoutePresenter>();
                    presenter31.Window = window;
                    return presenter31.View;

                case "DocumentManagerPresenter":
                    DocumentManagerPresenter presenter30 = container.Resolve<DocumentManagerPresenter>();
                    presenter30.Window = window;
                    return presenter30.View;

                case "SchedulePresenter":
                    ISchedulePresenter presenter29y = container.Resolve<SchedulePresenter>();
                    presenter29y.Window = window;
                    return presenter29y.View;


                case "ProductCategoryPresenter":
                    IProductCategoryPresenter presenter29c = container.Resolve<ProductCategoryPresenter>();
                    presenter29c.Window = window;
                    return presenter29c.View;

                case "InventoryCountPresenter":
                    IInventoryCountPresenter presenter29 = container.Resolve<InventoryCountPresenter>();
                    presenter29.Window = window;
                    return presenter29.View;


                case "QueriesPresenter":
                    IQueriesPresenter presenter = container.Resolve<QueriesPresenter>();
                    presenter.Window = window;
                    return presenter.View;


                case "ReceivingPresenter":
                    IReceivingPresenter presenter1 = container.Resolve<ReceivingPresenter>();
                    presenter1.Window = window;
                    return presenter1.View;


                case "ChangeLocationPresenter":

                    IChangeLocationPresenter presenter2 = container.Resolve<ChangeLocationPresenter>();
                    presenter2.Window = window;
                    return presenter2.View;


                case "PrintingPresenter":
                    IPrintingPresenter presenter3 = container.Resolve<PrintingPresenter>();
                    presenter3.Window = window;
                    return presenter3.View;

                case "InventoryAdjustmentPresenter":
                    IInventoryAdjustmentPresenter presenter4 = container.Resolve<InventoryAdjustmentPresenter>();
                    presenter4.Window = window;
                    return presenter4.View;



                case "ShippingPresenter":
                    IShippingPresenter presenter5 = container.Resolve<ShippingPresenter>();
                    presenter5.Window = window;
                    return presenter5.View;

                case "ZoneManagerPresenter":
                    IZoneManagerPresenter presenter6 = container.Resolve<ZoneManagerPresenter>();
                    presenter6.Window = window;
                    return presenter6.View;

                case "ShippingConsolePresenter":
                    IShippingConsolePresenter presenter7 = container.Resolve<ShippingConsolePresenter>();
                    presenter7.Window = window;
                    return presenter7.View;

                case "IntroInventoryPresenter":
                    IIntroInventoryPresenter presenter8 = container.Resolve<IntroInventoryPresenter>();
                    presenter8.Window = window;
                    return presenter8.View;

                case "ReceivingCubePresenter":
                    IReceivingCubePresenter presenter9 = container.Resolve<ReceivingCubePresenter>();
                    presenter9.Window = window;
                    return presenter9.View;



                case "CrossDockPresenter":
                    ICrossDockPresenter presenter10 = container.Resolve<CrossDockPresenter>();
                    presenter10.Window = window;
                    presenter10.SetShowProcess(false);
                    presenter10.ShowProcessPanel();
                    return presenter10.View;


                case "KitAssemblyPresenter":
                    IKitAssemblyPresenter presenter11 = container.Resolve<KitAssemblyPresenter>();
                    presenter11.Window = window;
                    return presenter11.View;

                //Febrero 15 de 2011
                case "KitAssemblyV2Presenter":
                    IKitAssemblyV2Presenter KitAssemblyPresenter = container.Resolve<KitAssemblyV2Presenter>();
                    KitAssemblyPresenter.Window = window;
                    return KitAssemblyPresenter.View;

                case "InventoryTaskPresenter":
                    IInventoryTaskPresenter presenter12 = container.Resolve<InventoryTaskPresenter>();
                    presenter12.Window = window;
                    return presenter12.View;


                case "BasicSetupPresenter":
                    IBasicSetupPresenter presenter13 = container.Resolve<BasicSetupPresenter>();
                    presenter13.Window = window;
                    return presenter13.View;

                case "BasicProcessPresenter":
                    IBasicProcessPresenter presenter14 = container.Resolve<BasicProcessPresenter>();
                    presenter14.Window = window;
                    return presenter14.View;

                case "TrackOptionPresenter":
                    ITrackOptionPresenter presenter15 = container.Resolve<TrackOptionPresenter>();
                    presenter15.Window = window;
                    return presenter15.View;

                case "ReportPresenter":
                    IReportPresenter presenter16 = container.Resolve<ReportPresenter>();
                    presenter16.Window = window;
                    return presenter16.View;

                case "ShipperMngrPresenter":
                    IShipperMngrPresenter pship = container.Resolve<ShipperMngrPresenter>();
                    pship.Window = window;
                    return pship.View;

                case "FileProcessPresenter":

                    IFileProcessPresenter presenter16x = container.Resolve<FileProcessPresenter>();
                    presenter16x.Window = window;
                    return presenter16x.View;

                #region Execute - Master



                case "BinPresenter":

                    IBinPresenter presenter17 = container.Resolve<BinPresenter>();
                    presenter17.Window = window;
                    return presenter17.View;


                case "CompanyPresenter":

                    ICompanyPresenter presenter18 = container.Resolve<CompanyPresenter>();
                    presenter18.Window = window;
                    return presenter18.View;

                case "MetaTypePresenter":
                    IMetaTypePresenter MetaTypePresenter = container.Resolve<MetaTypePresenter>();
                    MetaTypePresenter.Window = window;
                    return MetaTypePresenter.View;



                //case "ConnectionPresenter":
                //
                //    IConnectionPresenter presenter = container.Resolve<ConnectionPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(ConnectionPresenter).Name, true);
                //    break;
                //



                case "LocationPresenter":

                    ILocationPresenter presenter19 = container.Resolve<LocationPresenter>();
                    presenter19.Window = window;
                    return presenter19.View;


                case "ProductPresenter":

                    IProductPresenter presenter20 = container.Resolve<ProductPresenter>();
                    presenter20.Window = window;
                    return presenter20.View;


                case "AccountPresenter":

                    IAccountPresenter presenter20a = container.Resolve<AccountPresenter>();
                    presenter20a.Window = window;
                    return presenter20a.View;


                //case "TerminalPresenter":
                //
                //    ITerminalPresenter presenter = container.Resolve<TerminalPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(TerminalPresenter).Name, true);
                //    break;
                //

                case "UnitPresenter":

                    IUnitPresenter presenter21 = container.Resolve<UnitPresenter>();
                    presenter21.Window = window;
                    return presenter21.View;


                //case "UnitProductEquivalencePresenter":
                //
                //    IUnitProductEquivalencePresenter presenter = container.Resolve<UnitProductEquivalencePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(UnitProductEquivalencePresenter).Name, true);
                //    break;
                //

                //case "UnitProductLogisticPresenter":
                //
                //    IUnitProductLogisticPresenter presenter = container.Resolve<UnitProductLogisticPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(UnitProductLogisticPresenter).Name, true);
                //    break;
                //

                //case "UnitProductRelationPresenter":
                //
                //    IUnitProductRelationPresenter presenter = container.Resolve<UnitProductRelationPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(UnitProductRelationPresenter).Name, true);
                //    break;
                //

                //case "VehiclePresenter":
                //
                //    IVehiclePresenter presenter = container.Resolve<VehiclePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(VehiclePresenter).Name, true);
                //    break;
                //

                case "CustomProcessPresenter":

                    ICustomProcessPresenter presenter22 = container.Resolve<CustomProcessPresenter>();
                    presenter22.Window = window;
                    return presenter22.View;

                case "KitPresenter":

                    IKitPresenter presenter22a = container.Resolve<KitPresenter>();
                    presenter22a.Window = window;
                    return presenter22a.View;


                case "C_CasNumberPresenter":

                    IC_CasNumberPresenter ccnp = container.Resolve<C_CasNumberPresenter>();
                    ccnp.Window = window;
                    return ccnp.View;


                #endregion

                #region Execute - General

                //case "ClassEntityPresenter":
                //
                //    IClassEntityPresenter presenter = container.Resolve<ClassEntityPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(ClassEntityPresenter).Name, true);
                //    break;
                //

                //case "DocumentClassPresenter":
                //
                //    IDocumentClassPresenter presenter = container.Resolve<DocumentClassPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentClassPresenter).Name, true);
                //    break;
                //

                //case "DocumentConceptPresenter":
                //
                //    IDocumentConceptPresenter presenter = container.Resolve<DocumentConceptPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentConceptPresenter).Name, true);
                //    break;
                //

                //case "DocumentTypePresenter":
                //
                //    IDocumentTypePresenter presenter = container.Resolve<DocumentTypePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentTypePresenter).Name, true);
                //    break;
                //

                case "DocumentTypeSequencePresenter":

                    IDocumentTypeSequencePresenter presenter23 = container.Resolve<DocumentTypeSequencePresenter>();
                    presenter23.Window = window;
                    return presenter23.View;


                //case "GroupCriteriaPresenter":
                //
                //    IGroupCriteriaPresenter presenter = container.Resolve<GroupCriteriaPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(GroupCriteriaPresenter).Name, true);
                //    break;
                //

                //case "GroupCriteriaDetailPresenter":
                //
                //    IGroupCriteriaDetailPresenter presenter = container.Resolve<GroupCriteriaDetailPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(GroupCriteriaDetailPresenter).Name, true);
                //    break;
                //

                //case "GroupCriteriaRelationPresenter":
                //
                //    IGroupCriteriaRelationPresenter presenter = container.Resolve<GroupCriteriaRelationPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(GroupCriteriaRelationPresenter).Name, true);
                //    break;
                //

                //case "GroupCriteriaRelationDataPresenter":
                //
                //    IGroupCriteriaRelationDataPresenter presenter = container.Resolve<GroupCriteriaRelationDataPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(GroupCriteriaRelationDataPresenter).Name, true);
                //    break;
                //

                //case "LabelMappingPresenter":
                //
                //    ILabelMappingPresenter presenter = container.Resolve<LabelMappingPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(LabelMappingPresenter).Name, true);
                //    break;
                //

                //case "LabelTemplatePresenter":
                //
                //    ILabelTemplatePresenter presenter = container.Resolve<LabelTemplatePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(LabelTemplatePresenter).Name, true);
                //    break;
                //

                //case "MeasureTypePresenter":
                //
                //    IMeasureTypePresenter presenter = container.Resolve<MeasureTypePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(MeasureTypePresenter).Name, true);
                //    break;
                //

                //case "MeasureUnitPresenter":
                //
                //    IMeasureUnitPresenter presenter = container.Resolve<MeasureUnitPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(MeasureUnitPresenter).Name, true);
                //    break;
                //

                //case "MeasureUnitConvertionPresenter":
                //
                //    IMeasureUnitConvertionPresenter presenter = container.Resolve<MeasureUnitConvertionPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(MeasureUnitConvertionPresenter).Name, true);
                //    break;
                //


                //case "StatusPresenter":
                //
                //    IStatusPresenter presenter = container.Resolve<StatusPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(StatusPresenter).Name, true);
                //    break;
                //

                //case "StatusTypePresenter":
                //
                //    IStatusTypePresenter presenter = container.Resolve<StatusTypePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(StatusTypePresenter).Name, true);
                //    break;
                //


                #endregion

                #region Execute Profile


                case "ConfigOptionPresenter":
                    IConfigOptionPresenter presenter24 = container.Resolve<ConfigOptionPresenter>();
                    presenter24.Window = window;
                    return presenter24.View;


                //case "ConfigTypePresenter":
                //
                //    IConfigTypePresenter presenter = container.Resolve<ConfigTypePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(ConfigTypePresenter).Name, true);
                //    break;
                //

                case "MenuOptionPresenter":

                    IMenuOptionPresenter presenter25 = container.Resolve<MenuOptionPresenter>();
                    presenter25.Window = window;
                    return presenter25.View;


                //case "MenuOptionByRolPresenter":
                //
                //    IMenuOptionByRolPresenter presenter = container.Resolve<MenuOptionByRolPresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(MenuOptionByRolPresenter).Name, true);
                //    break;
                //

                //case "MenuOptionTypePresenter":
                //
                //    IMenuOptionTypePresenter presenter = container.Resolve<MenuOptionTypePresenter>();
                //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(MenuOptionTypePresenter).Name, true);
                //    break;
                //

                case "RolPresenter":

                    IRolPresenter presenter26 = container.Resolve<RolPresenter>();
                    presenter26.Window = window;
                    return presenter26.View;


                case "SysUserPresenter":

                    ISysUserPresenter presenter27 = container.Resolve<SysUserPresenter>();
                    presenter27.Window = window;
                    return presenter27.View;


                case "IqToolPresenter":
                    IIqToolPresenter presenter28 = container.Resolve<IqToolPresenter>();
                    presenter28.Window = window;
                    return presenter28.View;

            }

                #endregion


            #region Execute Trace


            //if (typePresenter == typeof(DocumentPresenter))
            //{
            //    IDocumentPresenter presenter = container.Resolve<DocumentPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentPresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(DocumentAddressPresenter))
            //{
            //    IDocumentAddressPresenter presenter = container.Resolve<DocumentAddressPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentAddressPresenter).Name, true);
            //    return;
            //}




            //if (typePresenter == typeof(DocumentLinePresenter))
            //{
            //    IDocumentLinePresenter presenter = container.Resolve<DocumentLinePresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(DocumentLinePresenter).Name, true);
            //    return;
            //}



            //if (typePresenter == typeof(LabelPresenter))
            //{
            //    ILabelPresenter presenter = container.Resolve<LabelPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(LabelPresenter).Name, true);
            //    return;
            //}


            //if (typePresenter == typeof(NodePresenter))
            //{
            //    INodePresenter presenter = container.Resolve<NodePresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(NodePresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(NodeDocumentTypePresenter))
            //{
            //    INodeDocumentTypePresenter presenter = container.Resolve<NodeDocumentTypePresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(NodeDocumentTypePresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(NodeExtensionPresenter))
            //{
            //    INodeExtensionPresenter presenter = container.Resolve<NodeExtensionPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(NodeExtensionPresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(NodeExtensionTracePresenter))
            //{
            //    INodeExtensionTracePresenter presenter = container.Resolve<NodeExtensionTracePresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(NodeExtensionTracePresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(NodeRoutePresenter))
            //{
            //    INodeRoutePresenter presenter = container.Resolve<NodeRoutePresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(NodeRoutePresenter).Name, true);
            //    return;
            //}



            //if (typePresenter == typeof(TaskDocumentRelationPresenter))
            //{
            //    ITaskDocumentRelationPresenter presenter = container.Resolve<TaskDocumentRelationPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(TaskDocumentRelationPresenter).Name, true);
            //    return;
            //}

            //if (typePresenter == typeof(TaskByUserPresenter))
            //{
            //    ITaskByUserPresenter presenter = container.Resolve<TaskByUserPresenter>();
            //    regionManager.Shell.ShowViewInShell(RegionNames.MainInformation, presenter.View, typeof(TaskByUserPresenter).Name, true);
            //    return;
            //}


            return null;

            #endregion

        }

        #endregion

        #region IModule Members

        public object ExecuteWebUrl(Type typePresenter, ToolWindow window, string WebUrl)
        {
            return new object();
        }

        #endregion


    }
}