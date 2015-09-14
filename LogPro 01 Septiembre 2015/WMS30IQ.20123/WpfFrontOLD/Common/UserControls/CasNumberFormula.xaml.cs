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

/*
 * Creado por: Jorge Armando Ortega
 * Fecha: Octubre 21 / 2010
 */

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class CasNumberFormula : UserControl, INotifyPropertyChanged
    {

        public CasNumberFormula()
        {
            InitializeComponent();
            DataContext = this;

            //Inicializo los listados y las variables
            Record = new C_CasNumberFormula();
            CasNumber = service.GetC_CasNumber(new C_CasNumber());
        }


        #region Model

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

        private C_CasNumberFormula _Record;
        public C_CasNumberFormula Record
        {
            get { return _Record; }
            set
            {
                _Record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<C_CasNumberFormula> _RecordList;
        public IList<C_CasNumberFormula> RecordList
        {
            get { return _RecordList; }
            set
            {
                _RecordList = value;
                OnPropertyChanged("RecordList");
            }
        }

        private IList<C_CasNumber> _CasNumber;
        public IList<C_CasNumber> CasNumber
        {
            get { return this._CasNumber; }
            set
            {
                _CasNumber = value;
                OnPropertyChanged("CasNumber");
            }
        }

        public static DependencyProperty ProductProperty = DependencyProperty.Register("Product", typeof(Product), typeof(CasNumberFormula));

        public Product Product
        {
            get { return (Product)GetValue(ProductProperty); }
            set
            {
                SetValue(ProductProperty, value);
            }
        }



        #endregion


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            if (ls_CasNumber.C_CasNumber == null || txt_percent.Text == "")
            {
                Util.ShowError("Cas# and Percent are required.");
                return;
            }

            try
            {
                Int32 zz = Int32.Parse(txt_percent.Text);

                if (zz == 0)
                    throw new Exception();

            }
            catch
            {
                Util.ShowError("Please enter a valid number.");
                return;
            }


            if (RecordList != null)
            {
                if ((RecordList.Sum(f => f.Percent) + Int32.Parse(txt_percent.Text)) > 100)
                {
                    Util.ShowError("Percent can't pass to 100%.");
                    return;
                }
            }

            /*if (service.GetC_CasNumberFormula(new C_CasNumberFormula { CasNumberComponent = (C_CasNumber)ls_CasNumber.SelectedItem }) != null)
            {
                Util.ShowError("Cas# already selected");
                return;
            }*/

                try
                {
                    //Guardo el registro
                    Record.CreatedBy = App.curUser.UserName;
                    Record.CreationDate = DateTime.Now;
                    Record.CasNumberComponent = (C_CasNumber)ls_CasNumber.C_CasNumber;
                    Record.Product = this.Product;
                    Record = service.SaveC_CasNumberFormula(Record);

                    //Obteniendo la lista con el nuevo dato
                    LoadExistingList();

                    //reinicio los campos para adicionar nuevos datos
                    txt_percent.Text = "";

                    Util.ShowMessage("Record Created.");

                }
                catch (Exception ex)
                {
                    Util.ShowError("Problem creating record.\n" + ex.Message);
                    txt_percent.Text = "";
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


            C_CasNumberFormula curFile;
            foreach (object obj in lvFileProcess.SelectedItems)
            {
                try
                {
                    curFile = (C_CasNumberFormula)obj;
                    service.DeleteC_CasNumberFormula(curFile);
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

        public void LoadExistingList()
        {
            if (this.Product != null)
            {
                this.RecordList = service.GetC_CasNumberFormula(new C_CasNumberFormula { Product = this.Product });

                //stkFiles.Visibility = Visibility.Collapsed;
                //if (this.RecordList != null && this.RecordList.Count > 0)
                //{
                    //stkFiles.Visibility = Visibility.Visible;
                    lvFileProcess.Items.Refresh();
                //}
            }
        }
    }
}
