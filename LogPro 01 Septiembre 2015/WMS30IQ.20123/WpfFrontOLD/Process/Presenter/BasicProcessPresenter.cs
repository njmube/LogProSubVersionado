using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;

namespace WpfFront.Presenters
{
    public interface IBasicProcessPresenter
    {
        IBasicProcessView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class BasicProcessPresenter : IBasicProcessPresenter
    {
        public IBasicProcessView View { get; set; }
        private readonly IUnityContainer container;
        public ToolWindow Window { get; set; }

        public BasicProcessPresenter(IUnityContainer container, IBasicProcessView view)
        {
            View = view;
            this.container = container;
            View.Model = this.container.Resolve<BasicProcessModel>();

            View.LoadView += new EventHandler<DataEventArgs<int>>(View_LoadView);

       
        }


        void View_LoadView(object sender, DataEventArgs<int> e)
        {
            if (e == null)
                return;

            switch (e.Value)
            {
                case 1:
                    LoadProcess();
                    break;

                case 2:
                    LoadActivities();
                    break;

                case 3:
                    LoadContext();
                    break;

                case 4:
                    LoadTransitions();
                    break;

                case 5:
                    LoadEntityContext();
                    break;

                case 6:
                    LoadProcessRoutes();
                    break;

                default:
                    return;

            }
        }

        private void LoadProcessRoutes()
        {
            ICustomProcessRoutePresenter presenter = container.Resolve<ICustomProcessRoutePresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void LoadEntityContext()
        {
            ICustomProcessContextByEntityPresenter presenter = container.Resolve<CustomProcessContextByEntityPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void LoadTransitions()
        {
            ICustomProcessTransitionPresenter presenter = container.Resolve<CustomProcessTransitionPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void LoadContext()
        {
            ICustomProcessContextPresenter presenter = container.Resolve<CustomProcessContextPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void LoadActivities()
        {
            ICustomProcessActivityPresenter presenter = container.Resolve<CustomProcessActivityPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void LoadProcess()
        {
            ICustomProcessPresenter presenter = container.Resolve<CustomProcessPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }


    }


    

}