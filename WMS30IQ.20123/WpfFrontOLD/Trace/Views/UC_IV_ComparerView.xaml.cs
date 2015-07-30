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
using WpfFront.Common.UserControls;
using WMComposite.Events;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using Xceed.Wpf.DataGrid;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for UC_IV_Comparer.xaml
    /// </summary>
    public partial class UC_IV_ComparerView : UserControl, IUC_IV_ComparerView
    {
        public UC_IV_ComparerView()
        {
            InitializeComponent();
        }



        //Events
        public event EventHandler<EventArgs> ProcessReplenish;
        public event EventHandler<EventArgs> LoadReplenishment;
        public event EventHandler<EventArgs> SelectAll;
        public event EventHandler<EventArgs> UnSelectAll;
        public event EventHandler<DataEventArgs<String>> FilterByBin;
        public event EventHandler<DataEventArgs<String>> FilterByProduct;
        public event EventHandler<DataEventArgs<ShowData>> ChangeSelector;


        public UC_IV_Model Model
        {
            get
            { return this.DataContext as UC_IV_Model; }
            set
            { this.DataContext = value; }

        }



        public ComboBox CboProduct
        {
            get { return this.cboProduct; }
            set { this.cboProduct = value; }
        }

        public DataGridControl DgList
        {
            get { return this.dgList; }
            set { this.dgList = value; }
        }


        public BinRange BinRange
        {
            get { return this.bRange; }
            set { this.bRange = value; }
        }

        public ComboBox CboSelector
        {
            get { return this.cboSelector; }
            set { this.cboSelector = value; }
        }


        private void btnProces_Click(object sender, RoutedEventArgs e)
        {
            btnProces.Focus();
            //Recorre el data grid y gener ana orden de empaque para los marcados.
            //Que deben proceder de los Bins con Stock.
            ProcessReplenish(sender, e);
        }

        private void cboLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReplenishment(sender, e);
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectAll(sender, e);

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UnSelectAll(sender, e);
        }


        private void cboProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboProduct.SelectedItem == null)
                return;

            FilterByProduct(sender, new DataEventArgs<String>((String)cboProduct.SelectedItem));
        }

        private void imgRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadReplenishment(sender, e);
        }

        private void cboSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSelector.SelectedItem == null)
                return;

            ChangeSelector(sender, new DataEventArgs<ShowData>((ShowData)cboSelector.SelectedItem));
        }

        private void bRange_OnLoadRange(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.bRange.Text))
                return;

            FilterByBin(sender, new DataEventArgs<String>(this.bRange.Text));
        }

    }


    public interface IUC_IV_ComparerView
    {
        //Clase Modelo
        UC_IV_Model Model { get; set; }

        //Properties
        DataGridControl DgList { get; set; }
        ComboBox CboProduct { get; set; }
        ComboBox CboSelector { get; set; }
        BinRange BinRange { get; set; }

        //Events
        event EventHandler<EventArgs> ProcessReplenish;
        event EventHandler<EventArgs> LoadReplenishment;
        event EventHandler<EventArgs> SelectAll;
        event EventHandler<EventArgs> UnSelectAll;
        event EventHandler<DataEventArgs<String>> FilterByBin;
        event EventHandler<DataEventArgs<String>> FilterByProduct;
        event EventHandler<DataEventArgs<ShowData>> ChangeSelector;

    }
}
