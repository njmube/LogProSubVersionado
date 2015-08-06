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

    public partial class NovedadesDTVView : UserControlBase, INovedadesDTVView
    {

        #region Eventos

        public event EventHandler<EventArgs> ExportNovedades;
        public event EventHandler<EventArgs> ActualizarListNovedades;

        #endregion

        public NovedadesDTVView()
        {
            InitializeComponent();
        }

        #region Variables

        public NovedadesDTVModel Model
        {
            get { return this.DataContext as NovedadesDTVModel; }
            set { this.DataContext = value; }
        }

        public ListView ListadoBusquedaNovedades
        {
            get { return this.lv_ListadoBusquedaNovedad; }
            set { this.lv_ListadoBusquedaNovedad = value; }
        }

        public GridView GridViewListaNovedades
        {
            get { return this.GridViewDetails_Novedad; }
            set { this.GridViewDetails_Novedad = value; }
        }

        #endregion

        #region Metodos

        private void Btn_ExportarNovedades_Click_1(object sender, RoutedEventArgs e)
        {
            ExportNovedades(sender, e);
        }

        private void ImgActualizar(object sender, RoutedEventArgs e)
        {
            ActualizarListNovedades(sender, e);
        }

        #endregion

    }

    public interface INovedadesDTVView
    {
        NovedadesDTVModel Model { get; set; }

        #region Variables

        ListView ListadoBusquedaNovedades { get; set; }
        GridView GridViewListaNovedades { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ExportNovedades;
        event EventHandler<EventArgs> ActualizarListNovedades;

        #endregion

    }
}