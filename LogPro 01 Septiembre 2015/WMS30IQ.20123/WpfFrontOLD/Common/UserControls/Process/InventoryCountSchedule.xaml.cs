using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Data;
using Core.BusinessEntity;
using WpfFront.Services;
using WpfFront.WMSBusinessService;

namespace WpfFront.Common.UserControls
{
    /// <summary>
    /// Interaction logic for InventoryCountSchedule.xaml
    /// </summary>
    public partial class InventoryCountSchedule : UserControl
    { 
        public InventoryCountSchedule()
        {
            InitializeComponent();
        }

        private string query;
        public string Query
        {
            get { return query; }
            set
            {
                query = value;
            }
        }

        private DataSet queryParam;
        public DataSet QueryParam
        {
            get { return queryParam; }
            set
            {
                queryParam = value;
            }
        }

        private DataTable products;
        public DataTable Products
        {
            get { return products; }
            set
            {
                products = value;
            }
        }

        private void rdOpt_Checked(object sender, RoutedEventArgs e)
        {
            if (dgEdit != null)
                dgEdit.Visibility = Visibility.Collapsed;
        }

        private void rdOpt2_Checked(object sender, RoutedEventArgs e)
        {
            dgEdit.Visibility = Visibility.Visible;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            WMSServiceClient serv = new WMSServiceClient();

            if (rdOpt.IsChecked == true)
            {
                // make the Count document
                if (Products == null || Products.Rows.Count == 0)
                {
                    Util.ShowError("Records not selected");
                    return;
                }

                NewTask();
            }
            else
            {
                //// make the schedule
                if (string.IsNullOrEmpty(Query))
                {
                    Util.ShowError("Records not selected");
                    return;
                }

                if (string.IsNullOrEmpty(txtTitle.Text))
                {
                    Util.ShowError("Name or Reference is required");
                    return;
                }

                if (string.IsNullOrEmpty(txtSchDateFrom.Text))
                {
                    Util.ShowError("Start Date is required");
                    return;
                }
                if (string.IsNullOrEmpty(txtSchDateTo.Text))
                {
                    Util.ShowError("End Date is required");
                    return;
                }
                if (string.IsNullOrEmpty(txtFrecuency.Text))
                {
                    Util.ShowError("Frecuency field is required");
                    return;
                }
                if (txtSchDateFrom.SelectedDate >= txtSchDateTo.SelectedDate)
                {
                    Util.ShowError("End Date must be older than Start Date");
                    return;
                }

                // CAA [2010/05/05] Valida que se incluyan las columnas necesarias en el reporte
                if (cboToDo.SelectedIndex == 0) //Only BIN
                {
                    if (!Query.Contains("as BinCode"))
                    {
                        Util.ShowError("BinCode column missing.");
                        return;
                    }
                }
                else
                    if (cboToDo.SelectedIndex == 1) // BIN & PRODUCT
                    {
                        if (!Query.Contains("as BinCode") || (!Query.Contains("as Product FROM") && !Query.Contains("as Product,")))
                        {
                            Util.ShowError("BinCode and/or Product columns are missing.");
                            return;
                        }
                    }



                CountSchedule sch = new CountSchedule
                {
                    Start = txtSchDateFrom.SelectedDate.Value, // DateTime.Parse(txtSchDateFrom.Text);
                    Finish = txtSchDateTo.SelectedDate.Value, // DateTime.Parse(txtSchDateTo.Text);
                    NextDateRun = txtSchDateFrom.SelectedDate.Value, //.AddDays(double.Parse(txtFrecuency.Text));
                    IsDone = false,
                    Query = Query,
                    Parameters = queryParam.GetXml(),
                    RepeatEach = short.Parse(txtFrecuency.Text),
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Location = App.curLocation,
                    CountOption = cboToDo.SelectedIndex,
                    Title = txtTitle.Text
                };
                

                sch = serv.SaveCountSchedule(sch);

                Util.ShowMessage("Cycle Count Scheduled.");
            }

            ClosePopup();
        }



        private void NewTask()
        {
            WMSServiceClient serv = new WMSServiceClient();

            DocumentType docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Task } };
            docType.DocTypeID = SDocType.CountTask;

            // CAA [2010/05/05] Valida que se incluyan las columnas necesarias en el reporte
            // Indica si utiliza Product para la programacion del conteo.
            bool useProduct = true;
            if (cboToDo.SelectedIndex == 0) //Only BIN
            {
                useProduct = false;
                if (!products.Columns.Contains("BinCode"))
                {
                    Util.ShowError("BinCode column missing.");
                    return;
                }
            }
            else
                if (cboToDo.SelectedIndex == 1) // BIN & PRODUCT
                {
                    if (!products.Columns.Contains("BinCode") || !products.Columns.Contains("Product"))
                    {
                        Util.ShowError("BinCode and/or Product columns are missing.");
                        return;
                    }
                }

            Document document = new Document
            {
                DocType = docType,
                CrossDocking = false,
                IsFromErp = false,
                Location = App.curLocation,
                Company = App.curCompany,
                Date1 = DateTime.Today,
                CreationDate = DateTime.Now,
                CreatedBy = App.curUser.UserName,
                Notes = cboToDo.SelectedIndex.ToString()
            };
            document = serv.CreateNewDocument(document, true); // SaveDocument(document);


            // Details
            foreach (DataRow row in products.Rows)
            {
                //  ojo...  siempre deben enviar los alias "producto" "binCode" en el reporte !!!
                Product prod = null;
                try
                {
                    if (!string.IsNullOrEmpty(row["Product"].ToString()) && useProduct)
                        prod = serv.GetProduct(new Product { ProductCode = row["Product"].ToString() })[0];
                }
                catch { }

                Bin bin = null;
                try
                {
                    if (!string.IsNullOrEmpty(row["BinCode"].ToString()))
                        bin = serv.GetBin(new Bin { BinCode = row["BinCode"].ToString() })[0];
                }
                catch { }

                //Crea el BinTask en el server
                BinByTask binByTask = new BinByTask
                {
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    Bin = bin,
                    Product = prod,
                    TaskDocument = document,
                    Status = new Status { StatusID = DocStatus.New }

                };

                try { serv.SaveBinByTask(binByTask); }
                catch  {  continue; }                

            }

            Util.ShowMessage("Counting Task " + document.DocNumber + " was created.");
            ClosePopup();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }


        private void ClosePopup()
        {
            ((Popup)((Border)((StackPanel)((StackPanel)((StackPanel)this.Parent).Parent).Parent).Parent).Parent).IsOpen = false;
        }

        
    }
}
