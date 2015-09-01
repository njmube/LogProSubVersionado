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
using Microsoft.Win32;
using System.IO;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class UploadFile : UserControl, INotifyPropertyChanged
    {

        public event EventHandler OnFileUpload;


        public UploadFile()
        {
            InitializeComponent();
            DataContext = this;
        }

        //Image envent    
        protected void Button_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "AllFiles|*.*";

            if ((bool)open.ShowDialog())
            {
                //SetLogo(sender, new DataEventArgs<Stream>(open.OpenFile()));
                //txtLogo.Text = open.FileName;

                this.StreamFile = open.OpenFile();
                this.FileName = open.FileName;
                this.txtLogo.Text = open.FileName;

                EventHandler temp = OnFileUpload;
                if (temp != null)
                    temp(sender, e);

            }

        }


        private String _FileName;
        public String FileName
        {
            get { return _FileName; }
            set
            {
                _FileName = value;
            }
        }


        private Stream _StreamFile;
        public Stream StreamFile
        {
            get { return _StreamFile; }
            set
            {
                _StreamFile = value;
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

              

    }
}
