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
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for AdministradorDTVView.xaml
    /// </summary>
    public partial class AdministradorDTVView : UserControlBase, IAdministradorDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargarDatosAdministradorDTV;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;

        public event EventHandler<EventArgs> ConsultarMovimientos;
        public event EventHandler<EventArgs> BuscarEquipoTracking;
        public event EventHandler<EventArgs> ReiniciarCapacitacion;

        #endregion

        Chart charts;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;
        public AdministradorDTVView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";

            //if (user.Contains("Admin") == true)
            //{
            //    GetReiniciar.Visibility = Visibility.Visible;
            //}

            charts = this.FindName("MyWinformChart") as Chart;
        }

        #region Variables

        public AdministradorDTVModel Model
        {
            get { return this.DataContext as AdministradorDTVModel; }
            set { this.DataContext = value; }
        }

        public ComboBox GetListBinInicio
        {
            get { return this.cb_BinInicio; }
            set { this.cb_BinInicio = value; }
        }

        public TextBox GetSerial1
        {
            get { return this.tb_Serial1; }
            set { this.tb_Serial1 = value; }
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

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        //Administrador

        public TextBox IdReceiver
        {
            get { return this.txt_IDReceiver; }
            set { this.txt_IDReceiver = value; }
        }

        public TextBox SmartCardEntrada
        {
            get { return this.txt_SmartCardEntrada; }
            set { this.txt_SmartCardEntrada = value; }
        }

        public ComboBox EstadoSerial
        {
            get { return this.cb_EstadoSerial; }
            set { this.cb_EstadoSerial = value; }
        }

        public TextBox IdPallet
        {
            get { return this.txt_IdPallet; }
            set { this.txt_IdPallet = value; }
        }

        public TextBox Ubicacion
        {
            get { return this.txt_Ubicacion; }
            set { this.txt_Ubicacion = value; }
        }

        public TextBox CodigoEmpaque
        {
            get { return this.txt_CodigoEmpaque; }
            set { this.txt_CodigoEmpaque = value; }
        }

        public TextBox CodigoEmpaque2
        {
            get { return this.txt_CodigoEmpaque2; }
            set { this.txt_CodigoEmpaque2 = value; }
        }

        public ComboBox Posicion
        {
            get { return this.cb_Posicion; }
            set { this.cb_Posicion = value; }
        }

        public ComboBox Modelo
        {
            get { return this.cb_Modelo; }
            set { this.cb_Modelo = value; }
        }

        public ComboBox EstadoMaterial
        {
            get { return this.cb_EstadoMaterial; }
            set { this.cb_EstadoMaterial = value; }
        }

        public ComboBox Origen
        {
            get { return this.cb_Origen; }
            set { this.cb_Origen = value; }
        }

        //public ComboBox Proveedor
        //{
        //    get { return this.cb_Proveedor; }
        //    set { this.cb_Proveedor = value; }
        //}

        //public ComboBox Dealer
        //{
        //    get { return this.cb_Dealer; }
        //    set { this.cb_Dealer = value; }
        //}

        public TextBox DocIngreso
        {
            get { return this.txt_DocIngreso; }
            set { this.txt_DocIngreso = value; }
        }

        public TextBox Descripcion
        {
            get { return this.txt_Descripcion; }
            set { this.txt_Descripcion = value; }
        }

        public ComboBox Ciudad
        {
            get { return this.cb_Ciudad; }
            set { this.cb_Ciudad = value; }
        }

        public ComboBox TipoDevolucion
        {
            get { return this.cb_TiposDevolucion; }
            set { this.cb_TiposDevolucion = value; }
        }

        public ComboBox DOA
        {
            get { return this.cb_DOA; }
            set { this.cb_DOA = value; }
        }
        
        public TextBox FechaIngreso
        {
            get { return this.txt_FechaIng; }
            set { this.txt_FechaIng = value; }
        }

        public TextBox FechaDespacho
        {
            get { return this.txt_FechaDespacho; }
            set { this.txt_FechaDespacho = value; }
        }

        public TextBox FechaDoc
        {
            get { return this.txt_FechaDoc; }
            set { this.txt_FechaDoc = value; }
        }

        public TextBox TipoDiagnostico
        {
            get { return this.txt_TipoDiagnostico; }
            set { this.txt_TipoDiagnostico = value; }
        }

        public ComboBox FallaDiagnostico
        {
            get { return this.cb_FallaDiag; }
            set { this.cb_FallaDiag = value; }
        }

        public TextBox EstatusDiagnostico
        {
            get { return this.txt_StatusDiag; }
            set { this.txt_StatusDiag = value; }
        }

        public TextBox TecnicoAsignadoRep
        {
            get { return this.txt_TencnicoAsig; }
            set { this.txt_TencnicoAsig = value; }
        }

        public ComboBox EstadoReparacion
        {
            get { return this.cb_EstadoRep; }
            set { this.cb_EstadoRep = value; }
        }

        public ComboBox FallaReparacion
        {
            get { return this.cb_FallaRep; }
            set { this.cb_FallaRep = value; }
        }

        public ComboBox FallaReparacion1
        {
            get { return this.cb_FallaRep1; }
            set { this.cb_FallaRep1 = value; }
        }

        public ComboBox FallaReparacion2
        {
            get { return this.cb_FallaRep2; }
            set { this.cb_FallaRep2 = value; }
        }

        public ComboBox FallaReparacion3
        {
            get { return this.cb_FallaRep3; }
            set { this.cb_FallaRep3 = value; }
        }

        public ComboBox FallaReparacion4
        {
            get { return this.cb_FallaRep4; }
            set { this.cb_FallaRep4 = value; }
        }

        public ComboBox MotivoScrap
        {
            get { return this.cb_MotivoScrap; }
            set { this.cb_MotivoScrap = value; }
        }

        public TextBox SmartCardSalida
        {
            get { return this.txt_SmartCardSalida; }
            set { this.txt_SmartCardSalida = value; }
        }

        public ComboBox FallaVerificacion
        {
            get { return this.cb_FallaVerif; }
            set { this.cb_FallaVerif = value; }
        }

        public TextBox StatusVerificacion
        {
            get { return this.txt_StatusVerif; }
            set { this.txt_StatusVerif = value; }
        }

        public ListView ListadoEquipos_Track
        {
            get { return this.lv_equipos; }
            set { this.lv_equipos = value; }
        }

        public Chart chart1
        {
            get { return charts; }
            set { charts = value; }
        }

        public TextBox GetSerialTrack
        {
            get { return this.tb_serial; }
            set { this.tb_serial = value; }
        }

        public TextBox GetMac_Track
        {
            get { return this.tb_mac; }
            set { this.tb_mac = value; }
        }

        //public Button GetReiniciar
        //{
        //    get { return this.Btn_Reiniciar; }
        //    set { this.Btn_Reiniciar = value; }
        //}

        #endregion

        #region Metodos

        private void lv_listEquipos_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConsultarMovimientos(sender, e);
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarDatosAdministradorDTV(sender, e);
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                CargarDatosAdministradorDTV(sender, e);
            }
        }

        private void btn_ActualizarCambioClasificacion_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistros(sender, e);
        }

        private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
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
                            //GetSerial2.Text = dr[dc.ColumnName].ToString();
                            break;
                    }
                    NumeroSerial++;
                }

                AddLine(sender, e);
            }

            //fUpload.StreamFile = null;

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

        private void cb_EstadoSerial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }
        private void cb_Modelo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }
        private void cb_FallaDiag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void cb_FallaVerif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddLine(sender, e);
        }

        private void cb_estadoRep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)EstadoReparacion.SelectedItem).Content.ToString() != "SCRAP")
            {
                MotivoScrap.SelectedIndex = -1;
                MotivoScrap.IsEnabled = false;               
            }
            else
            {
                MotivoScrap.IsEnabled = true;
            }
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

        private void tb_Serial_keydown_admin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_mac.Focus();
            }
        }

        private void tb_mac_keydown_admin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarEquipoTracking(sender, e);
            }
        }

        private void Btn_Reiniciar_Click_1(object sender, RoutedEventArgs e)
        {
            ReiniciarCapacitacion(sender, e);
        }

        #endregion

    }

    public interface IAdministradorDTVView
    {
        //Clase Modelo
        AdministradorDTVModel Model { get; set; }

        #region Variables

        ComboBox GetListBinInicio { get; set; }
        //ComboBox Ubicacion { get; set; }
        //ComboBox UnidadAlmacenamiento { get; set; }
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        //GridView ListadoEquipos { get; set; }
        Button GetButtonConfirmar { get; set; }
        TextBox GetSerial1 { get; set; }
        //TextBox GetSerial2 { get; set; }
        //TextBox CodigoEmpaque { get; set; }
        Border BorderDetails { get; set; }
        //ListView ListadoItems { get; set; }

        //AdministradorDTV

        TextBox IdReceiver { get; set; }
        TextBox SmartCardEntrada { get; set; }
        ComboBox EstadoSerial { get; set; }
        TextBox IdPallet { get; set; }
        TextBox Ubicacion { get; set; }
        TextBox CodigoEmpaque { get; set; }
        TextBox CodigoEmpaque2 { get; set; }
        ComboBox Posicion { get; set; }
        ComboBox Modelo { get; set; }
        ComboBox EstadoMaterial { get; set; }
        ComboBox Origen { get; set; }
        ComboBox Ciudad { get; set; }
        ComboBox TipoDevolucion { get; set; }
        TextBox Descripcion { get; set; }
        TextBox DocIngreso { get; set; }
        TextBox FechaDespacho { get; set; }
        //ComboBox EmpresaTransp { get; set; }
        //ComboBox CausaIngresoEquipo { get; set; }
        ComboBox DOA { get; set; }        
        TextBox FechaDoc { get; set; }
        TextBox TipoDiagnostico { get; set; }
        ComboBox FallaDiagnostico { get; set; }
        TextBox EstatusDiagnostico { get; set; }
        TextBox TecnicoAsignadoRep { get; set; }
        ComboBox EstadoReparacion { get; set; }
        ComboBox FallaReparacion { get; set; }
        ComboBox FallaReparacion1 { get; set; }
        ComboBox FallaReparacion2 { get; set; }
        ComboBox FallaReparacion3 { get; set; }
        ComboBox FallaReparacion4 { get; set; }
        ComboBox MotivoScrap { get; set; }
        TextBox SmartCardSalida { get; set; }
        ComboBox FallaVerificacion { get; set; }
        TextBox StatusVerificacion { get; set; }
        TextBox FechaIngreso { get; set; }

        ListView ListadoEquipos_Track { get; set; }
        Chart chart1 { get; set; }
        TextBox GetMac_Track { get; set; }
        TextBox GetSerialTrack { get; set; }

        //Button GetReiniciar { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> CargarDatosAdministradorDTV;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;

        event EventHandler<EventArgs> ConsultarMovimientos;
        event EventHandler<EventArgs> BuscarEquipoTracking;
        event EventHandler<EventArgs> ReiniciarCapacitacion;

        #endregion

    }

    
}