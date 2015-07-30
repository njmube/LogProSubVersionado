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
using System.Data;
using System.Reflection;
using Microsoft.Windows.Controls;
using WpfFront.Common.Windows;

namespace WpfFront.Presenters
{

    public interface IValidadorSerialesPresenter
    {
        IValidadorSerialesView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class ValidadorSerialesPresenter : IValidadorSerialesPresenter
    {
        public IValidadorSerialesView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        public int offset = 3; //# columnas que no se debe replicar porque son fijas.  
        public Connection Local;

        public ValidadorSerialesPresenter(IUnityContainer container, IValidadorSerialesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ValidadorSerialesModel>();

            #region Metodos

            #region Header

            View.BuscarSeriales += new EventHandler<EventArgs>(this.OnBuscarSeriales);

            #endregion

            #region Details

            View.EliminarSeriales += new EventHandler<EventArgs>(this.OnEliminarSeriales);

            #endregion

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document { CreatedBy = App.curUser.UserName };
            View.Model.LocationFromList = App.curUser.UserRols.Select(f => f.Location).Distinct().ToList();
            View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
            View.Model.ListDataInformation = new List<DataInformation>();

            #endregion

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

        }


        #region Header

        private void OnBuscarSeriales(object sender, EventArgs e)
        {
            //Variables auxiliares.
            WMSBusinessService.Location Cliente;
            string NumeroDelDocumento = "";
            DateTime? FechaUno;
            DateTime? FechaDos;
            WMSBusinessService.Document Documento = null;

            //Evaluo si fue seleccionado un Cliente para busqueda
            if (View.comboCliente.SelectedIndex != -1)
            {
                Cliente = (WMSBusinessService.Location)View.comboCliente.SelectedItem;
            }
            else
            {
                //Cliente = null;
                Util.ShowError("Debe seleccionar el cliente.");
                return;
            }

            //Evaluo si hay un numero de documento ingresado.
            if (View.NumeroDocumento.Text != "")
            {
                NumeroDelDocumento = View.NumeroDocumento.Text;
                try
                {
                    Documento = service.GetDocument(new WMSBusinessService.Document() { DocNumber = NumeroDelDocumento }).First();
                }
                catch (Exception)// ex)
                {
                    //Util.ShowError("Hubo un error al cargar el tipo de documento");
                    //return;
                    Documento = null;
                }
            }
            else
                NumeroDelDocumento = null;

            //Evaluar fecha 1
            if (View._FechaDesde.SelectedDate != null)
                FechaUno = View._FechaDesde.SelectedDate;
            else
                FechaUno = null;

            //Evaluar fecha 2
            if (View._FechaHasta.SelectedDate != null)
                FechaDos = View._FechaHasta.SelectedDate;
            else
                FechaDos = null;

            //La lista con los resultados.
            View.Model.ListaSerialesNoCargados = service.GetLabel(new WMSBusinessService.Label
            {
                CreTerminal = Cliente.LocationID.ToString(),
                Node = new Node { NodeID = NodeType.PreLabeled },
                ReceivingDocument = Documento
            }).Where(f => f.Node.NodeID == 1 && f.CreTerminal == Cliente.LocationID.ToString()).ToList();
            //&& f.CreationDate == FechaUno && f.CreationDate <= FechaDos
        }

        #endregion

        #region Details

        private void OnEliminarSeriales(object sender, EventArgs e)
        {
            //Variables auxiliares
            string CadenaQuery;

            //Valido que haya seriales seleccionados para eliminar
            if (View.LvDocumentMaster.SelectedItems.Count <= 0)
            {
                Util.ShowMessage("No hay seriales seleccionados para su eliminacion.");
                return;
            }
            else
            {
                //Recorro los seriales seleccionados
                foreach (WMSBusinessService.Label Equipo in View.LvDocumentMaster.SelectedItems)
                {
                    try
                    {
                        //Construyo el Query
                        CadenaQuery = "EXEC spAdminSeriales 1, '" + Equipo.LabelID + "'";

                        //Ejecutamos el procedimiento
                        service.DirectSQLNonQuery(CadenaQuery, Local);

                    }
                    catch (Exception ex)
                    {
                        Util.ShowError("Hubo un error al eliminar el serial'. Error: " + ex.Message);
                        return;
                    }
                }

            }

            Util.ShowMessage("Seriales eliminados correctamente.");
            OnBuscarSeriales(sender, e);
            View.LvDocumentMaster.Items.Refresh();
        }

        #endregion

    }
}