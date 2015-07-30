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
    public partial class SmartCardDTVViewP : UserControlBase, ISmartCardDTVViewP
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> AddLineReciclaje;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> SaveDetailsReciclaje;
        public event EventHandler<EventArgs> ActualizarSmart;
        public event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        public event EventHandler<EventArgs> ExportCargue;
        public event EventHandler<EventArgs> ExportCargueAsig;
        public event EventHandler<EventArgs> LoadSmartAsig;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_ColumnRec;

        #endregion

        public SmartCardDTVViewP()
        {
            InitializeComponent();
            //Text_ShowHide.Text = "<< Ocultar";
        }

        #region Variables

        public SmartCardDTVModelP Model
        {
            get { return this.DataContext as SmartCardDTVModelP; }
            set { this.DataContext = value; }
        }

        public ComboBox GetFiltroEstado
        {
            get { return this.cb_Estado; }
            set { this.cb_Estado = value; }
        }

        //public ComboBox SmartCardEstado
        //{
        //    get { return this.cb_smartEstado; }
        //    set { this.cb_smartEstado = value; }
        //}

        public TextBox GetSmartCard1
        {
            get { return this.tb_Smart1; }
            set { this.tb_Smart1 = value; }
        }

        public TextBox GetSmartCardReciclaje
        {
            get { return this.tb_Smart11; }
            set { this.tb_Smart11 = value; }
        }

        //public ComboBox UnidadAlmacenamiento
        //{
        //    get { return this.cb_UA; }
        //    set { this.cb_UA = value; }
        //}

        //public TextBlock GetTextHideShowHeader
        //{
        //    get { return this.Text_ShowHide; }
        //    set { this.Text_ShowHide = value; }
        //}

        //public ComboBox UnidadAlmacenamiento
        //{
        //    get { return this.cb_UA; }
        //    set { this.cb_UA = value; }
        //}

        //public Border GetBorderHeader
        //{
        //    get { return this.Border_Header; }
        //    set { this.Border_Header = value; }
        //}

        public ListView ListadoItems
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
        }

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public ListView ListEquiposReciclaje
        {
            get { return this.lvDocumentMaster1; }
            set { this.lvDocumentMaster1 = value; }
        }

        public ListView ListadoItemsAsignados
        {
            get { return this.lvDocumentMaster_22; }
            set { this.lvDocumentMaster_22 = value; }
        }
        //public StackPanel GetStackUploadFile
        //{
        //    get { return this.Stack_UploadFile; }
        //    set { this.Stack_UploadFile = value; }
        //}

        //public Button GetButtonConfirmar
        //{
        //    get { return this.btn_confirmar; }
        //    set { this.btn_confirmar = value; }
        //}
        //public TextBox GetSerial1
        //{
        //    get { return this.tb_Serial1; }
        //    set { this.tb_Serial1 = value; }
        //}

        //public TextBox GetSerial2
        //{
        //    get { return this.tb_Serial2; }
        //    set { this.tb_Serial2 = value; }
        //}

        //public TextBox CodigoEmpaque
        //{
        //    get { return this.txt_CodEmpaque; }
        //    set { this.txt_CodEmpaque = value; }
        //}

        //public Border BorderDetails
        //{
        //    get { return this.Border_Detail; }
        //    set { this.Border_Detail = value; }
        //}

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public GridView ListadoEquipos1
        {
            get { return this.GridViewDetails_2; }
            set { this.GridViewDetails_2 = value; }
        }

        public GridView ListadoEquiposReciclaje
        {
            get { return this.GridViewDetails1; }
            set { this.GridViewDetails1 = value; }
        }

        //Recibo
        //public TextBox BuscarEstibaRecibo
        //{
        //    get { return this.tb_BuscarEstibaRecibo; }
        //    set { this.tb_BuscarEstibaRecibo = value; }
        //}

        //public ComboBox BuscarPosicionRecibo
        //{
        //    get { return this.cb_smartEstado; }
        //    set { this.cb_smartEstado = value; }
        //}

        //public ListView ListadoBusquedaRecibo
        //{
        //    get { return this.lv_ListadoBusquedaRecibo; }
        //    set { this.lv_ListadoBusquedaRecibo = value; }
        //}

        #endregion

        #region Metodos

        private void tb_Smart_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //Paso el focus al siguiente campo de serial
                //Util.ShowError("Bienvenidos");
                AddLine(sender, e);
            }
            
        }

        //private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        //{
        //    ActualizarSmart(sender, e);
        //}

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

        //private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    ConfirmBasicData(sender, e);
        //}

        private void ImageRefresh_MouseDown2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadSmartAsig(sender, e);
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

        private void tb_Smart_KeyDown_11(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //Paso el focus al siguiente campo de serial
                //Util.ShowError("Bienvenidos");
                AddLineReciclaje(sender, e);
            }
            
        }

        private void Btn_Guardar_Click_11(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetailsReciclaje(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        private void GridViewColumnHeaderClickedHandler2(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_ColumnRec(sender, e);
        }

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCargue(sender, e);
        }

        private void Btn_Exportar_Click_2(object sender, RoutedEventArgs e)
        {
            ExportCargueAsig(sender, e);
        }

        #endregion

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }

        private void btn_BuscarSmartEstado_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }

    }

    public interface ISmartCardDTVViewP
    {
        //Clase Modelo
        SmartCardDTVModelP Model { get; set; }

        #region Variables

        //ComboBox GetListBinInicio { get; set; }
        //ComboBox SmartCardEstado { get; set; }
        TextBox GetSmartCard1 { get; set; }
        TextBox GetSmartCardReciclaje { get; set; }
        ComboBox GetFiltroEstado { get; set; }
        //TextBlock GetTextHideShowHeader { get; set; }
        //Border GetBorderHeader { get; set; }
        GridView ListadoEquipos { get; set; }
        GridView ListadoEquiposReciclaje { get; set; }
        GridView ListadoEquipos1 { get; set; }
        //Button GetButtonConfirmar { get; set; }
        //TextBox GetSerial1 { get; set; }
        //TextBox GetSerial2 { get; set; }
        //TextBox CodigoEmpaque { get; set; }
        //Border BorderDetails { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }
        ListView ListEquiposReciclaje { get; set; }
        ListView ListadoItemsAsignados { get; set; }
        //Recibo
        //TextBox BuscarEstibaRecibo { get; set; }
        //ComboBox BuscarPosicionRecibo { get; set; }
        //ListView ListadoBusquedaRecibo { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> AddLineReciclaje;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> SaveDetailsReciclaje;
        event EventHandler<EventArgs> ActualizarSmart;
        event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_ColumnRec;
        event EventHandler<EventArgs> ExportCargue;
        event EventHandler<EventArgs> ExportCargueAsig;
        event EventHandler<EventArgs> LoadSmartAsig;

        #endregion

    }
}