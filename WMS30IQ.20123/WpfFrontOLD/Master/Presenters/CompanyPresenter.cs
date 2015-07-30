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
using System.IO;
using System.Linq;
using WMComposite.Regions;


namespace WpfFront.Presenters
{

    public interface ICompanyPresenter
    {
        ICompanyView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class CompanyPresenter : ICompanyPresenter
    {
        public ICompanyView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }


        public CompanyPresenter(IUnityContainer container, ICompanyView view, IShellPresenter region)
        {
            View = view;
            this.container = container;
            this.region = region;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<CompanyModel>();

            //Event Delegate
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Company>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.SetLogo += new EventHandler<DataEventArgs<Stream>>(this.OnSetLogo);
            View.ViewConnections += new EventHandler<EventArgs>(this.OnViewConnections);


            ProcessWindow pw = new ProcessWindow("Loading ...");

            View.Model.EntityList = service.GetCompany(new Company());

            //load status
            View.Model.Status = service.GetStatus(new Status { StatusType = new StatusType {  StatusTypeID = SStatusType.Active } });
            
            //los connections
            View.Model.ErpConn = service.GetConnection(new Connection());

            if (View.Model.EntityList != null && View.Model.EntityList.Count > 0)
            {
                LoadData(View.Model.EntityList[0]);
                View.ListRecords.SelectedIndex = 0;
            }

            pw.Close();

        }




        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Company> e)
        {
            if (e.Value == null)
                return;

            LoadData(e.Value);
        }

        private void LoadData(Company company)
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = company;
            View.Model.Record.ErpConnection = View.Model.Record.ErpConnection == null ? new Connection() : View.Model.Record.ErpConnection;


            //Logo
            IList<ImageEntityRelation> logoList = service.GetImageEntityRelation(
                new ImageEntityRelation
                {
                    EntityRowID = App.curCompany.CompanyID,
                    ImageName = "LOGO",
                    Entity = new ClassEntity { ClassEntityID = EntityID.Company }
                });

            View.Model.Logo = (logoList != null && logoList.Count > 0) ? logoList.First(): new ImageEntityRelation();
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
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new Company();
            View.Model.Record.Status = new Status();
            View.Model.Record.ErpConnection = new Connection();
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";    
            bool isNew = false;


            if (string.IsNullOrEmpty(View.Model.Record.Name) || View.Model.Record.Status.StatusID == 0)
            {
                Util.ShowError("Name and Status are required.");
                return;
            }


            if (View.Model.Record.CompanyID == 0)
            {
                isNew = true;
            }

            //Opciones de combo no obligatorio
            View.Model.Record.ErpConnection = View.Model.Record.ErpConnection.ConnectionID == 0 
                ? null : View.Model.Record.ErpConnection;


            try
            {
                View.Model.Record.IsDefault = (View.Model.Record.IsDefault == true) ? true : false;

                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record.CreationDate = DateTime.Today;                    
                    View.Model.Record = service.SaveCompany(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    View.Model.Record.ModDate = DateTime.Today;
                    message = "Record updated.";
                    service.UpdateCompany(View.Model.Record);

                }


                //Saving Logo
                if (View.Model.Logo != null)
                {
                    if (View.Model.Logo.RowID == 0)
                    {
                        View.Model.Logo.CreatedBy = App.curUser.UserName;
                        View.Model.Logo.CreationDate = DateTime.Now;
                        View.Model.Logo.ImageName = "LOGO";
                        View.Model.Logo.EntityRowID = View.Model.Record.CompanyID;
                        View.Model.Logo.Entity = new ClassEntity { ClassEntityID = EntityID.Company };
                        View.Model.Logo = service.SaveImageEntityRelation(View.Model.Logo);
                    }
                    else
                    {
                        View.Model.Logo.ModifiedBy = App.curUser.UserName;
                        View.Model.Logo.ModDate = DateTime.Now;
                        service.UpdateImageEntityRelation(View.Model.Logo);
                    }

                }


                View.Model.EntityList = service.GetCompany(new Company());

                Util.ShowMessage(message);
            }
            catch (Exception ex) { Util.ShowError("Error saving company.\n" + ex.Message); }
            



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
                //Opciones de combo no obligatorio
                View.Model.Record.ErpConnection = View.Model.Record.ErpConnection.ConnectionID == 0
                    ? null : View.Model.Record.ErpConnection;

                service.DeleteCompany(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetCompany(new Company());
            }
            catch //(Exception ex)
            {
                Util.ShowError("Error deleting Company. Delete Products, Units and all records related to the company before delete the company.");
            }



        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnSetLogo(object sender, DataEventArgs<Stream> e)
        {
            if (e.Value == null)
                return;

            View.Model.Logo.Image = Util.GetImageByte(e.Value);

        }



        private void OnViewConnections(object sender, EventArgs e)
        {
            ProcessWindow pw = new  ProcessWindow("Loading Connections ... ");

            try
            {
                IConnectionPresenter presenter = container.Resolve<ConnectionPresenter>();

                InternalWindow window = Util.GetInternalWindow(this.Window.Parent, "Connections");
                presenter.Window = window;
                window.GridContent.Children.Add((ConnectionView)presenter.View);
                window.Show();

            }
            catch { }
            finally
            {
                pw.Close();
            }
        }

    }
}