using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;

namespace WpfFront.Common.Tools
{
    class Etiquetas
    {
        public string Tipo { get; set; }
        public string Parametros { get; set; }
        public StringBuilder Codigo_PL { get; set; }

        public void Imprimir(string producto, string Serial, string Mac, string Btn)
        {
            //trae el diseno segun lo que tenga la varible Tipo
            string Diseño = Diseño_Etiqueta(producto, Serial, Mac).ToString();
            //envia a imprimir
            Establecer_Impresora(Diseño, producto, Btn);

        }

        private StringBuilder Diseño_Etiqueta(string Producto, string Serial,string Mac)
        {
            #region Diseno PL
            StringBuilder sb = new StringBuilder();

            char[] delimiterChars = { ',' };
            string[] words = Parametros.Split(delimiterChars);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Contains("@Serial"))
                {
                    Codigo_PL.Replace(words[i], Serial);
                }
                else if (words[i].Contains("@Mac"))
                {
                    Codigo_PL.Replace(words[i], Mac);
                }
                else if (words[i].Contains("@producto"))
                {
                    Codigo_PL.Replace(words[i], Producto);
                }
                else if (words[i].Contains("@Date"))
                {
                    Codigo_PL.Replace(words[i], DateTime.Today.ToShortDateString());
                }
            }
            return sb;

            #region  Auxiliar
            //if (Tipo.Equals("DIRECTV(4,6*3,2)")) //OK
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW831");
            //    sb.AppendLine("^LL0400");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^FT190,47^A0N,37,14^FH\\^FD" + producto + "^FS");
            //    sb.AppendLine("^FT646,40^A0N,26,26^FH\\^FD" + DateTime.Today.ToShortDateString() + "^FS");
            //    sb.AppendLine("^BY3,3,71^FT55,170^BCN,,Y,N");
            //    sb.AppendLine("^FD>:" + Serial + ">69^FS");
            //    sb.AppendLine("^BY4,3,48^FT55,266^BCN,,Y,N");
            //    sb.AppendLine("^FD>:" + Mac + "^FS");
            //    sb.AppendLine("^FT792,0^A0R,33,98^FH\\^FDPOSPAGO^FS");
            //    sb.AppendLine("^FO601,351^GB119,31,31^FS");
            //    sb.AppendLine("^FT601,375^A0N,25,24^FR^FH\\^FDVERIFICADO^FS");
            //    sb.AppendLine("^FT6,48^A0N,43,45^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT73,367^A0N,16,50^FH\\^FDIQ  ELECTRONICS^FS");
            //    sb.AppendLine("^FT73,387^A0N,16,50^FH\\^FD      COLOMBIA^FS");
            //    sb.AppendLine("^LRY^FO3,343^GB783,0,49^FS^LRN");
            //    sb.AppendLine("^LRY^FO4,4^GB775,0,56^FS^LRN");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");


            //}
            //if (Tipo.Equals("DIRECTV(3,7*3,4)"))//OK
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW296");
            //    sb.AppendLine("^LL0280");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^BY1,3,57^FT2,107^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Mac + "^FS");
            //    sb.AppendLine("^FT2,36^A0N,28,19^FH\\^FDMODELO :" + producto + "^FS");
            //    sb.AppendLine("^FT2,139^A0N,25,16^FH\\^FDRECEIVER ID:" + Mac + "^FS");
            //    sb.AppendLine("^BY1,3,61^FT2,218^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Serial + "^FS");
            //    sb.AppendLine("^FT2,249^A0N,28,19^FH\\^FDSERIE :" + Serial + "^FS");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");

            //}
            //if (Tipo.Equals("DIRECTV(6*3,5)")) //ok
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("~DG000.GRF,01024,016,");
            //    sb.AppendLine(",:::::::N03FIF8,M01FJF8,M03FJF80H07FFE0,M07FJF8001FIFC,M0LF800FKF,L01FKF801FKFC0,L01FKF807FKFE0,L01FKF80FMF0,L01FKF01FHFH0IFC,L01FF0J03FF8001FFE,L01FF0J07FE0I03FF,L01FE0J0HF81FF81FF,L01FE0I01FF07FFE0FF80,L01FE0I01FE1FIF87FC0,L01FE0I03FC3FIFC3FC0,L01FE0I03F87FIFE,L01FE0I07F8FKF,L01FF0I07F0FFC3FF80,L01FE0I07F1FF00FFC0,L01FE0I0FE1FC003FC0,L01FE0I0FE3FC001FC0,L01FE0I0FC3F8,L01FFAAFHFC3F0,L01FLFC7F0,::::L01FLFC3F0,L01FF0I0FE3F8,L01FE0I0FE3FC0H0FC0,L01FE0I0FE1FC003FC0,L01FE0I07F1FF007FC0,L01FF0I07F0FFC1FF80,L01FE0I07F8FKF80,L01FE0I03FC7FJF,L01FE0I03FC3FIFE1FC0,L01FF0I01FE0FIF83FC0,L01FF0J0HF83FFE0FF80,L01FF0J0HFC0FF81FF80,L01FF0J07FE0I03FF,L01FF0J03FFC0H0HFE,L01FF0J01FHF80FHFC,L01FF0K0NF8,L01FF0K03FKFE0,L01FF0K01FKFC0,L01FF0L0LF80,L01FF0L01FIFC,M0HFN03FFE0,,::::::^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW480");
            //    sb.AppendLine("^LL0280");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^BY1,3,45^FT7,52^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Mac + "^FS");
            //    sb.AppendLine("^BY1,3,45^FT6,136^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Serial + "^FS");
            //    sb.AppendLine("^FT211,248^A0N,20,43^FH\\^FD" + producto + "^FS");
            //    sb.AppendLine("^FT96,256^XG000.GRF,1,1^FS");
            //    sb.AppendLine("^FT211,217^A0N,19,28^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT6,168^A0N,28,28^FH\\^FDSERIE:" + Serial + "^FS");
            //    sb.AppendLine("^FT7,82^A0N,28,28^FH\\^FDRECEIVER ID:" + Mac + "^FS");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");
            //    sb.AppendLine("^XA^ID000.GRF^FS^XZ");


            //}
            //if (Tipo.Equals("DIRECTV(4,5*2,1)"))//OK
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW360");
            //    sb.AppendLine("^LL0168");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^FT9,39^A0N,28,16^FH\\^FDMODEL:" + producto + "^FS");
            //    sb.AppendLine("^BY1,3,20^FT9,125^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Serial + "^FS");
            //    sb.AppendLine("^BY1,3,20^FT9,71^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Mac + "^FS");
            //    sb.AppendLine("^FO293,36^GB18,0,5^FS");
            //    sb.AppendLine("^FT9,148^A0N,18,28^FH\\^FDSERIAL:" + Serial + "^FS");
            //    sb.AppendLine("^FO293,17^GB20,0,5^FS");
            //    sb.AppendLine("^FT9,95^A0N,18,28^FH\\^FDRECEIVER ID:" + Mac + "^FS");
            //    sb.AppendLine("^FO307,17^GB0,24,5^FS");
            //    sb.AppendLine("^FO288,16^GB0,26,5^FS");
            //    sb.AppendLine("^FO283,48^GB34,0,5^FS");
            //    sb.AppendLine("^FO278,6^GB0,47,5^FS");
            //    sb.AppendLine("^FO283,7^GB38,0,4^FS");
            //    sb.AppendLine("^FO317,6^GB0,47,6^FS");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");


            //}
            //if (Tipo.Equals("DIRECTV(4,5*2,5)"))//OK
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW344");
            //    sb.AppendLine("^LL0256");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^BY1,3,49^FT4,141^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Serial + "^FS");
            //    sb.AppendLine("^BY1,3,49^FT4,57^BCN,,N,N");
            //    sb.AppendLine("^FD>:" + Mac + "^FS");
            //    sb.AppendLine("^FT9,230^A0N,28,14^FH\\^FDMODELO:" + producto + "^FS");
            //    sb.AppendLine("^FT4,161^A0N,16,28^FH\\^FDSERIAL:" + Serial + "^FS");
            //    sb.AppendLine("^FT4,77^A0N,16,28^FH\\^FDRECEIVER ID:" + Mac + "^FS");
            //    sb.AppendLine("^FT229,230^A0N,28,12^FH\\^FDMADE IN MEXICO^FS");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");

            //}
            //if (Tipo.Equals("DIRECTV(8,5*4,3)"))
            //{
            //    sb = new StringBuilder();
            //    sb.AppendLine("CT~~CD,~CC^~CT~");
            //    sb.AppendLine("^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ");
            //    sb.AppendLine("^XA");
            //    sb.AppendLine("^MMT");
            //    sb.AppendLine("^PW679");
            //    sb.AppendLine("^LL0344");
            //    sb.AppendLine("^LS0");
            //    sb.AppendLine("^FT310,249^A0N,11,28^FH\\^FDMODELO: " + producto + "^FS");
            //    sb.AppendLine("^FT5,249^A0N,11,28^FH\\^FDMODELO: " + producto + "^FS");
            //    sb.AppendLine("^FT310,78^A0N,11,28^FH\\^FDMODELO: " + producto + "^FS");
            //    sb.AppendLine("^FT5,78^A0N,11,28^FH\\^FDMODELO: " + producto + "^FS");
            //    sb.AppendLine("^FO301,1^GB0,342,1^FS");
            //    sb.AppendLine("^FO1,164^GB606,0,1^FS");
            //    sb.AppendLine("^FT310,201^A0N,22,33^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT310,227^A0N,12,19^FH\\^FDReceptor Decodificadorintegrado ^FS");
            //    sb.AppendLine("^FT5,201^A0N,22,33^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT310,272^A0N,12,19^FH\\^FD12v DC = = = = 18W, 1.5A MAX^FS");
            //    sb.AppendLine("^FT310,295^A0N,12,16^FH\\^FDUSB: 5V DC = = = = = 500mA max^FS");
            //    sb.AppendLine("^FT310,320^A0N,14,14^FH\\^FDHecho en Tailandia / Made in Thailand^FS");
            //    sb.AppendLine("^FT5,227^A0N,12,19^FH\\^FDReceptor Decodificadorintegrado ^FS");
            //    sb.AppendLine("^FT310,30^A0N,22,33^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT5,272^A0N,12,19^FH\\^FD12v DC = = = = 18W, 1.5A MAX^FS");
            //    sb.AppendLine("^FT5,295^A0N,12,16^FH\\^FDUSB: 5V DC = = = = = 500mA max^FS");
            //    sb.AppendLine("^FT5,320^A0N,14,14^FH\\^FDHecho en Tailandia / Made in Thailand^FS");
            //    sb.AppendLine("^FT310,56^A0N,12,19^FH\\^FDReceptor Decodificadorintegrado ^FS");
            //    sb.AppendLine("^FT5,30^A0N,22,33^FH\\^FDDIRECTV^FS");
            //    sb.AppendLine("^FT310,101^A0N,12,19^FH\\^FD12v DC = = = = 18W, 1.5A MAX^FS");
            //    sb.AppendLine("^FT310,125^A0N,12,16^FH\\^FDUSB: 5V DC = = = = = 500mA max^FS");
            //    sb.AppendLine("^FT310,150^A0N,14,14^FH\\^FDHecho en Tailandia / Made in Thailand^FS");
            //    sb.AppendLine("^FT5,56^A0N,12,19^FH\\^FDReceptor Decodificadorintegrado ^FS");
            //    sb.AppendLine("^FT5,101^A0N,12,19^FH\\^FD12v DC = = = = 18W, 1.5A MAX^FS");
            //    sb.AppendLine("^FT5,125^A0N,12,16^FH\\^FDUSB: 5V DC = = = = = 500mA max^FS");
            //    sb.AppendLine("^FT5,150^A0N,14,14^FH\\^FDHecho en Tailandia / Made in Thailand^FS");
            //    sb.AppendLine("^PQ1,0,1,Y^XZ");
            //}
            #endregion

            #endregion
        }

        public static string GetImpresoraDefecto()
        {
            //selecciona la impresora por defecto
            for (int i = 0; i <= PrinterSettings.InstalledPrinters.Count; i++)
            {
                PrinterSettings a = new PrinterSettings();
                a.PrinterName = PrinterSettings.InstalledPrinters[i].ToString();
                if (a.IsDefaultPrinter)
                {
                    return PrinterSettings.InstalledPrinters[i].ToString();
                }
            }
            return "";
        }

        private void Establecer_Impresora(string Diseño, string Nom_Producto, string Btn)
        {
            //envia la informacion a imprimir
            if (Btn.Equals("0"))
            {

                string imp = GetImpresoraDefecto();
                RawPrinterHelper.SendStringToPrinter(imp, Diseño, Nom_Producto);
            }
            else if (Btn.Equals("1"))
            {
                RawPrinterHelper.SendStringToPrinter("ZDesigner GT800 (EPL)", Diseño, Nom_Producto);
            }

        }
    }
    public class RawPrinterHelper
    {
        // estructura y declaraciones del API:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);


        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount, string Nom_Producto)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false;

            di.pDocName = "Etiqueta " + Nom_Producto;
            di.pDataType = "RAW";

            // Abra la impresora.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Inicie un documento.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Inicia página.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Escribe(Imprime).
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
                Util.ShowError("Error al conectar a la impresora " + szPrinterName);
            }
            return bSuccess;
        }

        public static bool SendStringToPrinter(string szPrinterName, string szString, string Nom_Producto)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // Cuenta caracteresque  se encuentran en la cadena
            dwCount = szString.Length;
            //Supongamos que la impresora está esperando texto ANSI y, a continuación, convertir
            //la cadena de texto ANSI.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            //Enviar la cadena ANSI convertidos a la impresora.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount, Nom_Producto);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }

}
