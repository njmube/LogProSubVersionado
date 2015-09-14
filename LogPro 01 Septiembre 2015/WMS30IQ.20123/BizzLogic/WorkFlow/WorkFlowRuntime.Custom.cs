using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities;
using Entities.Trace;
using System.Reflection;
using Entities.Master;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Entities.General;
using System.Threading;
using System.Data;
using BizzLogic.Logic;
using Entities.Workflow;
using Integrator;


namespace BizzLogic.Logic
{
    public partial class WorkFlowRuntime
    {

        public static void DataBaseRoutines()
        {
            Connection localSQL = null;
            
            BasicMngr BasicMngr = new BasicMngr();
            
            DaoFactory Factory = new DaoFactory();

            try { localSQL = Factory.DaoConnection().Select(new Connection { Name = "MY" }).First(); }
            catch { }

            try
            { //BasicMngr.DirectSQLNonQuery("EXEC WMS30.dbo.spRoutines", localSQL);
                Console.WriteLine("\tRoutines OK.");
            }
            catch (Exception ex) { Console.WriteLine("\tRoutines Fail. " + ex.Message); }


            //REcorre  los rtProcess que este pendientes por actulizar las tablas de consulta y las actuliza
            //1. Consultar lo procesosa
            //Aqui obtiene los registros XML que tiene que procesar
            IList<DataInformation> list = Factory.DaoDataInformation().Select(
                new DataInformation { ModTerminal = "T" }).Where(f => f.EntityRowID > 0).ToList();

            if (list != null && list.Count > 0)
            {
                Document Document;
                Label Label;
                string NombreTabla = "";
                string updQuery = "";
                IList<ShowData> metaData;
                foreach (DataInformation di in list.Where(f => !string.IsNullOrEmpty(f.XmlData)))
                {
                    try
                    {
                        if (di.Entity.ClassEntityID == EntityID.Document)
                        {
                            //Obtengo los datos del documento para tener el nombre de la bodega
                            Document = Factory.DaoDocument().Select(new Document { DocID = di.EntityRowID }).First();
                            //Obtengo el nombre de la bodega que pertenece el registro y creo el nombre de la tabla
                            NombreTabla = "Datos_" + Document.Location.ErpCode;
                        }
                        else if (di.Entity.ClassEntityID == EntityID.Label)
                        {
                            //Obtengo los datos del label para tener el nombre de la bodega
                            Label = Factory.DaoLabel().Select(new Label { LabelID = di.EntityRowID }).First();
                            try
                            {
                                Location location = Factory.DaoLocation().Select(new Location { LocationID = int.Parse(Label.CreTerminal) }).First();
                                NombreTabla = "Datos_" + location.ErpCode;
                            }
                            catch
                            {
                                //Obtengo el nombre de la bodega que pertenece el registro y creo el nombre de la tabla
                                NombreTabla = "Datos_" + Label.Bin.Location.ErpCode;
                            }
                        }
                        //Parte incial del update
                        updQuery = "UPDATE dbo." + NombreTabla + " SET ModDate = GETDATE(), RowID = " + di.EntityRowID.ToString() + " ";
                        //Obtiene la lista de campos a actualizar segun la bodega
                        //Es decir los codgios de campos que son tus nombres de columna
                        metaData = DeserializeMetaDataWF(di.XmlData);

                        if (metaData.Count == 0)
                        {
                            di.ModTerminal = null;
                            Factory.DaoDataInformation().Update(di);
                            continue;
                        }

                        //Crear el Update
                        //Aqui va contacenando nombre columna y valor para el update
                        List<string> sColumns = new List<string>();

                        for (int i = 0; i < metaData.Count; i++)
                        {
                            if (metaData[i].DataKey.ToLower().Equals("id"))
                                continue;

                            if (metaData[i].DataKey.ToLower().Equals("productid"))
                                continue;

                            if (metaData[i].DataKey.ToLower().Equals("producto"))
                                continue;

                            if (metaData[i].DataKey.ToLower().Equals("cantidad"))
                                continue;


                            if (!sColumns.Contains(metaData[i].DataKey))
                            {
                                updQuery += "," + metaData[i].DataKey + " = '" + metaData[i].DataValue + "' \n";
                                sColumns.Add(metaData[i].DataKey);
                            }

                        }

                        //parte final del update
                        updQuery += " WHERE  InstanceID = " + di.RowID.ToString();


                        //Intenta crear el ID por si no existe
                        //Esto lo hace por si el registro que vpy a actualizar no existe, entonces
                        ///primero se crea un registro en blano en la tabla para que el update funcione
                        ///el ID  del registro deberia ser el LabelID para elc aso de los labels y el docuemntid en los docuemntos
                        try { BasicMngr.DirectSQLNonQuery("EXEC dbo.spAdminDynamicData 2, " + di.RowID.ToString() + ",'" + NombreTabla + "'," + di.Entity.ClassEntityID.ToString(), localSQL); }
                        catch { }

                        //Ejecutando el query
                        BasicMngr.DirectSQLNonQuery(updQuery, localSQL);

                        //POniendo la entidad como actualizada
                        di.ModTerminal = null;
                        Factory.DaoDataInformation().Update(di);
                        Console.WriteLine("OK => " + di.EntityRowID + ". " + di.RowID);

                    }
                    catch (Exception ex)
                    {
                        //report the mistake.
                        ExceptionMngr.WriteEvent("Routines: ", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                        Console.WriteLine("  ERR => " + di.EntityRowID + ". " + di.RowID + ". " + ex.Message);
                    }

                }
            }

            try
            {
                //Missing process, si pasadas dos horas falta label o proceso  el proceso se recrea a partir del dato recibido.
                //MissingProcess(localSQL);
            }
            catch (Exception ex) {

                //report the mistake.
                ExceptionMngr.WriteEvent("Routines: MissingProcess", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
            }

        }

        //Generar la Lista de ShowData a partir de un Xml
        private static object XmlDeSerializerWF(string objString, Type objType)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString));
            XmlSerializer mySerializer = new XmlSerializer(objType);
            return mySerializer.Deserialize(ms);
        }

        public static List<ShowData> DeserializeMetaDataWF(string MetaData)
        {
            if (!String.IsNullOrEmpty(MetaData))
                return (List<ShowData>)XmlDeSerializerWF(MetaData, typeof(List<ShowData>));
            else
                return new List<ShowData>();
        }

    }
}
