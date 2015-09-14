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

    public partial class NovedadesView : UserControlBase, INovedadesView
    {

        #region Eventos

        public event EventHandler<EventArgs> VerEquiposNovedades;
        public event EventHandler<EventArgs> ExportPrealertas;
        public event EventHandler<EventArgs> ExportNovedadTipoA;
        public event EventHandler<EventArgs> ExportNovedadTipoB;
        public event EventHandler<EventArgs> BuscarPrealertas;
        //public event EventHandler<EventArgs> BuscarNoveTipoB;

        #endregion

        public NovedadesView()
        {
            InitializeComponent();
        }

        #region Variables

        public NovedadesModel Model
        {
            get { return this.DataContext as NovedadesModel; }
            set { this.DataContext = value; }
        }

        public ListView ListadoBusquedaPrealertas
        {
            get { return this.lv_ListadoBusquedaPrealertas; }
            set { this.lv_ListadoBusquedaPrealertas = value; }
        }

        public ListView ListadoBusquedaNovedadesTipoA
        {
            get { return this.lv_PreaNovedadTipoA; }
            set { this.lv_PreaNovedadTipoA = value; }
        }

        public ListView ListadoBusquedaNovedadesTipoB
        {
            get { return this.lv_PreaNovedadTipoB; }
            set { this.lv_PreaNovedadTipoB = value; }
        }

        public GridView GridViewListaNovedadA
        {
            get { return this.GridViewDetailsNoveA; }
            set { this.GridViewDetailsNoveA = value; }
        }

        public GridView GridViewListaNovedadB
        {
            get { return this.GridViewDetailsNoveB; }
            set { this.GridViewDetailsNoveB = value; }
        }

        public GridView GridViewListaPrealertas
        {
            get { return this.GridViewDetails_Prealerta; }
            set { this.GridViewDetails_Prealerta = value; }
        }

        public ComboBox ComboArchivos
        {
            get { return this.cb_BuscarArchivoPrealerta; }
            set { this.cb_BuscarArchivoPrealerta = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaEmitido
        {
            get { return this.cb_BuscarFechaEmitido; }
            set { this.cb_BuscarFechaEmitido = value; }
        }

        public System.Windows.Controls.DatePicker GetFechaRegistro
        {
            get { return this.cb_BuscarFechaRegistro; }
            set { this.cb_BuscarFechaRegistro = value; }
        }

        //public System.Windows.Controls.DatePicker GetFechaRegistroNoveTipoB
        //{
        //    get { return this.cb_BuscarFechaRegistroNoveTipoB; }
        //    set { this.cb_BuscarFechaRegistroNoveTipoB = value; }
        //}

        #endregion

        #region Metodos


        private void lv_ListadoBusquedaPrealertas_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            VerEquiposNovedades(sender, e);
        }

        private void Btn_ExportarPrealertas_Click_1(object sender, RoutedEventArgs e)
        {
            ExportPrealertas(sender, e);
        }

        private void Btn_ExportarNovedadA_Click_1(object sender, RoutedEventArgs e)
        {
            ExportNovedadTipoA(sender, e);
        }

        private void Btn_ExportarNovedadB_Click_1(object sender, RoutedEventArgs e)
        {
            ExportNovedadTipoB(sender, e);
        }

        private void btn_BuscarPrealertas_Click_1(object sender, RoutedEventArgs e)
        {
            BuscarPrealertas(sender, e);
        }

        //private void btn_BuscarNoveTipoB_Click_1(object sender, RoutedEventArgs e)
        //{
        //    BuscarNoveTipoB(sender, e);
        //}

        #endregion

    }

    public interface INovedadesView
    {
        //Clase Modelo
        NovedadesModel Model { get; set; }

        #region Variables

        ListView ListadoBusquedaPrealertas { get; set; }
        ListView ListadoBusquedaNovedadesTipoA { get; set; }
        ListView ListadoBusquedaNovedadesTipoB { get; set; }

        GridView GridViewListaPrealertas { get; set; }
        GridView GridViewListaNovedadA { get; set; }
        GridView GridViewListaNovedadB { get; set; }

        ComboBox ComboArchivos { get; set; }
        System.Windows.Controls.DatePicker GetFechaEmitido { get; set; }
        System.Windows.Controls.DatePicker GetFechaRegistro { get; set; }
       // System.Windows.Controls.DatePicker GetFechaRegistroNoveTipoB { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> VerEquiposNovedades;
        event EventHandler<EventArgs> ExportPrealertas;
        event EventHandler<EventArgs> ExportNovedadTipoA;
        event EventHandler<EventArgs> ExportNovedadTipoB;
        event EventHandler<EventArgs> BuscarPrealertas;
        //event EventHandler<EventArgs> BuscarNoveTipoB;

        #endregion

    }
}