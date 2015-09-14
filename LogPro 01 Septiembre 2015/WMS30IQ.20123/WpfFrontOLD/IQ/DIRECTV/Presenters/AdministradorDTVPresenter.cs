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
using WpfFront.Common.WFUserControls;
using Microsoft.Windows.Controls;
using WpfFront.Common.Windows;
using System.Windows.Input;

namespace WpfFront.Presenters
{

    public interface IAdministradorDTVPresenter
    {
        IAdministradorDTVView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class AdministradorDTVPresenter : IAdministradorDTVPresenter
    {
        public IAdministradorDTVView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares 
        public Connection Local;
        public int offset = 5; //# columnas que no se debe replicar porque son fijas.

        public AdministradorDTVPresenter(IUnityContainer container, IAdministradorDTVView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AdministradorDTVModel>();


            #region Metodos

            //View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            //View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.CargarDatosAdministradorDTV += new EventHandler<EventArgs>(this.CargarDatosAdministradorDTV);
            view.CargaMasiva += new EventHandler<DataEventArgs<DataTable>>(this.OnCargaMasiva);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ConfirmarMovimiento += new EventHandler<EventArgs>(this.OnConfirmarMovimiento);

            //Recibo
            View.BuscarRegistrosRecibo += this.OnBuscarRegistrosRecibo;
            View.ActualizarRegistrosRecibo += this.OnActualizarRegistrosRecibo;
            View.ConfirmarRecibo += this.OnConfirmarRecibo;

            View.ConsultarMovimientos += new EventHandler<EventArgs>(this.OnConsultarMovimientos);
            View.BuscarEquipoTracking += new EventHandler<EventArgs>(this.OnBuscarEquipoTracking);
            View.ReiniciarCapacitacion += new EventHandler<EventArgs>(this.OnReiniciarCapacitacion);

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
            View.Model.ListadoModelos = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DRIRECMODE" } });
            View.Model.ListadoOrigen = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TIPOORIGEND" } });
            View.Model.ListadoCiudades = service.GetMMaster(new MMaster { MetaType = new MType { Code = "CIUDAD" } });
            View.Model.ListadoTipoDev = service.GetMMaster(new MMaster { MetaType = new MType { Code = "TIPODEVDTV" } });
            View.Model.ListadoEstMaterial = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ESTADOMAT" } });
            View.Model.ListadoFallas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "DTVCFALLAR" } });
            View.Model.ListadoMotScrap = service.GetMMaster(new MMaster { MetaType = new MType { Code = "MOTSCRAP" } });
            View.Model.ListadoSINO = service.GetMMaster(new MMaster { MetaType = new MType { Code = "SI _NO" } });

            View.Model.ListadoEquipos = service.DirectSQLQuery("exec sp_GetProcesosDIRECTV2 'HISTORICOSERIAL'", "", "dbo.movimientoDIRECTV", Local);

            #endregion
        }


        //Recibo
        private void OnBuscarRegistrosRecibo(object sender, EventArgs e)
        {
            View.Descripcion.Text = View.Model.ListadoModelos.Where(f => f.Code == ((MMaster)View.Modelo.SelectedItem).Code.ToString()).First().Description.ToString();
            //View.CiudadDealer.Text = View.Model.ListadoDealers.Where(f => f.Code == ((MMaster)View.Dealer.SelectedItem).Code.ToString()).First().Description.ToString();
        }

        public void BuscarRegistrosRecibo()
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAAdministradorDTV', 'PARA AdministradorDTV'";

            //Valido si fue digitado una estiba para buscar
            //if (!String.IsNullOrEmpty(View.BuscarEstibaRecibo.Text.ToString()))
            //    ConsultaSQL += ",'" + View.BuscarEstibaRecibo.Text.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            //Valido si fue seleccionado ubicaciones para filtrar
            //if (View.BuscarPosicionRecibo.SelectedIndex != -1)
            //    ConsultaSQL += ",'" + ((MMaster)View.BuscarPosicionRecibo.SelectedItem).Code.ToString() + "'";
            //else
            //    ConsultaSQL += ",NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void OnActualizarRegistrosRecibo(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaSQL;

            //Creo la consulta para buscar los registros
            ConsultaSQL = "EXEC sp_GetProcesos 'BUSCARMERCANCIAAdministradorDTV', 'PARA AdministradorDTV', NULL, NULL";

            //Ejecuto la consulta
            View.Model.ListadoRecibo = service.DirectSQLQuery(ConsultaSQL, "", "dbo.EquiposCLARO", Local);
        }

        private void CargarDatosAdministradorDTV(object sender, EventArgs e)
        {

            DataTable Consulta;
            Consulta = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTVC 'BUSCARDATOSADMIN','" + View.GetSerial1.Text + "'", "", "dbo.EquiposDIRECTVC", Local);

            if (Consulta.Rows.Count == 0)
            {
                Util.ShowError("El serial no existe dentro del sistema.");
                return;
            }
            else
            {
                View.Serial.Text = Consulta.Rows[0]["Serial"].ToString();
                View.IdReceiver.Text = Consulta.Rows[0]["Receiver"].ToString();
                View.SmartCardEntrada.Text = Consulta.Rows[0]["SmartCard"].ToString();
                View.EstadoSerial.Text = Consulta.Rows[0]["Estado"].ToString();
                View.IdPallet.Text = Consulta.Rows[0]["IdPallet"].ToString();
                View.Ubicacion.Text = Consulta.Rows[0]["Ubicacion"].ToString();
                View.CodigoEmpaque.Text = Consulta.Rows[0]["CodigoEmpaque1"].ToString();
                View.CodigoEmpaque2.Text = Consulta.Rows[0]["CodigoEmpaque2"].ToString();
                View.Posicion.Text = Consulta.Rows[0]["Posicion"].ToString();
                View.FechaIngreso.Text = Consulta.Rows[0]["FechaIng"].ToString();
                View.Modelo.Text = Consulta.Rows[0]["Modelo"].ToString();
                View.Origen.Text = Consulta.Rows[0]["Origen"].ToString();
                View.Ciudad.Text = Consulta.Rows[0]["Ciudad"].ToString();
                View.TipoDevolucion.Text = Consulta.Rows[0]["TipoDev"].ToString();
                View.Descripcion.Text = Consulta.Rows[0]["Descripcion"].ToString();
                View.DocIngreso.Text = Consulta.Rows[0]["DocIngreso"].ToString();
                View.FechaDoc.Text = Consulta.Rows[0]["FechaDoc"].ToString();
                View.DOA.Text = Consulta.Rows[0]["Doa"].ToString();
                View.EstadoMaterial.Text = Consulta.Rows[0]["EstadoMaterial"].ToString();
                View.TipoDiagnostico.Text = Consulta.Rows[0]["TipoDiagnostico"].ToString();
                View.FallaDiagnostico.Text = Consulta.Rows[0]["FallaDiag"].ToString();
                View.EstatusDiagnostico.Text = Consulta.Rows[0]["EstatusDiag"].ToString();
                View.TecnicoAsignadoRep.Text = Consulta.Rows[0]["TecAsignadoRep"].ToString();
                View.EstadoReparacion.Text = Consulta.Rows[0]["EstatusRep"].ToString();
                View.FallaReparacion.Text = Consulta.Rows[0]["FallaRep"].ToString();
                View.FallaReparacion1.Text = Consulta.Rows[0]["FallaRep1"].ToString();
                View.FallaReparacion2.Text = Consulta.Rows[0]["FallaRep2"].ToString();
                View.FallaReparacion3.Text = Consulta.Rows[0]["FallaRep3"].ToString();
                View.FallaReparacion4.Text = Consulta.Rows[0]["FallaRep4"].ToString();
                View.MotivoScrap.Text = Consulta.Rows[0]["MotivoScrap"].ToString();
                View.SmartCardSalida.Text = Consulta.Rows[0]["SmartCardSalida"].ToString();
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

        private void OnCargaMasiva(object sender, DataEventArgs<DataTable> e)
        {
            //Variables Auxiliares
            DataRow dr1;
            int NumeroSerial;

            foreach (DataRow dr in e.Value.Rows)
            {
                dr1 = View.Model.ListRecords.NewRow();

                //Asigno los campos
                /*dr1[0] = View.Model.ProductoSerial.ProductID;
                dr1[1] = View.Model.ProductoSerial.Name;*/

                NumeroSerial = 1;
                foreach (DataColumn dc in e.Value.Columns)
                {
                    switch (NumeroSerial.ToString())
                    {
                        case "1":
                            dr1[3] = dr[dc.ColumnName].ToString();
                            break;
                        case "2":
                            dr1[4] = dr[dc.ColumnName].ToString();
                            break;
                        case "3":
                            dr1[5] = dr[dc.ColumnName].ToString();
                            break;

                    }
                    NumeroSerial++;
                }

                //Agrego el registro al listado
                View.Model.ListRecords.Rows.Add(dr1);
            }
        }


        private void OnReplicateDetails(object sender, EventArgs e)
        {
            //Recorre la primera linea y con esa setea el valor de las demas lineas.
            for (int i = 1; i < View.Model.ListRecords.Rows.Count; i++)
                for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                    View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[0][z];
        }

        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Variables Auxiliares
            String ConsultaGuardar = "";

            try
            {
                ConsultaGuardar += "";

                //Construyo la consulta para guardar los datos
                ConsultaGuardar += " UPDATE dbo.EquiposDIRECTVC SET SERIAL = '" + View.Serial.Text.ToString() + "', RECEIVER = '" + View.IdReceiver.Text.ToString() + "', SMART_CARD_ENTRADA = '" + View.SmartCardEntrada.Text.ToString() + "'";
                ConsultaGuardar += ", Estado = '" + View.EstadoSerial.Text.ToString() + "', IDPALLET = '" + View.IdPallet.Text.ToString() + "'";
                ConsultaGuardar += ", UBICACION = '" + View.Ubicacion.Text.ToString() + "', CodigoEmpaque = '" + View.CodigoEmpaque.Text.ToString() + "'";
                ConsultaGuardar += ", CodigoEmpaque2 = '" + View.CodigoEmpaque2.Text.ToString() + "', Posicion = '" + View.Posicion.Text.ToString() + "'";
                ConsultaGuardar += ",  Modelo = '" + View.Modelo.Text.ToString() + "'";
                ConsultaGuardar += ", TIPO_ORIGEN = '" + View.Origen.Text.ToString() + "', Ciudad = '" + View.Ciudad.Text.ToString() + "'";
                ConsultaGuardar += ", TIPOS_DEVOLUCIONES = '" + View.TipoDevolucion.Text.ToString() + "', DESCRIPCION = '" + View.Descripcion.Text.ToString() + "'";
                ConsultaGuardar += ", DOC_INGRESO = '" + View.DocIngreso.Text.ToString() + "'";
                ConsultaGuardar += ", DOA = '" + View.DOA.Text.ToString() + "', ESTADO_MATERIAL = '" + View.EstadoMaterial.Text.ToString() + "'";
                ConsultaGuardar += ", TIPO_DIAGNOSTICO = '" + View.TipoDiagnostico.Text.ToString() + "', FALLA_DIAGNOSTICO = '" + View.FallaDiagnostico.Text.ToString() + "'";
                ConsultaGuardar += ", ESTATUS_DIAGNOSTICO = '" + View.EstatusDiagnostico.Text.ToString() + "', TECNICO_REPARACION = '" + View.TecnicoAsignadoRep.Text.ToString() + "'";
                ConsultaGuardar += ", ESTATUS_REPARACION = '" + View.EstadoReparacion.Text.ToString() + "', FALLA_REP = '" + View.FallaReparacion.Text.ToString() + "'";
                ConsultaGuardar += ", FALLA_REP1 = '" + View.FallaReparacion1.Text.ToString() + "', FALLA_REP2 = '" + View.FallaReparacion2.Text.ToString() + "'";
                ConsultaGuardar += ", FALLA_REP3 = '" + View.FallaReparacion3.Text.ToString() + "', FALLA_REP4 = '" + View.FallaReparacion4.Text.ToString() + "'";
                ConsultaGuardar += ", FALLA_EQUIPO_VERIF = '" + View.FallaVerificacion.Text.ToString() + "', ESTATUS_VERIFICACION = '" + View.StatusVerificacion.Text.ToString() + "'";
                ConsultaGuardar += " WHERE Serial = '" + View.GetSerial1.Text.ToString() + "'";


                //Ejecuto la consulta
                service.DirectSQLNonQuery(ConsultaGuardar, Local);

                //Limpio la consulta para volver a generar la nueva
                ConsultaGuardar = "";


                //Evaluo si la consulta no envio los ultimos registros para forzar a enviarlos
                if (!String.IsNullOrEmpty(ConsultaGuardar))
                {
                    Console.WriteLine(ConsultaGuardar);
                    //Ejecuto la consulta
                    service.DirectSQLNonQuery(ConsultaGuardar, Local);

                    //Limpio la consulta para volver a generar la nueva
                    ConsultaGuardar = "";
                }

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Registros guardados satisfactoriamente.");

                //Reinicio los campos
                LimpiarDatosIngresoSeriales();

                return;
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error al momento de guardar los registros. Error: " + Ex.Message); }
        }

        public void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            //View.Model.ListRecords.Rows.Clear();
        }

        public void OnConsultarMovimientos(object sender, EventArgs e)
        {
            //Ejecuto la consulta
            //try
            //{
            String serial = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["Serial"].ToString();
            String receiver = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["Receiver"].ToString();
            String estado_actual = ((DataRowView)View.ListadoEquipos_Track.SelectedItem).Row["estado_actual"].ToString();

            if (serial != null && receiver != null)
            {
                DataTable datos_grafica;

                if (estado_actual == "ACTIVO")
                {
                    datos_grafica = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV2 'AGING_BYAREA','" + serial + "','" + receiver + "'", "", "dbo.movimientoDIRECTV", Local);
                }
                else
                {
                    datos_grafica = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV2 'AGING_BYAREA_DESPACHO','" + serial + "','" + receiver + "'", "", "dbo.movimientoDIRECTV", Local);
                }

                

                View.Model.ListMovimientos = service.DirectSQLQuery("EXEC sp_GetProcesosDIRECTV2 'TRACK_BYSERIAL','" + serial + "','" + receiver + "'", "", "dbo.movimientoDIRECTV", Local);
                List<String> colores = new List<String>(new String[] { "#21E10D", "#E60E0E", "#FA9D0A", "#7B5FBB", "#4CD8D8", "#0099FF", "#41F717" });

                View.chart1.Series.Clear();
                View.chart1.ChartAreas.Clear();

                View.chart1.Series.Add("S");
                View.chart1.ChartAreas.Add("A");

                View.chart1.Series["S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;

                int cont = 0;
                foreach (DataRow fila in datos_grafica.Rows)
                {
                    View.chart1.Series["S"].Points.AddXY(fila["mov_codigoUbicacion"].ToString() + " " + fila["tiempo"].ToString(), fila["segundos"]);
                    View.chart1.Series["S"].Points[cont].Color = System.Drawing.ColorTranslator.FromHtml(colores[cont]);
                    cont++;
                }

            }


        }

        public void OnBuscarEquipoTracking(object sender, EventArgs e)
        {

            String serial_track = View.GetSerialTrack.Text;
            String mac_track = View.GetMac_Track.Text;

            try
            {
                View.Model.ListadoEquipos = service.DirectSQLQuery("exec sp_GetProcesosDIRECTV2 'HISTORIAL_BYSERIAL','" + serial_track + "','" + mac_track + "'", "", "dbo.movimientoclaro", Local);
                View.Model.ListMovimientos = service.DirectSQLQuery("exec sp_GetProcesosDIRECTV2 'TRACK_BYSERIAL','" + serial_track + "','" + mac_track + "'", "", "dbo.movimientoclaro", Local);

                View.ListadoEquipos_Track.SelectedIndex = 0;
            }
            catch (Exception ex)
            {

            }
        }

        public void OnReiniciarCapacitacion(object sender, EventArgs e)
        {
            String ConsultaSQL = "EXEC ClearDIRECTV";
            try { 
                service.DirectSQLNonQuery(ConsultaSQL, Local);
                Util.ShowMessage("Capacitacion Reiniciada!!!");
            }catch(Exception ex){
                Util.ShowError(ex.Message);
            }
            
        }

    }



}