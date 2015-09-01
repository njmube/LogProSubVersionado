using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;
using WpfFront.IQ.Models;

namespace WpfFront.IQ.Views
{
    /// <summary>
    /// Lógica de interacción para MensajeCargueView.xaml
    /// </summary>
    public partial class MensajeCargueView : Window, IDialogoSerialesView
    {
        public MensajeCargueView()
        {
            InitializeComponent();
        }

        public void mostrar(List<String> ALRepetidos, List<String> ALRepetidosBD)
        {
            LV_Repetidos.Visibility = Visibility.Hidden;
            //BLV_sapCode.Visibility = Visibility.Hidden;
            //textblock_recuento_sap.Visibility = Visibility.Hidden;
            textblock_recuento_filas2.Visibility = Visibility.Hidden;
           
            DataTable dt = new DataTable();
            DataRow dr = dt.NewRow();

            if (ALRepetidosBD.Count > 0)
            {
                LV_Repetidos.Visibility = Visibility.Visible;
                textblock_recuento_filas2.Visibility = Visibility.Visible;
                textblock_recuento_filas2.Text = "Filas repetidas: " + ALRepetidos.Count.ToString();

                foreach (var i in ALRepetidosBD)
                {
                    //String[] words = i.Split(',');
                    
                    //MessageBox.Show(words[0] + " --- " + words[1]);
                    listView1.Items.Add(i);
                }
                //listView1.SelectedIndex = listView1.Items.Count - 1;               
            }

            //if (ALCodigoSAP.Count > 0)
            //{
            //    //BLV_sapCode.Visibility = Visibility.Visible;
            //    //textblock_recuento_sap.Visibility = Visibility.Visible;
            //    //textblock_recuento_sap.Text = "SAP no coincidentes: " + ALCodigoSAP.Count.ToString();

            //    foreach (var i in ALCodigoSAP)
            //    {
            //      //  LV_Repetidos_sap.Items.Add(i);
            //    }
            //}

            //if (ALRepetidosBD.Count > 0)
            //{
            //    //BLV_RepetidosBD.Visibility = Visibility.Visible;
            //    //textblock_countBD.Visibility = Visibility.Visible;
            //    //textblock_countBD.Text = "Seriales en la BD: " + ALRepetidosBD.Count.ToString();

            //    foreach (var i in ALRepetidosBD)
            //    {
            //        //LV_RepetidosBd.Items.Add(i);
            //    }
            //}
        }

        public GridView ListadoEquiposDialogo
        {
            get { return this.GridViewDetails_Dialog; }
            set { this.GridViewDetails_Dialog = value; }
        }
    }

    public interface IDialogoSerialesView
    {
        GridView ListadoEquiposDialogo { get; set; }
    }
}
