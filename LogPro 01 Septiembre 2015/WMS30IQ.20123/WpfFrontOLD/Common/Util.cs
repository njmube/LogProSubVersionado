using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Printing; // The PrinterSettings class is located inside the Printing namespace
using System.Management;
using System.Reflection;
using System.Collections.Specialized;
using System.DirectoryServices;
using WpfFront.WMSBusinessService;
using WpfFront.Services;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media.Imaging;
using Assergs.Windows;
using System.Collections;
using System.Linq;
using WMComposite.Modularity;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Xceed.Wpf.DataGrid.Settings;
using WMComposite.Regions;
using System.Windows.Controls;
using System.Data;
using System.Media;


namespace WpfFront.Common
{
    public partial class Util
    {

        public static void ShowError(string customMsg)
        {
            //MessageBox.Show(customMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UtilWindow.ShowError(customMsg);
            SystemSounds.Hand.Play();
        }

        public static void ShowMessage(string msg)
        {
            //MessageBox.Show(msg, "Process Done", MessageBoxButton.OK, MessageBoxImage.Information);
            UtilWindow.ShowMessage(msg);
            SystemSounds.Asterisk.Play();
        }


        public static string CryptPasswd(string data, string cryptKey)
        {
            if (string.IsNullOrEmpty(data))
                return "";


            Crypto cpt = new Crypto(Crypto.SymmProvEnum.DES);
            return cpt.Encrypting(data, cryptKey);

        }


        public static string DeCryptPasswd(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            Crypto cpt = new Crypto(Crypto.SymmProvEnum.DES);
            return cpt.Decrypting(value, key);

        }



        public static void LoadPrinters()
        {
            //Llamado Asyncronico al cargue de las impresoras
            Thread th = new Thread(new ThreadStart(GetPrinterList));
            th.Start();
        }



        public static void GetPrinterList() //out IList<Printer> printerList
        {

            IList<Printer> printerList = new List<Printer>();
            string[] path;
            Printer curPrinter;

            try
            {

                using (ManagementClass printerClass = new ManagementClass("win32_printer"))
                {

                    ManagementObjectCollection printers = printerClass.GetInstances();
                    foreach (ManagementObject printer in printers)
                    {
                        curPrinter = new Printer { PrinterName = printer["Name"].ToString() };

                        if ((bool)printer["Shared"] == true)
                        {
                            path = printer["Name"].ToString().Split("\\".ToCharArray());
                            curPrinter.PrinterPath = printer["Name"].ToString().Replace(path[path.Length - 1], printer["ShareName"].ToString());

                        }
                        else
                            curPrinter.PrinterPath = printer["PortName"].ToString();


                        if ((bool)printer["Default"])
                            curPrinter.IsDefault = true;


                        printerList.Add(curPrinter);
                    }
                }


                //Add Connection Printerts
                foreach (Connection prn in App.PrinterConnectionList)
                    printerList.Add(new Printer { PrinterName = prn.Name, PrinterPath = prn.CnnString, FromServer = true });


                App.printerList = printerList;
                //return printerList;

            }
            catch { }
        }


        public static string GetPlainTextString(Stream stream)
        {
            int streamLength = Convert.ToInt32(stream.Length);
            byte[] file = new byte[streamLength];
            stream.Read(file, 0, streamLength);
            stream.Close();
            return Encoding.ASCII.GetString(file);
        }


        public static IList<ShowData> ToShowData(object label)
        {

            IList<ShowData> sd = new List<ShowData>();
            string dataValue;

            foreach (PropertyInfo property in label.GetType().GetProperties())
            {
                if (!property.Name.ToLower().EndsWith("id"))
                {
                    dataValue = GetValue(property, label, 0);
                    if (!string.IsNullOrEmpty(dataValue) && dataValue != "0")
                        try
                        {
                            sd.Add(new ShowData { DataKey = MaskKey(property.Name, label), DataValue = FormatValue(property.PropertyType.FullName, dataValue) });
                        }
                        catch { }
                }
            }


            return sd.OrderBy(f => f.DataKey).ToList();
        }


        //Metodo que apoya a ToShowData para obtener los dato de un objeto usando Reflection
        private static string GetValue(PropertyInfo property, object obj, int level)
        {
            try
            {

                if (level == 0)
                {
                    string fType = property.PropertyType.Name;

                    if (fType == "String" || fType.Contains("Decimal") || fType.Contains("Double") || fType.Contains("Nullable") || fType.Contains("Int") || fType.Contains("DateTime") || fType.Contains("Bool"))
                        return property.GetValue(obj, null).ToString();
                    else
                        return GetValue(property, property.GetValue(obj, null), 1);
                }
                else
                {
                    return obj.GetType().GetProperty("Name").GetValue(obj, null).ToString();
                }
            }
            catch { }

            return "";

        }

        // Devuelve la cadena enviada con un formato especial, según el tipo.
        public static string FormatValue(string type, string value)
        {
            string result = value;
            if (type.Contains("Date"))
                result = DateTime.Parse(value).ToShortDateString();
            else
                if (type.Contains("Int"))
                    result = int.Parse(value).ToString("###,###,###");
                else
                    if (type.Contains("Double"))
                        result = Double.Parse(value).ToString("###,###,###.##");

            return result;
        }


        public static string MaskKey(string keyVal, object objType)
        {
            if (objType.GetType() == typeof(Document))
            {

                if (((Document)objType).DocType.DocClass.DocClassID == SDocClass.Receiving)
                {
                    switch (keyVal)
                    {
                        case "Date1":
                            return "Document Date";

                        case "Date2":
                            return "Promised Date";

                        case "Date3":
                            return "Promised Ship";

                        case "Date4":
                            return "Required Date";

                        case "Date5":
                            return "Due Date";

                        default:
                            return keyVal;
                    }
                }
            }

            return keyVal;

        }


        //Obtiene la configuracion del sistema y 
        public static Dictionary<string, string> GetConfigOptionValues()
        {
            WMSServiceClient service = new WMSServiceClient();

            IList<ConfigType> list = service.GetConfigType(new ConfigType());
            IList<ConfigOptionByCompany> listOptions;
            IList<ConfigOption> generalOptions;

            Dictionary<String, String> curInternal = new Dictionary<string, string>();

            foreach (ConfigType cType in list)
            {

                if (cType.ConfigTypeID == 1)
                {   //general {
                    generalOptions = service.GetConfigOption(new ConfigOption { ConfigType = new ConfigType { ConfigTypeID = 1 } });

                    foreach (ConfigOption option in generalOptions.Where(f => f.ConfigType.ConfigTypeID == cType.ConfigTypeID))
                        curInternal.Add(option.Code, option.DefValue);

                }
                else
                {
                    listOptions = service.GetConfigOptionByCompany(new ConfigOptionByCompany { Company = App.curCompany });

                    foreach (ConfigOptionByCompany option in listOptions.Where(f => f.ConfigOption.ConfigType.ConfigTypeID == cType.ConfigTypeID))
                        curInternal.Add(option.ConfigOption.Code, option.Value);
                }

            }

            //Configuracion de algunos defaults
            try { App.defTemplate = service.GetLabelTemplate(new LabelTemplate { RowID = int.Parse(curInternal["PROTEMPL"]) }).First(); }
            catch { }


            return curInternal;
        }

        #region OldMenu

        /*
        public static IList<UIMenu> GetMenuOptions()
        {

            IList<UIMenu> menuList = new List<UIMenu>();
            IList<MenuOptionByRol> optionsList = null;

            //Crea el menu principal Con Los tipos de menu que debe Haber
            WMSServiceClient service = new WMSServiceClient();

            IList<MenuOptionType> typeList = service.GetMenuOptionType(new MenuOptionType());

            //Si es admin tiene permiso a todas las funcionalidades
            if (App.curRol.Rol.RolID == BasicRol.Admin)
            {
                //Guarda las opciones del menu en la session para reuso
                App.curMenuOptions = service.GetMenuOption(new MenuOption()).Where(f => f.Active != false).ToList();
            }
            else
            {
                //Lista de Opciones que tiene el usuario segun su rol en la compania

                //optionsList = App.curUser.UserRols.Where(f => f.RowID == App.curRol.RowID).First()
                //    .Rol.MenuOptions.Where(f => f.Company.CompanyID == App.curCompany.CompanyID && f.Status.StatusID == EntityStatus.Active && f.MenuOption.Active != false)
                //    .OrderBy(f => f.MenuOption.NumOrder).ToList();

                optionsList = service.GetMenuOptionByRol(new MenuOptionByRol
                {
                    Rol = App.curRol.Rol,
                    Company = App.curCompany
                }).Where(f => f.Status.StatusID == EntityStatus.Active && f.MenuOption.Active != false)
                .OrderBy(f => f.MenuOption.NumOrder).ToList();

                //Guarda las opciones del menu en la session para reuso
                App.curMenuOptions = optionsList.Select(f => f.MenuOption).ToList();

            }




            UIMenu menuChild = null;

            foreach (MenuOptionType mType in typeList)
            {

                menuChild = new UIMenu();
                menuChild.Name = mType.Name;
                menuChild.Options = new List<UIMenuOption>();

                if (App.curRol.Rol.RolID == BasicRol.Admin)
                {
                    // recorremos lista de opciones del rol, para organizar los menus
                    foreach (MenuOption mOption in App.curMenuOptions.Where(f => f.MenuOptionType.MenuOptionTypeID == mType.MenuOptionTypeID && f.OptionType.OpTypeID == OptionTypes.Application).OrderBy(f => f.NumOrder))
                    {
                        menuChild.Options.Add(new UIMenuOption
                        {
                            Name = mOption.Name,
                            PresenterType = Type.GetType("WpfFront.Presenters." + mOption.Url + ""),
                            Icon = new BitmapImage(new Uri(WmsSetupValues.IconPath48 + mOption.Icon, UriKind.Relative))
                        });

                    }
                }
                else
                {
                    // recorremos lista de opciones del rol, para organizar los menus
                    foreach (MenuOptionByRol mOption in optionsList.Where(f => f.MenuOption.MenuOptionType.MenuOptionTypeID == mType.MenuOptionTypeID && f.MenuOption.OptionType.OpTypeID == OptionTypes.Application).OrderBy(f => f.MenuOption.NumOrder))
                    {
                        menuChild.Options.Add(new UIMenuOption
                        {
                            Name = mOption.MenuOption.Name,
                            PresenterType = Type.GetType("WpfFront.Presenters." + mOption.MenuOption.Url + ""),
                            Icon = new BitmapImage(new Uri(WmsSetupValues.IconPath48 + mOption.MenuOption.Icon, UriKind.Relative))
                        });

                    }
                }

                menuList.Add(menuChild);
            }

            return menuList;

        }
        */

        #endregion

        public static byte[] GetImageByte(Stream stream)
        {
            int streamLength = Convert.ToInt32(stream.Length);
            byte[] image = new byte[streamLength];
            stream.Read(image, 0, streamLength);
            stream.Close();
            return image;
        }

        public static void LoadServiceMasters()
        {
            WMSServiceClient service = new WMSServiceClient();

            //Status
            App.DocStatusList = service.GetStatus(new Status());
            App.EntityStatusList = App.DocStatusList.Where(f => f.StatusType.StatusTypeID == SStatusType.Active).ToList();
            App.DocStatusList = App.DocStatusList.Where(f => f.StatusType.StatusTypeID == SStatusType.Document).ToList();

            //Pick Methods
            App.PickMethodList = service.GetPickMethod(new PickMethod { Active = true });

            //Document Types
            App.DocTypeList = service.GetDocumentType(new DocumentType());

            //Locations
            App.LocationList = service.GetLocation(new Location { Status = new Status { StatusID = EntityStatus.Active } })
                .OrderBy(f => f.Name).ToList();

            //Companies
            App.CompanyList = service.GetCompany(new Company());


            //Data Types
            App.DataTypeList = service.GetDataType(new DataType());

            //DocumentConcepts
            App.DocumentConceptList = service.GetDocumentConcept(new DocumentConcept());


            //Bin Directions
            Hashtable binDirections = new Hashtable();
            binDirections.Add(2, "Out only");
            binDirections.Add(1, "In only");
            binDirections.Add(0, "In/Out");
            App.BinDirectionList = binDirections;

            //Custom Process
            App.CustomProcessList = service.GetCustomProcess(new CustomProcess());

            //Connection Printers
            App.PrinterConnectionList = service.GetConnection(new Connection { ConnectionType = new ConnectionType { RowID = CnnType.Printer } });


            App.ClassEntityList = service.GetClassEntity(new ClassEntity { });
        }

        public static string GetTechMessage(Exception ex)
        {
            if (ex == null)
                return "";

            string msg = ex.Message;
            Exception tmpEx = ex.InnerException;

            while (tmpEx != null)
            {
                msg += "\n" + tmpEx.Message;
                tmpEx = tmpEx.InnerException;
            }

            return msg;
        }

        internal static IList<ModuleRegion> GetMenuOptionsV2(StartModules module)
        {

            IList<ModuleRegion> menuList = new List<ModuleRegion>();
            IList<MenuOptionByRol> optionsList = null;

            //Crea el menu principal Con Los tipos de menu que debe Haber
            WMSServiceClient service = new WMSServiceClient();

            IList<MenuOptionType> typeList = service.GetMenuOptionType(new MenuOptionType());

            //Si es admin tiene permiso a todas las funcionalidades
            if (App.curRol.Rol.RolID == BasicRol.Admin)
            {
                //Guarda las opciones del menu en la session para reuso
                App.curMenuOptions = service.GetMenuOption(new MenuOption()).Where(f => f.Active != false && f.CreTerminal == App.currentLocation).ToList();
            }
            else
            {
                optionsList = service.GetMenuOptionByRol(new MenuOptionByRol
                {
                    Rol = App.curRol.Rol,
                    Company = App.curCompany
                }).Where(f => f.Status.StatusID == EntityStatus.Active && f.MenuOption.Active != false && f.MenuOption.CreTerminal == App.currentLocation)
                .OrderBy(f => f.MenuOption.NumOrder).ToList();

                //Guarda las opciones del menu en la session para reuso
                App.curMenuOptions = optionsList.Select(f => f.MenuOption).ToList();

            }

            ModuleRegion menuChild = null;

            foreach (MenuOptionType mType in typeList)
            {

                menuChild = new ModuleRegion();
                menuChild.Name = mType.Name;

                if (App.curRol.Rol.RolID == BasicRol.Admin)
                {
                    // recorremos lista de opciones del rol, para organizar los menus
                    foreach (MenuOption mOption in App.curMenuOptions.Where(f => f.MenuOptionType.MenuOptionTypeID == mType.MenuOptionTypeID && f.OptionType.OpTypeID == OptionTypes.Application).OrderBy(f => f.NumOrder))
                    {
                        menuChild.Options.Add(new ModuleSubmenu
                        {
                            Name = mOption.Name,
                            Module = module,
                            Image = GetImage(WmsSetupValues.IconPath48 + mOption.Icon),
                            PresenterType = Type.GetType("WpfFront.Presenters." + mOption.Url + ""),
                            IconPath = WmsSetupValues.IconPath16 + mOption.Icon
                        });

                    }
                }
                else
                {
                    // recorremos lista de opciones del rol, para organizar los menus
                    foreach (MenuOptionByRol mOption in optionsList.Where(f => f.MenuOption.MenuOptionType.MenuOptionTypeID == mType.MenuOptionTypeID && f.MenuOption.OptionType.OpTypeID == OptionTypes.Application).OrderBy(f => f.MenuOption.NumOrder))
                    {
                        menuChild.Options.Add(new ModuleSubmenu
                        {
                            Name = mOption.MenuOption.Name,
                            PresenterType = Type.GetType("WpfFront.Presenters." + mOption.MenuOption.Url + ""),
                            Image = GetImage(WmsSetupValues.IconPath48 + mOption.MenuOption.Icon),
                            Module = module,
                            IconPath = WmsSetupValues.IconPath16 + mOption.MenuOption.Icon
                        });

                    }
                }

                menuList.Add(menuChild);
            }

            return menuList;
        }

        public static Byte[] GetImage(String suri)
        {
            Uri uri = null;
            try { uri = new Uri(suri, UriKind.Relative); }
            catch { uri = new Uri(WmsSetupValues.IconPath48 + WmsSetupValues.DefaultMenuIcon, UriKind.Relative); }


            Stream ms = Application.GetResourceStream(uri).Stream;
            Byte[] image = new Byte[ms.Length];
            ms.Read(image, 0, Convert.ToInt32(ms.Length - 1));
            return image;
        }

        public static ImageSource GetImageSource(byte[] imageByte)
        {

            BitmapImage img = new BitmapImage();

            using (MemoryStream stream = new MemoryStream(imageByte))
                img.StreamSource = stream;

            return img;
        }

        public static string XmlSerializer(Object obj)
        {
            MemoryStream myMemStream = new MemoryStream();
            XmlSerializer mySerializer = new XmlSerializer(obj.GetType());
            mySerializer.Serialize(myMemStream, obj);
            myMemStream.Position = 0;

            // Load the serialized eConnect document object into an XML document object
            XmlTextReader xmlreader = new XmlTextReader(myMemStream);
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(xmlreader);
            return myXmlDocument.OuterXml;
        }



        public static SettingsRepository XmlDeSerializer(string objString)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString));
            XmlSerializer mySerializer = new XmlSerializer(typeof(SettingsRepository));
            SettingsRepository obj = mySerializer.Deserialize(ms) as SettingsRepository;
            return obj;

        }


        public static string GetConfigOption(string key)
        {
            try { return App.configOptions[key]; }
            catch { Util.ShowError("Option " + key + " not defined."); }
            return "";
        }


        public static InternalWindow GetInternalWindow(Panel parent, string wName)
        {
            InternalWindow window = new InternalWindow();
            window.Parent = parent;
            window.CanResize = true;
            window.ShowStatusBar = false;
            window.Header = wName;
            window.StartPosition = ToolWindowStartPosition.CenterParent;
            window.Height = SystemParameters.FullPrimaryScreenHeight - 150;
            window.Width = SystemParameters.FullPrimaryScreenWidth - 10;
            return window;
        }




        public static void WriteEventLog(string techError)
        {
            try
            {
                string sSource;
                string sLog;
                string sEvent;

                sSource = "WMS 3.0 Server";
                sLog = "WpfFront";
                sEvent = techError;

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                //EventLog.WriteEntry(sSource, sEvent);
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 301);
            }
            catch { }
        }



        internal static string ExtractFileName(string file)
        {
            try
            {
                string[] fname = file.Split("\\".ToCharArray());
                return fname[fname.Length - 1];
            }
            catch { return ""; }

        }

        //Return a dataset from a XML string  document
        public static DataSet GetDataSet(string xmlData)
        {
            XmlDocument myXmlOut = new XmlDocument();
            myXmlOut.LoadXml(xmlData);

            // convert to dataset in two lines
            DataSet ds = new DataSet();
            ds.ReadXml(new XmlNodeReader(myXmlOut));

            return ds;
        }

        internal static string GetFilePath(ImageEntityRelation file)
        {
            //try
            //{
            String File = Guid.NewGuid() + "_" + file.ImageName;

            File = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    WmsSetupValues.PrintReportDir + "\\" + File);

            //Save File to Disk Before Print.
            //file.
            FileStream oFileStream = new FileStream(File, FileMode.Create);
            oFileStream.Write(file.Image, 0, file.Image.Length);
            oFileStream.Close();

            return File;
            //}
            //catch { return ""; }
        }



        internal static void PrintShipmentPackLabels(Document shipment)
        {

            if (shipment == null || shipment.DocID == 0)
            {
                Util.ShowError("Shipment must be created before reprint all Labels.");
                return;
            }

            //Open View Packages
            LabelTemplate template = null;
            try
            {
                template = (new WMSServiceClient()).GetLabelTemplate(new LabelTemplate
                {
                    Header = WmsSetupValues.DefaultPackLabelTemplate
                }).First();
            }
            catch
            {
                Util.ShowError("No packing label defined.");
                return;
            }


            ProcessWindow pw = new ProcessWindow("Printing Labels ... ");
            int x = 0;

            try
            {
                x = (new WMSServiceClient()).PrintPackageLabels(shipment);
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Error: " + ex.Message);
            }


            pw.Close();
        }

        internal static bool AllowOption(string option)
        {
            return App.curMenuOptions.Any(f => f.Name == option);
        }


        internal static bool AllowOption(string option, Rol rol)
        {
            WMSServiceClient service = new WMSServiceClient();
            IList<MenuOption> optionsList = null;

            //Si es admin tiene permiso a todas las funcionalidades
            if (rol.RolID == BasicRol.Admin)
            {
                //optionsList = service.GetMenuOption(new MenuOption()).Where(f => f.Active != false).ToList();
                return true;
            }
            else
            {
                optionsList = service.GetMenuOptionByRol(new MenuOptionByRol
                {
                    Rol = rol,
                    Company = App.curCompany,
                    MenuOption = new MenuOption { Name = option }
                }).Where(f => f.Status.StatusID == EntityStatus.Active && f.MenuOption.Active != false)
                .Select(f => f.MenuOption).ToList();
            }

            return optionsList.Any(f => f.Name == option);

        }

        internal static bool ReviewOrderPerRandom(WMSServiceClient service)
        {
            //Revisa si el count % Random = 0 y devuelve true, REVRAND debe ser mayor a CERO

            try
            {
                //if (int.Parse( service.GetConfigOption(new ConfigOption{ Code = "COUNTRAND" }).First().DefValue) 
                //  % int.Parse( service.GetConfigOption(new ConfigOption{ Code = "REVRAND" }).First().DefValue) == 0)

                if (int.Parse(GetConfigOption("COUNTRAND")) % int.Parse(GetConfigOption("REVRAND")) == 0)
                    return true;
            }
            catch { return false; }

            return false;
        }


        internal static bool ReviewOrderPerOverAmount(WMSServiceClient service, double orderAmount)
        {
            //Primero revisa si pasa del monto acordado.
            //Revisa si el count % countamount = 0 y devuelve true, REVAMOUNT debe ser mayor a CERO

            try
            {
                if (double.Parse(Util.GetConfigOption("AMOUNTOVER")) < orderAmount)
                    return false;

                if (int.Parse(GetConfigOption("COUNTAMOUNT")) % int.Parse(GetConfigOption("REVAMOUNT")) == 0)


                    //if (int.Parse(service.GetConfigOption(new ConfigOption { Code = "COUNTAMOUNT" }).First().DefValue)
                    //% int.Parse(service.GetConfigOption(new ConfigOption { Code = "REVAMOUNT" }).First().DefValue) == 0)

                    return true;

            }
            catch { return false; }

            return false;
        }

        internal static DataSet GetListaDatosModulo(String modulo, Connection conexion)
        {
            try
            {
                if (conexion == null)
                    conexion = (new WMSServiceClient()).GetConnection(new Connection { Name = "LOCAL" }).First();
                return (new WMSServiceClient()).DirectSQLQueryDS("EXEC dbo.sp_GetProcesos '" + modulo + "'", "", "RDATA", conexion);

            }
            catch { return null; }
        }

        internal static DataSet GetListaDatosModuloDIRECTV(String modulo, Connection conexion)
        {
            try
            {
                if (conexion == null)
                    conexion = (new WMSServiceClient()).GetConnection(new Connection { Name = "LOCAL" }).First();
                return (new WMSServiceClient()).DirectSQLQueryDS("EXEC dbo.sp_GetProcesosDIRECTV '" + modulo + "'", "", "RDATA", conexion);

            }
            catch { return null; }
        }

        internal static DataSet GetListaDatosModuloDIRECTVC(String modulo, Connection conexion)
        {
            try
            {
                if (conexion == null)
                    conexion = (new WMSServiceClient()).GetConnection(new Connection { Name = "LOCAL" }).First();
                return (new WMSServiceClient()).DirectSQLQueryDS("EXEC dbo.sp_GetProcesosDIRECTVC '" + modulo + "'", "", "RDATA", conexion);

            }
            catch { return null; }
        }



        //internal static IList<ShowData> LoadLanguage(string culture)
        //{
        //    return (new WMSServiceClient()).GetLanguage(culture);
        //}


        internal static string GetResourceLanguage(string xKey)
        {
            try { return Application.Current.Resources.MergedDictionaries[0][xKey].ToString(); }
            catch { return "NO DEFINED!"; }
        }
    }
}