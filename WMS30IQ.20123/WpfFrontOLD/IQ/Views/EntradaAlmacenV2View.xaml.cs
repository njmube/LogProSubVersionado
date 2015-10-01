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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using WpfFront.IQ.Models;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for EntradaAlmacenV2View.xaml
    /// </summary>
    public partial class EntradaAlmacenV2View : UserControlBase, IEntradaAlmacenV2View
    {
        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<EventArgs> CargaMasiva;
        public event EventHandler<EventArgs> CargaMasiva_Alerta;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> SavePrealertAll;
        public event EventHandler<EventArgs> SaveDetails_Prealert;
        public event EventHandler<EventArgs> DeleteDetails;
        public event EventHandler<EventArgs> ExportCargue;
        public event EventHandler<EventArgs> ExportCargueAlerta;
        public event EventHandler<EventArgs> KillProcess;

        #endregion

        private int _noOfErrorsOnScreen = 0;
        private RequiredValidator _customer = new RequiredValidator();

        public EntradaAlmacenV2View()
        {
            InitializeComponent();
            //stackValidation.DataContext = _customer;
            //Stack_UploadFile.Visibility = Visibility.Collapsed;
        }

        #region Variables

        public EntradaAlmacenV2Model Model
        {
            get
            { return this.DataContext as EntradaAlmacenV2Model; }
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

        public TextBlock GetEstado_Cargue
        {
            get { return this.textblock_estadoCargue; }
            set { this.textblock_estadoCargue = value; }
        }

        public TextBlock GetEstado_CargueSer
        {
            get { return this.textblock_estadoCargueSer; }
            set { this.textblock_estadoCargueSer = value; }
        }

        public ComboBox Producto
        {
            get { return this.Producto; }
            set { this.Producto = value; }
        }

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public ProgressBar Progress_CargueAlertas
        {
            get { return this.PBar_cargue; }
            set { this.PBar_cargue = value; }
        }

        public ProgressBar Progress_CargueAlertasSer
        {
            get { return this.PBar_cargueSer; }
            set { this.PBar_cargueSer = value; }
        }

        public ListView ListadoEquiposPre_Alerta
        {
            get { return this.lvDocumentMaster1; }
            set { this.lvDocumentMaster1 = value; }
        }

        public ListView ListadoNo_Cargue
        {
            get { return this.lv_NoCargue; }
            set { this.lv_NoCargue = value; }
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

        public UploadFile GetUpLoadFile_Prealerta
        {
            get { return this.fUpload_Prealerta; }
            set { this.fUpload_Prealerta = value; }
        }

        public GridView ListadoEquipos
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public Dispatcher Dispatcher_PreAlertas
        {
            get { return this.Dispatcher; }
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

        //public String GetPreFecha_Emision {
        //    get { return this.datePicker1; }
        //    set { this.datePicker1 = value; }
        //}

        #endregion

        #region Metodos

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                if (tb_Serial1.Text.ToString().Contains("o") || tb_Serial1.Text.ToString().Contains("O"))
                {
                    Util.ShowError("No puede digitar el caracter 'o' 'O' debe reemplazarlo por el numero cero(0)");
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
                if (tb_Serial2.Text.ToString().Contains("o") || tb_Serial2.Text.ToString().Contains("O"))
                {
                    Util.ShowError("No puede digitar el caracter 'o' 'O' debe reemplazarlo por el numero cero(0)");
                }
                else
                {
                    if (tb_Serial1.Text.ToString() == tb_Serial2.Text.ToString())
                    {
                        Util.ShowError("El campo serial y mac no pueden ser iguales.");
                        tb_Serial1.Text = "";
                        tb_Serial2.Text = "";
                        tb_Serial1.Focus();
                    }
                    else
                    {
                        //Adiciono el registro al listado
                        AddLine(sender, e);
                    }
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
                //pw.Close();
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

        //Carga el archivo de pre-alertas
        private void fUpload_OnFileUpload_2(object sender, EventArgs e)
        {
            KillProcess(sender, e);
            string Cadena = fUpload_Prealerta.FileName.ToString();

            //Valido que el Archivo seleccionado tengo formato .XLS
            if (Cadena.Contains(".xls") == false || Cadena.Contains(".xlsx") == true)
            {
                Util.ShowError("El Archivo cargado no tiene el formato correcto");
                //pw.Close();
                return;
            }
            else
            {
                //Procesar el Archivo Cargado
                if (fUpload_Prealerta.StreamFile != null)
                {
                    try
                    {
                        fUpload_Prealerta.IsEnabled = false;
                        CargaMasiva_Alerta(sender, e);
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError("Error al cargar el archivo, revise que el formato de cargue sea correcto " + ex.Message);
                    }
                }
            }
            fUpload_Prealerta.StreamFile = null;

        }


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

        static bool FindBorderInListView(DependencyObject dep, ListView listView,
            out Border border, out ListViewItem lvItem)
        {
            border = null;
            lvItem = null;

            DependencyObject depObj = dep;
            while (depObj != listView)
            {
                if (border == null && depObj is Border)
                {
                    border = depObj as Border;
                    if (border.Name != "myOwnBorder")
                    {
                        border = null;
                    }
                }
                else if (depObj is ListViewItem)
                {
                    lvItem = depObj as ListViewItem;
                }

                depObj = VisualTreeHelper.GetParent(depObj);
            }
            return border != null && lvItem != null;
        }

        //public void GridViewColumnHeaderClickedHandler(object sender,RoutedEventArgs e)
        //{
        //  GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;


        //string header = headerClicked.Column.Header as string;;

        //System.Windows.MessageBox.Show();
        //}

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            fUpload_Prealerta.IsEnabled = true;
            DeleteDetails(sender, e);
        }

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCargue(sender, e);
        }

        private void Btn_ExportarAlert_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCargueAlerta(sender, e);
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ReplicateDetailsBy_Column(sender, e);

            //ListViewItem myListViewItem = lvDocumentMaster.SelectedItem as ListViewItem;

            //System.Windows.MessageBox.Show(lvDocumentMaster.SelectedIndex.ToString());

            //Metodo3
            ////String firstValue="";
            ////String secondValue="";

            ////for (int conditionVar = 0; conditionVar < lvDocumentMaster.Items.Count; conditionVar++)
            ////{
            ////    firstValue = lvDocumentMaster.Items[conditionVar].ToString();
            ////    secondValue = lvDocumentMaster.Items[conditionVar].ToString();
            ////}

            ////System.Windows.MessageBox.Show("HEYYYYYY "+firstValue+" +++ "+secondValue);

            //Metodo2

            ////DependencyObject dep = (DependencyObject)e.OriginalSource;

            ////while ((dep != null) && !(dep is ListViewItem))
            ////{
            ////    dep = VisualTreeHelper.GetParent(dep);
            ////}

            ////if (dep == null)
            ////    return;

            //////var item = lvDocumentMaster.ItemContainerGenerator.ItemFromContainer(dep);
            //////System.Windows.MessageBox.Show("HOLA--- "+item.ToString());

            ///Metodo1

            //var valor = lvDocumentMaster.SelectedItem;
            //var valor_real = lvDocumentMaster.Items.GetItemAt(lvDocumentMaster.SelectedIndex);

            //System.Windows.MessageBox.Show("EEE " + valor_real);                   

            ////System.Windows.MessageBox.Show("EEE "+valor_real.GetType()+"HOLA " + valor + " indice " + lvDocumentMaster.SelectedIndex+ "Valor real: "+valor_real);


            //var item = ((FrameworkElement)e.OriginalSource).DataContext as Track;
            //if (item != null)
            //{
            //    System.Windows.MessageBox.Show("Item's Double Click handled!");
            //}
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
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer replicar estos registros?") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Replicando registros...por favor espere...");
            //Replica la informacion de la primera linea en las demas.
            ReplicateDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Visible;
            pw.Close();
            chkRep.IsChecked = false;
        }

        private void Btn_Guardar_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros... por favor espere...");
            SaveDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            fUpload.IsEnabled = true;
            pw.Close();
        }

        private void Btn_GuardarPrealer_Click_1(object sender, RoutedEventArgs e)
        {

            if (GetPreaConsecutivo.Text.Equals("") || GetPreaNro_Pedido.Text.Equals("") || GetPreaOrigen.Text.Equals("") || GetPreaTipo_Origen.Text.Equals("") || GetPreaTipo_Recoleccion.Text.Equals("") || GetPreFecha_Emision.Text.Equals(""))
            {
                Util.ShowMessage("Debe completar los campos obligatorios del formulario");
                return;
            }

            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de crear una nueva prealerta") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SavePrealertAll(sender, e);

            pw.Visibility = Visibility.Collapsed;
            fUpload_Prealerta.IsEnabled = true;
            pw.Close();
        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnScreen++;
            else
                _noOfErrorsOnScreen--;
        }

        private void AddCustomer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnScreen == 0;
            e.Handled = true;
        }

        private void AddCustomer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RequiredValidator cust = stackValidation.DataContext as RequiredValidator;
            // write code here to add Customer

            // reset UI
            //_customer = new RequiredValidator();
            stackValidation.DataContext = _customer;
            //e.Handled = true;
        }

        #endregion
    }

    public interface IEntradaAlmacenV2View
    {
        //Clase Modelo
        EntradaAlmacenV2Model Model { get; set; }

        #region Obtener Variables

        StackPanel GetStackUploadFile { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBlock GetEstado_Cargue { get; set; }
        TextBlock GetEstado_CargueSer { get; set; }
        TextBox GetPreaNro_Pedido { get; set; }
        ComboBox GetPreaTipo_Recoleccion { get; set; }
        TextBox GetPreaCelular_contacto { get; set; }
        TextBox GetPreaNombre_contacto { get; set; }
        TextBox GetPreaDireccion { get; set; }
        TextBox GetPreaOrigen { get; set; }
        ComboBox GetPreaTipo_Origen { get; set; }
        TextBox GetPreaConsecutivo { get; set; }
        UploadFile GetUpLoadFile { get; set; }
        UploadFile GetUpLoadFile_Prealerta { get; set; }
        GridView ListadoEquipos { get; set; }
        ComboBox Producto { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }
        ListView ListadoEquiposPre_Alerta { get; set; }
        ListView ListadoNo_Cargue { get; set; }
        Dispatcher Dispatcher_PreAlertas { get; }
        ProgressBar Progress_CargueAlertas { get; set; }
        ProgressBar Progress_CargueAlertasSer { get; set; }
        System.Windows.Controls.DatePicker GetPreFecha_Emision { get; set; }
        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        event EventHandler<EventArgs> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> SavePrealertAll;
        event EventHandler<EventArgs> SaveDetails_Prealert;
        event EventHandler<EventArgs> DeleteDetails;
        event EventHandler<EventArgs> ExportCargue;
        event EventHandler<EventArgs> ExportCargueAlerta;
        event EventHandler<EventArgs> CargaMasiva_Alerta;
        event EventHandler<EventArgs> KillProcess;

        #endregion
    }
}
