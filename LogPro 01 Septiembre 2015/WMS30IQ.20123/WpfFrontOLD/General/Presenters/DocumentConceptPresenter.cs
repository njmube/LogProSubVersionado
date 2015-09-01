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

    public interface IDocumentConceptPresenter
    {
       IDocumentConceptView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class DocumentConceptPresenter : IDocumentConceptPresenter
    {
        public IDocumentConceptView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public DocumentConceptPresenter(IUnityContainer container, IDocumentConceptView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DocumentConceptModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<DocumentConcept>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.DocumentClassList = service.GetDocumentClass(new DocumentClass());

            View.Model.EntityList = service.GetDocumentConcept(new DocumentConcept())
                .Where(f => f.Name != "Default").ToList();
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetDocumentConcept(new DocumentConcept())
                        .Where(f => f.Name != "Default").ToList();
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetDocumentConcept(new DocumentConcept { Name = e.Value })
                .Where(f=>f.Name != "Default").ToList();

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<DocumentConcept> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;
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
            View.Model.Record = new DocumentConcept { DocClass = new DocumentClass() };
        }


        private void OnSave(object sender, EventArgs e)
        {
            string message = "";
            bool isNew = false;

            if (View.Model.Record.DocConceptID == 0)
            {
                isNew = true;
                ////View.Model.Record.Company = App.curLocation.Company;                
            }


            try
            {
                if(string.IsNullOrEmpty(View.Model.Record.Name) || View.Model.Record.DocClass.DocClassID == 0)
                {
                    Util.ShowError("Document Class and Name are required.");
                    return;                    
                }

                if (isNew)   // new
                {
                    message = "Record created.";
                    //View.Model.Record.DocClass = new DocumentClass{ DocClassID = }
                    View.Model.Record = service.SaveDocumentConcept(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateDocumentConcept(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetDocumentConcept(new DocumentConcept())
                    .Where(f=>f.Name != "Default").ToList();

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
                service.DeleteDocumentConcept(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetDocumentConcept(new DocumentConcept())
                    .Where(f => f.Name != "Default").ToList();
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}