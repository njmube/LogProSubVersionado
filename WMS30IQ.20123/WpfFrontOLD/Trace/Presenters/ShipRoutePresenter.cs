using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;

namespace WpfFront.Presenters
{

    public interface IShipRoutePresenter
    {
        IShipRouteView View { get; set; }
        ToolWindow Window { get; set; }
    }

    public class ShipRoutePresenter: IShipRoutePresenter
    {
        public IShipRouteView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        DocumentType docType;


        public ShipRoutePresenter(IUnityContainer container, IShipRouteView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ShipRouteModel>();

            //Load Documents
            View.LoadDocuments += new EventHandler<EventArgs>(View_LoadDocuments);
            View.ProcessLines += new EventHandler<EventArgs>(View_ProcessLines);
            View.LoadOpenProcess += new EventHandler<DataEventArgs<Document>>(View_LoadOpenProcess);
            View.ShowTicket += new EventHandler<DataEventArgs<Document>>(View_ShowTicket);
            View.UpdateDriver += new EventHandler<DataEventArgs<string>>(view_UpdateDriver);
            View.CreateShipment += new EventHandler<EventArgs>(View_CreateShipment);
            View.ShowShipTkt += new EventHandler<EventArgs>(View_ShowShipTkt);

            //Inicializo las variables

            ProcessWindow pw = new ProcessWindow("Cargando Vista ...");

            docType = new DocumentType { DocClass = new DocumentClass { DocClassID = SDocClass.Shipping } };

            //Load Dates
            LoadDates();
            //Load Routes
            LoadRoutes();
            //Load Drivers
            LoadDrivers();

            LoadOpenDocuments();

            try { View.CboLocation.SelectedItem = App.curLocation; }
            catch { }

            pw.Close();

        }

        private void LoadOpenDocuments()
        {
            //Lista de Documentos en Proceso
            View.Model.OpenDocList = service.GetPendingDocument(
                new Document
                {
                    DocType = new DocumentType { DocTypeID = SDocType.MergedSalesOrder },
                    Company = App.curCompany,
                    //Location = App.curLocation
                    Location = (Location)View.CboLocation.SelectedItem
                }, 0, 0)
            .OrderBy(f => f.DocNumber).ToList();

        }

        void View_ShowShipTkt(object sender, EventArgs e)
        {
            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"));

            Document document = service.GetDocument(new Document { CustPONumber = View.Model.CurDoc.DocNumber }).First();

            UtilWindow.ShowDocument(document.DocType.Template, document.DocID, "", true);

            pw.Close();
        }



        void View_ShowTicket(object sender, DataEventArgs<Document> e)
        {
            ShowTicket(e.Value);
        }



        private void ShowTicket(Document document)
        {
            if (document == null)
                return;

            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("GENERATING_DOCUMENT"));

            UtilWindow.ShowDocument(document.DocType.Template, document.DocID, "", true);

            pw.Close();
        }



        void View_CreateShipment(object sender, EventArgs e)
        {
            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("CREATING_SALES_SHIPMENT"));

            Document salesDocToFill = View.Model.CurDoc;
            Document shipment = null;

            //Llamdo proceso de creacion del Shipment
            try
            {
                shipment = service.CreateShipmentDocument(salesDocToFill);
            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError(Util.GetResourceLanguage("SHIPMENT_COULD_NOT_BE_CREATED") + "\n" + ex.Message);
                return;
            }
        }



        void view_UpdateDriver(object sender, DataEventArgs<string> e)
        {
            //if (string.IsNullOrEmpty(e.Value))
            //    return;
            try
            {
                View.Model.CurDoc.UserDef1 = e.Value;
                service.UpdateDocument(View.Model.CurDoc);
                Util.ShowMessage("El Documento fue Actualizado.");
            }
            catch { }
        }

        private void LoadDrivers()
        {
            View.UcDriver.DefaultList = service.GetCustomListV2("SELECT Distinct UserDef1, UserDef1 FROM Trace.Document WHERE DocTypeID=206 AND ISNULL(UserDef1,'') != ''");
        }



        void View_LoadOpenProcess(object sender, DataEventArgs<Document> e)
        {

            if (e.Value.DocID == 0)
            {
                View.Model.CurDoc = null;
                View.CboDate.SelectedIndex = 0;
                View.CboRoute.SelectedIndex = 0;
                View.DocNumberSearch.Text = "";
                LoadDocuments();
                return;
            }


            View.StkMain.Visibility = Visibility.Collapsed;

            ProcessWindow pw = new ProcessWindow("Cargando Documento " + e.Value.DocNumber +" ...");

            View.Model.DocumentList = service.GetDocument(
                new Document
                {
                    Company = App.curCompany,
                    //Location = App.curLocation,
                    Location = (Location)View.CboLocation.SelectedItem,
                    QuoteNumber = "M",
                    Reference = e.Value.DocNumber
                })
                .ToList();

            foreach (Document dx in View.Model.DocumentList)
                dx.AllowPartial = false;

            if (View.Model.DocumentList.Count == 0)
            {
                Util.ShowError("No se encontraron documentos para este Alistamiento.");
                return;
            }

            View.StkMain.Visibility = Visibility.Visible;
            View.Model.CurDoc = e.Value;


            //Lo que se debe ver
            View.BrdDocuments.Visibility = Visibility.Visible;
            View.BtnRemoveL.Visibility = Visibility.Visible;
            View.StkCreateP.Visibility = Visibility.Collapsed;

            pw.Close();

        }



        void View_ProcessLines(object sender, EventArgs e)
        {
            ProcessWindow pw = new ProcessWindow("Procesando Documentos para crear Planilla de Alistamiento ...");

            try
            {

                //Alerta de Confirmacion.
                //Crear un nuevo documento con las lineas agrupadas por producto.
                Document document = new Document
                    {
                        DocType = new DocumentType { DocTypeID = SDocType.MergedSalesOrder },
                        CreatedBy = App.curUser.UserName,
                        CreationDate = DateTime.Now,
                        //Location = App.curLocation,
                        Location = (Location)View.CboLocation.SelectedItem,
                        Company = App.curCompany,
                        IsFromErp = true,
                        CrossDocking = false,
                        Date1 = DateTime.Now,
                        UseAllocation = true,
                        ShippingMethod = (ShippingMethod)View.CboProcessRoute.SelectedItem,
                        QuoteNumber = "P"
                    };



                document = service.ConsolidateOrdersInNewDocument(document, View.Model.DocumentList.Where(f => f.AllowPartial == true).ToList());

                Util.ShowMessage("Planilla " + document.DocNumber + " creada");

                LoadDocuments();
                LoadOpenDocuments();

            }
            catch (Exception ex)
            {
                Util.ShowError("No se pudo crear el documento de alistamiento.\n" + ex.Message);
            }
            finally { pw.Close(); }
        }



        private void LoadRoutes()
        {
            View.Model.RouteList = service.GetShippingMethod(new ShippingMethod());
            View.Model.RouteList.Insert(0, new ShippingMethod {Name = " ... " });
        }



        private void LoadDates()
        {
            View.Model.DateList = new List<ShowData>();

            DateTime curDate;


            View.Model.DateList.Add(new ShowData
            {
                DataKey = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                DataValue = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") + " (Ayer)"
            });


            View.Model.DateList.Add(new ShowData
            {
                DataKey = DateTime.Today.ToString("yyyy-MM-dd"),
                DataValue = DateTime.Today.ToString("yyyy-MM-dd") + " (Hoy)"
            });


            View.Model.DateList.Add(new ShowData
            {
                DataKey = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                DataValue = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + " (Mañana)"
            });



            for (int i = 2; i < 4; i++)
            {
                curDate = DateTime.Today.AddDays(-1 * i);
                View.Model.DateList.Add(new ShowData { 
                    DataKey = curDate.ToString("yyyy-MM-dd"), 
                    DataValue = curDate.ToString("yyyy-MM-dd") });
            }

            for (int i = 2; i <= 3; i++)
            {
                curDate = DateTime.Today.AddDays(i);
                View.Model.DateList.Add(new ShowData { 
                    DataKey = curDate.ToString("yyyy-MM-dd"), 
                    DataValue = curDate.ToString("yyyy-MM-dd") });
            }

            View.Model.DateList = View.Model.DateList.OrderBy(f => f.DataKey).ToList();
            View.Model.DateList.Insert(0, new ShowData { DataKey = "", DataValue = " ... " });

            View.CboDate.Items.Refresh();
        }



        void View_LoadDocuments(object sender, EventArgs e)
        {
           
            View.StkMain.Visibility = Visibility.Collapsed;
            View.Model.CurDoc = null;

            ProcessWindow pw = new ProcessWindow("Cargando Pedidos Pendientes ...");

            LoadDocuments();

            if (!View.Model.OpenDocList.Any(f=>f.DocID == 0))
                View.Model.OpenDocList.Insert(0,new Document { DocNumber = "NUEVO DOCUMENTO ..." });

            View.StkMain.Visibility = Visibility.Visible;

            pw.Close();
        }

        private void LoadDocuments()
        {


            //Filtro del Date Time
            ShowData xDateTime = View.CboDate.SelectedItem as ShowData;
            DateTime? curDate = null;
            if (xDateTime != null && xDateTime.DataKey != "")
                curDate = (DateTime?)Convert.ToDateTime(xDateTime);

            //if (curDate == null)
            //{
            //    Util.ShowError("La Fecha de Despacho es obligatoria en la consulta.");
            //    return;
            //}

            ShippingMethod curMethod = View.CboRoute.SelectedItem == null ? null : (ShippingMethod)View.CboRoute.SelectedItem;


            double TotWeight=0, TotVolume = 0;


            View.Model.DocumentList = service.GetPendingDocument(
                new Document
                {
                    DocType = docType,
                    ShippingMethod = curMethod,
                    Company = App.curCompany,
                    Date2 = curDate,
                    //Location = App.curLocation
                    Location = (Location)View.CboLocation.SelectedItem
                }, 0, 0)
                .Where(f => f.QuoteNumber != "P" && f.QuoteNumber != "M")
                .ToList();


            foreach (Document dx in View.Model.DocumentList)
            {
                dx.AllowPartial = true;

                //TOTALES
                TotWeight += dx.DocWeight;
                TotVolume += dx.DocVolume;
            }


            View.Model.TotalList = new List<DocumentLine>() { new DocumentLine { LineWeight = TotWeight, 
                LineVolume = TotVolume } };

        }



        private void OnDeleteDocument(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            Util.ShowMessage(e.Value.DocNumber);
        }

        private void OnAddDocument(object sender, DataEventArgs<Document> e)
        {
            if (e.Value == null)
                return;

            Util.ShowMessage(e.Value.DocNumber);
        }


    }
}
