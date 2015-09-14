using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using WpfFront.Common.UserControls;
using WpfFront.Common;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for FileProcessView.xaml
    /// </summary>
    public partial class FileProcessView : UserControlBase, IFileProcessView
    {
        public FileProcessView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<Stream>> ProcessFile;


         public FileProcessModel Model
        {
            get
            { return this.DataContext as FileProcessModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

        public ComboBox CboProcess
        { get { return this.cboProcess; }
            set { this.cboProcess = value; }
        }

        public UploadFile UploadFile
        { get { return this.fUpload; } }


        public TextBlock TxtResult
        {
            get { return this.txtResult; }
            set { this.txtResult = value; }
        }


        #endregion



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cboProcess.SelectedItem == null)
            {
                Util.ShowError("Process is required.");
                return;
            }

            if (fUpload.StreamFile != null)
                ProcessFile(sender, new DataEventArgs<Stream>(fUpload.StreamFile));

            fUpload.StreamFile = null;
            fUpload.txtLogo.Text = "";
            cboProcess.SelectedIndex = -1;

        }

    }



    public interface IFileProcessView
    {
        //Clase Modelo
        FileProcessModel Model { get; set; }

        ComboBox CboProcess { get;  }
        UploadFile UploadFile { get;  }
        TextBlock TxtResult { get; }

        event EventHandler<DataEventArgs<Stream>> ProcessFile;

    }
}