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
    /// Interaction logic for MoverMercanciaDTVView.xaml
    /// </summary>
    public partial class MoverMercanciaDTVView : UserControlBase, IMoverMercanciaDTVView
    {

        #region Eventos

        #region Cambio Ubicaciones

        public event EventHandler<EventArgs> BuscarRegistrosCambioUbicaciones;
        public event EventHandler<EventArgs> ActualizarRegistrosCambioUbicaciones;
        public event EventHandler<EventArgs> HabilitarCambioUbicacion;
        public event EventHandler<EventArgs> GuardarNuevaUbicacion;
        public event EventHandler<EventArgs> GuardarNuevoEstado;

        #endregion

        #region Cambio Clasificacion

        public event EventHandler<EventArgs> BuscarRegistrosCambioClasificacion;
        public event EventHandler<EventArgs> ActualizarRegistrosCambioClasificacion;
        public event EventHandler<EventArgs> HabilitarCambioClasificacion;
        public event EventHandler<EventArgs> GuardarNuevaClasificacion;
        public event EventHandler<EventArgs> ImprimirRegistros;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<EventArgs> ExportPalletSeleccion;
        public event EventHandler<EventArgs> ExportSerialesSeleccion;

        #endregion

        #endregion

        public MoverMercanciaDTVView()
        {
            InitializeComponent();
        }

        #region Variables

        public MoverMercanciaDTVModel Model
        {
            get { return this.DataContext as MoverMercanciaDTVModel; }
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

        public System.Windows.Controls.DatePicker GetFechaIngresoCambioUbicacion 
        {
            get { return this.cb_BuscarFechaIngresoCambioUbicacion; }
            set { this.cb_BuscarFechaIngresoCambioUbicacion = value; }
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

        public ComboBox NuevaUbicacion
        {
            get { return this.cb_NuevaUbicacion; }
            set { this.cb_NuevaUbicacion = value; }
        }

        public TextBlock TextoEstadoActual
        {
            get { return this.txt_EstadoActual; }
            set { this.txt_EstadoActual = value; }
        }

        public ComboBox NuevoEstado
        {
            get { return this.cb_NuevoEstado; }
            set { this.cb_NuevoEstado = value; }
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

        public ComboBox BuscarModeloCambioClasificacion
        {
            get { return this.cb_BuscarModeloCambioClasificacion; }
            set { this.cb_BuscarModeloCambioClasificacion = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaIngresoCambioClasificacion
        {
            get { return this.cb_BuscarFechaIngresoCambioClasificacion; }
            set { this.cb_BuscarFechaIngresoCambioClasificacion = value; }
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

        public TextBlock TotalSeriales
        {
            get { return this.textblock_totalSeriales1; }
            set { this.textblock_totalSeriales1 = value; }
        }

        public TextBlock estibasSeleccionadas
        {
            get { return this.textblock_totalEstibas1; }
            set { this.textblock_totalEstibas1 = value; }
        }

        #endregion

        #endregion

        #region Metodos

        #region Cambio Ubicaciones

        private void btn_BuscarCambioUbicacion_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosCambioUbicaciones(sender, e);
        }

        private void lv_ListadoBusquedaCambioUbicacion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            HabilitarCambioUbicacion(sender, e);
            //FilaSeleccionada(sender, e);
        }

        private void btn_ConfirmarCambioUbicacion_Click_1(object sender, RoutedEventArgs e)
        {
            GuardarNuevaUbicacion(sender, e);
        }

        private void btn_ConfirmarCambioEstado_Click_1(object sender, RoutedEventArgs e)
        {
            GuardarNuevoEstado(sender, e);
        }

        #endregion

        #region Cambio Clasificacion

        private void btn_BuscarCambioClasificacion_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosCambioClasificacion(sender, e);
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

        private void Btn_ExportarPalletsSeleccion_Click_1(object sender, RoutedEventArgs e)
        {
            ExportPalletSeleccion(sender, e);
        }

        private void Btn_ExportarSerialesSeleccion_Click_1(object sender, RoutedEventArgs e)
        {
            ExportSerialesSeleccion(sender, e);
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }

        #endregion

        #endregion

    }

    public interface IMoverMercanciaDTVView
    {
        //Clase Modelo
        MoverMercanciaDTVModel Model { get; set; }

        #region Variables

        #region Cambio Ubicaciones

        TextBox BuscarEstibaCambioUbicacion { get; set; }
        ComboBox BuscarPosicionCambioUbicacion { get; set; }
        ComboBox BuscarModeloCambioUbicacion { get; set; }
        System.Windows.Controls.DatePicker GetFechaIngresoCambioUbicacion { get; set; }
        ListView ListadoBusquedaCambioUbicacion { get; set; }
        StackPanel StackCambioUbicacion { get; set; }
        TextBlock TextoUbicacionActual { get; set; }
        ComboBox NuevaUbicacion { get; set; }

        TextBlock TextoEstadoActual { get; set; }
        ComboBox NuevoEstado { get; set; }
        #endregion

        #region Cambio Clasificacion

        TextBox BuscarEstibaCambioClasificacion { get; set; }
        ComboBox BuscarPosicionCambioClasificacion { get; set; }
        ComboBox BuscarModeloCambioClasificacion { get; set; }
        System.Windows.Controls.DatePicker GetFechaIngresoCambioClasificacion { get; set; }
        ListView ListadoBusquedaCambioClasificacion { get; set; }
        ListView ListadoSerialesCambioClasificacion { get; set; }
        StackPanel StackCambioClasificacion { get; set; }
        TextBlock TextoClasificacionActual { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock estibasSeleccionadas { get; set; }
        ComboBox NuevaClasificacion { get; set; }
        GridView GridViewListaClasificacion { get; set; }
        GridView GridViewListaSerialesClasificacion { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Cambio Ubicaciones

        event EventHandler<EventArgs> BuscarRegistrosCambioUbicaciones;
        event EventHandler<EventArgs> ActualizarRegistrosCambioUbicaciones;
        event EventHandler<EventArgs> HabilitarCambioUbicacion;
        event EventHandler<EventArgs> GuardarNuevaUbicacion;
        event EventHandler<EventArgs> GuardarNuevoEstado;

        #endregion

        #region Cambio Clasificacion

        event EventHandler<EventArgs> BuscarRegistrosCambioClasificacion;
        event EventHandler<EventArgs> ActualizarRegistrosCambioClasificacion;
        event EventHandler<EventArgs> HabilitarCambioClasificacion;
        event EventHandler<EventArgs> GuardarNuevaClasificacion;
        event EventHandler<EventArgs> ImprimirRegistros;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<EventArgs> ExportPalletSeleccion;
        event EventHandler<EventArgs> ExportSerialesSeleccion;
        #endregion

        #endregion

    }
}