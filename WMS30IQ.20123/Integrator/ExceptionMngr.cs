using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Entities;
using Entities.General;
using Integrator.Dao;
using System.Diagnostics;



namespace Integrator
{
    /// <summary>
    /// Permite registrar un evento en el sistema, informativo, warning , error o fault,
    /// pemitiendo registralo por Mail, o EventViewer
    /// </summary>
    public static class ExceptionMngr
    {        
     
        /// <summary>
        /// Se encarga de escribir el error basado en la configuracion del log4Net
        /// </summary>
        public static void WriteEvent(String msgContent, ListValues.EventType type, 
            Exception ex, String user, ListValues.ErrorCategory category)
        {

            WriteLog wlog = new WriteLog();
            //ILog logger = wlog.getLogger();
            //log4net.Config.XmlConfigurator.Configure();
            //log4net.Config.BasicConfigurator.Configure();

/*
            if (type == ListValues.EventType.Error)
                logger.Error(msgContent, ex);

            if (type == ListValues.EventType.Fatal)
                logger.Fatal(msgContent, ex);

            if (type == ListValues.EventType.Info)
                logger.Info(msgContent);

            if (type == ListValues.EventType.Warn)
                logger.Warn(msgContent, ex);
*/

            //Incluir Salvar en base de datos clase error.
            LogError logError = new LogError
            {
                UserError = msgContent,
                TechError = WriteLog.GetTechMessage(ex), 
                Category = category.ToString(),  CreatedBy = (user != null) ? user: "", 
                CreationDate = DateTime.Now 
            };
            
            wlog.Factory.DaoLogError().Save(logError);

        }



        public static void WriteEventLog(string techError)
        {
            try
            {
                string sSource;
                string sLog;
                string sEvent;

                sSource = "WMS 3.0 Server";
                sLog = "Integrator";
                sEvent = techError;

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                //EventLog.WriteEntry(sSource, sEvent);
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 201);
            }
            catch { }
        }

    }


    public class WriteLog
    {
        //Instancia el Log4Net
        //private readonly ILog logger = LogManager.GetLogger(typeof(ExceptionMngr));
        public DaoFactory Factory { get; set; }

        public WriteLog() { Factory = new DaoFactory(); }

        //public ILog getLogger() { return logger; }


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

    }





}
    

 