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
    /// Interaction logic for ImpresionEtiquetasView.xaml
    /// </summary>
    public partial class ImpresionEtiquetasView : UserControlBase, IImpresionEtiquetasView
    {

        #region Busqueda

        public event EventHandler<EventArgs> LoadData;

        #endregion

        #region Eventos Botones

        public event EventHandler<EventArgs> Delete;
        public event EventHandler<EventArgs> Save;

        #endregion


        public ImpresionEtiquetasView()
        {
            InitializeComponent();
        }

        #region Variables

        public ImpresionEtiquetasModel Model
        {
            get
            { return this.DataContext as ImpresionEtiquetasModel; }
            set
            { this.DataContext = value; }
        }

        #region Busqueda

        public ComboBox GetTipoBusqueda
        {
            get { return this.cb_Tipo; }
            set { this.cb_Tipo = value; }
        }

        public TextBox GetNumeroBusqueda
        {
            get { return this.tb_NumeroBusqueda; }
            set { this.tb_NumeroBusqueda = value; }
        }

        public ListView GetSerialesEscaneados
        {
            get { return this.lvSerialesScaneados; }
            set { this.lvSerialesScaneados = value; }
        }

        #endregion

        #region Eventos Botones

        #endregion

        #endregion

        #region Metodos

        #region Busqueda

        private void tb_NumeroBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Evaluo si fue seleccionado una etiqueta
                if (GetTipoBusqueda.SelectedIndex == -1)
                {
                    Util.ShowError("Por favor seleccionar una etiqueta");
                    return;
                }

                //Evaluo si fue digitado o scaneado el serial
                if (String.IsNullOrEmpty(GetNumeroBusqueda.Text))
                    return;

                LoadData(sender, e);
            }
        }

        #endregion

        #region Eventos Botones

        private void Btn_Eliminar_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        #endregion

        #endregion

    }

    public interface IImpresionEtiquetasView
    {
        //Clase Modelo
        ImpresionEtiquetasModel Model { get; set; }

        #region Obtener Variables

        #region Busqueda

        ComboBox GetTipoBusqueda { get; set; }
        TextBox GetNumeroBusqueda { get; set; }
        ListView GetSerialesEscaneados { get; set; }

        #endregion

        #region Eventos Botones

        #endregion

        #endregion

        #region Obtener Metodos

        #region Busqueda

        event EventHandler<EventArgs> LoadData;

        #endregion

        #region Eventos Botones

        event EventHandler<EventArgs> Delete;
        event EventHandler<EventArgs> Save;

        #endregion

        #endregion

    }
}