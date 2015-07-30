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
using System.Linq;


namespace WpfFront.Presenters
{

    public interface IBinPresenter
    {
        IBinView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class BinPresenter : IBinPresenter
    {
        public IBinView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public BinPresenter(IUnityContainer container, IBinView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<BinModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<Bin>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            ProcessWindow pw = new ProcessWindow("Loading ...");


            View.Model.EntityList = service.GetBin(new Bin());
            View.Model.Record = null;


            //List Height
            View.ListRecords.MaxHeight = SystemParameters.FullPrimaryScreenHeight - 250;

            pw.Close();
        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {
            // CAA [2010/07/13] Busq. adicional por status
            if (string.IsNullOrEmpty(e.Value) && (View.CboStatusSearch.SelectedIndex == -1 || View.CboStatusSearch.SelectedValue.ToString().Equals("-1")))
                {
                    View.Model.EntityList = service.GetBin(new Bin { Location = App.curLocation });
                    return;
                }

            //if (e.Value.Length < WmsSetupValues.SearchLength)
            //    return;

            Bin tmpSearch = new Bin { Location = App.curLocation };
            if (!string.IsNullOrEmpty(e.Value))
                tmpSearch.BinCode = e.Value;
            if (View.CboStatusSearch.SelectedIndex != -1 && !View.CboStatusSearch.SelectedValue.ToString().Equals("-1"))
                tmpSearch.Status = new Status { StatusID = int.Parse(View.CboStatusSearch.SelectedValue.ToString()) };

            //Busca por Nombre y/o status
            View.Model.EntityList = service.GetBin(tmpSearch);

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<Bin> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            View.Model.Record.Process = View.Model.Record.Process == null 
                ? new CustomProcess() : View.Model.Record.Process;

        }


        private void OnNew(object sender, EventArgs e)
        {
            CleanToCreate();
        }


        public void CleanToCreate()
        {
            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Hidden;
            View.ListRecords.SelectedIndex = -1;
            View.Model.Record = new Bin { Process = new CustomProcess(), Status = new Status(), Location = App.curLocation };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.BinID == 0)
                isNew = true;


            //Opciones de combo no obligatorio
            View.Model.Record.Process = (View.Model.Record.Process == null || View.Model.Record.Process.ProcessID == 0)
                ? null : View.Model.Record.Process;


            try
            {


                if (string.IsNullOrEmpty(View.Model.Record.BinCode) || View.Model.Record.Status.StatusID==0)
                {
                    Util.ShowError("Bincode and Status are required.");
                    return;
                }


                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record = service.SaveBin(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModDate = DateTime.Now;
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    service.UpdateBin(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetBin(new Bin());

                Util.ShowMessage(message);
            }
            catch { Util.ShowError("Error saving record. please check if record already exists."); }
            



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
                View.Model.Record.Process = View.Model.Record.Process.ProcessID == 0
                    ? null : View.Model.Record.Process;

                service.DeleteBin(View.Model.Record);


                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetBin(new Bin());
                Util.ShowMessage("Record deleted.");

            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}