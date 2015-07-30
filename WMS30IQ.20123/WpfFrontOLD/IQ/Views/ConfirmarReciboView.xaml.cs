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
    /// Interaction logic for ConfirmarReciboView.xaml
    /// </summary>
    public partial class ConfirmarReciboView : UserControlBase, IConfirmarReciboView
    {

        #region Eventos

        public event EventHandler<EventArgs> ConfirmBasicData;
        public event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;
        public event EventHandler<EventArgs> ConfirmarMovimiento;
        public event EventHandler<EventArgs> BuscarRegistros;
        public event EventHandler<EventArgs> ActualizarLista;
        public event EventHandler<EventArgs> ConfirmarRecibo;


        #endregion

        public ConfirmarReciboView()
        {
            InitializeComponent();
            //Text_ShowHide.Text = "<< Ocultar";
        }

        #region Variables

        public ConfirmarReciboModel Model
        {
            get { return this.DataContext as ConfirmarReciboModel; }
            set { this.DataContext = value; }
        }

        public Border BorderDetails
        {
            get { return this.Border_Detail; }
            set { this.Border_Detail = value; }
        }

        public ListView ListadoEstibas
        {
            get { return this.lvDocumentMaster_2; }
            set { this.lvDocumentMaster_2 = value; }
        }

        public ComboBox CboUbicaciones
        {
            get { return this.cb_Ubicaciones; }
            set { this.cb_Ubicaciones = value; }
        }

        public ComboBox NuevaUbicacion
        {
            get { return this.cb_NuevaUbicacion; }
            set { this.cb_NuevaUbicacion = value; }
        }

        public TextBox Txt_Serial
        {
            get { return this.txt_Serial; }
            set { this.txt_Serial = value; }
        }


        #endregion

        #region Metodos


        private void btn_Buscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarRegistros(sender, e);
        }

        private void btn_Actualizar_Click(object sender, RoutedEventArgs e)
        {
            ActualizarLista(sender, e);
        }

        private void btn_Confirmar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarRecibo(sender, e);
        }

        private void CheckBox_Checked_HEADER(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Model.ListRecords_1.Rows.Count; i++)
                Model.ListRecords_1.Rows[i]["Checkm"] = true;
        }

        private void CheckBox_Unchecked_HEADER(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Model.ListRecords_1.Rows.Count; i++)
                Model.ListRecords_1.Rows[i]["Checkm"] = false;
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddLine(sender, e);
            }
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
            //chkRep.IsChecked = false;
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

        #endregion

    }

    public interface IConfirmarReciboView
    {
        //Clase Modelo
        ConfirmarReciboModel Model { get; set; }

        #region Variables

        ComboBox CboUbicaciones { get; set; }
        ComboBox NuevaUbicacion { get; set; }
        TextBox Txt_Serial { get; set; }
        Border BorderDetails { get; set; }
        ListView ListadoEstibas { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> ConfirmBasicData;
        event EventHandler<DataEventArgs<Product>> EvaluarTipoProducto;
        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;
        event EventHandler<EventArgs> ConfirmarMovimiento;
        event EventHandler<EventArgs> ActualizarLista;
        event EventHandler<EventArgs> BuscarRegistros;
        event EventHandler<EventArgs> ConfirmarRecibo;

        #endregion

    }
}