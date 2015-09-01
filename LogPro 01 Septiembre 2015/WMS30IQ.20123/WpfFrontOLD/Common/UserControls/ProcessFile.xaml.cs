using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfFront.WMSBusinessService;
using Core.BusinessEntity;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using WpfFront.Services;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class ProcessFile : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnLoadLocation;

        private WMSServiceClient _service;
        public WMSServiceClient service
        {
            get
            {

                if (_service == null)
                    return new WMSServiceClient();
                else
                    return _service;
            }
        }



        public ProcessFile()
        {
            InitializeComponent();
            DataContext = this;

            //Load Process
            this.ProcessList = service.GetCustomProcess(new CustomProcess { IsSystem = true });

            //Load FileTypes
            this.FTypeList = service.GetConnection(new Connection
            {
                ConnectionType = new ConnectionType { RowID = CnnType.File }
            });

            //Printers
            this.PrinterList = service.GetConnection(new Connection { 
                ConnectionType = new ConnectionType { RowID = CnnType.Printer } });

        }


        #region Model

        private ProcessEntityResource _Record;
        public ProcessEntityResource Record
        {
            get { return _Record; }
            set
            {
                _Record = value;
                OnPropertyChanged("Record");
            }
        }


        private IList<Connection> _PrinterList;
        public IList<Connection> PrinterList
        {
            get { return _PrinterList; }
            set
            {
                _PrinterList = value;
                OnPropertyChanged("PrinterList");
            }
        }


        private IList<CustomProcess> _ProcessList;
        public IList<CustomProcess> ProcessList
        {
            get { return _ProcessList; }
            set
            {
                _ProcessList = value;
                OnPropertyChanged("ProcessList");
            }
        }


        private IList<Connection> _FTypeList;
        public IList<Connection> FTypeList
        {
            get { return _FTypeList; }
            set
            {
                _FTypeList = value;
                OnPropertyChanged("FTypeList");
            }
        }


        private IList<ProcessEntityResource> _RecordList;
        public IList<ProcessEntityResource> RecordList
        {
            get { return _RecordList; }
            set
            {
                _RecordList = value;
                OnPropertyChanged("RecordList");
            }
        }



        public static DependencyProperty ClassEntityProperty = DependencyProperty.Register("ClassEntity", typeof(Int16), typeof(ProcessFile));

        public Int16 ClassEntity
        {
            get { return (Int16)GetValue(ClassEntityProperty); }
            set
            {
                SetValue(ClassEntityProperty, value);
            }
        }


        public static DependencyProperty RowIDProperty = DependencyProperty.Register("RowID", typeof(Int32), typeof(ProcessFile));

        public Int32 RowID
        {
            get { return (Int32)GetValue(RowIDProperty); }
            set
            {
                SetValue(RowIDProperty, value);
            }
        }



        #endregion


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            if (upFile.StreamFile == null || cboFtype.SelectedItem == null || cboProcess.SelectedItem == null)
            {
                Util.ShowError("File, File Type and Process are required.");
                return;
            }

            //Crear los registros en IMageEntityRelation y en PRocessEntityResource.

            ClassEntity curEntity = App.ClassEntityList.Where(f => f.ClassEntityID == this.ClassEntity).FirstOrDefault();

            try
            {

                //File to Save
                ImageEntityRelation file = new ImageEntityRelation
                {
                    FileType = (Connection)cboFtype.SelectedItem,
                    ImageName = Util.ExtractFileName(upFile.FileName),
                    Image = Util.GetImageByte(upFile.StreamFile),
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Entity = curEntity,
                    EntityRowID = this.RowID
                };

                file = service.SaveImageEntityRelation(file);

                //Process Entity Resourse to Save
                ProcessEntityResource per = new ProcessEntityResource
                {
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Entity = curEntity,
                    EntityRowID = this.RowID,
                    File = file,
                    Process = (CustomProcess)cboProcess.SelectedItem,
                    Printer = (Connection)cboPrinter.SelectedItem,
                    Status = App.EntityStatusList.Where(f => f.StatusID == EntityStatus.Active).First()
                };

                //Salvando el proceso
                service.SaveProcessEntityResource(per);


                //Obtreniendo la nueva lista
                LoadExistingList();


                cboFtype.SelectedIndex = -1;
                cboProcess.SelectedIndex = -1;
                upFile.txtLogo.Text = "";
                upFile.StreamFile =null;


                Util.ShowMessage("Record Created.");

            }
            catch (Exception ex)
            {
                Util.ShowError("Problem creating record.\n" + ex.Message);
                upFile.txtLogo.Text = "";
                upFile.StreamFile = null;
                return;
            }

        }

        private void chkSelectAllLines_Checked(object sender, RoutedEventArgs e)
        {

            this.lvFileProcess.SelectAll();
            this.lvFileProcess.Focus();

        }


        private void chkSelectAllLines_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lvFileProcess.UnselectAll();
        }


        private void btnRemBin_Click(object sender, RoutedEventArgs e)
        {
            if (lvFileProcess.SelectedItems == null)
                return;


            ImageEntityRelation curFile;
            foreach (object obj in lvFileProcess.SelectedItems)
            {
                try
                {
                    curFile = ((ProcessEntityResource)obj).File;
                    service.DeleteProcessEntityResource((ProcessEntityResource)obj);
                    service.DeleteImageEntityRelation(curFile);
                }
                catch { }
            }

            LoadExistingList();

        }






        #region INotifyPropertyChanged Members

        private event PropertyChangedEventHandler propertyChangedEvent;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChangedEvent += value; }
            remove { propertyChangedEvent -= value; }
        }

        protected void OnPropertyChanged(string prop)
        {
            if (propertyChangedEvent != null)
                propertyChangedEvent(this, new PropertyChangedEventArgs(prop));
        }

        #endregion



        internal void LoadResources(short classID, int rowID)
        {
            this.RowID = rowID;
            this.ClassEntity = classID;

            LoadExistingList();

        }

        private void LoadExistingList()
        {

            this.RecordList = service.GetProcessEntityResource(
                new ProcessEntityResource
                {
                    Entity = new ClassEntity { ClassEntityID = this.ClassEntity },
                    EntityRowID = this.RowID
                });

            stkFiles.Visibility = Visibility.Collapsed;
            if (this.RecordList != null && this.RecordList.Count > 0)
            {
                stkFiles.Visibility = Visibility.Visible;
                lvFileProcess.Items.Refresh();
            }
        }
    }
}
