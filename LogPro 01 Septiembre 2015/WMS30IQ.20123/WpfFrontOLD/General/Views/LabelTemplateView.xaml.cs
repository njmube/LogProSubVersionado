using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for LabelTemplateView.xaml
    /// </summary>
    public partial class LabelTemplateView : UserControlBase, ILabelTemplateView
    {
        public LabelTemplateView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<LabelTemplate>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

         public LabelTemplateModel Model
        {
            get
            { return this.DataContext as LabelTemplateModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

         public DataGridControl ListRecords 
            { get { return this.dgList; } }

        public StackPanel StkEdit 
            { get { return this.stkEdit; } }

        public Button BtnDelete
            { get { return this.btnDelete; } }

        #endregion




        #region ViewEvents

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSearch(sender, new DataEventArgs<string>(((TextBox)sender).Text));
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
        }


        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<LabelTemplate>((LabelTemplate)dgList.SelectedItem));

        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        #endregion

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem == null)
                return;

            this.chkIsUnique.Visibility = Visibility.Collapsed;
            DocumentType lblType = ((ComboBox)sender).SelectedItem as DocumentType;
            if (lblType.DocTypeID == LabelType.ProductLabel)
                this.chkIsUnique.Visibility = Visibility.Visible;
        }





    }



    public interface ILabelTemplateView
    {
        //Clase Modelo
        LabelTemplateModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }


        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<LabelTemplate>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;
    }
}