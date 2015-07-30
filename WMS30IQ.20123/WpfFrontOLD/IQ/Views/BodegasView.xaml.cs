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
    /// Interaction logic for BodegasView.xaml
    /// </summary>
    public partial class BodegasView : UserControlBase, IBodegasView
    {

        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> GenerarPallet;
        public event EventHandler<EventArgs> SaveDetails;
        //public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        //public event EventHandler<EventArgs> ConfirmarMovimiento;
        //public event EventHandler<EventArgs> Imprimir_Hablador;
        public event EventHandler<EventArgs> EliminarEquipo_Fila;
        //public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        //public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<EventArgs> SeleccionPallet_Consulta;
        public event EventHandler<KeyEventArgs> KeyConsultarPallet;
        public event EventHandler<KeyEventArgs> EnterConsultarPallet;
        public event EventHandler<EventArgs> ImprimirHablador;

        #endregion

        public BodegasView()
        {
            InitializeComponent();
            Stack_UploadFile.Visibility = Visibility.Collapsed;
        }

        #region Variables

        public BodegasModel Model
        {
            get { return this.DataContext as BodegasModel; }
            set { this.DataContext = value; }
        }

        public StackPanel GetStackUploadFile
        {
            get { return this.Stack_UploadFile; }
            set { this.Stack_UploadFile = value; }
        }

        public TextBox GetSerial1
        {
            get { return this.tb_Serial1; }
            set { this.tb_Serial1 = value; }
        }

        public TextBlock GetTotalEquipos
        {
            get { return this.textblock_totalEquipos; }
            set { this.textblock_totalEquipos = value; }
        }

        //public ListView ListadoBusquedaRecibo
        //{
        //    get { return this.lv_ListadoBusquedaRecibo; }
        //    set { this.lv_ListadoBusquedaRecibo = value; }
        //}

        //public ComboBox Ubicacion
        //{
        //    get { return this.cb_Ubicacion; }
        //    set { this.cb_Ubicacion = value; }
        //}

        //public ComboBox UbicacionDesp
        //{
        //    get { return this.cb_UbicacionDesp; }
        //    set { this.cb_UbicacionDesp = value; }
        //}

        //public TextBox BuscarEstibaRecibo
        //{
        //    get { return this.tb_BuscarEstibaRecibo; }
        //    set { this.tb_BuscarEstibaRecibo = value; }
        //}

        //txt_CodEmpaque
        public TextBox GetSerial2
        {
            get { return this.tb_Serial2; }
            set { this.tb_Serial2 = value; }
        }

        public TextBox GetCodPallet
        {
            get { return this.tb_CodPallet; }
            set { this.tb_CodPallet = value; }
        }

        public ComboBox GetUbicacionPallet
        {
            get { return this.tb_UbicacionPallet; }
            set { this.tb_UbicacionPallet = value; }
        }

        public UploadFile GetUpLoadFile
        {
            get { return this.fUpload; }
            set { this.fUpload = value; }
        }

        public ListView ListadoEquiposAlmacenamiento
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        //public TextBlock TotalSeriales
        //{
        //    get { return this.textblock_totalSeriales1; }
        //    set { this.textblock_totalSeriales1 = value; }
        //}

        //public TextBlock Estibas_Seleccionadas
        //{
        //    get { return this.textblock_totalEstibas1; }
        //    set { this.textblock_totalEstibas1 = value; }
        //}

        public ListView ListadoPalletsBusqueda
        {
            get { return this.lvPalletsAlmacenamiento; }
            set { this.lvPalletsAlmacenamiento = value; }
        }

        public TextBox GetCodPalletBusqueda
        {
            get { return this.tb_CodPalletBusqueda; }
            set { this.tb_CodPalletBusqueda = value; }
        }

        public TextBlock setFechaGuardado
        {
            get { return this.textblock_Guardar; }
            set { this.textblock_Guardar = value; }
        }

        #endregion

        #region Metodos

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
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

        private void ImgGenerate_estiba(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarPallet(sender, e);
        }

        private void fUpload_OnFileUpload_1(object sender, EventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            string Cadena = fUpload.FileName.ToString();

            //Valido que el Archivo seleccionado tengo formato .txt 
            if (Cadena.Contains(".txt") == false)
            {
                Util.ShowError("El Archivo cargado no tiene el formato correcto");
                pw.Close();
                return;
            }
            else
            {
                //Procesar el Archivo Cargado
                if (fUpload.StreamFile != null)
                {
                    string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

                    ProcessFile1(sender, e, dataFile);
                }
            }

            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
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

            fUpload.StreamFile = null;
        }

        //private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        //{
        //    Imprimir_Hablador(sender, e);
        //}

        //private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        //{
        //    ConfirmarMovimiento(sender, e);
        //}

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            EliminarEquipo_Fila(sender, e);
        }

        //private void btn_BuscarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        //{
        //    BuscarRegistrosRecibo(sender, e);
        //}

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

            fUpload.StreamFile = null;
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

        //private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        //{
        //    ActualizarRegistrosRecibo(sender, e);
        //}


        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
        }

        //private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    FilaSeleccionada(sender, e);
        //}

        private void lv_ListadoPallets_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            SeleccionPallet_Consulta(sender, e);
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

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirHablador(sender, e);
        }

        #endregion

    }

    public interface IBodegasView
    {
        //Clase Modelo
        BodegasModel Model { get; set; }

        #region Variables

        StackPanel GetStackUploadFile { get; set; }
        ListView ListadoPalletsBusqueda { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetCodPalletBusqueda { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox GetCodPallet { get; set; }
        ComboBox GetUbicacionPallet { get; set; }
        UploadFile GetUpLoadFile { get; set; }
        ListView ListadoEquiposAlmacenamiento { get; set; }
        GridView ListadoEquipos { get; set; }
        //TextBox BuscarEstibaRecibo { get; set; }
        //ListView ListadoBusquedaRecibo { get; set; }
        //ComboBox Ubicacion { get; set; }
        //ComboBox UbicacionDesp { get; set; }
        //TextBlock TotalSeriales { get; set; }
        TextBlock GetTotalEquipos { get; set; }
        //TextBlock Estibas_Seleccionadas { get; set; }
        TextBlock setFechaGuardado { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> GenerarPallet;
        event EventHandler<EventArgs> SaveDetails;
        //event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        //event EventHandler<EventArgs> ConfirmarMovimiento;
        //event EventHandler<EventArgs> Imprimir_Hablador;
        event EventHandler<EventArgs> EliminarEquipo_Fila;
        //event EventHandler<EventArgs> BuscarRegistrosRecibo;
        //event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<EventArgs> SeleccionPallet_Consulta;
        event EventHandler<KeyEventArgs> KeyConsultarPallet;
        event EventHandler<KeyEventArgs> EnterConsultarPallet;
        event EventHandler<EventArgs> ImprimirHablador;

        #endregion

    }
}