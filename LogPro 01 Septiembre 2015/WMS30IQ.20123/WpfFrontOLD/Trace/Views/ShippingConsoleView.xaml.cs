using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using Odyssey.Controls;
using Microsoft.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;


namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    /// 
    delegate Point GetPositionDelegate(IInputElement element);

    public partial class ShippingConsoleView : UserControlBase, IShippingConsoleView
    {

        int sourceIndex = -1;
        //ListView curSource = null;
        DataGridControl curSource = null;
        Document selectedItem;

        public ShippingConsoleView()
        {
            InitializeComponent();

            lvToday.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvToday_PreviewMouseLeftButtonDown);
            lvToday.Drop += new DragEventHandler(lvToday_Drop);

            lvPickOrders.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvPickOrders_PreviewMouseLeftButtonDown);
            lvPickOrders.Drop += new DragEventHandler(lvPickOrders_Drop);

        }


        //View Events
        public event EventHandler<DataEventArgs<Document>> RemoveTaskByUser;
        public event EventHandler<DataEventArgs<Document>> AddTaskByUser;
        public event EventHandler<EventArgs> LoadPickerDocuments;


        public ShippingConsoleModel Model
        {
            get
            { return this.DataContext as ShippingConsoleModel; }
            set
            { this.DataContext = value; }

        }

        private void exToday_Expanded(object sender, RoutedEventArgs e)
        {
            exMonth.IsExpanded = false;
            exWeek.IsExpanded = false;
        }

        private void exWeek_Expanded(object sender, RoutedEventArgs e)
        {
            exMonth.IsExpanded = false;
            exToday.IsExpanded = false;
        }

        private void exMonth_Expanded(object sender, RoutedEventArgs e)
        {
            exToday.IsExpanded = false;
            exWeek.IsExpanded = false;
            
        }

        private void btnAutomatic_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cboPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserByRol selected = (UserByRol)((ComboBox)sender).SelectedItem;

            if (selected == null)
                return;

            stkPikerOrders.Visibility = Visibility.Visible;
            this.Model.CurPicker = selected.User;

            LoadPickerDocuments(sender, e);

        }



        #region Drag And Drop

        
        void lvToday_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessDrag((DataGridControl)sender, lvPickOrders, e.GetPosition);
        }

        void lvPickOrders_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessDrag((DataGridControl)sender, lvToday, e.GetPosition);
        }


        private void ProcessDrag(DataGridControl lvSource, DataGridControl lvDest, GetPositionDelegate mousePoint)
        {

            sourceIndex = this.GetCurrentIndex(mousePoint, lvSource);
            curSource = lvSource;

            if (sourceIndex < 0)
                return;

            lvSource.SelectedIndex = sourceIndex;
            selectedItem = lvSource.Items[sourceIndex] as Document;

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
        void lvPickOrders_Drop(object sender, DragEventArgs e)
        {
            ProcessDrop((DataGridControl)sender, e.GetPosition);
        }

        // function called during drop operation
        void lvToday_Drop(object sender, DragEventArgs e)
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



            ((IList<Document>)curSource.Items.SourceCollection).Remove(selectedItem);

            if (curSource.Name == "lvPickOrders")
                RemoveTaskByUser(this, new DataEventArgs<Document>(selectedItem));
            

            //Destination Operation
            if (((IList<Document>)lvDest.Items.SourceCollection).Where(f => f.DocID == selectedItem.DocID).Count() == 0)
            {
                //Add Trask By User
                if (lvDest.Name == "lvPickOrders")
                    AddTaskByUser(this, new DataEventArgs<Document>(selectedItem));

                ((IList<Document>)lvDest.Items.SourceCollection).Insert(index, selectedItem);

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



    }



    public interface IShippingConsoleView
    {

        ShippingConsoleModel Model { get; set; }
        event EventHandler<DataEventArgs<Document>> AddTaskByUser;
        event EventHandler<DataEventArgs<Document>> RemoveTaskByUser;
        event EventHandler<EventArgs> LoadPickerDocuments;
    }

}
