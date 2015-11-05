using Assergs.Windows;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfFront.Common;
using WpfFront.IQ.Models;
using WpfFront.Services;
using WpfFront.Views;
using WpfFront.WMSBusinessService;

namespace WpfFront.Presenters
{

    public interface IConsultaSerialesPresenter
    {
        IConsultaSerialesView View { get; set; }
        ToolWindow Window { get; set; }
    }
    public class ConsultaSerialesPresenter : IConsultaSerialesPresenter
    {
        public IConsultaSerialesView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.
        private String userName = App.curUser.UserName;
        private String user = App.curUser.FirstName + " " + App.curUser.LastName;
        private int cont = 0;

        public ConsultaSerialesPresenter(IUnityContainer container, IConsultaSerialesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ConsultaSerialesModel>();

            #region Metodos

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.CargarDatosAdministrador += new EventHandler<EventArgs>(this.CargarDatosAdministrador);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);
            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.BuscarNombreMaterial += this.OnBuscarNombreMaterial;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

            //ConfirmarMovimiento
            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { BinID = 4 });

            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            View.Model.ListadoEstatusLogPro = service.GetMMaster(new MMaster { MetaType = new MType { Code = "LOGPROSTAT" } });
            View.Model.ListadoPosiciones = service.GetMMaster(new MMaster { MetaType = new MType { Code = "POSICION1" } });
            View.Model.ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ALIADO" } });
            View.Model.ListadoTipoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REMISIONRR" } });
            View.Model.ListadoTipoREC = service.GetMMaster(new MMaster { MetaType = new MType { Code = "REC" } });
            View.Model.ListadoFallas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "FALLADIA" } });
            View.Model.ListadoMotScrap = service.GetMMaster(new MMaster { MetaType = new MType { Code = "MOTSCRAP" } });
            View.Model.ListadoEstadoRR = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADO RR" } });

            DataTable aux = service.DirectSQLQuery("select brand,ProductID,ProductCode,Manufacturer from Master.Product", "", "Master.Product", Local);

            List<WpfFront.WMSBusinessService.Product> list = aux.AsEnumerable()
               .Select(row => new WpfFront.WMSBusinessService.Product
               {
                   ProductID = Convert.ToInt32(row["ProductID"]),
                   Brand = row["brand"].ToString(),
                   ProductCode = row["ProductCode"].ToString(),
                   Manufacturer = row["Manufacturer"].ToString()
               }).ToList<WpfFront.WMSBusinessService.Product>();

            View.Model.ListadoProductos = list;
            /////////////////////////////////////////////////////////////////////////////////////

            View.Model.ListadoEquipos = service.DirectSQLQuery("exec sp_GetProcesos2 'HISTORICOSERIAL'", "", "dbo.movimientoclaro", Local);

            #endregion
        }
        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            View.Ciudad.Text = View.Model.ListadoOrigen.Where(f => f.Code == ((MMaster)View.Origen.SelectedItem).Code.ToString()).First().Code2.ToString();
            View.Centro.Text = View.Model.ListadoOrigen.Where(f => f.Code == ((MMaster)View.Origen.SelectedItem).Code.ToString()).First().Description.ToString();
        }

        private void OnBuscarNombreMaterial(object sender, EventArgs e)
        {
            View.CodigoSAP.Text = View.Model.ListadoProductos.Where(f => f.ProductCode == ((Product)View.NombreMaterial.SelectedItem).ProductCode.ToString()).First().Brand.ToString();
            View.Familia.Text = View.Model.ListadoProductos.Where(f => f.ProductCode == ((Product)View.NombreMaterial.SelectedItem).ProductCode.ToString()).First().Manufacturer.ToString();
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAAdministrador', 'PARA Administrador', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void CargarDatosAdministrador(object sender, EventArgs e)
        {

            DataTable Consulta;
            Consulta = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARDATOSADMIN','" + View.GetSerial1.Text + "'", "", "dbo.EquiposClaro", Local);

            if (Consulta.Rows.Count == 0)
            {
                Consulta = service.DirectSQLQuery("EXEC sp_GetProcesos 'BUSCARDATOSADMINDESPACHO','" + View.GetSerial1.Text + "'", "", "dbo.Despacho_EquiposClaro", Local);
                if (Consulta.Rows.Count == 0)
                {
                    Util.ShowError("El serial no existe dentro del sistema.");
                    return;
                }
                else
                {
                    //View.EstadoSerial.Text = Consulta.Rows[0]["Estado"].ToString();

                    View.Mac.Text = Consulta.Rows[0]["Mac"].ToString();
                    View.NombreMaterial.Text = Consulta.Rows[0]["NombreMaterial"].ToString();
                    View.TipoOrigen.Text = Consulta.Rows[0]["TipoOrigen"].ToString();
                    View.Ciudad.Text = Consulta.Rows[0]["Ciudad"].ToString();
                    View.Origen.Text = Consulta.Rows[0]["Origen"].ToString();
                    View.CodigoSAP.Text = Consulta.Rows[0]["CodigoSAP"].ToString();
                    View.EstadoRR.Text = Consulta.Rows[0]["EstadoRR"].ToString();
                    View.TipoRecibo.Text = Consulta.Rows[0]["TipoRecibo"].ToString();
                    View.FechaIngreso.Text = Consulta.Rows[0]["FechaIng"].ToString();
                    View.DocIngreso.Text = Consulta.Rows[0]["DocIng"].ToString();
                    View.FechaDoc.Text = Consulta.Rows[0]["FechaDoc"].ToString();
                    View.Centro.Text = Consulta.Rows[0]["Centro"].ToString();
                    View.Familia.Text = Consulta.Rows[0]["Familia"].ToString();
                    View.Consecutivo.Text = Consulta.Rows[0]["Consecutivo"].ToString();
                    View.EstadoSerial.Text = Consulta.Rows[0]["EstadoSerial"].ToString();
                    View.IdPallet.Text = Consulta.Rows[0]["IdPallet"].ToString();
                    View.Ubicacion.Text = Consulta.Rows[0]["Ubicacion"].ToString();
                    View.CodigoEmpaque.Text = Consulta.Rows[0]["CodEmpaque"].ToString();
                    View.CodigoEmpaque2.Text = Consulta.Rows[0]["CodEmpaque2"].ToString();
                    View.Posicion.Text = Consulta.Rows[0]["Posicion"].ToString();
                    View.FallaDiagnostico.Text = Consulta.Rows[0]["FallaDiag"].ToString();
                    View.TecnicoAsigDiag.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                    View.EstatusDiagnostico.Text = Consulta.Rows[0]["EstatusDiag"].ToString();
                    View.TecnicoAsignadoRep.Text = Consulta.Rows[0]["TecnicoRep"].ToString();
                    View.EstadoReparacion.Text = Consulta.Rows[0]["EstatusRep"].ToString();
                    View.FallaReparacion.Text = Consulta.Rows[0]["FallaRep"].ToString();
                    View.FallaReparacion1.Text = Consulta.Rows[0]["FallaRep1"].ToString();
                    View.FallaReparacion2.Text = Consulta.Rows[0]["FallaRep2"].ToString();
                    View.FallaReparacion3.Text = Consulta.Rows[0]["FallaRep3"].ToString();
                    View.FallaReparacion4.Text = Consulta.Rows[0]["FallaRep4"].ToString();
                    View.MotivoScrap.Text = Consulta.Rows[0]["MotScrap"].ToString();
                    View.FallaVerificacion.Text = Consulta.Rows[0]["FallaVerif"].ToString();
                    View.StatusVerificacion.Text = Consulta.Rows[0]["EstatusVerif"].ToString();
                    View.FechaDespacho.Text = Consulta.Rows[0]["FechaDespacho"].ToString();
                }
            }
            else
            {
                //View.EstadoSerial.Text = Consulta.Rows[0]["Estado"].ToString();

                View.Mac.Text = Consulta.Rows[0]["Mac"].ToString();
                View.NombreMaterial.Text = Consulta.Rows[0]["NombreMaterial"].ToString();
                View.TipoOrigen.Text = Consulta.Rows[0]["TipoOrigen"].ToString();
                View.Ciudad.Text = Consulta.Rows[0]["Ciudad"].ToString();
                View.Origen.Text = Consulta.Rows[0]["Origen"].ToString();
                View.CodigoSAP.Text = Consulta.Rows[0]["CodigoSAP"].ToString();
                View.EstadoRR.Text = Consulta.Rows[0]["EstadoRR"].ToString();
                View.TipoRecibo.Text = Consulta.Rows[0]["TipoRecibo"].ToString();
                View.FechaIngreso.Text = Consulta.Rows[0]["FechaIng"].ToString();
                View.DocIngreso.Text = Consulta.Rows[0]["DocIng"].ToString();
                View.FechaDoc.Text = Consulta.Rows[0]["FechaDoc"].ToString();
                View.Centro.Text = Consulta.Rows[0]["Centro"].ToString();
                View.Familia.Text = Consulta.Rows[0]["Familia"].ToString();
                View.Consecutivo.Text = Consulta.Rows[0]["Consecutivo"].ToString();
                View.EstadoSerial.Text = Consulta.Rows[0]["EstadoSerial"].ToString();
                View.IdPallet.Text = Consulta.Rows[0]["IdPallet"].ToString();
                View.Ubicacion.Text = Consulta.Rows[0]["Ubicacion"].ToString();
                View.CodigoEmpaque.Text = Consulta.Rows[0]["CodEmpaque"].ToString();
                View.CodigoEmpaque2.Text = Consulta.Rows[0]["CodEmpaque2"].ToString();
                View.Posicion.Text = Consulta.Rows[0]["Posicion"].ToString();
                View.FallaDiagnostico.Text = Consulta.Rows[0]["FallaDiag"].ToString();
                View.TecnicoAsigDiag.Text = Consulta.Rows[0]["TecnicoDiag"].ToString();
                View.EstatusDiagnostico.Text = Consulta.Rows[0]["EstatusDiag"].ToString();
                View.TecnicoAsignadoRep.Text = Consulta.Rows[0]["TecnicoRep"].ToString();
                View.EstadoReparacion.Text = Consulta.Rows[0]["EstatusRep"].ToString();
                View.FallaReparacion.Text = Consulta.Rows[0]["FallaRep"].ToString();
                View.FallaReparacion1.Text = Consulta.Rows[0]["FallaRep1"].ToString();
                View.FallaReparacion2.Text = Consulta.Rows[0]["FallaRep2"].ToString();
                View.FallaReparacion3.Text = Consulta.Rows[0]["FallaRep3"].ToString();
                View.FallaReparacion4.Text = Consulta.Rows[0]["FallaRep4"].ToString();
                View.MotivoScrap.Text = Consulta.Rows[0]["MotScrap"].ToString();
                View.FallaVerificacion.Text = Consulta.Rows[0]["FallaVerif"].ToString();
                View.StatusVerificacion.Text = Consulta.Rows[0]["EstatusVerif"].ToString();
                View.FechaDespacho.Text = Consulta.Rows[0]["FechaDespacho"].ToString();
            }

        }

        private void OnConfirmarRecibo(object sender, EventArgs e)
        {
            View.Ubicacion.Text = View.Model.ListadoEstatusLogPro.Where(f => f.Code == ((MMaster)View.EstadoSerial.SelectedItem).Code.ToString()).First().Description.ToString();
        }

        private void OnAddLine(object sender, EventArgs e)
        {
            if (((MMaster)View.FallaVerificacion.SelectedItem).Code.ToString() == "SIN FALLA")
                View.StatusVerificacion.Text = "VERIFICADO";
            else
                View.StatusVerificacion.Text = "DAÑADO";
        }

        public void OnConfirmarMovimiento(object sender, EventArgs e)
        {
            if (((MMaster)View.FallaDiagnostico.SelectedItem).Code.ToString() == "SIN FALLA")
                View.EstatusDiagnostico.Text = "BUEN ESTADO";
            else
                View.EstatusDiagnostico.Text = "MAL ESTADO";
        }
    }
}
