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

    public interface ILabelTemplatePresenter
    {
       ILabelTemplateView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class LabelTemplatePresenter : ILabelTemplatePresenter
    {
        public ILabelTemplateView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public LabelTemplatePresenter(IUnityContainer container, ILabelTemplateView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<LabelTemplateModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<LabelTemplate>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            //View.Model.LabelTypeList = service.GetLabelType();

            View.Model.EntityList = service.GetLabelTemplate(new LabelTemplate());
            View.Model.LabelTypeList = service.GetDocumentType(new DocumentType())
                .Where(f => f.DocClass.DocClassID == 10 || f.DocClass.DocClassID == 11 || f.DocClass.DocClassID == 12).ToList();
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetLabelTemplate(new LabelTemplate());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetLabelTemplate(new LabelTemplate { Name = e.Value });

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<LabelTemplate> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            if (View.Model.Record.LabelType == null)
                View.Model.Record.LabelType = new DocumentType();
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
            View.Model.Record = new LabelTemplate { LabelType = new DocumentType() };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.RowID == 0)
            {
                isNew = true;        
            }


            try
            {
                if (string.IsNullOrEmpty(View.Model.Record.Name) || View.Model.Record.LabelType.DocTypeID == 0)
                {
                    Util.ShowError("Name and Label type are required.");
                    return;
                }

                if (View.Model.Record.LabelType.DocClass.DocClassID == SDocClass.Label &&
                    View.Model.Record.DefPrinter == null)
                {
                    Util.ShowError("Please select the default printer for the Label.");
                    return;
                }



                if (isNew)   // new
                {
                    message = "Record created.";
                    View.Model.Record.CreatedBy = App.curUser.UserName;
                    View.Model.Record.CreationDate = DateTime.Now;
                    View.Model.Record = service.SaveLabelTemplate(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    View.Model.Record.ModifiedBy = App.curUser.UserName;
                    View.Model.Record.ModDate = DateTime.Now;
                    service.UpdateLabelTemplate(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetLabelTemplate(new LabelTemplate());

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
                service.DeleteLabelTemplate(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetLabelTemplate(new LabelTemplate());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}