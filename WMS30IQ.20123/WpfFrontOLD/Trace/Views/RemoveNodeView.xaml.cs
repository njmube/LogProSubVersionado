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
    /// Interaction logic for RemoveNodeView.xaml
    /// </summary>
    public partial class RemoveNodeView : UserControlBase, IRemoveNodeView
    {
        public RemoveNodeView()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> RemovePrinted;
        public event EventHandler<EventArgs> RemoveManual;

        public RemoveNodeModel Model
        {
            get
            { return this.DataContext as RemoveNodeModel; }
            set
            { this.DataContext = value; }

        }

        public ListView ListPrinted
        { get { return this.lvPrintedLabels; } }
        
        public TextBox TxtQtyManualNew
        {
            get
            { return this.txtQtyRemove; }
            set
            { this.txtQtyRemove = value; }

        }


        public StackPanel StkUcBin
        {
            get
            { return this.stkUcBin; }
            set
            { this.stkUcBin = value; }

        }

        public Border BrdManual
        {
            get
            { return this.brdManual; }
            set
            { this.brdManual = value; }
        }

        public Border BrdPrinted
        {
            get
            { return this.brdPrinted; }
            set
            { this.brdPrinted = value; }
        }


        public BinLocation RestockBin { get { return this.binRestock; } }


        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAll.Name)
            {
                this.ListPrinted.SelectAll();
                this.ListPrinted.Focus();
            }
        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).Name == chkSelectAll.Name)
                this.ListPrinted.UnselectAll();

        }

        private void btnRemovePrinted_Click(object sender, RoutedEventArgs e)
        {
            RemovePrinted(sender, e);
        }

        private void btnRemoveManual_Click(object sender, RoutedEventArgs e)
        {
            RemoveManual(sender,e);
        }

    }

    public interface IRemoveNodeView
    {
        event EventHandler<EventArgs> RemovePrinted;
        event EventHandler<EventArgs> RemoveManual;

        RemoveNodeModel Model { get; set; }
        ListView ListPrinted { get; }
        TextBox TxtQtyManualNew { get; set; }
        Border BrdManual { get; set; }
        Border BrdPrinted { get; set; }
        StackPanel StkUcBin { get; set; }
        BinLocation RestockBin {get; }
    }
}
