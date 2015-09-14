using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using WpfFront.WMSBusinessService;
using System.Threading;
using System.Reflection;
using Microsoft.Reporting.WinForms;
using System.Data;
using WpfFront.Services;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Collections;

namespace WpfFront.Common
{
    public class PrinterControl
    {

        private static string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string error = "";
        private static string carpetaServidor = "\\\\192.168.2.6\\ZEBRA\\";
        private static string batFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\PRINT.BAT";
        //private static LocalReport localReport = null;

        //Envia un archivo de templates a imprimir
        public static bool PrintLabels(string labelData, string printerPort)
        {
            //TODO: Módulo de impresión
            bool result = false;
            string printPort = printerPort;
            string filePath = CreatePrintTemporaryFile("", labelData);

            if (!File.Exists(batFile))
                throw new Exception("Setup file " + batFile + " does not exists.\n");

            batFile = "\"" + batFile.Replace("\\\\", "\\") + "\""; //Comillas para que DOS lo reconozca

            try
            {
                if (File.Exists(filePath) == false)
                    throw new Exception("Please setup the temporary printing file.");

                ////Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(SendPrintProcess));
                ////Make the thread as background thread.
                objThread.IsBackground = true;
                ////Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                ////Start the thread.
                string printCommand = "\"" + filePath + "\" \"" + printPort + "\"";
                objThread.Start(printCommand);
                //SendPrintProcess(printCommand);
            }
            catch (Exception ex)
            {
                result = true;
                Util.ShowError(ex.Message);
                //throw ex;
            }

            if (!string.IsNullOrEmpty(error))
                Util.ShowError(error);

            return result;
        }

        public static string CreatePrintTemporaryFile(string printLot, string labelData)
        {
            if (string.IsNullOrEmpty(printLot))
                printLot = Guid.NewGuid().ToString();

            string tmpPrintFile = Path.Combine(appPath, WmsSetupValues.PrintReportDir + "\\" + printLot + ".prn");
            tmpPrintFile = tmpPrintFile.Replace("\\\\", "\\");

            StreamWriter writer = new StreamWriter(tmpPrintFile);

            writer.WriteLine(labelData);
            writer.Flush();
            writer.Close();

            return tmpPrintFile;
        }

        private static void SendPrintProcess(object printCommand)
        {
            try
            {
                //Declare and instantiate a new process component.
                Process cmdProcess = new Process();

                //Do not receive an event when the process exits.
                cmdProcess.EnableRaisingEvents = false;
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.FileName = batFile;
                cmdProcess.StartInfo.Arguments = printCommand.ToString();
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.Start();
                cmdProcess.WaitForExit();
                cmdProcess.Close();

                //Write Test.
                CreatePrintTemporaryFile("COMMAND" + Guid.NewGuid().ToString(), printCommand.ToString());
            }
            catch (Exception ex)
            {
                error += "Error with: [" + printCommand.ToString() + "] " + ex.Message + "\n";
                //TODO: Salver en los logs que no se pudo hallar el file
            }
        }

        internal static void PrintDocumentsInBatch(List<Document> documentList, Printer printer)
        {
            foreach (Document curDoc in documentList)
                ReportMngr.PrintDocument(curDoc, printer);
        }

        //Imprime el listado de seriales
        public static void PrintMovimientosBodega(DataTable Registros, String UA, String Ubicacion, String Producto, String Fecha, String Cantidad, String Estado)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            Int32 Control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtHeader.Columns.Add("Header");

            dtDetails.Columns.Add("Producto");
            dtDetails.Columns.Add("Fecha");
            dtDetails.Columns.Add("Estado");
            dtDetails.Columns.Add("Serial");
            dtDetails.Columns.Add("Mac");
            dtDetails.Columns.Add("Cantidad");
            dtDetails.Columns.Add("Ubicacion");
            dtDetails.Columns.Add("UA");

            dtHeader.Rows.Add(dtHeader.NewRow());

            //Creo el registro de los datos del header y los asigno
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los detalles
            foreach (DataRow Detalle in Registros.Rows)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[Control]["Producto"] = Producto;
                dtDetails.Rows[Control]["Fecha"] = Fecha;
                dtDetails.Rows[Control]["Estado"] = Estado;
                dtDetails.Rows[Control]["Serial"] = Detalle["Serial"].ToString();
                dtDetails.Rows[Control]["Mac"] = Detalle["Mac"].ToString();
                dtDetails.Rows[Control]["Cantidad"] = Cantidad;
                dtDetails.Rows[Control]["Ubicacion"] = Ubicacion;
                dtDetails.Rows[Control]["UA"] = UA;
                Control++;
            }

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "IQ - ReporteMovimientoBodegas.rdl");
            fv.Show();
        }

        public static void ImprimirEtiquetasDTV(String Modelo, String ReceiverID, String Serial)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            Int32 Control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtHeader.Columns.Add("Header");

            dtDetails.Columns.Add("Modelo");
            dtDetails.Columns.Add("ReceiverID");
            dtDetails.Columns.Add("Serial");

            //Creo el registro de los datos del header y los asigno
            dtHeader.Rows.Add(dtHeader.NewRow());
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los datos de los detalles
            dtDetails.Rows.Add(dtDetails.NewRow());
            dtDetails.Rows[Control]["Modelo"] = Modelo;
            dtDetails.Rows[Control]["ReceiverID"] = ReceiverID;
            dtDetails.Rows[Control]["Serial"] = Serial;

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient())
                    .GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "IQ - ImpresionEtiquetasDTV.rdl");
            fv.Show();
        }

        /* Hablador en el modulo de ALMACENAMIENTO*/
        public static void PrintMovimientosBodega(string USERNAME, DataTable SerialesImprimir, String unidad_almacenamiento, String codigoEmp, String destino, String repoEmpresa, String titulo, String estadoReparacion, String aux2)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            if (String.Compare(repoEmpresa, "CLARO") == 0)
            {
                //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
                dtDetails.Columns.Add("ESTADO");
                dtDetails.Columns.Add("CANTIDAD");
                dtDetails.Columns.Add("FECHAINGRESO");
                dtDetails.Columns.Add("MODELO");
                dtDetails.Columns.Add("NROPALLET");
                dtDetails.Columns.Add("TIEMPOBODEGA");
                dtDetails.Columns.Add("TIEMPOBODEGAH");
                dtDetails.Columns.Add("TRANSITO");
                dtDetails.Columns.Add("USERNAME");
                dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
                dtDetails.Columns.Add("SERIAL");
                dtDetails.Columns.Add("SAP");

                dtHeader.Columns.Add("Header");
                dtHeader.Rows.Add(dtHeader.NewRow());
                dtHeader.Rows[0]["Header"] = "";

                //Creo los registros de los detalles
                int cantidad_equipos = SerialesImprimir.Rows.Count;

                foreach (DataRow Detalle in SerialesImprimir.Rows)
                {
                    dtDetails.Rows.Add(dtDetails.NewRow());
                    dtDetails.Rows[control]["ESTADO"] = (aux2 == "") ? "PARA " + destino : destino;
                    dtDetails.Rows[control]["UNIDADALMACENAMIENTO"] = unidad_almacenamiento;
                    dtDetails.Rows[control]["CANTIDAD"] = cantidad_equipos;
                    dtDetails.Rows[control]["FECHAINGRESO"] = Detalle["Fecha_Ingreso"].ToString();
                    dtDetails.Rows[control]["NROPALLET"] = codigoEmp;
                    dtDetails.Rows[control]["TRANSITO"] = titulo;
                    dtDetails.Rows[control]["USERNAME"] = USERNAME.ToUpper() ;
                    dtDetails.Rows[control]["TIEMPOBODEGA"] = "";
                    dtDetails.Rows[control]["TIEMPOBODEGAH"] = "";

                    dtDetails.Rows[control]["SERIAL"] = Detalle["Serial"].ToString();
                    dtDetails.Rows[control]["SAP"] = Detalle["Codigo_SAP"].ToString();
                    dtDetails.Rows[control]["MODELO"] = Detalle["ProductoID"].ToString();
                    control++;
                }

                dsReporte.Tables.Add(dtHeader);
                dsReporte.Tables.Add(dtDetails);

                //Obtengo los datos de la impresora
                //Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

                //Muestro en pantalla el comprobante para luego imprimirlo
                ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHablador.rdl");
                fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
                fv.Show();
            }
            else
            {
                //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
                dtDetails.Columns.Add("ESTADO");
                dtDetails.Columns.Add("CANTIDAD");
                dtDetails.Columns.Add("FECHAINGRESO");
                dtDetails.Columns.Add("MODELO");
                dtDetails.Columns.Add("NROPALLET");
                dtDetails.Columns.Add("TIEMPOBODEGA");
                dtDetails.Columns.Add("TIEMPOBODEGAH");
                dtDetails.Columns.Add("TRANSITO");
                dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
                dtDetails.Columns.Add("SERIAL");
                dtDetails.Columns.Add("SAP");

                //dtHeader.Columns.Add("Header");
                //dtHeader.Rows.Add(dtHeader.NewRow());
                //dtHeader.Rows[0]["Header"] = "";
                //Creo los registros de los detalles
                foreach (DataRow Detalle in SerialesImprimir.Rows)
                {
                    dtDetails.Rows.Add(dtDetails.NewRow());
                    dtDetails.Rows[control]["SERIAL"] = Detalle["Serial"].ToString();

                    //Util.ShowMessage(Detalle["Serial"].ToString());

                    //idPallet,Posicion,modelo,receiver,serial
                    dtDetails.Rows[control]["UA"] = unidad_almacenamiento;
                    dtDetails.Rows[control]["CODIGO"] = codigoEmp;
                    dtDetails.Rows[control]["DESTINO"] = destino;
                    dtDetails.Rows[control]["RECEIVER"] = Detalle["Receiver"].ToString();
                    dtDetails.Rows[control]["PRODUCTO"] = Detalle["Modelo"].ToString();
                    dtDetails.Rows[control]["TITULO"] = titulo;
                    dtDetails.Rows[control]["AUX1"] = Detalle["Estado"].ToString();
                    dtDetails.Rows[control]["FECHA_INGRESO"] = Detalle["Fecha_Ingreso"].ToString();

                    //Util.ShowMessage("Prodcto id: " + Detalle["Modelo"].ToString() + " Receiver: " + Detalle["Receiver"].ToString());
                    control++;
                }

                //dsReporte.Tables.Add(dtHeader);
                dsReporte.Tables.Add(dtDetails);

                //Obtengo los datos de la impresora
                Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

                //Muestro en pantalla el comprobante para luego imprimirlo
                ViewDocument fv = new ViewDocument(dsReporte, "ReporteLeoDTV992.rdl");
                fv.Show();
            }
        }

        public static void PrintMovimientosBodegaHablador(DataTable SerialesImprimir, String unidad_almacenamiento, String codigoEmp, String destino, int nro_cajas)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable datos = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            datos.Columns.Add("UA");
            datos.Columns.Add("CODIGO");
            datos.Columns.Add("MAC");
            datos.Columns.Add("SERIAL");
            datos.Columns.Add("DESTINO");
            datos.Columns.Add("PRODUCTO");
            datos.Columns.Add("CODIGOSAP");
            datos.Columns.Add("TITULO");
            datos.Columns.Add("AUX1");
            datos.Columns.Add("NRO_CAJAS");

            dtHeader.Columns.Add("Header");
            dtHeader.Rows.Add(dtHeader.NewRow());
            dtHeader.Rows[0]["Header"] = "";
            //Creo los registros de los detalles
            foreach (DataRow Detalle in SerialesImprimir.Rows)
            {
                datos.Rows.Add(datos.NewRow());
                datos.Rows[control]["SERIAL"] = "";

                datos.Rows[control]["UA"] = unidad_almacenamiento;
                datos.Rows[control]["CODIGO"] = codigoEmp;
                datos.Rows[control]["DESTINO"] = destino;
                datos.Rows[control]["MAC"] = "";
                datos.Rows[control]["PRODUCTO"] = "";
                datos.Rows[control]["CODIGOSAP"] = "";
                datos.Rows[control]["TITULO"] = "";
                datos.Rows[control]["AUX1"] = "";
                datos.Rows[control]["NRO_CAJAS"] = nro_cajas;
                //Util.ShowMessage("Prodcto id: " + Detalle["ProductoID"].ToString() + " sap: " + Detalle["Codigo_SAP"].ToString());
                control++;
            }

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(datos);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "ReporteLeoHablador.rdl");
            fv.Show();
        }

        public static void PrintMovimientosBodegaHabladorDirectTV(DataTable SerialesImprimir, String unidad_almacenamiento, String codigoEmp, String destino, int nro_cajas)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable datos = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            datos.Columns.Add("UA");
            datos.Columns.Add("CODIGO");
            datos.Columns.Add("MAC");
            datos.Columns.Add("SERIAL");
            datos.Columns.Add("DESTINO");
            datos.Columns.Add("PRODUCTO");
            datos.Columns.Add("CODIGOSAP");
            datos.Columns.Add("TITULO");
            datos.Columns.Add("AUX1");
            datos.Columns.Add("NRO_CAJAS");

            dtHeader.Columns.Add("Header");
            dtHeader.Rows.Add(dtHeader.NewRow());
            dtHeader.Rows[0]["Header"] = "";
            //Creo los registros de los detalles
            //foreach (DataRow Detalle in SerialesImprimir.Rows)
            //{
            datos.Rows.Add(datos.NewRow());
            datos.Rows[0]["SERIAL"] = "";

            datos.Rows[0]["UA"] = unidad_almacenamiento;
            datos.Rows[0]["CODIGO"] = codigoEmp;
            datos.Rows[0]["DESTINO"] = destino;
            datos.Rows[0]["MAC"] = "";
            datos.Rows[0]["PRODUCTO"] = "";
            datos.Rows[0]["CODIGOSAP"] = "";
            datos.Rows[0]["TITULO"] = "";
            datos.Rows[0]["AUX1"] = "";
            datos.Rows[control]["NRO_CAJAS"] = nro_cajas;
            //Util.ShowMessage("Prodcto id: " + Detalle["ProductoID"].ToString() + " sap: " + Detalle["Codigo_SAP"].ToString());
            control++;
            //}

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(datos);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "ReporteLeoHabladorDTV.rdl");
            fv.Show();
        }

        //Imprime hablador para pallet
        public static void PrintMovimientosMercancia(string UserName, String transito, String ubicacion, String cantidad, String fechaIngreso, String idpallet, String dias, String tiempo, System.Windows.Controls.ListView SerialesImprimir)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            Int32 Control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtHeader.Columns.Add("Header");

            dtDetails.Columns.Add("ESTADO");
            dtDetails.Columns.Add("CANTIDAD");
            dtDetails.Columns.Add("FECHAINGRESO");
            dtDetails.Columns.Add("MODELO");
            dtDetails.Columns.Add("NROPALLET");
            dtDetails.Columns.Add("TIEMPOBODEGA");
            dtDetails.Columns.Add("TIEMPOBODEGAH");
            dtDetails.Columns.Add("TRANSITO");
            dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
            dtDetails.Columns.Add("SERIAL");
            dtDetails.Columns.Add("SAP");
            dtDetails.Columns.Add("USERNAME");

            dtHeader.Rows.Add(dtHeader.NewRow());

            //Creo el registro de los datos del header y los asigno
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los detalles Posicion
            foreach (DataRowView Detalle in SerialesImprimir.Items)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[Control]["ESTADO"] = ubicacion;
                dtDetails.Rows[Control]["UNIDADALMACENAMIENTO"] = "PALLET";
                dtDetails.Rows[Control]["CANTIDAD"] = cantidad;
                dtDetails.Rows[Control]["FECHAINGRESO"] = fechaIngreso;
                dtDetails.Rows[Control]["NROPALLET"] = idpallet;
                dtDetails.Rows[Control]["TIEMPOBODEGA"] = dias;
                dtDetails.Rows[Control]["TIEMPOBODEGAH"] = tiempo;
                dtDetails.Rows[Control]["TRANSITO"] = transito;
                dtDetails.Rows[Control]["USERNAME"] = UserName.ToUpper();

                dtDetails.Rows[Control]["SERIAL"] = Detalle["PSerial"].ToString();
                dtDetails.Rows[Control]["SAP"] = Detalle["SAP"].ToString();
                dtDetails.Rows[Control]["MODELO"] = Detalle["Modelo"].ToString();

                Console.WriteLine(Detalle["PSerial"].ToString() + " " + Detalle["SAP"].ToString() + " " + Detalle["Modelo"].ToString());
                Control++;
            }
            
            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            //ViewDocument fv = new ViewDocument(dsReporte, "MovMercancia.rdl");
            ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHablador.rdl");
            fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
            fv.Show();
        }

        //Imprime hablador para pallet
        //public static void PrintMovimientosMercancia(System.Windows.Controls.ListView SerialesImprimir, string p1, string p2, string p3, string p4, string p5, string p6)
        //{
        //    //Variables Auxiliares
        //    DataTable dtHeader = new DataTable("Header");
        //    DataTable dtDetails = new DataTable("DataSet1");
        //    DataSet dsReporte = new DataSet();
        //    String Printer;
        //    Int32 Control = 0;

        //    //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
        //    dtHeader.Columns.Add("Header");

        //    dtDetails.Columns.Add("ESTADO");
        //    dtDetails.Columns.Add("CANTIDAD");
        //    dtDetails.Columns.Add("FECHAINGRESO");
        //    dtDetails.Columns.Add("MODELO");
        //    dtDetails.Columns.Add("NROPALLET");
        //    dtDetails.Columns.Add("TIEMPOBODEGA");
        //    dtDetails.Columns.Add("TIEMPOBODEGAH");
        //    dtDetails.Columns.Add("TRANSITO");

        //    dtHeader.Rows.Add(dtHeader.NewRow());

        //    //Creo el registro de los datos del header y los asigno
        //    dtHeader.Rows[0]["Header"] = "";

        //    //Creo los registros de los detalles
        //    foreach (DataRowView Detalle in SerialesImprimir.SelectedItems)
        //    {
        //        dtDetails.Rows.Add(dtDetails.NewRow());
        //        dtDetails.Rows[Control]["ESTADO"] = Detalle["Ubicacion"].ToString();
        //        dtDetails.Rows[Control]["CANTIDAD"] = Detalle["Cantidad"].ToString();
        //        dtDetails.Rows[Control]["FECHAINGRESO"] = Detalle["FechaIngreso"].ToString();
        //        dtDetails.Rows[Control]["MODELO"] = Detalle["Modelo"].ToString();
        //        dtDetails.Rows[Control]["NROPALLET"] = Detalle["IdPallet"].ToString();
        //        dtDetails.Rows[Control]["TIEMPOBODEGA"] = Detalle["NumeroDias"].ToString();
        //        dtDetails.Rows[Control]["TIEMPOBODEGAH"] = Detalle["Horas"].ToString();
        //        dtDetails.Rows[Control]["TRANSITO"] = "REPARACION - DIAGNOSTICO";
        //        Control++;
        //    }

        //    dsReporte.Tables.Add(dtHeader);
        //    dsReporte.Tables.Add(dtDetails);

        //    //Obtengo los datos de la impresora
        //    Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

        //    //Muestro en pantalla el comprobante para luego imprimirlo
        //    ViewDocument fv = new ViewDocument(dsReporte, "MovMercancia.rdl");
        //    fv.Show();
        //}

        #region Impresión de etiquetas - Modulo Etiquetado

        public static void EtiquetadoEquipo(String NumSerial, String Mac, String modeloArchivo, String nombreImpresora)
        {
            Console.WriteLine("------- " + carpetaServidor + modeloArchivo + ".prn");
            String zplForLabel = LeerArchivo_Impresion(carpetaServidor + modeloArchivo + ".prn");

            StringBuilder zpl = new StringBuilder();
            zpl.Append(zplForLabel);
            zpl = zpl.Replace("sr1", Mac);
            zpl = zpl.Replace("mac", NumSerial);

            Console.WriteLine(zpl.ToString());

            // Send a printer-specific to the printer.
            RawPrinterHelper.SendStringToPrinter(nombreImpresora, zpl.ToString());
        }

        private static string LeerArchivo_Impresion(String modeloArchivo)
        {
            //\\\\192.168.2.90\\compartida\\COSHIPHD.prn
            StreamReader objReader = new StreamReader(modeloArchivo);

            string sLine = "";
            ArrayList arrText = new ArrayList();

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    arrText.Add(sLine);
            }

            objReader.Close();

            String concat = "";
            foreach (string sOutput in arrText)
            {
                concat = concat + "\n" + sOutput;
            }
            return concat;
        }

        public static void EtiquetadoEquipoIndividual(String serial, String mac, String Ruta, String ipServidor, List<String> codAuxiliares, Int32 CantImpresiones, String nombreImpresora)
        {
            try
            {
                String directorio = "\\\\" + ipServidor + "\\" + Ruta;
                String zplForLabel = LeerArchivo_Impresion(directorio);

                StringBuilder zpl = new StringBuilder();
                zpl.Append(zplForLabel);
                zpl = zpl.Replace("sr1", serial);
                zpl = zpl.Replace("mac", mac);

                int numAux = codAuxiliares.Count; //Número de codigos adicionales

                if (numAux > 0)
                {
                    for (int a = 1; a <= codAuxiliares.Count; a++)
                    {
                        zpl = zpl.Replace("cod" + a, codAuxiliares[a - 1]);
                    }
                }

                for (int b = 0; b < CantImpresiones; b++)
                {
                    // Send a printer-specific to the printer.
                    RawPrinterHelper.SendStringToPrinter(nombreImpresora, zpl.ToString());
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                Util.ShowError("Se presento un error en la impresión de la etiqueta " + ex.Message);
            }
            //Console.WriteLine(zpl.ToString());
            //throw new NotImplementedException();
        }

        public static void PrintMovimientosEmpaque(string USERNAME, DataTable seriales, String transito, String estado, String NroCajas, String NroSeriales, String fechaIngreso, String pallet, String modelo)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtDetails.Columns.Add("ESTADO");
            dtDetails.Columns.Add("CANTIDAD");
            dtDetails.Columns.Add("CANTIDADCAJAS");
            dtDetails.Columns.Add("FECHAINGRESO");
            dtDetails.Columns.Add("MODELO");
            dtDetails.Columns.Add("USERNAME");
            dtDetails.Columns.Add("NROPALLET");
            dtDetails.Columns.Add("TIEMPOBODEGA");
            dtDetails.Columns.Add("TIEMPOBODEGAH");
            dtDetails.Columns.Add("TRANSITO");
            dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
            dtDetails.Columns.Add("SERIAL");
            dtDetails.Columns.Add("SAP");

            dtHeader.Columns.Add("Header");
            dtHeader.Rows.Add(dtHeader.NewRow());
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los detalles Posicion
            foreach (DataRow Detalle in seriales.Rows)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[control]["ESTADO"] = estado;
                dtDetails.Rows[control]["UNIDADALMACENAMIENTO"] = "Pallet";
                dtDetails.Rows[control]["CANTIDAD"] = NroSeriales;
                dtDetails.Rows[control]["CANTIDADCAJAS"] = NroCajas;
                dtDetails.Rows[control]["USERNAME"] = USERNAME.ToUpper();
                dtDetails.Rows[control]["FECHAINGRESO"] = fechaIngreso;
                dtDetails.Rows[control]["NROPALLET"] = pallet;
                dtDetails.Rows[control]["TRANSITO"] = transito;

                dtDetails.Rows[control]["SERIAL"] = Detalle["serial"].ToString();
                dtDetails.Rows[control]["MODELO"] = Detalle["ProductoID"].ToString();
                dtDetails.Rows[control]["SAP"] = Detalle["CODIGO_SAP"].ToString();
                control++;
            }

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHabladorEmpaque.rdl");
            fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
            fv.Show();
        }

        #endregion

        public static void EtiquetaEmpaque(String archivoEtiq, DataTable lista_equipos, string aux_idPallet, string nroCaja, string familia, string modelo, string Codsap, string Nameprinter)
        {
            try
            {
                String zplForLabel = LeerArchivo_Impresion(carpetaServidor + archivoEtiq);
                StringBuilder zpl = new StringBuilder();
                zpl.Append(zplForLabel);
                DateTime now = DateTime.Now;

                int cont = 0;

                foreach (DataRow Detalle in lista_equipos.Rows)
                {
                    string serial = Detalle["Serial"].ToString();
                    string mac = Detalle["Mac"].ToString();

                    zpl = zpl.Replace("sr" + cont, Detalle["Serial"].ToString());
                    zpl = zpl.Replace("mac" + cont, Detalle["Mac"].ToString());
                    cont++;
                }

                zpl = zpl.Replace("fam1", familia); //Familia
                zpl = zpl.Replace("est1", aux_idPallet); //Codigio de Pallet/estiba
                zpl = zpl.Replace("caja1", nroCaja); //Nro de caja
                zpl = zpl.Replace("mod1", modelo);  //Modelo
                zpl = zpl.Replace("cods1", Codsap); //Codigo SAP
                zpl = zpl.Replace("date1", now.ToString("dd/MM/yyyy")); //Familia

                RawPrinterHelper.SendStringToPrinter(Nameprinter, zpl.ToString());
            }
            catch (Exception ex)
            {
                Util.ShowError("Se presento un error en la impresión de la etiqueta " + ex.Message);
            }
        }

        public static void PrintMovimientosBodegaDIRECTV(DataTable SerialesImprimir, String unidad_almacenamiento, String codigoEmp, String destino, String repoEmpresa, String titulo, String estadoReparacion, String aux2)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            if (String.Compare(repoEmpresa, "DIRECTV") == 0)
            {
                //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
                dtDetails.Columns.Add("ESTADO");
                dtDetails.Columns.Add("CANTIDAD");
                dtDetails.Columns.Add("FECHAINGRESO");
                dtDetails.Columns.Add("MODELO");
                dtDetails.Columns.Add("NROPALLET");
                dtDetails.Columns.Add("TIEMPOBODEGA");
                dtDetails.Columns.Add("TIEMPOBODEGAH");
                dtDetails.Columns.Add("TRANSITO");
                dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
                dtDetails.Columns.Add("SERIAL");
                dtDetails.Columns.Add("SAP");

                dtHeader.Columns.Add("Header");
                dtHeader.Rows.Add(dtHeader.NewRow());
                dtHeader.Rows[0]["Header"] = "";

                //Creo los registros de los detalles
                int cantidad_equipos = SerialesImprimir.Rows.Count;

                foreach (DataRow Detalle in SerialesImprimir.Rows)
                {
                    dtDetails.Rows.Add(dtDetails.NewRow());
                    dtDetails.Rows[control]["ESTADO"] = (aux2 == "") ? "PARA " + destino : destino;
                    dtDetails.Rows[control]["UNIDADALMACENAMIENTO"] = unidad_almacenamiento;
                    dtDetails.Rows[control]["CANTIDAD"] = cantidad_equipos;
                    dtDetails.Rows[control]["FECHAINGRESO"] = Detalle["Fecha_Ingreso"].ToString();
                    dtDetails.Rows[control]["NROPALLET"] = codigoEmp;
                    dtDetails.Rows[control]["TRANSITO"] = titulo;
                    dtDetails.Rows[control]["TIEMPOBODEGA"] = "";
                    dtDetails.Rows[control]["TIEMPOBODEGAH"] = "";

                    dtDetails.Rows[control]["SERIAL"] = Detalle["Serial"].ToString();
                    dtDetails.Rows[control]["SAP"] = "";
                    dtDetails.Rows[control]["MODELO"] = Detalle["MODELO"].ToString();
                    control++;
                }

                dsReporte.Tables.Add(dtHeader);
                dsReporte.Tables.Add(dtDetails);

                //Obtengo los datos de la impresora
                //Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

                //Muestro en pantalla el comprobante para luego imprimirlo
                ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHabladorDIRECTV.rdl");
                fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
                fv.Show();
            }
            else
            {
                //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
                dtDetails.Columns.Add("ESTADO");
                dtDetails.Columns.Add("CANTIDAD");
                dtDetails.Columns.Add("FECHA_INGRESO");
                dtDetails.Columns.Add("MODELO");
                dtDetails.Columns.Add("NROPALLET");
                dtDetails.Columns.Add("TITULO");
                dtDetails.Columns.Add("AUX1");
                dtDetails.Columns.Add("TRANSITO");
                dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
                dtDetails.Columns.Add("SERIAL");
                dtDetails.Columns.Add("RECEIVER");

                //dtHeader.Columns.Add("Header");
                //dtHeader.Rows.Add(dtHeader.NewRow());
                //dtHeader.Rows[0]["Header"] = "";
                //Creo los registros de los detalles
                foreach (DataRow Detalle in SerialesImprimir.Rows)
                {
                    dtDetails.Rows.Add(dtDetails.NewRow());
                    dtDetails.Rows[control]["SERIAL"] = Detalle["Serial"].ToString();

                    //Util.ShowMessage(Detalle["Serial"].ToString());

                    //idPallet,Posicion,modelo,receiver,serial
                    dtDetails.Rows[control]["UNIDADALMACENAMIENTO"] = unidad_almacenamiento;
                    dtDetails.Rows[control]["NROPALLET"] = codigoEmp;
                    dtDetails.Rows[control]["TRANSITO"] = destino;
                    dtDetails.Rows[control]["RECEIVER"] = Detalle["Receiver"].ToString();
                    dtDetails.Rows[control]["MODELO"] = Detalle["Modelo"].ToString();
                    dtDetails.Rows[control]["TITULO"] = titulo;
                    dtDetails.Rows[control]["AUX1"] = Detalle["Estado"].ToString();
                    dtDetails.Rows[control]["FECHA_INGRESO"] = Detalle["Fecha_Ingreso"].ToString();

                    //Util.ShowMessage("Prodcto id: " + Detalle["Modelo"].ToString() + " Receiver: " + Detalle["Receiver"].ToString());
                    control++;
                }

                //dsReporte.Tables.Add(dtHeader);
                dsReporte.Tables.Add(dtDetails);

                //Obtengo los datos de la impresora
                Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

                //Muestro en pantalla el comprobante para luego imprimirlo
                ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHabladorDIRECTV.rdl");
                fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
                fv.Show();
            }
        }

        public static void PrintMovimientosEmpaqueDIRECTV(DataTable seriales, String transito, String estado, String NroCajas, String NroSeriales, String fechaIngreso, String pallet, String modelo)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            int control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtDetails.Columns.Add("ESTADO");
            dtDetails.Columns.Add("CANTIDAD");
            dtDetails.Columns.Add("CANTIDADCAJAS");
            dtDetails.Columns.Add("FECHAINGRESO");
            dtDetails.Columns.Add("MODELO");
            dtDetails.Columns.Add("NROPALLET");
            dtDetails.Columns.Add("TIEMPOBODEGA");
            dtDetails.Columns.Add("TIEMPOBODEGAH");
            dtDetails.Columns.Add("TRANSITO");
            dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
            dtDetails.Columns.Add("SERIAL");
            dtDetails.Columns.Add("SAP");

            dtHeader.Columns.Add("Header");
            dtHeader.Rows.Add(dtHeader.NewRow());
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los detalles Posicion
            foreach (DataRow Detalle in seriales.Rows)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[control]["ESTADO"] = estado;
                dtDetails.Rows[control]["UNIDADALMACENAMIENTO"] = "Pallet";
                dtDetails.Rows[control]["CANTIDAD"] = NroSeriales;
                dtDetails.Rows[control]["CANTIDADCAJAS"] = NroCajas;
                dtDetails.Rows[control]["FECHAINGRESO"] = fechaIngreso;
                dtDetails.Rows[control]["NROPALLET"] = pallet;
                dtDetails.Rows[control]["TRANSITO"] = transito;

                dtDetails.Rows[control]["SERIAL"] = Detalle["serial"].ToString();
                dtDetails.Rows[control]["MODELO"] = Detalle["MODELO"].ToString();
                dtDetails.Rows[control]["SAP"] = Detalle["SMART_CARD_ENTRADA"].ToString();
                control++;
            }

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHabladorEmpaqueDIRECTV.rdl");
            fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
            fv.Show();
        }

        public static void PrintMovimientosMercanciaDIRECTV(String transito, String ubicacion, String cantidad, String fechaIngreso, String idpallet, String dias, String tiempo, System.Windows.Controls.ListView SerialesImprimir)
        {
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            Int32 Control = 0;

            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte
            dtHeader.Columns.Add("Header");

            dtDetails.Columns.Add("ESTADO");
            dtDetails.Columns.Add("CANTIDAD");
            dtDetails.Columns.Add("FECHAINGRESO");
            dtDetails.Columns.Add("MODELO");
            dtDetails.Columns.Add("NROPALLET");
            dtDetails.Columns.Add("TIEMPOBODEGA");
            dtDetails.Columns.Add("TIEMPOBODEGAH");
            dtDetails.Columns.Add("TRANSITO");
            dtDetails.Columns.Add("UNIDADALMACENAMIENTO");
            dtDetails.Columns.Add("SERIAL");
            dtDetails.Columns.Add("SAP");

            dtHeader.Rows.Add(dtHeader.NewRow());

            //Creo el registro de los datos del header y los asigno
            dtHeader.Rows[0]["Header"] = "";

            //Creo los registros de los detalles Posicion
            foreach (DataRowView Detalle in SerialesImprimir.Items)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[Control]["ESTADO"] = ubicacion;
                dtDetails.Rows[Control]["UNIDADALMACENAMIENTO"] = "PALLET";
                dtDetails.Rows[Control]["CANTIDAD"] = cantidad;
                dtDetails.Rows[Control]["FECHAINGRESO"] = fechaIngreso;
                dtDetails.Rows[Control]["NROPALLET"] = idpallet;
                dtDetails.Rows[Control]["TIEMPOBODEGA"] = dias;
                dtDetails.Rows[Control]["TIEMPOBODEGAH"] = tiempo;
                dtDetails.Rows[Control]["TRANSITO"] = transito;

                dtDetails.Rows[Control]["SERIAL"] = Detalle["PSerial"].ToString();
                dtDetails.Rows[Control]["SAP"] = "";
                dtDetails.Rows[Control]["MODELO"] = Detalle["Modelo"].ToString();

                //Console.WriteLine(Detalle["PSerial"].ToString() + " " + Detalle["Receiver"].ToString() + " " + Detalle["Modelo"].ToString());
                Control++;
            }

            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient()).GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;

            //Muestro en pantalla el comprobante para luego imprimirlo
            //ViewDocument fv = new ViewDocument(dsReporte, "MovMercancia.rdl");
            ViewDocument fv = new ViewDocument(dsReporte, "MovMercanciaHabladorDIRECTV.rdl");
            fv.Title = "Sistema Integrado de Logística y Producción LOGPRO";
            fv.Show();
        }

    }
}
