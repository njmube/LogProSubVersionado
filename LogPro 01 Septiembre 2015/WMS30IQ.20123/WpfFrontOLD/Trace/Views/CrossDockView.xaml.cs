using System;
using System.Windows.Controls;
using Core.WPF;
using WpfFront.WMSBusinessService;
using WpfFront.Models;
using WMComposite.Events;
using Xceed.Wpf.DataGrid;
using System.Windows;
using Odyssey.Controls;
using WpfFront.Common;
using WpfFront.Common.UserControls;

namespace WpfFront.Views
{
    /// <summary>
    /// Interaction logic for GenerateView.xaml
    /// </summary>
    public partial class CrossDockView : UserControlBase, ICrossDockView
    {
        public CrossDockView()
        {
            InitializeComponent();
        }


        //View Events
        public event EventHandler<DataEventArgs<int>> ProcessPending;
        public event EventHandler<EventArgs> RemoveFromList;
        public event EventHandler<EventArgs> AddDocumentToAssigned;
        public event EventHandler<DataEventArgs<string>> SearchDocument;
        public event EventHandler<EventArgs> CrossDockPreview;
        public event EventHandler<EventArgs> ConfirmCrossDock;
        public event EventHandler<DataEventArgs<string>> SearchHistDocument;
        public event EventHandler<DataEventArgs<Document>> LoadDetails;
        public event EventHandler<EventArgs> ShowTicket;
        public event EventHandler<EventArgs> ShowCrossDockDocuments;


        public CrossDockModel Model
        {
            get
            { return this.DataContext as CrossDockModel; }
            set
            { this.DataContext = value; }

        }



        #region Properties


        public ListView DgDocumentBalance
        {
            get { return this.dgDocumentBalance; }
        }

        public Border StkPendingMsg
        {
            get { return this.stkPendingMsg; }
            set { this.stkPendingMsg = value; }
        }

        public ListView LvAvailableDocs
        {
            get { return this.lvAvailableDocs; }
            set { this.lvAvailableDocs = value; }
        }

        public ListView LvAssignedDocs
        {
            get { return this.lvAsignedDocs; }
            set { this.lvAsignedDocs = value; }
        }

        public Button BtnStep1
        {
            get { return this.btnStep1; }
            set { this.btnStep1 = value; }
        }

        public OdcExpander ExpDocs
        {
            get { return this.expDocs; }
            set { this.expDocs = value; }
        }

        public OdcExpander ExpResult
        {
            get { return this.expResult; }
            set { this.expResult = value; }
        }

        public TextBlock TxtWarning
        {
            get { return this.txWarning; }
            set { this.txWarning = value; }
        }

        public Button BtnStep2
        {
            get { return this.btnStep2; }
            set { this.btnStep2 = value; }
        }

        public TabItem TbCross
        {
            get { return this.tbCross; }
            set { this.tbCross = value; }
        }


        public TabControl TbControl
        {
            get { return this.tbControl; }
            set { this.tbControl = value; }
        }

        public TabItem TbHistory
        {
            get { return this.tbHistory; }
            set { this.tbHistory = value; }
        }

        public DataGridControl DgHistList
        {
            get { return this.dgDocument; }
            set { this.dgDocument = value; }
        }


        public DataGridControl DgHistLines
        {
            get { return this.dgLines; }
            set { this.dgLines = value; }
        }

        public StackPanel StkDetail
        {
            get { return this.stkDetail; }
            set { this.stkDetail = value; }
        }

        #endregion


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDocument(this, new DataEventArgs<string>(txtSearch.Text));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddDocumentToAssigned(this, e);
        }


        private void btnRemDoc_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromList(sender, e);
        }


        private void btnProcessPending_Click(object sender, RoutedEventArgs e)
        {
            if (cboToDo.SelectedIndex >= 0)
                ProcessPending(this, new DataEventArgs<int>(cboToDo.SelectedIndex));
        }

        private void btnStep1_Click(object sender, RoutedEventArgs e)
        {            
            CrossDockPreview(sender, e);
        }


        private void btnStep2_Click(object sender, RoutedEventArgs e)
        {
            //string stringMsg = Util.GetConfigOption("General"]["CDKMSG"];
            if (UtilWindow.ConfirmOK("Cross Dock is ready to be completed.\nPress OK to confirm process ?.") == true)
                ConfirmCrossDock(sender, e);
        }

        private void txtSearchH_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchHistDocument(this, new DataEventArgs<string>(txtSearchH.Text));
        }

        private void dgDocument_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadDetails(this, new DataEventArgs<Document>((Document)dgDocument.SelectedItem));
        }

        private void btnTkt_Click(object sender, RoutedEventArgs e)
        {
            ShowTicket(sender, e);
        }

        private void tbControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbControl.SelectedIndex == 1)
                ShowCrossDockDocuments(sender, e);
        }

        private void btnDet_Click(object sender, RoutedEventArgs e)
        {
            //ucDocDetail.Document = this.Model.Document;
            //ucDocDetail.DocumentLines = (new WMSServiceClient)
            popup1.IsOpen = true;
            popup1.StaysOpen = true;
        }



        #region ViewEvents




        #endregion

 

    }



    public interface ICrossDockView
    {
        ListView DgDocumentBalance { get; }
        Border StkPendingMsg { get; set; }
        ListView LvAvailableDocs { get; set; }
        ListView LvAssignedDocs { get; set; }
        Button BtnStep1 { get; set; }
        OdcExpander ExpDocs { get; set; }
        TextBlock TxtWarning { get; set; }
        OdcExpander ExpResult { get; set; }
        Button BtnStep2 { get; set; }
        TabItem TbCross { get; set; }
        TabItem TbHistory { get; set; }
        TabControl TbControl { get; set; }
        CrossDockModel Model { get; set; }
        DataGridControl DgHistList { get; set; }
        DataGridControl DgHistLines { get; set; }
        StackPanel StkDetail { get; set; }


        event EventHandler<DataEventArgs<int>> ProcessPending;
        event EventHandler<EventArgs> RemoveFromList;
        event EventHandler<EventArgs> AddDocumentToAssigned;
        event EventHandler<DataEventArgs<string>> SearchDocument;
        event EventHandler<EventArgs> CrossDockPreview;
        event EventHandler<EventArgs> ConfirmCrossDock;
        event EventHandler<DataEventArgs<string>> SearchHistDocument;
        event EventHandler<DataEventArgs<Document>> LoadDetails;
        event EventHandler<EventArgs> ShowTicket;
        event EventHandler<EventArgs> ShowCrossDockDocuments;
    }

}