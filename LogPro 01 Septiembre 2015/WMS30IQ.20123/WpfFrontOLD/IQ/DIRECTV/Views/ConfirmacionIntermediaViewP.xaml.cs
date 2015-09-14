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
    public partial class ConfirmacionIntermediaViewP : UserControlBase, IConfirmacionIntermediaViewP
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> EmpacarConfirmacion;
        public event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        public event EventHandler<EventArgs> ImprimirHablador;
        public event EventHandler<EventArgs> GenerarCodigo;

        #endregion

        public ConfirmacionIntermediaViewP()
        {
            InitializeComponent();
        }

        #region Variables

        public ConfirmacionIntermediaModelP Model
        {
            get { return this.DataContext as ConfirmacionIntermediaModelP; }
            set { this.DataContext = value; }
        }

        //public ComboBox Ubicacion
        //{
        //    get { return this.cb_Ubicacion; }
        //    set { this.cb_Ubicacion = value; }
        //}

        //public ComboBox UnidadAlmacenamiento
        //{
        //    get { return this.cb_UA; }
        //    set { this.cb_UA = value; }
        //}

        //public TextBox CodigoEmpaque
        //{
        //    get { return this.txt_CodEmpaque; }
        //    set { this.txt_CodEmpaque = value; }
        //}

        public ListView ListadoItems
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
        }

        public GridView ListadoEquipos1
        {
            get { return this.GridViewDetails_2; }
            set { this.GridViewDetails_2 = value; }
        }

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

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            //ActualizarSmart
            EmpacarConfirmacion(sender, e);
        }

        private void btn_imprimir_Click_1(object sender, RoutedEventArgs e)
        {
            ImprimirHablador(sender, e);
        }

        private void ImageGenerate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GenerarCodigo(sender, e);
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            try { 
                if (e.Key == Key.Enter)
                {
                    AddLine(sender, e);
                }
            }catch(Exception ex){
                Util.ShowError(ex.Message);
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

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilaSeleccionada(sender, e);
        }


    }

    public interface IConfirmacionIntermediaViewP
    {
        //Clase Modelo
        ConfirmacionIntermediaModelP Model { get; set; }

        #region Variables

        //ComboBox Ubicacion { get; set; }
        //ComboBox UnidadAlmacenamiento { get; set; }
        //TextBox CodigoEmpaque { get; set; }
        //ComboBox SmartCardEstado { get; set; }
        // TextBox GetSmartCard1 { get; set; }
        //GridView ListadoEquipos { get; set; }
        GridView ListadoEquipos1 { get; set; }
        ListView ListadoItems { get; set; }

        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        //Recibo
        //ComboBox BuscarPosicionRecibo { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> EmpacarConfirmacion;
        event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<SelectionChangedEventArgs> FilaSeleccionada;
        event EventHandler<EventArgs> ImprimirHablador;
        event EventHandler<EventArgs> GenerarCodigo;

        #endregion

    }
}