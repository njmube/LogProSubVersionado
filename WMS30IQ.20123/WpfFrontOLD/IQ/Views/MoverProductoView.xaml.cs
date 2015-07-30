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
using WpfFront.WMSBusinessService;
using Odyssey.Controls;
using System.Data;
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for MoverProductoView.xaml
    /// </summary>
    public partial class MoverProductoView : UserControlBase, IMoverProductoView
    {
        #region Header

        public event EventHandler<DataEventArgs<Location>> LoadBinFrom;
        public event EventHandler<DataEventArgs<Bin>> LoadLocationTo;
        public event EventHandler<DataEventArgs<Location>> LoadBinTo;
        public event EventHandler<DataEventArgs<BinRoute>> LoadDocumentData;
        public event EventHandler<DataEventArgs<MMaster>> Cbx_Etiqueta1_SelectedValue;
        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<EventArgs> CancelBasicData;
        public event EventHandler<EventArgs> NewBasicData;
        public event EventHandler<EventArgs> Impresion_1;
        public event EventHandler<EventArgs> Impresion_2;



        #endregion

        #region Serial

        public event EventHandler<EventArgs> AddLine;
        /// <summary>
        /// Nuevas lineas de codigo.
        /// </summary>
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        /// <summary>
        /// Guardar productos movidos en cantidad.
        /// </summary>
        public event EventHandler<EventArgs> SaveProductos;

        #endregion

        #region Details

        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ReplicateDetails;


        /// <summary>
        /// Muetra la copia de la informacion de la lista de Entrada de Almacen.
        /// </summary>
        public event EventHandler<EventArgs> MostrarInformacion;


        #endregion

        public MoverProductoView()
        {
            InitializeComponent();
            expHeader.IsExpanded = true;
            expDetail.IsExpanded = false;
        }

        #region Variables

        public MoverProductoModel Model
        {
            get
            { return this.DataContext as MoverProductoModel; }
            set
            { this.DataContext = value; }
        }

        #region Header

        public OdcExpander expDetails
        {
            get { return this.expDetail; }
            set { this.expDetail = value; }
        }

        public ComboBox GetListLocationFrom
        {
            get { return this.cb_LocationFrom; }
            set { this.cb_LocationFrom = value; }
        }

        public ComboBox GetListBinFrom
        {
            get { return this.cb_BinFrom; }
            set { this.cb_BinFrom = value; }
        }

        public ComboBox GetListLocationTo
        {
            get { return this.cb_LocationTo; }
            set { this.cb_LocationTo = value; }
        }

        public ComboBox GetListBinTo
        {
            get { return this.cb_BinTo; }
            set { this.cb_BinTo = value; }
        }

        public ComboBox GetListCbx_Etiqueta1
        {
            get { return this.Cbx_Etiqueta1; }
            set { this.Cbx_Etiqueta1 = value; }
        }
        public StackPanel GetStackPanelHeader
        {
            get { return this.Stack_Panel_Header; }
            set { this.Stack_Panel_Header = value; }
        }

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
        }

        public Button GetButtonNuevo
        {
            get { return this.btn_nuevo; }
            set { this.btn_nuevo = value; }
        }

        public Button GetButtonCancelar
        {
            get { return this.btn_cancelar; }
            set { this.btn_cancelar = value; }
        }

        #endregion

        #region Serial

        public StackPanel GetStackSerial1
        {
            get { return this.Stack_Serial1; }
            set { this.Stack_Serial1 = value; }
        }

        public StackPanel GetStackSerial2
        {
            get { return this.Stack_Serial2; }
            set { this.Stack_Serial2 = value; }
        }

        public StackPanel GetStackSerial3
        {
            get { return this.Stack_Serial3; }
            set { this.Stack_Serial3 = value; }
        }

        public StackPanel GetStackUploadFile
        {
            get { return this.Stack_UploadFile; }
            set { this.Stack_UploadFile = value; }
        }

        public TextBlock GetTextSerial1
        {
            get { return this.txt_Serial1; }
            set { this.txt_Serial1 = value; }
        }

        public TextBlock GetTextSerial2
        {
            get { return this.txt_Serial2; }
            set { this.txt_Serial2 = value; }
        }

        public TextBlock GetTextSerial3
        {
            get { return this.txt_Serial3; }
            set { this.txt_Serial3 = value; }
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

        public TextBox GetSerial3
        {
            get { return this.tb_Serial3; }
            set { this.tb_Serial3 = value; }
        }

        /// <summary>
        /// Nuevas lineas de codigo.
        /// </summary>
        public SearchProduct GetProductLocation
        {
            get { return this.sp_Product; }
            set { this.sp_Product = value; }
        }

        /// <summary>
        /// Obtener el stackpanel que contiene el control "uc:SearchProduct".
        /// </summary>
        public StackPanel GetStackProduct
        {
            get { return this.Stack_Product; }
            set { this.Stack_Product = value; }
        }

        /// <summary>
        /// Control de cargar archivo.
        /// </summary>
        public UploadFile GetUpLoadFile
        {
            get { return this.fUpload; }
            set { this.fUpload = value; }
        }

        /// <summary>
        /// Stackpanel contenedor del textbox "Cantidad".
        /// </summary>
        public StackPanel GetStackCantidad
        {
            get { return this.Stack_Cantidad; }
            set { this.Stack_Cantidad = value; }
        }

        /// <summary>
        /// textbox Cantidad.
        /// </summary>
        public TextBox GetTextboxCantidad
        {
            get { return this.tb_Cantidad; }
            set { this.tb_Cantidad = value; }
        }

        #endregion

        #region Details

        public ListView LvDocumentMaster
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public GridView GetGridViewDetails
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        /// <summary>
        /// Texblock de recuento de filas.
        /// OK.
        /// </summary>
        public TextBlock RecuentoFilas
        {
            get { return this.textblock_recuento; }
            set { this.textblock_recuento = value; }
        }

        /// <summary>
        /// Registros no cargados.
        /// </summary>
        public TextBlock RegistrosNoCargados
        {
            get { return this.txt_RegistrosNoCargados; }
            set { this.txt_RegistrosNoCargados = value; }
        }

        #endregion

        #endregion

        #region Metodos

        #region Header

        private void expLabel_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, true);
        }

        private void expLabel_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckExpanders((OdcExpander)sender, false);
        }

        private void CheckExpanders(OdcExpander sender, bool expand)
        {

            if (expHeader == null || expDetail == null)
                return;

            if (sender.Name == "expHeader")
            {
                if (expand)
                    expDetail.IsExpanded = false;
                else
                    expDetail.IsExpanded = true;
                return;
            }

            if (sender.Name == "expDetail")
            {
                if (expand)
                    expHeader.IsExpanded = false;
                else
                    expHeader.IsExpanded = true;
                return;
            }
        }

        private void cb_LocationFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBinFrom(sender, new DataEventArgs<Location>((Location)GetListLocationFrom.SelectedItem));
        }

        private void cb_BinFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadLocationTo(sender, new DataEventArgs<Bin>((Bin)GetListBinFrom.SelectedItem));
        }

        private void cb_LocationTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBinTo(sender, new DataEventArgs<Location>((Location)GetListLocationTo.SelectedItem));
        }

        private void cb_BinTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDocumentData(sender, new DataEventArgs<BinRoute>((BinRoute)GetListBinTo.SelectedItem));
        }

        private void btn_confirmar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmBasicData(sender, e);
            Stack_Product.Visibility = Visibility.Collapsed;
        }

        private void btn_nuevo_Click(object sender, RoutedEventArgs e)
        {
            NewBasicData(sender, e);
        }

        private void btn_cancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelBasicData(sender, e);

            Border_1.IsEnabled = false; //Stack1
            Stack_Panel_Details.Visibility = Visibility.Collapsed; //Stack2
        }

        #endregion

        #region Serial

        private void tb_Serial1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSerial2.Visibility == Visibility.Visible)
                    GetSerial2.Focus();
                else
                    if (GetStackSerial3.Visibility == Visibility.Visible)
                        GetSerial3.Focus();
                    else
                        AddLine(sender, e);
            }
        }

        private void tb_Serial2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (GetStackSerial3.Visibility == Visibility.Visible)
                    GetSerial3.Focus();
                else
                    AddLine(sender, e);
            }
        }

        private void tb_Serial3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
            }
        }

        private void Click_Btn1_Impresiora_1(object sender, EventArgs e)
        {
            Impresion_1(sender, e);
        }

        private void Click_Btn2_Impresiora_2(object sender, EventArgs e)
        {
            Impresion_2(sender, e);
        }



        private void fUpload_OnFileUpload(object sender, EventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            //Procesar el Archivo Cargado
            if (fUpload.StreamFile != null)
            {
                string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

                ProcessFile(sender, e, dataFile);
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
                            if (GetStackSerial1.Visibility == Visibility.Visible)
                                GetSerial1.Text = dr[dc.ColumnName].ToString();
                            break;
                        case "2":
                            if (GetStackSerial2.Visibility == Visibility.Visible)
                                GetSerial2.Text = dr[dc.ColumnName].ToString();
                            break;
                        case "3":
                            if (GetStackSerial3.Visibility == Visibility.Visible)
                                GetSerial3.Text = dr[dc.ColumnName].ToString();
                            break;
                    }
                    NumeroSerial++;
                }
                AddLine(sender, e);
            }

            fUpload.StreamFile = null;

        }

        /// <summary>
        /// Buscar productos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_Product_OnLoadRecord(object sender, EventArgs e)
        {
            EvaluarTipoProducto(sender, new DataEventArgs<Product>(GetProductLocation.Product));
        }


        #endregion

        #region Details

        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            SaveDetails(sender, e);
            //Cierro ventana de Cargando...
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            Btn_Copiar.IsEnabled = true;
            btn_confirmar.IsEnabled = false;
        }

        #endregion



        #endregion

        private void chkRep_Checked(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Muetra la copia de la informacion de la lista de Mover Producto.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Copiar_Click(object sender, RoutedEventArgs e)
        {
            MostrarInformacion(sender, e);
        }

        /// <summary>
        /// Si el radiobutton SERIALIZABLE esta chequeado, habilitara ciertos campos.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb_serializable_Checked(object sender, RoutedEventArgs e)
        {
            Stack_Serial1.Visibility = Visibility.Visible;
            Stack_Serial2.Visibility = Visibility.Visible;
            Stack_UploadFile.Visibility = Visibility.Visible;
            Stack_Product.Visibility = Visibility.Collapsed;
            Stack_Cantidad.Visibility = Visibility.Collapsed;
            Btn_Guardar.Visibility = Visibility.Visible;
            Btn_GuardarProductosCantidad.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Si el radiobutton NO SERIALIZABLE esta chequeado, habilitara 
        /// dos campos y el resto lo ocultara.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb_no_serializable_Checked(object sender, RoutedEventArgs e)
        {
            //Ocultar campo "Cargar Archivo"
            //Mostrar nuevo campo "Cantidad", es decir, solo tendrian que haber
            //dos campos, "Producto" y "Cantidad".

            Stack_Serial1.Visibility = Visibility.Collapsed;
            Stack_Serial2.Visibility = Visibility.Collapsed;
            Stack_UploadFile.Visibility = Visibility.Collapsed;
            Stack_Product.Visibility = Visibility.Visible;
            Stack_Cantidad.Visibility = Visibility.Visible;
            Btn_Guardar.Visibility = Visibility.Collapsed;
            Btn_GuardarProductosCantidad.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Guardar informacion de Producto y Cantidad "mover productos no seriablizables".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_GuardarProductosCantidad_Click(object sender, RoutedEventArgs e)
        {
            SaveProductos(sender, e);
        }

        private void fUpload1_Loaded(object sender, RoutedEventArgs e)
        {

        }



        private void Cbx_Etiqueta_SelectedValue(object sender, SelectionChangedEventArgs e)
        {
            Cbx_Etiqueta1_SelectedValue(sender, new DataEventArgs<MMaster>((MMaster)GetListCbx_Etiqueta1.SelectedItem));
        }


    }


    public interface IMoverProductoView
    {
        //Clase Modelo
        MoverProductoModel Model { get; set; }

        #region Obtener Variables

        #region Header

        OdcExpander expDetails { get; set; }
        ComboBox GetListLocationFrom { get; set; }
        ComboBox GetListBinFrom { get; set; }
        ComboBox GetListLocationTo { get; set; }
        ComboBox GetListBinTo { get; set; }
        ComboBox GetListCbx_Etiqueta1 { get; set; }
        StackPanel GetStackPanelHeader { get; set; }
        Button GetButtonConfirmar { get; set; }
        Button GetButtonNuevo { get; set; }
        Button GetButtonCancelar { get; set; }

        #endregion

        #region Serial

        StackPanel GetStackSerial1 { get; set; }
        StackPanel GetStackSerial2 { get; set; }
        StackPanel GetStackSerial3 { get; set; }
        StackPanel GetStackUploadFile { get; set; }
        TextBlock GetTextSerial1 { get; set; }
        TextBlock GetTextSerial2 { get; set; }
        TextBlock GetTextSerial3 { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox GetSerial3 { get; set; }
        /// <summary>
        /// Nuevas lineas de codigo.
        /// </summary>
        SearchProduct GetProductLocation { get; set; }
        /// <summary>
        /// Obtener el stackpanel que contiene el control "uc:SearchProduct".
        /// </summary>
        StackPanel GetStackProduct { get; set; }
        /// <summary>
        /// Control de cargar archivo.
        /// </summary>
        UploadFile GetUpLoadFile { get; set; }
        /// <summary>
        /// Stackpanel contenedor del textbox "Cantidad".
        /// </summary>
        StackPanel GetStackCantidad { get; set; }
        TextBox GetTextboxCantidad { get; set; }

        #endregion

        #region Details

        ListView LvDocumentMaster { get; set; }
        GridView GetGridViewDetails { get; set; }
        Border BorderDetails { get; set; }

        //Nuevos campos
        TextBlock RecuentoFilas { get; set; }
        TextBlock RegistrosNoCargados { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Header

        event EventHandler<DataEventArgs<Location>> LoadBinFrom;
        event EventHandler<DataEventArgs<Bin>> LoadLocationTo;
        event EventHandler<DataEventArgs<Location>> LoadBinTo;
        event EventHandler<DataEventArgs<BinRoute>> LoadDocumentData;
        event EventHandler<DataEventArgs<MMaster>> Cbx_Etiqueta1_SelectedValue;
        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<EventArgs> CancelBasicData;
        event EventHandler<EventArgs> NewBasicData;


        #endregion

        #region Serial

        event EventHandler<EventArgs> AddLine;
        /// <summary>
        /// Nuevo metodo para seleccionar producto en "Mover Producto" por cantidades.
        /// </summary>
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        /// <summary>
        /// Guardar productos movidos en cantidad.
        /// </summary>
        event EventHandler<EventArgs> SaveProductos;

        #endregion

        #region Details

        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ReplicateDetails;
        /// <summary>
        /// Muetra la copia de la informacion de la lista de Entrada de Almacen.
        /// </summary>
        event EventHandler<EventArgs> MostrarInformacion;
        event EventHandler<EventArgs> Impresion_1;
        event EventHandler<EventArgs> Impresion_2;

        #endregion

        #endregion

    }
}