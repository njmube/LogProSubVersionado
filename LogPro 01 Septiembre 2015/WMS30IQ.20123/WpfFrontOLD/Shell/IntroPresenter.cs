using System;
using WpfFront.Models;
using WpfFront.Views;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;

namespace WpfFront.Presenters
{

    public interface IIntroPresenter
    {
       IIntroView View { get; set; }
    }


    public class IntroPresenter : IIntroPresenter
    {
        public IIntroView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;


        public IntroPresenter(IUnityContainer container, IIntroView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<IntroModel>();
        }


    }
}