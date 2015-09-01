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
    /// Interaction logic for TrackOptionShipView.xaml
    /// </summary>
    public partial class TrackOptionShipView : UserControlBase, ITrackOptionShipView
    {
        public TrackOptionShipView()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> PickToList;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<DataEventArgs<WpfFront.WMSBusinessService.Label>> LoadQuantity;
        public event EventHandler<EventArgs> LoadSetup;
        public event EventHandler<DataEventArgs<String>> PickUniqueLabel;
        public event EventHandler<EventArgs> RemoveUniqueTrack;



        public TrackOptionShipModel Model
        {
            get
            { return this.DataContext as TrackOptionShipModel; }
            set
            { this.DataContext = value; }

        }


        public Button BtnPick
        {
            get { return this.btnPick; }
            set { this.btnPick = value; }
        }

        public TextBox PickQty
        {
            get { return this.txtQty; }
            set { this.txtQty = value; }
        }


        public ListView ManualTrackList
        {
            get { return this.lvManualTrackList; }
            set { this.lvManualTrackList = value; }
        }


        public ListView ManualTrackPicked
        {
            get { return this.lvManualTrackPicked; }
            set { this.lvManualTrackPicked = value; }
        }


        public Button BtnTrackRemove
        {
            get { return this.btnTrackRemove; }
            set { this.btnTrackRemove = value; }

        }

        public StackPanel StkPrdDesc
        {
            get { return this.stkPrdDesc; }
            set { this.stkPrdDesc = value; }
        }

        public SearchProduct UcProduct
        {
            get { return this.ucProduct; }
            set { this.ucProduct = value; }
        }

        public ListView UniqueTrackList
        {
            get { return this.lvUniqueTrack; }
            set { this.lvUniqueTrack = value; }

        }

        public StackPanel StkUnique
        {
            get { return this.stkUnique; }
            set { this.stkUnique = value; }
        }

        public StackPanel StkNonUnique
        {
            get { return this.stkNonUnique; }
            set { this.stkNonUnique = value; }
        }

        public TextBox TxtUnique
        {
            get { return this.txtUnique; }
            set { this.txtUnique = value; }
        }



        private void btnTrackRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }


        private void btnPick_Click(object sender, RoutedEventArgs e)
        {
            PickToList(sender, e);
        }




        private void lvManualTrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvManualTrackList.SelectedItem == null)
                return;

            WpfFront.WMSBusinessService.Label curLabel;

            curLabel = this.lvManualTrackList.SelectedItem as WpfFront.WMSBusinessService.Label;

            LoadQuantity(this, new DataEventArgs<WpfFront.WMSBusinessService.Label>(curLabel));
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            lvUniqueTrack.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            lvUniqueTrack.UnselectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUnique.Text))
                return;

            PickUniqueLabel(this, new DataEventArgs<String>(txtUnique.Text));
            txtUnique.Focus();
        }

        private void txtUnique_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAddTrackOpt.Focus();
                PickUniqueLabel(this, new DataEventArgs<String>(txtUnique.Text));
                ((TextBox)sender).Focus();
            }
        }


    }



    public interface ITrackOptionShipView
    {

        TrackOptionShipModel Model { get; set; }

        event EventHandler<EventArgs> PickToList;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<DataEventArgs<WpfFront.WMSBusinessService.Label>> LoadQuantity;
        event EventHandler<EventArgs> LoadSetup;
        event EventHandler<DataEventArgs<String>> PickUniqueLabel;
        event EventHandler<EventArgs> RemoveUniqueTrack;


        ListView ManualTrackList { get; set; }
        ListView ManualTrackPicked { get; set; }
        TextBox PickQty { get; set; }
        Button BtnTrackRemove { get; set; }
        StackPanel StkPrdDesc { get; set; }
        Button BtnPick { get; set; }
        SearchProduct UcProduct { get; set; }
        ListView UniqueTrackList { get; set; }
        StackPanel StkNonUnique { get; set; }
        StackPanel StkUnique { get; set; }
        TextBox TxtUnique { get; set; }
    }
}
