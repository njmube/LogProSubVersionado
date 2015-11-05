using Core.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WMComposite.Events;
using WpfFront.Common;
using WpfFront.IQ.Models;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ConsultaSerialesView.xaml
    /// </summary>
    public partial class ConsultaSerialesView : UserControlBase, IConsultaSerialesView
    {
        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargarDatosAdministrador;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ActualizarRegistros;
        public event EventHandler<EventArgs> ReiniciarCapacitacion;
        public event EventHandler<EventArgs> ConsultarMovimientos;
        public event EventHandler<EventArgs> BuscarEquipoTracking;
        public event EventHandler<EventArgs> exportarTracking;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> BuscarNombreMaterial;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

        public ConsultaSerialesView()
        {
            InitializeComponent();
        }

        #region Variables

        public ConsultaSerialesModel Model
        {
            get { return this.DataContext as ConsultaSerialesModel; }
            set { this.DataContext = value; }
        }


        public TextBox GetSerial1
        {
            get { return this.tb_Serial1; }
            set { this.tb_Serial1 = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        //Administrador

        public TextBox Mac
        {
            get { return this.txt_Mac; }
            set { this.txt_Mac = value; }
        }

        public TextBox Centro
        {
            get { return this.txt_Centro; }
            set { this.txt_Centro = value; }
        }

        public TextBox Familia
        {
            get { return this.txt_Familia; }
            set { this.txt_Familia = value; }
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

        public ComboBox NombreMaterial
        {
            get { return this.cb_NombreMaterial; }
            set { this.cb_NombreMaterial = value; }
        }


        public ComboBox Origen
        {
            get { return this.cb_Origen; }
            set { this.cb_Origen = value; }
        }

        public ComboBox TipoOrigen
        {
            get { return this.cb_TipoOrigen; }
            set { this.cb_TipoOrigen = value; }
        }

        public ComboBox TipoRecibo
        {
            get { return this.cb_TipoRecibo; }
            set { this.cb_TipoRecibo = value; }
        }

        public TextBox DocIngreso
        {
            get { return this.txt_DocIngreso; }
            set { this.txt_DocIngreso = value; }
        }

        public TextBox CodigoSAP
        {
            get { return this.txt_CodigoSAP; }
            set { this.txt_CodigoSAP = value; }
        }

        public TextBox Ciudad
        {
            get { return this.txt_Ciudad; }
            set { this.txt_Ciudad = value; }
        }

        public TextBox Consecutivo
        {
            get { return this.txt_Consecutivo; }
            set { this.txt_Consecutivo = value; }
        }

        public ComboBox EstadoRR
        {
            get { return this.cb_EstadoRR; }
            set { this.cb_EstadoRR = value; }
        }

        public System.Windows.Controls.DatePicker FechaIngreso
        {
            get { return this.txt_FechaIng; }
            set { this.txt_FechaIng = value; }
        }

        public System.Windows.Controls.DatePicker FechaDespacho
        {
            get { return this.txt_FechaDespacho; }
            set { this.txt_FechaDespacho = value; }
        }

        public System.Windows.Controls.DatePicker FechaDoc
        {
            get { return this.txt_FechaDoc; }
            set { this.txt_FechaDoc = value; }
        }

        public TextBox TecnicoAsigDiag
        {
            get { return this.txt_TecAsigDiag; }
            set { this.txt_TecAsigDiag = value; }
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

        public TextBox TxtQuery
        {
            get { return this.tb_Query; }
            set { this.tb_Query = value; }
        }

        #endregion

        #region Metodos

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            exportarTracking(sender, e);
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void lv_listEquipos_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConsultarMovimientos(sender, e);
        }

        private void Btn_reiniciar_Click_1(object sender, RoutedEventArgs e)
        {
            string entrada = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la contraseña para reiniciar la prueba");

            if (entrada == "S0p0rt31t")
            {
                ReiniciarCapacitacion(sender, e);
            }
            else
            {
                Util.ShowMessage("Contraseña incorrecta");
            }
        }

        private void tb_mac_keydown_admin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarEquipoTracking(sender, e);
            }
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarDatosAdministrador(sender, e);
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                CargarDatosAdministrador(sender, e);
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

        private void cb_EstadoSerial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void cb_Origen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }

        private void cb_NombreMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuscarNombreMaterial(sender, e);
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

        #endregion
    }
    public interface IConsultaSerialesView
    {
        //Clase Modelo
        ConsultaSerialesModel Model { get; set; }

        #region Variables

        TextBox GetSerial1 { get; set; }
        Border BorderDetails { get; set; }
        ComboBox EstadoSerial { get; set; }
        TextBox IdPallet { get; set; }
        TextBox Ubicacion { get; set; }
        TextBox CodigoEmpaque { get; set; }
        TextBox CodigoEmpaque2 { get; set; }
        ComboBox Posicion { get; set; }
        TextBox Mac { get; set; }
        ComboBox NombreMaterial { get; set; }
        TextBox Familia { get; set; }
        ComboBox TipoOrigen { get; set; }
        TextBox CodigoSAP { get; set; }
        ComboBox TipoRecibo { get; set; }
        TextBox Consecutivo { get; set; }
        ComboBox EstadoRR { get; set; }
        TextBox Centro { get; set; }
        //ComboBox EstadoMaterial { get; set; }
        ComboBox Origen { get; set; }
        TextBox Ciudad { get; set; }
        TextBox DocIngreso { get; set; }
        //TextBox FechaDespacho { get; set; }
        //TextBox FechaDoc { get; set; }
        TextBox TecnicoAsigDiag { get; set; }
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
        //TextBox SmartCardSalida { get; set; }
        ComboBox FallaVerificacion { get; set; }
        TextBox StatusVerificacion { get; set; }
        //TextBox FechaIngreso { get; set; }
        TextBox TxtQuery { get; set; }
        System.Windows.Controls.DatePicker FechaIngreso { get; set; }
        System.Windows.Controls.DatePicker FechaDoc { get; set; }
        System.Windows.Controls.DatePicker FechaDespacho { get; set; }
        

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> CargarDatosAdministrador;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarRegistros;
        event EventHandler<EventArgs> ReiniciarCapacitacion;
        event EventHandler<EventArgs> ConsultarMovimientos;
        event EventHandler<EventArgs> BuscarEquipoTracking;
        event EventHandler<EventArgs> exportarTracking;
        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> BuscarNombreMaterial;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

    }
}
