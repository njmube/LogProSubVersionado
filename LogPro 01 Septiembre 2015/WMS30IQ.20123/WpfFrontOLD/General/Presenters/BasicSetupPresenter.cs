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
    public interface IBasicSetupPresenter
    {
        IBasicSetupView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class BasicSetupPresenter : IBasicSetupPresenter
    {
        public IBasicSetupView View { get; set; }
        private readonly IUnityContainer container;
        public ToolWindow Window { get; set; }

        public BasicSetupPresenter(IUnityContainer container, IBasicSetupView view)
        {
            View = view;
            this.container = container;
            View.Model = this.container.Resolve<BasicSetupModel>();

            View.LoadView += new EventHandler<DataEventArgs<int>>(View_LoadView);

       
        }


        void View_LoadView(object sender, DataEventArgs<int> e)
        {
            if (e == null)
                return;

            switch (e.Value)
            {
                case 1:
                    OnLoadConnection();
                    break;

                case 2:
                    View_LoadDocNumbers();
                    break;

                case 3:
                    View_LoadTrackOptions();
                    break;

                case 4:
                    View_LoadPickMethods();
                    break;

                //case 5:
                //    LoadReportDocuments();
                //    break;

                case 6:
                    OnLoadMessageRules();
                    break;

                case 7:
                    OnLoadLabeltemplates();
                    break;

                case 8:
                    OnLoadLabelMapping();
                    break;

                case 9:
                    OnDocumentConcept();
                    break;

                case 11:
                    OnShippingMethod();
                    break;

                default:
                    return;

            }
        }




        private void OnLoadLabeltemplates()
        {
            ILabelTemplatePresenter presenter = container.Resolve<LabelTemplatePresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void OnDocumentConcept()
        {
            IDocumentConceptPresenter presenter = container.Resolve<DocumentConceptPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void OnLoadLabelMapping()
        {
            ILabelMappingPresenter presenter = container.Resolve<LabelMappingPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        //private void LoadReportDocuments()
        //{
        //    IReportDocumentPresenter presenter = container.Resolve<ReportDocumentPresenter>();
        //    View.UCInfo.Items.Clear();
        //    View.UCInfo.Items.Add(presenter.View);
        //}

        private void OnLoadMessageRules()
        {
            IMessageRuleByCompanyPresenter presenter = container.Resolve<MessageRuleByCompanyPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }



        void View_LoadTrackOptions()
        {
            IAdminTrackOptionPresenter presenter = container.Resolve<AdminTrackOptionPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }


        void View_LoadDocNumbers()
        {
            IDocumentTypeSequencePresenter presenter = container.Resolve<DocumentTypeSequencePresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }


        private void OnLoadConnection()
        {
            IConnectionPresenter presenter = container.Resolve<ConnectionPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void View_LoadPickMethods()
        {
            IPickMethodPresenter presenter = container.Resolve<PickMethodPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

        private void OnShippingMethod()
        {
            IShippingMethodPresenter presenter = container.Resolve<ShippingMethodPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
        }

    }


    

}