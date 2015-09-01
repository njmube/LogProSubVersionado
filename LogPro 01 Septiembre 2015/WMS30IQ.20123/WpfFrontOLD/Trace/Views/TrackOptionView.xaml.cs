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
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for TrackOptionView.xaml
    /// </summary>
    /// 



    public partial class TrackOptionView : UserControlBase, ITrackOptionView
    {
        public TrackOptionView()
        {
            InitializeComponent();

            ////this.ucProduct.Focusable = false;
            ////this.ucProduct.txtData.Focusable = false;
            //this.ucProduct.imgLoad.Focusable = false;
            ////this.ucProduct.imgLook.Focusable = false;
            //ucProduct.imgXload.Focusable = false;

            //Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

        }

        private void TrackOption_Loaded(object sender, RoutedEventArgs e)
        {
            txtQtyTrack.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }


        public event EventHandler<EventArgs> AddManualTrackToList;
        public event EventHandler<EventArgs> RemoveManualTrack;
        public event EventHandler<EventArgs> LoadSetup;
        public event EventHandler<DataEventArgs<WpfFront.WMSBusinessService.Label>> LoadUniqueTracks;
        public event EventHandler<EventArgs> RemoveUniqueTrack;
        public event EventHandler<DataEventArgs<object[]>> UpdateTrackValue;

        public TrackOptionModel Model
        {
            get
            { return this.DataContext as TrackOptionModel; }
            set
            { this.DataContext = value; }

        }



        public TextBox TxtQtyTrack
        {
            get { return this.txtQtyTrack; }
            set { this.txtQtyTrack = value; }
        }

        public ListView ManualTrackList
        {
            get { return this.lvManualTrackList; }
            set { this.lvManualTrackList = value; }
        }


        public ListView LvTrackProduct
        {
            get { return this.lvTrackProduct; }
            set { this.lvTrackProduct = value; }
        }


        public Button BtnAddTrack
        {
            get { return this.btnAddTrackOpt; }
            set { this.btnAddTrackOpt = value; }
        }


        public Button BtnTrackRemove
        {
            get { return this.btnTrackRemove; }
            set { this.btnTrackRemove = value; }

        }

        public StackPanel StkAddTrack
        {
            get { return this.stkAddTrack; }
            set { this.stkAddTrack = value; }
        }

        public SearchProduct UcProduct
        {
            get { return this.ucProduct; }
            set { this.ucProduct = value; }
        }

        public StackPanel StkPrdDesc
        {
            get { return this.stkPrdDesc; }
            set { this.stkPrdDesc = value; }

        }


        public TextBlock TxtMsgQty
        {
            get { return this.txtMsgQty; }
            set { this.txtMsgQty = value; }

        }


        public ListView UniqueTrackList
        {
            get { return this.lvUniqueTrack; }
            set { this.lvUniqueTrack = value; }

        }


        public StackPanel StkUniqueLabels
        {
            get { return this.stkUniqueLabels; }
            set { this.stkUniqueLabels = value; }

        }

        public Button BtnUniqueRem
        {
            get { return this.btnUniqueRemove; }
            set { this.btnUniqueRemove = value; }

        }


        //Events

        //private void btnLabelTrakOpt_Click(object sender, RoutedEventArgs e)
        //{
        //    //Recibe el Label with the tracking option
        //    ReceiveLabelTrackOption(this, new DataEventArgs<string>(txtScanLabel.Text));
        //}

        private void Add_TrackOpt_Click(object sender, RoutedEventArgs e)
        {
            AddManualTrackToList(sender, e);
            
            //lvTrackProduct.Focus();
            txtQtyTrack.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            //MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
            

        }


        private void btnTrackRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveManualTrack(sender, e);
        }


        private void txtQtyTrack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAddTrackOpt.Focus();
                AddManualTrackToList(sender, e);
                ((TextBox)sender).Focus();
            }
        }

        private void ucProduct_OnLoadRecord(object sender, EventArgs e)
        {
            if (ucProduct.Product == null)
                return;

            this.Model.Product = ucProduct.Product;
            LoadSetup(sender, e);
            
        }

        private void btnUniqueRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveUniqueTrack(sender, e);
        }


        private void lvManualTrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvManualTrackList.SelectedItem == null)
                return;

            LoadUniqueTracks(this, new DataEventArgs<WpfFront.WMSBusinessService.Label>((WpfFront.WMSBusinessService.Label)lvManualTrackList.SelectedItem));
            txtQtyTrack.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            lvUniqueTrack.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            lvUniqueTrack.UnselectAll();
        }

        private void trak0_LostFocus(object sender, RoutedEventArgs e)
        {
            //Update Track Option.
            long labelID = long.Parse(((TextBox)(sender)).Tag.ToString());
            string trackValue = ((TextBox)sender).Text;
            UpdateTrackValue(this, new DataEventArgs<object[]>(new object[] { labelID, trackValue }));
        }




    }



    public interface ITrackOptionView
    {

        TrackOptionModel Model { get; set; }

        event EventHandler<EventArgs> AddManualTrackToList;
        event EventHandler<EventArgs> RemoveManualTrack;
        event EventHandler<EventArgs> LoadSetup;
        event EventHandler<DataEventArgs<WpfFront.WMSBusinessService.Label>> LoadUniqueTracks;
        event EventHandler<EventArgs> RemoveUniqueTrack;
        event EventHandler<DataEventArgs<object[]>> UpdateTrackValue;


        ListView ManualTrackList { get; set; }
        TextBox TxtQtyTrack { get; set; }
        Button BtnAddTrack { get; set; }
        Button BtnTrackRemove { get; set; }
        ListView LvTrackProduct { get; set; }
        StackPanel StkAddTrack { get; set; }
        SearchProduct UcProduct { get; set; }
        StackPanel StkPrdDesc { get; set; }
        TextBlock TxtMsgQty { get; set; }
        ListView UniqueTrackList { get; set; }
        StackPanel StkUniqueLabels { get; set; }
        Button BtnUniqueRem { get; set; }
    }
}
