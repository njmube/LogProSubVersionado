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
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using WpfFront.IQ.Models;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for AdministradorView.xaml
    /// </summary>
    public partial class ConsultaTrackingView : UserControlBase, IConsultaTrackingView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargarDatosAdministrador;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ActualizarRegistros;
        public event EventHandler<EventArgs> ReiniciarCapacitacion;
        public event EventHandler<EventArgs> ConsultarMovimientos;
        public event EventHandler<EventArgs> BuscarEquipoTracking;
        public event EventHandler<EventArgs> exportarTracking;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> BuscarNombreMaterial;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

        Chart charts;

        public ConsultaTrackingView()
        {
            InitializeComponent(); 

            charts = this.FindName("MyWinformChart") as Chart;
        }

        #region Variables

        public System.Windows.Controls.DataGrid ListadoSeriales
        {
            get { return this.dataGrid1; }
            set { this.dataGrid1 = value; }
        }
        public ConsultaTrackingModel Model
        {
            get { return this.DataContext as ConsultaTrackingModel; }
            set { this.DataContext = value; }
        }

        public Chart chart1
        {
            get { return charts; }
            set { charts = value; }
        }

      
        public TextBox GetSerialTrack
        {
            get { return this.tb_serial; }
            set { this.tb_serial = value; }
        }

       
        public TextBox GetMac_Track
        {
            get { return this.tb_mac; }
            set { this.tb_mac = value; }
        }


        public ListView ListadoEquipos_Track
        {
            get { return this.lv_equipos; }
            set { this.lv_equipos = value; }
        }

        #endregion

        #region Metodos

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            exportarTracking(sender, e);
        }

        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void lv_listEquipos_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConsultarMovimientos(sender, e);
        }

        private void Btn_reiniciar_Click_1(object sender, RoutedEventArgs e)
        {
            string entrada = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la contraseña para reiniciar la prueba");

            if (entrada == "S0p0rt31t")
            {
                ReiniciarCapacitacion(sender, e);
            }
            else
            {
                Util.ShowMessage("Contraseña incorrecta");
            }
        }

        private void tb_Serial_keydown_admin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_mac.Focus();
            }
        }

        private void tb_mac_keydown_admin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarEquipoTracking(sender, e);
            }
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarDatosAdministrador(sender, e);
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                CargarDatosAdministrador(sender, e);
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

        private void cb_EstadoSerial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void cb_Origen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuscarRegistrosRecibo(sender, e);
        }

        private void cb_NombreMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuscarNombreMaterial(sender, e);
        }

        private void cb_FallaDiag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void cb_FallaVerif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddLine(sender, e);
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

        private List<DOHoursChartItem> GetItems()
        {
            var items = new List<DOHoursChartItem>()
           {
              new DOHoursChartItem("John", 120),
              new DOHoursChartItem("Amanda", 40),
              new DOHoursChartItem("David", 70),
              new DOHoursChartItem("Rachel", 10),
           };
            // compute the percentages
            var totalHours = 500;
            foreach (var item in items)
            {
                item.Percent = (item.Hours * 100.0) / totalHours;
            }

            return items;
        }

        #endregion
    }

    public interface IConsultaTrackingView
    {
        //Clase Modelo
        ConsultaTrackingModel Model { get; set; }

        #region Variables
        TextBox GetSerialTrack { get; set; }
        TextBox GetMac_Track { get; set; }
        ListView ListadoEquipos_Track { get; set; }
        Chart chart1 { get; set; }
        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> CargarDatosAdministrador;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarRegistros;
        event EventHandler<EventArgs> ReiniciarCapacitacion;
        event EventHandler<EventArgs> ConsultarMovimientos;
        event EventHandler<EventArgs> BuscarEquipoTracking;
        event EventHandler<EventArgs> exportarTracking;
        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> BuscarNombreMaterial;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

    }

    class DOHoursChartItemConsultaTracking
    {
        public String Name { get; set; }
        public double Hours { get; set; }
        public double Percent { get; set; }
        public DOHoursChartItemConsultaTracking(string name, double hours)
        {
            this.Name = name;
            this.Hours = hours;
        }
    }
}