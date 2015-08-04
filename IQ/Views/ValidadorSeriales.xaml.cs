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
    /// Interaction logic for ValidadorSerialesView.xaml
    /// </summary>
    public partial class ValidadorSerialesView : UserControlBase, IValidadorSerialesView
    {
        #region Header

        /// <summary>
        /// Buscar los seriales con los parametros establacidos al principio;
        /// los parametros son: Cliente, Fecha Desde, Fecha Hasta y Numero del Documento.
        /// </summary>
        public event EventHandler<EventArgs> BuscarSeriales;

        #endregion

        #region Details

        /// <summary>
        /// Eliminar los seriales, la idea es recorrer solo lo seriales seleccionados de la lista
        /// y pasarle como parametros los datos a eliminar de las determinadas tablas.
        /// </summary>
        public event EventHandler<EventArgs> EliminarSeriales;

        #endregion

        public ValidadorSerialesView()
        {
            InitializeComponent();
        }

        public ValidadorSerialesModel Model
        {
            get
            { return this.DataContext as ValidadorSerialesModel; }
            set
            { this.DataContext = value; }
        }

        #region Variables

        #region Header

        public ComboBox GetListLocationFrom
        {
            get { return this.cb_LocationFrom; }
            set { this.cb_LocationFrom = value; }
        }

        /// <summary>
        /// btn_BuscarSeriales
        /// </summary>
        public Button GetButtonBuscar
        {
            get { return this.btn_BuscarSeriales; }
            set { this.btn_BuscarSeriales = value; }
        }

        /// <summary>
        /// Eliminar seriales "Boton".
        /// </summary>
        public Button GetButtonEliminar
        {
            get { return this.Btn_Eliminar_Seriales; }
            set { this.Btn_Eliminar_Seriales = value; }
        }

        /// <summary>
        /// Parametro 1 = Cliente.
        /// </summary>
        public ComboBox comboCliente
        {
            get { return this.cb_LocationFrom; }
            set { this.cb_LocationFrom = value; }
        }

        /// <summary>
        /// Parametro 2 = Fecha Desde
        /// </summary>
        public Microsoft.Windows.Controls.DatePicker _FechaDesde
        {
            get { return this.FechaDesde; }
            set { this.FechaDesde = value; }
        }

        /// <summary>
        /// Parametro 3 = Fecha Hasta
        /// </summary>
        public Microsoft.Windows.Controls.DatePicker _FechaHasta
        {
            get { return this.FechaHasta; }
            set { this.FechaHasta = value; }
        }

        /// <summary>
        /// Parametro 4 = Numero del documento.
        /// </summary>
        public TextBox NumeroDocumento
        {
            get { return this.txt_NoDocumento; }
            set { this.txt_NoDocumento = value; }
        }

        #endregion

        #region Details

        public ListView LvDocumentMaster
        {
            get { return this.lvDocumentMaster; }
            set { this.lvDocumentMaster = value; }
        }

        /// <summary>
        /// El gridview...
        /// </summary>
        public GridView GetGridViewDetails
        {
            get { return this.GridViewDetails; }
            set { this.GridViewDetails = value; }
        }

        /// <summary>
        /// Border 2 "Mostrar resultados"
        /// </summary>
        public Border BorderDetails
        {
            get { return this.Border_2; }
            set { this.Border_2 = value; }
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

        /// <summary>
        /// Mostrar el otro campo y eliminar los seriales.
        /// Falta programar el metodo en el PRESENTER!
        /// Los parametros son Cliente y numero del documento.
        /// OK!.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_BuscarSeriales_Click_1(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Buscando registros...por favor espere...");
            //Metodo.
            BuscarSeriales(sender, e);
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            //Mostrar el Border details.
            Border_2.Visibility = Visibility.Visible;
        }

        #endregion

        #region Details

        /// <summary>
        /// Seleccionar / Deseleccionar los registros.
        /// por lo que hice, no veo necesaria mas logica.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkRep_Checked(object sender, RoutedEventArgs e)
        {
            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Seleccionando registros...por favor espere...");

            //Valida la seleccion / deseleccion de los registros.
            if (lvDocumentMaster.SelectedItems.Count >= 1)
            {
                lvDocumentMaster.UnselectAll();
                //Cierro ventana de Cargando...
                pw.Visibility = Visibility.Collapsed;
                pw.Close();
                chkRep.IsChecked = false;
            }
            else
            {
                lvDocumentMaster.SelectAll();
                //Cierro ventana de Cargando...
                pw.Visibility = Visibility.Collapsed;
                pw.Close();
                chkRep.IsChecked = false;
            }
        }

        /// <summary>
        /// Eliminar seriales de la lista.
        /// Falta poner el metodo!
        /// OK!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Eliminar_Seriales_Click_1(object sender, RoutedEventArgs e)
        {
            //Confirmo si desea eliminar el equipo
            if (!UtilWindow.ConfirmOK("Esta seguro que desea eliminar estos seriales del sistema?") == true)
                return;

            //Mostrar ventana de Cargando...
            ProcessWindow pw = new ProcessWindow("Eliminando registros...por favor espere...");
            //Metodo.
            EliminarSeriales(sender, e);
            pw.Visibility = Visibility.Collapsed;
            pw.Close();
            //Btn_Eliminar_Seriales.IsEnabled = false;
        }

        #endregion

        #endregion
    }


    /// <summary>
    /// Interfaz ValidadorSeriales
    /// </summary>
    public interface IValidadorSerialesView
    {
        //Clase Modelo
        ValidadorSerialesModel Model { get; set; }

        #region Obtener Variables

        #region Header

        ComboBox GetListLocationFrom { get; set; }

        /// <summary>
        /// btn_BuscarSeriales
        /// </summary>
        Button GetButtonBuscar { get; set; }

        /// <summary>
        /// Eliminar seriales "Boton".
        /// </summary>
        Button GetButtonEliminar { get; set; }

        /// <summary>
        /// Parametro 1 = Cliente.
        /// </summary>
        ComboBox comboCliente { get; set; } //cb_LocationFrom

        /// <summary>
        /// Parametro 2 = Fecha Desde
        /// </summary>
        Microsoft.Windows.Controls.DatePicker _FechaDesde { get; set; } //FechaDesde

        /// <summary>
        /// Parametro 3 = Fecha Hasta
        /// </summary>
        Microsoft.Windows.Controls.DatePicker _FechaHasta { get; set; } //FechaHasta

        /// <summary>
        /// Parametro 4 = Numero del documento.
        /// </summary>
        TextBox NumeroDocumento { get; set; } //txt_NoDocumento

        #endregion

        #region Details

        ListView LvDocumentMaster { get; set; }
        /// <summary>
        /// El gridview...
        /// </summary>
        GridView GetGridViewDetails { get; set; }
        /// <summary>
        /// Border 2 "Mostrar resultados"
        /// </summary>
        Border BorderDetails { get; set; }
        TextBlock RecuentoFilas { get; set; }
        TextBlock RegistrosNoCargados { get; set; }

        #endregion

        #endregion

        #region Obtener Metodos

        #region Header

        /// <summary>
        /// Buscar los seriales con los parametros establacidos al principio;
        /// los parametros son: Cliente, Fecha Desde, Fecha Hasta y Numero del Documento.
        /// </summary>
        event EventHandler<EventArgs> BuscarSeriales;

        #endregion

        #region Details

        /// <summary>
        /// Eliminar los seriales, la idea es recorrer solo lo seriales seleccionados de la lista
        /// y pasarle como parametros los datos a eliminar de las determinadas tablas.
        /// </summary>
        event EventHandler<EventArgs> EliminarSeriales;

        #endregion

        #endregion

    }
}