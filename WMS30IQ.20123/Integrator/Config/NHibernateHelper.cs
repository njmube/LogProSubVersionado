using System;
using NHibernate;
using NHibernate.Cfg;
using System.Diagnostics;

namespace Integrator.Config 
{
    public class NHibernateHelper
    {
        private static readonly ISessionFactory sessionFactory;

        [ThreadStatic]
        private static ISession session = null;

        static NHibernateHelper()
        {
            sessionFactory = new Configuration()
                .Configure(AppDomain.CurrentDomain.BaseDirectory + "\\hibernate.cfg.xml").BuildSessionFactory();
        }


        public static ISessionFactory getSessionFactory()
        {
            return sessionFactory;
        }


        public static ISession CurrentSession()
        {

            try
            {

                if (session == null)
                    session = sessionFactory.OpenSession();
                if (session.IsOpen == false)
                    session = sessionFactory.OpenSession();
                if (session.IsConnected == false)
                    session.Reconnect();

                //if (session == null || !session.IsOpen)
                //    session = sessionFactory.OpenSession();
                //else
                //    session = sessionFactory.GetCurrentSession();


                return session;
            }
            catch (Exception ex)
            {
                WriteEventLog("CurrentSession: " + ex.Message);
                throw ex;
            }
            
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
}

