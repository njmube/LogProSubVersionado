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

    public interface IFileProcessPresenter
    {
        IFileProcessView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class FileProcessPresenter : IFileProcessPresenter
    {
        public IFileProcessView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        private readonly IShellPresenter region;
        public ToolWindow Window { get; set; }


        public FileProcessPresenter(IUnityContainer container, IFileProcessView view, IShellPresenter region)
        {
            View = view;
            this.container = container;
            this.region = region;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<FileProcessModel>();

            //Event Delegate
            View.ProcessFile += new EventHandler<DataEventArgs<Stream>>(View_ProcessFile);

            //Load Process
            View.Model.EntityList = service.GetCustomProcess(new CustomProcess { ProcessType = new DocumentType { DocTypeID = SDocType.FileProcess } });

        }

        void View_ProcessFile(object sender, DataEventArgs<Stream> e)
        {
            string result;
            CustomProcess process = View.CboProcess.SelectedItem as CustomProcess;

            result = "Process: " + process.Name + "\n";

            result += service.ProcessFile(process, Util.GetPlainTextString(e.Value));

            View.TxtResult.Text = result;
        }


        /*
        //Carga los datos al seleccionar un registro de la lista
        private void OnSetLogo(object sender, DataEventArgs<Stream> e)
        {
            if (e.Value == null)
                return;

            View.Model.Logo.Image = Util.GetImageByte(e.Value);

        }
         * */


    }
}