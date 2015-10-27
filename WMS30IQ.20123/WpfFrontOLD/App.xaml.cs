using System.Windows;
using WpfFront.WMSBusinessService;
using System.DirectoryServices;
using System;
using System.Collections.Generic;
using WpfFront.Common;
using WpfFront.Services;
using Assergs.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Linq;
using System.Collections;
using WMComposite.Regions;
using WMComposite.Modularity;
using System.Threading;
using System.Globalization;





namespace WpfFront
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SysUser curUser;
        public static Company curCompany;
        public static Location curLocation;
        public static UserByRol curRol;
        public static bool RefreshMenu = true;
        //public static IList<UIMenu> userMenu;
        public static IList<MenuOption> curMenuOptions;
        public static IList<Printer> printerList;
        public static Type curPresenter;
        //public static bool IsConnectedToErpReceving;
        //public static bool IsConnectedToErpShipping;
        //public static bool IsConnectedToErpInventory;
        public static Dictionary<string, string> configOptions;
        public static string curAuthUser;
        public static string langCode;
        public static string currentLocation;


        //General Service Masters
        public static IList<Status> DocStatusList;
        public static IList<Status> EntityStatusList;
        public static IList<PickMethod> PickMethodList;
        public static IList<DocumentType> DocTypeList;
        public static IList<Location> LocationList;
        public static IList<Company> CompanyList;
        public static Hashtable BinDirectionList;
        public static IList<DataType> DataTypeList;
        public static IList<DocumentConcept> DocumentConceptList;
        public static IList<CustomProcess> CustomProcessList;
        public static IList<Connection> PrinterConnectionList;
        public static IList<ClassEntity> ClassEntityList;

        public static LabelTemplate defTemplate;
        public static string curVersion;
        public static bool showingReports = false;
        //public static IList<ShowData> lang;


        public App()
        {

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            //this.Resources = new OfficeStyle();

            SetLanguage();

            try
            {
                if (LogonActive() == true)
                    StartupContainer();
                else
                    Shutdown(1);
            }
            catch (Exception ex) {
                Util.ShowError("Please check the WCF service.\n" + Util.GetTechMessage(ex));
                Shutdown(1);
            } 
        }


        private void SetLanguage()
        {
            try { App.langCode = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "LANGUAGE" }).First().DefValue; }
            catch { App.langCode = "en-US"; }

            ResourceDictionary myLang = new ResourceDictionary();

            myLang.Source = new Uri("Language." + App.langCode + ".xaml", UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Add(myLang);

            ResourceDictionary common = new ResourceDictionary();
            common.Source = new Uri("Common\\SkinOne.xaml", UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Add(common);

        }



        private bool? LogonActive()
        {
            Splash splash = new Splash();
            splash.DataContext = new SplashModel();
            splash.Show();

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            LogOnScreen logon = new LogOnScreen();
            

#if DEBUG
            logon.HintVisible = true;
#endif
      
            splash.Close();
            return logon.ShowDialog();

        }

        /// <summary>
        /// Load the Bootstraper once the user is authenticated
        /// </summary>
        private void StartupContainer()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Splash splash = new Splash();
            splash.DataContext = new SplashModel();
            splash.Show();

            //Carga la configuracion del usuario validado
            LoadConfiguration();

            Application.Current.MainWindow = null;

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            //IShellPresenter shellApplication = bootstrapper.Container.Resolve<IShellPresenter>();
            //IModuleEnumerator moduleEnumerator = bootstrapper.Container.Resolve<IModuleEnumerator>();
            //ModuleInfo[] moduleInfo = moduleEnumerator.GetStartupLoadedModules();


            ////Hay un solo Modulo, y para ese Modulo se debe Depurar el Menu
            //foreach (ModuleInfo item in moduleInfo)
            //    shellApplication.Shell.Model.AddModule(item);

            splash.Close();
        }



        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
                throw e.Exception;

            else
            {
                Exception ex = e.Exception;
                string message = ex.Message;

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message = ex.Message + "\r\n\r\n" + message;
                }

                //Util.ShowError("Application: " + message);
                Util.WriteEventLog(message);
                e.Handled = true;
            }

        }


        public void LoadConfiguration()
        {
            //Carga los Maestros de Servicios
            Util.LoadServiceMasters();

            //LoadPrinters
            Util.LoadPrinters();


            App.configOptions = Util.GetConfigOptionValues();

            //Version
            App.curVersion = Util.GetConfigOption("VERSION");


            //LoadCulture
            //string culture = "";
            //try { culture = Util.GetConfigOption("LANGUAGE"); } //"es-CO"; 
            //catch { culture = WmsSetupValues.Language; }

            //Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            //Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);


            //Load Idioma // Codigos y Valores
            //lang = Util.LoadLanguage(culture);


        }

    }
}