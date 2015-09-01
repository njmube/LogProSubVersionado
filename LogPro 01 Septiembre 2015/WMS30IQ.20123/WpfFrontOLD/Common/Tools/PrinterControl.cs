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

namespace WpfFront.Common.Tools
{
    public class PrinterControl
    {
        public static void Imprimir_certificados(DataTable ListRecord, string DocumentoConsultado)
        {
            
            //Variables Auxiliares
            DataTable dtHeader = new DataTable("Header");
            DataTable dtDetails = new DataTable("DataSet1");
            DataSet dsReporte = new DataSet();
            String Printer;
            Int32 Control = 0;            
            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte Header
            dtHeader.Columns.Add("FECHA");
            dtHeader.Columns.Add("USUARIO");
            dtHeader.Columns.Add("DOCUMENTO");
            //Lleno Los Datos del Header
            dtHeader.Rows.Add(dtHeader.NewRow());            
            dtHeader.Rows[0]["FECHA"] = DateTime.Today.ToString("yyyy-MM-dd");
            dtHeader.Rows[0]["USUARIO"] = App.curUser.UserName.ToString();
            dtHeader.Rows[0]["DOCUMENTO"] = DocumentoConsultado;
            //Creo los nombres de los datos a mostrar, deben ser iguales a los utilizados en el reporte Details
            dtDetails.Columns.Add("MODELO");
            dtDetails.Columns.Add("SERIAL");
            dtDetails.Columns.Add("RECEIVERID");

            
            foreach (DataRow Rows in ListRecord.Rows)
            {
                dtDetails.Rows.Add(dtDetails.NewRow());
                dtDetails.Rows[Control]["MODELO"] = Rows[2].ToString();
                dtDetails.Rows[Control]["SERIAL"] = Rows[4].ToString();
                dtDetails.Rows[Control]["RECEIVERID"] = Rows[5].ToString();
                Control++;                
            }
            dsReporte.Tables.Add(dtHeader);
            dsReporte.Tables.Add(dtDetails);

            //Obtengo los datos de la impresora
            Printer = (new WMSServiceClient())
                    .GetConfigOption(new ConfigOption { Code = "PRINTREPORTING" }).First().DefValue;
            //Muestro en pantalla el comprobante para luego imprimirlo

            ViewDocument fv = new ViewDocument(dsReporte, "IQCERTIFICADOREPORT.rdl");
            fv.Show();
        }
    }    
}
