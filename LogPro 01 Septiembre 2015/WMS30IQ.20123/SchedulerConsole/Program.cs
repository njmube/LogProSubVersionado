using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facade;
using Entities.Master;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Entities.Profile;
using System.ServiceProcess;


namespace SchedulerConsole
{
    class Program
    {

        private static Control ctrl;
        private static string throwEx = "";
        private static DateTime lastUpdate;


        static void Main(string[] args)
        {
            ctrl = new Control();

            if (args.Length > 0)
            {
                if (args[0].Equals("1"))
                    throwEx = args[0];
            }


            //Checking if Services is Running, restart if down.
            try { CheckForWCFService(); }
            catch { }

            //Load ERP Document for all companies since last loading

            //Si la variable esta bloqueda y no ha pasado mas de x minutos
            //NO puede ejecutar el proceso aun.

            ConfigOption cfgOption = null;
            string schAvailable = "";
            DateTime lastDate;

            try
            {
                cfgOption = ctrl.GetConfigOption(new ConfigOption { Code = "SCHSTA" }).First();
                string schStatus = cfgOption.DefValue;
                schAvailable = schStatus.Split('|')[0];
                lastDate = DateTime.Parse(schStatus.Split('|')[1]);


                long minutes = DateAndTime.DateDiff(DateInterval.Minute, lastDate, DateTime.Now, FirstDayOfWeek.System, FirstWeekOfYear.System);
                Console.WriteLine("SCHSTA: " + schAvailable + ", " + lastDate + ", Minutes: " + minutes.ToString());

                if (schAvailable == "F" && minutes < 10)
                    return;

            }
            catch
            {
                if (cfgOption != null)
                {
                    cfgOption.DefValue = "T|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    ctrl.UpdateConfigOption(cfgOption);
                }
            }


            //Bloqueado para que no se ejecuten varios al tiempo
            if (cfgOption != null)
            {
                cfgOption.DefValue = "F|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                ctrl.UpdateConfigOption(cfgOption);
            }

            int numHours = 6;
            try { numHours = int.Parse(ctrl.GetConfigOption(new ConfigOption { Code = "SCHTIME" }).First().DefValue); }
            catch { numHours = 6;  }



            IList<Company> companyList = ctrl.GetCompany(new Company());
            lastUpdate = DateTime.Now.AddHours(-1 * numHours);


            foreach (Company company in companyList)
            {

                try
                {

                    Console.WriteLine("------ WorkFlow Tasks -----");
                    ctrl.RunDataBaseRoutines(company);

                    Console.WriteLine("------ Depuration Tasks -----");
                    ctrl.DepurationTasks(company);

                    Console.WriteLine("------ Mail Service -----");
                    //try { ctrl.ProcessMessageRules(); }
                    //catch { }
             

                    //Quiere decir que la company no esta conectada a un ERP
                    if (company.ErpConnection == null)
                    {
                        if (throwEx == "1")
                            WriteEventLog("Set Connection for Company: " + company.Name);

                        continue;
                    }

                    //Quiere decir que la company esta inactivca
                    if (company.Status.StatusID == 1002 || company.ErpConnection == null)
                        continue;


                    if (args.Length > 0 && !args[0].Equals("1"))
                    {
                        Console.WriteLine("------ Global Process ERP References -----");
                        LoadERPDocumentsByOption(company, args[0]);
                    }
                    else
                    {
                        Console.WriteLine("------ Global Process ERP References -----");
                        LoadERPDocuments(company);
                    }

                    Console.WriteLine("------ Global Process ERP Posted -----");
                    ctrl.UpdatePostedProcessThread(company);

          
                    company.LastUpdate = lastUpdate;

                    //Guardando la nueva fecha
                    try { ctrl.UpdateCompany(company); }
                    catch { ctrl.UpdateCompany(company); }

                }
                catch (Exception ex)
                {
                    if (throwEx == "1")
                        Console.WriteLine(GetTechMessage(ex));

                    WriteEventLog(GetTechMessage(ex));
                }

            }


            if (cfgOption != null)
            {
                //Desbloqueando la Ejecucion
                cfgOption.DefValue = "T|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                ctrl.UpdateConfigOption(cfgOption);
            }

        }


        private static void CheckForWCFService()
        {
            ServiceController controller = new ServiceController();
            controller.MachineName = ".";
            controller.ServiceName = "WMS 3.0 WCF Service";

            if (controller.Status != ServiceControllerStatus.Running)
            {
                Console.WriteLine("Restarting WMS 3.0 WCF Service. Last Status: " + controller.Status.ToString());
                WriteEventLog("Restarting WMS 3.0 WCF Service. Last Status: " + controller.Status.ToString());

                try { controller.Stop(); }
                catch { }

                controller.Start();
            }
            else {
                Console.WriteLine("WMS 3.0 WCF Service. Rinning OK !");
            }
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


        private static void WriteEventLog(string techError)
        {
            try
            {
                string sSource;
                string sLog;
                string sEvent;

                sSource = "WMS 3.0 Server";
                sLog = "Application";
                sEvent = "Schedule Failing. " + techError;

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                //EventLog.WriteEntry(sSource, sEvent);
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 200);
            }
            catch { }
        }


        private static void LoadERPDocuments(Company company)
        {
            
            Console.WriteLine("Start " + DateTime.Now.ToString());


            if (company.LastUpdate == null)
            {
                //Update Allways
                Console.WriteLine("Full References Basic " + DateTime.Now.ToString());


                Console.WriteLine("\tUnits " + DateTime.Now.ToString());
                try { ctrl.GetErpAllUnits(company); }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    return;
                }


                Console.WriteLine("\tShipping Methods " + DateTime.Now.ToString());
                ctrl.GetErpShippingMethods(company);


                Console.WriteLine("\tLocations " + DateTime.Now.ToString());
                ctrl.GetErpLocations(company);


                //References
                Console.WriteLine("Full References Account " + DateTime.Now.ToString());
                ctrl.GetErpAllAccounts(company);

                try
                {
                    Console.WriteLine("Full References Product " + DateTime.Now.ToString());
                    ctrl.GetErpAllProducts(company);
                }
                catch { }

                Console.WriteLine("Kit & Assemblies " + DateTime.Now.ToString());
                ctrl.GetErpAllKitAssembly(company);


                //Documents
                Console.WriteLine("Full Receiving Documents " + DateTime.Now.ToString());
                try { ctrl.GetErpAllReceivingDocuments(company); }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    return;
                }

                Console.WriteLine("Full Shipping Documents " + DateTime.Now.ToString());

                ctrl.GetErpAllShippingDocuments(company);

                Console.WriteLine("Warehouse Transfers " + DateTime.Now.ToString());
                ctrl.GetErpAllLocationTransferDocuments(company);


                //Kit Assembly
                Console.WriteLine("Kit & Assemblies Documents " + DateTime.Now.ToString());
                ctrl.GetErpAllKitAssemblyDocuments(company);


                Console.WriteLine("Null Date All References And Documents " + DateTime.Now.ToString());
            }
            else
            {
                //Se actualiza una vez cada dia - Each day
                DateTime lastDayUpdate = DateTime.Parse(((DateTime)company.LastUpdate).ToShortDateString());
                long days = DateAndTime.DateDiff(DateInterval.Day, lastDayUpdate, DateTime.Now, FirstDayOfWeek.System, FirstWeekOfYear.System);

                if (Math.Abs(days) > 0)
                {
                    Console.WriteLine("Daily References And Documents " + DateTime.Now.ToString());

                    ctrl.GetErpAllUnits(company);
                    ctrl.GetErpShippingMethods(company);
                    ctrl.GetErpLocations(company);
                    ctrl.GetErpProductCategories(company);
                    //Modifcado marzo 3
                    ctrl.UpdatePostedProcessThread(company);
                }


                //References
                Console.WriteLine("Today  References Account " + DateTime.Now.ToString());
                ctrl.GetErpAccountsSince(company, (DateTime)company.LastUpdate);

                try
                {
                    Console.WriteLine("Today  References Product " + DateTime.Now.ToString());
                    ctrl.GetErpProductsSince(company, (DateTime)company.LastUpdate);
                }
                catch { }


                try
                {
                    //Kit Assembly
                    Console.WriteLine("Kit & Assemblies " + DateTime.Now.ToString());
                    ctrl.GetErpKitAssemblySince(company, (DateTime)company.LastUpdate);
                    //ctrl.GetErpAllKitAssembly(company);
                }
                catch { }

                try
                {
                    //Documents
                    Console.WriteLine("Today Shipping Documents " + DateTime.Now.ToString());
                    ctrl.GetErpShippingDocumentsSince(company, (DateTime)company.LastUpdate);
                }
                catch{}

                try {
                Console.WriteLine("Today Receiving Documents " + DateTime.Now.ToString());
                ctrl.GetErpReceivingDocumentsSince(company, (DateTime)company.LastUpdate);
                }catch{}

                try
                {
                    Console.WriteLine("Kit & Assembliy Documents " + DateTime.Now.ToString());
                    ctrl.GetErpAllKitAssemblyDocuments(company);
                }
                catch { }

                try
                {
                    //Warehouse Transfers
                    Console.WriteLine("Warehouse Transfers " + DateTime.Now.ToString());
                    ctrl.GetErpLocationTransferDocumentsSince(company, (DateTime)company.LastUpdate);
                }
                catch { }


            }


        }


        private static void LoadERPDocumentsByOption(Company company, string argument)
        {

            Console.WriteLine("Start " + DateTime.Now.ToString());


            if (company.LastUpdate == null)
            {
                if (argument.Equals("100"))
                {

                    //Update Allways
                    Console.WriteLine("Full References Basic " + DateTime.Now.ToString());
                    ctrl.GetErpAllUnits(company);
                    ctrl.GetErpShippingMethods(company);
                    ctrl.GetErpLocations(company);

                    //References
                    Console.WriteLine("Full References Account " + DateTime.Now.ToString());
                    ctrl.GetErpAllAccounts(company);
                }


                if (argument.Equals("200"))
                {
                    Console.WriteLine("Full References Product " + DateTime.Now.ToString());
                    ctrl.GetErpAllProducts(company);

                    Console.WriteLine("Kit & Assemblies " + DateTime.Now.ToString());
                    ctrl.GetErpAllKitAssembly(company);

                }

                if (argument.Equals("300"))
                {
                    //Documents
                    Console.WriteLine("Full Receiving Documents " + DateTime.Now.ToString());
                    ctrl.GetErpAllReceivingDocuments(company);

                    Console.WriteLine("Full Shipping Documents " + DateTime.Now.ToString());

                    ctrl.GetErpAllShippingDocuments(company);

                    Console.WriteLine("Warehouse Transfers " + DateTime.Now.ToString());
                    ctrl.GetErpAllLocationTransferDocuments(company);


                    //Kit Assembly
                    Console.WriteLine("Kit & Assemblies Documents " + DateTime.Now.ToString());
                    ctrl.GetErpAllKitAssemblyDocuments(company);


                    Console.WriteLine("Null Date All References And Documents " + DateTime.Now.ToString());
                }
            }
            else
            {
                //Se actualiza una vez cada dia - Each day
                DateTime lastDayUpdate = DateTime.Parse(((DateTime)company.LastUpdate).ToShortDateString());
                long days = DateAndTime.DateDiff(DateInterval.Day, lastDayUpdate, DateTime.Now, FirstDayOfWeek.System, FirstWeekOfYear.System);


                if (Math.Abs(days) > 0)
                {
                    Console.WriteLine("Daily References And Documents " + DateTime.Now.ToString());

                    ctrl.GetErpAllUnits(company);
                    ctrl.GetErpShippingMethods(company);
                    ctrl.GetErpLocations(company);
                    ctrl.GetErpProductCategories(company);


                }

                if (argument.Equals("100"))
                {
                    //References
                    Console.WriteLine("Today  References Account " + DateTime.Now.ToString());
                    ctrl.GetErpAccountsSince(company, (DateTime)company.LastUpdate);
                }


                if (argument.Equals("200"))
                {
                    Console.WriteLine("Today  References Product " + DateTime.Now.ToString());
                    ctrl.GetErpProductsSince(company, (DateTime)company.LastUpdate);


                    //Kit Assembly
                    Console.WriteLine("Kit & Assemblies " + DateTime.Now.ToString());
                    ctrl.GetErpKitAssemblySince(company, (DateTime)company.LastUpdate);
                    //ctrl.GetErpAllKitAssembly(company);

                }

                if (argument.Equals("300"))
                {
                    //Documents
                    Console.WriteLine("Today Shipping Documents " + DateTime.Now.ToString());
                    ctrl.GetErpShippingDocumentsSince(company, (DateTime)company.LastUpdate);

                    Console.WriteLine("Today Receiving Documents " + DateTime.Now.ToString());
                    ctrl.GetErpReceivingDocumentsSince(company, (DateTime)company.LastUpdate);



                    ctrl.GetErpAllKitAssemblyDocuments(company);

                    //Warehouse Transfers
                    Console.WriteLine("Warehouse Transfers " + DateTime.Now.ToString());
                    ctrl.GetErpLocationTransferDocumentsSince(company, (DateTime)company.LastUpdate);

                }
            }


        }

    }
}