using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect;
using Entities.Master;
using System.Data;
using System.Collections.Specialized;
using System.Xml;
using System.Data.SqlClient;
using System.IO;
using Entities.Profile;
using Integrator;

namespace ErpConnect.UNOEE
{
    class Unoee : ConnectFactory
    {

        private static SqlConnection curConnection;
        private static SqlCommand curCommand;


        public override IDocumentService Documents()
        { return new DocumentService(FactoryCompany); }


        public override IReferenceService References()
        { return new ReferenceService(FactoryCompany); }


        internal static void SendData(Company CurCompany, DataSet dsSetup, DataSet dsData, int numRegs)
        {
            string[] unoEEDocument = new string[numRegs];

           //Pone el encabezado
            unoEEDocument[0] = "000000100000001" + CurCompany.ErpCode.PadLeft(3, '0');         
           
            int i = 1, z = 0;
            //procesa cada una de las lineas Encabezado/Detalle
            foreach (DataTable dt in dsData.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    unoEEDocument[i] = FormatLineUnoee(dr, dsSetup.Tables[z]);
                    i++;
                }

                z++;
            }

           //Poner el Final
            unoEEDocument[numRegs - 1] = numRegs.ToString().PadLeft(7, '0') + "99990001" + CurCompany.ErpCode.PadLeft(3, '0');
           
           //Envia el archivo plano a UNOEE
            ImportDataToUnoEE(CurCompany, unoEEDocument);
          
        }


        private static string FormatLineUnoee(DataRow dr, DataTable dtSetup)
        {
            //La linea debe tener la tabla a la que pertenece y de esa tabla se obtienen los datos del archivo a formar.  
            //Configura la linea enviada segun el formato UNOEE del dato. (trae campo unoee y trae valor del dato)
            //string data - Nombre campo (entrega el valor que trae el datarow), [f662_campo]
            //string type : 1 - numero (int, decimal etc), 2 - string, [f662_tipo_dato]
            //string length longitud el campo [f662_tamano]
            //string sqltype: tipo SQL [f662_tipo_dato_sql]

            //Para cada una de las columnas en el DataRow Se ejecuta el proceso
            string type = "", length="", data  = "", sqltype = "";
            string lineResult = "";


            foreach (DataColumn c in dr.Table.Columns)
            {
                data = dr[c].ToString();
                type = dtSetup.Select("f662_campo = '" + c.ColumnName + "'")[0]["f662_tipo_dato"].ToString();
                length = dtSetup.Select("f662_campo = '" + c.ColumnName + "'")[0]["f662_tamano"].ToString();
                sqltype = dtSetup.Select("f662_campo = '" + c.ColumnName + "'")[0]["f662_tipo_dato_sql"].ToString();

                lineResult += FieldFormat(data, type, length, sqltype);
            }

            return lineResult;

        }


        public static DataTable GetUnoEEDataTable(DataTable dataTable)
        {
            //recibe un DT con los campos de unoEE y retorna el datatable para alimentar.
            DataTable result = new DataTable();

            foreach (DataRow dr in dataTable.Rows)
                result.Columns.Add(dr["f662_campo"].ToString());

            return result;
        }



        private static string FieldFormat(string data, string type, string lenght, string sqltype)
        {
            //Entero
            switch (type)
            {
                case "1": //Numeric

                    data = data.Replace(",", ".");

                    if (sqltype.Contains("money"))
                    {
                        data = data.Replace(",0000", ".0000");

                        if (data.EndsWith(".0000"))
                            return "+" + data.PadLeft(int.Parse(lenght) - 1, '0'); //una posicion adicional por el signo +

                        else if (data.Contains('.'))
                            return "+" + GetDoubleFormat(data, int.Parse(lenght) - 5, 4);

                        else
                            return "+" + data.PadLeft(int.Parse(lenght) - 6, '0') + ".0000";
                    }

                    if (sqltype.Contains("decimal"))
                    {
                        if (data.EndsWith(".0000"))
                            return data.PadLeft(int.Parse(lenght), '0');

                        else if (data.EndsWith(".00"))
                            return data.PadLeft(int.Parse(lenght), '0');

                        else if (data.Contains('.') && int.Parse(lenght) < 10)
                            return GetDoubleFormat(data, int.Parse(lenght) - 3, 2);

                        else if (int.Parse(lenght) < 10)
                            return data.PadLeft(int.Parse(lenght) - 3, '0') + ".00";

                        else if (data.Contains('.'))
                            return GetDoubleFormat(data, int.Parse(lenght) - 5, 4);

                        else
                            return data.PadLeft(int.Parse(lenght) - 5, '0') + ".0000";
                    }


                    return data.PadLeft(int.Parse(lenght), '0');
                //return data.PadLeft(15, '0') + ".0000";

                case "2": //String
                    return data.PadRight(int.Parse(lenght), ' ').Substring(0, int.Parse(lenght));

            }

            return data.PadRight(int.Parse(lenght), ' ');

        }



        private static string GetDoubleFormat(string data, int numEnteros, int numDecimales)
        {

            string[] number = data.Split(".".ToCharArray());

            string parteEntera = number[0].Trim()
                .Substring(0, number[0].Length > numEnteros ? numEnteros : number[0].Length)
                .PadLeft(numEnteros, '0');


            string parteDecimal = number[1].Trim()
                .Substring(0, number[0].Length > numDecimales ? numDecimales : number[1].Length)
                .PadLeft(numDecimales, '0');

            return parteEntera + "." + parteDecimal;

        }




        internal static StringDictionary GetUnoEEDocSetup(string xmlData)
        {

            StringDictionary data = new StringDictionary();

            XmlDocument myXmlOut = new XmlDocument();
            myXmlOut.LoadXml(xmlData);

            // convert to dataset in two lines
            DataSet ds = new DataSet();
            ds.ReadXml(new XmlNodeReader(myXmlOut));

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                    data.Add(dr[0].ToString(), dr[1].ToString());

                return data;
            }
            
            return null;

        }



        #region IMPORTACION UNOEE WS / IMPODATOS


        private static void ImportDataToUnoEE(Company company, string[] dataLines)
        {

            if (company.ErpConnection.UserDef == "IMPODATOS")
                ImportDataToUnoEE_Monitor(company, dataLines);

            else if (company.ErpConnection.UserDef == "WS")
                ImportDataToUnoEE_WS(company, dataLines);
            
            else
                throw new Exception("No hay conexión definida WS o IMPODATOS.");
        }


        private static void ImportDataToUnoEE_Monitor(Company company, string[] dataLines)
        {

            //############# CREATING CONTROL
            //f667_ts_proceso	char(25)	Unchecked
            //f667_id_cia	smallint	Unchecked
            //f667_referencia	varchar(255)	Unchecked
            //f667_ind_procesable	smallint	Unchecked
            //f667_ind_error	smallint	Unchecked
            //f667_estado_proceso	smallint	Unchecked


            //############# CREATING DETAILS ROWS
            //f668_ts_proceso	char(25)	Unchecked
            //f668_id_cia	smallint	Unchecked
            //f668_referencia	varchar(255)	Unchecked
            //f668_registro	varchar(3000)	Unchecked

            Random x = new Random();
            String tsString = x.Next(99).ToString() + DateTime.Now.ToString("yyMMddHHmmss");
            String reference = "IMP" + tsString;


            SqlDataAdapter objAdapter;

            SqlCommandBuilder ObjCmdBuilder, ObjCmdBuilderDet;
            curCommand = new SqlCommand();

            //Console.WriteLine(mapping.DestEntity.DataSource.CnnString);

            curConnection = new SqlConnection(company.ErpConnection.CnnString);
            curCommand.Connection = curConnection;
            curConnection.Open();

            DataRow objDBRow, objDBRowDet; //Temporal row object


            string t667_table = "t667_imp_conector_control";
            string t668_table = "t668_imp_conector_detalle";

            DataSet ds667 = new DataSet(t667_table); //Control Table
            DataSet ds668 = new DataSet(t668_table); //Records Table


            //Llenado los datasets conlas columnas de cada tabla
            objAdapter = new SqlDataAdapter("SELECT * FROM " + t667_table + " WHERE 1 = 2 ", curConnection);
            objAdapter.Fill(ds667, t667_table);

            SqlDataAdapter objAdapterDet = new SqlDataAdapter("SELECT * FROM " + t668_table + " WHERE 1 = 2 ", curConnection);
            objAdapterDet.Fill(ds668, t668_table);

            try
            {

                objDBRow = ds667.Tables[0].NewRow();

                //Llenando la tabla de control 667            
                objDBRow["f667_ts_proceso"] = tsString;
                objDBRow["f667_id_cia"] = company.ErpCode;
                objDBRow["f667_referencia"] = reference;
                objDBRow["f667_ind_procesable"] = "1";
                objDBRow["f667_ind_error"] = "0";
                objDBRow["f667_estado_proceso"] = "0";

                ds667.Tables[0].Rows.Add(objDBRow);

                //Guardando el nuevo control
                ObjCmdBuilder = new SqlCommandBuilder(objAdapter);
                objAdapter.Update(ds667, t667_table);


                for (int i = 0; i < dataLines.Length; i++)
                //recorre las lineas a importar, la primea y la ultima son los registros de control
                {
                    //Llenando la tabla de datos 668   
                    //registro inicial tipo 0 y el ultimo de tipo 9999
                    objDBRowDet = ds668.Tables[0].NewRow();
                    objDBRowDet["f668_ts_proceso"] = tsString;
                    objDBRowDet["f668_id_cia"] = company.ErpCode;
                    objDBRowDet["f668_referencia"] = reference;
                    objDBRowDet["f668_registro"] = dataLines[i];
                    ds668.Tables[0].Rows.Add(objDBRowDet);

                    //Console.WriteLine(dataLines[i]);
                }


                ObjCmdBuilderDet = new SqlCommandBuilder(objAdapterDet);
                objAdapterDet.Update(ds668, t668_table);

                //Executing UnoEE ImpoDatos
                Console.WriteLine("Listo para IMPODATOS");
                ExecuteImpoDatos(company, tsString, reference);

                //verifica si el impodatos no saco error para ese lote
                Console.WriteLine("Listo para CheckErrors");
                string error =  CheckForError(company, tsString, reference);

                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);

            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
                //return ExceptionMngr.GetTechMessage(e);
            }

        }


        private static string CheckForError(Company company, string ts, string reference)
        {
            //Ir a la tabla de 667/668 a mirar si guardo los datos
            string sqlQueryReg = "SELECT  t668.f668_ts_proceso FROM  t667_imp_conector_control t667 "
                      + "INNER JOIN t668_imp_conector_detalle t668 ON t667.f667_ts_proceso = t668.f668_ts_proceso "
                      + "AND t667.f667_id_cia = t668.f668_id_cia AND t667.f667_referencia = t668.f668_referencia "
                      + "WHERE t668.f668_ts_proceso = '" + ts + "' AND t668.f668_referencia = '" + reference + "'";





            //Ir a la tabla de control de errores y buscar el TS y la referencia
            //t664_imp_log_control Encabezado de el error
            //t665_imp_log_detalle Detalle del error

            string sqlQuery = "SELECT     det.f665_tipo_reg, det.f665_subtipo_reg, det.f665_nro_registro, det.f665_version, det.f665_nivel_error, det.f665_valor_error, det.f665_detalle_error, "
            + "          det.f665_ind_transaccion FROM t664_imp_log_control ctrl INNER JOIN "
            + " t665_imp_log_detalle det ON ctrl.f664_rowid = det.f665_rowid_imp_log WHERE ctrl.f664_ts_proceso = '" + ts + "' AND ctrl.f664_referencia = '" + reference + "'";


            try
            {

                SqlDataAdapter objAdapter;
                curConnection = new SqlConnection(company.ErpConnection.CnnString);
                curCommand.Connection = curConnection;
                curConnection.Open();


                DataTable dtR = new DataTable("ToSave"); //Control Table
                //Llenado los datasets conlas columnas de cada tabla\
                //Console.WriteLine(sqlQueryReg);

                objAdapter = new SqlDataAdapter(sqlQueryReg, curConnection);
                objAdapter.Fill(dtR);

                Console.WriteLine(dtR.Rows.Count + " Registros en 667/668");

                //if (dtR == null || dtR.Rows.Count == 0)
                //    return "No hay Registros en la tabla de monitor IMPODATOS.";


                DataTable dt = new DataTable("Errors"); //Control Table
                //Llenado los datasets conlas columnas de cada tabla
                objAdapter = new SqlDataAdapter(sqlQuery, curConnection);
                objAdapter.Fill(dt);

                if (dt == null || dt.Rows.Count == 0)
                    return "";

                string result = "";

                foreach (DataRow dr in dt.Rows)
                    result += dr[0].ToString() + ", " + dr[5].ToString() + "," + dr[6].ToString() + "\n";

                return result;

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
            finally { curConnection.Close(); }


        }

        
        //Ejecuta el comando de UnoEE para importar los datos.
        private static void ExecuteImpoDatos(Company company, string ts, string reference)
        {
            String exeIMpodatos = "";

            try { exeIMpodatos = (new WmsTypes()).GetCompanyOption(company, "IMPODATOS").ToString(); }
            catch {}

            if (string.IsNullOrEmpty(exeIMpodatos))
                throw new Exception("Variable de Configuración de IMPODATOS no definida.");


            string username = company.ErpConnection.UserName;
            string passwd = company.ErpConnection.Password;
            string connection = company.ErpConnection.Domain;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;

            if (!File.Exists(exeIMpodatos))
                throw new Exception("Archivo " + exeIMpodatos + " no existe.");

            exeIMpodatos = exeIMpodatos.Replace("\\\\", "\\");
            proc.StartInfo.FileName = "\"" + exeIMpodatos + "\"";


            //Console.Write(proc.StartInfo.FileName);
            //Console.Write(" 1," + connection + "," + username + "," + passwd + "," + company + "," + ts + "," + reference + ",0");

            proc.StartInfo.Arguments = "1," + connection + "," + username + "," + passwd + "," + company.ErpCode + "," + ts + "," + reference + ",0";
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
            proc.Close();


        }


        private static string ImportDataToUnoEE_WS(Company company, string[] dataLines)
        {
            string pvstrDatos = CreateXmlToImport(dataLines, company);

            /*
            WSImportar unoEEws = new WSImportar(); //Instancia del Ws de Unoee.
            unoEEws.Url = mapping.DestEntity.DataSource.Description;
            short printError = 1;
            DataSet dsResult = unoEEws.ImportarXML(pvstrDatos, ref printError);
            

            //Check Errors
            string result = "";
            if (dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsResult.Tables[0].Rows)
                    result += dr[1].ToString() + ", " + dr[5].ToString() + "," + dr[6].ToString() + "\n";


                return result;

            }
             * */

            return "";
        }


        private static string CreateXmlToImport(string[] dataLines, Company company)
        {

            string username = company.ErpConnection.UserName;
            string passwd = company.ErpConnection.Password;
            string connection = company.ErpConnection.Domain;


            //Creating XML File to import
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode importarNode = doc.CreateElement("Importar");
            doc.AppendChild(importarNode);


            XmlNode level2 = doc.CreateElement("NombreConexion");
            level2.AppendChild(doc.CreateTextNode(connection));
            importarNode.AppendChild(level2);


            level2 = doc.CreateElement("IdCia");
            level2.AppendChild(doc.CreateTextNode(company.ErpCode));
            importarNode.AppendChild(level2);

            level2 = doc.CreateElement("Usuario");
            level2.AppendChild(doc.CreateTextNode(username));
            importarNode.AppendChild(level2);

            level2 = doc.CreateElement("Clave");
            level2.AppendChild(doc.CreateTextNode(passwd));
            importarNode.AppendChild(level2);


            XmlNode level3 = doc.CreateElement("Datos");
            importarNode.AppendChild(level3);


            // Creando las lineas de importacion a UNOEE
            XmlNode linea = null;
            foreach (string curLine in dataLines)
            {
                linea = doc.CreateElement("Linea");
                linea.AppendChild(doc.CreateTextNode(curLine));
                level3.AppendChild(linea);
            }

            //linea = doc.CreateElement("Linea");
            //linea.AppendChild(doc.CreateTextNode(record));
            //level3.AppendChild(linea);

            //linea = doc.CreateElement("Linea");
            //linea.AppendChild(doc.CreateTextNode(endRecord));
            //level3.AppendChild(linea);

            return doc.OuterXml;
        }


        #endregion

    }
}
