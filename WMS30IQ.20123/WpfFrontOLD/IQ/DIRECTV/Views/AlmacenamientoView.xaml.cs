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
using System.Windows.Threading;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for AlmacenamientoView.xaml
    /// </summary>
    public partial class AlmacenamientoView : UserControlBase, IAlmacenamientoView
    {

        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        //public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;

        public event EventHandler<EventArgs> GenerarPallet;
        public event EventHandler<KeyEventArgs> KeyConsultarPallet;
        public event EventHandler<KeyEventArgs> EnterConsultarPallet;
        public event EventHandler<EventArgs> SeleccionPallet_Consulta;
        public event EventHandler<EventArgs> Imprimir_Hablador;
        public event EventHandler<EventArgs> ListarEquiposSeleccion;
        public event EventHandler<EventArgs> ImprimirHabladorAlmacen;
        public event EventHandler<EventArgs> EliminarEquipo_Fila;
        public event EventHandler<EventArgs> GenerarNumero;
        public event EventHandler<EventArgs> KillProcess;
        public event EventHandler<EventArgs> CargaMasiva;

        #endregion

        public AlmacenamientoView()
        {
            InitializeComponent();
        }

        #region Variables

        public AlmacenamientoModel Model
        {
            get { return this.DataContext as AlmacenamientoModel; }
            set { this.DataContext = value; }
        }

        public ListView ListadoBusquedaCambioClasificacion
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
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

        public ComboBox EstadoRecibo
        {
            get { return this.cb_EstadoPallet; }
            set { this.cb_EstadoPallet = value; }
        }

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }



        //public TextBox CodigoEmpaque
        //{
        //    get { return this.txt_CodEmpaque; }
        //    set { this.txt_CodEmpaque = value; }
        //}

        //public ComboBox UnidadAlmacenamiento
        //{
        //    get { return this.cb_UA; }
        //    set { this.cb_UA = value; }
        //}

        public TextBox BuscarEstibaRecibo
        {
            get { return this.tb_BuscarEstibaRecibo; }
            set { this.tb_BuscarEstibaRecibo = value; }
        }

        //txt_CodEmpaque
        public TextBox GetSerial2
        {
            get { return this.tb_Serial2; }
            set { this.tb_Serial2 = value; }
        }

        public UploadFile GetUpLoadFile
        {
            get { return this.fUpload; }
            set { this.fUpload = value; }
        }

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public TextBox GetCodPallet
        {
            get { return this.tb_CodPallet; }
            set { this.tb_CodPallet = value; }
        }

        public TextBox GetCodPalletBusqueda
        {
            get { return this.tb_CodPalletBusqueda; }
            set { this.tb_CodPalletBusqueda = value; }
        }

        public ListView ListadoPalletsBusqueda
        {
            get { return this.lvPalletsAlmacenamiento; }
            set { this.lvPalletsAlmacenamiento = value; }
        }

        public ComboBox GetUbicacionPallet
        {
            get { return this.tb_UbicacionPallet; }
            set { this.tb_UbicacionPallet = value; }
        }

        public TextBlock GetEstado_Cargue
        {
            get { return this.textblock_estadoCargue; }
            set { this.textblock_estadoCargue = value; }
        }

        public ProgressBar Progress_Cargue
        {
            get { return this.PBar_cargue; }
            set { this.PBar_cargue = value; }
        }


        public Dispatcher Dispatcher_Cargue
        {
            get { return this.Dispatcher; }
        }

        public ListView ListadoNo_Cargue
        {
            get { return this.lv_NoCargue; }
            set { this.lv_NoCargue = value; }
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

        private void fUpload_OnFileUpload_1(object sender, EventArgs e)
        {
            KillProcess(sender, e);
            string Cadena = fUpload.FileName.ToString();

            //Valido que el Archivo seleccionado tengo formato .txt 
            if (Cadena.Contains(".xls") == false || Cadena.Contains(".xlsx") == true)
            {
                Util.ShowError("El Archivo cargado no tiene el formato correcto");
                return;
            }
            else
            {
                //Procesar el Archivo Cargado
                if (fUpload.StreamFile != null)
                {
                    try
                    {
                        fUpload.IsEnabled = false;
                        CargaMasiva(sender, e);
                    }
                    catch (Exception)
                    {
                        Util.ShowError("Error al cargar el archivo, revise que el formato de cargue sea correcto");
                    }

                }
            }
            fUpload.StreamFile = null;
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

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void btn_BuscarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
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

            //CargaMasiva(sender, new DataEventArgs<DataTable>(lines));

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

        private void btn_ActualizarListadoEstibaRecibo_Click_1(object sender, RoutedEventArgs e)
        {
            ActualizarRegistrosRecibo(sender, e);
        }


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
            fUpload.IsEnabled = true;
            pw.Close();
        }

        private void ImgGenerate_estiba(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //GenerarPallet(sender, e);
            GenerarNumero(sender, e);
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

        private void lv_ListadoPallets_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            SeleccionPallet_Consulta(sender, e);
        }

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            Imprimir_Hablador(sender, e);
        }

        private void lv_ListadoBusquedaCambioClasificacion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ListarEquiposSeleccion(sender, e);
        }

        private void btn_imprimirAlmacen_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirHabladorAlmacen(sender, e);
        }

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            EliminarEquipo_Fila(sender, e);
        }

        #endregion

    }

    public interface IAlmacenamientoView
    {
        //Clase Modelo
        AlmacenamientoModel Model { get; set; }

        #region Variables

        StackPanel GetStackUploadFile { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        UploadFile GetUpLoadFile { get; set; }
        GridView ListadoEquipos { get; set; }
        TextBox BuscarEstibaRecibo { get; set; }
        ListView ListadoBusquedaRecibo { get; set; }
        ComboBox Ubicacion { get; set; }
        ComboBox UbicacionDesp { get; set; }

        ComboBox EstadoRecibo { get; set; }
        TextBox GetCodPallet { get; set; }
        TextBox GetCodPalletBusqueda { get; set; }
        ComboBox GetUbicacionPallet { get; set; }
        ListView ListadoPalletsBusqueda { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }

        ListView ListadoBusquedaCambioClasificacion { get; set; }

        TextBlock GetEstado_Cargue { get; set; }
        Dispatcher Dispatcher_Cargue { get; }
        ProgressBar Progress_Cargue { get; set; }

        ListView ListadoNo_Cargue { get; set; }

        //TextBox CodigoEmpaque { get; set; }
        //ComboBox UnidadAlmacenamiento { get; set; }


        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        //event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> BuscarRegistrosRecibo;

        event EventHandler<EventArgs> GenerarPallet;
        event EventHandler<KeyEventArgs> KeyConsultarPallet;
        event EventHandler<KeyEventArgs> EnterConsultarPallet;
        event EventHandler<EventArgs> SeleccionPallet_Consulta;
        event EventHandler<EventArgs> Imprimir_Hablador;
        event EventHandler<EventArgs> ListarEquiposSeleccion;
        event EventHandler<EventArgs> ImprimirHabladorAlmacen;
        event EventHandler<EventArgs> EliminarEquipo_Fila;
        event EventHandler<EventArgs> GenerarNumero;
        event EventHandler<EventArgs> KillProcess;
        event EventHandler<EventArgs> CargaMasiva;


        #endregion

    }
}