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
    /// Interaction logic for ZoneView.xaml
    /// </summary>
    /// 

    //delegate Point GetPositionDelegate(IInputElement element);

    public partial class ZoneManagerView : UserControlBase, IZoneManagerView
    {
        int sourceIndex = -1;
        DataGridControl curSource = null;
        Bin selectedItem;

        public ZoneManagerView()
        {
            InitializeComponent();
                        
            dgOpenBin.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(dgOpenBin_PreviewMouseLeftButtonDown);
            dgOpenBin.Drop += new DragEventHandler(dgOpenBin_Drop);

            dgRelatedBin.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(dgRelatedBin_PreviewMouseLeftButtonDown);
            dgRelatedBin.Drop += new DragEventHandler(dgRelatedBin_Drop);
            
        }


        //View Events
        public event EventHandler<DataEventArgs<Location>> LoadRecords;
        public event EventHandler<DataEventArgs<Zone>> LoadToAdmin;
        public event EventHandler<EventArgs> RemoveBinByUser;
        public event EventHandler<EventArgs> AddBinByUser;
        public event EventHandler<DataEventArgs<string>> LoadSearch;
        public event EventHandler<DataEventArgs<string>> LoadSearchCriteria;
        public event EventHandler<EventArgs> AddPicker;
        public event EventHandler<EventArgs> RemovePicker;
        public event EventHandler<EventArgs> LoadCriterias;
        public event EventHandler<EventArgs> AddRecord;
        public event EventHandler<EventArgs> RemoveRecord;

         public ZoneManagerModel Model
        {
            get
            { return this.DataContext as ZoneManagerModel; }
            set
            { this.DataContext = value; }

        }

        #region Properties

         public ListView ListRecords 
            { get { return this.dgList; } }


         public DataGridControl SubEntityList
         { get { return this.dgOpenBin; }
             set { this.dgOpenBin = value;  }
         }

         public DataGridControl AllowedList
         {
             get { return this.dgRelatedBin; }
             set { this.dgRelatedBin = value; }
         }

         public DataGridControl PickerList
         {
             get { return this.dgOpenPicker ; }
             set { this.dgOpenPicker = value; }
         }

         public DataGridControl PickerListReg
         {
             get { return this.dgRelatedPicker; }
             set { this.dgRelatedPicker = value; }
         }

         public StackPanel StkInfo
         { 
             get { return this.stkInfo; }
         }

         public ComboBox ClassEntityCmb
         { get { return this.cmbEntity ; } }

         public DataGridControl CriteriaList
         {
             get { return this.dgOpenCriterias ; }
             set { this.dgOpenCriterias = value; }
         }

         public DataGridControl CriteriaListReg
         {
             get { return this.dgRelatedCriterias ; }
             set { this.dgRelatedCriterias = value; }
         }

         public TextBox TxtSearch
         {
             get { return this.txtSearch; }
             set { this.txtSearch = value; }
         }

         public TextBox TxtSearchCriteria
         {
             get { return this.txtSearchCriteria ; }
             set { this.txtSearchCriteria = value; }
         }

        #endregion






        #region ViewEvents


         private void Location_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             Location location = (Location)((ComboBox)sender).SelectedItem;

             if (location == null)
                 return;

             LoadRecords(this, new DataEventArgs<Location>(location));       

         }


         private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             Zone zone = (Zone)((ListView)sender).SelectedItem;

             if (zone == null)
                 return;

             LoadToAdmin(this, new DataEventArgs<Zone>(zone));
         }

         private void Button_Click(object sender, RoutedEventArgs e)
         {
             AddBinByUser(this, e);
         }

         private void Button_Click_1(object sender, RoutedEventArgs e)
         {
             RemoveBinByUser(this, e);
         }

         private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
         {
             LoadSearch(sender, new DataEventArgs<string>(((TextBox)sender).Text));
         }

         private void AddPicker_Click(object sender, RoutedEventArgs e)
         {
             AddPicker(this, e);
         }

         private void RemovePicker_Click(object sender, RoutedEventArgs e)
         {
             RemovePicker(this, e);
         }

         private void cmbEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             LoadCriterias(this, e);
         }

         private void txtSearchCriteria_TextChanged(object sender, TextChangedEventArgs e)
         {
             LoadSearchCriteria(sender, new DataEventArgs<string>(((TextBox)sender).Text));
         }

         private void btnAddRecord_Click(object sender, RoutedEventArgs e)
         {
             AddRecord(this, e);
         }

         private void btnRemoveRecord_Click(object sender, RoutedEventArgs e)
         {
             RemoveRecord(this,e);
         }

         private void expBin_Expanded(object sender, System.Windows.RoutedEventArgs e)
         {
             CheckExpanders((OdcExpander)sender, true);
         }

         private void expBin_Collapsed(object sender, System.Windows.RoutedEventArgs e)
         {
             CheckExpanders((OdcExpander)sender, false);
         }

         private void CheckExpanders(OdcExpander sender, bool expand)
         {

             if (expBin == null || expPicker == null)
                 return;

             if (sender.Name == "expPicker")
             {
                 if (expand)
                 {
                     expBin.IsExpanded = false;
                 }
                 else
                 {
                     expBin.IsExpanded = true;

                 }

                 return;
             }

             if (sender.Name == "expBin")
             {
                 if (expand)
                 {
                     expPicker.IsExpanded = false;
                 }
                 else
                 {
                     expPicker.IsExpanded = true;
                 }

                 return;
             }

             if (sender.Name == "expCriterias")
             {
                 if (expand)
                 {
                    expCriterias.IsExpanded = true;
                 }
                 else
                 {
                     expCriterias.IsExpanded = false;
                 }

                 return;
             }
         }


        #endregion


         #region Drag And Drop


         void dgOpenBin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             ProcessDrag((DataGridControl)sender, dgRelatedBin, e.GetPosition);
         }

         void dgRelatedBin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             ProcessDrag((DataGridControl)sender, dgOpenBin, e.GetPosition);
         }


         private void ProcessDrag(DataGridControl lvSource, DataGridControl lvDest, GetPositionDelegate mousePoint)
         {
             return;
             sourceIndex = this.GetCurrentIndex(mousePoint, lvSource);
             curSource = lvSource;

             if (sourceIndex < 0)
                 return;

             lvSource.SelectedIndex = sourceIndex;
             if (curSource.Name == "dgRelatedBin")
                 selectedItem = ((ZoneBinRelation)lvSource.Items[sourceIndex]).Bin;
             else
                 selectedItem = lvSource.Items[sourceIndex] as Bin;

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
         void dgRelatedBin_Drop(object sender, DragEventArgs e)
         {
             ProcessDrop((DataGridControl)sender, e.GetPosition);
         }

         // function called during drop operation
         void dgOpenBin_Drop(object sender, DragEventArgs e)
         {
             ProcessDrop((DataGridControl)sender, e.GetPosition);
         }



         private void ProcessDrop(DataGridControl lvDest, GetPositionDelegate mousePoint)
         {
             return;
             if (curSource == lvDest)
                 return;

             if (sourceIndex < 0)
                 return;

             int index = this.GetCurrentIndex(mousePoint, lvDest);

             if (index < 0)
                 index = 0; //return;

             //RemoveBinByUser(this, new DataEventArgs<Bin>(selectedItem));

             //Destination Operation
             //Add Trask By User
             selectedItem.Rank = index;
             //AddBinByUser(this, new DataEventArgs<Bin>(selectedItem));
 
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



    public interface IZoneManagerView
    {
        event EventHandler<DataEventArgs<Location>> LoadRecords;
        event EventHandler<DataEventArgs<Zone>> LoadToAdmin;
        event EventHandler<EventArgs> AddBinByUser;
        event EventHandler<EventArgs> RemoveBinByUser;
        event EventHandler<DataEventArgs<string>> LoadSearch;
        event EventHandler<DataEventArgs<string>> LoadSearchCriteria;
        event EventHandler<EventArgs> AddPicker;
        event EventHandler<EventArgs> RemovePicker;
        event EventHandler<EventArgs> LoadCriterias;
        event EventHandler<EventArgs> AddRecord;
        event EventHandler<EventArgs> RemoveRecord;

        //Clase Modelo
        ZoneManagerModel Model { get; set; }

        ListView ListRecords { get; }
        DataGridControl SubEntityList { get; set; }
        DataGridControl AllowedList { get; set; }
        DataGridControl PickerList { get; set; }
        DataGridControl PickerListReg { get; set; }
        DataGridControl CriteriaList { get; set; }
        DataGridControl CriteriaListReg { get; set; }
        StackPanel StkInfo { get; }
        ComboBox ClassEntityCmb { get; }
        TextBox TxtSearch { get; set; }
        TextBox TxtSearchCriteria { get; set; }
    }
}