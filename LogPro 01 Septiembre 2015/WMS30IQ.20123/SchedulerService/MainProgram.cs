//Listing - 4, Program.cs
using System;
using System.ServiceProcess;

namespace ServiceConsole
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new WMSScheduleService() };
            ServiceBase.Run(ServicesToRun);

        }
    }


}
