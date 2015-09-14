using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using System.Threading;
using System.Timers;

namespace ServiceConsole
{
    public partial class WMSWCFService : ServiceBase
    {
        ServiceHost m_serviceHost;

        protected override void OnStart(string[] args)
        {
            //Loading the WCF service based on App.Config file
            m_serviceHost = new ServiceHost(typeof(WcfService.WMSProcess));

            //m_serviceHost.Closed += new EventHandler(host_Closed);
            //m_serviceHost.Closing += new EventHandler(host_Closing);
            //m_serviceHost.Faulted += new EventHandler(host_Faulted);

            m_serviceHost.Open();
        }


        protected override void OnStop()
        {
            //Fin to WCF Service
            if (m_serviceHost != null)
                m_serviceHost.Close();

            m_serviceHost = null;
        }

    }



}
