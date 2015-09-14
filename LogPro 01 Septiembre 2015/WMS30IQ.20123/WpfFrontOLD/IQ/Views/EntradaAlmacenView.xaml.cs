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
    /// Interaction logic for EntradaAlmacenView.xaml
    /// </summary>
    public partial class EntradaAlmacenView : UserControlBase, IEntradaAlmacenView
    {
        #region Header

        public event EventHandler<DataEventArgs<Bin>> CargarHeader;
        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> CancelBasicData;
        public event EventHandler<EventArgs> NewBasicData;
        /// <summary>
        /// Muetra la copia de la informacion de la lista de Entrada de Almacen.
        /// </summary>
        public event EventHandler<EventArgs> MostrarInformacion;

        private bool ShowHeader = true;

        #endregion

        #region Serial

        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;

        #endregion

        #region Details

        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> Imprimir;

        #endregion


        public EntradaAlmacenView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";

            //19/02/2013: Nuevas lineas de codigo 
            //Si se ha seleccionado el cliente DIRECTV, este boton estara habilitado y visible, de lo contrario, no lo estara.
            if (Text_Documento.Text == "DIRECTV")
            {
                Btn_AgregarTarjetas.Visibility = Visibility.Visible;
                Btn_AgregarTarjetas.IsEnabled = true;
            }
            else
            {
                Btn_AgregarTarjetas.Visibility = Visibility.Collapsed;
                Btn_AgregarTarjetas.IsEnabled = false;
            }
        }

        #region Variables

        public EntradaAlmacenModel Model
        {
            get
            { return this.DataContext as EntradaAlmacenModel; }
            set
            { this.DataContext = value; }
        }

        #region Header

        public ComboBox GetListBinInicio
        {
            get { return this.cb_BinInicio; }
            set { this.cb_BinInicio = value; }
        }

        public TextBlock GetTextHideShowHeader
        {
            get { return this.Text_ShowHide; }
            set { this.Text_ShowHide = value; }
        }

        public Border GetBorderHeader
        {
            get { return this.Border_Header; }
            set { this.Border_Header = value; }
        }

        public TextBlock GetNombreCliente
        {
            get { return this.txt_Cliente; }
            set { this.txt_Cliente = value; }
        }

        public StackPanel GetStackPanelHeader
        {
            get { return this.Stack_Panel_Header; }
            set { this.Stack_Panel_Header = value; }
        }

        public StackPanel GetStackUploadFile
        {
            get { return this.Stack_UploadFile; }
            set { this.Stack_UploadFile = value; }
        }

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }

        public Button GetButtonCancelar
        {
            get { return this.btn_cancelar; }
            set { this.btn_cancelar = value; }
        }

        public Button GetButtonNuevo
        {
            get { return this.btn_nuevo; }
            set { this.btn_nuevo = value; }
        }

        #endregion

        #region Serial

        public TextBox GetCantidadProducto
        {
            get { return this.tb_Cantidad; }
            set { this.tb_Cantidad = value; }
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

        public TextBox GetSerial1
        {
            get { return this.tb_Serial1; }
            set { this.tb_Serial1 = value; }
        }

        public TextBox GetSerial2
        {
            get { return this.tb_Serial2; }
            set { this.tb_Serial2 = value; }
        }

        public TextBox GetSerial3
        {
            get { return this.tb_Serial3; }
            set { this.tb_Serial3 = value; }
        }

        public SearchProduct GetProductLocation
        {
            get { return this.sp_Product; }
            set { this.sp_Product = value; }
        }

        public UploadFile GetUpLoadFile
        {
            get { return this.fUpload; }
            set { this.fUpload = value; }
        }

        #endregion

        #region Details

        public ListView LvDocumentMaster
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public GridView GetGridViewDetails
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        /// <summary>
        /// Texblock de recuento de filas.
        /// </summary>
        public TextBlock RecuentoFilas
        {
            get { return this.textblock_recuento; }
            set { this.textblock_recuento = value; }
        }

        public TextBlock RegistrosNoCargados
        {
            get { return this.txt_RegistrosNoCargados; }
            set { this.txt_RegistrosNoCargados = value; }
        }

        #endregion

        #region Variables varias

        public TextBlock NumeroDelDocumento { get { return this.Text_Documento; } set { this.Text_Documento = value; } }

        #endregion

        #endregion

        #region Metodos

        #region Header

        private void cb_BinInicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarHeader(sender, new DataEventArgs<Bin>((Bin)GetListBinInicio.SelectedItem));
        }

        private void Text_ShowHide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowHeader = !ShowHeader;
            if (ShowHeader)
            {
                GetBorderHeader.Visibility = Visibility.Visible;
                GetTextHideShowHeader.Text = "<< Ocultar";
            }
            else
            {
                GetBorderHeader.Visibility = Visibility.Collapsed;
                GetTextHideShowHeader.Text = "Mostrar >>";
            }
        }

        private void btn_confirmar_Click(object sender, RoutedEventArgs e)
        {
            //Evaluo si fue seleccionado el Bin de ingreso
            if (GetListBinInicio.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar una ubicacion para el ingreso de los equipos.");
                return;
            }
            ConfirmBasicData(sender, e);

        }

        private void btn_cancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelBasicData(sender, e);

            //Nuevas lineas
            Border_Detail.Visibility = Visibility.Collapsed;
            Border_Header.IsEnabled = false;
        }

        private void btn_nuevo_Click(object sender, RoutedEventArgs e)
        {
            NewBasicData(sender, e);
        }

        #endregion

        #region Serial

        private void sp_Product_OnLoadRecord(object sender, EventArgs e)
        {
            EvaluarTipoProducto(sender, new DataEventArgs<Product>(GetProductLocation.Product));
        }

        private void tb_Cantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetSerial1.IsEnabled)
                    GetSerial1.Focus();
                else
                    AddLine(sender, e);
            }
        }

        private void tb_Serial1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSerial2.Visibility == Visibility.Visible)
                    GetSerial2.Focus();
                else
                    if (GetStackSerial3.Visibility == Visibility.Visible)
                        GetSerial3.Focus();
                    else
                        AddLine(sender, e);
            }
        }

        private void tb_Serial2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSerial3.Visibility == Visibility.Visible)
                    GetSerial3.Focus();
                else
                    AddLine(sender, e);
            }
        }

        private void tb_Serial3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
            }
        }

        private void fUpload_OnFileUpload(object sender, EventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            //Procesar el Archivo Cargado
            if (fUpload.StreamFile != null)
            {
                string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

                ProcessFile(sender, e, dataFile);
            }
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();

        }

        private void ProcessFile(object sender, EventArgs e, string dataFile)
        {

            //Linea por linea obtiene el dato del serial y del RR y lo procesa.
            //Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            if (lines == null || lines.Rows.Count == 0)
            {
                Util.ShowError("No hay registros a procesar.\n" + dataFile);
                return;
            }

            int NumeroSerial;
            foreach (DataRow dr in lines.Rows)
            {
                NumeroSerial = 1;
                foreach (DataColumn dc in lines.Columns)
                {
                    switch (NumeroSerial.ToString())
                    {
                        case "1":
                            if (GetStackSerial1.Visibility == Visibility.Visible)
                                GetSerial1.Text = dr[dc.ColumnName].ToString();
                            break;
                        case "2":
                            if (GetStackSerial2.Visibility == Visibility.Visible)
                                GetSerial2.Text = dr[dc.ColumnName].ToString();
                            break;
                        case "3":
                            if (GetStackSerial3.Visibility == Visibility.Visible)
                                GetSerial3.Text = dr[dc.ColumnName].ToString();
                            break;
                    }
                    NumeroSerial++;
                }

                AddLine(sender, e);
            }

            fUpload.StreamFile = null;

        }

        #endregion

        #region Details

        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            Btn_Copiar.IsEnabled = true;
        }

        #endregion

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Replicando registros...por favor espere...");
            //Replica la informacion de la primera linea en las demas.
            ReplicateDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            chkRep.IsChecked = false;
        }

        #endregion

        /// <summary>
        /// Muetra la copia de la informacion de la lista de Entrada de Almacen.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Copiar_Click(object sender, RoutedEventArgs e)
        {
            MostrarInformacion(sender, e);
        }

        private void Btn_Imprimir_Click(object sender, RoutedEventArgs e)
        {
            Imprimir(sender, e);
        }


    }

    public interface IEntradaAlmacenView
    {
        //Clase Modelo
        EntradaAlmacenModel Model { get; set; }

        #region Obtener Variables

        #region Header

        ComboBox GetListBinInicio { get; set; }
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        StackPanel GetStackPanelHeader { get; set; }
        StackPanel GetStackUploadFile { get; set; }
        TextBlock GetNombreCliente { get; set; }
        Button GetButtonConfirmar { get; set; }
        Button GetButtonCancelar { get; set; }
        Button GetButtonNuevo { get; set; }

        #endregion

        #region Serial

        TextBox GetCantidadProducto { get; set; }
        StackPanel GetStackSerial1 { get; set; }
        StackPanel GetStackSerial2 { get; set; }
        StackPanel GetStackSerial3 { get; set; }
        TextBlock GetTextSerial1 { get; set; }
        TextBlock GetTextSerial2 { get; set; }
        TextBlock GetTextSerial3 { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox GetSerial3 { get; set; }
        SearchProduct GetProductLocation { get; set; }
        UploadFile GetUpLoadFile { get; set; }

        #endregion

        #region Details

        ListView LvDocumentMaster { get; set; }
        GridView GetGridViewDetails { get; set; }
        Border BorderDetails { get; set; }

        //Nuevo campo
        TextBlock RecuentoFilas { get; set; }
        TextBlock RegistrosNoCargados { get; set; }

        #endregion

        #region Variables varias

        TextBlock NumeroDelDocumento { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Header

        event EventHandler<DataEventArgs<Bin>> CargarHeader;
        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> CancelBasicData;
        event EventHandler<EventArgs> NewBasicData;
        /// <summary>
        /// Muetra la copia de la informacion de la lista de Entrada de Almacen.
        /// </summary>
        event EventHandler<EventArgs> MostrarInformacion;

        #endregion

        #region Serial

        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;

        #endregion

        #region Details

        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> Imprimir;

        #endregion

        #endregion

    }
}