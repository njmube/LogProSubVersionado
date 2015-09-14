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
 * Creado por: Jairo Murillo
 * Fecha: Nov 02 / 2010
 */

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SearchAccount.xaml
    /// </summary>
    public partial class CasNumberRule : UserControl, INotifyPropertyChanged
    {

        public CasNumberRule()
        {
            InitializeComponent();
            DataContext = this;

            //Carga la lista de Regulations
            RegulatorList = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REGCOM" } })
                .OrderBy(f=>f.NumOrder).ToList();
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


        private C_CasNumber _CasNumber;
        public C_CasNumber CasNumber
        {
            get { return _CasNumber; }
            set
            {
                _CasNumber = value;
                OnPropertyChanged("CasNumber");
            }
        }

        private IList<C_CasNumberRule> _RecordList;
        public IList<C_CasNumberRule> RecordList
        {
            get { return _RecordList; }
            set
            {
                _RecordList = value;
                OnPropertyChanged("RecordList");
            }
        }


        private IList<MMaster> _RegulatorList;
        public IList<MMaster> RegulatorList
        {
            get { return _RegulatorList; }
            set
            {
                _RegulatorList = value;
                OnPropertyChanged("RegulatorList");
            }
        }


        private  MMaster _Regulator;
        public MMaster Regulator 
        {
            get { return _Regulator; }
            set
            {
                _Regulator = value;
                OnPropertyChanged("Regulator");
            }
        }


        #endregion


        public void LoadData(C_CasNumber casnumber, bool reset)
        {
            
            this.CasNumber = casnumber;
            RecordList = null;

            if (reset)
            {
                //Regulator = null;
                //lvRules.SelectedIndex = -1;
            }

            if (Regulator == null)
            {
                lvRules.Items.Refresh();
                return;
            }

            //Y la lista de Rules
            RecordList = service.GetC_CasNumberRule(new C_CasNumberRule {
                CasNumber = this.CasNumber,
                Rule = new MMaster { MetaType = new MType { Code = Regulator.Code } }
            });


            lvRules.Items.Refresh();
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



        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string error = "";
            //Actualizar los Rules.
            foreach (C_CasNumberRule r in RecordList)
            {
                try
                {
                    if (r.RowID == 0)
                    {
                        r.CreatedBy = App.curUser.UserName;
                        r.CreationDate = DateTime.Now;
                        service.SaveC_CasNumberRule(r);
                    }
                    else
                    {
                        r.ModifiedBy = App.curUser.UserName;
                        r.ModDate = DateTime.Now;
                        service.UpdateC_CasNumberRule(r);
                    }

                }
                catch (Exception ex) {
                    error += r.Rule.Name + " " + ex.Message + "\n";
                }
            }

            if (string.IsNullOrEmpty(error))
                Util.ShowMessage("Process Completed.");
            else
                Util.ShowError(error);


        }



        private void ls_Regulation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            Regulator = (MMaster)e.AddedItems[0];

            LoadData(this.CasNumber, false);
        }


    }
}
