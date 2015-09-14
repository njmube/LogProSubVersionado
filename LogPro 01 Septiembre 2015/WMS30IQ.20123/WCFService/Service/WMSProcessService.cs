using System;
using Facade;
using System.ServiceModel;
using System.Threading;
using Entities.General;
using Entities.Trace;
using Entities.Master;

//Implementacion de los servicios expuestos en IService


namespace WcfService
{


   // [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)] // UseSynchronizationContext = false

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]


    public class WMSProcess : Control, IWMSProcess
    {


    }

}