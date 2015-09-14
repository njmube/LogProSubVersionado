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
    /// Interaction logic for ReparacionesDTVView.xaml
    /// </summary>
    public partial class ReparacionesDTVView : UserControlBase, IReparacionesDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> GenerarCodigo;
        public event EventHandler<EventArgs> FiltrarDatosEntrega;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargarDatosReparacion;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ConfirmarImpresion;
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
        //procesos
        public event EventHandler<EventArgs> CargarHistorico;
        public event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

        public ReparacionesDTVView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";
        }

        #region Variables


        public ReparacionesDTVModel Model
        {
            get { return this.DataContext as ReparacionesDTVModel; }
            set { this.DataContext = value; }
        }

        public ListView ListadoBusquedaCambioClasificacion
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
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
        public ListView ListaHistorico
        {
            get { return this.lv_HistoricoReparaciones; }
            set { this.lv_HistoricoReparaciones = value; }
        }
        public Border Border_ListaHP
        {
            get { return this.Border_DetailH; }
            set { this.Border_DetailH = value; }
        }
        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }

        public TextBox GetSerial1
        {
            get { return this.tb_Serial1_2; }
            set { this.tb_Serial1_2 = value; }
        }

        public TextBox PartesCambiadas
        {
            get { return this.txt_PartesCambiadas; }
            set { this.txt_PartesCambiadas = value; }
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

        public ComboBox MotivoSCRAP
        {
            get { return this.cb_MotivoScrap; }
            set { this.cb_MotivoScrap = value; }
        }

        public TextBlock TecnicoAsignado
        {
            get { return this.textblock_TecAsignado; }
            set { this.textblock_TecAsignado = value; }
        }

        public TextBlock TecnicoDiagnosticador
        {
            get { return this.textblock_TecDiag; }
            set { this.textblock_TecDiag = value; }
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

        public ComboBox GetListaEstado
        {
            get { return this.cb_BuscarItems; }
            set { this.cb_BuscarItems = value; }
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

        //public TextBlock TecnicoAsignado
        //{
        //    get { return this.txt_TecnicoAsignado; }
        //    set { this.txt_TecnicoAsignado = value; }
        //}

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

        public ComboBox SubFallas
        {
            get { return this.cb_SubFallaRep; }
            set { this.cb_SubFallaRep = value; }
        }

        #endregion

        #region Metodos


        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);

            ListadoItems.Visibility = Visibility.Collapsed;
        }

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

        private void btn_imprimirHablador_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresion(sender, e);
        }

        private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }

        private void cb_BuscarItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListadoItems.Visibility = Visibility.Visible;

            FiltrarDatosEntrega(sender, e);
        }

        private void cb_HabilitarMotivo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)FallaReparacionAdic5.SelectedItem).Content.ToString() != "SCRAP")
                MotivoSCRAP.IsEnabled = false;
            else
                MotivoSCRAP.IsEnabled = true;
        }

        private void tb_Serial_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                GetSerial2.Focus();
                CargarHistorico(sender , e);
            }
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSerialAsignacion.Text = GetSerialAsignacion.Text.ToString().ToUpper();
                GetMacAsignacion.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                CargarDatosReparacion(sender, e);
            }
        }

        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void tb_Serial2_KeyDown_2(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
            }
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
            
            GetSerial1.Focus();
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
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

        //private void lv_ListadoBusquedaAsignacion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ListadoBusquedaAsignacion.SelectedItem != null)
        //        ListarEquiposEstibas(sender, e);
        //}

        //private void lv_ListadoEquiposEstiba_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ListadoEquiposEstiba.SelectedItem != null)
        //        MostrarTecnicosEstibas(sender, e);
        //}

        private void lv_ListadoAsignacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stack_AsignacionTecnico_2.Visibility = Visibility.Visible;
        }

        private void btn_AsignarTecnico_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarTecnicoEquipo(sender, e);
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
            ListarEquiposSeleccion(sender, e);
        }

        #endregion

    }

    public interface IReparacionesDTVView
    {
        //Clase Modelo
        ReparacionesDTVModel Model { get; set; }

        #region Variables

        ComboBox GetListBinInicio { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UnidadAlmacenamiento { get; set; }
        ComboBox FallaReparacion { get; set; }
        ComboBox FallaReparacionAdic { get; set; }
        ComboBox FallaReparacionAdic2 { get; set; }
        ComboBox FallaReparacionAdic3 { get; set; }
        ComboBox FallaReparacionAdic4 { get; set; }
        ComboBox FallaReparacionAdic5 { get; set; }
        ComboBox MotivoSCRAP { get; set; }
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        GridView ListadoEquipos { get; set; }
        Button GetButtonConfirmar { get; set; }
        TextBox GetSerial1 { get; set; }
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
        //TextBox ObservacionesAdic5 { get; set; }
        TextBox PartesCambiadas { get; set; }
        Border BorderDetails { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoItemsAsignacion { get; set; }
        TextBlock TecnicoAsignado { get; set; }
        TextBlock TecnicoDiagnosticador { get; set; }
        ComboBox GetListaEstado { get; set; }

        //Recibo
        TextBox BuscarEstibaRecibo { get; set; }
        //ComboBox BuscarPosicionRecibo { get; set; }
        ListView ListadoBusquedaRecibo { get; set; }

        //Asignacion
        //TextBox BuscarEstibaAsignacion { get; set; }
        //ComboBox BuscarPosicionAsignacion { get; set; }
        //ListView ListadoBusquedaAsignacion { get; set; }
        //Border StackListadoEquiposEstiba { get; set; }
        //ListView ListadoEquiposEstiba { get; set; }
        //Border StackAsignacionTecnico { get; set; }
        //TextBlock TecnicoAsignado { get; set; }
        ComboBox TecnicosAsignar { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock Estibas_Seleccionadas { get; set; }
        StackPanel StackProcesoReparacion { get; set; }

        //PROCESO
        ListView ListaHistorico { get; set; }
        Border Border_ListaHP { get; set; }

        ListView ListadoBusquedaCambioClasificacion { get; set; }

        ComboBox SubFallas { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> ConfirmarImpresion;
        event EventHandler<EventArgs> GenerarCodigo;
        event EventHandler<EventArgs> FiltrarDatosEntrega;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
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
        //Procesos
        event EventHandler<EventArgs> CargarHistorico;
        event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

    }
}