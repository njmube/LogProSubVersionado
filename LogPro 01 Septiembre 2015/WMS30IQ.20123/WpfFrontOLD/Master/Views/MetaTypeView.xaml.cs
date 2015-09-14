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
    /// Interaction logic for MetaTypeView.xaml
    /// </summary>
    public partial class MetaTypeView : UserControlBase, IMetaTypeView
    {
        public MetaTypeView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<MType>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

        public event EventHandler<EventArgs> NewDetail;
        public event EventHandler<DataEventArgs<MMaster>> LoadDataDetail;
        public event EventHandler<EventArgs> SaveDetail;
        public event EventHandler<EventArgs> DeleteDetail;

         public MetaTypeModel Model
        {
            get
            { return this.DataContext as MetaTypeModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

         public DataGridControl ListRecords 
            { get { return this.dgList; } }

         public DataGridControl ListRecordsDet
         { get { return this.dgDetail; } }

        public StackPanel StkEdit 
            { get { return this.stkEdit; } }

        public Border StkEditDet
        { get { return this.Border_det ; } }

        public Border BorderDetail
        { get { return this.Brd_Detail; } }

        public Button BtnDelete
            { get { return this.btnDelete; } }

        public Button BtnDeleteDet
        { get { return this.btnDeleteDet; } }

        public TextBox txtNameType
        { get { return this.txtName; } }

        public TextBox txtNameMaster
        { get { return this.txtNameDet; } }

        public TextBox txtOrderNum
        { get { return this.txtOrder ; } }

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

        private void btnNewDet_Click(object sender, RoutedEventArgs e)
        {
            NewDetail(sender, e);
            chkActive.IsChecked = true;
        }

        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<MType>((MType)dgList.SelectedItem));

        }

        private void dgDetail_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadDataDetail(sender, new DataEventArgs<MMaster>((MMaster)dgDetail.SelectedItem));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        private void btnSaveDet_Click(object sender, RoutedEventArgs e)
        {
            SaveDetail(sender, e);
        }

        private void btnDeleteDet_Click(object sender, RoutedEventArgs e)
        {
            DeleteDetail(sender, e);
        }

        #endregion

    }



    public interface IMetaTypeView
    {
        //Clase Modelo
        MetaTypeModel Model { get; set; }

        DataGridControl ListRecords { get; }
        DataGridControl ListRecordsDet { get; }
        StackPanel StkEdit { get; }
        Border StkEditDet { get; }
        Border BorderDetail { get; }
        Button BtnDelete { get; }
        Button BtnDeleteDet { get; }
        TextBox txtNameType { get; }
        TextBox txtNameMaster { get; }
        TextBox txtOrderNum { get; }

        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<MType>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;

        event EventHandler<EventArgs> NewDetail;
        event EventHandler<DataEventArgs<MMaster>> LoadDataDetail;
        event EventHandler<EventArgs> SaveDetail;
        event EventHandler<EventArgs> DeleteDetail;
    }
}