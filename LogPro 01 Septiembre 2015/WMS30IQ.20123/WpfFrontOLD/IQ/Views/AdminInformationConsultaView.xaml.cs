using System;
using System.Windows.Controls;
using Core.WPF;
using WMComposite.Events; 
using Assergs.Windows;
using System.Windows;
using WpfFront.Common;
using Microsoft.Windows.Controls;
using WpfFront.Models;
using System.Windows.Input;
using System.Data;
using WpfFront.Common.UserControls;
using WpfFront.WMSBusinessService;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for AdminInformationConsultaView.xaml
    /// </summary>
    public partial class AdminInformationConsultaView : UserControlBase, IAdminInformationConsultaView
    {

        #region Busqueda

        public event EventHandler<DataEventArgs<ClassEntity>> LoadData;
        public event EventHandler<DataEventArgs<ClassEntity>> ShowFields;

        #endregion

        #region Datos Estaticos

        public event EventHandler<DataEventArgs<Product>> AsignarProducto;
        public event EventHandler<DataEventArgs<Location>> CargarDatos;

        #endregion

        #region Datos Dinamicos

        #endregion

        #region Eventos Botones

        public event EventHandler<EventArgs> UpdateData;
        public event EventHandler<EventArgs> DeleteData;

        #endregion


        public AdminInformationConsultaView()
        {
            InitializeComponent();
        }

        #region Variables

        public AdminInformationConsultaModel Model
        {
            get
            { return this.DataContext as AdminInformationConsultaModel; }
            set
            { this.DataContext = value; }
        }

        #region Busqueda

        public ComboBox GetTipoBusqueda
        {
            get { return this.cb_Tipo; }
            set { this.cb_Tipo = value; }
        }

        /*public TextBox GetNumeroBusqueda
        {
            get { return this.tb_NumeroBusqueda; }
            set { this.tb_NumeroBusqueda = value; }
        }*/

        public StackPanel GetStackSearchSerial1
        {
            get { return this.Stack_SearchSerial1; }
            set { this.Stack_SearchSerial1 = value; }
        }

        public StackPanel GetStackSearchSerial2
        {
            get { return this.Stack_SearchSerial2; }
            set { this.Stack_SearchSerial2 = value; }
        }

        public StackPanel GetStackSearchSerial3
        {
            get { return this.Stack_SearchSerial3; }
            set { this.Stack_SearchSerial3 = value; }
        }

        public TextBlock GetTextSearchSerial1
        {
            get { return this.txt_SearchSerial1; }
            set { this.txt_SearchSerial1 = value; }
        }

        public TextBlock GetTextSearchSerial2
        {
            get { return this.txt_SearchSerial2; }
            set { this.txt_SearchSerial2 = value; }
        }

        public TextBlock GetTextSearchSerial3
        {
            get { return this.txt_SearchSerial3; }
            set { this.txt_SearchSerial3 = value; }
        }

        public TextBox GetSearchSerial1
        {
            get { return this.tb_SearchSerial1; }
            set { this.tb_SearchSerial1 = value; }
        }

        public TextBox GetSearchSerial2
        {
            get { return this.tb_SearchSerial2; }
            set { this.tb_SearchSerial2 = value; }
        }

        public TextBox GetSearchSerial3
        {
            get { return this.tb_SearchSerial3; }
            set { this.tb_SearchSerial3 = value; }
        }

        public StackPanel GetStackDatosEstaticos
        {
            get { return this.Stack_DatosEstaticos; }
            set { this.Stack_DatosEstaticos = value; }
        }

        public StackPanel GetStackDatos
        {
            get { return this.Stack_Datos; }
            set { this.Stack_Datos = value; }
        }

        public StackPanel GetStackButtons
        {
            get { return this.Stack_Botones; }
            set { this.Stack_Botones = value; }
        }

        #endregion

        #region Datos Estaticos

        public StackPanel GetStackEstaticosDocumento
        {
            get { return this.Stack_Estaticos_Documento; }
            set { this.Stack_Estaticos_Documento = value; }
        }

        public StackPanel GetStackEstaticosLabel
        {
            get { return this.Stack_Estaticos_Label; }
            set { this.Stack_Estaticos_Label = value; }
        }

        public StackPanel GetStackSerial1
        {
            get { return this.Stack_Serial1; }
            set { this.Stack_Serial1 = value; }
        }

        public StackPanel GetStackSerial2
        {
            get { return this.Stack_Serial2; }
            set { this.Stack_Serial2 = value; }
        }

        public StackPanel GetStackSerial3
        {
            get { return this.Stack_Serial3; }
            set { this.Stack_Serial3 = value; }
        }

        public TextBlock GetTextSerial1
        {
            get { return this.txt_Serial1; }
            set { this.txt_Serial1 = value; }
        }

        public TextBlock GetTextSerial2
        {
            get { return this.txt_Serial2; }
            set { this.txt_Serial2 = value; }
        }

        public TextBlock GetTextSerial3
        {
            get { return this.txt_Serial3; }
            set { this.txt_Serial3 = value; }
        }

        public SearchProduct GetProductLabel
        {
            get { return this.sp_Product; }
            set { this.sp_Product = value; }
        }

        public ComboBox GetListClientes
        {
            get { return this.cb_Cliente; }
            set { this.cb_Cliente = value; }
        }

        public TextBlock GetClientes
        {
            get { return this.txt_Cliente; }
            set { this.txt_Cliente = value; }
        }

        #endregion

        #region Datos Dinamicos

        public StackPanel GetStackPanelDinamicos
        {
            get { return this.Stack_Panel_Dinamicos; }
            set { this.Stack_Panel_Dinamicos = value; }
        }

        #endregion

        #region Eventos Botones

        public Button GetButtonAnular
        {
            get { return this.Btn_Anular; }
            set { this.Btn_Anular = value; }
        }

        public Button GetButtonUpdate
        {
            get { return this.Btn_Actualizar; }
            set { this.Btn_Actualizar = value; }
        }

        #endregion

        #endregion


        #region Metodos

        #region Busqueda

        private void cb_Tipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetTipoBusqueda.SelectedIndex != -1)
            {
                ShowFields(sender, new DataEventArgs<ClassEntity>((ClassEntity)GetTipoBusqueda.SelectedItem));
            }
        }

        private void tb_SearchSerial1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSearchSerial2.Visibility == Visibility.Visible)
                    GetSearchSerial2.Focus();
                else
                    if (GetStackSearchSerial3.Visibility == Visibility.Visible)
                        GetSearchSerial3.Focus();
                    else
                        LoadData(sender, new DataEventArgs<ClassEntity>((ClassEntity)GetTipoBusqueda.SelectedItem));
            }
        }

        private void tb_SearchSerial2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSearchSerial3.Visibility == Visibility.Visible)
                    GetSearchSerial3.Focus();
                else
                    LoadData(sender, new DataEventArgs<ClassEntity>((ClassEntity)GetTipoBusqueda.SelectedItem));
            }
        }

        private void tb_SearchSerial3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadData(sender, new DataEventArgs<ClassEntity>((ClassEntity)GetTipoBusqueda.SelectedItem));
            }
        }

        #endregion

        #region Datos Estaticos

        private void sp_Product_OnLoadRecord(object sender, EventArgs e)
        {
            AsignarProducto(sender, new DataEventArgs<Product>(GetProductLabel.Product));
        }

        private void cb_Cliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarDatos(sender, new DataEventArgs<Location>((Location)GetListClientes.SelectedItem));
        }

        #endregion

        #region Datos Dinamicos

        #endregion

        #region Eventos Botones

        private void Btn_Actualizar_Click(object sender, RoutedEventArgs e)
        {
            UpdateData(sender, e);
        }

        private void Btn_Anular_Click(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea eliminar el equipo
            if (!UtilWindow.ConfirmOK("Esta seguro que desea eliminar este equipo del sistema?") == true)
                return;

            DeleteData(sender, e);
        }

        #endregion

        #endregion

    }

    public interface IAdminInformationConsultaView
    {
        //Clase Modelo
        AdminInformationConsultaModel Model { get; set; }

        #region Obtener Variables

        #region Busqueda

        ComboBox GetTipoBusqueda { get; set; }
        StackPanel GetStackSearchSerial1 { get; set; }
        StackPanel GetStackSearchSerial2 { get; set; }
        StackPanel GetStackSearchSerial3 { get; set; }
        TextBlock GetTextSearchSerial1 { get; set; }
        TextBlock GetTextSearchSerial2 { get; set; }
        TextBlock GetTextSearchSerial3 { get; set; }
        TextBox GetSearchSerial1 { get; set; }
        TextBox GetSearchSerial2 { get; set; }
        TextBox GetSearchSerial3 { get; set; }
        //TextBox GetNumeroBusqueda { get; set; }
        StackPanel GetStackDatosEstaticos { get; set; }
        StackPanel GetStackDatos { get; set; }
        StackPanel GetStackButtons { get; set; }

        #endregion

        #region Datos Estaticos

        StackPanel GetStackEstaticosDocumento { get; set; }
        StackPanel GetStackEstaticosLabel { get; set; }
        StackPanel GetStackSerial1 { get; set; }
        StackPanel GetStackSerial2 { get; set; }
        StackPanel GetStackSerial3 { get; set; }
        TextBlock GetTextSerial1 { get; set; }
        TextBlock GetTextSerial2 { get; set; }
        TextBlock GetTextSerial3 { get; set; }
        SearchProduct GetProductLabel { get; set; }
        ComboBox GetListClientes { get; set; }
        TextBlock GetClientes { get; set; }

        #endregion

        #region Datos Dinamicos

        StackPanel GetStackPanelDinamicos { get; set; }

        #endregion

        #region Eventos Botones

        Button GetButtonAnular { get; set; }
        Button GetButtonUpdate { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Busqueda

        event EventHandler<DataEventArgs<ClassEntity>> LoadData;
        event EventHandler<DataEventArgs<ClassEntity>> ShowFields;

        #endregion

        #region Datos Estaticos

        event EventHandler<DataEventArgs<Product>> AsignarProducto;
        event EventHandler<DataEventArgs<Location>> CargarDatos;

        #endregion

        #region Datos Dinamicos

        #endregion

        #region Eventos Botones

        event EventHandler<EventArgs> UpdateData;
        event EventHandler<EventArgs> DeleteData;

        #endregion

        #endregion

    }
}