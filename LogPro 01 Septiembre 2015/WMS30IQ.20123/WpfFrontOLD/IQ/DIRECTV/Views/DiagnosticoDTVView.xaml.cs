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
    /// Interaction logic for DiagnosticoDTVView.xaml
    /// </summary>
    public partial class DiagnosticoDTVView : UserControlBase, IDiagnosticoDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> GenerarCodigo;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ConfirmarImpresion;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        public event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

        public DiagnosticoDTVView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";
        }

        #region Variables

        public DiagnosticoDTVModel Model
        {
            get { return this.DataContext as DiagnosticoDTVModel; }
            set { this.DataContext = value; }
        }

        public ComboBox GetListBinInicio
        {
            get { return this.cb_BinInicio; }
            set { this.cb_BinInicio = value; }
        }

        public ComboBox GetListaEstado
        {
            get { return this.cb_BuscarItems; }
            set { this.cb_BuscarItems = value; }
        }
       
        public ComboBox Ubicacion
        {
            get { return this.cb_Ubicacion; }
            set { this.cb_Ubicacion = value; }
        }
        public ComboBox UnidadAlmacenamiento
        {
            get { return this.cb_UA; }
            set { this.cb_UA = value; }
        }

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
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

        public ListView ListadoItems
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
        }
        //public StackPanel GetStackUploadFile
        //{
        //    get { return this.Stack_UploadFile; }
        //    set { this.Stack_UploadFile = value; }
        //}

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }
        //public TextBox GetCantidadProducto
        //{
        //    get { return this.tb_Cantidad; }
        //    set { this.tb_Cantidad = value; }
        //}

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

        //public SearchProduct GetProductLocation
        //{
        //    get { return this.sp_Product; }
        //    set { this.sp_Product = value; }
        //}

        //public UploadFile GetUpLoadFile
        //{
        //    get { return this.fUpload; }
        //    set { this.fUpload = value; }
        //}

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public TextBox CodigoEmpaque
        {
            get { return this.txt_CodEmpaque; }
            set { this.txt_CodEmpaque = value; }
        }
        //public ListView ListadoEstibas
        //{
        //    get { return this.lvDocumentMaster_2; }
        //    set { this.lvDocumentMaster_2 = value; }
        //}

        //public ComboBox UbicacionDestino
        //{
        //    get { return this.cb_BodDestino; }
        //    set { this.cb_BodDestino = value; }
        //}

        //Recibo
        public TextBox BuscarEstibaRecibo
        {
            get { return this.tb_BuscarEstibaRecibo; }
            set { this.tb_BuscarEstibaRecibo = value; }
        }

        public TextBlock TotalSeriales
        {
            get { return this.textblock_totalSeriales1; }
            set { this.textblock_totalSeriales1 = value; }
        }

        public TextBlock Estibas_Seleccionadas
        {
            get { return this.textblock_totalEstibas1; }
            set { this.textblock_totalEstibas1 = value; }
        }


        //public ComboBox BuscarPosicionRecibo
        //{
        //    get { return this.cb_BuscarPosicionRecibo; }
        //    set { this.cb_BuscarPosicionRecibo = value; }
        //}

        public ListView ListadoBusquedaRecibo
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
        }

        public ListView ListadoBusquedaCambioClasificacion
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
        }

        #endregion

        #region Metodos


        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);

            ListadoItems.Visibility = Visibility.Collapsed;
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {           

            if (e.Key == Key.Enter)
            {
                GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                GetSerial2.Focus();
            }
        }

        private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
            }
        }

        //private void fUpload_OnFileUpload_1(object sender, EventArgs e)
        //{
        //    //Mostrar ventana de Cargando...
        //    ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
        //    //Procesar el Archivo Cargado
        //    if (fUpload.StreamFile != null)
        //    {
        //        string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

        //        ProcessFile1(sender, e, dataFile);
        //    }
        //    //Cierro ventana de Cargando...
        //    pw.Visibility = Visibility.Collapsed;
        //    pw.Close();
        //}

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
                            GetSerial1.Text = dr[dc.ColumnName].ToString();
                            break;
                        case "2":
                            GetSerial2.Text = dr[dc.ColumnName].ToString();
                            break;
                    }
                    NumeroSerial++;
                }

                AddLine(sender, e);
            }

            //fUpload.StreamFile = null;

        }

        private void cb_BuscarItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListadoItems.Visibility = Visibility.Visible;

            ConfirmBasicData(sender, e);
        }

        private void ProcessFile1(object sender, EventArgs e, string dataFile)
        {

            //Linea por linea obtiene el dato del serial y del RR y lo procesa.
            //Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            if (lines == null || lines.Rows.Count == 0)
            {
                Util.ShowError("No hay registros a procesar.\n" + dataFile);
                return;
            }

            CargaMasiva(sender, new DataEventArgs<DataTable>(lines));

            //fUpload.StreamFile = null;

        }

        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void chkRep_Checked_1(object sender, RoutedEventArgs e)
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

        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
        }

        //Recibo
        private void btn_BuscarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }

        private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosRecibo(sender, e);
        }

        private void btn_ConfirmarRecibo_Click_1(object sender, RoutedEventArgs e)
        {           
            ConfirmarRecibo(sender, e);
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
            ListarEquiposSeleccion(sender, e);
        }

        private void btn_imprimirHablador_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresion(sender, e);
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        #endregion

    }

    public interface IDiagnosticoDTVView
    {
        //Clase Modelo
        DiagnosticoDTVModel Model { get; set; }

        #region Variables

        ComboBox GetListBinInicio { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UnidadAlmacenamiento { get; set; }
        ComboBox GetListaEstado { get; set; }    
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        GridView ListadoEquipos { get; set; }
        Button GetButtonConfirmar { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox CodigoEmpaque { get; set; }
        Border BorderDetails { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock Estibas_Seleccionadas { get; set; }

        //Recibo
        TextBox BuscarEstibaRecibo { get; set; }
        //ComboBox BuscarPosicionRecibo { get; set; }
        ListView ListadoBusquedaRecibo { get; set; }

        ListView ListadoBusquedaCambioClasificacion { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> GenerarCodigo;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<EventArgs> ConfirmarImpresion;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

    }
}