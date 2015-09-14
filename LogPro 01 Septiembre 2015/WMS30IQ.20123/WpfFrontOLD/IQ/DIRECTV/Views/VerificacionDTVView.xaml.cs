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
    /// Interaction logic for VerificacionDTVView.xaml
    /// </summary>
    public partial class VerificacionDTVView : UserControlBase, IVerificacionDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> GenerarCodigo;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> Imprimir;
        public event EventHandler<EventArgs> ConfirmarImpresion;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;

        #endregion

        public VerificacionDTVView()
        {
            InitializeComponent();
        }

        #region Variables

        public VerificacionDTVModel Model
        {
            get { return this.DataContext as VerificacionDTVModel; }
            set { this.DataContext = value; }
        }

        //public StackPanel GetStackUploadFile
        //{
        //    get { return this.Stack_UploadFile; }
        //    set { this.Stack_UploadFile = value; }
        //}

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

        public ComboBox GetListaEstado
        {
            get { return this.cb_BuscarItems; }
            set { this.cb_BuscarItems = value; }
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

        public TextBox CodigoEmpaque
        {
            get { return this.txt_CodEmpaque; }
            set { this.txt_CodEmpaque = value; }
        }


        //public UploadFile GetUpLoadFile
        //{
        //    get { return this.fUpload; }
        //    set { this.fUpload = value; }
        //}

        //public ListView ListadoEquiposUbicacion
        //{
        //    get { return this.lv_EquiposUbicacion; }
        //    set { this.lv_EquiposUbicacion = value; }
        //}

        //public ComboBox ListadoCambioEstado
        //{
        //    get { return this.cb_BodDestino_2; }
        //    set { this.cb_BodDestino_2 = value; }
        //}

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        //Recibo
        //public TextBox BuscarEstibaRecibo
        //{
        //    get { return this.tb_BuscarEstibaRecibo; }
        //    set { this.tb_BuscarEstibaRecibo = value; }
        //}

        //public ComboBox BuscarPosicionRecibo
        //{
        //    get { return this.cb_BuscarPosicionRecibo; }
        //    set { this.cb_BuscarPosicionRecibo = value; }
        //}

        //public ListView ListadoBusquedaRecibo
        //{
        //    get { return this.lv_ListadoBusquedaRecibo; }
        //    set { this.lv_ListadoBusquedaRecibo = value; }
        //}

        #endregion

        #region Metodos

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Tab
            if (e.Key == Key.Tab)
            {
                GetSerial1.Focus();
            }

            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                //Paso el focus al siguiente campo de serial
                GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //Adiciono el registro al listado
                AddLine(sender, e);
            }
        }

        private void fUpload_OnFileUpload_1(object sender, EventArgs e)
        {
            ////Mostrar ventana de Cargando...
            //ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            ////Procesar el Archivo Cargado
            //if (fUpload.StreamFile != null)
            //{
            //    string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

            //    ProcessFile1(sender, e, dataFile);
            //}
            ////Cierro ventana de Cargando...
            //pw.Visibility = Visibility.Collapsed;
            //pw.Close();
        }

        private void ImageRefresh_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmBasicData(sender, e);
        }

        private void ProcessFile(object sender, EventArgs e, string dataFile)
        {

            ////Linea por linea obtiene el dato del serial y del RR y lo procesa.
            ////Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            //DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            //if (lines == null || lines.Rows.Count == 0)
            //{
            //    Util.ShowError("No hay registros a procesar.\n" + dataFile);
            //    return;
            //}

            //int NumeroSerial;
            //foreach (DataRow dr in lines.Rows)
            //{
            //    NumeroSerial = 1;
            //    foreach (DataColumn dc in lines.Columns)
            //    {
            //        switch (NumeroSerial.ToString())
            //        {
            //            case "1":
            //                GetSerial1.Text = dr[dc.ColumnName].ToString();
            //                break;
            //            case "2":
            //                GetSerial2.Text = dr[dc.ColumnName].ToString();
            //                break;
            //        }
            //        NumeroSerial++;
            //    }

            //    AddLine(sender, e);
            //}

            //fUpload.StreamFile = null;
        }

        private void ProcessFile1(object sender, EventArgs e, string dataFile)
        {

            ////Linea por linea obtiene el dato del serial y del RR y lo procesa.
            ////Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            //DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            //if (lines == null || lines.Rows.Count == 0)
            //{
            //    Util.ShowError("No hay registros a procesar.\n" + dataFile);
            //    return;
            //}

            //CargaMasiva(sender, new DataEventArgs<DataTable>(lines));

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
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);

            ListadoItems.Visibility = Visibility.Collapsed;
        }

        private void btn_imprimirHablador_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarImpresion(sender, e);
        }

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            Imprimir(sender, e);
        }

        private void cb_BuscarItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListadoItems.Visibility = Visibility.Visible;

            ConfirmBasicData(sender, e);
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
        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void btn_ConfirmarRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        #endregion

    }

    public interface IVerificacionDTVView
    {
        //Clase Modelo
        VerificacionDTVModel Model { get; set; }

        #region Variables

        //StackPanel GetStackUploadFile { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        //UploadFile GetUpLoadFile { get; set; }
        //ListView ListadoEquiposUbicacion { get; set; }
        //ComboBox ListadoCambioEstado { get; set; }
        GridView ListadoEquipos { get; set; }
        ListView ListadoItems { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }
        ComboBox GetListaEstado { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UnidadAlmacenamiento { get; set; }
        TextBox CodigoEmpaque { get; set; }

        //Recibo
        //TextBox BuscarEstibaRecibo { get; set; }
        //ComboBox BuscarPosicionRecibo { get; set; }
        //ListView ListadoBusquedaRecibo { get; set; }

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
        event EventHandler<EventArgs> Imprimir;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        #endregion

    }
}