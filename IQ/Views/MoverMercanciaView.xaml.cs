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
using System.Collections.ObjectModel;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for MoverMercanciaView.xaml
    /// </summary>
    public partial class MoverMercanciaView : UserControlBase, IMoverMercanciaView
    {

        #region Eventos
        private ObservableCollection<ProcessInfo> _processes = new ObservableCollection<ProcessInfo>();

        #region Cambio Ubicaciones

        public event EventHandler<EventArgs> BuscarRegistrosCambioUbicaciones;
        public event EventHandler<EventArgs> ActualizarRegistrosCambioUbicaciones;
        public event EventHandler<EventArgs> HabilitarCambioUbicacion;
        public event EventHandler<EventArgs> GuardarNuevaUbicacion;

        #endregion

        #region Cambio Clasificacion

        public event EventHandler<EventArgs> BuscarRegistrosCambioClasificacion;
        public event EventHandler<EventArgs> ActualizarRegistrosCambioClasificacion;
        public event EventHandler<EventArgs> HabilitarCambioClasificacion;
        public event EventHandler<EventArgs> GuardarNuevaClasificacion;
        public event EventHandler<EventArgs> ImprimirRegistros;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionadaRecibo;
        public event EventHandler<EventArgs> ExportPalletSeleccion;
        public event EventHandler<EventArgs> ExportSerialesSeleccion;

        #endregion

        #region Recibo

        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> Imprimir_Hablador;
        //public event EventHandler<SelectionChangedEventArgs> FilaSeleccionadaRecibo;

        #endregion

        #endregion

        public MoverMercanciaView()
        {
            InitializeComponent();
            Processes.Add(new ProcessInfo
            {
                CpuUsage = 10.3,
                MemUsage = 48.9,
                Processes = new ObservableCollection<Processleo>()
            });
            var pro = new Processleo { Name = "Process1", Processes = new ObservableCollection<Processleo>() };
            pro.Processes.Add(new Processleo { Name = "SubProcess1", Processes = new ObservableCollection<Processleo>() });
            Processes[0].Processes.Add(pro);
            Processes.Add(new ProcessInfo
            {
                CpuUsage = 0,
                MemUsage = 100,
                Processes = new ObservableCollection<Processleo>()
            });
            var pro2 = new Processleo { Name = "Process2", Processes = new ObservableCollection<Processleo>() };
            pro2.Processes.Add(new Processleo { Name = "SubProcess1", Processes = new ObservableCollection<Processleo>() });
            pro2.Processes.Add(new Processleo { Name = "SubProcess2", Processes = new ObservableCollection<Processleo>() });
            pro2.Processes.Add(new Processleo { Name = "SubProcess3", Processes = new ObservableCollection<Processleo>() });
            Processes[1].Processes.Add(pro2);
        }
        public ObservableCollection<ProcessInfo> Processes
        {
            get { return _processes; }
            set { _processes = value; }
        }

        #region Variables

        public MoverMercanciaModel Model
        {
            get { return this.DataContext as MoverMercanciaModel; }
            set { this.DataContext = value; }
        }

        #region Cambio Ubicaciones

        public TextBox BuscarEstibaCambioUbicacion
        {
            get { return this.tb_BuscarEstibaCambioUbicacion; }
            set { this.tb_BuscarEstibaCambioUbicacion = value; }
        }

        public ComboBox BuscarPosicionCambioUbicacion
        {
            get { return this.cb_BuscarPosicionCambioUbicacion; }
            set { this.cb_BuscarPosicionCambioUbicacion = value; }
        }

        public ComboBox BuscarModeloCambioUbicacion
        {
            get { return this.cb_BuscarModeloCambioUbicacion; }
            set { this.cb_BuscarModeloCambioUbicacion = value; }
        }

        public ComboBox BuscarModeloCambioClasificacion
        {
            get { return this.cb_BuscarModeloCambioClasificacion; }
            set { this.cb_BuscarModeloCambioClasificacion = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaIngresoCambioUbicacion
        {
            get { return this.cb_BuscarFechaIngresoCambioUbicacion; }
            set { this.cb_BuscarFechaIngresoCambioUbicacion = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaIngresoCambioClasificacion
        {
            get { return this.cb_BuscarFechaIngresoCambioClasificacion; }
            set { this.cb_BuscarFechaIngresoCambioClasificacion = value; }
        }

        public ListView ListadoBusquedaCambioUbicacion
        {
            get { return this.lv_ListadoBusquedaCambioUbicacion; }
            set { this.lv_ListadoBusquedaCambioUbicacion = value; }
        }

        public StackPanel StackCambioUbicacion
        {
            get { return this.Stack_CambioUbicacion; }
            set { this.Stack_CambioUbicacion = value; }
        }

        public TextBlock TextoUbicacionActual
        {
            get { return this.txt_UbicacionActual; }
            set { this.txt_UbicacionActual = value; }
        }

        public GridView GridViewListaClasificacion
        {
            get { return this.GridViewDetails_1; }
            set { this.GridViewDetails_1 = value; }
        }

        public GridView GridViewListaSerialesClasificacion
        {
            get { return this.GridViewDetails11; }
            set { this.GridViewDetails11 = value; }
        }

        public ComboBox NuevaUbicacion
        {
            get { return this.cb_NuevaUbicacion; }
            set { this.cb_NuevaUbicacion = value; }
        }

        public TextBlock TotalSeriales
        {
            get { return this.textblock_totalSeriales11s; }
            set { this.textblock_totalSeriales11s = value; }
        }

        public TextBlock Estibas_Seleccionadas
        {
            get { return this.textblock_totalEstibas11s; }
            set { this.textblock_totalEstibas11s = value; }
        }

        #endregion

        #region Cambio Clasificacion

        public TextBox BuscarEstibaCambioClasificacion
        {
            get { return this.tb_BuscarEstibaCambioClasificacion; }
            set { this.tb_BuscarEstibaCambioClasificacion = value; }
        }

        public ComboBox BuscarPosicionCambioClasificacion
        {
            get { return this.cb_BuscarPosicionCambioClasificacion; }
            set { this.cb_BuscarPosicionCambioClasificacion = value; }
        }

        public ComboBox BuscarProductoCambioClasificacion
        {
            get { return this.cb_BuscarProducto; }
            set { this.cb_BuscarProducto = value; }
        }

        public ComboBox Producto
        {
            get { return this.Producto; }
            set { this.Producto = value; }
        }

        public ListView ListadoBusquedaCambioClasificacion
        {
            get { return this.lv_ListadoBusquedaCambioClasificacion; }
            set { this.lv_ListadoBusquedaCambioClasificacion = value; }
        }

        public ListView ListadoSerialesCambioClasificacion
        {
            get { return this.lv_PalletSeriales; }
            set { this.lv_PalletSeriales = value; }
        }

        public StackPanel StackCambioClasificacion
        {
            get { return this.Stack_CambioClasificacion; }
            set { this.Stack_CambioClasificacion = value; }
        }

        public TextBlock TextoClasificacionActual
        {
            get { return this.txt_ClasificacionActual; }
            set { this.txt_ClasificacionActual = value; }
        }

        public ComboBox NuevaClasificacion
        {
            get { return this.cb_NuevaClasificacion; }
            set { this.cb_NuevaClasificacion = value; }
        }

        #endregion

        #region Recibo

        public TextBox BuscarEstibaRecibo
        {
            get { return this.tb_BuscarEstibaRecibo; }
            set { this.tb_BuscarEstibaRecibo = value; }
        }

        public ListView ListadoBusquedaRecibo
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
        }

        public ComboBox Ubicacion
        {
            get { return this.cb_Ubicacion; }
            set { this.cb_Ubicacion = value; }
        }

        public ComboBox UbicacionDesp
        {
            get { return this.cb_UbicacionDesp; }
            set { this.cb_UbicacionDesp = value; }
        }

        #endregion

        #endregion

        #region Metodos

        #region Cambio Ubicaciones

        private void btn_BuscarCambioUbicacion_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosCambioUbicaciones(sender, e);
        }

        private void btn_ActualizarCambioUbicacion_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosCambioUbicaciones(sender, e);
        }

        private void lv_ListadoBusquedaCambioUbicacion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            HabilitarCambioUbicacion(sender, e);
            FilaSeleccionada(sender, e);
        }

        private void btn_ConfirmarCambioUbicacion_Click_1(object sender, RoutedEventArgs e)
        {
            GuardarNuevaUbicacion(sender, e);
        }

        #endregion

        #region Cambio Clasificacion

        private void btn_BuscarCambioClasificacion_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosCambioClasificacion(sender, e);
        }

        private void Btn_ExportarPalletsSeleccion_Click_1(object sender, RoutedEventArgs e)
        {
            ExportPalletSeleccion(sender, e);
        }

        private void Btn_ExportarSerialesSeleccion_Click_1(object sender, RoutedEventArgs e)
        {
            ExportSerialesSeleccion(sender, e);
        }

        private void btn_ActualizarCambioClasificacion_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosCambioClasificacion(sender, e);
        }

        private void lv_ListadoBusquedaCambioClasificacion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            HabilitarCambioClasificacion(sender, e);
        }

        private void btn_ConfirmarCambioClasificacion_Click_1(object sender, RoutedEventArgs e)
        {
            GuardarNuevaClasificacion(sender, e);
        }

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirRegistros(sender, e);
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }

        #endregion

        #region Recibo

        private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosRecibo(sender, e);
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void btn_BuscarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }

        private void MySelectionChanged_Recibo(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionadaRecibo(sender, e);
        }

        private void btn_imprimir_Click_11(object sender, RoutedEventArgs e)
        {
            Imprimir_Hablador(sender, e);
        }

        #endregion

        #endregion

    }
    public class ProcessInfo
    {
        public ObservableCollection<Processleo> Processes { get; set; }
        public double CpuUsage { get; set; }
        public double MemUsage { get; set; }
    }

    public class Processleo
    {
        public string Name { get; set; }
        public ObservableCollection<Processleo> Processes { get; set; }
    }
    public interface IMoverMercanciaView
    {
        //Clase Modelo
        MoverMercanciaModel Model { get; set; }

        #region Variables

        #region Cambio Ubicaciones

        TextBox BuscarEstibaCambioUbicacion { get; set; }
        ComboBox BuscarPosicionCambioUbicacion { get; set; }
        ComboBox BuscarModeloCambioUbicacion { get; set; }
        ListView ListadoBusquedaCambioUbicacion { get; set; }
        StackPanel StackCambioUbicacion { get; set; }
        TextBlock TextoUbicacionActual { get; set; }
        ComboBox NuevaUbicacion { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock Estibas_Seleccionadas { get; set; }
        System.Windows.Controls.DatePicker GetFechaIngresoCambioUbicacion { get; set; }
        #endregion

        #region Cambio Clasificacion

        TextBox BuscarEstibaCambioClasificacion { get; set; }
        ComboBox BuscarPosicionCambioClasificacion { get; set; }
        ComboBox BuscarProductoCambioClasificacion { get; set; }
        ComboBox BuscarModeloCambioClasificacion { get; set; }
        ComboBox Producto { get; set; }
        ListView ListadoBusquedaCambioClasificacion { get; set; }
        ListView ListadoSerialesCambioClasificacion { get; set; }
        StackPanel StackCambioClasificacion { get; set; }
        TextBlock TextoClasificacionActual { get; set; }
        ComboBox NuevaClasificacion { get; set; }
        System.Windows.Controls.DatePicker GetFechaIngresoCambioClasificacion { get; set; }
        GridView GridViewListaClasificacion { get; set; }
        GridView GridViewListaSerialesClasificacion { get; set; }

        #endregion

        #region Recibo

        TextBox BuscarEstibaRecibo { get; set; }
        ListView ListadoBusquedaRecibo { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UbicacionDesp { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Cambio Ubicaciones

        event EventHandler<EventArgs> BuscarRegistrosCambioUbicaciones;
        event EventHandler<EventArgs> ActualizarRegistrosCambioUbicaciones;
        event EventHandler<EventArgs> HabilitarCambioUbicacion;
        event EventHandler<EventArgs> GuardarNuevaUbicacion;

        #endregion

        #region Cambio Clasificacion

        event EventHandler<EventArgs> BuscarRegistrosCambioClasificacion;
        event EventHandler<EventArgs> ActualizarRegistrosCambioClasificacion;
        event EventHandler<EventArgs> HabilitarCambioClasificacion;
        event EventHandler<EventArgs> GuardarNuevaClasificacion;
        event EventHandler<EventArgs> ImprimirRegistros;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionadaRecibo;
        event EventHandler<EventArgs> ExportPalletSeleccion;
        event EventHandler<EventArgs> ExportSerialesSeleccion;
        #endregion

        #region Recibo

        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> Imprimir_Hablador;
        //event EventHandler<SelectionChangedEventArgs> FilaSeleccionadaRecibo;

        #endregion

        #endregion

    }
}