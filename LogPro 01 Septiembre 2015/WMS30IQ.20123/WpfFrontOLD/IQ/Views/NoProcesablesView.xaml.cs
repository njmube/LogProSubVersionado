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
    /// Interaction logic for NoProcesablesView.xaml
    /// </summary>
    public partial class NoProcesablesView : UserControlBase, INoProcesablesView
    {

        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;

        #endregion

        public NoProcesablesView()
        {
            InitializeComponent();
        }

        #region Variables

        public NoProcesablesModel Model
        {
            get { return this.DataContext as NoProcesablesModel; }
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

        public ListView ListadoEstibas
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
        }

        public ComboBox UbicacionDestino
        {
            get { return this.cb_BodDestino; }
            set { this.cb_BodDestino = value; }
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
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            //Procesar el Archivo Cargado
            if (fUpload.StreamFile != null)
            {
                string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

                ProcessFile1(sender, e, dataFile);
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
        }

        #endregion

    }

    public interface INoProcesablesView
    {
        //Clase Modelo
        NoProcesablesModel Model { get; set; }

        #region Variables

        StackPanel GetStackUploadFile { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        UploadFile GetUpLoadFile { get; set; }
        GridView ListadoEquipos { get; set; }
        ListView ListadoEstibas { get; set; }
        ComboBox UbicacionDestino { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;

        #endregion

    }
}