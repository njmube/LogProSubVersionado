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
    public partial class PreDiagnosticoDTVView : UserControlBase, IPreDiagnosticoDTVView
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

        #endregion

        public PreDiagnosticoDTVView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";
        }

        #region Variables

        public PreDiagnosticoDTVModel Model
        {
            get { return this.DataContext as PreDiagnosticoDTVModel; }
            set { this.DataContext = value; }
        }

        public ComboBox GetListBinInicio
        {
            get { return this.cb_BinInicio; }
            set { this.cb_BinInicio = value; }
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

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
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

        private void btn_imprimirHablador_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresion(sender, e);
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        private void ImageGenerate_MouseDownPreDiag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }


        #endregion

    }

    public interface IPreDiagnosticoDTVView
    {
        //Clase Modelo
        PreDiagnosticoDTVModel Model { get; set; }

        #region Variables

        ComboBox GetListBinInicio { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UnidadAlmacenamiento { get; set; }
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

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> GenerarCodigo;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;

        event EventHandler<EventArgs> ConfirmarImpresion;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;

        #endregion

    }
}