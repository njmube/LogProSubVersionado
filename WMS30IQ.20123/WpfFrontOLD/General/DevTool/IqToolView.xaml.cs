using Core.WPF;
using System.Windows.Controls;
using System;
using Xceed.Wpf.DataGrid;
using System.Windows;
using System.Globalization;
using Xceed.Wpf.DataGrid.Export;
using System.IO;
using Xceed.Wpf.DataGrid.Print;
using Xceed.Wpf.DataGrid.Views;
using System.IO.Packaging;
using Xceed.Wpf.DataGrid.Settings;
using System.Xml.Serialization;
using System.Collections.Generic;
using Xceed.Wpf.DataGrid.Stats;
using WpfFront.Models;
using WpfFront.WMSBusinessService;
using WMComposite.Events;
using WpfFront.Common;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;


namespace WpfFront.Views
{

    //delegate Point GetPositionDelegate(IInputElement element);

    /// <summary>
    /// Interaction logic for Queries.xaml
    /// </summary>
    public partial class IqToolView : UserControlBase, IIqToolView
    {

        //private static ComboBox curDataType;
        int sourceIndex = -1;
        //ListView curSource = null;
        DataGridControl curSource = null;
        IqColumn selectedItem; //Actual Item Seleccionado en el selector de campos        

        public IqToolView()
        {
            m_doingInitializeComponent = true;
            InitializeComponent();
            m_doingInitializeComponent = false;
            string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;


            //this.lvSource.Height = SystemParameters.FullPrimaryScreenHeight - 320;

            lvSource.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvSource_PreviewMouseLeftButtonDown);
            //lvSource.Drop += new DragEventHandler(lvSource_Drop);

            //lvDest.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvDest_PreviewMouseLeftButtonDown);
            lvDest.Drop += new DragEventHandler(lvDest_Drop);

        }



        //Events
        public event EventHandler<DataEventArgs<IqReportTable>> SelectionData;
        public event EventHandler<DataEventArgs<IqColumn>> RemoveFromSelected;
        public event EventHandler<DataEventArgs<IqColumn>> AddToSelected;
        public event EventHandler<EventArgs> UpdateReport;


        public IIqToolModel Model
        {
            get
            {
                return this.DataContext as IIqToolModel;
            }
            set
            {
                this.DataContext = value;
            }
        }





        public StackPanel StkReport
        {
            get { return this.stkReport; }
            set { this.stkReport = value; }
        }





        #region Drag & Drop Functions

        void lvSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessDrag((DataGridControl)sender, lvDest, e.GetPosition);
        }


        private void ProcessDrag(DataGridControl lvSource, DataGridControl lvDest, GetPositionDelegate mousePoint)
        {

            sourceIndex = this.GetCurrentIndex(mousePoint, lvSource);
            curSource = lvSource;

            if (sourceIndex < 0)
                return;

            lvSource.SelectedIndex = sourceIndex;
            selectedItem = lvSource.Items[sourceIndex] as IqColumn;

            if (selectedItem == null)
                return;

            // this will create the drag "rectangle"
            DragDropEffects allowedEffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(lvSource, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                // The item was dropped into a new location,
                // so make it the new selected item.
                lvDest.SelectedItem = selectedItem;
            }
        }



        // function called during drop operation
        void lvSource_Drop(object sender, DragEventArgs e)
        {
            ProcessDrop((DataGridControl)sender, e.GetPosition);
        }


        void lvDest_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessDrag((DataGridControl)sender, lvSource, e.GetPosition);
        }


        // function called during drop operation
        void lvDest_Drop(object sender, DragEventArgs e)
        {
            ProcessDrop((DataGridControl)sender, e.GetPosition);
        }



        private void ProcessDrop(DataGridControl lvDest, GetPositionDelegate mousePoint)
        {
            if (curSource == lvDest)
                return;

            if (sourceIndex < 0)
                return;

            int index = this.GetCurrentIndex(mousePoint, lvDest);

            if (index < 0)
                index = 0; //return;



            ((IList<IqColumn>)curSource.Items.SourceCollection).Remove(selectedItem);

            if (curSource.Name == "lvDest")
                //Remueve de los seleccionados cambia el visible a false
                RemoveFromSelected(this, new DataEventArgs<IqColumn>(selectedItem));


            //Destination Operation
            if (((IList<IqReportColumn>)lvDest.Items.SourceCollection).Where(f => f.Column.ColumnId == selectedItem.ColumnId).Count() == 0)
            {
                //Add Trask By User
                if (lvDest.Name == "lvDest")
                    AddToSelected(this, new DataEventArgs<IqColumn>(selectedItem));

                //((IList<IqReportColumn>)lvDest.Items.SourceCollection).Insert(index, selectedItem);

            }

            lvDest.Items.Refresh();
            curSource.Items.Refresh();
        }




        Row GetListViewItem(int index, DataGridControl lvObject)
        {
            //if (lvObject.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            //    return null;

            //return lvObject.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
            return lvObject.GetContainerFromIndex(index) as Row;
        }

        // returns the index of the item in the ListView
        int GetCurrentIndex(GetPositionDelegate getPosition, DataGridControl lvObject)
        {
            int index = -1;
            for (int i = 0; i < lvObject.Items.Count; ++i)
            {

                Row item = GetListViewItem(i, lvObject);

                if (item != null && this.IsMouseOverTarget(item, getPosition))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }


        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }


        #endregion




        private void PropertiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                IqReportTable selected = e.AddedItems[0] as IqReportTable;
                if (selected != null)
                    Model.ReportTableSystem = selected;
            }
        }




        public bool m_doingInitializeComponent;



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //stkReport.Visibility = Visibility.Collapsed;

            if (e.AddedItems == null)
                return;


            //if (e.AddedItems != null && e.AddedItems.Count > 0)
            //{
            //    IqReport report = e.AddedItems[0] as IqReport;
            //    if (report != null)
            //    {
            //        SelectionData(sender, new DataEventArgs<IqReport>(report));
            //        //stkReport.Visibility = Visibility.Visible;
            //    }
            //}

        }

        private void cboTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
                return;


            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                IqReportTable table = e.AddedItems[0] as IqReportTable;
                if (table != null)
                {
                    SelectionData(sender, new DataEventArgs<IqReportTable>(table));
                    //stkReport.Visibility = Visibility.Visible;
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateReport(sender, e);
        }



     }


    public interface IIqToolView
    {
        IIqToolModel Model { get; set; }

        StackPanel StkReport { get; set; }

        event EventHandler<DataEventArgs<IqReportTable>> SelectionData;

        event EventHandler<DataEventArgs<IqColumn>> RemoveFromSelected;
        event EventHandler<DataEventArgs<IqColumn>> AddToSelected;
        event EventHandler<EventArgs> UpdateReport;

        
    }
}