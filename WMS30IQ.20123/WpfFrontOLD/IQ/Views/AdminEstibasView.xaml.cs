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

namespace WpfFront.IQ.Views
{
    /// <summary>
    /// Interaction logic for AdminEstibasView.xaml
    /// </summary>
    public partial class AdminEstibasView : UserControlBase, IAdminEstibasView
    {
        public AdminEstibasView()
        {
            InitializeComponent();
        }

        public AdminEstibasModel Model
        {
            get { return this.DataContext as AdminEstibasModel; }
            set { this.DataContext = value; }
        }
    }
    public interface IAdminEstibasView
    {
        //Clase Modelo
        AdminEstibasModel Model { get; set; }

        #region Variables

       
        #endregion

        #region Obtener Metodos
       

        #endregion

    }
}
