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

    public interface IInventoryTaskPresenter
    {
        IInventoryTaskView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class InventoryTaskPresenter : IInventoryTaskPresenter
    {
        public IInventoryTaskView View { get; set; }
        private readonly IUnityContainer container;
        public ToolWindow Window { get; set; }
        //private readonly WMSServiceClient service;


        public InventoryTaskPresenter(IUnityContainer container, IInventoryTaskView view)
        {
            View = view;
            this.container = container;
            //this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<InventoryTaskModel>();


            View.ShowRepAndPack += new EventHandler<EventArgs>(this.OnShowRepAndPack);
            View.InvenroryAdj += new EventHandler<EventArgs>(View_InvenroryAdj);
            View.ErpConciliation += new EventHandler<EventArgs>(View_ErpConciliation);

            //LoadRepAndPack();
            LoadInventoryAdj();

        }



        void View_ErpConciliation(object sender, EventArgs e)
        {
            IUC_IV_Presenter presenter = container.Resolve<UC_IV_Presenter>();
            presenter.SetComparer(new UC_IV_ComparerView());

            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.ViewComp);
            View.TxtTitle.Text = "WMS vs ERP Conciliation";
        }

        
        void View_InvenroryAdj(object sender, EventArgs e)
        {
            LoadInventoryAdj();
        }


        private void LoadInventoryAdj()
        {
            InventoryAdjustmentPresenter presenter = container.Resolve<InventoryAdjustmentPresenter>();
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.View);
            View.TxtTitle.Text = "Inventory Adjustments";
        }



        private void OnShowRepAndPack(object sender, EventArgs e)
        {
            LoadRepAndPack();
        }


        public void LoadRepAndPack()
        {
            IUC_IV_Presenter presenter = container.Resolve<UC_IV_Presenter>();
            presenter.SetRepPacking(new UC_IV_Replanish_PackingView());
            //Llama una funcion que crea la vista de repacking.
            View.UCInfo.Items.Clear();
            View.UCInfo.Items.Add(presenter.ViewRepPack);
            View.TxtTitle.Text = "Replenishment And Packing";
        }

    }
}