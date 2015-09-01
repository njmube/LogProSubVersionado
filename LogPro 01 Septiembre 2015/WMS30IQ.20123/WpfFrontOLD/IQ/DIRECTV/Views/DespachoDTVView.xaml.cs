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
    /// Interaction logic for DespachoDTVView.xaml
    /// </summary>
    public partial class DespachoDTVView : UserControlBase, IDespachoDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        //public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<EventArgs> ImprimirHablador;

        public event EventHandler<EventArgs> BuscarRegistrosDespachos;
        public event EventHandler<EventArgs> MostrarEquiposDespacho;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<EventArgs> ExportPalletSeleccion;
        public event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

        public DespachoDTVView()
        {
            InitializeComponent();
        }

        #region Variables

        public DespachoDTVModel Model
        {
            get { return this.DataContext as DespachoDTVModel; }
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

        public ComboBox BuscarModeloDespacho
        {
            get { return this.cb_BuscarModeloDespacho; }
            set { this.cb_BuscarModeloDespacho = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaDespacho
        {
            get { return this.cb_BuscarFechaDespacho; }
            set { this.cb_BuscarFechaDespacho = value; }
        }

        public ListView ListadoBusquedaDespachos
        {
            get { return this.lv_ListadoBusquedaDespachos; }
            set { this.lv_ListadoBusquedaDespachos = value; }
        }

        public ListView ListadoPalletSeriales
        {
            get { return this.lv_PalletSeriales; }
            set { this.lv_PalletSeriales = value; }
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

        public GridView GridViewListaDespacho
        {
            get { return this.GridViewDetails_1; }
            set { this.GridViewDetails_1 = value; }
        }

        public GridView GridViewListaEquiposDespacho
        {
            get { return this.GridViewDetails2; }
            set { this.GridViewDetails2 = value; }
        }

        //public ListView ListadoBusquedaCambioClasificacion
        //{
        //    get { return this.lvDocumentMaster_2; }
        //    set { this.lvDocumentMaster_2 = value; }
        //}

        #endregion

        #region Metodos


        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirHablador(sender, e);
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
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
                            //GetSerial1.Text = dr[dc.ColumnName].ToString();
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

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////      Recibo     ////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

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

        private void btn_BuscarDespachos_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosDespachos(sender, e);
        }

        private void lv_ListadoBusquedaDespachos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            MostrarEquiposDespacho(sender, e);
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }

        private void Btn_ExportarPalletsSeleccion_Click_1(object sender, RoutedEventArgs e)
        {
            ExportPalletSeleccion(sender, e);
        }

        private void MySelectionChangedDespacho(object sender, SelectionChangedEventArgs e)
        {
            ListarEquiposSeleccion(sender, e);
        }

        #endregion

    }

    public interface IDespachoDTVView
    {
        //Clase Modelo
        DespachoDTVModel Model { get; set; }

        #region Variables

        ComboBox Ubicacion { get; set; }
        ListView ListadoItems { get; set; }
        ComboBox BuscarModeloDespacho { get; set; }
        System.Windows.Controls.DatePicker GetFechaDespacho { get; set; }
        ListView ListadoBusquedaDespachos { get; set; }
        ListView ListadoPalletSeriales { get; set; }
        TextBlock TotalSeriales { get; set; }
        TextBlock Estibas_Seleccionadas { get; set; }
        GridView GridViewListaDespacho { get; set; }
        GridView GridViewListaEquiposDespacho { get; set; }

        //ListView ListadoBusquedaCambioClasificacion { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarRegistros;


        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////      Recibo     ////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<EventArgs> ImprimirHablador;

        event EventHandler<EventArgs> BuscarRegistrosDespachos;
        event EventHandler<EventArgs> MostrarEquiposDespacho;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<EventArgs> ExportPalletSeleccion;
        event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

    }
}