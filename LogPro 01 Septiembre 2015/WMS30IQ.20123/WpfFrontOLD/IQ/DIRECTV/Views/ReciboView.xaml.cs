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
    /// Interaction logic for ReciboView.xaml
    /// </summary>
    public partial class ReciboView : UserControlBase, IReciboView
    {

        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> DeleteDetails;
        public event EventHandler<EventArgs> KillProcess;
        public event EventHandler<EventArgs> ExportCargue;
        //PREALERTA
        public event EventHandler<EventArgs> CargaPrealerta;
        public event EventHandler<EventArgs> SaveDetailsPrealerta;
        public event EventHandler<EventArgs> ExportCarguePrea;

        #endregion

        public ReciboView()
        {
            InitializeComponent();
        }

        #region Variables

        public ReciboModel Model
        {
            get
            { return this.DataContext as ReciboModel; }
            set
            { this.DataContext = value; }
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

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public TextBox GetSerial2
        {
            get { return this.tb_Serial2; }
            set { this.tb_Serial2 = value; }
        }

        public TextBox GetSerial3
        {
            get { return this.tb_Serial3; }
            set { this.tb_Serial3 = value; }
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

        //PREALERTA

        public ListView ListadoEquiposPrealerta
        {
            get { return this.lvDocumentMasterPrea; }
            set { this.lvDocumentMasterPrea = value; }
        }

        public UploadFile GetUpLoadFilePrea
        {
            get { return this.fUploadPrea; }
            set { this.fUploadPrea = value; }
        }

        public TextBlock GetEstado_CarguePrealerta
        {
            get { return this.textblock_estadoCarguePrea; }
            set { this.textblock_estadoCarguePrea = value; }
        }

        public ProgressBar Progress_CarguePrealerta
        {
            get { return this.PBar_carguePrea; }
            set { this.PBar_carguePrea = value; }
        }


        public Dispatcher Dispatcher_CarguePrealerta
        {
            get { return this.Dispatcher; }
        }

        public ListView ListadoNo_CarguePrea
        {
            get { return this.lv_NoCarguePrea; }
            set { this.lv_NoCarguePrea = value; }
        }

        public TextBox GetPreaConsecutivo
        {
            get { return this.tb_consecutivo; }
            set { this.tb_consecutivo = value; }
        }

        public ComboBox GetPreaTipo_Origen
        {
            get { return this.tb_tipoOrigenPreale; }
            set { this.tb_tipoOrigenPreale = value; }
        }

        public TextBox GetPreaOrigen
        {
            get { return this.tb_alerOrigen; }
            set { this.tb_alerOrigen = value; }
        }

        public TextBox GetPreaDireccion
        {
            get { return this.tb_AlerDireccion; }
            set { this.tb_AlerDireccion = value; }
        }

        public TextBox GetPreaNombre_contacto
        {
            get { return this.tb_AlerNombreContacto; }
            set { this.tb_AlerNombreContacto = value; }
        }

        public TextBox GetPreaCelular_contacto
        {
            get { return this.tb_AlerCelular; }
            set { this.tb_AlerCelular = value; }
        }

        public ComboBox GetPreaTipo_Recoleccion
        {
            get { return this.tb_tipoRecoleccion; }
            set { this.tb_tipoRecoleccion = value; }
        }

        public TextBox GetPreaNro_Pedido
        {
            get { return this.tb_nroPedido; }
            set { this.tb_nroPedido = value; }
        }

        public System.Windows.Controls.DatePicker GetPreFecha_Emision
        {
            get { return this.datePicker1; }
            set { this.datePicker1 = value; }
        }

        public Dispatcher Dispatcher_PreAlertas
        {
            get { return this.Dispatcher; }
        }


        #endregion

        #region Metodos

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                if (tb_Serial1.Text.Length < 14 || tb_Serial1.Text.Length > 14)
                {
                    Util.ShowError("El serial debe contener 14 digitos!");
                    tb_Serial1.Text = "";
                    return;
                }

                if (tb_Serial1.Text.ToString().Contains("o") || tb_Serial1.Text.ToString().Contains("O"))
                {
                    Util.ShowError("No puede digitar el caracter 'o' 'O' debe reemplazarlo por el numero cero(0)");
                    tb_Serial1.Text = "";
                    return;
                }
                else
                {
                    //Paso el focus al siguiente campo de serial
                    GetSerial2.Focus();
                }   
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                if (tb_Serial2.Text.StartsWith("00") == false)
                {
                    Util.ShowError("La estructura del ID Receiver es incorrecta!");
                    tb_Serial1.Text = "";
                    return;
                }

                if (tb_Serial2.Text.Length < 12 || tb_Serial2.Text.Length > 12)
                {
                    Util.ShowError("El serial debe contener 12 digitos!");
                    tb_Serial1.Text = "";
                    tb_Serial2.Text = "";
                    GetSerial1.Focus();
                    return;
                }

                if (tb_Serial2.Text.ToString().Contains("o") || tb_Serial2.Text.ToString().Contains("O"))
                {
                    Util.ShowError("No puede digitar el caracter 'o' 'O' debe reemplazarlo por el numero cero(0)");
                    tb_Serial1.Text = "";
                    tb_Serial2.Text = "";
                    GetSerial1.Focus();
                    return;
                }
                else
                {
                    if(tb_Serial3.IsEnabled)
                    //Paso el focus al siguiente campo de serial
                        GetSerial3.Focus();
                    else
                        AddLine(sender, e);
                }   
            }
        }

        private void tb_Serial3_KeyDown_1(object sender, KeyEventArgs e)
        {

            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                if (!tb_Serial3.Text.StartsWith("000") || tb_Serial3.Text.StartsWith("0000") == true)
                {
                    Util.ShowError("La Smart Card no cuenta con la estructura correcta!");
                    tb_Serial1.Text = "";
                    return;
                }

                if (tb_Serial3.Text != "" && tb_Serial3.Text != null)
                {
                    if (tb_Serial3.Text.Length < 12 || tb_Serial3.Text.Length > 12)
                    {

                        Util.ShowError("La Smart Card debe contener 12 digitos!");
                        tb_Serial1.Text = "";
                        tb_Serial2.Text = "";
                        tb_Serial3.Text = "";
                        GetSerial1.Focus();
                        return;
                    }
                    else
                    {
                        AddLine(sender, e);
                    }
                }
                else { 
                    //Adiciono el registro al listado
                    AddLine(sender, e);
                }
                
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

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteDetails(sender, e);
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

        private void chkDisableSmart_Checked_1(object sender, RoutedEventArgs e)
        {
            tb_Serial3.IsEnabled = false;
        }

        private void UnchkDisableSmart_Checked_1(object sender, RoutedEventArgs e)
        {  
            tb_Serial3.IsEnabled = true;
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
                return;

            SaveDetails(sender, e);
        }

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCargue(sender, e);
        }

        //PREALERTA

        private void fUpload_OnFileUpload_Prea(object sender, EventArgs e)
        {
            KillProcess(sender, e);
            string Cadena = fUploadPrea.FileName.ToString();
            //Valido que el Archivo seleccionado tengo formato .txt 
            if (Cadena.Contains(".xls") == false || Cadena.Contains(".xlsx") == true)
            {
                Util.ShowError("El Archivo cargado no tiene el formato correcto");
                return;
            }
            else
            {
                //Procesar el Archivo Cargado
                if (fUploadPrea.StreamFile != null)
                {
                    try
                    {
                        fUploadPrea.IsEnabled = false;
                        CargaPrealerta(sender, e);
                    }
                    catch (Exception)
                    {
                        Util.ShowError("Error al cargar el archivo, revise que el formato de cargue sea correcto");
                    }

                }
            }
            fUploadPrea.IsEnabled = true;
            fUploadPrea.StreamFile = null;
        }

        private void Btn_GuardarPrea_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
                return;

            SaveDetailsPrealerta(sender, e);
        }

        private void Btn_ExportarPrea_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCarguePrea(sender, e);
        }

        #endregion

        #region Eventos formulario registro - PREALERTA

        /*Metodos utilizados para los eventos del formulario de registro de nuevas prealertas*/

        private void tb_nroPedido_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_tipoRecoleccion_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_AlerCelular_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_AlerNombreContacto_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_AlerDireccion_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //datePicker1.Focus();
            }
        }

        private void tb_alerOrigen_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_tipoOrigen_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }

        private void tb_consecutivo_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                Util.ShowError("Enter");
            }
        }


        #endregion

    }

    public interface IReciboView
    {
        //Clase Modelo
        ReciboModel Model { get; set; }

        #region Obtener 

        StackPanel GetStackUploadFile { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox GetSerial3 { get; set; }
        UploadFile GetUpLoadFile { get; set; }
        GridView ListadoEquipos { get; set; }
        
        ListView ListadoEquiposAProcesar { get; set; }

        TextBlock GetEstado_Cargue { get; set; }
        Dispatcher Dispatcher_Cargue { get; }
        ProgressBar Progress_Cargue { get; set; }
        ListView ListadoNo_Cargue { get; set; }

        //PREALERTA
        UploadFile GetUpLoadFilePrea { get; set; }
        TextBlock GetEstado_CarguePrealerta { get; set; }
        Dispatcher Dispatcher_CarguePrealerta { get; }
        ProgressBar Progress_CarguePrealerta { get; set; }
        ListView ListadoNo_CarguePrea { get; set; }

        TextBox GetPreaNro_Pedido { get; set; }
        ComboBox GetPreaTipo_Recoleccion { get; set; }
        TextBox GetPreaCelular_contacto { get; set; }
        TextBox GetPreaNombre_contacto { get; set; }
        TextBox GetPreaDireccion { get; set; }
        TextBox GetPreaOrigen { get; set; }
        ComboBox GetPreaTipo_Origen { get; set; }
        TextBox GetPreaConsecutivo { get; set; }
        System.Windows.Controls.DatePicker GetPreFecha_Emision { get; set; }
        Dispatcher Dispatcher_PreAlertas { get; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> DeleteDetails;
        event EventHandler<EventArgs> KillProcess;
        event EventHandler<EventArgs> ExportCargue;
        //PREALERTA
        event EventHandler<EventArgs> CargaPrealerta;
        event EventHandler<EventArgs> SaveDetailsPrealerta;
        event EventHandler<EventArgs> ExportCarguePrea;

        #endregion

    }
}