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
using System.Collections;
using System.Collections.Specialized;
using WpfFront.Common.Query;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for PopUpDocumentDetail.xaml
    /// </summary>
    public partial class QueryFilter : UserControl, INotifyPropertyChanged
    {

        public QueryFilter()
        {
            InitializeComponent();
            DataContext = this;
        }

        public StringDictionary StrOperator { get { return Operators.GetStrOperator(); } }
        public StringDictionary NumOperator { get { return Operators.GetNumOperator(); } }
        public StringDictionary Aggregation { get { return Operators.GetAggregation(); } }


        private IqReportColumn _RepColumn;
        public IqReportColumn RepColumn
        {
            get { return _RepColumn; }
            set
            {
                _RepColumn = value;
                OnPropertyChanged("RepColumn");
            }
        }


        private void cboStrComp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stkFilter2.Visibility = Visibility.Collapsed;

            if (e.AddedItems.Count == 0)
                return;

            DictionaryEntry operatorItem = (DictionaryEntry)cboStrComp.SelectedItem;

            if (operatorItem.Key.Equals("between (range)"))
                stkFilter2.Visibility = Visibility.Visible;
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
