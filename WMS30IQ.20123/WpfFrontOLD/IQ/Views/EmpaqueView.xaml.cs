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
    /// Interaction logic for EmpaqueView.xaml
    /// </summary>
    public partial class EmpaqueView : UserControlBase, IEmpaqueView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> GenerarCodigo;
        public event EventHandler<EventArgs> GenerarCodigoEmpaque;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ImprimirEtiqueta;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ConfirmarMovimientoProcesamiento;
        public event EventHandler<EventArgs> ConfirmarImpresion;
        public event EventHandler<EventArgs> ConfirmarImpresionHablador;
        public event EventHandler<EventArgs> SeleccionPallet_Consulta;
        public event EventHandler<EventArgs> SeleccionCaja_Consulta;
        public event EventHandler<EventArgs> GenerarPallet;
        public event EventHandler<KeyEventArgs> KeyConsultarPallet;
        public event EventHandler<KeyEventArgs> EnterConsultarPallet;
        public event EventHandler<EventArgs> CrearNuevaCaja;
        public event EventHandler<EventArgs> CrearNuevoPallet;
        public event EventHandler<EventArgs> CerrarPallet;
        public event EventHandler<EventArgs> CerrarCaja;
        public event EventHandler<EventArgs> EliminarCaja;
        public event EventHandler<EventArgs> AbrirCaja;
        public event EventHandler<EventArgs> AbrirPallet;
        public event EventHandler<EventArgs> EliminarPallet;
        public event EventHandler<EventArgs> DesempacarEquipos;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

        public EmpaqueView()
        {
            InitializeComponent();
            this.tb_CodPallet.IsEnabled = false;
            this.NuevaCaja.IsEnabled = false;
            this.StackCajas.IsEnabled = false;
            this.StackSeriales.IsEnabled = false;
        }

        #region Variables

        public EmpaqueModel Model
        {
            get { return this.DataContext as EmpaqueModel; }
            set { this.DataContext = value; }
        }


        public ComboBox Ubicacion
        {
            get { return this.cb_Ubicacion; }
            set { this.cb_Ubicacion = value; }
        }

        public ListView ListadoItems
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
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

        public ListView ListadoPalletsBusqueda
        {
            get { return this.lvPalletsEmpaque; }
            set { this.lvPalletsEmpaque = value; }
        }

        public ListView ListadoCajasBusqueda
        {
            get { return this.lvCajasEmpaque; }
            set { this.lvCajasEmpaque = value; }
        }

        public ListView ListadoSerialesBusqueda
        {
            get { return this.lvSerialesEmpaque; }
            set { this.lvSerialesEmpaque = value; }
        }

        public TextBox GetCodPallet
        {
            get { return this.tb_CodPallet; }
            set { this.tb_CodPallet = value; }
        }

        public TextBox GetCodPalletBusqueda
        {
            get { return this.tb_CodPalletBusqueda; }
            set { this.tb_CodPalletBusqueda = value; }
        }

        public TextBox NuevaCaja
        {
            get { return this.tb_NuevaCaja; }
            set { this.tb_NuevaCaja = value; }
        }

        public Button btnNuevaCaja
        {
            get { return this.btn_NuevaCaja; }
            set { this.btn_NuevaCaja = value; }
        }

        public Button btnCerrarCaja
        {
            get { return this.btn_CerrarCaja; }
            set { this.btn_CerrarCaja = value; }
        }

        public Button btnCerrarPallet
        {
            get { return this.btn_CerrarPallet; }
            set { this.btn_CerrarPallet = value; }
        }

        public Button btnEliminarPallet
        {
            get { return this.btn_EliminarPallet; }
            set { this.btn_EliminarPallet = value; }
        }

        public Button btnAbrirPallet
        {
            get { return this.btn_AbrirPallet; }
            set { this.btn_AbrirPallet = value; }
        }

        public Button btnEmpacar
        {
            get { return this.btn_confirm_2; }
            set { this.btn_confirm_2 = value; }
        }

        public Button btnDesempacar
        {
            get { return this.btn_Desempacar; }
            set { this.btn_Desempacar = value; }
        }

        public Button btnEliminarCaja
        {
            get { return this.btn_EliminarCaja; }
            set { this.btn_EliminarCaja = value; }
        }

        public Button btnAbrirCaja
        {
            get { return this.btn_AbrirCaja; }
            set { this.btn_AbrirCaja = value; }
        }

        public StackPanel StackCajas
        {
            get { return this.Stack_Panel12; }
            set { this.Stack_Panel12 = value; }
        }

        public StackPanel StackSeriales
        {
            get { return this.Stack_Panel13; }
            set { this.Stack_Panel13 = value; }
        }

        #endregion

        #region Metodos


        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void btn_Empacar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimientoProcesamiento(sender, e);
        }

        private void btn_Desempacar_Click_1(object sender, RoutedEventArgs e)
        {
            DesempacarEquipos(sender, e);
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Tab
            if (e.Key == Key.Tab)
            {
                GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                //Paso el focus al siguiente campo de serial
                GetSerial2.Focus();
            }

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

        private void ImageGenerateEntrega_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigoEmpaque(sender, e);
        }

        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                Console.WriteLine("enter ok");
                AddLine(sender, e);
            }

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


        private void btn_imprimirHablador_Click_2(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresionHablador(sender, e);
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

        private void chkRep_Checked_1(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Replicando registros...por favor espere...");
            //Replica la informacion de la primera linea en las demas.
            ReplicateDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            //chkRep.IsChecked = false;
        }

        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
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

        private void btn_PrintSticker_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirEtiqueta(sender, e);
        }

        private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosRecibo(sender, e);
        }

        private void btn_ConfirmarRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void tb_UbicacionPallet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lv_ListadoCajas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            SeleccionCaja_Consulta(sender, e);
        }

        private void lv_ListadoPallets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeleccionPallet_Consulta(sender, e);
        }

        private void ImgGenerate_estiba(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarPallet(sender, e);
        }

        private void KeyUp_BuscarPallet(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //Util.ShowMessage("You Entered: " + tb_CodPalletB.Text);
                EnterConsultarPallet(sender, e);
            }
            else
            {
                KeyConsultarPallet(sender, e);
            }
        }

        private void btn_NuevaCaja_Click_1(object sender, RoutedEventArgs e)
        {
            CrearNuevaCaja(sender, e);


        }

        private void btn_NuevoPallet_Click_1(object sender, RoutedEventArgs e)
        {
            CrearNuevoPallet(sender, e);
        }

        private void btn_CerrarPallet_Click_1(object sender, RoutedEventArgs e)
        {
            //cierro el pallet
            CerrarPallet(sender, e);

            //imprimo el hablador
            //ConfirmarImpresionHablador(sender, e);
        }

        private void btn_CerrarCaja_Click_1(object sender, RoutedEventArgs e)
        {
            //cierro la caja
            CerrarCaja(sender, e);

            //imprimo la etiqueta
            //ConfirmarImpresion(sender, e);
        }

        private void btn_EliminarCaja_Click_1(object sender, RoutedEventArgs e)
        {
            //eliminar la caja
            EliminarCaja(sender, e);
        }

        private void btn_AbrirCaja_Click_1(object sender, RoutedEventArgs e)
        {
            //Abrir la caja
            AbrirCaja(sender, e);
        }

        private void btn_AbrirPallet_Click_1(object sender, RoutedEventArgs e)
        {
            AbrirPallet(sender, e);
        }

        private void btn_EliminarPallet_Click_1(object sender, RoutedEventArgs e)
        {
            EliminarPallet(sender, e);
        }

        #endregion


    }

    public interface IEmpaqueView
    {
        //Clase Modelo
        EmpaqueModel Model { get; set; }

        #region Variables

        ComboBox Ubicacion { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoPalletsBusqueda { get; set; }
        ListView ListadoCajasBusqueda { get; set; }
        ListView ListadoSerialesBusqueda { get; set; }
        TextBox GetCodPallet { get; set; }
        TextBox GetCodPalletBusqueda { get; set; }
        TextBox NuevaCaja { get; set; }
        Button btnNuevaCaja { get; set; }
        Button btnCerrarCaja { get; set; }
        Button btnEmpacar { get; set; }
        Button btnDesempacar { get; set; }
        Button btnEliminarCaja { get; set; }
        Button btnAbrirCaja { get; set; }
        Button btnCerrarPallet { get; set; }
        Button btnAbrirPallet { get; set; }
        Button btnEliminarPallet { get; set; }
        StackPanel StackCajas { get; set; }
        StackPanel StackSeriales { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> GenerarCodigo;
        event EventHandler<EventArgs> GenerarCodigoEmpaque;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ImprimirEtiqueta;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ConfirmarMovimientoProcesamiento;
        event EventHandler<EventArgs> SeleccionPallet_Consulta;
        event EventHandler<EventArgs> SeleccionCaja_Consulta;
        event EventHandler<EventArgs> GenerarPallet;
        event EventHandler<KeyEventArgs> KeyConsultarPallet;
        event EventHandler<KeyEventArgs> EnterConsultarPallet;
        event EventHandler<EventArgs> CrearNuevaCaja;
        event EventHandler<EventArgs> CrearNuevoPallet;
        event EventHandler<EventArgs> CerrarPallet;
        event EventHandler<EventArgs> CerrarCaja;
        event EventHandler<EventArgs> EliminarCaja;
        event EventHandler<EventArgs> AbrirCaja;
        event EventHandler<EventArgs> AbrirPallet;
        event EventHandler<EventArgs> EliminarPallet;
        event EventHandler<EventArgs> DesempacarEquipos;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<EventArgs> ConfirmarImpresion;
        event EventHandler<EventArgs> ConfirmarImpresionHablador;

        #endregion

    }
}