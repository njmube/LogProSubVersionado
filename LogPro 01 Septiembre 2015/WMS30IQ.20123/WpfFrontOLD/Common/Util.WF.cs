using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Printing; // The PrinterSettings class is located inside the Printing namespace
using System.Management;
using System.Reflection;
using System.Collections.Specialized;
using System.DirectoryServices;
using WpfFront.WMSBusinessService;
using WpfFront.Services;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media.Imaging;
using Assergs.Windows;
using System.Collections;
using System.Linq;
using WMComposite.Modularity;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Xceed.Wpf.DataGrid.Settings;
using WMComposite.Regions;
using System.Windows.Controls;
using System.Data;
using System.Media;




namespace WpfFront.Common
{
    public partial class Util
    {

        //Generar el Xml desde una Lista de tipo ShowData
        public static string XmlSerializerWF(Object obj)
        {
            MemoryStream myMemStream = new MemoryStream();
            XmlSerializer mySerializer = new XmlSerializer(obj.GetType());
            mySerializer.Serialize(myMemStream, obj);
            myMemStream.Position = 0;

            // Load the serialized eConnect document object into an XML document object
            XmlTextReader xmlreader = new XmlTextReader(myMemStream);
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(xmlreader);
            return myXmlDocument.OuterXml;
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

        //Convierte una cadena en un DataTable
        public static DataTable ConvertToDataTable(string strFile, string TableName, string delimiter, bool useHeader)
        {

            if (string.IsNullOrEmpty(delimiter))
                delimiter = "\t";

            //The DataSet to Return
            DataTable result = new DataTable(TableName);


            //Open the file in a stream reader.
            //StreamReader s = new StreamReader(File);


            //Split off each row at the Carriage Return/Line Feed
            //Default line ending in most windows exports.  
            //You may have to edit this to match your particular file.
            //This will work for Excel, Access, etc. default exports.
            string[] rows = strFile.Split("\n".ToCharArray());


            //Split the first line into the columns       
            string[] columns = rows[0].Split(delimiter.ToCharArray());

            //Cycle the colums, adding those that don't exist yet 
            //and sequencing the one that do.;
            for (int i = 0; i < columns.Length; i++)
                result.Columns.Add("col_" + i.ToString());


            //Read the rest of the data in the file.        
            //string AllData = s.ReadToEnd();

            //Now add each row to the DataSet        
            foreach (string r in rows)
            {
                //Split the row at the delimiter.
                string[] items = r.Trim().Split(delimiter.ToCharArray());

                //Add the item
                result.Rows.Add(items);
            }

            //Return the imported data.        
            return result;
        }
    }
}