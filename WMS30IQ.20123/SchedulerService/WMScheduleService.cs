using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using System.IO;
using System.Reflection;

namespace ServiceConsole
{
    public partial class WMSScheduleService : ServiceBase
    {
        System.Timers.Timer _timer = new System.Timers.Timer();


        protected override void OnStart(string[] args)
        {
            //Schedule Console to Run Externals App Integration Process
            //Time Interval for the service
            _timer.Interval = 180000; //60000
            _timer.Elapsed += new ElapsedEventHandler(TimeElapsed);
            _timer.Start();

        }



        protected override void OnStop()
        {

        }


        void WriteEventLog(string techError)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = "WMS 3.0 Schedule";
            sLog = "Application";
            sEvent = "Schedule Failing. " + techError;

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            //EventLog.WriteEntry(sSource, sEvent);
            EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 200);
        }


        void TimeElapsed(object sender, ElapsedEventArgs args)
        {
            //string exePath = "%SystemRoot%\\system32\\notepad.exe " + schedule.ScheduleID.ToString();
            //string exePath = "D:\\Projects\\GAG\\WMS_30\\wms30\\RunConsole\\bin\\Debug\\RunConsole.exe ";

            try
            {
                string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SchedulerConsole.exe"); 
                //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                //Environment.CurrentDirectory

                if (!File.Exists(exePath))
                {
                    WriteEventLog("File " + exePath  + " not found.");
                    return;
                }

                AppLauncher launcher = new AppLauncher(exePath);

                //Manda a Ejecutar el Schedule
                new Thread(new ThreadStart(launcher.RunApp)).Start();
            }

            catch (Exception ex) {
                WriteEventLog(GetTechMessage(ex));
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


        //Clase Auxiliar que arranca el Executable de Conneccion a las otras appliaciones
        class AppLauncher
        {
            string app2Launch;
            public AppLauncher(string path) { app2Launch = path; }

            public void RunApp()
            {
                ProcessStartInfo pInfo = new ProcessStartInfo(app2Launch);
                pInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process p = Process.Start(pInfo);
            }
        }

    }





}
