using System;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using System.Runtime.Remoting.Messaging;

using WpfFront.Services;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using WpfFront.Models;
using WMComposite.Regions;



namespace WpfFront.Presenters
{

    public interface IConnectionPresenter
    {
       IConnectionView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class ConnectionPresenter : IConnectionPresenter
    {
        public IConnectionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private IShellPresenter regionManager;
        public ToolWindow Window { get; set; }

        public ConnectionPresenter(IUnityContainer container, IConnectionView view,
             IShellPresenter regionManager)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ConnectionModel>();
            this.regionManager = regionManager;

            //Event Delegate
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Connection>>(this.OnLoadData);
            View.LoadChilds += new EventHandler<DataEventArgs<ConnectionType>>(this.OnLoadChilds);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.TestConnection += new EventHandler<EventArgs>(this.OnTestConnection);

            View.Model.ListCnnType = service.GetConnectionType(new ConnectionType());

            if (View.Model.ListCnnType != null && View.Model.ListCnnType.Count == 1)
                LoadChilds(View.Model.ListCnnType[0]);

            View.Model.Record = null;
            View.DpChilds.Visibility = Visibility.Collapsed;

        }

        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadChilds(object sender, DataEventArgs<ConnectionType> e)
        {
            if (e.Value == null)
                return;

            LoadChilds(e.Value);

        }


        public void LoadChilds(ConnectionType cType)
        {

            View.Model.CnType = cType;
            View.Model.Record = null;
            View.StkEdit.Visibility = Visibility.Hidden;
            View.Model.ListConnection = service.GetConnection(new Connection { ConnectionType = cType });
            View.ListDataSource.Items.Refresh();

            View.DpChilds.Visibility = Visibility.Visible;

            //Load First Child
            if (View.Model.ListConnection.Count > 0)
            {
                View.ListDataSource.SelectedIndex = 0;
                View.Model.Record = View.Model.ListConnection[0];
                LoadData(View.Model.Record);
            }
        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Connection> e)
        {
            if (e.Value == null)
                return;

            LoadData(e.Value);
        }


        private void LoadData(Connection connection)
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = connection;

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
            View.ListDataSource.SelectedItem = null;
            View.Model.Record = new Connection();
            //View.BtnEntities.Visibility = Visibility.Collapsed;
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.ConnectionID == 0)
            {
                isNew = true;
                View.Model.Record.ConnectionType = View.Model.CnType;
            }


            try
            {

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record.CreationDate = DateTime.Today;
                    View.Model.Record = service.SaveConnection(View.Model.Record);                    
                    CleanToCreate();
                }
                else
                {
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    View.Model.Record.ModDate = DateTime.Today;
                    message = "Record updated.";
                    service.UpdateConnection(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.ListConnection = service.GetConnection(new Connection { ConnectionType = View.Model.CnType });
                App.PrinterConnectionList = View.Model.ListConnection;


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
                service.DeleteConnection(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.ListConnection = service.GetConnection(new Connection { ConnectionType = View.Model.CnType });

                App.PrinterConnectionList = View.Model.ListConnection;

            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }



        //Otiene las entidades del datasource en curso
        private void OnTestConnection(object sender, EventArgs e)
        {

            try
            {
                Company company = App.curCompany;
                company.ErpConnection = new Connection {
                    CnnString = View.Model.Record.CnnString,
                    ConnectionType = View.Model.CnType
                };
                service.TestConnection(company);
                Util.ShowMessage("Connection OK.");
            }
            catch (Exception ex)
            {
                Util.ShowError("Connection problem. " + ex.Message);
            }
        }




        /*
           delegate string UrlFetcher(string url);
    
    public static void Main()
    {
        UrlFetcher u = new UrlFetcher (Fetch);
        u.BeginInvoke ("some url", 
                       new AsyncCallback (AfterFetch),
                       "this is state");
        // Just to demonstrate stuff going on while
        // the fetch happens...
        for (int i=0; i < 10; i++)
        {
            Console.WriteLine ("Foreground thread counter: {0}", i);
            Thread.Sleep(1000);
        }
    }
    
    static string Fetch (string url)
    {
        // Just to simulate it taking a while to fetch the
        // contents
        Thread.Sleep (5000);
        return "Contents of the url";
    }
    
    static void AfterFetch (IAsyncResult result)
    {
        Console.WriteLine ("Delegate completed.");
        Console.WriteLine ("  State: {0}", result.AsyncState);
        AsyncResult async = (AsyncResult) result;
        UrlFetcher fetcher = (UrlFetcher) async.AsyncDelegate;
        Console.WriteLine ("  Contents: {0}", fetcher.EndInvoke(result));
    }
*/

    }
}