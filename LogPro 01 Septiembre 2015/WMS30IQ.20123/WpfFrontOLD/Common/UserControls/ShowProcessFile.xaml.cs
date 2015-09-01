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
using System.ComponentModel;
using WpfFront.Services;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for ShowDocuments.xaml
    /// </summary>
    public partial class ShowProcessFile : UserControl, INotifyPropertyChanged
    {

        public ShowProcessFile()
        {
            InitializeComponent();
            DataContext = this;
        }


        private WMSServiceClient _service;
        public WMSServiceClient service
        {
            get {

                if (_service == null)
                    return new WMSServiceClient();
                else
                    return _service;
            }
        }


        private IList<ProcessEntityResource> _DataList;
        public IList<ProcessEntityResource> DataList
        {
            get { return _DataList; }
            set
            {
                _DataList = value;
                OnPropertyChanged("DataList");
            }
        }


        public static DependencyProperty ClassEntityProperty = DependencyProperty.Register("ClassEntity", typeof(Int16), typeof(ShowProcessFile));

        public Int16 ClassEntity
        {
            get { return (Int16)GetValue(ClassEntityProperty); }
            set
            {
                SetValue(ClassEntityProperty, value);
            }
        }


        public static DependencyProperty RowIDProperty = DependencyProperty.Register("RowID", typeof(Int32), typeof(ShowProcessFile));

        public Int32 RowID
        {
            get { return (Int32)GetValue(RowIDProperty); }
            set
            {
                SetValue(RowIDProperty, value);
            }
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


        public void LoaFiles()
        {
            if (this.ClassEntity > 0 && this.RowID > 0)
                DataList = service.GetProcessEntityResource(
                    new ProcessEntityResource
                    {
                        EntityRowID = this.RowID,
                        Entity = new ClassEntity { ClassEntityID = this.ClassEntity }                        
                    });
        }


        private void GridDetails_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProcessWindow pw = new ProcessWindow("Displaying File ...");

            try
            {
                //Para el selected Item despliega el archivo.
                if (GridDetails.SelectedItem == null)
                    return;

                ProcessEntityResource per = GridDetails.SelectedItem as ProcessEntityResource;
                if (per.Template != null)
                    UtilWindow.ShowDocument(per.Template, per.EntityRowID, "", false);

                else if (per.File != null)
                    UtilWindow.ShowFile(per.File);

            }
            catch (Exception ex) { Util.ShowError("Fiel could not be displayed.\n" + ex.Message); }
            finally { pw.Close(); }

        }


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Recrear files nuevamente
            if (this.ClassEntity == EntityID.Document)
            {
                CustomProcess process;
                try { process = service.GetCustomProcess(new CustomProcess { Name = BasicProcess.Shipping }).First(); }
                catch { process = null; }

                service.PrintDocumentsInBatch(new List<Document> { new Document { DocID = this.RowID } }, null, process);
            }

            LoaFiles();
        }
    }
}
