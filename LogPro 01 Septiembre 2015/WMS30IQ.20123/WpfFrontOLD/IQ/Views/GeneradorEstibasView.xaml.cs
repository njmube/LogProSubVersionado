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
using System.Drawing;
using System.Drawing.Text;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GeneradorEstibasView.xaml
    /// </summary>
    public partial class GeneradorEstibasView : UserControlBase, IGeneradorEstibasView
    {

        #region Eventos

        public event EventHandler<EventArgs> AddLine;
        public event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        public event EventHandler<EventArgs> ReplicateDetails;
        public event EventHandler<EventArgs> SaveDetails;

        #endregion

        public GeneradorEstibasView()
        {
            InitializeComponent();           
        }

        #region Variables

        public GeneradorEstibasModel Model
        {
            get
            { return this.DataContext as GeneradorEstibasModel; }
            set
            { this.DataContext = value; }
        }


        //public StackPanel GetStackUploadFile
        //{
        //    get { return this.Stack_UploadFile; }
        //    set { this.Stack_UploadFile = value; }
        //}

        public TextBox NumeroEstiba
        {
            get { return this.tb_NoEstiba; }
            set { this.tb_NoEstiba = value; }
        }

        public TextBlock BarCode
        {
            get { return this.Text_block2; }
            set { this.Text_block2 = value; }
        }

        //

        //public TextBox GetSerial2
        //{
        //    get { return this.tb_Serial2; }
        //    set { this.tb_Serial2 = value; }
        //}

        //public UploadFile GetUpLoadFile
        //{
        //    get { return this.fUpload; }
        //    set { this.fUpload = value; }
        //}

        //public GridView ListadoEquipos
        //{
        //    get { return this.GridViewDetails; }
        //    set { this.GridViewDetails = value; }
        //}

        #endregion

        #region Metodos

        private void tb_Serial1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //Paso el focus al siguiente campo de serial
                //GetSerial2.Focus();
            }
        }

        private void tb_Serial2_KeyDown_1(object sender, KeyEventArgs e)
        {
            //Evaluo si la tecla es un Enter
            if (e.Key == Key.Enter)
            {
                //Adiciono el registro al listado
                AddLine(sender, e);
            }
        }

        private void fUpload_OnFileUpload_1(object sender, EventArgs e)
        {
            ////Mostrar ventana de Cargando...
            //ProcessWindow pw = new ProcessWindow("Cargando registros...por favor espere...");
            ////Procesar el Archivo Cargado
            //if (fUpload.StreamFile != null)
            //{
            //    string dataFile = Util.GetPlainTextString(fUpload.StreamFile);

            //    ProcessFile1(sender, e, dataFile);
            //}
            ////Cierro ventana de Cargando...
            //pw.Visibility = Visibility.Collapsed;
            //pw.Close();
        }

        private void ProcessFile(object sender, EventArgs e, string dataFile)
        {

            ////Linea por linea obtiene el dato del serial y del RR y lo procesa.
            ////Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            //DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            //if (lines == null || lines.Rows.Count == 0)
            //{
            //    Util.ShowError("No hay registros a procesar.\n" + dataFile);
            //    return;
            //}

            //int NumeroSerial;
            //foreach (DataRow dr in lines.Rows)
            //{
            //    NumeroSerial = 1;
            //    foreach (DataColumn dc in lines.Columns)
            //    {
            //        switch (NumeroSerial.ToString())
            //        {
            //            case "1":
            //                GetSerial1.Text = dr[dc.ColumnName].ToString();
            //                break;
            //            case "2":
            //                GetSerial2.Text = dr[dc.ColumnName].ToString();
            //                break;
            //        }
            //        NumeroSerial++;
            //    }

            //    AddLine(sender, e);
            //}

            //fUpload.StreamFile = null;
        }

        private void ProcessFile1(object sender, EventArgs e, string dataFile)
        {

            ////Linea por linea obtiene el dato del serial y del RR y lo procesa.
            ////Obtiene los errores de procesamiento, muestra los que el RR no es IQRE

            //DataTable lines = Util.ConvertToDataTable(dataFile, "RR", "\t", false);

            //if (lines == null || lines.Rows.Count == 0)
            //{
            //    Util.ShowError("No hay registros a procesar.\n" + dataFile);
            //    return;
            //}

            //CargaMasiva(sender, new DataEventArgs<DataTable>(lines));

            //fUpload.StreamFile = null;
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
           
            
            //FontFamily family;
            //PrivateFontCollection pfc;

            //pfc = new PrivateFontCollection();

            //pfc.AddFontFile("C:\\Documents and Settings\\farmacia\\Escritorio\\FRE3OF9X.TTF");
            //family = pfc.Families[0];
           
            //BarCode = new Font(family, 40f);
            //BarCode.Text = String.Format("*{0}*", NumeroEstiba.Text);


            ////Confirmo si desea confirmar el proceso
            //if (!UtilWindow.ConfirmOK("Esta seguro de querer procesar estos registros?") == true)
            //    return;

            ////Mostrar ventana de Cargando...
            //ProcessWindow pw = new ProcessWindow("Procesando registros...por favor espere...");
            //SaveDetails(sender, e);
            ////Cierro ventana de Cargando...
            //pw.Visibility = Visibility.Collapsed;
            //pw.Close();
        }

        #endregion

    }

    public interface IGeneradorEstibasView
    {
        //Clase Modelo
        GeneradorEstibasModel Model { get; set; }

        #region Obtener Variables

        //StackPanel GetStackUploadFile { get; set; }
        //TextBox GetSerial1 { get; set; }
        TextBlock BarCode { get; set; }
        //TextBox GetSerial2 { get; set; }
        //UploadFile GetUpLoadFile { get; set; }
        //GridView ListadoEquipos { get; set; }

        #endregion

        #region Obtener Metodos

        event EventHandler<EventArgs> AddLine;
        event EventHandler<DataEventArgs<DataTable>> CargaMasiva;
        event EventHandler<EventArgs> ReplicateDetails;
        event EventHandler<EventArgs> SaveDetails;

        #endregion

    }
}