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
using System.Collections.Specialized;
using System.Collections;
using System.Data;
using WpfFront.Common.UserControls;


namespace WpfFront.Views
{

    //delegate Point GetPositionDelegate(IInputElement element);

    /// <summary>
    /// Interaction logic for Queries.xaml
    /// </summary>
    public partial class QueriesView : UserControlBase, IQueriesView
    {

        //private static ComboBox curDataType;
        int sourceIndex = -1;
        //ListView curSource = null;
        DataGridControl curSource = null;
        IqReportColumn selectedItem; //Actual Item Seleccionado en el selector de campos
        bool firstTime = true;

        public QueriesView()
        {
            m_doingInitializeComponent = true;
            InitializeComponent();
            m_doingInitializeComponent = false;
            string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            if (listSeparator == ",")
            {
                this.separatorComboBox.SelectedIndex = 0;
            }
            else if (listSeparator == ";")
            {
                this.separatorComboBox.SelectedIndex = 1;
            }
            else
            {
                this.separatorComboBox.SelectedIndex = 2;
            }

            this.exportFormatComboBox.SelectionChanged += new SelectionChangedEventHandler(exportFormatComboBox_SelectionChanged);

            this.GridDetails.Height = SystemParameters.FullPrimaryScreenHeight - 230;
            //this.lvSource.Height = SystemParameters.FullPrimaryScreenHeight - 320;

            lvSource.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvSource_PreviewMouseLeftButtonDown);
            lvSource.Drop += new DragEventHandler(lvSource_Drop);

            lvDest.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvDest_PreviewMouseLeftButtonDown);
            lvDest.Drop += new DragEventHandler(lvDest_Drop);

        }



        //Events
        public event EventHandler<DataEventArgs<IqReport>> SaveData;
        public event EventHandler<DataEventArgs<IqReport>> UpdateData;
        public event EventHandler<DataEventArgs<IqReport>> SelectionData;
        public event EventHandler<DataEventArgs<IqReportColumn>> RemoveFromSelected;
        public event EventHandler<DataEventArgs<IqReportColumn>> AddToSelected;
        public event EventHandler<DataEventArgs<String>> RunData;
        public event EventHandler<DataEventArgs<IqReportColumn>> UpdateFilter;
        public event EventHandler<DataEventArgs<IqReport>> DeleteReport;
        public event EventHandler<EventArgs> LoadProcess;



        public IQueriesModel Model
        {
            get
            {
                return this.DataContext as IQueriesModel;
            }
            set
            {
                this.DataContext = value;
            }
        }


        public DataGridControl GridDet {
            get { return this.GridDetails; }
            set { this.GridDetails = value;}
        }

        public TableView TbView
        {
            get { return this.tbView; }
            set { this.tbView = value; }
        }


        public StackPanel StkReport
        {
            get { return this.stkReport; }
            set { this.stkReport = value; }
        }

        //public ListView LvFilters
        //{
        //    get { return this.lvFilters; }
        //    set { this.lvFilters = value; }
        //}

        public TextBlock NumRecords
        {
            get { return this.txtRecords; }
            set { this.txtRecords = value; }
        }


        public Button BtnDelRep
        {
            get { return this.btnDelRep; }
            set { this.btnDelRep = value; }
        }

        public ComboBox CboReport
        {
            get { return this.cboReport; }
            set { this.cboReport = value; }
        }

        public Button BtnUpdRep
        {
            get { return this.btnUpd; }
            set { this.btnUpd = value; }
        }

        public WrapPanel StkFilters
        {
            get { return this.stkFilters; }
            set { this.stkFilters = value; }
        }


        public InventoryCountSchedule UcCountSch
        {
            get { return this.ucCountSh; }
            set { this.ucCountSh = value; }
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
            selectedItem = lvSource.Items[sourceIndex] as IqReportColumn;

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



            ((IList<IqReportColumn>)curSource.Items.SourceCollection).Remove(selectedItem);

            if (curSource.Name == "lvDest")
                //Remueve de los seleccionados cambia el visible a false
                RemoveFromSelected(this, new DataEventArgs<IqReportColumn>(selectedItem));


            //Destination Operation
            if (((IList<IqReportColumn>)lvDest.Items.SourceCollection).Where(f => f.ReportColumnId == selectedItem.ReportColumnId).Count() == 0)
            {
                //Add Trask By User
                if (lvDest.Name == "lvDest")
                    AddToSelected(this, new DataEventArgs<IqReportColumn>(selectedItem));

                ((IList<IqReportColumn>)lvDest.Items.SourceCollection).Insert(index, selectedItem);

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




        private void Button_Run(object sender, System.Windows.RoutedEventArgs e)
        {
            if (popFields.IsOpen)
                popFields.IsOpen = false;

            if (popUcFilters.IsOpen)
                popUcFilters.IsOpen = false;

            //if (stkFilters.IsVisible)
            //    stkFilters.Visibility = Visibility.Collapsed;

            //A cada columna en Options poner el dato del filtro.


            RunData(this, new DataEventArgs<String>(txtToShow.Text));

        }

        private void ExportToCsv()
        {
            //if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
            //{
                //MessageBox.Show("Due to restricted user-access permissions, this feature cannot be demonstrated when the Live Explorer is running as an XBAP browser application. Please download the full Xceed DataGrid for WPF package and run the Live Explorer as a desktop application to try out this feature.", "Feature unavailable");
                //return;
            //}

            // The simplest way to export in CSV format is to call the 
            // DataGridControl.ExportToCsv() method. However, if you want to specify export
            // settings, you have to take the longer, more descriptive and flexible route: 
            // the CsvExporter class.
            CsvExporter csvExporter = new CsvExporter(this.GridDetails);

            // By setting the Culture to the CurrentCulture (system culture by default), the
            // date and number formats set in the regional settings will be used.
            csvExporter.FormatSettings.Culture = CultureInfo.CurrentCulture;

            csvExporter.FormatSettings.DateTimeFormat = (string)this.dateTimeFormatComboBox.SelectedValue;
            csvExporter.FormatSettings.NumericFormat = (string)this.numberFormatComboBox.SelectedValue;
            csvExporter.FormatSettings.Separator = (char)this.separatorComboBox.SelectedValue;
            csvExporter.FormatSettings.TextQualifier = (char)this.textQualifierComboBox.SelectedValue;

            csvExporter.IncludeColumnHeaders = this.includeColumnHeadersCheckBox.IsChecked.GetValueOrDefault();
            csvExporter.RepeatParentData = this.repeatParentDataCheckBox.IsChecked.GetValueOrDefault();
            csvExporter.DetailDepth = Convert.ToInt32(this.detailDepthTextBox.Value);
            csvExporter.UseFieldNamesInHeader = this.UseFieldNamesInHeaderCheckBox.IsChecked.GetValueOrDefault();

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.Filter = "CSV file (*.csv)|*.csv|Text file (*.txt)|*.txt|All files (*.*)|*.*";

            try
            {
                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                {
                    using (Stream stream = saveFileDialog.OpenFile())
                    {
                        csvExporter.Export(stream);
                    }
                }
            }
            catch { }
        }


        private void ExportToExcel()
        {
            //if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
            //{
                //MessageBox.Show("Due to restricted user-access permissions, this feature cannot be demonstrated when the Live Explorer is running as an XBAP browser application. Please download the full Xceed DataGrid for WPF package and run the Live Explorer as a desktop application to try out this feature.", "Feature unavailable");
                //return;
            //}

            // The simplest way to export in Excel format is to call the 
            // DataGridControl.ExportToExcel() method. However, if you want to specify export
            // settings, you have to take the longer, more descriptive and flexible route: 
            // the ExcelExporter class.

            // excelExporter.FixedColumnCount will automatically be set to the specified
            // grid's FixedColumnCount value.
            //ExcelExporter excelExporter = new ExcelExporter(this.GridDetails);
            //excelExporter.ExportStatFunctionsAsFormulas = this.exportStatFunctionsAsFormulasCheckBox.IsChecked.GetValueOrDefault();
            //excelExporter.IncludeColumnHeaders = this.includeColumnHeadersCheckBox.IsChecked.GetValueOrDefault();
            //excelExporter.IsHeaderFixed = this.isHeaderFixedCheckBox.IsChecked.GetValueOrDefault();
            //excelExporter.RepeatParentData = this.repeatParentDataCheckBox.IsChecked.GetValueOrDefault();
            //excelExporter.DetailDepth = Convert.ToInt32(this.detailDepthTextBox.Value);
            //excelExporter.StatFunctionDepth = Convert.ToInt32(this.statFunctionDepthTextBox.Value);
            //excelExporter.UseFieldNamesInHeader = this.UseFieldNamesInHeaderCheckBox.IsChecked.GetValueOrDefault();

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.Filter = "XML Spreadsheet (*.xml)|*.xml|All files (*.*)|*.*";

            try
            {
                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                {
                    using (Stream stream = saveFileDialog.OpenFile())
                    {
                        //excelExporter.Export(stream);
                        GridDetails.ExportToExcel(stream);
                    }
                }
            }
            catch { }
        }


        private void export_Click(object sender, RoutedEventArgs e)
        {            


            //if (exportFormatComboBox.SelectedIndex == 0)
            //    this.ExportToExcel();

            //else
                this.ExportToCsv();

        }


        private void exportFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (exportFormatComboBox.SelectedIndex == 0)
            {
                this.excelOptionsPanel.Visibility = Visibility.Visible;
                this.csvOptionsPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                this.excelOptionsPanel.Visibility = Visibility.Hidden;
                this.csvOptionsPanel.Visibility = Visibility.Visible;
            }
        }


        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();           
        }

        private void PrintReport()
        {
            this.textProgressInformation.Text = string.Empty;

            this.textProgressInformation.Text += "Document preparation started...";

            this.progressScrollViewer.ScrollToBottom();

            m_printButton.IsEnabled = false;
            m_exportButton.IsEnabled = false;
            try
            {
                bool jobCompleted = GridDetails.Print(cboReport.Text, true, new EventHandler<ProgressEventArgs>(this.ProgressionCallBack), true);

                if (jobCompleted)
                {
                    this.textProgressInformation.Text += "\n...Completed.";
                }
                else
                {
                    this.textProgressInformation.Text += "\n...Canceled.";
                }

                this.progressScrollViewer.ScrollToBottom();
            }
            finally
            {
                m_printButton.IsEnabled = true;
                m_exportButton.IsEnabled = true;
            }
        }


        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
 
            this.textProgressInformation.Text = "Document exportation started...";

            this.progressScrollViewer.ScrollToBottom();

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.Filter = "XPS files (*.xps)|*.xps|All files (*.*)|*.*";

            bool jobCompleted = false;
            if (saveFileDialog.ShowDialog().GetValueOrDefault())
            {
                Size size = new Size(8.5d * 96.0d, 11.0d * 96.0d);

                m_printButton.IsEnabled = false;
                m_exportButton.IsEnabled = false;
                try
                {
                    jobCompleted = GridDetails.ExportToXps(saveFileDialog.FileName,
                      size, new Rect(size),
                      new PageRange(1, 0), CompressionOption.Normal,
                      new EventHandler<ProgressEventArgs>(this.ProgressionCallBack), true);
                }
                catch (System.IO.IOException)
                {
                    jobCompleted = false;
                }
                finally
                {
                    m_printButton.IsEnabled = true;
                    m_exportButton.IsEnabled = true;
                }
            }

            if (jobCompleted)
            {
                this.textProgressInformation.Text += "\n...Completed.";
            }
            else
            {
                this.textProgressInformation.Text += "\n...Canceled.";
            }

            this.progressScrollViewer.ScrollToBottom();
        }


        private void ProgressionCallBack(object sender, ProgressEventArgs e)
        {
            this.textProgressInformation.Text += "\nPreparing page " + e.ProgressInfo.CurrentPageNumber.ToString();

            this.progressScrollViewer.ScrollToBottom();
        }


        private void SelectedPrintViewChanged(object sender, RoutedEventArgs e)
        {
            if (m_doingInitializeComponent)
                return;

            if (chkConfiguredPrintView.IsChecked.Value)
            {
                GridDetails.PrintView = this.Resources["configuredPrintView"] as PrintViewBase;
            }
            else if (chkCustomPrintView.IsChecked.Value)
            {
                GridDetails.PrintView = this.Resources["customPrintView"] as PrintViewBase;
            }
            else
            {
                GridDetails.PrintView = null;
            }
        }


        public bool m_doingInitializeComponent;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Save As

            SettingsRepository settings = new SettingsRepository();
            this.GridDetails.SaveUserSettings(settings, UserSettings.All);

            IqReport saveAs = Model.ReportSystem;

            saveAs.Settings = Util.XmlSerializer(settings);
            saveAs.Name = TextName.Text;

            foreach (IqReportTable tb in saveAs.ReportTables)
            {
                tb.ReportTableId = 0;
                foreach (IqReportColumn rc in tb.ReportColumns)
                    rc.ReportColumnId = 0;
            }

            SaveData(sender, new DataEventArgs<IqReport>(saveAs));


        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stkRptOp.Visibility = Visibility.Collapsed;
            stkReport.Visibility = Visibility.Collapsed;
            firstTime = true;

            if (e.AddedItems == null)
                return;


            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                IqReport report = e.AddedItems[0] as IqReport;
                if (report != null)
                {
                    SelectionData(sender, new DataEventArgs<IqReport>(report));
                    //RunData(sender, new DataEventArgs<String>(txtToShow.Text));
                    stkReport.Visibility = Visibility.Visible;
                    stkRptOp.Visibility = Visibility.Visible;

                    popUcFilters.IsOpen = true;
                    popUcFilters.StaysOpen = true;
                    //stkFilters.Visibility = Visibility.Visible;

                }
            }            
            //stkRptOp.Visibility = Visibility.Visible;
        }


        //private void lvSource_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (popup1.IsOpen)
        //        popup1.IsOpen = false;

        //    if (this.lvSource.SelectedItem == null)
        //        return;

        //    IqReportColumn rc = lvSource.SelectedItem as IqReportColumn;

        //    if (rc.IsFiltered == true)
        //    {
        //        stkFilter.Visibility = Visibility.Visible;
        //        stkAggregate.Visibility = Visibility.Collapsed;

        //        //Determinig DataType
        //        if (rc.Column.DbType.Contains("varchar"))
        //        {
        //            cboNumCom.Visibility = Visibility.Collapsed;
        //            cboStrComp.Visibility = Visibility.Visible;
        //            curDataType = cboStrComp;
        //        }
        //        else if (rc.Column.DbType.Contains("int"))
        //        {
        //            cboNumCom.Visibility = Visibility.Visible;
        //            cboStrComp.Visibility = Visibility.Collapsed;
        //            curDataType = cboNumCom;
        //        }
        //        else
        //            return;

        //    }
        //    else if (rc.IsAggregate == true)
        //    {
        //        stkFilter.Visibility = Visibility.Collapsed;
        //        stkAggregate.Visibility = Visibility.Visible;
        //    }
        //    else
        //        return;


        //    popup1.IsOpen = true;
        //    popup1.StaysOpen = true;
        //}


        private void btnPop_Click(object sender, RoutedEventArgs e)
        {
            popUcFilters.IsOpen = false;
        }

        /*



        private void btnSaveFilter_Click(object sender, RoutedEventArgs e)
        {

            if (cboColumns.SelectedIndex == -1)
            {
                if (firstTime)
                    RunFirstTime();
                else
                    return;
            }

            if (cboStrComp.SelectedIndex == -1)
            {
                if (firstTime)
                    RunFirstTime();
                else
                    return;
            }

            IqReportColumn rc = cboColumns.SelectedItem as IqReportColumn;

            //Si es un Filter y no entro valor no lo guarda.
            if (rc.IsFiltered == true && string.IsNullOrEmpty(txtFilter.Text))
            {
                if (firstTime)
                    RunFirstTime();
                else
                    return;
            }

            DictionaryEntry operatorItem = (DictionaryEntry)cboStrComp.SelectedItem;


            //Save Report Colum Options            
            string strOption = "", opDesc = "";

            if (rc.IsFiltered == true && operatorItem.Value.ToString() != "")
            {
                if (operatorItem.Key.Equals("between (range)"))
                {
                    strOption = "B:" + txtFilter.Text + ":" + txtFilter1.Text;
                    opDesc = rc.Alias + " Between [" + txtFilter.Text + " and " + txtFilter1.Text + "]";
                }
                else
                {
                    //KEY:VALUE (Cuando es un Filter)
                    strOption = "F:" + operatorItem.Value + ":" + txtFilter.Text;
                    opDesc = rc.Alias + " " + operatorItem.Key + " [" + txtFilter.Text + "]";

                }

            }

            rc.Options = strOption;
            rc.OptionsDesc = opDesc;
            UpdateFilter(sender, new DataEventArgs<IqReportColumn>(rc));

            txtFilter.Text = "";
            txtFilter1.Text = "";
            cboStrComp.SelectedIndex = -1;
            cboColumns.SelectedIndex = -1;

            if (firstTime)
                RunFirstTime();

        }
        */

        //public void RunFirstTime()
        //{

        //    firstTime = false;

        //    if (popFields.IsOpen)
        //        popFields.IsOpen = false;

        //    if (popup1.IsOpen)
        //        popup1.IsOpen = false;

        //    RunData(this, new DataEventArgs<String>(txtToShow.Text));

        //}



        private void btnUpd_Click(object sender, RoutedEventArgs e)
        {
            //Update Report
            SettingsRepository settings = new SettingsRepository();
            this.GridDetails.SaveUserSettings(settings, UserSettings.All);

            Model.ReportSystem.Settings = Util.XmlSerializer(settings);

            UpdateData(sender, new DataEventArgs<IqReport>(Model.ReportSystem));

        }

        private void btnFiledS_Click(object sender, RoutedEventArgs e)
        {
            popFields.IsOpen = true;
            popFields.StaysOpen = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            popFields.IsOpen = false;
        }

        //private void btnRemove_Click(object sender, RoutedEventArgs e)
        //{
        //    if (lvFilters.SelectedItems == null)
        //        return;

        //    foreach (Object obj in lvFilters.SelectedItems)
        //    {
        //        ((IqReportColumn)obj).Options = "";
        //        ((IqReportColumn)obj).OptionsDesc = "";
        //    }
            
        //    lvFilters.Items.Refresh();
        //    this.Model.AllColumns = this.Model.AllColumns;
        //}


        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            //stkFilters.Visibility = Visibility.Visible;
            popUcFilters.IsOpen = true;
            popUcFilters.StaysOpen = true;

            if (this.Model.Details != null && this.Model.Details.Rows.Count > 0 )
                LoadProcess(sender, e);
        }


        private void btnPrintx_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }


        private void btnDelRep_Click(object sender, RoutedEventArgs e)
        {
            DeleteReport(sender, new DataEventArgs<IqReport>(Model.ReportSystem));
        }

        //private void btnSched_Click(object sender, RoutedEventArgs e)
        //{
        //    ucCountSh.Query = Model.Query;
        //    ucCountSh.QueryParam = Model.QueryParams;
        //    ucCountSh.Products = Model.Details;
        //}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            QueryFilter r = null;
            foreach (Object obj in stkFilters.Children)
            {
                r = obj as QueryFilter;
                r.cboStrComp.SelectedValue = " = _val";
                r.RepColumn.FilteredValue = "";
            }
        }

        private void extToXls_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfdExportToxcel = new System.Windows.Forms.SaveFileDialog();
            ExportToExcel exp = new ExportToExcel();
            sfdExportToxcel.FileName = "FilenameToExport.xlsx";
            //sfdExportToxcel.Filter = "Archivos Excel (*.xls)|*.xls|Todos los Archivos (*.*)|*.*";
            sfdExportToxcel.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
            sfdExportToxcel.FilterIndex = 1;
            sfdExportToxcel.RestoreDirectory = true;

            if (sfdExportToxcel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = sfdExportToxcel.FileName;
                exp.dataGridView2ExcelDinamico(GridDetails, path, "NameSheet");
            }
        }


     }


    public interface IQueriesView
    {
        IQueriesModel Model { get; set; }

        DataGridControl GridDet {get; set;}
        TableView TbView { get; set; }
        StackPanel StkReport { get; set; }
        //ListView  LvFilters { get; set; }
        TextBlock NumRecords { get; set; }
        Button BtnDelRep { get; set; }
        Button BtnUpdRep { get; set; }
        ComboBox CboReport { get; set; }
        WrapPanel StkFilters { get; set; }
        InventoryCountSchedule UcCountSch{ get; set; }

        event EventHandler<DataEventArgs<IqReport>> SaveData;
        event EventHandler<DataEventArgs<String>> RunData;
        event EventHandler<DataEventArgs<IqReport>> SelectionData;
        event EventHandler<DataEventArgs<IqReport>> UpdateData;
        event EventHandler<DataEventArgs<IqReportColumn>> UpdateFilter;

        event EventHandler<DataEventArgs<IqReportColumn>> RemoveFromSelected;
        event EventHandler<DataEventArgs<IqReportColumn>> AddToSelected;
        event EventHandler<DataEventArgs<IqReport>> DeleteReport;
        event EventHandler<EventArgs> LoadProcess;

        
    }
}