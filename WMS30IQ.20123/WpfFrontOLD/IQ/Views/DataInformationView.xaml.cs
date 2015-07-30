using System;
using System.Windows.Controls;
using Core.WPF;
using WMComposite.Events; 
using Assergs.Windows;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common;
using Microsoft.Windows.Controls;
using WpfFront.Models;
using System.Windows.Input;
using WpfFront.WMSBusinessService;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for DataInformationView.xaml
    /// </summary>
    public partial class DataInformationView : UserControlBase, IDataInformationView
    {
        //Principal
        public event EventHandler<DataEventArgs<Location>> LoadLocation;
        //DataDefinition
        public event EventHandler<DataEventArgs<string>> LoadSearchDataDefinition;
        public event EventHandler<EventArgs> NewDataDefinition;
        public event EventHandler<DataEventArgs<DataDefinition>> LoadDataDefinition;
        public event EventHandler<EventArgs> SaveDataDefinition;
        public event EventHandler<EventArgs> DeleteDataDefinition;
        public event EventHandler<EventArgs> ShowHideMetaType;
        //DataDefinitionByBin
        public event EventHandler<DataEventArgs<Bin>> LoadDataDefinitionByBin;
        public event EventHandler<EventArgs> AddDataDefinitionByBin;
        public event EventHandler<EventArgs> DeleteDataDefinitionByBin;
        //BinRoute
        public event EventHandler<DataEventArgs<Location>> LoadBinToList;
        public event EventHandler<EventArgs> SaveBinRoute;
        public event EventHandler<EventArgs> DeleteBinRoute;

        public DataInformationView()
        {
            InitializeComponent();
        }

        #region Properties

        public DataInformationModel Model
        {
            get
            { return this.DataContext as DataInformationModel; }
            set
            { this.DataContext = value; }
        }

        //Principal
        public ComboBox GetLocationList
        {
            get { return this.ls_Location; }
            set { this.ls_Location = value; }
        }

        public StackPanel GetStackMenu
        {
            get { return this.Stack_Menu; }
            set { this.Stack_Menu = value; }
        }

        //DataDefinition
        public DataGridControl GetDataDefinitionList
        {
            get { return this.dgListDataDefinition; }
            set { this.dgListDataDefinition = value; }
        }

        public TextBox TextSearchDataDefinition
        {
            get { return this.txtSearchDataDefinition; }
            set { this.txtSearchDataDefinition = value; }
        }

        public Border GetBorderDataDefinition
        {
            get { return this.Border_FieldsDataDefinition; }
            set { this.Border_FieldsDataDefinition = value; }
        }

        public TextBox GetNombreCampo
        {
            get { return this.txt_NombreCampoDataDefinition; }
            set { this.txt_NombreCampoDataDefinition = value; }
        }

        public CheckBox GetCheckIsSerial
        {
            get { return this.chk_Serial; }
            set { this.chk_Serial = value; }
        }

        public TextBox GetTextSizeSerial
        {
            get { return this.tb_TamanoSerial; }
            set { this.tb_TamanoSerial = value; }
        }

        public ComboBox GetDataType
        {
            get { return this.ls_WFDataType; }
            set { this.ls_WFDataType = value; }
        }

        public StackPanel GetStackPanelMetaType
        {
            get { return this.Stack_MetaType; }
            set { this.Stack_MetaType = value; }
        }

        public ComboBox GetMetaType
        {
            get { return this.ls_MetaType; }
            set { this.ls_MetaType = value; }
        }

        public TextBox GetCode
        {
            get { return this.txt_Code; }
            set { this.txt_Code = value; }
        }

        //DataDefinitionByBin
        public ComboBox GetBinList
        {
            get { return this.ls_Bin; }
            set { this.ls_Bin = value; }
        }

        public ListView GetDataDefinitionListNotUse
        {
            get { return this.lvListDataDefinitionNotUse; }
            set { this.lvListDataDefinitionNotUse = value; }
        }

        public ListView GetDataDefinitionByBinListUsed
        {
            get { return this.lvListDataDefinitionUsed; }
            set { this.lvListDataDefinitionUsed = value; }
        }

        //BinRoute
        public ComboBox GetBinFromList
        {
            get { return this.ls_BinFrom; }
            set { this.ls_BinFrom = value; }
        }

        public ComboBox GetBinToList
        {
            get { return this.ls_BinTo; }
            set { this.ls_BinTo = value; }
        }

        public ListView GetBinRouteList
        {
            get { return this.lvListBinRouteAsigned; }
            set { this.lvListBinRouteAsigned = value; }
        }

        public ComboBox GetBinRouteLocationToList
        {
            get { return this.ls_LocationTo; }
            set { this.ls_LocationTo = value; }
        }

        #endregion


        #region Metodos

        //Principal
        private void ls_Location_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetLocationList.SelectedItem == null)
                return;

            LoadLocation(sender, new DataEventArgs<Location>((Location)GetLocationList.SelectedItem));
        }

        //DataDefinition
        private void txtSearchDataDefinition_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSearchDataDefinition(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        }

        private void btnNewDataDefinition_Click(object sender, RoutedEventArgs e)
        {
            NewDataDefinition(sender, e);
        }

        private void dgListDataDefinition_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadDataDefinition(sender, new DataEventArgs<DataDefinition>((DataDefinition)GetDataDefinitionList.SelectedItem));
        }

        private void btn_SaveDataDefinition_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(GetNombreCampo.Text))
            {
                Util.ShowError("Por favor digite el nombre del campo");
                return;
            }
            if (String.IsNullOrEmpty(GetCode.Text))
            {
                Util.ShowError("Por favor digite el codigo");
                return;
            }
            if (GetCheckIsSerial.IsChecked == true && String.IsNullOrEmpty(GetTextSizeSerial.Text))
            {
                Util.ShowError("Por favor digite el tamano para el serial");
                return;
            }
            if (GetCheckIsSerial.IsChecked == true && !GetCode.Text.Contains("SERIAL1") && !GetCode.Text.Contains("SERIAL2") && !GetCode.Text.Contains("SERIAL3"))
            {
                Util.ShowError("Codigo: " + GetCode.Text + ", no es valido para el tipo de dato serial, los codigos validos son SERIAL1, SERIAL2, SERIAL3.");
                return;
            }
            SaveDataDefinition(sender, e);
        }

        private void btn_DeleteDataDefinition_Click(object sender, RoutedEventArgs e)
        {
            DeleteDataDefinition(sender, e);
        }

        private void ls_WFDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowHideMetaType(sender, e);
        }

        //DataDefinitionByBin
        private void ls_Bin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetBinList.SelectedItem == null)
                return;
            LoadDataDefinitionByBin(sender, new DataEventArgs<Bin>((Bin)GetBinList.SelectedItem));
        }

        private void btn_AddDataDefinitionByBin_Click(object sender, RoutedEventArgs e)
        {
            AddDataDefinitionByBin(sender, e);
        }

        private void btn_DelDataDefinitionByBin_Click(object sender, RoutedEventArgs e)
        {
            DeleteDataDefinitionByBin(sender, e);
        }

        //BinRoute
        private void ls_LocationTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBinToList(sender, new DataEventArgs<Location>((Location)GetBinRouteLocationToList.SelectedItem));
        }

        private void btn_AddBinRoute_Click(object sender, RoutedEventArgs e)
        {
            if (GetBinFromList.SelectedItem == null)
                return;
            if (GetBinToList.SelectedItem == null)
                return;
            SaveBinRoute(sender, e);
        }

        private void btn_DelBinRoute_Click(object sender, RoutedEventArgs e)
        {
            DeleteBinRoute(sender, e);
        }

        #endregion

    }


    public interface IDataInformationView
    {
        //Clase Modelo
        DataInformationModel Model { get; set; }

        //Principal
        ComboBox GetLocationList { get; set; }
        StackPanel GetStackMenu { get; set; }
        //DataDefinition
        DataGridControl GetDataDefinitionList { get; set; }
        TextBox TextSearchDataDefinition { get; set; }
        Border GetBorderDataDefinition { get; set; }
        TextBox GetNombreCampo { get; set; }
        CheckBox GetCheckIsSerial { get; set; }
        TextBox GetTextSizeSerial { get; set; }
        ComboBox GetDataType { get; set; }
        StackPanel GetStackPanelMetaType { get; set; }
        ComboBox GetMetaType { get; set; }
        TextBox GetCode { get; set; }
        //DataDefinitionByBin
        ComboBox GetBinList { get; set; }
        ListView GetDataDefinitionListNotUse { get; set; }
        ListView GetDataDefinitionByBinListUsed { get; set; }
        //BinRoute
        ComboBox GetBinFromList { get; set; }
        ComboBox GetBinToList { get; set; }
        ListView GetBinRouteList { get; set; }
        ComboBox GetBinRouteLocationToList { get; set; }

        //Principal
        event EventHandler<DataEventArgs<Location>> LoadLocation;
        //DataDefinition
        event EventHandler<DataEventArgs<string>> LoadSearchDataDefinition;
        event EventHandler<EventArgs> NewDataDefinition;
        event EventHandler<DataEventArgs<DataDefinition>> LoadDataDefinition;
        event EventHandler<EventArgs> SaveDataDefinition;
        event EventHandler<EventArgs> DeleteDataDefinition;
        event EventHandler<EventArgs> ShowHideMetaType;
        //DataDefinitionByBin
        event EventHandler<DataEventArgs<Bin>> LoadDataDefinitionByBin;
        event EventHandler<EventArgs> AddDataDefinitionByBin;
        event EventHandler<EventArgs> DeleteDataDefinitionByBin;
        //BinRoute
        event EventHandler<DataEventArgs<Location>> LoadBinToList;
        event EventHandler<EventArgs> SaveBinRoute;
        event EventHandler<EventArgs> DeleteBinRoute;
    }
}