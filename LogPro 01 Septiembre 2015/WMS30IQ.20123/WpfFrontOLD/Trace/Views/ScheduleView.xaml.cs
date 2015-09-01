using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using WpfFront.Common;
using Microsoft.Windows.Controls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    /// 

    public interface IScheduleView
    {
        //Clase Modelo
        ScheduleModel Model { get; set; }

        DataGridControl ListRecords { get; }
        StackPanel StkEdit { get; }
        Button BtnDelete { get; }
        Microsoft.Windows.Controls.DatePicker TxtSchDateFrom { get; set; }
        Microsoft.Windows.Controls.DatePicker TxtSchDateTo { get; set; }
        Microsoft.Windows.Controls.DatePicker TxtSchNextDate { get; set; }
        StackPanel StkButtons { get; set; }

        //event EventHandler<DataEventArgs<string>> LoadSearch;
        //event EventHandler<EventArgs> New;
        event EventHandler<DataEventArgs<CountSchedule>> LoadData;
        event EventHandler<EventArgs> Save;
        event EventHandler<EventArgs> Delete;

    }
    public partial class ScheduleView : UserControlBase, IScheduleView
    {
        public ScheduleView()
        {
            InitializeComponent();
        }

        //View Events
       // public event EventHandler<DataEventArgs<string>> LoadSearch;
       // public event EventHandler<EventArgs> New;
        public event EventHandler<DataEventArgs<CountSchedule>> LoadData;
        public event EventHandler<EventArgs> Save;
        public event EventHandler<EventArgs> Delete;

        public ScheduleModel Model
        {
            get
            { return this.DataContext as ScheduleModel; }
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


        public Microsoft.Windows.Controls.DatePicker TxtSchDateFrom
        {
            get { return this.txtSchDateFrom; }
            set
            { this.txtSchDateFrom = value; }
        }

        public Microsoft.Windows.Controls.DatePicker TxtSchDateTo
        {
            get { return this.txtSchDateTo; }
            set
            { this.txtSchDateTo = value; }
        }

        public Microsoft.Windows.Controls.DatePicker TxtSchNextDate
        {
            get { return this.txtSchNextDate; }
            set
            { this.txtSchNextDate = value; }
        }

        public StackPanel StkButtons
        {
            get { return this.stkButtons; }
            set
            { this.stkButtons = value; }
        }

        #endregion

        #region ViewEvents

        private void dgList_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadData(sender, new DataEventArgs<CountSchedule>((CountSchedule)dgList.SelectedItem));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // validations
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                Util.ShowError("Title is required");
                return;
            }

            if (string.IsNullOrEmpty(txtSchDateFrom.Text))
            {
                Util.ShowError("Start date is required");
                return;
            }
            if (string.IsNullOrEmpty(txtSchDateTo.Text))
            {
                Util.ShowError("Final date is required");
                return;
            }
            if (string.IsNullOrEmpty(txtSchNextDate.Text))
            {
                Util.ShowError("Next date is required");
                return;
            }
            if (string.IsNullOrEmpty(txtFrecuency.Text))
            {
                Util.ShowError("Frecuency field is required");
                return;
            }
            if (txtSchDateFrom.SelectedDate >= txtSchDateTo.SelectedDate)
            {
                Util.ShowError("Finish date must be older than Start date");
                return;
            }

            if (txtSchNextDate.SelectedDate< txtSchDateFrom.SelectedDate || txtSchNextDate.SelectedDate > txtSchDateTo.SelectedDate || txtSchNextDate.SelectedDate < DateTime.Today)
            {
                Util.ShowError("Next date must be setted between start and finish date and older than today");
                return;
            }
            //
            Save(sender, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(sender, e);
        }

        #endregion

    }
}
