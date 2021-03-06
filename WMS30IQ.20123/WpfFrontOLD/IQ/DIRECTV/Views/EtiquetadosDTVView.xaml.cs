﻿using System;
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
    /// Interaction logic for EtiquetadosDTVView.xaml
    /// </summary>
    public partial class EtiquetadosDTVView : UserControlBase, IEtiquetadosDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        public event EventHandler<EventArgs> BuscarRegistrosRecibo;
        public event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        public event EventHandler<EventArgs> ConfirmarRecibo;
        public event EventHandler<EventArgs> DeleteDetails;
        public event EventHandler<EventArgs> ImprimirEtiqueta_Individual;
        public event EventHandler<SelectionChangedEventArgs> GetNumeroCodigos;

        public event EventHandler<EventArgs> ListarEquiposSeleccion;
        #endregion

        public EtiquetadosDTVView()
        {
            InitializeComponent();
            Text_ShowHide.Text = "<< Ocultar";
            txtCantidad_Impresiones.Text = _numValue.ToString();
        }

        #region Variables

        public EtiquetadosDTVModel Model
        {
            get { return this.DataContext as EtiquetadosDTVModel; }
            set { this.DataContext = value; }
        }

        public ComboBox GetListBinInicio
        {
            get { return this.cb_BinInicio; }
            set { this.cb_BinInicio = value; }
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

        public TextBlock GetTextHideShowHeader
        {
            get { return this.Text_ShowHide; }
            set { this.Text_ShowHide = value; }
        }

        public Border GetBorderHeader
        {
            get { return this.Border_Header; }
            set { this.Border_Header = value; }
        }

        //public ListView ListadoItems
        //{
        //    get { return this.lvDocumentMaster_2; }
        //    set { this.lvDocumentMaster_2 = value; }
        //}
        //public StackPanel GetStackUploadFile
        //{
        //    get { return this.Stack_UploadFile; }
        //    set { this.Stack_UploadFile = value; }
        //}

        public Button GetButtonConfirmar
        {
            get { return this.btn_confirmar; }
            set { this.btn_confirmar = value; }
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

        public TextBox GetSmartCard
        {
            get { return this.tb_Serial3; }
            set { this.tb_Serial3 = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

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

        public ListView ListadoEquiposAProcesar
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        public TextBox GetSerialImpre_Individual
        {
            get { return this.txt_serialImp01; }
            set { this.txt_serialImp01 = value; }
        }

        public TextBox GetMacImpre_Individual
        {
            get { return this.txt_MacImp01; }
            set { this.txt_MacImp01 = value; }
        }

        public TextBox GetCantidad_Impresiones
        {
            get { return this.txtCantidad_Impresiones; }
            set { this.txtCantidad_Impresiones = value; }
        }

        public StackPanel Get_StackCodigosAdicionales
        {
            get { return this.Stack_CodigosAdicionales; }
            set { this.Stack_CodigosAdicionales = value; }
        }

        public TextBox BuscarEstibaRecibo
        {
            get { return this.tb_BuscarEstibaRecibo; }
            set { this.tb_BuscarEstibaRecibo = value; }
        }

        public ListView ListadoBusquedaCambioClasificacion
        {
            get { return this.lv_ListadoBusquedaRecibo; }
            set { this.lv_ListadoBusquedaRecibo = value; }
        }


        #endregion

        #region Metodos


        private void btn_confirmar_Click_1(object sender, RoutedEventArgs e)
        {
            ConfirmarMovimiento(sender, e);
        }

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //GetSerial1.Text = GetSerial1.Text.ToString().ToUpper();
                GetSmartCard.Focus();
            }
        }

        private void tb_Serial3_KeyDown_1(object sender, KeyEventArgs e)
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
            chkRep.IsChecked = false;
        }
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ReplicateDetailsBy_Column(sender, e);
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

        private void Btn_Eliminar_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteDetails(sender, e);
        }

        private void Btn_ImpIndividual_Click_1(object sender, EventArgs e)
        {
            ImprimirEtiqueta_Individual(sender, e);
        }

        private void cb_BuscarSticker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetNumeroCodigos(sender, e);
        }

        private int _numValue = 0;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                txtCantidad_Impresiones.Text = value.ToString();
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (NumValue > 1)
            {
                NumValue--;
            }
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtCantidad_Impresiones == null)
            {
                return;
            }
            if (!txtCantidad_Impresiones.Text.ToString().Contains("-"))
            {
                if (!int.TryParse(txtCantidad_Impresiones.Text, out _numValue))
                {
                    if (_numValue > 0)
                    {
                        txtCantidad_Impresiones.Text = _numValue.ToString();
                    }
                    else
                    {
                        txtCantidad_Impresiones.Text = "1";
                    }
                }
            }
            else
            {
                txtCantidad_Impresiones.Text = "1";
            }
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListarEquiposSeleccion(sender, e);
        }

        #endregion

    }

    public interface IEtiquetadosDTVView
    {
        //Clase Modelo
        EtiquetadosDTVModel Model { get; set; }

        #region Variables

        ComboBox GetListBinInicio { get; set; }
        //ComboBox Ubicacion { get; set; }
        //ComboBox UnidadAlmacenamiento { get; set; }
        TextBlock GetTextHideShowHeader { get; set; }
        Border GetBorderHeader { get; set; }
        GridView ListadoEquipos { get; set; }
        Button GetButtonConfirmar { get; set; }
        TextBox GetSerial1 { get; set; }
        TextBox GetSerial2 { get; set; }
        TextBox GetSmartCard { get; set; }
        //TextBox CodigoEmpaque { get; set; }
        Border BorderDetails { get; set; }
        //ListView ListadoItems { get; set; }

        //Recibo
        TextBox BuscarEstibaRecibo { get; set; }
        
        ListView ListadoEquiposAProcesar { get; set; }

        TextBox GetMacImpre_Individual { get; set; }
        TextBox GetSerialImpre_Individual { get; set; }
        TextBox GetCantidad_Impresiones { get; set; }

        StackPanel Get_StackCodigosAdicionales { get; set; }
        ListView ListadoBusquedaCambioClasificacion { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarRegistros;

        //Recibo
        event EventHandler<EventArgs> BuscarRegistrosRecibo;
        event EventHandler<EventArgs> ActualizarRegistrosRecibo;
        event EventHandler<EventArgs> ConfirmarRecibo;
        event EventHandler<RoutedEventArgs> ReplicateDetailsBy_Column;
        event EventHandler<EventArgs> DeleteDetails;
        event EventHandler<EventArgs> ImprimirEtiqueta_Individual;
        event EventHandler<SelectionChangedEventArgs> GetNumeroCodigos;

        event EventHandler<EventArgs> ListarEquiposSeleccion;

        #endregion

    }
}