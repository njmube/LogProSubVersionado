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
using System.Text.RegularExpressions;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ReparacionesView.xaml
    /// </summary>
    public partial class ReparacionesView : UserControlBase, IReparacionesView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> ConfirmarImpresion;
        public event EventHandler<EventArgs> GenerarCodigo;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargarDatosReparacion;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<SelectionChangedEventArgs> FiltrarDatosEntrega;
        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        //Asignacion
        public event EventHandler<EventArgs> BuscarRegistrosAsignacion;
        public event EventHandler<EventArgs> ActualizarRegistrosAsignacion;
        public event EventHandler<EventArgs> ListarEquiposEstibas;
        public event EventHandler<EventArgs> MostrarTecnicosEstibas;
        public event EventHandler<EventArgs> ConfirmarTecnicoEquipo;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<SelectionChangedEventArgs> HabilitarUbicacion;
        public event EventHandler<EventArgs> ConsultaReparacionAnterior;
        public event EventHandler<EventArgs> DeleteDetails;
        public event EventHandler<EventArgs> FiltraPorTecnico;
        public event EventHandler<EventArgs> AddToList;
        public event EventHandler<EventArgs> RemoveSelection;
        public event EventHandler<EventArgs> HabilitarMotivo;
        public event EventHandler<EventArgs> CargarHistorico;
        public event EventHandler<MouseButtonEventArgs> BuscarEquiposPorTecnico;
        public event EventHandler<EventArgs> BuscarEquiposPorTecnicoEntrega;
        public event EventHandler<EventArgs> ConsultarTecnicos;

        #endregion

        public ReparacionesView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";
            txt_showItemsByUser.Text = "Mostrar equipos por técnico >>";
        }

        #region Variables


        public TabItem GetTabEntrega
        {
            get { return this.TabEntrega; }
            set { this.TabEntrega = value; }
        }

        public ComboBox NuevaUbicacion
        {
            get { return this.cb_NuevaUbicacion; }
            set { this.cb_NuevaUbicacion = value; }
        }

        public StackPanel StackUbicacion
        {
            get { return this.stack_nuevaUbicacion; }
            set { this.stack_nuevaUbicacion = value; }
        }

        public Button BTN_BuscarEquipos
        {
            get { return this.btn_BuscarEquipos; }
            set { this.btn_BuscarEquipos = value; }
        }

        public TextBlock TXT_filterResults
        {
            get { return this.txt_filterResults; }
            set {this.txt_filterResults = value;}
        }

        public TextBlock txt_User
        {
            get { return this.txt_currentUser; }
            set { this.txt_currentUser = value; }
        }

        public ReparacionesModel Model
        {
            get { return this.DataContext as ReparacionesModel; }
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

        public ComboBox FallaReparacion
        {
            get { return this.cb_FallaRep; }
            set { this.cb_FallaRep = value; }
        }

        public ComboBox FallaReparacionAdic
        {
            get { return this.cb_FallaAdic; }
            set { this.cb_FallaAdic = value; }
        }

        public ComboBox FallaReparacionAdic2
        {
            get { return this.cb_FallaAdic2; }
            set { this.cb_FallaAdic2 = value; }
        }

        public ComboBox FallaReparacionAdic3
        {
            get { return this.cb_FallaAdic3; }
            set { this.cb_FallaAdic3 = value; }
        }
        public ComboBox FallaReparacionAdic4
        {
            get { return this.cb_FallaAdic4; }
            set { this.cb_FallaAdic4 = value; }
        }
        public ComboBox FallaReparacionAdic5
        {
            get { return this.cb_FallaAdic5; }
            set { this.cb_FallaAdic5 = value; }
        }

        public ComboBox UnidadAlmacenamiento
        {
            get { return this.cb_UA; }
            set { this.cb_UA = value; }
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
            get { return this.lvDocumentMaster2_2; }
            set { this.lvDocumentMaster2_2 = value; }
        }

        public ListView ListadoItemsAsignacion
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }

        public TextBlock TecnicoDiagnosticador
        {
            get { return this.textblock_TecDiag; }
            set { this.textblock_TecDiag = value; }
        }

        public TextBlock TecnicoReparador
        {
            get { return this.textblock_TecRepAnterior; }
            set { this.textblock_TecRepAnterior = value; }
        }

        public TextBlock FechaReparacion
        {
            get { return this.textblock_FechaAnterior; }
            set { this.textblock_FechaAnterior = value; }
        }

        public TextBlock FallaAnteriorDiag
        {
            get { return this.textblock_FallaAnterior; }
            set { this.textblock_FallaAnterior = value; }
        }

        public TextBox GetSerial1
        {
            get { return this.tb_Serial1_2; }
            set { this.tb_Serial1_2 = value; }
        }

        public TextBox GetNroCaja
        {
            get { return this.txt_nroCaja; }
            set { this.txt_nroCaja = value; }
        }

        public TextBox GetSerialAsignacion
        {
            get { return this.tb_Serial_Asignacion; }
            set { this.tb_Serial_Asignacion = value; }
        }

        public TextBox GetMacAsignacion
        {
            get { return this.tb_Mac_Asignacion; }
            set { this.tb_Mac_Asignacion = value; }
        }

        public TextBox ProductoProcesamiento
        {
            get { return this.txt_Producto; }
            set { this.txt_Producto = value; }
        }

        public TextBox FallaProcesamiento
        {
            get { return this.txt_FallaDiag; }
            set { this.txt_FallaDiag = value; }
        }

        public TextBox GetSerial2
        {
            get { return this.tb_Serial2_2; }
            set { this.tb_Serial2_2 = value; }
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

        public TextBox ObservacionesRep
        {
            get { return this.txt_ObservacionesRep; }
            set { this.txt_ObservacionesRep = value; }
        }

        public TextBox BuscarEstibaRecibo
        {
            get { return this.tb_BuscarEstibaRecibo; }
            set { this.tb_BuscarEstibaRecibo = value; }
        }

        public TextBox ObservacionesAdic
        {
            get { return this.txt_ObservacionesAdic; }
            set { this.txt_ObservacionesAdic = value; }
        }

        public TextBox ObservacionesAdic2
        {
            get { return this.txt_ObservacionesAdic2; }
            set { this.txt_ObservacionesAdic2 = value; }
        }

        public TextBox ObservacionesAdic3
        {
            get { return this.txt_ObservacionesAdic3; }
            set { this.txt_ObservacionesAdic3 = value; }
        }

        public TextBox ObservacionesAdic4
        {
            get { return this.txt_ObservacionesAdic4; }
            set { this.txt_ObservacionesAdic4 = value; }
        }

        //public TextBox ObservacionesAdic5
        //{
        //    get { return this.txt_ObservacionesAdic5; }
        //    set { this.txt_ObservacionesAdic5 = value; }
        //}

        public ComboBox MotivoSCRAP
        {
            get { return this.cb_MotivoScrap; }
            set { this.cb_MotivoScrap = value; }
        }

        public ComboBox GetListaEstado
        {
            get { return this.cb_BuscarItems; }
            set { this.cb_BuscarItems = value; }
        }

        public TextBox PartesCambiadas
        {
            get { return this.txt_PartesCambiadas; }
            set { this.txt_PartesCambiadas = value; }
        }

        //
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

        //Asignacion
        //public TextBox BuscarEstibaAsignacion
        //{
        //    get { return this.tb_BuscarEstibaAsignacion; }
        //    set { this.tb_BuscarEstibaAsignacion = value; }
        //}

        //public ComboBox BuscarPosicionAsignacion
        //{
        //    get { return this.cb_BuscarPosicionAsignacion; }
        //    set { this.cb_BuscarPosicionAsignacion = value; }
        //}

        //public ListView ListadoBusquedaAsignacion
        //{
        //    get { return this.lv_ListadoBusquedaAsignacion; }
        //    set { this.lv_ListadoBusquedaAsignacion = value; }
        //}

        //public Border StackListadoEquiposEstiba
        //{
        //    get { return this.Stack_ListadoEquiposEstibas; }
        //    set { this.Stack_ListadoEquiposEstibas = value; }
        //}

        //public ListView ListadoEquiposEstiba
        //{
        //    get { return this.lv_ListadoEquiposEstiba; }
        //    set { this.lv_ListadoEquiposEstiba = value; }
        //}

        //public Border StackAsignacionTecnico
        //{
        //    get { return this.Stack_AsignacionTecnico; }
        //    set { this.Stack_AsignacionTecnico = value; }
        //}

        public TextBlock TecnicoAsignado
        {
            get { return this.textblock_TecAsignado; }
            set { this.textblock_TecAsignado = value; }
        }

        public ComboBox TecnicosAsignar
        {
            get { return this.cb_ListadoTecnicos_2; }
            set { this.cb_ListadoTecnicos_2 = value; }
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

        public StackPanel StackProcesoReparacion
        {
            get { return this.Stack_Adicionales_2; }
            set { this.Stack_Adicionales_2 = value; }
        }

        public ComboBox TecnicosReparacion
        {
            get { return this.cb_FiltroTecnico; }
            set { this.cb_FiltroTecnico = value; }
        }

        public ListView ListadoItemsAgregados
        {
            get { return this.lvDocumentMasterAddToPallet; }
            set { this.lvDocumentMasterAddToPallet = value; }
        }

        public TextBlock TextBlockTecnico
        {
            get { return this.txtTecnico; }
            set { this.txtTecnico = value; }
        }

        public StackPanel StackListaEquiposEntrega
        {
            get { return this.StackListaEquipos; }
            set { this.StackListaEquipos = value; }
        }

        public Border Border_ListaHP
        {
            get { return this.Border_DetailH; }
            set { this.Border_DetailH = value; }
        }

        public Border Border_ListEquipos
        {
            get { return this.BorderDetailJ; }
            set { this.BorderDetailJ = value; }
        }

        public TextBox GetQuery
        {
            get { return this.txt_Query; }
            set { this.txt_Query = value; }
        }

        #endregion

        #region Metodos

        private void btn_BuscarEquipos_Click(object sender, RoutedEventArgs e)
        {
            BuscarEquiposPorTecnicoEntrega(sender, e);
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void btn_BuscarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }

        private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosRecibo(sender, e);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btn_ConfirmarRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }

        private void cb_BuscarItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cb_FiltroTecnico.SelectedIndex == -1)
                return;

            this.BTN_BuscarEquipos.IsEnabled = true;
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Tab
            if (e.Key == Key.Tab)
            {
                GetSerialAsignacion.Text = GetSerialAsignacion.Text.ToString().ToUpper();
                //Paso el focus al siguiente campo de serial
                GetMacAsignacion.Focus();
            }

            if (e.Key == Key.Enter)
            {
                GetSerialAsignacion.Text = GetSerialAsignacion.Text.ToString().ToUpper();
                GetMacAsignacion.Focus();
            }
        }

        private void cb_HabilitarMotivo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HabilitarMotivo(sender, e);
            
        }

        private void tb_Serial_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                GetSerial2.Focus();
                CargarHistorico(sender, e);
            }
        }

        private void tb_Serial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarDatosReparacion(sender, e);
            }
        }

        private void tb_Serial2_KeyDown_2(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                AddLine(sender, e);
            }
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }

        private void CheckBox_Checked_HEADER(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Model.ListRecords.Rows.Count; i++)
                Model.ListRecords.Rows[i]["Checkm"] = true;
        }

        private void CheckBox_Unchecked_HEADER(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Model.ListRecords.Rows.Count; i++)
                Model.ListRecords.Rows[i]["Checkm"] = false;
        }
        private void refreshListadoTecnicos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ConsultarTecnicos(sender, e);
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
            chkRep.IsChecked = false;
        }

        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetails(sender, e);

            pw.Visibility = Visibility.Collapsed;
            pw.Close();
        }

        private void btn_imprimirHablador_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresion(sender, e);
        }

        private void cb_CambioDestino(object sender, SelectionChangedEventArgs e)
        {
            HabilitarUbicacion(sender, e);
        }

        //Asignacion
        private void btn_BuscarListadoEstibaAsignacion_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosAsignacion(sender, e);
        }

        private void btn_ActualizarListadoEstibaAsignacion_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosAsignacion(sender, e);
        }

        private void lv_ListadoAsignacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ConsultaReparacionAnterior(sender, e);
            Stack_AsignacionTecnico_2.Visibility = Visibility.Visible;
        }

        private void btn_AsignarTecnico_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarTecnicoEquipo(sender, e);
        }

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteDetails(sender, e);
        }

        private void cbFilter(object sender, SelectionChangedEventArgs e)
        {
            if (this.cb_BuscarItems.SelectedIndex == -1)
                return;

            this.BTN_BuscarEquipos.IsEnabled = true;
        }

        private void btn_AddToList_Click_1(object sender, RoutedEventArgs e)
        {
            AddToList(sender, e);
        }

        private void btn_Remove_Click_1(object sender, RoutedEventArgs e)
        {
            RemoveSelection(sender, e);
        }

        private void txt_showItemsByUser_MouseDownClick_1(object sender, MouseButtonEventArgs e)
        {
            BuscarEquiposPorTecnico(sender, e);
            Stack_EquiposPorUsuario.Visibility = Visibility.Visible;
            string texto = txt_showItemsByUser.Text;
            if (texto.Contains("Mostrar"))
            {
                txt_showItemsByUser.Text = "Ocultar busqueda de equipos <<";
                Border_ListEquipos.Visibility = Visibility.Visible;
            }
            else
            {
                txt_showItemsByUser.Text = "Mostrar equipos por técnico >>";
                Border_ListEquipos.Visibility = Visibility.Collapsed;
            }
        }
        
        #endregion
    }

    public interface IReparacionesView
    {
        //Clase Modelo
        ReparacionesModel Model { get; set; }

        #region Variables
        TextBlock TXT_filterResults { get; set; }
        Button BTN_BuscarEquipos { get; set; }
        ComboBox GetListBinInicio { get; set; }
        TabItem GetTabEntrega { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UnidadAlmacenamiento { get; set; }
        StackPanel StackUbicacion { get; set; }
        ComboBox NuevaUbicacion { get; set; }
        ComboBox FallaReparacion { get; set; }
        ComboBox FallaReparacionAdic { get; set; }
        ComboBox FallaReparacionAdic2 { get; set; }
        ComboBox FallaReparacionAdic3 { get; set; }
        ComboBox FallaReparacionAdic4 { get; set; }
        ComboBox FallaReparacionAdic5 { get; set; }
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        GridView ListadoEquipos { get; set; }
        Button GetButtonConfirmar { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetNroCaja { get; set; }
        TextBox GetSerialAsignacion { get; set; }
        TextBox GetMacAsignacion { get; set; }
        TextBox ProductoProcesamiento { get; set; }
        TextBox FallaProcesamiento { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox CodigoEmpaque { get; set; }
        TextBox ObservacionesRep { get; set; }
        TextBox ObservacionesAdic { get; set; }
        TextBox ObservacionesAdic2 { get; set; }
        TextBox ObservacionesAdic3 { get; set; }
        TextBox ObservacionesAdic4 { get; set; }
        ComboBox GetListaEstado { get; set; }
        //TextBox ObservacionesAdic5 { get; set; }
        TextBox PartesCambiadas { get; set; }
        Border BorderDetails { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoItemsAsignacion { get; set; }
        ComboBox MotivoSCRAP { get; set; }
        TextBlock TecnicoDiagnosticador { get; set; }
        TextBlock TecnicoReparador { get; set; }
        TextBlock FallaAnteriorDiag { get; set; }
        TextBlock FechaReparacion { get; set; }

        //Recibo
        TextBox BuscarEstibaRecibo { get; set; }
        ListView ListadoBusquedaRecibo { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock Estibas_Seleccionadas { get; set; }
        TextBlock TecnicoAsignado { get; set; }
        ComboBox TecnicosAsignar { get; set; }
        ComboBox TecnicosReparacion { get; set; }
        StackPanel StackProcesoReparacion { get; set; }
        ListView ListadoItemsAgregados { get; set; }
        TextBlock TextBlockTecnico { get; set; }
        StackPanel StackListaEquiposEntrega { get; set; }
        Border Border_ListaHP { get; set; }
        Border Border_ListEquipos { get; set; }
        TextBlock txt_User { get; set; }

        TextBox GetQuery { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> ConfirmarImpresion;
        event EventHandler<EventArgs> GenerarCodigo;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<SelectionChangedEventArgs> FiltrarDatosEntrega;
        event EventHandler<EventArgs> CargarDatosReparacion;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        //Asignacion
        event EventHandler<EventArgs> BuscarRegistrosAsignacion;
        event EventHandler<EventArgs> ActualizarRegistrosAsignacion;
        event EventHandler<EventArgs> ListarEquiposEstibas;
        event EventHandler<EventArgs> MostrarTecnicosEstibas;
        event EventHandler<EventArgs> ConfirmarTecnicoEquipo;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<SelectionChangedEventArgs> HabilitarUbicacion;
        event EventHandler<EventArgs> ConsultaReparacionAnterior;
        event EventHandler<EventArgs> DeleteDetails;
        event EventHandler<EventArgs> FiltraPorTecnico;
        event EventHandler<EventArgs> AddToList;
        event EventHandler<EventArgs> RemoveSelection;
        event EventHandler<EventArgs> HabilitarMotivo;
        event EventHandler<EventArgs> CargarHistorico;
        event EventHandler<MouseButtonEventArgs> BuscarEquiposPorTecnico;
        event EventHandler<EventArgs> BuscarEquiposPorTecnicoEntrega;
        event EventHandler<EventArgs> ConsultarTecnicos;

        #endregion

    }
}