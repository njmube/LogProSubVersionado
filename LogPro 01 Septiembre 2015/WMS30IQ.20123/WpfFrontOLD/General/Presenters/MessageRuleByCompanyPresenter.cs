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
using System.Collections;

namespace WpfFront.Presenters
{

    public interface IMessageRuleByCompanyPresenter
    {
       IMessageRuleByCompanyView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class MessageRuleByCompanyPresenter : IMessageRuleByCompanyPresenter
    {
        public IMessageRuleByCompanyView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }


        public MessageRuleByCompanyPresenter(IUnityContainer container, IMessageRuleByCompanyView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MessageRuleByCompanyModel>();

            //Event Delegate
            View.LoadSearch += new EventHandler<DataEventArgs<string>>(this.OnLoadSearch);
            View.New += new EventHandler<EventArgs>(this.OnNew);
            View.LoadData += new EventHandler<DataEventArgs<MessageRuleByCompany>>(this.OnLoadData);
            View.Save += new EventHandler<EventArgs>(this.OnSave);
            View.Delete += new EventHandler<EventArgs>(this.OnDelete);

            View.Model.ClassEntityList = service.GetClassEntity(new ClassEntity());
            view.Model.LabelTemplateList = service.GetLabelTemplate(new LabelTemplate
            {
                LabelType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Message } }
            });


            View.Model.EntityList = service.GetMessageRuleByCompany(new MessageRuleByCompany());
            View.Model.Record = null;

        }


        private void OnLoadSearch(object sender, DataEventArgs<string> e)
        {

            if (string.IsNullOrEmpty(e.Value))
                {
                    View.Model.EntityList = service.GetMessageRuleByCompany(new MessageRuleByCompany());
                    return;
                }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.EntityList = service.GetMessageRuleByCompany(new MessageRuleByCompany ());

        }


        //Carga los datos al seleccionar un registro de la lista
        private void OnLoadData(object sender, DataEventArgs<MessageRuleByCompany> e)
        {
            if (e.Value == null)
                return;

            View.StkEdit.Visibility = Visibility.Visible;
            View.BtnDelete.Visibility = Visibility.Visible;
            View.Model.Record = e.Value;

            if (View.Model.Record.Company == null)
                View.Model.Record.Company = new Company();

            if (View.Model.Record.Template == null)
                View.Model.Record.Template = new LabelTemplate();

            if (View.Model.Record.Entity == null)
                View.Model.Record.Entity = new ClassEntity();

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
            View.Model.Record = new MessageRuleByCompany {  Company = App.curCompany,
                                                            Template = new LabelTemplate(), 
                                                            Entity = new ClassEntity()};
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
                if (View.Model.Record.Template.RowID == 0 || View.Model.Record.Entity.ClassEntityID == 0 || View.Model.Record.Company.CompanyID == 0)
                {
                    Util.ShowError("Template and Class Entity are required.");
                    return;
                }

                try { View.Model.Record.FrequencyType = int.Parse(((DictionaryEntry)View.CboFreqType.SelectedItem).Key.ToString()); }
                catch { }

                if (View.Model.Record.NextRunTime == null)
                    View.Model.Record.NextRunTime = DateTime.Now.AddDays(1);

                if (isNew)   // new
                {
                    message = "Record created.";                    
                    View.Model.Record = service.SaveMessageRuleByCompany(View.Model.Record);
                    CleanToCreate();
                }
                else
                {
                    message = "Record updated.";
                    service.UpdateMessageRuleByCompany(View.Model.Record);

                }

                //View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetMessageRuleByCompany(new MessageRuleByCompany());

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
                service.DeleteMessageRuleByCompany(View.Model.Record);
                Util.ShowMessage("Record deleted.");

                View.StkEdit.Visibility = Visibility.Hidden;
                View.Model.EntityList = service.GetMessageRuleByCompany(new MessageRuleByCompany());
            }
            catch(Exception ex)
            {
                Util.ShowError(ex.Message);
            }



        }


    }
}