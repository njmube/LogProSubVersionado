using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ClassEntityView.xaml
    /// </summary>
    public partial class IntroView : UserControlBase, IIntroView
    {
        public IntroView()
        {
            InitializeComponent();
        }


         public IntroModel Model
        {
            get
            { return this.DataContext as IntroModel; }
            set
            { this.DataContext = value; }

        }




    }



    public interface IIntroView
    {
        //Clase Modelo
        IntroModel Model { get; set; }

    }
}