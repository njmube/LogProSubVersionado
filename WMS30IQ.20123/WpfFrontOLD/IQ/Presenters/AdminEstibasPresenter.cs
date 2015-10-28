using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assergs.Windows;
using Microsoft.Practices.Unity;
using WpfFront.Services;
using WpfFront.WMSBusinessService;

namespace WpfFront.IQ.Presenters
{
    public interface IAdminEstibasPresenter
    {
        IAdminEstibasPresenter View { get; set; }
        ToolWindow Window { get; set; }
    }
    class AdminEstibasPresenter : IAdminEstibasPresenter
    {
        public IAdminEstibasPresenter View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        public Connection Local;
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;
    }
}
