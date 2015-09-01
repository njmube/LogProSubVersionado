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

    public interface IIntroInventoryPresenter
    {
        IIntroInventoryView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class IntroInventoryPresenter : IIntroInventoryPresenter
    {
        public IIntroInventoryView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public IntroInventoryPresenter(IUnityContainer container, IIntroInventoryView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<IntroInventoryModel>();

        }


    }
}