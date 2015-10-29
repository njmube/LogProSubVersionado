using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core.WPF;
using WpfFront.Common.UserControls;
using WpfFront.IQ.Models;
using WpfFront.Models;
using WpfFront.Common;

namespace WpfFront.IQ.Views
{
    /// <summary>
    /// Interaction logic for AdminEstibasView.xaml
    /// </summary>
    public partial class AdminEstibasView : UserControlBase, IAdminEstibasView
    {
        #region Eventos
        #region Combinar estibas

        public event EventHandler<EventArgs> CombinarEstibas;
        public event EventHandler<EventArgs> GenerarEstiba;

        #endregion

        #region Adición seriales 1 a 1

        public event EventHandler<KeyEventArgs> AddLine;
        public event EventHandler<EventArgs> AnadirSeriales;
        public event EventHandler<EventArgs> RemoveItemsSelected;

        #endregion
        #endregion
        public AdminEstibasView()
        {
            InitializeComponent();
        }

        #region Variables get and Set

        public AdminEstibasModel Model
        {
            get { return this.DataContext as AdminEstibasModel; }
            set { this.DataContext = value; }
        }
        #region Actualización estibas
        public TextBox tbCodigoPrincipal
        {
            get { return this.tb_pallet; }
            set { this.tb_pallet = value; }
        }
        public ListView ListViewListadoPallets
        {
            get { return this.lv_ListadoPallets; }
            set { this.lv_ListadoPallets = value; }
        }
        public Button BtnBuscar
        {
            get { return this.btnBuscar; }
            set { this.btnBuscar = value; }
        }
        public TextBlock txt_RecuentoSeriales
        {
            get { return this.txt_RecuentoSerialesSeleccionados; }
            set { this.txt_RecuentoSerialesSeleccionados = value; }
        }
        public TextBlock txt_RecuentoEstibas
        {
            get { return this.txt_RecuentoEstibasSeleccionadas; }
            set { this.txt_RecuentoEstibasSeleccionadas = value; }
        }
        public ListView lv_serialesXPalletSeleccionado
        {
            get { return this.lv_serialesXPallet; }
            set { this.lv_serialesXPallet = value; }
        }
        public GridView Gv_SerialesXPalletSeleccionado
        {
            get { return this.GridViewSerialesXPallet; }
            set { this.GridViewSerialesXPallet = value; }
        }
        public TextBlock Txt_recuentoTotalEquipos
        {
            get { return this.txt_recuentoTotalEquipos; }
            set { this.txt_recuentoTotalEquipos = value; }
        }
        /////////// ZONA DE ACTUALIZACIÓN DE ESTIBAS ////////////
        public ComboBox cbo_Ubicacion
        {
            get { return this.cboUbicacion; }
            set { this.cboUbicacion = value; }
        }
        public ComboBox cbo_Estado
        {
            get { return this.cboEstado; }
            set { this.cboEstado = value; }
        }
        public ComboBox cbo_Posicion
        {
            get { return this.cboPosicion; }
            set { this.cboPosicion = value; }
        }
        public Image Btn_ActualizarinformacionEstibas
        {
            get { return this.img_ActualizarInformacionEstibas; }
            set { this.img_ActualizarInformacionEstibas = value; }
        }    
        #endregion
        #region Unión de estibas

        public ComboBox CBO_UbicacionUnionEstibas
        {
            get { return this.cbo_UbicacionUnionEstibas; }
            set { this.cbo_UbicacionUnionEstibas = value; }
        }

        public TextBlock TXT_seleccionarUbicacion
        {
            get { return this.txt_SeleccionarUbicacion; }
            set { this.txt_SeleccionarUbicacion = value; }
        }

        public TextBox TXT_palletGeneratedUnionEstibas
        {
            get { return this.txt_palletGeneratedUnionEstibas; }
            set { this.txt_palletGeneratedUnionEstibas = value; }
        }

        public Button BTN_CombinarEstibas
        {
            get { return this.btn_CombinarEstibas; }
            set { this.btn_CombinarEstibas = value; }
        }

        #endregion

        #region Adicion seriales 1 a 1

        public TextBox TXT_serialAdicionSeriales
        {
            get { return this.txt_serialAdicionSeriales; }
            set { this.txt_serialAdicionSeriales = value; }
        }

        public ListView LV_serialesOneByOne
        {
            get { return this.lv_serialesOneByOne; }
            set { this.lv_serialesOneByOne = value; }
        }

        public Button BTN_RemoverSerial1a1
        {
            get { return this.btn_RemoverSerial1a1; }
            set { this.btn_RemoverSerial1a1 = value; }
        }

        public Button BTN_AnadirSeriales1a1
        {
            get { return this.btn_AnadirSeriales1a1; }
            set { this.btn_AnadirSeriales1a1 = value; }
        }
        #endregion
        #endregion

        #region Metodos



        #region Union de estibas

        private void btn_CombinarEstibas_Click_1(object sender, RoutedEventArgs e)
        {
            CombinarEstibas(sender, e);
        }

        private void ImageRefreshUnionEstiba_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            GenerarEstiba(sender, e);
        }

        private void cbo_UbicacionUnionEstibas_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            this.txt_SeleccionarUbicacion.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Adicion de seriales 1 a 1

        private void txt_serialAdicionSeriales_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                AddLine(sender, e);
            }
            contentButton();
        }

        public void contentButton()
        {
            int numeroSeriales = lv_serialesOneByOne.Items.Count;
            string content = "";
            if (numeroSeriales <= 0)
            {
                content = "Registre un serial...";
                btn_AnadirSeriales1a1.IsEnabled = false;
                btn_RemoverSerial1a1.IsEnabled = false;
            }

            else if (numeroSeriales == 1)
            {
                content = "Añadir serial al pallet seleccionado";
                btn_AnadirSeriales1a1.IsEnabled = true;
                btn_RemoverSerial1a1.IsEnabled = true;
            }
            else
            {
                content = "Añadir " + numeroSeriales.ToString() + " seriales al pallet seleccionado";
                btn_AnadirSeriales1a1.IsEnabled = true;
                btn_RemoverSerial1a1.IsEnabled = true;
            }
            this.btn_AnadirSeriales1a1.Content = content;
        }

        private void btn_RemoverSerial1a1_Click_1(object sender, RoutedEventArgs e)
        {
            RemoveItemsSelected(sender, e);
            contentButton();
        }
        private void btn_AnadirSeriales1a1_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.lv_ListadoPallets.SelectedItems.Count == 1)
                if (this.lv_ListadoPallets.SelectedItems.Count > 1)
                    Util.ShowMessage("No puede seleccionar mas de una estiba.");
                else
                    AnadirSeriales(sender, e);
            else
                Util.ShowMessage("Por favor seleccionar una estiba.");
        }

        #endregion

        #endregion

    }
    public interface IAdminEstibasView
    {
        //Clase Modelo
        AdminEstibasModel Model { get; set; }

        #region Variables
        ListView ListViewListadoPallets { get; set; }

        #region Union de Estibas

        ComboBox CBO_UbicacionUnionEstibas { get; set; }
        TextBlock TXT_seleccionarUbicacion { get; set; }
        TextBox TXT_palletGeneratedUnionEstibas { get; set; }
        Button BTN_CombinarEstibas { get; set; }

        #endregion

        #region Adicion seriales 1 a 1

        TextBox TXT_serialAdicionSeriales { get; set; }
        ListView LV_serialesOneByOne { get; set; }
        Button BTN_RemoverSerial1a1 { get; set; }
        Button BTN_AnadirSeriales1a1 { get; set; }
        #endregion

        #endregion
        

        #region Obtener Metodos

        #region Combinar Estibas

        event EventHandler<EventArgs> CombinarEstibas;
        event EventHandler<EventArgs> GenerarEstiba;

        #endregion

        #region Adición de seriales 1 a 1

        event EventHandler<KeyEventArgs> AddLine;
        event EventHandler<EventArgs> AnadirSeriales;
        event EventHandler<EventArgs> RemoveItemsSelected;

        #endregion

        #endregion
    }
}