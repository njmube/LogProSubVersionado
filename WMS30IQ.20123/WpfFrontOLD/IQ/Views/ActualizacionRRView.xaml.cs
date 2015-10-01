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
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Threading;
using System.Diagnostics;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ActualizacionRRView.xaml
    /// </summary>
    public partial class ActualizacionRRView : UserControlBase, IActualizacionRRView
    {
        #region Eventos

        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> CargaMasiva_RR;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ClearDetails;
        public event EventHandler<EventArgs> KillProcess;
        public event EventHandler<EventArgs> ExportCuarentena;
        public event EventHandler<EventArgs> ExportNoLiberados;


        #endregion

        public ActualizacionRRView()
        {
            InitializeComponent();
            //StackPanel_2.Visibility = Visibility.Collapsed;
            //StackPanel_3.Visibility = Visibility.Collapsed;
            //StackPanel_4.Visibility = Visibility.Collapsed;
            //StackPanel_5.Visibility = Visibility.Collapsed;
            ExpCuarentena.Visibility = Visibility.Collapsed;
        }

        #region Variables

        public ActualizacionRRModel Model
        {
            get
            { return this.DataContext as ActualizacionRRModel; }
            set
            { this.DataContext = value; }
        }

        public StackPanel GetStackUploadFile
        {
            get { return this.Stack_UploadFile; }
            set { this.Stack_UploadFile = value; }
        }

        public Dispatcher Dispatcher_Liberacion
        {
            get { return this.Dispatcher; }
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
            get { return this.tb_CodPalletBusqueda; }
            set { this.tb_CodPalletBusqueda = value; }
        }

        public TextBlock GetEstado_Cargue
        {
            get { return this.textblock_estadoCargue; }
            set { this.textblock_estadoCargue = value; }
        }

        public ProgressBar Progress_CargueRR
        {
            get { return this.PBar_cargue; }
            set { this.PBar_cargue = value; }
        }


        public Dispatcher Dispatcher_RR
        {
            get { return this.Dispatcher; }
        }

        public ListView ListadoNo_Cargue
        {
            get { return this.lv_NoCargue; }
            set { this.lv_NoCargue = value; }
        }

        public ListView ListadoCuarentena
        {
            get { return this.lv_Cuarentena; }
            set { this.lv_Cuarentena = value; }
        }

        public Expander ExpCuarentena
        {
            get { return this.ExpanderCuarentena; }
            set { this.ExpanderCuarentena = value; }
        }

        public TextBlock TotalCuarentena
        {
            get { return this.txtTotal; }
            set { this.txtTotal = value; }
        }

        #endregion

        #region Metodos

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
                        CargaMasiva_RR(sender, e);
                    }
                    catch (Exception)
                    {
                        Util.ShowError("Error al cargar el archivo, revise que el formato de cargue sea correcto");
                    }

                }
            }
            fUpload.StreamFile = null;
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
            ClearDetails(sender, e);
            fUpload.IsEnabled = true;
            pw.Close();
        }

        private void Btn_Cancelar_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea confirmar el proceso
            if (!UtilWindow.ConfirmOK("Esta seguro de querer cancelar la operacion?") == true)
                return;

            ClearDetails(sender, e);

            fUpload.IsEnabled = true;
        }

        private void Btn_Exportar_Click_1(object sender, RoutedEventArgs e)
        {
            ExportCuarentena(sender, e);
        }

        private void Btn_ExportarNoliberados_Click_1(object sender, RoutedEventArgs e)
        {
            ExportNoLiberados(sender, e);
        }
        
        #endregion

    }

    public interface IActualizacionRRView
    {
        //Clase Modelo
        ActualizacionRRModel Model { get; set; }

        #region Obtener Variables

        StackPanel GetStackUploadFile { get; set; }
        Dispatcher Dispatcher_Liberacion { get; }
        UploadFile GetUpLoadFile { get; set; }
        GridView ListadoEquipos { get; set; }
        ComboBox Producto { get; set; }
        ListView ListadoEquiposAProcesar { get; set; }
        TextBox GetCodPallet { get; set; }
        //StackPanel StackInFile { get; set; }
        //StackPanel StackInDB { get; set; }
        //StackPanel StackSAP { get; set; }
        //StackPanel StackSAP_Serial { get; set; }

        ListView ListadoNo_Cargue { get; set; }
        ListView ListadoCuarentena { get; set; }
        Expander ExpCuarentena { get; set; }
        TextBlock TotalCuarentena { get; set; }
        //ListView repetidos { get; set; }
        //ListView repetidosDB { get; set; }

        TextBlock GetEstado_Cargue { get; set; }
        Dispatcher Dispatcher_RR { get; }
        ProgressBar Progress_CargueRR { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ClearDetails;
        event EventHandler<EventArgs> CargaMasiva_RR;
        event EventHandler<EventArgs> KillProcess;
        event EventHandler<EventArgs> ExportCuarentena;
        event EventHandler<EventArgs> ExportNoLiberados;

        #endregion
    }
}