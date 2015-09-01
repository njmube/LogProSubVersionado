using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace WcfWMSConsoleApp
{
    public class Program
    {
        const int SleepTime = 100;
        private static ServiceHost m_serviceHost = null;
        //private static ServiceHost m_serviceDeviceHost = null;
        private static Thread m_thread;
        private static bool m_running = true;


        static void Main(string[] args)
        {
            //using (ServiceHost host = new ServiceHost(typeof(WcfService.WMSProcess)))
            //{
            //    //host.Closed += new EventHandler(host_Closed);
            //    //host.Closing += new EventHandler(host_Closing);
            //    //host.Faulted += new EventHandler(host_Faulted);

            //    host.Open();
            //    Console.WriteLine("Press <Enter> to terminate the Host application.");
            //    Console.ReadLine();
            //}

            m_thread = new Thread(new ThreadStart(ThreadMethod));
            m_thread.Start();
            Console.WriteLine("Server is running.");

        }


        static void ThreadMethod()
        {
            try
            {
                m_serviceHost = new ServiceHost(typeof(WcfService.WMSProcess));
                m_serviceHost.Open();

                //m_serviceDeviceHost = new ServiceHost(typeof(WcfService.WMSDeviceProcess));
                //m_serviceDeviceHost.Open();

                while (m_running)
                {
                    // Wait until thread is stopped
                    Thread.Sleep(SleepTime);
                }

                // Stop the host
                m_serviceHost.Close();
            }
            catch (Exception ex)
            {

                Console.Write(ex.Message);
                while (ex.InnerException != null)
                {
                    Console.Write(ex.Message);
                    ex = ex.InnerException;
                }

                if (m_serviceHost != null)
                {
                    m_serviceHost.Close();
                    //m_serviceDeviceHost.Close();
                }
            }
        }

        /// <summary>
        /// Request the end of the thread method.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                m_running = false;
            }
        }

    }
}

