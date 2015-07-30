using System;
//using WpfFront.BusinessObject;
using WpfFront.Models;
using WpfFront.Services;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;



namespace WpfFront.Presenters

{


    public interface IShippingConsolePresenter
    {
        IShippingConsoleView View { get; set; }
        ToolWindow Window { get; set; }
    }



    public class ShippingConsolePresenter : IShippingConsolePresenter
    {
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }



        public ShippingConsolePresenter(IUnityContainer container, IShippingConsoleView view)
        {
            try
            {
                View = view;
                this.container = container;
                this.service = new WMSServiceClient();
                View.Model = this.container.Resolve<ShippingConsoleModel>();

                //Event Delegate
                View.AddTaskByUser += new EventHandler<DataEventArgs<Document>>(this.OnAddTaskByUser);
                View.RemoveTaskByUser += new EventHandler<DataEventArgs<Document>>(this.OnRemoveTaskByUser);
                View.LoadPickerDocuments += new EventHandler<EventArgs>(this.OnLoadPickerDocuments);


                //Loading Pickers
                UserByRol userByRol = new UserByRol { Location = App.curLocation, Rol = new Rol { RolID = BasicRol.Picker } };
                View.Model.PickerList = service.GetUserByRol(userByRol);
               
                //Loading Open Documents
                LoadDocuments();

            }
            catch (Exception ex)
            {
                Util.ShowError("Error loading view.\n" + ex.Message);
            }
        }


        public IShippingConsoleView View { get; set; }



        private void LoadDocuments()
        {
            try
            {

                DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };

                //Load the Base Documents (Pendig pero sin fecha de referencia)
                DateTime refDate = DateTime.Today; //DateTime.Parse("2017-04-12"); //

                DateTime month = refDate.AddMonths(1);
                DateTime week = refDate.AddDays(7);
                DateTime today = refDate; 

                View.Model.MonthList = service.GetPendingDocument(new Document { DocType = docType, Company = App.curCompany },0,0).Where(f => f.Date1 >= today && f.Date1 <= month).ToList();
                View.Model.WeekList = View.Model.MonthList.Where(f => f.Date1 >= today && f.Date1 <= week).ToList();
                View.Model.TodayList = View.Model.WeekList.Where(f => f.Date1 == today).ToList();
                View.Model.OrderByPicker = new List<Document>();


            }
            catch (Exception ex)
            {
                Util.ShowError(ex.Message);
            }
        }



        private void OnAddTaskByUser(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            try
            {

                if (View.Model.OrderByPicker.Where(f => f.DocID == e.Value.DocID).Count() == 0)
                {
                    TaskByUser task = new TaskByUser
                    {
                        CreatedBy = App.curUser.UserName,
                        CreationDate = DateTime.Now,
                        TaskDocument = e.Value,
                        User = View.Model.CurPicker
                    };

                    service.SaveTaskByUser(task);
                }
                
            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be loaded.\n" + ex.Message);
            }

        }


        private void OnRemoveTaskByUser(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            try
            {
                //TaskByUser task = service.GetTaskByUser(new TaskByUser
                //{ TaskDocument = e.Value, User = View.Model.CurPicker
                //}).FirstOrDefault(); 
                
                //if (task != null)
                //    service.DeleteTaskByUser(task);

            }
            catch (Exception ex)
            {
                Util.ShowError("Document could not be loaded.\n" + ex.Message);
            }

        }


        private void OnLoadPickerDocuments(object sender, EventArgs e)
        {
            try
            {
                //TaskByUser task = new TaskByUser
                //{
                //    User = View.Model.CurPicker
                //};

                ////MOstrar solo Open Documents
                //IList<TaskByUser> taskList = service.GetTaskByUser(task);

                //View.Model.OrderByPicker = taskList.Select(f=>f.TaskDocument).ToList();

            }
            catch (Exception ex)
            {
                Util.ShowError("Documents for this picker could not be loaded.\n" + ex.Message);
            }
        }
    }
}
