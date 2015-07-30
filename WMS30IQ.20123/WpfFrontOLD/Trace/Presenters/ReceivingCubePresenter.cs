using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows;
using WpfFront.Services;

namespace WpfFront.Presenters
{

    public interface IReceivingCubePresenter
    {
        IReceivingCubeView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ReceivingCubePresenter : IReceivingCubePresenter
    {
        public IReceivingCubeView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public ReceivingCubePresenter(IUnityContainer container, IReceivingCubeView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ReceivingCubeModel>();

        }


    }
}