using System;
using WpfFront.Models;
using WpfFront.Views;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using WMComposite.Events;
using Assergs.Windows;

namespace WpfFront.Presenters
{

    public interface IAccountPresenter
    {
       IAccountView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class AccountPresenter : IAccountPresenter
    {
        public IAccountView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public AccountPresenter(IUnityContainer container, IAccountView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AccountModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Account>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            //View.SaveAccountAddress += new EventHandler<EventArgs>(this.OnSaveAccountAddress);
            //View.DeleteAccountAddress += new EventHandler<EventArgs>(this.OnDeleteAccountAddress);
            //View.LoadDataAccountAddress += new EventHandler<DataEventArgs<AccountAddress>>(this.OnLoadDataAccountAddress);
            //View.NewAccountAddress += new EventHandler<EventArgs>(this.OnNewAccountAddress);

            View.Model.EntityList = service.GetAccount(new Account {Company = App.curCompany});
            View.Model.Record = null;
            View.Model.RecordAddress = new AccountAddress();
        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetAccount(new Account());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetAccount(new Account { Name = e.Value, Company = App.curCompany });

        }

        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Account> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            View.UCAddress.LoadAddresses(View.Model.Record);

            //View.Model.AccountAddressList = service.GetAccountAddress(new AccountAddress { Account = new Account { AccountID = e.Value.AccountID } });
            //Limpio los campos de las direcciones
            //LimpiarCampos();
        }

        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }

        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.Model.Record = null;
            View.ListRecords.SelectedItem = null;
            View.Model.Record = new Account();
        }

        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record == null)
            {
                isNew = true;       
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record = service.SaveAccount(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateAccount(View.Model.Record);

                }

                View.Model.EntityList = service.GetAccount(new Account());

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError(ex.Message); }
            
        }

        private void OnDelete(object sender, EventArgs e)
        {

            if (View.Model.Record == null)
            {
                Util.ShowError("No record selected.");
                return;
            }

            try
            {
                service.DeleteAccount(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetAccount(new Account());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }

        }

    }
}